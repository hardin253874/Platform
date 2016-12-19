// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Query.Structured;
using System;
using System.Collections.Generic;
using EDC.ReadiNow.Metadata.Reporting;
using EDC.ReadiNow.Metadata.Reporting.Formatting;
using Model = EDC.ReadiNow.Model;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Security;
using System.IO;
using EDC.ReadiNow.Core;
using EDC.ReadiNow.Core.Cache;
using ReadiNow.Reporting.Definitions;
using ReadiNow.Reporting.Helpers;

// Should the result include the settings?
using ReportSettings = ReadiNow.Reporting.Request.ReportSettings;

namespace ReadiNow.Reporting.Result
{
    /// <summary>
    /// Top level object for report result.
    /// </summary>
    public class ReportResult
    {
        // Populated at object instantiation
        private readonly long _typeIdForReport;
        private readonly string _typeNameForReport;
        private readonly Model.Report _report;
        private readonly StructuredQuery _query;
        private readonly GridReportDataView _gridReportDataView;
        private readonly ConditionalFormatter _conditionalFormatter;
        private readonly Dictionary<long, List<ChoiceItemDefinition>> _choiceSelections;
        private readonly Dictionary<long, long> _inlineReports;
                        
        // Actual data returned from the query engine
        private readonly QueryResult _queryResult;
        private readonly ReportSettings _settings;

        // For Meta Data
        private bool _metadataBuilt;
        private ReportMetadata _reportMetadata;
        private Dictionary<string, Dictionary<string, string>> _dateTimeValueSelections;

        // Grouping and rollups
        private readonly ClientAggregate _clientAggregate;
        private readonly QueryResult _rollupResult;
        
        /// <summary>
        /// Gets or sets the metadata.
        /// </summary>
        /// <value>The metadata.</value>
        public ReportMetadata Metadata
        {
            get
            {
                // Lazy initialize.
                return _metadataBuilt ? _reportMetadata : BuildReportMetadata();
            }
        }

        // For the report tabular data
        private bool _gridDataBuilt;
        private List<DataRow> _gridData;

        /// <summary>
        /// Gets or sets the grid data.
        /// </summary>
        /// <value>The grid data.</value>
        public List<DataRow> GridData
        {
            get
            {
                // Lazy initialize.
                return _gridDataBuilt ? _gridData : BuildGridData();
            }
        }

        private bool _aggregateDataBuilt;
        private List<ReportDataAggregate> _aggregateData;

        /// <summary>
        /// Gets or sets the aggregate data.
        /// </summary>
        /// <value>The aggregate data.</value>
        public List<ReportDataAggregate> AggregateData
        {
            get
            {
                // Lazy initialize.
                return _aggregateDataBuilt ? _aggregateData : BuildAggregateData();
            }
        }

        private bool _aggregateMetadataBuilt;
        private ReportMetadataAggregate _aggregateMetadata;

        /// <summary>
        /// Gets or sets the aggregate data.
        /// </summary>
        /// <value>The aggregate data.</value>
        public ReportMetadataAggregate AggregateMetadata
        {
            get
            {
                // Lazy initialize.
                return _aggregateMetadataBuilt ? _aggregateMetadata : BuildAggregateMetadata();
            }
        }

        private bool _reportQueryColumnsBuilt;
        private Dictionary<string, ReportColumn> _reportQueryColumns;
        public Dictionary<string, ReportColumn> ReportQueryColumns
        {
            get
            {
                return _reportQueryColumnsBuilt ? _reportQueryColumns : BuildReportQueryColumns();
            }
        }

        private bool _conditionalFormatRulesBuilt;
        private Dictionary<string, ReportColumnConditionalFormat> _conditionalFormatRules;
        /// <summary>
        /// Gets the conditional format rules.
        /// </summary>
        /// <value>The conditional format rules.</value>
        public Dictionary<string, ReportColumnConditionalFormat> ConditionalFormatRules
        {
            get
            {
                return _conditionalFormatRulesBuilt ? _conditionalFormatRules : BuildConditionalFormatRules();
            }
        }

        private bool _defaultConditionalFormatRulesBuilt;
        private Dictionary<string, ReportColumnConditionalFormat> _defaultConditionalFormatRules;
        /// <summary>
        /// Gets the conditional format rules.
        /// </summary>
        /// <value>The conditional format rules.</value>
        public Dictionary<string, ReportColumnConditionalFormat> DefaultConditionalFormatRules
        {
            get
            {
                return _defaultConditionalFormatRulesBuilt ? _defaultConditionalFormatRules : BuildDefaultConditionalFormatRules();
            }
        }

        private bool _valueFormatRulesBuilt;
        private Dictionary<string, ReportColumnValueFormat> _valueFormatRules;
        public Dictionary<string, ReportColumnValueFormat> ValueFormatRules
        {
            get
            {
                return _valueFormatRulesBuilt ? _valueFormatRules : BuildValueFormatRules();
            }
        }
        private bool _imageScaleRulesBuilt;
        private Dictionary<string, ReportImageScale> _imageScaleRules;
        /// <summary>
        /// Gets the image scale rules.
        /// </summary>
        /// <value>The image scale rules.</value>
        public Dictionary<string, ReportImageScale> ImageScaleRules
        {
            get 
            { 
                return _imageScaleRulesBuilt ? _imageScaleRules : BuildImageScaleRules();
            }
        }

        private bool _reportAnalyserColumnsBuilt;
        private Dictionary<string, ReportAnalyserColumn> _reportAnalyserColumns;

        public Dictionary<string, ReportAnalyserColumn> ReportAnalyserColumns
        {
            get { return _reportAnalyserColumnsBuilt ? _reportAnalyserColumns : BuildReportAnalyserColumns(); }
        }

        private bool _isImageRootReport = false;

        /// <summary>
        /// Gets or sets the  Invalid Report information.
        /// </summary>
        /// <value>The Invalid Report information.</value>
        public Dictionary<string, Dictionary<long, string>> InvalidReportInformation { get; set; }
        public ReportResult(Model.Report report, StructuredQuery structuredQuery, QueryResult queryResult, ReportSettings settings)
            : this(report, structuredQuery, queryResult, null, null, settings)
        {

        }

        /// <summary>
        /// Gets and caches the name field identifier.
        /// </summary>
        /// <value>
        /// The name field identifier.
        /// </value>
        private long NameFieldId
        {
            get
            {
                if (_nameFieldId == -1)
                {
                    _nameFieldId = Model.Entity.GetId("core", "name");
                }

                return _nameFieldId;
            }
        }
        private long _nameFieldId = -1;

        /// <summary>
        /// The root entity
        /// </summary>
        private readonly Model.EntityRef _rootEntity;

        /// <summary>
        /// The image type entity ref.
        /// </summary>
        private readonly Model.EntityRef _imageTypeRef;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportResult" /> class.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="structuredQuery">The structured query.</param>
        /// <param name="queryResult">The query result.</param>
        /// <param name="clientAggregate">The client aggregate.</param>
        /// <param name="rollupResult">The rollup result.</param>
        /// <param name="settings">The settings.</param>
        public ReportResult(Model.Report report, StructuredQuery structuredQuery, QueryResult queryResult, ClientAggregate clientAggregate, QueryResult rollupResult, ReportSettings settings)
        {
            _report = report;
            _query = structuredQuery;
            _queryResult = queryResult;
            _clientAggregate = clientAggregate;
            _rollupResult = rollupResult;
            // Strip out the components from the report object that are stored as XML and
            // cache them into classes for easy access.
            if (_report == null)
            {
                return;
            }

            // Data Views (i.e. Grid/Chart/Matrix...whatever)
            _gridReportDataView = new GridReportDataView(report);

            // Set up our type, it's used in a few places
            if (_query != null)
            {
                _rootEntity = StructuredQueryHelper.GetRootType(_query);
                Model.EntityType entityType = Model.Entity.Get<Model.EntityType>(_rootEntity);
                _typeIdForReport = entityType.Id;
                using ( CacheManager.ExpectCacheMisses( ) )
                {
                    _typeNameForReport = entityType.GetField( Model.Entity.Get( "core:name" ) ).ToString( );
                    // Is this an image report?
                    _imageTypeRef = new Model.EntityRef( "core", "imageFileType" );
                    _isImageRootReport = Model.EntityTypeHelper.IsDerivedFrom( entityType, _imageTypeRef );
                }
            }

            _choiceSelections = new Dictionary<long, List<ChoiceItemDefinition>>();
            _inlineReports = new Dictionary<long, long>();
            _settings = settings;
            if (structuredQuery != null)
            {
                InvalidReportInformation = structuredQuery.InvalidReportInformation;
            }           

            if ( _queryResult != null && _gridReportDataView != null)
            {
                //if column type is choicefield and with default conditional formatting. still requires ConditionalFormatter object.
                //create ConditionalFormatter even ColumnFormats is null or count is 0
                if (_gridReportDataView.ColumnFormats != null && _gridReportDataView.ColumnFormats.Count > 0)
                    _conditionalFormatter = new ConditionalFormatter(_gridReportDataView.ColumnFormats, _queryResult.Columns, this);
                else
                    _conditionalFormatter = new ConditionalFormatter(null, _queryResult.Columns, this);
            }            
        }


