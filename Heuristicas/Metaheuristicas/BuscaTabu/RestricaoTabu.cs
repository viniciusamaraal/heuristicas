using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heuristicas.Metaheuristicas.BuscaTabu
{
    public class RestricaoTabu
    {
        public int IteracaoProibicao { get; set; }
        public int QuantidadeIteracoesProibicao { get; set; }
        public List<int> Visitas { get; set; }

        public RestricaoTabu()
        {
            this.IteracaoProibicao = -1;
            this.Visitas = new List<int>();
        }
    }
}
