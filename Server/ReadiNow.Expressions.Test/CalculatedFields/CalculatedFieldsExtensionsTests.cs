// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Expressions;
using ReadiNow.Expressions.CalculatedFields;

namespace ReadiNow.Expressions.Test.CalculatedFields
{
    [TestFixture]
    public class CalculatedFieldsExtensionsTests
    {
        [Test]
        public void GetCalculatedFieldMetadata_SingleField_Valid()
        {
            long fieldId = 123;
            CalculatedFieldSettings settings = new CalculatedFieldSettings();
            Mock<ICalculatedFieldMetadataProvider> mockProvider;
            CalculatedFieldMetadata mockSingleResult;
            IReadOnlyCollection<CalculatedFieldMetadata> mockProviderResult;
            CalculatedFieldMetadata actualResult;

            // Mock provider
            mockSingleResult = new CalculatedFieldMetadata(fieldId, "calc", null, null);
            mockProviderResult = new[] { mockSingleResult };
            mockProvider = new Mock<ICalculatedFieldMetadataProvider>(MockBehavior.Strict);
            mockProvider
                .Setup(p => p.GetCalculatedFieldMetadata(It.Is<IReadOnlyCollection<long>>(fields => fields.Count == 1 && fields.First() == fieldId), settings))
                .Returns(mockProviderResult);

            // Test extension method
            actualResult = mockProvider.Object.GetCalculatedFieldMetadata(fieldId, settings);

            // Verify
            Assert.That(actualResult, Is.EqualTo(mockSingleResult));

            mockProvider.VerifyAll();
        }

        [Test]
        public void GetCalculatedFieldMetadata_SingleField_ParseException()
        {
            long fieldId = 123;
            CalculatedFieldSettings settings = new CalculatedFieldSettings();
            Mock<ICalculatedFieldMetadataProvider> mockProvider;
            CalculatedFieldMetadata mockSingleResult;
            IReadOnlyCollection<CalculatedFieldMetadata> mockProviderResult;
            CalculatedFieldMetadata actualResult;
            ParseException parseException = new ParseException("Test");

            // Mock provider
            mockSingleResult = new CalculatedFieldMetadata(fieldId, "calc", null, parseException);
            mockProviderResult = new[] { mockSingleResult };
            mockProvider = new Mock<ICalculatedFieldMetadataProvider>(MockBehavior.Strict);
            mockProvider
                .Setup(p => p.GetCalculatedFieldMetadata(It.Is<IReadOnlyCollection<long>>(fields => fields.Count == 1 && fields.First() == fieldId), settings))
                .Returns(mockProviderResult);

            // Test extension method
            actualResult = mockProvider.Object.GetCalculatedFieldMetadata(fieldId, settings);

            // Verify
            Assert.That(actualResult, Is.EqualTo(mockSingleResult));

            mockProvider.VerifyAll();
        }

        [Test]
        public void GetCalculatedFieldValues_SingleField_Valid()
        {
            long fieldId = 123;
            long[] entityIDs = new long[] { 456, 789 };
            CalculatedFieldSettings settings = new CalculatedFieldSettings();
            Mock<ICalculatedFieldProvider> mockProvider;
            IReadOnlyCollection<CalculatedFieldSingleResult> mockEvaluatorResult;
            IReadOnlyCollection<CalculatedFieldResult> mockProviderResult;
            CalculatedFieldResult actualResult;

            // Mock provider
            mockEvaluatorResult = entityIDs.Select(id => new CalculatedFieldSingleResult(id, "a")).ToArray();
            mockProviderResult = new[] { new CalculatedFieldResult(fieldId, mockEvaluatorResult) };
            mockProvider = new Mock<ICalculatedFieldProvider>(MockBehavior.Strict);
            mockProvider
                .Setup(p => p.GetCalculatedFieldValues(
                    It.Is<IReadOnlyCollection<long>>(fields => fields.Count == 1 && fields.First() == fieldId),
                    entityIDs,
                    settings))
                .Returns(mockProviderResult);

            // Test extension method
            actualResult = mockProvider.Object.GetCalculatedFieldValues(fieldId, entityIDs, settings);

            // Verify
            Assert.That(actualResult, Is.Not.Null);
            Assert.That(actualResult.FieldId, Is.EqualTo(fieldId));
            Assert.That(actualResult.Entities, Is.Not.Null);
            Assert.That(actualResult.Entities, Is.EqualTo(mockEvaluatorResult));
            
            mockProvider.VerifyAll();
        }

