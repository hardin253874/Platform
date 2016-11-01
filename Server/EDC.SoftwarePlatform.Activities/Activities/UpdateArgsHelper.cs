// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.Diagnostics;
using EDC.ReadiNow.Model;

namespace EDC.SoftwarePlatform.Activities
{
    /// <summary>
    /// Provides a method to update a resource based upon variable number of input vars.
    /// This is used by both Create and UpdateField.
    /// </summary>
    public sealed class UpdateArgsHelper
    {
        private static void Log(string message, params object[] parameters)
        {
            EventLog.Application.WriteInformation(message, parameters);
        }



        /// <summary>
        /// Given a update ac
        /// </summary>
        /// <param name="activityAs"></param>
        /// <param name="inputs"></param>
        /// <param name="entityToUpdate"></param>
        public static void UpdateEntityFromArgs(EntityWithArgsAndExits activityAs, ActivityInputs inputs, IEntity entityToUpdate)
        {
            // This activity may also have its own input arguments (as opposed to those on its type)
            // and these are grouped so we can update multiple fields, relationships etc.


            // find the list of base names (indexes) of the argument groups
            Entity.GetField(activityAs.InputArguments, new EntityRef("core:name"));

            IOrderedEnumerable<int> argGroups = GetInputArgumentGroups(activityAs);

            // process each group

            foreach (var baseName in argGroups.Select(g => g.ToString()))
            {
                var arg = activityAs.InputArguments.FirstOrDefault(p => p.Name == baseName);

                if (arg == null)
                {
                    Log("Failed to find input argument for activity {0} with name {1}", activityAs.Name, baseName);
                    continue;
                }

                if (!inputs.ContainsKey(arg))
                {
                    Log("Failed to input argument expression for activity {0} with name {1}", activityAs.Name, baseName);
                    continue;
                }

                var memberEntity = (IEntity)inputs[arg];

                if (memberEntity != null)
                {
                    var fieldEntity = memberEntity.Is<Field>() ? memberEntity.Cast<Field>() : null;
                    var relEntity = memberEntity.Is<Relationship>() ? memberEntity.Cast<Relationship>() : null;

                    if (fieldEntity != null)
                    {
                        var valueArg = activityAs.InputArguments.FirstOrDefault(p => p.Name == baseName + "_value");
                        if (valueArg == null || !inputs.ContainsKey(valueArg))
                        {
                            Log("Failed to find input argument for activity {0} with name {1}", activityAs.Name,
                                baseName + "_value");
                        }
                        else
                        {
                            //Log("Updating field {0} {1} with value {2}", fieldEntity.Id, fieldEntity.Name,
                            //    inputs[valueArg]);

                            entityToUpdate.SetField(fieldEntity, inputs[valueArg]);
                        }
                    }
                    else if (relEntity != null)
                    {
                        var reverseArg = activityAs.InputArguments.FirstOrDefault(p => p.Name == baseName + "_reverse");
                        var replaceArg = activityAs.InputArguments.FirstOrDefault(p => p.Name == baseName + "_replace");

                        var cardinality = relEntity.Cardinality_Enum ?? CardinalityEnum_Enumeration.ManyToMany;

                        var direction = reverseArg != null && inputs.ContainsKey(reverseArg)
                                            ? (((bool?)inputs[reverseArg] ?? false)
                                                   ? Direction.Reverse
                                                   : Direction.Forward)
                                            : Direction.Forward;
                        var replaceExisting = (replaceArg != null && inputs.ContainsKey(replaceArg) &&
                                               ((bool?)inputs[replaceArg] ?? false))
                                              || cardinality == CardinalityEnum_Enumeration.OneToOne
                                              ||
                                              (direction == Direction.Forward &&
                                               cardinality == CardinalityEnum_Enumeration.ManyToOne)
                                              ||
                                              (direction == Direction.Reverse &&
                                               cardinality == CardinalityEnum_Enumeration.OneToMany);

                        //Log("Updating relationship {0} {1} direction={2} replace={3}", relEntity.Id, relEntity.Name,
                        //    direction,
                        //    replaceExisting);

                        var relCollection = entityToUpdate.GetRelationships(relEntity, direction);

                        //Log("Before: relationship {0} {1} has {2} related entities", relEntity.Id, relEntity.Name,
                        //    relCollection.Count());

                        if (replaceExisting)
                            relCollection.Clear();

                        foreach (
                            var valueArg in
                                activityAs.InputArguments.Where(p => p.Name.StartsWith(baseName + "_value_")))
                        {
                            var value = inputs.ContainsKey(valueArg) ? inputs[valueArg] : null;

                            IEnumerable<IEntity> entityRefs;

                            if (value is IEntity)
                            {
                                entityRefs = new List<IEntity> { (IEntity)value };
                            }
                            else
                            {
                                entityRefs = (IEnumerable<IEntity>)value;
                            }

                            if (entityRefs != null)
                            {
                                foreach (var entity in entityRefs)
                                {
                                    if (entity != null)
                                    {
                                        var relatedEntity = entity.As<Resource>();

                                        if (relatedEntity != null)
                                        {
                                            //Log(">> adding to relationship {0} {1}, related {2} {3}", relEntity.Id,
                                            //    relEntity.Name,
                                            //    relatedEntity.Id, relatedEntity.Name);

                                            relCollection.Add(relatedEntity);
                                        }
                                    }
                                }
                            }
                        }

                        //Log("After: relationship {0} {1} has {2} related entities", relEntity.Id, relEntity.Name,
                        //    relCollection.Count());
                    }
                }
            }

        }

        /// <summary>
        /// Get the input argument groups.
        /// </summary>
        /// <param name="activityAs"></param>
        /// <returns></returns>
        public static IOrderedEnumerable<int> GetInputArgumentGroups(EntityWithArgsAndExits activityAs)
        {
            return activityAs.InputArguments.Select(e =>
            {
                var nameParts = e.Name.Split('_');
                var index = 0;
                int.TryParse(nameParts[0], out index);
                return index;
            }).Distinct().OrderBy(p => p);
        }
    }
}