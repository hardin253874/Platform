// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogViewer.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    internal class EventLogEntryInfoComparer : IComparer<EventLogEntryInfo>
    {
        #region IComparer
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value
        /// Condition
        /// Less than zero
        /// <paramref name="x"/> is less than <paramref name="y"/>.
        /// Zero
        /// <paramref name="x"/> equals <paramref name="y"/>.
        /// Greater than zero
        /// <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        public int Compare(EventLogEntryInfo x, EventLogEntryInfo y)
        {
            int result = -1;

            if (x != null &&
                y != null)
            {
                if (x == y ||
                    x.Id == y.Id)
                {
                    result = 0;
                }
                else
                {
                    // Sort by date first
                    result = DateTime.Compare(x.Date, y.Date);
                    if (result == 0)
                    {
                        // If both dates are equal, then sort by the timestamp.
                        if (x.Timestamp <= y.Timestamp)
                        {
                            result = -1;
                        }
                        else
                        {
                            result = 1;
                        }
                    }
                }
            }

            return result;
        }
        #endregion
    }
}
