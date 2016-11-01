// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EDC.ReadiNow.Services.Console;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.WebApi.Test.Actions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ActionControllerTestHelper
    {
        public static void RunActionsRequest( ActionRequest actionRequest, HttpStatusCode expectedResponse = HttpStatusCode.OK )
        {

            using ( var request = new PlatformHttpRequest( "data/v1/actions", PlatformHttpMethod.Post ) )
            {
                request.PopulateBody( actionRequest );
                HttpWebResponse response = request.GetResponse( );
                Assert.That( response.StatusCode, Is.EqualTo( expectedResponse ) );
            }
        }

        public static void GetReportQuickMenu( long reportId )
        {
            // Sample as scraped from console:
            //{"ids":[],"lastId":-1,"cellId":-1,"reportId":38503,"formDataEntityId":-1,"hostIds":[],"hostTypeIds":[],"data":{},"display":"quickmenu"}

            ActionRequest request = new ActionRequest
            {
                ReportId = reportId,
                ActionDisplayContext = ActionContext.QuickMenu,
                HostResourceIds = new long [ 0 ],
                HostTypeIds = new List<long>( ),
                AdditionalData = new Dictionary<string, object>( ),
                FormDataEntityId = -1,
                CellSelectedResourceId = -1,
                LastSelectedResourceId = -1,
                SelectedResourceIds = new long [ 0 ]
            };

            RunActionsRequest( request );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reportId">ID of report shown in tab</param>
        /// <param name="tabEntityTypeId">Type of entity being shown in the tab.</param>
        /// <param name="parentEntityId">The ID of the parent instance. I.e. of the parent entity being shown in the form.</param>
        public static void GetRelationshipTabQuickMenu( long reportId, long tabEntityTypeId, long parentEntityId = 9007199254740986 )
        {
            // Sample as scraped from console:
            //{"ids":[],"lastId":-1,"cellId":-1,"reportId":43180,"formDataEntityId":9007199254740986,"hostIds":[82474],"hostTypeIds":[4146],"entityTypeId":66026,"data":{},"display":"quickmenu"}

            long tabRelationshipControlId = new EntityRef("console:tabRelationshipRenderControl").Id;

            ActionRequest request = new ActionRequest
            {
                ReportId = reportId,
                EntityTypeId = tabEntityTypeId,                 
                ActionDisplayContext = ActionContext.QuickMenu,
                HostResourceIds = new long [ 0 ],   // we should set this to the id of the tabRelationshipRenderControl in question
                HostTypeIds = new List<long> { tabRelationshipControlId },
                AdditionalData = new Dictionary<string, object>( ),
                FormDataEntityId = parentEntityId,
                CellSelectedResourceId = -1,
                LastSelectedResourceId = -1,
                SelectedResourceIds = new long [ 0 ]
            };

            RunActionsRequest( request );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reportId">ID of report shown in picker</param>
        /// <param name="tabEntityTypeId">Type of entity being shown in the tab.</param>
        /// <param name="parentEntityId">The ID of the parent instance. I.e. of the parent entity being shown in the form.</param>
        public static void GetPickerQuickMenu( long reportId, long typeId )
        {
            // Sample as scraped from console:
            //{"ids":[],"lastId":-1,"cellId":-1,"reportId":53493,"formDataEntityId":-1,"hostIds":[],"hostTypeIds":[],"entityTypeId":20637,"data":{},"display":"quickmenu"}

            ActionRequest request = new ActionRequest
            {
                ReportId = reportId,
                EntityTypeId = typeId,
                ActionDisplayContext = ActionContext.QuickMenu,
                HostResourceIds = new long [ 0 ],   // we should set this to the id of the tabRelationshipRenderControl in question
                HostTypeIds = new List<long>(),
                AdditionalData = new Dictionary<string, object>( ),
                FormDataEntityId = -1,
                CellSelectedResourceId = -1,
                LastSelectedResourceId = -1,
                SelectedResourceIds = new long [ 0 ]
            };

            RunActionsRequest( request );
        }
    }
}
