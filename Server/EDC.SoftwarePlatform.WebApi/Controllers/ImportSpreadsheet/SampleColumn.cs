// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ImportSpreadsheet
{
    [DataContract]
    public class SampleColumn
    {
        /// <summary>
        /// Title from heading row of data.
        /// </summary>
        [DataMember( Name = "colName" )]
        public string Name { get; set; }

        /// <summary>
        /// Column reference. E.g. Excel column reference.
        /// </summary>
        [DataMember( Name = "colId" )]
        public string ColumnName { get; set; }
    }
}