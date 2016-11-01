// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EDC.SoftwarePlatform.WebApi.Controllers.Entity2;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test
{
    /// <summary>
    /// 
    /// </summary>
    public static class EntityControllerTestHelper
    {
        public static JsonQueryResult RunRequest( string query, HttpStatusCode expected = HttpStatusCode.OK )
        {
            using ( var request = new PlatformHttpRequest( @"data/v2/entity", PlatformHttpMethod.Post ) )
            {
                request.PopulateBodyString( query );

                var response = request.GetResponse( );

                // check that it worked (200)
                Assert.That(response.StatusCode, Is.EqualTo( expected ) );

                // Check response body
                JsonQueryResult res = request.DeserialiseResponseBody<JsonQueryResult>( );
                Assert.IsNotNull( res );

                return res;
            }
        }

        public static void RunSave( string body, HttpStatusCode expected = HttpStatusCode.OK )
        {
            using ( var request = new PlatformHttpRequest( @"data/v1/entity", PlatformHttpMethod.Post ) )
            {
                request.PopulateBodyString( body );
                var response = request.GetResponse( );
                Assert.That( response.StatusCode, Is.EqualTo( expected ) );
            }
        }

        public static JsonQueryResult Query_Name( long entityId )
        {
            string query = @"{
                'queries':['name'],
                'requests':[ {'get':'basic','ids':[1234],'hint':'1234','rq':0}]}";

            query = query.Replace( "'", @"""" );
            query = query.Replace( "1234", entityId.ToString( ) );           
            return RunRequest( query );
        }

        public static JsonQueryResult Query_PendingTasks( long typeId )
        {
            string query = @"{
                'queries':['tasksForRecord.{assignedToUser.id, name, taskComment, hideComment, openInEditMode, userTaskIsComplete, taskStatus.id,userTaskCompletedOn, availableTransitions.fromExitPoint.{name, description, exitPointOrdinal}, userResponse.id, waitForNextTask, workflowRunForTask.id, recordToPresent.name}'],
                'requests':[ {'get':'basic','ids':[1234],'hint':'spUserTask.getPendingTasksForRecord.TEST','rq':0}]}";

            query = query.Replace( "'", @"""" );
            query = query.Replace( "1234", typeId.ToString( ) );           
            return RunRequest( query );
        }

        public static JsonQueryResult Query_RefreshReport( long reportId )
        {
            string query = @"{
                'queries':['alias,name,description,isOfType.{alias,name}','resourceViewerConsoleForm.{alias,isOfType.alias}'],
                'requests':[{'get':'basic','ids':[38503],'hint':'refreshReport-38503','rq':0},{'get':'basic','ids':[38503],'hint':'refreshReportRV-38503','rq':1}]}";
            
            query = query.Replace( "'", @"""" );
            query = query.Replace( "38503", reportId.ToString( ) );
            return RunRequest( query );
        }

    }
}
