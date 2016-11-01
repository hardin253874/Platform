// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace ReadiMon.Plugin.Application
{
	/// <summary>
	///     AppLibrary application.
	/// </summary>
	public class AppLibraryApp
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="AppLibraryApp" /> class.
		/// </summary>
		/// <param name="application">The application.</param>
		/// <param name="applicationEntityId">The application entity identifier.</param>
		/// <param name="packageId">The package identifier.</param>
		/// <param name="packageEntityId">The package entity identifier.</param>
		/// <param name="version">The version.</param>
		/// <param name="applicationId">The application identifier.</param>
		/// <param name="publisher">The publisher.</param>
		/// <param name="publisherUrl">The publisher URL.</param>
		/// <param name="releaseDate">The release date.</param>
		public AppLibraryApp( string application, long applicationEntityId, Guid packageId, long packageEntityId, string version, Guid applicationId, string publisher, string publisherUrl, DateTime releaseDate )
		{
			Application = application;
			ApplicationEntityId = applicationEntityId;
			PackageId = packageId;
			PackageEntityId = packageEntityId;
			Version = version;
			ApplicationId = applicationId;
			Publisher = publisher;
			PublisherUrl = publisherUrl;
			ReleaseDate = releaseDate;
		}

		/// <summary>
		///     Gets or sets the application.
		/// </summary>
		/// <value>
		///     The application.
		/// </value>
		public string Application
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the application entity identifier.
		/// </summary>
		/// <value>
		///     The application entity identifier.
		/// </value>
		public long ApplicationEntityId
		{
			get;
		}

		/// <summary>
		///     Gets or sets the application identifier.
		/// </summary>
		/// <value>
		///     The application identifier.
		/// </value>
		public Guid ApplicationId
		{
			get;
		}

		/// <summary>
		///     Gets or sets the package entity identifier.
		/// </summary>
		/// <value>
		///     The package entity identifier.
		/// </value>
		public long PackageEntityId
		{
			get;
		}

		/// <summary>
		///     Gets or sets the package identifier.
		/// </summary>
		/// <value>
		///     The package identifier.
		/// </value>
		public Guid PackageId
		{
			get;
		}

		/// <summary>
		///     Gets or sets the publisher.
		/// </summary>
		/// <value>
		///     The publisher.
		/// </value>
		public string Publisher
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the publisher URL.
		/// </summary>
		/// <value>
		///     The publisher URL.
		/// </value>
		public string PublisherUrl
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the release date.
		/// </summary>
		/// <value>
		///     The release date.
		/// </value>
		public DateTime ReleaseDate
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the tool-tip.
		/// </summary>
		/// <value>
		///     The tool-tip.
		/// </value>
		public string Tooltip => $"Application Entity Id: {ApplicationEntityId}\nApplication Id: {ApplicationId.ToString( "B" )}\nPackage Entity Id: {PackageEntityId}\nPackage Id: {PackageId.ToString( "B" )}";

		/// <summary>
		///     Gets or sets the version.
		/// </summary>
		/// <value>
		///     The version.
		/// </value>
		public string Version
		{
			get;
			private set;
		}
	}
}