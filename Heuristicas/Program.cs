using Heuristicas.Metaheuristicas;
using Heuristicas.Metaheuristicas.BuscaTabu;
using Heuristicas.Metaheuristicas.ILS;
using Heuristicas.Metaheuristicas.LAHC;
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
            bool execucaoDebug = false;

            int quantidadeExcecucoes = 10;
            int quantidadeExecucoesSimultaneas = 1;
            execucaoDebug = quantidadeExecucoesSimultaneas > 1 ? false : execucaoDebug;
            var informacoesExecucaoInstancias = new Dictionary<string, List<MetaHeuristicaBase>>();

            // Declaração de variáveis e definição valores default
            var listaInstancias = new List<string>();
            string heuristica = Constantes.HeuristicasImplementadas.BuscaTabu;

            double multiplicadorNumeroMaximoRejeicoesLAHC = 10000;
            double multiplicadorTamanhoMemoriaLAHC = 100;

            double multiplicadorIteracoesSemMelhoraBT = 200; // multiplicado pelo número de vértices do grafo
            double multiplicadorIteracoesProibicaoListaBT = 0.2; // multiplicado pelo número de vértices do grafo
            double multiplicadorMaximoIteracoesProibicaoListaBT = 2.5; // valor proporcional somado ao número máximo de iterações (o tamanho máximo da lista não pode ser maior que o [número de vértices * multiplicadorIteracoesSemMelhoraBT] / moduloIteracaoSemMelhoraIncrementoListaTabu]
            int moduloIteracaoSemMelhoraIncrementoListaTabu = 30;

            double multiplicadorNumeroMaximoIteracoesSemMelhoraILS = 50; // multiplicado pelo número de vértices do grafo
            double divisorNumeroMaximoIteracoesMesmoNivelILS = 5; // dividido pelo número máximo aceitável de execuções sem melhora

            MetaHeuristicaBase metaHeuristica = null;

            // Caso a execução tenha sido chamada via linha de comando, recupera os parâmetros enviados
            if (args.Length > 0)
            {
                int inicioNomeInstancia = args[1].LastIndexOf("/") + 1;
                int fimNomeInstancia = args[1].Length - inicioNomeInstancia;

                listaInstancias.Add(args[1].Substring(inicioNomeInstancia, fimNomeInstancia));
                if (args[2].Equals("--algoritmo"))
                    heuristica = args[3];

                switch (heuristica)
                {
                    case Constantes.HeuristicasImplementadas.LAHC:
                        if (args[4].Equals(""))
                            multiplicadorTamanhoMemoriaLAHC = double.Parse(args[4]);
                        if (args[6].Equals(""))
                            multiplicadorNumeroMaximoRejeicoesLAHC = double.Parse(args[6]);
                        break;
                    case Constantes.HeuristicasImplementadas.BuscaTabu:
                        if (args[4].Equals("--iter_sem_melhora"))
                            multiplicadorIteracoesSemMelhoraBT = double.Parse(args[5]);
                        if (args[6].Equals("--iter_proibicao"))
                            multiplicadorIteracoesProibicaoListaBT = double.Parse(args[7]);
                        if (args[8].Equals("--iter_proibicao_max"))
                            multiplicadorMaximoIteracoesProibicaoListaBT = double.Parse(args[9]);
                        if (args[10].Equals("--mod_incremento_lista"))
                            moduloIteracaoSemMelhoraIncrementoListaTabu = int.Parse(args[11]);
                        break;
                    case Constantes.HeuristicasImplementadas.ILS:
                        if (args[4].Equals(""))
                            multiplicadorNumeroMaximoIteracoesSemMelhoraILS = double.Parse(args[4]);
                        if (args[6].Equals(""))
                            divisorNumeroMaximoIteracoesMesmoNivelILS = double.Parse(args[6]);
                        break;
                    default:
                        throw new Exception("Heurística não implementada.");
                }

                execucaoDebug = false;
            }
            else
            {
                //listaInstancias.Add("p50_19_25");

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
                    //var tarefas = new Task[quantidadeExecucoesSimultaneas];

                    for (int k = j; k < j + quantidadeExecucoesSimultaneas && k < listaInstancias.Count; k++)
                    {
                        if (!informacoesExecucaoInstancias.ContainsKey(listaInstancias[k]))
                            informacoesExecucaoInstancias.Add(listaInstancias[k], new List<MetaHeuristicaBase>());

                        Console.WriteLine($"Tentativa de execução da instancia [{ listaInstancias[k] }] pela [{ i + 1 }ª] vez às [{ DateTime.Now.ToString("HH:mm:ss") }]...\n");
                        switch (heuristica)
                        {
                            case Constantes.HeuristicasImplementadas.LAHC:
                                metaHeuristica = new ImplementacaoLAHC(listaInstancias[k], execucaoDebug, multiplicadorNumeroMaximoRejeicoesLAHC, multiplicadorTamanhoMemoriaLAHC);
                                break;
                            case Constantes.HeuristicasImplementadas.BuscaTabu:
                                metaHeuristica = new ImplementacaoBuscaTabu(listaInstancias[k], execucaoDebug, multiplicadorIteracoesSemMelhoraBT, multiplicadorIteracoesProibicaoListaBT, multiplicadorMaximoIteracoesProibicaoListaBT, moduloIteracaoSemMelhoraIncrementoListaTabu);
                                break;
                            case Constantes.HeuristicasImplementadas.ILS:
                                metaHeuristica = new ImplementacaoILS(listaInstancias[k], execucaoDebug, multiplicadorNumeroMaximoIteracoesSemMelhoraILS, divisorNumeroMaximoIteracoesMesmoNivelILS);
                                break;
                            default:
                                throw new Exception("Heurística não implementada.");
                        }

                        informacoesExecucaoInstancias[listaInstancias[k]].Add(metaHeuristica);

                        //tarefas[k - j] = metaHeuristica.ExecutarMetaheuristica();
                        metaHeuristica.ExecutarMetaheuristica();
                    }

                    //int qtdTarefasProcessarAgora = tarefas.Where(x => x != null).Count();

                    //if (qtdTarefasProcessarAgora == tarefas.Length)
                    //    await Task.WhenAll(tarefas);
                    //else
                    //{
                    //    // Caso o número de tarefas a processar não seja múltiplo do número de tarefas executadas simultaneamente...
                    //    var tarefasNovo = tarefas.Where(x => x != null).ToArray();
                    //    tarefas = null;
                    //    await Task.WhenAll(tarefasNovo);
                    //}

                    GravarLogGeral(informacoesExecucaoInstancias);
                }
            }

            if (!execucaoDebug)
                Console.Write(metaHeuristica.FOMenorCutwidthMelhorSolucao);
            //Console.ReadKey();
        }

        static void GravarLogGeral(Dictionary<string, List<MetaHeuristicaBase>> informacoesExecucaoInstancias)
        {
            string arquivoLogGeral = string.Format(ConfigurationManager.AppSettings["CAMINHO_ARQUIVO_LOG_GERAL"]);
            if (File.Exists(arquivoLogGeral))
                File.Delete(arquivoLogGeral);

            using (var escritorArquivo = new StreamWriter(arquivoLogGeral))
            {
                escritorArquivo.WriteLine($"Log gravado após { informacoesExecucaoInstancias.First().Value.Count } execuções às { DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss") }\n");
                escritorArquivo.WriteLine("Instância\tCutwidth\tCutwidth médio\tMenor qtd Vértices maior Cutwidth\t\tMenor soma\tMenor melhor iteração\tMaior melhor iteração\tTempo médio\t\tMelhor solução ");
                foreach (var instancia in informacoesExecucaoInstancias)
                {
                    int menorCutwidth = instancia.Value.Min(x => x.FOMenorCutwidthMelhorSolucao);
                    double mediaCutwidth = instancia.Value.Average(x => x.FOMenorCutwidthMelhorSolucao);
                    var melhorSolucao = instancia.Value.Where(x => x.FOMenorCutwidthMelhorSolucao == menorCutwidth).First().MelhorSolucao;
                    double tempoMedio = instancia.Value.Sum(x => x.Cronometro.Elapsed.TotalSeconds) / instancia.Value.Count;
                    int menorMelhorIteracao = instancia.Value.Min(x => x.MelhorIteracao);
                    int maiorMelhorIteracao = instancia.Value.Max(x => x.MelhorIteracao);
                    int menorSoma = instancia.Value.Min(x => x.FOMenorSomaCutwidthMelhorSolucao);
                    int menorQuantidadeVerticesMaiorCutWidth = instancia.Value.First(x => x.FOMenorCutwidthMelhorSolucao == menorCutwidth).FOMenorQuantidadeVerticesMaiorCutwidthMelhorSolucao;

                    escritorArquivo.WriteLine($"{ instancia.Value.First().Instancia }\t{ menorCutwidth }\t\t\t{ mediaCutwidth }\t\t\t\t{ menorQuantidadeVerticesMaiorCutWidth }\t\t\t\t\t\t\t\t\t\t{ menorSoma }\t\t\t{ menorMelhorIteracao.ToString().PadLeft(4, '0') }\t\t\t\t\t{ maiorMelhorIteracao.ToString().PadLeft(4, '0') }\t\t\t\t\t{ tempoMedio }\t\t\t{ string.Join(" | ", melhorSolucao.Select(x => x.ToString().PadLeft(2, '0'))) } ");
                }
            }
        }
    }
}