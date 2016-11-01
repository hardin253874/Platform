
-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spRevert]
	@transactionId BIGINT,
	@tenantId BIGINT = NULL,
	@context VARCHAR(128) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	-----
	-- Confirm the transaction is for the specified tenant.
	-----
	IF ( @tenantId IS NOT NULL )
	BEGIN
		DECLARE @transactionTenantId BIGINT
		SELECT @transactionTenantId = TenantId FROM Hist_Transaction WHERE TransactionId = @transactionId

		IF (@transactionTenantId <> @tenantId)
		BEGIN
			DECLARE @errorMessage NVARCHAR ( 255 )
			SET @errorMessage = N'Tenant ''' + CAST( @tenantId AS NVARCHAR( 20 ) ) + ''' does not own transaction ''' + CAST( @transactionId AS NVARCHAR( 20 ) ) + '''.';
			THROW 50000, @errorMessage, 1;
			RETURN
		END
	END

	-----
	-- Handle context
	-----
	IF ( @context IS NULL )
	BEGIN
		IF ( @tenantId IS NULL )
		BEGIN
			SET @context = 'Reverting transaction ' + CAST( @transactionId AS VARCHAR( 20 ) )
		END
		ELSE
		BEGIN
			SET @context = 'Reverting transaction ' + CAST( @transactionId AS VARCHAR( 20 ) ) + ' for tenant ' + CAST( @tenantId AS VARCHAR( 20 ) )
		END
	END

	IF ( @context IS NOT NULL )
	BEGIN
		DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
		SET CONTEXT_INFO @contextInfo
	END

	-----
	-- Setup temporary tables with appropriate indexes
	-----
	CREATE TABLE #Entity (
		Action INT,
		Id BIGINT,
		TenantId BIGINT,
		UpgradeId UNIQUEIDENTIFIER
	)

	CREATE CLUSTERED INDEX [IX_#Entity] ON [#Entity] ([TenantId] ASC, [Id] ASC, [Action] ASC)

	CREATE TABLE #Relationship (
		Action INT,
		Id BIGINT,
		TenantId BIGINT,
		TypeId BIGINT,
		FromId BIGINT,
		ToId BIGINT
	)

	CREATE CLUSTERED INDEX [IX_#Relationship] ON [#Relationship] ([TenantId] ASC, [TypeId] ASC, [FromId] ASC, [ToId] ASC, [Action] ASC, [Id] ASC)

	CREATE TABLE #Data_Alias (
		Action INT,
		EntityId BIGINT,
		TenantId BIGINT,
		FieldId BIGINT,
		Data NVARCHAR ( 100 ) COLLATE Latin1_General_CS_AS,
		Namespace NVARCHAR( 100 ) COLLATE Latin1_General_CS_AS,
		AliasMarkerId INT
	)

	CREATE CLUSTERED INDEX [IX_#Data_Alias] ON [#Data_Alias] ([TenantId] ASC, [EntityId] ASC, [FieldId] ASC, [Action] ASC)

	CREATE TABLE #Data_Bit (
		Action INT,
		EntityId BIGINT,
		TenantId BIGINT,
		FieldId BIGINT,
		Data BIT
	)

	CREATE CLUSTERED INDEX [IX_#Data_Bit] ON [#Data_Bit] ([TenantId] ASC, [EntityId] ASC, [FieldId] ASC, [Action] ASC)

	CREATE TABLE #Data_DateTime (
		Action INT,
		EntityId BIGINT,
		TenantId BIGINT,
		FieldId BIGINT,
		Data DATETIME
	)

	CREATE CLUSTERED INDEX [IX_#Data_DateTime] ON [#Data_DateTime] ([TenantId] ASC, [EntityId] ASC, [FieldId] ASC, [Action] ASC)

	CREATE TABLE #Data_Decimal (
		Action INT,
		EntityId BIGINT,
		TenantId BIGINT,
		FieldId BIGINT,
		Data DECIMAL ( 38, 10 )
	)

	CREATE CLUSTERED INDEX [IX_#Data_Decimal] ON [#Data_Decimal] ([TenantId] ASC, [EntityId] ASC, [FieldId] ASC, [Action] ASC)

	CREATE TABLE #Data_Guid (
		Action INT,
		EntityId BIGINT,
		TenantId BIGINT,
		FieldId BIGINT,
		Data UNIQUEIDENTIFIER
	)

	CREATE CLUSTERED INDEX [IX_#Data_Guid] ON [#Data_Guid] ([TenantId] ASC, [EntityId] ASC, [FieldId] ASC, [Action] ASC)

	CREATE TABLE #Data_Int (
		Action INT,
		EntityId BIGINT,
		TenantId BIGINT,
		FieldId BIGINT,
		Data INT
	)

	CREATE CLUSTERED INDEX [IX_#Data_Int] ON [#Data_Int] ([TenantId] ASC, [EntityId] ASC, [FieldId] ASC, [Action] ASC)

	CREATE TABLE #Data_NVarChar (
		Action INT,
		Id BIGINT,
		EntityId BIGINT,
		TenantId BIGINT,
		FieldId BIGINT,
		Data NVARCHAR( MAX ) COLLATE Latin1_General_CI_AI
	)

	CREATE CLUSTERED INDEX [IX_#Data_NVarChar] ON [#Data_NVarChar] ([TenantId] ASC, [EntityId] ASC, [FieldId] ASC, [Action] ASC, [Id] ASC)

	CREATE TABLE #Data_Xml (
		Action INT,
		EntityId BIGINT,
		TenantId BIGINT,
		FieldId BIGINT,
		Data NVARCHAR( MAX ) COLLATE Latin1_General_CS_AS
	)

	CREATE CLUSTERED INDEX [IX_#Data_Xml] ON [#Data_Xml] ([TenantId] ASC, [EntityId] ASC, [FieldId] ASC, [Action] ASC)

	-----
	-- Populate the temporary tables
	-----
	INSERT INTO
		#Entity
	SELECT
		Action,
		Id,
		TenantId,
		UpgradeId
	FROM
		Hist_Entity
	WHERE
		TransactionId = @transactionId

	INSERT INTO
		#Relationship
	SELECT
		Action,
		Id,
		TenantId,
		TypeId,
		FromId,
		ToId
	FROM
		Hist_Relationship
	WHERE
		TransactionId = @transactionId

	INSERT INTO
		#Data_Alias
	SELECT
		Action,
		EntityId,
		TenantId,
		FieldId,
		Data,
		Namespace,
		AliasMarkerId
	FROM
		Hist_Data_Alias
	WHERE
		TransactionId = @transactionId

	INSERT INTO
		#Data_Bit
	SELECT
		Action,
		EntityId,
		TenantId,
		FieldId,
		Data
	FROM
		Hist_Data_Bit
	WHERE
		TransactionId = @transactionId

	INSERT INTO
		#Data_DateTime
	SELECT
		Action,
		EntityId,
		TenantId,
		FieldId,
		Data
	FROM
		Hist_Data_DateTime
	WHERE
		TransactionId = @transactionId

	INSERT INTO
		#Data_Decimal
	SELECT
		Action,
		EntityId,
		TenantId,
		FieldId,
		Data
	FROM
		Hist_Data_Decimal
	WHERE
		TransactionId = @transactionId

	INSERT INTO
		#Data_Guid
	SELECT
		Action,
		EntityId,
		TenantId,
		FieldId,
		Data
	FROM
		Hist_Data_Guid
	WHERE
		TransactionId = @transactionId

	INSERT INTO
		#Data_Int
	SELECT
		Action,
		EntityId,
		TenantId,
		FieldId,
		Data
	FROM
		Hist_Data_Int
	WHERE
		TransactionId = @transactionId

	INSERT INTO
		#Data_NVarChar
	SELECT
		Action,
		Id,
		EntityId,
		TenantId,
		FieldId,
		Data
	FROM
		Hist_Data_NVarChar
	WHERE
		TransactionId = @transactionId

	INSERT INTO
		#Data_Xml
	SELECT
		Action,
		EntityId,
		TenantId,
		FieldId,
		Data
	FROM
		Hist_Data_Xml
	WHERE
		TransactionId = @transactionId

	DECLARE @entityInsert INT = 0
	DECLARE @entityDelete INT = 0
	DECLARE @relationshipInsert INT = 0
	DECLARE @relationshipDelete INT = 0
	DECLARE @aliasInsert INT = 0
	DECLARE @aliasDelete INT = 0
	DECLARE @bitInsert INT = 0
	DECLARE @bitDelete INT = 0
	DECLARE @dateTimeInsert INT = 0
	DECLARE @dateTimeDelete INT = 0
	DECLARE @decimalInsert INT = 0
	DECLARE @decimalDelete INT = 0
	DECLARE @guidInsert INT = 0
	DECLARE @guidDelete INT = 0
	DECLARE @intInsert INT = 0
	DECLARE @intDelete INT = 0
	DECLARE @nVarCharInsert INT = 0
	DECLARE @nVarCharDelete INT = 0
	DECLARE @xmlInsert INT = 0
	DECLARE @xmlDelete INT = 0

	BEGIN TRANSACTION

	-----
	-- Insert entities
	-----
	SET IDENTITY_INSERT Entity ON

	INSERT INTO
		Entity WITH (PAGLOCK) (
			Id,
			TenantId,
			UpgradeId )
	SELECT
		del.Id,
		del.TenantId,
		del.UpgradeId
	FROM (
		SELECT
			Id,
			TenantId,
			UpgradeId,
			[Count] = COUNT( Id )
		FROM
			#Entity h
		WHERE
			h.Action = 0
		GROUP BY
			h.Id,
			h.TenantId,
			h.UpgradeId,
			h.Action
		) del
	LEFT JOIN (
		SELECT
			Id,
			TenantId,
			UpgradeId,
			[Count] = COUNT( Id )
		FROM
			#Entity h
		WHERE
			h.Action = 1
		GROUP BY
			h.Id,
			h.TenantId,
			h.UpgradeId,
			h.Action
		) ins ON
			ins.Id = del.Id
			AND ins.TenantId = del.TenantId
			AND ins.UpgradeId = del.UpgradeId
	LEFT JOIN
		Entity p ON
			del.Id = p.Id
			AND del.TenantId = p.TenantId
	WHERE
		ISNULL( del.[Count], 0 ) > ISNULL( ins.[Count], 0 )
		AND p.Id IS NULL

	SELECT @entityInsert = ISNULL( @@ROWCOUNT, 0 )

	SET IDENTITY_INSERT Entity OFF

	-----
	-- Insert relationships
	-----
	SET IDENTITY_INSERT Relationship ON

	INSERT INTO
		Relationship WITH (PAGLOCK) (
			Id,
			TenantId,
			TypeId,
			FromId,
			ToId )
	SELECT
		del.Id,
		del.TenantId,
		del.TypeId,
		del.FromId,
		del.ToId
	FROM (
		SELECT
			Id,
			TenantId,
			TypeId,
			FromId,
			ToId,
			[Count] = COUNT( Id )
		FROM
			#Relationship h
		WHERE
			h.Action = 0
		GROUP BY
			h.Id,
			h.TenantId,
			h.TypeId,
			h.FromId,
			h.ToId,
			h.Action
		) del
	LEFT JOIN (
		SELECT
			Id,
			TenantId,
			TypeId,
			FromId,
			ToId,
			[Count] = COUNT( Id )
		FROM
			#Relationship h
		WHERE
			h.Action = 1
		GROUP BY
			h.Id,
			h.TenantId,
			h.TypeId,
			h.FromId,
			h.ToId,
			h.Action
		) ins ON
			ins.Id = del.Id
			AND ins.TenantId = del.TenantId
			AND ins.TypeId = del.TypeId
			AND ins.FromId = del.FromId
			AND ins.ToId = del.ToId
	LEFT JOIN
		Relationship p ON
			del.TenantId = p.TenantId
			AND del.TypeId = p.TypeId
			AND del.FromId = p.FromId
			AND del.ToId = p.ToId
	WHERE
		ISNULL( del.[Count], 0 ) > ISNULL( ins.[Count], 0 )
		AND p.FromId IS NULL

	SELECT @relationshipInsert = ISNULL( @@ROWCOUNT, 0 )

	SET IDENTITY_INSERT Relationship OFF

	-----
	-- Delete alias
	-----
	DELETE
		Data_Alias
	FROM
		Data_Alias d WITH (PAGLOCK)
	JOIN (
		SELECT
			ins.EntityId,
			ins.TenantId,
			ins.FieldId,
			ins.Data,
			ins.Namespace,
			ins.AliasMarkerId
		FROM (
			SELECT
				EntityId,
				TenantId,
				FieldId,
				Data,
				Namespace,
				AliasMarkerId,
				[Count] = COUNT( EntityId )
			FROM
				#Data_Alias h
			WHERE
				h.Action = 1
			GROUP BY
				h.EntityId,
				h.TenantId,
				h.FieldId,
				h.Data,
				h.Namespace,
				h.AliasMarkerId,
				h.Action
			) ins
		LEFT JOIN (
			SELECT
				EntityId,
				TenantId,
				FieldId,
				Data,
				Namespace,
				AliasMarkerId,
				[Count] = COUNT( EntityId )
			FROM
				#Data_Alias h
			WHERE
				h.Action = 0
			GROUP BY
				h.EntityId,
				h.TenantId,
				h.FieldId,
				h.Data,
				h.Namespace,
				h.AliasMarkerId,
				h.Action
			) del ON
				ins.EntityId = del.EntityId
				AND ins.TenantId = del.TenantId
				AND ins.FieldId = del.FieldId
				AND ins.Data = del.Data
				AND ins.Namespace = del.Namespace
				AND ins.AliasMarkerId = del.AliasMarkerId
		WHERE
			ISNULL( del.[Count], 0 ) < ISNULL( ins.[Count], 0 )
	) a ON
		d.EntityId = a.EntityId
		AND d.TenantId = a.TenantId
		AND d.FieldId = a.FieldId
		AND d.Data = a.Data
		AND d.Namespace = a.Namespace
		AND d.AliasMarkerId = a.AliasMarkerId
	
	SELECT @aliasDelete = ISNULL( @@ROWCOUNT, 0 )
	-----
	-- Insert alias
	-----
	INSERT INTO
		Data_Alias WITH (PAGLOCK) (
			EntityId,
			TenantId,
			FieldId,
			Data,
			Namespace,
			AliasMarkerId )
	SELECT
		del.EntityId,
		del.TenantId,
		del.FieldId,
		del.Data,
		del.Namespace,
		del.AliasMarkerId
	FROM (
		SELECT
			EntityId,
			TenantId,
			FieldId,
			Data,
			Namespace,
			AliasMarkerId,
			[Count] = COUNT( EntityId )
		FROM
			#Data_Alias h
		WHERE
			h.Action = 0
		GROUP BY
			h.EntityId,
			h.TenantId,
			h.FieldId,
			h.Data,
			h.Namespace,
			h.AliasMarkerId,
			h.Action
		) del
	LEFT JOIN (
		SELECT
			EntityId,
			TenantId,
			FieldId,
			Data,
			Namespace,
			AliasMarkerId,
			[Count] = COUNT( EntityId )
		FROM
			#Data_Alias h
		WHERE
			h.Action = 1
		GROUP BY
			h.EntityId,
			h.TenantId,
			h.FieldId,
			h.Data,
			h.Namespace,
			h.AliasMarkerId,
			h.Action
		) ins ON
			ins.EntityId = del.EntityId
			AND ins.TenantId = del.TenantId
			AND ins.FieldId = del.FieldId
			AND ins.Data = del.Data
			AND ins.Namespace = del.Namespace
			AND ins.AliasMarkerId = del.AliasMarkerId
	LEFT JOIN
		Data_Alias p ON
			del.EntityId = p.EntityId
			AND del.TenantId = p.TenantId
			AND del.FieldId = p.FieldId
	WHERE
		ISNULL( del.[Count], 0 ) > ISNULL( ins.[Count], 0 )
		AND p.EntityId IS NULL

	SELECT @aliasInsert = ISNULL( @@ROWCOUNT, 0 )

	-----
	-- Delete bit
	-----
	DELETE
		Data_Bit
	FROM
		Data_Bit d WITH (PAGLOCK)
	JOIN (
		SELECT
			ins.EntityId,
			ins.TenantId,
			ins.FieldId,
			ins.Data
		FROM (
			SELECT
				EntityId,
				TenantId,
				FieldId,
				Data,
				[Count] = COUNT( EntityId )
			FROM
				#Data_Bit h
			WHERE
				h.Action = 1
			GROUP BY
				h.EntityId,
				h.TenantId,
				h.FieldId,
				h.Data,
				h.Action
			) ins
		LEFT JOIN (
			SELECT
				EntityId,
				TenantId,
				FieldId,
				Data,
				[Count] = COUNT( EntityId )
			FROM
				#Data_Bit h
			WHERE
				h.Action = 0
			GROUP BY
				h.EntityId,
				h.TenantId,
				h.FieldId,
				h.Data,
				h.Action
			) del ON
				ins.EntityId = del.EntityId
				AND ins.TenantId = del.TenantId
				AND ins.FieldId = del.FieldId
				AND ins.Data = del.Data
		WHERE
			ISNULL( del.[Count], 0 ) < ISNULL( ins.[Count], 0 )
	) a ON
		d.EntityId = a.EntityId
		AND d.TenantId = a.TenantId
		AND d.FieldId = a.FieldId
		AND d.Data = a.Data
	
	SELECT @bitDelete = ISNULL( @@ROWCOUNT, 0 )

	-----
	-- Insert bit
	-----
	INSERT INTO
		Data_Bit WITH (PAGLOCK) (
			EntityId,
			TenantId,
			FieldId,
			Data )
	SELECT
		del.EntityId,
		del.TenantId,
		del.FieldId,
		del.Data
	FROM (
		SELECT
			EntityId,
			TenantId,
			FieldId,
			Data,
			[Count] = COUNT( EntityId )
		FROM
			#Data_Bit h
		WHERE
			h.Action = 0
		GROUP BY
			h.EntityId,
			h.TenantId,
			h.FieldId,
			h.Data,
			h.Action
		) del
	LEFT JOIN (
		SELECT
			EntityId,
			TenantId,
			FieldId,
			Data,
			[Count] = COUNT( EntityId )
		FROM
			#Data_Bit h
		WHERE
			h.Action = 1
		GROUP BY
			h.EntityId,
			h.TenantId,
			h.FieldId,
			h.Data,
			h.Action
		) ins ON
			ins.EntityId = del.EntityId
			AND ins.TenantId = del.TenantId
			AND ins.FieldId = del.FieldId
			AND ins.Data = del.Data
	LEFT JOIN
		Data_Bit p ON
			del.EntityId = p.EntityId
			AND del.TenantId = p.TenantId
			AND del.FieldId = p.FieldId
	WHERE
		ISNULL( del.[Count], 0 ) > ISNULL( ins.[Count], 0 )
		AND p.EntityId IS NULL

	SELECT @bitInsert = ISNULL( @@ROWCOUNT, 0 )

	-----
	-- Delete datetime
	-----
	DELETE
		Data_DateTime
	FROM
		Data_DateTime d WITH (PAGLOCK)
	JOIN (
		SELECT
			ins.EntityId,
			ins.TenantId,
			ins.FieldId,
			ins.Data
		FROM (
			SELECT
				EntityId,
				TenantId,
				FieldId,
				Data,
				[Count] = COUNT( EntityId )
			FROM
				#Data_DateTime h
			WHERE
				h.Action = 1
			GROUP BY
				h.EntityId,
				h.TenantId,
				h.FieldId,
				h.Data,
				h.Action
			) ins
		LEFT JOIN (
			SELECT
				EntityId,
				TenantId,
				FieldId,
				Data,
				[Count] = COUNT( EntityId )
			FROM
				#Data_DateTime h
			WHERE
				h.Action = 0
			GROUP BY
				h.EntityId,
				h.TenantId,
				h.FieldId,
				h.Data,
				h.Action
			) del ON
				ins.EntityId = del.EntityId
				AND ins.TenantId = del.TenantId
				AND ins.FieldId = del.FieldId
				AND ins.Data = del.Data
		WHERE
			ISNULL( del.[Count], 0 ) < ISNULL( ins.[Count], 0 )
	) a ON
		d.EntityId = a.EntityId
		AND d.TenantId = a.TenantId
		AND d.FieldId = a.FieldId
		AND d.Data = a.Data
	
	SELECT @dateTimeDelete = ISNULL( @@ROWCOUNT, 0 )
	
	-----
	-- Insert datetime
	-----
	INSERT INTO
		Data_DateTime WITH (PAGLOCK) (
			EntityId,
			TenantId,
			FieldId,
			Data )
	SELECT
		del.EntityId,
		del.TenantId,
		del.FieldId,
		del.Data
	FROM (
		SELECT
			EntityId,
			TenantId,
			FieldId,
			Data,
			[Count] = COUNT( EntityId )
		FROM
			#Data_DateTime h
		WHERE
			h.Action = 0
		GROUP BY
			h.EntityId,
			h.TenantId,
			h.FieldId,
			h.Data,
			h.Action
		) del
	LEFT JOIN (
		SELECT
			EntityId,
			TenantId,
			FieldId,
			Data,
			[Count] = COUNT( EntityId )
		FROM
			#Data_DateTime h
		WHERE
			h.Action = 1
		GROUP BY
			h.EntityId,
			h.TenantId,
			h.FieldId,
			h.Data,
			h.Action
		) ins ON
			ins.EntityId = del.EntityId
			AND ins.TenantId = del.TenantId
			AND ins.FieldId = del.FieldId
			AND ins.Data = del.Data
	LEFT JOIN
		Data_DateTime p ON
			del.EntityId = p.EntityId
			AND del.TenantId = p.TenantId
			AND del.FieldId = p.FieldId
	WHERE
		ISNULL( del.[Count], 0 ) > ISNULL( ins.[Count], 0 )
		AND p.EntityId IS NULL

	SELECT @dateTimeInsert = ISNULL( @@ROWCOUNT, 0 )

	-----
	-- Delete decimal
	-----
	DELETE
		Data_Decimal
	FROM
		Data_Decimal d WITH (PAGLOCK)
	JOIN (
		SELECT
			ins.EntityId,
			ins.TenantId,
			ins.FieldId,
			ins.Data
		FROM (
			SELECT
				EntityId,
				TenantId,
				FieldId,
				Data,
				[Count] = COUNT( EntityId )
			FROM
				#Data_Decimal h
			WHERE
				h.Action = 1
			GROUP BY
				h.EntityId,
				h.TenantId,
				h.FieldId,
				h.Data,
				h.Action
			) ins
		LEFT JOIN (
			SELECT
				EntityId,
				TenantId,
				FieldId,
				Data,
				[Count] = COUNT( EntityId )
			FROM
				#Data_Decimal h
			WHERE
				h.Action = 0
			GROUP BY
				h.EntityId,
				h.TenantId,
				h.FieldId,
				h.Data,
				h.Action
			) del ON
				ins.EntityId = del.EntityId
				AND ins.TenantId = del.TenantId
				AND ins.FieldId = del.FieldId
				AND ins.Data = del.Data
		WHERE
			ISNULL( del.[Count], 0 ) < ISNULL( ins.[Count], 0 )
	) a ON
		d.EntityId = a.EntityId
		AND d.TenantId = a.TenantId
		AND d.FieldId = a.FieldId
		AND d.Data = a.Data
	
	SELECT @decimalDelete = ISNULL( @@ROWCOUNT, 0 )
	
	-----
	-- Insert decimal
	-----
	INSERT INTO
		Data_Decimal WITH (PAGLOCK) (
			EntityId,
			TenantId,
			FieldId,
			Data )
	SELECT
		del.EntityId,
		del.TenantId,
		del.FieldId,
		del.Data
	FROM (
		SELECT
			EntityId,
			TenantId,
			FieldId,
			Data,
			[Count] = COUNT( EntityId )
		FROM
			#Data_Decimal h
		WHERE
			h.Action = 0
		GROUP BY
			h.EntityId,
			h.TenantId,
			h.FieldId,
			h.Data,
			h.Action
		) del
	LEFT JOIN (
		SELECT
			EntityId,
			TenantId,
			FieldId,
			Data,
			[Count] = COUNT( EntityId )
		FROM
			#Data_Decimal h
		WHERE
			h.Action = 1
		GROUP BY
			h.EntityId,
			h.TenantId,
			h.FieldId,
			h.Data,
			h.Action
		) ins ON
			ins.EntityId = del.EntityId
			AND ins.TenantId = del.TenantId
			AND ins.FieldId = del.FieldId
			AND ins.Data = del.Data
	LEFT JOIN
		Data_Decimal p ON
			del.EntityId = p.EntityId
			AND del.TenantId = p.TenantId
			AND del.FieldId = p.FieldId
	WHERE
		ISNULL( del.[Count], 0 ) > ISNULL( ins.[Count], 0 )
		AND p.EntityId IS NULL

	SELECT @decimalInsert = ISNULL( @@ROWCOUNT, 0 )

	-----
	-- Delete guid
	-----
	DELETE
		Data_Guid
	FROM
		Data_Guid d WITH (PAGLOCK)
	JOIN (
		SELECT
			ins.EntityId,
			ins.TenantId,
			ins.FieldId,
			ins.Data
		FROM (
			SELECT
				EntityId,
				TenantId,
				FieldId,
				Data,
				[Count] = COUNT( EntityId )
			FROM
				#Data_Guid h
			WHERE
				h.Action = 1
			GROUP BY
				h.EntityId,
				h.TenantId,
				h.FieldId,
				h.Data,
				h.Action
			) ins
		LEFT JOIN (
			SELECT
				EntityId,
				TenantId,
				FieldId,
				Data,
				[Count] = COUNT( EntityId )
			FROM
				#Data_Guid h
			WHERE
				h.Action = 0
			GROUP BY
				h.EntityId,
				h.TenantId,
				h.FieldId,
				h.Data,
				h.Action
			) del ON
				ins.EntityId = del.EntityId
				AND ins.TenantId = del.TenantId
				AND ins.FieldId = del.FieldId
				AND ins.Data = del.Data
		WHERE
			ISNULL( del.[Count], 0 ) < ISNULL( ins.[Count], 0 )
	) a ON
		d.EntityId = a.EntityId
		AND d.TenantId = a.TenantId
		AND d.FieldId = a.FieldId
		AND d.Data = a.Data
	
	SELECT @guidDelete = ISNULL( @@ROWCOUNT, 0 )
	
	-----
	-- Insert guid
	-----
	INSERT INTO
		Data_Guid WITH (PAGLOCK) (
			EntityId,
			TenantId,
			FieldId,
			Data )
	SELECT
		del.EntityId,
		del.TenantId,
		del.FieldId,
		del.Data
	FROM (
		SELECT
			EntityId,
			TenantId,
			FieldId,
			Data,
			[Count] = COUNT( EntityId )
		FROM
			#Data_Guid h
		WHERE
			h.Action = 0
		GROUP BY
			h.EntityId,
			h.TenantId,
			h.FieldId,
			h.Data,
			h.Action
		) del
	LEFT JOIN (
		SELECT
			EntityId,
			TenantId,
			FieldId,
			Data,
			[Count] = COUNT( EntityId )
		FROM
			#Data_Guid h
		WHERE
			h.Action = 1
		GROUP BY
			h.EntityId,
			h.TenantId,
			h.FieldId,
			h.Data,
			h.Action
		) ins ON
			ins.EntityId = del.EntityId
			AND ins.TenantId = del.TenantId
			AND ins.FieldId = del.FieldId
			AND ins.Data = del.Data
	LEFT JOIN
		Data_Guid p ON
			del.EntityId = p.EntityId
			AND del.TenantId = p.TenantId
			AND del.FieldId = p.FieldId
	WHERE
		ISNULL( del.[Count], 0 ) > ISNULL( ins.[Count], 0 )
		AND p.EntityId IS NULL

	SELECT @guidInsert = ISNULL( @@ROWCOUNT, 0 )

	-----
	-- Delete int
	-----
	DELETE
		Data_Int
	FROM
		Data_Int d WITH (PAGLOCK)
	JOIN (
		SELECT
			ins.EntityId,
			ins.TenantId,
			ins.FieldId,
			ins.Data
		FROM (
			SELECT
				EntityId,
				TenantId,
				FieldId,
				Data,
				[Count] = COUNT( EntityId )
			FROM
				#Data_Int h
			WHERE
				h.Action = 1
			GROUP BY
				h.EntityId,
				h.TenantId,
				h.FieldId,
				h.Data,
				h.Action
			) ins
		LEFT JOIN (
			SELECT
				EntityId,
				TenantId,
				FieldId,
				Data,
				[Count] = COUNT( EntityId )
			FROM
				#Data_Int h
			WHERE
				h.Action = 0
			GROUP BY
				h.EntityId,
				h.TenantId,
				h.FieldId,
				h.Data,
				h.Action
			) del ON
				ins.EntityId = del.EntityId
				AND ins.TenantId = del.TenantId
				AND ins.FieldId = del.FieldId
				AND ins.Data = del.Data
		WHERE
			ISNULL( del.[Count], 0 ) < ISNULL( ins.[Count], 0 )
	) a ON
		d.EntityId = a.EntityId
		AND d.TenantId = a.TenantId
		AND d.FieldId = a.FieldId
		AND d.Data = a.Data
	
	SELECT @intDelete = ISNULL( @@ROWCOUNT, 0 )
	
	-----
	-- Insert int
	-----
	INSERT INTO
		Data_Int WITH (PAGLOCK) (
			EntityId,
			TenantId,
			FieldId,
			Data )
	SELECT
		del.EntityId,
		del.TenantId,
		del.FieldId,
		del.Data
	FROM (
		SELECT
			EntityId,
			TenantId,
			FieldId,
			Data,
			[Count] = COUNT( EntityId )
		FROM
			#Data_Int h
		WHERE
			h.Action = 0
		GROUP BY
			h.EntityId,
			h.TenantId,
			h.FieldId,
			h.Data,
			h.Action
		) del
	LEFT JOIN (
		SELECT
			EntityId,
			TenantId,
			FieldId,
			Data,
			[Count] = COUNT( EntityId )
		FROM
			#Data_Int h
		WHERE
			h.Action = 1
		GROUP BY
			h.EntityId,
			h.TenantId,
			h.FieldId,
			h.Data,
			h.Action
		) ins ON
			ins.EntityId = del.EntityId
			AND ins.TenantId = del.TenantId
			AND ins.FieldId = del.FieldId
			AND ins.Data = del.Data
	LEFT JOIN
		Data_Int p ON
			del.EntityId = p.EntityId
			AND del.TenantId = p.TenantId
			AND del.FieldId = p.FieldId
	WHERE
		ISNULL( del.[Count], 0 ) > ISNULL( ins.[Count], 0 )
		AND p.EntityId IS NULL

	SELECT @intInsert = ISNULL( @@ROWCOUNT, 0 )

	-----
	-- Delete nvarchar
	-----
	DELETE
		Data_NVarChar
	FROM
		Data_NVarChar d WITH (PAGLOCK)
	JOIN (
		SELECT
			ins.Id,
			ins.EntityId,
			ins.TenantId,
			ins.FieldId,
			ins.Data
		FROM (
			SELECT
				Id,
				EntityId,
				TenantId,
				FieldId,
				Data,
				[Count] = COUNT( EntityId )
			FROM
				#Data_NVarChar h
			WHERE
				h.Action = 1
			GROUP BY
				h.Id,
				h.EntityId,
				h.TenantId,
				h.FieldId,
				h.Data,
				h.Action
			) ins
		LEFT JOIN (
			SELECT
				Id,
				EntityId,
				TenantId,
				FieldId,
				Data,
				[Count] = COUNT( EntityId )
			FROM
				#Data_NVarChar h
			WHERE
				h.Action = 0
			GROUP BY
				h.Id,
				h.EntityId,
				h.TenantId,
				h.FieldId,
				h.Data,
				h.Action
			) del ON
				ins.Id = del.Id
				AND ins.EntityId = del.EntityId
				AND ins.TenantId = del.TenantId
				AND ins.FieldId = del.FieldId
				AND ins.Data = del.Data
		WHERE
			ISNULL( del.[Count], 0 ) < ISNULL( ins.[Count], 0 )
	) a ON
		d.Id = a.Id
		AND d.EntityId = a.EntityId
		AND d.TenantId = a.TenantId
		AND d.FieldId = a.FieldId
		AND d.Data = a.Data
	
	SELECT @nVarCharDelete = ISNULL( @@ROWCOUNT, 0 )

	-----
	-- Insert nvarchar
	-----
	SET IDENTITY_INSERT Data_NVarChar ON

	INSERT INTO
		Data_NVarChar WITH (PAGLOCK) (
			Id,
			EntityId,
			TenantId,
			FieldId,
			Data )
	SELECT
		del.Id,
		del.EntityId,
		del.TenantId,
		del.FieldId,
		del.Data
	FROM (
		SELECT
			Id,
			EntityId,
			TenantId,
			FieldId,
			Data,
			[Count] = COUNT( EntityId )
		FROM
			#Data_NVarChar h
		WHERE
			h.Action = 0
		GROUP BY
			h.Id,
			h.EntityId,
			h.TenantId,
			h.FieldId,
			h.Data,
			h.Action
		) del
	LEFT JOIN (
		SELECT
			Id,
			EntityId,
			TenantId,
			FieldId,
			Data,
			[Count] = COUNT( EntityId )
		FROM
			#Data_NVarChar h
		WHERE
			h.Action = 1
		GROUP BY
			h.Id,
			h.EntityId,
			h.TenantId,
			h.FieldId,
			h.Data,
			h.Action
		) ins ON
			ins.Id = del.Id
			AND ins.EntityId = del.EntityId
			AND ins.TenantId = del.TenantId
			AND ins.FieldId = del.FieldId
			AND ins.Data = del.Data
	LEFT JOIN
		Data_NVarChar p ON
			del.EntityId = p.EntityId
			AND del.TenantId = p.TenantId
			AND del.FieldId = p.FieldId
	WHERE
		ISNULL( del.[Count], 0 ) > ISNULL( ins.[Count], 0 )
		AND p.EntityId IS NULL

	SELECT @nVarCharInsert = ISNULL( @@ROWCOUNT, 0 )

	SET IDENTITY_INSERT Data_NVarChar OFF

	-----
	-- Delete xml
	-----
	DELETE
		Data_Xml
	FROM
		Data_Xml d WITH (PAGLOCK)
	JOIN (
		SELECT
			ins.EntityId,
			ins.TenantId,
			ins.FieldId,
			ins.Data
		FROM (
			SELECT
				EntityId,
				TenantId,
				FieldId,
				Data,
				[Count] = COUNT( EntityId )
			FROM
				#Data_Xml h
			WHERE
				h.Action = 1
			GROUP BY
				h.EntityId,
				h.TenantId,
				h.FieldId,
				h.Data,
				h.Action
			) ins
		LEFT JOIN (
			SELECT
				EntityId,
				TenantId,
				FieldId,
				Data,
				[Count] = COUNT( EntityId )
			FROM
				#Data_Xml h
			WHERE
				h.Action = 0
			GROUP BY
				h.EntityId,
				h.TenantId,
				h.FieldId,
				h.Data,
				h.Action
			) del ON
				ins.EntityId = del.EntityId
				AND ins.TenantId = del.TenantId
				AND ins.FieldId = del.FieldId
				AND ins.Data = del.Data
		WHERE
			ISNULL( del.[Count], 0 ) < ISNULL( ins.[Count], 0 )
	) a ON
		d.EntityId = a.EntityId
		AND d.TenantId = a.TenantId
		AND d.FieldId = a.FieldId
		AND d.Data = a.Data
	
	SELECT @xmlDelete = ISNULL( @@ROWCOUNT, 0 )
	
	-----
	-- Insert xml
	-----
	INSERT INTO
		Data_Xml WITH (PAGLOCK) (
			EntityId,
			TenantId,
			FieldId,
			Data )
	SELECT
		del.EntityId,
		del.TenantId,
		del.FieldId,
		del.Data
	FROM (
		SELECT
			EntityId,
			TenantId,
			FieldId,
			Data,
			[Count] = COUNT( EntityId )
		FROM
			#Data_Xml h
		WHERE
			h.Action = 0
		GROUP BY
			h.EntityId,
			h.TenantId,
			h.FieldId,
			h.Data,
			h.Action
		) del
	LEFT JOIN (
		SELECT
			EntityId,
			TenantId,
			FieldId,
			Data,
			[Count] = COUNT( EntityId )
		FROM
			#Data_Xml h
		WHERE
			h.Action = 1
		GROUP BY
			h.EntityId,
			h.TenantId,
			h.FieldId,
			h.Data,
			h.Action
		) ins ON
			ins.EntityId = del.EntityId
			AND ins.TenantId = del.TenantId
			AND ins.FieldId = del.FieldId
			AND ins.Data = del.Data
	LEFT JOIN
		Data_Xml p ON
			del.EntityId = p.EntityId
			AND del.TenantId = p.TenantId
			AND del.FieldId = p.FieldId
	WHERE
		ISNULL( del.[Count], 0 ) > ISNULL( ins.[Count], 0 )
		AND p.EntityId IS NULL

	SELECT @xmlInsert = ISNULL( @@ROWCOUNT, 0 )

	-----
	-- Delete relationship
	-----
	DELETE
		Relationship
	FROM
		Relationship r WITH (PAGLOCK)
	JOIN (
		SELECT
			ins.Id,
			ins.TenantId,
			ins.TypeId,
			ins.FromId,
			ins.ToId
		FROM (
			SELECT
				Id,
				TenantId,
				TypeId,
				FromId,
				ToId,
				[Count] = COUNT( Id )
			FROM
				#Relationship h
			WHERE
				h.Action = 1
			GROUP BY
				h.Id,
				h.TenantId,
				h.TypeId,
				h.FromId,
				h.ToId,
				h.Action
			) ins
		LEFT JOIN (
			SELECT
				Id,
				TenantId,
				TypeId,
				FromId,
				ToId,
				[Count] = COUNT( Id )
			FROM
				#Relationship h
			WHERE
				h.Action = 0
			GROUP BY
				h.Id,
				h.TenantId,
				h.TypeId,
				h.FromId,
				h.ToId,
				h.Action
			) del ON
				ins.Id = del.Id
				AND ins.TenantId = del.TenantId
				AND ins.TypeId = del.TypeId
				AND ins.FromId = del.FromId
				AND ins.ToId = del.ToId
		WHERE
			ISNULL( del.[Count], 0 ) < ISNULL( ins.[Count], 0 )
	) a ON
		r.Id = a.Id
		AND r.TenantId = a.TenantId
		AND r.TypeId = a.TypeId
		AND r.FromId = a.FromId
		AND r.ToId = a.ToId
	
	SELECT @relationshipDelete = ISNULL( @@ROWCOUNT, 0 )

	-----
	-- Delete entities
	-----
	DELETE
		Entity
	FROM
		Entity e WITH (PAGLOCK)
	JOIN (
		SELECT
			ins.Id,
			ins.TenantId,
			ins.UpgradeId
		FROM (
			SELECT
				Id,
				TenantId,
				UpgradeId,
				[Count] = COUNT( Id )
			FROM
				#Entity h
			WHERE
				h.Action = 1
			GROUP BY
				h.Id,
				h.TenantId,
				h.UpgradeId,
				h.Action
			) ins
		LEFT JOIN (
			SELECT
				Id,
				TenantId,
				UpgradeId,
				[Count] = COUNT( Id )
			FROM
				#Entity h
			WHERE
				h.Action = 0
			GROUP BY
				h.Id,
				h.TenantId,
				h.UpgradeId,
				h.Action
			) del ON
				ins.Id = del.Id
				AND ins.TenantId = del.TenantId
				AND ins.UpgradeId = del.UpgradeId
		WHERE
			ISNULL( del.[Count], 0 ) < ISNULL( ins.[Count], 0 )
	) a ON
		e.Id = a.Id
		AND e.TenantId = a.TenantId
		AND e.UpgradeId = a.UpgradeId
	
	SELECT @entityDelete = ISNULL( @@ROWCOUNT, 0 )

	COMMIT TRANSACTION

	DROP TABLE #Entity
	DROP TABLE #Relationship
	DROP TABLE #Data_Alias
	DROP TABLE #Data_Bit
	DROP TABLE #Data_DateTime
	DROP TABLE #Data_Decimal
	DROP TABLE #Data_Guid
	DROP TABLE #Data_Int
	DROP TABLE #Data_NVarChar
	DROP TABLE #Data_Xml

	SELECT 'Entity', Inserted = @entityInsert, Deleted = @entityDelete
	UNION ALL
	SELECT 'Relationship', Inserted = @relationshipInsert, Deleted = @relationshipDelete
	UNION ALL
	SELECT 'Data_Alias', Inserted = @aliasInsert, Deleted = @aliasDelete
	UNION ALL
	SELECT 'Data_Bit', Inserted = @bitInsert, Deleted = @bitDelete
	UNION ALL
	SELECT 'Data_DateTime', Inserted = @dateTimeInsert, Deleted = @dateTimeDelete
	UNION ALL
	SELECT 'Data_Decimal', Inserted = @decimalInsert, Deleted = @decimalDelete
	UNION ALL
	SELECT 'Data_Guid', Inserted = @guidInsert, Deleted = @guidDelete
	UNION ALL
	SELECT 'Data_Int', Inserted = @intInsert, Deleted = @intDelete
	UNION ALL
	SELECT 'Data_NVarChar', Inserted = @nVarCharInsert, Deleted = @nVarCharDelete
	UNION ALL
	SELECT 'Data_Xml', Inserted = @xmlInsert, Deleted = @xmlDelete
END