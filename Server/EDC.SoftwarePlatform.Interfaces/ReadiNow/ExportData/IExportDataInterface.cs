// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.ExportData
{
    /// <summary>
    /// Interface for a report data exporter
    /// </summary>
    public interface IExportDataInterface
    {
        ExportInfo ExportData(long reportId, ExportSettings settings);

    }

    /// <summary>
    /// Settings to be configured for exporting a report
    /// </summary>
    public class ExportSettings
    {
        public string TimeZone;
        public ExportFormat  Format;
    }

    /// <summary>
    /// Defines the Export Format type.
    /// </summary>
    public enum ExportFormat
    {
        /// <summary>
        ///     The Excel format type.
        /// </summary>
        Excel,

        /// <summary>
        ///     The CSV format type.
        /// </summary>
        Csv,

        /// <summary>
        ///     The Word format type.
        /// </summary>
        Word
    }

    /// <summary>
    ///     ExportInfo.
    /// </summary>
    public class ExportInfo
    {
        public string FileHash
        {
            get;
            set;
        }

        public string ResponseMessage
        {
            get;
            set;
        }
    }
}
