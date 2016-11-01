// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using EDC.ReadiNow.EditForm;

namespace EDC.ReadiNow.Test.EditForm
{
	[TestFixture]
	[RunWithTransaction]
	public class CustomEditFormTest
	{
		private EntityData FindEntity( EntityData entityData, long entityId )
		{
			if ( entityData.Id.Id == entityId )
			{
				return entityData;
			}

			return ( from relType in entityData.Relationships
			         from relInst in relType.Instances
			         select FindEntity( relInst.Entity, entityId ) ).FirstOrDefault( found => found != null );
		}

		[Test]
		[RunAsDefaultTenant]
		public void Test_GetFieldsRelationshipsControlsStructuredQueryString( )
		{
			EntityMemberRequest request = EntityRequestHelper.BuildRequest( "k:context.name" );

			var svc = new EntityInfoService( );
			EntityData result = svc.GetEntityData( new EntityRef( "console", "singleLineTextControl" ), request );

			Assert.IsNotNull( FindEntity( result, new EntityRef( "console", "uiContextHtml" ).Id ) );
		}

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetForm_ViewMode()
        {
            EntityData form = EditFormHelper.GetFormAsEntityData("console:resourceInfoEditForm", false);
            Assert.IsNotNull(form);
        }

        [Test]
        [RunAsDefaultTenant]
        public void Test_GetForm_DesignMode()
        {
            EntityData form = EditFormHelper.GetFormAsEntityData("console:resourceInfoEditForm", true);
            Assert.IsNotNull(form);
        }
	}
}