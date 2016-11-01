// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Model;
using EDC.ReadiNow.Model.Client;

namespace EDC.SoftwarePlatform.Services.Common
{
    public static class Extension
    {
        public static IEnumerable<RelationshipData> GetAllRelationships(this EntityData ed, EntityRef relationshipTypeId)
        {
            return ed.Relationships.Where(r => r.RelationshipTypeId.Id == relationshipTypeId.Id);
        }

        public static object ValueForField(this EntityData ed, EntityRef fieldTypeId)
        {
            return ed.Fields.Where(f => f.FieldId.Id == fieldTypeId.Id).Select(v => v.Value.Value).First();
        }
    }
}