// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	/// <summary>
	///     Holds a row of field data returned from the database being imported.
	///     All IDs are global and refer to the UpgradeId of the applicable entity.
	/// </summary>
	public class DataEntry : IEntry<Tuple<Guid, Guid>>
	{
		/// <summary>
		///     Additional data associated with this row. (I.e. the alias marker id)
		/// </summary>
		public int AliasMarkerId;

		/// <summary>
		///     The data, in its natural .Net type.
		/// </summary>
		public object Data;

		/// <summary>
		///     ID of the entity that this field data is for.
		/// </summary>
		public Guid EntityId;

		/// <summary>
		///     The existing data, in its natural .Net type.
		/// </summary>
		public object ExistingData;

		/// <summary>
		///     ID of the field that this data is for.
		/// </summary>
		public Guid FieldId;

		/// <summary>
		///     Additional data associated with this row. (I.e. the alias name space)
		/// </summary>
		public string Namespace;

		/// <summary>
		///     The type of change that this data row represents.
		/// </summary>
		public DataState State;

		/// <summary>
		///     Gets the primary key.
		/// </summary>
		public Tuple<Guid, Guid> GetKey( )
		{
			return new Tuple<Guid, Guid>( EntityId, FieldId );
		}

		/// <summary>
		///     Compares the non-key component of two entries.
		/// </summary>
		/// <remarks>
		///     Implementer can assume that this will be only called for two objects with identical keys.
		/// </remarks>
		public bool IsSameData( object alt )
		{
			var other = ( DataEntry ) alt;
			return Equals( Data, other.Data )
			       && Equals( Namespace, other.Namespace )
			       && Equals( AliasMarkerId, other.AliasMarkerId );
		}
	}
}