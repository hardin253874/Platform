// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Specialized;

namespace EDC.Core
{
	/// <summary>
	///     Summary description for the TaskProgressCallbackArgs class.
	/// </summary>
	[Serializable]
	public class TaskProgressCallbackArgs : CallbackArgs
	{
		private readonly int _current;
		private readonly int _max = 100;
		private string _task = string.Empty;

		/// <summary>
		///     Initializes a new instance of a TaskProgressCallbackArgs.
		/// </summary>
		public TaskProgressCallbackArgs( )
		{
		}

		/// <summary>
		///     Initializes a new instance of a TaskProgressCallbackArgs.
		/// </summary>
		/// <param name="current">
		///     The current amount of work done by the task.
		/// </param>
		/// <param name="max">
		///     The total amount of work required to be done by the task.
		/// </param>
		public TaskProgressCallbackArgs( int current, int max )
		{
			_current = current;
			_max = max;
		}

		/// <summary>
		///     Initializes a new instance of a TaskProgressCallbackArgs.
		/// </summary>
		/// <param name="current">
		///     The current amount of work done by the task.
		/// </param>
		/// <param name="max">
		///     The total amount of work required to be done by the task.
		/// </param>
		/// <param name="task">
		///     A string describing the current task.
		/// </param>
		public TaskProgressCallbackArgs( int current, int max, string task )
		{
			_current = current;
			_max = max;
			_task = task;
		}

		/// <summary>
		///     Initializes a new instance of a TaskProgressCallbackArgs.
		/// </summary>
		/// <param name="current">
		///     The current amount of work done by the task.
		/// </param>
		/// <param name="max">
		///     The total amount of work required to be done by the task.
		/// </param>
		/// <param name="task">
		///     A string describing the current task.
		/// </param>
		/// <param name="parameters">
		///     A dictionary containing any optional additional parameters.
		/// </param>
		public TaskProgressCallbackArgs( int current, int max, string task, StringDictionary parameters )
			: base( parameters )
		{
			_current = current;
			_max = max;
			_task = task;
		}

		/// <summary>
		///     Gets the current amount of work done by the task.
		/// </summary>
		public int Current
		{
			get
			{
				return _current;
			}
		}

		/// <summary>
		///     Gets the total amount of work required to be done by the task.
		/// </summary>
		public int Max
		{
			get
			{
				return _max;
			}
		}

		/// <summary>
		///     Gets or sets the string describing the current task.
		/// </summary>
		public string Task
		{
			get
			{
				return _task;
			}

			set
			{
				_task = value;
			}
		}
	}
}