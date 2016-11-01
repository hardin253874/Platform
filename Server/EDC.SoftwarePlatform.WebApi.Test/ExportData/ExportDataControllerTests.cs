// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.WebApi.Controllers.Report;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;
using EDC.ReadiNow.Test;
using Entity = EDC.ReadiNow.Model.Entity;
using EDC.SoftwarePlatform.WebApi.Controllers.ExportData;

namespace EDC.SoftwarePlatform.WebApi.Test.ExportData
{
    [TestFixture]
    class ExportDataControllerTests
    {
        private const string ExcelFile = @"Sample.xlsx";
        private const string CsvFile = @"Sample.csv";
        private const string WordFile = @"Sample.docx";

        /// <summary>
        ///     Teardown
        /// </summary>
        [TestFixtureTearDown]
        public static void TestClassCleanup()
        {
            // Delete all files used for the test
            if (File.Exists(ExcelFile))
                File.Delete(ExcelFile);
            if (File.Exists(CsvFile))
                File.Delete(CsvFile);
            if (File.Exists(WordFile))
                File.Delete(WordFile);
        }
        [Test]
        [RunAsDefaultTenant]
        public void Test01ExportWrongFormat()
        {
            long reportId = GetReportByAlias("test:managerReport");
            PostExportRequest(string.Format(@"data/v2/report/export/{0}/xlsx", reportId), HttpStatusCode.BadRequest,
                   new ReportParameters());
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test02aExportWithInvalidReportId()
        {
            long reportId = 0;
            PostExportRequest(string.Format(@"data/v2/report/export/{0}/excel", reportId), HttpStatusCode.BadRequest,
                   new ReportParameters());
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test02bExportWithUnknownReportId()
        {
            long reportId = EntityId.Max;
            PostExportRequest(string.Format(@"data/v2/report/export/{0}/excel", reportId), HttpStatusCode.NotFound,
                   new ReportParameters());
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test03ExportToExcel()
        {
            long reportId = GetReportByAlias("test:managerReport");
            ExportDataInfo response = PostExportRequest(string.Format(@"data/v2/report/export/{0}/excel", reportId), HttpStatusCode.OK,
                   new ReportParameters());
            Assert.IsNotNullOrEmpty(response.FileHash,"The exported file has been saved to the database.");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test04ExportToCsv()
        {
            long reportId = GetReportByAlias("test:managerReport");
            ExportDataInfo response = PostExportRequest(string.Format(@"data/v2/report/export/{0}/csv", reportId), HttpStatusCode.OK,
                   new ReportParameters());
            Assert.IsNotNullOrEmpty(response.FileHash, "The exported file has been saved to the database.");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test05ExportToWord()
        {
            long reportId = GetReportByAlias("test:managerReport");
            ExportDataInfo response = PostExportRequest(string.Format(@"data/v2/report/export/{0}/word", reportId), HttpStatusCode.OK,
                   new ReportParameters());
            Assert.IsNotNullOrEmpty(response.FileHash, "The exported file has been saved to the database.");
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test06DownloadExcelExportedFile()
        {
            long reportId = GetReportByAlias("test:managerReport");
            ExportDataInfo res = PostExportRequest(string.Format(@"data/v2/report/export/{0}/excel", reportId), HttpStatusCode.OK,
                   new ReportParameters());
            Assert.IsNotNullOrEmpty(res.FileHash, "The exported file has been saved to the database.");
          
            using (
                PlatformHttpRequest request = new PlatformHttpRequest(string.Format(@"data/v2/report/export/download/{0}/sample/excel", res.FileHash),
                    PlatformHttpMethod.Get))
            {
                HttpWebResponse response = request.GetResponse();
                Assert.IsTrue(response.StatusCode == HttpStatusCode.OK, "We have a {0} returned, expected {1}",
                    response.StatusCode, HttpStatusCode.OK);
                Assert.AreEqual(response.ContentType, "text/html");
             
                using (Stream stream = response.GetResponseStream())
                {
                    Assert.IsNotNull(stream);
                    using (FileStream fileStream = File.Create(ExcelFile))
                    {
                        CopyToFile(stream, fileStream);

                        using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(fileStream, false))
                        {
                            IEnumerable<Sheet> sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                            Sheet sheet = sheets.FirstOrDefault();
                            Assert.NotNull(sheet);
                            Assert.AreEqual("Table", sheet.Name.ToString());
                            WorksheetPart worksheetPart =
                                (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById("rId1");
                            Worksheet workSheet = worksheetPart.Worksheet;

                            SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                            IEnumerable<Row> rows = sheetData.Descendants<Row>();
                            Assert.IsNotNull(rows);
                            Assert.IsTrue(rows.Any(), "There are no rows in the sheet.");
                            //Check if the first cell of the first row is the Name heading.
                            SharedStringTablePart stringTablePart = spreadSheetDocument.WorkbookPart.SharedStringTablePart;
                            Row row = rows.First();
                            Cell cell = row.GetFirstChild<Cell>();

                            Assert.AreEqual("0",cell.CellValue.Text);
                            var cellValue = stringTablePart.SharedStringTable.ChildElements[Int32.Parse(cell.CellValue.Text)].InnerText;
                            Assert.AreEqual("Name", cellValue);
                            //get second row 
                            Row row1 = rows.Skip(1).First();
                            Cell cell1 = row1.GetFirstChild<Cell>();
                            var cellValue1 = stringTablePart.SharedStringTable.ChildElements[Int32.Parse(cell1.CellValue.Text)].InnerText;
                            Assert.AreEqual("Glenn Uidam", cellValue1);
                        }
                    }
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test07DownloadCsvExportedFile()
        {
            long reportId = GetReportByAlias("test:managerReport");
            ExportDataInfo res = PostExportRequest(string.Format(@"data/v2/report/export/{0}/csv", reportId), HttpStatusCode.OK,
                   new ReportParameters());
            Assert.IsNotNullOrEmpty(res.FileHash, "The exported file has been saved to the database.");
            using (
                PlatformHttpRequest request =
                    new PlatformHttpRequest(
                        string.Format(@"data/v2/report/export/download/{0}/sample/csv", res.FileHash),
                        PlatformHttpMethod.Get))
            {
                HttpWebResponse response = request.GetResponse();
                Assert.IsTrue(response.StatusCode == HttpStatusCode.OK, "We have a {0} returned, expected {1}",
                    response.StatusCode, HttpStatusCode.OK);
                Assert.AreEqual(response.ContentType, "text/html");
                using (Stream stream = response.GetResponseStream())
                {
                    Assert.IsNotNull(stream);
                    using (FileStream fileStream = File.Create(CsvFile))
                    {
                        CopyToFile(stream, fileStream);
                        //Read data from the file
                        fileStream.Position = 0;
                        StreamReader sr = new StreamReader(fileStream);
                        int readLineCount = 0;
                        while (!sr.EndOfStream)
                        {
                            sr.ReadLine();
                            readLineCount++;
                        }
                        Assert.Greater(readLineCount, 0, "There are no rows in the file.");

                        //Read the first row
                        fileStream.Position = 0;
                        sr = new StreamReader(fileStream);
                        string rowString = sr.ReadLine();
                        Assert.AreEqual("Name", rowString);
                        //read secong row 
                        string row2 = sr.ReadLine();
                        Assert.AreEqual("Glenn Uidam", row2);
                    }

                }
            }
        }
       
        [Test]
        [RunAsDefaultTenant]
        public void Test08DownloadWordExportedFile()
        {
            long reportId = GetReportByAlias("test:managerReport");
            ExportDataInfo res = PostExportRequest(string.Format(@"data/v2/report/export/{0}/word", reportId), HttpStatusCode.OK,
                   new ReportParameters());
            Assert.IsNotNullOrEmpty(res.FileHash, "The exported file has been saved to the database.");
            using (
                PlatformHttpRequest request =
                    new PlatformHttpRequest(
                        string.Format(@"data/v2/report/export/download/{0}/sample/word", res.FileHash),
                        PlatformHttpMethod.Get))
            {
                HttpWebResponse response = request.GetResponse();
                Assert.IsTrue(response.StatusCode == HttpStatusCode.OK, "We have a {0} returned, expected {1}",
                    response.StatusCode, HttpStatusCode.OK);
                Assert.AreEqual(response.ContentType, "text/html");
                using (Stream stream = response.GetResponseStream())
                {
                    Assert.IsNotNull(stream);
                    using (FileStream fileStream = File.Create(WordFile))
                    {
                        CopyToFile(stream, fileStream);
                        fileStream.Position = 0;
                        using (WordprocessingDocument doc = WordprocessingDocument.Open(fileStream, false))
                        {
                            Body body = doc.MainDocumentPart.Document.Body;
                            DocumentFormat.OpenXml.Wordprocessing.Table table = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Table>().FirstOrDefault();
                           Assert.IsNotNull(table,"The word document hasn't got a table");
                            var rows = table.Descendants<TableRow>();
                            Assert.Greater(rows.Count(),0,"There are no rows in the table");
                            //Get first row.
                            TableRow row = rows.First();
                            TableCell cell = row.GetFirstChild<TableCell>();
                            Assert.AreEqual("Name", cell.FirstChild.InnerText);
                            //Get second row
                            TableRow row1 = rows.Skip(1).First();
                            TableCell cell1 = row1.GetFirstChild<TableCell>();
                            Assert.AreEqual("Glenn Uidam", cell1.FirstChild.InnerText);
                        }
                    }
                }
            }
         
        }

        /// <summary>
        /// Copy the stream content to a file.
        /// </summary>
        private static void CopyToFile(Stream stream, FileStream fileStream)
        {
            int count = 0;
            do
            {
                byte[] buf = new byte[1024];
                count = stream.Read(buf, 0, 1024);
                fileStream.Write(buf, 0, count);
            } while (stream.CanRead && count > 0);
        }

        /// <summary>
        /// Get test report Id from alias
        /// </summary>
        private static long GetReportByAlias(string reportAlias)
        {
            long reportId;
            using (DatabaseContext.GetContext(true))
            {
                reportId = Entity.Get(reportAlias).Id;
            }
            Assert.IsTrue(reportId > 0, "We have report id of {0}", reportId);
            return reportId;
        }

        private static ExportDataInfo PostExportRequest(string url, HttpStatusCode expectedResposeCode, ReportParameters parameters)
        {
            using (var request = new PlatformHttpRequest(url, PlatformHttpMethod.Post))
            {
                request.PopulateBody(parameters);
                var response = request.GetResponse();
                Assert.AreEqual(expectedResposeCode, response.StatusCode, "We have a {0} returned, expected {1}", response.StatusCode, expectedResposeCode);
                if (expectedResposeCode != HttpStatusCode.OK)
                    return null;
                return request.DeserialiseResponseBody<ExportDataInfo>();
            }
        }
    }
}
