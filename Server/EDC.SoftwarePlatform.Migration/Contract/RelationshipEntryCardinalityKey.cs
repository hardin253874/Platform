// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	public class RelationshipEntryCardinalityKey
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RelationshipEntryKey" /> class.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="sourceId">The source identifier.</param>
		public RelationshipEntryCardinalityKey( Guid typeId, Guid sourceId )
		{
			TypeId = typeId;
			SourceId = sourceId;
		}

		/// <summary>
		///     Gets the source identifier.
		/// </summary>
		/// <value>
		///     The source identifier.
		/// </value>
		public Guid SourceId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the type identifier.
		/// </summary>
		/// <value>
		///     The type identifier.
		/// </value>
		public Guid TypeId
		{
			get;
			private set;
		}

		/// <summary>
		///     Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals( object obj )
		{
			var key = obj as RelationshipEntryCardinalityKey;

			if ( key == null )
			{
				return false;
			}

			if ( key == this )
			{
				return true;
			}

			return TypeId == key.TypeId && SourceId == key.SourceId;
		}

		/// <summary>
		///     Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode( )
		{
			unchecked
			{
				int hash = 17;

				hash = hash * 92821 + TypeId.GetHashCode( );

				hash = hash * 92821 + SourceId.GetHashCode( );

				return hash;
			}
		}
	}
}