// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDC.ReadiNow.Common.ConfigParser.Containers
{
    /// <summary>
    /// Represents a XML child element of an entity element.
    /// May be either a field, or a relationship. 
    /// Only used while parsing XML, or interpreting parsed content.
    /// </summary>
    class Member
    {
        // Example:
        // <person>
        //    <firstName>Peter</firstName>
        //    <worksFor>edc</worksFor>
        //    <ownsProcess>
        //       <process>...</process>
        //       <process>...</process>
        //    <ownsProcess>
        // </person>
        // Each of firstName, worksFor, ownsProcess are Members. 'person' and 'process' are entities, not members.
        // The tags 'firstName', 'worksFor', 'ownsProcess' are each held in the MemberDefinition reference.
        // The values 'Peter' and 'edc' are held in the Value property.
        // The 'process' entities are held in Children.

        public Member()
        {
            Children = new List<EntityRef>();
        }

        /// <summary>
        /// The XML text. May be either a field value, or a relationship alias.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The XML text, translated as a list of aliases. May be either a field value, or relationship aliases.
        /// </summary>
        public Alias[] ValueAsAliases { get; set; }

        /// <summary>
        /// A reference to the name of the member. Typically a reference by alias, based on the XML element name.
        /// </summary>
        public EntityRef MemberDefinition { get; set; }

        /// <summary>
        /// If the member contains child nodes then it is likely a relationship with child entities inline.
        /// Those entries get held here. Typically the entity itself is set in these entity refs.
        /// </summary>
        public List<EntityRef> Children { get; private set; }
    }
}
