// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	/// <summary>
	///     Holds a row returned from the database being imported.
	///     All IDs are global and refer to the UpgradeId of the applicable entity.
	/// </summary>
	public class RelationshipEntry : IEntry<RelationshipEntryKey>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RelationshipEntry" /> class.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="fromId">From identifier.</param>
		/// <param name="toId">To identifier.</param>
		public RelationshipEntry( Guid typeId, Guid fromId, Guid toId )
		{
			TypeId = typeId;
			FromId = fromId;
			ToId = toId;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RelationshipEntry" /> class.
		/// </summary>
		/// <param name="typeId">The type identifier.</param>
		/// <param name="fromId">From identifier.</param>
		/// <param name="toId">To identifier.</param>
		/// <param name="cardinality">The cardinality.</param>
		public RelationshipEntry( Guid typeId, Guid fromId, Guid toId, CardinalityEnum_Enumeration cardinality )
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
		}

		/// <summary>
		///     Gets the previous value.
		/// </summary>
		/// <value>
		///     The previous value.
		/// </value>
		public Guid? PreviousValue
		{
			get;
			set;
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
		}

		/// <summary>
		///     Gets or sets a value indicating whether to update the from value.
		/// </summary>
		/// <value>
		///     <c>true</c> if the from value requires updating; otherwise, <c>false</c>.
		/// </value>
		public bool UpdateFrom
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether to update the to value.
		/// </summary>
		/// <value>
		///     <c>true</c> if the to value requires updating; otherwise, <c>false</c>.
		/// </value>
		public bool UpdateTo
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the key.
		/// </summary>
		/// <returns></returns>
		public RelationshipEntryKey GetKey( )
		{
			return new RelationshipEntryKey( TypeId, FromId, ToId, Cardinality );
		}

		/// <summary>
		///     Compares the non-key component of two entries.
		/// </summary>
		/// <remarks>
		///     Implementer can assume that this will be only called for two objects with identical keys.
		/// </remarks>
		public bool IsSameData( object alt )
		{
			var entry = ( RelationshipEntry ) alt;

			if ( Cardinality == CardinalityEnum_Enumeration.ManyToOne )
			{
				return entry.ToId == ToId;
			}

			if ( Cardinality == CardinalityEnum_Enumeration.OneToMany )
			{
				return entry.FromId == FromId;
			}

			if ( Cardinality == CardinalityEnum_Enumeration.OneToOne )
			{
				return entry.FromId == FromId && entry.ToId == ToId;
			}

			return true;
		}
	}
}