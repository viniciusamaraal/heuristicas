using System;
using System.Linq;

namespace Heuristicas.Metaheuristicas
{
    public class Restricao
    {
        public int IteracaoProibicao { get; set; }
        public int QuantidadeIteracoesProibicao { get; set; }
        public int QuantidadeTrocas { get; set; }

        public Restricao()
        {
            this.IteracaoProibicao = -1;
            this.QuantidadeTrocas = 0;
        }
    }

    public class EstruturaTabu
    {
        private int QuantidadeIteracoesPriobicao { get; set; }
        private Restricao[,] ListaRestricoes { get; set; }

        public EstruturaTabu(int dimensao, int qtdIteracoesProibicao)
        {
            this.QuantidadeIteracoesPriobicao = qtdIteracoesProibicao;

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
            ListaRestricoes[i, j].IteracaoProibicao = iteracaoProibicao + this.QuantidadeIteracoesPriobicao;
            ListaRestricoes[i, j].QuantidadeIteracoesProibicao += this.QuantidadeIteracoesPriobicao;
            ListaRestricoes[i, j].QuantidadeTrocas++;

            ListaRestricoes[j, i].IteracaoProibicao = iteracaoProibicao + this.QuantidadeIteracoesPriobicao;
            ListaRestricoes[j, i].QuantidadeIteracoesProibicao += this.QuantidadeIteracoesPriobicao;
            ListaRestricoes[j, i].QuantidadeTrocas++;
        }

        public bool ElementoProibido(int i, int j, int iteracaoAtual)
        {
            return ListaRestricoes[i, j].IteracaoProibicao >= iteracaoAtual;
        }
        
        public int QuantidadeTrocas(int i, int j)
        {
            return this.ListaRestricoes[i, j].QuantidadeTrocas;
        }

        public void ImprimirTrocasListaTabu()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\t\t\t\tQuantidade de trocas \n ");

            Console.Write("     ");
            for (int i = 0; i < ListaRestricoes.GetLength(0); i++)
                Console.Write($" { i.ToString().PadLeft(3, '0') } ");

            Console.ResetColor();
            
            for (int i = 0; i < ListaRestricoes.GetLength(0); i++)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($" { i.ToString().PadLeft(3, '0') } ");
                Console.ResetColor();

                for (int j = 0; j < ListaRestricoes.GetLength(1); j++)
                    Console.Write($" { ListaRestricoes[i, j].QuantidadeTrocas.ToString().PadLeft(3, '0') } ");
            }

            Console.WriteLine("\n");
        }

        public void ImprimirQuantidadeIteracoesProibicaoListaTabu()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\t\t\tQuantidade de iterações com movimento proibido \n ");

            Console.Write("     ");
            for (int i = 0; i < ListaRestricoes.GetLength(0); i++)
                Console.Write($" { i.ToString().PadLeft(3, '0') } ");

            Console.ResetColor();

            for (int i = 0; i < ListaRestricoes.GetLength(0); i++)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($" { i.ToString().PadLeft(3, '0') } ");
                Console.ResetColor();

                for (int j = 0; j < ListaRestricoes.GetLength(1); j++)
                    Console.Write($" { ListaRestricoes[i, j].QuantidadeIteracoesProibicao.ToString().PadLeft(3, '0') } ");
            }

            Console.WriteLine("\n");
        }
    }


    public class BuscaTabu : MetaHeuristicaBase
    {
        private int NumeroMaximoIteracoesSemMelhora { get; set; }
        private int NumeroIteracoesProibicaoLista { get; set; }

        public BuscaTabu(string instancia, bool logAtivo, string arquivoLogGeral, double multiplicadorNumeroMaximoIteracoesSemMelhora, double multiplicadorNumeroIteracoesProibicaoLista)
            : base(instancia, Constantes.HeuristicasImplementadas.BuscaTabu, logAtivo, arquivoLogGeral)
        {
            this.NumeroMaximoIteracoesSemMelhora = (int)(base.NumeroVertices * multiplicadorNumeroMaximoIteracoesSemMelhora);
            this.NumeroIteracoesProibicaoLista = (int)(base.NumeroVertices * multiplicadorNumeroIteracoesProibicaoLista);
        }

        public override void ExecutarMetaheuristica()
        {
            int iterAtual = 0, melhorIter = 0, melhor_i = -1, melhor_j = -1, foSolucaoAtual = 0;

            var estruturaTabu = new EstruturaTabu(base.NumeroVertices, this.NumeroIteracoesProibicaoLista);

            var solucaoAtual = GerarSolucaoInicial(); 
            Array.Copy(solucaoAtual, MelhorSolucao, solucaoAtual.Length);

            foSolucaoAtual = FOMelhorSolucao = ExecutarFuncaoAvaliacao(solucaoAtual);

            GravarLogDuranteExecucao($"{ melhor_i }; { melhor_j }; { foSolucaoAtual }; {  string.Join(" | ", solucaoAtual.Select(x => x.ToString().PadLeft(2, '0'))) }\n");

            while (iterAtual - melhorIter < this.NumeroMaximoIteracoesSemMelhora)
            {
                iterAtual++;

                CalcularMelhorVizinho(solucaoAtual, iterAtual, estruturaTabu, ref melhor_i, ref melhor_j, ref foSolucaoAtual);

                // Troca os elementos de acordo com a melhor vizinhança retornada
                int aux = solucaoAtual[melhor_i];
                solucaoAtual[melhor_i] = solucaoAtual[melhor_j];
                solucaoAtual[melhor_j] = aux;

                GravarLogDuranteExecucao($"{ melhor_i.ToString().PadLeft(2, '0') }; { melhor_j.ToString().PadLeft(2, '0') }; { foSolucaoAtual.ToString().PadLeft(2, '0') }; {  string.Join(" | ", solucaoAtual.Select(x => x.ToString().PadLeft(2, '0'))) }");

                estruturaTabu.DefinirTabu(melhor_i, melhor_j, iterAtual);

                if (foSolucaoAtual < FOMelhorSolucao)
                {
                    melhorIter = iterAtual;
                    FOMelhorSolucao = foSolucaoAtual;

                    Array.Copy(solucaoAtual, MelhorSolucao, solucaoAtual.Length);

                    this.IteracoesMelhoraSolucaoGlobal.Add(iterAtual);
                }
            }

            GravarLogDuranteExecucao($"\n\nMelhorias solução global: {string.Join(" | ", base.IteracoesMelhoraSolucaoGlobal) }");
            GravarLogDuranteExecucao($"Cutdwidth: { base.FOMelhorSolucao }");
            GravarLogDuranteExecucao($"Solução Final: {  string.Join(" | ", MelhorSolucao.Select(x => x.ToString().PadLeft(2, '0'))) }");

            estruturaTabu.ImprimirTrocasListaTabu();
            estruturaTabu.ImprimirQuantidadeIteracoesProibicaoListaTabu();
        }

        private void CalcularMelhorVizinho(int[] solucaoAtual, int iteracaoAtual, EstruturaTabu estruturaTabu, ref int melhor_i, ref int melhor_j, ref int foSolucaoAtual)
        {
            int teste = 1111; // apagar

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
                        if (foVizinho < foSolucaoAtual || (melhor_i >= 0 && melhor_j >= 0 && foVizinho == foSolucaoAtual && estruturaTabu.QuantidadeTrocas(i, j) < estruturaTabu.QuantidadeTrocas(melhor_i, melhor_j)))
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