// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Database;
using EDC.ReadiNow.Expressions;
using ReadiNow.Expressions.Compiler;
using ReadiNow.Expressions.Tree.Nodes;

namespace ReadiNow.Expressions.Tree
{
    /// <summary>
    /// Represents the data type returned by an expression, or sub expression.
    /// </summary>
    class ExprTypeCastHelper
    {
        /// <summary>
        /// Determines if an expression of the source type can be assigned to a parameter or other input of the target type.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="allowedCasts">Indicates what type of cast may be permitted.</param>
        /// <returns></returns>
        internal static Assignable IsAssignable(ExprType sourceType, ExprType targetType, CastType allowedCasts)
        {
            int priority;
            return IsAssignable(sourceType, targetType, allowedCasts, out priority);
        }


        /// <summary>
        /// Determines if an expression of the source type can be assigned to a parameter or other input of the target type.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="allowedCasts">Indicates what type of cast may be permitted.</param>
        /// <param name="priority">The priority.</param>
        /// <returns></returns>
        internal static Assignable IsAssignable(ExprType sourceType, ExprType targetType, CastType allowedCasts, out int priority)
        {
            priority = 0;

            // Target requires a literal
            if (targetType.Constant && !sourceType.Constant)
                return Assignable.No;

            Assignable result = Assignable.Yes;

            // IsList mismatch
            if (sourceType.IsList && targetType.DisallowList)
                return Assignable.No;

            // Entity type 
            if (sourceType.EntityTypeId != targetType.EntityTypeId)
                result = Assignable.RequiresCast;

            // Exact match of type
            if (sourceType.Type == targetType.Type)
                return result;

            if (sourceType.Type == DataType.None)
                return Assignable.RequiresCast;

            // Check cast
            result = Assignable.No;
            priority = int.MaxValue;
            foreach (var cast in LanguageManager.Instance.Casts)
            {
                if (cast.FromType != sourceType.Type || cast.ToType != targetType.Type)
                    continue;

                // Check if we can do this cast
                // (Implicit casts can implicitly be used as explicit casts)
                if (cast.CastType == CastType.Implicit && allowedCasts == CastType.Implicit || allowedCasts == CastType.Explicit)
                {
                    priority = Math.Min(priority, cast.Priority);
                    result = Assignable.RequiresCast;
                }
            }
            return result;
        }


        /// <summary>
        /// Produces an expression that will perform a type cast.
        /// </summary>
        /// <param name="source">The source expression to be cast.</param>
        /// <param name="targetType">The target type.</param>
        /// <param name="context"></param>
        /// <returns>An expression that represents the source casted to the new type.</returns>
        internal static ExpressionNode Cast(ExpressionNode source, ExprType targetType, CompileContext context)
        {
            ExprType sourceType = source.ResultType;
            ExpressionNode result = null;

            // Check cast
            if (sourceType.Type == targetType.Type)
            {
                if (sourceType.Type == DataType.Entity && sourceType.EntityTypeId != targetType.EntityTypeId)
                {
                    result = new EntityTypeCast { Argument = source };
                    result.ResultType = sourceType.Clone();
                    result.ResultType.EntityType = targetType.EntityType ?? sourceType.EntityType;
                    ((EntityNode)result).ChildContainer.RegisterChild((EntityNode)source);
                    //StaticBuilder.DecorateExpression(result);
                }
                else
                {
                    // No cast necessary
                    result = source;
                }
            }
            else
            {
                // Find matching cast
                foreach (var cast in LanguageManager.Instance.Casts)
                {
                    if (cast.FromType != sourceType.Type || cast.ToType != targetType.Type)
                        continue;

                    var castNode = (UnaryOperatorNode)Activator.CreateInstance(cast.Type);
                    castNode.Argument = source;
                    castNode.ResultType = targetType.Clone();
                    castNode.ResultType.IsList = sourceType.IsList;
                    result = castNode;
                    //StaticBuilder.DecorateExpression(result);
                    break;
                }
            }

            if (result == null)
                throw new InvalidOperationException("This cast could not be performed. Call IsAssignable.");

            // Copy number of decimal places
            if (DataTypeHelper.IsDecimal(sourceType.Type))
            {
                if (DataTypeHelper.IsDecimal(targetType.Type))
                {
                    // Copy decimal places into result type
                    result.ResultType.DecimalPlaces = source.ResultType.DecimalPlaces;
                }
                else if (targetType.Type == DataType.String)
                {
                    // Force determine decimal places, or fall back to defaults, on the input expression, so it's available for string formatting.
                    source.ResultType.DecimalPlaces = context.GetDecimalPlaces(source.ResultType);
                }
            }

            return result;

        }


    }


    /// <summary>
    /// Indicates assignability.
    /// </summary>
    enum Assignable
    {
        Yes,
        No,
        RequiresCast
    }

}
