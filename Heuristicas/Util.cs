using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heuristicas
{
    public static class Util
    {
        public static void GerarDoisNumerosAleatoriosDiferentes(int menor, int maior, ref int valor1, ref int valor2)
        {
            var r = new Random();

            valor1 = valor2 = r.Next(menor, maior);
            do
            {
                valor2 = r.Next(menor, maior);
            } while (valor1 == valor2);
        }
    }
}
