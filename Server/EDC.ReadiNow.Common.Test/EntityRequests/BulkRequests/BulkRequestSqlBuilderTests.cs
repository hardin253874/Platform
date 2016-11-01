// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.EntityRequests.BulkRequests
{
    /// <summary>
    ///     Tests.
    /// </summary>
    [TestFixture]
	[RunWithTransaction]
    public class BulkRequestSqlBuilderTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void TwoStringFields()
        {
            var request = "name, description";
            var rqObj = EntityRequestHelper.BuildRequest(request);

            var res = BulkRequestSqlBuilder.BuildSql(rqObj);            

        }

        [Test]
        [RunAsDefaultTenant]
        public void StringFieldAndIntField()
        {
            var request = "name, maxInt";
            var rqObj = EntityRequestHelper.BuildRequest(request);

            var res = BulkRequestSqlBuilder.BuildSql(rqObj);

        }

        [Test]
        [RunAsDefaultTenant]
        public void ForwardRelationship()
        {
            var request = "name, maxInt, isOfType.{name, description}";
            var rqObj = EntityRequestHelper.BuildRequest(request);

            var res = BulkRequestSqlBuilder.BuildSql(rqObj);

        }

        [Test]
        [RunAsDefaultTenant]
        public void ForwardRelationshipIdOnly()
        {
            var request = "isOfType.id";
            var rqObj = EntityRequestHelper.BuildRequest(request);

            var res = BulkRequestSqlBuilder.BuildSql(rqObj);

        }

        [Test]
        [RunAsDefaultTenant]
        public void Chart()
        {
            var request = "name, description, chartTitle, chartReport.name, chartTitleAlign.alias, chartHasSeries.{ "
            + "name, { chartType, stackMethod, markerShape, markerSize, dataLabelPos }.alias,"
            + "chartCustomColor, chartNegativeColor, hideColorLegend, useConditionalFormattingColor,"
            + "{ primaryAxis, valueAxis }.{ name, showGridLines, axisScaleType.alias, axisMinimumValue, axisMaximumValue },"
            + "{primarySource,valueSource,endValueSource,sizeSource,colorSource,imageSource,textSource,symbolSource,associateSource}.{ name, chartReportColumn.id, specialChartSource.alias, sourceAggMethod.alias } }";
            var rqObj = EntityRequestHelper.BuildRequest(request);

            var res = BulkRequestSqlBuilder.BuildSql(rqObj);

        }

    }
}
