// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Entity
{
	/// <summary>
	///     Json EntityRef class.
	/// </summary>
	[DataContract]
	public class JsonEntityRef
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="JsonEntityRef" /> class.
		/// </summary>
		public JsonEntityRef( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="JsonEntityRef" /> class.
		/// </summary>
		/// <param name="entityRef">The entity reference.</param>
		public JsonEntityRef( EntityRef entityRef )
		{
			Id = entityRef.Id;
			NameSpace = entityRef.Namespace;
			Alias = entityRef.Alias;
		}

		/// <summary>
		///     Gets or sets the identifier.
		/// </summary>
		/// <value>
		///     The identifier.
		/// </value>
		[DataMember( Name = "id" )]
		public long Id
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the alias.
		/// </summary>
		/// <value>
		///     The alias.
		/// </value>
		[DataMember( Name = "alias" )]
		public string Alias
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name space.
		/// </summary>
		/// <value>
		///     The name space.
		/// </value>
		[DataMember( Name = "ns" )]
		public string NameSpace
		{
			get;
			set;
		}

		/// <summary>
		///     To the entity reference.
		/// </summary>
		/// <returns></returns>
		public EntityRef ToEntityRef( )
		{
			if ( !string.IsNullOrEmpty( Alias ) )
			{
				return new EntityRef( NameSpace, Alias );
			}
			return new EntityRef( Id );
		}
	}
}