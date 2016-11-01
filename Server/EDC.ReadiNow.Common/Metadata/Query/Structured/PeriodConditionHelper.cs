// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    public static class PeriodConditionHelper
    {
        /// <summary>
        /// Returns zero-based index of quarters starting from 01/01/0001.
        /// </summary>
        /// <param name="date">A date to specify quarter.</param>
        /// <returns>Zero-based index of quarters since 01/01/0001 until the quarter containing the specified date.</returns>
        public static int GetQuarterIndexSinceBc(DateTime date)
        {
            var quartersInFullYears = (date.Year - DateTime.MinValue.Year) * 4;
            var quartersWithinFinalYear = (date.Month - 1) / 3; //Jan~Mar: 0, Apr~Jun: 1, Jul~Sep: 2, and Oct~Dec: 3
            return quartersInFullYears + quartersWithinFinalYear;
        }

        /// <summary>
        /// Returns zero-based month index of the first month of the specified quarter.
        /// </summary>
        /// <param name="quarterIndex">Zero-based quarter index.</param>
        /// <returns></returns>
        public static int GetFirstMonthOfQuarter(int quarterIndex)
        {
            var quarterIndexWithoutYearComponent = quarterIndex % 4;
            return quarterIndexWithoutYearComponent * 3 + 1;
        }

        /// <summary>
        /// Extracts year component from quarter index.
        /// </summary>
        /// <param name="quarterIndex">Zero-based quarter index.</param>
        /// <returns>Human friendly year numnber.</returns>
        public static int GetYearFromQuarter(int quarterIndex)
        {
            return quarterIndex / 4 + 1;
        }

        /// <summary>
        /// Returns first date of the specified quarter.
        /// </summary>
        /// <param name="quarterIndex">Zero-based index of quarter.</param>
        /// <returns>Date marking beginning of the specified quarter.</returns>
        public static DateTime GetFirstDateOfQuarter(int quarterIndex)
        {
            var year = GetYearFromQuarter(quarterIndex);
            var firstMonth = GetFirstMonthOfQuarter(quarterIndex);
            return new DateTime(year, firstMonth, 1);
        }

        /// <summary>
        /// Returns first date of the specified quarter range and first date of quarter following the specified quarter range; so that it can be easily used in date range comparisons.
        /// </summary>
        /// <param name="startQuarterIndex">Zero-based quarter index of the beginning of quarter range.</param>
        /// <param name="endQuarterIndex">Zero-based quarter index of the end of quarter range.</param>
        /// <param name="startDate">First date of the specified quarter.</param>
        /// <param name="startDateOfNextQuarter">First date of following quarter.</param>
        public static void GetStartAndEndDateOfQuarterRange(int startQuarterIndex, int endQuarterIndex, out DateTime startDate, out DateTime startDateOfNextQuarter)
        {
            startDate = GetFirstDateOfQuarter(startQuarterIndex);
            startDateOfNextQuarter = GetFirstDateOfQuarter(endQuarterIndex + 1);
        }

        /// <summary>
        /// Sets the start and end dates of next N quarters.
        /// </summary>
        /// <param name="numberOfQuarters">The number of quarters.</param>
        /// <param name="incRemainderOfCurrentQuarter">if set to <c>true</c> [include current quarter].</param>
        /// <param name="localTodayDate">The local today date.</param>
        /// <param name="quarterStartDate">The quarter start date.</param>
        /// <param name="quarterEndDate">The quarter end date.</param>
        public static void SetStartAndEndDatesOfNextNQuarters(int numberOfQuarters, bool incRemainderOfCurrentQuarter, DateTime localTodayDate, out DateTime quarterStartDate, out DateTime quarterEndDate)
        {
            // Q1 = Jan-Mar, Q2 = Apr-Jun, Q3 = Jul-Sep, Q4 = Oct-Dec 
            //var today = DateTime.Today;
            int currQuarter = (localTodayDate.Month - 1) / 3 + 1;

            var currentQuarterStartDate = new DateTime(localTodayDate.Year, 3 * currQuarter - 2, 1);

            quarterStartDate = incRemainderOfCurrentQuarter
                                 ? localTodayDate
                                 : currentQuarterStartDate.AddMonths(3);

            quarterEndDate = currentQuarterStartDate.AddMonths((3 * numberOfQuarters) + 3);
        }

        /// <summary>
        /// Gets the financial year start month.
        /// </summary>
        /// <exception cref="System.ArgumentException">FinYearStartMonth</exception>
        public static int GetFinancialYearStartMonth(string monthAlias)
        {
            switch (monthAlias)
            {
                case "moyJanuary":
                    return 1;

                case "moyFebruary":
                    return 2;

                case "moyMarch":
                    return 3;

                case "moyApril":
                    return 4;

                case "moyMay":
                    return 5;

                case "moyJune":
                    return 6;

                case "moyJuly":
                    return 7;

                case "moyAugust":
                    return 8;

                case "moySeptember":
                    return 9;

                case "moyOctober":
                    return 10;

                case "moyNovember":
                    return 11;

                case "moyDecember":
                    return 12;

                default:
                    throw new ArgumentException("FinYearStartMonth");
            }
        }

        static public void GetPeriodFromConditionType(ConditionType conditionType, DateTime today, int? argument, int financialYearStartMonth, bool isDate, out DateTime minDate, out DateTime maxDate)
        {
            minDate = maxDate = DateTime.MinValue;

            var startDateOfCurrentYear = new DateTime(today.Year, 1, 1);
            var startDateOfCurrentFinancialYear = GetCurrentFinancialYearStartDate(today, financialYearStartMonth);
            var startDateOfCurrentMonth = new DateTime(today.Year, today.Month, 1);

            switch (conditionType)
            {
                case ConditionType.Today:
                    minDate = today;
                    maxDate = minDate.AddDays(1);
                    break;

                case ConditionType.ThisWeek:
                    minDate = GetFirstDayOfWeek(today);
                    maxDate = minDate.AddDays(7);
                    break;

                case ConditionType.ThisMonth:
                    minDate = new DateTime(today.Year, today.Month, 1);
                    maxDate = minDate.AddMonths(1);
                    break;

                case ConditionType.ThisQuarter:
                    var thisQuarterIndex = GetQuarterIndexSinceBc(today);
                    GetStartAndEndDateOfQuarterRange(thisQuarterIndex, thisQuarterIndex, out minDate,
                        out maxDate);
                    break;

                case ConditionType.ThisYear:
                    minDate = new DateTime(today.Year, 1, 1);
                    maxDate = minDate.AddYears(1);
                    break;

                case ConditionType.CurrentFinancialYear:
                    minDate = startDateOfCurrentFinancialYear;
                    maxDate = minDate.AddYears(1);
                    break;

                case ConditionType.LastNDays:
                    if (argument != null)
                    {
                        minDate = today.AddDays(0 - argument.Value);
                        maxDate = today;
                    }
                    break;

                case ConditionType.LastNDaysTillNow:
                    if (argument != null)
                    {
                        minDate = today.AddDays(0 - argument.Value);
                        maxDate = isDate ? DateTime.Today.AddDays(1).AddSeconds(-1) : DateTime.Now;
                    }
                    break;

                case ConditionType.NextNDays:
                    if (argument != null)
                    {
                        minDate = today.AddDays(1);
                        maxDate = today.AddDays(1 + argument.Value);
                    }
                    break;

                case ConditionType.NextNDaysFromNow:
                    if (argument != null)
                    {
                        minDate = isDate ? DateTime.Today: DateTime.Now;
                        maxDate = today.AddDays(1 + argument.Value);
                    }
                    break;

                case ConditionType.LastNWeeks:
                    if (argument != null)
                    {
                        maxDate = GetFirstDayOfWeek(today);
                        minDate = maxDate.AddDays(-(7*argument.Value));
                    }
                    break;

                case ConditionType.NextNWeeks:
                    if (argument != null)
                    {
                        minDate = GetFirstDayOfWeek(today).AddDays(7);
                        maxDate = minDate.AddDays(7*argument.Value);
                    }
                    break;

                case ConditionType.LastNMonths:
                    if (argument != null)
                    {
                        minDate = startDateOfCurrentMonth.AddMonths(0 - argument.Value);
                        maxDate = startDateOfCurrentMonth;
                    }
                    break;

                case ConditionType.NextNMonths:
                    if (argument != null)
                    {
                        minDate = startDateOfCurrentMonth.AddMonths(1);
                        maxDate = startDateOfCurrentMonth.AddMonths(argument.Value + 1);
                    }
                    break;

                case ConditionType.LastNQuarters:
                    if (argument != null)
                    {
                        thisQuarterIndex = GetQuarterIndexSinceBc(today);
                        var startQuarterForLastNQuarters = thisQuarterIndex - argument.Value;
                        var endQuarterForLastNQuarters = thisQuarterIndex - 1; //until end of last quarter
                        GetStartAndEndDateOfQuarterRange(startQuarterForLastNQuarters, endQuarterForLastNQuarters, out minDate, out maxDate);
                    }
                    break;

                case ConditionType.NextNQuarters:
                    if (argument != null)
                    {

                        thisQuarterIndex = GetQuarterIndexSinceBc(today);
                        var startQuarterForNextNQuarters = thisQuarterIndex + 1; //from next quarter
                        var endQuarterForNextNQuarters = thisQuarterIndex + argument.Value;
                        GetStartAndEndDateOfQuarterRange(startQuarterForNextNQuarters, endQuarterForNextNQuarters, out minDate, out maxDate);
                    }
                    break;

                case ConditionType.LastNYears:
                    if (argument != null)
                    {
                        minDate = startDateOfCurrentYear.AddYears(-argument.Value);
                        maxDate = startDateOfCurrentYear;
                    }
                    break;

                case ConditionType.NextNYears:
                    if (argument != null)
                    {
                        minDate = startDateOfCurrentYear.AddYears(1);
                        maxDate = startDateOfCurrentYear.AddYears(argument.Value + 1);
                    }
                    break;

                case ConditionType.LastNFinancialYears:
                    if (argument != null)
                    {
                        minDate = startDateOfCurrentFinancialYear.AddYears(-argument.Value);
                        maxDate = startDateOfCurrentFinancialYear;
                    }
                    break;

                case ConditionType.NextNFinancialYears:
                    if (argument != null)
                    {
                        minDate = startDateOfCurrentFinancialYear.AddYears(1);
                        maxDate = startDateOfCurrentFinancialYear.AddYears(argument.Value + 1);
                    }
                    break;

                default:
                    throw new ArgumentException("Unknown condition type supplied.");
            }
        }

        /// <summary>
        /// Gets the current financial year start date.
        /// </summary>
        /// <returns></returns>
        public static DateTime GetCurrentFinancialYearStartDate(DateTime localTodayDate, int financialYearStartMonth)
        {
            return localTodayDate.Month < financialYearStartMonth
                                         ? new DateTime(localTodayDate.Year - 1, financialYearStartMonth, 1)
                                         : new DateTime(localTodayDate.Year, financialYearStartMonth, 1);
        }

        /// <summary>
        /// Returns date of Monday of the same week that specified date belongs to.
        /// </summary>
        //TODO: It is assumed that first day of week is Monday however there is backlog items to improve upon it.
        public static DateTime GetFirstDayOfWeek(DateTime date)
        {
            int offset;
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Tuesday:
                    offset = 1;
                    break;

                case DayOfWeek.Wednesday:
                    offset = 2;
                    break;

                case DayOfWeek.Thursday:
                    offset = 3;
                    break;

                case DayOfWeek.Friday:
                    offset = 4;
                    break;

                case DayOfWeek.Saturday:
                    offset = 5;
                    break;

                case DayOfWeek.Sunday:
                    offset = 6;
                    break;

                default:
                    offset = 0;
                    break;
            }

            return date.AddDays(-offset);
        }
    }
}
