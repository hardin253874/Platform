// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Simulates following a relationship, but simply joins back to the originating entity.
    /// This allows aggregates and where clauses to be applied to arbitraty nodes without worrying about
    /// potential impact on other expressions.
    /// Does not correspond to an entity model entry. Only used in supporting calculated expressions.
    /// </summary>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class JoinToSelfEntity : ResourceEntity
    {
    }
}
