// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;
using ReadiMon.Shared;
using ReadiMon.Shared.Core;
using ReadiMon.Shared.Data;
using ReadiMon.Shared.Messages;

namespace ReadiMon.Plugin.Security
{
	/// <summary>
	///     UserAccount view model.
	/// </summary>
	public class UserAccountsViewModel : ViewModelBase
	{
		/// <summary>
		///     The _accounts
		/// </summary>
		private List<Account> _accounts;

		/// <summary>
		///     The filtered accounts
		/// </summary>
		private List<Account> _filteredAccounts;

		/// <summary>
		///     The plugin settings
		/// </summary>
		private IPluginSettings _pluginSettings;

		/// <summary>
		///     The selected tenant.
		/// </summary>
		private TenantInfo _selectedTenant;

		/// <summary>
		///     The tenant names
		/// </summary>
		private Dictionary<long, string> _tenantNames;

		/// <summary>
		///     The tenants
		/// </summary>
		private List<TenantInfo> _tenants;

		/// <summary>
		///     Initializes a new instance of the <see cref="UserAccountsViewModel" /> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public UserAccountsViewModel( IPluginSettings settings )
		{
			PluginSettings = settings;

			ResetBadLogonCountCommand = new DelegateCommand<Account>( ResetBadLogonCount );
			EnableDisableCommand = new DelegateCommand<Account>( EnableDisable );
			SetPasswordCommand = new DelegateCommand<Account>( SetPassword );
			RefreshCommand = new DelegateCommand( Refresh );
			CopyIdCommand = new DelegateCommand<Account>( CopyIdClick );
		}

		/// <summary>
		///     Gets or sets the copy identifier command.
		/// </summary>
		/// <value>
		///     The copy identifier command.
		/// </value>
		public ICommand CopyIdCommand
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the accounts.
		/// </summary>
		/// <value>
		///     The accounts.
		/// </value>
		private List<Account> Accounts
		{
			get
			{
				return _accounts;
			}
			set
			{
				SetProperty( ref _accounts, value );
			}
		}

