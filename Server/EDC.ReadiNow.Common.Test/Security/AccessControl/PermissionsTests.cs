// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class PermissionsTests
    {
        [Test]
        public void TryGetCreatePermission_NullTenantPermissionMap()
        {
            Assert.That(() => Permissions.TryGetCreatePermission(null, () => null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("tenantPermissionMap"));
        }

        [Test]
        public void TryGetCreatePermission_NullCreateTenant()
        {
            Assert.That(() => Permissions.TryGetCreatePermission(new ConcurrentDictionary<long, EntityRef>(), null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("createFunc"));
        }

        [Test]
        public void TryGetCreatePermission_NoRequestContext()
        {
            Assert.That(() => Permissions.TryGetCreatePermission(new ConcurrentDictionary<long, EntityRef>(), () => null),
                Throws.InvalidOperationException);
        }

        [Test]
        [RunAsDefaultTenant]
        public void TryGetCreatePermission_CachedPermission()
        {
            const string testNamespace = "core";
            const string testAlias = "read";

            Permissions.TryGetCreatePermission(new ConcurrentDictionary<long, EntityRef>(), () => new EntityRef(testNamespace, testAlias));

            Assert.That(Permissions.ReadMap[RequestContext.TenantId],
                Has.Property("Namespace").EqualTo(testNamespace).And.Property("Alias").EqualTo(testAlias));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Read()
        {
            Assert.That(Permissions.Read, 
                Has.Property("Namespace").EqualTo("core").And.Property("Alias").EqualTo("read"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Modify()
        {
            Assert.That(Permissions.Modify,
                Has.Property("Namespace").EqualTo("core").And.Property("Alias").EqualTo("modify"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Delete()
        {
            Assert.That(Permissions.Delete,
                Has.Property("Namespace").EqualTo("core").And.Property("Alias").EqualTo("delete"));
        }

        [Test]
        [RunAsDefaultTenant]
        public void Create()
        {
            Assert.That(Permissions.Create,
                Has.Property("Namespace").EqualTo("core").And.Property("Alias").EqualTo("create"));
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCaseSource("GetPermissionAlias_TestCaseSource")]
        public void GetPermissionByAlias(Func<Tuple<EntityRef, string>> entityRefFactory)
        {
            Tuple<EntityRef,string> entityRef = entityRefFactory();
            Assert.That(Permissions.GetPermissionByAlias(entityRef.Item1), Is.EqualTo(entityRef.Item2));
        }

        internal IEnumerable GetPermissionAlias_TestCaseSource()
        {           
            yield return new TestCaseData((Func<Tuple<EntityRef, string>>) (() => new Tuple<EntityRef, string>(Permissions.Read, Permissions.Read.Alias)));
            yield return new TestCaseData((Func<Tuple<EntityRef, string>>) (() => new Tuple<EntityRef, string>(Permissions.Modify, Permissions.Modify.Alias)));
            yield return new TestCaseData((Func<Tuple<EntityRef, string>>) (() => new Tuple<EntityRef, string>(Permissions.Delete, Permissions.Delete.Alias)));
            yield return new TestCaseData((Func<Tuple<EntityRef, string>>) (() => new Tuple<EntityRef, string>(Permissions.Create, Permissions.Create.Alias)));
            yield return new TestCaseData((Func<Tuple<EntityRef, string>>) (() => new Tuple<EntityRef, string>(new EntityRef(Permissions.Read.Entity.Id), Permissions.Read.Alias)));
            yield return new TestCaseData((Func<Tuple<EntityRef, string>>) (() => new Tuple<EntityRef, string>(new EntityRef(Permissions.Modify.Entity.Id), Permissions.Modify.Alias)));
            yield return new TestCaseData((Func<Tuple<EntityRef, string>>) (() => new Tuple<EntityRef, string>(new EntityRef(Permissions.Delete.Entity.Id), Permissions.Delete.Alias)));
            yield return new TestCaseData((Func<Tuple<EntityRef, string>>) (() => new Tuple<EntityRef, string>(new EntityRef(Permissions.Create.Entity.Id), Permissions.Create.Alias)));
            yield return new TestCaseData((Func<Tuple<EntityRef, string>>) (() => new Tuple<EntityRef, string>(new EntityRef(42), "42")));                          
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCaseSource( "MostSpecificPermission_TestCaseSource" )]
        public void MostSpecificPermission( Func<Tuple<EntityRef [ ], EntityRef>> callback )
        {
            // Note: calback to run inside request context
            var args = callback();
            EntityRef[] permissionIDs = args.Item1;
            EntityRef expected = args.Item2;

            long result = Permissions.MostSpecificPermission( permissionIDs.Select( e => e.Id ) );
            Assert.That( result, Is.EqualTo( expected.Id ) );
        }

        internal IEnumerable MostSpecificPermission_TestCaseSource( )
        {
            yield return new TestCaseData((Func<Tuple<EntityRef [ ], EntityRef>>) (() => new Tuple<EntityRef [ ], EntityRef>( new EntityRef [ ] { Permissions.Read }, Permissions.Read ) ) );
            yield return new TestCaseData((Func<Tuple<EntityRef [ ], EntityRef>>) (() => new Tuple<EntityRef [ ], EntityRef>( new EntityRef [ ] { Permissions.Modify }, Permissions.Modify ) ) );
            yield return new TestCaseData((Func<Tuple<EntityRef [ ], EntityRef>>) (() => new Tuple<EntityRef [ ], EntityRef>( new EntityRef [ ] { Permissions.Delete }, Permissions.Delete ) ) );
            yield return new TestCaseData((Func<Tuple<EntityRef [ ], EntityRef>>) (() => new Tuple<EntityRef [ ], EntityRef>( new EntityRef [ ] { Permissions.Create }, Permissions.Create ) ) );
            yield return new TestCaseData((Func<Tuple<EntityRef [ ], EntityRef>>) (() => new Tuple<EntityRef [ ], EntityRef>( new EntityRef [ ] { Permissions.Read, Permissions.Modify }, Permissions.Modify ) ) );
            yield return new TestCaseData((Func<Tuple<EntityRef [ ], EntityRef>>) (() => new Tuple<EntityRef [ ], EntityRef>( new EntityRef [ ] { Permissions.Read, Permissions.Modify, Permissions.Delete }, Permissions.Delete ) ) );
            yield return new TestCaseData((Func<Tuple<EntityRef [ ], EntityRef>>) (() => new Tuple<EntityRef [ ], EntityRef>( new EntityRef [ ] { Permissions.Read, Permissions.Modify, Permissions.Delete, Permissions.Create }, Permissions.Create ) ) );
            yield return new TestCaseData( ( Func<Tuple<EntityRef [ ], EntityRef>> ) ( ( ) => new Tuple<EntityRef [ ], EntityRef>( new EntityRef [ ] { Permissions.Modify, Permissions.Read }, Permissions.Modify ) ) );
        }
    }
}
