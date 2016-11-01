// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    // internal only; not in entity model
    [Serializable]
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public enum ComparisonOperator
    {
        [EnumMember]
        LessThan,

        [EnumMember]
        GreaterThan,

        [EnumMember]
        LessThanEqual,

        [EnumMember]
        GreaterThanEqual,

        [EnumMember]
        Equal,

        [EnumMember]
        NotEqual,

        [EnumMember]
        IsNull,

        [EnumMember]
        IsNotNull,

        [EnumMember]
        Like,

        [EnumMember]
        NotLike
    }


    // internal only; not in entity model
    [DataContract(Namespace = Constants.DataContractNamespace)]   
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class ComparisonExpression : ScalarExpression, ICompoundExpression
    {
        public ComparisonExpression()
        {
            Expressions = new List<ScalarExpression>();
        }


        [DataMember(Order = 1)]
        public ComparisonOperator Operator
        {
            get;
            set;
        }

        [DataMember(Order = 2)]
        [XmlArray("Expressions")]
        [XmlArrayItem("Expression")]
        public List<ScalarExpression> Expressions
        {
            get;
            set;
        }

        public IEnumerable<ScalarExpression> Children
        {
            get { return Expressions; }
        }
    }
}
