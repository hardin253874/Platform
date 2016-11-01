// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EDC.SoftwarePlatform.Install.Common;

namespace PlatformConfigure
{
	/// <summary>
	///     The front end argument parser class.
	/// </summary>
	internal class FrontEndArgumentParser : IFrontEndArgumentParser
	{
		/// <summary>
		///     The attribute parser
		/// </summary>
		private readonly AttributeArgParser _attributeParser;

		/// <summary>
		///     The argument cache
		/// </summary>
		private readonly Dictionary<string, string> _cache = new Dictionary<string, string>( );

		/// <summary>
		///     The command line parser
		/// </summary>
		private readonly CommandLineParser _commandLineParser;

		/// <summary>
		///     Initializes a new instance of the <see cref="FrontEndArgumentParser" /> class.
		/// </summary>
		/// <param name="commandLineParser">The command line parser.</param>
		/// <param name="attributeParser">The attribute parser.</param>
		/// <exception cref="ArgumentNullException">
		/// </exception>
		public FrontEndArgumentParser( CommandLineParser commandLineParser, AttributeArgParser attributeParser )
		{
			if ( commandLineParser == null )
			{
				throw new ArgumentNullException( nameof( commandLineParser ) );
			}

			if ( attributeParser == null )
			{
				throw new ArgumentNullException( nameof( attributeParser ) );
			}

			_commandLineParser = commandLineParser;
			_attributeParser = attributeParser;
		}

		/// <summary>
		///     Gets the configuration symbolic link path.
		/// </summary>
		/// <value>
		///     The configuration symbolic link path.
		/// </value>
		public string ConfigSymbolicLinkPath => GetCacheValue( );

		/// <summary>
		///     Gets the application library file repo directory.
		/// </summary>
		/// <value>
		///     The application library file repo directory.
		/// </value>
		public string AppLibFileRepoDirectory => GetCacheValue( );

		/// <summary>
		///     Gets the application pool domain.
		/// </summary>
		/// <value>
		///     The application pool domain.
		/// </value>
		public string AppPoolDomain => GetCacheValue( );

		/// <summary>
		///     Gets the application pool pass.
		/// </summary>
		/// <value>
		///     The application pool pass.
		/// </value>
		public string AppPoolPassword => GetCacheValue( );

		/// <summary>
		///     Gets the application pool user.
		/// </summary>
		/// <value>
		///     The application pool user.
		/// </value>
		public string AppPoolUser => GetCacheValue( );

		/// <summary>
		///     Gets the bin file repo directory.
		/// </summary>
		/// <value>
		///     The bin file repo directory.
		/// </value>
		public string BinFileRepoDirectory => GetCacheValue( );

		/// <summary>
		///     Gets the dacpac path.
		/// </summary>
		/// <value>
		///     The dacpac path.
		/// </value>
		public string DacpacPath => GetCacheValue( );

		/// <summary>
		///     Gets the database catalog.
		/// </summary>
		/// <value>
		///     The database catalog.
		/// </value>
		public string DatabaseCatalog => GetCacheValue( );

		/// <summary>
		///     Gets the database role.
		/// </summary>
		/// <value>
		///     The database role.
		/// </value>
		public string DatabaseRole => GetCacheValue( );

		/// <summary>
		///     Gets the database server.
		/// </summary>
		/// <value>
		///     The database server.
		/// </value>
		public string DatabaseServer => GetCacheValue( );

		/// <summary>
		///     Gets the data directory.
		/// </summary>
		/// <value>
		///     The data directory.
		/// </value>
		public string DataDirectory => GetCacheValue( );

		/// <summary>
		///     Gets the default tenant.
		/// </summary>
		/// <value>
		///     The default tenant.
		/// </value>
		public string DefaultTenant => GetCacheValue( );

		/// <summary>
		///     Gets the document file repo directory.
		/// </summary>
		/// <value>
		///     The document file repo directory.
		/// </value>
		public string DocFileRepoDirectory => GetCacheValue( );

		/// <summary>
		///     Gets the file repo directory.
		/// </summary>
		/// <value>
		///     The file repo directory.
		/// </value>
		public string FileRepoDirectory => GetCacheValue( );

		/// <summary>
		///     Gets the log path.
		/// </summary>
		/// <value>
		///     The log path.
		/// </value>
		public string LogPath => GetCacheValue( );


		/// <summary>
		///     Gets the path.
		/// </summary>
		/// <value>
		///     The path.
		/// </value>
		public string Path => GetCacheValue( );

		/// <summary>
		///     Gets the redis domain.
		/// </summary>
		/// <value>
		///     The redis domain.
		/// </value>
		public string RedisDomain => GetCacheValue( );

