// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    [DataContract( Namespace = Constants.DataContractNamespace )]
    [XmlType( Namespace = Constants.StructuredQueryNamespace )]
    public class SingleRowNode : ResourceEntity
    {
    }
}
