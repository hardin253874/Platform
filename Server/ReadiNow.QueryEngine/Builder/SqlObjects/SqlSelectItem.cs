// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace ReadiNow.QueryEngine.Builder.SqlObjects
{
	/// <summary>
	///     An individual selected column.
	///     This is a transient class only; used when generating SQL from a query.
	/// </summary>
	internal class SqlSelectItem
	{
		/// <summary>
		///     The label to be applied to this column in code.
		///     Note: If two columns have the same alias then SQL server will automatically rename one of them.
		/// </summary>
		public string Alias
		{
			get;
			set;
		}

		/// <summary>
		///     The SQL expression to render for this select statement.
		/// </summary>
		public SqlExpression Expression
		{
			get;
			set;
		}
	}
}