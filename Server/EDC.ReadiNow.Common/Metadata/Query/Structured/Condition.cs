// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;

using EDC.Database;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// An individual filter in a set of constraints.
    /// </summary>
    /// <remarks>
    /// Note: this class is distinct from QueryCondition. This one is for ad-hoc conditions as it is used by the analyzer
    /// and the conditional formatting.
    /// </remarks>
    [DataContract(Namespace = Constants.DataContractNamespace)]    
    public class Condition
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Condition()
        {
            Arguments = new List<TypedValue>();
        }

        /// <summary>
        /// The programatic name of the column to which this filter is applied.
        /// </summary>
        // field: name
        [DataMember(Order = 1)]
        public string ColumnName { get; set; }
        // Note: this is to be obsoleted (remember to remove from client contract as well... needs to be replaced with ColumnId)

        /// <summary>
        /// The expected type of the column to which it is being applied.
        /// </summary>
        [DataMember(Order = 2)]
        public DatabaseType ColumnType { get; set; }
        // Note: this is to be obsoleted (remember to remove from client contract as well)
        // In entity model, access via: condition.conditionExpression.reportExpressionResultType


        /// <summary>
        /// The type of comparison being applied.
        /// </summary>
        // rel: operator
        [DataMember(Order = 3)]
        public ConditionType Operator { get; set; }


        /// <summary>
        /// The parameters to the operation, excluding the column value itself.
        /// The argument is expressed in its native .Net type for the given DatabaseType. For example, a boxed System.DateTime for a DateTime column.
        /// There is typically only one argument, but operations such as 'between' may require more.
        /// </summary>
        // rel: conditionParameter
        [DataMember(Order = 4)]
        public IList<TypedValue> Arguments { get; set; }

        /// <summary>
        /// Convenient accessor to the first argument.
        /// </summary>
        public TypedValue Argument
        {
            get
            {
                if ( Arguments == null || Arguments.Count == 0 )
                    return null;
                
                return Arguments[0];
            }
            set
            {
                if (Arguments.Count == 0)
                    Arguments.Add(value);
                else
                    Arguments[0] = value;
            }
        }
    }
}
