// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EDC.Core;
using EDC.ReadiNow.Annotations;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// An entity that represents an aggregation.
    /// Note: this is modelled as a separate entity so that it can be nested.
    /// Note: typically a grouped entity does not have its children modelled as related entities.
    /// </summary>
    // type: aggregateReportNode
    [DataContract(Namespace = Constants.DataContractNamespace)]
    [XmlType(Namespace = Constants.StructuredQueryNamespace)]
    public class AggregateEntity : Entity, IDeserializationCallback
    {
        public AggregateEntity()
        {
            // See also: OnDeserialization
            GroupBy = new List<ScalarExpression>();
        }

        /// <summary>
        /// The entity actually being grouped.
        /// </summary>
        // rel: groupedNode
        [DataMember(Order = 1)]
        public Entity GroupedEntity { get; set; }


        /// <summary>
        /// List of auxillary grouping expressions that should be used.
        /// </summary>
        /// <remarks>
        /// Note that GroupBy expressions do not need to be specified when grouping by a related resource.
        /// They are only used when grouping on an indivual expression such as a field.
        /// For example: 'show all companies, and the number of employees in each' does not require a group-by expression.
        /// Whereas: 'show distinct last names, and the number of people with each' requires a group-by expression to access last-name.
        /// Any expressions listed here may also be used outside of the parent query. 
        /// </remarks>
        // rel: groupedBy
        [DataMember(Order = 2)]
        [XmlArray("GroupBy")]
        [XmlArrayItem("Expression")]
        public List<ScalarExpression> GroupBy { get; set; }

        #region XML Formatters

        /// <summary>
        /// Returns true if IsGrouped should be serialized.
        /// Grouping is uncommon, so only render it if it is switched on.
        /// </summary>
        /// <returns>True if the GroupBy expression list should be serialized</returns>
		public bool ShouldSerializeGroupBy()
        {
            return GroupBy != null &&
                GroupBy.Count > 0;
        }

        /// <summary>
        /// Ensure lists are created after deserialization.
        /// </summary>
        /// <param name="sender"></param>
        void IDeserializationCallback.OnDeserialization(object sender)
        {
            if (GroupBy == null)
                GroupBy = new List<ScalarExpression>();
            if (RelatedEntities == null)
                RelatedEntities = new List<Entity>();
        }
        #endregion


    }
}
