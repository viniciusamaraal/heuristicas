using System;
using System.Linq;

namespace Heuristicas.Metaheuristicas
{
    public class Restricao
    {
        public int IteracaoProibicao { get; set; }
        public int QuantidadeTrocas { get; set; }

        public Restricao()
        {
            this.IteracaoProibicao = -1;
            this.QuantidadeTrocas = 0;
        }
    }

    public class EstruturaTabu
    {
        public Restricao[,] ListaRestricoes { get; set; }

        public EstruturaTabu(int dimensao)
        {
            ListaRestricoes = new Restricao[dimensao, dimensao];

            for (int i = 0; i < ListaRestricoes.GetLength(0); i++)
            {
                for (int j = 0; j < ListaRestricoes.GetLength(1); j++)
                {
                    ListaRestricoes[i, j] = new Restricao();
                }
            }
        }
        
        public void DefinirTabu(int i, int j, int iteracaoProibicao)
        {
            ListaRestricoes[i, j].IteracaoProibicao = iteracaoProibicao;
            ListaRestricoes[i, j].QuantidadeTrocas++;

            ListaRestricoes[j, i].IteracaoProibicao = iteracaoProibicao;
            ListaRestricoes[j, i].QuantidadeTrocas++;
        }

        public bool ElementoProibido(int i, int j, int iteracaoAtual)
        {
            return ListaRestricoes[i, j].IteracaoProibicao >= iteracaoAtual;
        }
    }

    public class BuscaTabu : MetaHeuristicaBase
    {
        private int NumeroMaximoIteracoesSemMelhora { get; set; }
        private int NumeroIteracoesProibicaoLista { get; set; }

        public BuscaTabu(string instancia, bool logAtivo, int numeroMaximoIteracoesSemMelhora, int numeroIteracoesProibicaoLista)
            : base(instancia, Constantes.HeuristicasImplementadas.BuscaTabu, logAtivo)
        {
            this.NumeroMaximoIteracoesSemMelhora = numeroMaximoIteracoesSemMelhora;
            this.NumeroIteracoesProibicaoLista = numeroIteracoesProibicaoLista;
        }

        public override void ExecutarMetaheuristica()
        {
            int iterAtual = 0, melhorIter = 0, melhor_i = -1, melhor_j = -1, foSolucaoAtual = 0;

            var estruturaTabu = new EstruturaTabu(base.NumeroVertices);

            var solucaoAtual = GerarSolucaoInicial(); 
            Array.Copy(solucaoAtual, MelhorSolucao, solucaoAtual.Length);

            foSolucaoAtual = FOMelhorSolucao = ExecutarFuncaoAvaliacao(solucaoAtual);

            GravarLog($"{ melhor_i }; { melhor_j }; { foSolucaoAtual }; {  string.Join(" | ", solucaoAtual.Select(x => x.ToString().PadLeft(2, '0'))) }\n");

            while (iterAtual - melhorIter < this.NumeroMaximoIteracoesSemMelhora)
            {
                iterAtual++;

                CalcularMelhorVizinho(solucaoAtual, iterAtual, estruturaTabu, ref melhor_i, ref melhor_j, ref foSolucaoAtual);

                // Troca os elementos de acordo com a melhor vizinhança retornada
                int aux = solucaoAtual[melhor_i];
                solucaoAtual[melhor_i] = solucaoAtual[melhor_j];
                solucaoAtual[melhor_j] = aux;

                GravarLog($"{ melhor_i.ToString().PadLeft(2, '0') }; { melhor_j.ToString().PadLeft(2, '0') }; { foSolucaoAtual.ToString().PadLeft(2, '0') }; {  string.Join(" | ", solucaoAtual.Select(x => x.ToString().PadLeft(2, '0'))) }");

                estruturaTabu.DefinirTabu(melhor_i, melhor_j, iterAtual + this.NumeroIteracoesProibicaoLista);

                if (foSolucaoAtual < FOMelhorSolucao)
                {
                    melhorIter = iterAtual;
                    FOMelhorSolucao = foSolucaoAtual;

                    Array.Copy(solucaoAtual, MelhorSolucao, solucaoAtual.Length);

                    this.IteracoesMelhoraSolucaoGlobal.Add(iterAtual);
                }
            }

            GravarLog($"\n\nMelhorias solução global: {string.Join(" | ", base.IteracoesMelhoraSolucaoGlobal) }");
            GravarLog($"Cutdwidth: { base.FOMelhorSolucao }");
            GravarLog($"Solução Final: {  string.Join(" | ", MelhorSolucao.Select(x => x.ToString().PadLeft(2, '0'))) }");
        }

        private void CalcularMelhorVizinho(int[] solucaoAtual, int iteracaoAtual, EstruturaTabu estruturaTabu, ref int melhor_i, ref int melhor_j, ref int foSolucaoAtual)
        {
            int aux;
            int foAtual = 0, foVizinho = 0;

            foSolucaoAtual = int.MaxValue;

            for (int i = 0; i < solucaoAtual.Length - 1; i++)
            {
                for (int j = i + 1; j < solucaoAtual.Length; j++)
                {
                    foAtual = ExecutarFuncaoAvaliacao(solucaoAtual);

                    // Faz o movimento de troca da vizinhança
                    aux = solucaoAtual[j];
                    solucaoAtual[j] = solucaoAtual[i];
                    solucaoAtual[i] = aux;

                    foVizinho = ExecutarFuncaoAvaliacao(solucaoAtual);

                    // se a lista tabu não restringe o elemento ou, mesmo que haja restrição, o resultado da função objetivo encontrado no momento é melhor que a melhor solução (fo_star)
                    if (foVizinho < FOMelhorSolucao || !estruturaTabu.ElementoProibido(i, j, iteracaoAtual))
                    {
                        if (foVizinho < foSolucaoAtual)
                        {
                            melhor_i = i;
                            melhor_j = j;
                            foSolucaoAtual = foVizinho;
                        }
                    }

                    // Desfaz o movimento de troca para analisar o restante da vizinhança
                    aux = solucaoAtual[j];
                    solucaoAtual[j] = solucaoAtual[i];
                    solucaoAtual[i] = aux;
                }
            }
        }
    }
}