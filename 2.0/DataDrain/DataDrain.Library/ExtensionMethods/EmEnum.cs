using System;
using System.Collections.Generic;
using System.Linq;

namespace DataDrain.Library.ExtensionMethods
{
    public static class EmEnum
    {
        public static List<KeyValuePair<int, string>> ToKeyPar(this Enum input)
        {
            var names = Enum.GetNames(input.GetType());

            return (from t in names let c = Convert.ChangeType(Enum.Parse(input.GetType(), t, true), input.GetType())
                    select new KeyValuePair<int, string>((int)c, t)).ToList();
        }
    }
}
