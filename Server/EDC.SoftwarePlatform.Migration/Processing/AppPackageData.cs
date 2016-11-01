// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Application Package Data
	/// </summary>
	public class AppPackageData
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="AppPackageData" /> class.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="packageId">The package identifier.</param>
		public AppPackageData( string version, Guid packageId )
		{
			Version = version;
			PackageId = packageId;
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
			private set;
		}

		/// <summary>
		///     Gets the version.
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