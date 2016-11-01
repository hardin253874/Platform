// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.Database;
using EDC.ReadiNow.Core;
using ReadiNow.Expressions.CalculatedFields;

namespace ReadiNow.Expressions.Test.CalculatedFields
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class CalculatedFieldMetadataProviderTests
    {
        [TestCase("core:name")]
        [TestCase("core:fieldCalculation")]
        [TestCase("core:report")]
        public void IsCalculatedField_False(string fieldAlias)
        {
            bool result = Factory.CalculatedFieldMetadataProvider.IsCalculatedField( new EntityRef(fieldAlias).Id );

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsCalculatedField_Unsaved()
        {
            StringField field = new StringField();
            field.FieldCalculation = "'abc'+123";

            bool result = Factory.CalculatedFieldMetadataProvider.IsCalculatedField(field.Id);
        }

        [Test]
        [RunWithTransaction]
        public void IsCalculatedField_Saved()
        {
            StringField field = new StringField();
            field.FieldCalculation = "'abc'+123";
            field.Save();

            bool result = Factory.CalculatedFieldMetadataProvider.IsCalculatedField(field.Id);
        }

        [TestCase("core:stringField", DataType.String)]
        [TestCase("core:intField", DataType.Int32)]
        [TestCase("core:dateField", DataType.Date)]
        [TestCase("core:timeField", DataType.Time)]
        [TestCase("core:dateTimeField", DataType.DateTime)]
        [TestCase("core:boolField", DataType.Bool)]
        [TestCase("core:decimalField", DataType.Decimal)]
        [TestCase("core:currencyField", DataType.Currency)]
        public void CreateBuilderSettingsForField(string fieldAlias, DataType expectedType)
        {
            Field field = Entity.Create(fieldAlias).As<Field>();
            EntityType type = new EntityType();
            field.FieldIsOnType = type;

            BuilderSettings settings = CalculatedFieldMetadataProvider.CreateBuilderSettingsForField(field, CalculatedFieldSettings.Default);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.ScriptHost, Is.EqualTo(ScriptHostType.Any));
            Assert.That(settings.RootContextType.EntityTypeId, Is.EqualTo(type.Id));
            Assert.That(settings.ExpectedResultType.DisallowList, Is.True);
            Assert.That(settings.ExpectedResultType.Type, Is.EqualTo(expectedType));
        }

        [Test]
        public void GetCalculatedFieldMetadata()
        {
            long fieldId;
            Field field;
            string calculation;
            EntityType type;
            Mock<IEntityRepository> mockRepository;
            Mock<IExpressionCompiler> mockCompiler;
            Mock<IExpression> mockExpression;
            CalculatedFieldMetadataProvider provider;
            IReadOnlyCollection<CalculatedFieldMetadata> actualResult;
            CalculatedFieldMetadata metadata;

            // Mock calculated field
            calculation = "Name + 'Hello'";
            field = new StringField().As<Field>();
            type = new EntityType();
            field.FieldIsOnType = type;
            field.FieldCalculation = calculation;
            fieldId = field.Id;

            // Mock repository (loading field data)
            mockRepository = new Mock<IEntityRepository>(MockBehavior.Strict);
            mockRepository
                .Setup(r => r.Get<Field>(It.Is<IEnumerable<long>>(fields => fields.Count() == 1 && fields.First() == fieldId), It.IsAny<string>()))
                .Returns(new Field[] { field });

            // Mock compiler (compiling the calculation)
            mockExpression = new Mock<IExpression>();
            mockCompiler = new Mock<IExpressionCompiler>(MockBehavior.Strict);
            mockCompiler
                .Setup(c => c.Compile(calculation, It.IsAny<BuilderSettings>()))
                .Returns(mockExpression.Object);

            // Get provider
            provider = new CalculatedFieldMetadataProvider(mockCompiler.Object, mockRepository.Object);

            // Run the test
            actualResult = provider.GetCalculatedFieldMetadata(new[] { fieldId }, CalculatedFieldSettings.Default);

            // Verify
            Assert.That(actualResult, Is.Not.Null);
            Assert.That(actualResult.Count, Is.EqualTo(1));
            metadata = actualResult.First();
            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.CalculatedFieldId, Is.EqualTo(fieldId));
            Assert.That(metadata.Expression, Is.EqualTo(mockExpression.Object));
            Assert.That(metadata.Calculation, Is.EqualTo(calculation));

            mockRepository.VerifyAll();
            mockCompiler.VerifyAll();
        }

        [Test]
        public void GetCalculatedFieldMetadata_ParseException()
        {
            long fieldId;
            Field field;
            string calculation;
            EntityType type;
            Mock<IEntityRepository> mockRepository;
            Mock<IExpressionCompiler> mockCompiler;
            Mock <IExpression> mockExpression;
            CalculatedFieldMetadataProvider provider;
            IReadOnlyCollection<CalculatedFieldMetadata> actualResult;
            CalculatedFieldMetadata metadata;
            ParseException exception = new ParseException("Test error");

            // Mock calculated field
            calculation = "Name + 'Hello'";
            field = new StringField().As<Field>();
            type = new EntityType();
            field.FieldIsOnType = type;
            field.FieldCalculation = calculation;
            fieldId = field.Id;

            // Mock repository (loading field data)
            mockRepository = new Mock<IEntityRepository>(MockBehavior.Strict);
            mockRepository
                .Setup(r => r.Get<Field>(It.Is<IEnumerable<long>>( fields => fields.Count() == 1 && fields.First() == fieldId ), It.IsAny<string>() ))
                .Returns(new Field[] { field });

            // Mock compiler (compiling the calculation)
            mockExpression = new Mock<IExpression>();
            mockCompiler = new Mock<IExpressionCompiler>(MockBehavior.Strict);
            mockCompiler
                .Setup(c => c.Compile(calculation, It.IsAny<BuilderSettings>()))
                .Returns( () => { throw exception; } );

            // Get provider
            provider = new CalculatedFieldMetadataProvider(mockCompiler.Object, mockRepository.Object);

            // Run the test
            actualResult = provider.GetCalculatedFieldMetadata(new[] { fieldId }, CalculatedFieldSettings.Default);

            // Verify
            Assert.That(actualResult, Is.Not.Null);
            Assert.That(actualResult.Count, Is.EqualTo(1));
            metadata = actualResult.First();
            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.CalculatedFieldId, Is.EqualTo(fieldId));
            Assert.That(metadata.Expression, Is.Null);
            Assert.That(metadata.Exception, Is.EqualTo(exception));
            Assert.That(metadata.Calculation, Is.EqualTo(calculation));

            mockRepository.VerifyAll();
            mockCompiler.VerifyAll();
        }

        [Test]
        public void GetCalculatedFieldMetadata_Error_NullFieldList()
        {
            Mock<IEntityRepository> mockRepository;
            Mock<IExpressionCompiler> mockCompiler;
            CalculatedFieldMetadataProvider provider;
            
            // Mock repository (loading field data)
            mockRepository = new Mock<IEntityRepository>(MockBehavior.Strict);
            
            // Mock compiler (compiling the calculation)
            mockCompiler = new Mock<IExpressionCompiler>(MockBehavior.Strict);
            
            // Get provider
            provider = new CalculatedFieldMetadataProvider(mockCompiler.Object, mockRepository.Object);

            // Run the test
            Assert.Throws<ArgumentNullException>(() => provider.GetCalculatedFieldMetadata(null, CalculatedFieldSettings.Default));

            // Verify
            mockRepository.VerifyAll();
            mockCompiler.VerifyAll();
        }

        [Test]
        public void GetCalculatedFieldMetadata_Error_NotAField()
        {
            Mock<IExpressionCompiler> mockCompiler;
            CalculatedFieldMetadataProvider provider;

            // Mock compiler (compiling the calculation)
            mockCompiler = new Mock<IExpressionCompiler>(MockBehavior.Strict);

            // Get provider
            provider = new CalculatedFieldMetadataProvider(mockCompiler.Object, Factory.EntityRepository);

            // Run the test
            Assert.Throws<ArgumentException>(() => provider.GetCalculatedFieldMetadata( new long[] { 1 }, CalculatedFieldSettings.Default));
        }

        [Test]
        public void GetCalculatedFieldMetadata_Error_FieldHasNoCalculation()
        {
            long fieldId;
            Field field;
            EntityType type;
            Mock<IEntityRepository> mockRepository;
            Mock<IExpressionCompiler> mockCompiler;
            Mock<IExpression> mockExpression;
            CalculatedFieldMetadataProvider provider;

            // Mock calculated field
            field = new StringField().As<Field>();
            type = new EntityType();
            field.FieldIsOnType = type;
            fieldId = field.Id;

            // Mock repository (loading field data)
            mockRepository = new Mock<IEntityRepository>(MockBehavior.Strict);
            mockRepository
                .Setup(r => r.Get<Field>(It.Is<IEnumerable<long>>(fields => fields.Count() == 1 && fields.First() == fieldId), It.IsAny<string>()))
                .Returns(new Field[] { field });

            // Mock compiler (compiling the calculation)
            mockExpression = new Mock<IExpression>();
            mockCompiler = new Mock<IExpressionCompiler>(MockBehavior.Strict);

            // Get provider
            provider = new CalculatedFieldMetadataProvider(mockCompiler.Object, mockRepository.Object);

            // Run the test
            Assert.Throws<ArgumentException>(() => provider.GetCalculatedFieldMetadata(new[] { fieldId }, CalculatedFieldSettings.Default));
        }
    }
}
