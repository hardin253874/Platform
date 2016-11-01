// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.Activities.Test
{
	[TestFixture]
	public class SetChoiceImplementationTest : TestBase
	{
		[Test]
		[RunAsDefaultTenant]
		public void SetChoice( )
		{
			var trigger = new WfTriggerUserUpdatesResource( );

			trigger.Save( );

			ToDelete.Add( trigger.Id );


			var setAction = new SetChoiceActivity( );
			setAction.Save( );
			ToDelete.Add( setAction.Id );

			var setChoiceAs = setAction.As<WfActivity>( );

			ActivityImplementationBase nextActivity = setChoiceAs.CreateWindowsActivity( );

			var args = new Dictionary<string, object>
			{
				{
					"Resource to Update", trigger
				},
				{
					"Field to Update", new EntityRef( "core:triggeringCondition" ).Entity
				},
				{
					"New Value", new EntityRef( "core:triggeredOnEnumCreate" ).Entity
				},
			};

			RunActivity( nextActivity, args );

			trigger = Entity.Get<WfTriggerUserUpdatesResource>( trigger.Id );

			Assert.IsNotNull( trigger.TriggeringCondition, "Triggering condition should be set" );
			Assert.AreEqual( "core:triggeredOnEnumCreate", trigger.TriggeringCondition.Alias );

			args = new Dictionary<string, object>
			{
				{
					"Resource to Update", trigger
				},
				{
					"Field to Update", new EntityRef( "core:triggeringCondition" ).Entity
				},
				{
					"New Value", new EntityRef( "core:triggeredOnEnumUpdate" ).Entity
				},
				{
					"Replace Existing Values", false
				} // should ignore this value
			};

			RunActivity( nextActivity, args );

			Assert.IsNotNull( trigger.TriggeringCondition, "Triggering condition should be set" );
			Assert.AreEqual( "core:triggeredOnEnumUpdate", trigger.TriggeringCondition.Alias );
		}

		[Test]
		[RunAsDefaultTenant]
		public void SetChoiceMultiple( )
		{
			var sch = new ScheduleDailyRepeat
			{
				Name = "Test sch" + DateTime.Now
			};


			sch.Save( );

			// _toDelete.Add(sch.Id);


			var setAction = new SetChoiceActivity( );
			setAction.Save( );
			ToDelete.Add( setAction.Id );

			var setChoiceAs = setAction.As<WfActivity>( );

			ActivityImplementationBase nextActivity = setChoiceAs.CreateWindowsActivity( );

			var dayOfWeekRef = ( EntityRef ) "core:sdrDayOfWeek";

			var args = new Dictionary<string, object>
			{
				{
					"Resource to Update", sch
				},
				{
					"Field to Update", dayOfWeekRef.Entity
				},
				{
					"New Value", new EntityRef( "core:dowSunday" ).Entity
				},
			};

			RunActivity( nextActivity, args );

			sch = Entity.Get<ScheduleDailyRepeat>( sch.Id );

			IEntityRelationshipCollection<IEntity> dowRefs = sch.GetRelationships( dayOfWeekRef );
			Assert.AreEqual( 1, dowRefs.Count( ), "has been set" );
			Assert.IsTrue( dowRefs.Any( w => w.Entity.Alias == "dowSunday" ) );

			args = new Dictionary<string, object>
			{
				{
					"Resource to Update", sch
				},
				{
					"Field to Update", dayOfWeekRef.Entity
				},
				{
					"New Value", new EntityRef( "core:dowMonday" ).Entity
				},
				{
					"Replace Existing Values", false
				}
			};

			RunActivity( nextActivity, args );

			sch = Entity.Get<ScheduleDailyRepeat>( sch.Id );
			dowRefs = sch.GetRelationships( dayOfWeekRef );
			Assert.AreEqual( 2, dowRefs.Count( ), "has been added" );
			Assert.IsTrue( dowRefs.Any( w => w.Entity.Alias == "dowMonday" ) );

			args = new Dictionary<string, object>
			{
				{
					"Resource to Update", sch
				},
				{
					"Field to Update", dayOfWeekRef.Entity
				},
				{
					"New Value", new EntityRef( "core:dowTuesday" ).Entity
				},
				{
					"Replace Existing Values", true
				}
			};

			RunActivity( nextActivity, args );

			sch = Entity.Get<ScheduleDailyRepeat>( sch.Id );
			dowRefs = sch.GetRelationships( dayOfWeekRef );
			Assert.AreEqual( 1, dowRefs.Count( ), "has been reset" );
			Assert.IsTrue( dowRefs.Any( w => w.Entity.Alias == "dowTuesday" ) );
		}
	}
}