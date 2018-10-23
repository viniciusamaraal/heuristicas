using Heuristicas.Metaheuristicas;
using System;

namespace Heuristicas
{
    class Program
    {
        static void Main(string[] args)
        {
            bool execucaoDebug = true;

            // Declaração de variáveis e definição valores default
            string instancia = "p17_16_24";
            string heuristica = Constantes.HeuristicasImplementadas.BuscaTabu;

            int multiplicadorMemoriaLAHC = 100;
            int numeroMaximoRejeicoesLAHC = 10000;

            int numeroMaximoIteracoesSemMelhoraBT = 100;
            int numeroIteracoesProibicaoListaBT = 7;

            MetaHeuristicaBase metaHeuristica = null;

            // Caso a execução tenha sido chamada via linha de comando, recupera os parâmetros enviados
            if (args.Length > 0)
            {
                instancia = args[0];
                heuristica = args[1];

                switch (heuristica)
                {
                    case Constantes.HeuristicasImplementadas.LAHC:
                        multiplicadorMemoriaLAHC = int.Parse(args[2]);
                        numeroMaximoRejeicoesLAHC = int.Parse(args[3]);
                        break;
                    case Constantes.HeuristicasImplementadas.BuscaTabu:
                        numeroMaximoIteracoesSemMelhoraBT = int.Parse(args[2]);
                        numeroIteracoesProibicaoListaBT = int.Parse(args[3]);
                        break;
                    default:
                        throw new Exception("Heurística não implementada.");
                }

                execucaoDebug = false;
            }

            switch (heuristica)
            {
                case Constantes.HeuristicasImplementadas.LAHC:
                    metaHeuristica = new LAHC(instancia, multiplicadorMemoriaLAHC, numeroMaximoRejeicoesLAHC);
                    break;
                case Constantes.HeuristicasImplementadas.BuscaTabu:
                    metaHeuristica = new BuscaTabu(instancia, numeroMaximoIteracoesSemMelhoraBT, numeroIteracoesProibicaoListaBT);
                    break;
                default:
                    throw new Exception("Heurística não implementada.");
            }

            metaHeuristica.ExecutarHeuristica();

            if (execucaoDebug)
            {
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