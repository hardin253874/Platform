// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity2
{
	/// <summary>
	///     Relationship metadata stored in the members section for a single direction.
	/// </summary>
	[DataContract]
	public class JsonRelationshipMember
	{
		/// <summary>
		///     Gets or sets the alias.
		/// </summary>
		/// <value>
		///     The alias.
		/// </value>
		[DataMember( Name = "alias", EmitDefaultValue = false )]
		public string Alias
		{
			get;
			set;
		}

		/// <summary>
		///		Should the alias be serialized.
		/// </summary>
		/// <returns></returns>
	    [UsedImplicitly]
		private bool ShouldSerializeAlias( )
	    {
			return Alias != null;
	    }

		/// <summary>
		///     Gets or sets a value indicating whether this instance is lookup.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is lookup; otherwise, <c>false</c>.
		/// </value>
		[DataMember( Name = "isLookup" )]
		public bool IsLookup
		{
			get;
			set;
		}
	}
}