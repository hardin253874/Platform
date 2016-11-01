// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.SoftwarePlatform.Install.Common
{
	/// <summary>
	///     IFronEndArgumentParser interface.
	/// </summary>
	public interface IFrontEndArgumentParser
	{
		/// <summary>
		///     Gets the application library file repo directory.
		/// </summary>
		/// <value>
		///     The application library file repo directory.
		/// </value>
		string AppLibFileRepoDirectory
		{
			get;
		}

		/// <summary>
		///     Gets the application pool domain.
		/// </summary>
		/// <value>
		///     The application pool domain.
		/// </value>
		string AppPoolDomain
		{
			get;
		}

		/// <summary>
		///     Gets the application pool pass.
		/// </summary>
		/// <value>
		///     The application pool pass.
		/// </value>
		string AppPoolPassword
		{
			get;
		}

		/// <summary>
		///     Gets the application pool user.
		/// </summary>
		/// <value>
		///     The application pool user.
		/// </value>
		string AppPoolUser
		{
			get;
		}

		/// <summary>
		///     Gets the bin file repo directory.
		/// </summary>
		/// <value>
		///     The bin file repo directory.
		/// </value>
		string BinFileRepoDirectory
		{
			get;
		}

		/// <summary>
		///     Gets the configuration symbolic link path.
		/// </summary>
		/// <value>
		///     The configuration symbolic link path.
		/// </value>
		string ConfigSymbolicLinkPath
		{
			get;
		}

		/// <summary>
		///     Gets the dacpac path.
		/// </summary>
		/// <value>
		///     The dacpac path.
		/// </value>
		string DacpacPath
		{
			get;
		}

		/// <summary>
		///     Gets the database catalog.
		/// </summary>
		/// <value>
		///     The database catalog.
		/// </value>
		string DatabaseCatalog
		{
			get;
		}

		/// <summary>
		///     Gets the database role.
		/// </summary>
		/// <value>
		///     The database role.
		/// </value>
		string DatabaseRole
		{
			get;
		}

		/// <summary>
		///     Gets the database server.
		/// </summary>
		/// <value>
		///     The database server.
		/// </value>
		string DatabaseServer
		{
			get;
		}

		/// <summary>
		///     Gets the data directory.
		/// </summary>
		/// <value>
		///     The data directory.
		/// </value>
		string DataDirectory
		{
			get;
		}

		/// <summary>
		///     Gets the default tenant.
		/// </summary>
		/// <value>
		///     The default tenant.
		/// </value>
		string DefaultTenant
		{
			get;
		}

		/// <summary>
		///     Gets the document file repo directory.
		/// </summary>
		/// <value>
		///     The document file repo directory.
		/// </value>
		string DocFileRepoDirectory
		{
			get;
		}

		/// <summary>
		///     Gets the file repo directory.
		/// </summary>
		/// <value>
		///     The file repo directory.
		/// </value>
		string FileRepoDirectory
		{
			get;
		}

		/// <summary>
		///     Gets the log path.
		/// </summary>
		/// <value>
		///     The log path.
		/// </value>
		string LogPath
		{
			get;
		}


		/// <summary>
		///     Gets the path.
		/// </summary>
		/// <value>
		///     The path.
		/// </value>
		string Path
		{
			get;
		}

		/// <summary>
		///     Gets the redis domain.
		/// </summary>
		/// <value>
		///     The redis domain.
		/// </value>
		string RedisDomain
		{
			get;
		}

		/// <summary>
		///     Gets the redis password.
		/// </summary>
		/// <value>
		///     The redis password.
		/// </value>
		string RedisPassword
		{
			get;
		}

		/// <summary>
		///     Gets the redis port.
		/// </summary>
		/// <value>
		///     The redis port.
		/// </value>
		string RedisPort
		{
			get;
		}

		/// <summary>
		///     Gets the redis server.
		/// </summary>
		/// <value>
		///     The redis server.
		/// </value>
		string RedisServer
		{
			get;
		}

		/// <summary>
		///     Gets the display name of the redis service.
		/// </summary>
		/// <value>
		///     The display name of the redis service.
		/// </value>
		string RedisServiceDisplayName
		{
			get;
		}

		/// <summary>
		///     Gets the name of the redis service.
		/// </summary>
		/// <value>
		///     The name of the redis service.
		/// </value>
		string RedisServiceName
		{
			get;
		}

		/// <summary>
		///     Gets the redis user.
		/// </summary>
		/// <value>
		///     The redis user.
		/// </value>
		string RedisUser
		{
			get;
		}

		/// <summary>
		///     Gets the display name of the scheduler service.
		/// </summary>
		/// <value>
		///     The display name of the scheduler service.
		/// </value>
		string SchedulerServiceDisplayName
		{
			get;
		}

		/// <summary>
		///     Gets the name of the scheduler service.
		/// </summary>
		/// <value>
		///     The name of the scheduler service.
		/// </value>
		string SchedulerServiceName
		{
			get;
		}

		/// <summary>
		///     Gets the security domain.
		/// </summary>
		/// <value>
		///     The security domain.
		/// </value>
		string SecurityDomain
		{
			get;
		}

		/// <summary>
		///     Gets the security group.
		/// </summary>
		/// <value>
		///     The security group.
		/// </value>
		string SecurityGroup
		{
			get;
		}

		/// <summary>
		///     Gets the site address.
		/// </summary>
		/// <value>
		///     The site address.
		/// </value>
		string SiteAddress
		{
			get;
		}

		/// <summary>
		///     Gets the name of the site.
		/// </summary>
		/// <value>
		///     The name of the site.
		/// </value>
		string SiteName
		{
			get;
		}

		/// <summary>
		///     Gets the site root.
		/// </summary>
		/// <value>
		///     The site root.
		/// </value>
		string SiteRoot
		{
			get;
		}

		/// <summary>
		///     Gets the sp API physical path.
		/// </summary>
		/// <value>
		///     The sp API physical path.
		/// </value>
		string SpApiPhysicalPath
		{
			get;
		}

		/// <summary>
		///     Gets the sp API virtual path.
		/// </summary>
		/// <value>
		///     The sp API virtual path.
		/// </value>
		string SpApiVirtualPath
		{
			get;
		}

		/// <summary>
		///     Gets the sp physical path.
		/// </summary>
		/// <value>
		///     The sp physical path.
		/// </value>
		string SpPhysicalPath
		{
			get;
		}

		/// <summary>
		///     Gets the sp virtual path.
		/// </summary>
		/// <value>
		///     The sp virtual path.
		/// </value>
		string SpVirtualPath
		{
			get;
		}

		/// <summary>
		///     Gets the temporary file repo directory.
		/// </summary>
		/// <value>
		///     The temporary file repo directory.
		/// </value>
		string TempFileRepoDirectory
		{
			get;
		}

		/// <summary>
		///     Gets the upload directory.
		/// </summary>
		/// <value>
		///     The upload directory.
		/// </value>
		string UploadDirectory
		{
			get;
		}

		/// <summary>
		///     Gets the virtual directory path.
		/// </summary>
		/// <value>
		///     The virtual directory path.
		/// </value>
		string VirtualDirectoryPath
		{
			get;
		}
	}
}