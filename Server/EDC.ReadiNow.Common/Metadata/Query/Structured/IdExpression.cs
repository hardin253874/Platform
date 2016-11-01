// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Identifies a column of resource data.
    /// Not necessarily for select.
    /// </summary>
    // not implemented in entity model (open to discussion)
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class IdExpression : EntityExpression
    {
        [DataMember (Order = 1)]
        public int Number { get; set; }
    }
}
