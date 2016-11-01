// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using ReadiMon.Shared.Core;

namespace ReadiMon.Plugin.Security
{
	/// <summary>
	///     The Account class.
	/// </summary>
	/// <seealso cref="ReadiMon.Shared.Core.ViewModelBase" />
	public class Account : ViewModelBase
	{
		/// <summary>
		///     The account expiration
		/// </summary>
		private DateTime _accountExpiration;

		/// <summary>
		///     The account status
		/// </summary>
		private string _accountStatus;

		/// <summary>
		///     The bad log on count
		/// </summary>
		private int _badLogonCount;

		/// <summary>
		///     The id
		/// </summary>
		private long _id;

		/// <summary>
		///     The last lockout
		/// </summary>
		private DateTime _lastLockout;

		/// <summary>
		///     The last log on
		/// </summary>
		private DateTime _lastLogon;

		/// <summary>
		///     The name
		/// </summary>
		private string _name;

		/// <summary>
		///     The password change at next log on
		/// </summary>
		private bool _passwordChangeAtNextLogon;

		/// <summary>
		///     The password last changed
		/// </summary>
		private DateTime _passwordLastChanged;

		/// <summary>
		///     The password never expires
		/// </summary>
		private bool _passwordNeverExpires;

		/// <summary>
		///     The tenant
		/// </summary>
		private string _tenant;

		/// <summary>
		///     The tenant identifier
		/// </summary>
		private long _tenantId;

		/// <summary>
		///     Initializes a new instance of the <see cref="Account" /> class.
		/// </summary>
		/// <param name="id">The identifier.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="name">The name.</param>
		/// <param name="accountExpiration">The account expiration.</param>
		/// <param name="passwordNeverExpires">if set to <c>true</c> [password never expires].</param>
		/// <param name="lastLogon">The last log on.</param>
		/// <param name="passwordLastChanged">The password last changed.</param>
		/// <param name="lastLockout">The last lockout.</param>
		/// <param name="badLogonCount">The bad log on count.</param>
		/// <param name="passwordChangeAtNextLogon">if set to <c>true</c> the password needs to be changed at next log on.</param>
		/// <param name="accountStatus">The account status.</param>
		public Account( long id, long tenantId, string name, DateTime accountExpiration, bool passwordNeverExpires, DateTime lastLogon, DateTime passwordLastChanged, DateTime lastLockout, int badLogonCount, bool passwordChangeAtNextLogon, string accountStatus )
		{
			Id = id;
			TenantId = tenantId;
			Name = name;
			AccountExpiration = accountExpiration;
			PasswordNeverExpires = passwordNeverExpires;
			LastLogon = lastLogon;
			PasswordLastChanged = passwordLastChanged;
			LastLockout = lastLockout;
			BadLogonCount = badLogonCount;
			PasswordChangeAtNextLogon = passwordChangeAtNextLogon;
			AccountStatus = accountStatus;
		}

		/// <summary>
		///     Gets the account expiration.
		/// </summary>
		/// <value>
		///     The account expiration.
		/// </value>
		private DateTime AccountExpiration
		{
			get
			{
				return _accountExpiration;
			}
			set
			{
				SetProperty( ref _accountExpiration, value );
			}
		}

		/// <summary>
		///     Gets the account status.
		/// </summary>
		/// <value>
		///     The account status.
		/// </value>
		public string AccountStatus
		{
			get
			{
				return _accountStatus;
			}
			set
			{
				SetProperty( ref _accountStatus, value );
			}
		}

		/// <summary>
		///     Gets the bad log on count.
		/// </summary>
		/// <value>
		///     The bad log on count.
		/// </value>
		public int BadLogonCount
		{
			get
			{
				return _badLogonCount;
			}
			set
			{
				SetProperty( ref _badLogonCount, value );
			}
		}

		/// <summary>
		///     Gets the enable disable.
		/// </summary>
		/// <value>
		///     The enable disable.
		/// </value>
		public string EnableDisable
		{
			get
			{
				string status = AccountStatus.ToLower( ).Trim( );

				if ( status == "disabled" )
				{
					return "Enable";
				}

				if ( status == "expired" )
				{
					return "Reset";
				}

				if ( status == "locked" )
				{
					return "Unlock";
				}

				return "Disable";
			}
		}

		/// <summary>
		///     Gets the identifier.
		/// </summary>
		/// <value>
		///     The identifier.
		/// </value>
		public long Id
		{
			get
			{
				return _id;
			}
			set
			{
				SetProperty( ref _id, value );
			}
		}

		/// <summary>
		///     Gets the last lockout.
		/// </summary>
		/// <value>
		///     The last lockout.
		/// </value>
		private DateTime LastLockout
		{
			get
			{
				return _lastLockout;
			}
			set
			{
				SetProperty( ref _lastLockout, value );
			}
		}

		/// <summary>
		///     Gets the last log on.
		/// </summary>
		/// <value>
		///     The last log on.
		/// </value>
		public DateTime LastLogon
		{
			get
			{
				return _lastLogon;
			}
			set
			{
				SetProperty( ref _lastLogon, value );
			}
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				SetProperty( ref _name, value );
			}
		}

		/// <summary>
		///     Gets a value indicating whether the password needs to be changed at next log on.
		/// </summary>
		/// <value>
		///     <c>true</c> if the password needs to be changed at next log on; otherwise, <c>false</c>.
		/// </value>
		private bool PasswordChangeAtNextLogon
		{
			get
			{
				return _passwordChangeAtNextLogon;
			}
			set
			{
				SetProperty( ref _passwordChangeAtNextLogon, value );
			}
		}

		/// <summary>
		///     Gets the password last changed.
		/// </summary>
		/// <value>
		///     The password last changed.
		/// </value>
		private DateTime PasswordLastChanged
		{
			get
			{
				return _passwordLastChanged;
			}
			set
			{
				SetProperty( ref _passwordLastChanged, value );
			}
		}

		/// <summary>
		///     Gets a value indicating whether [password never expires].
		/// </summary>
		/// <value>
		///     <c>true</c> if [password never expires]; otherwise, <c>false</c>.
		/// </value>
		private bool PasswordNeverExpires
		{
			get
			{
				return _passwordNeverExpires;
			}
			set
			{
				SetProperty( ref _passwordNeverExpires, value );
			}
		}

		/// <summary>
		///     Gets or sets the tenant.
		/// </summary>
		/// <value>
		///     The tenant.
		/// </value>
		public string Tenant
		{
			get
			{
				return _tenant;
			}
			set
			{
				SetProperty( ref _tenant, value );
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
			get
			{
				return _tenantId;
			}
			private set
			{
				SetProperty( ref _tenantId, value );
			}
		}

		/// <summary>
		///     Gets the tool tip.
		/// </summary>
		/// <value>
		///     The tool tip.
		/// </value>
		public string Tooltip => $@"Account Expiration: {FormatDate( AccountExpiration, "Never" )}
Password Never Expires: {PasswordNeverExpires}
Password Last Changed: {FormatDate( PasswordLastChanged, "Never" )}
Last Lockout: {FormatDate( LastLockout, "Never" )}
Password Change At Next Logon: {PasswordChangeAtNextLogon}";

		/// <summary>
		///     Formats the date.
		/// </summary>
		/// <param name="dateTime">The date time.</param>
		/// <param name="alternative">The alternative.</param>
		/// <returns></returns>
		private string FormatDate( DateTime dateTime, string alternative )
		{
			if ( dateTime == DateTime.MinValue )
			{
				return alternative;
			}

			return dateTime.ToLongDateString( );
		}
	}
}