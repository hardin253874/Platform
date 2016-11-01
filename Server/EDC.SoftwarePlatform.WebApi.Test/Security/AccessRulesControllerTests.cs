// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.Security;
using EDC.SoftwarePlatform.WebApi.Test.Infrastructure;
using NUnit.Framework;
using AccessRule = EDC.ReadiNow.Model.AccessRule;

namespace EDC.SoftwarePlatform.WebApi.Test.Security
{
    [TestFixture]
    public class AccessRulesControllerTests
    {
        [Test]
        [RunAsDefaultTenant]
        public void Test_AccessRuleCreate()
        {
            Definition definition;
            long accessRuleId;
            List<long> entitiesToDelete;
            HttpWebResponse response;
            AccessRule accessRule;
            UserAccount userAccount;
            IEntity entity;

            entitiesToDelete = new List<long>();
            try
            {
                definition = Entity.Create<Definition>();
                definition.Save();
                entitiesToDelete.Add(definition.Id);

                userAccount = Entity.Create<UserAccount>();
                userAccount.Save();
                entitiesToDelete.Add(userAccount.Id);

                using (var request = new PlatformHttpRequest("data/v1/accessrule", PlatformHttpMethod.Post))
                {
                    request.PopulateBody(new NewAccessRuleInfo
                    {
                        SubjectId = userAccount.Id,
                        SecurableEntityId = definition.Id
                    });

                    response = request.GetResponse();
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

                    accessRuleId = request.DeserialiseResponseBody<long>();
                }

                entitiesToDelete.Add(accessRuleId);

                accessRule = Entity.Get<AccessRule>(accessRuleId);

                Assert.That(accessRule, Has.Property("AllowAccessBy").Not.Null
                                              .And.Property("AllowAccessBy").Property("Id").EqualTo(userAccount.Id));
                Assert.That(accessRule, Has.Property("ControlAccess").Not.Null
                                              .And.Property("ControlAccess").Property("Id").EqualTo(definition.Id));
                Assert.That(accessRule, Has.Property("AccessRuleEnabled").False);
                Assert.That(accessRule.PermissionAccess.Select(p => new EntityRef(p)), 
                    Is.EquivalentTo(new [] { Permissions.Read }).Using(EntityRefComparer.Instance));

                entity = Entity.Create(definition);
                entitiesToDelete.Add(entity.Id);

                using (new SetUser(userAccount))
                {
                    Assert.That(() => Entity.Get(entity.Id), Throws.Nothing,
                        "Read access not granted to user");    
                }
            }
            finally
            {
                try
                {
                    Entity.Delete(entitiesToDelete);
                }
                catch (Exception)
                {
                    // Do nothing
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AccessRuleCreate_MissingEntityType()
        {
            Subject subject;
            List<long> entitiesToDelete;
            HttpWebResponse response;

            entitiesToDelete = new List<long>();
            try
            {
                subject = Entity.Create<Subject>();
                subject.Save();
                entitiesToDelete.Add(subject.Id);

                using (var request = new PlatformHttpRequest("data/v1/accessrule", PlatformHttpMethod.Post))
                {
                    request.PopulateBody(new NewAccessRuleInfo
                    {
                        SubjectId = subject.Id,
                        SecurableEntityId = 0
                    });

                    response = request.GetResponse();
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                }
            }
            finally
            {
                try
                {
                    Entity.Delete(entitiesToDelete);
                }
                catch (Exception)
                {
                    // Do nothing
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AccessRuleCreate_MissingSubject()
        {
            Definition definition;
            List<long> entitiesToDelete;
            HttpWebResponse response;

            entitiesToDelete = new List<long>();
            try
            {
                definition = Entity.Create<Definition>();
                definition.Save();
                entitiesToDelete.Add(definition.Id);

                using (var request = new PlatformHttpRequest("data/v1/accessrule", PlatformHttpMethod.Post))
                {
                    request.PopulateBody(new NewAccessRuleInfo
                    {
                        SubjectId = 0,
                        SecurableEntityId = definition.Id
                    });

                    response = request.GetResponse();
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                }
            }
            finally
            {
                try
                {
                    Entity.Delete(entitiesToDelete);
                }
                catch (Exception)
                {
                    // Do nothing
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AccessRuleCreate_AccessDenied()
        {
            UserAccount userAccount;
            Subject subject;
            Definition definition;
            List<long> entitiesToDelete;
            HttpWebResponse response;

            entitiesToDelete = new List<long>();
            try
            {
                userAccount = Entity.Create<UserAccount>();
                userAccount.Name = Guid.NewGuid().ToString() + DateTime.Now;
                userAccount.Save();
                entitiesToDelete.Add(userAccount.Id);

                subject = Entity.Create<Subject>();
                subject.Save();
                entitiesToDelete.Add(subject.Id);

                definition = Entity.Create<Definition>();
                definition.Save();
                entitiesToDelete.Add(definition.Id);

                using (var request = new PlatformHttpRequest("data/v1/accessrule", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new NewAccessRuleInfo
                    {
                        SubjectId = subject.Id,
                        SecurableEntityId = definition.Id
                    });

                    response = request.GetResponse();
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
                }
            }
            finally
            {
                try
                {
                    Entity.Delete(entitiesToDelete);
                }
                catch (Exception)
                {
                    // Do nothing
                }
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_AccessRuleCreate_NonDefinition()
        {
            UserAccount userAccount;
            Subject subject;
            EntityType entityType;
            List<long> entitiesToDelete;
            HttpWebResponse response;

            entitiesToDelete = new List<long>();
            try
            {
                userAccount = Entity.Create<UserAccount>();
                userAccount.Name = Guid.NewGuid().ToString() + DateTime.Now;
                userAccount.Save();
                entitiesToDelete.Add(userAccount.Id);

                subject = Entity.Create<Subject>();
                subject.Save();
                entitiesToDelete.Add(subject.Id);

                entityType = Entity.Create<EntityType>();
                entityType.Save();
                entitiesToDelete.Add(entityType.Id);

                using (var request = new PlatformHttpRequest("data/v1/accessrule", PlatformHttpMethod.Post, userAccount))
                {
                    request.PopulateBody(new NewAccessRuleInfo
                    {
                        SubjectId = subject.Id,
                        SecurableEntityId = entityType.Id
                    });

                    response = request.GetResponse();
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                }
            }
            finally
            {
                try
                {
                    Entity.Delete(entitiesToDelete);
                }
                catch (Exception)
                {
                    // Do nothing
                }
            }
        }
    }
}
