// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model.PartialClasses;

/////
// ReSharper disable CheckNamespace
/////

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Solution class.
	/// </summary>
	public static class SolutionHelper
	{
		/// <summary>
		/// Ensures the upgrade path.
		/// </summary>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="dependencyFailure">The dependency failure.</param>
		public static void EnsureUpgradePath( long tenantId, DependencyFailure dependencyFailure )
		{
			if ( tenantId < 0 )
			{
				return;
			}

			if ( dependencyFailure == null )
			{
				return;
			}

			AppPackage upgradePackage;

			bool upgradePackageFound = TryGetUpgradePackage( dependencyFailure, out upgradePackage );

			if ( upgradePackageFound )
			{
				EnsureUpgradeCompatibility( dependencyFailure, upgradePackage, tenantId );
			}
			else
			{
				dependencyFailure.Reason = DependencyFailureReason.NoUpgradePathAvailable;
			}
		}

		/// <summary>
		/// Ensures the upgrade compatibility.
		/// </summary>
		/// <param name="dependencyFailure">The dependency failure.</param>
		/// <param name="upgradePackage">The upgrade package.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		private static void EnsureUpgradeCompatibility( DependencyFailure dependencyFailure, AppPackage upgradePackage, long tenantId )
		{
			if ( dependencyFailure == null )
			{
				return;
			}

			if ( upgradePackage == null )
			{
				return;
			}

			if ( tenantId < 0 )
			{
				return;
			}

			Version upgradePackageVersion;

			if ( !Version.TryParse( upgradePackage.AppVersionString, out upgradePackageVersion ) )
			{
				return;
			}

			using ( new TenantAdministratorContext( tenantId ) )
			{
				var applicationDependencies = Entity.GetInstancesOfType<ApplicationDependency> ( );

				if ( applicationDependencies == null )
				{
					return;
				}

				var dependencies = applicationDependencies.ToList( );

				if ( dependencies.Count <= 0 )
				{
					return;
				}

				foreach ( ApplicationDependency dependency in dependencies )
				{
					if ( dependency.DependencyApplication.UpgradeId == dependencyFailure.ApplicationId )
					{
						Version dependencyMinVersion;
						Version dependencyMaxVersion;

						if ( Version.TryParse( dependency.ApplicationMinimumVersion, out dependencyMinVersion )  && Version.TryParse( dependency.ApplicationMaximumVersion, out dependencyMaxVersion) )
						{
							if ( upgradePackageVersion < dependencyMinVersion || upgradePackageVersion > dependencyMaxVersion )
							{
								dependencyFailure.Reason = DependencyFailureReason.IncompatibleUpgradePath;
								dependencyFailure.DependentName = dependency.DependentApplication.Name;
								break;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Determines whether a viable upgrade is available.
		/// </summary>
		/// <param name="dependency">The dependency.</param>
		/// <param name="upgradePackage">The upgrade package.</param>
		/// <returns>
		///   <c>true</c> if a viable upgrade is available; otherwise, <c>false</c>.
		/// </returns>
		private static bool TryGetUpgradePackage( DependencyFailure dependency, out AppPackage upgradePackage )
		{
			upgradePackage = null;

			if ( dependency == null )
			{
				return false;
			}

			long appId = GetApplicationId( dependency.ApplicationId );

			if ( appId < 0 )
			{
				return false;
			}

			App app = Entity.Get<App>( appId );

			var applicationPackages = app.ApplicationPackages;

			if ( applicationPackages != null && applicationPackages.Count > 0 )
			{
				List<Tuple<Version, AppPackage>> list = new List<Tuple<Version, AppPackage>>( );

				foreach ( AppPackage applicationPackage in applicationPackages )
				{
					Version packageVersion;

					if ( Version.TryParse( applicationPackage.AppVersionString, out packageVersion ) )
					{
						list.Add( new Tuple<Version, AppPackage>( packageVersion, applicationPackage ) );
					}
				}

				if ( list.Count > 0 )
				{
					var pair = list.OrderByDescending( p => p.Item1 ).FirstOrDefault( p => ( dependency.MinVersion == null || p.Item1 >= dependency.MinVersion ) && ( dependency.MaxVersion == null || p.Item1 <= dependency.MaxVersion ) );

					if ( pair != null )
					{
						upgradePackage = pair.Item2;
						return true;
					}
				}
			}

			return false;

		}

		/// <summary>
		///     Gets the application dependencies.
		/// </summary>
		/// <param name="applicationId">The application identifier.</param>
		/// <param name="immediateOnly">if set to <c>true</c> [immediate only].</param>
		/// <returns></returns>
		public static IList<ApplicationDependency> GetApplicationDependencies( long applicationId, bool immediateOnly = false )
		{
			if ( applicationId <= 0 )
			{
				throw new ArgumentException( "Invalid application id." );
			}

			Solution application = Entity.Get<Solution>( applicationId );

			if ( application == null )
			{
				throw new ArgumentException( "Specified application Id does not represent a known application." );
			}

			var dependencies = GetDependencies( application, immediateOnly, s => s.DependentApplicationDetails, d => d.DependencyApplication, Ordering.Prefix );

			return dependencies;
		}

		/// <summary>
		///     Gets the application dependents.
		/// </summary>
		/// <param name="applicationId">The application identifier.</param>
		/// <param name="immediateOnly">if set to <c>true</c> [immediate only].</param>
		/// <returns></returns>
		public static IList<ApplicationDependency> GetApplicationDependents( long applicationId, bool immediateOnly = false )
		{
			if ( applicationId <= 0 )
			{
				throw new ArgumentException( "Invalid application id." );
			}

			Solution application = Entity.Get<Solution>( applicationId );

			if ( application == null )
			{
				throw new ArgumentException( "Specified application Id does not represent a known application." );
			}

			var dependents = GetDependencies( application, immediateOnly, s => s.DependencyApplicationDetails, d => d.DependentApplication, Ordering.Postfix );

			return dependents;
		}

		/// <summary>
		///     Gets the dependencies.
		/// </summary>
		/// <param name="solution">The solution.</param>
		/// <param name="immediateOnly">if set to <c>true</c> [immediate only].</param>
		/// <returns></returns>
		public static IList<ApplicationDependency> GetDependencies( this Solution solution, bool immediateOnly = false )
		{
			if ( solution == null )
			{
				throw new NullReferenceException( );
			}

			return GetApplicationDependencies( solution.Id, immediateOnly );
		}

		/// <summary>
		///     Gets the dependents.
		/// </summary>
		/// <param name="solution">The solution.</param>
		/// <param name="immediateOnly">if set to <c>true</c> [immediate only].</param>
		/// <returns></returns>
		public static IList<ApplicationDependency> GetDependents( this Solution solution, bool immediateOnly = false )
		{
			if ( solution == null )
			{
				throw new NullReferenceException( );
			}

			return GetApplicationDependents( solution.Id, immediateOnly );
		}

		/// <summary>
		///     Gets the missing dependencies.
		/// </summary>
		/// <param name="package">The package.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="immediateOnly">if set to <c>true</c> [immediate only].</param>
		/// <returns></returns>
		/// <exception cref="NullReferenceException"></exception>
		public static IList<DependencyFailure> GetMissingDependencies( this AppPackage package, long tenantId, bool immediateOnly = false )
		{
			if ( package == null )
			{
				throw new NullReferenceException( );
			}

			if ( tenantId < 0 )
			{
				throw new ArgumentException( @"Invalid tenant id specified.", nameof( tenantId ) );
			}

			return GetMissingPackageDependencies( package, tenantId, immediateOnly );
		}

		/// <summary>
		///     Gets the missing package dependencies.
		/// </summary>
		/// <param name="packageId">The package identifier.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="immediateOnly">if set to <c>true</c> [immediate only].</param>
		/// <returns></returns>
		public static IList<DependencyFailure> GetMissingPackageDependencies( long packageId, long tenantId, bool immediateOnly = false )
		{
			if ( packageId < 0 )
			{
				throw new ArgumentException( @"Invalid package id specified.", nameof( packageId ) );
			}

			if ( tenantId < 0 )
			{
				throw new ArgumentException( @"Invalid tenant id specified.", nameof( tenantId ) );
			}

			AppPackage package = Entity.Get<AppPackage>( packageId );

			if ( package == null )
			{
				throw new ArgumentException( "Specified package Id does not represent a known package." );
			}

			return GetMissingPackageDependencies( package, tenantId, immediateOnly );
		}

		/// <summary>
		///     Gets the missing package dependencies.
		/// </summary>
		/// <param name="package">The package.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="immediateOnly">if set to <c>true</c> [immediate only].</param>
		/// <returns></returns>
		public static IList<DependencyFailure> GetMissingPackageDependencies( AppPackage package, long tenantId, bool immediateOnly = false )
		{
			if ( package == null )
			{
				throw new ArgumentNullException( nameof( package ) );
			}

			if ( tenantId < 0 )
			{
				throw new ArgumentException( @"Invalid tenant id specified.", nameof( tenantId ) );
			}

			IList<AppPackageDependency> dependencyDetails = package.DependentAppPackageDetails.ToList( );

			IList<DependencyFailure> dependencyFailures = null;

			foreach ( AppPackageDependency dependency in dependencyDetails )
			{
				Guid applicationUpgradeId = dependency.AppPackageDependencyId ?? Guid.Empty;
				string name = dependency.Name;
				string dependencyName = dependency.AppPackageDependencyName;
				string minimumVersion = dependency.AppPackageMinimumVersion;
				string maximumVersion = dependency.AppPackageMaximumVersion;
				bool isRequired = dependency.AppPackageIsRequired ?? true;

				Version minVersion;

				if ( string.IsNullOrEmpty( minimumVersion ) || minimumVersion.Equals( "any", StringComparison.InvariantCultureIgnoreCase ) || !Version.TryParse( minimumVersion, out minVersion ) )
				{
					minVersion = null;
				}

				Version maxVersion;

				if ( string.IsNullOrEmpty( maximumVersion ) || maximumVersion.Equals( "any", StringComparison.InvariantCultureIgnoreCase ) || !Version.TryParse( maximumVersion, out maxVersion ) )
				{
					maxVersion = null;
				}

				if ( applicationUpgradeId != Guid.Empty && isRequired )
				{
					using ( new TenantAdministratorContext( tenantId ) )
					{
						long applicationId = Entity.GetIdFromUpgradeId( applicationUpgradeId );

						if ( applicationId >= 0 )
						{
							Solution dependencySolution = Entity.Get<Solution>( applicationId, Solution.SolutionVersionString_Field );

							if ( dependencySolution != null )
							{
								Version version;

								if ( !string.IsNullOrEmpty( dependencySolution.SolutionVersionString ) && Version.TryParse( dependencySolution.SolutionVersionString, out version ) )
								{
									if ( minVersion != null )
									{
										if ( version < minVersion )
										{
											if ( dependencyFailures == null )
											{
												dependencyFailures = new List<DependencyFailure>( );
											}

											var dependencyFailure = new DependencyFailure( applicationUpgradeId, name, dependencyName, minVersion, maxVersion, DependencyFailureReason.BelowMinVersion )
											{
												CurrentVersion = version
											};

											dependencyFailures.Add( dependencyFailure );
											continue;
										}
									}

									if ( maxVersion != null )
									{
										if ( version > maxVersion )
										{
											if ( dependencyFailures == null )
											{
												dependencyFailures = new List<DependencyFailure>( );
											}

											dependencyFailures.Add( new DependencyFailure( applicationUpgradeId, name, dependencyName, minVersion, maxVersion, DependencyFailureReason.AboveMaxVersion ) );
										}
									}
								}
							}
						}
						else
						{
							if ( dependencyFailures == null )
							{
								dependencyFailures = new List<DependencyFailure>( );
							}

							var failureReason = !ApplicationExistsInLibrary( applicationUpgradeId ) ? DependencyFailureReason.NotInstalled : DependencyFailureReason.Missing;

							dependencyFailures.Add( new DependencyFailure( applicationUpgradeId, name, dependencyName, minVersion, maxVersion, failureReason ) );
						}
					}
				}
			}


			return dependencyFailures;
		}

		/// <summary>
		///     Applications the exists in library.
		/// </summary>
		/// <param name="applicationId">The application identifier.</param>
		/// <returns></returns>
		private static bool ApplicationExistsInLibrary( Guid applicationId )
		{
			if ( applicationId == Guid.Empty )
			{
				throw new ArgumentException( "Invalid application id" );
			}

			return GetApplicationId( applicationId ) >= 0;
		}

		/// <summary>
		/// Gets the application identifier.
		/// </summary>
		/// <param name="applicationUpgradeId">The application upgrade identifier.</param>
		/// <returns></returns>
		private static long GetApplicationId( Guid applicationUpgradeId )
		{
			using ( DatabaseContext ctx = DatabaseContext.GetContext( ) )
			{
				string commandText = @"
DECLARE @appId BIGINT = dbo.fnAliasNsId( 'applicationId', 'core', DEFAULT )

SELECT
	EntityId
FROM
	Data_Guid
WHERE
	TenantId = 0
	AND FieldId = @appId
	AND Data = @applicationUpgradeId";

				using ( IDbCommand command = ctx.CreateCommand( commandText ) )
				{
					ctx.AddParameter( command, "@applicationUpgradeId", DbType.Guid, applicationUpgradeId );

					object result = command.ExecuteScalar( );

					if ( result != null && result != DBNull.Value )
					{
						long id = ( long ) result;

						return id;
					}

					return -1;
				}
			}
		}

		/// <summary>
		/// Gets the dependencies.
		/// </summary>
		/// <param name="application">The application.</param>
		/// <param name="immediateOnly">if set to <c>true</c> [immediate only].</param>
		/// <param name="applicationWalk">The application walk.</param>
		/// <param name="dependencyWalk">The dependency walk.</param>
		/// <param name="ordering">The ordering.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">application
		/// or
		/// applicationWalk
		/// or
		/// dependencyWalk</exception>
		private static IList<ApplicationDependency> GetDependencies( Solution application, bool immediateOnly, Func<Solution, IEnumerable<ApplicationDependency>> applicationWalk, Func<ApplicationDependency, Solution> dependencyWalk, Ordering ordering )
		{
			if ( application == null )
			{
				throw new ArgumentNullException( nameof( application ) );
			}

			if ( applicationWalk == null )
			{
				throw new ArgumentNullException( nameof( applicationWalk ) );
			}

			if ( dependencyWalk == null )
			{
				throw new ArgumentNullException( nameof( dependencyWalk ) );
			}

			HashSet<long> discoveredDependencies = new HashSet<long>( );

			List<ApplicationDependency> results = new List<ApplicationDependency>( );

			GetDependenciesRecursive( application, immediateOnly, applicationWalk, dependencyWalk, ordering, discoveredDependencies, results );

			return results;
		}

		/// <summary>
		/// Gets the dependencies recursive.
		/// </summary>
		/// <param name="application">The application.</param>
		/// <param name="immediateOnly">if set to <c>true</c> [immediate only].</param>
		/// <param name="applicationWalk">The application walk.</param>
		/// <param name="dependencyWalk">The dependency walk.</param>
		/// <param name="ordering">The ordering.</param>
		/// <param name="discoveredDependencies">The discovered dependencies.</param>
		/// <param name="results">The results.</param>
		/// <exception cref="System.ArgumentNullException">
		/// application
		/// or
		/// applicationWalk
		/// or
		/// dependencyWalk
		/// </exception>
		private static void GetDependenciesRecursive( Solution application, bool immediateOnly, Func<Solution, IEnumerable<ApplicationDependency>> applicationWalk, Func<ApplicationDependency, Solution> dependencyWalk, Ordering ordering, ISet<long> discoveredDependencies, IList<ApplicationDependency> results )
		{
			if ( application == null )
			{
				throw new ArgumentNullException( nameof( application ) );
			}

			if ( applicationWalk == null )
			{
				throw new ArgumentNullException( nameof( applicationWalk ) );
			}

			if ( dependencyWalk == null )
			{
				throw new ArgumentNullException( nameof( dependencyWalk ) );
			}

			IEnumerable<ApplicationDependency> applicationDependencies = applicationWalk( application );

			if ( applicationDependencies != null )
			{
				foreach ( var applicationDependency in applicationDependencies )
				{
					if ( !immediateOnly )
					{
						application = dependencyWalk( applicationDependency );

						if ( discoveredDependencies.Contains( application.Id ) )
						{
							continue;
						}

						discoveredDependencies.Add( application.Id );

						if ( ordering == Ordering.Prefix )
						{
							results.Add( applicationDependency );
						}

						GetDependenciesRecursive( application, false, applicationWalk, dependencyWalk, ordering, discoveredDependencies, results );
					}

					if ( ordering == Ordering.Postfix || immediateOnly )
					{
						results.Add( applicationDependency );
					}
				}
			}
		}
	}

	/// <summary>
	/// Discovery ordering
	/// </summary>
	public enum Ordering
	{
		/// <summary>
		/// Prefix ordering
		/// </summary>
		Prefix,

		/// <summary>
		/// Postfix ordering
		/// </summary>
		Postfix
	}
}

/////
// ReSharper restore CheckNamespace
/////