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
        /// If true then all related items are returned, not just those linked from the parent.
        /// </summary>
        // field: parentNeedNotExist
        [DataMember( Order = 2 )]
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
