// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Data;

namespace ReadiNow.Database
{
    /// <summary>
    /// Generic interface for acting on the database.
    /// </summary>
    public interface IDatabaseContext : IDisposable
    {
        /// <summary>
        /// Create an <see cref="IDbCommand"/>.  (DO NOT attempt to cast it to SqlCommand).
        /// </summary>
        /// <param name="commandText">The select text for the adapter.</param>
        /// <param name="commandType">The type of command.</param>
        /// <returns>An IDbCommand. (But maybe not a SqlCommand).</returns>
        IDbCommand CreateCommand(string commandText = null, CommandType commandType = CommandType.Text);

        /// <summary>
        /// Create an IDbDataAdapter.  (DO NOT attempt to cast it to SqlDataAdapter).
        /// </summary>
        /// <param name="commandText">The select text for the adapter.</param>
        /// <returns>An IDbDataAdapter. (But maybe not a SqlDataAdapter).</returns>
        IDbDataAdapter CreateDataAdapter(string commandText);

        /// <summary>
        /// Use a data adapter to fill a table.
        /// </summary>
        /// <param name="adapter">The data adapter.</param>
        /// <param name="dataTable">The data table.</param>
        void Fill(IDbDataAdapter adapter, DataTable dataTable);
    }
}
