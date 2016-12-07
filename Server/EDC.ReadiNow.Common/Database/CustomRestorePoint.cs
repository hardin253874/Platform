// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Database
{
	/// <summary>
	///     Custom Restore Point class.
	/// </summary>
	[DataContract]
	public class CustomRestorePoint
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="CustomRestorePoint" /> class.
		/// </summary>
		public CustomRestorePoint( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="CustomRestorePoint" /> class.
		/// </summary>
		/// <param name="reader">The reader.</param>
		internal CustomRestorePoint( IDataReader reader )
			: this( )
		{
			if ( reader == null )
			{
				throw new ArgumentNullException( nameof( reader ) );
			}

			Date = reader.GetDateTime( 0, DateTimeKind.Utc );
			Name = reader.GetString( 1, null );
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
		///     Gets or sets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		[DataMember( Name = "name" )]
		public string Name
		{
			get;
			set;
		}
	}
}