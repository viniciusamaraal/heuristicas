using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heuristicas.Metaheuristicas
{
    public class LAHC : MetaHeuristicaBase
    {
        private int TamanhoMemoria { get; set; }
        private int NumeroMaximoRejeicoesConsecutivas { get; set; }

        public LAHC(string instancia, bool logAtivo, double numeroMaximoRejeicoesConsecutivas, double multiplicadorTamanhoMemoria)
            :base (instancia, Constantes.HeuristicasImplementadas.LAHC, logAtivo)
        {
            this.NumeroMaximoRejeicoesConsecutivas = (int)(base.NumeroVertices * numeroMaximoRejeicoesConsecutivas); ;
            this.TamanhoMemoria = (int)(base.NumeroVertices * multiplicadorTamanhoMemoria);
        }

        private int[] CriarMemoria(int foSolucaoInicial, int tamanhoMemoria)
        {
            var memoria = new int[tamanhoMemoria];

            for (int i = 0; i < tamanhoMemoria; i++)
                memoria[i] = foSolucaoInicial;

            return memoria;
        }

        /// <summary>
        /// Executa movimento vizinho aleatório
        /// </summary>
        /// <param name="solucaoAtual"> Solução que será modificada pelo movimento </param>
        /// <returns> Retorna um vetor contendo a nova solução após a execução do movimento </returns>
        private int[] ExecutarMovimento(int[] solucaoAtual)
        {
            var r = new Random();

            // Gera duas posições diferentes de modo aleatório
            int posicao1 = -1, posicao2 = -1;
            Util.GerarDoisNumerosAleatoriosDiferentes(0, solucaoAtual.Length, ref posicao1, ref posicao2);

            // Realiza a troca de posições de acordo com os índices gerados
            int[] solucaoVizinha = (int[])solucaoAtual.Clone();
            int posicaoAux = solucaoVizinha[posicao1];
            solucaoVizinha[posicao1] = solucaoVizinha[posicao2];
            solucaoVizinha[posicao2] = posicaoAux;

            return solucaoVizinha;
        }

        public override Task ExecutarMetaheuristica()
        {
            return Task.Factory.StartNew(() =>
            {
                var solucaoAtual = GerarSolucaoAleatoria(); // new int[] { 3, 1, 4, 5, 2, 6 }; // 
                int foSolucaoAtual = ExecutarFuncaoAvaliacao(solucaoAtual);

                MelhorSolucao = (int[])solucaoAtual.Clone();
                FOMelhorSolucao = foSolucaoAtual;

                int qtdVertices = Grafo.Count;
                int controleMemoria = 0;

                var memoria = CriarMemoria(foSolucaoAtual, this.TamanhoMemoria);

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
                    controleMemoria = (controleMemoria + 1) % (this.TamanhoMemoria - 1);
                    numeroRejeicoes++;
                }
            });
        }
    }
}
