// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;
using EDC.ReadiNow.Metadata.Reporting;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System;
using ProtoBuf;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlRoot("ClientAggregate", Namespace = Constants.StructuredQueryNamespace)]
    public class ClientAggregate
    {

        public ClientAggregate(Report report, StructuredQuery query)
        {
            GroupedColumns = ClientAggregateHelper.GroupedColumnsForReport(report, query);
            AggregatedColumns = ClientAggregateHelper.AggregatedColumnsForReport(report, query);
        }

        public ClientAggregate()
        {
            // See also: OnDeserialization
            GroupedColumns = new List<ReportGroupField>();
            AggregatedColumns = new List<ReportAggregateField>();
        }

        #region Public Properties
        [DataMember(Order = 1)]
        [XmlArray("GroupedColumns")]
        public List<ReportGroupField> GroupedColumns { get; set; }

        [DataMember(Order = 2)]
        [XmlArray("AggregatedColumns")]
        public List<ReportAggregateField> AggregatedColumns { get; set; }

        [DataMember(Order = 3)]
        public bool IncludeRollup { get; set; }       
        #endregion


        /// <summary>
        /// Creates a unique string that represents the query, for use in cache key.
        /// </summary>
        /// <returns></returns>
        public string CacheKeyToken( )
        {
            using ( Profiler.Measure( "StructureQuery.CacheKeyToken" ) )
            {
                // Utilise the datacontract serializer to create a copy of the structured query                
	            byte[ ] bytes = Serialize( );
               
                using (HashAlgorithm hasher = new MD5Cng())
                {
                    byte[] hash = hasher.ComputeHash(bytes);
                    string base64 = Convert.ToBase64String(hash);
                    return base64;
                }

            }
        }

		/// <summary>
		/// Serializes this instance.
		/// </summary>
		/// <returns></returns>
	    public byte[ ] Serialize( )
	    {
			byte [ ] bytes;
			using ( var memoryStream = new MemoryStream( ) )
			{
				// Serialize the query                    
				serializer.Serialize( memoryStream, this );
				memoryStream.Flush( );
				bytes = memoryStream.ToArray( );
			}

			return bytes;
	    }

	    static ProtoBuf.Meta.RuntimeTypeModel serializer = StructuredQueryHelper.CreateSerializer();      
    }

}
