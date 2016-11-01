// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.Report;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Entity = EDC.ReadiNow.Model.Entity;
using ReportColumn = EDC.SoftwarePlatform.WebApi.Controllers.Report.ReportColumn;

namespace EDC.SoftwarePlatform.WebApi.Test.Report
{
    [TestFixture]
    class ReportControllerTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test002GetUnknownId()
        {
            long reportId = EntityId.Max;
            ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}", reportId), HttpStatusCode.BadRequest);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test003GetNotReport()
        {
            long reportId;
            using (DatabaseContext.GetContext(true))
            {
                reportId = Entity.Get("test:person").Id;
            }

            Assert.IsTrue(reportId > 0);
            ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}", reportId), HttpStatusCode.BadRequest);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test004GetBasicMetadata()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test005GetFullMetadata()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test006PageStart()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=0,2", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 2, "Returned rows that are too many");
            Assert.IsNull(body.Metadata, "Have metadata but have not requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test007PageMiddle()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=2,2", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 2, "Returned rows that are too many");
            Assert.IsNull(body.Metadata, "Have metadata but have not requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test008PageMiddleMaximum()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=1,999", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 3, "Returned rows that are too many");
            Assert.IsNull(body.Metadata, "Have metadata but have not requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test009PageStartBasicMetadata()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=0,2&metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 2, "Returned rows that are too many");
            Assert.IsNotNull(body.Metadata, "Don't have metadata but have requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test010PageMiddleBasicMetadata()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=2,2&metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 2, "Returned rows that are too many");
            Assert.IsNotNull(body.Metadata, "Don't have metadata but have requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test011PageMiddleMaximumBasicMetadata()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=1,999&metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 3, "Returned rows that are too many");
            Assert.IsNotNull(body.Metadata, "Don't have metadata but have requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test012PageStartFullMetadata()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=0,2&metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 2, "Returned rows that are too many");
            Assert.IsNotNull(body.Metadata, "Don't have metadata but have requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test013PageMiddleFullMetadata()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=2,2&metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 2, "Returned rows that are too many");
            Assert.IsNotNull(body.Metadata, "Don't have metadata but have requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test014PageMiddleMaximumFullMetadata()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=1,999&metadata=full", reportId), HttpStatusCode.OK);

            Assert.IsTrue(body.GridData.Count == 3, "Returned rows that are too many");
            Assert.IsNotNull(body.Metadata, "Don't have metadata but have requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test015GetColumnChoke()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?cols=4", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsTrue(body.GridData[0].Values.Count == 4, "Incorrect number of columns returned - returned {0} should be 4", body.GridData[0].Values.Count);
            Assert.IsNull(body.Metadata, "Have metadata but have not requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test016GetBasicMetadataColumnChoke()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic&cols=4", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsTrue(body.Metadata.ReportColumns.Count == 4, "Incorrect number of metadata columns returned - returned {0} should be 4", body.Metadata.ReportColumns.Count);
            Assert.IsTrue(body.GridData[0].Values.Count == 4, "Incorrect number of columns returned - returned {0} should be 4", body.GridData[0].Values.Count);
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test017GetFullMetadataColumnChoke()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full&cols=4", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsTrue(body.Metadata.ReportColumns.Count == 4, "Incorrect number of metadata columns returned - returned {0} should be 4", body.Metadata.ReportColumns.Count);
            Assert.IsTrue(body.GridData[0].Values.Count == 4, "Incorrect number of columns returned - returned {0} should be 4", body.GridData[0].Values.Count);
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test018PageStartColumnChoke()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=0,2&cols=4", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 2, "Returned rows that are too many");
            Assert.IsTrue(body.GridData[0].Values.Count == 4, "Incorrect number of columns returned - returned {0} should be 4", body.GridData[0].Values.Count);
            Assert.IsNull(body.Metadata, "Have metadata but have not requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test019PageMiddleColumnChoke()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=2,2&cols=4", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 2, "Returned rows that are too many");
            Assert.IsTrue(body.GridData[0].Values.Count == 4, "Incorrect number of columns returned - returned {0} should be 4", body.GridData[0].Values.Count);
            Assert.IsNull(body.Metadata, "Have metadata but have not requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test020PageMiddleMaximumColumnChoke()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=1,999&cols=4", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 3, "Returned rows that are too many");
            Assert.IsTrue(body.GridData[0].Values.Count == 4, "Incorrect number of columns returned - returned {0} should be 4", body.GridData[0].Values.Count);
            Assert.IsNull(body.Metadata, "Have metadata but have not requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test021PageStartBasicMetadataColumnChoke()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=0,2&metadata=basic&cols=4", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 2, "Returned rows that are too many");
            Assert.IsTrue(body.Metadata.ReportColumns.Count == 4, "Incorrect number of metadata columns returned - returned {0} should be 4", body.Metadata.ReportColumns.Count);
            Assert.IsTrue(body.GridData[0].Values.Count == 4, "Incorrect number of columns returned - returned {0} should be 4", body.GridData[0].Values.Count);
            Assert.IsNotNull(body.Metadata, "Don't have metadata but have requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test022PageMiddleBasicMetadataColumnChoke()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=2,2&metadata=basic&cols=4", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 2, "Returned rows that are too many");
            Assert.IsTrue(body.Metadata.ReportColumns.Count == 4, "Incorrect number of metadata columns returned - returned {0} should be 4", body.Metadata.ReportColumns.Count);
            Assert.IsTrue(body.GridData[0].Values.Count == 4, "Incorrect number of columns returned - returned {0} should be 4", body.GridData[0].Values.Count);
            Assert.IsNotNull(body.Metadata, "Don't have metadata but have requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test023PageMiddleMaximumBasicMetadataColumnChoke()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=1,999&metadata=basic&cols=4", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 3, "Returned rows that are too many");
            Assert.IsTrue(body.Metadata.ReportColumns.Count == 4, "Incorrect number of metadata columns returned - returned {0} should be 4", body.Metadata.ReportColumns.Count);
            Assert.IsTrue(body.GridData[0].Values.Count == 4, "Incorrect number of columns returned - returned {0} should be 4", body.GridData[0].Values.Count);
            Assert.IsNotNull(body.Metadata, "Don't have metadata but have requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test024PageStartFullMetadataColumnChoke()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=0,2&metadata=full&cols=4", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 2, "Returned rows that are too many");
            Assert.IsTrue(body.Metadata.ReportColumns.Count == 4, "Incorrect number of metadata columns returned - returned {0} should be 4", body.Metadata.ReportColumns.Count);
            Assert.IsTrue(body.GridData[0].Values.Count == 4, "Incorrect number of columns returned - returned {0} should be 4", body.GridData[0].Values.Count);
            Assert.IsNotNull(body.Metadata, "Don't have metadata but have requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test025PageMiddleFullMetadataColumnChoke()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=2,2&metadata=full&cols=4", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 2, "Returned rows that are too many");
            Assert.IsTrue(body.Metadata.ReportColumns.Count == 4, "Incorrect number of metadata columns returned - returned {0} should be 4", body.Metadata.ReportColumns.Count);
            Assert.IsTrue(body.GridData[0].Values.Count == 4, "Incorrect number of columns returned - returned {0} should be 4", body.GridData[0].Values.Count);
            Assert.IsNotNull(body.Metadata, "Don't have metadata but have requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test026PageMiddleMaximumFullMetadataColumnChoke()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?page=1,999&metadata=full&cols=4", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 3, "Returned rows that are too many");
            Assert.IsTrue(body.Metadata.ReportColumns.Count == 4, "Incorrect number of metadata columns returned - returned {0} should be 4", body.Metadata.ReportColumns.Count);
            Assert.IsTrue(body.GridData[0].Values.Count == 4, "Incorrect number of columns returned - returned {0} should be 4", body.GridData[0].Values.Count);
            Assert.IsNotNull(body.Metadata, "Don't have metadata but have requested it");
        }


        /////
        // Sorting
        /////
        [Test]
        [RunAsDefaultTenant]
        public void Test027SortTextField()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");
            Assert.IsNull(body.Metadata.SortOrders, "The report should have no sort order applied");

            // Create a sort order ascending on the text field
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "TextField").Select(rc => rc.Key).FirstOrDefault();
            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            ReportResult sortedReport = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK, new ReportParameters { SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsNotNull(sortedReport.Metadata.SortOrders, "The report should have a sort order applied");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test028SortTextFieldPageStart()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");
            Assert.IsNull(body.Metadata.SortOrders, "The report should have no sort order applied");

            // Create a sort order ascending on the text field
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "TextField").Select(rc => rc.Key).FirstOrDefault();
            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            ReportResult sortedReport = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full&page=0,2", reportId), HttpStatusCode.OK, new ReportParameters { SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(sortedReport.GridData.Count == 2, "Returned rows that are too many");
            Assert.IsNotNull(sortedReport.Metadata.SortOrders, "The report should have a sort order applied");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test029SortTextFieldPageMiddle()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");
            Assert.IsNull(body.Metadata.SortOrders, "The report should have no sort order applied");

            // Create a sort order ascending on the text field
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "TextField").Select(rc => rc.Key).FirstOrDefault();
            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            ReportResult sortedReport = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full&page=2,2", reportId), HttpStatusCode.OK, new ReportParameters { SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(sortedReport.GridData.Count == 2, "Returned rows that are too many");
            Assert.IsNotNull(sortedReport.Metadata.SortOrders, "The report should have a sort order applied");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test030SortTextFieldPageMiddleMaximum()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");
            Assert.IsNull(body.Metadata.SortOrders, "The report should have no sort order applied");

            // Create a sort order ascending on the text field
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "TextField").Select(rc => rc.Key).FirstOrDefault();
            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            ReportResult sortedReport = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full&page=1,999", reportId), HttpStatusCode.OK, new ReportParameters { SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(sortedReport.GridData.Count == 3, "Returned rows that are too many");
            Assert.IsNotNull(sortedReport.Metadata.SortOrders, "The report should have a sort order applied");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test031SortTextFieldBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");
            Assert.IsNull(body.Metadata.SortOrders, "The report should have no sort order applied");

            // Create a sort order ascending on the text field
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "TextField").Select(rc => rc.Key).FirstOrDefault();
            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            ReportResult sortedReport = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, new ReportParameters { SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsNotNull(sortedReport.Metadata.SortOrders, "The report should have a sort order applied");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test032SortTextFieldPageStartBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");
            Assert.IsNull(body.Metadata.SortOrders, "The report should have no sort order applied");

            // Create a sort order ascending on the text field
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "TextField").Select(rc => rc.Key).FirstOrDefault();
            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            ReportResult sortedReport = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic&page=0,2", reportId), HttpStatusCode.OK, new ReportParameters { SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(sortedReport.GridData.Count == 2, "Returned rows that are too many");
            Assert.IsNotNull(sortedReport.Metadata.SortOrders, "The report should have a sort order applied");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test033SortTextFieldPageMiddleBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");
            Assert.IsNull(body.Metadata.SortOrders, "The report should have no sort order applied");

            // Create a sort order ascending on the text field
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "TextField").Select(rc => rc.Key).FirstOrDefault();
            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            ReportResult sortedReport = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic&page=2,2", reportId), HttpStatusCode.OK, new ReportParameters { SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(sortedReport.GridData.Count == 2, "Returned rows that are too many");
            Assert.IsNotNull(sortedReport.Metadata.SortOrders, "The report should have a sort order applied");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test034SortTextFieldPageMiddleMaximumBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");
            Assert.IsNull(body.Metadata.SortOrders, "The report should have no sort order applied");

            // Create a sort order ascending on the text field
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "TextField").Select(rc => rc.Key).FirstOrDefault();
            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            ReportResult sortedReport = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic&page=1,999", reportId), HttpStatusCode.OK, new ReportParameters { SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(sortedReport.GridData.Count == 3, "Returned rows that are too many");
            Assert.IsNotNull(sortedReport.Metadata.SortOrders, "The report should have a sort order applied");
        }


        // Analyser tests
        [Test]
        [RunAsDefaultTenant]
        public void Test038GetAnalyserReport()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.That(body, Has.Property("GridData").Property("Count").Positive, "We have NO All SimpleTypes records");
            Assert.That(body, Has.Property("Metadata").Not.Null, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.That(body.Metadata.AnalyserColumns, Has.Property("Count").EqualTo(10), "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test039GetAnalyserReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test040ApplyAnalyserTextFilterReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "TextField");

            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator, Value = "o", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK, new ReportParameters { AnalyserConditions = analyserConditions });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 3, "Should be 3 rows and not {0}", body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test041ApplyAnalyserTextFilterSortReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "TextField");
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "TextField").Select(rc => rc.Key).FirstOrDefault();

            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator, Value = "o", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, 
                new ReportParameters { AnalyserConditions = analyserConditions, SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 3, "Should be 3 rows and not {0}", body.GridData.Count);
            Assert.IsNotNull(body.Metadata.SortOrders, "The report should have a sort order applied");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test042ApplyAnalyserBoolFilterReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Yes/No Field");

            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = "IsTrue", Value = "True", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, 
                new ReportParameters { AnalyserConditions = analyserConditions });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 1, "Should be 1 rows and not {0}", body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test043ApplyAnalyserBoolFilterSortReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Yes/No Field");
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "Yes/No Field").Select(rc => rc.Key).FirstOrDefault();

            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = "IsTrue", Value = "True", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, 
                new ReportParameters { AnalyserConditions = analyserConditions, SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 1, "Should be 1 rows and not {0}", body.GridData.Count);
            Assert.IsNotNull(body.Metadata.SortOrders, "The report should have a sort order applied");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test044ApplyAnalyserTimeFilterReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Time only Field");

            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator, Value = "1753-01-01T02:00:00.0000000", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, 
                new ReportParameters { AnalyserConditions = analyserConditions });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 2, "Should be 2 rows and not {0}", body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test045ApplyAnalyserTimeFilterSortReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Time only Field");
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "Time only Field").Select(rc => rc.Key).FirstOrDefault();

            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator, Value = "1753-01-01T02:00:00.0000000", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, 
                new ReportParameters { AnalyserConditions = analyserConditions, SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 2, "Should be 2 rows and not {0}", body.GridData.Count);
            Assert.IsNotNull(body.Metadata.SortOrders, "The report should have a sort order applied");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test046ApplyAnalyserDateTimeFilterReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Date and Time Field");

            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator, Value = "2013-09-30T15:00:00.0000000", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, 
                new ReportParameters { AnalyserConditions = analyserConditions });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 3, "Should be 3 rows and not {0}", body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test047ApplyAnalyserDateTimeFilterSortReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Date and Time Field");
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "Date and Time Field").Select(rc => rc.Key).FirstOrDefault();

            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator, Value = "2013-09-30T15:00:00.0000000", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, 
                new ReportParameters { AnalyserConditions = analyserConditions, SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 3, "Should be 3 rows and not {0}", body.GridData.Count);
            Assert.IsNotNull(body.Metadata.SortOrders, "The report should have a sort order applied");
        }


        //
        [Test]
        [RunAsDefaultTenant]
        public void Test048ApplyAnalyserDateFilterReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Date only Field");

            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator, Value = "2013-10-03T00:00:00.0000000", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, 
                new ReportParameters { AnalyserConditions = analyserConditions });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 1, "Should be 1 rows and not {0}", body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test049ApplyAnalyserDateFilterSortReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Date only Field");
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "Date only Field").Select(rc => rc.Key).FirstOrDefault();

            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator, Value = "2013-10-03T00:00:00.0000000", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, 
                new ReportParameters { AnalyserConditions = analyserConditions, SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 1, "Should be 1 rows and not {0}", body.GridData.Count);
            Assert.IsNotNull(body.Metadata.SortOrders, "The report should have a sort order applied");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test050ApplyAnalyserCurrencyFilterReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Currency Field");

            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator, Value = "44.44", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, 
                new ReportParameters { AnalyserConditions = analyserConditions });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            // In the case that there is no data then we should not have any grid data
            Assert.IsNull(body.GridData, "Should be 0 rows (Null grid data)");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test051ApplyAnalyserCurrencyFilterSortReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Currency Field");
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "Currency Field").Select(rc => rc.Key).FirstOrDefault();

            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator, Value = "44.44", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, 
                new ReportParameters { AnalyserConditions = analyserConditions, SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            // In the case that there is no data then we should not have any grid data
            Assert.IsNull(body.GridData, "Should be 0 rows (Null grid data)");
            // We still should have a sort order however
            Assert.IsNotNull(body.Metadata.SortOrders, "The report should have a sort order applied");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test052ApplyAnalyserDecimalFilterReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Decimal Field");

            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator, Value = "300", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, 
                new ReportParameters { AnalyserConditions = analyserConditions });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 2, "Should be 2 rows and not {0}", body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test053ApplyAnalyserDecimalFilterSortReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Decimal Field");
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "Decimal Field").Select(rc => rc.Key).FirstOrDefault();

            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator, Value = "300", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, 
                new ReportParameters { AnalyserConditions = analyserConditions, SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 2, "Should be 2 rows and not {0}", body.GridData.Count);
            Assert.IsNotNull(body.Metadata.SortOrders, "The report should have a sort order applied");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test054ApplyAnalyserMultiFilterReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Multiline Text Field");

            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator, Value = "o", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, 
                new ReportParameters { AnalyserConditions = analyserConditions });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 3, "Should be 3 rows and not {0}", body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test055ApplyAnalyserMultiFilterSortReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Multiline Text Field");
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "Multiline Text Field").Select(rc => rc.Key).FirstOrDefault();

            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator, Value = "o", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, 
                new ReportParameters { AnalyserConditions = analyserConditions, SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 3, "Should be 3 rows and not {0}", body.GridData.Count);
            Assert.IsNotNull(body.Metadata.SortOrders, "The report should have a sort order applied");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test056ApplyAnalyserNumberFilterReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Number Field");

            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator, Value = "1", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, 
                new ReportParameters { AnalyserConditions = analyserConditions });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 3, "Should be 3 rows and not {0}", body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test057ApplyAnalyserNumberFilterSortReportBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Number Field");
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "Number Field").Select(rc => rc.Key).FirstOrDefault();

            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator, Value = "1", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK, 
                new ReportParameters { AnalyserConditions = analyserConditions, SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 3, "Should be 3 rows and not {0}", body.GridData.Count);
            Assert.IsNotNull(body.Metadata.SortOrders, "The report should have a sort order applied");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test058DateTimeUtcConformanceForFirefoxBeingStrictAndChromeNotBeingSoFussy()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");
            // Check we have data returned
            Assert.IsTrue(body.GridData.Count == 4, "Incorrect number of records, expect 4 but got {0}", body.GridData.Count);
            // Check that all of our date values have a 'Z' on the end for the new format.
            ReportColumn column = body.Metadata.ReportColumns.FirstOrDefault(ac => ac.Value.Title == "Date and Time Field").Value;
            int dateTimeValueIndex = Convert.ToInt32(column.Ordinal);
            foreach (string dateTimeString in body.GridData.Select(dataRow => dataRow.Values[dateTimeValueIndex].Value))
            {
                Assert.IsTrue(dateTimeString[dateTimeString.Length -1] == 'Z', "Ensure that the last character is Z for Zulu.");
                DateTime dateTime;
                Assert.IsTrue(DateTime.TryParse(dateTimeString, out dateTime), "Cannot Parse the date time string.");
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test059TestConditionalFormatReturns()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnlSimpleChoiceCond");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
			Assert.IsTrue( body.Metadata.ValueFormatRules.Count > 0 );
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test126ReportForType()
        {
            long entityType;
            long reportId = ReportControllerTestHelper.GetReportByAlias("core:templateReport");
            // We will use person for the report type
            using (DatabaseContext.GetContext(true))
            {
                entityType = Entity.Get("test:person").Id;
            }

            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?type={1}", reportId, entityType), HttpStatusCode.OK);
            Assert.IsNull(body.Metadata, "Have metadata but have not requested it.");
            Assert.NotNull(body.GridData, "Grid data should not be null");
            Assert.IsTrue(body.GridData.Count >= 116, "Incorrect number of rows returned, expected at least 116, have {0}", body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test127ReportForTypeFullMetadata()
        {
            long entityType;
            long reportId = ReportControllerTestHelper.GetReportByAlias("core:templateReport");
            // We will use person for the report type
            using (DatabaseContext.GetContext(true))
            {
                entityType = Entity.Get("test:person").Id;
            }

            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?type={1}&metadata=full", reportId, entityType), HttpStatusCode.OK);
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it.");
            Assert.NotNull(body.GridData, "Grid data should not be null");
            Assert.IsTrue(body.GridData.Count >= 116, "Incorrect number of rows returned, expected at least 116, have {0}", body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test128ReportInvalidTypeFullMetadata()
        {
            const long entityType = 66666666;
            long reportId = ReportControllerTestHelper.GetReportByAlias("core:templateReport");

            ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?type={1}&metadata=full", reportId, entityType), HttpStatusCode.BadRequest);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test129InvalidQueryParameter()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("core:templateReport");

            ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?amIBogus=youbet", reportId), HttpStatusCode.BadRequest);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test130RelationshipReportMetadata()
        {
            long entityInstance;
            long relationshipIdentifier;
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:managerReport");
            // We will use person for the report type
            using (DatabaseContext.GetContext(true))
            {
                entityInstance = Entity.Get("test:aaPeterAylett").Id;
                relationshipIdentifier = Entity.Get("test:directReports").Id;
            }

            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},fwd&metadata=full", reportId, entityInstance, relationshipIdentifier), HttpStatusCode.OK);
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it.");
            Assert.IsNull(body.GridData, "Grid data should be null");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test131RelationshipTemplateReportMetadata()
        {
            long entityInstance;
            long relationshipIdentifier;
            long reportId = ReportControllerTestHelper.GetReportByAlias("core:templateReport");
            // We will use person for the report type
            using (DatabaseContext.GetContext(true))
            {
                entityInstance = Entity.Get("test:aaPeterAylett").Id;
                relationshipIdentifier = Entity.Get("test:directReports").Id;
            }

            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},fwd&metadata=full", reportId, entityInstance, relationshipIdentifier), HttpStatusCode.OK);
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it.");
            Assert.IsNotNull(body.GridData, "Grid data should be null");
            Assert.IsTrue(body.GridData.Count == 6, "Expecting 6 direct reports, have {0}", body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test132RootLevelAggregateReportMetadata()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rootAggregateReport");

            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it.");
            Assert.IsNotNull(body.GridData, "Grid data should be null");
            
            // Metadata
            Assert.AreEqual(2, body.Metadata.ReportColumns.Count, "Expect two metadata columns");
            Assert.AreEqual("Job Title", body.Metadata.ReportColumns.FirstOrDefault(rc => rc.Value.Title == "Job Title").Value.Title, "Job Title");
            Assert.AreEqual(0, body.Metadata.ReportColumns.FirstOrDefault(rc => rc.Value.Ordinal == 0).Value.Ordinal, "Ordinal 0");
            Assert.AreEqual("Average : Age", body.Metadata.ReportColumns.FirstOrDefault(rc => rc.Value.Title == "Average : Age").Value.Title, "Average : Age");
            Assert.AreEqual(1, body.Metadata.ReportColumns.FirstOrDefault(rc => rc.Value.Ordinal == 1).Value.Ordinal, "Ordinal 1");
            
            // Data
            Assert.AreEqual(0, body.GridData[0].EntityId, "No Entity ID");
            Assert.AreEqual(2, body.GridData[0].Values.Count, "Value count 2");
        }

        // test:rpt_Employee
        [Test]
        [RunAsDefaultTenant]
        public void Test134ApplyAnalyserChoiceFilterReport()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_Employee");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Type == "ChoiceRelationship");

            long entityIdentifierSelector = 999999999;
            List<ChoiceItemDefinition> selections = body.Metadata.ChoiceSelections[analyserColumn.Value.TypeId];
            if (selections != null)
            {
                entityIdentifierSelector = selections.First().EntityIdentifier;
            }
            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator,
                                                EntityIdentifiers = new Dictionary<long, string>{{ entityIdentifierSelector, string.Empty }}, Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK, new ReportParameters { AnalyserConditions = analyserConditions });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.GridData.Count == 15, "Should be 15 rows and not {0}", body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test135ApplyAnalyserChoiceFilterSortReport()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_Employee");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Type == "ChoiceRelationship");

            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "Manager").Select(rc => rc.Key).FirstOrDefault();
            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            long entityIdentifierSelector = 999999999;
            List<ChoiceItemDefinition> selections = body.Metadata.ChoiceSelections[analyserColumn.Value.TypeId];
            if (selections != null)
            {
                entityIdentifierSelector = selections.First().EntityIdentifier;
            }
            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator,
                                                EntityIdentifiers = new Dictionary<long, string>{{ entityIdentifierSelector, string.Empty}}, Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK,
                new ReportParameters { AnalyserConditions = analyserConditions, SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.GridData.Count == 15, "Should be 15 rows and not {0}", body.GridData.Count);
            Assert.IsNotNull(body.Metadata.SortOrders, "The report should have a sort order applied");

        }

        [Test]
        [RunAsDefaultTenant]
        public void Test136ApplyAnalyserMultiChoiceFilterReport()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_Employee");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Type == "ChoiceRelationship");

            long firstIdentifierSelector = 999999999;
            long lastIdentifierSelector = 999999999;
            List<ChoiceItemDefinition> selections = body.Metadata.ChoiceSelections[analyserColumn.Value.TypeId];
            if (selections != null)
            {
                firstIdentifierSelector = selections.First().EntityIdentifier;
                lastIdentifierSelector = selections.Last().EntityIdentifier;
            }
            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator,
                                                EntityIdentifiers = new Dictionary<long, string>{{ firstIdentifierSelector, string.Empty}, {lastIdentifierSelector, string.Empty}}, 
                                                Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK, new ReportParameters { AnalyserConditions = analyserConditions });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.GridData.Count == 16, "Should be 16 rows and not {0}", body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test137ApplyAnalyserMultiChoiceFilterSortReport()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_Employee");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Type == "ChoiceRelationship");
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "Manager").Select(rc => rc.Key).FirstOrDefault();
            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");

            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            long firstIdentifierSelector = 999999999;
            long lastIdentifierSelector = 999999999;
            List<ChoiceItemDefinition> selections = body.Metadata.ChoiceSelections[analyserColumn.Value.TypeId];
            if (selections != null)
            {
                firstIdentifierSelector = selections.First().EntityIdentifier;
                lastIdentifierSelector = selections.Last().EntityIdentifier;
            }
            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator,
                                                EntityIdentifiers = new Dictionary<long, string>{{ firstIdentifierSelector, string.Empty}, {lastIdentifierSelector, string.Empty}}, 
                                                Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK,
                new ReportParameters { AnalyserConditions = analyserConditions, SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.GridData.Count == 16, "Should be 16 rows and not {0}", body.GridData.Count);
            Assert.IsNotNull(body.Metadata.SortOrders, "The report should have a sort order applied");

        }

        [Test]
        [RunAsDefaultTenant]
        public void Test138ApplyAnalyserInlineFilterReport()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_Employee");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Type == "InlineRelationship");
            // Extract the report id and type and get the report
            Assert.IsNotNull(body.Metadata.InlineReportPickers, "Inline picker reports should not be null");
            long pickerReport = body.Metadata.InlineReportPickers[analyserColumn.Value.TypeId];
            long pickerReportType = analyserColumn.Value.TypeId;
            ReportResult pickerReportBody = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?type={1}", pickerReport, pickerReportType), HttpStatusCode.OK);
            // Grab a manager identifier
            long manager = pickerReportBody.GridData.First(c => c.Values.Any(v => v.Value == "Jude Jacobs")).EntityId;

            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator,
                                                EntityIdentifiers = new Dictionary<long, string>{{ manager, string.Empty}}, Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK, new ReportParameters { AnalyserConditions = analyserConditions });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.GridData.Count == 12, "Should be 12 rows and not {0}", body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test139ApplyAnalyserInlineFilterSortReport()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_Employee");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Type == "InlineRelationship");

            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "Manager").Select(rc => rc.Key).FirstOrDefault();
            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            // Extract the report id and type and get the report
            Assert.IsNotNull(body.Metadata.InlineReportPickers, "Inline picker reports should not be null");
            long pickerReport = body.Metadata.InlineReportPickers[analyserColumn.Value.TypeId];
            long pickerReportType = analyserColumn.Value.TypeId;
            ReportResult pickerReportBody = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?type={1}", pickerReport, pickerReportType), HttpStatusCode.OK);
            // Grab a manager identifier
            long manager = pickerReportBody.GridData.First(c => c.Values.Any(v => v.Value == "Jude Jacobs")).EntityId;

            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator,
                                                EntityIdentifiers = new Dictionary<long, string>{{ manager, string.Empty}}, Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK,
                new ReportParameters { AnalyserConditions = analyserConditions, SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.GridData.Count == 12, "Should be 12 rows and not {0}", body.GridData.Count);
            Assert.IsNotNull(body.Metadata.SortOrders, "The report should have a sort order applied");

        }

        [Test]
        [RunAsDefaultTenant]
        public void Test140ApplyAnalyserMultiInlineFilterReport()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_Employee");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Type == "InlineRelationship");
            // Extract the report id and type and get the report
            Assert.IsNotNull(body.Metadata.InlineReportPickers, "Inline picker reports should not be null");
            long pickerReport = body.Metadata.InlineReportPickers[analyserColumn.Value.TypeId];
            long pickerReportType = analyserColumn.Value.TypeId;
            ReportResult pickerReportBody = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?type={1}", pickerReport, pickerReportType), HttpStatusCode.OK);
            // Grab a manager identifiers
            long manager = pickerReportBody.GridData.First(c => c.Values.Any(v => v.Value == "Jude Jacobs")).EntityId;
            long subManager = pickerReportBody.GridData.First(c => c.Values.Any(v => v.Value == "Scott Hopwood")).EntityId;

            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator,
                                                EntityIdentifiers = new Dictionary<long, string>{{ manager, string.Empty}, {subManager, string.Empty}}, 
                                                Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK, new ReportParameters { AnalyserConditions = analyserConditions });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.GridData.Count == 14, "Should be 14 rows and not {0}", body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test141ApplyAnalyserMultiInlineFilterSortReport()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_Employee");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Type == "InlineRelationship");
            string reportSortColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "Manager").Select(rc => rc.Key).FirstOrDefault();
            Assert.IsNotNull(reportSortColumn, "Cannot find the report text field column");
            // Extract the report id and type and get the report
            Assert.IsNotNull(body.Metadata.InlineReportPickers, "Inline picker reports should not be null");
            long pickerReport = body.Metadata.InlineReportPickers[analyserColumn.Value.TypeId];
            long pickerReportType = analyserColumn.Value.TypeId;
            ReportResult pickerReportBody = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?type={1}", pickerReport, pickerReportType), HttpStatusCode.OK);
            // Grab a manager identifiers
            long manager = pickerReportBody.GridData.First(c => c.Values.Any(v => v.Value == "Jude Jacobs")).EntityId;
            long subManager = pickerReportBody.GridData.First(c => c.Values.Any(v => v.Value == "Scott Hopwood")).EntityId;

            List<ReportSortOrder> singleColumnSort = new List<ReportSortOrder>
                {
                    new ReportSortOrder {ColumnId = reportSortColumn, Order = "Ascending"}
                };
            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = analyserColumn.Value.DefaultOperator,
                                                EntityIdentifiers = new Dictionary<long, string>{{ manager, string.Empty}, {subManager, string.Empty}}, 
                                                Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK,
                new ReportParameters { AnalyserConditions = analyserConditions, SortColumns = singleColumnSort });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.GridData.Count == 14, "Should be 14 rows and not {0}", body.GridData.Count);
            Assert.IsNotNull(body.Metadata.SortOrders, "The report should have a sort order applied");

        }

        [Test]
        [RunAsDefaultTenant]
        public void Test150ValidateChartReportInlineRelationshipReturnsCorrectStuff()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_CHART_EmployeeManager");

            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count == 6, "Should have 6 rows but have {0}", body.GridData.Count);
            // Check that the first column is either empty _or_ is a dictionary keyed by entity Identifier
            foreach (DataRow dataRow in body.GridData)
            {
                Assert.IsNull(dataRow.Values[0].Value, "This column data should be a key value pair, but found {0}", dataRow.Values[0].Value);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test196TestReportWithName()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_MultiChoice");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO records");
            Assert.IsNotNull(body.Metadata, "Have no metadata");
            Assert.IsTrue(body.Metadata.ReportColumns.Any(col => col.Value.EntityNameField));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test197TestReportWithNoName()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_CalculatedRelationsNoName");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO records");
            Assert.IsNotNull(body.Metadata, "Have no metadata");
            Assert.IsTrue(body.Metadata.ReportColumns.All(col => col.Value.EntityNameField == false));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test247ApplyAnalyserInlineFilter()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_Employee");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            string columnKey = body.Metadata.AnalyserColumns.Where(rc => rc.Value.Title == "Manager").Select(rc => rc.Key).FirstOrDefault();
            ReportColumn reportColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "Manager").Select(rc => rc.Value).FirstOrDefault();
            Assert.IsNotNull(reportColumn);
            // Get the report for type
            long ourType = reportColumn.TypeId;
            long reportForTypeId = body.Metadata.InlineReportPickers[ourType];
            ReportResult pickerBody = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?type={1}", reportForTypeId, ourType), HttpStatusCode.OK);
            long managerVictim = (from dataRow in pickerBody.GridData from cellValue in dataRow.Values where cellValue.Value == "Jude Jacobs" select dataRow.EntityId).FirstOrDefault();
            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = columnKey, Operator = "AnyOf",
                                                EntityIdentifiers = new Dictionary<long, string>{{ managerVictim, string.Empty }}, Type = "InlineRelationship"}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK,
                new ReportParameters { AnalyserConditions = analyserConditions });
            Assert.IsTrue(body.GridData.Count == 12, "Should be 12 rows and not {0}", body.GridData.Count);
        }
        [Test]
        [RunAsDefaultTenant]
        public void Test248ApplyAnalyserMultiInlineFilter()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_Employee");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            string columnKey = body.Metadata.AnalyserColumns.Where(rc => rc.Value.Title == "Manager").Select(rc => rc.Key).FirstOrDefault();
            ReportColumn reportColumn = body.Metadata.ReportColumns.Where(rc => rc.Value.Title == "Manager").Select(rc => rc.Value).FirstOrDefault();
            Assert.IsNotNull(reportColumn);
            // Get the report for type
            long ourType = reportColumn.TypeId;
            long reportForTypeId = body.Metadata.InlineReportPickers[ourType];
            ReportResult pickerBody = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?type={1}", reportForTypeId, ourType), HttpStatusCode.OK);
            long managerVictim = (from dataRow in pickerBody.GridData from cellValue in dataRow.Values where cellValue.Value == "Jude Jacobs" select dataRow.EntityId).FirstOrDefault();
            long managerVictim2 = (from dataRow in pickerBody.GridData from cellValue in dataRow.Values where cellValue.Value == "Scott Hopwood" select dataRow.EntityId).FirstOrDefault();
            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = columnKey, Operator = "AnyOf",
                                                EntityIdentifiers = new Dictionary<long, string>{{ managerVictim, string.Empty }, {managerVictim2, string.Empty }}, Type = "InlineRelationship"}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK,
                new ReportParameters { AnalyserConditions = analyserConditions });
            Assert.IsTrue(body.GridData.Count == 14, "Should be 14 rows and not {0}", body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test251AnalyserChoiceCalculatedInlineTypeBasic()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_CalculatedRelations");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have no records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            // Check that there is no inline stuff
            Assert.IsNotNull(body.Metadata.InlineReportPickers, "Have no inline pickers but should");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test252AnalyserChoiceCalculatedInlineTypeFull()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_CalculatedRelations");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have no records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            Assert.IsNotNull(body.Metadata.InlineReportPickers, "Have no inline pickers but should");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test253AnalyserChoiceCalculatedInlineTypeBasicMetadataOnly()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_CalculatedRelations");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic&page=0,0", reportId), HttpStatusCode.OK);
            Assert.IsNull(body.GridData, "We should have no records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            Assert.IsNotNull(body.Metadata.InlineReportPickers, "Have no inline pickers but should");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test254AnalyserChoiceCalculatedInlineTypeFullMetadataOnly()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_CalculatedRelations");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full&page=0,0", reportId), HttpStatusCode.OK);
            Assert.IsNull(body.GridData, "We should have no records");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            Assert.IsNotNull(body.Metadata.InlineReportPickers, "Have no inline pickers but should");
        }
        [Test]
        [RunAsDefaultTenant]
        public void Test263TestRelationshipMetadataReport()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("core:templateReport");
            long relationshipId = ReportControllerTestHelper.GetEntityIdByAlias("test:directReports");
            long entityId = ReportControllerTestHelper.GetEntityIdByAlias("test:aaMitchellMurray");
            const string direction = "fwd";
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},{3}&metadata=full",
                                    reportId, entityId, relationshipId, direction), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO data for the report");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test264TestRelationshipNoMetadataReport()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("core:templateReport");
            long relationshipId = ReportControllerTestHelper.GetEntityIdByAlias("test:directReports");
            long entityId = ReportControllerTestHelper.GetEntityIdByAlias("test:aaMitchellMurray");
            const string direction = "fwd";
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},{3}",
                                    reportId, entityId, relationshipId, direction), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO data for the report");
            Assert.IsNull(body.Metadata, "Should have no metadata but have requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test265TestRelationshipMetadataIncludesReport()
        {
            // Run the report with the new injected entity
            long reportId = ReportControllerTestHelper.GetReportByAlias("core:templateReport");
            long relationshipId = ReportControllerTestHelper.GetEntityIdByAlias("test:directReports");
            long entityId = ReportControllerTestHelper.GetEntityIdByAlias("test:aaMitchellMurray");
            const string direction = "fwd";
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},{3}&metadata=full",
                                    reportId, entityId, relationshipId, direction), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO data for the report");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            int currentReportRows = body.GridData.Count + 1;

            // Run the report picker and grab a new entity to inject
            long pickerReportId = ReportControllerTestHelper.GetReportByAlias("core:resourceReport");
            long pickerTypeId = ReportControllerTestHelper.GetEntityIdByAlias("test:employee");
            ReportResult pickerBody = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?type={1}", pickerReportId, pickerTypeId), HttpStatusCode.OK);
            long employeeVictim = (from dataRow in pickerBody.GridData from cellValue in dataRow.Values where cellValue.Value == "Karen Jones" select dataRow.EntityId).FirstOrDefault();

            // Run the report again and check that we have one more row
            ReportParameters reportParameters = new ReportParameters { RelationshipEntities = new RelationshipDetail { IncludedEntityIdentifiers = new List<long> { employeeVictim } } };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},{3}&metadata=full",
                                    reportId, entityId, relationshipId, direction), HttpStatusCode.OK, reportParameters);
            Assert.AreEqual(currentReportRows, body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test266TestRelationshipMetadataExcludesReport()
        {
            // Run the report with the new injected entity
            long reportId = ReportControllerTestHelper.GetReportByAlias("core:templateReport");
            long relationshipId = ReportControllerTestHelper.GetEntityIdByAlias("test:directReports");
            long entityId = ReportControllerTestHelper.GetEntityIdByAlias("test:aaMitchellMurray");
            const string direction = "fwd";
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},{3}&metadata=full",
                                    reportId, entityId, relationshipId, direction), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO data for the report");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            int currentReportRows = body.GridData.Count - 1;

            // Run the report picker and grab a new entity to inject
            long pickerReportId = ReportControllerTestHelper.GetReportByAlias("core:resourceReport");
            long pickerTypeId = ReportControllerTestHelper.GetEntityIdByAlias("test:employee");
            ReportResult pickerBody = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?type={1}", pickerReportId, pickerTypeId), HttpStatusCode.OK);
            long employeeVictim = (from dataRow in pickerBody.GridData from cellValue in dataRow.Values where cellValue.Value == "Michelle Smith" select dataRow.EntityId).FirstOrDefault();

            // Run the report again and check that we have one more row
            ReportParameters reportParameters = new ReportParameters { RelationshipEntities = new RelationshipDetail { ExcludedEntityIdentifiers = new List<long> { employeeVictim } } };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},{3}&metadata=full",
                                    reportId, entityId, relationshipId, direction), HttpStatusCode.OK, reportParameters);
            Assert.AreEqual(currentReportRows, body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test267TestRelationshipMetadataExcludesIncludesReport()
        {
            // Run the report with the new injected entity
            long reportId = ReportControllerTestHelper.GetReportByAlias("core:templateReport");
            long relationshipId = ReportControllerTestHelper.GetEntityIdByAlias("test:directReports");
            long entityId = ReportControllerTestHelper.GetEntityIdByAlias("test:aaMitchellMurray");
            const string direction = "fwd";
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},{3}&metadata=full",
                                    reportId, entityId, relationshipId, direction), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO data for the report");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            int currentReportRows = body.GridData.Count;

            // Run the report picker and grab a new entity to inject
            long pickerReportId = ReportControllerTestHelper.GetReportByAlias("core:resourceReport");
            long pickerTypeId = ReportControllerTestHelper.GetEntityIdByAlias("test:employee");
            ReportResult pickerBody = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?type={1}", pickerReportId, pickerTypeId), HttpStatusCode.OK);
            long employeeVictim = (from dataRow in pickerBody.GridData from cellValue in dataRow.Values where cellValue.Value == "Karen Jones" select dataRow.EntityId).FirstOrDefault();
            long excludedEmployeeVictim = (from dataRow in pickerBody.GridData from cellValue in dataRow.Values where cellValue.Value == "Michelle Smith" select dataRow.EntityId).FirstOrDefault();

            // Run the report again and check that we have one more row
            ReportParameters reportParameters = new ReportParameters { RelationshipEntities = new RelationshipDetail { IncludedEntityIdentifiers = new List<long> { employeeVictim }, ExcludedEntityIdentifiers = new List<long>{excludedEmployeeVictim}} };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},{3}&metadata=full",
                                    reportId, entityId, relationshipId, direction), HttpStatusCode.OK, reportParameters);
            Assert.AreEqual(currentReportRows, body.GridData.Count);
        }

        //
        [Test]
        [RunAsDefaultTenant]
        public void Test268TestRelationshipNewEntityMetadataReport()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("core:templateReport");
            long relationshipId = ReportControllerTestHelper.GetEntityIdByAlias("test:directReports");
            long entityId = EntityId.Max - 1;
            const string direction = "fwd";
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},{3}&metadata=full",
                                    reportId, entityId, relationshipId, direction), HttpStatusCode.OK);
            Assert.IsNull(body.GridData, "We have data for the report");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test269TestRelationshipNewEntityNoMetadataReport()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("core:templateReport");
            long relationshipId = ReportControllerTestHelper.GetEntityIdByAlias("test:directReports");
            long entityId = EntityId.Max - 1;
            const string direction = "fwd";
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},{3}",
                                    reportId, entityId, relationshipId, direction), HttpStatusCode.OK);
            Assert.IsNull(body.GridData, "We have data for the report");
            Assert.IsNull(body.Metadata, "Should have no metadata but have requested it");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test270TestRelationshipNewEntityMetadataIncludesReport()
        {
            // Run the report with the new injected entity
            long reportId = ReportControllerTestHelper.GetReportByAlias("core:templateReport");
            long relationshipId = ReportControllerTestHelper.GetEntityIdByAlias("test:directReports");
            long entityId = EntityId.Max - 1;
            const string direction = "rev";
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},{3}&metadata=full",
                                    reportId, entityId, relationshipId, direction), HttpStatusCode.OK);
            Assert.IsNull(body.GridData, "We have data for the report");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Run the report picker and grab a new entity to inject
            long pickerReportId = ReportControllerTestHelper.GetReportByAlias("core:resourceReport");
            long pickerTypeId = ReportControllerTestHelper.GetEntityIdByAlias("test:employee");
            ReportResult pickerBody = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?type={1}", pickerReportId, pickerTypeId), HttpStatusCode.OK);
            long employeeVictim = (from dataRow in pickerBody.GridData from cellValue in dataRow.Values where cellValue.Value == "Karen Jones" select dataRow.EntityId).FirstOrDefault();

            entityId = ReportControllerTestHelper.GetEntityIdByAlias("test:aaJudeJacobs");
            // Run the report again and check that we have one more row
            ReportParameters reportParameters = new ReportParameters { RelationshipEntities = new RelationshipDetail { IncludedEntityIdentifiers = new List<long> { employeeVictim } } };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},{3}&metadata=full",
                                    reportId, entityId, relationshipId, direction), HttpStatusCode.OK, reportParameters);
            Assert.AreEqual(1, body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test271TestRelationshipNewEntityMetadataExcludesReport()
        {
            // Run the report with the new injected entity
            long reportId = ReportControllerTestHelper.GetReportByAlias("core:templateReport");
            long relationshipId = ReportControllerTestHelper.GetEntityIdByAlias("test:directReports");
            long entityId = EntityId.Max - 1;
            const string direction = "fwd";
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},{3}&metadata=full",
                                    reportId, entityId, relationshipId, direction), HttpStatusCode.OK);
            Assert.IsNull(body.GridData, "We have data for the report");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Run the report picker and grab a new entity to inject
            long pickerReportId = ReportControllerTestHelper.GetReportByAlias("core:resourceReport");
            long pickerTypeId = ReportControllerTestHelper.GetEntityIdByAlias("test:employee");
            ReportResult pickerBody = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?type={1}", pickerReportId, pickerTypeId), HttpStatusCode.OK);
            long employeeVictim = (from dataRow in pickerBody.GridData from cellValue in dataRow.Values where cellValue.Value == "Michelle Smith" select dataRow.EntityId).FirstOrDefault();

            // Run the report again and check that we have one more row
            ReportParameters reportParameters = new ReportParameters { RelationshipEntities = new RelationshipDetail { ExcludedEntityIdentifiers = new List<long> { employeeVictim } } };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},{3}&metadata=full",
                                    reportId, entityId, relationshipId, direction), HttpStatusCode.OK, reportParameters);
            Assert.IsNull(body.GridData, "We have data for the report");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test272TestRelationshipNewEntityMetadataExcludesIncludesReport()
        {
            // Run the report with the new injected entity
            long reportId = ReportControllerTestHelper.GetReportByAlias("core:templateReport");
            long relationshipId = ReportControllerTestHelper.GetEntityIdByAlias("test:directReports");
            long entityId = EntityId.Max - 1;
            const string direction = "rev";
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},{3}&metadata=full",
                                    reportId, entityId, relationshipId, direction), HttpStatusCode.OK);
            Assert.IsNull(body.GridData, "We have data for the report");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // Run the report picker and grab a new entity to inject
            long pickerReportId = ReportControllerTestHelper.GetReportByAlias("core:resourceReport");
            long pickerTypeId = ReportControllerTestHelper.GetEntityIdByAlias("test:employee");
            ReportResult pickerBody = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?type={1}", pickerReportId, pickerTypeId), HttpStatusCode.OK);
            long employeeVictim = (from dataRow in pickerBody.GridData from cellValue in dataRow.Values where cellValue.Value == "Karen Jones" select dataRow.EntityId).FirstOrDefault();
            long excludedEmployeeVictim = (from dataRow in pickerBody.GridData from cellValue in dataRow.Values where cellValue.Value == "Michelle Smith" select dataRow.EntityId).FirstOrDefault();

            entityId = ReportControllerTestHelper.GetEntityIdByAlias("test:aaJudeJacobs");

            // Run the report again and check that we have one more row
            ReportParameters reportParameters = new ReportParameters { RelationshipEntities = new RelationshipDetail { IncludedEntityIdentifiers = new List<long> { employeeVictim }, ExcludedEntityIdentifiers = new List<long> { excludedEmployeeVictim } } };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?relationship={1},{2},{3}&metadata=full",
                                    reportId, entityId, relationshipId, direction), HttpStatusCode.OK, reportParameters);
            Assert.AreEqual(1, body.GridData.Count);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test273ReportForImages()
        {
            // Run the report with the new injected entity
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_SingleMalt");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full",reportId), HttpStatusCode.OK);
            Assert.IsNotNull(body.GridData, "We have no data for the report");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
			Assert.IsTrue( body.Metadata.ValueFormatRules.Count > 0 );
        }


        [Test]
        [RunAsDefaultTenant]
        public void Test275ApplyAnalyserUnspecifiedFilterReport()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_Employee");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            long originalCount = body.GridData.Count;
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Type == "InlineRelationship");
            // Extract the report id and type and get the report
            Assert.IsNotNull(body.Metadata.InlineReportPickers, "Inline picker reports should not be null");
            long pickerReport = body.Metadata.InlineReportPickers[analyserColumn.Value.TypeId];
            long pickerReportType = analyserColumn.Value.TypeId;
            ReportResult pickerReportBody = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?type={1}", pickerReport, pickerReportType), HttpStatusCode.OK);
            // Grab a manager identifier
            long manager = pickerReportBody.GridData.First(c => c.Values.Any(v => v.Value == "Jude Jacobs")).EntityId;

            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = "Unspecified",
                                                EntityIdentifiers = new Dictionary<long, string>{{manager, string.Empty}}, Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), 
                HttpStatusCode.OK, new ReportParameters { AnalyserConditions = analyserConditions });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.GridData.Count == originalCount, "Should be {0} rows and not {1}", originalCount, body.GridData.Count);
            Assert.IsTrue(body.Metadata.AnalyserColumns[analyserColumn.Key].Operator == "Unspecified");
        }

        // Value formatting tests
        [Test]
        [RunAsDefaultTenant]
        public void Test276TestSetValueRuleNumberPrefix()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalSimpleChoice");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            // Set some conditional formatting for a prefix
            string numberColumnId = body.Metadata.ReportColumns.First(rc => rc.Value.Title == "Number Field").Key;
            ReportColumnValueFormat valueFormat = new ReportColumnValueFormat { Prefix = "My Number Prefix " };
            ReportParameters parameters = new ReportParameters
            {
                ValueFormatRules = new Dictionary<string, ReportColumnValueFormat> { { numberColumnId, valueFormat } }
            };
            // Send it up and check the result
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK, parameters);
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            Assert.IsNotNull(body.Metadata.ValueFormatRules, "Should have value rules returned");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test279ApplySingleGroupBy()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_Drivers");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            Assert.IsNull(body.Metadata.AggregateMetadata);
            Assert.IsNull(body.Metadata.AggregateData);

            // Get the nationality column id
            string columnStringId = body.Metadata.ReportColumns.First(rc => rc.Value.Title == "Nationality").Key;
            long columnId = Convert.ToInt64(columnStringId);
            // Send it up and check the result
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK, new ReportParameters
            {
                GroupAggregateRules = new ReportMetadataAggregate
                {
                    IncludeRollup = true,
                    ShowCount = true,
                    ShowGrandTotals = true,
                    ShowSubTotals = true,
                    ShowGroupLabel = true,
                    ShowOptionLabel = true,
                    Groups = new List<Dictionary<long, GroupingDetail>>
                        {
                            new Dictionary<long, GroupingDetail>
                              {
                                  {columnId, new GroupingDetail{Style = "groupList"}}
                              }
                        }
                }
            });
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            Assert.IsNotNull(body.Metadata.AggregateData);
            Assert.AreEqual(body.Metadata.AggregateData.Count, 11);
            Assert.IsNotNull(body.Metadata.AggregateMetadata);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowGrandTotals);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowSubTotals);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowCount);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowGroupLabel);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowOptionLabel);
            Assert.IsNotNull( body.Metadata.AggregateMetadata.Aggregates);
            Assert.IsNotNull(body.Metadata.AggregateMetadata.Groups);
            Assert.AreEqual(body.Metadata.AggregateMetadata.Groups.Count, 1);
            Assert.AreEqual(body.Metadata.AggregateMetadata.Groups.First().First().Value.Value, "Nationality");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test280ApplyMultipleGroupBy()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_Drivers");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            Assert.IsNull(body.Metadata.AggregateMetadata);
            Assert.IsNull(body.Metadata.AggregateData);

            // Get the nationality column id
            string columnNationalityStringId = body.Metadata.ReportColumns.First(rc => rc.Value.Title == "Nationality").Key;
            string columnLanguageStringId = body.Metadata.ReportColumns.First(rc => rc.Value.Title == "List : Language").Key;
            long columnNationalityId = Convert.ToInt64(columnNationalityStringId);
            long columnLanguageId = Convert.ToInt64(columnLanguageStringId);
            // Send it up and check the result
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK, new ReportParameters
            {
                GroupAggregateRules = new ReportMetadataAggregate
                {
                    IncludeRollup = true,
                    ShowCount = true,
                    ShowGrandTotals = true,
                    ShowSubTotals = true,
                    ShowGroupLabel = true,
                    ShowOptionLabel = true,
                    Groups = new List<Dictionary<long, GroupingDetail>>
                        {
                            new Dictionary<long, GroupingDetail>
                                {
                                    {columnNationalityId, new GroupingDetail{Style = "groupList"}}
                                },
                            new Dictionary<long, GroupingDetail>
                                {
                                    {columnLanguageId, new GroupingDetail{Style = "groupList"}}
                                }
                        }
                }
            });
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            Assert.IsNotNull(body.Metadata.AggregateData);
            Assert.AreEqual(body.Metadata.AggregateData.Count, 50);
            Assert.IsNotNull(body.Metadata.AggregateMetadata);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowGrandTotals);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowSubTotals);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowCount);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowGroupLabel);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowOptionLabel);
            Assert.IsNotNull(body.Metadata.AggregateMetadata.Aggregates);
            Assert.IsNotNull(body.Metadata.AggregateMetadata.Groups);
            Assert.AreEqual(body.Metadata.AggregateMetadata.Groups.Count, 2);
            Assert.AreEqual(body.Metadata.AggregateMetadata.Groups.First().First().Value.Value, "Nationality");
            Assert.AreEqual(body.Metadata.AggregateMetadata.Groups.Last().First().Value.Value, "List : Language");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test281ApplySingleAggregate()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_Drivers");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            Assert.IsNull(body.Metadata.AggregateMetadata);
            Assert.IsNull(body.Metadata.AggregateData);

            // Get the nationality column id
            string columnStringId = body.Metadata.ReportColumns.First(rc => rc.Value.Title == "Nationality").Key;
            long columnId = Convert.ToInt64(columnStringId);
            // Send it up and check the result
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK, new ReportParameters
            {
                GroupAggregateRules = new ReportMetadataAggregate
                {
                    IncludeRollup = true,
                    ShowCount = true,
                    ShowGrandTotals = true,
                    ShowSubTotals = true,
                    ShowGroupLabel = true,
                    ShowOptionLabel = true,
                    Aggregates = new Dictionary<long, List<AggregateDetail>>
                        {
                            {columnId, new List<AggregateDetail>{new AggregateDetail{Style = "aggCount"}}}
                        }
                }
            });
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            Assert.IsNotNull(body.Metadata.AggregateData);
            Assert.AreEqual(body.Metadata.AggregateData.Count, 1);
            Assert.IsNotNull(body.Metadata.AggregateMetadata);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowGrandTotals);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowSubTotals);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowCount);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowGroupLabel);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowOptionLabel);
            Assert.IsNull(body.Metadata.AggregateMetadata.Groups);
            Assert.IsNotNull(body.Metadata.AggregateMetadata.Aggregates);
            Assert.AreEqual(body.Metadata.AggregateMetadata.Aggregates.Count, 1);
            Assert.AreEqual(body.Metadata.AggregateMetadata.Aggregates.First().Value.First().Style, "aggCount");
            Assert.AreEqual(body.Metadata.AggregateMetadata.Aggregates.First().Value.First().Type, "Int32");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test282ApplyMultipleAggregate()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_Drivers");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            Assert.IsNull(body.Metadata.AggregateMetadata);
            Assert.IsNull(body.Metadata.AggregateData);

            // Get the nationality column id
            string columnNationalityStringId = body.Metadata.ReportColumns.First(rc => rc.Value.Title == "Nationality").Key;
            long columnNationalityId = Convert.ToInt64(columnNationalityStringId);
            string columnLanguageStringId = body.Metadata.ReportColumns.First(rc => rc.Value.Title == "List : Language").Key;
            long columnLanguageId = Convert.ToInt64(columnLanguageStringId);
            // Send it up and check the result
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK, new ReportParameters
            {
                GroupAggregateRules = new ReportMetadataAggregate
                {
                    IncludeRollup = true,
                    ShowCount = true,
                    ShowGrandTotals = true,
                    ShowSubTotals = true,
                    ShowGroupLabel = true,
                    ShowOptionLabel = true,
                    Aggregates = new Dictionary<long, List<AggregateDetail>>
                        {
                            {columnNationalityId, new List<AggregateDetail>{new AggregateDetail{Style = "aggCount"}}},
                            {columnLanguageId, new List<AggregateDetail>{new AggregateDetail{Style = "aggCount"}}}
                        }
                }
            });
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");
            Assert.IsNotNull(body.Metadata.AggregateData);
            Assert.AreEqual(body.Metadata.AggregateData.Count, 1);
            Assert.AreEqual(body.Metadata.AggregateData.First().Aggregates.Count, 2);
            Assert.IsNotNull(body.Metadata.AggregateMetadata);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowGrandTotals);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowSubTotals);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowCount);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowGroupLabel);
            Assert.IsTrue(body.Metadata.AggregateMetadata.ShowOptionLabel);
            Assert.IsNull(body.Metadata.AggregateMetadata.Groups);
            Assert.IsNotNull(body.Metadata.AggregateMetadata.Aggregates);
            Assert.AreEqual(body.Metadata.AggregateMetadata.Aggregates.Count, 2);
            Assert.AreEqual(body.Metadata.AggregateMetadata.Aggregates.First().Key, columnNationalityId);
            Assert.AreEqual(body.Metadata.AggregateMetadata.Aggregates.First().Value.First().Style, "aggCount");
            Assert.AreEqual(body.Metadata.AggregateMetadata.Aggregates.First().Value.First().Type, "Int32");
            Assert.AreEqual(body.Metadata.AggregateMetadata.Aggregates.Last().Key, columnLanguageId);
            Assert.AreEqual(body.Metadata.AggregateMetadata.Aggregates.Last().Value.First().Style, "aggCount");
            Assert.AreEqual(body.Metadata.AggregateMetadata.Aggregates.Last().Value.First().Type, "Int32");
        }


        [Test]
        [Ignore]
        [RunAsDefaultTenant]
        public void Test287ApplyNdaysForDate()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 3, "Should be 3 rows and not {0}", body.GridData.Count);
            KeyValuePair<string, ReportAnalyserColumn> analyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Date and Time Field");

            List<AnalyserColumnCondition> analyserConditions = new List<AnalyserColumnCondition>
                {
                    new AnalyserColumnCondition{ExpressionId = analyserColumn.Key, Operator = "LastNDays", Value = "350", Type = analyserColumn.Value.Type}
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=basic", reportId), HttpStatusCode.OK,
                new ReportParameters { AnalyserConditions = analyserConditions });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 3, "Should be 3 rows and not {0}", body.GridData.Count);

            KeyValuePair<string, ReportAnalyserColumn> newAnalyserColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "Date and Time Field");
            Assert.IsTrue(newAnalyserColumn.Value.Operator == "LastNDays");
            Assert.IsTrue(newAnalyserColumn.Value.Value == "350");
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestQuickSearch()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO All SimpleTypes records");
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");

            // Create a sort order ascending on the text field
            ReportResult filteredReport = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK, new ReportParameters { QuickSearch = "Two" });
            // Check we have a sort order returned in the metadata
            Assert.IsTrue(filteredReport.GridData.Count == 1, "The report should have a single row");
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestQuickSearchLookup()
        {
            var reports = Entity.GetByName("RPT_A_Lookup");

            Assert.AreEqual(1, reports.Count());

            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reports.First().Id), HttpStatusCode.OK);
            Assert.IsTrue(body.GridData.Count > 0, "We have NO RPT_A_Lookup records");
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");

            string searchValue = "Forward 2";
            ReportResult filteredReport = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reports.First().Id), HttpStatusCode.OK, new ReportParameters { QuickSearch = searchValue });
            Assert.IsTrue(filteredReport.GridData.Count == 1, "The report should have a single row");

            Assert.AreEqual(searchValue, filteredReport.GridData[0].Values[1].Values.Values.First());
        }

        [Test]
        [RunAsDefaultTenant]
        [Ignore("Anthony: Failing on trunk but not on a branch.")]
        public void Test288GetBasicMetadataByAlias()
        {
            ReportResult body = ReportControllerTestHelper.GetReportRequest(@"data/v1/report/core/templateReport?metadata=basic", HttpStatusCode.OK);
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");
        }

        [Test]
        [RunAsDefaultTenant]
        [Ignore("Anthony: Failing on trunk but not on a branch.")]
        public void Test289PostBasicMetadataByAlias()
        {
            ReportResult body = ReportControllerTestHelper.PostReportRequest(@"data/v1/report/core/templateReport?metadata=basic", HttpStatusCode.OK, new ReportParameters());
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestReportColumnRelationshipCardinality()
        {
            var reports = Entity.GetByName("AF_Relationships");

            Assert.AreEqual(1, reports.Count());            
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reports.First().Id), HttpStatusCode.OK);            
            Assert.IsNotNull(body.Metadata, "We don't have report metadata");

            Assert.AreEqual(5, body.Metadata.ReportColumns.Count);

            ReportColumn column = body.Metadata.ReportColumns.Values.FirstOrDefault(rc => rc.Title == "AA_DogBreeds (Rev)");
            Assert.IsNotNull(column, "The AA_DogBreeds (Rev) column was not found");
            Assert.AreEqual("InlineRelationship", column.Type, "AA_DogBreeds (Rev) column has an incorrect type");
            Assert.AreEqual("OneToMany", column.Cardinality, "AA_DogBreeds (Rev) column has an incorrect cardinality");

            column = body.Metadata.ReportColumns.Values.FirstOrDefault(rc => rc.Title == "AA_All Fields");
            Assert.IsNotNull(column, "The AA_All Fields column was not found");
            Assert.AreEqual("String", column.Type, "AA_All Fields column has an incorrect type");
            Assert.IsNullOrEmpty(column.Cardinality, "AA_All Fields column has an incorrect cardinality");

            column = body.Metadata.ReportColumns.Values.FirstOrDefault(rc => rc.Title == "AA_Herb");
            Assert.IsNotNull(column, "The AA_Herb Fields column was not found");
            Assert.AreEqual("InlineRelationship", column.Type, "AA_Herb column has an incorrect type");
            Assert.AreEqual("ManyToMany", column.Cardinality, "AA_Herb column has an incorrect cardinality");

            column = body.Metadata.ReportColumns.Values.FirstOrDefault(rc => rc.Title == "AA_Truck");
            Assert.IsNotNull(column, "The AA_Truck Fields column was not found");
            Assert.AreEqual("InlineRelationship", column.Type, "AA_Truck column has an incorrect type");
            Assert.AreEqual("ManyToMany", column.Cardinality, "AA_Truck column has an incorrect cardinality");

            column = body.Metadata.ReportColumns.Values.FirstOrDefault(rc => rc.Title == "AA_Snacks (Rev)");
            Assert.IsNotNull(column, "The AA_Snacks (Rev) Fields column was not found");
            Assert.AreEqual("InlineRelationship", column.Type, "AA_Snacks (Rev) column has an incorrect type");
            Assert.AreEqual("ManyToMany", column.Cardinality, "AA_Snacks (Rev) column has an incorrect cardinality");
        }

        [Test]        
        [RunAsDefaultTenant]
        public void TestGetColBasicMetadata()
        {
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:rpt_AnalAllSimpleTypes");
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?metadata=colbasic", reportId), HttpStatusCode.OK);
            Assert.Greater(body.Metadata.ReportColumns.Count, 0, "We should have report columns");
            Assert.IsNull(body.Metadata.AnalyserColumns);
        }

        [Test]
        [RunAsDefaultTenant]
        public void TestSingleRelationshipFilterSingleEntity()
        {
            var nationality = Entity.GetByName<Resource>("FRA");
            var french = nationality.First(l => l.IsOfType.Any(t => t.Alias == "test:nationalityEnumType"));

            // Get the drivers report and create a filter that will only return drivers whose nationality is French
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:driversReport");
            // Send it up and check the result
            ReportResult body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK, new ReportParameters
            {
                RelatedEntityFilters = new List<RelatedEntityFilter>
                {
                    new RelatedEntityFilter
                    {
                        RelationshipId = Entity.GetId("test:driverNationality"),
                        RelationshipDirection = RelationshipDirection.Forward,
                        RelatedEntityIds = new List<long>{french.Id}
                    }
                }
            });

            Assert.IsTrue(body.GridData.All(d => d.Values[1].Values.ContainsKey(french.Id)), "We should only have French nationality");            
        }


        [Test]
        [RunAsDefaultTenant]
        public void TestSingleRelationshipFilterMultipleEntities()
        {
            var nationality = Entity.GetByName<Resource>("FRA");
            var french = nationality.First(l => l.IsOfType.Any(t => t.Alias == "test:nationalityEnumType"));

            nationality = Entity.GetByName<Resource>("GBR");
            var english = nationality.First(l => l.IsOfType.Any(t => t.Alias == "test:nationalityEnumType"));

            // Get the drivers report and create a filter that will only return drivers whose nationality is French or English
            long reportId = ReportControllerTestHelper.GetReportByAlias("test:driversReport");
            // Send it up and check the result
            ReportResult body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reportId), HttpStatusCode.OK, new ReportParameters
            {
                RelatedEntityFilters = new List<RelatedEntityFilter>
                {
                    new RelatedEntityFilter
                    {
                        RelationshipId = Entity.GetId("test:driverNationality"),
                        RelationshipDirection = RelationshipDirection.Forward,
                        RelatedEntityIds = new List<long>{french.Id, english.Id}
                    }
                }
            });

            Assert.IsTrue(body.GridData.All(d => d.Values[1].Values.ContainsKey(french.Id) || d.Values[1].Values.ContainsKey(english.Id)), "We should only have French or English nationality");
        }


        [Test]
        [RunAsDefaultTenant]
        public void TestMultipleRelationshipFilters()
        {
            var marjoram = Entity.GetByName<Resource>("Marjoram").First();

            var eliminator = Entity.GetByName<Resource>("Eliminator").First();            
            
            var reports = Entity.GetByName("AF_Relationships");

            // Send it up and check the result
            ReportResult body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?metadata=full", reports.First().Id), HttpStatusCode.OK, new ReportParameters
            {
                RelatedEntityFilters = new List<RelatedEntityFilter>
                {
                    new RelatedEntityFilter
                    {
                        RelationshipId = Entity.GetId("test:herbs"),
                        RelationshipDirection = RelationshipDirection.Forward,
                        RelatedEntityIds = new List<long>{marjoram.Id}
                    },
                    new RelatedEntityFilter
                    {
                        RelationshipId = Entity.GetId("test:trucks"),
                        RelationshipDirection =  RelationshipDirection.Forward,
                        RelatedEntityIds = new List<long>{eliminator.Id}
                    }
                }
            });

            Assert.IsTrue(body.GridData.All(d => d.Values[1].Values.ContainsKey(marjoram.Id) && d.Values[2].Values.ContainsKey(eliminator.Id)), "We should only have Marjoram and Eliminator references");
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase("Date only Field", "Today", "")]
        [TestCase("Date only Field", "ThisWeek", "")]
        [TestCase("Date only Field", "ThisMonth", "")]
        [TestCase("Date only Field", "ThisQuarter", "")]
        [TestCase("Date only Field", "ThisYear", "")]
        [TestCase("Date only Field", "CurrentFinancialYear", "")]
        [TestCase("Date only Field", "LastNDays", "5")]
        [TestCase("Date only Field", "LastNDaysTillNow", "5")]
        [TestCase("Date only Field", "NextNDays", "5")]
        [TestCase("Date only Field", "NextNDaysFromNow", "5")]
        [TestCase("Date only Field", "LastNWeeks", "5")]
        [TestCase("Date only Field", "NextNWeeks", "5")]
        [TestCase("Date only Field", "LastNMonths", "5")]
        [TestCase("Date only Field", "NextNMonths", "5")]
        [TestCase("Date only Field", "LastNQuarters", "5")]
        [TestCase("Date only Field", "NextNQuarters", "5")]
        [TestCase("Date only Field", "LastNFinancialYears", "5")]
        [TestCase("Date only Field", "NextNFinancialYears", "5")]
        [TestCase("Date and Time Field", "Today", "")]
        [TestCase("Date and Time Field", "ThisWeek", "")]
        [TestCase("Date and Time Field", "ThisMonth", "")]
        [TestCase("Date and Time Field", "ThisQuarter", "")]
        [TestCase("Date and Time Field", "ThisYear", "")]
        [TestCase("Date and Time Field", "CurrentFinancialYear", "")]
        [TestCase("Date and Time Field", "LastNDays", "6")]
        [TestCase("Date and Time Field", "LastNDaysTillNow", "6")]
        [TestCase("Date and Time Field", "NextNDays", "6")]
        [TestCase("Date and Time Field", "NextNDaysFromNow", "6")]
        [TestCase("Date and Time Field", "LastNWeeks", "6")]
        [TestCase("Date and Time Field", "NextNWeeks", "6")]
        [TestCase("Date and Time Field", "LastNMonths", "6")]
        [TestCase("Date and Time Field", "NextNMonths", "6")]
        [TestCase("Date and Time Field", "LastNQuarters", "6")]
        [TestCase("Date and Time Field", "NextNQuarters", "6")]
        [TestCase("Date and Time Field", "LastNFinancialYears", "6")]
        [TestCase("Date and Time Field", "NextNFinancialYears", "6")]
        public void TestAnalyzerDateFieldSpecialOperators(string columnTitle, string oper, string value)
        {
            bool isDateOnlyField = columnTitle == "Date only Field";
            var report = Entity.Get("test:rpt_AnalAllSimpleTypes").As<ReadiNow.Model.Report>();
            var reportId = report.Id;

            var newRecordPrefix = string.Format("ANL_{0}_{1}", oper, DateTime.Now.ToString("d"));

            var financialYearStartMonth = 1;
            var generalSetting = Entity.Get<TenantGeneralSettings>(new EntityRef("core", "tenantGeneralSettingsInstance"));
            if (generalSetting != null && generalSetting.FinYearStartMonth != null)
            {
                financialYearStartMonth = PeriodConditionHelper.GetFinancialYearStartMonth(generalSetting.FinYearStartMonth.Alias.Replace("core:", ""));
            }

            // run report to get the column detail
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?refreshcachedresult&metadata=basic", reportId), HttpStatusCode.OK);
            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);


            // create analyser condition for 'TextField' column to make sure no record exist with the prefix that we are going to use to create new records
            var analyserTextFieldColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "TextField");
            var analyserFieldColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == columnTitle);
            var textFieldPrefixCond = new AnalyserColumnCondition
            {
                ExpressionId = analyserTextFieldColumn.Key,
                Operator = "Contains",
                Value = newRecordPrefix,
                Type = analyserTextFieldColumn.Value.Type
            };

            var analyserConditions = new List<AnalyserColumnCondition>
                {
                    textFieldPrefixCond
                };

            // run the report by filtering with  "newRecordPrefix". we shouldn't have any records
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?refreshcachedresult&metadata=basic", reportId), HttpStatusCode.OK, new ReportParameters { AnalyserConditions = analyserConditions });
            Assert.IsTrue(body.GridData == null, "We have existing All SimpleTypes records that match the prefix that we want to use to create new test data");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // insert 3 new test recrds
            var newEntities = ReportControllerTestHelper.InsertDateFieldDummyData(report.ReportUsesDefinition, newRecordPrefix, columnTitle, oper, value, isDateOnlyField, financialYearStartMonth);

            // actual filter condition that this test is checking
            var analyserCond = new AnalyserColumnCondition
            {
                ExpressionId = analyserFieldColumn.Key,
                Operator = oper,
                Value = value,
                Type = analyserFieldColumn.Value.Type
            };

            // run the report again with TextField filter and passed in date filter
            analyserConditions = new List<AnalyserColumnCondition>
                {
                    textFieldPrefixCond,
                    analyserCond
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?refreshcachedresult&metadata=basic", reportId), HttpStatusCode.OK, new ReportParameters { AnalyserConditions = analyserConditions });
            // Check we have exactly 3 records
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 3, "Should be 3 rows and not {0}", body.GridData.Count);

            // delete the newly created records
            Entity.Delete(newEntities);
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase("Date only Field", "CurrentFinancialYear", "")]
        [TestCase("Date only Field", "LastNFinancialYears", "2")]
        [TestCase("Date only Field", "NextNFinancialYears", "3")]
        [TestCase("Date and Time Field", "CurrentFinancialYear", "")]
        [TestCase("Date and Time Field", "LastNFinancialYears", "4")]
        [TestCase("Date and Time Field", "NextNFinancialYears", "5")]
        public void TestAnalyzerDateFields_FinancialYearOperators(string columnTitle, string oper, string value)
        {
            //  SETUP //
            // get initial current financial year start month
            var generalSettingRef = new EntityRef("core", "tenantGeneralSettingsInstance");
            var generalSetting = Entity.Get<TenantGeneralSettings>(generalSettingRef, true);
            var initialFinYearStartMonthEntity = generalSetting.FinYearStartMonth;

            // set current financial year start month to a random number between 1 and 12 
            var rand = new Random();
            var newFinYearStartMonth = rand.Next(1, 12);
            var newFinYearStartMonthAlias = ReportControllerTestHelper.GetFinancialYearStartMonthAlias(newFinYearStartMonth);
            var newFinYearStartMonthEntity = Entity.Get<MonthOfYearEnum>(new EntityRef("core", newFinYearStartMonthAlias));
            var newFinYearStartMonthNsAlias = string.Format("core:{0}", newFinYearStartMonthAlias);
            generalSetting.FinYearStartMonth = newFinYearStartMonthEntity;
            generalSetting.Save();

            // get and check if financial year start month is set to the value in step above
            generalSetting = Entity.Get<TenantGeneralSettings>(generalSettingRef, true);
            Assert.IsTrue(newFinYearStartMonthNsAlias == generalSetting.FinYearStartMonth.Alias, "invalid financial year start month");

            // actual test
            bool isDateOnlyField = columnTitle == "Date only Field";
            var report = Entity.Get("test:rpt_AnalAllSimpleTypes").As<ReadiNow.Model.Report>();
            var reportId = report.Id;

            var newRecordPrefix = string.Format("ANL_{0}_{1}", oper, DateTime.Now.ToString("d"));

            // run report to get the column detail
            ReportResult body = ReportControllerTestHelper.GetReportRequest(string.Format(@"data/v1/report/{0}?refreshcachedresult&metadata=basic", reportId), HttpStatusCode.OK);
            // Check the number of analyser fields
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 4, "Should be 4 rows and not {0}", body.GridData.Count);


            // create analyser condition for 'TextField' column to make sure no record exist with the prefix that we are going to use to create new records
            var analyserTextFieldColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == "TextField");
            var analyserFieldColumn = body.Metadata.AnalyserColumns.FirstOrDefault(ac => ac.Value.Title == columnTitle);
            var textFieldPrefixCond = new AnalyserColumnCondition
            {
                ExpressionId = analyserTextFieldColumn.Key,
                Operator = "Contains",
                Value = newRecordPrefix,
                Type = analyserTextFieldColumn.Value.Type
            };

            var analyserConditions = new List<AnalyserColumnCondition>
                {
                    textFieldPrefixCond
                };

            // run the report by filtering with  "newRecordPrefix". we shouldn't have any records
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?refreshcachedresult&metadata=basic", reportId), HttpStatusCode.OK, new ReportParameters { AnalyserConditions = analyserConditions });
            Assert.IsTrue(body.GridData == null, "We have existing All SimpleTypes records that match the prefix that we want to use to create new test data");
            Assert.IsNotNull(body.Metadata, "Have no metadata but have requested it");

            // insert 3 new test recrds
            var newEntities = ReportControllerTestHelper.InsertDateFieldDummyData(report.ReportUsesDefinition, newRecordPrefix, columnTitle, oper, value, isDateOnlyField, newFinYearStartMonth);

            // actual filter condition that this test is checking
            var analyserCond = new AnalyserColumnCondition
            {
                ExpressionId = analyserFieldColumn.Key,
                Operator = oper,
                Value = value,
                Type = analyserFieldColumn.Value.Type
            };

            // run the report again with TextField filter and passed in date filter
            analyserConditions = new List<AnalyserColumnCondition>
                {
                    textFieldPrefixCond,
                    analyserCond
                };
            body = ReportControllerTestHelper.PostReportRequest(string.Format(@"data/v1/report/{0}?refreshcachedresult&metadata=basic", reportId), HttpStatusCode.OK, new ReportParameters { AnalyserConditions = analyserConditions });
            // Check we have exactly 3 records
            Assert.IsTrue(body.Metadata.AnalyserColumns.Count == 10, "We have {0} analyser columns but should have 10", body.Metadata.AnalyserColumns.Count);
            Assert.IsTrue(body.GridData.Count == 3, "Should be 3 rows and not {0}", body.GridData.Count);

            // delete the newly created records
            Entity.Delete(newEntities);

            // set current financial year start month back to the initial value 
            generalSetting.FinYearStartMonth = initialFinYearStartMonthEntity;
            generalSetting.Save();

            // get and check if financial year start month is set to the initial value
            generalSetting = Entity.Get<TenantGeneralSettings>(generalSettingRef);
            Assert.IsTrue(initialFinYearStartMonthEntity.Alias == generalSetting.FinYearStartMonth.Alias, "invalid financial year start month");
        }
    }
}
