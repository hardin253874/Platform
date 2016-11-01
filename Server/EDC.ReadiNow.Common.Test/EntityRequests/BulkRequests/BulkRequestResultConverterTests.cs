// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.EntityRequests.BulkRequests;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;
using EDC.ReadiNow.Security;
using EDC.ReadiNow.Test;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.EntityRequests.BulkRequests
{
    [TestFixture]
    [RunAsDefaultTenant]
    public class BulkRequestResultConverterTests
    {
        [Test]
        public void BuildAndSecureResults_WriteOnlyFields()
        {
            BulkRequestResult bulkRequestResult;
            long administratorUserAccountId;
            EntityRequest entityRequest;
            IList<EntityData> results;
            EntityData result;
            EntityRef passwordField;
            EntityRef nameField;
            PasswordPolicy passwordPolicy;
            
            administratorUserAccountId = Entity.GetId("core:administratorUserAccount");
            Assert.That(administratorUserAccountId, Is.Positive, "Administrator account missing");

            passwordField = new EntityRef("core:password");
            Assert.That(passwordField.Entity.As<Field>(), Has.Property("IsFieldWriteOnly").True);

            nameField = new EntityRef("core:name");

            passwordPolicy = Entity.Get<PasswordPolicy>("core:passwordPolicyInstance");
            Assert.That(passwordPolicy, Is.Not.Null, "Password policy missing");

            // Build the results
            entityRequest = new EntityRequest
            {
                QueryType = QueryType.Basic,
                Entities = new [] { new EntityRef("core:administratorUserAccount") },
                RequestString = "name, password",
                Hint = "NUnit test"
            };
            bulkRequestResult = BulkResultCache.GetBulkResult(entityRequest);

            results = BulkRequestResultConverter.BuildAndSecureResults(
                bulkRequestResult,
                new[] {administratorUserAccountId}, 
                SecurityOption.SkipDenied).ToList();
            Assert.That(results, Has.Count.EqualTo(1), "Incorrect number of results");

            result = results.FirstOrDefault();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Fields, Has.Count.EqualTo(2), "Incorrect number of fields");
            Assert.That(
                result.Fields,
                Has.Exactly(1)
                    .Property("FieldId").EqualTo(nameField)
                   .And.Property("Value").Property("Value").EqualTo("Administrator"),
                "Incorrect password field value");
            Assert.That(
                result.Fields,
                Has.Exactly(1)
                    .Property("FieldId").EqualTo(passwordField)
                    .And.Property("Value").Property("Value").EqualTo(
                        new string('*', 
                            passwordPolicy.MinimumPasswordLength ?? WriteOnlyFieldReadValueGenerator.DefaultWriteOnlyStringResultLength)), 
                "Incorrect password field value");
        }
    }
}
