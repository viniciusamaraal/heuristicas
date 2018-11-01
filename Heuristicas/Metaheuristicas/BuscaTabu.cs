using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Heuristicas.Metaheuristicas
{
    public class Restricao
    {
        public int IteracaoProibicao { get; set; }
        public int QuantidadeIteracoesProibicao { get; set; }
        public List<int> Visitas { get; set; }

        public Restricao()
        {
            this.IteracaoProibicao = -1;
            this.Visitas = new List<int>();
        }
    }

    public class EstruturaTabu
    {
        private int IncrementoTamanhoListaTabu { get; set; }
        private int QuantidadeIteracoesProibicaoOriginal { get; set; }
        private int QuantidadeIteracoesPriobicao { get; set; }
        private Restricao[,] ListaRestricoes { get; set; }

        public EstruturaTabu(int dimensao, int qtdIteracoesProibicao, int incrementoTamanhoListaTabu)
        {
            this.IncrementoTamanhoListaTabu = incrementoTamanhoListaTabu;
            this.QuantidadeIteracoesProibicaoOriginal = qtdIteracoesProibicao;
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

        public void IncrementarTamanhoLista()
        {
            this.QuantidadeIteracoesPriobicao += this.IncrementoTamanhoListaTabu;
        }

        public void ResetarTamanhoLista()
        {
            this.QuantidadeIteracoesPriobicao = this.QuantidadeIteracoesProibicaoOriginal;
        }
        
        public void DefinirTabu(int i, int j, int iteracaoProibicao)
        {
            ListaRestricoes[i, j].IteracaoProibicao = iteracaoProibicao + this.QuantidadeIteracoesPriobicao;
            ListaRestricoes[i, j].QuantidadeIteracoesProibicao += this.QuantidadeIteracoesPriobicao;
            ListaRestricoes[i, j].Visitas.Add(iteracaoProibicao);

            ListaRestricoes[j, i].IteracaoProibicao = iteracaoProibicao + this.QuantidadeIteracoesPriobicao;
            ListaRestricoes[j, i].QuantidadeIteracoesProibicao += this.QuantidadeIteracoesPriobicao;
            ListaRestricoes[j, i].Visitas.Add(iteracaoProibicao);
        }

        public bool ElementoProibido(int i, int j, int iteracaoAtual)
        {
            return ListaRestricoes[i, j].IteracaoProibicao >= iteracaoAtual;
        }
        
        public int QuantidadeTrocas(int i, int j)
        {
            return this.ListaRestricoes[i, j].Visitas.Count;
        }

        public void ImprimirTrocasListaTabu(string nomeInstancia)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\t\t\tQuantidade de trocas (instância { nomeInstancia }) \n ");

            Console.Write("     ");
            for (int i = 0; i < ListaRestricoes.GetLength(0); i++)
                Console.Write($" { (i + 1).ToString().PadLeft(3, '0') } ");

            Console.ResetColor();
            
            for (int i = 0; i < ListaRestricoes.GetLength(0); i++)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($" { (i + 1).ToString().PadLeft(3, '0') } ");
                Console.ResetColor();

                for (int j = 0; j < ListaRestricoes.GetLength(1); j++)
                {
                    int qtdTrocas = QuantidadeTrocas(i, j);

                    if (qtdTrocas == 0)
                        Console.ForegroundColor = ConsoleColor.DarkGray;

                    Console.Write($" { qtdTrocas.ToString().PadLeft(3, '0') } ");

                    if (qtdTrocas == 0)
                        Console.ResetColor();
                }
            }

            Console.WriteLine("\n");
        }

        public void ImprimirQuantidadeIteracoesProibicaoListaTabu(string nomeInstancia)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\t\tQuantidade de iterações com movimento proibido (instância { nomeInstancia }) \n ");

            Console.Write("     ");
            for (int i = 0; i < ListaRestricoes.GetLength(0); i++)
                Console.Write($" { (i + 1).ToString().PadLeft(3, '0') } ");

            Console.ResetColor();

            for (int i = 0; i < ListaRestricoes.GetLength(0); i++)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($" { (i + 1).ToString().PadLeft(3, '0') } ");
                Console.ResetColor();

                for (int j = 0; j < ListaRestricoes.GetLength(1); j++)
                {
                    if (ListaRestricoes[i, j].QuantidadeIteracoesProibicao == 0)
                        Console.ForegroundColor = ConsoleColor.DarkGray;

                    Console.Write($" { ListaRestricoes[i, j].QuantidadeIteracoesProibicao.ToString().PadLeft(3, '0') } ");

                    if (ListaRestricoes[i, j].QuantidadeIteracoesProibicao == 0)
                        Console.ResetColor();
                }
            }

            Console.WriteLine("\n");
        }
    }


    public class BuscaTabu : MetaHeuristicaBase
    {
        private int NumeroMaximoIteracoesSemMelhora { get; set; }
        private int NumeroIteracoesProibicaoLista { get; set; }
        private int IncrementoTamanhoListaTabu { get; set; }
        private int ModuloIteracaoSemMelhoraIncrementoListaTabu { get; set; }        

        public BuscaTabu(string instancia, bool logAtivo, double multiplicadorNumeroMaximoIteracoesSemMelhora, double multiplicadorNumeroIteracoesProibicaoLista, int incrementoTamanhoListaTabu, int moduloIteracaoSemMelhoraIncrementoListaTabu)
            : base(instancia, Constantes.HeuristicasImplementadas.BuscaTabu, logAtivo)
        {
            this.NumeroMaximoIteracoesSemMelhora = (int)(base.NumeroVertices * multiplicadorNumeroMaximoIteracoesSemMelhora);
            this.NumeroIteracoesProibicaoLista = (int)(base.NumeroVertices * multiplicadorNumeroIteracoesProibicaoLista);
            this.IncrementoTamanhoListaTabu = incrementoTamanhoListaTabu;
            this.ModuloIteracaoSemMelhoraIncrementoListaTabu = moduloIteracaoSemMelhoraIncrementoListaTabu;
        }

        public override Task ExecutarMetaheuristica()
        {
            return Task.Factory.StartNew(() =>
            {
                Cronometro.Start();

                int iterAtual = 0, melhor_i = -1, melhor_j = -1, foSolucaoAtual = 0;

                var estruturaTabu = new EstruturaTabu(base.NumeroVertices, this.NumeroIteracoesProibicaoLista, this.IncrementoTamanhoListaTabu);

                var solucaoAtual = new List<int> { 1, 15, 6, 2, 5, 9, 8, 13, 11, 3, 10, 7, 4, 12, 19, 17, 16, 14, 18 }; // GerarSolucaoAleatoria(); // GerarSolucaoInicial();
                MelhorSolucao = new List<int>(solucaoAtual);

                foSolucaoAtual = FOMelhorSolucao = ExecutarFuncaoAvaliacao(solucaoAtual).Max(x => x.Value);

                GravarLogDuranteExecucao($"{ melhor_i }; { melhor_j }; { foSolucaoAtual }; {  string.Join(" | ", solucaoAtual.Select(x => x.ToString().PadLeft(2, '0'))) }\n");

                while (iterAtual - this.MelhorIteracao < this.NumeroMaximoIteracoesSemMelhora)
                {
                    iterAtual++;

                    CalcularMelhorVizinhoBestImprovement(solucaoAtual, iterAtual, estruturaTabu, ref melhor_i, ref melhor_j, ref foSolucaoAtual);

                    // Troca os elementos de acordo com a melhor vizinhança retornada
                    int aux = solucaoAtual[melhor_i];
                    solucaoAtual[melhor_i] = solucaoAtual[melhor_j];
                    solucaoAtual[melhor_j] = aux;

                    GravarLogDuranteExecucao($"{ melhor_i.ToString().PadLeft(2, '0') }; { melhor_j.ToString().PadLeft(2, '0') }; { foSolucaoAtual.ToString().PadLeft(2, '0') }; {  string.Join(" | ", solucaoAtual.Select(x => x.ToString().PadLeft(2, '0'))) }");

                    estruturaTabu.DefinirTabu(melhor_i, melhor_j, iterAtual);

                    if (foSolucaoAtual < FOMelhorSolucao)
                    {
                        this.MelhorIteracao = iterAtual;
                        FOMelhorSolucao = foSolucaoAtual;

                        MelhorSolucao = new List<int>(solucaoAtual);

                        this.IteracoesMelhoraSolucaoGlobal.Add(iterAtual);

                        estruturaTabu.ResetarTamanhoLista();
                    }
                    else
                    {
                        if ((iterAtual - this.MelhorIteracao) % this.ModuloIteracaoSemMelhoraIncrementoListaTabu == 0)
                            estruturaTabu.IncrementarTamanhoLista();
                    }
                }

                Cronometro.Stop();

                GravarLogDuranteExecucao($"\n\nMelhorias solução global: {string.Join(" | ", base.IteracoesMelhoraSolucaoGlobal) }");
                GravarLogDuranteExecucao($"Cutdwidth: { base.FOMelhorSolucao }");
                GravarLogDuranteExecucao($"Solução Final: {  string.Join(" | ", MelhorSolucao.Select(x => x.ToString().PadLeft(2, '0'))) }");

                estruturaTabu.ImprimirTrocasListaTabu(base.Instancia);
                estruturaTabu.ImprimirQuantidadeIteracoesProibicaoListaTabu(base.Instancia);
            });
        }

        private void CalcularMelhorVizinhoBestImprovement(List<int> solucaoAtual, int iteracaoAtual, EstruturaTabu estruturaTabu, ref int melhor_i, ref int melhor_j, ref int foSolucaoAtual)
        {
            int aux;
            int foAtual = 0, foVizinho = 0;
            var listaCandidatos = new List<Tuple<int, int>>();

            var informacoesPosicoes = RetornarGrauVerticesPosicoes(solucaoAtual);
            var funcaoAvaliacaoSolucalInicial = ExecutarFuncaoAvaliacao(solucaoAtual);

            foSolucaoAtual = int.MaxValue;
            foAtual = funcaoAvaliacaoSolucalInicial.Max(x => x.Value);

            for (int i = 0; i < solucaoAtual.Count - 1; i++)
            {
                for (int j = i + 1; j < solucaoAtual.Count; j++)
                {
                    // Faz o movimento de troca da vizinhança
                    aux = solucaoAtual[j];
                    solucaoAtual[j] = solucaoAtual[i];
                    solucaoAtual[i] = aux;

                    foVizinho = ExecutarFuncaoAvaliacao(solucaoAtual).Max(x => x.Value);

                    // se a lista tabu não restringe o elemento ou, mesmo que haja restrição, o resultado da função objetivo encontrado no momento é melhor que a melhor solução (fo_star)
                    if (foVizinho < FOMelhorSolucao || !estruturaTabu.ElementoProibido(i, j, iteracaoAtual))
                    {
                        // Caso a nova solução melhore a solução atual ou seja igual à solução atual mas tenham sido feitas menos trocas
                        if (foVizinho < foSolucaoAtual)
                        {
                            listaCandidatos = new List<Tuple<int, int>>();
                            listaCandidatos.Add(Tuple.Create<int, int>(i, j));
                            foSolucaoAtual = foVizinho;

                            melhor_i = i;
                            melhor_j = j;
                        }

                        if (foVizinho == foSolucaoAtual && estruturaTabu.QuantidadeTrocas(i, j) < estruturaTabu.QuantidadeTrocas(melhor_i, melhor_j))
                            listaCandidatos.Add(Tuple.Create<int, int>(i, j));
                    }
                    
                    // Desfaz o movimento de troca para analisar o restante da vizinhança
                    aux = solucaoAtual[j];
                    solucaoAtual[j] = solucaoAtual[i];
                    solucaoAtual[i] = aux;

                    foAtual = foVizinho; // evitar o recalculo do fo atual a cada nova iteração
                }
            }

            // Dentre os melhores candidatos disponíveis, escolhe-se aleatoriamente algum deles
            int escolhaAleatoria = new Random().Next(0, listaCandidatos.Count);
            melhor_i = listaCandidatos[escolhaAleatoria].Item1;
            melhor_j = listaCandidatos[escolhaAleatoria].Item2;
        }

        private void ExecutarBuscaLocalAdaptada(int[] solucaoAtual)
        {
            bool houveMelhoria = false;

            int[] melhorSolucao = new int[solucaoAtual.Length];
            MelhorSolucao = new List<int>(solucaoAtual);

            do
            {

            } while (houveMelhoria);
        }

        private void RecuperarVerticesPassiveisMudanca(List<int> solucao)
        {
            var candidatosMelhoriaSolucao = new Dictionary<int, List<int>>();
            var informacoesPosicoes = RetornarGrauVerticesPosicoes(solucao);

            for (int i = 0; i < solucao.Count; i++)
            {
                int situacaoPosicao = informacoesPosicoes[i].GrauVerticeEsquerda - informacoesPosicoes[i].GrauVerticeEsquerda;
                if (situacaoPosicao == 0)
                    candidatosMelhoriaSolucao.Add(i, new List<int> { informacoesPosicoes[i].ObterPosicaoMediana() });
                else
                {
                    if (situacaoPosicao % 2 != 0)
                    {
                        candidatosMelhoriaSolucao.Add(i, new List<int>());

                    }
                }

            }
        }

        private PosicaoSolucao[] RetornarGrauVerticesPosicoes(List<int> solucao)
        {
            PosicaoSolucao[] posicoes = new PosicaoSolucao[solucao.Count];

            for (int i = 0; i < solucao.Count; i++)
            {
                posicoes[i] = new PosicaoSolucao();
                var ligacoesVertice = Grafo[solucao[i]];
                foreach (int verticeRelacionado in ligacoesVertice)
                {
                    int posicaoVerticeRelacionado = -1;
                    for (int j = 0; j < solucao.Count && posicaoVerticeRelacionado == -1; j++)
                    {
                        if (solucao[j] == verticeRelacionado)
                            posicaoVerticeRelacionado = j;
                    }

                    if (posicaoVerticeRelacionado < i)
                        posicoes[i].GrauVerticeEsquerda++; 
                    else
                        posicoes[i].GrauVerticeDireita++;

                    if (posicaoVerticeRelacionado < posicoes[i].MenorPosicaoVerticeRelacionado)
                        posicoes[i].MenorPosicaoVerticeRelacionado = posicaoVerticeRelacionado;

                    if (posicaoVerticeRelacionado > posicoes[i].MaiorPosicaoVerticeRelacionado)
                        posicoes[i].MaiorPosicaoVerticeRelacionado = posicaoVerticeRelacionado;
                }
            }

            return posicoes;
        }
    }
}

public class PosicaoSolucao
{
    public int GrauVerticeEsquerda { get; set; }
    public int GrauVerticeDireita { get; set; }
    public int MenorPosicaoVerticeRelacionado { get; set; }
    public int MaiorPosicaoVerticeRelacionado { get; set; }

    public PosicaoSolucao()
    {
        this.MenorPosicaoVerticeRelacionado = int.MaxValue;
        this.MaiorPosicaoVerticeRelacionado = int.MinValue;
    }

    public int ObterPosicaoMediana()
    {
        return this.MenorPosicaoVerticeRelacionado + this.MaiorPosicaoVerticeRelacionado / 2;
    }
}