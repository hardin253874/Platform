// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.ReadiNow.Model;
using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Metadata.Query.Structured.Helpers
{
    public class FromEntityContext : IReportToQueryContext
    {
        internal ReportToQueryConverterSettings Settings { get; set; }

        internal Dictionary<long, Guid> ReportNodeMap { get; }

        internal Dictionary<long, Entity> ReportNodeToEntityMap { get; }

        internal Dictionary<long, Guid> ReportColumnMap { get; }

        internal Dictionary<Guid, long> ColumnReferenceMap { get; }

        internal Dictionary<Guid, long> ReportExpressionMap { get; }

        internal Dictionary<long, ScalarExpression> InstanceExpressionMap { get; }


        internal Dictionary<long, string> ReportInvalidNodes { get; }

        internal Dictionary<long, string> ReportInvalidColumns { get; }

        internal Dictionary<long, string> ReportInvalidConditions { get; }

        public FromEntityContext()
        {
            ReportNodeMap = new Dictionary<long, Guid>();
            ReportNodeToEntityMap = new Dictionary<long, Entity>();
            ReportColumnMap = new Dictionary<long, Guid>();
            ColumnReferenceMap = new Dictionary<Guid, long>();
            ReportExpressionMap = new Dictionary<Guid, long>();
            InstanceExpressionMap = new Dictionary<long, ScalarExpression>();
            ReportInvalidNodes = new Dictionary<long, string>();
            ReportInvalidColumns = new Dictionary<long, string>();
            ReportInvalidConditions = new Dictionary<long, string>();
            Settings = new ReportToQueryConverterSettings( );
        }

        internal Report Report { get; set; }

        internal string DebugInfo
        {
            get
            {
                if (Report == null)
                    return "";
                string fullMessage = $"Report {Report.Name} ({Report.Id}): ";
                return fullMessage;
            }
        }


    }
}
