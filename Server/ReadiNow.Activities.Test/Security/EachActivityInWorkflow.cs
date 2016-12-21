// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Test.Security.AccessControl;
using EDC.ReadiNow.Security.AccessControl;
using EDC.ReadiNow.Security;

namespace EDC.SoftwarePlatform.Activities.Test.Security
{
    [TestFixture]
    //[Category("ExtendedTests")]
    [Category("WorkflowTests")]
    public class EachActivityInWorkflow
    {
        RunAsDefaultTenant _ratAttrib;

        UserAccount _userAccount = null;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _ratAttrib = new RunAsDefaultTenant();          // Explicitly calling the RunAsDefaultTenant attribute so that we can set up some common objects to speed up the test.

            _ratAttrib.BeforeTest(null);

            _userAccount = Entity.Create<UserAccount>();
            _userAccount.Name = "Test user " + Guid.NewGuid().ToString();
            _userAccount.Save();


            new AccessRuleFactory().AddAllowByQuery(
                _userAccount.As<Subject>(),
                Workflow.Workflow_Type.As<SecurableEntity>(),
                Permissions.Read.ToEnumerable(),
                TestQueries.WorkflowWithName("A").ToReport());

            new AccessRuleFactory().AddAllowByQuery(
                 _userAccount.As<Subject>(),
                 Resource.Resource_Type.As<SecurableEntity>(),
                 new EntityRef("core:read").ToEnumerable(),
                 TestQueries.EntitiesWithName("Readable").ToReport());

            new AccessRuleFactory().AddAllowByQuery(
                _userAccount.As<Subject>(),
                Resource.Resource_Type.As<SecurableEntity>(),
                new EntityRef("core:modify").ToEnumerable(),
                TestQueries.EntitiesWithName("Writable").ToReport());

            new AccessRuleFactory().AddAllowByQuery(
                _userAccount.As<Subject>(),
                Resource.Resource_Type.As<SecurableEntity>(),
                new EntityRef("core:create").ToEnumerable(),
                TestQueries.EntitiesWithName("Creatable").ToReport());

            new AccessRuleFactory().AddAllowByQuery(
                _userAccount.As<Subject>(),
                Resource.Resource_Type.As<SecurableEntity>(),
                new EntityRef("core:delete").ToEnumerable(),
                TestQueries.EntitiesWithName("Deletable").ToReport());

            new AccessRuleFactory().AddAllowByQuery(
                _userAccount.As<Subject>(),
                Resource.Resource_Type.As<SecurableEntity>(),
                new EntityRef("core:read").ToEnumerable(),
                TestQueries.EntitiesWithName("Deletable").ToReport());

        }

        [TestFixtureTearDown]

        public void TearDown()
        {
            _userAccount.Delete();

            _ratAttrib.AfterTest(null);
        }

        void TestWorkflow(Func<Workflow> generateWf, Action<WorkflowRun> testWf = null)
        {

            var wf = generateWf();

            wf.Name = "A";
            wf.Save();

 
            using (new SetUser(_userAccount))
            {
                var dummyWf = Entity.Get<Workflow>(wf.Id);
                

                var run = (TestBase.RunWorkflow(wf));

                if (testWf != null)
                    testWf(run);
            }
        }

        //
        // Positive tests
        //

