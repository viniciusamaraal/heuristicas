using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heuristicas.Metaheuristicas
{
    public class ILS : MetaHeuristicaBase
    {
        private int NumeroMaximoIteracoesSemMelhora { get; set; }
        private int NumeroMaximoIteracoesMesmoNivel { get; set; }

        public ILS(string instancia, bool logAtivo, double multiplicadorNumeroMaximoIteracoesSemMelhora, double divisorNumeroMaximoIteracoesMesmoNivel)
            : base(instancia, Constantes.HeuristicasImplementadas.ILS, logAtivo)
        {
            this.NumeroMaximoIteracoesSemMelhora = (int)(base.NumeroVertices * multiplicadorNumeroMaximoIteracoesSemMelhora);
            this.NumeroMaximoIteracoesMesmoNivel = (int)(this.NumeroMaximoIteracoesSemMelhora / divisorNumeroMaximoIteracoesMesmoNivel);
        }

        public override Task ExecutarMetaheuristica()
        {
            return Task.Factory.StartNew(() =>
            {
                int iterAtual = 1, melhorIter = 0, nivelAtual = 1, iterMesmoNivel = 1, foSolucaoAtual = 0, foSolucaoPerturbadaAposDescida = 0;
                List<int> solucaoPerturbada;

                var solucaoAtual = GerarSolucaoAleatoria();
                foSolucaoAtual = ExecutarFuncaoAvaliacao(solucaoAtual).Max(x => x.Value);

                while (iterAtual - melhorIter < this.NumeroMaximoIteracoesSemMelhora)
                {
                    solucaoPerturbada = PerturbarVetor(solucaoAtual, nivelAtual);
                    ExecutarDescidaFistImprovement(solucaoPerturbada);

                    foSolucaoPerturbadaAposDescida = ExecutarFuncaoAvaliacao(solucaoPerturbada).Max(x => x.Value);

                    GravarLogDuranteExecucao($"{ iterAtual }; {nivelAtual}; {foSolucaoAtual}; { foSolucaoPerturbadaAposDescida }; {  string.Join(" | ", solucaoAtual.Select(x => x.ToString().PadLeft(2, '0'))) }");

                    if (foSolucaoPerturbadaAposDescida < foSolucaoAtual)
                    {
                        solucaoAtual = new List<int>(solucaoPerturbada);
                        foSolucaoAtual = foSolucaoPerturbadaAposDescida;

                        melhorIter = iterAtual;
                        nivelAtual = 1;
                        iterMesmoNivel = 1;

                        this.IteracoesMelhoraSolucaoGlobal.Add(iterAtual);
                    }
                    else
                    {
                        if (iterMesmoNivel > NumeroMaximoIteracoesMesmoNivel)
                        {
                            nivelAtual++;
                            iterMesmoNivel = 1;
                        }
                        else
                            iterMesmoNivel++;
                    }

                    iterAtual++;
                }

                MelhorSolucao = new List<int>(solucaoAtual);
                FOMelhorSolucao = foSolucaoAtual;

                GravarLogDuranteExecucao($"\n\nMelhorias solução global: {string.Join(" | ", base.IteracoesMelhoraSolucaoGlobal) }");
                GravarLogDuranteExecucao($"Cutdwidth: { base.FOMelhorSolucao }");
                GravarLogDuranteExecucao($"Solução Final: {  string.Join(" | ", MelhorSolucao.Select(x => x.ToString().PadLeft(2, '0'))) }");
            });
        }

        private List<int> PerturbarVetor(List<int> solucaoAtual, int nivelAtual)
        {
            var solucaoPerturbada = new List<int>(solucaoAtual.Count);
            solucaoPerturbada = new List<int>(solucaoAtual);

            int posicao1 = 0, posicao2 = 0, aux = 0;

            int numeroTrocas = 0;
            while (numeroTrocas < nivelAtual)
            {
                Util.GerarDoisNumerosAleatoriosDiferentes(0, solucaoAtual.Count, ref posicao1, ref posicao2);

                aux = solucaoPerturbada[posicao1];
                solucaoPerturbada[posicao1] = solucaoPerturbada[posicao2];
                solucaoPerturbada[posicao2] = aux;

                numeroTrocas++;
            }

            return solucaoPerturbada;
        }

        private void ExecutarDescidaFistImprovement(List<int> solucaoPerturbada)
        {
            int foSolucaoAtual = 0, foPrimeiroMelhorVizinho = 0, melhor_i = -1, melhor_j = -1, aux = -1;
            bool melhorou;

            foSolucaoAtual = ExecutarFuncaoAvaliacao(solucaoPerturbada).Max(x => x.Value);

            do
            {
                melhorou = false;
                foPrimeiroMelhorVizinho = CalcularPrimeiroMelhorVizinho(solucaoPerturbada, foSolucaoAtual, ref melhor_i, ref melhor_j);
                if (foPrimeiroMelhorVizinho < foSolucaoAtual)
                {
                    aux = solucaoPerturbada[melhor_j];
                    solucaoPerturbada[melhor_j] = solucaoPerturbada[melhor_i];
                    solucaoPerturbada[melhor_i] = aux;

                    foSolucaoAtual = foPrimeiroMelhorVizinho;

                    melhorou = true;
                }
            } while (melhorou);
        }

        private int CalcularPrimeiroMelhorVizinho(List<int> solucaoAtual, int foSolucaoAtual, ref int melhor_i, ref int melhor_j)
        {
            int aux, foVizinho, foMelhorVizinho = foSolucaoAtual;
            bool encontrou = false;

            for (int i = 0; i < solucaoAtual.Count - 1 && !encontrou; i++)
            {
                for (int j = i + 1; j < solucaoAtual.Count && !encontrou; j++)
                {
                    aux = solucaoAtual[j];
                    solucaoAtual[j] = solucaoAtual[i];
                    solucaoAtual[i] = aux;

                    foVizinho = ExecutarFuncaoAvaliacao(solucaoAtual).Max(x => x.Value);

                    if (foVizinho < foSolucaoAtual)
                    {
                        melhor_i = i;
                        melhor_j = j;
                        foMelhorVizinho = foVizinho;

                        encontrou = true;
                    }

                    aux = solucaoAtual[j];
                    solucaoAtual[j] = solucaoAtual[i];
                    solucaoAtual[i] = aux;
                }
            }

            return foMelhorVizinho;
        }
    }
}