        /// <summary>
        /// Builds the report metadata upon request.
        /// </summary>
        /// <remarks>
        /// This is only performed once and is cached in the object.
        /// </remarks>
        /// <returns>ReportMetadata.</returns>
        private ReportMetadata BuildReportMetadata()
        {
            using (new SecurityBypassContext())
            using (Profiler.Measure("BuildReportMetadata"))
            {
                if (!_metadataBuilt)
                {
                    if (_settings.RequireBasicMetadata || _settings.RequireFullMetadata)
                    {
                        if (_report != null)
                        {
                            Dictionary<string, string> reportStyleEnumeration;
                            using (Profiler.Measure("GetReportStyleEnumerationPairs"))
                            {
                                reportStyleEnumeration = GetReportStyleEnumerationPairs();
                            }
                            _reportMetadata = new ReportMetadata();
                            _reportMetadata.Alias = _report.Alias;
                            using (Profiler.Measure("ReportTitle"))
                            {
                                _reportMetadata.ReportTitle = _report.Name;
                            }
                            using (Profiler.Measure("Hide stuff"))
                            {
                                _reportMetadata.HideAddButton = _report.HideAddButton ?? false;
                                _reportMetadata.HideNewButton = (_report.HideNewButton ?? false) && !(_report.CanCreate ?? false);
                                _reportMetadata.HideDeleteButton = _report.HideRemoveButton ?? false;
                                _reportMetadata.HideActionBar = _report.HideActionBar == null ? false : (bool)_report.HideActionBar;
                                _reportMetadata.HideReportHeader = _report.HideReportHeader == null ? false : (bool)_report.HideReportHeader;
                            }
                            using (Profiler.Measure("ReportStyle"))
                            {
                                _reportMetadata.ReportStyle = _report.ReportStyle != null && reportStyleEnumeration != null ? reportStyleEnumeration[EnumerationAlias(_report.ReportStyle.Alias)] : "Default";
                            }
                            using (Profiler.Measure("DefaultFormId"))
                            {
                                SetDefaultFormForReport(_reportMetadata);
                            }
                            using (Profiler.Measure("TypeConditionalFormatStyles"))
                            {
                                _reportMetadata.TypeConditionalFormatStyles = GetConditionalFormatTypes();
                            }
                            using (Profiler.Measure("ReportColumns"))
                            {
                                _reportMetadata.ReportColumns = ReportQueryColumns;
                            }
                            using (Profiler.Measure("AnalyserColumns"))
                            {
                                _reportMetadata.AnalyserColumns = GetAnalyserColumnsForReport();
                            }
                            using (Profiler.Measure("SortOrders"))
                            {
                                _reportMetadata.SortOrders = SortOrderForReport();
                            }
                            using (Profiler.Measure("ChoiceSelections"))
                            {
								_reportMetadata.ChoiceSelections = _choiceSelections != null && _choiceSelections.Count > 0 ? _choiceSelections : null;
                            }
                            using (Profiler.Measure("ConditionalFormatRules"))
                            {
                                _reportMetadata.ConditionalFormatRules = ConditionalFormatRules;
                            }
                            using (Profiler.Measure("ValueFormatRules"))
                            {
                                _reportMetadata.ValueFormatRules = ValueFormatRules;
                            }
                            using (Profiler.Measure("FormatValueTypeSelectors"))
                            {
                                _reportMetadata.FormatValueTypeSelectors = _dateTimeValueSelections;
                            }
                            using (Profiler.Measure("InlineReportPickers"))
                            {
                                _reportMetadata.InlineReportPickers = new Dictionary<long, long>(_inlineReports);
                            }
                            using (Profiler.Measure("AggregateMetadata"))
                            {
                                _reportMetadata.AggregateMetadata = AggregateMetadata;
                            }
                            using (Profiler.Measure("AggregateData"))
                            {
                                _reportMetadata.AggregateData = AggregateData;
                            }
                            using (Profiler.Measure("InvalidReportInformation"))
                            {
                                _reportMetadata.InvalidReportInformation = InvalidReportInformation;
                            }
                            using (Profiler.Measure("Modified"))
                            {
                                if (_report.ModifiedDate.HasValue)
                                {
                                    _reportMetadata.Modified = _report.ModifiedDate.Value;
                                }
                            }

                            Dictionary<string, ReportImageScale> imageScaleRules = ImageScaleRules;
                            if (imageScaleRules != null)
                            {
                                foreach (KeyValuePair<string, ReportImageScale> reportImageScale in imageScaleRules)
                                {
                                    ReportColumnValueFormat rcvf;
                                    if (!ValueFormatRules.TryGetValue(reportImageScale.Key, out rcvf))
                                    {
                                        continue;
                                    }
                                    rcvf.ImageHeight = reportImageScale.Value.ImageHeight;
                                    rcvf.ImageWidth = reportImageScale.Value.ImageWidth;
                                    rcvf.ImageScaleId = reportImageScale.Value.ImageScaleId;
                                    rcvf.ImageSizeId = reportImageScale.Value.ImageSizeId;                                    
                                }
                            }
                        }
                    }
                    else if (_settings.RequireColumnBasicMetadata)
                    {
                        _reportMetadata = new ReportMetadata
                        {                            
                            ReportColumns = ReportQueryColumns                         
                        };

                        if (_report != null)
                        {
                            _reportMetadata.Alias = _report.Alias;
                        }
                    }
                    else if (_settings.RequireSchemaMetadata)
                    {
                        _reportMetadata = new ReportMetadata();
                        using (Profiler.Measure("ReportTitle"))
                        {
                            _reportMetadata.ReportTitle = _report.Name;
                        }
                       
                        using (Profiler.Measure("ReportColumns"))
                        {
                            _reportMetadata.ReportColumns = ReportQueryColumns;
                        }
                        using (Profiler.Measure("AnalyserColumns"))
                        {
                            _reportMetadata.AnalyserColumns = GetAnalyserColumnsForReport();
                        }
                        using (Profiler.Measure("SortOrders"))
                        {
                            _reportMetadata.SortOrders = SortOrderForReport();
                        }
                        using (Profiler.Measure("InvalidReportInformation"))
                        {
                            _reportMetadata.InvalidReportInformation = InvalidReportInformation;
                        }
                        if (_report != null)
                        {
                            _reportMetadata.Alias = _report.Alias;
                        }
                    }
                    _metadataBuilt = true;
                }
                return _reportMetadata;
            }
        }

        /// <summary>
        /// Sets the default form for the report.
        /// </summary>
        /// <returns>System.Nullable{System.Int64}.</returns>
        private void SetDefaultFormForReport(ReportMetadata reportMetadata)
        {
            long? defaultFormId = null, resourceViewerFormId = null;

            if (_report.ResourceViewerConsoleForm != null)
            {
                defaultFormId = _report.ResourceViewerConsoleForm.Id;
                resourceViewerFormId = defaultFormId;
            }
            else
            {
                //get default edit form from root entity type

                Model.EntityRef entityRef = StructuredQueryHelper.GetRootType(_query);
                Model.EntityType entityType = Model.Entity.Get<Model.EntityType>(entityRef);
                if (entityType != null && entityType.DefaultEditForm != null)
                {
                    defaultFormId = entityType.DefaultEditForm.Id;
                }
            }

            reportMetadata.DefaultFormId = defaultFormId;
            reportMetadata.ResourceViewerFormId = resourceViewerFormId;            
        }

        /// <summary>
        /// Builds the image scale rules.
        /// </summary>
        /// <returns>Dictionary{GuidReportImageScale}.</returns>
        private Dictionary<string, ReportImageScale> BuildImageScaleRules()
        {
            // Later on we will need to include image scale rules for image columns however for now we leave it out.

            // Also check to see if we need to build the conditional formatting rules, if so then build them now
            // since they also might have image formats associated to them
            if (!_conditionalFormatRulesBuilt)
            {
                BuildConditionalFormatRules();
            }
            _imageScaleRulesBuilt = true;
            return _imageScaleRules;
        }
        
        
        #region Conditional Formatting Metadata

        /// <summary>
        /// Conditionals the format for report.
        /// </summary>
        /// <returns>List{ReportConditionalFormatRule}.</returns>
        private Dictionary<string, ReportColumnConditionalFormat> BuildConditionalFormatRules()
        {
            Dictionary<string, ReportImageScale> scalingRules = null;
            _conditionalFormatRules = _conditionalFormatter != null && _conditionalFormatter.HasRules ? _conditionalFormatter.FormatsForReport(out scalingRules) : null;
			if ( scalingRules != null && scalingRules.Count > 0 )
            {
                // Append these to the image scaling rules for the report
                if (_imageScaleRules == null)
                {
                    _imageScaleRules = new Dictionary<string, ReportImageScale>(scalingRules);
                }                
                else
                {
                    foreach (KeyValuePair<string, ReportImageScale> reportImageScale in scalingRules)
                    {
                        _imageScaleRules[reportImageScale.Key] = reportImageScale.Value;
                    }
                }
            }
            _conditionalFormatRulesBuilt = true;
            return _conditionalFormatRules;
        }

        private Dictionary<string, ReportColumnConditionalFormat> BuildDefaultConditionalFormatRules()
        {            
            if (_choiceSelections != null)
            {
                foreach (var choiceSelection in _choiceSelections)
                {
                    ReportColumnConditionalFormat columnFormat = new ReportColumnConditionalFormat();
                    List<ReportConditionalFormatRule> rules = new List<ReportConditionalFormatRule>();                    
                    columnFormat.Rules = new List<ReportConditionalFormatRule>(choiceSelection.Value.Select(csv => new ReportConditionalFormatRule
                    {
                        Operator = ConditionType.AnyOf,
                        Values = getConditionValues(csv),
                        BackgroundColor = !string.IsNullOrEmpty(csv.BackgroundColor) ? ColourFromString(csv.BackgroundColor) : null,
                        ForegroundColor = !string.IsNullOrEmpty(csv.ForegroundColor) ? ColourFromString(csv.ForegroundColor) : null,
                        ImageEntityId = csv.ImageEntityId,
                        Predicate = PredicateForRule(csv.EntityIdentifier)
                    }));

                    if (_defaultConditionalFormatRules == null)
                        _defaultConditionalFormatRules = new Dictionary<string, ReportColumnConditionalFormat>();

                    if (columnFormat.Rules.Any(rule => rule != null && rule.Values != null))
                    {
                        _defaultConditionalFormatRules.Add(choiceSelection.Key.ToString(), columnFormat);
                    }
                    else
                    {
                        _defaultConditionalFormatRules.Add(choiceSelection.Key.ToString(), null);
                    }
                    

                }
                _defaultConditionalFormatRulesBuilt = true;
            }
            
            return _defaultConditionalFormatRules;
        }

       

        private Predicate<object> PredicateForRule(long conditionValue)
        {
            try
            {

                Predicate<object> valueFilterPredicate = BuildValueFilterPredicateNoNulls(conditionValue);

                return value =>
                {
                    try
                    {
                        // Handle nulls
                        // for now ANY type of filter on a column will reject null values
                        if (value == null || value is DBNull)
                        {
                            return false;
                        }

                        var xml = (string)value;
                        value = DatabaseTypeHelper.GetEntityXmlId(xml);

                        // Evaluate the specific predicate
                        return valueFilterPredicate(value);
                    }
                    catch
                    {
                        return false;                       
                    }
                };
            }
            catch
            {
                return null;
            }
        }

        private  Predicate<object> BuildValueFilterPredicateNoNulls(long value)
        {
            // Get and check argument           
            return cell => checkEqual(cell, value);            

        }

        private bool checkEqual(object cellValue, long value)
        {
            if (cellValue == null || value == 0)
                return false;
            bool isEqualed = cellValue.ToString() == value.ToString();
            return isEqualed;
        }


        private Dictionary<long, string> getConditionValues(ChoiceItemDefinition choiceItem)
        {
            if (choiceItem.ImageEntityId == null && 
                string.IsNullOrEmpty(choiceItem.BackgroundColor) &&
                string.IsNullOrEmpty(choiceItem.ForegroundColor))
                return null;

            Dictionary<long, string> values = new Dictionary<long, string>();
            values[choiceItem.EntityIdentifier] = choiceItem.DisplayName;
            return values;
        }

        private ReportConditionColor ColourFromString(string colourHexString)
        {
            try
            {
                UInt32 barColour = Convert.ToUInt32(colourHexString, 16);
                ReportConditionColor colourInfo = new ReportConditionColor
                {
                    Alpha = (byte)((barColour & 0xff000000) >> 24),
                    Red = (byte)((barColour & 0x00ff0000) >> 16),
                    Green = (byte)((barColour & 0x0000ff00) >> 8),
                    Blue = (byte)(barColour & 0x000000ff)
                };
                return colourInfo;
            }
            catch (Exception)
            {
                // Failed to format, just leave it as null
                return null;
            }
        }

        internal string EnumerationAlias(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            string[] enumValueParts = value.Split(':');
            return enumValueParts.Count() == 2 ? enumValueParts[1] : enumValueParts[0];

        }

        private Dictionary<string, string> GetAlignmentEnumerationPairs()
        {                                 
            Dictionary<string, string> typePairs = new Dictionary<string, string>();
            foreach (var enumValue in Model.Entity.GetInstancesOfType<Model.AlignEnum>(false).ToList())
            {
                typePairs[EnumerationAlias(enumValue.Alias)] = enumValue.Name;
            }
            return typePairs;
        }

        private Dictionary<string, string> GetReportStyleEnumerationPairs()
        {
            Dictionary<string, string> typePairs = new Dictionary<string, string>();
            foreach (var enumValue in Model.Entity.GetInstancesOfType<Model.ReportStyleEnum>(false).ToList())
            {
                typePairs[EnumerationAlias(enumValue.Alias)] = enumValue.Name;
            }
            return typePairs;
        } 

