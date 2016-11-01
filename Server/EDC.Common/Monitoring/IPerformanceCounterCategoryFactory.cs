// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;
using System.Security;

namespace EDC.Monitoring
{
    public interface IPerformanceCounterCategoryFactory
    {
        /// <summary>
        /// Create a performance counter category containing all the previously
        /// added performance counters. Calling this a second or subsequent time
        /// will delete and recreate the category.
        /// </summary>
        /// <remarks>
        /// If the executing user is not a member of the local 'Administrators' group, 
        /// this will return an empty list of counters.
        /// </remarks>
        /// <param name="name">
        /// The category name. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="help">
        /// The category description or help. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="categoryType">
        /// Whether the instance is single or multi instance. This cannot be Unknown.
        /// </param>
        /// <exception cref="SecurityException">
        /// The calling user must be a member of the local 'Administrators' group to
        /// create a performance counter category.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Neither <paramref cref="name"/> nor <paramref name="help"/> can be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="categoryType"/> cannot be <see cref="PerformanceCounterCategoryType.Unknown"/>.
        /// </exception>
        void CreateCategory(string name, string help, PerformanceCounterCategoryType categoryType);

        /// <summary>
        /// Add an average timer performance counter to the list that will be created.
        /// </summary>
        /// <param name="name">
        /// The name of the performance counter. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="help">
        /// The description or help for the performance counter. This cannot be null, empty
        /// or whitespace.
        /// </param>
        /// <returns>
        /// A <seealso cref="PerformanceCounterCategory"/> used for creating additional
        /// performance counters.
        /// </returns>
        /// <seealso cref="PerformanceCounterCategoryFactory.CreateCategory"/>
        IPerformanceCounterCategoryFactory AddAverageTimer32(string name, string help);

        /// <summary>
        /// Add a 32-bit rate per second performance counter to the list that will be created.
        /// </summary>
        /// <param name="name">
        /// The name of the performance counter. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="help">
        /// The description or help for the performance counter. This cannot be null, empty
        /// or whitespace.
        /// </param>
        /// <returns>
        /// A <seealso cref="PerformanceCounterCategory"/> used for creating additional
        /// performance counters.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null, empty or whitespace.
        /// </exception>
        /// <seealso cref="CreateCategory"/>
        IPerformanceCounterCategoryFactory AddRatePerSecond32(string name, string help);

        /// <summary>
        /// Add a 32-bit sample fraction (i.e. percentage over a time period) to the list that will be created.
        /// </summary>
        /// <param name="name">
        /// The name of the performance counter. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="help">
        /// The description or help for the performance counter. This cannot be null, empty
        /// or whitespace.
        /// </param>
        /// <returns>
        /// A <seealso cref="PerformanceCounterCategory"/> used for creating additional
        /// performance counters.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null, empty or whitespace.
        /// </exception>
        /// <seealso cref="CreateCategory"/>
        IPerformanceCounterCategoryFactory AddPercentageRate(string name, string help);

        /// <summary>
        /// Add a 64-bit counter to the list that will be created.
        /// </summary>
        /// <param name="name">
        /// The name of the performance counter. This cannot be null, empty or whitespace.
        /// </param>
        /// <param name="help">
        /// The description or help for the performance counter. This cannot be null, empty
        /// or whitespace.
        /// </param>
        /// <returns>
        /// A <seealso cref="PerformanceCounterCategory"/> used for creating additional
        /// performance counters.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// No argument can be null, empty or whitespace.
        /// </exception>
        /// <seealso cref="CreateCategory"/>
        IPerformanceCounterCategoryFactory AddNumberOfItems64(string name, string help);
    }
}