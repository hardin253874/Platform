// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Core.Cache;

namespace EDC.ReadiNow.Model
{
    /// <summary>
    /// Resource class.
    /// </summary>
    public static class ResourceHelper
    {        
        /// <summary>
        /// Determines whether the specified entity has the specified flag.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="flag">The flag.</param>
        /// <returns>
        ///   <c>true</c> if the specified entity has the specified flag; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// resource
        /// or
        /// flag
        /// </exception>
        public static bool HasFlag(IEntity resource, EntityRef flag)
        {
            if (resource == null)
            {
                throw new ArgumentNullException("resource");
            }

            if (flag == null)
            {
                throw new ArgumentNullException("flag");
            }

            IEnumerable<IEntity> flags = resource.GetRelationships("core:flags", Direction.Forward).Entities;

            bool hasFlag = flags.Any(f => f.Id == flag.Id);

            if (hasFlag)
            {
                return true;
            }

            foreach (IEntity type in Entity.Get(resource.TypeIds))
            {                
                IEnumerable<IEntity> instanceFlags = type.GetRelationships("core:instanceFlags", Direction.Forward).Entities;

                hasFlag = instanceFlags.Any(f => f.Id == flag.Id);

                if (hasFlag)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        ///     No Delete alias
        /// </summary>
        public static EntityRef NoDeleteFlag
        {
            get
            {
                return Entity.GetId("core:noDeleteFlag");
            }
        }


        /// <summary>
        ///     Read-Only alias
        /// </summary>
        public static EntityRef ReadOnlyFlag
        {
            get
            {
                return Entity.GetId("core:readOnlyFlag");
            }
        }
    }
}
