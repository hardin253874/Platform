// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace RedisInit
{
	/// <summary>
	///     Tenant Info
	/// </summary>
	public class EntityInfo
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="EntityInfo" /> class.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="tenantId">The tenant identifier.</param>
		/// <param name="entityId">The entity identifier.</param>
		/// <param name="fieldId">The field identifier.</param>
		/// <param name="value">The value.</param>
		/// <param name="data">The data.</param>
		public EntityInfo( Types type, long tenantId, long entityId, long fieldId, object value, object data = null )
		{
			Type = type;
			TenantId = tenantId;
			EntityId = entityId;
			FieldId = fieldId;
			Value = value;
			Data = data;
		}

		/// <summary>
		///     Gets or sets the data.
		/// </summary>
		/// <value>
		///     The data.
		/// </value>
		public object Data
		{
			get;
			set;
		}

		/// <summary>
		///     Gets the entity identifier.
		/// </summary>
		/// <value>
		///     The entity identifier.
		/// </value>
		public long EntityId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the field identifier.
		/// </summary>
		/// <value>
		///     The field identifier.
		/// </value>
		public long FieldId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the tenant identifier.
		/// </summary>
		/// <value>
		///     The tenant identifier.
		/// </value>
		public long TenantId
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the type.
		/// </summary>
		/// <value>
		///     The type.
		/// </value>
		public Types Type
		{
			get;
			private set;
		}

		/// <summary>
		///     Gets the value.
		/// </summary>
		/// <value>
		///     The value.
		/// </value>
		public object Value
		{
			get;
			private set;
		}
	}
}