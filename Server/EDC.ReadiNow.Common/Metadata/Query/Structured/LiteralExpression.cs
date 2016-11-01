// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.Core;
using System.Xml.Serialization;

namespace EDC.ReadiNow.Metadata.Query.Structured
{

    // internal only; not in entity model
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class LiteralExpression : ScalarExpression
    {
        [DataMember(Order = 1)] 
        public TypedValue Value
        {
            get;
            set;
        }
    }
}
