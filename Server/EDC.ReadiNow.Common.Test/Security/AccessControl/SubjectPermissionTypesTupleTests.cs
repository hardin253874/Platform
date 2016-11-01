// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
    [TestFixture]
	[RunWithTransaction]
    [FailOnEvent]
    public class SubjectPermissionTypesTupleTests
    {
        [Test]
        public void Test_Creation()
        {
            SubjectPermissionTypesTuple tuple;
            const long testSubjectId = 1;
            EntityRef testPermissionId = new EntityRef( 1 );
            long [ ] testEntityTypes = new long[] {5, 4, 3};

            tuple = new SubjectPermissionTypesTuple(testSubjectId, testPermissionId, testEntityTypes);
            Assert.That(tuple, Has.Property("SubjectId").EqualTo(testSubjectId));
            Assert.That(tuple, Has.Property("PermissionId").EqualTo(testPermissionId.Id));
            Assert.That(tuple, Has.Property("EntityTypes").EquivalentTo(testEntityTypes.OrderBy(x => x)));
        }

        [Test]
        public void Test_Creation_NullPermission( )
        {
            SubjectPermissionTypesTuple tuple;
            const long testSubjectId = 1;
            EntityRef testPermissionId = null;
            long [ ] testEntityTypes = new long [ ] { 5, 4, 3 };

            tuple = new SubjectPermissionTypesTuple( testSubjectId, testPermissionId, testEntityTypes );
            Assert.That( tuple, Has.Property( "SubjectId" ).EqualTo( testSubjectId ) );
            Assert.That( tuple, Has.Property( "PermissionId" ).EqualTo( -1 ) );
            Assert.That( tuple, Has.Property( "EntityTypes" ).EquivalentTo( testEntityTypes.OrderBy( x => x ) ) );
        }

        [Test]
        public void Test_Creation_NullEntityTypes()
        {
            SubjectPermissionTypesTuple tuple;
            const long testSubjectId = 1;
            const long testPermissionId = 2;

            tuple = new SubjectPermissionTypesTuple( testSubjectId, testPermissionId, null );
            Assert.That( tuple, Has.Property( "SubjectId" ).EqualTo( testSubjectId ) );
            Assert.That( tuple, Has.Property( "PermissionId" ).EqualTo( testPermissionId ) );
            Assert.That( tuple, Has.Property( "EntityTypes" ).Null );
        }

        [Test]
        [TestCase(1, 2, new long[]{ 3, 4}, true)]
        [TestCase(100, 2, new long[] { 3, 4 }, false)]
        [TestCase(1, 200, new long[] { 3, 4 }, false)]
        [TestCase(1, 2, new long[] { 3 }, false)]
        public void Test_Equal(long subjectId, long permissionId, long[] entityTypes, bool expectedResult)
        {
            SubjectPermissionTypesTuple tuple1;
            SubjectPermissionTypesTuple tuple2;
            const long testSubjectId = 1;
            const long testPermissionId = 2;
            long[] testEntityTypes = new long[] { 3, 4 };

            tuple1 = new SubjectPermissionTypesTuple(testSubjectId, testPermissionId, testEntityTypes);
            tuple2 = new SubjectPermissionTypesTuple(subjectId, permissionId, entityTypes);
            Assert.That(tuple1.Equals(tuple2), Is.EqualTo(expectedResult));
        }
    }
}
