// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using DataDrain.ORM.Toolkit;
using ExpressionVisitor = DataDrain.ORM.Toolkit.ExpressionVisitor;

namespace DataDrain.ORM.Data.Common.Expressions
{
    /// <summary>
    /// An extended expression visitor including custom DbExpression nodes
    /// </summary>
    public abstract class DbExpressionVisitor : ExpressionVisitor
    {
        protected override Expression Visit(Expression exp)
        {
            if (exp == null)
            {
                return null;
            }
            switch ((DbExpressionType)exp.NodeType)
            {
                case DbExpressionType.Table:
                    return this.VisitTable((TableExpression)exp);
                case DbExpressionType.Column:
                    return this.VisitColumn((ColumnExpression)exp);
                case DbExpressionType.Join:
                    return this.VisitJoin((JoinExpression)exp);
                case DbExpressionType.OuterJoined:
                    return this.VisitOuterJoined((OuterJoinedExpression)exp);
                case DbExpressionType.Aggregate:
                    return this.VisitAggregate((AggregateExpression)exp);
                case DbExpressionType.Scalar:
                case DbExpressionType.Exists:
                case DbExpressionType.IsNull:
                    return this.VisitIsNull((IsNullExpression)exp);
                case DbExpressionType.Between:
                    return this.VisitBetween((BetweenExpression)exp);
                case DbExpressionType.NamedValue:
                    return this.VisitNamedValue((NamedValueExpression)exp);
                case DbExpressionType.Insert:
                case DbExpressionType.Update:
                case DbExpressionType.Delete:
                case DbExpressionType.If:
                case DbExpressionType.Block:
                case DbExpressionType.Batch:
                    return this.VisitBatch((BatchExpression)exp);
                case DbExpressionType.Variable:
                    return this.VisitVariable((VariableExpression)exp);
                case DbExpressionType.Entity:
                    return this.VisitEntity((EntityExpression)exp);
                default:
                    return base.Visit(exp);
            }
        }

        protected virtual Expression VisitEntity(EntityExpression entity)
        {
            var exp = this.Visit(entity.Expression);
            return this.UpdateEntity(entity, exp);
        }

        protected EntityExpression UpdateEntity(EntityExpression entity, Expression expression)
        {
            if (expression != entity.Expression)
            {
                return new EntityExpression(entity.Entity, expression);
            }
            return entity;
        }

        protected virtual Expression VisitTable(TableExpression table)
        {
            return table;
        }

        protected virtual Expression VisitColumn(ColumnExpression column)
        {
            return column;
        }

        protected virtual Expression VisitJoin(JoinExpression join)
        {
            var left = this.VisitSource(join.Left);
            var right = this.VisitSource(join.Right);
            var condition = this.Visit(join.Condition);
            return this.UpdateJoin(join, join.Join, left, right, condition);
        }

        protected JoinExpression UpdateJoin(JoinExpression join, JoinType joinType, Expression left, Expression right, Expression condition)
        {
            if (joinType != join.Join || left != join.Left || right != join.Right || condition != join.Condition)
            {
                return new JoinExpression(joinType, left, right, condition);
            }
            return join;
        }

        protected virtual Expression VisitOuterJoined(OuterJoinedExpression outer)
        {
            var test = this.Visit(outer.Test);
            var expression = this.Visit(outer.Expression);
            return this.UpdateOuterJoined(outer, test, expression);
        }

        protected OuterJoinedExpression UpdateOuterJoined(OuterJoinedExpression outer, Expression test, Expression expression)
        {
            if (test != outer.Test || expression != outer.Expression)
            {
                return new OuterJoinedExpression(test, expression);
            }
            return outer;
        }

        protected virtual Expression VisitAggregate(AggregateExpression aggregate)
        {
            var arg = this.Visit(aggregate.Argument);
            return this.UpdateAggregate(aggregate, aggregate.Type, aggregate.AggregateName, arg, aggregate.IsDistinct);
        }

        protected AggregateExpression UpdateAggregate(AggregateExpression aggregate, Type type, string aggType, Expression arg, bool isDistinct)
        {
            if (type != aggregate.Type || aggType != aggregate.AggregateName || arg != aggregate.Argument || isDistinct != aggregate.IsDistinct)
            {
                return new AggregateExpression(type, aggType, arg, isDistinct);
            }
            return aggregate;
        }

        protected virtual Expression VisitIsNull(IsNullExpression isnull)
        {
            var expr = this.Visit(isnull.Expression);
            return this.UpdateIsNull(isnull, expr);
        }

        protected IsNullExpression UpdateIsNull(IsNullExpression isnull, Expression expression)
        {
            if (expression != isnull.Expression)
            {
                return new IsNullExpression(expression);
            }
            return isnull;
        }

        protected virtual Expression VisitBetween(BetweenExpression between)
        {
            var expr = this.Visit(between.Expression);
            var lower = this.Visit(between.Lower);
            var upper = this.Visit(between.Upper);
            return this.UpdateBetween(between, expr, lower, upper);
        }

        protected BetweenExpression UpdateBetween(BetweenExpression between, Expression expression, Expression lower, Expression upper)
        {
            if (expression != between.Expression || lower != between.Lower || upper != between.Upper)
            {
                return new BetweenExpression(expression, lower, upper);
            }
            return between;
        }

