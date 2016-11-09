using System;

namespace DataDrain.Library.ExtensionMethods
{
    public static class EmObject
    {
        public static object ConverteParaTipo(this object valor)
        {
            if (valor == null || valor.ToString() == "null")
            {
                return null;
            }

            DateTime data;
            int numerico;
            long numericoGrande;
            ulong numericoMuitoGrande;
            double numericoDecimalGrande;
            TimeSpan hora;

            if (DateTime.TryParse(valor.ToString(), out data)) { return data; }

            if (int.TryParse(valor.ToString(), out numerico)) { return numerico; }

            if (long.TryParse(valor.ToString(), out numericoGrande)) { return numericoGrande; }

            if (ulong.TryParse(valor.ToString(), out numericoMuitoGrande)) { return numericoMuitoGrande; }

            if (double.TryParse(valor.ToString(), out numericoDecimalGrande)) { return numericoDecimalGrande; }

            if (TimeSpan.TryParse(valor.ToString(), out hora)) { return hora; }

            return valor;
        }
    }
}
