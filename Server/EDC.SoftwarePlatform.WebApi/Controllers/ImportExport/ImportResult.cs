// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ImportExport
{
    /// <summary>
    /// Represents results from an XML import.
    /// </summary>
    [DataContract]
    public class ImportResult
    {
        /// <summary>
        /// If true, the import succeeded.
        /// </summary>
        [DataMember( Name = "success" )]
        public bool Success { get; set; }

        /// <summary>
        /// Error messages.
        /// </summary>
        [DataMember( Name = "message" )]
        public string Message { get; set; }

        /// <summary>
        /// List of root level entities imported.
        /// </summary>
        [DataMember(Name = "entities" )]
        public List<ImportResultEntry> Entities { get; set; }
    }

    /// <summary>
    /// Represents a root level entity imported.
    /// </summary>
    [DataContract]
    public class ImportResultEntry
    {
        /// <summary>
        /// Name of the entity.
        /// </summary>
        [DataMember( Name = "name" )]
        public string Name { get; set; }

        /// <summary>
        /// Type name of the entity.
        /// </summary>
        [DataMember( Name = "typeName" )]
        public string TypeName { get; set; }
    }

}