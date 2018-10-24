using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heuristicas.Metaheuristicas
{
    public abstract class MetaHeuristicaBase
    {
        private string CaminhoBaseInstancias = ConfigurationManager.AppSettings["CAMINHO_BASE_INSTANCIAS"];
        private string Instancia { get; set; }

        private bool LogAtivo { get; set; }
        private DateTime HorarioExcecucao { get; set; }
        private string NomeArquivoLogExecucao { get; set; }
        private string NomeArquivoLogGeral { get; set; }
        public string NomeHeuristica { get; set; }

        internal Dictionary<int, List<int>> Grafo { get; set; }
        internal int NumeroVertices { get { return Grafo.Count; } }

        public int[] MelhorSolucao { get; set; }
        public int FOMelhorSolucao { get; set; }
        public List<int> IteracoesMelhoraSolucaoGlobal { get; set; }

        public abstract void ExecutarMetaheuristica();

        public MetaHeuristicaBase(string instancia, string nomeHeuristica, bool logAtivo, string arquivoLogGeral = null)
        {
            this.Instancia = instancia;
            this.NomeHeuristica = nomeHeuristica;
            this.HorarioExcecucao = DateTime.Now;

            this.IteracoesMelhoraSolucaoGlobal = new List<int>();
            this.LogAtivo = logAtivo;
            this.NomeArquivoLogExecucao = string.Format(ConfigurationManager.AppSettings["CAMINHO_ARQUIVO_LOG_EXECUCAO"], NomeHeuristica, "111");// HorarioExcecucao.ToString("yyyy-MM-dd-HHmmss"));
            if (this.LogAtivo && File.Exists(this.NomeArquivoLogExecucao))
                File.Delete(this.NomeArquivoLogExecucao);
            this.NomeArquivoLogGeral = arquivoLogGeral;

            this.Grafo = new Dictionary<int, List<int>>();
            CarregarInformacoesInstancia();

            this.MelhorSolucao = new int[this.NumeroVertices];
        }

        /// <summary>
        /// Carrega os dados do grafo para a instâncias small
        /// </summary>
        protected void CarregarInformacoesInstancia()
        {
            string nomeArquivo = this.CaminhoBaseInstancias + Instancia;

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
            var contadorLigacoesPosicoes = new Dictionary<string, int>(); // inicialização do dicionário que representa cada "corte" do arranjo linear formado pela solução
            for (int i = 0; i < solucao.Length - 1; i++)
                contadorLigacoesPosicoes.Add(i + "_" + (i + 1), 0);

            for (int i = 0; i < solucao.Length - 1; i++)
            {
                var ligacoesVertice = Grafo[solucao[i]]; // recupera a lista de vértices relacionados ao vétice da posição corrente (i) da solução
                foreach (int verticeRelacionado in ligacoesVertice)
                {
                    // busca a posição do vértice relacionado
                    int posicaoVerticeRelacionado = -1;
                    for (int j = i + 1; j < solucao.Length && posicaoVerticeRelacionado == -1; j++)
                    {
                        if (solucao[j] == verticeRelacionado)
                            posicaoVerticeRelacionado = j;
                    }
                    // se o vértice relacionado está em alguma posição de índice maior que a posição atual da solução que está sendo avaliada...
                    if (posicaoVerticeRelacionado > i) 
                    { 
                        for (int j = i; j < posicaoVerticeRelacionado; j++) // incrementa o item do dicionário referente ao corte transpassado
                            contadorLigacoesPosicoes[j + "_" + (j + 1)] += 1;
                    }
                }
            }
            return contadorLigacoesPosicoes.Max(x => x.Value); // retorna o maior valor encontrado (cutwidth)
        }

        protected void GravarLogDuranteExecucao(string logString)
        {
            if (this.LogAtivo)
            {
                using (var escritorArquivo = new StreamWriter(this.NomeArquivoLogExecucao, true))
                    escritorArquivo.WriteLine(logString);
            }
        }

        public void GravarArquivoLogGeral()
        {
            if (this.LogAtivo && !string.IsNullOrEmpty(this.NomeArquivoLogGeral))
            {
                using (var escritorArquivo = new StreamWriter(this.NomeArquivoLogGeral, true))
                    escritorArquivo.WriteLine($"Instancia: { this.Instancia } - Cudtwidth: { FOMelhorSolucao } - Solução: { string.Join(" | ", MelhorSolucao.Select(x => x.ToString().PadLeft(2, '0'))) } ");
            }
        }
    }
}