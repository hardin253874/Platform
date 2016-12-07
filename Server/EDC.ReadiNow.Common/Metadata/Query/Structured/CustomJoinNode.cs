// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;
using EDC.ReadiNow.Expressions;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    // type: relationshipReportNode
    [DataContract( Namespace = Constants.DataContractNamespace )]
    [XmlType( Namespace = Constants.StructuredQueryNamespace )]
    public class CustomJoinNode : ResourceEntity
    {
        // field: joinPredicateCalculation
        [DataMember( Order = 1 )]
        public string JoinPredicateScript { get; set; }

        /// <summary>
        /// If true then a row will only be shown if the target exists (inner join).
        /// If false, then the row will be shown regardless (left join).
        /// </summary>
        // field: targetMustExist
        [DataMember( Order = 2 )]
        [DefaultValue( false )]
        public bool ResourceMustExist { get; set; }

        /// <summary>
        /// If true then this relationship will never constrain the parent node (forced left join),
        /// even if a child node or expression has requires.
        /// </summary>
        // field: targetMustExist
        [DataMember( Order = 3 )]
        [DefaultValue( false )]
        // field: resourceNeedNotExist
        public bool ResourceNeedNotExist { get; set; }

        /// <summary>
        /// If true then all related items are returned, not just those linked from the parent.
        /// </summary>
        // field: parentNeedNotExist
        [DataMember( Order = 4 )]
        [DefaultValue( false )]
        public bool ParentNeedNotExist { get; set; }

        // used internal to the query engine.
        [XmlIgnore]
        public ScalarExpression Calculation { get; set; }

        // used internal to the query engine.
        [XmlIgnore]
        public string StaticError { get; set; }

        /// <summary>
        /// Expression tree from the evaluation engine.
        /// </summary>
        [XmlIgnore]
        public IExpression ExpressionTree { get; set; }
    }
}
