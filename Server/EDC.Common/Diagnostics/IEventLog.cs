// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.Diagnostics
{
    /// <summary>
    /// Delegate for the <see cref="IEventLog.WriteEvent"/>.
    /// </summary>
    /// <param name="sender">
    /// The object that raised the event.
    /// </param>
    /// <param name="args">
    /// Event-specific arguments, including the <see cref="EventLogEntry"/> raised.
    /// </param>
    public delegate void WriteEventEventHandler(object sender, EventLogWriteEventArgs args);

	/// <summary>
	///     Defines the base methods and properties for the application event log.
	/// </summary>
	public interface IEventLog
	{
		/// <summary>
		///     Gets or sets whether error messages are logged.
		/// </summary>
		bool ErrorEnabled
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets whether informational messages are logged.
		/// </summary>
		bool InformationEnabled
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets whether trace messages are logged.
		/// </summary>
		bool TraceEnabled
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets whether warning messages are logged.
		/// </summary>
		bool WarningEnabled
		{
			get;
			set;
		}

		/// <summary>
		///     Write an error message to the event log.
		/// </summary>
		/// <param name="message">
		///     A string containing zero or more format items.
		/// </param>
		/// <param name="parameters">
		///     An object array containing zero or more objects to format.
		/// </param>
		void WriteError( string message, params object [ ] parameters );

		/// <summary>
		///     Write an informational message to the event log.
		/// </summary>
		/// <param name="message">
		///     A string containing zero or more format items.
		/// </param>
		/// <param name="parameters">
		///     An object array containing zero or more objects to format.
		/// </param>
		void WriteInformation( string message, params object [ ] parameters );

		/// <summary>
		///     Write a trace message to the event log.
		/// </summary>
		/// <param name="message">
		///     A string containing zero or more format items.
		/// </param>
		/// <param name="parameters">
		///     An object array containing zero or more objects to format.
		/// </param>
		void WriteTrace( string message, params object [ ] parameters );

		/// <summary>
		///     Write a warning message to the event log.
		/// </summary>
		/// <param name="message">
		///     A string containing zero or more format items.
		/// </param>
		/// <param name="parameters">
		///     An object array containing zero or more objects to format.
		/// </param>
		void WriteWarning( string message, params object [ ] parameters );

        /// <summary>
        /// Fired when an entry is written to the event log.
        /// </summary>
        event WriteEventEventHandler WriteEvent;
	}
}