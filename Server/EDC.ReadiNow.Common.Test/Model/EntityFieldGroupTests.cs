// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.ReadiNow.Model;
using NUnit.Framework;

namespace EDC.ReadiNow.Test.Model
{
    [TestFixture]
	[RunWithTransaction]
    public class EntityFieldGroupTests
    {
        /// <summary>
        ///     The product installation date.
        /// </summary>
        private DateTime _productInstallationDate;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        private bool IsInTestSolution(Resource resource)
        {
	        return resource.InSolution != null && resource.InSolution.Alias == "core:testSolution";
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private bool SkipType(EntityType entityType)
        {
            bool skipType = false;
            
            if (entityType != null)
            {
                if (entityType.EntityTypes.Any(et => et.Namespace == "core" && (et.Alias == "fieldType" || et.Alias == "enumType")) || entityType.Alias == "core:fieldGroup")
                {
                    skipType = true;
                }                
            }

            return skipType;
        }


        /// <summary>
        ///     Setup the test fixture.
        /// </summary>
        [TestFixtureSetUp]
        public void Initialise()
        {
            // Get the product installation date
            _productInstallationDate = TestHelpers.GetReadiNowProductInstallationDate();
        }

            /// <summary>
        /// 
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void EnsureFieldsAreAssignedToFieldGroups()
        {
            var errors = new StringBuilder();            

            IEnumerable<Field> fields = Entity.GetInstancesOfType<Field>();            

            // Ensure fields are assigned to field groups
            foreach (Field field in fields)
            {
                // Skip entities whose created date is after the installation date
                DateTime? entityCreatedDate = field.CreatedDate;
                if (entityCreatedDate != null &&
                    entityCreatedDate.Value.ToLocalTime() > _productInstallationDate)
                {
                    continue;
                }

                if (IsInTestSolution(field.As<Resource>()) ||
                    SkipType(field.FieldIsOnType))
                {
                    // Skip entities that are only in the test solution
                    continue;
                }
                
                if (field.HideField ?? false)
                {
                    continue;
                }

                if (field.FieldInGroup != null)
                {
                    continue;                        
                }

                EntityType type = field.FieldIsOnType;
                long typeId = 0;
                string typeAlias = string.Empty;
                string groups = string.Empty;
                if (type != null)
                {
                    typeId = type.Id;
                    typeAlias = type.Alias;
                    if (type.FieldGroups != null)
                    {
                        var groupAliases = type.FieldGroups.Select(f => f.Alias);
                        groups = string.Join(", ", groupAliases);
                    }
                }

                errors.AppendFormat("Field id:{0} alias:{1} has no group. Entity id:{2} alias:{3} groups:{4}", field.Id, field.Alias, typeId, typeAlias, groups);
                errors.AppendLine();                
            }                                

            Assert.IsNullOrEmpty(errors.ToString());
        }


        /// <summary>
        /// Ensure that relationships are assigned to field groups.
        /// </summary>
        [Test]
        [RunAsDefaultTenant]
        public void EnsureRelationshipsAreAssignedToFieldGroups()
        {
            var errors = new StringBuilder();

            IEnumerable<Relationship> relationships = Entity.GetInstancesOfType<Relationship>();
           
            // Ensure forward relationships are assigned to field groups
            foreach (Relationship relationship in relationships)
            {
                if (!TestHelpers.InValidatableSolution(relationship.InSolution))
                    continue;
				
                // Skip entities whose created date is after the installation date
                DateTime? entityCreatedDate = relationship.CreatedDate;
                if (entityCreatedDate != null &&
                    entityCreatedDate.Value.ToLocalTime() > _productInstallationDate)
                {
                    continue;
                }

                if (IsInTestSolution(relationship.As<Resource>()))
                {
                    continue;
                }

                bool skipToGroup = SkipType(relationship.ToType);
                bool skipFromGroup = SkipType(relationship.FromType);                    
                bool hideOnFromType = (relationship.HideOnFromType ?? false);
                bool hideOnToType = (relationship.HideOnToType ?? false);

                if (!skipFromGroup &&
                    !hideOnFromType &&
                    relationship.RelationshipInFromTypeGroup == null)
                {
                    EntityType type = relationship.FromType;
                    long typeId = 0;
                    string typeAlias = string.Empty;
                    string groups = string.Empty;
                    if (type != null)
                    {
                        typeId = type.Id;
                        typeAlias = type.Alias;
                        if (type.FieldGroups != null)
                        {
                            var groupAliases = type.FieldGroups.Select(f => f.Alias);
                            groups = string.Join(", ", groupAliases);
                        }
                    }

                    errors.AppendFormat("Relationship id:{0} alias:{1} has no 'from' group. Entity id:{2} alias:{3} groups:{4}", relationship.Id, relationship.Alias, typeId, typeAlias, groups);
                    errors.AppendLine();
                }              

                if (!skipToGroup &&
                    !hideOnToType &&
                    relationship.RelationshipInToTypeGroup == null)
                {
                    EntityType type = relationship.ToType;
                    long typeId = 0;
                    string typeAlias = string.Empty;
                    string groups = string.Empty;
                    if (type != null)
                    {
                        typeId = type.Id;
                        typeAlias = type.Alias;
                        if (type.FieldGroups != null)
                        {
                            var groupAliases = type.FieldGroups.Select(f => f.Alias);
                            groups = string.Join(", ", groupAliases);
                        }
                    }

                    errors.AppendFormat("Relationship id:{0} alias:{1} has no 'to' group. Entity id:{2} alias:{3} groups:{4}", relationship.Id, relationship.Alias, typeId, typeAlias, groups);
                    errors.AppendLine();
                }
            }                

            Assert.IsNullOrEmpty(errors.ToString());
        }
    }
}
