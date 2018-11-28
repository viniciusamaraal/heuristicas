using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heuristicas.Metaheuristicas.BuscaTabu
{
    public class EstruturaTabu
    {
        private int IncrementoTamanhoListaTabu { get; set; }
        public int QuantidadeIteracoesProibicao { get; set; }
        private int QuantidadeIteracoesProibicaoOriginal { get; set; }
        private int QuantidadeMaximaIteracoesProibicao { get; set; }
        private int QuantidadeMaximaIteracoesProibicaoOriginal { get; set; }
        private RestricaoTabu[,] ListaRestricoes { get; set; }

        public EstruturaTabu(int dimensao, int qtdIteracoesProibicao, int qtdMaximaIteracoesProibicao, int incrementoTamanhoListaTabu)
        {
            this.IncrementoTamanhoListaTabu = incrementoTamanhoListaTabu;
            this.QuantidadeIteracoesProibicao = qtdIteracoesProibicao;
            this.QuantidadeIteracoesProibicaoOriginal = qtdIteracoesProibicao;
            this.QuantidadeMaximaIteracoesProibicao = qtdMaximaIteracoesProibicao;
            this.QuantidadeMaximaIteracoesProibicaoOriginal = qtdMaximaIteracoesProibicao;

            ListaRestricoes = new RestricaoTabu[dimensao, dimensao];

            for (int i = 0; i < ListaRestricoes.GetLength(0); i++)
            {
                for (int j = 0; j < ListaRestricoes.GetLength(1); j++)
                {
                    ListaRestricoes[i, j] = new RestricaoTabu();
                }
            }
        }

        public void IncrementarTamanhoLista()
        {
            if (this.QuantidadeIteracoesProibicao < this.QuantidadeMaximaIteracoesProibicao)
                this.QuantidadeIteracoesProibicao += this.IncrementoTamanhoListaTabu;
        }

        public void DecrementarTamanhoLista()
        {
            if (this.QuantidadeIteracoesProibicao > this.QuantidadeIteracoesProibicaoOriginal)
                this.QuantidadeIteracoesProibicao--;
        }

        public void ResetarTamanhoLista()
        {
            this.QuantidadeIteracoesProibicao = this.QuantidadeIteracoesProibicaoOriginal;
            this.QuantidadeMaximaIteracoesProibicao = this.QuantidadeMaximaIteracoesProibicaoOriginal;
        }

        public void DefinirTabu(int i, int j, int iteracaoProibicao)
        {
            ListaRestricoes[i, j].IteracaoProibicao = iteracaoProibicao + this.QuantidadeIteracoesProibicao;
            ListaRestricoes[i, j].QuantidadeIteracoesProibicao += this.QuantidadeIteracoesProibicao;
            ListaRestricoes[i, j].Visitas.Add(iteracaoProibicao);

            ListaRestricoes[j, i].IteracaoProibicao = iteracaoProibicao + this.QuantidadeIteracoesProibicao;
            ListaRestricoes[j, i].QuantidadeIteracoesProibicao += this.QuantidadeIteracoesProibicao;
            ListaRestricoes[j, i].Visitas.Add(iteracaoProibicao);
        }

        public bool ElementoProibido(int i, int j, int iteracaoAtual)
        {
            return ListaRestricoes[i, j].IteracaoProibicao >= iteracaoAtual || ListaRestricoes[j, i].IteracaoProibicao >= iteracaoAtual;
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
}
