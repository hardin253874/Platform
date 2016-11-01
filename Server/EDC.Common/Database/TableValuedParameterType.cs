// Copyright 2011-2016 Global Software Innovation Pty Ltd

namespace EDC.Database
{
	/// <summary>
	/// </summary>
	public enum TableValuedParameterType
	{
		/// <summary>
		///     Represents a table valued parameter that can map directly to the 'Data_Alias' table.
		/// </summary>
		/// <remarks>
		///     EntityId: BIGINT
		///     TenantId: BIGINT
		///     FieldId: BIGINT
		///     Data: NVARCHAR(100)
		///     Namespace: NVARCHAR(100)
		///     AliasMarkerId: INT
		/// </remarks>
		DataAlias,

		/// <summary>
		///     Represents a table valued parameter that can map directly to the 'Data_Bit' table.
		/// </summary>
		/// <remarks>
		///     EntityId: BIGINT
		///     TenantId: BIGINT
		///     FieldId: BIGINT
		///     Data: BIT
		/// </remarks>
		DataBit,

		/// <summary>
		///     Represents a table valued parameter that can map directly to the 'Data_DateTime' table.
		/// </summary>
		/// <remarks>
		///     EntityId: BIGINT
		///     TenantId: BIGINT
		///     FieldId: BIGINT
		///     Data: DateTime
		/// </remarks>
		DataDateTime,

		/// <summary>
		///     Represents a table valued parameter that can map directly to the 'Data_Decimal' table.
		/// </summary>
		/// <remarks>
		///     EntityId: BIGINT
		///     TenantId: BIGINT
		///     FieldId: BIGINT
		///     Data: Decimal
		/// </remarks>
		DataDecimal,

		/// <summary>
		///     Represents a table valued parameter that can map directly to the 'Data_Guid' table.
		/// </summary>
		/// <remarks>
		///     EntityId: BIGINT
		///     TenantId: BIGINT
		///     FieldId: BIGINT
		///     Data: Guid
		/// </remarks>
		DataGuid,

		/// <summary>
		///     Represents a table valued parameter that can map directly to the 'Data_Int' table.
		/// </summary>
		/// <remarks>
		///     EntityId: BIGINT
		///     TenantId: BIGINT
		///     FieldId: BIGINT
		///     Data: Int
		/// </remarks>
		DataInt,

		/// <summary>
		///     Represents a table valued parameter that can map directly to the 'Data_NVarChar' table.
		/// </summary>
		/// <remarks>
		///     EntityId: BIGINT
		///     TenantId: BIGINT
		///     FieldId: BIGINT
		///     Data: NVARCHAR(MAX)
		/// </remarks>
		DataNVarChar,

		/// <summary>
		///     Represents a table valued parameter that can map directly to the 'Data_Xml' table.
		/// </summary>
		/// <remarks>
		///     EntityId: BIGINT
		///     TenantId: BIGINT
		///     FieldId: BIGINT
		///     Data: NVARCHAR(MAX)
		/// </remarks>
		DataXml,

		/// <summary>
		///     Represents a table valued parameter that can map directly to the 'Relationship' table.
		/// </summary>
		/// <remarks>
		///     TenantId: BIGINT
		///     TypeId: BIGINT
		///     FromId: BIGINT
		///     ToId: BIGINT
		/// </remarks>
		Relationship,

		/// <summary>
		///     Represents a table valued parameter that can map directly to the 'EntityBatch' table.
		/// </summary>
		/// <remarks>
		///     BatchId: BIGINT
		///     EntityId: BIGINT
		/// </remarks>
		EntityBatch,

		/// <summary>
		///     Represents a table valued parameter that contains a SourceId, DestinationId and CloneOption columns.
		/// </summary>
		/// <remarks>
		///     BatchId: BIGINT
		///     EntityId: BIGINT
		/// </remarks>
		EntityClone,

		/// <summary>
		///     Represents a table valued parameter that contains a mapping between SourceId and DestinationId.
		/// </summary>
		/// <remarks>
		///     SourceId: BIGINT
		///     DestinationId: BIGINT
		/// </remarks>
		EntityMap,

		/// <summary>
		///     Represents a table valued parameter that contains an Id, TypeId and IsClone value.
		/// </summary>
		/// <remarks>
		///     Id: BIGINT
		///     TypeId: BIGINT
		///     IsClone: INT
		/// </remarks>
		InputEntityType,

		/// <summary>
		///     Represents a table valued parameter that can be used to search any Data table using the EntityId, TenantId and
		///     FieldId columns only.
		/// </summary>
		/// <remarks>
		///     EntityId: BIGINT
		///     TenantId: BIGINT
		///     FieldId: BIGINT
		/// </remarks>
		LookupFieldKey,

		/// <summary>
		///     Represents a table valued parameter that can be used to search the Alias Data table using the TenantId, Namespace
		///     and Data columns only.
		/// </summary>
		/// <remarks>
		///     EntityId: BIGINT
		///     TenantId: BIGINT
		///     FieldId: BIGINT
		/// </remarks>
		LookupAliasData,

		/// <summary>
		///     Represents a table valued parameter that can be used to search the Relationship table using the TenantId, TypeId
		///     and FromId columns only.
		/// </summary>
		/// <remarks>
		///     TenantId: BIGINT
		///     TypeId: BIGINT
		///     FromId: BIGINT
		/// </remarks>
		LookupRelationshipForward,

		/// <summary>
		///     Represents a table valued parameter that can be used to search the Relationship table using the TenantId, TypeId
		///     and ToId columns only.
		/// </summary>
		/// <remarks>
		///     TenantId: BIGINT
		///     TypeId: BIGINT
		///     ToId: BIGINT
		/// </remarks>
		LookupRelationshipReverse,

		/// <summary>
		///     Represents a table valued parameter that contains a list of BigInts.
		/// </summary>
		/// <remarks>
		///     Id: BIGINT
		/// </remarks>
		BigInt,

        /// <summary>
        ///     Represents a table valued parameter that contains a list of Guids.
        /// </summary>
        /// <remarks>
        ///     Id: Guid
        /// </remarks>
        Guid,

		/// <summary>
		///     The bulk relative type
		/// </summary>
		/// <remarks>
		///     NodeTag: INT
		///     RelTypeId: BIGINT
		///     NextTag: INT
		/// </remarks>
		BulkRelType,

		/// <summary>
		///     The bulk field type
		/// </summary>
		/// <remarks>
		///     NodeTag: INT
		///     FieldId: BIGINT
		/// </remarks>
        BulkFldType,

        /// <summary>
        ///     NVarCharMaxListType
        /// </summary>
        /// <remarks>
        ///     Data: NVarChar(max)
        /// </remarks>
        NVarCharMaxListType,

        /// <summary>
        ///     Int
        /// </summary>
        /// <remarks>
        ///     Data: INT
        /// </remarks>
        Int,

        /// <summary>
        ///     Decimal
        /// </summary>
        /// <remarks>
        ///     Data: Decimal(38,10)
        /// </remarks>
        Decimal,

        /// <summary>
        ///     DateTime
        /// </summary>
        /// <remarks>
        ///     Data: DateTime
        /// </remarks>
        DateTime,

        /// <summary>
        ///     GuidList
        /// </summary>
        /// <remarks>
        ///     Data: UniqueIdentifier
        /// </remarks>
        GuidList
    }
}