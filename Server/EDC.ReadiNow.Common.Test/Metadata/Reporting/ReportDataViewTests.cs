// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using EDC.ReadiNow.Metadata.Reporting;
using EDC.ReadiNow.Metadata.Reporting.Formatting;
using EDC.ReadiNow.Model;
using NUnit.Framework;
using FluentAssertions;

namespace EDC.ReadiNow.Test.Metadata.Reporting
{
	/// <summary>
	///     Tests the ReportDataView class.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class ReportDataViewTests
	{
		/// <summary>
		///     Test that a valid report view can be created.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		public void CreateValidReportDataViewTest( )
		{
			var dataView = new ReportDataView( );

		    dataView.Should().NotBeNull("The data view should not be null");
		}

        /// <summary>
        ///     Test that a valid report view can be created.
        /// </summary>
        [Test]
        [RunAsGlobalTenant]
        public void BuildColumnFormatsTest()
        {            
            Report reportToLoad =new Report();
            var dataView = new ReportDataView();
            List<ColumnFormatting> reportFormats = ReadiNow.Metadata.Reporting.Helpers.ReportingEntityHelper.BuildColumnFormats(reportToLoad.ReportColumns, null);

            Assert.AreEqual(reportFormats.Count, 0);

        }
	}
}