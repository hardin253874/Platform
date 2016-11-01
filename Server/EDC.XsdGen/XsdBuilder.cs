// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EDC.ReadiNow.Common.ConfigParser;
using EDC.ReadiNow.Common.ConfigParser.Containers;
using System.Xml.Schema;
using System.Xml;

namespace EDC.ReadiNow.XsdGen
{
    /// <summary>
    /// Builds XSDs.
    /// </summary>
    public class XsdBuilder
    {
        // The stream of entities
        private IEnumerable<Entity> _entities;

        // Provides lookup from alias to entity
        private IAliasResolver _aliasResolver;

        // Provides informationa about the entity schema
        private SchemaResolver _schemaManager;

        // Map of field types to XSD type strings
        private Dictionary<Entity, string> _fieldTypeToXsdType;

        // Map of full schema names to the schema files they will be written to
        private Dictionary<string, SchemaFile> _namespaceToSchema;

        // Set of names that got referenced
        private HashSet<XmlQualifiedName> _namesUsed;

        // Set of names that got referenced
        private HashSet<XmlQualifiedName> _namesDeclared;

        // It can be convenient to switch this off during debugging
        const bool AllowRelationshipInstances = true;

        /// <summary>
        /// Builds the schema. Main entry point.
        /// </summary>
        public static void BuildSchemas(IEnumerable<Entity> entities, IEnumerable<SchemaFile> schemas)
        {
            XsdBuilder builder = new XsdBuilder();
            builder.BuildSchemaImpl(entities, schemas);
        }


        /// <summary>
        /// Builds the schema. Main list of activities to perform.
        /// </summary>
        private void BuildSchemaImpl(IEnumerable<Entity> entities, IEnumerable<SchemaFile> schemas)
        {
            _namespaceToSchema = schemas.ToDictionary(schema => schema.Namespace);
            _entities = entities;
            _aliasResolver = new EntityStreamAliasResolver(entities);
            _schemaManager = new SchemaResolver(entities, _aliasResolver);
            _schemaManager.Initialize();
            _namesUsed = new HashSet<XmlQualifiedName>();
            _namesDeclared = new HashSet<XmlQualifiedName>();

            AddRootElement();
            AddRootChildren();

            FindFieldTypes();
            AddFields();
            AddTypes();
            AddRelationships();

            var errors = _namesUsed.Except(_namesDeclared).ToList( );
			if ( errors.Count > 0 )
                throw new Exception("Names were used but not declared: " + string.Join(", ", errors.Select(qn => string.Format("{0}:{1}", qn.Namespace, qn.Name))));

        }


        /// <summary>
        /// Gets the schema that is representing this namespace.
        /// </summary>
        /// <param name="ns">The namespace</param>
        /// <returns>The schema.</returns>
        private XmlSchema GetSchema(string ns)
        {
            var schemaFile = _namespaceToSchema[ns];
            if (schemaFile.XmlSchema == null)
            {
                var xsd = CreateSchema(schemaFile);
                schemaFile.XmlSchema = xsd;

            }
            return schemaFile.XmlSchema;
        }


        /// <summary>
        /// Creates a new schema object, and adds various headers.
        /// </summary>
        /// <param name="schemaFile">The schema info object. (Just used so it doesn't add a reference to itself).</param>
        /// <returns>The new schema object.</returns>
        private XmlSchema CreateSchema(SchemaFile schemaFile)
        {
            // Create schema object
            var xsd = new XmlSchema();

            // Set up namespace
            xsd.TargetNamespace = schemaFile.Namespace;
            xsd.Namespaces.Add("", schemaFile.Namespace);
            xsd.ElementFormDefault = XmlSchemaForm.Qualified;

            // Add <imports> and namespaces from schemas to all schemas
            // (this could be improved so that only referenced schemas are imported)
            foreach (SchemaFile foreignSchema in _namespaceToSchema.Values)
            {
                // Don't import self
                if (foreignSchema == schemaFile)
                    continue;

                // Add namespace
                string nsAlias = foreignSchema.Namespace;   // use the namespace as its own XML alias. Ok, while aliases are nice and simple.
                xsd.Namespaces.Add(nsAlias, foreignSchema.Namespace);

                // Add import
                XmlSchemaImport import = new XmlSchemaImport();
                import.Namespace = foreignSchema.Namespace;
                import.SchemaLocation = RelativePath(foreignSchema.Path, schemaFile.Path);   // todo: calculate relative path of one schema to another
                xsd.Includes.Add(import);
                
            }
            return xsd;
        }


