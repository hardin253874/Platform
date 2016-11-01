// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows.Input;
using ReadiMon.Plugin.Database.Status;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Data;

namespace ReadiMon.Plugin.Database
{
	/// <summary>
	///     General Settings View Model
	/// </summary>
	public class GeneralSettingsViewModel : ViewModelBase
	{
		/// <summary>
		///     The databases
		/// </summary>
		private List<SqlServerDatabase> _databases;

		/// <summary>
		///     The details
		/// </summary>
		private List<SqlServerKeyValueProperty> _details;

		/// <summary>
		///     The instances
		/// </summary>
		private List<SqlServerInstance> _instances;

		/// <summary>
		///     The logins
		/// </summary>
		private List<SqlServerLogin> _logins;

		/// <summary>
		///     The plans
		/// </summary>
		private List<SqlServerMaintenancePlan> _plans;

		/// <summary>
		///     The plugin settings
		/// </summary>
		private IPluginSettings _pluginSettings;

		/// <summary>
		///     The server
		/// </summary>
		private string _server;

		/// <summary>
		///     The services
		/// </summary>
		private List<SqlServerService> _services;

		/// <summary>
		///     Initializes a new instance of the <see cref="GeneralSettingsViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public GeneralSettingsViewModel( IPluginSettings settings )
		{
			PluginSettings = settings;

			RefreshCommand = new DelegateCommand( Refresh );
		}

		/// <summary>
		///     Gets or sets the databases.
		/// </summary>
		/// <value>
		///     The databases.
		/// </value>
		public List<SqlServerDatabase> Databases
		{
			get
			{
				return _databases;
			}
			set
			{
				SetProperty( ref _databases, value );
			}
		}

		/// <summary>
		///     Gets or sets the details.
		/// </summary>
		/// <value>
		///     The details.
		/// </value>
		public List<SqlServerKeyValueProperty> Details
		{
			get
			{
				return _details;
			}
			set
			{
				SetProperty( ref _details, value );
			}
		}

		/// <summary>
		///     Gets or sets the instances.
		/// </summary>
		/// <value>
		///     The instances.
		/// </value>
		public List<SqlServerInstance> Instances
		{
			get
			{
				return _instances;
			}
			set
			{
				SetProperty( ref _instances, value );
			}
		}

		/// <summary>
		///     Gets or sets the logins.
		/// </summary>
		/// <value>
		///     The logins.
		/// </value>
		public List<SqlServerLogin> Logins
		{
			get
			{
				return _logins;
			}
			set
			{
				SetProperty( ref _logins, value );
			}
		}

		/// <summary>
		///     Gets or sets the plans.
		/// </summary>
		/// <value>
		///     The plans.
		/// </value>
		public List<SqlServerMaintenancePlan> Plans
		{
			get
			{
				return _plans;
			}
			set
			{
				SetProperty( ref _plans, value );
			}
		}

		/// <summary>
		///     Gets or sets the plugin settings.
		/// </summary>
		/// <value>
		///     The plugin settings.
		/// </value>
		public IPluginSettings PluginSettings
		{
			private get
			{
				return _pluginSettings;
			}
			set
			{
				_pluginSettings = value;

				Server = PluginSettings.DatabaseSettings.ServerName;

				LoadSettings( );
			}
		}

		/// <summary>
		///     Gets the refresh command.
		/// </summary>
		/// <value>
		///     The refresh command.
		/// </value>
		public ICommand RefreshCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the server.
		/// </summary>
		/// <value>
		///     The server.
		/// </value>
		public string Server
		{
			get
			{
				return _server;
			}
			set
			{
				SetProperty( ref _server, value );
			}
		}

		/// <summary>
		///     Gets or sets the services.
		/// </summary>
		/// <value>
		///     The services.
		/// </value>
		public List<SqlServerService> Services
		{
			get
			{
				return _services;
			}
			set
			{
				SetProperty( ref _services, value );
			}
		}