		/// <summary>
		///     Gets the redis password.
		/// </summary>
		/// <value>
		///     The redis password.
		/// </value>
		public string RedisPassword => GetCacheValue( );

		/// <summary>
		///     Gets the redis port.
		/// </summary>
		/// <value>
		///     The redis port.
		/// </value>
		public string RedisPort => GetCacheValue( );

		/// <summary>
		///     Gets the redis server.
		/// </summary>
		/// <value>
		///     The redis server.
		/// </value>
		public string RedisServer => GetCacheValue( );

		/// <summary>
		///     Gets the display name of the redis service.
		/// </summary>
		/// <value>
		///     The display name of the redis service.
		/// </value>
		public string RedisServiceDisplayName => GetCacheValue( );

		/// <summary>
		///     Gets the name of the redis service.
		/// </summary>
		/// <value>
		///     The name of the redis service.
		/// </value>
		public string RedisServiceName => GetCacheValue( );

		/// <summary>
		///     Gets the redis user.
		/// </summary>
		/// <value>
		///     The redis user.
		/// </value>
		public string RedisUser => GetCacheValue( );

		/// <summary>
		///     Gets the display name of the scheduler service.
		/// </summary>
		/// <value>
		///     The display name of the scheduler service.
		/// </value>
		public string SchedulerServiceDisplayName => GetCacheValue( );

		/// <summary>
		///     Gets the name of the scheduler service.
		/// </summary>
		/// <value>
		///     The name of the scheduler service.
		/// </value>
		public string SchedulerServiceName => GetCacheValue( );

		/// <summary>
		///     Gets the security domain.
		/// </summary>
		/// <value>
		///     The security domain.
		/// </value>
		public string SecurityDomain => GetCacheValue( );

		/// <summary>
		///     Gets the security group.
		/// </summary>
		/// <value>
		///     The security group.
		/// </value>
		public string SecurityGroup => GetCacheValue( );

		/// <summary>
		///     Gets the site address.
		/// </summary>
		/// <value>
		///     The site address.
		/// </value>
		public string SiteAddress => GetCacheValue( );

		/// <summary>
		///     Gets the name of the site.
		/// </summary>
		/// <value>
		///     The name of the site.
		/// </value>
		public string SiteName => GetCacheValue( );

		/// <summary>
		///     Gets the site root.
		/// </summary>
		/// <value>
		///     The site root.
		/// </value>
		public string SiteRoot => GetCacheValue( );

		/// <summary>
		///     Gets the sp API physical path.
		/// </summary>
		/// <value>
		///     The sp API physical path.
		/// </value>
		public string SpApiPhysicalPath => GetCacheValue( );

		/// <summary>
		///     Gets the sp API virtual path.
		/// </summary>
		/// <value>
		///     The sp API virtual path.
		/// </value>
		public string SpApiVirtualPath => GetCacheValue( );

		/// <summary>
		///     Gets the sp physical path.
		/// </summary>
		/// <value>
		///     The sp physical path.
		/// </value>
		public string SpPhysicalPath => GetCacheValue( );

		/// <summary>
		///     Gets the sp virtual path.
		/// </summary>
		/// <value>
		///     The sp virtual path.
		/// </value>
		public string SpVirtualPath => GetCacheValue( );

		/// <summary>
		///     Gets the temporary file repo directory.
		/// </summary>
		/// <value>
		///     The temporary file repo directory.
		/// </value>
		public string TempFileRepoDirectory => GetCacheValue( );

		/// <summary>
		///     Gets the upload directory.
		/// </summary>
		/// <value>
		///     The upload directory.
		/// </value>
		public string UploadDirectory => GetCacheValue( );

		/// <summary>
		///     Gets the virtual directory path.
		/// </summary>
		/// <value>
		///     The virtual directory path.
		/// </value>
		public string VirtualDirectoryPath => GetCacheValue( );

		/// <summary>
		///     Gets the argument.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="argumentName">Name of the argument.</param>
		/// <returns></returns>
		private T GetArgument<T>( string argumentName )
		{
			return _attributeParser.GetArgument<T>( _commandLineParser, argumentName );
		}

		/// <summary>
		///     Gets the cache value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		private string GetCacheValue( [CallerMemberName] string key = null )
		{
			if ( key == null )
			{
				throw new ArgumentNullException( nameof( key ) );
			}

			string value;

			if ( !_cache.TryGetValue( key, out value ) )
			{
				value = GetArgument<string>( key );

				_cache[ key ] = value;
			}

			return value;
		}
	}
}