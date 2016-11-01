// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogViewer.Common;
using System.Windows.Input;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Windows;

namespace LogViewer.ViewModels
{       
    internal class FiltersWindowViewModel : ObservableObject
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="FiltersWindowViewModel"/> class.
        /// </summary>
        public FiltersWindowViewModel()
        {
            columnFilterDefinitions.Add(new ColumnFilterDefinition { ColumnName = "Date", ColumnType = typeof(DateTime), ApplicableComparisonOperators = GetApplicableOperators(typeof(DateTime)) });
            columnFilterDefinitions.Add(new ColumnFilterDefinition { ColumnName = "Message", ColumnType = typeof(string), ApplicableComparisonOperators = GetApplicableOperators(typeof(string)) });
            columnFilterDefinitions.Add(new ColumnFilterDefinition { ColumnName = "Process", ColumnType = typeof(string), ApplicableComparisonOperators = GetApplicableOperators(typeof(string)) });
            columnFilterDefinitions.Add(new ColumnFilterDefinition { ColumnName = "Source", ColumnType = typeof(string), ApplicableComparisonOperators = GetApplicableOperators(typeof(string)) });
            columnFilterDefinitions.Add(new ColumnFilterDefinition { ColumnName = "ThreadId", ColumnType = typeof(Int32), ApplicableComparisonOperators = GetApplicableOperators(typeof(int)) });
            columnFilterDefinitions.Add(new ColumnFilterDefinition { ColumnName = "Machine", ColumnType = typeof(string), ApplicableComparisonOperators = GetApplicableOperators(typeof(string)) });
            columnFilterDefinitions.Add(new ColumnFilterDefinition { ColumnName = "TenantName", ColumnType = typeof(string), ApplicableComparisonOperators = GetApplicableOperators(typeof(string)) });
            columnFilterDefinitions.Add(new ColumnFilterDefinition { ColumnName = "UserName", ColumnType = typeof(string), ApplicableComparisonOperators = GetApplicableOperators(typeof(string)) });

            SelectedColumnFilterDefinition = columnFilterDefinitions.First();

            filterActions.Add(FilterAction.Include);
            filterActions.Add(FilterAction.Exclude);

            AddFilterCommand = new RelayCommand(ExecuteAddFilterCommand, CanExecuteAddFilterCommand);
            RemoveFilterCommand = new RelayCommand(ExecuteRemoveFilterCommand, CanExecuteRemoveFilterCommand);
            ClearFiltersCommand = new RelayCommand(ExecuteClearFiltersCommand, CanExecuteClearFiltersCommand);

            OKButtonCommand = new RelayCommand<Window>(ExecuteOKButtonCommand);
            CancelButtonCommand = new RelayCommand<Window>(ExecuteCancelButtonCommand);
            ApplyButtonCommand = new RelayCommand(ExecuteApplyButtonCommand);

            columnFilters.Source = columnFiltersList;
            columnFilters.View.MoveCurrentTo(null);
        }
        #endregion


        #region Properties
        /// <summary>
        /// Gets or sets the min filter value.
        /// </summary>
        /// <value>
        /// The min filter value.
        /// </value>
        public object MinFilterValue
        {
            get
            {
                return minFilterValue;
            }
            set
            {
                if (minFilterValue != value)
                {
                    minFilterValue = value;
                    RaisePropertyChanged(() => MinFilterValue);
                }
            }
        }
        private object minFilterValue;


        /// <summary>
        /// Gets or sets the max filter value.
        /// </summary>
        /// <value>
        /// The max filter value.
        /// </value>
        public object MaxFilterValue
        {
            get
            {
                return maxFilterValue;
            }
            set
            {
                if (maxFilterValue != value)
                {
                    maxFilterValue = value;
                    RaisePropertyChanged(() => MaxFilterValue);
                }
            }
        }
        private object maxFilterValue;


        /// <summary>
        /// Gets the column filters.
        /// </summary>
        public CollectionViewSource ColumnFilters
        {
            get
            {                
                return columnFilters;
            }
        }
        private CollectionViewSource columnFilters = new CollectionViewSource();


        /// <summary>
        /// Gets or sets the OK button command.
        /// </summary>
        /// <value>
        /// The OK button command.
        /// </value>
        public ICommand OKButtonCommand { get; set; }


        /// <summary>
        /// Gets or sets the cancel button command.
        /// </summary>
        /// <value>
        /// The cancel button command.
        /// </value>
        public ICommand CancelButtonCommand { get; set; }


        /// <summary>
        /// Gets or sets the apply button command.
        /// </summary>
        /// <value>
        /// The apply button command.
        /// </value>
        public ICommand ApplyButtonCommand { get; set; }


