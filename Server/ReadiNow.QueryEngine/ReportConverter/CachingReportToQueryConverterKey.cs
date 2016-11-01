// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Runtime.Serialization;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;

namespace ReadiNow.QueryEngine.ReportConverter
{
    /// <summary>
    /// Cache key for CachingReportToQueryConverter.
    /// </summary>
    /// <remarks>
    /// It only needs to be comparable with itself.
    /// </remarks>
    [DataContract]
    public class CachingReportToQueryConverterKey : IEquatable<CachingReportToQueryConverterKey>
    {
        private int _hashCode;

        /// <summary>
        /// Constructor
        /// </summary>
        internal CachingReportToQueryConverterKey(Report report, ReportToQueryConverterSettings settings)
        {
            ReportId = report.Id;
            ConditionsOnly = settings.ConditionsOnly;
            _hashCode = GenerateHashCode();
        }

        /// <summary>
        /// Parameterless constructor used by serialization only.
        /// </summary>
        private CachingReportToQueryConverterKey()
        {
            // Do nothing
        }

		/// <summary>
		/// Called after deserialization.
		/// </summary>
		[OnDeserialized]
		private void OnAfterDeserialization( )
		{
			_hashCode = GenerateHashCode( );
		}

        /// <summary>
        /// The report ID.
        /// </summary>
        [DataMember(Order = 1)]
        public long ReportId { get; private set; }

        /// <summary>
        /// True if conditions only, false otherwise.
        /// </summary>
        [DataMember(Order = 2)]
        public bool ConditionsOnly { get; private set; }

        public bool Equals(CachingReportToQueryConverterKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ReportId == other.ReportId && ConditionsOnly.Equals(other.ConditionsOnly);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CachingReportToQueryConverterKey) obj);
        }

        public override int GetHashCode()
        {            
            return _hashCode;
        }

        private int GenerateHashCode()
        {
			unchecked
			{
				int hash = 17;

				hash = hash * 92821 + ReportId.GetHashCode( );

				hash = hash * 92821 + ConditionsOnly.GetHashCode( );

				return hash;
			}
        }

        public static bool operator ==(CachingReportToQueryConverterKey left, CachingReportToQueryConverterKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CachingReportToQueryConverterKey left, CachingReportToQueryConverterKey right)
        {
            return !Equals(left, right);
        }
    }
}
