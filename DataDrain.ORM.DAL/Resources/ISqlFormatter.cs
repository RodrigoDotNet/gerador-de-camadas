using System.Collections.Generic;
using System.Data.Common;
using DataDrain.ORM.Data.Common.Language;
using System.Linq.Expressions;

namespace DataDrain.Factories
{
    internal interface ISqlFormatter
    {
        string Format(Expression expression, QueryLanguage language);

        List<DbParameter> Parametros { get; set; }
    }
}
