// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.Common
{
	/// <summary>
	///     Helper to track whether something has been called for the first time, or subsequent times.
	/// </summary>
	public class First
	{
		private bool _first = true;

		/// <summary>
		///     Returns whether it is currently the first.
		/// </summary>
		public bool Peek
		{
			get
			{
				return _first;
			}
		}

		/// <summary>
		///     Returns true on the first call, then false after that.
		/// </summary>
		public bool Value
		{
			get
			{
				bool result = _first;
				_first = false;
				return result;
			}
			set
			{
				_first = value;
			}
		}

		/// <summary>
		///     Implicit convert to bool.
		/// </summary>
		public static implicit operator bool( First first )
		{
			return first.Value;
		}
	}
}