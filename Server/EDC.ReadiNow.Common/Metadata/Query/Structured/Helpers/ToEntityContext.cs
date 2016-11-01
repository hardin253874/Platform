// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Helper class to keep track of state context when converting StructuredQuery object structures to entity model.
    /// </summary>
    internal class ToEntityContext
    {
        public ToEntityContext()
        {
            Nodes = new Dictionary<Guid, Model.ReportNode>();
            Expressions = new Dictionary<Guid, Model.ReportExpression>();
            ColumnReferencesToBeMapped = new Dictionary<Model.ColumnReferenceExpression, Guid>();
            ToDelete = new List<long>();
            Columns = new Dictionary<Guid, Model.ReportColumn>();
            SelectColumns = new Dictionary<Guid, SelectColumn>();
        }

        /// <summary>
        /// The settings object that was originally passed in.
        /// </summary>
        public ToEntitySettings Settings { get; set; }

        /// <summary>
        /// The root report object that is being converted.
        /// </summary>
        public ReportInfo ReportInfo { get; set; }

        /// <summary>
        /// The root (writable) report object that is being populated.
        /// </summary>
        public Model.Report ReportEntity { get; set; }

        /// <summary>
        /// The query result metadata returned from the query builder engine.
        /// </summary>
        public QueryBuild QueryResult { get; set; }

        /// <summary>
        /// Dictionary of nodes that have been converted.
        /// </summary>
        public Dictionary<Guid, Model.ReportNode> Nodes { get; set; }

        /// <summary>
        /// Dictionary of expressions that have been converted.
        /// </summary>
        public Dictionary<Guid, Model.ReportExpression> Expressions { get; set; }

        /// <summary>
        /// Map of column reference expressions that still need to be attached to their columns.
        /// </summary>
        public Dictionary<Model.ColumnReferenceExpression, Guid> ColumnReferencesToBeMapped { get; set; }

        /// <summary>
        /// Generated column entities.
        /// </summary>
        public Dictionary<Guid, Model.ReportColumn> Columns { get; set; }

        /// <summary>
        /// Inputted columns from structured query
        /// </summary>
        public Dictionary<Guid, SelectColumn> SelectColumns { get; set; }

        /// <summary>
        /// List of entities to be deleted.
        /// </summary>
        public List<long> ToDelete { get; set; }
    }
}
