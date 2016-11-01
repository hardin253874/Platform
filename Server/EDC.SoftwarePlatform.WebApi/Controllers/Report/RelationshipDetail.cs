// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Report
{
	/// <summary>
	///     Class RelationshipDetail.
	/// </summary>
	[DataContract]
	public class RelationshipDetail
	{
		/// <summary>
		///     Gets or sets the included entity identifiers.
		/// </summary>
		/// <value>The included entity identifiers.</value>
		[DataMember( Name = "inclids", EmitDefaultValue = false, IsRequired = false )]
		public List<long> IncludedEntityIdentifiers
		{
			get;
			set;
		}

		/// <summary>
		///		Should the included entity identifiers be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeIncludedEntityIdentifiers( )
	    {
			return IncludedEntityIdentifiers != null;
	    }

		/// <summary>
		///     Gets or sets the excluded entity identifiers.
		/// </summary>
		/// <value>The excluded entity identifiers.</value>
		[DataMember( Name = "exclids", EmitDefaultValue = false, IsRequired = false )]
		public List<long> ExcludedEntityIdentifiers
		{
			get;
			set;
		}

		/// <summary>
		///		Should the excluded entity identifiers be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeExcludedEntityIdentifiers( )
	    {
			return ExcludedEntityIdentifiers != null;
	    }
	}
}