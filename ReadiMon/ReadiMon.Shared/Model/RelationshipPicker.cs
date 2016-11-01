// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Data;

namespace ReadiMon.Shared.Model
{
	/// <summary>
	///     The RelationshipPicker class.
	/// </summary>
	public class RelationshipPicker : ViewModelBase
	{
		private Instance _selectedInstance;
		private Visibility _visible;

		/// <summary>
		///     Initializes a new instance of the <see cref="RelationshipPicker" /> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="upgradeId">The upgrade identifier.</param>
		/// <param name="name">The name.</param>
		/// <param name="description">The description.</param>
		/// <param name="fromId">From identifier.</param>
		/// <param name="from">From.</param>
		/// <param name="toId">To identifier.</param>
		/// <param name="to">To.</param>
		/// <param name="cardinality">The cardinality.</param>
		/// <param name="isForward">if set to <c>true</c> [is forward].</param>
		/// <param name="pluginSettings">The plugin settings.</param>
		public RelationshipPicker( long id, long tenantId, Guid upgradeId, string name, string description, long fromId, string from, long toId, string to, string cardinality, bool isForward, IPluginSettings pluginSettings )
		{
			Id = id;
			TenantId = tenantId;
			UpgradeId = upgradeId;
			Name = name;
			Description = description;
			FromId = fromId;
			From = from;
			ToId = toId;
			To = to;
			Cardinality = cardinality;
			IsForward = isForward;
			PluginSettings = pluginSettings;

			Visible = Visibility.Hidden;
		}

		/// <summary>
		///     Gets the cardinality.
		/// </summary>
		/// <value>
		///     The cardinality.
		/// </value>
		public string Cardinality
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="RelationshipPicker" /> is disabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if disabled; otherwise, <c>false</c>.
		/// </value>
		public bool Disabled
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets from.
		/// </summary>
		/// <value>
		///     From.
		/// </value>
		public string From
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets from identifier.
		/// </summary>
		/// <value>
		///     From identifier.
		/// </value>
		public long FromId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the identifier.
		/// </summary>
		/// <value>
		///     The identifier.
		/// </value>
		public long Id
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the instances.
		/// </summary>
		/// <value>
		///     The instances.
		/// </value>
		public List<Instance> Instances
		{
			get
			{
				List<Instance> instances = new List<Instance>( );

				if ( Visible == Visibility.Visible )
				{
					GetInstances( instances );

					if ( instances.Count > 0 )
					{
						SelectedInstance = instances[ 0 ];
					}
				}

				return instances;
			}
		}

		/// <summary>
		///     Gets a value indicating whether this instance is forward.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is forward; otherwise, <c>false</c>.
		/// </value>
		public bool IsForward
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the plugin settings.
		/// </summary>
		/// <value>
		///     The plugin settings.
		/// </value>
		public IPluginSettings PluginSettings
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the selected instance.
		/// </summary>
		/// <value>
		///     The selected instance.
		/// </value>
		public Instance SelectedInstance
		{
			get
			{
				return _selectedInstance;
			}
			set
			{
				SetProperty( ref _selectedInstance, value );
			}
		}

		/// <summary>
		///     Gets the tenant identifier.
		/// </summary>
		/// <value>
		///     The tenant identifier.
		/// </value>
		public long TenantId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets to.
		/// </summary>
		/// <value>
		///     To.
		/// </value>
		public string To
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets to identifier.
		/// </summary>
		/// <value>
		///     To identifier.
		/// </value>
		public long ToId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the upgrade identifier.
		/// </summary>
		/// <value>
		///     The upgrade identifier.
		/// </value>
		public Guid UpgradeId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets a value indicating whether this <see cref="RelationshipPicker" /> is visible.
		/// </summary>
		/// <value>
		///     <c>true</c> if visible; otherwise, <c>false</c>.
		/// </value>
		public Visibility Visible
		{
			get
			{
				return _visible;
			}
			set
			{
				SetProperty( ref _visible, value );

				OnPropertyChanged( "Instances" );
			}
		}

