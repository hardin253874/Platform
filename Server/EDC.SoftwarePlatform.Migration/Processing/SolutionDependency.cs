// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     The solution dependency class.
	/// </summary>
	public class SolutionDependency
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="SolutionDependency" /> class.
		/// </summary>
		internal SolutionDependency( )
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SolutionDependency" /> class.
		/// </summary>
		/// <param name="dependency">The dependency.</param>
		public SolutionDependency( ApplicationDependency dependency )
		{
			if ( dependency == null )
			{
				throw new ArgumentNullException( nameof( dependency ) );
			}

			Name = dependency.Name;

			if ( dependency.DependencyApplication != null )
			{
				DependencyName = dependency.DependencyApplication.Name;
				DependencyApplication = dependency.DependencyApplication.UpgradeId;
			}

			Version minVersion;

			if ( string.IsNullOrEmpty( dependency.ApplicationMinimumVersion ) || dependency.ApplicationMinimumVersion.Equals( "any", StringComparison.InvariantCultureIgnoreCase ) || !Version.TryParse( dependency.ApplicationMinimumVersion, out minVersion ) )
			{
				MinimumVersion = null;
			}
			else
			{
				MinimumVersion = minVersion;
			}

			Version maxVersion;

			if ( string.IsNullOrEmpty( dependency.ApplicationMaximumVersion ) || dependency.ApplicationMaximumVersion.Equals( "any", StringComparison.InvariantCultureIgnoreCase ) || !Version.TryParse( dependency.ApplicationMaximumVersion, out maxVersion ) )
			{
				MaximumVersion = null;
			}
			else
			{
				MaximumVersion = maxVersion;
			}

			IsRequired = dependency.ApplicationIsRequired ?? true;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SolutionDependency" /> class.
		/// </summary>
		/// <param name="dependency">The dependency.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException">No dependency id specified.</exception>
		public SolutionDependency( AppPackageDependency dependency )
		{
			if ( dependency == null )
			{
				throw new ArgumentNullException( nameof( dependency ) );
			}

			if ( dependency.AppPackageDependencyId == null )
			{
				throw new ArgumentException( "No dependency id specified." );
			}

			Name = dependency.Name;
			DependencyName = dependency.AppPackageDependencyName;

			Version minVersion;

			if ( string.IsNullOrEmpty( dependency.AppPackageMinimumVersion ) || dependency.AppPackageMinimumVersion.Equals( "any", StringComparison.InvariantCultureIgnoreCase ) || !Version.TryParse( dependency.AppPackageMinimumVersion, out minVersion ) )
			{
				MinimumVersion = null;
			}
			else
			{
				MinimumVersion = minVersion;
			}

			Version maxVersion;

			if ( string.IsNullOrEmpty( dependency.AppPackageMaximumVersion ) || dependency.AppPackageMaximumVersion.Equals( "any", StringComparison.InvariantCultureIgnoreCase ) || !Version.TryParse( dependency.AppPackageMaximumVersion, out maxVersion ) )
			{
				MaximumVersion = null;
			}
			else
			{
				MaximumVersion = maxVersion;
			}

			IsRequired = dependency.AppPackageIsRequired ?? true;
			DependencyApplication = dependency.AppPackageDependencyId.Value;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="SolutionDependency" /> class.
		/// </summary>
		/// <param name="dependencyApplication">The dependency application.</param>
		/// <param name="name">The name.</param>
		/// <param name="dependencyName">Name of the dependency.</param>
		/// <param name="minimumVersion">The minimum version.</param>
		/// <param name="maximumVersion">The maximum version.</param>
		/// <param name="isRequired">if set to <c>true</c> [is required].</param>
		public SolutionDependency( Guid dependencyApplication, string name = null, string dependencyName = null, Version minimumVersion = null, Version maximumVersion = null, bool isRequired = true )
		{
			Name = name;
			DependencyName = dependencyName;
			DependencyApplication = dependencyApplication;
			MinimumVersion = minimumVersion;
			MaximumVersion = maximumVersion;
			IsRequired = isRequired;
		}

		/// <summary>
		///     Gets the dependency application.
		/// </summary>
		/// <value>
		///     The dependency application.
		/// </value>
		public Guid DependencyApplication
		{
			get;
			internal set;
		}

		/// <summary>
		///     Gets the name of the dependency.
		/// </summary>
		/// <value>
		///     The name of the dependency.
		/// </value>
		public string DependencyName
		{
			get;
			internal set;
		}

		/// <summary>
		///     Gets a value indicating whether this instance is required.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance is required; otherwise, <c>false</c>.
		/// </value>
		public bool IsRequired
		{
			get;
			internal set;
		}

		/// <summary>
		///     Gets the maximum version.
		/// </summary>
		/// <value>
		///     The maximum version.
		/// </value>
		public Version MaximumVersion
		{
			get;
			internal set;
		}

		/// <summary>
		///     Gets the minimum version.
		/// </summary>
		/// <value>
		///     The minimum version.
		/// </value>
		public Version MinimumVersion
		{
			get;
			internal set;
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
			internal set;
		}
	}
}