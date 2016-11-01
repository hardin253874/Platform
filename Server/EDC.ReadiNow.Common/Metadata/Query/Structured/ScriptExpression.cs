// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;
using EDC.Database;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Metadata.Query.Structured.Builder;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    // type: scriptExpression
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class ScriptExpression : EntityExpression, ICompoundExpression
    {
        // field: reportScipt
        [DataMember(Order = 1)]
        public string Script { get; set; }

        // rel: reportExpressionResultType
        [DataMember(Order = 2)]
        public DatabaseType ResultType { get; set; }

        // used internal to the query engine.
        [XmlIgnore]
        public ScalarExpression Calculation { get; set; }

        // used internal to the query engine.
        [XmlIgnore]
        public string StaticError { get; set; }

        // result is always static (note: this includes function that return current-date values, etc)
        [XmlIgnore]
        public bool Constant
        {
            get { return ExpressionTree == null || ExpressionTree.ResultType.Constant; }
        }

        /// <summary>
        /// Expression tree from the evaluation engine.
        /// </summary>
        [XmlIgnore]
        public IExpression ExpressionTree { get; set; }

        public IEnumerable<ScalarExpression> Children
        {
            get
            {
                if (Calculation != null)
                    yield return Calculation;                
            }
        }
    }
}
