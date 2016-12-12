// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.Diagnostics;
using EDC.ReadiNow.Resources;
using Microsoft.Win32;
using System;
using EDC.ReadiNow.Configuration;

namespace EDC.ReadiNow.Diagnostics
{
    /// <summary>
    ///     Represents a process monitor writer.
    /// </summary>
    public class ProcessMonitorWriter
    {
        /// <summary>
		///     Static instance of the ProcessMonitorWriter.
		/// </summary>
		private static readonly Lazy<ProcessMonitorWriter> instance = new Lazy<ProcessMonitorWriter>(() => new ProcessMonitorWriter(), true);

        /// <summary>
        /// The time in milliseconds to check the enabled flag.
        /// </summary>
        private readonly long _isEnabledUpdatePeriodMilliseconds = 30 * 1000;

        /// <summary>
        /// The application prefix.
        /// </summary>
        private const string AppPrefix = "ReadiNow: ";

        /// <summary>
        /// True if the writer is enabled, false otherwise.
        /// </summary>
        private volatile bool _isEnabled;

        /// <summary>
        /// The last time the enabled flag was checked.
        /// </summary>
        private volatile int _isEnabledLastCheckedMilliseconds = int.MaxValue;

        /// <summary>
        ///     Prevents a default instance of the <see cref="ProcessMonitorWriter" /> class from being created.
        /// </summary>
        private ProcessMonitorWriter()
        {
        }

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        public static ProcessMonitorWriter Instance
        {
            get
            {
                return instance.Value;
            }
        }

        /// <summary>
        /// Returns true if the writer is enabled. False otherwise
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return GetIsEnabled();
            }
        }

		/// <summary>
		/// Writes the specified message to process monitor.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The arguments.</param>
		/// <exception cref="System.ArgumentNullException">message</exception>
        public void Write(string message, params object[] args)
        {
            try
            {
                if (message == null || !IsEnabled)
                {
                    return;
                }

                ProcessMonitorNativeMethods.ProcMonDebugOutput(string.Concat(AppPrefix, message), args);
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch
            // ReSharper restore EmptyGeneralCatchClause
            {
                // Ignore errors
            }
        }

        /// <summary>
        /// Checks if the process monitor is enabled in the configuration file.
        /// </summary>
        /// <returns></returns>
        private static bool GetProcMonWriterEnabledRegValue()
        {
	        return ConfigurationSettings.GetServerConfigurationSection( ).ProcessMonitor.Enabled;
        }

        /// <summary>
        /// Returns true if the writer is enabled, false otherwise.
        /// </summary>
        /// <returns></returns>
        private bool GetIsEnabled()
        {
            try
            {
                int nowTicks = Environment.TickCount;                
                int diffMs = nowTicks - _isEnabledLastCheckedMilliseconds;

                if (diffMs >= 0 && diffMs < _isEnabledUpdatePeriodMilliseconds)
                {
                    // Less than the update period, return the cached state
                    return _isEnabled;
                }

                // Update the last checked time
                _isEnabledLastCheckedMilliseconds = nowTicks;

                // Check the configuration file
                bool isEnabled = GetProcMonWriterEnabledRegValue();

                // Update the enabled state
                _isEnabled = isEnabled;

                return isEnabled;
            }
            catch
            {
                return false;
            }
        }
    }
}