        /// <summary>
        /// Gets or sets the add filter command.
        /// </summary>
        /// <value>
        /// The add filter command.
        /// </value>
        public ICommand AddFilterCommand { get; set; }


        /// <summary>
        /// Gets or sets the remove filter command.
        /// </summary>
        /// <value>
        /// The remove filter command.
        /// </value>
        public ICommand RemoveFilterCommand { get; set; }


        /// <summary>
        /// Gets or sets the clear filters command.
        /// </summary>
        /// <value>
        /// The clear filters command.
        /// </value>
        public ICommand ClearFiltersCommand { get; set; }


        /// <summary>
        /// Gets or sets the selected operator.
        /// </summary>
        /// <value>
        /// The selected operator.
        /// </value>
        public ComparisonOperator SelectedOperator
        {
            get
            {
                return selectedOperator;
            }
            set
            {
                if (selectedOperator != value)
                {
                    selectedOperator = value;
                    RaisePropertyChanged(() => SelectedOperator);

                    IsRangeComparison = selectedOperator == ComparisonOperator.Between;                    
                }
            }
        }
        private ComparisonOperator selectedOperator;


        /// <summary>
        /// Gets or sets the selected column filter definition.
        /// </summary>
        /// <value>
        /// The selected column filter definition.
        /// </value>
        public ColumnFilterDefinition SelectedColumnFilterDefinition
        {
            get
            {
                return selectedColumnFilterDefinition;
            }
            set
            {
                if (selectedColumnFilterDefinition != value)
                {
                    selectedColumnFilterDefinition = value;
                    RaisePropertyChanged(() => SelectedColumnFilterDefinition);
                    SelectedOperator = selectedColumnFilterDefinition.ApplicableComparisonOperators.First();
                    MinFilterValue = null;
                    MaxFilterValue = null;
                }
            }
        }
        private ColumnFilterDefinition selectedColumnFilterDefinition;


        /// <summary>
        /// Gets the column filter definitions.
        /// </summary>
        public List<ColumnFilterDefinition> ColumnFilterDefinitions 
        {
            get
            {
                return columnFilterDefinitions;
            }
        }
        private List<ColumnFilterDefinition> columnFilterDefinitions = new List<ColumnFilterDefinition>();


        /// <summary>
        /// Gets the filter actions.
        /// </summary>
        public List<FilterAction> FilterActions 
        {
            get
            {
                return filterActions;
            }
        }
        private List<FilterAction> filterActions = new List<FilterAction>();


        /// <summary>
        /// Gets or sets the selected action.
        /// </summary>
        /// <value>
        /// The selected action.
        /// </value>
        public FilterAction SelectedAction 
        {
            get
            {
                return selectedAction;
            }
            set
            {
                if (selectedAction != value)
                {
                    selectedAction = value;
                    RaisePropertyChanged(() => SelectedAction);
                }
            }
        }
        private FilterAction selectedAction;


        /// <summary>
        /// Gets or sets a value indicating whether this instance is range comparison.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is range comparison; otherwise, <c>false</c>.
        /// </value>
        public bool IsRangeComparison 
        {
            get
            {
                return isRangeComparison;
            }
            set
            {
                if (isRangeComparison != value)
                {
                    isRangeComparison = value;
                    RaisePropertyChanged(() => IsRangeComparison);
                }
            }
        }
        private bool isRangeComparison;
        #endregion


        #region Public Methods
        /// <summary>
        /// Sets the current filters.
        /// </summary>
        /// <param name="columnFilters">The column filters.</param>
        public void SetCurrentFilters(IEnumerable<ColumnFilter> columnFilters)
        {
            foreach (ColumnFilter cf in columnFilters)
            {
                columnFiltersList.Add(cf);
            }
        }        
        #endregion


        /// <summary>
        /// Occurs when the column filters are changed.
        /// </summary>
        public event EventHandler<ColumnFiltersArgs> ColumnFiltersChanged;       


        #region Non-Public Methods
        /// <summary>
        /// Called when the column filters are changed.
        /// </summary>
        /// <param name="e">The e.</param>
        private void OnColumnFiltersChanged(ColumnFiltersArgs e)
        {
            if (ColumnFiltersChanged != null)
            {
                ColumnFiltersChanged(this, e);
            }
        }


        /// <summary>
        /// Executes the OK button command.
        /// </summary>
        /// <param name="currentWindow">The current window.</param>
        private void ExecuteOKButtonCommand(Window currentWindow)
        {
            ColumnFiltersArgs filterArgs = new ColumnFiltersArgs(columnFiltersList);
            OnColumnFiltersChanged(filterArgs);

            currentWindow.Close();
        }


