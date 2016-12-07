// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.Cache.Providers;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Test;
using EDC.SoftwarePlatform.WebApi.Controllers.EditForm;
using NUnit.Framework;

namespace EDC.SoftwarePlatform.WebApi.Test.EditForm
{
    [TestFixture]
    [RunAsDefaultTenant]
    [RunWithTransaction]
    public class FormControllerRequestHandlerTests
    {
        private class TestState
        {
            public SingleLineTextControl NameControl { get; set; }
            public SingleLineTextControl DescriptionControl { get; set; }

            public CustomEditForm EditForm { get; set; }

            public IEntity Instance { get; set; }
        }

        private TestState CreateTestEntities(string nameVisibilityCalc, string descriptionVisibilityCalc, string name,
            string description)
        {
            var testType = new EntityType {Name = "VisCalc Type"};
            var testTypeForm = new CustomEditForm {Name = "VisCalc Type Form", TypeToEditWithForm = testType};

            var nameControl = new SingleLineTextControl
            {
                FieldToRender = EntityType.Name_Field.As<Field>(),
                VisibilityCalculation = nameVisibilityCalc
            };

            testTypeForm.ContainedControlsOnForm.Add(nameControl.As<ControlOnForm>());

            var descriptionControl = new SingleLineTextControl
            {
                FieldToRender = EntityType.Description_Field.As<Field>(),
                VisibilityCalculation = descriptionVisibilityCalc
            };

            testTypeForm.ContainedControlsOnForm.Add(descriptionControl.As<ControlOnForm>());

            testTypeForm.Save();

            var instance = Entity.Create(testType);
            instance.SetField("core:name", name);
            instance.SetField("core:description", description);
            instance.Save();

            return new TestState
            {
                Instance = instance,
                NameControl = nameControl,
                DescriptionControl = descriptionControl,
                EditForm = testTypeForm
            };
        }

