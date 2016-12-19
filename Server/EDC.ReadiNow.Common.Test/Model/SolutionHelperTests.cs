// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.PartialClasses;
using EDC.SoftwarePlatform.Migration;
using EDC.SoftwarePlatform.Migration.Processing;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
	/// <summary>
	///     Solution Helper Tests class.
	/// </summary>
	[ReadiNowTestFixture]
    [Category( "ExtendedTests" )]
    public class SolutionHelperTests
	{
		/// <summary>
		///     Afters the test.
		/// </summary>
		[TearDown]
		public void AfterTest( )
		{
			TenantHelper.Flush( );
		}

		/// <summary>
		///     Tests the get application dependencies.
		/// </summary>
		/// <param name="applicationId">The application identifier.</param>
		/// <param name="immediateOnly">if set to <c>true</c> [immediate only].</param>
		/// <param name="expectedCount">The expected count.</param>
		/// <param name="expectedResults">The expected results.</param>
		/// <returns></returns>
		[RunAsDefaultTenant]
		[TestCaseSource( nameof( TestGetApplicationDependenciesSource ) )]
		public void TestGetApplicationDependencies( long applicationId, bool immediateOnly, int expectedCount = 0, IEnumerable<Guid> expectedResults = null )
		{
			IList<ApplicationDependency> results = SolutionHelper.GetApplicationDependencies( applicationId, immediateOnly );

			Assert.IsNotNull( results );
			Assert.AreEqual( expectedCount, results.Count );

			if ( expectedResults != null )
			{
				List<Guid> dependencies = results.Select( r => Entity.GetUpgradeId( r.DependencyApplication.Id ) ).ToList( );

				foreach ( Guid expectedResult in expectedResults )
				{
					Assert.Contains( expectedResult, dependencies );
				}
			}
		}

		/// <summary>
		///     Tests the get application dependencies.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void TestGetApplicationDependencies( )
		{
			Solution appA = new Solution
			{
				Name = "appA"
			};
			Solution appB = new Solution
			{
				Name = "appB"
			};
			Solution appC = new Solution
			{
				Name = "appC"
			};
			Solution appD = new Solution
			{
				Name = "appD"
			};
			Solution appE = new Solution
			{
				Name = "appE"
			};

			ApplicationDependency aDependsOnB = new ApplicationDependency
			{
				Name = "appA depends on appB",
				DependentApplication = appA,
				DependencyApplication = appB
			};
			appA.DependentApplicationDetails.Add( aDependsOnB );

			ApplicationDependency aDependsOnC = new ApplicationDependency
			{
				Name = "appA depends on appC",
				DependentApplication = appA,
				DependencyApplication = appC
			};
			appA.DependentApplicationDetails.Add( aDependsOnC );

			appA.Save( );

			ApplicationDependency bDependsOnD = new ApplicationDependency
			{
				Name = "appB depends on appD",
				DependentApplication = appB,
				DependencyApplication = appD
			};
			appB.DependentApplicationDetails.Add( bDependsOnD );

			appB.Save( );

			ApplicationDependency cDependsOnE = new ApplicationDependency
			{
				Name = "appC depends on appE",
				DependentApplication = appC,
				DependencyApplication = appE
			};
			appC.DependentApplicationDetails.Add( cDependsOnE );

			appC.Save( );

			IList<ApplicationDependency> results = SolutionHelper.GetApplicationDependencies( appA.Id, true );

			Assert.IsNotNull( results );
			Assert.AreEqual( 2, results.Count );

			List<long> dependencies = results.Select( r => r.DependencyApplication.Id ).ToList( );

			Assert.Contains( appB.Id, dependencies );
			Assert.Contains( appC.Id, dependencies );

			results = SolutionHelper.GetApplicationDependencies( appA.Id );

			Assert.IsNotNull( results );
			Assert.AreEqual( 4, results.Count );

			dependencies = results.Select( r => r.DependencyApplication.Id ).ToList( );

			Assert.Contains( appB.Id, dependencies );
			Assert.Contains( appC.Id, dependencies );
			Assert.Contains( appD.Id, dependencies );
			Assert.Contains( appE.Id, dependencies );
		}

		/// <summary>
		///     Tests the get application dependencies convergence.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void TestGetApplicationDependenciesConvergence( )
		{
			Solution appA = new Solution
			{
				Name = "appA"
			};
			Solution appB = new Solution
			{
				Name = "appB"
			};
			Solution appC = new Solution
			{
				Name = "appC"
			};
			Solution appD = new Solution
			{
				Name = "appD"
			};

			ApplicationDependency aDependsOnB = new ApplicationDependency
			{
				Name = "appA depends on appB",
				DependentApplication = appA,
				DependencyApplication = appB
			};
			appA.DependentApplicationDetails.Add( aDependsOnB );

			ApplicationDependency aDependsOnC = new ApplicationDependency
			{
				Name = "appA depends on appC",
				DependentApplication = appA,
				DependencyApplication = appC
			};
			appA.DependentApplicationDetails.Add( aDependsOnC );

			appA.Save( );

			ApplicationDependency bDependsOnD = new ApplicationDependency
			{
				Name = "appB depends on appD",
				DependentApplication = appB,
				DependencyApplication = appD
			};
			appB.DependentApplicationDetails.Add( bDependsOnD );

			appB.Save( );

			ApplicationDependency cDependsOnE = new ApplicationDependency
			{
				Name = "appC depends on appD",
				DependentApplication = appC,
				DependencyApplication = appD
			};
			appC.DependentApplicationDetails.Add( cDependsOnE );

			appC.Save( );

			IList<ApplicationDependency> results = SolutionHelper.GetApplicationDependencies( appA.Id, true );

			Assert.IsNotNull( results );
			Assert.AreEqual( 2, results.Count );

			List<long> dependencies = results.Select( r => r.DependencyApplication.Id ).ToList( );

			Assert.Contains( appB.Id, dependencies );
			Assert.Contains( appC.Id, dependencies );

			results = SolutionHelper.GetApplicationDependencies( appA.Id );

			Assert.IsNotNull( results );
			Assert.AreEqual( 3, results.Count );

			dependencies = results.Select( r => r.DependencyApplication.Id ).ToList( );

			Assert.Contains( appB.Id, dependencies );
			Assert.Contains( appC.Id, dependencies );
			Assert.Contains( appD.Id, dependencies );
		}

		/// <summary>
		///     Tests the get application dependencies extension.
		/// </summary>
		/// <param name="applicationId">The application identifier.</param>
		/// <param name="immediateOnly">if set to <c>true</c> [immediate only].</param>
		/// <param name="expectedCount">The expected count.</param>
		/// <param name="expectedResults">The expected results.</param>
		/// <exception cref="System.ArgumentException">Invalid application id specified</exception>
		[RunAsDefaultTenant]
		[TestCaseSource( nameof( TestGetApplicationDependenciesSource ) )]
		public void TestGetApplicationDependenciesExtension( long applicationId, bool immediateOnly, int expectedCount = 0, IEnumerable<Guid> expectedResults = null )
		{
			Solution app = Entity.Get<Solution>( applicationId );

			if ( app == null )
			{
				throw new ArgumentException( "Invalid application id specified" );
			}

			IList<ApplicationDependency> results = app.GetDependencies( immediateOnly );

			Assert.IsNotNull( results );
			Assert.AreEqual( expectedCount, results.Count );

			if ( expectedResults != null )
			{
				List<Guid> dependencies = results.Select( r => Entity.GetUpgradeId( r.DependencyApplication.Id ) ).ToList( );

				foreach ( Guid expectedResult in expectedResults )
				{
					Assert.Contains( expectedResult, dependencies );
				}
			}
		}

		/// <summary>
		///     Tests the get application dependencies extension null.
		/// </summary>
		/// <param name="applicationId">The application identifier.</param>
		/// <param name="immediateOnly">if set to <c>true</c> [immediate only].</param>
		/// <param name="expectedCount">The expected count.</param>
		/// <param name="expectedResults">The expected results.</param>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof( NullReferenceException ) )]
		public void TestGetApplicationDependenciesExtensionNull( )
		{
			Solution app = null;

			app.GetDependencies( true );
		}

		/// <summary>
		///     Tests the get application dependents.
		/// </summary>
		/// <param name="applicationId">The application identifier.</param>
		/// <param name="immediateOnly">if set to <c>true</c> [immediate only].</param>
		/// <param name="minExpectedCount">The minimum expected count.</param>
		/// <param name="minExpectedResults">The minimum expected results.</param>
		[RunAsDefaultTenant]
		[TestCaseSource( nameof( TestGetApplicationDependentsSource ) )]
		public void TestGetApplicationDependents( long applicationId, bool immediateOnly, int minExpectedCount = 0, IEnumerable<Guid> minExpectedResults = null )
		{
			IList<ApplicationDependency> results = SolutionHelper.GetApplicationDependents( applicationId, immediateOnly );

			Assert.IsNotNull( results );
			Assert.GreaterOrEqual( results.Count, minExpectedCount );

			if ( minExpectedResults != null )
			{
				List<Guid> dependents = results.Select( r => Entity.GetUpgradeId( r.DependentApplication.Id ) ).ToList( );

				foreach ( Guid expectedResult in minExpectedResults )
				{
					Assert.Contains( expectedResult, dependents );
				}
			}
		}

		/// <summary>
		///     Tests the get application dependents extension.
		/// </summary>
		/// <param name="applicationId">The application identifier.</param>
		/// <param name="immediateOnly">if set to <c>true</c> [immediate only].</param>
		/// <param name="minExpectedCount">The minimum expected count.</param>
		/// <param name="minExpectedResults">The minimum expected results.</param>
		/// <exception cref="System.ArgumentException">Invalid application id specified</exception>
		[RunAsDefaultTenant]
		[TestCaseSource( nameof( TestGetApplicationDependentsSource ) )]
		public void TestGetApplicationDependentsExtension( long applicationId, bool immediateOnly, int minExpectedCount = 0, IEnumerable<Guid> minExpectedResults = null )
		{
			Solution app = Entity.Get<Solution>( applicationId );

			if ( app == null )
			{
				throw new ArgumentException( "Invalid application id specified" );
			}

			IList<ApplicationDependency> results = app.GetDependents( immediateOnly );

			Assert.IsNotNull( results );
			Assert.GreaterOrEqual( results.Count, minExpectedCount );

			if ( minExpectedResults != null )
			{
				List<Guid> dependents = results.Select( r => Entity.GetUpgradeId( r.DependentApplication.Id ) ).ToList( );

				foreach ( Guid expectedResult in minExpectedResults )
				{
					Assert.Contains( expectedResult, dependents );
				}
			}
		}

		/// <summary>
		///     Tests the get application dependents extension null.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof( NullReferenceException ) )]
		public void TestGetApplicationDependentsExtensionNull( )
		{
			Solution app = null;

			app.GetDependents( true );
		}

		/// <summary>
		///     Tests the get missing dependencies.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void TestGetMissingDependencies( )
		{
			Solution appA = new Solution
			{
				Name = "appA"
			};
			Solution appB = new Solution
			{
				Name = "appB"
			};
			Solution appC = new Solution
			{
				Name = "appC"
			};

			ApplicationDependency aDependsOnB = new ApplicationDependency
			{
				Name = "appA depends on appB",
				DependentApplication = appA,
				DependencyApplication = appB
			};
			appA.DependentApplicationDetails.Add( aDependsOnB );

			appA.Save( );

			ApplicationDependency bDependsOnC = new ApplicationDependency
			{
				Name = "appB depends on appC",
				DependentApplication = appB,
				DependencyApplication = appC
			};
			appB.DependentApplicationDetails.Add( bDependsOnC );

			appB.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "appA" );

			Guid appAUpgradeId = Entity.GetUpgradeId( appA.Id );
			long tenantId = RequestContext.TenantId;

			using ( new GlobalAdministratorContext( ) )
			{
				AppPackage appPackage = SystemHelper.GetLatestPackageByGuid( appAUpgradeId );

				SolutionHelper.GetMissingPackageDependencies( appPackage.Id, tenantId );
			}
		}

		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void TestGetMissingDependencies_BelowMinVersion( )
		{
			Solution appA = new Solution
			{
				Name = "appA",
				SolutionVersionString = "1.0.0.0"
			};
			Solution appB = new Solution
			{
				Name = "appB",
				SolutionVersionString = "1.0.0.0"
			};
			Solution appC = new Solution
			{
				Name = "appC",
				SolutionVersionString = "1.0.0.0"
			};

			ApplicationDependency aDependsOnB = new ApplicationDependency
			{
				Name = "appA depends on appB",
				DependentApplication = appA,
				DependencyApplication = appB
			};
			appA.DependentApplicationDetails.Add( aDependsOnB );

			appA.Save( );

			ApplicationDependency bDependsOnC = new ApplicationDependency
			{
				Name = "appB depends on appC",
				DependentApplication = appB,
				DependencyApplication = appC
			};
			appB.DependentApplicationDetails.Add( bDependsOnC );

			appB.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "appA" );
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "appB" );
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "appC" );

			Guid appAUpgradeId = Entity.GetUpgradeId( appA.Id );
			Guid appBUpgradeId = Entity.GetUpgradeId( appB.Id );

			long tenantId = TenantHelper.CreateTenant( "ABC" );

			AppManager.DeployApp( "ABC", Applications.CoreApplicationId.ToString( "B" ) );
			AppManager.DeployApp( "ABC", appAUpgradeId.ToString( "B" ) );

			appC.SolutionVersionString = "2.0.0.0";
			appC.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "appC" );

			bDependsOnC.ApplicationMinimumVersion = "2.0.0.0";
			bDependsOnC.Save( );
			appB.SolutionVersionString = "2.0.0.0";
			appB.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "appB" );

			using ( new GlobalAdministratorContext( ) )
			{
				AppPackage appPackage = SystemHelper.GetLatestPackageByGuid( appBUpgradeId );

				var applicationDependencies = SolutionHelper.GetMissingPackageDependencies( appPackage.Id, tenantId );

				if ( applicationDependencies != null && applicationDependencies.Count > 0 )
				{
					Assert.AreEqual( 1, applicationDependencies.Count );

					DependencyFailure dependency = applicationDependencies[ 0 ];

					if ( dependency.Reason == DependencyFailureReason.BelowMinVersion )
					{
						SolutionHelper.EnsureUpgradePath( tenantId, dependency );
					}

					Assert.AreEqual( DependencyFailureReason.BelowMinVersion, dependency.Reason );
				}
			}
		}

		/// <summary>
		///     Tests the get missing dependencies invalid package identifier.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof( ArgumentException ) )]
		public void TestGetMissingDependencies_InvalidPackageId( )
		{
			SolutionHelper.GetMissingPackageDependencies( 1234, 0 );
		}

		/// <summary>
		///     Tests the get missing dependencies invalid tenant.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof( ArgumentException ) )]
		public void TestGetMissingDependencies_InvalidTenant( )
		{
			using ( new GlobalAdministratorContext( ) )
			{
				AppPackage appPackage = new AppPackage( );

				SolutionHelper.GetMissingPackageDependencies( appPackage, -1 );
			}
		}

		/// <summary>
		///     Tests the get missing dependencies missing.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void TestGetMissingDependencies_Missing( )
		{
			Solution appA = new Solution
			{
				Name = "appA"
			};
			Solution appB = new Solution
			{
				Name = "appB"
			};
			Solution appC = new Solution
			{
				Name = "appC"
			};

			ApplicationDependency aDependsOnB = new ApplicationDependency
			{
				Name = "appA depends on appB",
				DependentApplication = appA,
				DependencyApplication = appB
			};
			appA.DependentApplicationDetails.Add( aDependsOnB );

			appA.Save( );

			ApplicationDependency bDependsOnC = new ApplicationDependency
			{
				Name = "appB depends on appC",
				DependentApplication = appB,
				DependencyApplication = appC
			};
			appB.DependentApplicationDetails.Add( bDependsOnC );

			appB.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "appA" );
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "appB" );

			long tenantId = TenantHelper.CreateTenant( "ABC" );

			AppManager.DeployApp( "ABC", Applications.CoreApplicationId.ToString( "B" ) );

			Guid appAUpgradeId = Entity.GetUpgradeId( appA.Id );

			using ( new GlobalAdministratorContext( ) )
			{
				AppPackage appPackage = SystemHelper.GetLatestPackageByGuid( appAUpgradeId );

				var applicationDependencies = SolutionHelper.GetMissingPackageDependencies( appPackage.Id, tenantId );

				Assert.AreEqual( 1, applicationDependencies.Count );

				DependencyFailure dependency = applicationDependencies[ 0 ];

				Assert.AreEqual( DependencyFailureReason.Missing, dependency.Reason );
			}
		}

		/// <summary>
		///     Tests the get missing dependencies negative package identifier.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof( ArgumentException ) )]
		public void TestGetMissingDependencies_NegativePackageId( )
		{
			SolutionHelper.GetMissingPackageDependencies( -1, 0 );
		}

		/// <summary>
		///     Tests the get missing dependencies negative tenant identifier.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof( ArgumentException ) )]
		public void TestGetMissingDependencies_NegativeTenantId( )
		{
			SolutionHelper.GetMissingPackageDependencies( 0, -1 );
		}

		/// <summary>
		///     Tests the get missing dependencies not installed.
		/// </summary>
		/// <exception cref="ApplicationDependencyException"></exception>
		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void TestGetMissingDependencies_NotInstalled( )
		{
			Solution appA = new Solution
			{
				Name = "appA"
			};
			Solution appB = new Solution
			{
				Name = "appB"
			};
			Solution appC = new Solution
			{
				Name = "appC"
			};

			ApplicationDependency aDependsOnB = new ApplicationDependency
			{
				Name = "appA depends on appB",
				DependentApplication = appA,
				DependencyApplication = appB
			};
			appA.DependentApplicationDetails.Add( aDependsOnB );

			appA.Save( );

			ApplicationDependency bDependsOnC = new ApplicationDependency
			{
				Name = "appB depends on appC",
				DependentApplication = appB,
				DependencyApplication = appC
			};
			appB.DependentApplicationDetails.Add( bDependsOnC );

			appB.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "appA" );

			long tenantId = TenantHelper.CreateTenant( "ABC" );

			AppManager.DeployApp( "ABC", Applications.CoreApplicationId.ToString( "B" ) );

			Guid appAUpgradeId = Entity.GetUpgradeId( appA.Id );

			using ( new GlobalAdministratorContext( ) )
			{
				AppPackage appPackage = SystemHelper.GetLatestPackageByGuid( appAUpgradeId );

				var applicationDependencies = SolutionHelper.GetMissingPackageDependencies( appPackage.Id, tenantId );

				if ( applicationDependencies != null && applicationDependencies.Count > 0 )
				{
					Assert.AreEqual( 1, applicationDependencies.Count );

					DependencyFailure dependency = applicationDependencies[ 0 ];

					Assert.AreEqual( DependencyFailureReason.NotInstalled, dependency.Reason );
				}
			}
		}

		[Test]
		[RunAsDefaultTenant]
		[RunWithTransaction]
		public void TestGetMissingDependencies_NoUpgradePath( )
		{
			Solution appA = new Solution
			{
				Name = "appA",
				SolutionVersionString = "1.0.0.0"
			};
			Solution appB = new Solution
			{
				Name = "appB",
				SolutionVersionString = "1.0.0.0"
			};
			Solution appC = new Solution
			{
				Name = "appC",
				SolutionVersionString = "1.0.0.0"
			};

			ApplicationDependency aDependsOnB = new ApplicationDependency
			{
				Name = "appA depends on appB",
				DependentApplication = appA,
				DependencyApplication = appB
			};
			appA.DependentApplicationDetails.Add( aDependsOnB );

			appA.Save( );

			ApplicationDependency bDependsOnC = new ApplicationDependency
			{
				Name = "appB depends on appC",
				DependentApplication = appB,
				DependencyApplication = appC
			};
			appB.DependentApplicationDetails.Add( bDependsOnC );

			appB.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "appA" );
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "appB" );
			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "appC" );

			Guid appAUpgradeId = Entity.GetUpgradeId( appA.Id );
			Guid appBUpgradeId = Entity.GetUpgradeId( appB.Id );

			long tenantId = TenantHelper.CreateTenant( "ABC" );

			AppManager.DeployApp( "ABC", Applications.CoreApplicationId.ToString( "B" ) );
			AppManager.DeployApp( "ABC", appAUpgradeId.ToString( "B" ) );

			bDependsOnC.ApplicationMinimumVersion = "2.0.0.0";
			bDependsOnC.Save( );
			appB.SolutionVersionString = "2.0.0.0";
			appB.Save( );

			AppManager.PublishApp( RunAsDefaultTenant.DefaultTenantName, "appB" );

			using ( new GlobalAdministratorContext( ) )
			{
				AppPackage appPackage = SystemHelper.GetLatestPackageByGuid( appBUpgradeId );

				var applicationDependencies = SolutionHelper.GetMissingPackageDependencies( appPackage.Id, tenantId );

				if ( applicationDependencies != null && applicationDependencies.Count > 0 )
				{
					Assert.AreEqual( 1, applicationDependencies.Count );

					DependencyFailure dependency = applicationDependencies[ 0 ];

					if ( dependency.Reason == DependencyFailureReason.BelowMinVersion )
					{
						SolutionHelper.EnsureUpgradePath( tenantId, dependency );
					}

					Assert.AreEqual( DependencyFailureReason.NoUpgradePathAvailable, dependency.Reason );
				}
			}
		}

		/// <summary>
		///     Tests the get missing dependencies null package.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		[ExpectedException( typeof( ArgumentNullException ) )]
		public void TestGetMissingDependencies_NullPackage( )
		{
			SolutionHelper.GetMissingPackageDependencies( null, 0 );
		}

		/// <summary>
		///     Tests the get application dependencies source.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<TestCaseData> TestGetApplicationDependenciesSource( )
		{
			long testSolutionId;

			using ( new TenantAdministratorContext( "EDC" ) )
			{
				EntityAlias testSolutionAlias = new EntityAlias( "core", "testSolution" );
				testSolutionId = testSolutionAlias.ToEntityId( );
			}

			yield return new TestCaseData( -1, false, 0, null ).Throws( typeof( ArgumentException ) );
			yield return new TestCaseData( 0, false, 0, null ).Throws( typeof( ArgumentException ) );
			yield return new TestCaseData( 1, false, 0, null ).Throws( typeof( ArgumentException ) );
			yield return new TestCaseData( -1, true, 0, null ).Throws( typeof( ArgumentException ) );
			yield return new TestCaseData( 0, true, 0, null ).Throws( typeof( ArgumentException ) );
			yield return new TestCaseData( 1, true, 0, null ).Throws( typeof( ArgumentException ) );
			yield return new TestCaseData( testSolutionId, false, 4, new[ ]
			{
				Applications.SharedApplicationId,
				Applications.CoreDataApplicationId,
				Applications.ConsoleApplicationId,
				Applications.CoreApplicationId
			} );
			yield return new TestCaseData( testSolutionId, true, 1, new[ ]
			{
				Applications.SharedApplicationId
			} );
		}

		/// <summary>
		///     Tests the get application dependents source.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<TestCaseData> TestGetApplicationDependentsSource( )
		{
			long coreSolutionId;

			using ( new TenantAdministratorContext( "EDC" ) )
			{
				EntityAlias testSolutionAlias = new EntityAlias( "core", "coreSolution" );
				coreSolutionId = testSolutionAlias.ToEntityId( );
			}

			yield return new TestCaseData( -1, false, 0, null ).Throws( typeof( ArgumentException ) );
			yield return new TestCaseData( 0, false, 0, null ).Throws( typeof( ArgumentException ) );
			yield return new TestCaseData( 1, false, 0, null ).Throws( typeof( ArgumentException ) );
			yield return new TestCaseData( -1, true, 0, null ).Throws( typeof( ArgumentException ) );
			yield return new TestCaseData( 0, true, 0, null ).Throws( typeof( ArgumentException ) );
			yield return new TestCaseData( 1, true, 0, null ).Throws( typeof( ArgumentException ) );
			yield return new TestCaseData( coreSolutionId, false, 4, new[ ]
			{
				Applications.SharedApplicationId,
				Applications.CoreDataApplicationId,
				Applications.ConsoleApplicationId,
				Applications.TestApplicationId
			} );
			yield return new TestCaseData( coreSolutionId, true, 1, new[ ]
			{
				Applications.ConsoleApplicationId
			} );
		}
	}
}