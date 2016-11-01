// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;
namespace EDC.ReadiNow.Metadata.Query.Structured
{

    /// <summary>
    /// This enum type is descript the Sql Logical operator. And|Or|Not 
    /// </summary>
    // internal only; not in entity model
     [Serializable]
     [DataContract(Namespace = Constants.DataContractNamespace)]
    public enum LogicalOperator
    {
          [EnumMember]
        And,

          [EnumMember]
        Or,

          [EnumMember]
        Not
    }

    /// <summary>
    /// 
    /// </summary>
    // internal only; not in entity model
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    [KnownType(typeof(IfElseExpression)), XmlInclude(typeof(IfElseExpression))]
    public class LogicalExpression : ScalarExpression, ICompoundExpression
    {
        public LogicalExpression()
        {
            Expressions = new List<ScalarExpression>();
        }

        [DataMember(Order = 1)]
        public LogicalOperator Operator { get; set; }

        [DataMember(Order = 2)]
        [XmlArray("Expressions")]
        [XmlArrayItem("Expression")]
        public List<ScalarExpression> Expressions { get; set; }

        public IEnumerable<ScalarExpression> Children
        {
            get { return Expressions; }
        }
    }


    // internal only; not in entity model
    [DataContract(Namespace = Constants.DataContractNamespace)] 
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class IfElseExpression : ScalarExpression, ICompoundExpression 
    {
 
        /// <summary>
        /// BooleanExpression
        /// </summary>
        [DataMember(Order = 1)]
        public ScalarExpression BooleanExpression
        {
            get;
            set;
        }

        [DataMember(Order = 2)]
        public ScalarExpression IfBlockExpression
        {
            get;
            set;
        }

        [DataMember(Order = 3)]
        public ScalarExpression ElseBlockExpression
        {
            get;
            set;
        }

        public IEnumerable<ScalarExpression> Children
        {
            get
            {
                yield return BooleanExpression;
                yield return IfBlockExpression;
                yield return ElseBlockExpression;
            }
        }
    }
}
