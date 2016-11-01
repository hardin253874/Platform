// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity2
{
	/// <summary>
	///     Data for a single relationship instance.
	/// </summary>
	[DataContract]
	public class JsonRelationshipInstance
	{
		/// <summary>
		///     The entity being pointed to.
		/// </summary>
		/// <value>
		///     The entity identifier.
		/// </value>
		[DataMember( Name = "e" )]
		public long EntityId
		{
			get;
			set;
		}

		/// <summary>
		///     The instance ID for the relationship instance itself. May be null.
		/// </summary>
		/// <value>
		///     The relationship instance.
		/// </value>
		[DataMember( Name = "ri", EmitDefaultValue = false )]
		public long RelationshipInstance
		{
			get;
			set;
		}

		/// <summary>
		///		Should the relationship instance be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeRelationshipInstance( )
	    {
			return RelationshipInstance != 0;
	    }
	}
}