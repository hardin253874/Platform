// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

using EDC.Core;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// An individual filter in a set of constraints.
    /// </summary>
    /// <remarks>
    /// Note: this class is distinct from Condition. This one is for conditions that are persisted with the query.
    /// </remarks>
    // type: reportCondition
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType("Condition", Namespace = Constants.StructuredQueryNamespace)]
    public class QueryCondition : IDeserializationCallback
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public QueryCondition()
        {
            // See also: OnDeserialization
            Arguments = new List<TypedValue>();
        }

        /// <summary>
        /// The programmatic name of the column to which this filter is applied.
        /// </summary>
        // rel: conditionExpression
        [DataMember(Order = 1)]
        public ScalarExpression Expression { get; set; }

        /// <summary>
        /// The type of comparison being applied.
        /// </summary>
        // rel: operator
        [DataMember(Order = 2)]
        public ConditionType Operator { get; set; }


        /// <summary>
        /// The parameters to the operation, excluding the column value itself.
        /// The argument is expressed in its native .Net type for the given DatabaseType. For example, a boxed System.DateTime for a DateTime column.
        /// There is typically only one argument, but operations such as 'between' may require more.
        /// </summary>
        // rel: conditionParameter  (lists of selected resources should be stored in a resourceListArgument)
        [DataMember(Order = 3)]
        public List<TypedValue> Arguments { get; set; }


        /// <summary>
        /// Convenient access to the first argument.
        /// </summary>
        // rel: conditionParameter
        [XmlIgnore]
        public TypedValue Argument
        {
            get
            {
                if (Arguments.Count == 0)
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

        [DataMember(Order = 4)]
        [XmlIgnore]
        public long EntityId { get; set; }

        /// <summary>
        /// If not null, a parameter name to use instead of arguments.
        /// The parameter name must be prefixed with @.
        /// </summary>
        [DataMember(Order = 5)]
        [XmlIgnore]
        public string Parameter { get; set; }

        /// <summary>
        /// Ensure lists are created after deserialization.
        /// </summary>
        /// <param name="sender"></param>
        void IDeserializationCallback.OnDeserialization( object sender )
        {
            if ( Arguments == null )
                Arguments = new List<TypedValue>( );
        }
    }
}