        /// <summary>
        /// Transforms the file path 'path' so that it is relative to 'relativeTo'.
        /// </summary>
        private static string RelativePath(string path, string relativeTo)
        {
            Uri uriPath = new Uri(Path.Combine(Environment.CurrentDirectory, path));
            Uri uriRel = new Uri(Path.Combine(Environment.CurrentDirectory, relativeTo));
            string result = uriRel.MakeRelativeUri(uriPath).ToString();
            return result;
        }


        /// <summary>
        /// Finds all field types (e.g. StringField).
        /// Looks up their xsdType property and stores the results into the _fieldTypeToXsdType dictionary.
        /// </summary>
        private void FindFieldTypes()
        {
            _fieldTypeToXsdType = new Dictionary<Entity, string>();
            var fieldTypes = _schemaManager.GetInstancesOfType(A(Aliases.FieldType));

            foreach (var fieldType in fieldTypes)
            {
                string xsdType = _schemaManager.GetStringFieldValue(fieldType, Aliases2.XsdType);
                if (xsdType != null)
                {
                    _fieldTypeToXsdType.Add(fieldType, xsdType);
                }                    
            }
        }


        /// <summary>
        /// Adds the top level 'resources' element to the 'Core' namespace.
        /// </summary>
        private void AddRootElement()
        {
            // Declare that the 'resource' root element can contain any type of resource or relationship instance.
            var complexType = new XmlSchemaComplexType();
            complexType.Particle = new XmlSchemaGroupRef()
            {
                MinOccurs = 0,
                MaxOccursString = "unbounded",
                RefName = new XmlQualifiedName("rootchildren")
            };

            // Declare the 'defaultSolution' attribute, which specifies the default solution of all enclosed resources.
            complexType.Attributes.Add(new XmlSchemaAttribute()
            {
                Name = "defaultSolution",
                SchemaTypeName = XmlSchemaSimpleType.GetBuiltInSimpleType(XmlTypeCode.String).QualifiedName
            });
            
            // Register the root 'resources' element.
            XmlSchemaElement rootElem = new XmlSchemaElement();
            rootElem.Name = "resources";
            rootElem.SchemaType = complexType;

            // Add to schema
            XmlSchema xsd = GetSchema(Aliases.CoreNamespace);
            xsd.Items.Insert(0, rootElem);
        }

        /// <summary>
        /// Creates a group that contains all of the elements that may be children of the root.
        /// That is, all types, and and explicit relationship instances.
        /// </summary>
        private void AddRootChildren()
        {
            // Example of inheritance declaration
            //<xs:group name="rootchildren">
            //  <xs:choice>
            //    <xs:element ref="person" />
            //    <xs:element ref="employee" />
            //    <xs:element ref="manager" />
            //    <xs:element ref="worksFor.instance" />
            //  </xs:choice>
            //</xs:group>


            // Create a group, named "rootchildren"
            var rootChildrenGroup = new XmlSchemaGroup();
            rootChildrenGroup.Name = "rootchildren";
            rootChildrenGroup.Particle = new XmlSchemaChoice();

            // All types
            var types = EffectiveTypes(_schemaManager.GetInstancesOfType(A(Aliases.Type)));
            
            foreach (EffectiveType type in types)
            {
                bool isAbstract = _schemaManager.GetBoolFieldValue(type.Type, Aliases2.IsAbstract);
                if (isAbstract)
                    continue;

                var typeElement = new XmlSchemaElement();
                typeElement.RefName = NameUsed(type.Alias.ToQualifiedName());
                rootChildrenGroup.Particle.Items.Add(typeElement);
            }

            // All relationships
            var relationships = EffectiveRelationships(_schemaManager.GetInstancesOfType(A(Aliases.Relationship)));

            foreach (EffectiveRelationship relationship in relationships)
            {
                var relElement = new XmlSchemaElement();
                relElement.RefName = NameUsed(relationship.Alias.ToQualifiedName(suffix: XmlParser.RelationshipInstanceSuffix));
                rootChildrenGroup.Particle.Items.Add(relElement);
            }

            XmlSchema xsd = GetSchema(Aliases.CoreNamespace);
            xsd.Items.Insert(0, rootChildrenGroup);
        }


