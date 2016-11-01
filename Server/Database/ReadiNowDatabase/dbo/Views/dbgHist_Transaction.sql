-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE VIEW [dbo].[dbgHist_Transaction] AS

	WITH Transactions
	AS (
		SELECT
			t.*,
			[EntityAction] = CASE e.[Action]
						WHEN 0 THEN 'Entity_Deleted'
						WHEN 1 THEN 'Entity_Added'
					 END,
			[EntityCount] = e.[Count],
			[RelationshipAction] = CASE r.Action
						WHEN 0 THEN 'Relationship_Deleted'
						WHEN 1 THEN 'Relationship_Added'
					 END,
			[RelationshipCount] = r.[Count],
			[AliasAction] = CASE d1.[Action]
						WHEN 0 THEN 'Alias_Deleted'
						WHEN 1 THEN 'Alias_Added'
					 END,
			[AliasCount] = d1.[Count],
			[BitAction] = CASE d2.[Action]
						WHEN 0 THEN 'Bit_Deleted'
						WHEN 1 THEN 'Bit_Added'
					 END,
			[BitCount] = d2.[Count],
			[DateTimeAction] = CASE d3.[Action]
						WHEN 0 THEN 'DateTime_Deleted'
						WHEN 1 THEN 'DateTime_Added'
					 END,
			[DateTimeCount] = d3.[Count],
			[DecimalAction] = CASE d4.[Action]
						WHEN 0 THEN 'Decimal_Deleted'
						WHEN 1 THEN 'Decimal_Added'
					 END,
			[DecimalCount] = d4.[Count],
			[GuidAction] = CASE d5.[Action]
						WHEN 0 THEN 'Guid_Deleted'
						WHEN 1 THEN 'Guid_Added'
					 END,
			[GuidCount] = d5.[Count],
			[IntAction] = CASE d6.[Action]
						WHEN 0 THEN 'Int_Deleted'
						WHEN 1 THEN 'Int_Added'
					 END,
			[IntCount] = d6.[Count],
			[NVarCharAction] = CASE d7.[Action]
						WHEN 0 THEN 'NVarChar_Deleted'
						WHEN 1 THEN 'NVarChar_Added'
					 END,
			[NVarCharCount] = d7.[Count],
			[XmlAction] = CASE d8.[Action]
						WHEN 0 THEN 'Xml_Deleted'
						WHEN 1 THEN 'Xml_Added'
					 END,
			[XmlCount] = d8.[Count]
		FROM
			Hist_Transaction t
		LEFT JOIN (
			SELECT
				TransactionId,
				Action,
				[Count] = COUNT( * )
			FROM
				Hist_Entity d
			GROUP BY
				TransactionId, [Action]
		) e ON
				t.TransactionId = e.TransactionId
		LEFT JOIN (
			SELECT
				TransactionId,
				Action,
				[Count] = COUNT( * )
			FROM
				Hist_Relationship d
			GROUP BY
				TransactionId, [Action]
		) r ON
				t.TransactionId = r.TransactionId
		LEFT JOIN (
			SELECT
				TransactionId,
				Action,
				[Count] = COUNT( * )
			FROM
				Hist_Data_Alias d
			GROUP BY
				TransactionId, [Action]
		) d1 ON
				t.TransactionId = d1.TransactionId
		LEFT JOIN (
			SELECT
				TransactionId,
				Action,
				[Count] = COUNT( * )
			FROM
				Hist_Data_Bit d
			GROUP BY
				TransactionId, [Action]
		) d2 ON
				t.TransactionId = d2.TransactionId
		LEFT JOIN (
			SELECT
				TransactionId,
				Action,
				[Count] = COUNT( * )
			FROM
				Hist_Data_DateTime d
			GROUP BY
				TransactionId, [Action]
		) d3 ON
				t.TransactionId = d3.TransactionId
		LEFT JOIN (
			SELECT
				TransactionId,
				Action,
				[Count] = COUNT( * )
			FROM
				Hist_Data_Decimal d
			GROUP BY
				TransactionId, [Action]
		) d4 ON
				t.TransactionId = d4.TransactionId
		LEFT JOIN (
			SELECT
				TransactionId,
				Action,
				[Count] = COUNT( * )
			FROM
				Hist_Data_Guid d
			GROUP BY
				TransactionId, [Action]
		) d5 ON
				t.TransactionId = d5.TransactionId
		LEFT JOIN (
			SELECT
				TransactionId,
				Action,
				[Count] = COUNT( * )
			FROM
				Hist_Data_Int d
			GROUP BY
				TransactionId, [Action]
		) d6 ON
				t.TransactionId = d6.TransactionId
		LEFT JOIN (
			SELECT
				TransactionId,
				Action,
				[Count] = COUNT( * )
			FROM
				Hist_Data_NVarChar d
			GROUP BY
				TransactionId, [Action]
		) d7 ON
				t.TransactionId = d7.TransactionId
		LEFT JOIN (
			SELECT
				TransactionId,
				Action,
				[Count] = COUNT( * )
			FROM
				Hist_Data_Xml d
			GROUP BY
				TransactionId, [Action]
		) d8 ON
				t.TransactionId = d8.TransactionId
	)
	SELECT
		[TransactionId]			= P.TransactionId,
		[UserId]				= P.UserId,
		[TenantId]				= P.TenantId,
		[Spid]					= P.Spid,
		[Timestamp]				= P.[Timestamp],
		[HostName]				= P.HostName,
		[ProgramName]			= P.ProgramName,
		[Domain]				= P.Domain,
		[UserName]				= P.Username,
		[LoginName]				= P.LoginName,
		[Context]				= P.Context,
		[UserDefined]			= P.UserDefined,
		[SystemUpgrade]			= P.SystemUpgrade,
		[RevertTo]				= P.RevertTo,
		[Entity Added]			= ISNULL( Entity_Added, 0 ),
		[Entity Deleted]		= ISNULL( Entity_Deleted, 0 ),
		[Relationship Added]	= ISNULL( Relationship_Added, 0 ),
		[Relationship Deleted]	= ISNULL( Relationship_Deleted, 0 ),
		[Alias Added]			= ISNULL( Alias_Added, 0 ),
		[Alias Deleted]			= ISNULL( Alias_Deleted, 0 ),
		[Bit Added]				= ISNULL( Bit_Added, 0 ),
		[Bit Deleted]			= ISNULL( Bit_Deleted, 0 ),
		[DateTime Added]		= ISNULL( DateTime_Added, 0 ),
		[DateTime Deleted]		= ISNULL( DateTime_Deleted, 0 ),
		[Decimal Added]			= ISNULL( Decimal_Added, 0 ),
		[Decimal Deleted]		= ISNULL( Decimal_Deleted, 0 ),
		[Guid Added]			= ISNULL( Guid_Added, 0 ),
		[Guid Deleted]			= ISNULL( Guid_Deleted, 0 ),
		[Int Added]				= ISNULL( Int_Added, 0 ),
		[Int Deleted]			= ISNULL( Int_Deleted, 0 ),
		[NVarChar Added]		= ISNULL( NVarChar_Added, 0 ),
		[NVarChar Deleted]		= ISNULL( NVarChar_Deleted, 0 ),
		[Xml Added]				= ISNULL( Xml_Added, 0 ),
		[Xml Deleted]			= ISNULL( Xml_Deleted, 0 )
	FROM
		Transactions
	PIVOT (
		MAX( [EntityCount] ) FOR EntityAction IN ( Entity_Deleted, Entity_Added ) ) P
	PIVOT (
		MAX( [RelationshipCount] ) FOR RelationshipAction IN ( Relationship_Deleted, Relationship_Added ) ) P
	PIVOT (
		MAX( [AliasCount] ) FOR AliasAction IN ( Alias_Deleted, Alias_Added ) ) P
	PIVOT (
		MAX( [BitCount] ) FOR BitAction IN ( Bit_Deleted, Bit_Added ) ) P
	PIVOT (
		MAX( [DateTimeCount] ) FOR DateTimeAction IN ( DateTime_Deleted, DateTime_Added ) ) P
	PIVOT (
		MAX( [DecimalCount] ) FOR DecimalAction IN ( Decimal_Deleted, Decimal_Added ) ) P
	PIVOT (
		MAX( [GuidCount] ) FOR GuidAction IN ( Guid_Deleted, Guid_Added ) ) P
	PIVOT (
		MAX( [IntCount] ) FOR IntAction IN ( Int_Deleted, Int_Added ) ) P
	PIVOT (
		MAX( [NVarCharCount] ) FOR NVarCharAction IN ( NVarChar_Deleted, NVarChar_Added ) ) P
	PIVOT (
		MAX( [XmlCount] ) FOR XmlAction IN ( Xml_Deleted, Xml_Added ) ) P