// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity2
{
	/// <summary>
	///     Class representing the JsonDeleteDependentDetail type.
	/// </summary>
	[DataContract]
	public class JsonDeleteDependentDetail
	{
		/// <summary>
		///     Gets or sets the entity identifier.
		/// </summary>
		/// <value>
		///     The entity identifier.
		/// </value>
		[DataMember( Name = "id" )]
		public long EntityId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		[DataMember( Name = "name" )]
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		[DataMember( Name = "description" )]
		public string Description
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name of the type.
		/// </summary>
		/// <value>
		///     The name of the type.
		/// </value>
		[DataMember( Name = "typeName" )]
		public string TypeName
		{
			get;
			set;
		}
	}
}