        /// <summary>
        /// Add scalar field definitions.
        /// </summary>
        private void AddFields()
        {
            var fields = _schemaManager.GetInstancesOfType(A(Aliases.Field));

            foreach (Entity field in fields)
            {
                if (field.Alias == null)
                    continue;

                XmlSchemaType fieldType = GetFieldSchemaType(field);
                
                // Create the element declaration
                XmlSchemaElement fieldElem = new XmlSchemaElement();
                fieldElem.Name = NameDeclared(field.Alias.Value, field.Alias.Namespace);
                fieldElem.SchemaType = fieldType;

                // Add to schema
                XmlSchema xsd = GetSchema(field.Alias.Namespace);
                xsd.Items.Add(fieldElem);
            }
        }

        /// <summary>
        /// Gets the schema type object to represent this field type.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private XmlSchemaType GetFieldSchemaType(Entity field)
        {
            // Lookup the field type
            var typeEntity = _aliasResolver[field.Type];
            if (!_fieldTypeToXsdType.ContainsKey(typeEntity))
                throw new BuildException("Could not determine XSD type for " + typeEntity.ToString(), typeEntity);

            // Look up the XSD type
            string typeName = _fieldTypeToXsdType[typeEntity];

            if (typeName == "platform-custom-xml")
            {
                // Create a 'custom xml' field
                return GetXmlFieldType(field);
            }
            else
            {
                // Create a simple type
                XmlSchemaSimpleTypeRestriction fieldRestriction = new XmlSchemaSimpleTypeRestriction();
                fieldRestriction.BaseTypeName = new XmlQualifiedName(typeName, "http://www.w3.org/2001/XMLSchema");
                var simpleType = new XmlSchemaSimpleType();
                simpleType.Content = fieldRestriction;

                AddFieldRestrictions(fieldRestriction, field, typeEntity.Alias.Value);

                return simpleType;
            }
        }


        /// <summary>
        /// Creates the complex type to represent a 'custom XML' field.
        /// </summary>
        private XmlSchemaType GetXmlFieldType(Entity fieldEntity)
        {
            //  <xs:complexType>
            //    <xs:sequence>
            //      <xs:element name="xml" minOccurs='1' maxOccurs='1'>
            //        <xs:complexType>
            //          <xs:sequence>
            //            <xs:any minOccurs='0' maxOccurs='1' processContents='lax'/>
            //          </xs:sequence>
            //        </xs:complexType>
            //      </xs:element>
            //    </xs:sequence>
            //  </xs:complexType>

            // Define the complex type for the actual custom-content itself.
            // Lax: The XML processor attempts to obtain the schema for the required namespaces and validate any element from those namespaces; however, if the schema cannot be obtained, no errors will occur.
            var any = new XmlSchemaAny()
            {
                MinOccurs = 0,
                MaxOccurs = 1,
                ProcessContents = XmlSchemaContentProcessing.Lax
            };
            var innerSequence = new XmlSchemaSequence();
            innerSequence.Items.Add(any);
            var innerComplex = new XmlSchemaComplexType();
            innerComplex.Particle = innerSequence;

            // Determine namespace
            string xmlNamespace = _schemaManager.GetStringFieldValue(fieldEntity, Aliases2.XmlFieldNamespace);
            if (xmlNamespace != null)
            {
                any.Namespace = xmlNamespace;
                any.ProcessContents = XmlSchemaContentProcessing.Strict;
            }

            // Define the complex type for the outer 'xml' element itself
            var xmlElement = new XmlSchemaElement()
            {
                Name = "xml",
                MinOccurs = 1,
                MaxOccurs = 1,
                SchemaType = innerComplex
            };
            var outerSequence = new XmlSchemaSequence();
            outerSequence.Items.Add(xmlElement);
            var xmlComplex = new XmlSchemaComplexType();
            xmlComplex.Particle = outerSequence;

            return xmlComplex;
        }


