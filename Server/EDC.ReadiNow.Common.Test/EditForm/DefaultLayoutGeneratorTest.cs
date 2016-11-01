// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.EditForm;
using EDC.ReadiNow.Test;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.EditForm
{
	[TestFixture]
	[RunWithTransaction]
	public class DefaultLayoutGeneratorTest
	{
		private void CheckAllTempObjectMarkedAsCreate( EntityData entityData, HashSet<long> passedObjects = null )
		{
            if (passedObjects == null)
                passedObjects = new HashSet<long>();

			// prevent loops
			if ( passedObjects.Contains( entityData.Id.Id ) )
			{
				return;
			}

			passedObjects.Add( entityData.Id.Id );

			if ( entityData.Id.Id > 1000000 )
			{
				Assert.AreEqual( entityData.DataState, DataState.Create, "Temp item has Create datastate" );

				long isOfType = Entity.GetId( "core:isOfType" );
				foreach ( RelationshipData relationship in entityData.Relationships.Where( r => r.RelationshipTypeId.Id != isOfType ) )
				{
					foreach ( RelationshipInstanceData ri in relationship.Instances )
					{
						Assert.AreEqual( ri.DataState, DataState.Create, "Temp item relationship has Create data state" );
					}
				}
			}

			// Walk the rest of the tree
			foreach ( RelationshipData relationship in entityData.Relationships )
			{
				foreach ( RelationshipInstanceData relationshipInstance in relationship.Instances )
				{
					CheckAllTempObjectMarkedAsCreate( relationshipInstance.Entity, passedObjects );
				}
			}
		}

		[Test]
		[RunAsDefaultTenant]
		public void TestDefaultLayoutEntityDataCorrectDataState( )
		{
            EntityData form = EditFormHelper.GenerateDefaultFormForResourceType("core:person", false);
            Assert.IsNotNull(form);
            CheckAllTempObjectMarkedAsCreate(form);
		}

        [Test]
        [RunAsDefaultTenant]
        public void GenerateDefaultFormForResourceType_ViewMode()
        {
            EntityData form = EditFormHelper.GenerateDefaultFormForResourceType("test:allFields", false);
            Assert.IsNotNull(form);
            CheckAllTempObjectMarkedAsCreate(form);
        }

        [Test]
        [RunAsDefaultTenant]
        public void GenerateDefaultFormForResourceType_DesignMode()
        {
            EntityData form = EditFormHelper.GenerateDefaultFormForResourceType("test:allFields", true);
            Assert.IsNotNull(form);
            CheckAllTempObjectMarkedAsCreate(form);
        }

        [Test]
        [RunAsDefaultTenant]
        public void GenerateDefaultFormForResourceType_Has_CanCreateType_Field()
        {
            EntityData form = EditFormHelper.GenerateDefaultFormForResourceType("test:allFields", true);
            Assert.IsNotNull(form);
            bool canCreateType;
            bool.TryParse(form.GetRelationship("console:typeToEditWithForm").Instances[0].Entity.GetField("canCreateType").Value.ValueString, out canCreateType);
            Assert.IsTrue(canCreateType);
        }
	}
}