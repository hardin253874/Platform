// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.ReadiNow.Configuration;
namespace EDC.SoftwarePlatform.WebApi.Controllers.Login
{
    /// <summary>
    ///     Login constants.
    /// </summary>
    public static class LoginConstants
    {
        /// <summary>
        /// The readi now identity provider alias
        /// </summary>
        public const string ReadiNowIdentityProviderAlias = "core:readiNowIdentityProviderInstance";
        /// <summary>
        ///     The provider name suffix. 'OpenId Provider'
        /// </summary>
        public const string ProviderNameSuffix = "OpenId Provider";

        /// <summary>
        ///     The unknown provider name. 'Unknown'
        /// </summary>
        public const string UnknownProvider = "Unknown";                  

        /// <summary>
        /// The readinow identity provider type alias name;
        /// </summary>
        public const string ReadiNowIdentityProviderTypeAlias = "core:readiNowIdentityProvider";

        /// <summary>
        ///     The default tenant name. 'EDC'
        /// </summary>
        /// <remarks>
        ///     This may be removed at a later date.
        /// </remarks>
        public const string DefaultTenantName = "EDC";

        /// <summary>
        ///     Callback string values.
        /// </summary>
        public static class CallbackArguments
        {
            /// <summary>
            ///     The provider string. 'provider'
            /// </summary>
            public const string Provider = "provider";

            /// <summary>
            ///     The return URL string. 'returnUrl'
            /// </summary>
            public const string ReturnUrl = "returnUrl";

            /// <summary>
            ///     The callback string. 'callback'
            /// </summary>
            public const string Callback = "callback";

            /// <summary>
            ///     The json callback string. 'JSON_CALLBACK'
            /// </summary>
            public const string JsonCallback = "JSON_CALLBACK";
        }

        /// <summary>
        ///     Cookie constants.
        /// </summary>
        public static class Cookie
        {
            static double _timeout = -1;

            /// <summary>
            ///     The open id status string. 'openIdStatus'
            /// </summary>
            public const string OpenIdStatus = "openIdStatus";

            /// <summary>
            ///     Status string. 'status'
            /// </summary>
            public const string Status = "status";

            /// <summary>
            ///     The cookie name string. 'SoftwarePlatformAuthentication'
            /// </summary>
            public static readonly string CookieName = "SoftwarePlatformAuthentication";

            /// <summary>
            ///     Default cookie/header XSRF token name in AngularJS.
            ///     According to AngularJS doco [https://docs.angularjs.org/api/ng/service/$http] default cookie name
            ///     for the XSRF token is XSRF-TOKEN and default HTTP Header is X-XSRF-TOKEN.
            /// </summary>
            public static readonly string AngularDefaultXsrfCookieName = "XSRF-TOKEN";

            /// <summary>
            ///     Cookie timeout. This is used to lock the console.
            /// </summary>
            public static double Timeout
            {
                get
                {
                    if (_timeout == -1)
                    {
                        _timeout = ConfigurationSettings.GetServerConfigurationSection().Security.ConsoleLockTimeoutMinutes;
                    }

                    return _timeout;
                }
            }

            /// <summary>
            ///     The provider string. 'Provider'
            /// </summary>
            public static readonly string Provider = "Provider";

            /// <summary>
            ///     The persist key. 'Persist'
            /// </summary>
            public static readonly string Persist = "Persist";

            /// <summary>
            ///     The XsrfToken token; same value as the XSRF-TOKEN cookie value duplicated inside ASPXAUTH as additional value for increased protection.
            /// </summary>
            public static readonly string XsrfToken = "XsrfToken";
        }

        /// <summary>
        ///     Request/Response header values.
        /// </summary>
        public static class Headers
        {
            /// <summary>
            ///     The location string. 'Location'
            /// </summary>
            public const string Location = "Location";

            /// <summary>
            ///     The default AngularJS XSRF HTTP Header when submitting $http requests; refer to "Cross Site Request Forgery (XSRF) Protection" section under https://docs.angularjs.org/api/ng/service/$http
            /// </summary>
            public const string AngularJsXsrfToken = "X-XSRF-TOKEN";
        }

        /// <summary>
        ///     Query string values.
        /// </summary>
        public static class QueryString
        {
            /// <summary>
            ///     The open id identifier string. 'openid_identifier'
            /// </summary>
            public const string OpenIdIdentifier = "openid_identifier";

            /// <summary>
            ///     The provider string. 'provider'
            /// </summary>
            public const string Provider = "provider";

            /// <summary>
            ///     The name of the URL parameter than represents the return URL post sign in
            /// </summary>
            public const string ReturnUrl = "returnUrl";

            /// <summary>
            ///     The XSRF-TOKEN value added to non XHR requests via query string
            /// </summary>
            public const string XsrfToken = "X-XSRF-TOKEN";
        }
    }
}