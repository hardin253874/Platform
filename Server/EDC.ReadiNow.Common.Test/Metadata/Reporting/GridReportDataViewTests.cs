// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Metadata.Reporting;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Metadata.Reporting
{
	/// <summary>
	///     Tests the GridReportDataView class.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class GridReportDataViewTests
	{
		/// <summary>
		///     Test that a valid report view can be created.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		public void CreateValidGridReportDataViewTest( )
		{
			var dataView = new GridReportDataView( );
			Assert.IsNotNull( dataView, "The data view should not be null" );
			Assert.IsNotNull( dataView.ColumnFormats, "The column formats should not be null" );
		}
	}
}