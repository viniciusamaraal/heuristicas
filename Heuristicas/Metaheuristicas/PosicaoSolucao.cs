using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heuristicas.Metaheuristicas
{
    public class PosicaoSolucao
    {
        public int Vertice { get; set; }
        public int GrauVerticeEsquerda { get; set; }
        public int GrauVerticeDireita { get; set; }
        public int GrauVertice { get { return this.GrauVerticeEsquerda + this.GrauVerticeDireita; } }
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
}
