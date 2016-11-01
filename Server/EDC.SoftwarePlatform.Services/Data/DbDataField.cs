// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.Services.Data
{
	/// <summary>
	///     Defines a data field within a data table.
	/// </summary>
	[DataContract]
	public class DbDataField
	{
		/// <summary>
		///     Initializes a new instance of the DbDataField class.
		/// </summary>
		public DbDataField( )
		{
			Value = null;
		}

		/// <summary>
		///     Initializes a new instance of the DbDataField class.
		/// </summary>
		public DbDataField( object value )
		{
			Value = value == null ? null : value.ToString( );
		}

		/// <summary>
		///     Gets or sets the value of the data field.
		/// </summary>
		[DataMember]
		public string Value
		{
			get;
			set;
		}
	}
}