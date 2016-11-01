// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogViewer.ViewModels
{
    internal class ColumnFilterDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnFilterDefinition"/> class.
        /// </summary>
        public ColumnFilterDefinition()
        {
            ApplicableComparisonOperators = new List<ComparisonOperator>();
        }


        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        /// <value>
        /// The name of the column.
        /// </value>
        public string ColumnName { get; set; }


        /// <summary>
        /// Gets or sets the applicable comparison operators.
        /// </summary>
        /// <value>
        /// The applicable comparison operators.
        /// </value>
        public List<ComparisonOperator> ApplicableComparisonOperators { get; set; }


        /// <summary>
        /// Gets or sets the type of the column.
        /// </summary>
        /// <value>
        /// The type of the column.
        /// </value>
        public Type ColumnType { get; set; }
    }
}
