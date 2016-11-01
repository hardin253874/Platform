// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDC.Collections;
using EDC.Globalization;

using EDC.ReadiNow.Core;
using EDC.ReadiNow.IO;
using EDC.ReadiNow.Metadata;
using EDC.ReadiNow.Metadata.Data;
using EDC.ReadiNow.Metadata.Tenants;
using EDC.ReadiNow.Metadata.Query;

namespace EDC.Test
{
    /// <summary>
    /// Shared Static methods for Resource Unit Test
    /// </summary>
    public class TestHelper
    {
        /// <summary>
        /// Test Resource Definition Exists
        /// </summary>
        public static bool Test_Definition_ResourceExists(Guid ResourceDefinitionId)
        {
            bool isExisting = false;
            try
            {
                IResource resourceDefinition = Resource.Get(ResourceDefinitionId);
                if (resourceDefinition != null && resourceDefinition is IResourceDefinition)
                {
                    isExisting = true;
                }
            }
            catch 
            {
                isExisting = false;
            }

            return isExisting;
            
        }
        /// <summary>
        /// Test Resource Table Exists
        /// </summary>
        public static bool Test_Table_ResourceExists(Guid ResourceTableId)
        {
            bool isExisting = false;
            try
            {
                IResource resource = Resource.Get(ResourceTableId);

                if (resource != null)
                {
                    if (resource is IResourceDataTable)
                    {
                        isExisting = true;
                    }
                }
            }
            catch
            {
                isExisting = false;
            }

            return isExisting;
        }

        /// <summary>
        /// Test Resource Key Exists
        /// </summary>
        public static bool Test_Key_ResourceExists(Guid ResourceKeyId)
        {
            bool isExisting = false;
            try
            {
                IResource resource = Resource.Get(ResourceKeyId);

                if (resource != null)
                {
                    if (resource is ResourceDataKey)
                    {
                        isExisting = true;
                    }
                }
            }
            catch
            {
                isExisting = false;
            }

            return isExisting;
        }


        /// <summary>
        /// Test the Table Definition contains All tables which belong it
        /// </summary>
        /// <param name="DefinitionId"></param>
        /// <param name="ResourceTableIds">List of Table Guid</param>
        public static bool Test_DefinitionContainsTables_ResourceExists(Guid DefinitionId, GuidCollection ResourceTableIds)
        {
            bool isExisting = false;
            try
            {
                IResource resourceDefinition = Resource.Get(DefinitionId);

                int tableIdsCounter = ResourceTableIds.Count;
                //Get the definition tables
                GuidCollection tables = ((IResourceDefinition)resourceDefinition).Tables;

                //check if the supplied tables exists

                foreach (Guid table in tables)
                {
                    foreach (Guid tableId in ResourceTableIds)
                    {
                        if (table == tableId)
                        {
                            tableIdsCounter--;
                            
                        }
                    }
                }
                if (tableIdsCounter == 0)
                    isExisting = true;
            }
            catch
            {
                isExisting = false;
            }


            return isExisting;
        }


        /// <summary>
        /// Test the Table Definition contains All keys which belong it
        /// </summary>
        /// <param name="DefinitionId"></param>
        /// <param name="ResourceTableIds">List of key Guid</param>
        public static bool Test_DefinitionContainsKeys_ResourceExists(Guid DefinitionId, GuidCollection ResourceKeyIds)
        {
            bool isExisting = false;
            try
            {
                IResource resourceDefinition = Resource.Get(DefinitionId);

                int keyIdsCounter = ResourceKeyIds.Count;
                //Get the definition keys
                GuidCollection keys = ((IResourceDefinition)resourceDefinition).Keys;

                //check if the supplied keys exists

                foreach (Guid key in keys)
                {
                    foreach (Guid keyId in ResourceKeyIds)
                    {
                        if (key == keyId)
                        {
                            keyIdsCounter--;

                        }
                    }
                }
                if (keyIdsCounter == 0)
                    isExisting = true;
            }
            catch
            {
                isExisting = false;
            }


            return isExisting;
        }


        /// <summary>
        /// Test the Table Definition contain the key
        /// </summary>
        public static bool Test_DefinitionContainsKey_ResourceExists(Guid DefinitionId, Guid ResourceKeyId)
        {
            bool isExisting = false;
            try
            {
                IResource resourceDefinition = Resource.Get(DefinitionId);

                //Get the definition keys
                GuidCollection keys = ((IResourceDefinition)resourceDefinition).Keys;

                //check if the supplied key exists
                foreach (Guid key in keys)
                {
                    if (key == ResourceKeyId)
                    {
                        isExisting = true;
                        break;
                    }
                }
            }
            catch
            {
                isExisting = false;
            }


            return isExisting;
        }

