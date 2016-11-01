// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.SoftwarePlatform.Migration.Processing
{
	/// <summary>
	///     Xml Constants.
	/// </summary>
	public static class XmlConstants
	{
        /// <summary>
        ///     The identifier
        /// </summary>
        public const string HeaderComment = " ReadiNow Data Package ";

        /// <summary>
        ///     The identifier
        /// </summary>
        public const string Id = "id";

		/// <summary>
		///     Xml constant
		/// </summary>
		public const string Xml = "xml";

		/// <summary>
		///     Xml Namespace constant
		/// </summary>
		public const string XmlNs = "xmlns";

		/// <summary>
		///     Version constant.
		/// </summary>
		public const string Version = "version";

		/// <summary>
		///     Encoding constant.
		/// </summary>
		public const string Encoding = "encoding";

		/// <summary>
		///     Alias map constant
		/// </summary>
		public const string AliasMap = "aliasMap";

		/// <summary>
		///     Type constant
		/// </summary>
		public const string Type = "type";

		/// <summary>
		///     The upgrade identifier
		/// </summary>
		public const string UpgradeId = "upgradeId";

        /// <summary>
        ///     The packageId identifier
        /// </summary>
        public const string PackageId = "packageId";

        /// <summary>
        ///     Metadata constants.
        /// </summary>
        public static class MetadataConstants
		{
			/// <summary>
			///     The application
			/// </summary>
			public const string Application = "application";

			/// <summary>
			///     Package constant
			/// </summary>
			public const string Package = "package";

			/// <summary>
			///     Name constant
			/// </summary>
			public const string Name = "name";

			/// <summary>
			///     Dependency name
			/// </summary>
			public const string DependencyName = "dependencyName";

			/// <summary>
			///     Application name constant
			/// </summary>
			public const string ApplicationName = "appName";

			/// <summary>
			///     Description constant
			/// </summary>
			public const string Description = "description";

			/// <summary>
			///     Type constant
			/// </summary>
			public const string Type = "type";

            /// <summary>
            ///     Metadata constant
            /// </summary>
            public const string Metadata = "metadata";

            /// <summary>
            ///     Version constant
            /// </summary>
            public const string Version = "version";

			/// <summary>
			///     Platform version constant
			/// </summary>
			public const string PlatformVersion = "platformVersion";

			/// <summary>
			///     Release date constant
			/// </summary>
			public const string ReleaseDate = "releaseDate";

			/// <summary>
			///     Publish date constant
			/// </summary>
			public const string PublishDate = "publishDate";

			/// <summary>
			///     Dependencies constant
			/// </summary>
			public const string Dependencies = "dependencies";

			/// <summary>
			///     The dependency
			/// </summary>
			public const string Dependency = "dependency";

			/// <summary>
			///     Minimum version constant
			/// </summary>
			public const string MinimumVersion = "minimumVersion";

			/// <summary>
			///     Maximum version constant
			/// </summary>
			public const string MaximumVersion = "maximumVersion";

			/// <summary>
			///     Is required constant
			/// </summary>
			public const string IsRequired = "isRequired";

			/// <summary>
			///     Application identifier constant
			/// </summary>
			public const string ApplicationId = "applicationId";

            /// <summary>
            ///     Root entity being exported/imported
            /// </summary>
            public const string Root = "root";
        }

        /// <summary>
        ///     Do Not Remove xml constants.
        /// </summary>
        public static class DoNotRemoveConstants
        {

            /// <summary>
            ///     Do not remove
            /// </summary>
            public const string DoNotRemove = "doNotRemove";

            /// <summary>
            ///     Entities to leave
            /// </summary>
            public const string LeaveEntity = "leaveEntity";
        }

        /// <summary>
        ///     Entity Xml constants.
        /// </summary>
        public static class EntityConstants
		{
			/// <summary>
			///     Entities constant
			/// </summary>
			public const string Entities = "entities";

			/// <summary>
			///     Entity constant
			/// </summary>
			public const string Entity = "entity";

            /// <summary>
            ///     Type constant
            /// </summary>
            public const string Type = "type";

            /// <summary>
            ///     TypeId constant
            /// </summary>
            public const string TypeId = "typeId";

            /// <summary>
            ///     Group constant
            /// </summary>
            public const string Group = "group";
        }


		/// <summary>
		///     Relationship Xml constants.
		/// </summary>
		public static class RelationshipConstants
		{
			/// <summary>
			///     Relationships constant
			/// </summary>
			public const string Relationships = "relationships";

            /// <summary>
            ///     ReverseRelationship constant
            /// </summary>
            public const string ReverseRelationship = "reverseRelationship";

            /// <summary>
            ///     Relationship constant
            /// </summary>
            public const string Relationship = "relationship";

			/// <summary>
			///     From constant
			/// </summary>
			public const string From = "from";

			/// <summary>
			///     To constant
			/// </summary>
			public const string To = "to";

            /// <summary>
            ///     Rel constant
            /// </summary>
            public const string Rel = "rel";

            /// <summary>
            ///     RevRel constant
            /// </summary>
            public const string RevRel = "revRel";
        }

		/// <summary>
		///     Binary constants.
		/// </summary>
		public static class BinaryConstants
		{
			/// <summary>
			///     Binaries constant
			/// </summary>
			public const string Binaries = "binaries";

			/// <summary>
			///     Binary constant
			/// </summary>
			public const string Binary = "binary";

			/// <summary>
			///     Hash constant
			/// </summary>
			public const string Hash = "hash";

			/// <summary>
			///     Extension constant
			/// </summary>
			public const string Extension = "extension";
		}

		/// <summary>
		///     Document constants.
		/// </summary>
		public static class DocumentConstants
		{
			/// <summary>
			///     Documents constant
			/// </summary>
			public const string Documents = "documents";

			/// <summary>
			///     Document constant
			/// </summary>
			public const string Document = "document";

			/// <summary>
			///     Hash constant
			/// </summary>
			public const string Hash = "hash";

			/// <summary>
			///     Extension constant
			/// </summary>
			public const string Extension = "extension";
		}

		/// <summary>
		///     Class representing the FieldConstants type.
		/// </summary>
		public static class FieldConstants
		{
			/// <summary>
			///     The alias field
			/// </summary>
			public const string AliasField = "alias";

			/// <summary>
			///     The reverse alias field
			/// </summary>
			public const string ReverseAliasField = "reverseAlias";

			/// <summary>
			///     The bit field - deprecated .. use BoolField
			/// </summary>
			public const string BitField = "bit";

            /// <summary>
            ///     The bool field
            /// </summary>
            public const string BoolField = "bool";

            /// <summary>
            ///     The date time field
            /// </summary>
            public const string DateTimeField = "dateTime";

			/// <summary>
			///     The decimal field
			/// </summary>
			public const string DecimalField = "decimal";

			/// <summary>
			///     The unique identifier field
			/// </summary>
			public const string GuidField = "guid";

			/// <summary>
			///     The int field
			/// </summary>
			public const string IntField = "int";

            /// <summary>
            ///     The NVarChar field .. use TextField
            /// </summary>
            public const string NVarCharField = "nVarChar";

            /// <summary>
            ///     The text field
            /// </summary>
            public const string TextField = "text";

            /// <summary>
            ///     The XML field
            /// </summary>
            public const string XmlField = "xml";

			/// <summary>
			///     The alias identifier marker
			/// </summary>
			public const string AliasIdMarker = "aliasMarkerId";
		}


		/// <summary>
		///     Class representing secure data
		/// </summary>
		public class SecureDataConstants
		{
			/// <summary>
			///     Secure data
			/// </summary>
			public const string SecureData = "secureData";

			/// <summary>
			///     Secure data entry
			/// </summary>
			public const string SecureDataEntry = "entry";

			/// <summary>
			///     Secure Id
			/// </summary>
			public const string SecureId = "secureId";

			/// <summary>
			///     Context
			/// </summary>
			public const string Context = "context";
		}
	}
}