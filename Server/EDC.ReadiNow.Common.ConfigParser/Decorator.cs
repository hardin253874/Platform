// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDC.ReadiNow.Common.ConfigParser.Containers;

namespace EDC.ReadiNow.Common.ConfigParser
{
    public static class Decorator
    {
        /// <summary>
        /// Adds additional data to entities.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="logger"></param>
        public static void DecorateEntities(IEnumerable<Entity> entities, Action<string> logger = null)
        {
            if (logger == null)
                logger = s => { };
            
            logger("Applying entity decorations");
            ApplyRelationshipTypesRules(entities, logger);
            logger("Completed entity decorations");
        }

        /// <summary>
        /// Sets relationship types.
        /// </summary>
        /// <param name="entities"></param>
        private static void ApplyRelationshipTypesRules(IEnumerable<Entity> entities, Action<string> logger)
        {
            var rules = new Dictionary<string, RelTypeRules>();
            rules.Add(Aliases.RelLookup.Value, new RelTypeRules(Aliases.ManyToOne, false, false, Aliases.CloneReferences, Aliases.Drop, false, false));
            rules.Add(Aliases.RelDependantOf.Value, new RelTypeRules(Aliases.ManyToOne, true, false, Aliases.CloneReferences, Aliases.Drop, false, false));
            rules.Add(Aliases.RelComponentOf.Value, new RelTypeRules(Aliases.ManyToOne, true, false, Aliases.CloneReferences, Aliases.CloneEntities, false, true));
            rules.Add(Aliases.RelChoiceField.Value, new RelTypeRules(Aliases.ManyToOne, false, false, Aliases.CloneReferences, Aliases.Drop, false, false, true, null));
            rules.Add(Aliases.RelSingleLookup.Value, new RelTypeRules(Aliases.OneToOne, false, false, Aliases.Drop, Aliases.Drop, false, false));
            rules.Add(Aliases.RelSingleComponentOf.Value, new RelTypeRules(Aliases.OneToOne, true, false, Aliases.Drop, Aliases.CloneEntities, false, true));
            rules.Add(Aliases.RelSingleComponent.Value, new RelTypeRules(Aliases.OneToOne, false, true, Aliases.CloneEntities, Aliases.Drop, true, false));
            rules.Add(Aliases.RelExclusiveCollection.Value, new RelTypeRules(Aliases.OneToMany, false, false, Aliases.Drop, Aliases.CloneReferences, false, false));
            rules.Add(Aliases.RelDependants.Value, new RelTypeRules(Aliases.OneToMany, false, true, Aliases.Drop, Aliases.CloneReferences, false, false));
            rules.Add(Aliases.RelComponents.Value, new RelTypeRules(Aliases.OneToMany, false, true, Aliases.CloneEntities, Aliases.CloneReferences, true, false));
            rules.Add(Aliases.RelManyToMany.Value, new RelTypeRules(Aliases.ManyToMany, false, false, Aliases.CloneReferences, Aliases.CloneReferences, false, false));
            rules.Add(Aliases.RelMultiChoiceField.Value, new RelTypeRules(Aliases.ManyToMany, false, false, Aliases.CloneReferences, Aliases.Drop, false, false, true, null));
            rules.Add(Aliases.RelSharedDependantsOf.Value, new RelTypeRules(Aliases.ManyToMany, true, false, Aliases.CloneReferences, Aliases.CloneReferences, false, false));
            rules.Add(Aliases.RelSharedDependants.Value, new RelTypeRules(Aliases.ManyToMany, false, true, Aliases.CloneReferences, Aliases.CloneReferences, false, false));
            rules.Add(Aliases.RelManyToManyFwd.Value, new RelTypeRules(Aliases.ManyToMany, false, false, Aliases.CloneReferences, Aliases.Drop, false, false));
            rules.Add(Aliases.RelManyToManyRev.Value, new RelTypeRules(Aliases.ManyToMany, false, false, Aliases.Drop, Aliases.CloneReferences, false, false));

            foreach (var entity in entities)
            {
                var relTypeMemb = entity.Members.FirstOrDefault(memb => memb.MemberDefinition.Alias == Aliases.RelType);
                if (relTypeMemb != null)
                {
                    if (relTypeMemb.ValueAsAliases != null && relTypeMemb.ValueAsAliases.Length == 1)
                    {
                        var relTypeAlias = relTypeMemb.ValueAsAliases[0];
                        RelTypeRules rule;
                        if (rules.TryGetValue(relTypeAlias.Value, out rule))
                        {
                            SetRelField(entity, Aliases.Cardinality, rule.Cardinality);
                            SetBoolField(entity, Aliases.CascadeDelete, rule.CascadeDelete);
                            SetBoolField(entity, Aliases.CascadeDeleteTo, rule.CascadeDeleteTo);
                            SetRelField(entity, Aliases.CloneAction, rule.CloneAction);
                            SetRelField(entity, Aliases.ReverseCloneAction, rule.ReverseCloneAction);
                            SetBoolField(entity, Aliases.ImplicitInSolution, rule.ImplicitInSolution);
                            SetBoolField(entity, Aliases.ReverseImplicitInSolution, rule.ReverseImplicitInSolution);

                            if (rule.SecuresTo.HasValue)
                                SetBoolField(entity, Aliases.SecuresTo, rule.SecuresTo.Value);
                            if (rule.SecuresFrom.HasValue)
                                SetBoolField(entity, Aliases.SecuresFrom, rule.SecuresFrom.Value);

                            logger("Set rule for " + entity.Alias);
                        }
                    }
                }
            }
        }

        private static void SetBoolField(Entity entity, Alias memberAlias, bool value)
        {
            Member member = entity.Members.FirstOrDefault(memb => memb.MemberDefinition.Alias == memberAlias);
            if (member == null)
            {
                member = new Member
                {
                    MemberDefinition = new EntityRef(memberAlias),
                };
                entity.Members.Add(member);
            }
            member.Value = value ? "true" : "false";
        }

        private static void SetRelField(Entity entity, Alias memberAlias, Alias value)
        {
            Member member = entity.Members.FirstOrDefault(memb => memb.MemberDefinition.Alias == memberAlias);
            if (member == null)
            {
                member = new Member
                {
                    MemberDefinition = new EntityRef(memberAlias),
                };
                entity.Members.Add(member);
            }
            member.Value = value.Value;
            member.ValueAsAliases = new[] { value };
        }

        private class RelTypeRules
        {
            public Alias Cardinality { get; private set; }
            public bool CascadeDelete { get; private set; }
            public bool CascadeDeleteTo { get; private set; }
            public Alias CloneAction { get; private set; }
            public Alias ReverseCloneAction { get; private set; }
            public bool ImplicitInSolution { get; private set; }
            public bool ReverseImplicitInSolution { get; private set; }
            public bool? SecuresTo { get; private set; }
            public bool? SecuresFrom { get; private set; }

            public RelTypeRules(Alias card, bool cd, bool cdTo, Alias ca, Alias revCa, bool impSoln, bool revImpSoln, bool? securesTo = null, bool? securesFrom = null)
            {
                Cardinality = card;
                CascadeDelete = cd;
                CascadeDeleteTo = cdTo;
                CloneAction = ca;
                ReverseCloneAction = revCa;
                ImplicitInSolution = impSoln;
                ReverseImplicitInSolution = revImpSoln;
                SecuresTo = securesTo;
                SecuresFrom = securesFrom;
            }
        }

    }
}
