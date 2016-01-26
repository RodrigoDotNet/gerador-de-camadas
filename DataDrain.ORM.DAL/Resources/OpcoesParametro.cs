using System.Data;
using System.Data.Linq.Mapping;
using System.Reflection;
using System.Text;
using TesteDAL.Apoio.Enumeradores;

namespace TesteDAL.Apoio.Mapping
{
    internal class OpcoesParametro
    {
        public PropertyInfo Prop { get; set; }

        public IDbCommand Cmd { get; set; }

        public ColumnAttribute DaoProperty { get; set; }

        public bool HasCondition { get; set; }

        public StringBuilder SbWhere { get; set; }

        public ETipoConsulta TipoConsulta { get; set; }
    }
}
