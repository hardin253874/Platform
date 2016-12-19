// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Linq;

namespace EDC.Diagnostics
{
    /// <summary>
    /// Represents an event in the event log.
    /// </summary>
	[Serializable]
	public class EventLogEntry
	{
		private readonly DateTime _date;
		private Guid _id = Guid.Empty;
        private string _machine = string.Empty;
        private string _process = string.Empty;
        private readonly EventLogLevel _level;
		private readonly string _message;
		private readonly string _source;
		private readonly int _threadId;
		private readonly long _timestamp;
        private readonly long _tenantId;
        private readonly string _tenantName;
        private readonly string _userName;
        private readonly string _logFilePath;

        /// <summary>
        ///     Initializes a new instance of the EventLogEntry class.
        /// </summary>
        public EventLogEntry(DateTime date, long timestamp, EventLogLevel level, int threadId, string source, string message, long tenantId, string tenantName, string userName, string logFilePath = null)
        {
            _date = date;
            _timestamp = timestamp;
            _level = level;
            _threadId = threadId;
            _source = source;
            _message = message;
            _tenantId = tenantId;
            _userName = userName;
            _logFilePath = logFilePath;
            _tenantName = tenantName;
        }

		/// <summary>
		///     Initializes a new instance of the EventLogEntry class.
		/// </summary>
		public EventLogEntry( Guid id, DateTime date, long timestamp, EventLogLevel level, string machine, string process, int threadId, string source, string message, long tenantId, string tenantName, string userName, string logFilePath = null)
		{
			_id = id;
			_date = date;
			_timestamp = timestamp;
			_level = level;
			_machine = machine;
			_process = process;
			_threadId = threadId;
			_source = source;
			_message = message;
		    _tenantId = tenantId;
		    _userName = userName;
		    _logFilePath = logFilePath;
		    _tenantName = tenantName;
		}

		/// <summary>
		///     Gets the log entry's date.
		/// </summary>
		public DateTime Date
		{
			get
			{
				return _date;
			}
		}

		/// <summary>
		///     Gets the log entry's Id.
		/// </summary>
		public Guid Id
		{
			get
			{
				return _id;
			}
            set
			{
				_id = value;
			}
		}

		/// <summary>
		///     Gets the log entry's level
		/// </summary>
		public EventLogLevel Level
		{
			get
			{
				return _level;
			}
		}

		/// <summary>
		///     Gets the machine associated with the log entry.
		/// </summary>
		public string Machine
		{
			get
			{
				return _machine;
			}
            set
			{
				_machine = value;
			}
		}

		/// <summary>
		///     Gets the log entry's message.
		/// </summary>
		public string Message
		{
			get
			{
				return _message;
			}
		}

		/// <summary>
		///     Gets the process associated with the log entry.
		/// </summary>
		public string Process
		{
			get
			{
				return _process;
			}
            set
			{
				_process = value;
			}
		}

		/// <summary>
		///     Gets the source of the log entry.
		/// </summary>
		public string Source
		{
			get
			{
				return _source;
			}
		}

		/// <summary>
		///     Gets the thread id associated with the log entry.
		/// </summary>
		public int ThreadId
		{
			get
			{
				return _threadId;
			}
		}

		/// <summary>
		///     Gets the log entry's timestamp.
		///     This is used to order log entries correctly.
		/// </summary>
		/// <remarks>This value is the result of the Stopwatch.GetTimestamp method.</remarks>
		public long Timestamp
		{
			get
			{
				return _timestamp;
			}
		}       

        /// <summary>
        /// Gets the tenant id.
        /// </summary>
        /// <value>
        /// The tenant id.
        /// </value>
        public long TenantId
        {
            get
            {
                return _tenantId;
            }
        }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName
        {
            get
            {
                return _userName;
            }
        }

        /// <summary>
        /// Gets the log file path.
        /// </summary>
        /// <value>
        /// The log file path.
        /// </value>
        public string LogFilePath
        {
            get
            {
                return _logFilePath;
            }
        }

        /// <summary>
        /// Gets the name of the tenant.
        /// </summary>
        /// <value>
        /// The name of the tenant.
        /// </value>
        public string TenantName
        {
            get
            {
                return _tenantName;
            }
        }

        /// <summary>
        /// Private a human readable representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}: {1} {2} '{3}'", Date, Level, Source, string.Join("", Message.Take(70)));
        }
	}
}