using Heuristicas.Metaheuristicas;
using System;

namespace Heuristicas
{
    class Program
    {
        static void Main(string[] args)
        {
            // Declaração de variáveis e definição valores default
            bool execucaoDebug = true;
            string instancia = "p17_16_24";
            string heuristica = "LAHC";

            int multiplicadorMemoriaLAHC = 100;
            int numeroMaximoRejeicoesLAHC = 10000;

            MetaHeuristicaBase metaHeuristica = null;

            // Caso a execução tenha vindo por linha de comando
            if (args.Length > 0)
            {
                execucaoDebug = false;
            }

            switch (heuristica)
            {
                case Constantes.HeuristicasImplementadas.LAHC:
                    metaHeuristica = new LAHC(instancia, multiplicadorMemoriaLAHC, numeroMaximoRejeicoesLAHC);
                    break;
                case Constantes.HeuristicasImplementadas.BuscaTabu:
                    metaHeuristica = new BuscaTabu(instancia);
                    break;
                default:
                    throw new Exception("Heurística não implementada.");
            }

            metaHeuristica.ExecutarHeuristica();

            if (execucaoDebug)
            {
                Console.WriteLine($"O valor encontrado para a melhor solução foi { metaHeuristica.FOMelhorSolucao }");
                Console.WriteLine($"Organização dos componentes: [ | { string.Join(" | ", metaHeuristica.MelhorSolucao) } | ]");
            }
            else
            {
                Console.Write(metaHeuristica.FOMelhorSolucao);
            }
            
            Console.ReadKey();
        }
    }
}