		/// <summary>
		///     Gets the enable disable command.
		/// </summary>
		/// <value>
		///     The enable disable command.
		/// </value>
		public ICommand EnableDisableCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the filtered accounts.
		/// </summary>
		/// <value>
		///     The filtered accounts.
		/// </value>
		public List<Account> FilteredAccounts
		{
			get
			{
				return _filteredAccounts;
			}
			set
			{
				SetProperty( ref _filteredAccounts, value );
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

				Refresh( );
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
		///     Gets the reset bad log on count command.
		/// </summary>
		/// <value>
		///     The reset bad log on count command.
		/// </value>
		public ICommand ResetBadLogonCountCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the selected tenant.
		/// </summary>
		/// <value>
		///     The selected tenant.
		/// </value>
		public TenantInfo SelectedTenant
		{
			get
			{
				return _selectedTenant;
			}
			set
			{
				SetProperty( ref _selectedTenant, value );

				if ( _selectedTenant != null )
				{
					FilterAccounts( );
				}
			}
		}

		/// <summary>
		///     Gets the set password command.
		/// </summary>
		/// <value>
		///     The set password command.
		/// </value>
		public ICommand SetPasswordCommand
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the tenant names.
		/// </summary>
		/// <value>
		///     The tenant names.
		/// </value>
		private Dictionary<long, string> TenantNames
		{
			get
			{
				if ( _tenantNames == null )
				{
					var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

					const string commandText = @"--ReadiMon - TenantNames
SELECT Id, name FROM _vTenant";

					try
					{
						using ( IDbCommand command = databaseManager.CreateCommand( commandText ) )
						{
							using ( IDataReader reader = command.ExecuteReader( ) )
							{
								var tenantNames = new Dictionary<long, string>( );

								while ( reader.Read( ) )
								{
									var id = reader.GetInt64( 0 );
									var name = reader.GetString( 1, "<Unnamed>" );

									tenantNames[ id ] = name;
								}

								_tenantNames = tenantNames;
							}
						}
					}
					catch ( Exception exc )
					{
						PluginSettings.EventLog.WriteException( exc );
					}
				}

				return _tenantNames;
			}
		}

		/// <summary>
		///     Gets the tenants.
		/// </summary>
		/// <value>
		///     The tenants.
		/// </value>
		public List<TenantInfo> Tenants
		{
			get
			{
				return _tenants;
			}
			set
			{
				SetProperty( ref _tenants, value );
			}
		}

		/// <summary>
		///     Creates the salted hash.
		/// </summary>
		/// <param name="input">The input.</param>
		/// <param name="salt">The salt.</param>
		/// <param name="iterations">The iterations.</param>
		/// <param name="hashSize">Size of the hash.</param>
		/// <returns></returns>
		private static byte[ ] CreateSaltedHash( string input, byte[ ] salt, int iterations, int hashSize )
		{
			var pbkdf2 = new Rfc2898DeriveBytes( input, salt, iterations );
			return pbkdf2.GetBytes( hashSize );
		}

		/// <summary>
		///     Encodes the salted hash.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="salt">The salt.</param>
		/// <param name="hash">The hash.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentOutOfRangeException"></exception>
		/// <exception cref="System.ArgumentNullException">
		/// </exception>
		/// <exception cref="System.ArgumentException">
		///     @The salt is empty.
		///     or
		///     @The hash is empty.
		/// </exception>
		private static string EncodeSaltedHash( int version, byte[ ] salt, byte[ ] hash )
		{
			if ( version <= 0 )
			{
				throw new ArgumentOutOfRangeException( nameof( version ) );
			}

			if ( salt == null )
			{
				throw new ArgumentNullException( nameof( salt ) );
			}

			if ( salt.Length == 0 )
			{
				throw new ArgumentException( @"The salt is empty.", nameof( salt ) );
			}

			if ( hash == null )
			{
				throw new ArgumentNullException( nameof( hash ) );
			}

			if ( hash.Length == 0 )
			{
				throw new ArgumentException( @"The hash is empty.", nameof( hash ) );
			}

			return $"{version}|{Convert.ToBase64String( salt )}|{Convert.ToBase64String( hash )}";
		}

		/// <summary>
		///     Gets the random bytes.
		/// </summary>
		/// <param name="size">The size.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentOutOfRangeException">size</exception>
		private static byte[ ] GetRandomBytes( int size )
		{
			if ( size <= 0 )
			{
				throw new ArgumentOutOfRangeException( nameof( size ) );
			}

			var bytes = new byte[size];

			var rng = new RNGCryptoServiceProvider( );
			rng.GetBytes( bytes );

			return bytes;
		}

		/// <summary>
		///     Activates the account.
		/// </summary>
		/// <param name="account">The account.</param>
		private void ActivateAccount( Account account )
		{
			var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

			const string commandText = @"--ReadiMon - ActivateAccount
DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), 'User Accounts->Activate Account' )
SET CONTEXT_INFO @contextInfo

DECLARE @active BIGINT = dbo.fnAliasNsId( 'active', 'core', @tenantId )
DECLARE @accountStatus BIGINT = dbo.fnAliasNsId( 'accountStatus', 'core', @tenantId )

UPDATE Relationship SET ToId = @active WHERE TenantId = @tenantId AND TypeId = @accountStatus AND FromId = @entityId";

			try
			{
				using ( SqlCommand command = databaseManager.CreateCommand( commandText ) )
				{
					databaseManager.AddParameter( command, "@entityId", account.Id );
					databaseManager.AddParameter( command, "@tenantId", account.TenantId );

					command.ExecuteNonQuery( );

					account.AccountStatus = "Active";
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}
		}

		/// <summary>
		///     Copies the identifier click.
		/// </summary>
		/// <param name="account">The application.</param>
		private void CopyIdClick( Account account )
		{
			RetryHandler.Retry( ( ) =>
			{
				Clipboard.SetData( DataFormats.Text, account.Id.ToString( ) );

				PluginSettings.Channel.SendMessage( new StatusTextMessage( @"Id copied to clipboard...", 2000 ).ToString( ) );
			}, exceptionHandler: e => PluginSettings.EventLog.WriteException( e ) );
		}

		/// <summary>
		///     Disables the account.
		/// </summary>
		/// <param name="account">The account.</param>
		private void DisableAccount( Account account )
		{
			var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

			const string commandText = @"--ReadiMon - DisableAccount
DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), 'User Accounts->Disable Account' )
SET CONTEXT_INFO @contextInfo

DECLARE @disabled BIGINT = dbo.fnAliasNsId( 'disabled', 'core', @tenantId )
DECLARE @accountStatus BIGINT = dbo.fnAliasNsId( 'accountStatus', 'core', @tenantId )

UPDATE Relationship SET ToId = @disabled WHERE TenantId = @tenantId AND TypeId = @accountStatus AND FromId = @entityId";

