using System;

namespace DataDrain.ORM.Data.Common
{
    public interface ICreateExecutor
    {
        QueryExecutor CreateExecutor();
    }

    public abstract class QueryExecutor
    {
        // called from compiled execution plan
        public abstract int RowsAffected { get; }
        public abstract object Convert(object value, Type type);

    }
}