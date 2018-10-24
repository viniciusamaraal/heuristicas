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

            // Declaração de variáveis e definição valores default
            var listaInstancias = new List<string>();
            string heuristica = Constantes.HeuristicasImplementadas.BuscaTabu;

            int multiplicadorMemoriaLAHC = 100;
            int numeroMaximoRejeicoesLAHC = 10000;

            double multiplicadorIteracoesSemMelhoraBT = 20; // multiplicado pelo número de vértices do grafo
            double multiplicadorIteracoesProibicaoListaBT = 1; // multiplicado pelo número de vértices do grafo

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
                listaInstancias.Add("p31_18_21"); // TODO: remover (apenas teste)

                if (!listaInstancias.Any())
                {
                    string diretorioInstancias = ConfigurationManager.AppSettings["CAMINHO_BASE_INSTANCIAS"];
                    var diretorio = new DirectoryInfo(diretorioInstancias);
                    var arquivosDiretorio = diretorio.GetFiles();

                    foreach (FileInfo arquivo in arquivosDiretorio)
                    {
                        if (arquivo.Name != "p100_24_34" && listaInstancias.Count < 10) // TODO: remover (apenas teste)
                            listaInstancias.Add(arquivo.Name);
                    }
                    
                    listaInstancias = listaInstancias.OrderBy(x => x).ToList();
                }
            }

            string arquivoLogGeral = string.Format(ConfigurationManager.AppSettings["CAMINHO_ARQUIVO_LOG_GERAL"]);
            if (File.Exists(arquivoLogGeral))
                File.Delete(arquivoLogGeral);

            foreach (var instancia in listaInstancias)
            {
                switch (heuristica)
                {
                    case Constantes.HeuristicasImplementadas.LAHC:
                        metaHeuristica = new LAHC(instancia, execucaoDebug, arquivoLogGeral, multiplicadorMemoriaLAHC, numeroMaximoRejeicoesLAHC);
                        break;
                    case Constantes.HeuristicasImplementadas.BuscaTabu:
                        metaHeuristica = new BuscaTabu(instancia, execucaoDebug, arquivoLogGeral, multiplicadorIteracoesSemMelhoraBT, multiplicadorIteracoesProibicaoListaBT);
                        break;
                    default:
                        throw new Exception("Heurística não implementada.");
                }

                metaHeuristica.ExecutarMetaheuristica();
                metaHeuristica.GravarArquivoLogGeral();
            }
            
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
    }
}