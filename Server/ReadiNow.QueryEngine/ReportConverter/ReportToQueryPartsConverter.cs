// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.Database;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using EDC.ReadiNow.Model;

namespace ReadiNow.QueryEngine.ReportConverter
{
    /// <summary>
    /// Allows us to uncouple a few systems that are hooking into query conversion externally.
    /// </summary>
    class ReportToQueryPartsConverter : IReportToQueryPartsConverter
    {
        /// <summary>
        /// Create a context object that can be passed to conversion methods.
        /// </summary>
        public IReportToQueryContext CreateContext( Report report )
        {
            var context = new FromEntityContext { Report = report };
            return context;
        }

        public bool IsValidExpression( ReportExpression expression )
        {
            return StructuredQueryEntityHelper.IsValidExpression( expression );
        }

        /// <summary>
        /// Convert a single condition.
        /// </summary>
        public Condition ConvertCondition( ReportCondition reportCondition, DatabaseType columnType, IReportToQueryContext converterContext )
        {
            FromEntityContext context = ( FromEntityContext ) converterContext;

            ConditionType conditionType = ConditionType.Unspecified;
            if ( reportCondition.Operator != null )
            {
                string [ ] conditionOperatorParts = reportCondition.Operator.Alias.Split( ':' );
                string conditionOperator = conditionOperatorParts.Length == 2 ? conditionOperatorParts [ 1 ].Substring( 4 ) : conditionOperatorParts [ 0 ].Substring( 4 );
                conditionType = ( ConditionType ) Enum.Parse( typeof( ConditionType ), conditionOperator, true );
            }

            QueryCondition queryCondition = new QueryCondition
            {
                EntityId = reportCondition.Id,
                Operator = conditionType,
                Expression = reportCondition.ConditionExpression != null ? StructuredQueryEntityHelper.BuildExpression( reportCondition.ConditionExpression, context ) : null
            };

            //for report column condtion format's else condition, without conditionParameter or condtionExpression
            //return empty condition
            if ( reportCondition.ConditionParameter == null && reportCondition.ConditionExpression == null )
            {
                return new Condition
                {
                    Arguments = null,
                    Operator = ConditionType.Unspecified,
                    ColumnType = columnType
                };
            }
            else
            {
                StructuredQueryEntityHelper.BuildConditionParameter( reportCondition, queryCondition, context );
                Condition condition = new Condition
                {
                    Arguments = queryCondition.Arguments,
                    Operator = queryCondition.Operator,
                    ColumnType = columnType
                };

                return condition;
            }
        }
    }
}
