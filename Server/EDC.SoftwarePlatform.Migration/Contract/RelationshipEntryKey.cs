// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	/// <summary>
	///     Relationship Entry Key
	/// </summary>
	public class RelationshipEntryKey
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RelationshipEntryKey" /> class.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="fromId">From identifier.</param>
		/// <param name="toId">To identifier.</param>
		public RelationshipEntryKey( Guid typeId, Guid fromId, Guid toId )
		{
			TypeId = typeId;
			FromId = fromId;
			ToId = toId;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RelationshipEntryKey" /> class.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="fromId">From identifier.</param>
		/// <param name="toId">To identifier.</param>
		/// <param name="cardinality">The cardinality.</param>
		public RelationshipEntryKey( Guid typeId, Guid fromId, Guid toId, CardinalityEnum_Enumeration? cardinality )
			: this( typeId, fromId, toId )
		{
			Cardinality = cardinality;
		}

		/// <summary>
		///     Gets the cardinality.
		/// </summary>
		/// <value>
		///     The cardinality.
		/// </value>
		public CardinalityEnum_Enumeration? Cardinality
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets from identifier.
		/// </summary>
		/// <value>
		///     From identifier.
		/// </value>
		public Guid FromId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets to identifier.
		/// </summary>
		/// <value>
		///     To identifier.
		/// </value>
		public Guid ToId
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
			var key = obj as RelationshipEntryKey;

			if ( key == null )
			{
				return false;
			}

			if ( key == this )
			{
				return true;
			}

			return TypeId == key.TypeId && FromId == key.FromId && ToId == key.ToId && Cardinality == key.Cardinality;
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

				hash = hash * 92821 + FromId.GetHashCode( );

				hash = hash * 92821 + ToId.GetHashCode( );

				if ( Cardinality != null )
				{
					hash = hash * 92821 + Cardinality.GetHashCode( );
				}

				return hash;
			}
		}
	}
}