// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata.Query.Structured;
using EDC.ReadiNow.Model;
using System.Collections.Generic;
using System.Linq;
using EDC.Security;
using Entity = EDC.ReadiNow.Model.Entity;

namespace EDC.SoftwarePlatform.Install.Common
{
    public class ReportMigration
    {
        /// <summary>
        /// Converts any reports that have query XML and no report root node. Upon completion of the conversion, the query XML and related fields are removed.
        /// </summary>
        public static void ConvertReports()
        {
            
        }
    }
}
