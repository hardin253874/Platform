// Copyright 2011-2016 Global Software Innovation Pty Ltd

using Autofac;
using EDC.ReadiNow.Configuration;

namespace EDC.ReadiNow.IO.RemoteFileFetcher
{
	/// <summary>
	///     Autofac dependency injection module for the file repository module.
	/// </summary>
	public class RemoteFileFetcherModule : Module
	{
		/// <summary>
		///     Perform any registrations
		/// </summary>
		/// <param name="builder">The autofac container builder.</param>
		protected override void Load( ContainerBuilder builder )
		{
			// Register file repositories            
			builder.Register( c => 
                new FtpFileFetcher( 
                    ConfigurationSettings.GetServerConfigurationSection( ).Security.FtpBypassSslCertificateCheck,
                    new SftpFileFetcher(),
                    new FtpsFileFetcher()
                    ) 
                )
			    .As<IRemoteFileFetcher>( )
			    .SingleInstance( );
		}
	}
}