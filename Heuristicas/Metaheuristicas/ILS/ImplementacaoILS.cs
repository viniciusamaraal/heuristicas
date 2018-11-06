using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heuristicas.Metaheuristicas.ILS
{
    public class ImplementacaoILS : MetaHeuristicaBase
    {
        private int NumeroMaximoIteracoesSemMelhora { get; set; }
        private int NumeroMaximoIteracoesMesmoNivel { get; set; }

        public ImplementacaoILS(string instancia, bool logAtivo, double multiplicadorNumeroMaximoIteracoesSemMelhora, double divisorNumeroMaximoIteracoesMesmoNivel)
            : base(instancia, Constantes.HeuristicasImplementadas.ILS, logAtivo)
        {
            this.NumeroMaximoIteracoesSemMelhora = (int)(base.NumeroVertices * multiplicadorNumeroMaximoIteracoesSemMelhora);
            this.NumeroMaximoIteracoesMesmoNivel = (int)(this.NumeroMaximoIteracoesSemMelhora / divisorNumeroMaximoIteracoesMesmoNivel);
        }

        public override Task ExecutarMetaheuristica()
        {
            return Task.Factory.StartNew(() =>
            {
                int iterAtual = 1, melhorIter = 0, nivelAtual = 1, iterMesmoNivel = 1, foMenorCutwidthSolucaoAtual = 0, foMenorSomaCutwidthSolucaoAtual = 0, foMenorCutwidthSolucaoPerturbadaAposDescida = 0, foMenorSomaCutwidthSolucaoPerturbadaAposDescida;
                List<int> solucaoPerturbada;

                var solucaoAtual = GerarSolucaoAleatoria();

                ExecutarFuncaoAvaliacao(solucaoAtual);
                foMenorCutwidthSolucaoAtual = CutwidthGrafo.Max(x => x.Value);
                foMenorSomaCutwidthSolucaoAtual = CutwidthGrafo.Sum(x => x.Value);

                while (iterAtual - melhorIter < this.NumeroMaximoIteracoesSemMelhora)
                {
                    solucaoPerturbada = PerturbarVetor(solucaoAtual, nivelAtual);
                    ExecutarDescidaFistImprovement(solucaoPerturbada);

                    ExecutarFuncaoAvaliacao(solucaoPerturbada);
                    foMenorCutwidthSolucaoPerturbadaAposDescida = CutwidthGrafo.Max(x => x.Value);
                    foMenorSomaCutwidthSolucaoPerturbadaAposDescida = CutwidthGrafo.Sum(x => x.Value);

                    GravarLogDuranteExecucao($"{ iterAtual }; {nivelAtual}; {foMenorCutwidthSolucaoAtual}; { foMenorCutwidthSolucaoPerturbadaAposDescida }; {  string.Join(" | ", solucaoAtual.Select(x => x.ToString().PadLeft(2, '0'))) }");

                    if (foMenorCutwidthSolucaoPerturbadaAposDescida < foMenorCutwidthSolucaoAtual || (foMenorCutwidthSolucaoPerturbadaAposDescida == foMenorCutwidthSolucaoAtual && foMenorSomaCutwidthSolucaoPerturbadaAposDescida < foMenorSomaCutwidthSolucaoAtual))
                    {
                        solucaoAtual = new List<int>(solucaoPerturbada);
                        foMenorCutwidthSolucaoAtual = foMenorCutwidthSolucaoPerturbadaAposDescida;
                        foMenorSomaCutwidthSolucaoAtual = foMenorSomaCutwidthSolucaoPerturbadaAposDescida;

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
                FOMenorCutwidthMelhorSolucao = foMenorCutwidthSolucaoAtual;

                GravarLogDuranteExecucao($"\n\nMelhorias solução global: {string.Join(" | ", base.IteracoesMelhoraSolucaoGlobal) }");
                GravarLogDuranteExecucao($"Cutdwidth: { base.FOMenorCutwidthMelhorSolucao }");
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
            int foMenorCutwidthSolucaoAtual = 0, foMenorSomaCutwidthSolucaoAtual = 0, foMenorCutwidthPrimeiroMelhorVizinho = 0, foMenorSomaCutwidthPrimeiroMelhorVizinho = 0, melhor_i = -1, melhor_j = -1, aux = -1;
            bool melhorou;

            ExecutarFuncaoAvaliacao(solucaoPerturbada);
            foMenorCutwidthSolucaoAtual = CutwidthGrafo.Max(x => x.Value);
            foMenorSomaCutwidthSolucaoAtual = CutwidthGrafo.Sum(x => x.Value);

            do
            {
                melhorou = false;
                var resultadoBuscaLocal = CalcularPrimeiroMelhorVizinho(solucaoPerturbada, foMenorCutwidthSolucaoAtual, foMenorSomaCutwidthSolucaoAtual, ref melhor_i, ref melhor_j);
                foMenorCutwidthPrimeiroMelhorVizinho = resultadoBuscaLocal.Item1;
                foMenorSomaCutwidthPrimeiroMelhorVizinho = resultadoBuscaLocal.Item2;

                if (foMenorCutwidthPrimeiroMelhorVizinho < foMenorCutwidthSolucaoAtual || (foMenorCutwidthPrimeiroMelhorVizinho == foMenorCutwidthSolucaoAtual && foMenorSomaCutwidthPrimeiroMelhorVizinho < foMenorSomaCutwidthSolucaoAtual))
                {
                    aux = solucaoPerturbada[melhor_j];
                    solucaoPerturbada[melhor_j] = solucaoPerturbada[melhor_i];
                    solucaoPerturbada[melhor_i] = aux;

                    foMenorCutwidthSolucaoAtual = foMenorCutwidthPrimeiroMelhorVizinho;
                    foMenorSomaCutwidthSolucaoAtual = foMenorSomaCutwidthPrimeiroMelhorVizinho;

                    melhorou = true;
                }
            } while (melhorou);
        }

        private Tuple<int, int> CalcularPrimeiroMelhorVizinho(List<int> solucaoAtual, int foMenorCutwidthSolucaoAtual, int foMenorSomaCutwidthSolucaoAtual, ref int melhor_i, ref int melhor_j)
        {
            int aux, foMenorCutwidthVizinho, foMenorSomaCutwidthVizinho, foMenorCutwidthMelhorVizinho = foMenorCutwidthSolucaoAtual, foMenorSomaCutwidthMelhorVizinho = foMenorSomaCutwidthSolucaoAtual;
            bool encontrou = false;

            for (int i = 0; i < solucaoAtual.Count - 1 && !encontrou; i++)
            {
                for (int j = i + 1; j < solucaoAtual.Count && !encontrou; j++)
                {
                    aux = solucaoAtual[j];
                    solucaoAtual[j] = solucaoAtual[i];
                    solucaoAtual[i] = aux;

                    ExecutarFuncaoAvaliacao(solucaoAtual);
                    foMenorCutwidthVizinho = CutwidthGrafo.Max(x => x.Value);
                    foMenorSomaCutwidthVizinho = CutwidthGrafo.Sum(x => x.Value);

                    if (foMenorCutwidthVizinho < foMenorCutwidthSolucaoAtual || (foMenorCutwidthVizinho == foMenorCutwidthSolucaoAtual && foMenorSomaCutwidthVizinho < foMenorSomaCutwidthSolucaoAtual))
                    {
                        melhor_i = i;
                        melhor_j = j;
                        foMenorCutwidthMelhorVizinho = foMenorCutwidthVizinho;
                        foMenorSomaCutwidthMelhorVizinho = foMenorSomaCutwidthVizinho;

                        encontrou = true;
                    }

                    aux = solucaoAtual[j];
                    solucaoAtual[j] = solucaoAtual[i];
                    solucaoAtual[i] = aux;
                }
            }

            return Tuple.Create<int, int>(foMenorCutwidthMelhorVizinho, foMenorSomaCutwidthMelhorVizinho);
        }
    }
}