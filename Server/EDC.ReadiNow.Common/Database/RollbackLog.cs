// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Data;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Database
{
	/// <summary>
	///     Rollback log class.
	/// </summary>
	[DataContract]
	public class RollbackLog
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="RollbackLog" /> class.
		/// </summary>
		public RollbackLog( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="RollbackLog" /> class.
		/// </summary>
		/// <param name="reader">The reader.</param>
		internal RollbackLog( IDataReader reader )
		{
			if ( reader == null )
			{
				throw new ArgumentNullException( nameof( reader ) );
			}

			Date = reader.GetDateTime( 0, DateTimeKind.Utc );
			RollbackDate = reader.GetDateTime( 1, DateTimeKind.Utc );
			UserName = reader.GetString( 2, null );
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
		///     Gets or sets the rollback date.
		/// </summary>
		/// <value>
		///     The rollback date.
		/// </value>
		[DataMember( Name = "rollbackDate" )]
		public DateTime RollbackDate
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the username.
		/// </summary>
		/// <value>
		///     The username.
		/// </value>
		[DataMember( Name = "userName" )]
		public string UserName
		{
			get;
			set;
		}
	}
}