        [Test]
        public void ConstructorNullValueTest()
        {
            Assert.That(() => new FormControllerRequestHandler(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("formsCache"));
        }

        [Test]
        public void ConstructorValidValueTest()
        {
            // ReSharper disable once ObjectCreationAsStatement
            Assert.DoesNotThrow(() => new FormControllerRequestHandler(new DictionaryCache<long, EntityData>()));
        }

        [Test]
        public void GetFormAsEntityData()
        {
            // Create a form with the name and description controls depending on each other
            var testState = CreateTestEntities(null, null, "name", "description");

            var handler = new FormControllerRequestHandler(new DictionaryCache<long, EntityData>());

            var entityData = handler.GetFormAsEntityData(testState.EditForm.Id, false);

            Assert.AreEqual(testState.EditForm.Id, entityData.Id.Id);
        }

        [Test]
        public void GetFormCalculationDependencies_FieldCalculations()
        {
            // Create a form with the name and description controls depending on each other
            var testState = CreateTestEntities("[Description] = 'test A'", "[Name] = 'test B'", "name", "description");

            var handler = new FormControllerRequestHandler(new DictionaryCache<long, EntityData>());

            var response = handler.GetFormCalculationDependencies(new EntityRef(testState.EditForm.Id));

            Assert.AreEqual(2, response.VisibilityCalcDependencies.Count);
            VisibilityCalcDependencies nameDeps;
            Assert.IsTrue(response.VisibilityCalcDependencies.TryGetValue(testState.NameControl.Id, out nameDeps));
            Assert.IsTrue(nameDeps.Fields.Contains(EntityType.Description_Field.Id));
            Assert.IsNull(nameDeps.Relationships);

            VisibilityCalcDependencies descriptionDeps;
            Assert.IsTrue(response.VisibilityCalcDependencies.TryGetValue(testState.DescriptionControl.Id,
                out descriptionDeps));
            Assert.IsTrue(descriptionDeps.Fields.Contains(EntityType.Name_Field.Id));
            Assert.IsNull(descriptionDeps.Relationships);
        }

        [Test]
        public void GetFormCalculationDependencies_InvalidCalculation()
        {
            // Create a form with one of the calcs being invalid
            var testState = CreateTestEntities("[Description] = 'test'", "xyz!!!", "name", "description");

            var handler = new FormControllerRequestHandler(new DictionaryCache<long, EntityData>());

            var response = handler.GetFormCalculationDependencies(new EntityRef(testState.EditForm.Id));

            Assert.AreEqual(1, response.VisibilityCalcDependencies.Count);
            VisibilityCalcDependencies nameDeps;
            Assert.IsTrue(response.VisibilityCalcDependencies.TryGetValue(testState.NameControl.Id, out nameDeps));
            Assert.AreEqual(1, nameDeps.Fields.Count);

            Assert.IsTrue(nameDeps.Fields.Contains(EntityType.Description_Field.Id));
        }


        [Test]
        public void GetFormCalculationDependencies_NoCalculations()
        {
            var testState = CreateTestEntities(null, null, "name", "description");

            var handler = new FormControllerRequestHandler(new DictionaryCache<long, EntityData>());

            var response = handler.GetFormCalculationDependencies(new EntityRef(testState.EditForm.Id));

            Assert.IsNull(response.VisibilityCalcDependencies);
        }

        [Test]
        public void GetFormCalculationDependencies_NonBoolCalculation()
        {
            // Create a form with one of the calcs being a non bool expression
            var testState = CreateTestEntities("[Description] = 'test'", "[Name]", "name", "description");

            var handler = new FormControllerRequestHandler(new DictionaryCache<long, EntityData>());

            var response = handler.GetFormCalculationDependencies(new EntityRef(testState.EditForm.Id));

            Assert.AreEqual(1, response.VisibilityCalcDependencies.Count);
            VisibilityCalcDependencies nameDeps;
            Assert.IsTrue(response.VisibilityCalcDependencies.TryGetValue(testState.NameControl.Id, out nameDeps));
            Assert.AreEqual(1, nameDeps.Fields.Count);

            Assert.IsTrue(nameDeps.Fields.Contains(EntityType.Description_Field.Id));
        }

        [Test]
        public void GetFormCalculationDependencies_NullForm()
        {
            var handler = new FormControllerRequestHandler(new DictionaryCache<long, EntityData>());

            Assert.That(() => handler.GetFormCalculationDependencies(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("formRef"));
        }

        [Test]
        public void GetFormCalculationDependencies_RelationshipCalculations()
        {
            // Create a form with the name and description controls depending on each other
            var testState = CreateTestEntities("[Created by].Name = 'test'", null, "name", "description");

            var handler = new FormControllerRequestHandler(new DictionaryCache<long, EntityData>());

            var response = handler.GetFormCalculationDependencies(new EntityRef(testState.EditForm.Id));

            Assert.AreEqual(1, response.VisibilityCalcDependencies.Count);
            VisibilityCalcDependencies nameDeps;
            Assert.IsTrue(response.VisibilityCalcDependencies.TryGetValue(testState.NameControl.Id, out nameDeps));
            Assert.AreEqual(1, nameDeps.Fields.Count);
            Assert.AreEqual(1, nameDeps.Relationships.Count);

            Assert.IsTrue(nameDeps.Fields.Contains(EntityType.Name_Field.Id));
            Assert.IsTrue(nameDeps.Relationships.Contains(EntityType.CreatedBy_Field.Id));
        }

        [Test]
        public void GetFormData_InvalidEntityRequest()
        {
            var handler = new FormControllerRequestHandler(new DictionaryCache<long, EntityData>());

            var request = new FormDataRequest
            {
                EntityId = "-100",
                Query = "name, description"
            };

            var response = handler.GetFormData(request);
            Assert.IsNull(response);
        }

        [Test]
        public void GetFormData_InvalidVisCalculation()
        {
            // Name will be initially hidden
            var testState = CreateTestEntities("[Description] = 'test'", "xyz!!", "name", "description");

            var handler = new FormControllerRequestHandler(new DictionaryCache<long, EntityData>());

            var request = new FormDataRequest
            {
                FormId = testState.EditForm.Id.ToString(),
                EntityId = testState.Instance.Id.ToString(),
                Query = "name, description"
            };

            var response = handler.GetFormData(request);

            Assert.AreEqual(2, response.InitiallyHiddenControls.Count);
            Assert.IsTrue(response.InitiallyHiddenControls.Contains(testState.NameControl.Id));
            Assert.IsTrue(response.InitiallyHiddenControls.Contains(testState.DescriptionControl.Id));
            Assert.IsTrue(response.FormDataEntity.Entities.ContainsKey(testState.Instance.Id));
        }

        [Test]
        public void GetFormData_NonBoolVisCalculation()
        {
            var testState = CreateTestEntities("[Description]", null, "name", "description");

            var handler = new FormControllerRequestHandler(new DictionaryCache<long, EntityData>());

            var request = new FormDataRequest
            {
                FormId = testState.EditForm.Id.ToString(),
                EntityId = testState.Instance.Id.ToString(),
                Query = "name, description"
            };

            var response = handler.GetFormData(request);

            Assert.AreEqual(1, response.InitiallyHiddenControls.Count);
            Assert.IsTrue(response.InitiallyHiddenControls.Contains(testState.NameControl.Id));
            Assert.IsTrue(response.FormDataEntity.Entities.ContainsKey(testState.Instance.Id));
        }

        [Test]
        public void GetFormData_NoVisCalculation()
        {
            // Create a form with the name and description controls depending on each other
            var testState = CreateTestEntities(null, null, "name", "description");

            var handler = new FormControllerRequestHandler(new DictionaryCache<long, EntityData>());

            var request = new FormDataRequest
            {
                FormId = testState.EditForm.Id.ToString(),
                EntityId = testState.Instance.Id.ToString(),
                Query = "name, description"
            };

            var response = handler.GetFormData(request);

            Assert.IsNull(response.InitiallyHiddenControls);
            Assert.IsTrue(response.FormDataEntity.Entities.ContainsKey(testState.Instance.Id));
        }

        [Test]
        public void GetFormData_NullRequest()
        {
            var handler = new FormControllerRequestHandler(new DictionaryCache<long, EntityData>());

            Assert.That(() => handler.GetFormData(null),
                Throws.TypeOf<ArgumentNullException>().And.Property("ParamName").EqualTo("request"));
        }

        [Test]
        public void GetFormData_VisCalculation()
        {
            // Name will be initially hidden
            var testState = CreateTestEntities("[Description] = 'test'", null, "name", "description");

            var handler = new FormControllerRequestHandler(new DictionaryCache<long, EntityData>());

            var request = new FormDataRequest
            {
                FormId = testState.EditForm.Id.ToString(),
                EntityId = testState.Instance.Id.ToString(),
                Query = "name, description"
            };

            var response = handler.GetFormData(request);

            Assert.AreEqual(1, response.InitiallyHiddenControls.Count);
            Assert.IsTrue(response.InitiallyHiddenControls.Contains(testState.NameControl.Id));
            Assert.IsTrue(response.FormDataEntity.Entities.ContainsKey(testState.Instance.Id));
        }

        [Test]
        [TestCase("123", "123", "name", true, "")]
        [TestCase(null, "123", "name", false, "EntityId was null.")]
        [TestCase("123", null, "name", false, "FormId was null.")]
        [TestCase("123", "123", null, false, "Query was null.")]
        [TestCase("123", "123", "--", false, "Failed to parse query string.")]
        public void ValidateRequest_InvalidRequest(string entityId, string formId, string query, bool expectedResult,
            string errorMessageContains)
        {
            var handler = new FormControllerRequestHandler(new DictionaryCache<long, EntityData>());

            var request = new FormDataRequest {EntityId = entityId, FormId = formId, Query = query};
            string message;
            var result = handler.ValidateRequest(request, out message);

            Assert.AreEqual(expectedResult, result, "The validation result is invalid");
            if (!expectedResult)
                Assert.IsTrue(message.Contains(errorMessageContains), "The error message is incorrect.");
            else
                Assert.IsTrue(string.IsNullOrWhiteSpace(message));
        }
    }
}