        /// <summary>
        /// Builds the value format rules.
        /// </summary>
        /// <returns>Dictionary{GuidReportColumnValueFormat}.</returns>
        private Dictionary<string, ReportColumnValueFormat> BuildValueFormatRules()
        {
            _valueFormatRules = new Dictionary<string, ReportColumnValueFormat>();
            // Populate based on the content of the grid data view and the columns sent as part of the report metadata
            // BuildReportColumns
            if (!_reportQueryColumnsBuilt)
            {
                BuildReportQueryColumns();
            }
            if (_gridReportDataView != null)
            {
                foreach (KeyValuePair<string, ReportColumn> reportQueryColumn in _reportQueryColumns)
                {                    
                    ColumnFormatting columnFormatting = _gridReportDataView.ColumnFormats.FirstOrDefault(cf => cf.EntityId.ToString(CultureInfo.InvariantCulture) == reportQueryColumn.Key);
                    ReportColumnValueFormat reportColumnValueFormat = null;
                    if (columnFormatting != null)
                    {
                        Dictionary<string, string> alignmentEnumeration = GetAlignmentEnumerationPairs();
                        // General column formatting
                        reportColumnValueFormat = new ReportColumnValueFormat
                            {
                                
                                HideDisplayValue = !columnFormatting.ShowText,
                                DisableDefaultFormat = columnFormatting.DisableDefaultFormat,
                                Alignment = columnFormatting.Alignment != null && alignmentEnumeration != null ? alignmentEnumeration[EnumerationAlias(columnFormatting.Alignment.Alias)] : null,
                                Prefix = string.IsNullOrEmpty(columnFormatting.Prefix) ? null : columnFormatting.Prefix, 
                                Suffix = string.IsNullOrEmpty(columnFormatting.Suffix) ? null : columnFormatting.Suffix,
                                NumberOfLines = columnFormatting.Lines,
                                DecimalPlaces = columnFormatting.DecimalPlaces
                            };
                        // Only specific for image formats
                        ImageFormattingRule imageFormattingRule = columnFormatting.FormattingRule as ImageFormattingRule;
                        if (imageFormattingRule != null)
                        {
                            // Validate thumbnail size identifier
                            Model.EntityRef imageSizeId = null;
                            if (imageFormattingRule.ThumbnailSizeId == null || imageFormattingRule.ThumbnailSizeId.IsEmpty)
                            {
                                imageSizeId = new Model.EntityRef("console", "iconThumbnailSize");
                            }
                            // Validate thumbnail size identifier
                            Model.EntityRef imageScaleId = null;
                            if (imageFormattingRule.ThumbnailScaleId == null || imageFormattingRule.ThumbnailScaleId.IsEmpty)
                            {
                                imageScaleId = new Model.EntityRef("core", "scaleImageProportionally");
                            }
                            Model.ThumbnailSizeEnum sizeEnum = Model.Entity.Get<Model.ThumbnailSizeEnum>(imageSizeId ?? imageFormattingRule.ThumbnailSizeId);
                            //
                            // <hack>
                            // The default image size if not specified is 16; this has been hard coded in the original reporting engine; really should be a default
                            // on the type. Leaving for now due to time constraints.
                            // </hack>
                            //
                            reportColumnValueFormat.ImageHeight = sizeEnum.ThumbnailHeight ?? 16;
                            reportColumnValueFormat.ImageWidth = sizeEnum.ThumbnailWidth ?? 16;
                            reportColumnValueFormat.ImageSizeId = imageSizeId != null ? imageSizeId.Id : imageFormattingRule.ThumbnailSizeId.Id;
                            reportColumnValueFormat.ImageScaleId = imageScaleId != null ? imageScaleId.Id : imageFormattingRule.ThumbnailScaleId.Id;                            
                        }
                        // Check for any special date/time formatting
                        switch (reportQueryColumn.Value.Type)
                        {
                            case "Date":
                                if (columnFormatting.DateFormat != null)
                                {
                                    reportColumnValueFormat.DateTimeFormat = DateTimeFormatHelper.DateTimeFormatFromEnumeration(columnFormatting.DateFormat.Alias);
                                }
                                break;
                            case "Time":
                                if (columnFormatting.TimeFormat != null)
                                {
                                    reportColumnValueFormat.DateTimeFormat = DateTimeFormatHelper.DateTimeFormatFromEnumeration(columnFormatting.TimeFormat.Alias);
                                }
                                break;
                            case "DateTime":
                                if (columnFormatting.DateTimeFormat != null)
                                {
                                    reportColumnValueFormat.DateTimeFormat = DateTimeFormatHelper.DateTimeFormatFromEnumeration(columnFormatting.DateTimeFormat.Alias);
                                }
                                break;
                        }
                        if (reportQueryColumn.Value.Type == "Date" || reportQueryColumn.Value.Type == "Time" || reportQueryColumn.Value.Type == "DateTime")
                        {
                            // Populate the selection list for these formats
                            if (_dateTimeValueSelections == null)
                            {
                                _dateTimeValueSelections = new Dictionary<string, Dictionary<string, string>>();
                            }
                            if (!_dateTimeValueSelections.ContainsKey(reportQueryColumn.Value.Type))
                            {
                                Dictionary<string, string> typeEnumeration = DateTimeFormatHelper.DisplayNamesForType(reportQueryColumn.Value.Type);
                                if (typeEnumeration != null)
                                {
                                    _dateTimeValueSelections[reportQueryColumn.Value.Type] = typeEnumeration;
                                }
                            }
                        }
                        
                        if (reportQueryColumn.Value.IsAggregateColumn &&
                            columnFormatting.EntityListColumnFormat != null &&
                            (reportQueryColumn.Value.Type == "ChoiceRelationship" || 
                            reportQueryColumn.Value.Type == "InlineRelationship" || 
                            reportQueryColumn.Value.Type == "UserInlineRelationship" ||
                            reportQueryColumn.Value.Type == "StructureLevels"))
                        {
                            reportColumnValueFormat.EntityListColumnFormat = EnumerationAlias(columnFormatting.EntityListColumnFormat.Alias);
                        }
                    }
                    else if (reportQueryColumn.Value.Type != null)
                    {
                        // Inject value rules based on the type
                        switch (reportQueryColumn.Value.Type)
                        {
                            case "Currency":
                                reportColumnValueFormat = new ReportColumnValueFormat
                                    {
                                        HideDisplayValue = false,
                                        DecimalPlaces = reportQueryColumn.Value.DecimalPlaces
                                    };
                                break;
                            case "Decimal":
                                reportColumnValueFormat = new ReportColumnValueFormat
                                    {
                                        HideDisplayValue = false,
                                        DecimalPlaces = reportQueryColumn.Value.DecimalPlaces
                                    };
                                break;
                        }
                    }
                    // <hack>
                    // The default image size if not specified is 16; this has been hard coded in the original reporting engine; really should be a default
                    // on the type. Leaving for now due to time constraints.
                    // </hack>
                    if (reportQueryColumn.Value.Type == "Image" && reportColumnValueFormat == null)
                    {
                        reportColumnValueFormat = new ReportColumnValueFormat
                            {
                                ImageHeight = 16, 
                                ImageWidth = 16,
                                ImageSizeId = new Model.EntityRef("console", "iconThumbnailSize").Id, 
                                ImageScaleId = new Model.EntityRef("core", "scaleImageProportionally").Id
                            };
                    }
                    if (reportColumnValueFormat != null)
                    {
                        _valueFormatRules[reportQueryColumn.Key] = reportColumnValueFormat;
                    }
                }
            }
            _valueFormatRulesBuilt = true;
            return _valueFormatRules;
        }

        /// <summary>
        /// Gets the conditional format types.
        /// </summary>
        /// <returns>Dictionary{System.StringList{System.String}}.</returns>
        private Dictionary<string, List<ConditionalFormatStyleEnum>> GetConditionalFormatTypes()
        {
            if (_queryResult == null || _queryResult.Columns == null)
            {
                return null;
            }
            IList<ResultColumn> columns = _queryResult.Columns.Where(col => !col.IsHidden).ToArray();
			if ( columns.Count <= 0 )
            {
                return null;
            }
            Dictionary<string, List<ConditionalFormatStyleEnum>> formatStyles = new Dictionary<string, List<ConditionalFormatStyleEnum>>(columns.Count());
            foreach (ResultColumn resultColumn in columns.Where(resultColumn => resultColumn.ColumnType != null && !formatStyles.ContainsKey(resultColumn.ColumnType.GetDisplayName())))
            {
                List<ConditionalFormatStyleEnum> conditionalCategoryTypes = new List<ConditionalFormatStyleEnum> { ConditionalFormatStyleEnum.Highlight, ConditionalFormatStyleEnum.Icon };
                if (DatabaseTypeHelper.CanLinearScale(resultColumn.ColumnType))
                {
                    conditionalCategoryTypes.Add(ConditionalFormatStyleEnum.ProgressBar);
                }
                formatStyles[resultColumn.ColumnType.GetDisplayName()] = conditionalCategoryTypes;
            }
            return formatStyles;
        }

        #endregion Conditional Formatting Metadata
        
        #region Operator Condition Type Definitions

        /// <summary>
        /// Gets the name of the extended type name for a given query condition.
        /// </summary>
        /// <remarks>
        /// This is specifically designed to create synthesized analyser condition types for non-core types.
        /// It includes handling for operators such as 'CurrentUser' and 'FullTextSearch' but can be extended at a latter date.
        /// </remarks>
        /// <param name="queryCondition">The query condition.</param>
        /// <returns>System.String.</returns>
        private string GetExtendedTypeName(QueryCondition queryCondition)
        {
            if (queryCondition.Argument == null)
            {
                return null;
            }

            string extendedTypeName = queryCondition.Argument.Type.GetDisplayName();
            return extendedTypeName;
        }

        
        #endregion Operator Condition Type Definitions

        #region Analyser Column Definitions

        /// <summary>
        /// Gets the analyser columns for report.
        /// </summary>
        /// <returns>Dictionary{GuidReportAnalyserColumn}.</returns>
        private Dictionary<string, ReportAnalyserColumn> GetAnalyserColumnsForReport()
        {
            if (_query == null || _report == null)
            {
                return null;
            }

            Dictionary<string, ReportAnalyserColumn> analyserColumns = ReportAnalyserColumns;
            // Apply any user defined settings that the report was run with
			if ( _settings != null && _settings.ReportParameters != null && _settings.ReportParameters.AnalyserConditions != null && _settings.ReportParameters.AnalyserConditions.Count > 0 )
            {
                foreach (SelectedColumnCondition condition in _settings.ReportParameters.AnalyserConditions)
                {
                    ReportAnalyserColumn analyserCondition;
                    if (!analyserColumns.TryGetValue(condition.ExpressionId, out analyserCondition))
                    {
                        continue;
                    }
                    // Update the analyser condition with the settings value
                    analyserCondition.Operator = condition.Operator;
                    analyserCondition.Value = condition.Value;
                    analyserCondition.Type = condition.Type;


					if ( condition.EntityIdentifiers == null || condition.EntityIdentifiers.Count <= 0 )
                    {
                        //reset condtion values to null if without entityIdentifiers, otherwise the report parameters's condition values will be overwrite by current analyzer condition's values
                        //base on bug 25445, there are two senarios.  if user click reset on analyzer, the current analzyer condition's value should overwrite the report parameters' condition values
                        // if user clean the analzyer conditions value and click apply on analzyer, the the report parameters's condition values should be set as null.
                        if (!_settings.ReportParameters.IsReset)
                        {
                            analyserCondition.Values = null;
                        }                       
                        continue;
                    }
                    Dictionary<long, string> values = new Dictionary<long, string>(condition.EntityIdentifiers.Count);
                    foreach (long entityId in condition.EntityIdentifiers)
                    {
                        Model.Resource entity = Model.Entity.Get<Model.Resource>(entityId);
                        if (entity != null)
                        {
                            values[entityId] = entity.Name;
                        }
                        else
                        {
                            values[entityId] = string.Empty;
                        }
                    }
                    analyserCondition.Values = values;
                }
            }
            return analyserColumns;
        }

        private Dictionary<string, ReportAnalyserColumn> BuildReportAnalyserColumns()
        {            
            _reportAnalyserColumns = new Dictionary<string, ReportAnalyserColumn>(_report.HasConditions.Count);
            // Get all analyser columns
            foreach (Model.ReportCondition reportCondition in _report.HasConditions)
            {                
                ReportAnalyserColumn reportAnalyserColumn = AnalyserExpressionHelper.AnalyserColumnForCondition(_report, reportCondition);
                if (reportAnalyserColumn == null)
                {
                    continue;
                }
                Model.EntityType entityType = null;

                if (reportAnalyserColumn.TypeId != 0)
                {
                    entityType = Model.Entity.Get<Model.EntityType>(reportAnalyserColumn.TypeId);
                }
                PopulateRelationshipPickers(entityType, reportAnalyserColumn.Type.GetDisplayName(), reportAnalyserColumn.IsNameColumnForType);
                _reportAnalyserColumns[reportCondition.Id.ToString(CultureInfo.InvariantCulture)] = reportAnalyserColumn;
            }
            _reportAnalyserColumnsBuilt = true;
            return _reportAnalyserColumns;
        }

        #endregion Analyser Column Definitions