        [Test]
        public void GetCalculatedFieldValues_SingleField_EvaluationException()
        {
            long fieldId = 123;
            long[] entityIDs = new long[] { 456, 789 };
            CalculatedFieldSettings settings = new CalculatedFieldSettings();
            Mock<ICalculatedFieldProvider> mockProvider;
            IReadOnlyCollection<CalculatedFieldSingleResult> mockEvaluatorResult;
            IReadOnlyCollection<CalculatedFieldResult> mockProviderResult;
            CalculatedFieldResult actualResult;

            // Mock provider
            mockEvaluatorResult = new[] { new CalculatedFieldSingleResult(456, "a"), new CalculatedFieldSingleResult(789, new EvaluationException("Error")) };
            mockProviderResult = new[] { new CalculatedFieldResult(fieldId, mockEvaluatorResult) };
            mockProvider = new Mock<ICalculatedFieldProvider>(MockBehavior.Strict);
            mockProvider
                .Setup(p => p.GetCalculatedFieldValues(
                    It.Is<IReadOnlyCollection<long>>(fields => fields.Count == 1 && fields.First() == fieldId),
                    entityIDs,
                    settings))
                .Returns(mockProviderResult);

            // Test extension method
            // (evaluation exceptions should not cause the bulk-entity mechanism to throw)
            actualResult = mockProvider.Object.GetCalculatedFieldValues(fieldId, entityIDs, settings);

            // Verify
            Assert.That(actualResult, Is.Not.Null);
            Assert.That(actualResult.FieldId, Is.EqualTo(fieldId));
            Assert.That(actualResult.Entities, Is.Not.Null);
            Assert.That(actualResult.Entities, Is.EqualTo(mockEvaluatorResult));

            mockProvider.VerifyAll();
        }

        [Test]
        public void GetCalculatedFieldValues_SingleField_ParseException()
        {
            long fieldId = 123;
            long[] entityIDs = new long[] { 456, 789 };
            CalculatedFieldSettings settings = new CalculatedFieldSettings();
            Mock<ICalculatedFieldProvider> mockProvider;
            IReadOnlyCollection<CalculatedFieldResult> mockProviderResult;
            ParseException exception;
            CalculatedFieldResult actualResult;

            // Mock provider
            exception = new ParseException("Test error");
            mockProviderResult = new[] { new CalculatedFieldResult(fieldId, exception) };
            mockProvider = new Mock<ICalculatedFieldProvider>(MockBehavior.Strict);
            mockProvider
                .Setup(p => p.GetCalculatedFieldValues(
                    It.Is<IReadOnlyCollection<long>>(fields => fields.Count == 1 && fields.First() == fieldId),
                    entityIDs,
                    settings))
                .Returns(mockProviderResult);

            // Test extension method
            // (evaluation exceptions should not cause the bulk-entity mechanism to throw)
            actualResult = mockProvider.Object.GetCalculatedFieldValues(fieldId, entityIDs, settings);

            // Verify
            Assert.That(actualResult, Is.Not.Null);
            Assert.That(actualResult.FieldId, Is.EqualTo(fieldId));
            Assert.That(actualResult.Entities, Is.Null);

            mockProvider.VerifyAll();
        }

