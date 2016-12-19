// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace ReadiNow.ImportExport
{
    /// <summary>
    ///     Settings class for importing entities.
    /// </summary>
    public class EntityXmlImportSettings
    {
        /// <summary>
        /// Default settings
        /// </summary>
        public static readonly EntityXmlImportSettings Default = new EntityXmlImportSettings
        {
            IgnoreMissingDependencies = false
        };

        /// <summary>
        /// If true, an import will proceed even if there are missing dependencies.
        /// </summary>
        public bool IgnoreMissingDependencies { get; set; }
    }
}