			try
			{
				using ( SqlCommand command = databaseManager.CreateCommand( commandText ) )
				{
					databaseManager.AddParameter( command, "@entityId", account.Id );
					databaseManager.AddParameter( command, "@tenantId", account.TenantId );

					command.ExecuteNonQuery( );

					account.AccountStatus = "Disabled";
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}
		}

		/// <summary>
		///     Enables the disable.
		/// </summary>
		/// <param name="account">The account.</param>
		private void EnableDisable( Account account )
		{
			string status = account.AccountStatus.ToLower( ).Trim( );

			switch ( status )
			{
				case "disabled":
				case "expired":
				case "locked":
					ActivateAccount( account );
					break;
				default:
					DisableAccount( account );
					break;
			}
		}

		/// <summary>
		///     Filters the accounts.
		/// </summary>
		private void FilterAccounts( )
		{
			var accounts = new List<Account>( );

			if ( SelectedTenant.Id == -1 )
			{
				accounts = _accounts;
			}
			else
			{
				foreach ( var account in Accounts )
				{
					if ( account.TenantId == SelectedTenant.Id )
					{
						accounts.Add( account );
					}
				}
			}

			FilteredAccounts = accounts;
		}

		/// <summary>
		///     Loads the accounts.
		/// </summary>
		private void LoadAccounts( )
		{
			var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

			const string commandText = @"--ReadiMon - LoadAccounts
SET NOCOUNT ON

DECLARE cur CURSOR LOCAL FAST_FORWARD FOR SELECT Id FROM _vTenant ORDER BY name

OPEN cur

DECLARE @tenantId BIGINT
DECLARE @userAccount BIGINT
DECLARE @isOfType BIGINT
DECLARE @name BIGINT
DECLARE @accountExpiration BIGINT
DECLARE @passwordNeverExpires BIGINT
DECLARE @lastLogon BIGINT
DECLARE @passwordLastChanged BIGINT
DECLARE @lastLockout BIGINT
DECLARE @badLogonCount BIGINT
DECLARE @changePasswordAtNextLogon BIGINT
DECLARE @accountStatus BIGINT

FETCH NEXT FROM cur
INTO @tenantId

WHILE @@FETCH_STATUS = 0
BEGIN
	
	SELECT @userAccount = dbo.fnAliasNsId( 'userAccount', 'core', @tenantId )
	SELECT @isOfType = dbo.fnAliasNsId( 'isOfType', 'core', @tenantId )
	SELECT @name = dbo.fnAliasNsId( 'name', 'core', @tenantId )
	SELECT @accountExpiration = dbo.fnAliasNsId( 'accountExpiration', 'core', @tenantId )
	SELECT @passwordNeverExpires = dbo.fnAliasNsId( 'passwordNeverExpires', 'core', @tenantId )
	SELECT @lastLogon = dbo.fnAliasNsId( 'lastLogon', 'core', @tenantId )
	SELECT @passwordLastChanged = dbo.fnAliasNsId( 'passwordLastChanged', 'core', @tenantId )
	SELECT @lastLockout = dbo.fnAliasNsId( 'lastLockout', 'core', @tenantId )
	SELECT @badLogonCount = dbo.fnAliasNsId( 'badLogonCount', 'core', @tenantId )
	SELECT @changePasswordAtNextLogon = dbo.fnAliasNsId( 'changePasswordAtNextLogon', 'core', @tenantId )
	SELECT @accountStatus = dbo.fnAliasNsId( 'accountStatus', 'core', @tenantId )

	SELECT
		AccountId = r.FromId,
		r.TenantId,
		Name = n.Data,
		AccountExpiration = ex.Data,
		PasswordNeverExpires = pne.Data,
		LastLogon = ll.Data,
		PasswordLastChanged = plc.Data,
		LastLockout = llo.Data,
		BadLogonCount = blc.Data,
		ChangePasswordNextLogon = cpnl.Data,
		[Status] = sn.Data
	FROM
		Relationship r
	LEFT JOIN
		Data_NVarChar n ON
			r.FromId = n.EntityId AND
			r.TenantId = n.TenantId AND
			n.FieldId = @name
	LEFT JOIN
		Data_DateTime ex ON
			r.FromId = ex.EntityId AND
			r.TenantId = ex.TenantId AND
			ex.FieldId = @accountExpiration
	LEFT JOIN
		Data_Bit pne ON
			r.FromId = pne.EntityId AND
			r.TenantId = pne.TenantId AND
			pne.FieldId = @passwordNeverExpires
	LEFT JOIN
		Data_DateTime ll ON
			r.FromId = ll.EntityId AND
			r.TenantId = ll.TenantId AND
			ll.FieldId = @lastLogon
	LEFT JOIN
		Data_DateTime plc ON
			r.FromId = plc.EntityId AND
			r.TenantId = plc.TenantId AND
			plc.FieldId = @passwordLastChanged
	LEFT JOIN
		Data_DateTime llo ON
			r.FromId = llo.EntityId AND
			r.TenantId = llo.TenantId AND
			llo.FieldId = @lastLockout
	LEFT JOIN
		Data_Int blc ON
			r.FromId = blc.EntityId AND
			r.TenantId = blc.TenantId AND
			blc.FieldId = @badLogonCount
	LEFT JOIN
		Data_Bit cpnl ON
			r.FromId = cpnl.EntityId AND
			r.TenantId = cpnl.TenantId AND
			cpnl.FieldId = @changePasswordAtNextLogon
	LEFT JOIN
		Relationship s ON
			r.FromId = s.FromId AND
			r.TenantId = s.TenantId AND
			s.TypeId = @accountStatus
	LEFT JOIN
		Data_NVarChar sn ON
			s.ToId = sn.EntityId AND
			r.TenantId = sn.TenantId AND
			sn.FieldId = @name
	WHERE
		r.TenantId = @tenantId
		AND r.TypeId = @isOfType
		AND r.ToId = @userAccount
	ORDER BY
		n.Data

	FETCH NEXT FROM cur INTO @tenantId
END

CLOSE cur
DEALLOCATE cur";

			try
			{
				using ( IDbCommand command = databaseManager.CreateCommand( commandText ) )
				{
					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						var accounts = new List<Account>( );

						do
						{
							while ( reader.Read( ) )
							{
								var id = reader.GetInt64( 0 );
								var tenantId = reader.GetInt64( 1 );
								var name = reader.GetString( 2, "<Unnamed>" );
								var accountExpiration = reader.GetDateTime( 3, DateTime.MinValue );
								var passwordNeverExpires = reader.GetBoolean( 4, false );
								var lastLogon = reader.GetDateTime( 5, DateTime.MinValue );
								var passwordLastChanged = reader.GetDateTime( 6, DateTime.MinValue );
								var lastLockout = reader.GetDateTime( 7, DateTime.MinValue );
								var badLogonCount = reader.GetInt32( 8, 0 );
								var passwordChangeAtNextLogon = reader.GetBoolean( 9, false );
								var accountStatus = reader.GetString( 10, "Active" );

								var account = new Account( id, tenantId, name, accountExpiration, passwordNeverExpires, lastLogon, passwordLastChanged, lastLockout, badLogonCount, passwordChangeAtNextLogon, accountStatus )
								{
									Tenant = TenantNames[ tenantId ]
								};

								accounts.Add( account );
							}
						}
						while ( reader.NextResult( ) );

						Accounts = accounts;
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}
		}

		/// <summary>
		///     Gets the search strings.
		/// </summary>
		/// <value>
		///     The search strings.
		/// </value>
		private void LoadTenants( )
		{
			_tenantNames = null;

			var tenants = new List<TenantInfo>
			{
				new TenantInfo( -1, "All", "All tenants" ),
				new TenantInfo( 0, "Global", "The global tenant" )
			};

			try
			{
				const string commandText = @"--ReadiMon - LoadTenants
SELECT Id, name, description FROM _vTenant";

				var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

				using ( var command = databaseManager.CreateCommand( commandText ) )
				{
					using ( IDataReader reader = command.ExecuteReader( ) )
					{
						while ( reader.Read( ) )
						{
							long id = reader.GetInt64( 0 );
							string name = reader.GetString( 1, null );
							string description = reader.GetString( 2, null );

							if ( string.IsNullOrEmpty( name ) )
							{
								continue;
							}

							tenants.Add( new TenantInfo( id, name, description ) );
						}
					}
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}

			Tenants = tenants;

			SelectedTenant = _tenants[ 0 ];
		}

		/// <summary>
		///     Refreshes this instance.
		/// </summary>
		private void Refresh( )
		{
			LoadTenants( );

			LoadAccounts( );

			FilterAccounts( );
		}

		/// <summary>
		///     Resets the bad log on count.
		/// </summary>
		/// <param name="account">The account.</param>
		private void ResetBadLogonCount( Account account )
		{
			var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

			const string commandText = @"--ReadiMon - ResetBadLogonCount
DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), 'User Accounts->Reset Bad Logon Count' )
SET CONTEXT_INFO @contextInfo

DECLARE @badLogonCount BIGINT = dbo.fnAliasNsId( 'badLogonCount', 'core', @tenantId )

UPDATE Data_Int SET Data = 0 WHERE EntityId = @entityId AND TenantId = @tenantId AND FieldId = @badLogonCount";

			try
			{
				using ( SqlCommand command = databaseManager.CreateCommand( commandText ) )
				{
					databaseManager.AddParameter( command, "@entityId", account.Id );
					databaseManager.AddParameter( command, "@tenantId", account.TenantId );

					command.ExecuteNonQuery( );

					account.BadLogonCount = 0;
				}
			}
			catch ( Exception exc )
			{
				PluginSettings.EventLog.WriteException( exc );
			}
		}

