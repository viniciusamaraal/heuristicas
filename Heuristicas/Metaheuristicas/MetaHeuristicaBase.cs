using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heuristicas.Metaheuristicas
{
    public abstract class MetaHeuristicaBase
    {
        private string CaminhoBaseInstancias = ConfigurationManager.AppSettings["CAMINHO_BASE_INSTANCIAS"];
        public string Instancia { get; set; }

        private bool LogAtivo { get; set; }
        public Stopwatch Cronometro { get; set; }
        private string NomeArquivoLogExecucao { get; set; }
        public string NomeHeuristica { get; set; }

        internal Dictionary<int, List<int>> Grafo { get; set; }
        internal Dictionary<string, int> CutwidthGrafo { get; set; }
        internal int NumeroVertices { get { return Grafo.Count; } }

        public int MelhorIteracao { get; set; }
        public List<int> MelhorSolucao { get; set; }
        public int FOMenorCutwidthMelhorSolucao { get; set; }
        public int FOMenorSomaCutwidthMelhorSolucao { get; set; }
        public int FOMenorQuantidadeVerticesMaiorCutwidthMelhorSolucao { get; set; }
        public List<int> IteracoesMelhoraSolucaoGlobal { get; set; }
        public int ContadorChamadasFuncaoObjetivo { get; set; }

        public abstract Task ExecutarMetaheuristica();

        public MetaHeuristicaBase(string instancia, string nomeHeuristica, bool logAtivo)
        {
            this.Instancia = instancia;
            this.NomeHeuristica = nomeHeuristica;

            this.Cronometro = new Stopwatch();
            this.IteracoesMelhoraSolucaoGlobal = new List<int>();
            this.LogAtivo = logAtivo;
            this.NomeArquivoLogExecucao = string.Format(ConfigurationManager.AppSettings["CAMINHO_ARQUIVO_LOG_EXECUCAO"], NomeHeuristica, "111");// HorarioExcecucao.ToString("yyyy-MM-dd-HHmmss"));
            if (this.LogAtivo && File.Exists(this.NomeArquivoLogExecucao))
                File.Delete(this.NomeArquivoLogExecucao);

            this.Grafo = new Dictionary<int, List<int>>();
            CarregarInformacoesInstancia();

            this.MelhorSolucao = new List<int>(NumeroVertices);

            this.CutwidthGrafo = new Dictionary<string, int>(); // inicialização do dicionário que representa cada "corte" do arranjo linear formado pela solução
            for (int i = 0; i < NumeroVertices - 1; i++)
                CutwidthGrafo.Add(i + "_" + (i + 1), 0);

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
        protected List<int> GerarSolucaoAleatoria()
        {
            var solucao = new List<int>(NumeroVertices);
            for (int i = 0; i < NumeroVertices; i++)
                solucao.Add(0);

            var r = new Random();

            // Para cada vértice do grafo...
            for (int i = 1; i <= NumeroVertices; i++)
            {
                // Gera uma posição para o vértice da iteração corrente no vetor de solução inicial
                int posicaoAleatoria = r.Next(0, NumeroVertices);

                // Se a posição já possui algum elemento, gera um novo índice até que uma posição vazia seja encontrada
                while (solucao[posicaoAleatoria] != 0)
                    posicaoAleatoria = r.Next(0, NumeroVertices);

                solucao[posicaoAleatoria] = i;
            }

            return solucao;
        }

        protected List<int> GerarSolucaoSequencial()
        {
            var solucao = new List<int>(NumeroVertices);

            for (int i = 0; i < solucao.Count; i++)
                solucao[i] = i + 1;

            return solucao;
        }

        protected List<int> GerarSolucaoLiteratura()
        {
            // TODO: continuar de acordo com o artigo [38]
            var solucao = new List<int>(NumeroVertices);
            var listaVerticesCandidatos = new List<int>();
            for (int i = 1; i <= this.NumeroVertices; i++)
                listaVerticesCandidatos.Add(i);

            do
            {

            } while (listaVerticesCandidatos.Any());

            return solucao;
        }

        /// <summary>
        /// Executa o cálculo da função objetivo retornando o cutwidth da solução
        /// </summary>
        /// <param name="solucao"> Organização linear dos vértices que compõem o grafo </param>
        /// <returns> Retorna o número de cutdwith (maior número de arestas que transpassam um único vértice) </returns>
        protected void ExecutarFuncaoAvaliacao(List<int> solucao)
        {
            this.ContadorChamadasFuncaoObjetivo++;

            for (int i = 0; i < solucao.Count - 1; i++)
                CutwidthGrafo[i + "_" + (i + 1)] = 0;

            for (int i = 0; i < solucao.Count - 1; i++)
            {
                var ligacoesVertice = Grafo[solucao[i]]; // recupera a lista de vértices relacionados ao vétice da posição corrente (i) da solução
                foreach (int verticeRelacionado in ligacoesVertice)
                {
                    // busca a posição do vértice relacionado
                    int posicaoVerticeRelacionado = -1;
                    for (int j = i + 1; j < solucao.Count && posicaoVerticeRelacionado == -1; j++)
                    {
                        if (solucao[j] == verticeRelacionado)
                            posicaoVerticeRelacionado = j;
                    }

                    // se o vértice relacionado está em alguma posição de índice maior que a posição atual da solução que está sendo avaliada...
                    if (posicaoVerticeRelacionado > i) 
                    { 
                        for (int j = i; j < posicaoVerticeRelacionado; j++) // incrementa o item do dicionário referente ao corte transpassado
                            CutwidthGrafo[j + "_" + (j + 1)] += 1;
                    }
                }
            }
        }

        protected int ExecutarFuncaoAvaliacaoNova(List<int> solucao, int posicaoInicial, int? posicaoFinal = null)
        {
            posicaoFinal = posicaoFinal == null ? NumeroVertices - 1 : posicaoFinal;

            var cutWidthAuxiliar = new Dictionary<string, int>();
            for (int i = 0; i < NumeroVertices - 1; i++)
                cutWidthAuxiliar.Add(i + "_" + (i + 1), CutwidthGrafo[i + "_" + (i + 1)]);

            for (int i = posicaoInicial; i < posicaoFinal; i++)
            {
                var ligacoesVertice = Grafo[solucao[i]]; // recupera a lista de vértices relacionados ao vétice da posição corrente (i) da solução
                foreach (int verticeRelacionado in ligacoesVertice)
                {
                    // busca a posição do vértice relacionado
                    int posicaoVerticeRelacionado = -1;
                    for (int j = i + 1; j < solucao.Count && posicaoVerticeRelacionado == -1; j++)
                    {
                        if (solucao[j] == verticeRelacionado)
                            posicaoVerticeRelacionado = j;
                    }

                    // se o vértice relacionado está em alguma posição de índice maior que a posição atual da solução que está sendo avaliada...
                    if (posicaoVerticeRelacionado > i)
                    {
                        for (int j = i; j < posicaoVerticeRelacionado && j < posicaoFinal; j++) // incrementa o item do dicionário referente ao corte transpassado
                            cutWidthAuxiliar[j + "_" + (j + 1)] += 1;
                    }
                }
            }

            return cutWidthAuxiliar.Sum(x => x.Value);
        }

        protected void CalcularCutwidthAposMovimento(int[] solucao, Dictionary<string, int> cutwidthAtual, int posicao1, int posicao2)
        {

        }

        protected void GravarLogDuranteExecucao(string logString)
        {
            if (this.LogAtivo)
            {
                using (var escritorArquivo = new StreamWriter(this.NomeArquivoLogExecucao, true))
                    escritorArquivo.WriteLine(logString);
            }
        }
    }
}