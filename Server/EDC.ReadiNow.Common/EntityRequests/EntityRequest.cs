// Copyright 2011-2016 Global Software Innovation Pty Ltd
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using EDC.ReadiNow.Model;
using EDC.Core;
using EDC.ReadiNow.Security;

namespace EDC.ReadiNow.EntityRequests
{
    /// <summary>
    /// Type of request being placed.
    /// </summary>
    [DataContract]
    public enum QueryType
    {
        [EnumMember(Value = "basic")]
        Basic,
        [EnumMember(Value = "basicwithdemand")]
        BasicWithDemand,
        [EnumMember(Value = "instances")]
        Instances,
        [EnumMember(Value = "exactinstances")]
        ExactInstances,
        [EnumMember(Value = "filterinstances")]
        FilterInstances,
        [EnumMember(Value = "filterexactinstances")]
        FilterExactInstances
    }

    /// <summary>
    /// Captures a request.
    /// Specify either the RequestString or the Request object.
    /// The RequestString is required to participate in caching.
    /// </summary>
    public class EntityRequest
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public EntityRequest()
        {
        }

        /// <summary>
        /// Convenient constructor.
        /// </summary>
        public EntityRequest(EntityRef entity, string requestString, string hint = null)
        {
            Entities = entity.ToEnumerable();
            RequestString = requestString;
            QueryType = QueryType.Basic;
            Hint = hint;
        }

        /// <summary>
        /// Convenient constructor.
        /// </summary>
        public EntityRequest(EntityRef entity, string requestString, QueryType queryType, string hint = null)
        {
            Entities = entity.ToEnumerable();
            RequestString = requestString;
            QueryType = queryType;
            Hint = hint;
        }

        /// <summary>
        /// Convenient constructor.
        /// </summary>
        public EntityRequest(IEnumerable<EntityRef> entities, string requestString, string hint = null)
        {
            Entities = entities;
            RequestString = requestString;
            QueryType = QueryType.Basic;
            Hint = hint;
        }
        
        /// <summary>
        /// The type of request.
        /// </summary>
        public QueryType QueryType { get; set; }

        /// <summary>
        /// The request string.
        /// </summary>
        public string RequestString { get; set; }

        /// <summary>
        /// A calculation string that will be applied as a filter.
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// The request object.
        /// </summary>
        public EntityMemberRequest Request { get; set; }

        /// <summary>
        /// Any hint text that was passed in.
        /// </summary>
        public string Hint { get; set; }

        /// <summary>
        /// Isolated mode. Do not cache as metadata that is shared between user rule sets.
        /// </summary>
        public bool Isolated { get; set; }

        /// <summary>
        /// The entities being requested. Or if multiple instances of a type are being requested, the type to load.
        /// </summary>
        public IEnumerable<EntityRef> Entities
        {
            get { return _entities; }
            set
            {
                _entities = value;
                _entityIDs = null;
                _entityIDsCanonical = null;
            }
        }
        private IEnumerable<EntityRef> _entities;

        /// <summary>
        /// If true, the result cache is not used.
        /// </summary>
        public bool IgnoreResultCache { get; set; }

        /// <summary>
        /// Set true during processing if the result came from cache.
        /// </summary>
        public bool ResultFromCache { get; set; }

        /// <summary>
        /// Set by the caller to indicate that the bulk loader should not bother processing/returning if they would just come from cache.
        /// </summary>
        public bool DontProcessResultIfFromCache { get; set; }

        /// <summary>
        /// The entities being requested.
        /// </summary>
        public IEnumerable<long> EntityIDs
        {
            get
            {
                if (_entityIDs == null)
                {
                    _entityIDs = Entities.Select(ConvertId).Where(id => id != 0).ToList();
                }
                // TODO: Make alias->ID resolution faster
                return _entityIDs;
            }
        }
        private IEnumerable<long> _entityIDs;

        /// <summary>
        /// The entities being requested, in canonical form with duplicates removed.
        /// </summary>
        public IEnumerable<long> EntityIDsCanonical
        {
            get
            {
                if (_entityIDsCanonical == null)
                {
                    _entityIDsCanonical = EntityIDs.Distinct().OrderBy(i => i).ToList();
                }
                // TODO: Make alias->ID resolution faster
                return _entityIDsCanonical;
            }
        }
        private IEnumerable<long> _entityIDsCanonical;

        /// <summary>
        /// Get the ID number out of an entity ref. Or zero if it cannot be loaded.
        /// </summary>
        private static long ConvertId(EntityRef entityRef)
        {
            if (entityRef == null)
                return 0;

            try
            {
                return entityRef.Id;
            }
            catch
            {
                return 0;
            }
        }
    }

}