        /// <summary>
        /// Test the Table Definition contain the table
        /// </summary>
        /// <param name="DefinitionId"></param>
        /// <param name="ResourceTableId"></param>
        public static bool Test_DefinitionContainsTable_ResourceExists(Guid DefinitionId, Guid ResourceTableId)
        {
            bool isExisting = false;
            try
            {
                IResource resourceDefinition = Resource.Get(DefinitionId);

                //Get the definition tables
                GuidCollection tables = ((IResourceDefinition)resourceDefinition).Tables;

                //check if the supplied table exists
                foreach (Guid table in tables)
                {
                    if (table == ResourceTableId)
                    {
                        isExisting = true;
                        break;
                    }
                }
            }
            catch
            {
                isExisting = false;
            }


            return isExisting;
        }
        /// <summary>
        /// Test Query resource exists
        /// </summary>
        public static bool Test_Query_ResourceExists(Guid ResourceQueryId)
        {
            bool isExisting = false;
            try
            {
                //Get the resource from the database
                IResource resourceQuery = Resource.Get(ResourceQueryId);
                if (resourceQuery != null && resourceQuery is IResourceQuery)
                {
                    isExisting = true;
                }

            }
            catch
            {
                isExisting = false;
            }
            return isExisting;
        }
        /// <summary>
        /// Test Relationship Constraint resource exists
        /// </summary>
        public static bool Test_RelationshipConstraint_ResourceExist(Guid ResourceConstraintId)
        {
            bool isExisting = false;
            try
            {
                //get the constraint by Guid
                IResource resourceConstraint = Resource.Get(ResourceConstraintId);

                //check if constraint is not null and the type is IResourceRelationshipConstraint
                if (resourceConstraint != null && resourceConstraint is IResourceRelationshipConstraint)
                {
                    isExisting = true;
                }

 
            }
            catch
            {
                isExisting = false;
            }
            return isExisting;
        }

        /// <summary>
        /// Test all Relationship Constraints resource exists
        /// </summary>
        public static bool Test_RelationshipConstraints_ResourceExist(GuidCollection ResourceConstraintIds)
        {
            bool isExisting = false;
            
            try
            {
                int constraintCounter = ResourceConstraintIds.Count;
                foreach (Guid contraintId in ResourceConstraintIds)
                {
                    //get the constraint by Guid
                    IResource resourceConstraint = Resource.Get(contraintId);

                    //check if constraint is not null and the type is IResourceRelationshipConstraint
                    if (resourceConstraint != null && resourceConstraint is IResourceRelationshipConstraint)
                    {
                        constraintCounter--;
                    }
                }
                if(constraintCounter==0)
                    isExisting = true;

            }
            catch
            {
                isExisting = false;
            }
            return isExisting;
        }

        /// <summary>
        ///     Test all Relationship definition resource exists
        /// </summary>
        public static bool Test_RelationshipDefinition_ResourceExists(Guid ResourceRelationshipId)
        {
            bool isExisting = false;
            try
            {
                //get the relationship from the database
                IResource resourceRelationship = Resource.Get(ResourceRelationshipId);

                //check if the relationship is not null and the type is IResourceRelationship
                if (resourceRelationship != null && resourceRelationship is IResourceRelationshipDefinition)
                {
                    isExisting = true;
                }
      
            }
            catch
            {
                isExisting = false;
            }
            return isExisting;
        }
        /// <summary>
        /// Test All Relationship Constraint in the relationship definition
        /// </summary>
        /// <param name="ResourceRelationShipDefinitonId"></param>
        /// <param name="RelationshipConstraintIds"></param>
        /// <returns></returns>
        public static bool Test_RelationshipDefinition_ContainConstraints(Guid ResourceRelationShipDefinitonId, GuidCollection RelationshipConstraintIds)
        {
            bool isExisting = false;
            try
            {
                //get the relationship from the database
                IResource resourceRelationship = Resource.Get(ResourceRelationShipDefinitonId);

                int constraintIdCounter = RelationshipConstraintIds.Count;

                //get  the relationship constaints
                GuidCollection constraints = ((IResourceRelationshipDefinition)resourceRelationship).RelationshipConstraints;

                foreach (Guid constraint in constraints)
                {
                    foreach (Guid constraintId in RelationshipConstraintIds)
                    {
                        if (constraint == constraintId)
                        {
                            constraintIdCounter--;
                        }
                    }
                }

                if (constraintIdCounter == 0)
                    isExisting = true;
            }
            catch
            {
                isExisting = false;
            }
            return isExisting;
        }
        /// <summary>
        /// Test the Relationship Constraint in the relationship definition
        /// </summary>
        /// <param name="ResourceRelationShipDefinitonId"></param>
        /// <param name="RelationshipConstraintIds"></param>
        /// <returns></returns>
        public static bool Test_RelationshipDefinition_ContainConstraint(Guid ResourceRelationShipDefinitonId, Guid RelationshipConstraintId)
        {
            bool isExisting = false;
            try
            {
                //get the relationship from the database
                IResource resourceRelationship = Resource.Get(ResourceRelationShipDefinitonId);

                //get  the relationship constaints
                GuidCollection constraints = ((IResourceRelationshipDefinition)resourceRelationship).RelationshipConstraints;

                foreach (Guid constraint in constraints)
                {

                    if (constraint == RelationshipConstraintId)
                    {
                         isExisting = true;
                        break ;
                    }

                }

            }
            catch
            {
                isExisting = false;
            }
            return isExisting;
        }
    }
}
