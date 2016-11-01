// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Text;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     IAttachment interface.
	/// </summary>
	public interface IAttachment : IEncodableDataType
	{
		/// <summary>
		///     A binary representation of the data that was loaded.
		/// </summary>
		/// <value>
		///     The data.
		/// </value>
		byte[] Data
		{
			get;
			set;
		}

		/// <summary>
		///     To specify the content type of a referenced object.
		///     This optional value should be an IANA-registered
		///     MIME type, if specified.
		/// </summary>
		/// <value>
		///     The type of the format.
		/// </value>
		string FormatType
		{
			get;
			set;
		}

		/// <summary>
		///     The URI where the attachment information can be located.
		/// </summary>
		/// <value>
		///     The URI.
		/// </value>
		Uri Uri
		{
			get;
			set;
		}

		/// <summary>
		///     A Unicode-encoded version of the data that was loaded.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		string Value
		{
			get;
			set;
		}

		/// <summary>
		///     Gets/sets the encoding used to store the value.
		/// </summary>
		/// <value>
		///     The value encoding.
		/// </value>
		Encoding ValueEncoding
		{
			get;
			set;
		}

		/// <summary>
		///     Loads (fills) the <c>Data</c> property with the file designated
		///     at the given <see cref="Uri" />.
		/// </summary>
		void LoadDataFromUri( );

		/// <summary>
		///     Loads (fills) the <c>Data</c> property with the file designated
		///     at the given <see cref="Uri" />.
		/// </summary>
		/// <param name="username">The username to supply for credentials</param>
		/// <param name="password">The password to supply for credentials</param>
		void LoadDataFromUri( string username, string password );

		/// <summary>
		///     Loads (fills) the <c>Data</c> property with the file designated
		///     at the given <see cref="Uri" />.
		/// </summary>
		/// <param name="uri">
		///     The Uri from which to download the <c>Data</c>
		/// </param>
		/// <param name="username">The username to supply for credentials</param>
		/// <param name="password">The password to supply for credentials</param>
		void LoadDataFromUri( Uri uri, string username, string password );
	}
}