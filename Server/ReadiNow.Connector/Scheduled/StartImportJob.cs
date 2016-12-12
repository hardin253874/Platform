// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Core;
using ReadiNow.Connector.ImportSpreadsheet;
using Autofac;
using EDC.ReadiNow.IO.RemoteFileFetcher;
using EDC.ReadiNow.Security;

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

            try
            {
                var importConfig = scheduledImportConfig.SicImportConfig;
                
                if (importConfig == null)
                {
                    throw GenerateJobException("Failed to import, no import configuration provided.", scheduledImportConfig);
                }

                var url = scheduledImportConfig.SicUrl;

                if (String.IsNullOrEmpty(url))
                {
                    throw GenerateJobException("Failed to import, no FTP address provided.", scheduledImportConfig);
                }

                var fileToken = GetToTemporaryFile(scheduledImportConfig);

                if (fileToken != null)
                {
                    var settings = new ImportSettings
                    {
                        FileName = url,
                        FileToken = fileToken,
                        ImportConfigId = importConfig.Id,
                        SuppressSecurityCheckOnImportConfig = false,
                        TimeZoneName = null /* UTC */
                    };

                    var importer = Factory.Current.Resolve<ISpreadsheetImporter>();

                    importer.StartImport(settings);
                }
               
            }

            catch (PlatformSecurityException ex)
            {
                EventLog.Application.WriteInformation($"StartImportJob.Execute: Platform security exception: {ex}");
                throw GenerateJobException(ex.Message, scheduledImportConfig);
            }
            catch (Exception ex)
            {
                EventLog.Application.WriteError("StartImportJob.Execute: Unexpected exception thrown: {0}", ex);
                throw GenerateJobException("Unexpected exception when performing scheduled import.", scheduledImportConfig);
            }
        }


        private string GetToTemporaryFile(ScheduledImportConfig config)
        {
            var secureId = config.SicPasswordSecureId;
            var password = secureId != null ? Factory.SecuredData.Read((Guid) secureId) : string.Empty;
            try
            { 
                return Factory.RemoteFileFetcher.GetToTemporaryFile(config.SicUrl, config.SicUsername, password);
            }
            catch (ConnectionException ex)
            {
                var message = $"Scheduled Import '{config.Name}' failed.\nDetails:\n   File: {config.SicUrl} \n   Message: {ex.Message}";
                EventLog.Application.WriteInformation(message);

                SecurityBypassContext.Elevate(() =>
                {
                    CreateFailedImportRunEntity(config, message);
                });

                return null;
            }
            catch (Exception ex)
            {
                throw GenerateJobException(ex.Message, config);
            }
        }





        /// <summary>
        ///     Creates an importRun entity - does not save it.
        /// </summary>
        /// <param name="config">The import configuration.</param>
        /// <param name="message"></param>
        /// <returns>Returns the ID of the import run.</returns>
        private void CreateFailedImportRunEntity(ScheduledImportConfig config, string message)
        {
            var importConfig = config.SicImportConfig;

            // Create a new import run
            ImportRun importRun = Entity.Create<ImportRun>();
            importRun.ImportFileName = config.SicUrl ?? string.Empty;
            importRun.ImportRunStatus_Enum = WorkflowRunState_Enumeration.WorkflowRunFailed;
            importRun.ImportConfigUsed = importConfig;
            importRun.ImportMessages = message;
            importRun.ImportRunStarted = DateTime.UtcNow;
            importRun.ImportRunFinished = DateTime.UtcNow;

            importRun.Save();
        }
    }
}
