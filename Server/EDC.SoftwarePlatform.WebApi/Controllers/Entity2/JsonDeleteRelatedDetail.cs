// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity2
{
	/// <summary>
	///     Class representing the JsonDeleteRelatedDetail type.
	/// </summary>
	[DataContract]
	public class JsonDeleteRelatedDetail
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

		/// <summary>
		///     Gets or sets the depth.
		/// </summary>
		/// <value>
		///     The depth.
		/// </value>
		[DataMember( Name = "depth" )]
		public int Depth
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the direction.
		/// </summary>
		/// <value>
		///     The direction.
		/// </value>
		[DataMember( Name = "direction" )]
		public string Direction
		{
			get;
			set;
		}
	}
}