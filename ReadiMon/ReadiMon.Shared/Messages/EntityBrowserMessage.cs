// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;
using ReadiMon.Shared.Core;

namespace ReadiMon.Shared.Messages
{
	/// <summary>
	///     Entity Browser message.
	/// </summary>
	[DataContract( Name = "EntityBrowserMessage" )]
	public class EntityBrowserMessage
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EntityBrowserMessage" /> class.
		/// </summary>
		/// <param name="entity">The entity identifier.</param>
		public EntityBrowserMessage( string entity )
		{
			Entity = entity;
		}

		/// <summary>
		///     Gets or sets the entity identifier.
		/// </summary>
		/// <value>
		///     The entity identifier.
		/// </value>
		[DataMember]
		public string Entity
		{
			get;
			set;
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			return Serializer.SerializeObject( this );
		}
	}
}