// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Core;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EDC.ReadiNow.Metadata.Query.Structured
{
    /// <summary>
    /// Top level object in representation of the constraints imposed by the analyzer.
    /// </summary>
    /// <remarks>
    /// It is likely that this class will eventually become a DataContract. Please keep in mind.
    /// </remarks>
    [DataContract(Namespace = Constants.DataContractNamespace)]
    public class Conditions : IDeserializationCallback
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Conditions()
        {
            ConditionList = new List<Condition>();
        }


        /// <summary>
        /// The list of individual filters, which are implied to all be satisfied. (That is 'AND'ed together).
        /// </summary>
        /// <remarks>
        /// Not every column will be present. And, conceptually at least, a column may have more than one filter applied against it.
        /// </remarks>
        [DataMember(Order = 1)]
        public IList<Condition> ConditionList { get; private set; }


        /// <summary>
        /// Ensure lists are created after deserialization.
        /// </summary>
        /// <param name="sender"></param>
        void IDeserializationCallback.OnDeserialization(object sender)
        {
            if (ConditionList == null)
                ConditionList = new List<Condition>();
        }
    }
}
