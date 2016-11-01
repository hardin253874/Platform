// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace EDC.ReadiNow.Common.ConfigParser.Containers
{
    /// <summary>
    /// Represents an entity, as it appears in the config XML. Nothing more.
    /// </summary>
    /// <remarks>
    /// Has no role outside of XML config parsing.
    /// </remarks>
    public class Entity
    {
        // Example:
        // <relationship>
        //   <alias>worksFor</alias>
        //   <reverseAlias>employees</reverseAlias>
        //   <cardinality>manyToOne</name>
        //   ..etc..
        // </relationship>
        //
        // this.Type    holds a reference to the entity with alias 'relationship'
        // this.Members holds a member for cardinality
        // this.Alias   holds 'worksFor'
        // this.ReverseAlias holds 'employees'


        public Entity()
        {
            this.Guid = Guid.NewGuid();
            this.Members = new List<Member>();
        }


        /// <summary>
        /// The alias of this entity. This is only set if the entity itself has an alias property element.
        /// </summary>
        public Alias Alias { get; set; }

        /// <summary>
        /// The alias of this entity. This is only set if the entity itself has a reverseAlias property.
        /// </summary>
        public Alias ReverseAlias { get; set; }

        /// <summary>
        /// The database ID of the element. Only set after being inserted into the database.
        /// </summary>
        public Int64 Id { get; set; }

        /// <summary>
        /// The unique GUID identifier of the resource. Randomly assigned when the config is parsed for the first time.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Reference to the entity's type, as specified by the tag name of the entity element.
        /// Does not include any other types that the entity may be related to by the 'isAlso' relationship.
        /// </summary>
        public EntityRef Type { get; set; }

        /// <summary>
        /// The list of fields and relationships that were declared in the XML immediately under this entity.
        /// </summary>
        internal List<Member> Members { get; private set; }

        /// <summary>
        /// Special case container to hold relationship instance 'from' elements.
        /// </summary>
        internal Member RelationshipInstanceFrom { get; set; }

        /// <summary>
        /// Special case container to hold relationship instance 'to' elements.
        /// </summary>
        internal Member RelationshipInstanceTo { get; set; }


        /// <summary>
        /// The line number that contained the reference.
        /// </summary>
        public int LineNumber = 0;


        /// <summary>
        /// The line number that contained the reference.
        /// </summary>
        public int ColumnNumber = 0;


        /// <summary>
        /// The line number that contained the reference.
        /// </summary>
        public string File = string.Empty;


        /// <summary>
        /// Convenient debug information.
        /// </summary>
        /// <remarks>Type alias: Entity alias</remarks>
        public override string ToString()
        {
            return string.Format("{0}: {1}", this.Type.Alias, this.Alias == null ? "<anon>" : this.Alias.ToString());
        }

        /// <summary>
        /// Provides a string description for debug and error reporting.
        /// </summary>
        /// <returns></returns>
        public string LocationInfo
        {
            get
            {
                Alias entityElem = this.Type.Alias;
                if (entityElem != null)
                    return BuildException.FormatLocation(entityElem.File, entityElem.LineNumber, entityElem.ColumnNumber);
                else
                    return BuildException.FormatLocation(File, LineNumber, ColumnNumber);
            }
        }

        internal void SetLineInfo(XElement element)
        {

            IXmlLineInfo elemInfo = (IXmlLineInfo)element;
            LineNumber = elemInfo.HasLineInfo() ? elemInfo.LineNumber : 0;
            ColumnNumber = elemInfo.HasLineInfo() ? elemInfo.LinePosition : 0;
            File = element.Document.BaseUri;
        }
    }
}
