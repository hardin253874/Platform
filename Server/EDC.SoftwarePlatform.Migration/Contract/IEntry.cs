// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.SoftwarePlatform.Migration.Contract
{
	/// <summary>
	///     Represents some form of data.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	internal interface IEntry<out TKey>
	{
		/// <summary>
		///     Gets an object (possibly tuple) that represents the primary key component of this entry.
		/// </summary>
		TKey GetKey( );

		/// <summary>
		///     Compares the non-key component of two entries.
		/// </summary>
		/// <remarks>
		///     Implementer can assume that this will be only called for two objects with identical keys.
		/// </remarks>
		bool IsSameData( object alt );
	}
}