// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;

namespace EDC.ReadiNow.Model.EventClasses
{
    /// <summary>
    /// </summary>
    internal static class EventTargetStateHelper
    {
        #region Constants
        /// <summary>
        ///     The savegraph key name
        /// </summary>
        private const string SaveGraphKeyName = "EventTargetStateHelper_SaveGraph";


        /// <summary>
        ///     The temporary to id mapping key name
        /// </summary>
        private const string TemporaryToIdMappingKeyName = "EventTargetStateHelper_TemporaryToIdMapping";


        /// <summary>
        ///     The id to temporary mapping key name
        /// </summary>
        private const string IdToTemporaryMappingKeyName = "EventTargetStateHelper_IdToTemporaryMapping";

        #endregion Constants

        #region Public Methods

        /// <summary>
        /// Gets the save graph.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        internal static SaveGraph GetSaveGraph(IDictionary<string, object> state)
        {
            return GetValue<SaveGraph>(state, SaveGraphKeyName);
        }


        /// <summary>
        /// Sets the save graph.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="saveGraph">The save graph.</param>
        internal static void SetSaveGraph(IDictionary<string, object> state, SaveGraph saveGraph)
        {
            if (state != null)
            {
                state[SaveGraphKeyName] = saveGraph;
            }
        }


        /// <summary>
        ///     Gets the id from temporary id.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="temporaryId">The temporary id.</param>
        /// <returns></returns>
        internal static long GetIdFromTemporaryId(IDictionary<string, object> state, long temporaryId)
        {
            return GetMappedId(state, TemporaryToIdMappingKeyName, temporaryId);
        }


        /// <summary>
        ///     Gets the temporary id from id.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        internal static long GetTemporaryIdFromId(IDictionary<string, object> state, long id)
        {
            return GetMappedId(state, IdToTemporaryMappingKeyName, id);
        }


        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="state">The state.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="createFunc">The create function.</param>
        /// <returns></returns>
        internal static T GetValue<T>(IDictionary<string, object> state, string keyName, Func<T> createFunc = null)
        {
            T result = default(T);

            if (state != null &&
                 !string.IsNullOrEmpty(keyName))
            {
                object value;
                if (state.TryGetValue(keyName, out value))
                {
                    result = (T)value;
                }
                else
                {
                    if (createFunc != null)
                    {
                        result = createFunc();
                        state[keyName] = result;
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Sets the id mapping.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="temporaryId">The temporary id.</param>
        /// <param name="id">The id.</param>
        internal static void SetIdMapping(IDictionary<string, object> state, long temporaryId, long id)
        {
            // Set temporary to id mapping
            SetMappedId(state, TemporaryToIdMappingKeyName, temporaryId, id);
            // Set id to temporary id mapping
            SetMappedId(state, IdToTemporaryMappingKeyName, id, temporaryId);
        }

        #endregion

        #region Non-Public Methods

        /// <summary>
        ///     Gets the mapped id.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        private static long GetMappedId(IDictionary<string, object> state, string keyName, long id)
        {
            long result = id;

            var idDictionary = GetValue<Dictionary<long, long>>(state, keyName);

            if (idDictionary != null &&
                 !idDictionary.TryGetValue(id, out result))
            {
                // Return the incoming id if not found.     
                result = id;
            }

            return result;
        }


        /// <summary>
        ///     Sets the mapped id.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="fromId">From id.</param>
        /// <param name="toId">To id.</param>
        private static void SetMappedId(IDictionary<string, object> state, string keyName, long fromId, long toId)
        {
            if (state == null || string.IsNullOrEmpty(keyName))
            {
                return;
            }

            var idDictionary = GetValue<Dictionary<long, long>>(state, keyName);

            if (idDictionary == null)
            {
                idDictionary = new Dictionary<long, long>();
                state[keyName] = idDictionary;
            }

            idDictionary[fromId] = toId;
        }

        #endregion
    }
}