        /// <summary>
        /// Render field-criteria such as max length and patterns.
        /// </summary>
        /// <param name="restrictions">XSD restriction object to receive the facets.</param>
        /// <param name="fieldEntity">The field being described.</param>
        /// <param name="fieldTypeAlias">The XSD type string for the field type. E.g. 'string'.</param>
        private void AddFieldRestrictions(XmlSchemaSimpleTypeRestriction restrictions, Entity fieldEntity, string fieldTypeAlias)
        {
            switch (fieldTypeAlias)
            {
                // Int constraints
                case "intField":
                    int? minValue = _schemaManager.GetIntFieldValue(fieldEntity, Aliases2.MinInt);
                    if (minValue != null)
                    {
                        var minFacet = new XmlSchemaMinInclusiveFacet() { Value = minValue.ToString() };
                        restrictions.Facets.Add(minFacet);
                    }
                    int? maxValue = _schemaManager.GetIntFieldValue(fieldEntity, Aliases2.MaxInt);
                    if (maxValue != null)
                    {
                        var maxFacet = new XmlSchemaMaxInclusiveFacet() { Value = maxValue.ToString() };
                        restrictions.Facets.Add(maxFacet);
                    }
                    break;

                // String constraints
                case "stringField":
                    int? minLength = _schemaManager.GetIntFieldValue(fieldEntity, Aliases2.MinLength);
                    if (minLength != null)
                    {
                        var minFacet = new XmlSchemaMinLengthFacet() { Value = minLength.ToString() };
                        restrictions.Facets.Add(minFacet);
                    }
                    int? maxLength = _schemaManager.GetIntFieldValue(fieldEntity, Aliases2.MaxLength);
                    if (maxLength != null)
                    {
                        var maxFacet = new XmlSchemaMaxLengthFacet() { Value = maxLength.ToString() };
                        restrictions.Facets.Add(maxFacet);
                    }
                    var stringPattern = _schemaManager.GetRelationshipsFromEntity(fieldEntity, A(Aliases2.Pattern)).FirstOrDefault();
                    if (stringPattern != null)
                    {
                        string sRegex = _schemaManager.GetStringFieldValue(stringPattern, Aliases2.Regex);
                        if (sRegex != null)
                        {
                            var regexFacet = new XmlSchemaPatternFacet() { Value = sRegex };
                            restrictions.Facets.Add(regexFacet);
                        }
                    }
                    break;

                // String constraints
                case "aliasField":
                    // optional namespace prefix, followed by lower-case alpha, alphanumeric
                    string aliasRegex = @"([_a-zA-Z][_a-zA-Z0-9]*\:)?[_a-zA-Z][_a-zA-Z0-9]{0,99}";
                    var aliasRegexFacet = new XmlSchemaPatternFacet() { Value = aliasRegex };
                    restrictions.Facets.Add(aliasRegexFacet);
                    break;
            }

        }


        /// <summary>
        /// Generate XSD schema info for entity types.
        /// One complexType is registered for each type, which contains the fields and relationships of that type.
        /// One group is registered for each type, which contains the type itself, and all derived types that may be used in its place.
        /// </summary>
        private void AddTypes()
        {
            var types = EffectiveTypes(_schemaManager.GetInstancesOfType(A(Aliases.Type)));
            
            foreach (EffectiveType entityType in types)
            {
                // Ensure all types inherit something
                if (!_schemaManager.GetAncestors(entityType.Type).Skip(1).Any())
                    if (entityType.Alias != Aliases2.Resource)
                        throw new BuildException(string.Format("{0} does not inherit from anything.", entityType.Alias), entityType.Alias);

                // Generate XSD
                XmlSchema xsd = GetSchema( entityType.Alias.Namespace );

				AddTypeElement( entityType, xsd );
				AddTypeMembers( entityType, xsd );
				AddTypeDescendents( entityType, xsd );
            }
        }


        /// <summary>
        /// Declares the element type for this entity. Refers to the the complex type that describes the members.
        /// Is referred to by the 'is a' groups.
        /// </summary>
        /// <param name="entityType">The type entity.</param>
        private void AddTypeElement(EffectiveType entityType, XmlSchema xsd)
        {
            // Example:
            //<xs:element name="person" type="type_person" />

            var typeElement = new XmlSchemaElement();
            typeElement.Name = NameDeclared(entityType.Alias.Value, entityType.Alias.Namespace);
            typeElement.SchemaTypeName = entityType.Alias.ToQualifiedName(prefix:"type_");
            xsd.Items.Add(typeElement);
        }


