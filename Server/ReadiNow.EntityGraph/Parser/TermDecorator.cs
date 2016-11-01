// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.Linq;
using EDC.ReadiNow.EntityRequests;
using EDC.ReadiNow.Model;
using SecurityOption = EDC.ReadiNow.Security.SecurityOption;
using EntityRefHelper = EDC.ReadiNow.Model.EntityRefHelper;

namespace ReadiNow.EntityGraph.Parser
{
	/// <summary>
	/// Mechanism for taking a list of parse terms and converting it into an entity request structure.
	/// </summary>
	class TermDecorator
	{
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entityRepository">Repository to load schema data from.</param>
        internal TermDecorator(IEntityRepository entityRepository)
        {
            if (entityRepository == null)
                throw new ArgumentNullException("entityRepository");
            EntityRepository = entityRepository;
        }


        /// <summary>
        ///     Repository to load schema data from.
        /// </summary>
        internal IEntityRepository EntityRepository { get; private set; }


		/// <summary>
		/// Map of namespace prefixes to full namespaces, to use when resolving aliases.s
		/// </summary>
		readonly Dictionary<string, string> _namespacePrefixes = EntityRefHelper.DefaultNsPrefixes;

        /// <summary>
        /// Variables that were declared during parsing.
        /// </summary>
        internal Dictionary<string, List<Term>> Variables { get; set; }

        /// <summary>
        /// List of variable names and the actions that need to be invoked once the variable is available.
        /// </summary>
        List<Tuple<string, Action<EntityMemberRequest>>> _variableBindTasks = new List<Tuple<string, Action<EntityMemberRequest>>>();

		/// <summary>
		/// Map of (internal) paths to individual relationship requests.
		/// Used to determine if two relationship requests should be consolidated into a single request object.
		/// E.g. for this query: isOfType.inherits.name, isOfType.inherits.description
		/// the parser will return two top-level Terms, each with an inherits.
		/// This cache will be used to consolidate into a single isOfType, with a single inherits, with two fields.
		/// </summary>
        Dictionary<string, RelationshipRequest> _pathCache;

		/// <summary>
		/// Converts the list of terms to an entity member request.
		/// </summary>
        /// <param name="terms">Terms returned from the RequestParser.</param>
        /// <param name="validate"></param>
		public EntityMemberRequest ConvertToRequest(List<Term> terms, bool validate = true)
		{
            Dictionary<string, EntityMemberRequest> converted = new Dictionary<string, EntityMemberRequest>();

            // Treat the result as an anonymous variable.
            string ResultKey = ""; // or whatever
            Variables[ResultKey] = terms;

            // And it may access a variable
            var rootVarAccess = TermListGetsVariable(terms);
            if (rootVarAccess != null)
                AddBindTask(rootVarAccess, rq => { converted[ResultKey] = rq; });

            // Process all variables
            foreach (var pair in Variables)
            {
                converted[pair.Key] = ConvertTermsToRequest(pair.Value, validate);
            }

            // Stitch up variables
            foreach (var task in _variableBindTasks)
            {
                string varName = task.Item1;
                Action<EntityMemberRequest> callback = task.Item2;
                EntityMemberRequest rq;
                if (!converted.TryGetValue(varName, out rq))
                    throw new Exception("Attempted to access unknown variable @" + varName);
                callback(rq);
            }

            return converted[ResultKey];
		}

        /// <summary>
        /// Converts an individual top-level list of terms (variable or result)
        /// </summary>
        private EntityMemberRequest ConvertTermsToRequest(List<Term> terms, bool validate)
        {
            _pathCache = new Dictionary<string, RelationshipRequest>();

            var rq = new EntityMemberRequest
            {
                Fields = new List<IEntityRef>(),
                Relationships = new List<RelationshipRequest>()
            };
            ConvertTermsToRequest(rq, terms, "", validate);
            return rq;
        }

