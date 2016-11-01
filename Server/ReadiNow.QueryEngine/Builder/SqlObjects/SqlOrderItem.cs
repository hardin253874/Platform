// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.QueryEngine.Builder.SqlObjects
{
	/// <summary>
	///     Represents a single order-by expression.
	///     This is a transient class only; used when generating SQL from a query.
	/// </summary>
	internal class SqlOrderItem
	{
		/// <summary>
		///     Specified ascending or descending sort order.
		/// </summary>
		public OrderByDirection Direction
		{
			get;
			set;
		}

		/// <summary>
		///     The sort expression.
		/// </summary>
		public SqlExpression Expression
		{
		    get { return _expression; }
            set
            { 
                _expression = value;
                string s = _expression.OrderingSql; // force the property to evaluate early, since we know it's going to get used; and we need to eval prior to starting any SQL generation.
            }
		}
        SqlExpression _expression;
	}

}