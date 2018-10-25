using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heuristicas.Metaheuristicas
{
    public class LAHC : MetaHeuristicaBase
    {
        private int MultiplicadorTamanhoMemoria { get; set; }
        private int NumeroMaximoRejeicoesConsecutivas { get; set; }

        public LAHC(string instancia, bool logAtivo, int multiplicadorMemoria, int numeroMaximoRejeicoesConsecutivas)
            :base (instancia, Constantes.HeuristicasImplementadas.LAHC, logAtivo)
        {
            this.MultiplicadorTamanhoMemoria = multiplicadorMemoria;
            this.NumeroMaximoRejeicoesConsecutivas = NumeroMaximoRejeicoesConsecutivas;
        }

        private int[] CriarMemoria(int foSolucaoInicial, int tamanhoMemoria)
        {
            var memoria = new int[tamanhoMemoria];

            for (int i = 0; i < tamanhoMemoria; i++)
                memoria[i] = foSolucaoInicial;

            return memoria;
        }

        public override void ExecutarMetaheuristica()
        {
            var solucaoAtual = GerarSolucaoInicial(); // new int[] { 3, 1, 4, 5, 2, 6 }; // 
            int foSolucaoAtual = ExecutarFuncaoAvaliacao(solucaoAtual);

            MelhorSolucao = (int[])solucaoAtual.Clone();
            FOMelhorSolucao = foSolucaoAtual;

            int qtdVertices = Grafo.Count;
            int controleMemoria = 0;
            int tamanhoMaximoMemoria = qtdVertices * this.MultiplicadorTamanhoMemoria;

            var memoria = CriarMemoria(foSolucaoAtual, tamanhoMaximoMemoria);

            int numeroRejeicoes = 0;
            while (numeroRejeicoes < this.NumeroMaximoRejeicoesConsecutivas)
            {
                var solucaoVizinha = ExecutarMovimento(solucaoAtual);
                int foSolucaoVizinha = ExecutarFuncaoAvaliacao(solucaoVizinha);

                if (foSolucaoVizinha < foSolucaoAtual || foSolucaoVizinha < memoria[controleMemoria])
                {
                    if (foSolucaoVizinha < foSolucaoAtual)
                        numeroRejeicoes = 0;

                    solucaoAtual = (int[])solucaoVizinha.Clone();
                    foSolucaoAtual = foSolucaoVizinha;

                    if (foSolucaoVizinha < FOMelhorSolucao)
                    {
                        FOMelhorSolucao = foSolucaoVizinha;
                        MelhorSolucao = (int[])solucaoVizinha.Clone();
                    }
                }

                memoria[controleMemoria] = foSolucaoAtual;
                controleMemoria = (controleMemoria + 1) % (tamanhoMaximoMemoria - 1);
                numeroRejeicoes++;
            }
        }
    }
}
