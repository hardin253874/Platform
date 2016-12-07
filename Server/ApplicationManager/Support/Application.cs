// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Input;
using ApplicationManager.Core;
using EDC.Database;
using EDC.ReadiNow.Database;

namespace ApplicationManager.Support
{
	/// <summary>
	///     Application
	/// </summary>
	public class Application : ViewModelBase
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="Application" /> class.
		/// </summary>
		public Application( )
		{
			ActionCommand = new DelegateCommand<string>( HandleAction );
			PublisherCommand = new DelegateCommand( HandlePublisher );
		}

		/// <summary>
		///     Gets or sets the action command.
		/// </summary>
		/// <value>
		///     The action command.
		/// </value>
		public ICommand ActionCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the application id.
		/// </summary>
		/// <value>
		///     The application id.
		/// </value>
		public Guid ApplicationId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
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
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the publisher.
		/// </summary>
		/// <value>
		///     The publisher.
		/// </value>
		public string Publisher
		{
			get;
			set;
		}

		public ICommand PublisherCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the publisher URL.
		/// </summary>
		/// <value>
		///     The publisher URL.
		/// </value>
		public string PublisherUrl
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the refresh all.
		/// </summary>
		/// <value>
		///     The refresh all.
		/// </value>
		public Action RefreshAll
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the release date.
		/// </summary>
		/// <value>
		///     The release date.
		/// </value>
		public DateTime ReleaseDate
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the solution entity id.
		/// </summary>
		/// <value>
		///     The solution entity id.
		/// </value>
		public long SolutionEntityId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the applications.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<Application> GetApplications( )
		{
			var applications = new List<Application>( );

			/////
			// Set the base properties
			/////
			var databaseInfo = new SqlDatabaseInfo( Config.ServerName, Config.DatabaseName, DatabaseAuthentication.Integrated, null, 60, 30, 300 );

			using ( DatabaseContext ctx = DatabaseContext.GetContext( databaseInfo: databaseInfo ) )
			{
				using ( IDbCommand command = ctx.CreateCommand( ) )
				{
					command.CommandText = @"
SELECT
	SolutionEntityId = r.FromId,
	SolutionName = n.Data,
	SolutionDescription = d.Data,
	Publisher = p.Data,
	PublisherUrl = u.Data,
	ReleaseDate = c.Data,
	ApplicationId = g.Data
FROM
	Relationship r
JOIN
	Data_Alias aty ON
		aty.TenantId = r.TenantId AND
		r.TypeId = aty.EntityId AND
		aty.Data = 'isOfType' AND
		aty.Namespace = 'core'
JOIN
	Data_Alias ato ON
		ato.TenantId = r.TenantId AND
		r.ToId = ato.EntityId AND
		ato.Data = 'app' AND
		ato.Namespace = 'core'
JOIN
	Data_Guid g ON
		g.TenantId = r.TenantId AND
		r.FromId = g.EntityId
JOIN
	Data_Alias ag ON
		ag.TenantId = g.TenantId AND
		g.FieldId = ag.EntityId AND
		ag.Data = 'applicationId' AND
		ag.Namespace = 'core'
JOIN
(
	SELECT
		n.EntityId,
		n.TenantId,
		n.Data
	FROM
		Data_NVarChar n
	JOIN
		Data_Alias an ON
			n.TenantId = an.TenantId AND
			n.FieldId = an.EntityId AND
			an.Data = 'name' AND
			an.Namespace = 'core'
	WHERE
		n.TenantId = 0
) n ON
	n.TenantId = r.TenantId AND
	r.FromId = n.EntityId
LEFT JOIN
(
	SELECT
		d.EntityId,
		d.TenantId,
		d.Data
	FROM
		Data_NVarChar d
	JOIN
		Data_Alias ad ON
			d.TenantId = ad.TenantId AND
			d.FieldId = ad.EntityId AND
			ad.Data = 'description' AND
			ad.Namespace = 'core'
	WHERE
		d.TenantId = 0
) d ON
	d.TenantId = r.TenantId AND
	r.FromId = d.EntityId
LEFT JOIN
(
	SELECT
		p.EntityId,
		p.TenantId,
		p.Data
	FROM
		Data_NVarChar p
	JOIN
		Data_Alias ap ON
			p.TenantId = ap.TenantId AND
			p.FieldId = ap.EntityId AND
			ap.Data = 'publisher' AND
			ap.Namespace = 'core'
) p ON
	p.TenantId = r.TenantId AND
	r.FromId = p.EntityId
LEFT JOIN
(
	SELECT
		u.EntityId,
		u.TenantId,
		u.Data
	FROM
		Data_NVarChar u
	JOIN
		Data_Alias au ON
			u.TenantId = au.TenantId AND
			u.FieldId = au.EntityId AND
			au.Data = 'publisherUrl' AND
			au.Namespace = 'core'
	WHERE
		u.TenantId = 0
) u ON
	u.TenantId = r.TenantId AND
	r.FromId = u.EntityId
LEFT JOIN
(
	SELECT
		c.EntityId,
		c.TenantId,
		c.Data
	FROM
		Data_DateTime c
	JOIN
		Data_Alias ac ON
		c.TenantId = ac.TenantId AND
			c.FieldId = ac.EntityId AND
			ac.Data = 'releaseDate' AND
			ac.Namespace = 'core'
	WHERE
		c.TenantId = 0
) c ON
	c.TenantId = r.TenantId AND
	r.FromId = c.EntityId
WHERE
	r.TenantId = 0";

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							var app = new Application
							{
								SolutionEntityId = reader.GetInt64( 0 ),
								Name = reader.GetString( 1 ),
								Description = reader.GetString( 2, null ),
								Publisher = reader.GetString( 3, null ),
								PublisherUrl = reader.GetString( 4, null ),
								ReleaseDate = reader.GetDateTime( 5, DateTime.MinValue ),
								ApplicationId = reader.GetGuid( 6 )
							};

							applications.Add( app );
						}
					}
				}
			}

			return applications;
		}

		/// <summary>
		///     Gets the version string.
		/// </summary>
		/// <returns></returns>
		public string GetVersionString( )
		{
			/////
			// Set the base properties
			/////
			var databaseInfo = new SqlDatabaseInfo( Config.ServerName, Config.DatabaseName, DatabaseAuthentication.Integrated, null, 60, 30, 300 );

			using ( DatabaseContext ctx = DatabaseContext.GetContext( databaseInfo: databaseInfo ) )
			{
				using ( IDbCommand command = ctx.CreateCommand( ) )
				{
					command.CommandText = @"
SELECT
	v.Data
FROM
	AppData_NVarChar v
JOIN
	AppData_Alias a ON a.EntityUid = v.FieldUid
	AND
		a.Data = 'solutionVersionString'
	AND
		a.Namespace = 'core'
	AND
		a.AliasMarkerId = 0
WHERE
	v.EntityUid = @entityUid";

					ctx.AddParameter( command, "@entityUid", DbType.Guid, ApplicationId );

					object val = command.ExecuteScalar( );

					if ( val == null || val == DBNull.Value )
					{
						return string.Empty;
					}

					return ( string ) val;
				}
			}
		}

		/// <summary>
		///     Handles the action.
		/// </summary>
		/// <param name="actionString">The action string.</param>
		private void HandleAction( string actionString )
		{
			var action = ( ApplicationAction ) Enum.Parse( typeof( ApplicationAction ), actionString );

			switch ( action )
			{
				case ApplicationAction.Delete:
				{
					var delete = new DeleteApplication( this );
					delete.ShowDialog( );

					if ( delete.ViewModel.PackagesDeleted )
					{
						RefreshAll( );
					}
				}
					break;

				case ApplicationAction.Deploy:
				{
					var deploy = new DeployApplication( this );
					deploy.ShowDialog( );
				}
					break;

				case ApplicationAction.Export:
				{
					var export = new ExportApplication( this );
					export.ShowDialog( );
				}
					break;

				case ApplicationAction.Repair:
				{
					var repair = new RepairApplication( this );
					repair.ShowDialog( );
				}
					break;

				default:
				{
				}
					break;
			}
		}

		/// <summary>
		///     Handles the publisher.
		/// </summary>
		private void HandlePublisher( )
		{
			Process.Start( PublisherUrl );
		}
	}
}