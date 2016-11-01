// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.Security;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace EDC.SoftwarePlatform.WebApi.Test.Security
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    class AccessControlControllerTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_TypeAccessReport_EveryoneByAlias( )
        {
            using ( var request = new PlatformHttpRequest( "data/v1/accesscontrol/typeaccessreport/everyoneRole", PlatformHttpMethod.Get ) )
            {
                var response = request.GetResponse( );
                Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.OK ) );

                List<TypeAccessReason> reasons = request.DeserialiseResponseBody<List<TypeAccessReason>>( );
                Assert.That( reasons, Has.Count.GreaterThan( 0 ) );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_TypeAccessReport_StaffRoleById( )
        {
            long roleId = Entity.GetByName<Role>( "Staff" ).Single().Id;

            string url = $"data/v1/accesscontrol/typeaccessreport/{roleId}";
            using ( var request = new PlatformHttpRequest( url, PlatformHttpMethod.Get ) )
            {
                var response = request.GetResponse( );
                Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.OK ) );

                List<TypeAccessReason> reasons = request.DeserialiseResponseBody<List<TypeAccessReason>>( );
                Assert.That( reasons, Has.Count.GreaterThan( 0 ) );

                // Find rule
                TypeAccessReason reason = reasons.Single( r => r.Reason == "Access rule: Students have qualification in my faculty" );
                Assert.That( reason.SubjectName, Is.EqualTo( "Staff" ) );
                Assert.That( reason.TypeName, Is.EqualTo( "Student" ) ); // hmm .. object should have been named 'Subject'
                Assert.That( reason.Permissions, Is.EqualTo( "Read" ) );
                Assert.That( reason.Scope, Is.EqualTo( "PerUser" ) );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_TypeAccessReport_BadId( )
        {
            string url = $"data/v1/accesscontrol/typeaccessreport/321";
            using ( var request = new PlatformHttpRequest( url, PlatformHttpMethod.Get ) )
            {
                var response = request.GetResponse( );
                Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.NotFound ) );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_TypeAccessReport_NonAdmin( )
        {
            UserAccount userAccount = null;
            try
            {
                userAccount = new UserAccount
                {
                    Name = "Test User " + Guid.NewGuid( ),
                    Password = "whatever123!@#",
                    AccountStatus_Enum = UserAccountStatusEnum_Enumeration.Active,
                    ChangePasswordAtNextLogon = false
                };
                userAccount.Save( );

                string url = $"data/v1/accesscontrol/typeaccessreport/everyoneRole";
                using ( var request = new PlatformHttpRequest( url, PlatformHttpMethod.Get, userAccount ) )
                {
                    var response = request.GetResponse( );
                    Assert.That( response.StatusCode, Is.EqualTo( HttpStatusCode.Forbidden ) );
                }

            }
            finally
            {
                userAccount?.Delete( );

            }
        }
    }
}
