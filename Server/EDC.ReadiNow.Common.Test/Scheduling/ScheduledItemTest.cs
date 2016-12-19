// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling;
using NUnit.Framework;
using Quartz;
using EDC.ReadiNow.Database;

namespace EDC.ReadiNow.Test.Scheduling
{
    [TestFixture]
	
    public class ScheduledItemTest
    {
        List<long> toDelete;

        [TestFixtureSetUp]
        public void SetUp()
        {
            //  Start a local scheduler so we are not relying on the service
            SchedulingHelper.Instance.Start();
          
            toDelete = new List<long>();
        }

        [TestFixtureTearDown]
        [RunAsDefaultTenant]
        public void CleanUp()
        {
            SchedulingHelper.Instance.Standby();

            using (new TenantAdministratorContext("EDC"))
            {
                var deleteList = toDelete.Distinct().ToList( );

				if ( deleteList.Count > 0 )
                {
                    Entity.Delete(deleteList);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [Timeout(60000)]
        [Description("Tests that the three schedule types fire as expected for the ScheduleItem. NOTE: This test will not function if an off-box sceduler is used.")]
        public void TestCronExecutes()
        {
            TestSchedule(SaveAndAddDelete(CreateCron()));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        [Timeout(60000)]
        [Description("Tests that the three schedule types fire as expected for the ScheduleItem. NOTE: This test will not function if an off-box sceduler is used.")]
        public void TestOneOffExecutes()
        {
            TestSchedule(SaveAndAddDelete(CreateOneOff()));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        //[Ignore]
        [Timeout(60000)]
        [Description("Tests that the three schedule types fire as expected for the ScheduleItem. NOTE: This test will not function if an off-box sceduler is used.")]
        public void TestDailyRepeatExecutes()
        {
            TestSchedule(SaveAndAddDelete(CreateDailyRepeat()));
        }

        void TestSchedule(Schedule schedule)
        {
            var myClass = CreateSyncActionClass();
            var action = CreateAction(myClass);

            var actionInstance = Entity.Create(action).Cast<ScheduledItem>();
            actionInstance.ScheduleForTrigger.Add(schedule);
            actionInstance.TriggerEnabled = true;

            actionInstance.Save();

            toDelete.Add(actionInstance.Id);

            //schedule.TriggersForSchedule.Add(actionInstance.As<ScheduledItem>());
            //schedule.Save();

            WaitForFire(actionInstance);
            //using (var mutex = SyncAction.CreateEventHandle((EntityRef)actionInstance))
            //{
            //    mutex.Reset();
            //    // wait for the scheduler to trigger it
            //    mutex.WaitOne();
            //}
        }


        [Test]
        [RunAsDefaultTenant]
        //[Ignore("Waiting to be fixed")]
        [Timeout(60000)]
        [Description("Tests that two triggers on the one schedule both fire - bug 22274")]
        public void TestTwoTriggersFiringCron_22274()
        {
            TestTwoTriggerOnSchedule(SaveAndAddDelete(CreateCron()));
        }

        [Test]
        [RunAsDefaultTenant]
        //[Ignore("Waiting to be fixed")]
        [Timeout(60000)]
        [Description("Tests that two triggers on the one schedule both fire - bug 22274")]
        public void TestTwoTriggersFiringDailyRepeat_22274()
        {
            TestTwoTriggerOnSchedule(SaveAndAddDelete(CreateDailyRepeat()));
        }

        [Test]
        [RunAsDefaultTenant]
        //[Ignore("Waiting to be fixed")]
        [Timeout(60000)]
        [Description("Tests that two triggers on the one schedule both fire - bug 22274")]
        public void TestTwoTriggersFiringOneOff_22274()
        {
            TestTwoTriggerOnSchedule(SaveAndAddDelete(CreateOneOff()));
        }


        void TestTwoTriggerOnSchedule(Schedule schedule)
        {

            var myClass = CreateSyncActionClass();
            var action = CreateAction(myClass);
            var actionInstance1 = Entity.Create(action).Cast<ScheduledItem>();
            var actionInstance2 = Entity.Create(action).Cast<ScheduledItem>();

            actionInstance1.ScheduleForTrigger.Add(schedule);
            actionInstance1.TriggerEnabled = true;
            actionInstance2.ScheduleForTrigger.Add(schedule);
            actionInstance2.TriggerEnabled = true;


            actionInstance1.Save();
            actionInstance2.Save();

            toDelete.Add(actionInstance1.Id);
            toDelete.Add(actionInstance2.Id);

            WaitForFire(actionInstance1);
            WaitForFire(actionInstance2);
        }

        void WaitForFire(ScheduledItem actionInstance1)
        {
            for (int i = 0; i < 30 * 2; i++)
            {
                Thread.Sleep(500);
                var res = Entity.GetByName<Resource>(actionInstance1.Id.ToString());   // The Id is the name of the created reosurce
                if (res.Count() > 0)
                {
                    return;
                }
            }

            Assert.Fail("Didn't run");
        }


        Schedule SaveAndAddDelete(Schedule schedule)
        {
            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                schedule.Save();

                ctx.CommitTransaction();
            }
            toDelete.Add(schedule.Id);
            return schedule;
        }


        Schedule CreateOneOff()
        {
            var now = DateTime.UtcNow;
            return new ScheduleOneOff { Name = "TestTwoTriggersFiring_22274 Schedule", ScheduleSpecificTime = now.AddSeconds(1) }.As<Schedule>();
        }

        Schedule CreateCron() 
        {
            return new ScheduleCron
                {
                    Name = "Cron",
                    CronDefinition = "0/5 * * * * ?", // fire every three seconds
                }.As<Schedule>();
        }
        
        Schedule CreateDailyRepeat() 
        {
            var now = DateTime.UtcNow;
            
            return new ScheduleDailyRepeat
                        {
                            Name = "Daily Repeat",
                            SdrTimeOfDay = new DateTime(1753, 01, 01, now.Hour, now.Minute, now.Second).AddSeconds(5),
                            SdrTimeZone = "Coordinated Universal Time"
                        }.As<Schedule>();
        }

     


        private Class CreateSyncActionClass()
        {
            // Workaround for a security demand issue. This really should say:
            //      var myClass = new Class(); 
            // This is only necessary because we are trying to create the ActionClass in code.
            var r = new Resource();
            r.Save();
            r.IsOfType.Add(Class.Class_Type);
            var myClass = r.As<Class>();

            myClass.AssemblyName = typeof(SyncAction).Assembly.FullName;
            myClass.TypeName = typeof(SyncAction).FullName;
            
            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                myClass.Save();

                ctx.CommitTransaction();
            }

            toDelete.Add(myClass.Id);

            return myClass;
        }


        private ScheduleAction CreateAction(Class myClass)
        {
            var action = new ScheduleAction() { Name = "MyAction", OnScheduleFire = myClass };
            action.Inherits.Add(Entity.Get<EntityType>("scheduledItem"));

            using (var ctx = DatabaseContext.GetContext(true, preventPostSaveActionsPropagating: true))
            {
                action.Save();

                ctx.CommitTransaction();
            }
            
            toDelete.Add(action.Id);
            return action;
        }
        
    }
}
