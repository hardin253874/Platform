// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.SoftwarePlatform.Migration.Contract.Statistics
{
	public class StatisticsCount
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="StatisticsCount" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="count">The count.</param>
		/// <param name="countType">Type of the count.</param>
		public StatisticsCount( string name, int count, StatisticsCountType countType )
		{
			Name = name;
			Count = count;
			CountType = countType;
		}

		/// <summary>
		///     Gets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the count.
		/// </summary>
		/// <value>
		///     The count.
		/// </value>
		public int Count
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets or sets the type of the count.
		/// </summary>
		/// <value>
		///     The type of the count.
		/// </value>
		public StatisticsCountType CountType
		{
			get;
			set;
		}
	}
}