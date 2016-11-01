// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Expressions;
using ReadiNow.Expressions.CalculatedFields;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.IO;

namespace ReadiNow.Expressions.Test.CalculatedFields
{
    /// <summary>
    /// Test CalculatedFieldProvider.
    /// </summary>
    [TestFixture]
    public class CalculatedFieldProviderTests
    {
        [Test]
        public void Test_MultiField_MultiEntity()
        {
            long[] fieldIds = new long[] { 11, 22 };
            long[] entityIds = new long[] { 33, 44, 55 };
            CalculatedFieldSettings settings = new CalculatedFieldSettings { TimeZone = "Australia/Sydney" };
            CalculatedFieldProvider provider;
            Mock<ICalculatedFieldMetadataProvider> mockMetadataProvider;
            Mock<IEntityRepository> mockEntityRepository;
            Mock<IExpressionRunner> mockExpressionRunner;
            IReadOnlyCollection<CalculatedFieldMetadata> mockMetadata;
            IReadOnlyCollection<CalculatedFieldResult> actualResult;
            IReadOnlyCollection<IEntity> mockEntities;
            int mockFirstResult = 100;
            Mock<IExpression> mockExpression;

            // Mock metadata provider
            mockExpression = new Mock<IExpression>();
            mockMetadata = fieldIds.Select(id => new CalculatedFieldMetadata(id, "abc", mockExpression.Object, null)).ToArray();
            mockMetadataProvider = new Mock<ICalculatedFieldMetadataProvider>(MockBehavior.Strict);
            mockMetadataProvider
                .Setup(p => p.GetCalculatedFieldMetadata(fieldIds, settings))
                .Returns(mockMetadata);

            // Mock entity repository
            mockEntities = entityIds.Select(id => new Mock<IEntity>().Object).ToArray();
            mockEntityRepository = new Mock<IEntityRepository>(MockBehavior.Strict);
            mockEntityRepository.Setup(er => er.Get(entityIds)).Returns(mockEntities);

            // Mock expression runner
            mockExpressionRunner = new Mock<IExpressionRunner>(MockBehavior.Strict);
            int runResult = mockFirstResult;
            foreach (IEntity entity in mockEntities)
            {
                mockExpressionRunner
                    .Setup(exprRun => exprRun.Run(
                        mockExpression.Object,
                        It.Is<EvaluationSettings>(evalSetting => evalSetting.ContextEntity == entity)))
                    .Returns(() => new ExpressionRunResult(runResult++));
            }

            // Run test
            provider = new CalculatedFieldProvider(mockMetadataProvider.Object, mockExpressionRunner.Object, mockEntityRepository.Object);
            actualResult = provider.GetCalculatedFieldValues(fieldIds, entityIds, settings);

            // Verify
            Assert.That(actualResult, Has.Count.EqualTo(2));

            var firstField = actualResult.First();
            Assert.That(firstField, Is.Not.Null);
            Assert.That(firstField.FieldId, Is.EqualTo(fieldIds.First()));
            Assert.That(firstField.ParseException, Is.Null);
            Assert.That(firstField.Entities, Has.Count.EqualTo(3));

            var singleResult = firstField.Entities.First();
            Assert.That(singleResult, Is.Not.Null);
            Assert.That(singleResult.EvaluationException, Is.Null);
            Assert.That(singleResult.Result, Is.EqualTo(mockFirstResult));

            var lastField = actualResult.Last();
            var lastResult = lastField.Entities.Last();
            Assert.That(lastResult.Result, Is.EqualTo(mockFirstResult + 5));

            mockMetadataProvider.VerifyAll();
            mockEntityRepository.VerifyAll();
            mockExpressionRunner.VerifyAll();
        }

