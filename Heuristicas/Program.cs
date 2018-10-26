using Heuristicas.Metaheuristicas;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace Heuristicas
{
    class Program
    {
        static void Main(string[] args)
        {
            bool execucaoDebug = true;

            int quantidadeExcecucoes = 3;
            var informacoesExecucaoInstancias = new Dictionary<string, List<MetaHeuristicaBase>>();

            // Declaração de variáveis e definição valores default
            var listaInstancias = new List<string>();
            string heuristica = Constantes.HeuristicasImplementadas.BuscaTabu;

            int multiplicadorMemoriaLAHC = 100;
            int numeroMaximoRejeicoesLAHC = 10000;

            double multiplicadorIteracoesSemMelhoraBT = 50; // multiplicado pelo número de vértices do grafo
            double multiplicadorIteracoesProibicaoListaBT = 0.5 ; // multiplicado pelo número de vértices do grafo

            MetaHeuristicaBase metaHeuristica = null;

            // Caso a execução tenha sido chamada via linha de comando, recupera os parâmetros enviados
            if (args.Length > 0)
            {
                listaInstancias.Add(args[0]);
                heuristica = args[1];

                switch (heuristica)
                {
                    case Constantes.HeuristicasImplementadas.LAHC:
                        multiplicadorMemoriaLAHC = int.Parse(args[2]);
                        numeroMaximoRejeicoesLAHC = int.Parse(args[3]);
                        break;
                    case Constantes.HeuristicasImplementadas.BuscaTabu:
                        multiplicadorIteracoesSemMelhoraBT = int.Parse(args[2]);
                        multiplicadorIteracoesProibicaoListaBT = int.Parse(args[3]);
                        break;
                    default:
                        throw new Exception("Heurística não implementada.");
                }

                execucaoDebug = false;
            }
            else
            {
                //listaInstancias.Add("p38_18_19");

                //listaInstancias.Add("p31_18_21");
                //listaInstancias.Add("p37_18_20");
                //istaInstancias.Add("p50_19_25");
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

            foreach (var instancia in listaInstancias)
            {
                informacoesExecucaoInstancias.Add(instancia, new List<MetaHeuristicaBase>());

                for (int i = 0; i < quantidadeExcecucoes; i++)
                {
                    switch (heuristica)
                    {
                        case Constantes.HeuristicasImplementadas.LAHC:
                            metaHeuristica = new LAHC(instancia, execucaoDebug, multiplicadorMemoriaLAHC, numeroMaximoRejeicoesLAHC);
                            break;
                        case Constantes.HeuristicasImplementadas.BuscaTabu:
                            metaHeuristica = new BuscaTabu(instancia, execucaoDebug, multiplicadorIteracoesSemMelhoraBT, multiplicadorIteracoesProibicaoListaBT);
                            break;
                        default:
                            throw new Exception("Heurística não implementada.");
                    }

                    metaHeuristica.ExecutarMetaheuristica();

                    informacoesExecucaoInstancias[instancia].Add(metaHeuristica);
                }
            }

            GravarLogGeral(informacoesExecucaoInstancias);

            if (listaInstancias.Count == 1)
            {
                if (execucaoDebug)
                {
                    Console.SetWindowSize(200, 60);
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
                escritorArquivo.WriteLine("Instância\t Cut\t Avg");
                foreach (var instancia in informacoesExecucaoInstancias)
                {
                    int menorCutwidth = instancia.Value.Min(x => x.FOMelhorSolucao);
                    double mediaCutwidth = instancia.Value.Average(x => x.FOMelhorSolucao);

                    escritorArquivo.WriteLine($"{ instancia.Value.First().Instancia }\t { menorCutwidth }\t\t { mediaCutwidth } ");
                }
            }
        }
    }
}