        /// <summary>
        /// Declares 'field' members for a type.
        /// That is, all fields that apply to this type, either directly or through inheritance.
        /// </summary>
        /// <param name="entityType">The type entity.</param>
        private void AddTypeMembers(EffectiveType entityType, XmlSchema xsd)
        {
            // Example of member declaration
            // This contains both fields and relationships (and reverse relationships)
            //<xs:complexType name="type_person">
            //  <xs:all>
            //    <!-- fields -->
            //    <xs:element minOccurs="0" maxOccurs="1" ref="name" />
            //    <xs:element minOccurs="0" maxOccurs="1" ref="description" />
            //    <xs:element minOccurs="0" maxOccurs="1" ref="alias" />
            //    <xs:element minOccurs="0" maxOccurs="1" ref="firstName" />
            //    <xs:element minOccurs="0" maxOccurs="1" ref="lastName" />
            //    <!-- relationships -->
            //    <xs:element minOccurs="0" maxOccurs="1" ref="worksFor" /> 
            //  </xs:all>
            //</xs:complexType>

            // Create a complex type, named "type_%typename%"
            var typeMembers = new XmlSchemaComplexType();
            typeMembers.Name = NameDeclared("type_" + entityType.Alias.Value, entityType.Alias.Namespace);

            // Create a group to hold the members
            var memberGroup = new XmlSchemaAll();
            typeMembers.Particle = memberGroup;

            // Add members
            AddTypeFields(entityType, memberGroup);
            AddTypeRelationships(entityType, memberGroup);

            // Add 'source' attribute for file types
            Entity file = _aliasResolver[Aliases2.File];
            if (_schemaManager.IsSameOrDerivedType(entityType.Type, file))
            {
                XmlSchemaAttribute sourceAttrib = new XmlSchemaAttribute();
                sourceAttrib.Name = "source";
                sourceAttrib.SchemaTypeName = XmlSchemaSimpleType.GetBuiltInSimpleType(XmlTypeCode.String).QualifiedName;
                typeMembers.Attributes.Add(sourceAttrib);
            }

            xsd.Items.Add(typeMembers);
        }


        /// <summary>
        /// Declares 'field' members for a type.
        /// That is, all fields that apply to this type, either directly or through inheritance.
        /// </summary>
        /// <param name="entityType">The type entity.</param>
        /// <param name="memberGroup">The XSD container that will hold the members.</param>
        private void AddTypeFields(EffectiveType entityType, XmlSchemaGroupBase memberGroup)
        {
            // Example:
            // <xs:element minOccurs="0" maxOccurs="1" ref="firstName" />

            // Define fields applicable to type
            // This is defining that a given field can appear under a resource of a particular type
            var fields =
                from type in _schemaManager.GetAncestors(entityType.Type)
                from field in _schemaManager.GetDeclaredFields(type)
                select field;
            foreach (Entity field in fields)
            {
                if (field.Alias == null)
                    continue;
                var fieldElem = new XmlSchemaElement();
                fieldElem.RefName = NameUsed(field.Alias.ToQualifiedName());
                fieldElem.MaxOccurs = 1;

                bool required =
                    (_schemaManager.GetBoolFieldValue(field, Aliases2.IsRequired)
                    && _schemaManager.GetStringFieldValue(field, Aliases2.DefaultValue) == null
                    )
                    || field.Alias == Aliases.Alias;    // special case: alias is required in config, but not at runtime

                fieldElem.MinOccurs = required ? 1 : 0;
                memberGroup.Items.Add(fieldElem);
            }
        }


