// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Utc;

namespace EDC.ReadiNow.IO
{
	/// <summary>
	///     Represents the core data associated with each application request.
	/// </summary>
	[Serializable]
	[DebuggerStepThrough]
	public class RequestContextData : ILogicalThreadAffinative
	{
		private string _culture = String.Empty;
		private IdentityInfo _identity;
		private IdentityInfo _secondaryIdentity;
		private StringDictionary _parameters = new StringDictionary( );
		private TenantInfo _tenant; // legacy
        private string _timeZone = string.Empty;
        private TimeZoneInfo _timeZoneInfo;

        /// <summary>
        ///     Initializes a new instance of the CultureContextData class.
        /// </summary>
        public RequestContextData( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the CultureContextData class.
		/// </summary>
        public RequestContextData( IdentityInfo identity, TenantInfo tenant, string culture, IdentityInfo secondaryIdentity = null)
		{
			_identity = identity;
			_secondaryIdentity = secondaryIdentity;
            _tenant = tenant;
			_culture = culture;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RequestContextData" /> class.
		/// </summary>
		/// <param name="identity">The identity.</param>
		/// <param name="tenant">The tenant.</param>
		/// <param name="culture">The culture.</param>
		/// <param name="timeZone">The time zone.</param>
		/// <param name="secondaryIdentity">The secondary identity.</param>
		public RequestContextData( IdentityInfo identity, TenantInfo tenant, string culture, string timeZone, IdentityInfo secondaryIdentity = null)
			: this( identity, tenant, culture, secondaryIdentity)
		{
			_timeZone = timeZone;
		}

		/// <summary>
		///     Initializes a new instance of the CultureContextData class based on an existing RequestContext
		/// </summary>
		public RequestContextData( RequestContext context )
			: this( context.Identity, context.Tenant, context.Culture, context.SecondaryIdentity)
		{
		}

		/// <summary>
		///     Gets or sets the culture information associated with the request context.
		/// </summary>
		public String Culture
		{
			get
			{
				return _culture;
			}

			set
			{
				_culture = value;
			}
		}

		/// <summary>
		///     Gets or sets the user identity information associated with the request context.
		/// </summary>
		public IdentityInfo Identity
		{
			get
			{
				return _identity;
			}

			set
			{
				_identity = value;
			}
		}


        /// <summary>
        ///     Gets or sets a secondary user identity - usually the result of workflow triggers.
        /// </summary>
        public IdentityInfo SecondaryIdentity
        {
            get
            {
                return _secondaryIdentity;
            }

            set
            {
                _secondaryIdentity = value;
            }
        }

        /// <summary>
        ///     Gets or sets any optional parameters.
        /// </summary>
        public StringDictionary Parameters
		{
			get
			{
				return _parameters;
			}

			set
			{
				_parameters = value;
			}
		}

		/// <summary>
		///     Gets or sets the tenant information associated with the request context.
		/// </summary>
		public TenantInfo Tenant
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
		///     Gets or sets the time zone.
		/// </summary>
		/// <value>
		///     The time zone.
		/// </value>
		public string TimeZone
		{
			get
			{
				return _timeZone;
			}
			set
			{
				_timeZone = value;
			}
        }

        /// <summary>
        ///     Gets or sets the time zone info.
        /// </summary>
        /// <value>
        ///     The time zone info.
        /// </value>
        public TimeZoneInfo TimeZoneInfo
        {
            get
            {
                if ( _timeZoneInfo == null && !string.IsNullOrEmpty( TimeZone ) )
                {
                    _timeZoneInfo = TimeZoneHelper.GetTimeZoneInfo( TimeZone );
                }
                return _timeZoneInfo;
            }
        }

    }
}