// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.ReadiNow.Common.ConfigParser.Containers;

namespace EDC.ReadiNow.XsdGen
{
    /// <summary>
    /// Accessors for additional aliases that are not already provided by Aliases class.
    /// </summary>
    public static class Aliases2
    {
        public static readonly Alias Resource = Aliases.CoreAlias("resource");
        public static readonly Alias XsdType = Aliases.CoreAlias("xsdType");
        public static readonly Alias MaxLength = Aliases.CoreAlias("maxLength");
        public static readonly Alias MinLength = Aliases.CoreAlias("minLength");
        public static readonly Alias MaxInt = Aliases.CoreAlias("maxInt");
        public static readonly Alias MinInt = Aliases.CoreAlias("minInt");
        public static readonly Alias IsRequired = Aliases.CoreAlias("isRequired");
        public static readonly Alias DefaultValue = Aliases.CoreAlias("defaultValue");
        public static readonly Alias IsAbstract = Aliases.CoreAlias("isAbstract");
        public static readonly Alias IsSealed = Aliases.CoreAlias("isSealed");
        public static readonly Alias OneToOne = Aliases.CoreAlias("oneToOne");
        public static readonly Alias OneToMany = Aliases.CoreAlias("oneToMany");
        public static readonly Alias ManyToMany = Aliases.CoreAlias("manyToMany");
        public static readonly Alias ManyToOne = Aliases.CoreAlias("manyToOne");
        public static readonly Alias Pattern = Aliases.CoreAlias("pattern");
        public static readonly Alias Regex = Aliases.CoreAlias("regex");
        public static readonly Alias XmlFieldNamespace = Aliases.CoreAlias("xmlFieldNamespace");
        public static readonly Alias File = Aliases.CoreAlias("fileType");
    }

}
