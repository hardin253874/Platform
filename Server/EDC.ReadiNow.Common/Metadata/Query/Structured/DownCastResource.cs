// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.Core;
using System.Xml.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Represents a query node for down casting a resource.
    /// </summary>
    /// <remarks>
    /// A relationship may point to resources of a particular type. (For example, a project has an Actor as a stakeholder).
    /// However, the user may wish to access field and relationships on a particular derived type, such as Person or Employee.
    /// </remarks>
    // type: derivedTypeReportNode
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class DownCastResource : ResourceEntity
    {
        /// <summary>
        /// If true, resource must be of the specified type for the row to be returned.
        /// If false, the row will still be returned, but any referenced fields of the downcast type will be null.
        /// </summary>
        // field: targetMustExist
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public bool MustExist { get; set; }

		/// <summary>
		///		Should the must exist value be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeMustExist( )
	    {
		    return MustExist;
	    }
    }
}