        [Test]
        public void Test_ParseException()
        {
            long[] fieldIds = new long[] { 11, 22 };
            long[] entityIds = new long[] { 33, 44, 55 };
            CalculatedFieldSettings settings = new CalculatedFieldSettings { TimeZone = "Australia/Sydney" };
            CalculatedFieldProvider provider;
            Mock<ICalculatedFieldMetadataProvider> mockMetadataProvider;
            Mock<IEntityRepository> mockEntityRepository;
            Mock<IExpressionRunner> mockExpressionRunner;
            IReadOnlyCollection<CalculatedFieldMetadata> mockMetadata;
            IReadOnlyCollection<CalculatedFieldResult> actualResult;
            IReadOnlyCollection<IEntity> mockEntities;
            int mockFirstResult = 100;
            Mock<IExpression> mockExpression;
            ParseException exception = new ParseException("Test error");

            // Mock metadata provider
            mockExpression = new Mock<IExpression>();
            mockMetadata = new[] { new CalculatedFieldMetadata(11, "abc", null, exception), new CalculatedFieldMetadata(22, "abc", mockExpression.Object, null) };
            mockMetadataProvider = new Mock<ICalculatedFieldMetadataProvider>(MockBehavior.Strict);
            mockMetadataProvider
                .Setup(p => p.GetCalculatedFieldMetadata(fieldIds, settings))
                .Returns(mockMetadata);

            // Mock entity repository
            mockEntities = entityIds.Select(id => new Mock<IEntity>().Object).ToArray();
            mockEntityRepository = new Mock<IEntityRepository>(MockBehavior.Strict);
            mockEntityRepository.Setup(er => er.Get(entityIds)).Returns(mockEntities);

            // Mock expression runner
            mockExpressionRunner = new Mock<IExpressionRunner>(MockBehavior.Strict);
            int runResult = mockFirstResult;
            foreach (IEntity entity in mockEntities)
            {
                mockExpressionRunner
                    .Setup(exprRun => exprRun.Run(
                        mockExpression.Object,
                        It.Is<EvaluationSettings>(evalSetting => evalSetting.ContextEntity == entity)))
                    .Returns( () => new ExpressionRunResult(runResult++));
            }
                        
            // Run test
            provider = new CalculatedFieldProvider(mockMetadataProvider.Object, mockExpressionRunner.Object, mockEntityRepository.Object);
            actualResult = provider.GetCalculatedFieldValues(fieldIds, entityIds, settings);

            // Verify
            Assert.That(actualResult, Has.Count.EqualTo(2));

            var firstField = actualResult.First();
            Assert.That(firstField, Is.Not.Null);
            Assert.That(firstField.FieldId, Is.EqualTo(fieldIds.First()));
            Assert.That(firstField.ParseException, Is.EqualTo(exception));
            Assert.That(firstField.Entities, Is.Null);

            var lastField = actualResult.Last();
            var lastResult = lastField.Entities.Last();
            Assert.That(lastResult, Is.Not.Null);
            Assert.That(lastResult.EvaluationException, Is.Null);
            Assert.That(lastResult.Result, Is.EqualTo(mockFirstResult+2));

            mockMetadataProvider.VerifyAll();
            mockEntityRepository.VerifyAll();
            mockExpressionRunner.VerifyAll();
        }

        [Test]
        public void GetEvaluationSettings_FromCalcSettings()
        {
            CalculatedFieldSettings calcSettings = new CalculatedFieldSettings();
            calcSettings.TimeZone = "Japan/Tokyo";

            EvaluationSettings evalSettings = CalculatedFieldProvider.CreateEvaluationSettings(calcSettings);
            Assert.That(evalSettings.TimeZoneName, Is.EqualTo("Japan/Tokyo"));
        }

        [Test]
        public void GetEvaluationSettings_FromRequestContext()
        {
            try
            {
                RequestContext.SetContext(null, null, null, "Japan/Tokyo");

                EvaluationSettings evalSettings = CalculatedFieldProvider.CreateEvaluationSettings(CalculatedFieldSettings.Default);
                Assert.That(evalSettings.TimeZoneName, Is.EqualTo("Japan/Tokyo"));
            }
            finally
            {
                RequestContext.FreeContext();
            }
        }

        [Test]
        public void GetEvaluationSettings_Both()
        {
            try
            {
                RequestContext.SetContext(null, null, null, "Japan/Tokyo");

                CalculatedFieldSettings calcSettings = new CalculatedFieldSettings();
                calcSettings.TimeZone = "Australia/Brisbane";

                EvaluationSettings evalSettings = CalculatedFieldProvider.CreateEvaluationSettings(calcSettings);
                Assert.That(evalSettings.TimeZoneName, Is.EqualTo("Australia/Brisbane"));
            }
            finally
            {
                RequestContext.FreeContext();
            }
        }

        [Test]
        public void GetEvaluationSettings_None()
        {
            Assert.Throws<InvalidOperationException>(
                () => CalculatedFieldProvider.CreateEvaluationSettings(CalculatedFieldSettings.Default));
        }
    }
}
