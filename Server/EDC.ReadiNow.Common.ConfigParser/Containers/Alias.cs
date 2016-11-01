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
    /// Represents an alias string and namespace.
    /// </summary>
    public class Alias
    {
        /// <summary>
        /// The alias name.
        /// </summary>
        public string Value
        {
            get { return _value; }
            set
            {
                this.RelationshipInstance = value.EndsWith(XmlParser.RelationshipInstanceSuffix);
                if (this.RelationshipInstance)
                    _value = value.Substring(0, value.Length - XmlParser.RelationshipInstanceSuffix.Length);
                else
                    _value = value;
            }
        }
        string _value;


        /// <summary>
        /// The alias namespace.
        /// </summary>
        public string Namespace = string.Empty;


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
        /// If true, indicates that this alias was specifically referring to a relationship entity, rather than a relationship member.
        /// </summary>
        public bool RelationshipInstance = false;

        #region Constructors
        public Alias()
        {
        }

        public Alias(XName xName)
        {
            this.Value = xName.LocalName;
            this.Namespace = xName.NamespaceName;
        }
        #endregion


        /// <summary>
        /// Convert to Xml Name.
        /// </summary>
        public XmlQualifiedName ToQualifiedName(string prefix = null, string suffix = null)
        {
            string name = string.Format("{0}{1}{2}", prefix, this.Value, suffix);
            return new XmlQualifiedName(name, this.Namespace);
        }


        /// <summary>
        /// For convenient debugging.
        /// </summary>
        public override string ToString()
        {
            return Value;       
        }

        /// <summary>
        /// For convenient debugging.
        /// </summary>
        public string NsAlias
        {
            get
            {
                return string.Concat(Namespace, ":", Value);
            }
        }

        #region Equality Test
        public override bool Equals(object obj)
        {
            Alias other = obj as Alias;
            if (other == null)
                return false;
            else
                return other.Value == this.Value && other.Namespace == this.Namespace;
        }

        public override int GetHashCode()
        {
			unchecked
			{
				int hash = 17;

				if ( Value != null )
				{
					hash = hash * 92821 + Value.GetHashCode( );
				}

				hash = hash * 92821 + Namespace.GetHashCode( );

				return hash;
			}
        }

        public static bool operator == (Alias a, Alias b)
        {
            bool aNull = object.ReferenceEquals(a, null);
            bool bNull = object.ReferenceEquals(b, null);
            if (aNull || bNull)
                return aNull && bNull;
            return a.Value == b.Value && a.Namespace == b.Namespace;
        }

        public static bool operator != (Alias a, Alias b)
        {
            bool aNull = object.ReferenceEquals(a, null);
            bool bNull = object.ReferenceEquals(b, null);
            if (aNull || bNull)
                return aNull != bNull;
            return a.Value != b.Value || a.Namespace != b.Namespace;
        }
        #endregion

        internal void SetLineInfo(XElement element)
        {
            
            IXmlLineInfo elemInfo = (IXmlLineInfo)element;
            LineNumber = elemInfo.HasLineInfo() ? elemInfo.LineNumber : 0;
            ColumnNumber = elemInfo.HasLineInfo() ? elemInfo.LinePosition : 0;
            File = element.Document.BaseUri;
        }
    }

}
