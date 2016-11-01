// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.SoftwarePlatform.WebApi.Controllers.ImportSpreadsheet
{
    [DataContract]
    public class SampleRow
    {
        /// <summary>
        /// Cell values
        /// </summary>
        [DataMember( Name = "vals")]
        public List<string> Values { get; set; }
    }
}