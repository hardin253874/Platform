// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EDC.ReadiNow.Diagnostics;

using CL = Common.Logging;


namespace EDC.ReadiNow.Scheduling
{


    /// <summary>
    /// A logger that takes Common.Logging messages and pushes them into the EDC logging.
    /// </summary>
    public class EdcLogger : CL.Factory.AbstractLogger
    {
        protected override void WriteInternal(CL.LogLevel level, object message, Exception exception)
        {
            if (IsAnnoyingMessage(level, message))
                level = CL.LogLevel.Trace;

            string displayString;

            if (exception != null)
                displayString = string.Format("{0}: Exception: {1}", message.ToString(), exception.ToString());
            else
                displayString = message.ToString();
        
            switch (level)
            {
                case CL.LogLevel.Error:
                case CL.LogLevel.Fatal:
                    EventLog.Application.WriteError(displayString);
                    break;

                case CL.LogLevel.Info:

                    EventLog.Application.WriteInformation(displayString);
                    break;

                case CL.LogLevel.Warn:
                    EventLog.Application.WriteWarning(displayString);
                    break;

                default:
                    EventLog.Application.WriteTrace(displayString);
                    break;
            }
        }

        bool IsAnnoyingMessage(CL.LogLevel level, object message)
        {
            var messageString = message.ToString();
            return
                messageString.StartsWith("ConnectionAndTransactionHolder passed to RollbackConnection was null, ignoring") ||
                (level == CL.LogLevel.Info
                && (
                       (messageString.StartsWith("ClusterManager: detected") &&
                        messageString.EndsWith("failed or restarted instances."))
                       ||
                       (messageString.StartsWith("ClusterManager: Scanning for instance") &&
                        messageString.EndsWith("failed in-progress jobs."))));
        }

        public override bool IsTraceEnabled
        {
            get { return EventLog.Application.TraceEnabled; }
        }

        public override bool IsDebugEnabled
        {
            get { return false; }
        }

        public override bool IsErrorEnabled
        {
            get { return EventLog.Application.ErrorEnabled; }
        }

        public override bool IsFatalEnabled
        {
            get { return IsErrorEnabled; }
        }

        public override bool IsInfoEnabled
        {
            get { return  EventLog.Application.InformationEnabled; }
        }

        public override bool IsWarnEnabled
        {
            get { return EventLog.Application.WarningEnabled; }
        }
    }

    
    public class EdcLoggerFactoryAdapter : CL.Factory.AbstractCachingLoggerFactoryAdapter
    {
        protected override CL.ILog CreateLogger(string name)
        {
            return new EdcLogger();
        }
    }
}
