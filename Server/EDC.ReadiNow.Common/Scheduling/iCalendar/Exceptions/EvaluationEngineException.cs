// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Scheduling.iCalendar
{
	/// <summary>
	///     EvaluationEngineException class.
	/// </summary>
	public class EvaluationEngineException : Exception
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EvaluationEngineException" /> class.
		/// </summary>
		public EvaluationEngineException( )
			: base( "An error occurred during the evaluation process that could not be automatically handled. Check the evaluation mode and restrictions." )
		{
		}
	}
}