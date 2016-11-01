// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogViewer.Common;

namespace LogViewer.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    internal class ColumnFilter : ObservableObject
    {
        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        /// <value>
        /// The name of the column.
        /// </value>
        public string ColumnName { get; set; }


        /// <summary>
        /// Gets or sets the operator.
        /// </summary>
        /// <value>
        /// The operator.
        /// </value>
        public ComparisonOperator Operator { get; set; }


        /// <summary>
        /// Gets or sets the min value.
        /// </summary>
        /// <value>
        /// The min value.
        /// </value>
        public object MinValue { get; set; }


        /// <summary>
        /// Gets or sets the max value.
        /// </summary>
        /// <value>
        /// The max value.
        /// </value>
        public object MaxValue { get; set; }


        /// <summary>
        /// Gets the value description.
        /// </summary>
        public string ValueDescription
        {
            get
            {
                if (Operator == ComparisonOperator.Between)
                {
                    if (MinValue != null && MaxValue != null)
                    {
                        return string.Format("{0} and {1}", MinValue.ToString(), MaxValue.ToString());
                    }
                    else if (MinValue == null && MaxValue != null)
                    {
                        return string.Format("Min and {0}", MaxValue.ToString());
                    }
                    else if (MinValue != null && MaxValue == null)
                    {
                        return string.Format("{0} and Max", MinValue.ToString());
                    }
                    else
                    {
                        return MinValue.ToString();
                    }
                }
                else
                {
                    return MinValue.ToString();
                }                
            }
        }


        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public FilterAction Action { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    RaisePropertyChanged(() => IsEnabled);
                }
            }
        }
        private bool isEnabled;
    }
}
