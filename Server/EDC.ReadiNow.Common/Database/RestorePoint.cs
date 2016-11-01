// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Database
{
	/// <summary>
	///     Restore Point class.
	/// </summary>
	[DataContract]
	public class RestorePoint
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RestorePoint" /> class.
		/// </summary>
		public RestorePoint( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RestorePoint" /> class.
		/// </summary>
		/// <param name="reader">The reader.</param>
		internal RestorePoint( IDataReader reader )
			: this( )
		{
			if ( reader == null )
			{
				throw new ArgumentNullException( nameof( reader ) );
			}

			UserName = reader.GetString( 0, "Unknown" );

			int year = reader.GetInt32( 1 );
			int month = reader.GetInt32( 2 );
			int day = reader.GetInt32( 3 );
			int hour = reader.GetInt32( 4 );
			int minute = reader.GetInt32( 5 );

			Date = new DateTime( year, month, day, hour, minute, 0, DateTimeKind.Utc );
		}

		/// <summary>
		///     Gets or sets the date.
		/// </summary>
		/// <value>
		///     The date.
		/// </value>
		[DataMember( Name = "date" )]
		public DateTime Date
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name of the user.
		/// </summary>
		/// <value>
		///     The name of the user.
		/// </value>
		[DataMember( Name = "userName" )]
		public string UserName
		{
			get;
			set;
		}
	}
}