// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;
using EDC.Database;
using EDC.Database.Types;
using EDC.ReadiNow.Annotations;
using EDC.ReadiNow.Metadata.Query.Structured.Helpers;
using EDC.ReadiNow.Model;
using System.Diagnostics;
using EDC.ReadiNow.Diagnostics;
using EntityRef = EDC.ReadiNow.Common.ConfigParser.Containers.EntityRef;
using System.Security.Cryptography;
using ProtoBuf;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    // type: report
    [DataContract(Namespace = Constants.DataContractNamespace)]    
    [XmlRoot("Query", Namespace = Constants.StructuredQueryNamespace)]
    public class StructuredQuery : IDeserializationCallback
    {        
        /// <summary>
        /// Create new structured query.
        /// </summary>
        public StructuredQuery() : this(true)
        {
        }

        /// <summary>
        /// Create new structured query.
        /// </summary>
        public StructuredQuery( bool initializeCollections )
        {
            if ( initializeCollections )
            {
                // See also: OnDeserialization
                SelectColumns = new List<SelectColumn>( );
                OrderBy = new List<OrderByItem>( );
                Conditions = new List<QueryCondition>( );
                InvalidReportInformation = new Dictionary<string, Dictionary<long, string>>( );
            }
        }

        // rel: rootNode
        [DataMember(Order=1)]
        public Entity RootEntity { get; set; }

        // rel: reportColumns
        [DataMember(Order = 2)]
        [XmlArray("Columns")]
        public List<SelectColumn> SelectColumns { get; set; }

        // rel: hasConditions
        [DataMember(Order = 3)]
        [XmlArray("Conditions")]
        [XmlArrayItem("Condition")]
        public List<QueryCondition> Conditions { get; set; }

        // rel: reportOrderBys
        [DataMember(Order = 4)]
        [XmlArray("Ordering")]
        [XmlArrayItem("OrderBy")]
        public List<OrderByItem> OrderBy { get; set; }
        
        //not serialized into XML
        [DataMember(Order = 5)]
        [XmlIgnore]
        public Dictionary<string, Dictionary<long, string>> InvalidReportInformation { get; set; }

        // not in entity model
        [DataMember(Order = 6)]
        public bool Distinct { get; set; }

        /// <summary>
        /// Gets or sets the timezone of the client.
        /// </summary>
        /// <remarks>
        /// Note: this is passed as part of the contract, but not serialized into XML, because it needs to be passed any time a query is used
        /// but should not generally be saved within a report.
        /// </remarks>
        // not in entity model
        [DataMember(Order = 7)]
        [XmlIgnore]
        public string TimeZoneName { get; set; }

        /// <summary>
        /// Gets or sets the report associated with this structured query.        
        /// </summary>
        [XmlIgnore]
        public Report Report { get; set; }

        #region XML Formatters
        /// <summary>
        /// Returns true if the ordering should be serialized.        
        /// This is to ensure no element appears if empty.        
        /// </summary>
        /// <returns>True if the ordering should be serialized</returns>
        /// <remarks>The method name must be ShouldSerialize followed by the 
        /// PropertyName, in this case OrderBy.</remarks>
		public bool ShouldSerializeOrderBy()
        {
            return OrderBy != null && 
                   OrderBy.Count > 0;
        }

        /// <summary>
        /// Returns true if the conditions should be serialized.
        /// This is to ensure no element appears if empty.        
        /// </summary>
        /// <returns>True if the conditions should be serialized</returns>
        /// <remarks>The method name must be ShouldSerialize followed by the 
        /// PropertyName, in this case Conditions.</remarks>
		public bool ShouldSerializeConditions()
        {
            return Conditions != null && 
                   Conditions.Count > 0;
        }

        /// <summary>
        /// Ensure lists are created after deserialization.
        /// </summary>
        /// <param name="sender"></param>
        void IDeserializationCallback.OnDeserialization(object sender)
        {
            if (SelectColumns == null)
                SelectColumns = new List<SelectColumn>();
            if (OrderBy == null)
                OrderBy = new List<OrderByItem>();
            if (Conditions == null)
                Conditions = new List<QueryCondition>();
        }

        /// <summary>
        /// Returns true if the conditions should be serialized.
        /// This is to ensure no element appears if empty.        
        /// </summary>
        /// <returns>True if Distinct is set, otherwise false.</returns>
        /// <remarks>The method name must be ShouldSerialize followed by the 
        /// PropertyName, in this case Conditions.</remarks>
		public bool ShouldSerializeDistinct()
        {
            return Distinct;
        }
        #endregion


        /// <summary>
        /// Creates a shallow copy of the structured query.
        /// </summary>
        /// <returns></returns>
        public StructuredQuery ShallowCopy()
        {
            var queryCopy = new StructuredQuery();

            queryCopy.RootEntity = RootEntity;
            queryCopy.SelectColumns.AddRange(SelectColumns);
            queryCopy.Conditions.AddRange(Conditions);
            queryCopy.OrderBy.AddRange(OrderBy);
            queryCopy.Distinct = Distinct;
            queryCopy.TimeZoneName = TimeZoneName;
            queryCopy.Report = Report;
            queryCopy.InvalidReportInformation = InvalidReportInformation;            

            return queryCopy;
        }


        /// <summary>
        /// Creates a deep copy of the structured query.
        /// </summary>
        /// <returns></returns>
        public StructuredQuery DeepCopy()
        {
            using ( Profiler.Measure( "StructureQuery.DeepCopy" ) )
            {                
                StructuredQuery deepCopy = (StructuredQuery)serializer.DeepClone(this);
                return deepCopy;
            }
        }


		/// <summary>
		/// Creates a unique string that represents the query, for use in cache key.
		/// </summary>
		/// <returns></returns>
        public string CacheKeyToken( )
        {
            using ( Profiler.Measure( "StructureQuery.CacheKeyToken" ) )
            {
                // Utilise the datacontract serializer to create a copy of the structured query                
                
                byte [ ] bytes;
                using ( var memoryStream = new MemoryStream( ) )
                using ( new StructuredQueryHashingContext( ) )
                {
                    // Serialize the query                    
                    serializer.Serialize(memoryStream, this);
                    memoryStream.Flush( );
                    bytes = memoryStream.ToArray( );
                }
                
                using (HashAlgorithm hasher = new MD5Cng( ))
                {
                    byte[] hash = hasher.ComputeHash(bytes);
                    string base64 = Convert.ToBase64String(hash);
                    return base64;
                }
            }
        }
        
        static ProtoBuf.Meta.RuntimeTypeModel serializer = StructuredQueryHelper.CreateSerializer( );
     
    }
}
