﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Heuristicas.Metaheuristicas.BuscaTabu
{
    public class ImplementacaoBuscaTabu : MetaHeuristicaBase
    {
        private int NumeroMaximoIteracoesSemMelhora { get; set; }
        private int NumeroIteracoesProibicaoLista { get; set; }
        private int NumeroMaximoIteracoesProibicaoLista { get; set; }
        private int ModuloIteracaoSemMelhoraIncrementoListaTabu { get; set; }
        private int IncrementoTamanhoListaTabu { get; set; }

        public ImplementacaoBuscaTabu(string instancia, bool logAtivo, double multiplicadorNumeroMaximoIteracoesSemMelhora, double multiplicadorNumeroIteracoesProibicaoLista, double multiplicadorTamanhoMaximoLista, int moduloIteracaoSemMelhoraIncrementoListaTabu)
            : base(instancia, Constantes.HeuristicasImplementadas.BuscaTabu, logAtivo)
        {
            this.NumeroMaximoIteracoesSemMelhora = (int)(base.NumeroVertices * multiplicadorNumeroMaximoIteracoesSemMelhora);
            this.NumeroIteracoesProibicaoLista = (int)(base.NumeroVertices * multiplicadorNumeroIteracoesProibicaoLista);
            this.NumeroMaximoIteracoesProibicaoLista = (int)(NumeroIteracoesProibicaoLista + (NumeroIteracoesProibicaoLista * multiplicadorTamanhoMaximoLista));
            this.IncrementoTamanhoListaTabu = 1;
            this.ModuloIteracaoSemMelhoraIncrementoListaTabu = moduloIteracaoSemMelhoraIncrementoListaTabu;
        }

        public override void ExecutarMetaheuristica()
        {
            //return Task.Factory.StartNew(() =>
            //{
            int iterAtual = 0, melhor_i = -1, melhor_j = -1, foMenorCutwidthSolucaoAtual = 0, foMenorSomaCutwidthSolucaoAtual = 0, foMenorQuantidadeVerticesMaiorCutwidthSolucaoAtual = 0;

            var estruturaTabu = new EstruturaTabu(base.NumeroVertices, this.NumeroIteracoesProibicaoLista, this.NumeroMaximoIteracoesProibicaoLista, this.IncrementoTamanhoListaTabu);

            var solucaoAtual = GerarSolucaoLiteraturaC1(); // GerarSolucaoAleatoria(); // GerarSolucaoInicial(); // GerarSolucaoLiteraturaC1() // new List<int> { 1, 15, 6, 2, 5, 9, 8, 13, 11, 3, 10, 7, 4, 12, 19, 17, 16, 14, 18 };
            MelhorSolucao = new List<int>(solucaoAtual);

            ExecutarFuncaoAvaliacao(solucaoAtual);
            foMenorCutwidthSolucaoAtual = FOMenorCutwidthMelhorSolucao = CutwidthGrafo.Max(x => x.Value);
            foMenorSomaCutwidthSolucaoAtual = FOMenorSomaCutwidthMelhorSolucao = CutwidthGrafo.Sum(x => x.Value);
            foMenorQuantidadeVerticesMaiorCutwidthSolucaoAtual = FOMenorQuantidadeVerticesMaiorCutwidthMelhorSolucao = CutwidthGrafo.Where(x => x.Value == foMenorCutwidthSolucaoAtual).Count();

            GravarLogDuranteExecucao($"{ melhor_i }; { melhor_j }; { foMenorCutwidthSolucaoAtual }; {  string.Join(" | ", solucaoAtual.Select(x => x.ToString().PadLeft(2, '0'))) }\n");

            Cronometro.Start();

            while (iterAtual - this.MelhorIteracao < this.NumeroMaximoIteracoesSemMelhora)
            {
                iterAtual++;

                melhor_i = melhor_j = -1;
                CalcularMelhorVizinhoBestImprovementTroca(solucaoAtual, iterAtual, estruturaTabu, ref melhor_i, ref melhor_j, ref foMenorCutwidthSolucaoAtual, ref foMenorSomaCutwidthSolucaoAtual);

                if (melhor_i >= 0 && melhor_j >= 0)
                {
                    CutwidthGrafo = ExecutarFuncaoAvaliacaoMovimentoTroca(solucaoAtual, melhor_i, melhor_j);

                    // Troca os elementos de acordo com a melhor vizinhança retornada
                    int aux = solucaoAtual[melhor_i];
                    solucaoAtual[melhor_i] = solucaoAtual[melhor_j];
                    solucaoAtual[melhor_j] = aux;

                    GravarLogDuranteExecucao($"{ melhor_i.ToString().PadLeft(2, '0') }; { melhor_j.ToString().PadLeft(2, '0') }; { foMenorCutwidthSolucaoAtual.ToString().PadLeft(2, '0') }; {  string.Join(" | ", solucaoAtual.Select(x => x.ToString().PadLeft(2, '0'))) }");

                    estruturaTabu.DefinirTabu(melhor_i, melhor_j, iterAtual);

                    if (foMenorCutwidthSolucaoAtual < FOMenorCutwidthMelhorSolucao ||
                        (foMenorCutwidthSolucaoAtual == FOMenorCutwidthMelhorSolucao && foMenorSomaCutwidthSolucaoAtual < FOMenorSomaCutwidthMelhorSolucao))
                    {
                        this.MelhorIteracao = iterAtual;
                        FOMenorCutwidthMelhorSolucao = foMenorCutwidthSolucaoAtual;
                        FOMenorSomaCutwidthMelhorSolucao = foMenorSomaCutwidthSolucaoAtual;

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
                else
                    estruturaTabu.DecrementarTamanhoLista();
            }

            Cronometro.Stop();

            ExecutarFuncaoAvaliacao(MelhorSolucao);

            GravarLogDuranteExecucao($"\n\nMelhorias solução global: {string.Join(" | ", base.IteracoesMelhoraSolucaoGlobal) }");
            GravarLogDuranteExecucao($"Cutdwidth: { base.FOMenorCutwidthMelhorSolucao }");
            GravarLogDuranteExecucao($"Solução Final: {  string.Join(" | ", MelhorSolucao.Select(x => x.ToString().PadLeft(2, '0'))) }");

            //Console.WriteLine(estruturaTabu.QuantidadeIteracoesProibicao);
            //estruturaTabu.ImprimirTrocasListaTabu(base.Instancia);
            //estruturaTabu.ImprimirQuantidadeIteracoesProibicaoListaTabu(base.Instancia);
            //});
        }

        private void CalcularMelhorVizinhoBestImprovementTroca(List<int> solucaoAtual, int iteracaoAtual, EstruturaTabu estruturaTabu, ref int melhor_i, ref int melhor_j, ref int foMenorCutwidthSolucaoAtual, ref int foMenorSomaCutwidthSolucaoAtual)
        {
            int difGrauI, difGrauJ;
            int foMenorCutwidthVizinho = 0, foMenorCutwidthSomaVizinho = 0;

            Dictionary<string, int> cutwidthAposTroca = null;
            var listaCandidatos = new List<Tuple<int, int>>();

            //ExecutarFuncaoAvaliacao(solucaoAtual);
            var informacoesPosicoesSolucaoAtual = RetornarGrauVerticesPosicoes(solucaoAtual);

            foMenorCutwidthSolucaoAtual = int.MaxValue;
            foMenorSomaCutwidthSolucaoAtual = int.MaxValue;

            for (int i = 0; i < solucaoAtual.Count - 1; i++)
            {
                for (int j = i + 1; j < solucaoAtual.Count; j++)
                {
                    // Se o grau do vértice i for maior à esquerda do que à direita, não se deve movê-lo para ainda mais a direita
                    difGrauI = informacoesPosicoesSolucaoAtual[i].GrauVerticeEsquerda - informacoesPosicoesSolucaoAtual[i].GrauVerticeDireita;
                    if (difGrauI > 0) continue;

                    // Se o grau do vértice j for maior à direita do que à esquerda, não se deve movê-lo para ainda mais a esquerda
                    difGrauJ = informacoesPosicoesSolucaoAtual[j].GrauVerticeDireita - informacoesPosicoesSolucaoAtual[j].GrauVerticeEsquerda;
                    if (difGrauJ > 0) continue;

                    cutwidthAposTroca = ExecutarFuncaoAvaliacaoMovimentoTroca(solucaoAtual, i, j);
                    foMenorCutwidthVizinho = cutwidthAposTroca.Max(x => x.Value);
                    foMenorCutwidthSomaVizinho = cutwidthAposTroca.Sum(x => x.Value);

                    // se a lista tabu não restringe o elemento ou, mesmo que haja restrição, o resultado da função objetivo encontrado no momento é melhor que a melhor solução (fo_star)
                    if (foMenorCutwidthVizinho < this.FOMenorCutwidthMelhorSolucao ||
                        (foMenorCutwidthVizinho == this.FOMenorCutwidthMelhorSolucao && foMenorCutwidthSomaVizinho < this.FOMenorSomaCutwidthMelhorSolucao) ||
                        (!estruturaTabu.ElementoProibido(i, j, iteracaoAtual)))
                    {
                        // Caso seja o melhor vizinho encontrado
                        if (foMenorCutwidthVizinho < foMenorCutwidthSolucaoAtual || (foMenorCutwidthVizinho == foMenorCutwidthSolucaoAtual && foMenorCutwidthSomaVizinho <= foMenorSomaCutwidthSolucaoAtual))
                        {
                            if (foMenorCutwidthVizinho < foMenorCutwidthSolucaoAtual || foMenorCutwidthSomaVizinho < foMenorSomaCutwidthSolucaoAtual) // cria uma nova lista se o cutwidth diminuiu
                                listaCandidatos = new List<Tuple<int, int>>();

                            listaCandidatos.Add(Tuple.Create<int, int>(i, j));
                            foMenorCutwidthSolucaoAtual = foMenorCutwidthVizinho;
                            foMenorSomaCutwidthSolucaoAtual = foMenorCutwidthSomaVizinho;

                            melhor_i = i;
                            melhor_j = j;
                        }
                        // Caso a nova solução melhore a solução atual ou seja igual à solução atual mas tenham sido feitas menos trocas
                        else if ((foMenorCutwidthVizinho == foMenorCutwidthSolucaoAtual && foMenorCutwidthSomaVizinho == foMenorSomaCutwidthSolucaoAtual && estruturaTabu.QuantidadeTrocas(i, j) < estruturaTabu.QuantidadeTrocas(melhor_i, melhor_j)))
                            listaCandidatos.Add(Tuple.Create<int, int>(i, j));
                    }
                }
            }

            if (listaCandidatos.Any())
            {
                // Dentre os melhores candidatos disponíveis, escolhe-se aleatoriamente algum deles
                int escolhaAleatoria = new Random().Next(0, listaCandidatos.Count);
                melhor_i = listaCandidatos[escolhaAleatoria].Item1;
                melhor_j = listaCandidatos[escolhaAleatoria].Item2;
            }
            else
            {
                int a = 0;
            }
        }


        // NÃO ESTÁ PRONTO
        private void CalcularMelhorVizinhoBestImprovementInsercao(List<int> solucaoAtual, int iteracaoAtual, EstruturaTabu estruturaTabu, ref int melhor_i, ref int melhor_j, ref int foMenorCutwidthSolucaoAtual, ref int foMenorSomaCutwidthSolucaoAtual)
        {
            int difGrauI, difGrauJ;
            int foMenorCutwidthVizinho = 0, foMenorCutwidthSomaVizinho = 0;

            Dictionary<string, int> cutwidthAposTroca = null;
            var listaCandidatos = new List<Tuple<int, int>>();

            //ExecutarFuncaoAvaliacao(solucaoAtual);
            var informacoesPosicoesSolucaoAtual = RetornarGrauVerticesPosicoes(solucaoAtual);

            foMenorCutwidthSolucaoAtual = int.MaxValue;
            foMenorSomaCutwidthSolucaoAtual = int.MaxValue;

            for (int i = 0; i < solucaoAtual.Count; i++)
            {
                for (int j = 0; j < solucaoAtual.Count; j++)
                {
                    if (i == j) continue;

                    // Se o grau do vértice i for maior à esquerda do que à direita, não se deve movê-lo para ainda mais a direita
                    difGrauI = informacoesPosicoesSolucaoAtual[i].GrauVerticeEsquerda - informacoesPosicoesSolucaoAtual[i].GrauVerticeDireita;
                    if (difGrauI > 0) continue;

                    // Se o grau do vértice j for maior à direita do que à esquerda, não se deve movê-lo para ainda mais a esquerda
                    difGrauJ = informacoesPosicoesSolucaoAtual[j].GrauVerticeDireita - informacoesPosicoesSolucaoAtual[j].GrauVerticeEsquerda;
                    if (difGrauJ > 0) continue;



                    ExecutarFuncaoAvaliacao(solucaoAtual);
                    int elementoInsercao = solucaoAtual[i];
                    solucaoAtual.Remove(elementoInsercao);
                    solucaoAtual.Insert(j, elementoInsercao);

                    ExecutarFuncaoAvaliacao(solucaoAtual);
                    int cutwidthFuncaoAntiga = CutwidthGrafo.Max(x => x.Value);
                    int somaCutwidthFuncaoAntiga = CutwidthGrafo.Sum(x => x.Value);

                    solucaoAtual.Remove(elementoInsercao);
                    solucaoAtual.Insert(i, elementoInsercao);
                    ExecutarFuncaoAvaliacao(solucaoAtual);

                    cutwidthAposTroca = ExecutarFuncaoAvaliacaoMovimentoInsercao(solucaoAtual, i, j);
                    foMenorCutwidthVizinho = cutwidthAposTroca.Max(x => x.Value);
                    foMenorCutwidthSomaVizinho = cutwidthAposTroca.Sum(x => x.Value);



                    if (cutwidthFuncaoAntiga != foMenorCutwidthVizinho || somaCutwidthFuncaoAntiga != foMenorCutwidthSomaVizinho)
                    {

                    }




                    // se a lista tabu não restringe o elemento ou, mesmo que haja restrição, o resultado da função objetivo encontrado no momento é melhor que a melhor solução (fo_star)
                    if (foMenorCutwidthVizinho < this.FOMenorCutwidthMelhorSolucao ||
                        (foMenorCutwidthVizinho == this.FOMenorCutwidthMelhorSolucao && foMenorCutwidthSomaVizinho < this.FOMenorSomaCutwidthMelhorSolucao) ||
                        (!estruturaTabu.ElementoProibido(i, j, iteracaoAtual)))
                    {
                        // Caso seja o melhor vizinho encontrado
                        if (foMenorCutwidthVizinho < foMenorCutwidthSolucaoAtual || (foMenorCutwidthVizinho == foMenorCutwidthSolucaoAtual && foMenorCutwidthSomaVizinho <= foMenorSomaCutwidthSolucaoAtual))
                        {
                            if (foMenorCutwidthVizinho < foMenorCutwidthSolucaoAtual || foMenorCutwidthSomaVizinho < foMenorSomaCutwidthSolucaoAtual) // cria uma nova lista se o cutwidth diminuiu
                                listaCandidatos = new List<Tuple<int, int>>();

                            listaCandidatos.Add(Tuple.Create<int, int>(i, j));
                            foMenorCutwidthSolucaoAtual = foMenorCutwidthVizinho;
                            foMenorSomaCutwidthSolucaoAtual = foMenorCutwidthSomaVizinho;

                            melhor_i = i;
                            melhor_j = j;
                        }
                        // Caso a nova solução melhore a solução atual ou seja igual à solução atual mas tenham sido feitas menos trocas
                        else if (foMenorCutwidthVizinho == foMenorCutwidthSolucaoAtual && foMenorCutwidthSomaVizinho == foMenorSomaCutwidthSolucaoAtual && estruturaTabu.QuantidadeTrocas(i, j) < estruturaTabu.QuantidadeTrocas(melhor_i, melhor_j))
                            listaCandidatos.Add(Tuple.Create<int, int>(i, j));
                    }
                }
            }

            // Dentre os melhores candidatos disponíveis, escolhe-se aleatoriamente algum deles
            int escolhaAleatoria = new Random().Next(0, listaCandidatos.Count);
            melhor_i = listaCandidatos[escolhaAleatoria].Item1;
            melhor_j = listaCandidatos[escolhaAleatoria].Item2;
        }
    }
}