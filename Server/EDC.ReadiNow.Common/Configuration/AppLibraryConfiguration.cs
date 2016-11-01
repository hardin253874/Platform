// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Configuration;

namespace EDC.ReadiNow.Configuration
{
    /// <summary>
    ///     Defines the workflow configuration section
    /// </summary>
    public class AppLibraryConfiguration : ConfigurationSection
    {
        /// <summary>
        ///     Gets or sets a value indicating what type of application library security is used.
        /// </summary>
        [ConfigurationProperty( "appSecurity" )]
        public ApplicationSecuritySettings AppSecurity
        {
            get
            {
                return ( ApplicationSecuritySettings ) this [ "appSecurity" ];
            }

            set
            {
                this [ "appSecurity" ] = value;
            }
        }
    }

    /// <summary>
    ///     Application security settings
    /// </summary>
    public class ApplicationSecuritySettings : ConfigurationElement
    {
        /// <summary>
        /// Gets the current configuration settings.
        /// </summary>
        public static ApplicationSecuritySettings Current
        {
            get
            {
                ApplicationSecuritySettings settings = null;

                var section = ConfigurationSettings.GetAppLibraryConfigurationSection( );
                if ( section != null )
                {
                    settings = section.AppSecurity;
                }

                return settings;
            }
        }

        /// <summary>
        /// The maximum depth of triggers that is supportedwithin a workflow. If this depth is exceeded the triggered workflow will fail.
        /// </summary>
        /// <remarks>Full, Restricted, PerTenant.</remarks>
        [ConfigurationProperty( "accessModel", DefaultValue = "Full", IsRequired = false )]
        public AppSecurityModel AppSecurityModel
        {
            get
            {
                return ( AppSecurityModel ) Enum.Parse( typeof( AppSecurityModel ), this [ "accessModel" ].ToString( ) );
            }

            set
            {
                this [ "accessModel" ] = value.ToString( );
            }
        }
    }

    public enum AppSecurityModel
    {
        /// <summary>
        /// No security on who can publish or install applications.
        /// </summary>
        Full,

        /// <summary>
        /// Lock down all tenants so they can only see applications they have installed.
        /// </summary>
        Restricted,

        /// <summary>
        /// Access can be configured per-application per-tenant using data stored in the global tenant.
        /// </summary>
        PerTenant
    }

}