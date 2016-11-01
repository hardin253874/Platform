// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogViewer.ViewModels
{
    public class DateTimeComparer : IComparer<DateTime>
    {
        private bool reverse;


        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeComparer"/> class.
        /// </summary>
        /// <param name="reverse">if set to <c>true</c> [reverse].</param>
        public DateTimeComparer(bool reverse)
        {
            this.reverse = reverse;
        }


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
        public int Compare(DateTime x, DateTime y)
        {
            int result = -1;

            if (x <= y)
            {
                result = -1;
            }
            else
            {
                result = 1;
            }

            if (reverse)
            {
                result *= -1;
            }

            return result;
        }
    }
}
