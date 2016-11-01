// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Application Data.
	/// </summary>
	public class AppData
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="AppData" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="applicationId">The application identifier.</param>
        public AppData( string name, Guid applicationId, bool hasInstallPermission = false, bool hasPublishPermission = false )
		{
			Name = name;
			ApplicationId = applicationId;
			Packages = new List<AppPackageData>( );
            HasInstallPermission = hasInstallPermission;
            HasPublishPermission = hasPublishPermission;
		}

		/// <summary>
		///     Gets the application identifier.
		/// </summary>
        public Guid ApplicationId { get; private set; }

		/// <summary>
		///     Gets the name.
		/// </summary>
        public string Name { get; private set; }

		/// <summary>
		///     Gets the packages.
		/// </summary>
        public List<AppPackageData> Packages { get; private set; }

        /// <summary>
        ///     HasInstallPermission
        /// </summary>
        public bool HasInstallPermission { get; private set; }

        /// <summary>
        ///     HasPublishPermission
        /// </summary>
        public bool HasPublishPermission { get; private set; }
	}
}