// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.CacheInvalidation;
using EDC.ReadiNow.Metadata.Query.Structured;

namespace ReadiNow.QueryEngine.ReportConverter
{
    /// <summary>
    /// Convert a <see cref="Report"/> to a <see cref="StructuredQuery"/>.
    /// </summary>
    public class ReportToQueryConverter : IReportToQueryConverter
    {
        /// <summary>
        /// Shared instance, for convenience
        /// </summary>
        private static readonly Lazy<ReportToQueryConverter> _instance =
            new Lazy<ReportToQueryConverter>( ( ) => new ReportToQueryConverter( ) );

        /// <summary>
        /// Shared instance, for convenience
        /// </summary>
        public static ReportToQueryConverter Instance => _instance.Value;

        /// <summary>
        /// Convert a <see cref="Report" /> to a <see cref="StructuredQuery" />.
        /// </summary>
        /// <param name="report">The <see cref="Report" /> to convert. This cannot be null.</param>
        /// <returns>
        /// The converted report.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">report</exception>
        public StructuredQuery Convert( Report report )
        {
            return Convert( report, null );
        }

        /// <summary>
        /// Convert a <see cref="Report" /> to a <see cref="StructuredQuery" />.
        /// </summary>
        /// <param name="report">The <see cref="Report" /> to convert. This cannot be null.</param>
        /// <param name="settings">Settings that control the converter behavior.</param>
        /// <returns>
        /// The converted report.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">report</exception>
        public StructuredQuery Convert(Report report, ReportToQueryConverterSettings settings)
        {
            if (report == null)
            {
                throw new ArgumentNullException("report");
            }
            if ( settings == null )
            {
                settings = ReportToQueryConverterSettings.Default;
            }

            StructuredQuery query = StructuredQueryEntityHelper.ConvertReport( report, settings );

            // Tell the cache what entities were referenced to return
            // the StructuredQuery result
            using (CacheContext cacheContext = CacheContext.GetContext())
            {
                cacheContext.Entities.Add(report.Id);

	            if ( query.Conditions != null )
	            {
		            foreach ( var condition in query.Conditions )
		            {
						cacheContext.Entities.Add( condition.EntityId );

			            if ( condition.Expression != null )
			            {
							cacheContext.Entities.Add( condition.Expression.EntityId );
			            }
		            }
	            }

	            if ( query.OrderBy != null )
	            {
		            foreach ( var orderBy in query.OrderBy )
		            {
			            if ( orderBy.Expression != null )
			            {
				            cacheContext.Entities.Add( orderBy.Expression.EntityId );
			            }
		            }
	            }

	            if ( query.SelectColumns != null )
	            {
		            foreach ( var column in query.SelectColumns )
		            {
			            cacheContext.Entities.Add( column.EntityId );

			            if ( column.Expression != null )
			            {
							cacheContext.Entities.Add( column.Expression.EntityId );
			            }
		            }
	            }

	            if ( report.ReportOrderBys != null )
	            {
		            foreach ( var orderBy in report.ReportOrderBys )
		            {
			            if ( orderBy != null )
			            {
							cacheContext.Entities.Add( orderBy.Id );
			            }
		            }
	            }
            }

            StructuredQueryHelper.IdentifyStructureCacheDependencies( query, settings.ConditionsOnly );

            return query;
        }
    }
}
