// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Scheduling.iCalendar.Serialization
{
	/// <summary>
	///     IEncodingProvider interface.
	/// </summary>
	public interface IEncodingProvider
	{
		/// <summary>
		///     Decodes the data.
		/// </summary>
		/// <param name="encoding">The encoding.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		byte[] DecodeData( string encoding, string value );

		/// <summary>
		///     Decodes the string.
		/// </summary>
		/// <param name="encoding">The encoding.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		string DecodeString( string encoding, string value );

		/// <summary>
		///     Encodes the specified encoding.
		/// </summary>
		/// <param name="encoding">The encoding.</param>
		/// <param name="data">The data.</param>
		/// <returns></returns>
		string Encode( string encoding, byte[] data );
	}
}