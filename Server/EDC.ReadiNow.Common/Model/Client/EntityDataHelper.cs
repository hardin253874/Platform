// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;


namespace EDC.ReadiNow.Model.Client
{
    /// <summary>
    /// Various helpers for working with EntityData.
    /// </summary>
    public static partial class EntityDataHelper
    {
        /// <summary>
        /// Generate a string representation of the EntityData for diagnostic purposes.
        /// </summary>
        /// <param name="data">The entity to debug.</param>
        /// <returns>A formatted string.</returns>
        public static string GetDebug(EntityData data)
        {
            StringBuilder sb = new StringBuilder();
            BuildDebug(data, sb, 0, new HashSet<EntityData>());
            return sb.ToString();
        }


        /// <summary>
        /// Recursive implementation of GetDebug.
        /// </summary>
        private static void BuildDebug(EntityData data, StringBuilder sb, int prefix, HashSet<EntityData> visited)
        {
            if (visited.Contains(data))
            {
                WriteLine(sb, prefix, "ID = {0} again", data.Id);
                return;
            }
            visited.Add(data);
            WriteLine(sb, prefix, "ID = {0} ({1})", data.Id, data.DataState);
            if (data.TypeIds == null)
                WriteLine(sb, prefix, "Types: null");
            else
                WriteLine(sb, prefix, "Types: {0}", string.Join(", ", data.TypeIds));
            
            // Show fields
			if ( data.Fields != null && data.Fields.Count > 0 )
            {
                WriteLine(sb, prefix, "Fields:");
                foreach (var field in data.Fields)
                {
                    if (field == null)
                    {
                        WriteLine(sb, prefix, "    null field");
                        continue;
                    }
                    string value = "null";
                    if (field.Value != null)
                        value = string.Format("{0}  ({1})", field.Value.Value, field.Value.Type.GetType().Name);
                    WriteLine(sb, prefix + 1, "{0} = {1}", field.FieldId, value);
                }
            }

            // Show relationships
			if ( data.Relationships != null && data.Relationships.Count > 0 )
            {
                WriteLine(sb, prefix, "Relationships:");
                foreach (var rel in data.Relationships)
                {
                    if (rel == null)
                    {
                        WriteLine(sb, prefix + 1, "null relationship");
                        continue;
                    }
                    WriteLine(sb, prefix + 1, "Rel {0}{1}", rel.RelationshipTypeId, rel.IsReverse ? " (Rev)" : "", rel.RemoveExisting ? " (Clear)" : "");
                    foreach (var inst in rel.Instances)
                    {
                        WriteLine(sb, prefix + 2, "{0}", inst.DataState);
                        BuildDebug(inst.Entity, sb, prefix+3,visited);
                        if (inst.RelationshipInstanceEntity != null)
                        {
                            WriteLine(sb, prefix + 3, "(rel instance)");
                            BuildDebug(inst.RelationshipInstanceEntity, sb, prefix + 3, visited);
                        }
                    }                    
                }
            }


        }


        /// <summary>
        /// Writes an indented formatted line to assist BuildDebug.
        /// </summary>
        private static void WriteLine(StringBuilder sb, int prefix, string format, params object[] args)
        {
            sb.AppendFormat(new string(' ', prefix * 4) + format + "\r\n", args);
        }

    }


    public partial class EntityData
    {
        public string Debug
        {
            get
            {
                try
                {
                    return EntityDataHelper.GetDebug(this);
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }

            }
        }
    }
}