        /// <summary>
        /// Declares 'relationship' members for a type.
        /// That is, all forward or reverse relationships that can connect to this type at one end or the other.
        /// </summary>
        /// <param name="entityType">The type entity.</param>
        /// <param name="memberGroup">The XSD container that will hold the members.</param>
        private void AddTypeRelationships(EffectiveType entityType, XmlSchemaGroupBase memberGroup)
        {
            // Define relationships applicable to type
            // This is defining that a given relationship container can appear under a resource of a particular type
            // e.g. <xs:element ref="worksFor" minOccurs="0" maxOccurs="1"/>
            // Note: maxOccurs refers to the relationship container, not the cardinality of the relationship itself.

            var relationships =
                from type in _schemaManager.GetAncestors(entityType.Type)
                from relationshipDefn in _schemaManager.GetRelationshipsToEntity(type, A(Aliases.FromType))
                select new { Relationship = relationshipDefn, Alias = relationshipDefn.Alias };

            var relationshipsRev =
                from type in _schemaManager.GetAncestors(entityType.Type)
                from relationshipDefn in _schemaManager.GetRelationshipsToEntity(type, A(Aliases.ToType))
                select new { Relationship = relationshipDefn, Alias = relationshipDefn.ReverseAlias };

            HashSet<XmlQualifiedName> hash = new HashSet<XmlQualifiedName>();
            foreach (var rel in relationships.Concat(relationshipsRev))
            {
                if (rel.Alias == null || string.IsNullOrEmpty(rel.Alias.Value))
                    continue;
                var fieldElem = new XmlSchemaElement();
                fieldElem.MaxOccurs = 1;    // the relationship container, not the relationships themselves
                fieldElem.MinOccurs = 0;
                fieldElem.RefName = NameUsed(rel.Alias.ToQualifiedName());
                if (hash.Contains(fieldElem.RefName))
                    throw new Exception("Already used: " + fieldElem.RefName.ToString());   // note: this can also get triggered if an inline-relationship defines a fromType node.
                hash.Add(fieldElem.RefName);
                memberGroup.Items.Add(fieldElem);
            }
        }


        /// <summary>
        /// Creates a group that contains all of the types that inherit from a given type,
        /// as well as the type itself. Reference this group when any inherited type is suitable.
        /// </summary>
        private void AddTypeDescendents(EffectiveType entityType, XmlSchema xsd)
        {
            // Example of inheritance declaration
            //<xs:group name="is_person">
            //  <xs:choice>
            //    <xs:element ref="person" />
            //    <xs:element ref="employee" />
            //    <xs:element ref="manager" />
            //  </xs:choice>
            //</xs:group>

            // Create a group, named "is_%typename%"
            var isType = new XmlSchemaGroup();
            isType.Name = NameDeclared("is_" + entityType.Alias.Value, entityType.Alias.Namespace);
            isType.Particle = new XmlSchemaChoice();

            // Determine valid descendents
            IEnumerable<Entity> descendents;
            bool isSealed = _schemaManager.GetBoolFieldValue(entityType.Type, Aliases2.IsSealed);
            if (isSealed)
                descendents = new List<Entity> { entityType.Type };
            else
                descendents = _schemaManager.GetDecendants(entityType.Type);

            // Render each type
            foreach (EffectiveType desc in EffectiveTypes(descendents))
            {
                bool isAbstract = _schemaManager.GetBoolFieldValue(desc.Type, Aliases2.IsAbstract);
                if (isAbstract)
                    continue;

                var derivedType = new XmlSchemaElement();
                derivedType.RefName = NameUsed(desc.Alias.ToQualifiedName());
                isType.Particle.Items.Add(derivedType);
            }
            xsd.Items.Add(isType);
        }

        
        /// <summary>
        /// Generate XSD schema info for relationships.
        /// Specifically, register a named complexTyped, which can be referred to from entity types that are valid endpoints.
        /// </summary>
        private void AddRelationships()
        {
            // Note: both of either explicit relationship instances, or direct instances of the foreign type, can be embedded here.
            // However due to XSD limitations, the former must appear before the latter in the XML files.

            // E.g.
            //  <xs:element name="fieldIsOnType">
            //    <xs:complexType mixed="true">
            //      <xs:sequence>
            //        <xs:element minOccurs="0" maxOccurs="unbounded" ref="fieldIsOnType.instance" />
            //        <xs:group minOccurs="0" maxOccurs="unbounded" ref="is_type" />
            //      <xs:sequence>
            //    </xs:complexType>
            //  </xs:element>

            var relationships = EffectiveRelationships(_schemaManager.GetInstancesOfType(A(Aliases.Relationship)));

            // Prefetch the cardinality enum values

            foreach (EffectiveRelationship relationship in relationships)
            {
                XmlSchema xsd = GetSchema(relationship.Alias.Namespace);

                // Provide for explicit relationships instances that specify from/to
                AddRelationshipInstanceElement(relationship, xsd);

                if (relationship.IsSingleEnum)
                {
                    AddEnumValues(relationship);
                    continue;
                }

                // Get expected child type
                Entity toType;
                if (relationship.IsReverseAlias)
                    toType = _schemaManager.GetRelationshipFromType(relationship.Type);
                else
                    toType = _schemaManager.GetRelationshipToType(relationship.Type);

                // Determine cardinality
                var cardinality = _schemaManager.GetCardinality(relationship.Type);

                bool isBounded =
                    cardinality == Aliases.OneToOne
                    || cardinality == Aliases.ManyToOne && !relationship.IsReverseAlias
                    || cardinality == Aliases.OneToMany && relationship.IsReverseAlias;
                
                string maxOccurs = isBounded ? "1" : "unbounded";

                // Build XSD
                var sequence = new XmlSchemaSequence();
                sequence.Items.Add(new XmlSchemaGroupRef()
                {
                    RefName = NameUsed(toType.Alias.ToQualifiedName("is_", null)),
                    MinOccurs = 0,
                    MaxOccursString = maxOccurs
                });
                var complexType = new XmlSchemaComplexType()
                {
                    IsMixed = true,
                    Particle = sequence
                };

                var relElement = new XmlSchemaElement();
                relElement.Name = NameDeclared(relationship.Alias.Value, relationship.Alias.Namespace);
                relElement.SchemaType = complexType;

                // Add to schema
                xsd.Items.Add(relElement);
            }
        }

