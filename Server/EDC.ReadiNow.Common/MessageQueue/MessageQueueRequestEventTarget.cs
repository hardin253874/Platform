// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Collections.Generic;
using EDC.ReadiNow.Model;

namespace EDC.ReadiNow.MessageQueue
{
    /// <summary>
    /// Adds save and delete events to <see cref="MessageQueueRequest"/> entities.
    /// </summary>
    public class MessageQueueRequestEventTarget : IEntityEventSave, IEntityEventDelete
    {
        /// <summary>
        /// Adds the just saved message queue request to the response manager for watching.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        public void OnAfterSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            foreach (var entity in entities)
            {
                MessageQueueResponseManager.Add(entity.As<MessageQueueRequest>());
            }
        }

        /// <summary>
        /// Removes the about to be deleted message queue request from the watch list of the response manager.
        /// </summary>
		/// <param name="entities">The entities.</param>
		/// <param name="state">The state passed between the before delete and after delete callbacks.</param>
        /// <returns>True to cancel the delete operation; false otherwise.</returns>
        public bool OnBeforeDelete(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            foreach (var entity in entities)
            {
                MessageQueueResponseManager.Remove(entity.As<MessageQueueRequest>());
            }

            return false;
        }

        /// <summary>
        /// Called before saving the enumeration of entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before save and after save callbacks.</param>
        /// <returns>True to cancel the save operation; false otherwise.</returns>
        public bool OnBeforeSave(IEnumerable<IEntity> entities, IDictionary<string, object> state)
        {
            return false;
        }

        /// <summary>
        /// Called after deletion of the specified enumeration of entities has taken place.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="state">The state passed between the before delete and after delete callbacks.</param>
        public void OnAfterDelete(IEnumerable<long> entities, IDictionary<string, object> state)
        {

        }
    }
}
