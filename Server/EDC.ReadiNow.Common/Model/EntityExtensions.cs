// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections;
using System.Collections.Generic;
using EDC.Collections.Generic;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Entity extension methods.
	/// </summary>
	public static class EntityExtensions
	{

        /// <summary>
        ///     Get the set of fields and relationships that have changed
        /// </summary>
        /// <param name="entity">The entity that his being tested.</param>
        /// <param name="fields">The fields that have changed.</param>
        /// <param name="forwardRelationships">The forward relationships that have changed.</param>
        /// <param name="reverseRelationships">The reverse relationships that have changed.</param>
        public static void GetChanges(this IEntity entity, out IEnumerable<long> fields, out IEnumerable<long> forwardRelationships, out IEnumerable<long> reverseRelationships)
        {
            IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelCache, reverseRelCache;
            IEntityFieldValues fieldChanges;

            entity.GetChanges(out fieldChanges, out forwardRelCache, out reverseRelCache);

            fields = fieldChanges != null ? fieldChanges.FieldIds : new List<long>();
            forwardRelationships = forwardRelCache != null ? forwardRelCache.Keys : new List<long>();
            reverseRelationships = reverseRelCache != null ? reverseRelCache.Keys : new List<long>();
        }

        /// <summary>
        /// Gets the changes.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="fields">The fields.</param>
        /// <param name="forwardRelationships">The forward relationships.</param>
        /// <param name="reverseRelationships">The reverse relationships.</param>
        /// <param name="getFields">if set to <c>true</c> [get fields].</param>
        /// <param name="getForwardRelationships">if set to <c>true</c> [get forward relationships].</param>
        /// <param name="getReverseRelationships">if set to <c>true</c> [get reverse relationships].</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="entity"/> cannot be null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="entity"/> must also implement IEntityInternal.
        /// </exception>
        internal static void GetChanges(this IEntity entity, out IEntityFieldValues fields, 
            out IDictionary<long, IChangeTracker<IMutableIdKey>> forwardRelationships, 
            out IDictionary<long, IChangeTracker<IMutableIdKey>> reverseRelationships, 
            bool getFields = true, bool getForwardRelationships = true, bool getReverseRelationships = true)
        {
            if (entity == null)
            {
                throw new ArgumentException("entity");
            }

            IEntityModificationToken token;
            IEntityInternal entityInternal;

            entityInternal = entity as IEntityInternal;
            if (entityInternal != null)
            {
                token = entityInternal.ModificationToken;
            }
            else
            {
                throw new ArgumentException("Must implement IEntityIternal", "entity");
            }

            Entity.GetChanges(token, out fields, out forwardRelationships, out reverseRelationships, getFields,
                              getForwardRelationships, getReverseRelationships);
        }

        /// <summary>
        ///     Clones this instance (Deep copy).
        /// </summary>
        /// <param name="entity">The entity to clone.</param>
        /// <returns>
        ///     An in memory clone of the current entity.
        /// </returns>
        public static IEntity Clone(this IEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            return entity.Clone(CloneOption.Deep);
        }

        /// <summary>
        ///     Clones this instance.
        /// </summary>
        /// <typeparam name="T">The type of result to return.</typeparam>
        /// <param name="entity">The entity to clone.</param>
        /// <param name="cloneOption">The option.</param>
        /// <returns>
        ///     A cloned instance of the current entity.
        /// </returns>
        public static T Clone<T>(this IEntity entity, CloneOption cloneOption)
            where T : class, IEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            return entity.Clone(cloneOption).As<T>();
        }

        /// <summary>
        ///     Clones this instance (Deep copy).
        /// </summary>
        /// <typeparam name="T">The type of the returned clone instance.</typeparam>
        /// <param name="entity">The entity to clone.</param>
        /// <returns>
        ///     A cloned instance of the current instance.
        /// </returns>
        public static T Clone<T>(this IEntity entity) where T : class, IEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            return entity.Clone<T>(CloneOption.Deep);
        }

        /// <summary>
        ///     Returns a new instance of the current instance that allows modifications to be made.
        /// </summary>
        /// <typeparam name="T">The type of entity to be returned.</typeparam>
        /// <param name="entity">The entity to clone.</param>
        /// <returns>
        ///     A writable instance of the current instance.
        /// </returns>
        public static T AsWritable<T>(this IEntity entity) where T : class, IEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            return entity.AsWritable().As<T>();
        }

        /// <summary>
        ///     Casts the specified entity to the specified type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <returns>
        ///     The cast entity if possible, InvalidCastException otherwise.
        /// </returns>
        public static T Cast<T>(this IEntity entity) where T : class, IEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            var cast = entity.As<T>();

            if (cast == null)
            {
                /////
                // Throw an invalid cast exception.
                /////
                throw new InvalidCastException("Specified cast is not valid.");
            }

            return cast;
        }

        /// <summary>
        ///     Gets the entities field value.
        /// </summary>
        /// <typeparam name="T">The expected return type.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        ///     The field value if found, null otherwise.
        /// </returns>
        public static T GetField<T>(this IEntity entity, EntityRef field)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            return (T)entity.GetField(field);
        }

        /// <summary>
        ///     Gets the entities field value.
        /// </summary>
        /// <typeparam name="T">The expected return type.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        ///     The field value if found, null otherwise.
        /// </returns>
        public static T GetField<T>(this IEntity entity, IEntityRef field)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            return (T)entity.GetField(field);
        }

        /// <summary>
        ///     Gets the field.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public static T GetField<T>(this IEntity entity, long field)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            return (T)entity.GetField(field);
        }

		
	}
}