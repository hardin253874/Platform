// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogViewer.ViewModels
{
    internal class ColumnFiltersArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnFiltersArgs"/> class.
        /// </summary>
        /// <param name="columnFilters">The column filters.</param>
        public ColumnFiltersArgs(IEnumerable<ColumnFilter> columnFilters)
        {
            ColumnFilters = new List<ColumnFilter>(columnFilters);
        }


        /// <summary>
        /// Gets or sets the column filters.
        /// </summary>
        /// <value>
        /// The column filters.
        /// </value>
        public IList<ColumnFilter> ColumnFilters { get; set; }
    }
}
