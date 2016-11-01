// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.Core;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    [DataContract( Namespace = Constants.DataContractNamespace )]
    public enum MutateType
    {
        /// <summary>
        /// Convert the expression to whatever result it would be if used in a top-level select.
        /// </summary>
        [EnumMember]
        DisplaySql,

        /// <summary>
        /// Convert the expression to whatever result it would be if used as a bool column.
        /// </summary>
        [EnumMember]
        BoolSql
    }

    // internal only; not in entity model
    // Causes an expression to have one of its purpose-specific callbacks invoked.
    [DataContract( Namespace = Constants.DataContractNamespace )]
    [XmlType( Namespace = Constants.StructuredQueryNamespace )]
    public class MutateExpression : ScalarExpression, ICompoundExpression
    {
        [DataMember(Order = 1)]
        public ScalarExpression Expression
        {
            get;
            set;
        }

        public MutateType MutateType
        {
            get;
            set;
        }

        public IEnumerable<ScalarExpression> Children
        {
            get
            {
                yield return Expression;
            }
        }

    }
}
