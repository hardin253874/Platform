// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.SoftwarePlatform.Migration.Processing
{
    /// <summary>
    /// Untyped interface for missing dependencies
    /// </summary>
    public interface IMissingDependency
    {
        /// <summary>
        ///     Gets the log message.
        /// </summary>
        string GetLogMessage( );
    }


	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class MissingDependency<T> : IMissingDependency
    {
		/// <summary>
		///     Initializes a new instance of the <see cref="MissingDependency{T}" /> class.
		/// </summary>
		/// <param name="entry">The entry.</param>
		/// <param name="result">The result.</param>
		public MissingDependency( T entry, PopulateRowResult result, Func<T, PopulateRowResult, string> generateLogMessage )
		{
			Entry = entry;
			Result = result;
			GenerateLogMessage = generateLogMessage;
		}

		/// <summary>
		///     Gets or sets the entry.
		/// </summary>
		/// <value>
		///     The entry.
		/// </value>
		public T Entry
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the generate log message.
		/// </summary>
		/// <value>
		///     The generate log message.
		/// </value>
		public Func<T, PopulateRowResult, string> GenerateLogMessage
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the result.
		/// </summary>
		/// <value>
		///     The result.
		/// </value>
		public PopulateRowResult Result
		{
			get;
			private set;
		}

        /// <summary>
        /// Generates the log message.
        /// </summary>
	    public string GetLogMessage( )
	    {
	        return GenerateLogMessage( Entry, Result );
	    }
	}
}