        void DefaultTest(WorkflowRun run)
        {
            Assert.That(run.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunCompleted));  // user can see the run status
            Assert.That(run.RunLog.Count, Is.GreaterThanOrEqualTo(0));                                               // user can see the run log for the workflow
        }

        [Test]
        [RunWithTransaction()]
        [TestCaseSource("PositiveCases")]
        [Description("Create a workflow for each activity type containing just the activity. Give a user permission to read the workflow and then run the workflow and check the status as that user. (Note that displayForm is not included in this test.)")]
        public void Activity_TestPositive(Func<Workflow> wfFactory)
        {
            TestWorkflow(wfFactory, DefaultTest);
        }

        static Func<Workflow>[] PositiveCases =

            new Func<Workflow>[] { 
                () =>   // log
                    Entity
                    .Create<Workflow>()
                    .AddDefaultExitPoint()
                    .AddLog("Log", "Log message"),

                () =>   // Assign to var
                    Entity
                    .Create<Workflow>()
                    .AddDefaultExitPoint()
                    .AddVariable<StringArgument>("v1")
                    .AddAssignToVar("Assign to var", "'my string'", "v1"),

                () => // update field
                {
                    var entityType = Entity.Create<EntityType>();
                    entityType.Inherits.Add(UserResource.UserResource_Type);
                    entityType.Save();

                    return UpdateFieldWfFactory(CreateWritable(entityType), Resource.Name_Field.As<Field>(), "'New name'")();
                },
                () =>   // create 
                {
                    var definition = CreateCreatable(Definition.Definition_Type).As<Definition>();
                    definition.Inherits.Add(UserResource.UserResource_Type);
                    definition.Save();

                    var wf =
                        Entity
                        .Create<Workflow>()
                        .AddDefaultExitPoint()
                        .AddVariable<ResourceArgument>("vDefinition", null, Definition.Definition_Type)
                        .AddEntityExpressionToVariable("vDefinition", definition.Id)
                        .AddCreate("Create", "[vDefinition]");

                    return wf;
                },

                () =>   // clone 
                {
                    var definition = CreateCreatable(Definition.Definition_Type).As<Definition>();
                    definition.Inherits.Add(Resource.Resource_Type);
                    definition.Save();

                    var resource = Entity.Create(definition.Id).As<Resource>();
                    resource.Name = "Readable";
                    resource.Save();

                    var wf =
                        Entity
                        .Create<Workflow>()
                        .AddDefaultExitPoint()
                        .AddVariable<ResourceArgument>("vRes")
                        .AddEntityExpressionToVariable("vRes", resource.Id)
                        .AddClone("Create", "[vRes]");

                    return wf;
                },
                   
                

                () =>   // delete 
                {
                    var entityType = Entity.Create<EntityType>();
                    entityType.Inherits.Add(UserResource.UserResource_Type);
                    entityType.Save();

                    var resource = CreateDeletable(entityType);                    
                    resource.Save();

                    var wf =
                        Entity
                        .Create<Workflow>()
                        .AddDefaultExitPoint()
                        .AddVariable<ResourceArgument>("vRes")
                        .AddEntityExpressionToVariable("vRes", resource.Id)
                        .AddDelete("Delete", "[vRes]");

                    return wf;
                },

	            ( ) =>
	            {
		            var definition = CreateCreatable( Definition.Definition_Type ).As<Definition>( );
		            definition.Inherits.Add( Resource.Resource_Type );
		            definition.Save( );

		            // getRecords 
		            var wf = 
						Entity
			            .Create<Workflow>( )
			            .AddDefaultExitPoint( )
			            .AddVariable<ResourceArgument>( "vDef", null, Resource.Resource_Type )
			            .AddEntityExpressionToVariable( "vDef", definition.Id )
			            .AddGetRecords( "getRecords", "[vDef]" );

					return wf;
	            },
                
                // MISSING GetRecords using a report

                () =>   // foreach 
               
                        Entity
                        .Create<Workflow>()
                        .AddDefaultExitPoint()
                        .AddForEach("foreach", "all(Folder)", Folder.Folder_Type)
                        .AddLog("log", "loop message", "foreach",  "Loop")
                        .AddTransition("log", "foreach"),

                                        
                () =>   // switch 
               
                        Entity
                        .Create<Workflow>()
                        .AddDefaultExitPoint()
                        .AddSwitch("foreach", "'first'", new string[]{"first", "second"})

                // displayFormActivity is more complicated and needs it's own test.
                // decisionActivity is deprecated - yet to be removed from code
            };



        [Test]
        [RunWithTransaction()]
        public void DisplayForm_TestPositive()
        {
            var visiblePerson = CreateVisible(Person.Person_Type);
            var visibleForm = CreateVisible(CustomEditForm.CustomEditForm_Type);

            var wfFactory = DisplayFormWfFactory(visiblePerson, visibleForm);

            TestWorkflow(wfFactory, (run) =>
                {
                    Assert.That(run.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunPaused));  // user can see the run status
                }
            );
        }

        //
        // Negative tests
        //

        [Test]
        [RunWithTransaction()]
        public void UpdateField_TestNegative_Resource()
        {
            var hiddenRes = CreateHidden(Resource.Resource_Type);
            var wfFactory = UpdateFieldWfFactory(hiddenRes, Resource.Name_Field.As<Field>(), "'New Name'");
            
            TestWorkflow(wfFactory,  
                (run) => TestFailWithSecurityError(run)
               );
        }

        // TEST NOT VALID
        //[Test]
        //[RunAsDefaultTenant]
        //[RunWithTransaction()]
        //public void DisplayForm_TestNegative_UserAccount()
        //{
        //    var hiddenUserAccount = CreateHidden(UserAccount.UserAccount_Type);
        //    var visibleForm = CreateVisible(CustomEditForm.CustomEditForm_Type);

        //    var wfFactory = DisplayFormWfFactory(hiddenUserAccount, visibleForm);

        //    TestWorkflow(wfFactory, (run) => TestFailWithSecurityError(run)
        //       );
        //}

        void TestFailWithSecurityError(WorkflowRun run)
        {
            Assert.That(run.WorkflowRunStatus_Enum, Is.EqualTo(WorkflowRunState_Enumeration.WorkflowRunFailed));  // user can see the run status
            Assert.That(run.ErrorLogEntry.Description, Is.StringContaining("Security violation"));  
        }

        //TEST NOT VALID
        //[Test]
        //[RunAsDefaultTenant]
        //[RunWithTransaction()]
        //public void DisplayForm_TestNegative_Form()
        //{
        //    var visibleUserAccount = CreateVisible(UserAccount.UserAccount_Type);
        //    var hiddenForm = CreateHidden(CustomEditForm.CustomEditForm_Type);
            
        //    var wfFactory = DisplayFormWfFactory(visibleUserAccount, hiddenForm);

        //    TestWorkflow(
        //        wfFactory,
        //        (run) => TestFailWithSecurityError(run)
        //       );
        //}

        // MISSING - GetRecords negative test on report and type

        //
        // Activity WF Factories
        //

        static Func<Workflow> UpdateFieldWfFactory(IEntity resource, Field field, string valueExpression)
        {
            return () =>
                       Entity
                       .Create<Workflow>()
                       .AddDefaultExitPoint()
                       .AddVariable<ResourceArgument>("vRes", null, Resource.Resource_Type)
                       .AddEntityExpressionToVariable("vRes", resource.Id)
                       .AddUpdateField("Update field", Resource.Name_Field.As<Resource>(), "[vRes]", valueExpression);
        }

        Func<Workflow> DisplayFormWfFactory(IEntity person, IEntity form)
        {
            return () =>
                Entity
                    .Create<Workflow>()
                    .AddDefaultExitPoint()
                    .AddVariable<ResourceArgument>("vPerson", null, Person.Person_Type)
                    .AddEntityExpressionToVariable("vPerson", person.Id)
                    .AddVariable<ResourceArgument>("vForm", null, CustomEditForm.CustomEditForm_Type)
                    .AddEntityExpressionToVariable("vForm", form.Id)
                    .AddDisplayForm("Display Form", new string[]{"Exit1"}, "vForm", null, "vPerson");
        }


        static IEntity CreateVisible(EntityType et)  { return CreateEntity(et, "Readable"); }

        static IEntity CreateHidden(EntityType et) { return CreateEntity(et, "Hidden"); }

        static IEntity CreateWritable(EntityType et) { return CreateEntity(et, "Writable"); }

        static IEntity CreateCreatable(EntityType et) { return CreateEntity(et, "Creatable"); }

        static IEntity CreateDeletable(EntityType et) { return CreateEntity(et, "Deletable"); }


        static IEntity CreateEntity(EntityType et, string name) 
        {
            var entity = Entity.Create(et);
            entity.SetField(Resource.Name_Field, name);  // all "A"s are visible, all "B"s are editable
            entity.Save();
            return entity;
        }
    }
}
