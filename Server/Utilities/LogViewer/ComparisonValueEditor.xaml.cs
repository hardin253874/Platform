// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for ComparisonValueEditor.xaml
    /// </summary>
    public partial class ComparisonValueEditor : UserControl
    {
        public ComparisonValueEditor()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Type of the data being represented.
        /// </summary>
        public static readonly DependencyProperty DataTypeProperty =
            DependencyProperty.Register("DataType", typeof(Type), typeof(ComparisonValueEditor), new PropertyMetadata(null, new PropertyChangedCallback(OnDataTypeChanged)));


        /// <summary>
        /// True if a range comparison is being done, false otherwise
        /// </summary>
        public static readonly DependencyProperty IsRangeComparisonProperty =
            DependencyProperty.Register("IsRangeComparison", typeof(bool), typeof(ComparisonValueEditor), new PropertyMetadata(false, new PropertyChangedCallback(OnIsRangeComparisonChanged)));


        /// <summary>
        /// Value of the data being represented, in its native .Net type.
        /// </summary>
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(object), typeof(ComparisonValueEditor), null);


        /// <summary>
        /// Value of the data being represented, in its native .Net type.
        /// </summary>
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(object), typeof(ComparisonValueEditor), null);


        /// <summary>
        /// Gets or sets the data type.
        /// </summary>
        public Type DataType
        {
            get
            {
                return (Type)GetValue(DataTypeProperty);
            }
            set
            {
                SetValue(DataTypeProperty, value);
            }
        }


        /// <summary>
        /// Gets or sets the data value.
        /// </summary>
        public object MinValue
        {
            get
            {
                return GetValue(MinValueProperty);
            }
            set
            {
                SetValue(MinValueProperty, value);
            }
        }


        /// <summary>
        /// Gets or sets the data value.
        /// </summary>
        public object MaxValue
        {
            get
            {
                return GetValue(MaxValueProperty);
            }
            set
            {
                SetValue(MaxValueProperty, value);
            }
        }


        /// <summary>
        /// Gets or sets the range comparison value
        /// </summary>
        public bool IsRangeComparison
        {
            get
            {
                return (bool)GetValue(IsRangeComparisonProperty);
            }
            set
            {
                SetValue(IsRangeComparisonProperty, value);
            }
        }


        /// <summary>
        /// Fired when DataType is changed.
        /// </summary>
        private static void OnDataTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ComparisonValueEditor control = (ComparisonValueEditor)d;
            Type newValue = null;
            if (e.NewValue is Type)
                newValue = (Type)e.NewValue;

            string templateName = newValue.FullName;

            if (control.IsRangeComparison)
            {
                templateName += "-IsRange";
            }

            // Get the data template
            var template = control.Resources[templateName];

            control.content.ContentTemplate = template as DataTemplate;
        }


        /// <summary>
        /// Fired when IsRangeComparison is changed.
        /// </summary>
        private static void OnIsRangeComparisonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ComparisonValueEditor control = (ComparisonValueEditor)d;
            bool newValue = false;
            if (e.NewValue is bool)
                newValue = (bool)e.NewValue;

            string templateName = control.DataType.FullName;            

            if (newValue)
            {
                templateName += "-IsRange";
            }

            // Get the data template
            var template = control.Resources[templateName];

            control.content.ContentTemplate = template as DataTemplate;
        }
    }
}
