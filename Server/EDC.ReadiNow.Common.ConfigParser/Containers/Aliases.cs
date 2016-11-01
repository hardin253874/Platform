// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDC.ReadiNow.Common.ConfigParser.Containers
{
    /// <summary>
    /// Well known aliases.
    /// </summary>
    public static class Aliases
    {
        public static readonly string CoreNamespace = "core";

        public static readonly Alias Alias = CoreAlias("alias");
        public static readonly Alias ReverseAlias = CoreAlias("reverseAlias");
        public static readonly Alias Type = CoreAlias("type");
        public static readonly Alias IsAlsoType = CoreAlias("isOfType");
        public static readonly Alias Relationship = CoreAlias("relationship");
        public static readonly Alias Field = CoreAlias("field");
        public static readonly Alias FieldIsOnType = CoreAlias("fieldIsOnType");
        public static readonly Alias FieldType = CoreAlias("fieldType");
        public static readonly Alias Inherits = CoreAlias("inherits");
        public static readonly Alias InstancesInheritByDefault = CoreAlias("instancesInheritByDefault");
        public static readonly Alias FromType = Aliases.CoreAlias("fromType");
        public static readonly Alias ToType = Aliases.CoreAlias("toType");
        public static readonly Alias From = Aliases.CoreAlias("from");
        public static readonly Alias To = Aliases.CoreAlias("to");
		public static readonly Alias AliasNamespace = CoreAlias( "aliasNS" );
		public static readonly Alias Relationships = CoreAlias( "relationships" );
		public static readonly Alias Fields = CoreAlias( "fields" );
		public static readonly Alias Name = Aliases.CoreAlias( "name" );
		public static readonly Alias Description = Aliases.CoreAlias( "description" );
		public static readonly Alias ClassName = Aliases.CoreAlias( "className" );
		public static readonly Alias ClassType = Aliases.CoreAlias( "classType" );
		public static readonly Alias XsdType = Aliases.CoreAlias( "xsdType" );
		public static readonly Alias IsAbstract = Aliases.CoreAlias( "isAbstract" );
		public static readonly Alias IsSealed = Aliases.CoreAlias( "isSealed" );
        internal static readonly Alias Cardinality = Aliases.CoreAlias( "cardinality" );
        public static readonly Alias OneToOne = Aliases.CoreAlias( "oneToOne" );
        public static readonly Alias ManyToOne = Aliases.CoreAlias( "manyToOne" );
        public static readonly Alias OneToMany = Aliases.CoreAlias( "oneToMany" );
        public static readonly Alias ManyToMany = Aliases.CoreAlias("manyToMany");
        public static readonly Alias InSolution = Aliases.CoreAlias("inSolution");
        public static readonly Alias Resource = CoreAlias("resource");
        public static readonly Alias DefaultPointsTo = Aliases.CoreAlias("defaultPointTo");
        public static readonly Alias ReadiNowType = Aliases.CoreAlias("readiNowType");
		public static readonly Alias Mandatory = Aliases.CoreAlias( "mandatory" );
		public static readonly Alias Optional = Aliases.CoreAlias( "optional" );
		public static readonly Alias Forbidden = Aliases.CoreAlias( "forbidden" );
		public static readonly Alias GenerateCode = Aliases.CoreAlias( "generateCode" );
		public static readonly Alias DbFieldTable = Aliases.CoreAlias( "dbFieldTable" );
	    public static readonly Alias Flags = Aliases.CoreAlias( "flags" );
		public static readonly Alias ReadOnlyFlag = Aliases.CoreAlias( "readOnlyFlag" );
	    public static readonly Alias TypeName = Aliases.CoreAlias( "typeName" );
        public static readonly Alias AssemblyName = Aliases.CoreAlias("assemblyName");
        public static readonly Alias EnumType = Aliases.CoreAlias("enumType");

        // Relationship types
        public static readonly Alias RelType = Aliases.CoreAlias("relType");
        public static readonly Alias RelLookup = Aliases.CoreAlias("relLookup");
        public static readonly Alias RelDependantOf = Aliases.CoreAlias("relDependantOf");
        public static readonly Alias RelComponentOf = Aliases.CoreAlias("relComponentOf");
        public static readonly Alias RelChoiceField = Aliases.CoreAlias("relChoiceField");
        public static readonly Alias RelSingleLookup = Aliases.CoreAlias("relSingleLookup");
        public static readonly Alias RelSingleComponentOf = Aliases.CoreAlias("relSingleComponentOf");
        public static readonly Alias RelSingleComponent = Aliases.CoreAlias("relSingleComponent");
        public static readonly Alias RelExclusiveCollection = Aliases.CoreAlias("relExclusiveCollection");
        public static readonly Alias RelDependants = Aliases.CoreAlias("relDependants");
        public static readonly Alias RelComponents = Aliases.CoreAlias("relComponents");
        public static readonly Alias RelManyToMany = Aliases.CoreAlias("relManyToMany");
        public static readonly Alias RelManyToManyFwd = Aliases.CoreAlias( "relManyToManyFwd" );
        public static readonly Alias RelManyToManyRev = Aliases.CoreAlias( "relManyToManyRev" );
        public static readonly Alias RelMultiChoiceField = Aliases.CoreAlias("relMultiChoiceField");
        public static readonly Alias RelSharedDependantsOf = Aliases.CoreAlias("relSharedDependantsOf");
        public static readonly Alias RelSharedDependants = Aliases.CoreAlias("relSharedDependants");
        public static readonly Alias RelCustomRelType = Aliases.CoreAlias("relCustomRelType");

        // Used by decorator
        public static readonly Alias CascadeDelete = Aliases.CoreAlias("cascadeDelete");
        public static readonly Alias CascadeDeleteTo = Aliases.CoreAlias("cascadeDeleteTo");
        public static readonly Alias CloneAction = Aliases.CoreAlias("cloneAction");
        public static readonly Alias ReverseCloneAction = Aliases.CoreAlias("reverseCloneAction");
        public static readonly Alias ImplicitInSolution = Aliases.CoreAlias("implicitInSolution");
        public static readonly Alias ReverseImplicitInSolution = Aliases.CoreAlias("reverseImplicitInSolution");
        public static readonly Alias CloneEntities = Aliases.CoreAlias("cloneEntities");
        public static readonly Alias CloneReferences = Aliases.CoreAlias("cloneReferences");
        public static readonly Alias Drop = Aliases.CoreAlias("drop");
        public static readonly Alias SecuresFrom = Aliases.CoreAlias("securesFrom");
        public static readonly Alias SecuresTo = Aliases.CoreAlias("securesTo");

        /// <summary>
        /// Convenient method for constructing aliases in default namespace.
        /// </summary>
        public static Alias CoreAlias(string alias)
        {
            return new Alias()
            {
                Value = alias,
                Namespace = CoreNamespace,
                File = "XsdGen.exe internals reference"
            };
        }
    }
}