        #region Report Columns
        /// <summary>
        /// Gets the report columns keyed by entity ID.
        /// </summary>
        /// <returns>System.Collections.Generic.Dictionary{System.Guid,EDC.SoftwarePlatform.Services.Reporting.Definitions.ReportColumn}.</returns>
        private Dictionary<string, ReportColumn> BuildReportQueryColumns()
        {
            if (_reportQueryColumnsBuilt)
            {
                return _reportQueryColumns;
            }
            if (_settings.RequireSchemaMetadata)
            {
                return BuildReportQuerySelectColumns();
            }

			if ( _query == null || _query.SelectColumns == null || _query.SelectColumns.Count <= 0 || _queryResult.Columns == null )
            {
                _reportQueryColumnsBuilt = true;
                return null;
            }
            Dictionary<string, ReportColumn> reportColumns = new Dictionary<string, ReportColumn>(_query.SelectColumns.Count);
            List<ResultColumn> queryResultColumns = _queryResult.Columns;                       

            // Choke off the columns returned if specified (allowing for the first hidden column which is the ID column)
            int firstColumn = queryResultColumns[0].IsHidden ? 1 : 0; // skip ID column
            int maxColumnCount = queryResultColumns.Count - firstColumn;
            int availableColumnCount = maxColumnCount;
            if (_settings.ColumnCount.HasValue && _settings.ColumnCount < maxColumnCount)
            {
                maxColumnCount = _settings.ColumnCount.Value;
            }
            int ordinalOffset = 0;

            // If this is an image based report, inject the first column with a bogus value (that points to the image type)
            if (_isImageRootReport)
            {
                ordinalOffset = 1;
                // Create dummy column
                ReportColumn imageColumn = new ReportColumn { Title = "Image", Type = "Image", FieldId = _imageTypeRef.Id, IsHidden = false, Ordinal = 0, EntityNameField = false, TypeId = _imageTypeRef.Id };
                reportColumns[_imageTypeRef.Id.ToString(CultureInfo.InvariantCulture)] = imageColumn;
            }
            
            int addedColumnsCount = 0;
            int ordinal = ordinalOffset;
            
            for (int i = firstColumn; i < firstColumn + availableColumnCount; i++)
            {
                using (Profiler.Measure("BuildReportQueryColumn"))
                {
                    // Load up the column ID GUID
                    Guid columnId = queryResultColumns [ i ].RequestColumn.ColumnId;
                    SelectColumn selectColumn = queryResultColumns [ i ].RequestColumn;
                    ResultColumn resultColumn = queryResultColumns[i];
                    bool isGroupByColumn = _clientAggregate != null &&
                                           _clientAggregate.GroupedColumns != null &&
                                           _clientAggregate.GroupedColumns.Any(gc => gc.ReportColumnEntityId == selectColumn.EntityId);

                    if (addedColumnsCount < maxColumnCount || isGroupByColumn)
                    {
                        BuildReportQueryColumn(ordinal, columnId, selectColumn, resultColumn, reportColumns, _rootEntity, _isImageRootReport, NameFieldId, _imageTypeRef);
                        ordinal++;

                        // Don't include grouped columns in the count of columns to add.
                        // These are needed for the report to render correctly.
                        if (!isGroupByColumn)
                        {
                            addedColumnsCount++;     
                        }                        
                    } 
                }
            }

            _reportQueryColumns = reportColumns;
            _reportQueryColumnsBuilt = true;
            return _reportQueryColumns;
        }

        private Dictionary<string, ReportColumn> BuildReportQuerySelectColumns()
        {
			if ( _query == null || _query.SelectColumns == null || _query.SelectColumns.Count <= 0 )
            {
                _reportQueryColumnsBuilt = true;
                return null;
            }

            Dictionary<string, ReportColumn> reportColumns = new Dictionary<string, ReportColumn>(_query.SelectColumns.Count);
            //List<ResultColumn> queryResultColumns = _query.SelectColumns;

            // Choke off the columns returned if specified (allowing for the first hidden column which is the ID column)
            int firstColumn = _query.SelectColumns[0].IsHidden ? 1 : 0; // skip ID column
            int maxColumnCount = _query.SelectColumns.Count - firstColumn;
            int availableColumnCount = maxColumnCount;
            if (_settings.ColumnCount.HasValue && _settings.ColumnCount < maxColumnCount)
            {
                maxColumnCount = _settings.ColumnCount.Value;
            }
            int ordinalOffset = 0;

            // If this is an image based report, inject the first column with a bogus value (that points to the image type)
            if (_isImageRootReport)
            {
                ordinalOffset = 1;
                // Create dummy column
                ReportColumn imageColumn = new ReportColumn { Title = "Image", Type = "Image", FieldId = _imageTypeRef.Id, IsHidden = false, Ordinal = 0, EntityNameField = false, TypeId = _imageTypeRef.Id };
                reportColumns[_imageTypeRef.Id.ToString(CultureInfo.InvariantCulture)] = imageColumn;
            }

            int addedColumnsCount = 0;
            int ordinal = ordinalOffset;

            for (int i = firstColumn; i < firstColumn + availableColumnCount; i++)
            {
                using (Profiler.Measure("BuildReportQueryColumn"))
                {
                    // Load up the column ID GUID
                    Guid columnId = _query.SelectColumns[i].ColumnId;
                    SelectColumn selectColumn = _query.SelectColumns[i];
                   // ResultColumn resultColumn = queryResultColumns[i];
                    bool isGroupByColumn = _clientAggregate != null &&
                                           _clientAggregate.GroupedColumns != null &&
                                           _clientAggregate.GroupedColumns.Any(gc => gc.ReportColumnEntityId == selectColumn.EntityId);

                    if (addedColumnsCount < maxColumnCount || isGroupByColumn)
                    {
                        BuildReportQueryColumn(ordinal, columnId, selectColumn, null, reportColumns, _rootEntity, _isImageRootReport, NameFieldId, _imageTypeRef);
                        ordinal++;

                        // Don't include grouped columns in the count of columns to add.
                        // These are needed for the report to render correctly.
                        if (!isGroupByColumn)
                        {
                            addedColumnsCount++;
                        }
                    }
                }
            }

            _reportQueryColumns = reportColumns;
            _reportQueryColumnsBuilt = true;
            return _reportQueryColumns;
        } 

        private void BuildReportQueryColumn(int ordinal, Guid columnId, SelectColumn selectColumn, ResultColumn resultColumn, Dictionary<string, ReportColumn> reportColumns, Model.EntityRef rootEntity, bool haveImageTypeColumns, long nameFieldId, Model.EntityRef imageTypeRef)
        {            
            String type = null;

            // Get the column type
            using (Profiler.Measure("type"))
            {                
                if (_clientAggregate != null && _clientAggregate.AggregatedColumns.Any(ac => ac.ReportColumnId == columnId))
                {
                    type = GetClientAggregateColumnType(haveImageTypeColumns, imageTypeRef, selectColumn, resultColumn);
                }
                else
                {
                    type = GetColumnType(haveImageTypeColumns, imageTypeRef, selectColumn, resultColumn);
                }
            }

            // Extract the display name as the title if it contains something otherwise use the column name for the title.
            string title = string.IsNullOrEmpty(selectColumn.DisplayName) ? selectColumn.ColumnName : selectColumn.DisplayName;

            // Get FieldID
            // And determine if this is the name column
            long fieldId = 0;
            bool isNameColumnForType = false;
            using (Profiler.Measure("FieldId"))
            {
                if (resultColumn != null)
                {
                    ResourceDataColumn resourceDataColumn = resultColumn.RequestColumn.Expression as ResourceDataColumn;
                    if (resourceDataColumn != null)
                    {
                        if (resourceDataColumn.FieldId.HasId)
                        {
                            fieldId = resourceDataColumn.FieldId.Id;
                            // Check to see if this column is the "core:name" field for the type being reported on
                            if (rootEntity != null && resourceDataColumn.NodeId != null &&
                                _query.RootEntity.NodeId == resourceDataColumn.NodeId && fieldId == nameFieldId)
                            {
                                isNameColumnForType = true;
                            }
                        }
                    }

                    // For structure views the field is the name field
                    var structureViewExpression = resultColumn.RequestColumn.Expression as StructureViewExpression;
                    if (structureViewExpression != null)
                    {
                        fieldId = nameFieldId;
                    }
                }
            }

            // Build result
            ReportColumn reportColumn = new ReportColumn
                {
                    Title = title,
                    Type = type,
                    FieldId = fieldId,
                    IsHidden = selectColumn.IsHidden,
                    Ordinal = ordinal,
                    EntityNameField = isNameColumnForType,
                    ColumnError = resultColumn != null ? resultColumn.ColumnError : ""
                };

            // Special processing
            AggregateExpression aggregateExpression = selectColumn.Expression as AggregateExpression;
            reportColumn.IsAggregateColumn = aggregateExpression != null;

            if (type == "InlineRelationship" || type == "ChoiceRelationship" || isNameColumnForType || type == "StructureLevels")
            {
                using (Profiler.Measure("DecorateResourceColumns"))
                {
                    DecorateResourceColumns(selectColumn, type, reportColumn, resultColumn, aggregateExpression, isNameColumnForType);
                }
            }

            using (Profiler.Measure("Populate editable details"))
            {
                // Populate the details to allow this column to be editable
                if (fieldId > 0)
                {
                    PopulateFieldConstraints(reportColumn);
                }
                else if (type == "Currency")
                {
                    PopulateAggregateCurrencyFieldFormat(selectColumn, resultColumn, reportColumn);
                }
            }
            // Populate operator type
            using (Profiler.Measure("OperatorTypeForColumn"))
            {                
                reportColumn.OperatorType = OperatorTypeForColumn(selectColumn.EntityId, type, isNameColumnForType);
            }
            reportColumns[selectColumn.EntityId.ToString(CultureInfo.InvariantCulture)] = reportColumn;
        }

        private static void PopulateAggregateCurrencyFieldFormat(SelectColumn selectColumn, ResultColumn resultColumn, ReportColumn reportColumn)
        {
            //for aggregate column, get aggregate grouped expression field to check current type is currency, 
            //the decimail place is retrived by field's DecimalPlaces property or default 2
            AggregateExpression currencyAggregateExpression = resultColumn != null ? resultColumn.RequestColumn.Expression as AggregateExpression : selectColumn.Expression as AggregateExpression;
            if (currencyAggregateExpression != null)
            {
                ResourceDataColumn aggregateGroupedExpression = currencyAggregateExpression.Expression as ResourceDataColumn;
                if (aggregateGroupedExpression != null)
                {
                    Model.Field field = Model.Entity.Get<Model.Field>(aggregateGroupedExpression.FieldId);
                    if (field != null && field.Is<Model.CurrencyField>())
                    {
                        Model.CurrencyField currencyField = field.As<Model.CurrencyField>();
                        reportColumn.DecimalPlaces = currencyField.DecimalPlaces ?? 2;
                    }
                }
            }
            //As request to keep consistency, all currency field's default decimalplace is 2
            ScriptExpression scriptExpression = selectColumn.Expression as ScriptExpression;
            if (scriptExpression != null)
            {
                reportColumn.DecimalPlaces = 2;
            }
        }

