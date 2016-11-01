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
    [ValueConversion(typeof(string), typeof(FilterAction))]
    internal class ActionToImageConverter : IValueConverter
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

            FilterAction compOperator = (FilterAction)value;
                        
            switch (compOperator)
            {
                case FilterAction.Include:
                    convertedValue = "/Resources/OK.png";
                    break;
                case FilterAction.Exclude:
                    convertedValue = "/Resources/Critical.png";
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

            FilterAction convertedValue = FilterAction.Include;
            if (name == "/Resources/OK.png")
            {
                convertedValue = FilterAction.Include;
            }
            else if (name == "/Resources/Critical.png")
            {
                convertedValue = FilterAction.Exclude;
            }

            return convertedValue;
        }
        #endregion
    }
}
