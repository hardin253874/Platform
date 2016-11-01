// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Solution details container.
	/// </summary>
	public class SolutionDetails
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="SolutionDetails" /> class.
		/// </summary>
		/// <param name="solution">The solution.</param>
		/// <remarks>Copy constructor.</remarks>
		public SolutionDetails( Solution solution )
		{
			if ( solution == null )
			{
				throw new ArgumentNullException( "solution" );
			}

			Name = solution.Name;
			Description = solution.Description;
			UpgradeId = solution.UpgradeId;
			Publisher = solution.SolutionPublisher;
			PublisherUrl = solution.SolutionPublisherUrl;
			ReleaseDate = solution.SolutionReleaseDate ?? DateTime.UtcNow;
			Dependencies = new List<SolutionDependency>( );

			if ( solution.PackageId != null )
			{
				PackageId = solution.PackageId.Value;
			}

			string existingVersionString = null;

			if ( PackageId != Guid.Empty )
			{
				using ( new GlobalAdministratorContext( ) )
				{
					ExistingPackage = SystemHelper.GetPackageByVerId( PackageId );

					if ( ExistingPackage != null )
					{
						/////
						// Get the existing packages version number since it may be required to set the current version.
						/////
						existingVersionString = ExistingPackage.AppVersionString;
					}
				}
			}

			Version version = null;
			string solutionVersionString = solution.SolutionVersionString;

			if ( !string.IsNullOrEmpty( solutionVersionString ) )
			{
				Version.TryParse( solutionVersionString, out version );
			}


			if ( version == null )
			{
				if ( !string.IsNullOrEmpty( existingVersionString ) )
				{
					Version existingVersion;

					if ( Version.TryParse( existingVersionString, out existingVersion ) )
					{
						int major = existingVersion.Major;
						int minor = existingVersion.Minor;

						if ( major <= 0 )
						{
							major = 1;
						}

						if ( minor < 0 )
						{
							minor = 0;
						}
						version = new Version( major, ++minor );
					}
				}
			}

			if ( version == null )
			{
				version = new Version( 1, 0 );
			}

			Version = version;

			if ( solution.DependentApplicationDetails != null )
			{
				foreach ( ApplicationDependency dependency in solution.DependentApplicationDetails )
				{
					Dependencies.Add( new SolutionDependency( dependency ) );
				}
			}
		}

		/// <summary>
		///     Gets the dependencies.
		/// </summary>
		/// <value>
		///     The dependencies.
		/// </value>
		public List<SolutionDependency> Dependencies
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public string Description
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the existing package.
		/// </summary>
		/// <value>
		///     The existing package.
		/// </value>
		public AppPackage ExistingPackage
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the package unique identifier.
		/// </summary>
		/// <value>
		///     The package unique identifier.
		/// </value>
		public Guid PackageId
		{
			get;
			private set;
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
		///     Gets the upgrade unique identifier.
		/// </summary>
		/// <value>
		///     The upgrade unique identifier.
		/// </value>
		public Guid UpgradeId
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
		public Version Version
		{
			get;
			private set;
		}
	}
}