// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Database
{
	/// <summary>
	///     Tenant Rollback data
	/// </summary>
	[DataContract]
	public class TenantRollbackData
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="TenantRollbackData" /> class.
		/// </summary>
		public TenantRollbackData( )
		{
			RestorePoints = new List<RestorePoint>( );
			CustomRestorePoints = new List<CustomRestorePoint>( );
			RollbackLogs = new List<RollbackLog>( );
		}

		/// <summary>
		///     Gets or sets the restore points.
		/// </summary>
		/// <value>
		///     The restore points.
		/// </value>
		[DataMember( Name = "restorePoints" )]
		public IList<RestorePoint> RestorePoints
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the custom restore points.
		/// </summary>
		/// <value>
		/// The custom restore points.
		/// </value>
		[DataMember( Name = "customRestorePoints" )]
		public IList<CustomRestorePoint> CustomRestorePoints
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the rollback logs.
		/// </summary>
		/// <value>
		///     The rollback logs.
		/// </value>
		[DataMember( Name = "rollbackLogs" )]
		public IList<RollbackLog> RollbackLogs
		{
			get;
			set;
		}
	}
}