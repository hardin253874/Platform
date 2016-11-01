// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity2
{
	/// <summary>
	///     Class representing the JsonDeleteDetails type.
	/// </summary>
	[DataContract]
	public class JsonDeleteDetails
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="JsonDeleteDetails" /> class.
		/// </summary>
		public JsonDeleteDetails( )
		{
			Dependents = new List<JsonDeleteDependentDetail>( );
			Related = new List<JsonDeleteRelatedDetail>( );
		}

		/// <summary>
		///     Gets or sets the dependents.
		/// </summary>
		/// <value>
		///     The dependents.
		/// </value>
		[DataMember( Name = "dependents" )]
		public List<JsonDeleteDependentDetail> Dependents
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the related.
		/// </summary>
		/// <value>
		///     The related.
		/// </value>
		[DataMember( Name = "related" )]
		public List<JsonDeleteRelatedDetail> Related
		{
			get;
			set;
		}
	}
}