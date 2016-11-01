// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Core;
using ReadiNow.Connector.ImportSpreadsheet;
using Autofac;
using Quartz;
using EDC.ReadiNow.IO.RemoteFileFetcher;

namespace ReadiNow.Connector.Scheduled
{
    /// <summary>
    /// A Scheduled job that runs a workflow
    /// </summary>
    public class StartImportJob: ItemBase
    {
        public override void Execute(EntityRef scheduledItemRef)
        {
            var scheduledImportConfig = Entity.Get<ScheduledImportConfig>(scheduledItemRef);

            var importConfig = scheduledImportConfig.SicImportConfig;
            
            if (importConfig == null)
            {
                throw GenerateException("Failed to import, no import configuration provided.", scheduledImportConfig);
            }

            var url = scheduledImportConfig.SicUrl;

            if (String.IsNullOrEmpty(url))
            {
                throw GenerateException("Failed to import, no FTP address provided.", scheduledImportConfig);
            }

            // username and password are optional. they may be embedded in the URL
            
            try
            {
                var fileToken = FetchToTemporaryFile(scheduledImportConfig);

                if (fileToken != null)
                {
                    var settings = new ImportSettings
                    {
                        FileToken = fileToken,
                        ImportConfigId = importConfig.Id,
                        SuppressSecurityCheckOnImportConfig = false,
                        TimeZoneName = null /* UTC */
                    };

                    var importer = Factory.Current.Resolve<ISpreadsheetImporter>();

                    importer.StartImport(settings);
                }
            }
            catch (Exception ex)
            {
                EventLog.Application.WriteError("StartImportJob.Execute: Unexpected exception thrown: {0}", ex);
                throw GenerateException("Unexpected exception when performing scheduled import.", scheduledImportConfig);
            }
        }


        private string FetchToTemporaryFile(ScheduledImportConfig config)
        {
            var secureId = config.SicPasswordSecureId;
            var password = secureId != null ? Factory.SecuredData.Read((Guid) secureId) : string.Empty;
            try
            { 
                return Factory.RemoteFileFetcher.FetchToTemporaryFile(config.SicUrl, config.SicUsername, password);
            }
            catch (ConnectionException ex)
            {
                var message = $"Scheduled Import '{config.Name}' failed.\nDetails:\n   File: {config.SicUrl} \n   Message: {ex.Message}";
                EventLog.Application.WriteInformation(message);

                CreateFailedImportRunEntity(config.SicImportConfig, message);

                return null;
            }
            catch (Exception ex)
            {
                throw GenerateException(ex.Message, config);
            }
        }
     

        private JobExecutionException GenerateException(string message, ScheduledImportConfig entity)
        {
            string name = entity.Name ?? "[Unnamed]";
            return new JobExecutionException($"'{name}'({entity.Id}) failed. {message}");

        }


        /// <summary>
        ///     Creates an importRun entity - does not save it.
        /// </summary>
        /// <param name="importConfig">The import configuration.</param>
        /// <param name="importSettings">Settings passed in for the current run.</param>
        /// <returns>Returns the ID of the import run.</returns>
        private void CreateFailedImportRunEntity(ImportConfig importConfig, string message)
        {
            // Create a new import run
            ImportRun importRun = Entity.Create<ImportRun>();
            importRun.ImportRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunFailed;
            importRun.ImportConfigUsed = importConfig;
            importRun.ImportMessages = message;
            importRun.ImportRunStarted = DateTime.UtcNow;
            importRun.ImportRunFinished = DateTime.UtcNow;

            importRun.Save();
        }
    }
}
