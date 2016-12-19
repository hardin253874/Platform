// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Scheduling;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Core;
using Autofac;
using EDC.ReadiNow.IO.RemoteFileFetcher;
using EDC.ReadiNow.Security;
using ReadiNow.ExportData;
using Quartz;

namespace ReadiNow.Connector.Scheduled
{
    /// <summary>
    /// A Scheduled job that runs a workflow
    /// </summary>
    public class StartExportJob: ItemBase
    {
        protected override bool RunAsOwner
        {
            get
            {
                return true;
            }
        }

        public override void Execute(EntityRef scheduledItemRef)
        {
            string logMessage = null;
            bool failed = true;

            var scheduledExportConfig = Entity.Get<ScheduledExportConfig>(scheduledItemRef, ScheduledExportConfig.Name_Field);

            if (scheduledExportConfig == null)
            {
                throw GenerateJobException( "Unexpected error, ScheduleItemRef is not a ScheduledExportConfig.", null );
            }

            

            try
            {
               var report = scheduledExportConfig.SecReport;
                
                if (report == null)
                {
                    throw new ExpectedErrorCondition("Failed to import, no report configuration provided.");
                }

                var url = scheduledExportConfig.SicUrl;

                if (String.IsNullOrEmpty(url))
                {
                    throw new ExpectedErrorCondition("Failed to export, no FTP address provided.");
                }

                var username = scheduledExportConfig.SicUsername;

                if (String.IsNullOrEmpty(username))
                {
                    throw new ExpectedErrorCondition("Failed to export, no username provided.");
                }

                var secureId = scheduledExportConfig.SicPasswordSecureId;
                var password = secureId != null ? Factory.SecuredData.Read((Guid)secureId) : string.Empty;

                var format = scheduledExportConfig.SecFileType_Enum;

                if (format == null)
                {
                    throw new ExpectedErrorCondition("Failed to export, no file format type provided.");
                }

                var fileformat = ConvertFileFormat(format); 

                var exportInterface = Factory.Current.Resolve<IExportDataInterface>();

                var exportInfo = exportInterface.ExportData(report.Id, new ExportSettings { Format = fileformat, TimeZone = null /* UTC */ });

                var fileFetcher = Factory.Current.Resolve<IRemoteFileFetcher>();

                fileFetcher.PutFromTemporaryFile(exportInfo.FileHash, url, username, password);

                logMessage = "Success";
                failed = false;
            }


            catch (ExpectedErrorCondition ex)
            {
                logMessage = ex.Message;
                // expected exception swallowed, log message generated in finally
            }
            catch (ConnectionException ex)
            {
                logMessage = ex.Message;
                // expected exception swallowed, log message generated in finally
            }
            catch (PlatformSecurityException ex)
            {
                logMessage = ex.Message;
                // expected exception swallowed, log message generated in finally
            }
            catch (JobExecutionException ex)
            {
                EventLog.Application.WriteError("StartImportJob.Execute: Unexpected exception thrown: {0}", ex);

                logMessage = "Failed with an internal error.";
                throw;
            }

            catch (Exception ex)
            {
                EventLog.Application.WriteError("StartImportJob.Execute: Unexpected exception thrown: {0}", ex);
                logMessage = "Failed with an internal error.";
                throw GenerateJobException("Unexpected exception when performing scheduled import.", scheduledExportConfig);
            }
            finally
            {
                EventLog.Application.WriteTrace($"{scheduledExportConfig.Id} {(failed ? "Fail" : "Success")}: {logMessage}");

                SecurityBypassContext.Elevate(() =>
                {
                   var logEntry = new TenantLogEntry
                   {
                       Name = $"Schedule Export: {scheduledExportConfig.Name ?? "[Unnamed]"}",
                       Description = logMessage,
                       LogEntrySeverity_Enum = failed ? LogSeverityEnum_Enumeration.ErrorSeverity : LogSeverityEnum_Enumeration.InformationSeverity,
                       LogEventTime = DateTime.Now
                   };
                    logEntry.GetRelationships("core:secRunLog", Direction.Reverse).Add(scheduledExportConfig);
                    logEntry.Save();
                });
            }
        }




        private ExportFormat ConvertFileFormat(ExportFileTypeEnum_Enumeration? importFormat) 
        {
            switch(importFormat)
            {
                case ExportFileTypeEnum_Enumeration.ExportFileTypeCsv:   return ExportFormat.Csv;
                case ExportFileTypeEnum_Enumeration.ExportFileTypeExcel: return ExportFormat.Excel;
                case ExportFileTypeEnum_Enumeration.ExportFileTypeWord:  return ExportFormat.Word;
                default:
                    throw new ArgumentException(nameof(importFormat));
            }
        }

        /// <summary>
        /// Thrown whe hitting an error condition that is expected and handled.
        /// </summary>
        private class ExpectedErrorCondition: Exception
        {
            public ExpectedErrorCondition(string message): base(message)
            {

            }
        }
    }
}
