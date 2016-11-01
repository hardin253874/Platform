
-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spCreateRestorePoint]
	@tenantId BIGINT,
	@context VARCHAR( 128 ) = NULL,
	@userDefined BIT = 0,
	@systemUpgrade BIT = 0,
	@revertTo BIGINT = NULL,
	@tranId BIGINT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @spid SMALLINT = @@SPID
	DECLARE @internalId BIGINT
	DECLARE @userId BIGINT = NULL

	SELECT
		@internalId = [transaction_id]
	FROM
		[sys].[dm_tran_current_transaction]

	IF ( @context IS NULL )
	BEGIN
		DECLARE @contextInfo VARBINARY(128) = CONTEXT_INFO()

		IF ( @contextInfo IS NOT NULL )
		BEGIN
			SET @context  = REPLACE( CAST( CAST( @contextInfo AS VARCHAR( 128 ) ) COLLATE SQL_Latin1_General_CP1_CS_AS AS VARCHAR( 128 ) ), CHAR( 0 ), '' )
		END
	END

	IF ( @userId IS NULL )
	BEGIN
		IF ( @context IS NOT NULL AND LEN( @context ) > 0 )
		BEGIN
			IF ( SUBSTRING( @context, 1, 2 ) = 'u:' )
			BEGIN
				DECLARE @userEndPosition INT = CHARINDEX(',', @context )

				DECLARE @userIdString VARCHAR( 128 )

				IF ( @userEndPosition > 2 )
				BEGIN
					SET @userIdString = SUBSTRING( @context, 3, @userEndPosition - 3 )

					SET @userId = TRY_CONVERT( BIGINT, @userIdString )

					IF ( LEN( @context ) > @userEndPosition )
					BEGIN
						SET @context = SUBSTRING( @context, @userEndPosition + 1, LEN( @context ) - @userEndPosition )
					END
					ELSE
					BEGIN
						SET @context = ''
					END
				END
				ELSE
				BEGIN
					IF ( LEN( @context ) > 2 )
					BEGIN
						SET @userIdString = SUBSTRING( @context, 3, LEN( @context ) - 2 )

						SET @userId = TRY_CONVERT( BIGINT, @userIdString )
						SET @context = ''
					END
				END
			END
		END
	END

	IF ( @tenantId IS NULL )
	BEGIN
		SET @tenantId = -1
	END

	IF ( @userId IS NULL )
	BEGIN
		SET @userId = 0
	END

	DECLARE @existingTransactionId BIGINT = TRY_CONVERT( BIGINT, @context )

	IF ( @existingTransactionId IS NOT NULL )
	BEGIN
		SELECT
			@tranId = TransactionId
		FROM
			Hist_Transaction
		WHERE
			TransactionId = @existingTransactionId
	END
	
	IF ( @tranId IS NULL )
	BEGIN
		SELECT
			@internalId = [transaction_id]
		FROM
			[sys].[dm_tran_current_transaction]

		DECLARE @now DATETIME = GETUTCDATE()

		-----
		-- Ensure that the transaction occurred within the last hour.
		-- This is to handle the case where the transaction id is
		-- reset due to a server restart.
		-----
		SELECT
			@tranId = TransactionId
		FROM
			Hist_Transaction
		WHERE
			InternalId = @internalId
			AND DATEDIFF( hour, [Timestamp], @now ) < 1
	END

	IF ( @tranId IS NULL )
	BEGIN
		INSERT INTO
			[Hist_Transaction] (
				[InternalId],
				[UserId],
				[TenantId],
				[Spid],
				[Timestamp],
				[HostName],
				[ProgramName],
				[Domain],
				[Username],
				[LoginName],
				[Context],
				[UserDefined],
				[SystemUpgrade],
				[RevertTo]
			)		
		SELECT
			@internalId,
			@userId,
			@tenantId,
			@spid,
			GETUTCDATE(),
			[hostname],
			[program_name],
			[nt_domain],
			[nt_username],
			[loginame],
			@context,
			@userDefined,
			@systemUpgrade,
			@revertTo
		FROM
			[master].[dbo].[sysprocesses] [sp]
		WHERE
			[sp].[spid] = @spid

		SELECT @tranId = SCOPE_IDENTITY()
	END
END