        /// <summary>
        /// Declares the ability to have specific relationship instances that connect a 'from' to a 'to'.
        /// </summary>
        /// <param name="entityType">The type entity.</param>
        private void AddRelationshipInstanceElement(EffectiveRelationship entityType, XmlSchema xsd)
        {
            // Example:
            //<xs:element name="worksFor.instance" type="ri_worksFor" />

            var typeElement = new XmlSchemaElement();
            typeElement.Name = NameDeclared(entityType.Alias.Value + XmlParser.RelationshipInstanceSuffix, entityType.Alias.Namespace);
            typeElement.SchemaTypeName = entityType.Alias.ToQualifiedName(prefix: "ri_");
            xsd.Items.Add(typeElement);


            // Example of member declaration
            // This contains both fields and relationships (and reverse relationships)
            //<xs:complexType name="ri_worksFor">
            //  <xs:all>
            //    <!-- explicit relationships -->
            //    <xs:element minOccurs="1" maxOccurs="1" name="from" type="is_fromType" />           
            //    <xs:element minOccurs="1" maxOccurs="1" name="to" type="is_toType" />           
            //  </xs:all>
            //</xs:complexType>

            // Create a complex type, named "ri_%typename%"
            var typeMembers = new XmlSchemaComplexType();
            typeMembers.Name = NameDeclared("ri_" + entityType.Alias.Value, entityType.Alias.Namespace);

            // Create a group to hold the members
            var memberGroup = new XmlSchemaAll();
            typeMembers.Particle = memberGroup;

            // Add from/to
            if (entityType.IsReverseAlias)
            {
                AddRelationshipInstanceMember(entityType, memberGroup, "to", _schemaManager.GetRelationshipFromType(entityType.Type), required: true);
                AddRelationshipInstanceMember(entityType, memberGroup, "from", _schemaManager.GetRelationshipToType(entityType.Type), required: true);
            }
            else
            {
                AddRelationshipInstanceMember(entityType, memberGroup, "from", _schemaManager.GetRelationshipFromType(entityType.Type), required: true);
                AddRelationshipInstanceMember(entityType, memberGroup, "to", _schemaManager.GetRelationshipToType(entityType.Type), required: true);
            }

            xsd.Items.Add(typeMembers);
        }

        /// <summary>
        /// Declares 'field' members for a type.
        /// That is, all fields that apply to this type, either directly or through inheritance.
        /// </summary>
        /// <param name="entityType">The type entity.</param>
        /// <param name="memberGroup">The XSD container that will hold the members.</param>
        private void AddRelationshipInstanceMember(EffectiveRelationship entityType, XmlSchemaGroupBase memberGroup, string elementName, Entity targetType, bool required)
        {
            //  <xs:element name="fields">
            //    <xs:complexType mixed="true">
            //      <xs:group minOccurs="0" maxOccurs="unbounded" ref="is_field" />
            //    </xs:complexType>
            //  </xs:element>

            XmlSchemaElement elem = new XmlSchemaElement();
            elem.MinOccurs = required ? 1 : 0;
            elem.MaxOccurs = 1;
            elem.Name = elementName;
            elem.SchemaType = new XmlSchemaComplexType()
            {
                IsMixed = true,
                Particle = new XmlSchemaGroupRef()
                {
                    MinOccurs = 0,  // zero, since we might refer by alias text
                    MaxOccurs = 1,
                    RefName = targetType.Alias.ToQualifiedName(prefix: "is_")
                }
            };
            memberGroup.Items.Add(elem);
        }

