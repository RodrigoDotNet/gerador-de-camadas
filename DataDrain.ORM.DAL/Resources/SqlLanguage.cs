using System;
using System.Linq.Expressions;
using System.Reflection;
using DataDrain.ORM.Data.Common.Language;

namespace DataDrain
{

    internal class SqlLanguage : QueryLanguage
    {
        //DbTypeSystem typeSystem = new DbTypeSystem();

        public override QueryTypeSystem TypeSystem
        {
            get { return null; }
        }

        public override bool AllowsMultipleCommands
        {
            get { return false; }
        }

        public override bool AllowDistinctInAggregates
        {
            get { return true; }
        }

        public override Expression GetGeneratedIdExpression(MemberInfo member)
        {
            return null;
        }

        public override string Quote(string name)
        {
            return name;
        }

        private static readonly char[] splitChars = new char[] { '.' };



        public static Type GetMemberType(MemberInfo mi)
        {
            FieldInfo fi = mi as FieldInfo;
            if (fi != null) return fi.FieldType;
            PropertyInfo pi = mi as PropertyInfo;
            if (pi != null) return pi.PropertyType;
            EventInfo ei = mi as EventInfo;
            if (ei != null) return ei.EventHandlerType;
            MethodInfo meth = mi as MethodInfo;  // property getters really
            if (meth != null) return meth.ReturnType;
            return null;
        }

        public static readonly QueryLanguage Default = new SqlLanguage();
    }
}