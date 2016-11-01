// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.Expressions.Tree
{
    /// <summary>
    /// Marks up nodes that can be directly translated to a CalculationExpression.
    /// </summary>
    public class QueryEngineAttribute : Attribute
    {
        /// <summary>
        /// Creates a new QueryEngineAttribute.
        /// </summary>
        /// <param name="calculationOperator">The type of operator that is represented.</param>
        public QueryEngineAttribute(CalculationOperator calculationOperator)
        {
            CalculationOperator = calculationOperator;
        }

        /// <summary>
        /// Creates a new QueryEngineAttribute.
        /// </summary>
        /// <param name="comparisonOperator">The type of comparison operator that is represented.</param>
        public QueryEngineAttribute(ComparisonOperator comparisonOperator)
        {
            ComparisonOperator = comparisonOperator;
        }

        /// <summary>
        /// Creates a new QueryEngineAttribute.
        /// </summary>
        /// <param name="logicalOperator">The type of operator that is represented.</param>
        public QueryEngineAttribute(LogicalOperator logicalOperator)
        {
            LogicalOperator = logicalOperator;
        }


        /// <summary>
        /// The type of operator that this node represents.
        /// </summary>
        public CalculationOperator? CalculationOperator { get; set; }

        /// <summary>
        /// The type of comparison operator that this node represents, if applicable.
        /// </summary>
        public ComparisonOperator? ComparisonOperator { get; set; }

        /// <summary>
        /// The type of comparison operator that this node represents, if applicable.
        /// </summary>
        public LogicalOperator? LogicalOperator { get; set; }
    }
}
