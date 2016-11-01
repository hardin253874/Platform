// Copyright 2011-2015 Global Software Innovation Pty Ltd

using System.AddIn.Pipeline;
using ReadiMon.Contract;
using ReadiMon.Shared;

namespace ReadiMon.AddinSideAdapter
{
	/// <summary>
	///     Database Settings contract to view addin adapter class.
	/// </summary>
	[AddInAdapter]
	public class DatabaseSettingsContractToViewAddinAdapter : ContractBase, IDatabaseSettings
	{
		/// <summary>
		///     The plugin view.
		/// </summary>
		private readonly IDatabaseSettingsContract _view;

		/// <summary>
		///     Initializes a new instance of the <see cref="DatabaseSettingsContractToViewAddinAdapter" /> class.
		/// </summary>
		/// <param name="view">The view.</param>
		public DatabaseSettingsContractToViewAddinAdapter( IDatabaseSettingsContract view )
		{
			_view = view;
		}

		/// <summary>
		///     Gets the name of the catalog.
		/// </summary>
		/// <value>
		///     The name of the catalog.
		/// </value>
		public string CatalogName
		{
			get
			{
				return _view.CatalogName;
			}
		}

		/// <summary>
		///     Gets the password.
		/// </summary>
		/// <value>
		///     The password.
		/// </value>
		public string Password
		{
			get
			{
				return _view.Password;
			}
		}

		/// <summary>
		///     Gets the name of the server.
		/// </summary>
		/// <value>
		///     The name of the server.
		/// </value>
		public string ServerName
		{
			get
			{
				return _view.ServerName;
			}
		}

		/// <summary>
		///     Gets a value indicating whether [use integrated security].
		/// </summary>
		/// <value>
		///     <c>true</c> if [use integrated security]; otherwise, <c>false</c>.
		/// </value>
		public bool UseIntegratedSecurity
		{
			get
			{
				return _view.UseIntegratedSecurity;
			}
		}

		/// <summary>
		///     Gets the username.
		/// </summary>
		/// <value>
		///     The username.
		/// </value>
		public string Username
		{
			get
			{
				return _view.Username;
			}
		}
	}
}