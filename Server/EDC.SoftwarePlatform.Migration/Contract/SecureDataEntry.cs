// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	/// <summary>
	///     Holds a row of secureData returned from the database being imported.
    ///     The Data is assumed to have been encrypted by DPAPI
	/// </summary>
	public class SecureDataEntry : IEntry<Guid>
	{
		/// <summary>
		///     The primary key for the secureData
		/// </summary>
		public Guid SecureId;

		/// <summary>
		///     The encrypted data.
		/// </summary>
		public Byte[] Data;

		/// <summary>
		///     Context for the secure data
		/// </summary>
		public string Context;

		/// <summary>
		///     The type of change that this data row represents.
		/// </summary>
		public DataState State;

        /// <summary>
        /// Constructor for a SecureDataEntry
        /// </summary>
        public SecureDataEntry(Guid secureId, string context, Byte[] data)
        {
            SecureId = secureId;
            Context = context;
            Data = data;
        }

		/// <summary>
		///     Gets the primary key.
		/// </summary>
		public Guid GetKey( )
		{
			return SecureId;
		}

		/// <summary>
		///     Compares the non-key component of two entries.
		/// </summary>
		/// <remarks>
		///     Implementer can assume that this will be only called for two objects with identical keys.
		/// </remarks>
		public bool IsSameData( object alt )
		{
			var other = ( SecureDataEntry ) alt;
            return Equals(Context, other.Context)
                   && Equals(Data, other.Data);
		}
	}
}