// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Security.Cryptography;
using EDC.ReadiNow.Configuration;
using EDC.ReadiNow.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace EDC.SoftwarePlatform.WebApi.Controllers.Login
{
	/// <summary>
	///     Test authorization is used only integration testing, and provides an alternative to cookie based authorization.
	///     Only one test token is available at a time.
	/// </summary>
	public class TestAuthorization : IDisposable
	{
		private const int TokenByteLength = 128; // the number of random bytes for the token 

		private const string Explaination = "This could indicate a serious security breach. Ignore this error messages during integration tests.";
		private static readonly TimeSpan SingletonTokenTimeToLive = new TimeSpan( 1, 0, 0 ); // token lives for one hour
		public static readonly TestAuthorization Instance;
		private readonly RNGCryptoServiceProvider _rngCsp = new RNGCryptoServiceProvider( );


		private readonly TimeSpan _timeToLive;

		/// <summary>
		///		The cached enabled value
		/// </summary>
		private static bool? _isEnabled;

       
		private readonly SynchronizedCollection<Tuple<long, long, string, string, DateTime>> _idTokenTTL = new SynchronizedCollection<Tuple<long, long, string, string, DateTime>>();
        

		/// <summary>
		///     Initializes the <see cref="TestAuthorization" /> class.
		/// </summary>
		static TestAuthorization( )
		{
			Instance = new TestAuthorization( SingletonTokenTimeToLive );

			if ( IsEnabled )
				EventLog.Application.WriteWarning( "TestAuthorization has been turned on. " + Explaination );

			/////
			// Watch for changes to the configuration settings rather than polling it continually.
			/////
			ConfigurationSettings.Changed += ConfigurationSettings_Changed;
		}

		/// <summary>
		/// Handles the Changed event of the ConfigurationSettings control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		static void ConfigurationSettings_Changed( object sender, EventArgs e )
		{
			/////
			// Clear the cached value.
			/////
			_isEnabled = null;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="TestAuthorization" /> class.
		/// </summary>
		/// <param name="timeToLive">The time to live.</param>
		/// <exception cref="System.ArgumentException">Can not have a TestAuthorization with a negative timespan.</exception>
		public TestAuthorization( TimeSpan timeToLive )
		{
			if ( timeToLive < new TimeSpan( ) )
				throw new ArgumentException( "Can not have a TestAuthorization with a negative timespan." );

			_timeToLive = timeToLive;
		}

		/// <summary>
		///     Is the testAuthorization enabled.
		///     This setting is stored in the Web.Config key "IntegrationTestSupportEnabled"
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		///     Changing this configuration requires an IIS restart.
		/// </remarks>
		public static bool IsEnabled
		{
			get
			{
				if ( !_isEnabled.HasValue )
				{
					ServerConfiguration configSection = ConfigurationSettings.GetServerConfigurationSection( );
					_isEnabled = configSection.Security.IntegratedTestingModeEnabled;	
				}
				
				return _isEnabled.Value;
			}
		}

        /// <summary>
        /// Gets the test token.
        /// </summary>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="idProvider">The identifier provider.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns>System.String.</returns>
        /// <value>
        /// The test token.
        /// </value>
        public string GetTestToken(long tenantId, long idProvider, string userName)
		{
			EventLog.Application.WriteWarning( "TestAuthorization has be used. " + Explaination );

            var tokenVal = GetTokenContainerById(tenantId, idProvider, userName);
            return tokenVal != null ? tokenVal.Item4 : null;
		}

		/// <summary>
		///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		void IDisposable.Dispose( )
		{
			_rngCsp.Dispose( );
		}


        /// <summary>
        /// Update the test token.
        /// If the identifier has changed or the token has expired, create a new token, otherwise refresh the old one.
        /// </summary>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="idProvider">The identifier provider.</param>
        /// <param name="userName">Name of the user.</param>
        public void SetTokenIdentifier( long tenantId, long idProvider, string userName )
		{
         
            bool generateToken = true;

            var tokenVal = GetTokenContainerById(tenantId, idProvider, userName);
            
            if (tokenVal != null)
            {
                generateToken = DateTime.Now > tokenVal.Item5;
                
                if (generateToken)
                {
                    _idTokenTTL.Remove(tokenVal);
                }
            }

            if (generateToken) 
            {
                _idTokenTTL.Add(new Tuple<long, long, string, string, DateTime>(tenantId, idProvider, userName, GenerateToken(), DateTime.Now + _timeToLive));
            }
		}

        /// <summary>
        /// Remove the testToken - used for testing.
        /// </summary>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="idProvider">The identifier provider.</param>
        /// <param name="userName">Name of the user.</param>
        public void ClearToken(long tenantId, long idProvider, string userName)
        {
            _idTokenTTL.Remove(GetTokenContainerById(tenantId, idProvider, userName));
        }


        /// <summary>
        /// Tries the get identifier.
        /// </summary>
        /// <param name="providedToken">The provided token.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="idProvider">The identifier provider.</param>
        /// <param name="userName">Name of the user.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">providedToken</exception>
        /// <exception cref="System.ArgumentException">Null token provided</exception>
        public bool TryGetIdentifier( string providedToken, out long tenantId, out long idProvider, out string userName )
		{
			if ( providedToken == null )
                throw new ArgumentNullException("providedToken");

            var tokenVal = GetTokenContainerByToken(providedToken);
            
            if (tokenVal != null) 
            {
                if (DateTime.Now <= tokenVal.Item5)
                {
                    tenantId = tokenVal.Item1;
                    idProvider = tokenVal.Item2;
                    userName = tokenVal.Item3;
                    return true;
                }
            }

			EventLog.Application.WriteError( "Attempted to to use invalid TestAuthorization token. " + Explaination );
            userName = null;
		    tenantId = -1;
		    idProvider = -1;

            return false;
		}

        Tuple<long, long, string, string, DateTime> GetTokenContainerById(long tenantId, long idProvider, string userName)
        {
			return _idTokenTTL.ToList( ).FirstOrDefault( t => t.Item1 == tenantId && t.Item2 == idProvider && t.Item3 == userName);
        }

        Tuple<long, long, string, string, DateTime> GetTokenContainerByToken(string id)
        {
			return _idTokenTTL.ToList( ).FirstOrDefault( t => t.Item4 == id );
        }

		/// <summary>
		///     Generates the token.
		/// </summary>
		/// <returns></returns>
		private string GenerateToken( )
		{
			var buffer = new byte[TokenByteLength];
			_rngCsp.GetBytes( buffer );

			return Convert.ToBase64String( buffer );
		}
	}
}