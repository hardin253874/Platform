// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
	[TestFixture]
	[RunWithTransaction]
	public class ReportEventTargetTests
	{
		/// <summary>
		///     Tests the report event target with no report structures.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestReportEventTargetNoContent( )
		{
			var report = new Report
				{
					Name = "TestReportEventTargetNullContent"
				};
			report.Save( );

			report = Entity.Get<Report>( report.Id );

			Assert.IsNull( report.ReportUsesDefinition, "ReportUsesDefinition should not be null" );
		}


		/// <summary>
		///     Tests the report event target does not set the report uses
		///     definition if it is already specified.
		/// </summary>
		[Test]
		[RunAsDefaultTenant]
		public void TestReportEventTargetValidQueryXmlNonNullReportUsesDefinition( )
		{
			var reportEntityType = Entity.Get<EntityType>( "core:report" );

			// Ensure the report has the ReportUsesDefinition set.
			var newReport = new Report
				{
					ReportUsesDefinition = reportEntityType
				};
			newReport.Save( );

			newReport = Entity.Get<Report>( newReport.Id );

			Assert.AreEqual( newReport.ReportUsesDefinition.Id, reportEntityType.Id, "The ReportUsesDefinition is invalid." );
		}

	}
}