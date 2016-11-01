// Copyright 2011-2016 Global Software Innovation Pty Ltd

using EDC.SoftwarePlatform.Migration.Contract.Statistics;

namespace EDC.SoftwarePlatform.Migration.Contract
{
	/// <summary>
	///     ProcessingContext interface.
	/// </summary>
	public interface IProcessingContext
	{
		/// <summary>
		///     Gets the report.
		/// </summary>
		/// <value>
		///     The report.
		/// </value>
		StatisticsReport Report
		{
			get;
		}

		/// <summary>
		///     Writes the error.
		/// </summary>
		/// <param name="message">The message.</param>
		void WriteError( string message );

		/// <summary>
		///     Writes the info.
		/// </summary>
		/// <param name="message">The message.</param>
		void WriteInfo( string message );

		/// <summary>
		///     Writes the progress.
		/// </summary>
		/// <param name="message">The message.</param>
		void WriteProgress( string message );

		/// <summary>
		///     Writes the warning.
		/// </summary>
		/// <param name="message">The message.</param>
		void WriteWarning( string message );
	}
}