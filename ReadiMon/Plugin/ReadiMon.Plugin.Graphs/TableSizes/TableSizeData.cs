// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiMon.Plugin.Graphs.TableSizes
{
	/// <summary>
	///     TableSizeData class.
	/// </summary>
	public class TableSizeData
	{
		/// <summary>
		///     Gets or sets the amount.
		/// </summary>
		/// <value>
		///     The amount.
		/// </value>
		public double Amount
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the average row bytes.
		/// </summary>
		/// <value>
		///     The average row bytes.
		/// </value>
		public int AvgRowBytes
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the maximum row bytes.
		/// </summary>
		/// <value>
		///     The maximum row bytes.
		/// </value>
		public int MaxRowBytes
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the minimum row bytes.
		/// </summary>
		/// <value>
		///     The minimum row bytes.
		/// </value>
		public int MinRowBytes
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the row count.
		/// </summary>
		/// <value>
		///     The row count.
		/// </value>
		public int RowCount
		{
			get;
			set;
		}
	}
}