        /// <summary>
        /// Generate valid values for an enum.
        /// </summary>
        private void AddEnumValues(EffectiveRelationship relationship)
        {
            var enumType = _schemaManager.GetRelationshipToType(relationship.Type);
            var instances = _schemaManager.GetInstancesOfType(enumType);
            
            var restriction = new XmlSchemaSimpleTypeRestriction();
            restriction.BaseTypeName = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String).QualifiedName;
            foreach (Entity instance in instances)
            {
                XmlSchemaEnumerationFacet facet = new XmlSchemaEnumerationFacet();
                if (instance.Alias.Namespace == "core")
                    facet.Value = instance.Alias.Value;
                else
                    facet.Value = instance.Alias.Namespace + ":" + instance.Alias.Value;
                restriction.Facets.Add(facet);
            }


            var simpleType = new XmlSchemaSimpleType();
            simpleType.Content = restriction;

            var relElement = new XmlSchemaElement();
            relElement.Name = NameDeclared(relationship.Alias.Value, relationship.Alias.Namespace);
            relElement.SchemaType = simpleType;

            // Add to schema
            XmlSchema xsd = GetSchema(relationship.Alias.Namespace);
            xsd.Items.Add(relElement);
        }


        class EffectiveType
        {
            public Entity Type;

            public Alias Alias;
        }

        class EffectiveRelationship
        {
            public Entity Type;

            public Alias Alias;

            public bool IsReverseAlias;

            public bool IsSingleEnum;
        }

        /// <summary>
        /// Given a stream of types, returns a stream of types where any 'relationship' types are duplicated so that their
        /// reverse alias also appears in the output stream.
        /// Caution: remember to invert the from/to types, and the cardinality, as applicable.
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        IEnumerable<EffectiveType> EffectiveTypes(IEnumerable<Entity> types)
        {
            foreach (var type in types)
            {
                bool isRelationship = _schemaManager.IsInstance(type, A(Aliases.Relationship));
                if (isRelationship)
                    continue;

                var et = new EffectiveType() { Type = type, Alias = type.Alias };
                yield return et;
            }
        }

        /// <summary>
        /// Given a stream of relationships, returns a stream where applicable relationships are duplicated so that their
        /// reverse alias also appears in the output stream.
        /// Caution: remember to invert the from/to types, and the cardinality, as applicable.
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        IEnumerable<EffectiveRelationship> EffectiveRelationships(IEnumerable<Entity> relationships)
        {
            foreach (var type in relationships)
            {
                bool isSingleEnum = false;
                var card = _schemaManager.GetCardinality(type);
                if (card == Aliases.ManyToOne)
                {
                    var relToType = _schemaManager.GetRelationshipToType(type);
                    var entType = _schemaManager.GetEntityType(relToType).Alias;
                    isSingleEnum = entType == Aliases.EnumType;
                }
                var et = new EffectiveRelationship() { Type = type, Alias = type.Alias, IsSingleEnum = isSingleEnum };
                yield return et;

                if (type.ReverseAlias != null)
                {
                    var rev = new EffectiveRelationship() { Type = type, Alias = type.ReverseAlias, IsReverseAlias = true, IsSingleEnum = false };
                    yield return rev;
                }
            }
        }

        /// <summary>
        /// Helper method to resolve aliases in as few characters as possible.
        /// </summary>
        private Entity A(Alias alias)
        {
            return _aliasResolver[alias];
        }

        private XmlQualifiedName NameUsed(XmlQualifiedName name)
        {
            _namesUsed.Add(name);
            return name;
        }

        private string NameDeclared(string name, string ns)
        {
            var qn = new XmlQualifiedName(name, ns);
            if (_namesDeclared.Contains(qn))
                throw new Exception(string.Format("{0}:{1} already declared"));
            _namesDeclared.Add(qn);
            return name;
        }

        

    }
}
