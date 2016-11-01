// Copyright 2011-2016 Global Software Innovation Pty Ltd
extern alias EdcReadinowCommon;
using EntityType = EdcReadinowCommon::EDC.ReadiNow.Model.EntityType;

using System.Collections.Generic;
using System.Linq;

namespace ReadiNow.DocGen.DataSources
{
    /// <summary>
    /// Returns nothing. Used when some instruction failed to resolve.
    /// </summary>
    class EmptySource : DataSource
    {
        /// <summary>
        /// Return the entities for this data source.
        /// </summary>
        /// <param name="context">The context, such as the parent entity, from which these entities are being loaded.</param>
        /// <returns>
        /// Empty collection.
        /// </returns>
        public override IEnumerable<DataElement> GetData(WriterContext context)
        {
            return Enumerable.Empty<DataElement>();
        }


        /// <summary>
        /// Determine the type of entity that would be found in the context generated by this instruction.
        /// </summary>
        /// <param name="owner">The instruction that owns this data source.</param>
        /// <returns>An EntityType</returns>
        public override EntityType GetContextEntityType(Instruction owner)
        {
            return null;
        }

    }
}