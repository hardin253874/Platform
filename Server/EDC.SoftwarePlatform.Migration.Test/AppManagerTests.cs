// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Migration.Test
{
	/// <summary>
	///     The application manager tests class.
	/// </summary>
	[TestFixture]
	public class AppManagerTests
	{
		/// <summary>
		///     The test solution upgrade identifier
		/// </summary>
		private readonly Guid _testSolutionUpgradeId = new Guid( "5f7eb596-1f47-409d-a4d8-33c2e16b079f" );

		/// <summary>
		///     The core upgrade identifier
		/// </summary>
		private readonly Guid _coreUpgradeId = new Guid( "7062aade-2e72-4a71-a7fa-a412d20d6f01" );

		[Test]
		[RunAsDefaultTenant]
		public void TestGetDependencies( )
		{
			long testSolutionId = Entity.GetIdFromUpgradeId( _testSolutionUpgradeId );

			var applicationDependencies = SolutionHelper.GetApplicationDependencies( testSolutionId );

			Assert.IsTrue( applicationDependencies.Count > 0, "No application dependencies found" );
		}

		/// <summary>
		///     Tests the get dependents.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestGetDependents( )
		{
			long coreSolutionId = Entity.GetIdFromUpgradeId( _coreUpgradeId );

			var applicationDependents = SolutionHelper.GetApplicationDependents( coreSolutionId );

			Assert.IsTrue( applicationDependents.Count > 0, "No application dependents found" );
		}
	}
}