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

        public abstract void ExecutarMetaheuristica();

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

        // Scatter search for the Cutwidth Minimization Problem (C1)
        protected List<int> GerarSolucaoLiteraturaC1()
        {
            Random random = new Random();
            int primeiroVertice, menorGrau = -1, posicaoSelecionada = -1, minCutwidth = int.MaxValue, minSomaCutwidth = int.MaxValue, maxCutwidth = int.MinValue, maxSomaCutwidth = int.MinValue, cutwidthSolucao = 0, menorCutwidthSolucao = int.MaxValue;
            double limiarRLC = 0, cutwidthLimite = 0, cutwidthSomaLimite = 0;

            List<int> melhorSolucao = null;
            var solucao = new List<int>(NumeroVertices);

            var cutwidthListaVerticesCandidatos = new List<int>();
            var somaCutwidthListaVerticesCandidatos = new List<int>();
            var listaVerticesCandidatosRLC = new List<int>();
            var listaVerticesCandidatos = new List<int>();
            var listaVerticesNaoInseridos = new List<int>();

            int tentativas = 0;
            while (tentativas < 10)
            {
                solucao = new List<int>(NumeroVertices);
                for (int i = 1; i <= this.NumeroVertices; i++)
                    listaVerticesNaoInseridos.Add(i);

                PosicaoSolucao[] informacoesPosicoesOriginal = RetornarGrauVerticesPosicoes(listaVerticesNaoInseridos).OrderBy(x => x.GrauVertice).ToArray();
                PosicaoSolucao[] informacoesPosicoesAtualizado = null;

                // definição da primeira posição
                menorGrau = informacoesPosicoesOriginal.Min(x => x.GrauVertice);
                informacoesPosicoesAtualizado = informacoesPosicoesOriginal.Where(x => x.GrauVertice == menorGrau).ToArray();
                posicaoSelecionada = random.Next(0, informacoesPosicoesAtualizado.Length);
                primeiroVertice = informacoesPosicoesAtualizado[posicaoSelecionada].Vertice;
                solucao.Add(primeiroVertice);
                listaVerticesNaoInseridos.Remove(primeiroVertice);

                do
                {
                    // Reinicialização de variáveis
                    cutwidthListaVerticesCandidatos = new List<int>();
                    somaCutwidthListaVerticesCandidatos = new List<int>();
                    listaVerticesCandidatos = new List<int>();
                    listaVerticesCandidatosRLC = new List<int>();
                    minCutwidth = int.MaxValue;
                    maxCutwidth = int.MinValue;

                    for (int i = 0; i < solucao.Count; i++)
                    {
                        var verticesAdjacentes = Grafo[solucao[i]];
                        foreach (var vertice in verticesAdjacentes)
                        {
                            if (!listaVerticesCandidatos.Contains(vertice) && !solucao.Contains(vertice))
                                listaVerticesCandidatos.Add(vertice);
                        }
                    }

                    for (int i = 0; i < listaVerticesCandidatos.Count; i++)
                    {
                        int verticeCandidato = listaVerticesCandidatos[i];
                        solucao.Add(verticeCandidato);

                        ExecutarFuncaoAvaliacao(solucao);
                        cutwidthListaVerticesCandidatos.Add(CutwidthGrafo.Max(x => x.Value));
                        somaCutwidthListaVerticesCandidatos.Add(CutwidthGrafo.Sum(x => x.Value));

                        if (cutwidthListaVerticesCandidatos[i] < minCutwidth || (cutwidthListaVerticesCandidatos[i] == minCutwidth && somaCutwidthListaVerticesCandidatos[i] < minSomaCutwidth))
                        {
                            minCutwidth = cutwidthListaVerticesCandidatos[i];
                            minSomaCutwidth = somaCutwidthListaVerticesCandidatos[i];
                        }

                        if (cutwidthListaVerticesCandidatos[i] > maxCutwidth || (cutwidthListaVerticesCandidatos[i] == minCutwidth && somaCutwidthListaVerticesCandidatos[i] > minSomaCutwidth))
                        {
                            maxCutwidth = cutwidthListaVerticesCandidatos[i];
                            maxSomaCutwidth = somaCutwidthListaVerticesCandidatos[i];
                        }

                        solucao.Remove(verticeCandidato);
                    }

                    limiarRLC = random.NextDouble();
                    cutwidthLimite = minCutwidth + (limiarRLC * (maxCutwidth - minCutwidth));
                    cutwidthSomaLimite = minSomaCutwidth + (limiarRLC * (maxSomaCutwidth - minSomaCutwidth));
                    for (int i = 0; i < listaVerticesCandidatos.Count; i++)
                    {
                        if (cutwidthListaVerticesCandidatos[i] <= cutwidthLimite || somaCutwidthListaVerticesCandidatos[i] <= cutwidthSomaLimite)
                            listaVerticesCandidatosRLC.Add(listaVerticesCandidatos[i]);
                    }

                    int elementoEscolhido = listaVerticesCandidatosRLC[random.Next(0, listaVerticesCandidatosRLC.Count)];
                    solucao.Add(elementoEscolhido);
                    listaVerticesNaoInseridos.Remove(elementoEscolhido);

                } while (listaVerticesNaoInseridos.Any());

                ExecutarFuncaoAvaliacao(solucao);
                cutwidthSolucao = CutwidthGrafo.Max(x => x.Value);

                if (cutwidthSolucao < menorCutwidthSolucao)
                {
                    melhorSolucao = new List<int>(solucao);
                    menorCutwidthSolucao = cutwidthSolucao;
                }

                tentativas++;
            }

            ExecutarFuncaoAvaliacao(melhorSolucao);
            menorCutwidthSolucao = CutwidthGrafo.Max(x => x.Value);

            return solucao;
        }

        // Scatter search for the Cutwidth Minimization Problem (C1)
        protected List<int> GerarSolucaoLiteraturaC2()
        {
            Random random = new Random();
            int verticeSelecionado = 0, menorGrau = 0, posicaoSelecionada = -1, minCutwidth = int.MaxValue, cutwidthSolucao = 0, menorCutwidthSolucao = int.MaxValue;

            List<int> melhorSolucao = null, solucao = new List<int>(NumeroVertices), listaVerticesNaoInseridos = new List<int>(this.NumeroVertices), listaVerticesCandidatos = null, cutwidthListaVerticesCandidatos = null;

            int tentativas = 0;
            while (tentativas < 1000)
            {
                solucao = new List<int>(NumeroVertices);
                listaVerticesNaoInseridos = GerarSolucaoAleatoria();

                PosicaoSolucao[] informacoesPosicoesOriginal = RetornarGrauVerticesPosicoes(listaVerticesNaoInseridos).OrderBy(x => x.GrauVertice).ToArray();
                PosicaoSolucao[] informacoesPosicoesAtualizado = null;

                // definição da primeira posição
                menorGrau = informacoesPosicoesOriginal.Min(x => x.GrauVertice);
                informacoesPosicoesAtualizado = informacoesPosicoesOriginal.Where(x => x.GrauVertice == menorGrau).ToArray();
                posicaoSelecionada = random.Next(0, informacoesPosicoesAtualizado.Length);
                verticeSelecionado = informacoesPosicoesAtualizado[posicaoSelecionada].Vertice;
                solucao.Add(verticeSelecionado);
                listaVerticesNaoInseridos.Remove(verticeSelecionado);

                do
                {
                    if (listaVerticesNaoInseridos.Count == 1)
                        listaVerticesCandidatos = new List<int>() { listaVerticesNaoInseridos[0] };
                    else
                        listaVerticesCandidatos = new List<int>(listaVerticesNaoInseridos.GetRange(0, random.Next(1, listaVerticesNaoInseridos.Count)));

                    cutwidthListaVerticesCandidatos = new List<int>(listaVerticesCandidatos.Count);
                    minCutwidth = int.MaxValue;

                    for (int i = 0; i < listaVerticesCandidatos.Count; i++)
                    {
                        int verticeCandidato = listaVerticesCandidatos[i];
                        solucao.Add(verticeCandidato);

                        ExecutarFuncaoAvaliacao(solucao);
                        cutwidthListaVerticesCandidatos.Add(CutwidthGrafo.Max(x => x.Value));

                        if (cutwidthListaVerticesCandidatos[i] < minCutwidth)
                        {
                            verticeSelecionado = verticeCandidato;
                            minCutwidth = cutwidthListaVerticesCandidatos[i];
                        }

                        solucao.Remove(verticeCandidato);
                    }

                    solucao.Add(verticeSelecionado);
                    listaVerticesNaoInseridos.Remove(verticeSelecionado);

                } while (listaVerticesNaoInseridos.Any());

                ExecutarFuncaoAvaliacao(solucao);
                cutwidthSolucao = CutwidthGrafo.Max(x => x.Value);

                if (cutwidthSolucao < menorCutwidthSolucao)
                {
                    melhorSolucao = new List<int>(solucao);
                    menorCutwidthSolucao = cutwidthSolucao;
                }

                tentativas++;
            }

            ExecutarFuncaoAvaliacao(melhorSolucao);
            menorCutwidthSolucao = CutwidthGrafo.Max(x => x.Value);

            return melhorSolucao;
        }

        /// <summary>
        /// Executa o cálculo da função objetivo retornando o cutwidth da solução
        /// </summary>
        /// <param name="solucao"> Organização linear dos vértices que compõem o grafo </param>
        /// <returns> Retorna o número de cutdwith (maior número de arestas que transpassam um único vértice) </returns>
        protected void ExecutarFuncaoAvaliacao(List<int> solucao)
        {
            this.ContadorChamadasFuncaoObjetivo++;

            for (int i = 0; i < solucao.Count; i++)
                CutwidthGrafo[i + "_" + (i + 1)] = 0;

            for (int i = 0; i < solucao.Count; i++)
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

        protected Dictionary<string, int> ExecutarFuncaoAvaliacaoMovimentoTroca(List<int> solucao, int pos_i, int pos_j)
        {
            var cutwidthGrafoAposAtualizacao = new Dictionary<string, int>(CutwidthGrafo);

            var verticesAdjacentesPosI = new List<int>(Grafo[solucao[pos_i]]);
            var verticesAdjacentesPosJ = new List<int>(Grafo[solucao[pos_j]]);

            for (int i = 0; i < NumeroVertices; i++)
            {
                if (verticesAdjacentesPosI.Contains(solucao[i]))
                {
                    if (i < pos_i)
                    {
                        for (int j = pos_i; j < pos_j; j++)
                            cutwidthGrafoAposAtualizacao[$"{j}_{j + 1}"]++;
                    }

                    if (i > pos_j)
                    {
                        for (int j = pos_i; j < pos_j; j++)
                            cutwidthGrafoAposAtualizacao[$"{j}_{j + 1}"]--;
                    }

                    if (i > pos_i && i < pos_j)
                    {
                        for (int j = pos_i; j < i; j++)
                            cutwidthGrafoAposAtualizacao[$"{j}_{j + 1}"]--;

                        for (int j = i; j < pos_j; j++)
                            cutwidthGrafoAposAtualizacao[$"{j}_{j + 1}"]++;
                    }

                    verticesAdjacentesPosI.Remove(solucao[i]);
                }

                if (verticesAdjacentesPosJ.Contains(solucao[i]))
                {
                    if (i > pos_j)
                    {
                        for (int j = pos_i; j < pos_j; j++)
                            cutwidthGrafoAposAtualizacao[$"{j}_{j + 1}"]++;
                    }

                    if (i < pos_i)
                    {
                        for (int j = pos_i; j < pos_j; j++)
                            cutwidthGrafoAposAtualizacao[$"{j}_{j + 1}"]--;
                    }

                    if (i > pos_i && i < pos_j)
                    {
                        for (int j = pos_i; j < i; j++)
                            cutwidthGrafoAposAtualizacao[$"{j}_{j + 1}"]++;

                        for (int j = i; j < pos_j; j++)
                            cutwidthGrafoAposAtualizacao[$"{j}_{j + 1}"]--;
                    }

                    verticesAdjacentesPosJ.Remove(solucao[i]);
                }
            }

            return cutwidthGrafoAposAtualizacao;
        }


        // NÃO ESTÁ PRONTO
        protected Dictionary<string, int> ExecutarFuncaoAvaliacaoMovimentoInsercao(List<int> solucao, int posicaoAntiga, int posicaoNova)
        {
            var cutwidthGrafoAposAtualizacao = new Dictionary<string, int>(CutwidthGrafo);

            var verticesAdjacentesPosicaoAntiga = new List<int>(Grafo[solucao[posicaoAntiga]]);

            int indiceInicioAfetados = 0, indiceFimAfetados = 0;
            if (posicaoAntiga < posicaoNova)
            {
                indiceInicioAfetados = posicaoAntiga + 1;
                indiceFimAfetados = posicaoNova;
            }
            else
            {
                indiceInicioAfetados = posicaoNova;
                indiceFimAfetados = posicaoAntiga - 1;
            }

            for (int i = 0; i < NumeroVertices; i++)
            {
                if (i != posicaoAntiga)
                {
                    if (verticesAdjacentesPosicaoAntiga.Contains(solucao[i]))
                    {
                        // arestas que irão AUMENTAR o cutwidth para outros vértices
                        if (posicaoAntiga < posicaoNova)
                        {
                            if (i < posicaoAntiga)
                            {
                                for (int j = posicaoAntiga; j < posicaoNova; j++)
                                    cutwidthGrafoAposAtualizacao[$"{j}_{j + 1}"]--;
                            }

                            if (i > posicaoNova)
                            {
                                for (int j = posicaoAntiga; j < posicaoNova; j++)
                                    cutwidthGrafoAposAtualizacao[$"{j}_{j + 1}"]++;
                            }

                            if (i > posicaoAntiga && i < posicaoNova)
                            {
                                for (int j = posicaoAntiga; j < i; j++)
                                    cutwidthGrafoAposAtualizacao[$"{j}_{j + 1}"]--;

                                for (int j = i; j < posicaoNova; j++)
                                    cutwidthGrafoAposAtualizacao[$"{j}_{j + 1}"]++;
                            }
                            
                        }
                        else 
                        {
                            if (i < posicaoNova)
                            {
                                for (int j = posicaoNova; j < posicaoAntiga; j++)
                                    cutwidthGrafoAposAtualizacao[$"{j}_{j + 1}"]--;
                            }

                            if (i > posicaoAntiga)
                            {
                                for (int j = posicaoNova; j < posicaoAntiga; j++)
                                    cutwidthGrafoAposAtualizacao[$"{j}_{j + 1}"]++;
                            }

                            if (i > posicaoNova && i < posicaoAntiga)
                            {
                                for (int j = posicaoAntiga; j < i; j++)
                                    cutwidthGrafoAposAtualizacao[$"{j}_{j + 1}"]++;

                                for (int j = i; j < posicaoNova; j++)
                                    cutwidthGrafoAposAtualizacao[$"{j}_{j + 1}"]--;
                            }
                        }
                    }

                    // demais vértices relacionaods aos vértices afetados
                    if (i < indiceInicioAfetados || i > indiceFimAfetados)
                    {
                        for (int j = indiceInicioAfetados; j < indiceFimAfetados; j++)
                        {
                            if (Grafo[solucao[i]].Contains(solucao[j]))
                            {
                                if (posicaoAntiga < posicaoNova) // arastando para a esquerda
                                {
                                    if (i < j)
                                        cutwidthGrafoAposAtualizacao[$"{j - 2}_{j - 1}"]--;
                                    else
                                        cutwidthGrafoAposAtualizacao[$"{j - 2}_{j - 1}"]++;
                                }
                                else // arrastando para a direita
                                {
                                    if (i < j)
                                        cutwidthGrafoAposAtualizacao[$"{j - 1}_{j}"]++;
                                    else
                                        cutwidthGrafoAposAtualizacao[$"{j - 1}_{j}"]--;
                                }
                            }
                        }
                    }
                }
            }

            // vértices afetados com relacionamento entre si
            for (int i = indiceInicioAfetados; i < indiceFimAfetados - 1; i++)
            {
                var listaAdjacentesAfetados = Grafo[solucao[i]];

                for (int j = indiceInicioAfetados + 1; j < indiceFimAfetados; j++)
                {
                    if (i != j)
                    {
                        if (listaAdjacentesAfetados.Contains(solucao[j]))
                        {
                            if (posicaoAntiga < posicaoNova) // arrastando para a esquerda
                            {
                                cutwidthGrafoAposAtualizacao[$"{i - 1}_{i}"]++;
                                cutwidthGrafoAposAtualizacao[$"{j - 1}_{j}"]--;
                            }
                            else // arrastando para a direita
                            {
                                cutwidthGrafoAposAtualizacao[$"{i}_{i + 1}"]--;
                                cutwidthGrafoAposAtualizacao[$"{j}_{j + 1}"]++;
                            }
                        }
                    }
                }
            }

            return cutwidthGrafoAposAtualizacao;
        }

        protected PosicaoSolucao[] RetornarGrauVerticesPosicoes(List<int> solucao)
        {
            PosicaoSolucao[] posicoes = new PosicaoSolucao[solucao.Count];

            for (int i = 0; i < solucao.Count; i++)
            {
                posicoes[i] = new PosicaoSolucao() { Vertice = solucao[i] };
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