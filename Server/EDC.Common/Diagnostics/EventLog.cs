// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;

namespace EDC.Diagnostics
{
    /// <summary>
    ///     An event log.
    ///     This event log passes the events to any registered writers which do
    ///     the actual writing of events to logs.
    /// </summary>
    public class EventLog : IEventLog
    {
        /// <summary>
        ///     True if any of the writers have errors enabled, false otherwise.
        /// </summary>
        private readonly bool _errorEnabled;


        /// <summary>
        ///     The list of event log writers.
        /// </summary>
        private readonly List<IEventLogWriter> _eventLogWriters;


        /// <summary>
        ///     True if any of the writers have information enabled, false otherwise.
        /// </summary>
        private readonly bool _informationEnabled;


        /// <summary>
        ///     True if any of the writers have trace enabled, false otherwise.
        /// </summary>
        private readonly bool _traceEnabled;


        /// <summary>
        ///     True if any of the writers have warnings enabled, false otherwise.
        /// </summary>
        private readonly bool _warningEnabled;


        /// <summary>
        /// The machine name.
        /// </summary>
        private readonly string _machineName;


        /// <summary>
        /// The process name.
        /// </summary>
        private readonly string _processName;


        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="eventLogWriters">The list of writers.</param>
        public EventLog(IEnumerable<IEventLogWriter> eventLogWriters)
        {
            if (eventLogWriters == null)
            {
                throw new ArgumentNullException(nameof(eventLogWriters));
            }

            _eventLogWriters = new List<IEventLogWriter>();

            // Ensure we only register writers that will result in messages being logged.
            foreach (IEventLogWriter writer in eventLogWriters.Where(writer => writer.ErrorEnabled || writer.InformationEnabled || writer.TraceEnabled || writer.WarningEnabled))
            {
                _eventLogWriters.Add(writer);
            }

            _machineName = GetHostName();            
            _processName = Process.GetCurrentProcess().MainModule.ModuleName;

            // Cache the settings for performance reasons.
            _errorEnabled = _eventLogWriters.Any(w => w.ErrorEnabled);
            _informationEnabled = _eventLogWriters.Any(w => w.InformationEnabled);
            _warningEnabled = _eventLogWriters.Any(w => w.WarningEnabled);
            _traceEnabled = _eventLogWriters.Any(w => w.TraceEnabled);
        }


        #region IEventLog Members


        /// <summary>
        ///     Gets whether error messages are logged.
        /// </summary>
        public bool ErrorEnabled
        {
            get { return _errorEnabled; }
            set { throw new NotImplementedException(); }
        }


        /// <summary>
        ///     Gets whether informational messages are logged.
        /// </summary>
        public bool InformationEnabled
        {
            get { return _informationEnabled; }
            set { throw new NotImplementedException(); }
        }


        /// <summary>
        ///     Gets whether trace messages are logged.
        /// </summary>
        public bool TraceEnabled
        {
            get { return _traceEnabled; }
            set { throw new NotImplementedException(); }
        }


        /// <summary>
        ///     Gets whether warning messages are logged.
        /// </summary>
        public bool WarningEnabled
        {
            get { return _warningEnabled; }
            set { throw new NotImplementedException(); }
        }


        /// <summary>
        ///     Fired when an entry is written to the event log.
        /// </summary>
        public event WriteEventEventHandler WriteEvent;


        /// <summary>
        ///     Write a error message to the event log.
        /// </summary>
        /// <param name="message">
        ///     A string containing zero or more format items.
        /// </param>
        /// <param name="parameters">
        ///     An object array containing zero or more objects to format.
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void WriteError(string message, params object[] parameters)
        {
            Write(EventLogLevel.Error, message, parameters);
        }


        /// <summary>
        ///     Write an informational message to the event log.
        /// </summary>
        /// <param name="message">
        ///     A string containing zero or more format items.
        /// </param>
        /// <param name="parameters">
        ///     An object array containing zero or more objects to format.
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void WriteInformation(string message, params object[] parameters)
        {
            Write(EventLogLevel.Information, message, parameters);
        }


        /// <summary>
        ///     Write a trace message to the event log.
        /// </summary>
        /// <param name="message">
        ///     A string containing zero or more format items.
        /// </param>
        /// <param name="parameters">
        ///     An object array containing zero or more objects to format.
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void WriteTrace(string message, params object[] parameters)
        {
            Write(EventLogLevel.Trace, message, parameters);
        }


