// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.Services.ApplicationManager
{
	/// <summary>
	///     Holds information about an available application.
	/// </summary>
	[DataContract]
	[KnownType( typeof ( InstalledApplication ) )]
	public class AvailableApplication
	{
		/// <summary>
		///     The identifier of the entity representing the application.
		/// </summary>
		[DataMember( Name = "eid" )]
		public long ApplicationEntityId
		{
			get;
			set;
		}

		/// <summary>
		///     The identifier for the application.
		/// </summary>
		[DataMember( Name = "id" )]
		public Guid ApplicationId
		{
			get;
			set;
		}

		/// <summary>
		///     The identifier for the version of the application.
		/// </summary>
		[DataMember( Name = "vid" )]
		public Guid ApplicationVersionId
		{
			get;
			set;
		}

		/// <summary>
		///     The name of the application.
		/// </summary>
		[DataMember( Name = "name" )]
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		///     The package entity identifier.
		/// </summary>
		[DataMember( Name = "pid" )]
		public long PackageEntityId
		{
			get;
			set;
		}

		/// <summary>
		///     The version of the package.
		/// </summary>
		[DataMember( Name = "packageVersion" )]
		public string PackageVersion
		{
			get;
			set;
		}

		/// <summary>
		///     The publisher of the application.
		/// </summary>
		[DataMember( Name = "publisher" )]
		public string Publisher
		{
			get;
			set;
		}

		/// <summary>
		///     The web site address for the <see cref="Publisher" />.
		/// </summary>
		[DataMember( Name = "publisherUrl" )]
		public string PublisherUrl
		{
			get;
			set;
		}

		/// <summary>
		///     The release date of the application.
		/// </summary>
		[DataMember( Name = "releaseDate" )]
		public DateTime? ReleaseDate
		{
			get;
			set;
		}

        /// <summary>
        ///     True if the current tenant can publish changes to the application, according to global tenant metadata.
        /// </summary>
        public bool HasPublishPermission
        {
            get;
            set;
        }

        /// <summary>
        ///     True if the current tenant can install the application, according to global tenant metadata.
        /// </summary>
        public bool HasInstallPermission
        {
            get;
            set;
        }
	}
}