        private string GetColumnType(bool haveImageTypeColumns, Model.EntityRef imageTypeRef, SelectColumn selectColumn, ResultColumn resultColumn)
        {
            string type = null;

            if (resultColumn != null)
            {
                // If we have a Display Pattern then we assume it's an Auto Increment type.
                if (resultColumn.IsRelatedResource)
                {
                    // Related resources are dictionaries of entityIds and strings.
                    type = resultColumn.ColumnType != null
                        ? resultColumn.ColumnType.GetDisplayName()
                        : GetColumnDatabaseType(selectColumn);

                    // <Hack>
                    // NOTE: Inline relationships are related resources however they come back as strings for their type which is sad.
                    // </Hack>
                    if (type == "String")
                    {
                        type = "InlineRelationship";
                    }

                    if (IsColumnAggregateStructureView(selectColumn))
                    {
                        type = "StructureLevels";
                    }
                    else
                    {
                        // A related resource could actually be an image type so look out for it.
                        ResourceDataColumn resourceData = selectColumn.Expression as ResourceDataColumn;
                        if (haveImageTypeColumns && _query.SelectColumns.Any(sc =>
                        {
                            IdExpression expression = sc.Expression as IdExpression;
                            return (expression != null && expression.NodeId == _query.RootEntity.NodeId);
                        }))
                        {
                            type = "Image";
                        }
                        else if (resourceData != null)
                        {
                            Model.EntityRef relatedEntityRef = StructuredQueryHelper.GetEntityType(resourceData.NodeId,
                                _query.RootEntity);
                            if (relatedEntityRef != null)
                            {
                                Model.EntityType relatedEntityType = Model.Entity.Get<Model.EntityType>(relatedEntityRef);
                                if (relatedEntityType != null &&
                                    Model.EntityTypeHelper.IsDerivedFrom(relatedEntityType, imageTypeRef))
                                {
                                    type = "Image";
                                }
                            }
                        }
                    }                
                }               
            }
            else
            {
                // Fall back to the query column type
                type = selectColumn != null ? GetColumnDatabaseType(selectColumn) : new StringType().GetDisplayName();
            }

            if (type == null)
            {
                if (resultColumn != null && resultColumn.ColumnType != null)
                {
                    type = resultColumn.ColumnType.GetDisplayName();
                }
                else if (selectColumn != null)
                {
                    // Fall back to the query column type
                    type = GetColumnDatabaseType(selectColumn);
                }
                else
                {
                    type = new StringType().GetDisplayName();
                }
            }
            return type;
        }

        private string GetClientAggregateColumnType(bool haveImageTypeColumns, Model.EntityRef imageTypeRef, SelectColumn selectColumn, ResultColumn resultColumn)
        {
            string type = null;

            if (resultColumn != null)
            {
                type = resultColumn.ColumnType != null ? resultColumn.ColumnType.GetDisplayName() : new StringType().GetDisplayName();
            }
            else
            {
                type = selectColumn != null ? GetColumnDatabaseType(selectColumn) : new StringType().GetDisplayName();
            }
            
            // A related resource could actually be an image type so look out for it.
            ResourceDataColumn resourceData = selectColumn != null ? selectColumn.Expression as ResourceDataColumn : null;
            if (haveImageTypeColumns && _query.SelectColumns.Any(sc =>
            {
                IdExpression expression = sc.Expression as IdExpression;
                return (expression != null && expression.NodeId == _query.RootEntity.NodeId);
            }))
            {
                type = "Image";
            }
            else if (resourceData != null)
            {
                Model.EntityRef relatedEntityRef = StructuredQueryHelper.GetEntityType(resourceData.NodeId, _query.RootEntity);
                if (relatedEntityRef != null)
                {
                    Model.EntityType relatedEntityType = Model.Entity.Get<Model.EntityType>(relatedEntityRef);
                    if (relatedEntityType != null && Model.EntityTypeHelper.IsDerivedFrom(relatedEntityType, imageTypeRef))
                    {
                        type = "Image";
                    }
                }
            }
            return type;
        }

        private void DecorateResourceColumns(SelectColumn selectColumn, String type, ReportColumn reportColumn, ResultColumn resultColumn, AggregateExpression aggregateExpression, bool isNameColumnForType)
        {
            ResourceDataColumn rdc = selectColumn.Expression as ResourceDataColumn;
            if (rdc != null)
            {
                Model.EntityRef entityRef = StructuredQueryHelper.GetEntityType(rdc.NodeId, _query.RootEntity);
                if (entityRef != null)
                {
                    Model.EntityType entityType = Model.Entity.Get<Model.EntityType>(entityRef);
                    PopulateRelationshipPickers(entityType, reportColumn, type, isNameColumnForType);
                }

                if (type == "InlineRelationship" || type == "ChoiceRelationship")
                {
                    Entity relatedEntity = getRelatedEntityByNodeId(_query.RootEntity, rdc.NodeId);
                    RelatedResource relatedResource = relatedEntity as RelatedResource;

                    if (type == "InlineRelationship")
                    {
                        SetReportColumnRelationshipCardinality(reportColumn, relatedResource);
                    }                    

                    if (relatedResource?.RelationshipTypeId != null)
                    {
                        reportColumn.RelationshipTypeId = relatedResource.RelationshipTypeId.Id;
                        reportColumn.RelationshipIsReverse = relatedResource.RelationshipDirection == RelationshipDirection.Reverse;
                    }
                }                
            }

            StructureViewExpression sve = selectColumn.Expression as StructureViewExpression;
            if (sve != null)
            {
                Model.EntityRef entityRef = StructuredQueryHelper.GetEntityType(sve.NodeId, _query.RootEntity);
                if (entityRef != null)
                {
                    Model.EntityType entityType = Model.Entity.Get<Model.EntityType>(entityRef);
                    PopulateRelationshipPickers(entityType, reportColumn, type, isNameColumnForType);
                }                
            }

            if (aggregateExpression != null)
            {
                ResourceDataColumn expressionDataColumn = aggregateExpression.Expression as ResourceDataColumn;
                if (expressionDataColumn != null)
                {
                    AggregateEntity aggregateRootEntity = _query.RootEntity as AggregateEntity;
                    AggregateEntity aggregateEntity = null;
                    RelatedResource relatedResource = null;
                    // Look up the query to see if this expression node is a related resource and get the entity type id for this column                            
                    //if root entity is aggregate entity, lookup the grouped entity
                    if (aggregateRootEntity != null && aggregateRootEntity.NodeId == aggregateExpression.NodeId)
                    {
                        Entity relatedEntity = getRelatedEntityByNodeId(aggregateRootEntity.GroupedEntity,
                            expressionDataColumn.NodeId);                            

                        relatedResource = relatedEntity as RelatedResource;


                    }
                    else
                    {
                        Entity relatedEntity = getRelatedEntityFromNode(_query.RootEntity, aggregateExpression.NodeId);

                        aggregateEntity = relatedEntity as AggregateEntity;

                        if (aggregateEntity != null && aggregateEntity.GroupedEntity != null)
                        {
                            relatedResource = aggregateEntity.GroupedEntity as RelatedResource;
                        }
                    }

                    // Get the type of this entity
                    if (relatedResource != null && relatedResource.EntityTypeId != null)
                    {
                        Model.EntityType entityType = Model.Entity.Get<Model.EntityType>(relatedResource.EntityTypeId);
                        PopulateRelationshipPickers(entityType, reportColumn, type, false);
                    }

                    if (type == "InlineRelationship" || type == "ChoiceRelationship")
                    {
                        if (type == "InlineRelationship")
                        {
                            SetReportColumnRelationshipCardinality(reportColumn, relatedResource);
                        }                        

                        if (relatedResource?.RelationshipTypeId != null)
                        {
                            reportColumn.RelationshipTypeId = relatedResource.RelationshipTypeId.Id;
                            reportColumn.RelationshipIsReverse = relatedResource.RelationshipDirection == RelationshipDirection.Reverse;
                        }
                    }
                }
            }

            // Get resource type from query builder result. E.g. for calculated columns.
            // (Ideally every column type should return its ResourceTypeId this way)
            if ( reportColumn.TypeId == 0 && resultColumn != null && resultColumn.ResourceTypeId != 0 )
            {
                Model.EntityType entityType = Model.Entity.Get<Model.EntityType>( resultColumn.ResourceTypeId );
                PopulateRelationshipPickers(entityType, reportColumn, type, false);
            }
        }

        private Entity getRelatedEntityByNodeId(Entity entity, Guid nodeId)
        {
            if (entity == null)
                return null;

            if (entity.NodeId == nodeId)
            {
                return entity;
            }
            Entity retEntity = null;
            if (entity.RelatedEntities != null && entity.RelatedEntities.Count > 0)
            {
                
                foreach (Entity relatedEntity in entity.RelatedEntities)
                {
                    retEntity = getRelatedEntityByNodeId(relatedEntity, nodeId);
                    if (retEntity != null)
                        break;
                }
            }

            return retEntity;
        }

        /// <summary>
        /// Get the relatedEntity from query rootnode tree, looping though entity's relatedEntity collection to find the match nodeId
        /// if current entity is aggregateEntity, get from groupedEntity node
        /// </summary>
        /// <param name="currentEntity"></param>
        /// <param name="nodeId"></param>
        /// <returns>the related entity match nodeid</returns>
        private Entity getRelatedEntityFromNode(Entity currentEntity, Guid nodeId)
        {
            Entity relatedEntity = null;
            AggregateEntity aggregateRootEntity = currentEntity as AggregateEntity;
            List<Entity> relatedEntities = aggregateRootEntity != null && aggregateRootEntity.GroupedEntity != null
                ? aggregateRootEntity.GroupedEntity.RelatedEntities
                : currentEntity.RelatedEntities;

            foreach (Entity currentRelatedEntity in relatedEntities)
            {
                if (currentRelatedEntity.NodeId == nodeId)
                {
                    relatedEntity = currentRelatedEntity;
                    break;
                }
                else
                {
                    relatedEntity = getRelatedEntityFromNode(currentRelatedEntity, nodeId);
                    if (relatedEntity != null)
                        break;
                }
            }                 
            return relatedEntity;
        }

        private string OperatorTypeForColumn(long entityId, string type, bool isNameColumnForType)
        {
            Model.ReportColumn reportColumn = _report.ReportColumns.First(rc => rc.Id == entityId);

            // Get the result type from the expression
            Model.ReportExpression reportExpression = reportColumn.ColumnExpression;
            string resultType = type ?? new StringType().GetDisplayName();

            // Find the node for this expression
            Model.NodeExpression nodeExpression = reportExpression != null ? reportExpression.As<Model.NodeExpression>() : null;
            if (nodeExpression == null)
            {
                return resultType;
            }

            Model.ResourceReportNode resourceReportNode =  nodeExpression.SourceNode.As<Model.ResourceReportNode>() ;
            if (resourceReportNode == null)
            {
                return resultType;
            }

            // Is this a resource expression or a field expression where the field type is 'core:name'
            Model.ResourceExpression resourceExpression = reportExpression.As<Model.ResourceExpression>();
            Model.FieldExpression fieldExpression = null;
            if (resourceExpression == null)
            {
                fieldExpression = reportExpression.As<Model.FieldExpression>();
            }
            
            if (resourceExpression == null && fieldExpression == null)
            {
                return resultType;
            }
            if (fieldExpression != null && fieldExpression.FieldExpressionField.Alias != "core:name")
            {
                return resultType;
            }

            Model.EntityType entityTypeFromReport = resourceReportNode.ResourceReportNodeType;
            if ( entityTypeFromReport == null )
            {
                return resultType;
            }

            // Refetch as we are leaving the report's graph
            Model.EntityType entityType = Model.Entity.Get<Model.EntityType>( entityTypeFromReport );
            if ( entityType == null )
            {
                return resultType;
            }

            if (!Model.EntityTypeHelper.IsDerivedFrom(entityType, "core:person") && !Model.EntityTypeHelper.IsDerivedFrom(entityType, "core:userAccount"))
            {
                return isNameColumnForType ? "InlineRelationship" : resultType;
            }

            // If this is a relationship then make it as a user inline relationship otherwise a simple user string.
            return nodeExpression.SourceNode.Is<Model.RelationshipReportNode>() ? "UserInlineRelationship" : "UserString";
        }


        /// <summary>
        /// Sets the report column relationship cardinality.
        /// </summary>
        /// <param name="reportColumn">The report column.</param>
        /// <param name="relatedResource">The related resource.</param>
        private void SetReportColumnRelationshipCardinality(ReportColumn reportColumn, RelatedResource relatedResource)
        {
            if (reportColumn == null || relatedResource == null)
            {
                return;
            }            
            
            if (relatedResource.RelationshipTypeId == null)
            {
                return;
            }

            var relationship = Model.Entity.Get<Model.Relationship>(relatedResource.RelationshipTypeId);
            if (relationship == null)
            {
                return;
            }

            Model.CardinalityEnum_Enumeration? cardinalityEnum = relationship.Cardinality_Enum;
            if (cardinalityEnum == null)
            {
                return;
            }

            // Switch the cardinality for reverse relationships
            if (relatedResource.RelationshipDirection == RelationshipDirection.Reverse)
            {
                switch (cardinalityEnum.Value)
                {
                    case Model.CardinalityEnum_Enumeration.ManyToOne:
                        cardinalityEnum = Model.CardinalityEnum_Enumeration.OneToMany;
                        break;
                    case Model.CardinalityEnum_Enumeration.OneToMany:
                        cardinalityEnum = Model.CardinalityEnum_Enumeration.ManyToOne;
                        break;
                }
            }

            // Set the cardinality
            reportColumn.Cardinality = cardinalityEnum.Value.ToString();
        }

