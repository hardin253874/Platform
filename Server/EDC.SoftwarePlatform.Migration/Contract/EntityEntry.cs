// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Serialization;
using EDC.ReadiNow.Annotations;
using Jil;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	/// <summary>
	///     Holds a row returned from the database being imported.
	///     All IDs are global and refer to the UpgradeId of the applicable entity.
	/// </summary>
	[DataContract( Name = "entityEntry" )]
	public class EntityEntry : IEntry<Guid>
	{
		/// <summary>
		///     The ID of the entity being represented. Note: this ID is only meaningful within
		///     the data store. (In practice it is the tenant-specific entity ID at the time of export,
		///     unless the row represents a deletion, in which case it is the global-tenant ID at the time
		///     of export).
		/// </summary>
		[DataMember( Name = "uid", IsRequired = true, EmitDefaultValue = true )]
		public Guid EntityId;

		/// <summary>
		///     The type of change that this data row represents.
		///     Optional.
		/// </summary>
		[DataMember( Name = "state", IsRequired = false, EmitDefaultValue = false )]
		[JilDirective( TreatEnumerationAs = typeof( Int32 ) )]
		public DataState State;

		/// <summary>
		///		Whether the named property should be serialized.
		/// </summary>
		/// <returns></returns>
		[UsedImplicitly]
		private bool ShouldSerializeState( )
	    {
			return State != DataState.Unchanged;
	    }

		/// <summary>
		///     Gets the primary key.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		///     Implementer can assume that this will be only called for two objects with identical keys.
		/// </remarks>
		public Guid GetKey( )
		{
			return EntityId;
		}

		/// <summary>
		///     Compares the non-key component of two entries.
		/// </summary>
		/// <param name="alt">The alt.</param>
		/// <returns>
		///     <c>true</c> if [is same data] [the specified alt]; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="System.InvalidOperationException"></exception>
		/// <remarks>
		///     Implementer can assume that this will be only called for two objects with identical keys.
		/// </remarks>
		public bool IsSameData( object alt )
		{
			// This operation doesn't make sense for EntityEntry
			return true;
		}

		/// <summary>
		///     Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		///     A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString( )
		{
			return EntityId.ToString( "B" );
		}
	}
}