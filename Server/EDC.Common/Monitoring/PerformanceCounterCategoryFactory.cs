// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Diagnostics;
using System.Security;

namespace EDC.Monitoring
{
    /// <summary>
    /// Create performance counter categories and the associated performance counters.
    /// </summary>
    public class PerformanceCounterCategoryFactory : IPerformanceCounterCategoryFactory
    {
        private readonly CounterCreationDataCollection counterCreationDataCollection;

        /// <summary>
        /// Create a new <see cref="PerformanceCounterCategoryFactory"/>.
        /// </summary>
        public PerformanceCounterCategoryFactory()
        {
            counterCreationDataCollection = new CounterCreationDataCollection();
        }

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
        public void CreateCategory(string name, string help, PerformanceCounterCategoryType categoryType)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException( nameof( name ) );
            }
            if (string.IsNullOrWhiteSpace(help))
            {
                throw new ArgumentNullException( nameof( help ) );
            }
            if (categoryType == PerformanceCounterCategoryType.Unknown)
            {
                throw new ArgumentException("Must be either single or multi instance", 
                    nameof( categoryType ));
            }

            if (PerformanceCounterCategory.Exists(name))
            {
                PerformanceCounterCategory.Delete(name);
            }

            PerformanceCounterCategory.Create(name, help, categoryType,
                counterCreationDataCollection);
        }

        /// <summary>
        /// Add an average 32-bit timer performance counter to the list that will be created.
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
        public IPerformanceCounterCategoryFactory AddAverageTimer32(string name, string help)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException( nameof( name ) );
            }
            if (string.IsNullOrWhiteSpace(help))
            {
                throw new ArgumentNullException( nameof( help ) );
            }

            // Note: the base must immediately follow the non-base in the list.
            counterCreationDataCollection.AddRange(new []
                {
                    new CounterCreationData(name, help,
                                            PerformanceCounterType.AverageTimer32),
                    new CounterCreationData(name + PerformanceCounterConstants.BaseSuffix, help,
                                            PerformanceCounterType.AverageBase)
                });

            return this;
        }

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
        public IPerformanceCounterCategoryFactory AddRatePerSecond32(string name, string help)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException( nameof( name ) );
            }
            if (string.IsNullOrWhiteSpace(help))
            {
                throw new ArgumentNullException( nameof( help ) );
            }

            counterCreationDataCollection.AddRange(new[]
                {
                    new CounterCreationData(name, help,
                                            PerformanceCounterType.RateOfCountsPerSecond32)
                });

            return this;            
        }

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
        public IPerformanceCounterCategoryFactory AddPercentageRate(string name, string help)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException( nameof( name ) );
            }
            if (string.IsNullOrWhiteSpace(help))
            {
                throw new ArgumentNullException( nameof( help ) );
            }

            counterCreationDataCollection.AddRange(new[]
                {
                    new CounterCreationData(name, help,
                                            PerformanceCounterType.SampleFraction),
                    new CounterCreationData(name + PerformanceCounterConstants.BaseSuffix, help,
                                            PerformanceCounterType.SampleBase)
                });

            return this;
        }

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
        public IPerformanceCounterCategoryFactory AddNumberOfItems64(string name, string help)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException( nameof( name ) );
            }
            if (string.IsNullOrWhiteSpace(help))
            {
                throw new ArgumentNullException( nameof( help ) );
            }

            counterCreationDataCollection.AddRange(new[]
                {
                    new CounterCreationData(name, help,
                                            PerformanceCounterType.NumberOfItems64)
                });

            return this;
        }
    }
}