        /// <summary>
        ///     Write a warning message to the event log.
        /// </summary>
        /// <param name="message">
        ///     A string containing zero or more format items.
        /// </param>
        /// <param name="parameters">
        ///     An object array containing zero or more objects to format.
        /// </param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void WriteWarning(string message, params object[] parameters)
        {
            Write(EventLogLevel.Warning, message, parameters);
        }


        #endregion


        /// <summary>
        ///     Raise the <see cref="WriteEvent" /> event.
        /// </summary>
        /// <param name="entry">
        ///     The entry written to event log.
        /// </param>
        protected void OnWriteEvent(EventLogEntry entry)
        {
            WriteEvent?.Invoke(this, new EventLogWriteEventArgs(entry));
        }


        /// <summary>
        ///     Returns true if the specified error level can be written, false otherwise.
        /// </summary>
        /// <param name="level">The error level.</param>
        /// <returns></returns>
        private bool CanWriteEntry(EventLogLevel level)
        {
            bool canWriteEntry = false;

            if ((level == EventLogLevel.Error) && (_errorEnabled))
            {
                canWriteEntry = true;
            }
            else if ((level == EventLogLevel.Warning) && (_warningEnabled))
            {
                canWriteEntry = true;
            }
            else if ((level == EventLogLevel.Information) && (_informationEnabled))
            {
                canWriteEntry = true;
            }
            else if ((level == EventLogLevel.Trace) && (_traceEnabled))
            {
                canWriteEntry = true;
            }

            return canWriteEntry;
        }        


        /// <summary>
        ///     Writes the specified message using the specified level.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="message">The message.</param>
        /// <param name="parameters">The parameters.</param>
        private void Write(EventLogLevel level, string message, params object[] parameters)
        {
            string source;
            
            if (!CanWriteEntry(level))
            {
                // No writers are enabled for this level.
                return;
            }

            if (level == EventLogLevel.Error || level == EventLogLevel.Warning || level == EventLogLevel.Information)
            {
                var stackFrame = new StackFrame(2);
                Type reflectedType = stackFrame.GetMethod().ReflectedType;

                source = reflectedType != null ? string.Concat(reflectedType.FullName, "::", stackFrame.GetMethod().Name) : "(unknown)";
            }
            else
            {
                source = "(Omitted for trace messages)";
            }

            if (parameters == null || parameters.Length <= 0)
            {
                WriteEntry(level, source, message);
            }
            else
            {
                WriteEntry(level, source, string.Format(message, parameters));
            }
        }


        /// <summary>
        ///     Writes the specified entry to the event log writers,
        /// </summary>
        /// <param name="level">The error level.</param>
        /// <param name="source">The message source.</param>
        /// <param name="message">The message.</param>
        private void WriteEntry(EventLogLevel level, string source, string message)
        {
            DiagnosticsRequestContextData context = DiagnosticsRequestContext.GetContext();

            string userName = string.Empty;
            long tenantId = -1;
            string tenantName = string.Empty;

            if (context != null)
            {
                userName = context.UserName;
                tenantId = context.TenantId;
                tenantName = context.TenantName;
            }

            // Create an event log entry and pass it the writers.                
            var logEntry = new EventLogEntry(DateTime.UtcNow,
                Stopwatch.GetTimestamp(),
                level,
                Thread.CurrentThread.ManagedThreadId,
                source ?? string.Empty,
                message ?? string.Empty,
                tenantId,
                tenantName,
                userName)
            {
                Machine = _machineName,
                Process = _processName
            };


            OnWriteEvent(logEntry);

            foreach (IEventLogWriter logWriter in _eventLogWriters)
            {
                try
                {                    
                    logWriter.WriteEntry(logEntry);
                }
                catch (Exception ex)
                {
                    // Prevent errors from one writer affecting others.
                    Trace.TraceError($"An error occured writing a message to a log writer. Writer type {logWriter.GetType().Name}, error {ex}.");
                }
            }
        }

        /// <summary>
        /// Gets the name of the host.
        /// </summary>
        /// <returns></returns>
        private string GetHostName()
        {
            try
            {
                return Dns.GetHostEntry(Dns.GetHostName()).HostName;
            }
            catch
            {
                return Environment.MachineName;
            }            
        }
    }
}