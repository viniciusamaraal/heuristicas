using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heuristicas
{
    class Program
    {
        static Dictionary<int, List<int>> BuscarInformacoesInstancia(string instancia)
        {
            var grafo = new Dictionary<int, List<int>>();

            string nomeArquivo = ConfigurationManager.AppSettings["SMALL_CAMINHO_BASE"] + instancia;
            using (var leitorArquivo = new StreamReader(nomeArquivo))
            {
                string linha = leitorArquivo.ReadLine();
                string[] parametrosInstancia = leitorArquivo.ReadLine().Split(' ');
                int qtdVertices = int.Parse(parametrosInstancia[0]);
                
                for (int i = 1; i <= qtdVertices; i++)
                    grafo.Add(i, new List<int>());

                linha = leitorArquivo.ReadLine();
                while (!string.IsNullOrEmpty(linha))
                {
                    int[] aresta = linha.Split(' ').Select(x => int.Parse(x)).ToArray();

                    grafo[aresta[0]].Add(aresta[1]);
                    grafo[aresta[1]].Add(aresta[0]);

                    linha = leitorArquivo.ReadLine();
                }
            }

            return grafo;
        }

        static int[] GerarSolucaoInicial(int numeroVertices)
        {
            var solucaoInicial = new int[numeroVertices];
            
            var r = new Random();
            // Para cada vértice do grafo...
            for (int i = 1; i <= numeroVertices; i++)
            {
                // Gera uma posição para o vértice da iteração corrente no vetor de solução inicial
                int posicaoAleatoria = r.Next(0, solucaoInicial.Length);

                // Se a posição já possui algum elemento, gera um novo índice até que uma posição vazia seja encontrada
                while (solucaoInicial[posicaoAleatoria] != 0)
                    posicaoAleatoria = r.Next(0, solucaoInicial.Length);

                solucaoInicial[posicaoAleatoria] = i;
            }

            return solucaoInicial;
        }

        static int[,] PreencherMemoriaSolucaoInicial(int[] solucaoInicial, int tamanhoMemoria)
        {
            var memoria = new int[solucaoInicial.Length * tamanhoMemoria, solucaoInicial.Length];

            for (int i = 0; i < memoria.GetLength(0); i++)
            {
                for (int j = 0; j < memoria.GetLength(1); j++)
                    memoria[i, j] = solucaoInicial[j];
            }

            return memoria;
        }

        static int[] ExecutarMovimento(int[] solucaoAtual)
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

        static int ExecutarFuncaoAvaliacao(Dictionary<int, List<int>> grafo, int[] solucao)
        {
            // Inicialização do dicionário que representa cada "corte" do arranjo linear formado pela solução (ex: [ 0_1, 1_2, 2_3, 3_4 ] para a solução de um grafo com 5 vértices)
            var contadorLigacoesPosicoes = new Dictionary<string, int>();
            for (int i = 0; i < solucao.Length - 1; i++)
                contadorLigacoesPosicoes.Add(i + "_" + (i + 1), 0);
            
            for (int i = 0; i < solucao.Length - 1; i++)
            {
                // Recupera a lista de vértices relacionados ao vétice da posição corrente (i) da solução
                var ligacoesVertice = grafo[solucao[i]];
                foreach (int verticeRelacionado in ligacoesVertice)
                {
                    // Busca a posição do vértice relacionado
                    int posicaoVerticeRelacionado = -1;
                    for (int j = i + 1; j < solucao.Length && posicaoVerticeRelacionado == -1; j++)
                    {
                        if (solucao[j] == verticeRelacionado)
                            posicaoVerticeRelacionado = j;
                    }

                    // Se o vértice relacionado está em alguma posição de índice maior que a posição atual da solução que está sendo avaliada...
                    if (posicaoVerticeRelacionado > i)
                    {
                        // Incrementa o dicionário que representa cada corte passado, do vértice da posição atual (i) até o vértice relacionado (foreach)
                        for (int j = i; j < posicaoVerticeRelacionado; j++)
                            contadorLigacoesPosicoes[j + "_" + (j + 1)] += 1;
                    }
                }
            }

            // Retorna o maior valor encontrado (cutwidth)
            return contadorLigacoesPosicoes.Max(x=> x.Value);
        }

        static void LAHillClimbing(int multiplicadorTamanhoMemoria, int numeroMaximoRejeicoesConsecutivas)
        {
            string instancia = "teste.txt";

            var grafo = BuscarInformacoesInstancia(instancia);
            var melhorSolucao = new int[] { 3, 1, 4, 5, 2, 6 }; // GerarSolucaoInicial(grafo.Count);

            int qtdVertices = grafo.Count, tamanhoMaximoMemoria = qtdVertices * multiplicadorTamanhoMemoria;

            //var memoria = PreencherMemoriaSolucaoInicial(solucaoInicial, tamanhoMaximoMemoria);

            int valorMelhorSolucao = ExecutarFuncaoAvaliacao(grafo, melhorSolucao);

            int numeroRejeicoes = 0;
            while (numeroRejeicoes < numeroMaximoRejeicoesConsecutivas)
            {
                var solucaoVizinha = ExecutarMovimento(melhorSolucao);
                int valorSolucaoVizinha = ExecutarFuncaoAvaliacao(grafo, solucaoVizinha);

                if (valorSolucaoVizinha < valorMelhorSolucao)
                {
                    valorMelhorSolucao = valorSolucaoVizinha;
                    melhorSolucao = (int[])solucaoVizinha.Clone();
                    numeroRejeicoes = 0;
                }
                else
                    numeroRejeicoes++;
            }

            Console.WriteLine($"O valor encontrado para a melhor solução foi { valorMelhorSolucao }");
            Console.WriteLine($"Organização dos componentes: [ | { string.Join(" | ", melhorSolucao ) } | ]");
        }

        static void Main(string[] args)
        {
            LAHillClimbing(100, 10000);

            Console.ReadKey();
        }
    }
}