// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DataDrain.ORM.Data.Common.Expressions;

namespace DataDrain.ORM.Data.Common.Language
{
    /// <summary>
    /// Defines the language rules for the query provider
    /// </summary>
    public abstract class QueryLanguage
    {
        public abstract QueryTypeSystem TypeSystem { get; }
        public abstract Expression GetGeneratedIdExpression(MemberInfo member);

        public virtual string Quote(string name)
        {
            return name;
        }

        public virtual bool AllowsMultipleCommands
        {
            get { return false; }
        }

        public virtual bool AllowSubqueryInSelectWithoutFrom
        {
            get { return false; }
        }

        public virtual bool AllowDistinctInAggregates
        {
            get { return false; }
        }


        /// <summary>
        /// Determines whether the CLR type corresponds to a scalar data type in the query language
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>


        public virtual bool IsAggregate(MemberInfo member)
        {
            var method = member as MethodInfo;
            if (method != null)
            {
                if (method.DeclaringType == typeof(Queryable)
                    || method.DeclaringType == typeof(Enumerable))
                {
                    switch (method.Name)
                    {
                        case "Count":
                        case "LongCount":
                        case "Sum":
                        case "Min":
                        case "Max":
                        case "Average":
                            return true;
                    }
                }
            }
            var property = member as PropertyInfo;
            if (property != null
                && property.Name == "Count"
                && typeof(IEnumerable).IsAssignableFrom(property.DeclaringType))
            {
                return true;
            }
            return false;
        }

        public virtual bool AggregateArgumentIsPredicate(string aggregateName)
        {
            return aggregateName == "Count" || aggregateName == "LongCount";
        }

        /// <summary>
        /// Determines whether the given expression can be represented as a column in a select expressionss
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual bool CanBeColumn(Expression expression)
        {
            // by default, push all work in projection to client
            return this.MustBeColumn(expression);
        }

        public virtual bool MustBeColumn(Expression expression)
        {
            switch (expression.NodeType)
            {
                case (ExpressionType)DbExpressionType.Column:
                case (ExpressionType)DbExpressionType.Scalar:
                case (ExpressionType)DbExpressionType.Exists:
                case (ExpressionType)DbExpressionType.AggregateSubquery:
                case (ExpressionType)DbExpressionType.Aggregate:
                    return true;
                default:
                    return false;
            }
        }

    }

}