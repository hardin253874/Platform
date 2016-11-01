// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using TenantDiffTool.Core;

namespace TenantDiffTool.SupportClasses.Diff
{
	/// <summary>
	///     Entity.
	/// </summary>
	public class Entity : DiffBase
	{
		/// <summary>
		///     The Entity name
		/// </summary>
		private string _entityName;

		/// <summary>
		///     Gets or sets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		[Browsable( false )]
		public string Description
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the entity upgrade id.
		/// </summary>
		/// <value>
		///     The entity upgrade id.
		/// </value>
		[Browsable( false )]
		public Guid EntityUpgradeId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the name.
		/// </summary>
		/// <value>
		///     The name.
		/// </value>
		[Browsable( false )]
		public string Name
		{
			get
			{
				if ( string.IsNullOrEmpty( _entityName ) )
				{
					string name;

					if ( UpgradeIdCache.Instance.TryGetValue( EntityUpgradeId, out name ) )
					{
						_entityName = name;
					}
				}

				return _entityName;
			}
			set
			{
				_entityName = value;
			}
		}

		/// <summary>
		///     Gets the data.
		/// </summary>
		/// <param name="props">The props.</param>
		protected override void GetData( PropertyDescriptorCollection props )
		{
			base.GetData( props );

			var propertySource = Source as IEntityPropertySource;

			if ( propertySource != null )
			{
				var state = new Dictionary<string, object>
				{
					["entityUpgradeId"] = EntityUpgradeId
				};

				propertySource.GetEntityProperties( props, state );
				propertySource.GetEntityFieldProperties( props, state );
				propertySource.GetEntityRelationshipProperties( props, state );
			}
		}
	}
}