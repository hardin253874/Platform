// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using LogViewer.ViewModels;

namespace LogViewer.Common
{
    /// <summary>
    /// Value converter to convert from a comparison operator to a string.
    /// </summary>
    [ValueConversion(typeof(ComparisonOperator), typeof(string))]
    internal class ComparisonOperatorToStringConverter : IValueConverter
    {
        #region Public Methods
        /// <summary>
        /// Convert from a boolean to a visibility value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string convertedValue = string.Empty;

            ComparisonOperator compOperator = (ComparisonOperator)value;
                        
            switch (compOperator)
            {
                case ComparisonOperator.Between:
                    convertedValue = "between";
                    break;
                case ComparisonOperator.BeginsWith:
                    convertedValue = "begins with";
                    break;
                case ComparisonOperator.Contains:
                    convertedValue = "contains";
                    break;
                case ComparisonOperator.EndsWith:
                    convertedValue = "ends with";
                    break;
                case ComparisonOperator.Equals:
                    convertedValue = "equals";
                    break;
                case ComparisonOperator.GreaterThan:
                    convertedValue = "greater than";
                    break;
                case ComparisonOperator.GreaterThanOrEqualTo:
                    convertedValue = "greater than or equal to";
                    break;
                case ComparisonOperator.LessThan:
                    convertedValue = "less than";
                    break;
                case ComparisonOperator.LessThanForEqualTo:
                    convertedValue = "less than or equal to";
                    break;
                case ComparisonOperator.NotEquals:
                    convertedValue = "not equals";
                    break;
                default:
                    convertedValue = "none";
                    break;
            }             

            return convertedValue;
        }


        /// <summary>
        /// Convert from a string value back to a comparison operator.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string name = (string)value;
            
            ComparisonOperator convertedValue = ComparisonOperator.None;
            if (name == "between")
            {
                convertedValue = ComparisonOperator.Between;
            }
            else if (name == "begins with")
            {
                convertedValue = ComparisonOperator.BeginsWith;
            }
            else if (name == "contains")
            {
                convertedValue = ComparisonOperator.Contains;
            }
            else if (name == "ends with")
            {
                convertedValue = ComparisonOperator.EndsWith;
            }
            else if (name == "equals")
            {
                convertedValue = ComparisonOperator.Equals;
            }
            else if (name == "greater than")
            {
                convertedValue = ComparisonOperator.GreaterThan;
            }
            else if (name == "greater than or equal to")
            {
                convertedValue = ComparisonOperator.GreaterThanOrEqualTo;
            }
            else if (name == "less than")
            {
                convertedValue = ComparisonOperator.LessThan;
            }
            else if (name == "less than or equal to")
            {
                convertedValue = ComparisonOperator.LessThanForEqualTo;
            }
            else if (name == "not equals")
            {
                convertedValue = ComparisonOperator.NotEquals;
            }

            return convertedValue;
        }
        #endregion
    }
}
