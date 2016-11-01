// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Metadata.Reporting;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Metadata.Reporting
{
	/// <summary>
	///     Tests the ChartDataView class.
	/// </summary>
	[TestFixture]
	[RunWithTransaction]
	public class ChartDataViewTests
	{
		/// <summary>
		///     Test that a valid chart data view can be created.
		/// </summary>
		[Test]
		[RunAsGlobalTenant]
		public void CreateValidChartDataViewTest( )
		{
			var dataView = new ChartDataView( );
			Assert.IsNotNull( dataView, "The data view should not be null" );
			Assert.IsNotNull( dataView.DependentAxisColumnIds, "The column formats should not be null" );
		}
	}
}