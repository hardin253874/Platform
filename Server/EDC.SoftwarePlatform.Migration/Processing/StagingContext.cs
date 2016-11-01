// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.SoftwarePlatform.Migration.Contract;
using EDC.SoftwarePlatform.Migration.Contract.Statistics;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Staging context
	/// </summary>
	public class StagingContext : IProcessingContext
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="StagingContext" /> class.
		/// </summary>
		public StagingContext( )
		{
			Report = new StatisticsReport( );
		}

		/// <summary>
		///     Gets the report.
		/// </summary>
		/// <value>
		///     The report.
		/// </value>
		public StatisticsReport Report
		{
			get;
		}

		/// <summary>
		///     Writes the error.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void WriteError( string message )
		{
		}

		/// <summary>
		///     Writes the info.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void WriteInfo( string message )
		{
		}

		/// <summary>
		///     Writes the progress.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void WriteProgress( string message )
		{
		}

		/// <summary>
		///     Writes the warning.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <exception cref="System.NotImplementedException"></exception>
		public void WriteWarning( string message )
		{
		}
	}
}