        protected virtual Expression VisitNamedValue(NamedValueExpression value)
        {
            return value;
        }


        protected virtual Expression VisitSource(Expression source)
        {
            return this.Visit(source);
        }


        protected virtual Expression VisitBatch(BatchExpression batch)
        {
            var operation = (LambdaExpression)this.Visit(batch.Operation);
            var batchSize = this.Visit(batch.BatchSize);
            var stream = this.Visit(batch.Stream);
            return this.UpdateBatch(batch, batch.Input, operation, batchSize, stream);
        }

        protected BatchExpression UpdateBatch(BatchExpression batch, Expression input, LambdaExpression operation, Expression batchSize, Expression stream)
        {
            if (input != batch.Input || operation != batch.Operation || batchSize != batch.BatchSize || stream != batch.Stream)
            {
                return new BatchExpression(input, operation, batchSize, stream);
            }
            return batch;
        }

        protected virtual Expression VisitIf(IFCommand ifx)
        {
            var check = this.Visit(ifx.Check);
            var ifTrue = this.Visit(ifx.IfTrue);
            var ifFalse = this.Visit(ifx.IfFalse);
            return this.UpdateIf(ifx, check, ifTrue, ifFalse);
        }

        protected IFCommand UpdateIf(IFCommand ifx, Expression check, Expression ifTrue, Expression ifFalse)
        {
            if (check != ifx.Check || ifTrue != ifx.IfTrue || ifFalse != ifx.IfFalse)
            {
                return new IFCommand(check, ifTrue, ifFalse);
            }
            return ifx;
        }

        protected virtual Expression VisitVariable(VariableExpression vex)
        {
            return vex;
        }


        protected virtual ColumnAssignment VisitColumnAssignment(ColumnAssignment ca)
        {
            ColumnExpression c = (ColumnExpression)this.Visit(ca.Column);
            Expression e = this.Visit(ca.Expression);
            return this.UpdateColumnAssignment(ca, c, e);
        }

        protected ColumnAssignment UpdateColumnAssignment(ColumnAssignment ca, ColumnExpression c, Expression e)
        {
            if (c != ca.Column || e != ca.Expression)
            {
                return new ColumnAssignment(c, e);
            }
            return ca;
        }

        protected virtual ReadOnlyCollection<ColumnAssignment> VisitColumnAssignments(ReadOnlyCollection<ColumnAssignment> assignments)
        {
            List<ColumnAssignment> alternate = null;
            for (int i = 0, n = assignments.Count; i < n; i++)
            {
                ColumnAssignment assignment = this.VisitColumnAssignment(assignments[i]);
                if (alternate == null && assignment != assignments[i])
                {
                    alternate = assignments.Take(i).ToList();
                }
                if (alternate != null)
                {
                    alternate.Add(assignment);
                }
            }
            if (alternate != null)
            {
                return alternate.AsReadOnly();
            }
            return assignments;
        }

        protected virtual ReadOnlyCollection<ColumnDeclaration> VisitColumnDeclarations(ReadOnlyCollection<ColumnDeclaration> columns)
        {
            List<ColumnDeclaration> alternate = null;
            for (int i = 0, n = columns.Count; i < n; i++)
            {
                ColumnDeclaration column = columns[i];
                Expression e = this.Visit(column.Expression);
                if (alternate == null && e != column.Expression)
                {
                    alternate = columns.Take(i).ToList();
                }
                if (alternate != null)
                {
                    alternate.Add(new ColumnDeclaration(column.Name, e, column.QueryType));
                }
            }
            if (alternate != null)
            {
                return alternate.AsReadOnly();
            }
            return columns;
        }

        protected virtual ReadOnlyCollection<VariableDeclaration> VisitVariableDeclarations(ReadOnlyCollection<VariableDeclaration> decls)
        {
            List<VariableDeclaration> alternate = null;
            for (int i = 0, n = decls.Count; i < n; i++)
            {
                VariableDeclaration decl = decls[i];
                Expression e = this.Visit(decl.Expression);
                if (alternate == null && e != decl.Expression)
                {
                    alternate = decls.Take(i).ToList();
                }
                if (alternate != null)
                {
                    alternate.Add(new VariableDeclaration(decl.Name, decl.QueryType, e));
                }
            }
            if (alternate != null)
            {
                return alternate.AsReadOnly();
            }
            return decls;
        }

        protected virtual ReadOnlyCollection<OrderExpression> VisitOrderBy(ReadOnlyCollection<OrderExpression> expressions)
        {
            if (expressions != null)
            {
                List<OrderExpression> alternate = null;
                for (int i = 0, n = expressions.Count; i < n; i++)
                {
                    OrderExpression expr = expressions[i];
                    Expression e = this.Visit(expr.Expression);
                    if (alternate == null && e != expr.Expression)
                    {
                        alternate = expressions.Take(i).ToList();
                    }
                    if (alternate != null)
                    {
                        alternate.Add(new OrderExpression(expr.OrderType, e));
                    }
                }
                if (alternate != null)
                {
                    return alternate.AsReadOnly();
                }
            }
            return expressions;
        }
    }
}