        /// <summary>
        /// Populates the relationship pickers.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="type">The type.</param>
        /// <param name="isNameColumnForType">if set to <c>true</c> the type if for the name column for the type.</param>
        private void PopulateRelationshipPickers(Model.EntityType entityType, string type, bool isNameColumnForType)
        {
            PopulateRelationshipPickers(entityType, null, type, isNameColumnForType);
        }

        /// <summary>
        /// Populates the relationship pickers.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="reportColumn">The report column.</param>
        /// <param name="type">The type.</param>
        /// <param name="isNameColumnForType">if set to <c>true</c> the type if for the name column for the type.</param>
        private void PopulateRelationshipPickers(Model.EntityType entityType, ReportColumn reportColumn, string type, bool isNameColumnForType)
        {
            if (entityType == null)
            {
                return;
            }
            if (reportColumn != null)
            {
                reportColumn.TypeId = entityType.Id;
            }            

            if (isNameColumnForType)
            {
                type = "InlineRelationship";
            }

            if (type == "ChoiceRelationship" && !_choiceSelections.ContainsKey(entityType.Id))
            {
				if ( entityType.InstancesOfType != null && entityType.InstancesOfType.Count > 0 )
                {
                    _choiceSelections[entityType.Id] = new List<ChoiceItemDefinition>(entityType.InstancesOfType.OrderBy(t => t.As<Model.EnumValue>() != null ? t.As<Model.EnumValue>().EnumOrder : 0).
                                                                                                 Select(resource => BuildChoiceItemDefinition(resource.As<Model.EnumValue>())));
                }
            }
            if ((type == "InlineRelationship" || type == "StructureLevels") && !_inlineReports.ContainsKey(entityType.Id))
            {
                _inlineReports[entityType.Id] = entityType.DefaultPickerReport != null ?
                                                    entityType.DefaultPickerReport.Id : Model.Entity.Get<Model.Report>("core:templateReport").Id;
            }
        }

        private ChoiceItemDefinition BuildChoiceItemDefinition(Model.EnumValue choiceItem)
        {
            if (choiceItem == null)
                return null;

            Model.FormattingRule formattingRule = choiceItem.EnumFormattingRule;
            Model.ColorFormattingRule colorFormattingRule = formattingRule != null ? formattingRule.As<Model.ColorFormattingRule>() : null;
            Model.IconFormattingRule iconFormattingRule = formattingRule != null ? formattingRule.As<Model.IconFormattingRule>() : null;

            return new ChoiceItemDefinition
            {
                DisplayName = choiceItem.Name,
                EntityIdentifier = choiceItem.Id,
                BackgroundColor = colorFormattingRule != null && colorFormattingRule.ColorRules.Count > 0 ? colorFormattingRule.ColorRules.First().ColorRuleBackground : String.Empty,
                ForegroundColor = colorFormattingRule != null && colorFormattingRule.ColorRules.Count > 0 ? colorFormattingRule.ColorRules.First().ColorRuleForeground : String.Empty,
                ImageEntityId = iconFormattingRule != null && iconFormattingRule.IconRules.Count > 0 ? iconFormattingRule.IconRules.First().IconRuleImage.Id : default(long?)
            };
        }

        /// <summary>
        /// Populates the field constraints.
        /// </summary>
        /// <param name="reportColumn">The report column.</param>
        private static void PopulateFieldConstraints(ReportColumn reportColumn)
        {
            if (reportColumn == null || reportColumn.FieldId <= 0)
            {
                return;
            }
            Model.Field field = Model.Entity.Get<Model.Field>(reportColumn.FieldId);

            if (field == null)
            {
                return;
            }
            // General Values
            reportColumn.DefaultValue = field.DefaultValue;
            reportColumn.IsRequired = field.IsRequired ?? false;
            reportColumn.IsReadOnly = field.IsFieldReadOnly ?? false;

            // Populate based on type
            if (field.Is<Model.StringField>())
            {
                Model.StringField stringField = field.As<Model.StringField>();
                // Lengths
                reportColumn.MinimumLength = stringField.MinLength ?? 0;
                reportColumn.MaximumLength = stringField.MaxLength ?? 0;
                // Regular expression validation
                if (stringField.Pattern != null)
                {
                    reportColumn.RegularExpression = stringField.Pattern.Regex;
                    reportColumn.RegularExpressionErrorMessage = stringField.Pattern.RegexDescription;
                }
                // Allow multiline?
                reportColumn.MultiLine = stringField.AllowMultiLines ?? false;
            }
            else if (field.Is<Model.DateField>())
            {
                Model.DateField dateField = field.As<Model.DateField>();
                reportColumn.MinimumDate = dateField.MinDate;
                reportColumn.MaximumDate = dateField.MaxDate;
            }
            else if (field.Is<Model.DateTimeField>())
            {
                Model.DateTimeField dateTimeField = field.As<Model.DateTimeField>();
                reportColumn.MinimumDate = dateTimeField.MinDateTime;
                reportColumn.MaximumDate = dateTimeField.MaxDateTime;
            }
            else if (field.Is<Model.CurrencyField>())
            {
                Model.CurrencyField currencyField = field.As<Model.CurrencyField>();
                reportColumn.DecimalPlaces = currencyField.DecimalPlaces ?? 2;
            }
            else if (field.Is<Model.DecimalField>())
            {
                Model.DecimalField decimalField = field.As<Model.DecimalField>();
                reportColumn.MinimumDecimal = decimalField.MinDecimal ?? 2;
                reportColumn.MaximumDecimal = decimalField.MaxDecimal ?? 0;
                // Places
                reportColumn.DecimalPlaces = decimalField.DecimalPlaces ?? 3; // Default to 3 decimal places if not specified
            }
            else if (field.Is<Model.AutoNumberField>())
            {
                Model.AutoNumberField autoNumberField = field.As<Model.AutoNumberField>();
                reportColumn.AutoNumberDisplayPattern = autoNumberField.AutoNumberDisplayPattern;
            }
            //else if (field.Is<Model.IntField>())
            //{
            //    Model.IntField intField = field.As<Model.IntField>();
            //}
            else if (field.Is<Model.TimeField>())
            {
                Model.TimeField timeField = field.As<Model.TimeField>();
                reportColumn.MinimumDate = timeField.MinTime;
                reportColumn.MaximumDate = timeField.MaxTime;
            }
            else if (field.Is<Model.ImageFileType>())
            {
                reportColumn.Type = "Image";
            }
        }


        #region Legacy rubbish lifted from DbDataTableHelper (Needs to be completely removed at some point)

        /// <summary>
        /// Gets the type of the column database.
        /// </summary>
        /// <param name="selectColumn">The select column.</param>
        /// <returns>DatabaseType.</returns>
        private string GetColumnDatabaseType(SelectColumn selectColumn)
        {
            string columnType = "Unknown";
            
            ResourceDataColumn field = selectColumn.Expression as ResourceDataColumn;
            if (field != null)
            {
                ResourceDataColumn dataField = field;
                if (dataField.CastType != null && dataField.CastType.GetDisplayName() != DatabaseType.UnknownType.GetDisplayName())
                    columnType = dataField.CastType.GetDisplayName();
                else
                    columnType = GetFieldDataType(dataField.FieldId);
            }
            else if (selectColumn.Expression is StructureViewExpression)
            {
                columnType = DatabaseType.StructureLevelsType.GetDisplayName();
            }
            else
            {
                CalculationExpression calculationExpression = selectColumn.Expression as CalculationExpression;
                if (calculationExpression != null)
                {
                    if (calculationExpression.DisplayType != null && calculationExpression.DisplayType != DatabaseType.UnknownType)
                        return calculationExpression.DisplayType.GetDisplayName();

                }
                else
                {
                    ColumnReference reference = selectColumn.Expression as ColumnReference;
                    if (reference != null)
                    {
                        ColumnReference columnReference = reference;
                        // Sanity check. Prevent infinite recursion
                        if (columnReference.ColumnId != selectColumn.ColumnId)
                        {
                            SelectColumn referencedColumn = _query.SelectColumns.FirstOrDefault(c => c.ColumnId == columnReference.ColumnId);
                            columnType = GetColumnDatabaseType(referencedColumn);
                        }
                    }
                    else
                    {
                        AggregateExpression aggregateExpression = selectColumn.Expression as AggregateExpression;
                        if (aggregateExpression != null)
                        {
                            ResourceDataColumn expressionDataColumn = aggregateExpression.Expression as ResourceDataColumn;
                            if (expressionDataColumn != null)
                            {
                                if (expressionDataColumn.CastType != null)
                                {
                                    return expressionDataColumn.CastType.GetDisplayName();
                                }
                                // Look up the query to see if this expression node is a related resource and get the entity type id for this column
                                Entity relatedEntity = getRelatedEntityByNodeId(_query.RootEntity, aggregateExpression.NodeId);                                                                                                         
                                AggregateEntity aggregateEntity = relatedEntity as AggregateEntity;
                                if (aggregateEntity != null && aggregateEntity.GroupedEntity != null)
                                {
                                    RelatedResource relatedResource = aggregateEntity.GroupedEntity as RelatedResource;
                                    // Get the type of this entity
                                    if (relatedResource != null)
                                    {
                                        Model.EntityType type = Model.Entity.Get<Model.EntityType>(relatedResource.EntityTypeId);
                                        Model.EntityRef enumType = new Model.EntityRef("core", "enumValue");
                                        var ancestorsAndSelf = Model.EntityTypeHelper.GetAncestorsAndSelf(type);
                                        if (ancestorsAndSelf.FirstOrDefault(a => a.Id == enumType.Id) != null)
                                        {
                                            return DatabaseType.ChoiceRelationshipType.GetDisplayName();
                                        }
                                    }
                                }
                            }
                            return DatabaseType.StringType.GetDisplayName();
                        }
                        if (selectColumn.Expression is IdExpression)
                        {
                            columnType = DatabaseType.IdentifierType.GetDisplayName();
                        }
                        else if (selectColumn.Expression is ScriptExpression)
                        {
                            ScriptExpression scriptExpression = selectColumn.Expression as ScriptExpression;
                            columnType = scriptExpression.ResultType.GetDisplayName();
                        }
                    }
                }
            }

            return columnType;
        }


        /// <summary>
        /// Gets the type of the field data.
        /// </summary>
        /// <param name="fieldEntityRef">The field entity preference.</param>
        /// <returns>DatabaseType.</returns>
        private static string GetFieldDataType(Model.EntityRef fieldEntityRef)
        {
            string type = "Unknown";

            if (fieldEntityRef != null)
            {
                try
                {
                    Model.Field field = Model.Entity.Get<Model.Field>(fieldEntityRef);
                    Model.FieldType fieldType = EDC.ReadiNow.Model.FieldExtensionMethods.GetFieldType(field);
                    string readiNowType = string.Format("EDC.Database.Types.{0}, {1}", fieldType.ReadiNowType, typeof(DatabaseType).Assembly.FullName);
                    Type ourType = Type.GetType(readiNowType);
                    if (ourType == null)
                    {
                        return type;
                    }
                    DatabaseType ourDatabaseType = Activator.CreateInstance(ourType) as DatabaseType;
                    if (ourDatabaseType != null)
                    {
                        type = ourDatabaseType.GetDisplayName();
                    }
                }
                catch
                {
                    type = "Unknown";
                }
            }

            return type;
        }

        #endregion Legacy rubbish lifted from DbDataTableHelper (Needs to be completely removed at some point and have them use the 'ExtendedProperties' property)

        #endregion Report Columns

