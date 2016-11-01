// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Autofac;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Security.AccessControl.Diagnostics;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Security.AccessControl;

namespace EDC.ReadiNow.Test.Security.AccessControl.Diagnostics
{
    [TestFixture]
    public class TypeAccessReasonServiceTests
    {
        /// <summary>
        /// Service under test
        /// </summary>
        public ITypeAccessReasonService Service => Factory.Current.Resolve<ITypeAccessReasonService>( );

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetTypeAccessReasons_InvalidSubjectId( )
        {
            IEntity dummy;
            ITypeAccessReasonService service = Factory.Current.Resolve<ITypeAccessReasonService>( );

            using ( DatabaseContext.GetContext( true ) )
            {
                dummy = Entity.Create<EntityType>( ); // or whatever
                dummy.Save( );

                long badSubjectId = dummy.Id;
                Assert.That( ( ) => Service.GetTypeAccessReasons( badSubjectId, TypeAccessReasonSettings.Default ),
                    Throws.ArgumentException.And.Property( "ParamName" ).EqualTo( "subjectId" ) );
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        [RunAsDefaultTenant]
        public void Test_GetTypeAccessReasons_Everyone( bool advancedTypes)
        {
            var results = Test_GetTypeAccessReasonsImpl( "Everyone", false, advancedTypes);
            string info = string.Join( "\n", results );

            Assert.That(info, Is.StringContaining("Object=User Account"));
            Assert.That(info, Is.StringContaining("Object=Task"));

            if ( advancedTypes )
            {
                Assert.That(info, Is.StringContaining("Object=Resource"));
                Assert.That(info, Is.StringContaining("Object=Approval Task"));
            }
            else
            {
                Assert.That(info, Is.Not.StringContaining("Object=Resource"));
                Assert.That(info, Is.Not.StringContaining("Object=Approval Task"));
            }
        }

        [Test]
        [Explicit]
        [TestCase( "Everyone" )]
        [TestCase( "Deans" )]
        [TestCase( "Staff" )]
        [TestCase( "Self Serve" )]
        [TestCase( "Students" )]
        [TestCase( "Administrators" )]
        [RunAsDefaultTenant]
        public void Test_GetTypeAccessReasons( string roleName )
        {
            Test_GetTypeAccessReasonsImpl( roleName, true );
        }

        public List<string> Test_GetTypeAccessReasonsImpl( string roleName, bool writeResults, bool advancedTypes = true )
        {
            Role role = Entity.GetByName<Role>( roleName ).Single( );

            IReadOnlyList<AccessReason> result;
            ITypeAccessReasonService service = Factory.Current.Resolve<ITypeAccessReasonService>( );

            using ( DatabaseContext.GetContext( true ) )
            {
                result = service.GetTypeAccessReasons( role.Id, new TypeAccessReasonSettings(advancedTypes) );
                Assert.That( result, Is.Not.Empty );

                List<string> info = new List<string>( );

                foreach ( AccessReason reason in result)
                {
                    EntityType type = Entity.Get<EntityType>( reason.TypeId );
                    Subject subject = Entity.Get<Subject>( reason.SubjectId );

                    string line = $"Subject={subject?.Name}\tObject={type?.Name}\tScope={reason.AccessRuleScope}\tPerms={reason.PermissionsText}\tDesc={reason.Description}";
                    info.Add( line );
                }
                info.Sort( );
                if ( writeResults )
                {
                    info.ForEach( Console.WriteLine );
                }
                return info;
            }
        }

        [Test]
        [TestCase( "fwd,securesTo", ExpectedResult = "Secured via 'ExplicitType' object: 'ToName' relationship" )] // e.g. "Secured via Disaster Plan Steps relationship"
        [TestCase( "fwd,securesFrom", ExpectedResult = null )]
        [TestCase( "rev,securesFrom", ExpectedResult = "Secured via 'ExplicitType' object: 'FromName' relationship" )]
        [TestCase( "rev,securesTo", ExpectedResult = null )]
        [TestCase( "fwd,securesTo,fromAncestor", ExpectedResult = "Secured via 'ExplicitType' object: 'ToName' relationship" )]
        [TestCase( "fwd,securesTo,fromDerived", ExpectedResult = "Secured via 'ExplicitType' object: 'ToName' relationship" )]
        [TestCase( "fwd,securesTo,toAncestor", ExpectedResult = "Inherits access from 'ToAncestor' object" )]
        [TestCase( "fwd,securesTo,toDerived", ExpectedResult = null )]
        [RunAsDefaultTenant]
        public string Test_Secured_By_Relationship( string settings )
        {
            // Note: ExpectedResult is null when we expect no grant

            Definition typeExplicitlyGranted = null; // needs to be definition, otherwise gets filtered from the report.
            Definition typeToBeChecked = null;
            Definition fromType = null;
            Definition toType = null;
            Relationship rel = null;
            Role role = null;
            List<IEntity> cleanup = new List<IEntity>( );

            try
            {
                EntityType editableResource = UserResource.UserResource_Type;
                
                // Setup scenario
                typeExplicitlyGranted = Entity.Create<Definition>( );
                typeExplicitlyGranted.Name = "ExplicitType";
                typeExplicitlyGranted.Inherits.Add( editableResource );
                typeExplicitlyGranted.Save( );
                cleanup.Add( typeExplicitlyGranted );

                typeToBeChecked = Entity.Create<Definition>( );
                typeToBeChecked.Name = "CheckedType";
                typeToBeChecked.Inherits.Add( editableResource );
                typeToBeChecked.Save( );
                cleanup.Add( typeToBeChecked );

                // Set up relationship
                rel = Entity.Create<Relationship>( );
                rel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
                rel.Name = "RelName";
                rel.ToName = "ToName";
                rel.FromName = "FromName";
                cleanup.Add( rel );

                if ( settings.Contains("fwd") )
                {
                    fromType = typeExplicitlyGranted;
                    toType = typeToBeChecked;
                }
                else if ( settings.Contains( "rev" ) )
                {
                    fromType = typeToBeChecked;
                    toType = typeExplicitlyGranted;
                }
                if ( settings.Contains("fromAncestor") || settings.Contains( "fromDerived" ) )
                {
                    var oldFrom = fromType;
                    fromType = new Definition( );
                    if ( settings.Contains( "fromAncestor" ) )
                    {
                        fromType.Name = "FromAncestor";
                        fromType.Inherits.Add( editableResource );
                        fromType.DerivedTypes.Add( oldFrom.Cast<EntityType>( ) );
                    }
                    else
                    {
                        fromType.Name = "FromDerived";
                        fromType.Inherits.Add( oldFrom.Cast<EntityType>( ) );
                    }
                    fromType.Save( );
                    cleanup.Add( fromType );
                }
                if ( settings.Contains( "toAncestor" ) || settings.Contains( "toDerived" ) )
                {
                    var oldTo = toType;
                    toType = new Definition( );
                    if ( settings.Contains( "toAncestor" ) )
                    {
                        toType.Name = "ToAncestor";
                        toType.Inherits.Add( editableResource );
                        toType.DerivedTypes.Add( oldTo.Cast<EntityType>( ) );
                    }
                    else
                    {
                        toType.Name = "ToDerived";
                        toType.Inherits.Add( oldTo.Cast<EntityType>( ) );
                    }
                    toType.Save( );
                    cleanup.Add( toType );
                }

                rel.FromType = fromType.Cast<EntityType>( );
                rel.ToType = toType.Cast<EntityType>( );
                rel.SecuresTo = settings.Contains( "securesTo" );
                rel.SecuresFrom = settings.Contains( "securesFrom" );
                rel.Save( );

                // Set up role
                role = new Role { Name = "Role1" };
                role.Save( );
                cleanup.Add( role );
                new AccessRuleFactory( ).AddAllowReadQuery( role.As<Subject>( ), typeExplicitlyGranted.As<SecurableEntity>( ), TestQueries.Entities( typeExplicitlyGranted ).ToReport( ) );


                // Tests
                ITypeAccessReasonService service = Factory.Current.Resolve<ITypeAccessReasonService>( );
                var reasons = service.GetTypeAccessReasons( role.Id, TypeAccessReasonSettings.Default );

                // Sanity check for the type we explicitly grant accses
                Assert.That( reasons.Where( r => r.TypeId == typeExplicitlyGranted.Id ).Count( ), Is.EqualTo( 1 ), "typeExplicitlyGranted" );
                AccessReason reason1 = reasons.Single( r => r.TypeId == typeExplicitlyGranted.Id );

                Assert.That( reason1.AccessRuleScope, Is.EqualTo( AccessRuleScope.AllInstances ), "rule1 scope" );
                Assert.That( reason1.PermissionsText, Is.EqualTo( "Read" ), "rule1 perms" );
                Assert.That( reason1.SubjectId, Is.EqualTo( role.Id ), "rule1 subject" );
                Assert.That( reason1.Description, Is.EqualTo( "Access rule: 'Role1' accessing 'ExplicitType'" ), "rule1 desc" );

                AccessReason reason2 = reasons.FirstOrDefault( r => r.TypeId == typeToBeChecked.Id );

                if ( reason2 != null )
                {
                    Assert.That( reason2.AccessRuleScope, Is.EqualTo( AccessRuleScope.SecuredRelationship ), "rule2 scope" );
                    Assert.That( reason2.PermissionsText, Is.EqualTo( "Read" ), "rule2 perms" );
                    Assert.That( reason2.SubjectId, Is.EqualTo( role.Id ), "rule2 subject" );
                }

                return reason2?.Description; // actual string being tested
            }
            finally
            {
                Entity.Delete( cleanup.Select( e => new EntityRef( e ) ) );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_Secured_By_Relationship_Chained( )
        {
            // Note: ExpectedResult is null when we expect no grant

            Definition typeExplicitlyGranted = null; // needs to be definition, otherwise gets filtered from the report.
            Definition typeMiddle = null;
            Definition typeToBeChecked = null;
            Relationship rel = null;
            Relationship rel2 = null;
            Role role = null;
            List<IEntity> cleanup = new List<IEntity>( );

            try
            {
                EntityType editableResource = UserResource.UserResource_Type;

                // Setup scenario
                typeExplicitlyGranted = Entity.Create<Definition>( );
                typeExplicitlyGranted.Name = "ExplicitType";
                typeExplicitlyGranted.Inherits.Add( editableResource );
                typeExplicitlyGranted.Save( );
                cleanup.Add( typeExplicitlyGranted );

                typeMiddle = Entity.Create<Definition>( );
                typeMiddle.Name = "MiddleType";
                typeMiddle.Inherits.Add( editableResource );
                typeMiddle.Save( );
                cleanup.Add( typeMiddle );

                typeToBeChecked = Entity.Create<Definition>( );
                typeToBeChecked.Name = "CheckedType";
                typeToBeChecked.Inherits.Add( editableResource );
                typeToBeChecked.Save( );
                cleanup.Add( typeToBeChecked );

                // Set up relationship
                rel = Entity.Create<Relationship>( );
                rel.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
                rel.Name = "RelName1";
                rel.ToName = "ToName1";
                rel.FromName = "FromName1";
                rel.FromType = typeExplicitlyGranted.Cast<EntityType>( );
                rel.ToType = typeMiddle.Cast<EntityType>( );
                rel.SecuresTo = true;
                rel.Save( );
                cleanup.Add( rel );

                rel2 = Entity.Create<Relationship>( );
                rel2.Cardinality_Enum = CardinalityEnum_Enumeration.ManyToMany;
                rel2.Name = "RelName2";
                rel2.ToName = "ToName2";
                rel2.FromName = "FromName2";
                rel2.ToType = typeMiddle.Cast<EntityType>( );
                rel2.FromType = typeToBeChecked.Cast<EntityType>( );
                rel2.SecuresFrom = true;
                rel2.Save( );
                cleanup.Add( rel2 );

                // Set up role
                role = new Role { Name = "Role1" };
                role.Save( );
                cleanup.Add( role );
                new AccessRuleFactory( ).AddAllowReadQuery( role.As<Subject>( ), typeExplicitlyGranted.As<SecurableEntity>( ), TestQueries.Entities( typeExplicitlyGranted ).ToReport( ) );


                // Tests
                ITypeAccessReasonService service = Factory.Current.Resolve<ITypeAccessReasonService>( );
                var reasons = service.GetTypeAccessReasons( role.Id, TypeAccessReasonSettings.Default );

                // Sanity check for the type we explicitly grant accses
                Assert.That( reasons.Where( r => r.TypeId == typeExplicitlyGranted.Id ).Count( ), Is.EqualTo( 1 ), "typeExplicitlyGranted" );

                AccessReason reason1 = reasons.Single( r => r.TypeId == typeExplicitlyGranted.Id );
                AccessReason reason2 = reasons.Single( r => r.TypeId == typeMiddle.Id );
                AccessReason reason3 = reasons.Single( r => r.TypeId == typeToBeChecked.Id );

                Assert.That( reason1.Description, Is.EqualTo( "Access rule: 'Role1' accessing 'ExplicitType'" ), "rule1 desc" );
                Assert.That( reason2.Description, Is.EqualTo( "Secured via 'ExplicitType' object: 'ToName1' relationship" ), "rule2 desc" );
                Assert.That( reason3.Description, Is.EqualTo( "Secured via 'ExplicitType' object: 'ToName1' -> 'FromName2' relationships" ), "rule3 desc" );
            }
            finally
            {
                Entity.Delete( cleanup.Select( e => new EntityRef( e ) ) );
            }
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_IncludedRoles( )
        {
            // Note: ExpectedResult is null when we expect no grant

            Definition type = null; // needs to be definition, otherwise gets filtered from the report.
            Role parentRole = null;
            Role childRole = null;
            List<IEntity> cleanup = new List<IEntity>( );

            try
            {
                EntityType editableResource = UserResource.UserResource_Type;

                // Setup scenario
                type = Entity.Create<Definition>( );
                type.Name = Guid.NewGuid().ToString( );
                type.Inherits.Add( editableResource );
                type.Save( );
                cleanup.Add( type );

                // Set up roles
                parentRole = Entity.Create<Role>( );
                childRole = Entity.Create<Role>( );
                parentRole.IncludesRoles.Add( childRole );
                parentRole.Save( );

                cleanup.Add( parentRole );
                cleanup.Add( childRole );

                new AccessRuleFactory( ).AddAllowReadQuery( parentRole.As<Subject>( ), type.As<SecurableEntity>( ), TestQueries.Entities( type ).ToReport( ) );
                new AccessRuleFactory( ).AddAllowModifyQuery( childRole.As<Subject>( ), type.As<SecurableEntity>( ), TestQueries.Entities( type ).ToReport( ) );

                // Tests
                ITypeAccessReasonService service = Factory.Current.Resolve<ITypeAccessReasonService>( );
                var reasons = service.GetTypeAccessReasons( childRole.Id, TypeAccessReasonSettings.Default );

                Assert.That( reasons.Where( reason => reason.SubjectId == WellKnownAliases.CurrentTenant.EveryoneRole ).Count(), Is.GreaterThan( 0 ) );

                // Sanity check for the type we explicitly grant accses
                AccessReason childReason = reasons.SingleOrDefault( r => r.SubjectId == childRole.Id );
                AccessReason parentReason = reasons.SingleOrDefault( r => r.SubjectId == parentRole.Id );

                Assert.That( childReason, Is.Not.Null, "child reason" );
                Assert.That( parentReason, Is.Not.Null, "parent reason" );
                Assert.That( childReason.TypeId, Is.EqualTo(type.Id), "child reason type" );
                Assert.That( parentReason.TypeId, Is.EqualTo( type.Id ), "parent reason type" );
            }
            finally
            {
                Entity.Delete( cleanup.Select( e => new EntityRef( e ) ) );
            }
        }
    }
}
