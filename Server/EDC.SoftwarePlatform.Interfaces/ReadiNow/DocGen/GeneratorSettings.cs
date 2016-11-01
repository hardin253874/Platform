// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;

namespace ReadiNow.DocGen
{
    /// <summary>
    /// Settings that are passed into the document generator.
    /// </summary>
    public class GeneratorSettings
    {
        /// <summary>
        /// Optional. Receives any notifications about progress.
        /// </summary>
        public Action<string> CurrentActivityCallback { get; set; }

        /// <summary>
        /// Should the diagnostic files be written?
        /// </summary>
        public bool WriteDebugFiles { get; set; }

        /// <summary>
        /// Optional. The ID of a top-level resource to start generating content for.
        /// </summary>
        public long SelectedResourceId { get; set; }

        /// <summary>
        /// The name of the timezone to use in any date-time offsets required as part of this document generation.
        /// </summary>
        public string TimeZoneName { get; set; }

        /// <summary>
        /// If true Generation will throw an error on the first error encountered.
        /// </summary>
        public bool ThrowOnError { get; set; }
    }
}
