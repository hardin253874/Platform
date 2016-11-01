// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.Cache;
using EDC.Cache.Providers;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Security.AccessControl;
using Moq;
using NUnit.Framework;
using Entity = EDC.ReadiNow.Model.Entity;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class SecurityQueryCacheInvalidatorHelperTests
    {
        [Test]
        [TestCaseSource("Test_IsSecurityQueryRelationship_Source")]
        [RunAsDefaultTenant]
        public void Test_IsSecurityQueryRelationship(string alias, bool expectedResult)
        {
            Assert.That(SecurityQueryCacheInvalidatorHelper.IsSecurityQueryRelationship(Entity.Get(alias).Id),
                Is.EqualTo(expectedResult));
        }

        private IEnumerable<TestCaseData> Test_IsSecurityQueryRelationship_Source
        {
            get
            {
                yield return new TestCaseData("core:allowAccess", true);
                yield return new TestCaseData("core:controlAccess", true);
                yield return new TestCaseData("core:permissionAccess", true);
                yield return new TestCaseData("core:accessRuleReport", true);
                yield return new TestCaseData("core:resource", false);
            }
        }
        
        [Test]
        public void Test_IsSecurityQueryEntity_NullEntity()
        {
            Assert.That(() => SecurityQueryCacheInvalidatorHelper.IsSecurityQueryEntity(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("entity"));
        }

        [Test]
        [TestCaseSource("Test_IsSecurityQueryEntity_Source")]
        public void Test_IsSecurityQueryEntity(IEntity entity, bool expectedResult)
        {
            Assert.That(SecurityQueryCacheInvalidatorHelper.IsSecurityQueryEntity(entity),
                Is.EqualTo(expectedResult));
        }

        private IEnumerable<TestCaseData> Test_IsSecurityQueryEntity_Source
        {
            get
            {
                MockRepository mockRepository;
                Mock<IEntity> mockAccessRule;
                Mock<IEntity> mockSubject;
                Mock<IEntity> mockReport;
                Mock<IEntity> mockPermission;
                Mock<IEntity> mockSecurableEntity;
                Mock<IEntity> mockOther;
                IEntity accessRule;
                IEntity subject;
                IEntity report;
                IEntity permission;
                IEntity securableEntity;
                IEntity other;

                mockRepository = new MockRepository(MockBehavior.Strict);

                mockAccessRule = mockRepository.Create<IEntity>();
                mockAccessRule.Setup(a => a.Is<AccessRule>()).Returns(true);
                mockAccessRule.Setup(a => a.Is<Subject>()).Returns(false);
                mockAccessRule.Setup(a => a.Is<Report>()).Returns(false);
                mockAccessRule.Setup(a => a.Is<Permission>()).Returns(false);
                mockAccessRule.Setup(a => a.Is<SecurableEntity>()).Returns(false);

                mockSubject = mockRepository.Create<IEntity>();
                mockSubject.Setup(a => a.Is<AccessRule>()).Returns(false);
                mockSubject.Setup(a => a.Is<Subject>()).Returns(true);
                mockSubject.Setup(a => a.Is<Report>()).Returns(false);
                mockSubject.Setup(a => a.Is<Permission>()).Returns(false);
                mockSubject.Setup(a => a.Is<SecurableEntity>()).Returns(false);

                mockReport = mockRepository.Create<IEntity>();
                mockReport.Setup(a => a.Is<AccessRule>()).Returns(false);
                mockReport.Setup(a => a.Is<Subject>()).Returns(false);
                mockReport.Setup(a => a.Is<Report>()).Returns(true);
                mockReport.Setup(a => a.Is<Permission>()).Returns(false);
                mockReport.Setup(a => a.Is<SecurableEntity>()).Returns(false);

                mockPermission = mockRepository.Create<IEntity>();
                mockPermission.Setup(a => a.Is<AccessRule>()).Returns(false);
                mockPermission.Setup(a => a.Is<Subject>()).Returns(false);
                mockPermission.Setup(a => a.Is<Report>()).Returns(false);
                mockPermission.Setup(a => a.Is<Permission>()).Returns(true);
                mockPermission.Setup(a => a.Is<SecurableEntity>()).Returns(false);

                mockSecurableEntity = mockRepository.Create<IEntity>();
                mockSecurableEntity.Setup(a => a.Is<AccessRule>()).Returns(false);
                mockSecurableEntity.Setup(a => a.Is<Subject>()).Returns(false);
                mockSecurableEntity.Setup(a => a.Is<Report>()).Returns(false);
                mockSecurableEntity.Setup(a => a.Is<Permission>()).Returns(false);
                mockSecurableEntity.Setup(a => a.Is<SecurableEntity>()).Returns(true);

                mockOther = mockRepository.Create<IEntity>();
                mockOther.Setup(a => a.Is<AccessRule>()).Returns(false);
                mockOther.Setup(a => a.Is<Subject>()).Returns(false);
                mockOther.Setup(a => a.Is<Report>()).Returns(false);
                mockOther.Setup(a => a.Is<Permission>()).Returns(false);
                mockOther.Setup(a => a.Is<SecurableEntity>()).Returns(false);

                accessRule = mockAccessRule.Object;
                subject = mockSubject.Object;
                report = mockReport.Object;
                permission = mockPermission.Object;
                securableEntity = mockSecurableEntity.Object;
                other = mockOther.Object;

                yield return new TestCaseData(other, false);
                yield return new TestCaseData(accessRule, true);
                yield return new TestCaseData(subject, true);
                yield return new TestCaseData(report, true);
                yield return new TestCaseData(permission, true);
                yield return new TestCaseData(securableEntity, true);
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Maps()
        {
            Assert.That(
                SecurityQueryCacheInvalidatorHelper.AllowAccess, 
                Has.Property("Namespace").EqualTo("core")
                   .And.Property("Alias").EqualTo("allowAccess"));
            Assert.That(
                SecurityQueryCacheInvalidatorHelper.ControlAccess, 
                Has.Property("Namespace").EqualTo("core")
                   .And.Property("Alias").EqualTo("controlAccess"));
            Assert.That(
                SecurityQueryCacheInvalidatorHelper.PermissionAccess, 
                Has.Property("Namespace").EqualTo("core")
                   .And.Property("Alias").EqualTo("permissionAccess"));
            Assert.That(
                SecurityQueryCacheInvalidatorHelper.AccessRuleReport, 
                Has.Property("Namespace").EqualTo("core")
                   .And.Property("Alias").EqualTo("accessRuleReport"));
        }
    }
}
