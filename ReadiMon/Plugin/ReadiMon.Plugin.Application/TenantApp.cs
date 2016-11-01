// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace ReadiMon.Plugin.Application
{
	/// <summary>
	///     Tenant App
	/// </summary>
	public class TenantApp
	{
        /// <summary>
        ///     Initializes a new instance of the <see cref="TenantApp" /> class.
        /// </summary>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="tenantName">Name of the tenant.</param>
        /// <param name="solution">The solution.</param>
        /// <param name="solutionEntityId">The solution entity identifier.</param>
        /// <param name="solutionVersion">The solution version.</param>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="packageEntityId">The package entity identifier.</param>
        /// <param name="packageVersion">The package version.</param>
        /// <param name="applicationEntityId">The application entity identifier.</param>
        /// <param name="applicationId">The application identifier.</param>
        /// <param name="publisher">The publisher.</param>
        /// <param name="publisherUrl">The publisher URL.</param>
        /// <param name="releaseDate">The release date.</param>
        /// <param name="isProtected"></param>		
        public TenantApp( long tenantId, string tenantName, string solution, long solutionEntityId, string solutionVersion, Guid packageId, long packageEntityId, string packageVersion, long applicationEntityId, Guid applicationId, string publisher, string publisherUrl, DateTime releaseDate, bool isProtected)
		{
			TenantId = tenantId;
			TenantName = tenantName;
			Solution = solution;
			SolutionEntityId = solutionEntityId;
			SolutionVersion = solutionVersion;
			PackageId = packageId;
			PackageEntityId = packageEntityId;
			PackageVersion = packageVersion;
			ApplicationEntityId = applicationEntityId;
			ApplicationId = applicationId;
			Publisher = publisher;
			PublisherUrl = publisherUrl;
			ReleaseDate = releaseDate;
            IsProtected = isProtected;
		}

		/// <summary>
		///     Gets the application entity identifier.
		/// </summary>
		/// <value>
		///     The application entity identifier.
		/// </value>
		public long ApplicationEntityId
		{
			get;
		}

		/// <summary>
		///     Gets the application identifier.
		/// </summary>
		/// <value>
		///     The application identifier.
		/// </value>
		public Guid ApplicationId
		{
			get;
		}

		/// <summary>
		///     Gets the package entity identifier.
		/// </summary>
		/// <value>
		///     The package entity identifier.
		/// </value>
		public long PackageEntityId
		{
			get;
		}

		/// <summary>
		///     Gets the package identifier.
		/// </summary>
		/// <value>
		///     The package identifier.
		/// </value>
		public Guid PackageId
		{
			get;
		}

		/// <summary>
		///     Gets the package version.
		/// </summary>
		/// <value>
		///     The package version.
		/// </value>
		public string PackageVersion
		{
			get;
		}

		/// <summary>
		///     Gets the publisher.
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
		///     Gets the publisher URL.
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
		///     Gets the release date.
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
		///     Gets the solution.
		/// </summary>
		/// <value>
		///     The solution.
		/// </value>
		public string Solution
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the solution entity identifier.
		/// </summary>
		/// <value>
		///     The solution entity identifier.
		/// </value>
		public long SolutionEntityId
		{
			get;
		}

		/// <summary>
		///     Gets the solution version.
		/// </summary>
		/// <value>
		///     The solution version.
		/// </value>
		public string SolutionVersion
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the tenant identifier.
		/// </summary>
		/// <value>
		///     The tenant identifier.
		/// </value>
		public long TenantId
		{
			get;
		}

		/// <summary>
		///     Gets the name of the tenant.
		/// </summary>
		/// <value>
		///     The name of the tenant.
		/// </value>
		public string TenantName
		{
			get;
			private set;
		}


        /// <summary>
        /// Gets a value indicating whether this instance is protected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is protected; otherwise, <c>false</c>.
        /// </value>
        public bool IsProtected
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
        public string Tooltip => $"Tenant Id: {TenantId}\nSolution Entity Id: {SolutionEntityId}\nPackage Id: {PackageId.ToString( "B" )}\nPackage Entity Id: {PackageEntityId}\nPackage Version: {PackageVersion}\nApplication Entity Id: {ApplicationEntityId}\nApplication Id: {ApplicationId.ToString( "B" )}";
	}
}