        [Test]
        public void GetCalculatedFieldValues_SingleEntity_Valid()
        {
            long fieldId = 123;
            long entityId = 456;
            CalculatedFieldSettings settings = new CalculatedFieldSettings();
            Mock<ICalculatedFieldProvider> mockProvider;
            IReadOnlyCollection<CalculatedFieldSingleResult> mockEvaluatorResult;
            IReadOnlyCollection<CalculatedFieldResult> mockProviderResult;
            object mockResult = "a";
            object actualResult;

            // Mock provider
            mockEvaluatorResult = new[] { new CalculatedFieldSingleResult(entityId, mockResult) };
            mockProviderResult = new[] { new CalculatedFieldResult(fieldId, mockEvaluatorResult) };
            mockProvider = new Mock<ICalculatedFieldProvider>(MockBehavior.Strict);
            mockProvider
                .Setup(p => p.GetCalculatedFieldValues(
                    It.Is<IReadOnlyCollection<long>>(fields => fields.Count == 1 && fields.First() == fieldId),
                    It.Is<IReadOnlyCollection<long>>(fields => fields.Count == 1 && fields.First() == entityId),
                    settings))
                .Returns(mockProviderResult);

            // Test extension method
            actualResult = mockProvider.Object.GetCalculatedFieldValue(fieldId, entityId, settings);

            // Verify
            Assert.That(actualResult, Is.EqualTo(mockResult));

            mockProvider.VerifyAll();
        }

        [Test]
        public void GetCalculatedFieldValues_SingleEntity_ParseException()
        {
            long fieldId = 123;
            long entityId = 456;
            CalculatedFieldSettings settings = new CalculatedFieldSettings();
            Mock<ICalculatedFieldProvider> mockProvider;
            IReadOnlyCollection<CalculatedFieldResult> mockProviderResult;
            ParseException exception;
            object actualResult;

            // Mock provider
            exception = new ParseException("Test error");
            mockProviderResult = new[] { new CalculatedFieldResult(fieldId, exception) };
            mockProvider = new Mock<ICalculatedFieldProvider>(MockBehavior.Strict);
            mockProvider
                .Setup(p => p.GetCalculatedFieldValues(
                    It.Is<IReadOnlyCollection<long>>(fields => fields.Count == 1 && fields.First() == fieldId),
                    It.Is<IReadOnlyCollection<long>>(fields => fields.Count == 1 && fields.First() == entityId),
                    settings))
                .Returns(mockProviderResult);

            // Test extension method
            actualResult = mockProvider.Object.GetCalculatedFieldValue(fieldId, entityId, settings);

            // Verify
            // (Parse exception causes fields to return null)
            Assert.That(actualResult, Is.Null);

            mockProvider.VerifyAll();
        }

        [Test]
        public void GetCalculatedFieldValues_SingleEntity_EvaluationException()
        {
            long fieldId = 123;
            long entityId = 456;
            CalculatedFieldSettings settings = new CalculatedFieldSettings();
            Mock<ICalculatedFieldProvider> mockProvider;
            IReadOnlyCollection<CalculatedFieldSingleResult> mockEvaluatorResult;
            IReadOnlyCollection<CalculatedFieldResult> mockProviderResult;
            EvaluationException exception;
            object actualResult;

            // Mock provider
            exception = new EvaluationException("Test error");
            mockEvaluatorResult = new[] { new CalculatedFieldSingleResult(entityId, exception) };
            mockProviderResult = new[] { new CalculatedFieldResult(fieldId, mockEvaluatorResult) };
            mockProvider = new Mock<ICalculatedFieldProvider>(MockBehavior.Strict);
            mockProvider
                .Setup(p => p.GetCalculatedFieldValues(
                    It.Is<IReadOnlyCollection<long>>(fields => fields.Count == 1 && fields.First() == fieldId),
                    It.Is<IReadOnlyCollection<long>>(fields => fields.Count == 1 && fields.First() == entityId),
                    settings))
                .Returns(mockProviderResult);

            // Test extension method
            actualResult = mockProvider.Object.GetCalculatedFieldValue(fieldId, entityId, settings);

            // Verify
            // (Parse exception causes fields to return null)
            Assert.That(actualResult, Is.Null);
            mockProvider.VerifyAll();
        }

    }
}
