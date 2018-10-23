using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heuristicas.Metaheuristicas
{
    public abstract class MetaHeuristicaBase
    {
        private string CaminhoBaseInstancias = "..\\..\\_instancias\\";
        private string Instancia { get; set; }

        internal Dictionary<int, List<int>> Grafo { get; set; }
        internal int NumeroVertices { get { return Grafo.Count; } }

        public int[] MelhorSolucao { get; set; }
        public int FOMelhorSolucao { get; set; }

        public abstract void ExecutarHeuristica();

        public MetaHeuristicaBase(string instancia)
        {
            this.Instancia = instancia;

            this.Grafo = new Dictionary<int, List<int>>();
            this.MelhorSolucao = new int[this.NumeroVertices];

            CarregarInformacoesInstanciaSmall();
        }

        /// <summary>
        /// Carrega os dados do grafo para a instâncias small
        /// </summary>
        protected void CarregarInformacoesInstanciaSmall()
        {
            string nomeArquivo = this.CaminhoBaseInstancias + "small\\" + Instancia;

            using (var leitorArquivo = new StreamReader(nomeArquivo))
            {
                string linha = leitorArquivo.ReadLine();
                string[] parametrosInstancia = leitorArquivo.ReadLine().Split(' ');
                int qtdVertices = int.Parse(parametrosInstancia[0]);

                for (int i = 1; i <= qtdVertices; i++)
                    Grafo.Add(i, new List<int>());

                linha = leitorArquivo.ReadLine();
                while (!string.IsNullOrEmpty(linha))
                {
                    int[] aresta = linha.Split(' ').Select(x => int.Parse(x)).ToArray();

                    Grafo[aresta[0]].Add(aresta[1]);
                    Grafo[aresta[1]].Add(aresta[0]);

                    linha = leitorArquivo.ReadLine();
                }
            }
        }

        /// <summary>
        /// Gera uma solução aleatória qualquer de acordo com o número de vértices do grafo.
        /// </summary>
        /// <returns> Retorna uma solução aleatória qualquer. </returns>
        protected int[] GerarSolucaoInicial()
        {
            var solucaoInicial = new int[NumeroVertices];

            var r = new Random();

            // Para cada vértice do grafo...
            for (int i = 1; i <= NumeroVertices; i++)
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

        /// <summary>
        /// Executa movimento vizinho aleatório
        /// </summary>
        /// <param name="solucaoAtual"> Solução que será modificada pelo movimento </param>
        /// <returns> Retorna um vetor contendo a nova solução após a execução do movimento </returns>
        protected int[] ExecutarMovimento(int[] solucaoAtual)
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

        /// <summary>
        /// Executa o cálculo da função objetivo retornando o cutwidth da solução
        /// </summary>
        /// <param name="solucao"> Organização linear dos vértices que compõem o grafo </param>
        /// <returns> Retorna o número de cutdwith (maior número de arestas que transpassam um único vértice) </returns>
        protected int ExecutarFuncaoAvaliacao(int[] solucao)
        {
            // Inicialização do dicionário que representa cada "corte" do arranjo linear formado pela solução (ex: [ 1_2, 2_3, 3_4, 4_5 ] para a solução de um grafo com 5 vértices)
            var contadorLigacoesPosicoes = new Dictionary<string, int>();
            for (int i = 0; i < solucao.Length - 1; i++)
                contadorLigacoesPosicoes.Add(i + "_" + (i + 1), 0);

            for (int i = 0; i < solucao.Length - 1; i++)
            {
                // Recupera a lista de vértices relacionados ao vétice da posição corrente (i) da solução
                var ligacoesVertice = Grafo[solucao[i]];
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
            return contadorLigacoesPosicoes.Max(x => x.Value);
        }
    }
}