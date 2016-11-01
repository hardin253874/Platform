// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	/// </summary>
	/// <typeparam name="TEntry">The type of the entry.</typeparam>
	public class LibraryAppTargetExecutionArguments<TEntry> : ExecutionArguments<TEntry>
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="LibraryAppTargetExecutionArguments{TEntry}" /> class.
		/// </summary>
		public LibraryAppTargetExecutionArguments( )
		{
			ClearExistingData = true;
		}

		/// <summary>
		///     Gets or sets a value indicating whether [clear existing data].
		/// </summary>
		/// <value>
		///     <c>true</c> if [clear existing data]; otherwise, <c>false</c>.
		/// </value>
		public bool ClearExistingData
		{
			get;
			set;
		}
	}
}