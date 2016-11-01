// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiMon.Plugin.Database.Status
{
	/// <summary>
	///     Class representing the SqlServerKeyValueProperty type.
	/// </summary>
	public class SqlServerKeyValueProperty
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="SqlServerKeyValueProperty" /> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public SqlServerKeyValueProperty( string key, string value )
		{
			Key = key;
			Value = value;
		}

		/// <summary>
		///     Gets or sets the key.
		/// </summary>
		/// <value>
		///     The key.
		/// </value>
		public string Key
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		public string Value
		{
			get;
			private set;
		}
	}
}