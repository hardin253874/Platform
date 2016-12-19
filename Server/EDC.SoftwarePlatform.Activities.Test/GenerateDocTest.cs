// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Test;
using NUnit.Framework;
using EDC.ReadiNow.Database;
using EDC.ReadiNow.Utc;
using System.Reflection;
using System.IO;
using EDC.ReadiNow.IO;

namespace EDC.SoftwarePlatform.Activities.Test
{
    [TestFixture]
    //[Category("ExtendedTests")]
    [Category("WorkflowTests")]
    public class GenerateDocTest : TestBase
    {
        [Test]
        //[RunWithTransaction]
        [RunAsDefaultTenant]
        public void TestGenerate()
        {
            var template = CreateTestTemplate("Single employee.docx");
            var employeeType = Entity.GetByName<EntityType>("Employee").First();
            var employee = Entity.Create(employeeType);

            var doc = GenerateDocImplementation.GenerateDoc(template.ReportTemplateUsesDocument, employee, "testName" + DateTime.UtcNow.ToString(), "testDescription" + DateTime.UtcNow.ToString(), TimeZoneHelper.SydneyTimeZoneName);

            Assert.That(doc, Is.Not.Null);
            Assert.That(doc.Name, Is.Not.Null);
            Assert.That(doc.CurrentDocumentRevision, Is.Not.Null);
            Assert.That(doc.CurrentDocumentRevision.Name, Is.Not.Null);
            Assert.That(doc.CurrentDocumentRevision.FileDataHash, Is.Not.Null);
            using (var stream = FileRepositoryHelper.GetFileDataStreamForEntity(new EntityRef(doc.Id)))
            {
                Assert.That(stream, Is.Not.Null);
                Assert.That(stream.Length, Is.Not.EqualTo(0));
            }
        }


        [Test]
        [RunWithTransaction]
        [RunAsDefaultTenant]
        [ExpectedException("ReadiNow.DocGen.DocGenException")]
        public void TestGenerate_MissingContext()
        {
            var template = CreateTestTemplate("Single employee.docx");
            var doc = GenerateDocImplementation.GenerateDoc(template.ReportTemplateUsesDocument, null, "testName" + DateTime.UtcNow.ToString(), "testDescription" + DateTime.UtcNow.ToString(), TimeZoneHelper.SydneyTimeZoneName);

            Assert.That(doc, Is.Null);
        }

        private ReportTemplate CreateTestTemplate(string fileName)
        {
            using (var ctx = DatabaseContext.GetContext(requireTransaction: true))
            {
                var wordDocType = Entity.Get<DocumentType>(new EntityRef("wordDocumentDocumentType"));
                var stream = GetTestDocStream(fileName);
                //var hash = CryptoHelper.ComputeSha256Hash(stream);
                var tempHash = FileRepositoryHelper.AddTemporaryFile(stream);

                var docRev = Entity.Create<DocumentRevision>();
                docRev.FileDataHash = tempHash;
                docRev.Name = fileName;
                docRev.FileExtension = "docx";
                docRev.DocumentFileType = wordDocType;
                docRev.Save();

                var hash = docRev.FileDataHash;

                var doc = Entity.Create<Document>();
                doc.FileDataHash = hash;
                doc.DocumentHasDocumentRevision.Add(docRev);
                doc.CurrentDocumentRevision = docRev;
                doc.FileDataHash = hash;
                doc.Name = fileName;
                doc.FileExtension = "docx";
                doc.DocumentFileType = wordDocType;

                var template = Entity.Create<ReportTemplate>();
                template.Name = fileName;
                template.ReportTemplateUsesDocument = doc;

                ctx.CommitTransaction();
                return template;

            }
        }

        private Stream GetTestDocStream(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("EDC.SoftwarePlatform.Activities.Test.Files." + fileName);
            return stream;
        }
    }
}