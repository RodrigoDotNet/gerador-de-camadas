using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Reflection;

namespace TesteDAL.Apoio.Mapping
{
    internal class ObjectMap
    {
        public List<KeyValuePair<PropertyInfo, ColumnAttribute>> Propriedades { get; set; }

        public Object ObjetoAlvo { get; set; }

        public List<string> NomeCampos { get; set; }
    }
}
