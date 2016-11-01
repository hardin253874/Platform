// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.ReadiNow.Security
{
	/// <summary>
	///     Represents a user's application credentials.
	/// </summary>
	public sealed class Credentials
	{
		private string _password = string.Empty;
		private string _tenant = string.Empty;
		private string _username = string.Empty;

		/// <summary>
		///     Initializes a new instance of a Credentials class.
		/// </summary>
		public Credentials( string username, string password, string tenant )
		{
			_username = username;
			_password = password;
			_tenant = tenant;
		}

		/// <summary>
		///     Get the anonymous credentials.
		/// </summary>
		/// <returns>
		///     An object representing the anonymous credentials.
		/// </returns>
		public static Credentials Anonymous
		{
			get
			{
				var credentials = new Credentials( "username", "password", "tenant" );
				return credentials;
			}
		}

		/// <summary>
		///     Gets or sets the password associated with the credentials.
		/// </summary>
		public string Password
		{
			get
			{
				return _password;
			}

			set
			{
				_password = value;
			}
		}

		/// <summary>
		///     Gets or sets the tenant associated with the credentials.
		/// </summary>
		public string Tenant
		{
			get
			{
				return _tenant;
			}

			set
			{
				_tenant = value;
			}
		}

		/// <summary>
		///     Gets or sets the username associated with the credentials.
		/// </summary>
		public string Username
		{
			get
			{
				return _username;
			}

			set
			{
				_username = value;
			}
		}

		/// <summary>
		///     Converts a credential to a fully qualified account name.
		/// </summary>
		/// <param name="credential">
		///     The credential to convert.
		/// </param>
		/// <returns>
		///     A string containing the fully qualified account name associated with the credentials.
		/// </returns>
		public static string GetFullyQualifiedName( Credentials credential )
		{
			string name = credential.Username;

			if ( ( !string.IsNullOrEmpty( credential.Username ) ) && ( !string.IsNullOrEmpty( credential.Tenant ) ) )
			{
				name = string.Format( "{0}\\{1}", credential.Tenant, credential.Username );
			}

			return name;
		}
	}
}