        #region Column Sort Order
        /// <summary>
        /// Sorts the order for report.
        /// </summary>
        /// <returns>List{ReportSortOrder}.</returns>
        private List<ReportSortOrder> SortOrderForReport()
        {
            List<ReportSortOrder> ordering = null;
            List<OrderByItem> orderByItems = _query.OrderBy.Where(item => item.Expression is ColumnReference)
                .Where(cr => ((ColumnReference)cr.Expression).ColumnId != Guid.Empty)
                .Where(crc => _query.SelectColumns.Any(sc => sc.ColumnId == ((ColumnReference)crc.Expression).ColumnId)).ToList();
			if ( orderByItems.Count > 0 )
            {
                ordering = new List<ReportSortOrder>(orderByItems.Select(orderByItem => new ReportSortOrder 
                {
                    ColumnId = _query.SelectColumns.First(rc => rc.ColumnId == ((ColumnReference)orderByItem.Expression).ColumnId).EntityId.ToString(CultureInfo.InvariantCulture), 
                    Order = orderByItem.Direction.ToString() 
                }));
            }
            return ordering;
        }

        #endregion

        #region Report Data (Grid) row population

        /// <summary>
        /// Builds the grid data.
        /// </summary>
        /// <returns>List{DataRow}.</returns>
        private List<DataRow> BuildGridData()
        {
            if ( _gridDataBuilt )
            {
                return _gridData;
            }

            _gridData = new List<DataRow>();
            if ( _queryResult == null || _queryResult.DataTable == null )
            {
                _gridDataBuilt = true;
                return _gridData;
            }
                
            bool hasIdColumn = _queryResult.Columns[0].IsHidden;
            int firstColumn = hasIdColumn ? 1 : 0; // skip ID column
            int maxColumnCount = _queryResult.Columns.Count - firstColumn;
            int availableColumnCount = maxColumnCount;
            if (_settings.ColumnCount.HasValue && _settings.ColumnCount < maxColumnCount)
            {
                maxColumnCount = _settings.ColumnCount.Value;
            }                    

            for (int rowIndex = 0; rowIndex < _queryResult.DataTable.Rows.Count; rowIndex++)
            {
                if (_queryResult.DataTable.Rows[rowIndex].ItemArray.Length <= 0)
                {
                    continue;
                }
                System.Data.DataRow row = _queryResult.DataTable.Rows[rowIndex];

                object cell0 = row[ 0 ];
                long rowEntityId = hasIdColumn && cell0 != null && cell0 != DBNull.Value ? (long) cell0 : 0;
                DataRow gridRow = new DataRow { EntityId = rowEntityId, Values = new List<CellValue>() };

                // If this is an image based report, inject the first column with a bogus value (that points to the image type)
                if (_isImageRootReport)
                {
                    // Populate dummy column data for the image
                    Model.ImageFileType imageFileType = Model.Entity.Get<Model.ImageFileType>(rowEntityId);
                    string imageInfo = string.IsNullOrEmpty(imageFileType.ImageBackgroundColor) ? imageFileType.Name : string.Format("{0}#{1}", imageFileType.Name, imageFileType.ImageBackgroundColor);

                    CellValue imageCellValue = new CellValue { Values = new Dictionary<long, string> { { imageFileType.Id, imageInfo } } };
                    gridRow.Values.Add(imageCellValue);
                }

                int addedColumnsCount = 0;
                        
                for (int columnIndex = firstColumn; columnIndex < firstColumn + availableColumnCount; columnIndex++)
                {
                    ResultColumn column = _queryResult.Columns[columnIndex];

                    bool isGroupByColumn = _clientAggregate != null &&
                                            _clientAggregate.GroupedColumns != null &&
                                            _clientAggregate.GroupedColumns.Any(gc => gc.ReportColumnEntityId == column.RequestColumn.EntityId);

                    if (addedColumnsCount < maxColumnCount || isGroupByColumn)
                    {
                        CellValue cellValue = BuildGridCellData(row, rowEntityId, columnIndex, column);
                        gridRow.Values.Add(cellValue);

                        // Don't include grouped columns in the count of columns to add.
                        // These are needed for the report to render correctly.
                        if (!isGroupByColumn)
                        {
                            addedColumnsCount++;   
                        }                                
                    }
                }
                _gridData.Add(gridRow);
            }
            _gridDataBuilt = true;
            return _gridData;
        }

        private CellValue BuildGridCellData( System.Data.DataRow row, long rowEntityId, int columnIndex, ResultColumn column )
        {
            CellValue cellValue;

            object value = row.ItemArray [ columnIndex ];
            string columnIdIndex = column.RequestColumn.EntityId.ToString( CultureInfo.InvariantCulture );                        

            if ( column.IsRelatedResource || column.ColumnType is InlineRelationshipType || column.ColumnType is ChoiceRelationshipType )
            {
                string columnIdString = columnIdIndex.ToString(CultureInfo.InvariantCulture);
                long? formatIndex = ConditionalFormatIndex(columnIdIndex, value);

                //if current column type is choice relationship and with default conditional format is set.  add format index from default condition formatting.
                if (column.ColumnType is ChoiceRelationshipType &&
                    ValueFormatRules != null &&
                    (
                        !ValueFormatRules.ContainsKey(columnIdString) ||
                        (
                            ValueFormatRules.ContainsKey(columnIdString) &&
                            ValueFormatRules[columnIdString] != null &&
                            !ValueFormatRules[columnIdString].DisableDefaultFormat)
                        )
                    )

                  
                {
                    string choiceFieldId = column.ResourceTypeId.ToString();

                    long? defaultFormatIndex = DefaultConditionalFormatIndex(choiceFieldId, value);

                    if (defaultFormatIndex != null)
                    {
                        formatIndex = defaultFormatIndex;                      
                    }                    
                }


                cellValue = new CellValue
                    {
                        Values = value != null ? PopulateRelatedResourceItems(value.ToString(), column.RequestColumn) : null,
                        ConditionalFormatIndex = formatIndex
                };
            }
            else
            {
                cellValue = new CellValue
                    {
                        Value = value != null ? PopulateSimpleValue( value, column.ColumnType ) : null,
                        ConditionalFormatIndex = ConditionalFormatIndex(
                            columnIdIndex,                            
                            GetConditionalFormatData( IsResultColumnEntityNameColumn( column ),
                            rowEntityId, value ) )
                    };
            }
            return cellValue;
        }


        /// <summary>
        /// Determines whether the specified result column returns the name of the root entity.
        /// </summary>
        /// <param name="resultColumn">The result column.</param>
        /// <returns></returns>
        internal bool IsResultColumnEntityNameColumn(ResultColumn resultColumn)
        {
            if (resultColumn == null || resultColumn.RequestColumn == null) return false;                        

            var resourceDataColumn = resultColumn.RequestColumn.Expression as ResourceDataColumn;

            return _rootEntity != null && 
                   resourceDataColumn != null &&
                   resourceDataColumn.FieldId != null &&
                   resourceDataColumn.FieldId.HasId &&                
                   _query.RootEntity.NodeId == resourceDataColumn.NodeId && 
                   resourceDataColumn.FieldId.Id == NameFieldId;
        }

        /// <summary>
        /// Gets the conditional format data.
        /// </summary>
        /// <param name="isEntityNameColumn">if set to <c>true</c> [is entity name column].</param>
        /// <param name="rowEntityId">The row entity identifier.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private object GetConditionalFormatData(bool isEntityNameColumn, long rowEntityId, object data)
        {
            return isEntityNameColumn ? ConvertToEntityXml(rowEntityId, data != null ? data.ToString() : null) : data;
        }


