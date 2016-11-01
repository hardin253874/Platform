// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System;
using System.Collections.Generic;
using System.ComponentModel;
using TenantDiffTool.Core;

namespace TenantDiffTool.SupportClasses.Diff
{
	/// <summary>
	///     Entity Data.
	/// </summary>
	public class Data : DiffBase
	{
		/// <summary>
		///     Data types.
		/// </summary>
		[Browsable( false )]
		public static readonly string[ ] DataTypes =
		{
			"Alias",
			"Bit",
			"DateTime",
			"Decimal",
			"Guid",
			"Int",
			"NVarChar",
			"Xml"
		};

		/// <summary>
		///     The Entity name
		/// </summary>
		private string _entityName;

		/// <summary>
		///     The Field name
		/// </summary>
		private string _fieldName;

		/// <summary>
		///     Gets or sets the name of the entity.
		/// </summary>
		/// <value>
		///     The name of the entity.
		/// </value>
		[Browsable( false )]
		public string EntityName
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
		///     Gets or sets the name of the field.
		/// </summary>
		/// <value>
		///     The name of the field.
		/// </value>
		[Browsable( false )]
		public string FieldName
		{
			get
			{
				if ( string.IsNullOrEmpty( _fieldName ) )
				{
					string name;

					if ( UpgradeIdCache.Instance.TryGetValue( FieldUpgradeId, out name ) )
					{
						_fieldName = name;
					}
				}

				return _fieldName;
			}
			set
			{
				_fieldName = value;
			}
		}

		/// <summary>
		///     Gets or sets the field upgrade id.
		/// </summary>
		/// <value>
		///     The field upgrade id.
		/// </value>
		[Browsable( false )]
		public Guid FieldUpgradeId
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the type.
		/// </summary>
		/// <value>
		///     The type.
		/// </value>
		[Browsable( false )]
		public string Type
		{
			get;
			set;
		}

		/// <summary>
		///     Gets or sets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		[Browsable( false )]
		public object Value
		{
			get;
			set;
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