        /// <summary>
        /// Executes the cancel button command.
        /// </summary>
        /// <param name="currentWindow">The current window.</param>
        private void ExecuteCancelButtonCommand(Window currentWindow)
        {
            currentWindow.Close();
        }


        /// <summary>
        /// Executes the apply button command.
        /// </summary>
        private void ExecuteApplyButtonCommand()
        {
            ColumnFiltersArgs filterArgs = new ColumnFiltersArgs(columnFiltersList);
            OnColumnFiltersChanged(filterArgs);
        }


        /// <summary>
        /// Executes the add filter command.
        /// </summary>
        private void ExecuteAddFilterCommand()
        {
            if (SelectedColumnFilterDefinition != null &&
                SelectedOperator != ComparisonOperator.None &&
                ((SelectedOperator != ComparisonOperator.Between && MinFilterValue != null) ||
                 (SelectedOperator == ComparisonOperator.Between && MinFilterValue != null && MaxFilterValue != null)))
            {
                ColumnFilter filter = new ColumnFilter 
                { 
                    IsEnabled = true, 
                    ColumnName = SelectedColumnFilterDefinition.ColumnName, 
                    Operator = SelectedOperator, 
                    MinValue = MinFilterValue,
                    MaxValue = MaxFilterValue, 
                    Action = SelectedAction 
                };
                columnFiltersList.Add(filter);
                columnFilters.View.Refresh();
            }
        }


        /// <summary>
        /// Determines whether this instance can execute the  add filter command.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can execute the  add filter command; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExecuteAddFilterCommand()
        {
            if (SelectedColumnFilterDefinition != null &&
                SelectedOperator != ComparisonOperator.None &&
                ((SelectedOperator != ComparisonOperator.Between && MinFilterValue != null) ||
                 (SelectedOperator == ComparisonOperator.Between && MinFilterValue != null && MaxFilterValue != null)))
            {
                return true;
            }
            else
            {
                return false;
            }            
        }


        /// <summary>
        /// Executes the remove filter command.
        /// </summary>
        private void ExecuteRemoveFilterCommand()
        {
            if (columnFilters.View.CurrentItem != null)
            {
                columnFiltersList.Remove(columnFilters.View.CurrentItem as ColumnFilter);
                columnFilters.View.Refresh();
            }
        }


        /// <summary>
        /// Determines whether this instance can execute the remove filter command.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can execute the remove filter command; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExecuteRemoveFilterCommand()
        {
            return (columnFilters.View.CurrentItem != null);
        }


        /// <summary>
        /// Executes the clear filters command.
        /// </summary>
        private void ExecuteClearFiltersCommand()
        {
            columnFiltersList.Clear();
            columnFilters.View.Refresh();
        }


        /// <summary>
        /// Determines whether this instance can execute the clear filters command.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance can execute clear filters command; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExecuteClearFiltersCommand()
        {
            return true;
        }


        /// <summary>
        /// Gets the applicable operators.
        /// </summary>
        /// <param name="columnType">Type of the column.</param>
        /// <returns></returns>
        private List<ComparisonOperator> GetApplicableOperators(Type columnType)
        {
            List<ComparisonOperator> operators = null;

            if (typeof(DateTime) == columnType)
            {
                operators = new List<ComparisonOperator>{ ComparisonOperator.Between, ComparisonOperator.GreaterThan, ComparisonOperator.GreaterThanOrEqualTo, ComparisonOperator.LessThan, ComparisonOperator.LessThanForEqualTo, ComparisonOperator.Equals, ComparisonOperator.NotEquals };
            }
            else if (typeof(string) == columnType)
            {
                operators = new List<ComparisonOperator>{ ComparisonOperator.BeginsWith, ComparisonOperator.EndsWith, ComparisonOperator.Contains, ComparisonOperator.Equals, ComparisonOperator.NotEquals };
            }
            else if (typeof(int) == columnType)
            {
                operators = new List<ComparisonOperator> { ComparisonOperator.Between, ComparisonOperator.GreaterThan, ComparisonOperator.GreaterThanOrEqualTo, ComparisonOperator.LessThan, ComparisonOperator.LessThanForEqualTo, ComparisonOperator.Equals, ComparisonOperator.NotEquals };
            }
            else
            {
                operators = new List<ComparisonOperator> { ComparisonOperator.Between, ComparisonOperator.GreaterThan, ComparisonOperator.GreaterThanOrEqualTo, ComparisonOperator.LessThan, ComparisonOperator.LessThanForEqualTo, ComparisonOperator.Equals, ComparisonOperator.NotEquals };
            }

            return operators;
        }
        #endregion


        #region Fields
        /// <summary>
        /// 
        /// </summary>
        private ObservableCollection<ColumnFilter> columnFiltersList = new ObservableCollection<ColumnFilter>();
        #endregion
    }            
}