		/// <summary>
		/// Recursively converts the list of terms to an entity member request.
		/// </summary>
        private void ConvertTermsToRequest(EntityMemberRequest rq, IEnumerable<Term> terms, string path, bool validate)
		{
			if (terms == null)
				return;

			foreach (Term term in terms)
			{
				// A term is a relationship if it has children, or if it is of the form: alias*
			    bool isRelationship =
			        term.Children != null && term.Children.Count > 0
			        || !string.IsNullOrEmpty(term.AliasOrId) && term.Asterisk;

                bool isField = !isRelationship && !term.VariableAccess && term.AliasOrId != "id" && !term.Asterisk && !term.MetadataOnly;

				if (isField)
				{
					EntityRef fieldId = ToEntityRef(term);
                    if (validate && Entity.Get<Field>(fieldId, SecurityOption.SkipDenied) == null)
				        throw new Exception("Expected " + fieldId + " to be a field.");
				    if (term.Reverse)
				        throw new Exception("Unexpected reverse sign before a field.");
					rq.Fields.Add(fieldId);
				}

				if (term.Asterisk && !isRelationship)
				{
                    rq.AllFields = true;
                    if (term.Reverse)
                        throw new Exception("Unexpected reverse sign before all fields *.");
                }

				if (isRelationship)
				{
                    // Try to consolidate paths
                    string termKey = term.AliasOrId;
                    if (term.Reverse) termKey = "-" + termKey;
                    if (term.MetadataOnly) termKey = termKey + "?";
                    if (term.Asterisk) termKey = termKey + "*";
                    string newPath = path + "." + termKey;

                    string accessesVariable = TermListGetsVariable(term.Children);
                    bool forceUnique = accessesVariable != null;

					RelationshipRequest relReq;
					if (forceUnique || !_pathCache.TryGetValue(newPath, out relReq))
					{
                        EntityRef relId = ToEntityRef(term);
                        if (validate && Entity.Get<Relationship>(relId, SecurityOption.SkipDenied) == null)
                            throw new Exception("Expected " + relId + " to be a relationship.");
						bool metadataOnly = term.Children != null && term.Children.Count > 0 && term.Children.First( ).MetadataOnly;
                        relReq = new RelationshipRequest
							{
                                RelationshipTypeId = relId,
                                IsReverse = term.Reverse,
                                MetadataOnly = metadataOnly
							};
                        rq.Relationships.Add(relReq);

                        if (accessesVariable != null)
                            AddBindTask(accessesVariable, variable => { relReq.RequestedMembers = variable; });
                        else
                            relReq.RequestedMembers = NewRequest();

                        if (!forceUnique)
    						_pathCache.Add(newPath, relReq);
					}

					relReq.IsRecursive |= term.Asterisk;

                    if (!relReq.MetadataOnly && accessesVariable == null)
				    {
				        if (term.Children != null && term.Children.Count > 0)
				        {
				            ConvertTermsToRequest(relReq.RequestedMembers, term.Children, newPath, validate);
				        }
				    }
				}
			}
		}

        /// <summary>
        /// Check if a term accesses a variable as its only child, and if so returns its name.
        /// </summary>
        private string TermListGetsVariable(List<Term> termList)
        {
            if (termList == null || termList.Count == 0)
                return null;
            if (termList.Any(child => child.VariableAccess))
            {
                if (termList.Count != 1)
                    throw new Exception("Attempted to access variables and members at the same time.");
                return termList[0].Identifier;
            }
            return null;
        }

        /// <summary>
        /// Queue a callback task to bind a variable to its request
        /// </summary>
        private void AddBindTask(string variable, Action<EntityMemberRequest> callback)
        {
            _variableBindTasks.Add(new Tuple<string, Action<EntityMemberRequest>>(variable, callback));
        }

		/// <summary>
		/// Create a new EntityMemberRequest instance, and initialize collections.
		/// </summary>
		/// <returns></returns>
		private EntityMemberRequest NewRequest()
		{
			var request = new EntityMemberRequest
				{
                    Fields = new List<IEntityRef>(),
                    Relationships = new List<RelationshipRequest>()
				};
			return request;
		}

		/// <summary>
		/// Convert a term (identifier or ID number) to an EntityRef.
		/// </summary>
		private EntityRef ToEntityRef(Term term)
		{
			if (term.Id != 0)
				return new EntityRef(term.Id);

			return EntityRefHelper.ConvertAliasWithNamespace(term.Identifier, _namespacePrefixes);
		}


	}
}