        /// <summary>
        /// Converts to entity XML.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        private string ConvertToEntityXml(long id, string text)
        {
            return string.Concat( @"<e id=""", id.ToString(CultureInfo.InvariantCulture), @""" text=""", text, @"""/>" );
        }


        /// <summary>
        /// Conditionals the index of the format.
        /// </summary>
        /// <param name="columnIndex">Index of the column.</param>
        /// <param name="data">The data.</param>
        /// <returns>System.Nullable{System.Int64}.</returns>
        private long? ConditionalFormatIndex(string columnIndex, object data)
        {
            if (ConditionalFormatRules != null && ConditionalFormatRules.ContainsKey(columnIndex))
            {
                long conditionIndex;
                if (_conditionalFormatter.TryGetRule(columnIndex, data, out conditionIndex))
                {
                    return conditionIndex;
                }
            }
            return null;
        }

        /// <summary>
        /// Default Conditionals the index of the format.
        /// </summary>
        /// <param name="choiceValueId">choice Value Id.</param>
        /// <param name="data">The data.</param>
        /// <returns>System.Nullable{System.Int64}.</returns>
        private long? DefaultConditionalFormatIndex(string choiceValueId, object data)
        {
            if (DefaultConditionalFormatRules != null && DefaultConditionalFormatRules.ContainsKey(choiceValueId))
            {
                long conditionIndex;               

                if (_conditionalFormatter.TryGetDefaultRule(DefaultConditionalFormatRules[choiceValueId], data, out conditionIndex))
                {
                    return conditionIndex;
                }
            }
            return null;
        }

        /// <summary>
        /// Populates the simple value based on the object passed in.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="columnType">Type of the column.</param>
        /// <returns>System.String.</returns>
        /// <remarks>This method handles and special formatting prior to the data being returned.</remarks>
        private static string PopulateSimpleValue(object value, DatabaseType columnType)
        {
            if (value != null)
            {
                if (columnType is DateType)
                {
                    DateTime? dateValue = value as DateTime?;
                    return dateValue.HasValue ? dateValue.Value.ToString("yyyy-MM-dd") : null;
                }
                if ((columnType is DateTimeType) || (columnType is TimeType))
                {
                    DateTime? dateTimeValue = value as DateTime?;
                    return dateTimeValue.HasValue ? dateTimeValue.Value.ToString("yyyy-MM-ddTHH:mm:ssZ") : null;
                }                
                return value.ToString();
            }
            return null;
        }

        /// <summary>
        /// Returns true if the column is for an aggregate structure view.
        /// </summary>
        /// <param name="selectColumn"></param>
        /// <returns></returns>
        private static bool IsColumnAggregateStructureView(SelectColumn selectColumn)
        {
            bool isAggregateStructureView = false;

            if (selectColumn != null && selectColumn.Expression is AggregateExpression)
            {
                var aggregateExpression = selectColumn.Expression as AggregateExpression;

                if (aggregateExpression.Expression is StructureViewExpression)
                {
                    isAggregateStructureView = true;
                }
            }

            return isAggregateStructureView;
        }


        /// <summary>
        /// Populates the related resource items.
        /// </summary>
        /// <param name="relatedResourceXmlFragment">The related resource XML fragment.</param>
        /// <param name="selectColumn">The select column</param>
        /// <returns>Dictionary{System.Int64System.String}.</returns>
        private static Dictionary<long, string> PopulateRelatedResourceItems(string relatedResourceXmlFragment, SelectColumn selectColumn = null)
        {                        
            if (string.IsNullOrEmpty(relatedResourceXmlFragment))
                return null;

            bool isAggregateStructureView = IsColumnAggregateStructureView(selectColumn);

            Dictionary<long, string> items = new Dictionary<long, string>( );

            long idStart = 0;
            
            using ( StringReader stringReader = new StringReader( "<root>" + relatedResourceXmlFragment + "</root>" ) )
            {
                var xmlReaderSettings = new XmlReaderSettings { CheckCharacters = false };
                using (XmlReader xmlReader = XmlReader.Create(stringReader, xmlReaderSettings))
                {
                    XDocument xdoc = XDocument.Load(xmlReader);
                    IEnumerable<XElement> elements = xdoc.Root.Descendants("e");

                    foreach (var element in elements)
                    {
                        long id;

                        if (!isAggregateStructureView)
                        {
                            XAttribute idAttr = element.Attribute("id");
                            if (idAttr == null)
                                continue;

                            id = Convert.ToInt64(idAttr.Value);
                        }
                        else
                        {
                            // Aggregate structure views have no ids defined
                            // put invalid ids, just in case.
                            id = --idStart;
                        }                        

                        XAttribute textAttr = element.Attribute("text");
                        string text = textAttr == null ? "" : textAttr.Value ?? "";

                        items[id] = text;
                    }
                }
            }

            return items;
        }

        #endregion Report Data (Grid) row population

        #region Grouping and Rollup Aggregates

        private ReportMetadataAggregate BuildAggregateMetadata()
        {
            _aggregateMetadataBuilt = true;
            // Only return _if_ we have either groups _or_ aggregates against any column
            if (_clientAggregate == null)
            {
                _aggregateMetadata = null;
                return _aggregateMetadata;
            }
            bool showGrandTotals;
            bool showSubTotals;
            bool showCount;
            bool showLabel;
            bool showOptionLabel;
			if ( _clientAggregate.GroupedColumns != null && _clientAggregate.GroupedColumns.Count > 0 )
            {
                var firstGroupedCol = _clientAggregate.GroupedColumns.First();
                showGrandTotals = firstGroupedCol.ShowGrandTotals;
                showSubTotals = firstGroupedCol.ShowSubTotals;
                showCount = firstGroupedCol.ShowRowCounts;
                showLabel = firstGroupedCol.ShowRowLabels;
                showOptionLabel = firstGroupedCol.ShowOptionLabel;
                
            }
			else if ( _clientAggregate.AggregatedColumns != null && _clientAggregate.AggregatedColumns.Count > 0 )
            {
                var firstAggCol = _clientAggregate.AggregatedColumns.First();
                showGrandTotals = firstAggCol.ShowGrandTotals;
                showSubTotals = firstAggCol.ShowSubTotals;
                showCount = firstAggCol.ShowRowCounts;
                showLabel = firstAggCol.ShowRowLabels;
                showOptionLabel = firstAggCol.ShowOptionLabel;
            }
            else
            {
                showGrandTotals = _report.RollupGrandTotals ?? false;
                showSubTotals = _report.RollupSubTotals ?? false;
                showCount = _report.RollupRowCounts ?? false;
                showLabel = _report.RollupRowLabels ?? false;
                showOptionLabel = _report.RollupOptionLabels ?? false;
            }
            _aggregateMetadata = new ReportMetadataAggregate
            {
                IncludeRollup = true,
                ShowGrandTotals = showGrandTotals,
                ShowSubTotals = showSubTotals,
                ShowCount = showCount,
                ShowGroupLabel = showLabel,
                ShowOptionLabel = showOptionLabel
            };

            // Build the groups with their labels only if they exist and we are showing the label.
			if ( _clientAggregate.GroupedColumns != null && _clientAggregate.GroupedColumns.Count > 0 )
            {
                _aggregateMetadata.Groups = new List<Dictionary<long, GroupingDetail>>(_clientAggregate.GroupedColumns.Count);
                foreach (ReportGroupField rowGroup in _clientAggregate.GroupedColumns.OrderBy(rg => rg.Order))
                {
                    _aggregateMetadata.Groups.Add(new Dictionary<long, GroupingDetail>{
                        {
                            rowGroup.ReportColumnEntityId,
                            new GroupingDetail
                                {
                                    Style = GroupNameFromMethod(rowGroup.GroupMethod),
                                    Value = _report.ReportColumns.First(rc => rc.Id == rowGroup.ReportColumnEntityId).Name,
                                    Collapsed = rowGroup.Collapsed
                                }
                        }});
                }
            }

            // Build up the aggregate column definitions if any.
			if ( _clientAggregate.AggregatedColumns != null && _clientAggregate.AggregatedColumns.Count > 0 )
            {
                _aggregateMetadata.Aggregates = new Dictionary<long, List<AggregateDetail>>(_clientAggregate.AggregatedColumns.Count);
                foreach (SelectColumn column in _query.SelectColumns)
                {
                    SelectColumn selectColumn = column; // Prevent closure getting us.
                    List<AggregateDetail> details = _clientAggregate.AggregatedColumns
                        .Where(ac => ac.ReportColumnId == selectColumn.ColumnId && !ac.IncludedCount)
                        .Select(result => new AggregateDetail
                        {
                            Style = AggretateNameFromMethod(result.AggregateMethod),
                            Type = AggregateTypeForColumnMethod(column, result.AggregateMethod)
                        }).ToList();
					if ( details.Count > 0 )
                    {
                        _aggregateMetadata.Aggregates[selectColumn.EntityId] = details;
                    }
                }
                // Include metadata for 'count', which is not associated with a column.
                if ( _clientAggregate.AggregatedColumns.Any( ac => ac.IncludedCount ) )
                {
                    List<AggregateDetail> details = new List<AggregateDetail>( );
                    details.Add( new AggregateDetail
                    {
                        Style = AggretateNameFromMethod( AggregateMethod.Count ),
                        Type = AggregateTypeForColumnMethod( null, AggregateMethod.Count )
                    } );
                    _aggregateMetadata.Aggregates[ 0 ] = details;
                }
            }
            return _aggregateMetadata;
        }

        private string AggregateTypeForColumnMethod(SelectColumn column, AggregateMethod aggregateMethod)
        {
            switch (aggregateMethod)
            {
                case AggregateMethod.Count:
                case AggregateMethod.CountUniqueItems:
                case AggregateMethod.CountUniqueNotBlanks:
                case AggregateMethod.CountWithValues:
                    return DatabaseType.Int32Type.GetDisplayName();
            }
            
            string result = _queryResult.Columns.First(qc => qc.RequestColumn.ColumnId == column.ColumnId).ColumnType.GetDisplayName();

            if (aggregateMethod == AggregateMethod.Average)
            {
                if (result == DatabaseType.CurrencyType.GetDisplayName())
                    return result;
                return DatabaseType.DecimalType.GetDisplayName();
            }

            return result;
        }

        private static string GroupNameFromMethod(GroupMethod groupMethod)
        {
            string alias = "group" + groupMethod;
            Model.GroupingMethodEnum enumEntity = Model.Entity.Get<Model.GroupingMethodEnum>(new Model.EntityRef(alias));
            return enumEntity.Alias.Split(':').Last();
        }

        private static string AggretateNameFromMethod(AggregateMethod aggregateMethod)
        {
            string alias = "agg" + aggregateMethod;
            Model.AggregateMethodEnum enumEntity = Model.Entity.Get<Model.AggregateMethodEnum>(new Model.EntityRef(alias));
            return enumEntity.Alias.Split(':').Last();
        }
        
        private List<ReportDataAggregate> BuildAggregateData()
        {
            if (!_aggregateDataBuilt)
            {
                if (_clientAggregate != null && _rollupResult != null)
                {
                    _aggregateData = new List<ReportDataAggregate>();
                    // Populate only if we have grouping
					if ( _clientAggregate.GroupedColumns.Count > 0 )
                    {
                        foreach (System.Data.DataRow row in _rollupResult.AggregateDataTable.Rows)
                        {
                            _aggregateData.Add(new ReportDataAggregate
                                {
                                    GroupBitmap = Convert.ToInt64(row["GroupingBitmap"]),
                                    Total = TotalForGroup(row),
                                    GroupHeadings = GroupHeadingsForRow(row),
                                    Aggregates = AggregatesForRow(row)
                                });
                        }
                    }
                    else if ( _clientAggregate.AggregatedColumns != null )
                    {
                        bool getCount = _clientAggregate.AggregatedColumns.Any( ac => ac.IncludedCount );

                        // Cater for a single line of totals at the top of the page if we have totals specified
                        if ( _clientAggregate.AggregatedColumns.Count > 0
                            && _rollupResult.AggregateDataTable.Rows.Count > 0)
                        {
                            _aggregateData.Add(new ReportDataAggregate
                            {
                                GroupBitmap = 0,
                                Total = getCount ? TotalForGroup( _rollupResult.AggregateDataTable.Rows [ 0 ] ) : null,
                                Aggregates = AggregatesForRow(_rollupResult.AggregateDataTable.Rows[0])
                            });
                        }
                    }
                }
                _aggregateDataBuilt = true;
            }
            return _aggregateData;
        }

        private long? TotalForGroup(System.Data.DataRow row)
        {
            // Extract the index of the count column and add the grouped column count
            int index = _clientAggregate.AggregatedColumns.TakeWhile(aggregatedColumn => aggregatedColumn.AggregateMethod != AggregateMethod.Count).Count() + _clientAggregate.GroupedColumns.Count;
            // Attempt to convert the total element
            long total;
            if (long.TryParse(row[index].ToString(), out total))
            {
                return total;
            }
            // Cannot parse so return null
            return null;
        }

        private Dictionary<long, List<AggregateItem>> AggregatesForRow(System.Data.DataRow row)
        {
            // Get the metadata for the aggregates
            ReportMetadataAggregate aggregateMetadata = AggregateMetadata;
            // Move the offset past the grouped headers
            int offset = _clientAggregate.GroupedColumns.Count;
            int aggregateDetailIndex = -1;
            long lastColumnEntityId = -1;
            Dictionary<long, List<AggregateItem>> aggregates = new Dictionary<long, List<AggregateItem>>();
            foreach ( long columnEntityId in from aggregatedColumn in _clientAggregate.AggregatedColumns
                                             where !aggregatedColumn.IncludedCount && aggregatedColumn.ReportColumnId != Guid.Empty
                                             select _query.SelectColumns.First( sc => sc.ColumnId == aggregatedColumn.ReportColumnId ).EntityId )
            {
                //reset aggregateDetail Index value when the aggregates belone new column
                if (columnEntityId != lastColumnEntityId)
                {
                    aggregateDetailIndex = -1;
                    lastColumnEntityId = columnEntityId;
                }

                List<AggregateItem> itemList;
                if (!aggregates.TryGetValue(columnEntityId, out itemList))
                {
                    itemList = new List<AggregateItem>();
                    aggregates[columnEntityId] = itemList;
                }
                List<AggregateDetail> aggregateDetails;

				if ( !aggregateMetadata.Aggregates.TryGetValue( columnEntityId, out aggregateDetails ) || aggregateDetails.Count <= 0 )
                {
                    continue;
                }

                int offsetCount = offset++;
                aggregateDetailIndex ++;
                DatabaseType aggregateColumnType = DatabaseTypeHelper.ConvertFromDisplayName(aggregateDetails.ToArray()[aggregateDetailIndex].Type);
                object rowValue = row[offsetCount];
                

                AggregateItem item =   (aggregateColumnType is InlineRelationshipType || aggregateColumnType is ChoiceRelationshipType) ?
                                        new AggregateItem { Values = PopulateRelatedResourceItems(rowValue.ToString()) } :
                                        new AggregateItem { Value = PopulateSimpleValue(rowValue, aggregateColumnType) };
                
                itemList.Add(item);
            }
            return aggregates;
        }

        private List<Dictionary<long, CellValue>> GroupHeadingsForRow(System.Data.DataRow row)
        {
            int columnIndex = 0;
            return _clientAggregate.GroupedColumns.Select(groupedColumn => new Dictionary<long, CellValue>
                {
                    {
                        _query.SelectColumns.First(sc => sc.ColumnId == groupedColumn.ReportColumnId).EntityId, 
                        ValueForGroupElement(row[columnIndex++], groupedColumn.ReportColumnId, _query.SelectColumns.First(sc => sc.ColumnId == groupedColumn.ReportColumnId).EntityId)
                    }
                }).ToList();
        }

        private CellValue ValueForGroupElement(object element, Guid reportColumn, long columnId)
        {
            ResultColumn reportResultColumn = _queryResult.Columns.First(rc => rc.RequestColumn.ColumnId == reportColumn);
            if (reportResultColumn.IsRelatedResource || reportResultColumn.ColumnType is InlineRelationshipType || reportResultColumn.ColumnType is ChoiceRelationshipType)
            {                
                return new CellValue
                    {
                        Values = PopulateRelatedResourceItems(element.ToString(), reportResultColumn.RequestColumn),
                        ConditionalFormatIndex = ConditionalFormatIndex(columnId.ToString(CultureInfo.InvariantCulture), element)
                };
            }
            return new CellValue { Value = PopulateSimpleValue(element, reportResultColumn.ColumnType), 
                                   ConditionalFormatIndex = ConditionalFormatIndex(columnId.ToString(CultureInfo.InvariantCulture), element) };
        }

        #endregion Grouping and Rollup Aggregates
    }
}