		/// <summary>
		///     Sets the password.
		/// </summary>
		/// <param name="account">The account.</param>
		private void SetPassword( Account account )
		{
			var newPassword = new NewPassword( );
			newPassword.ShowDialog( );

			var vm = newPassword.DataContext as NewPasswordViewModel;

			if ( vm?.CloseWindow != null && vm.CloseWindow.Value )
			{
				string input = vm.Password1;

				if ( string.IsNullOrEmpty( input ) )
				{
					return;
				}

				HashSettings hashSettings = HashSettings.GetHashSettings( );

				// Create the salt data
				byte[ ] salt = GetRandomBytes( hashSettings.SaltBytesCount );

				// Hash the input using the specified salt and settings.
				byte[ ] hash = CreateSaltedHash( input, salt, hashSettings.IterationsCount, hashSettings.HashBytesCount );

				// Encode the hash as a string
				string saltedHash = EncodeSaltedHash( hashSettings.Version, salt, hash );

				var databaseManager = new DatabaseManager( PluginSettings.DatabaseSettings );

				const string commandText = @"--ReadiMon - SetPassword
DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), 'User Accounts->Set Password' )
SET CONTEXT_INFO @contextInfo

DECLARE @password BIGINT = dbo.fnAliasNsId( 'password', 'core', @tenantId )

UPDATE Data_NVarChar SET Data = @value WHERE EntityId = @entityId AND TenantId = @tenantId AND FieldId = @password";

				try
				{
					using ( SqlCommand command = databaseManager.CreateCommand( commandText ) )
					{
						databaseManager.AddParameter( command, "@entityId", account.Id );
						databaseManager.AddParameter( command, "@tenantId", account.TenantId );
						databaseManager.AddParameter( command, "@value", saltedHash );

						command.ExecuteNonQuery( );
					}
				}
				catch ( Exception exc )
				{
					PluginSettings.EventLog.WriteException( exc );
				}
			}
		}
	}
}