// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;

namespace EDC.ReadiNow.Common.ConfigParser.Containers
{
    /// <summary>
    /// A reference to an entity.
    /// Typically referenced by alias, but sometimes the EntityRef is created with the Entity already embedded.
    /// </summary>
    /// <remarks>
    /// Used to hold onto entities referenced by name. The target entity may not have been parsed yet.
    /// Aliases are typically used either as element names of other entities or members, or as targets of relationships.
    /// </remarks>
    public class EntityRef
    {
        /// <summary>
        /// Create a reference from an entity itself. Used when parsing related entities that are embedded inline.
        /// </summary>
        /// <param name="entity">The entity being referenced.</param>
        internal EntityRef(Entity entity)
        {
            this.Entity = entity;
        }


        /// <summary>
        /// Creates a reference to an entity by alias.
        /// </summary>
        /// <param name="alias">The alias. May be a reverse alias.</param>
        internal EntityRef(Alias alias)
        {
            this.Alias = alias;
        }


        /// <summary>
        /// Creates a reference to an entity by alias, extracting the alias from an XElement tag name.
        /// </summary>
        /// <param name="xElement"></param>
        internal EntityRef(XElement xElement)
        {
            this.Alias = new Alias(xElement.Name);
            this.Alias.SetLineInfo(xElement);
        }


        /// <summary>
        /// The alias.
        /// May be null.
        /// May be a reverse alias.
        /// </summary>
        public Alias Alias { get; set; }


        /// <summary>
        /// The entity. May not yet be loaded.
        /// </summary>
		internal Entity Entity { get; set; }
    }

}
