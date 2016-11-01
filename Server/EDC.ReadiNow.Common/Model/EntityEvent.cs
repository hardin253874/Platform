// Copyright 2011-2016 Global Software Innovation Pty Ltd
using EDC.Cache;
using EDC.ReadiNow.Core.Cache;
using EDC.ReadiNow.Core.Cache.Providers;

namespace EDC.ReadiNow.Model
{
	/// <summary>
	///     Entity event class.
	/// </summary>
	public static class EntityEvent
	{
		/// <summary>
		///     The synchronize root
		/// </summary>
		private static readonly object SyncRoot = new object( );

		/// <summary>
		///     The cache
		/// </summary>		
        private static readonly ICache<string, IEntityRef> Cache = PerTenantCache<string, IEntityRef>.CreatePerTenantCache("Entity Event Cache");

		/// <summary>
		///     Gets the on after delete.
		/// </summary>
		/// <value>
		///     The on after delete.
		/// </value>
		public static IEntityRef OnAfterDelete
		{
			get
			{
				return GetByAlias( "core:onAfterDelete" );
			}
		}

		/// <summary>
		///     Gets the on after deploy.
		/// </summary>
		/// <value>
		///     The on after deploy.
		/// </value>
		public static IEntityRef OnAfterDeploy
		{
			get
			{
				return GetByAlias( "core:onAfterDeploy" );
			}
		}

        /// <summary>
        ///     Gets the on deploy failed target entity ref.
        /// </summary>
        /// <value>
        ///     The on deploy failed target entity ref.
        /// </value>
        public static IEntityRef OnDeployFailed
        {
            get
            {
                return GetByAlias("core:onDeployFailed");
            }
        }

        /// <summary>
        ///     Gets the on after publish target entity ref.
        /// </summary>
        /// <value>
        ///     The on after publish target entity ref.
        /// </value>
        public static IEntityRef OnAfterPublish
        {
            get
            {
                return GetByAlias("core:onAfterPublish");
            }
        }

        /// <summary>
        ///     Gets the on publish failed target entity ref.
        /// </summary>
        /// <value>
        ///     The on publish failed target entity ref.
        /// </value>
        public static IEntityRef OnPublishFailed
        {
            get
            {
                return GetByAlias("core:onPublishFailed");
            }
        }

		/// <summary>
		///     Gets the on after save.
		/// </summary>
		/// <value>
		///     The on after save.
		/// </value>
		public static IEntityRef OnAfterSave
		{
			get
			{
				return GetByAlias( "core:onAfterSave" );
			}
		}

        /// <summary>
        ///     Gets the on save failed target entity ref;
        /// </summary>
        /// <value>
        ///     The on save failed target entity ref.
        /// </value>
        public static IEntityRef OnSaveFailed
        {
            get
            {
                return GetByAlias("core:onSaveFailed");
            }
        }

        /// <summary>
        ///     Gets the on delete failed target entity ref;
        /// </summary>
        /// <value>
        ///     The on delete failed target entity ref.
        /// </value>
        public static IEntityRef OnDeleteFailed
        {
            get
            {
                return GetByAlias("core:onDeleteFailed");
            }
        }

		/// <summary>
		///     Gets the on before delete.
		/// </summary>
		/// <value>
		///     The on before delete.
		/// </value>
		public static IEntityRef OnBeforeDelete
		{
			get
			{
				return GetByAlias( "core:onBeforeDelete" );
			}
		}

		/// <summary>
		///     Gets the on before save.
		/// </summary>
		/// <value>
		///     The on before save.
		/// </value>
		public static IEntityRef OnBeforeSave
		{
			get
			{
				return GetByAlias( "core:onBeforeSave" );
			}
		}

        /// <summary>
        ///     Gets the on before upgrade event.
        /// </summary>
        /// <value>
        ///     The on before save.
        /// </value>
        public static IEntityRef OnBeforeUpgrade
        {
            get
            {
                return GetByAlias("core:onBeforeUpgrade");
            }
        }

        /// <summary>
        ///     Gets the on after upgrade event.
        /// </summary>
        /// <value>
        ///     The on before save.
        /// </value>
        public static IEntityRef OnAfterUpgrade
        {
            get
            {
                return GetByAlias("core:onAfterUpgrade");
            }
        }

        /// <summary>
		///     Gets the entity ref by alias.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <returns>The entity ref if found; null otherwise.</returns>
		private static IEntityRef GetByAlias( string alias )
		{			
			IEntityRef entityRef;		

			if ( !Cache.TryGetValue( alias, out entityRef ) )
			{
                lock (SyncRoot)
                {
                    if (!Cache.TryGetValue(alias, out entityRef))
                    {
                        entityRef = new EntityRef(alias);

                        Cache[alias] = entityRef;
                    }
                }				
			}

			return entityRef;
		}
	}
}