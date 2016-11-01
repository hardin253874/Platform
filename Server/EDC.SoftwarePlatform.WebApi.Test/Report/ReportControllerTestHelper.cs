// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.Common;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.SoftwarePlatform.WebApi.Controllers.Report;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;
using System.Net;
using Entity = EDC.ReadiNow.Model.Entity;

namespace EDC.SoftwarePlatform.WebApi.Test.Report
{
    internal class ReportControllerTestHelper
    {
        public static ReportResult GetReportFull( long reportId, HttpStatusCode expectedResposeCode = HttpStatusCode.OK )
        {
            string uri = string.Format( @"data/v1/report/{0}?metadata=full&page=0,200", reportId );
            return ReportControllerTestHelper.GetReportRequest( uri, expectedResposeCode );
        }

        public static ReportResult GetRelationshipTabReportFull( long reportId, long relTypeId, bool isReverse, HttpStatusCode expectedResposeCode = HttpStatusCode.OK )
        {
            long foreignId = 9007199254740986;

            string uri = string.Format( @"data/v1/report/{0}?metadata=full&page=0,200&relationship={1},{2},{3}",
                reportId, foreignId, relTypeId, isReverse ? "rev" : "fwd" );
            return ReportControllerTestHelper.GetReportRequest( uri, expectedResposeCode );
        }

        /// <summary>
        /// The report loaded to drive a listbox on an edit form.
        /// </summary>
        public static ReportResult GetEditFormListboxContents( long reportId, HttpStatusCode expectedResposeCode = HttpStatusCode.OK )
        {
            string uri = string.Format( @"data/v1/report/{0}?metadata=colbasic&page=0,200", reportId );
            return ReportControllerTestHelper.GetReportRequest( uri, expectedResposeCode );
        }

        internal static ReportResult GetReportRequest(string url, HttpStatusCode expectedResposeCode)
        {
	        int failures = 0;

	        while ( failures < 3 )
	        {
		        using ( var request = new PlatformHttpRequest( url ) )
		        {
			        HttpWebResponse response = request.GetResponse( );

			        if ( response == null )
			        {
				        failures++;
				        continue;
			        }

                    Assert.That(response.StatusCode, Is.EqualTo(expectedResposeCode));
			        return request.DeserialiseResponseBody<ReportResult>( );
		        }
	        }

	        Assert.Fail( "Failed to Get Report Request" );
			return null;
        }

        internal static ReportResult PostReportRequest(string url, HttpStatusCode expectedResposeCode, ReportParameters parameters)
        {
            using (var request = new PlatformHttpRequest(url, PlatformHttpMethod.Post))
            {
                request.PopulateBody(parameters);
                var response = request.GetResponse();
                Assert.AreEqual(expectedResposeCode, response.StatusCode, "We have a {0} returned, expected {1}", response.StatusCode, expectedResposeCode);
                return request.DeserialiseResponseBody<ReportResult>();
            }
        }

        internal static ReportResult PutReportRequest(string url, HttpStatusCode expectedResponseCode, ReportParameters parameters)
        {
            using (var request = new PlatformHttpRequest(url, PlatformHttpMethod.Put))
            {
                request.PopulateBody(parameters);
                var response = request.GetResponse();
                Assert.That(response, Has.Property("StatusCode").EqualTo(expectedResponseCode), "We have a {0} returned, expected {1}", response.StatusCode, expectedResponseCode);
                return request.DeserialiseResponseBody<ReportResult>();
            }
        }

        internal static long GetReportByAlias(string reportAlias)
        {
            long reportId;
            using (DatabaseContext.GetContext(true))
            {
                reportId = Entity.Get(reportAlias).Id;
            }
            Assert.IsTrue(reportId > 0, "We have report id of {0}", reportId);
            return reportId;
        }

        internal static long GetIconByAlias(string iconAlias)
        {
            long iconId;
            using (DatabaseContext.GetContext(true))
            {
                iconId = Entity.Get(iconAlias).Id;
            }
            Assert.IsTrue(iconId > 0, "We have icon id of {0}", iconId);
            return iconId;
        }

        internal static long GetEntityIdByAlias(string entityAlias)
        {
            long entityId;
            using (DatabaseContext.GetContext(true))
            {
                entityId = Entity.Get(entityAlias).Id;
            }
            Assert.IsTrue(entityId > 0, "We have icon id of {0}", entityId);
            return entityId;
        }

        internal static IEnumerable<long> InsertDateFieldDummyData(EntityType definition, string newRecordPrefix, string analyserFieldColumnName, string oper, string value, bool isDateOnlyField, int financialYearStartMonth)
        {
            // generate 3 random dateTime values
            DateTime minDate, maxDate;
            var conditionOper = (ConditionType)Enum.Parse(typeof(ConditionType), oper);
            int? argument = null;
            if (!string.IsNullOrEmpty(value))
            {
                argument = int.Parse(value);
            }
            
            

            // workout minDate and maxDate
            PeriodConditionHelper.GetPeriodFromConditionType(conditionOper, DateTime.Today, argument, financialYearStartMonth, isDateOnlyField, out minDate, out maxDate);

            // get a random value between min and max date values
            var rand = new Random();
            var dateDiffTicks = maxDate.Ticks - minDate.Ticks;

            var newEntities= new long[3];
            var textFieldColumnId = definition.Fields.FirstOrDefault(f => f.Name == "TextField").Id;
            var analyserFieldColumnId = definition.Fields.FirstOrDefault(f => f.Name == analyserFieldColumnName).Id;

            // get 
            using (DatabaseContext.GetContext(false))
            {
                for (int ctr = 0; ctr < 3; ctr++)
                {
                    string randomTextFieldColValue = string.Format("{0}-{1}", newRecordPrefix, DateTime.Now.ToUniversalTime().ToLongTimeString());
                    var randPercentNum = rand.Next(1, 100);
                    var randomTicks = dateDiffTicks*randPercentNum/100;
                    var dt = minDate.AddTicks(randomTicks);     // add a random percentage of difference to the min date value

                    // server stores value in utc. 'dateToSave' is saved as it is. 
                    // in case of dateOnly field, time portion is ignored and only date value is used. thats why we save 'dt' as it is.
                    // in case of dateAndTime field, the whole vale is adjusted to current timezone. Thats why we save the utc vale of 'dt'. 
                    var dateToSave = isDateOnlyField ? dt : dt.ToUniversalTime();   
                     
                    var entity = Entity.Create(definition.Id);
                    entity.SetField(textFieldColumnId, randomTextFieldColValue);
                    entity.SetField(analyserFieldColumnId, dateToSave);
                    entity.Save();

                    newEntities[ctr] = entity.Id;
                }
            }
            return newEntities;
        }

        internal static string GetFinancialYearStartMonthAlias(int monthNumber)
        {
            switch (monthNumber)
            {
                case 1:
                    return "moyJanuary";

                case 2:
                    return "moyFebruary";

                case 3:
                    return "moyMarch";

                case 4:
                    return "moyApril";

                case 5:
                    return "moyMay";

                case 6:
                    return "moyJune";

                case 7:
                    return "moyJuly";

                case 8:
                    return "moyAugust";

                case 9:
                    return "moySeptember";

                case 10:
                    return "moyOctober";

                case 11:
                    return "moyNovember";

                case 12:
                    return "moyDecember";

                default:
                    throw new ArgumentException("FinYearStartMonthNumber");
            }
        }
    }
}
