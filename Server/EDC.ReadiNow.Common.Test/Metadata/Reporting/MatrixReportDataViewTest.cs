// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Metadata.Reporting;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Metadata.Reporting
{
	/// <summary>
	///     Tests the MatrixReportDataViewTest class.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class MatrixReportDataViewTest
	{
		/// <summary>
		///     Test that a valid report view can be created.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		public void CreateValidMatrixReportDataViewTest( )
		{
			var dataView = new MatrixReportDataView( );
			Assert.IsNotNull( dataView, "The data view should not be null" );
			Assert.IsNotNull( dataView.RowHeaderColumnIds, "The row header column id collection should not be null" );
			Assert.IsNotNull( dataView.ColumnHeaderColumnIds, "The column header column id collection should not be null" );
			Assert.IsNotNull( dataView.ColumnFormats, "The column formats should not be null" );
		}
	}
}