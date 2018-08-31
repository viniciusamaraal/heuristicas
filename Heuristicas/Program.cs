using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heuristicas
{
    class Program
    {
        static int tamanhoMemoria = 100, numMaximoRejeicoesConsecutivas;
        static Dictionary<int, List<int>> grafo;
        static int[] solucao;
        
        static int[,] memoria;

        static void Main(string[] args)
        {
            LeituraArquivo();
            GerarSolucaoInicial();

            int controleEntradaSaidaMemoria = 0, numeroRejeicoes = 0;
            while (numeroRejeicoes < numMaximoRejeicoesConsecutivas)
            {
                int[] solucaoVizinha = ObterSolucaoVizinha();
            }
            

            Console.ReadKey();
        }

        static void LeituraArquivo()
        {
            string nomeArquivo = @"D:\Onedrive\Documents\_mine\CEFET\Disciplinas\Heurísticas Computacionais\Dados\Cutwidth\Instancias\small\p92_24_26";

            using (var leitor = new StreamReader(nomeArquivo))
            {
                string linha = leitor.ReadLine();
                string[] parametros = leitor.ReadLine().Split(' ');
                int qtdVertices = int.Parse(parametros[0]);

                solucao = new int[qtdVertices];
                grafo = new Dictionary<int, List<int>>(qtdVertices);
                for (int i = 1; i <= qtdVertices; i++)
                    grafo.Add(i, new List<int>());

                linha = leitor.ReadLine();
                while (!string.IsNullOrEmpty(linha))
                {
                    int[] aresta = linha.Split(' ').Select(x => int.Parse(x)).ToArray();

                    grafo[aresta[0]].Add(aresta[1]);
                    grafo[aresta[1]].Add(aresta[0]);

                    linha = leitor.ReadLine();
                }
            }
        }

        static void GerarSolucaoInicial()
        {
            // gera a solução inicial aleatoriamente
            Random r = new Random();
            for (int i = 1; i <= solucao.Length; i++)
            {
                int posicaoAleatoria = r.Next(0, solucao.Length);
                while (solucao[posicaoAleatoria] != 0)
                    posicaoAleatoria = r.Next(0, solucao.Length);

                solucao[posicaoAleatoria] = i;
            }

            // preenche a memória com a solução inicial
            memoria = new int[solucao.Length * tamanhoMemoria, solucao.Length];
            for (int i = 0; i < memoria.GetLength(0); i++)
            {
                for (int j = 0; j < memoria.GetLength(1); j++)
                    memoria[i, j] = solucao[j];
            }
        }

        static int[] ObterSolucaoVizinha()
        {
            Random r = new Random();
            int posicao1 = r.Next(0, grafo.Count - 1);
            int posicao2 = r.Next(0, grafo.Count - 1);
            while (posicao1 == posicao2)
                posicao2 = r.Next(0, grafo.Count - 1);

            int[] solucaoVizinha = solucao;
            int aux = solucao[posicao1];
            solucao[posicao1] = solucao[posicao2];
            solucao[posicao2] = aux;

            return solucaoVizinha;
        }

        static bool CalcularFuncaoObjetivo(int[] novaSolucao)
        {
            int numeroMaximoLigacoes = 0;
            for (int i = 0; i < novaSolucao.Length; i++)
            {

            }
        }
    }
}