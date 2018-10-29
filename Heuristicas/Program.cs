﻿using Heuristicas.Metaheuristicas;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Heuristicas
{
    class Program
    {
        static void Main(string[] args)
        {
            Execucao(args).Wait();
        }

        static async Task Execucao(string[] args)
        {
            bool execucaoDebug = true;

            int quantidadeExcecucoes = 10;
            int quantidadeExecucoesSimultaneas = 8;
            execucaoDebug = quantidadeExecucoesSimultaneas > 1 ? false : execucaoDebug;
            var informacoesExecucaoInstancias = new Dictionary<string, List<MetaHeuristicaBase>>();

            // Declaração de variáveis e definição valores default
            var listaInstancias = new List<string>();
            string heuristica = Constantes.HeuristicasImplementadas.BuscaTabu;

            double multiplicadorNumeroMaximoRejeicoesLAHC = 10000;
            double multiplicadorTamanhoMemoriaLAHC = 100;

            double multiplicadorIteracoesSemMelhoraBT = 200; // multiplicado pelo número de vértices do grafo
            double multiplicadorIteracoesProibicaoListaBT = 0.5; // multiplicado pelo número de vértices do grafo
            int incrementoTamanhoListaTabuBT = 1;
            int moduloIteracaoSemMelhoraIncrementoListaTabu = 50;

            double multiplicadorNumeroMaximoIteracoesSemMelhoraILS = 1000; // multiplicado pelo número de vértices do grafo
            double divisorNumeroMaximoIteracoesMesmoNivelILS = 5; // dividido pelo número máximo aceitável de execuções sem melhora

            MetaHeuristicaBase metaHeuristica = null;

            // Caso a execução tenha sido chamada via linha de comando, recupera os parâmetros enviados
            if (args.Length > 0)
            {
                listaInstancias.Add(args[0]);
                heuristica = args[1];

                switch (heuristica)
                {
                    case Constantes.HeuristicasImplementadas.LAHC:
                        multiplicadorTamanhoMemoriaLAHC = double.Parse(args[2]);
                        multiplicadorNumeroMaximoRejeicoesLAHC = double.Parse(args[3]);
                        break;
                    case Constantes.HeuristicasImplementadas.BuscaTabu:
                        multiplicadorIteracoesSemMelhoraBT = double.Parse(args[2]);
                        multiplicadorIteracoesProibicaoListaBT = double.Parse(args[3]);
                        break;
                    case Constantes.HeuristicasImplementadas.ILS:
                        multiplicadorNumeroMaximoIteracoesSemMelhoraILS = double.Parse(args[2]);
                        divisorNumeroMaximoIteracoesMesmoNivelILS = double.Parse(args[3]);
                        break;
                    default:
                        throw new Exception("Heurística não implementada.");
                }

                execucaoDebug = false;
            }
            else
            {
                listaInstancias.Add("p64_21_22");
                listaInstancias.Add("p97_24_26");

                //listaInstancias.Add("p31_18_21");
                //listaInstancias.Add("p37_18_20");
                //listaInstancias.Add("p50_19_25");
                //listaInstancias.Add("p55_20_24");
                //listaInstancias.Add("p58_20_21");
                //listaInstancias.Add("p64_21_22");
                //listaInstancias.Add("p67_21_22");
                //listaInstancias.Add("p82_23_24");
                //listaInstancias.Add("p83_23_24");
                //listaInstancias.Add("p92_24_26");
                //listaInstancias.Add("p93_24_27");
                //listaInstancias.Add("p95_24_27");
                //listaInstancias.Add("p97_24_26");
                //listaInstancias.Add("p98_24_29");

                if (!listaInstancias.Any())
                {
                    string diretorioInstancias = ConfigurationManager.AppSettings["CAMINHO_BASE_INSTANCIAS"];
                    var diretorio = new DirectoryInfo(diretorioInstancias);
                    var arquivosDiretorio = diretorio.GetFiles();

                    foreach (FileInfo arquivo in arquivosDiretorio)
                    {
                        //if (arquivo.Name != "p100_24_34" && listaInstancias.Count < 5) // TODO: remover (apenas teste)
                        listaInstancias.Add(arquivo.Name);
                    }

                    listaInstancias = listaInstancias.OrderBy(x => x).ToList();
                }
            }

            for (int i = 0; i < quantidadeExcecucoes; i++)
            {
                for (int j = 0; j < listaInstancias.Count; j += quantidadeExecucoesSimultaneas)
                {
                    var tarefas = new Task[quantidadeExecucoesSimultaneas];

                    for (int k = j; k < j + quantidadeExecucoesSimultaneas && k < listaInstancias.Count; k++)
                    {
                        if (!informacoesExecucaoInstancias.ContainsKey(listaInstancias[k]))
                            informacoesExecucaoInstancias.Add(listaInstancias[k], new List<MetaHeuristicaBase>());

                        Console.WriteLine($"Tentativa de execução da instancia [{ listaInstancias[k] }] pela [{ i + 1 }ª] vez às [{ DateTime.Now.ToString("HH:mm:ss") }]...\n");
                        switch (heuristica)
                        {
                            case Constantes.HeuristicasImplementadas.LAHC:
                                metaHeuristica = new LAHC(listaInstancias[k], execucaoDebug, multiplicadorNumeroMaximoRejeicoesLAHC, multiplicadorTamanhoMemoriaLAHC);
                                break;
                            case Constantes.HeuristicasImplementadas.BuscaTabu:
                                metaHeuristica = new BuscaTabu(listaInstancias[k], execucaoDebug, multiplicadorIteracoesSemMelhoraBT, multiplicadorIteracoesProibicaoListaBT, incrementoTamanhoListaTabuBT, moduloIteracaoSemMelhoraIncrementoListaTabu);
                                break;
                            case Constantes.HeuristicasImplementadas.ILS:
                                metaHeuristica = new ILS(listaInstancias[k], execucaoDebug, multiplicadorNumeroMaximoIteracoesSemMelhoraILS, divisorNumeroMaximoIteracoesMesmoNivelILS);
                                break;
                            default:
                                throw new Exception("Heurística não implementada.");
                        }

                        informacoesExecucaoInstancias[listaInstancias[k]].Add(metaHeuristica);

                        tarefas[k - j] = metaHeuristica.ExecutarMetaheuristica();
                    }

                    int qtdTarefasProcessarAgora = tarefas.Where(x => x != null).Count();

                    if (qtdTarefasProcessarAgora == tarefas.Length)
                        await Task.WhenAll(tarefas);
                    else
                    {
                        // Caso o número de tarefas a processar não seja múltiplo do número de tarefas executadas simultaneamente...
                        var tarefasNovo = tarefas.Where(x => x != null).ToArray();
                        tarefas = null;
                        await Task.WhenAll(tarefasNovo);
                    }
                }

                GravarLogGeral(informacoesExecucaoInstancias);
            }

            if (listaInstancias.Count == 1)
            {
                if (execucaoDebug)
                {
                    Console.SetWindowSize(200, 60);
                    Console.WriteLine("Instância: " + metaHeuristica.Instancia);
                    Console.WriteLine("Meta-heurística executada: " + metaHeuristica.NomeHeuristica);
                    Console.WriteLine($"O valor encontrado para a melhor solução foi { metaHeuristica.FOMelhorSolucao }");
                    Console.WriteLine($"Organização dos componentes: [ | { string.Join(" | ", metaHeuristica.MelhorSolucao) } | ]");

                    Console.ReadKey();
                }
                else
                {
                    Console.Write(metaHeuristica.FOMelhorSolucao);
                }
            }
        }

        static void GravarLogGeral(Dictionary<string, List<MetaHeuristicaBase>> informacoesExecucaoInstancias)
        {
            string arquivoLogGeral = string.Format(ConfigurationManager.AppSettings["CAMINHO_ARQUIVO_LOG_GERAL"]);
            if (File.Exists(arquivoLogGeral))
                File.Delete(arquivoLogGeral);

            using (var escritorArquivo = new StreamWriter(arquivoLogGeral))
            {
                escritorArquivo.WriteLine($"Log gravado após { informacoesExecucaoInstancias.First().Value.Count } execuções às { DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") }\n");
                escritorArquivo.WriteLine("Instância\tCutwidth\tCutwidth médio\tMenor melhor iteração\tMaior melhor iteração\tTempo médio\t\tMelhor solução ");
                foreach (var instancia in informacoesExecucaoInstancias)
                {
                    int menorCutwidth = instancia.Value.Min(x => x.FOMelhorSolucao);
                    double mediaCutwidth = instancia.Value.Average(x => x.FOMelhorSolucao);
                    var melhorSolucao = instancia.Value.Where(x => x.FOMelhorSolucao == menorCutwidth).First().MelhorSolucao;
                    double tempoMedio = instancia.Value.Sum(x => x.Cronometro.Elapsed.Seconds) / instancia.Value.Count;
                    int menorMelhorIteracao = instancia.Value.Min(x => x.MelhorIteracao);
                    int maiorMelhorIteracao = instancia.Value.Max(x => x.MelhorIteracao);

                    escritorArquivo.WriteLine($"{ instancia.Value.First().Instancia }\t{ menorCutwidth }\t\t\t{ mediaCutwidth }\t\t\t\t{ menorMelhorIteracao.ToString().PadLeft(4, '0') }\t\t\t\t\t{ maiorMelhorIteracao.ToString().PadLeft(4, '0') }\t\t\t\t\t{ tempoMedio }\t\t\t\t { string.Join(" | ", melhorSolucao.Select(x => x.ToString().PadLeft(2, '0'))) } ");
                }
            }
        }
    }
}