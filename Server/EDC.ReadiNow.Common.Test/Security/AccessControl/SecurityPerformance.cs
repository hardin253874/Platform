// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Security.AccessControl;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Security.AccessControl
{
	[TestFixture]
	[RunWithTransaction]
	[FailOnEvent]
	public class SecurityPerformance
	{
		[Test]
		[Explicit( "Long running performance test." )]
		[RunAsDefaultTenant]
		public void Test_LargeSecurityCheck( )
		{
			IList<EntityRef> entities;
			UserAccount userAccount;
			const int numberEntities = 1000;

			using ( new SecurityBypassContext( ) )
			{
				entities = Entity.GetInstancesOfType<Resource>( )
					.Select( r => new EntityRef( r ) )
					.Take( numberEntities )
					.ToList( );

				userAccount = new UserAccount
				{
					Name = "Test User " + Guid.NewGuid( )
				};

				userAccount.Save( );

				Factory.EntityAccessControlService.ClearCaches( );
			}

			using ( new SetUser( userAccount ) )
			using ( Profiler.Measure( "Access to {0} entities", numberEntities ) )
			{
				Factory.EntityAccessControlService.Check( entities, new[ ]
				{
					Permissions.Read
				} );
			}
		}

		/*
        [Test]
        [Explicit("Testing only")]
        [RunAsDefaultTenant]
        public void Test_ReportAccess()
        {
            UserAccount userAccount;
            Report report;
            const long entityId = 23025;

            userAccount = Entity.GetByName<UserAccount>("user").First();

            using (new ForceSecurityTraceContext(new long[] { entityId }))
            using (new SetUser(userAccount))
            {
                report = Entity.Get<Report>(entityId);
            }

            //TabRelationshipRenderControl tabRelationshipRenderControl;

            //tabRelationshipRenderControl = Entity.Get<TabRelationshipRenderControl>(22753);
            //IEntityRelationshipCollection<IEntity> relationships = tabRelationshipRenderControl.GetRelationships("console:relationshipDisplayReport", Direction.Forward);
        }

        [Test]
        [Explicit("Testing only")]
        [RunAsDefaultTenant]
        public void Test_RelationshipsForAccess()
        {
            // Follow these relationships from the report
            // -> 'Report to render' (console:reportToRender) to type 'Report Render Control' (console:reportRenderControl)
            //     -> 'Contained controls' (console:containedControlsOnForm) to type 'Structure Control On Form' (console:structureControlOnForm)
            //         -> 'Default edit form' (console:defaultEditForm) to type 'Type' (core:type)

            const string reportAlias = "core:report";
            const string reportToRenderAlias = "console:reportToRender";
            const string reportRenderControlAlias = "console:reportRenderControl";
            const string structureControlOnFormAlias = "console:structureControlOnForm";
            const string containedControlsOnFormAlias = "console:containedControlsOnForm";
            const string controlOnFormAlias = "console:controlOnForm";
            const string defaultEditFormAlias = "console:defaultEditForm";
            const string typeAlias = "core:type";
            const string customEditFormAlias = "console:customEditForm";
            Relationship reportToRenderRelationship;
            Relationship containedControlsOnFormRelationship;
            Relationship defaultEditFormRelationship;
            EntityMemberRequest entityMemberRequest;
            Report report;
            const long nonDefaultReportId = 23025;
            const long typeId = 22748;
            IList<EntityData> entitiesData;
            List<EntityRef> relatedEntitiesToCheck;

            // Ensure the relationships and secures flags are set correctly.
            reportToRenderRelationship = Entity.Get<Relationship>(reportToRenderAlias);
            Assert.That(reportToRenderRelationship, Has.Property("SecuresTo").True, 
                "reportToRenderAlias secures to");
            Assert.That(reportToRenderRelationship, Has.Property("FromType").Property("Alias").EqualTo(reportRenderControlAlias),
                "reportToRenderAlias from type");
            Assert.That(reportToRenderRelationship, Has.Property("ToType").Property("Alias").EqualTo(reportAlias),
                "reportToRenderAlias to type");
            containedControlsOnFormRelationship = Entity.Get<Relationship>(containedControlsOnFormAlias);
            Assert.That(containedControlsOnFormRelationship, Has.Property("SecuresTo").True, 
                "containedControlsOnFormAlias secures to");
            Assert.That(containedControlsOnFormRelationship, Has.Property("FromType").Property("Alias").EqualTo(structureControlOnFormAlias),
                "containedControlsOnFormAlias from type");
            Assert.That(containedControlsOnFormRelationship, Has.Property("ToType").Property("Alias").EqualTo(controlOnFormAlias),
                "containedControlsOnFormAlias to type");
            defaultEditFormRelationship = Entity.Get<Relationship>(defaultEditFormAlias);
            Assert.That(defaultEditFormRelationship, Has.Property("SecuresTo").True, 
                "defaultEditFormAlias secures to");
            Assert.That(defaultEditFormRelationship, Has.Property("FromType").Property("Alias").EqualTo(typeAlias),
                "defaultEditFormAlias from type");
            Assert.That(defaultEditFormRelationship, Has.Property("ToType").Property("Alias").EqualTo(customEditFormAlias),
                "defaultEditFormAlias to type");

            // Ensure the relationship exists.
            // report = Entity.Get<Report>(nonDefaultReportId);
            // Assert.That(report.GetRelationships(reportToRenderAlias, Direction.Forward), Is.Not.Empty);

            IList<Tuple<Relationship, Direction, EntityType, object>> x = new EntityMemberRequestFactory().GetAllSecuringRelationships(
                Entity.Get<EntityType>("console:relationshipControlOnForm"), false);

            // Ensure an entity member request gives those relationships
            using (
                MessageContext messageContext = new MessageContext(EntityAccessControlService.MessageName,
                    MessageContextBehavior.New | MessageContextBehavior.Capturing))
            {
                entityMemberRequest =
                    new EntityMemberRequestFactory().BuildEntityMemberRequest(Entity.Get<EntityType>(reportAlias));
                new SecuresFlagEntityAccessControlChecker().TraceEntityMemberRequest(entityMemberRequest);
                Console.Out.WriteLine(messageContext.GetMessage());
            } 
            Assert.That(entityMemberRequest.Relationships
                                           .Where(r => r.RelationshipTypeId.Id == reportToRenderRelationship.Id)
                                           .SelectMany(r => r.RequestedMembers.Relationships)
                                           .Where(r => r.RelationshipTypeId.Id == containedControlsOnFormRelationship.Id)
                                           .SelectMany(r => r.RequestedMembers.Relationships)
                                           .Where(r => r.RelationshipTypeId.Id == defaultEditFormRelationship.Id)
                                           .Select(r => Entity.Get<Relationship>(r.RelationshipTypeId).FromType.Alias),
                        Is.EquivalentTo(new[] { typeAlias }));

            // Run the entity member request and ensure we are checking the correct entities
            entitiesData = new EntityInfoService().GetEntitiesData(new[] {new EntityRef(nonDefaultReportId)}, entityMemberRequest)
                                                  .ToList();
            relatedEntitiesToCheck = Delegates
                .WalkGraph(
                    entitiesData,
                    entityData =>
                        entityData.Relationships.SelectMany(relType => relType.Entities))
                .Select(ed => ed.Id)
                .ToList();
            Assert.That(relatedEntitiesToCheck.Select(ed => ed.Id), Has.Exactly(1).EqualTo(typeId));

            //Assert.That(report.GetRelationships("console:reportToRender")
            //                  .SelectMany(r => Entity.GetRelationships(new EntityRef(r.Entity), new EntityRef("console:containedControlsOnForm"), Direction.Forward))
            //                  .SelectMany(p => p.Second.))
        }

        [Test]
        [RunAsDefaultTenant]
        [TestCase("core:report")]
        // [TestCase("console:customEditForm")]
        // [TestCase("console:structureControlOnForm")]
        public void Test_EntityMemberRequestCreation(string entityTypeAlias)
        {
            EntityMemberRequestFactory entityMemberRequestFactory;
            EntityMemberRequest entityMemberRequest;
            SecuresFlagEntityAccessControlChecker securesFlagEntityAccessControlChecker;

            entityMemberRequestFactory = new EntityMemberRequestFactory();
            securesFlagEntityAccessControlChecker = new SecuresFlagEntityAccessControlChecker();
            using (
                MessageContext messageContext = new MessageContext(EntityAccessControlService.MessageName,
                    MessageContextBehavior.New | MessageContextBehavior.Capturing))
            {
                entityMemberRequest =
                    entityMemberRequestFactory.BuildEntityMemberRequest(Entity.Get<EntityType>(entityTypeAlias));
                securesFlagEntityAccessControlChecker.TraceEntityMemberRequest(entityMemberRequest);
                Console.Out.WriteLine(messageContext.GetMessage());
            }      
        }

        [Test]
        [RunAsDefaultTenant]
        // [TestCase("console:customEditForm", false)]
        // [TestCase("console:customEditForm", true)]
        [TestCase("console:structureControlOnForm", false)]
        [TestCase("console:structureControlOnForm", true)]
        public void Test_GetAllSecuringRelationships(string entityTypeAlias, bool ancestorsOnly)
        {
            EntityMemberRequestFactory entityMemberRequestFactory;
            IList<Tuple<Relationship, Direction, EntityType, object>> result;

            entityMemberRequestFactory = new EntityMemberRequestFactory();
            result = entityMemberRequestFactory.GetAllSecuringRelationships(Entity.Get<EntityType>(entityTypeAlias), ancestorsOnly);
            foreach (Tuple<Relationship, Direction, EntityType, object> tuple in result)
            {
                Console.Out.WriteLine("-> '{0}' ({1}) to type '{2}' ({3})",
                            tuple.Item1.Name,
                            tuple.Item1.Id,
                            tuple.Item3.Name,
                            tuple.Item3.Id);       
            }
        }
        */
	}
}