		/// <summary>
		///     Loads the settings.
		/// </summary>
		private void LoadSettings( )
		{
			const string commandText = @"--ReadiMon - LoadSettings
SET NOCOUNT ON 
 
--setup temp tables and variables 
CREATE TABLE #Instance (value VARCHAR(50),data VARCHAR(50)) 
CREATE TABLE #AuditData (value VARCHAR(50),data VARCHAR(100)) 
CREATE TABLE #msver (indx INT, name VARCHAR(50), internal_value INT, character_value VARCHAR(255)) 
CREATE TABLE #WinverSP (value    VARCHAR (255),data VARCHAR(255)) 
 
DECLARE @Instance VARCHAR(50) 
DECLARE @InstanceLoc VARCHAR(50) 
DECLARE @RegKey VARCHAR(255) 
DECLARE @CPUCount INT 
DECLARE @CPUID INT 
DECLARE @AffinityMask INT 
DECLARE @CPUList VARCHAR(50) 
DECLARE @InstCPUCount INT 
DECLARE @sql VARCHAR(255) 
DECLARE @Database VARCHAR(50) 
DECLARE @WINVERSP VARCHAR(255) 
 
INSERT INTO #msver EXEC xp_msver  
 
--get Windows server version and its service pack 
SET @RegKey = 'SOFTWARE\Microsoft\Windows NT\CurrentVersion' 
INSERT INTO #WinverSP EXEC xp_regread 'HKEY_LOCAL_MACHINE',@RegKey,'ProductName' 
INSERT INTO #WinverSP EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'CSDVersion' 
 
SELECT CAST(value AS VARCHAR(50)) AS value,  
       CAST(data AS VARCHAR(50)) AS data  
  FROM #WinverSP 
 
--get instance location FROM registry 
SET @RegKey = 'SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL' 
 
INSERT INTO #Instance EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, @@servicename 
 
SELECT @InstanceLoc=data FROM #Instance WHERE value = @@servicename 
 
--get audit data FROM registry and insert into #AuditData 
 
SET @RegKey = 'SOFTWARE\Microsoft\Microsoft SQL Server\' + @InstanceLoc + '\Setup' 
 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'Edition' 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'SqlCluster' 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'SqlProgramDir' 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'SQLDataRoot' 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'SQLPath' 
 
SET @RegKey = 'SOFTWARE\Microsoft\Microsoft SQL Server\' + @InstanceLoc + '\MSSQLSERVER' 
 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'AuditLevel' 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'LoginMode' 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'DefaultData' 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'DefaultLog' 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'BackupDirectory' 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'NumErrorLogs' 
 
SET @RegKey = 'SOFTWARE\Microsoft\Microsoft SQL Server\' + @InstanceLoc + '\SQLServerAgent' 
 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'RestartSQLServer' 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'RestartServer' 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'UseDatabaseMail' 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'DatabaseMailProfile' 
 
SET @RegKey = 'SOFTWARE\Microsoft\Microsoft SQL Server\' + @InstanceLoc + '\MSSQLSERVER\SuperSocketNetLib\Tcp\IPAll' 
 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'TcpDynamicPorts' 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE', @RegKey, 'TcpPort' 
INSERT INTO #AuditData EXEC xp_regread 'HKEY_LOCAL_MACHINE','SOFTWARE\McAfee\VSCore\On Access Scanner\McShield\Configuration\Default', 'szExcludeExts' 
 
 
UPDATE #AuditData  
   SET value = 'Antivirusprofile' where  value = 'szExcludeExts' 
   
UPDATE #AuditData  
   SET data =   
      CASE  
         WHEN data = 0 THEN 'captures no logins' 
         WHEN data = 1 THEN 'captures only success login attempts' 
         WHEN data = 2 THEN 'captures only failed login attempts' 
         WHEN data = 3 THEN 'captures both success and failed login attempts' 
         ELSE data  
      END 
 WHERE value IN ('AuditLevel') 
 
UPDATE #AuditData  
   SET data =   
      CASE  
         WHEN data = 1 THEN 'Windows Authentication' 
         WHEN data = 2 THEN 'Mixed Mode Authentication' 
         ELSE data  
      END 
 WHERE value IN ('LoginMode') 
 
UPDATE #AuditData  
   SET data =   
      CASE  
         WHEN data = 0 THEN 'FALSE' 
         WHEN data = 1 THEN 'TRUE' 
         ELSE data  
      END 
 WHERE value IN ('RestartServer','RestartSQLServer','SqlCluster','UseDatabaseMail') 
 
SELECT CAST(@@servicename AS VARCHAR(25)) AS instance 
 
SELECT CAST(value AS VARCHAR(25)) AS instance,  
       CAST(data AS VARCHAR(25)) AS location  
  FROM #Instance 
 
SELECT CAST(name AS VARCHAR(25)) AS name,  
       CAST(character_value AS VARCHAR(50)) AS value  
  FROM #msver 
 WHERE name in ('productname','productversion','platform','filedescription') 
 
SELECT CAST(value AS VARCHAR(25)) AS value,  
       CAST(data AS VARCHAR(100)) AS data  
  FROM #AuditData 
 ORDER BY value 
 
SELECT SERVERPROPERTY( 'Collation' ) AS Server_Default_Collation; 
 
SELECT file_id AS fileid, name, physical_name AS filename, size / 128 AS [size-mb], max_size AS [max-8kb-pages], growth FROM sys.master_files WHERE database_id NOT IN ( DB_ID('master'), DB_ID('master'), DB_ID('master'), DB_ID('master') )
  
--------------------- 
exec sp_configure 'show advanced options',1 
reconfigure with override 
 
create table #xp_cmd 
( 
name varchar(50), 
minvalue int, 
maxvalue int, 
config_value int, 
run_value int, 
) 
 
insert into #xp_cmd 
exec sp_configure 
 
select name,config_value from #xp_cmd where name in('xp_cmdshell','SQL Mail XPs','Database Mail XPs') 
 
exec sp_configure 'show advanced options',0 
reconfigure with override 
 
drop table #xp_cmd 
 
------------------ 
 
SELECT CAST(loginname AS VARCHAR(35)) AS loginname,  
       hasaccess,  
       isntname,  
       isntgroup,  
       sysadmin  
  FROM syslogins  
 WHERE name LIKE '%ds_s_amg_sqldba_l%' 
    OR name LIKE '%ds_wimmssqladmin_oa%' 
    OR name LIKE '%ds_wimmssqldba_ap%' 
    OR name LIKE '%ms sql admin%' 
    OR name LIKE '%mssqldba%' 
    OR sysadmin = 1 
    ORDER BY isntname, isntgroup, loginname 
     
SELECT CAST(loginname AS VARCHAR(35)) AS loginname,  
       hasaccess,  
       isntname,  
       isntgroup,  
       sysadmin  
  FROM syslogins  
 WHERE name LIKE '%administrators%' 
 
SELECT CAST(loginname AS VARCHAR(35)) AS loginname,  
       hasaccess,  
       isntname,  
       isntgroup,  
       sysadmin  
  FROM syslogins  
 WHERE name LIKE '%SYSTEM%' 
 
SELECT CAST(name AS VARCHAR(35)) AS name,  
       issqlrole  
  FROM msdb..sysusers  
 WHERE name LIKE 'dtsadminrole' 
    OR name LIKE 'jobadminrole' 
 
SELECT CAST(description AS VARCHAR(50)) AS description,  
       CAST(value AS VARCHAR(50)) AS value 
  FROM sys.configurations 
 WHERE name IN ('awe enabled','max server memory (MB)','min server memory (MB)','priority boost') 
 ORDER BY name 
 
SELECT @CPUCount = internal_value  
  FROM #msver  
 WHERE name = 'processorcount' 
 
SELECT @AffinityMask = CAST(value as int)  
  FROM sys.configurations  
 WHERE name = 'affinity mask' 
 
SET @CPUID = 0 
SET @InstCPUCount = 0 
SET @CPUList = '' 
 
IF @AffinityMask = 0 
      BEGIN 
            SET @InstCPUCount = @CPUCount 
            SET @CPUList = 'No affinity set - all CPUs available to instance' 
      END 
ELSE 
      BEGIN 
            WHILE(@CPUID <= @CPUCount - 1) 
                  BEGIN 
                        IF(@AffinityMask & POWER(2, @CPUID)) > 0 
                              BEGIN 
                                    SET @CPUList = @CPUList + 'CPU' + CAST(@CPUID AS VARCHAR(2)) + ' ' 
                                    SET @InstCPUCount = @InstCPUCount + 1 
                              END 
                        SET @CPUID = @CPUID + 1 
                  END 
      END 
 
SELECT id,  
       CAST(name AS VARCHAR(25)) AS name,  
       enabled,  
       CAST(email_address AS VARCHAR(100)) AS email_address,  
       CAST(pager_address AS VARCHAR(100)) AS pager_address  
  FROM msdb..sysoperators 
 
SELECT CAST(j.name AS VARCHAR(50)) AS name,  
       j.date_created,  
       j.date_modified,  
       j.enabled,  
       j.notify_level_email,  
       CAST(o1.name AS VARCHAR(25)) AS email_operator,  
--       CAST(o1.email_address AS VARCHAR(50)) AS email_address,  
       j.notify_level_page,   
       CAST(o2.name AS VARCHAR(25)) AS pager_operator 
--       CAST(o2.pager_address AS VARCHAR(50)) AS pager_address 
  FROM msdb.dbo.sysjobs_view j 
  LEFT JOIN msdb..sysoperators o1  
    ON j.notify_email_operator_id = o1.id 
  LEFT JOIN msdb..sysoperators o2  
    ON j.notify_page_operator_id = o2.id 
 ORDER BY j.name 
 
SELECT CAST(a.name AS VARCHAR(50)) AS name,  
       CAST(a.event_source AS VARCHAR(20)) AS event_source,  
       a.event_category_id,  
       a.event_id,  
       a.message_id,  
       a.severity,  
       a.enabled,  
       CAST(o1.name AS VARCHAR(25)) AS email_operator,  
--       CAST(o1.email_address AS VARCHAR(100)) AS email_addr,  
       CAST(o2.name AS VARCHAR(25)) AS pager_operator,  
--       CAST(o2.pager_address AS VARCHAR(100)) AS pager_addr, 
       CAST(database_name AS VARCHAR(25)) AS database_name 
  FROM msdb..sysalerts a 
  LEFT JOIN msdb..sysnotifications n1 on a.id = n1.alert_id and n1.notification_method=1 
  LEFT JOIN msdb..sysnotifications n2 on a.id = n2.alert_id and n2.notification_method=2 
  LEFT JOIN msdb..sysoperators o1 on o1.id = n1.operator_id 
  LEFT JOIN msdb..sysoperators o2 on o2.id = n2.operator_id 
 
SELECT CAST(name AS VARCHAR(25)) AS account_name,  
       CAST(description AS VARCHAR(25)) AS description 
  FROM msdb.dbo.sysmail_profile 
 ORDER BY name 
 
SELECT CAST(a.name AS VARCHAR(25)) AS account_name,  
       CAST(a.description AS VARCHAR(25)) AS description, 
       CAST(a.email_address AS VARCHAR(50)) AS email_address, 
       CAST(a.display_name AS VARCHAR(25)) AS display_name, 
       CAST(a.replyto_address AS VARCHAR(25)) AS replyto_address, 
       CAST(s.servertype AS VARCHAR(10)) AS servertype, 
       CAST(s.servername AS VARCHAR(25)) AS servername, 
       s.port, 
       CAST(s.username AS VARCHAR(20)) AS username, 
       s.use_default_credentials, 
       s.enable_ssl 
  FROM msdb.dbo.sysmail_account a 
  JOIN msdb.dbo.sysmail_server s ON a.account_id = s.account_id 
 
--clean up 
DROP TABLE #Instance 
DROP TABLE #AuditData 
DROP TABLE #msver 
DROP TABLE #WinverSP 
 
--================================= Services ================================ 

SELECT 
    DSS.servicename,        
    DSS.startup_type_desc,        
    DSS.status_desc,        
    DSS.last_startup_time,        
    DSS.service_account,        
    DSS.filename,        
    DSS.process_id
FROM    sys.dm_server_services AS DSS;
";

			var details = new List<SqlServerKeyValueProperty>( );
			var databases = new List<SqlServerDatabase>( );
			var instances = new List<SqlServerInstance>( );
			var logins = new List<SqlServerLogin>( );
			var plans = new List<SqlServerMaintenancePlan>( );
			var services = new List<SqlServerService>( );

			try
			{
				var manager = new DatabaseManager( PluginSettings.DatabaseSettings );

				using ( var command = manager.CreateCommand( commandText ) )
				{
					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						if ( reader.Read( ) && reader.FieldCount == 2 )
						{
							details.Add( new SqlServerKeyValueProperty( reader.GetString( 0 ), reader.GetString( 1 ) ) );
						}

						reader.NextResult( );
						reader.NextResult( );

						try
						{
							while ( reader.Read( ) && reader.FieldCount == 2 )
							{
								instances.Add( new SqlServerInstance( reader.GetString( 0 ), reader.GetString( 1 ) ) );
							}
						}
						catch ( Exception exc )
						{
							PluginSettings.EventLog.WriteException( exc );
						}

						reader.NextResult( );
						reader.NextResult( );

						try
						{
							while ( reader.Read( ) && reader.FieldCount == 2 )
							{
								details.Add( new SqlServerKeyValueProperty( reader.GetString( 0 ), reader.GetString( 1, "" ) ) );
							}
						}
						catch ( Exception exc )
						{
							PluginSettings.EventLog.WriteException( exc );
						}

						reader.NextResult( );

						if ( reader.Read( ) && reader.FieldCount == 2 )
						{
							details.Add( new SqlServerKeyValueProperty( "DefaultServerCollation", reader.GetString( 0 ) ) );
						}

						while ( reader.NextResult( ) )
						{
							if ( reader.FieldCount != 6 )
							{
								break;
							}

							try
							{
								while ( reader.Read( ) )
								{
									databases.Add( new SqlServerDatabase( reader.GetInt32( 0 ), reader.GetString( 1 ), reader.GetString( 2 ), reader.GetInt32( 3 ), reader.GetInt32( 4 ), reader.GetInt32( 5 ) ) );
								}
							}
							catch ( Exception exc )
							{
								PluginSettings.EventLog.WriteException( exc );
							}
						}

						try
						{
							while ( reader.Read( ) && reader.FieldCount == 2 )
							{
								details.Add( new SqlServerKeyValueProperty( reader.GetString( 0 ), reader.GetInt32( 1 ).ToString( CultureInfo.InvariantCulture ) ) );
							}
						}
						catch ( Exception exc )
						{
							PluginSettings.EventLog.WriteException( exc );
						}

						while ( reader.NextResult( ) )
						{
							if ( reader.FieldCount != 5 )
							{
								break;
							}

							try
							{
								while ( reader.Read( ) )
								{
									logins.Add( new SqlServerLogin( reader.GetString( 0 ), reader.GetInt32( 1 ), reader.GetInt32( 2 ), reader.GetInt32( 3 ), reader.GetInt32( 4 ) ) );
								}
							}
							catch ( Exception exc )
							{
								PluginSettings.EventLog.WriteException( exc );
							}
						}

						reader.NextResult( );

						try
						{
							while ( reader.Read( ) && reader.FieldCount == 2 )
							{
								details.Add( new SqlServerKeyValueProperty( reader.GetString( 0 ), reader.GetString( 1 ) ) );
							}
						}
						catch ( Exception exc )
						{
							PluginSettings.EventLog.WriteException( exc );
						}

						reader.NextResult( );
						reader.NextResult( );

						try
						{
							while ( reader.Read( ) && reader.FieldCount == 8 )
							{
								plans.Add( new SqlServerMaintenancePlan( reader.GetString( 0 ), reader.GetDateTime( 1 ), reader.GetDateTime( 2 ), reader.GetByte( 3 ), reader.GetInt32( 4 ), reader.GetString( 5, "" ) ) );
							}
						}
						catch ( Exception exc )
						{
							PluginSettings.EventLog.WriteException( exc );
						}

						reader.NextResult( );
						reader.NextResult( );
						reader.NextResult( );
						reader.NextResult( );

						try
						{
							while ( reader.Read( ) && reader.FieldCount == 7 )
							{
								services.Add( new SqlServerService( reader.GetString( 0 ), reader.GetString( 1 ), reader.GetString( 2 ), reader.GetValue( 3 ).ToString( ), reader.GetString( 4 ), reader.GetString( 5 ), reader.GetInt32( 6 ) ) );
							}
						}
						catch ( Exception exc )
						{
							PluginSettings.EventLog.WriteException( exc );
						}
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}

			Details = details;
			Databases = databases;
			Instances = instances;
			Logins = logins;
			Plans = plans;
			Services = services;
		}

		/// <summary>
		///     Refreshes this instance.
		/// </summary>
		private void Refresh( )
		{
			LoadSettings( );
		}
	}
}