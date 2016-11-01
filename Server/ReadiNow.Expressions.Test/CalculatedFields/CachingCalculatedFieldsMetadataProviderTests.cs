// Copyright 2011-2016 Global Software Innovation Pty Ltd
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Test;
using EDC.ReadiNow.Expressions;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Core;
using ReadiNow.Expressions.CalculatedFields;

namespace ReadiNow.Expressions.Test.CalculatedFields
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class CachingCalculatedFieldMetadataProviderTests
    {
        [Test]
        public void EnsureMetadata_Invalidates_WhenCalculationChanged()
        {
            EntityType type = null;
            Field field = null;
            Field calcField = null;

            try
            {
                // Create scenario
                type = new EntityType();
                type.Name = "CFType";
                field = new StringField().As<Field>();
                field.Name = "CFField";
                field.Save();
                calcField = new StringField().As<Field>();
                calcField.Name = "CFCalcField";
                calcField.FieldCalculation = "'A'+CFField";
                calcField.IsCalculatedField = true;
                calcField.Save();
                type.Fields.Add(field);
                type.Fields.Add(calcField);
                type.Save();

                // Get provider
                ICalculatedFieldMetadataProvider provider = Factory.CalculatedFieldMetadataProvider;

                // Run
                CalculatedFieldMetadata res1 = provider.GetCalculatedFieldMetadata(calcField.Id, CalculatedFieldSettings.Default);
                CalculatedFieldMetadata res2 = provider.GetCalculatedFieldMetadata(calcField.Id, CalculatedFieldSettings.Default);

                // Ensure same instance was returned (came from cache)
                Assert.That(res2, Is.SameAs(res1));

                // Delete the field
                calcField.FieldCalculation = "'B'+CFField";
                calcField.Save();

                // Rerun
                CalculatedFieldMetadata res3 = provider.GetCalculatedFieldMetadata(calcField.Id, CalculatedFieldSettings.Default);

                // Ensure different instance was returned (not from cache)
                Assert.That(res3, Is.Not.SameAs(res1));
                Assert.That(res3.Calculation, Is.EqualTo("'B'+CFField")); // because the script is now invalid
            }
            finally
            {
                if (field != null)
                    field.Delete();
                if (calcField != null)
                    calcField.Delete();
                if (type != null)
                    type.Delete();
            }
        }

        [Test]
        public void EnsureMetadata_Invalidates_WhenFieldDeleted()
        {
            EntityType type = null;
            Field field = null;
            Field calcField = null;

            try
            {
                // Create scenario
                type = new EntityType();
                type.Name = "CFType";
                field = new StringField().As<Field>();
                field.Name = "CFField";
                field.Save();
                calcField = new StringField().As<Field>();
                calcField.Name = "CFCalcField";
                calcField.FieldCalculation = "'A'+CFField";
                calcField.IsCalculatedField = true;
                calcField.Save();
                type.Fields.Add(field);
                type.Fields.Add(calcField);
                type.Save();

                // Get provider
                ICalculatedFieldMetadataProvider provider = Factory.CalculatedFieldMetadataProvider;

                // Run
                CalculatedFieldMetadata res1 = provider.GetCalculatedFieldMetadata(calcField.Id, CalculatedFieldSettings.Default);
                CalculatedFieldMetadata res2 = provider.GetCalculatedFieldMetadata(calcField.Id, CalculatedFieldSettings.Default);

                // Ensure same instance was returned (came from cache)
                Assert.That(res2, Is.SameAs(res1));

                // Delete the field
                field.Delete();

                // Rerun
                CalculatedFieldMetadata res3 = provider.GetCalculatedFieldMetadata(calcField.Id, CalculatedFieldSettings.Default);

                // Ensure different instance was returned (not from cache)
                Assert.That(res3, Is.Not.SameAs(res1));
                Assert.That(res3.Exception, Is.Not.Null); // because the script is now invalid
            }
            finally
            {
                if (field != null)
                    field.Delete();
                if (calcField != null)
                    calcField.Delete();
                if (type != null)
                    type.Delete();
            }
        }

        [Test]
        public void EnsureMetadata_Invalidates_WhenFieldRenamedToCorrectName()
        {
            EntityType type = null;
            Field field = null;
            Field calcField = null;

            try
            {
                // Create scenario
                type = new EntityType();
                type.Name = "CFType";
                field = new StringField().As<Field>();
                field.Name = "CFFieldWrongName";
                field.Save();
                calcField = new StringField().As<Field>();
                calcField.Name = "CFCalcField";
                calcField.FieldCalculation = "'A'+CFFieldRightName";
                calcField.IsCalculatedField = true;
                calcField.Save();
                type.Fields.Add(field);
                type.Fields.Add(calcField);
                type.Save();

                // Get provider
                ICalculatedFieldMetadataProvider provider = Factory.CalculatedFieldMetadataProvider;

                // Run
                CalculatedFieldMetadata res1 = provider.GetCalculatedFieldMetadata(calcField.Id, CalculatedFieldSettings.Default);
                CalculatedFieldMetadata res2 = provider.GetCalculatedFieldMetadata(calcField.Id, CalculatedFieldSettings.Default);

                // Ensure same instance was returned (came from cache)
                Assert.That(res1.Exception, Is.Not.Null, "res1 should have error"); // because the script is invalid
                Assert.That(res2, Is.SameAs(res1), "res2 should be same as res1");

                // Rename the field
                field.Name = "CFFieldRightName";
                field.Save();

                // Rerun
                CalculatedFieldMetadata res3 = provider.GetCalculatedFieldMetadata(calcField.Id, CalculatedFieldSettings.Default);

                // Ensure different instance was returned (not from cache)
                Assert.That(res3, Is.Not.SameAs(res1), "res3 should be != res1");
                Assert.That(res3.Exception, Is.Null, "res3 should not have error"); // because the script is now fixed
                Assert.That(res3.Expression, Is.Not.Null, "res3 should have expression"); // because the script is now fixed
            }
            finally
            {
                if (field != null)
                    field.Delete();
                if (calcField != null)
                    calcField.Delete();
                if (type != null)
                    type.Delete();
            }
        }

    }
}
