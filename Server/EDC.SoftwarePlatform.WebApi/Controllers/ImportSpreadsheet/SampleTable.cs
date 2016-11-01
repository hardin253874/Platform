// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ImportSpreadsheet
{
    [DataContract]
    public class SampleTable
    {
        /// <summary>
        /// Table columns
        /// </summary>
        [DataMember( Name = "columns")]
        public List<SampleColumn> Columns { get; set; }

        /// <summary>
        /// Data rows.
        /// </summary>
        [DataMember( Name = "rows" )]
        public List<SampleRow> Rows { get; set; }
    }
}