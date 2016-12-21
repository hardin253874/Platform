// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.IO;
using System.Globalization;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Utc;
using EDC.ReadiNow.IO;
using ReadiNow.DocGen;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// Implementation of the log activity available for use in Workflows.  Allows a workflow to add a message to the server logs.
    /// </summary>
    public class GenerateDocImplementation : ActivityImplementationBase, IRunNowActivity
    {
        private const string DescriptionFormat = "Report generated from template '{0}' '{1}'";

        /// <summary>
        /// Runs when the activity is run by the workflow.
        /// </summary>
        /// <param name="context">The run state.</param>
        /// <param name="inputs">The inputs.</param>
        void IRunNowActivity.OnRunNow(IRunState context, ActivityInputs inputs)
        {
            using (Profiler.Measure("GenerateDocImplementation.OnRunNow"))
            {
                var inGenerateDocTemplateKey = GetArgumentKey("inGenerateDocTemplate");
                var inGenerateDocNameKey = GetArgumentKey("inGenerateDocName");
                var inGenerateDocSourceKey = GetArgumentKey("inGenerateDocSource");

                var reportTemplate = ((IEntity)inputs[inGenerateDocTemplateKey]).As<ReportTemplate>();

                var name = inputs.ContainsKey(inGenerateDocNameKey) ? (string)inputs[inGenerateDocNameKey] : null;

                if (String.IsNullOrWhiteSpace(name))
                    name = GenerateName(reportTemplate.Name);

                var description = string.Format(DescriptionFormat, reportTemplate.Name ?? "[Unnamed]", DateTime.Now.ToShortTimeString());

                var sourceResource = inputs.ContainsKey(inGenerateDocSourceKey) ? ((IEntity)inputs[inGenerateDocSourceKey]) : null;

                Document generatedDoc;

                using (CustomContext.SetContext(context.EffectiveSecurityContext))
                {
                    var timeZone = RequestContext.GetContext().TimeZone ?? TimeZoneHelper.SydneyTimeZoneName;
                    var templateFile = reportTemplate.ReportTemplateUsesDocument;

                    try
                    { 
                        generatedDoc = GenerateDoc(templateFile, sourceResource, name, description, timeZone);
                    }
                    catch (DocGenException ex)
                    {
                        throw new WorkflowRunException(ex.Message);
                    }
                }

                context.SetArgValue(ActivityInstance, GetArgumentKey("core:outGenerateDocDoc"), generatedDoc);
            }            
        }


        /// <summary>
        /// Generate a doucment
        /// </summary>
        /// <param name="templateDoc">The template file to generate from</param>
        /// <param name="sourceResource">The optional resource to use a data for the generation</param>
        /// <param name="newName">The name of the generated file</param>
        /// <param name="newDescription">The name of the generated description</param>
        /// <param name="timezoneName">the timezone the report will be evaluated in</param>
        public static Document GenerateDoc(Document templateDoc, IEntity sourceResource, string newName, string newDescription, string timezoneName)
        {
            var sourceResourceId = sourceResource != null ? sourceResource.Id : 0;
            var templateName = templateDoc.Name ?? "[Unnamed]";

            var tmpFileName = string.Format("{0:yyyyMMddhhmmssfff}.doc", DateTime.UtcNow);
            var tmpFilePath = Path.Combine(Path.GetTempPath(), tmpFileName);

            try
            {
                // Generate document
                using (var templateStream = FileRepositoryHelper.GetFileDataStreamForEntity(templateDoc))
                {
                    if (templateStream == null)
                        throw new WorkflowRunException("Unable to find the template file '{0}'", templateName);

                    using (Stream targetStream = File.Create(tmpFilePath))
                    {
                        var genSettings = new GeneratorSettings
                        {
                            WriteDebugFiles = false,
                            ThrowOnError = true,
                            TimeZoneName = timezoneName,                                     
                            SelectedResourceId = sourceResourceId,
                        };

                        Factory.DocumentGenerator.CreateDocument(templateStream, targetStream, genSettings);        // Any gen failures are handled higher up               
                    }
                }


                string tempHash;

                using (var source = new FileStream(tmpFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    tempHash = FileRepositoryHelper.AddTemporaryFile(source);
                }

                var document = DocHelper.CreateDoc(tempHash, newName, newDescription, templateDoc.FileExtension, templateDoc.DocumentFileType);

                return document;
            }
            finally
            {
                // clean up
                if (File.Exists(tmpFileName))
                    File.Delete(tmpFilePath);
            }
        }

        static string GenerateName(string templateName)
        {
            return templateName + ' ' + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
        }

      
    }
}