		private void GetInstances( List<Instance> instances )
		{
			try
			{
				const string commandText = @"--ReadiMon - LoadTypes
DECLARE @inherits BIGINT = dbo.fnAliasNsId( 'inherits', 'core', @tenantId )
DECLARE @isOfType BIGINT = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
DECLARE @inSolution BIGINT = dbo.fnAliasNsId( 'inSolution', 'core', @tenantId )
DECLARE @name BIGINT = dbo.fnAliasNsId( 'name', 'core', @tenantId )
DECLARE @alias BIGINT = dbo.fnAliasNsId( 'alias', 'core', @tenantId )
DECLARE @description BIGINT = dbo.fnAliasNsId( 'description', 'core', @tenantId )
DECLARE @createdDate BIGINT = dbo.fnAliasNsId( 'createdDate', 'core', @tenantId )
DECLARE @modifiedDate BIGINT = dbo.fnAliasNsId( 'modifiedDate', 'core', @tenantId )

SELECT DISTINCT
	[Id] = r.FromId,
	[Name] = n.Data,
	[Description] = d.Data,
	[Alias] = ia.Data,
	[TypeName] = tn.Data,
	[CreatedDate] = cd.Data,
	[ModifiedDate] = md.Data,
	[Solutions] = SUBSTRING(
		(
			SELECT
				',' + sn.Data AS [text()]
			FROM
				Relationship iis
			LEFT JOIN
				Data_NVarChar sn ON
				iis.TenantId = sn.TenantId
				AND sn.EntityId = iis.ToId
				AND sn.FieldId = @name
			WHERE
				r.TenantId = iis.TenantId
				AND iis.FromId = r.FromId
				AND iis.TypeId = @inSolution
			ORDER BY
				sn.Data
			FOR XML PATH('')
		), 2, 10000 )
FROM
	dbo.fnDescendantsAndSelf( @inherits, @typeId, @tenantId ) a
JOIN
	Relationship r ON
		r.TenantId = @tenantId
		AND a.Id = r.ToId
		AND r.TypeId = @isOfType
LEFT JOIN
	Data_Alias ia ON
		r.TenantId = ia.TenantId
		AND ia.EntityId = r.FromId
		AND ia.FieldId = @alias
LEFT JOIN
	Data_NVarChar n ON
		r.TenantId = n.TenantId
		AND n.EntityId = r.FromId
		AND n.FieldId = @name
LEFT JOIN
	Data_NVarChar d ON
		r.TenantId = d.TenantId
		AND d.EntityId = r.FromId
		AND d.FieldId = @description
LEFT JOIN
	Data_NVarChar tn ON
		r.TenantId = tn.TenantId
		AND tn.EntityId = r.ToId
		AND tn.FieldId = @name
LEFT JOIN
	Data_DateTime cd ON
		r.TenantId = cd.TenantId
		AND cd.EntityId = r.FromId
		AND cd.FieldId = @createdDate
LEFT JOIN
	Data_DateTime md ON
		r.TenantId = md.TenantId
		AND md.EntityId = r.FromId
		AND md.FieldId = @modifiedDate
ORDER BY
	n.Data, r.FromId";

				DatabaseManager dbManager = new DatabaseManager( PluginSettings.DatabaseSettings );

				using ( var command = dbManager.CreateCommand( commandText ) )
				{
					dbManager.AddParameter( command, "@tenantId", TenantId );
					dbManager.AddParameter( command, "@typeId", IsForward ? ToId : FromId );

					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							long id = reader.GetInt64( 0 );
							string name = reader.GetString( 1, "<Unnamed>" );
							string description = reader.GetString( 2, null );
							string alias = reader.GetString( 3, null );
							string typeName = reader.GetString( 4, null );
							DateTime createdDate = reader.GetDateTime( 5, DateTime.MinValue );
							DateTime modifiedDate = reader.GetDateTime( 6, DateTime.MinValue );
							string applications = reader.GetString( 7, null );

							var instance = new Instance( id, name, description, alias, typeName, createdDate, modifiedDate, applications );

							instances.Add( instance );
						}
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}
		}
	}
}