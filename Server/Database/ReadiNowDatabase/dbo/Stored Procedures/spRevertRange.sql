
-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spRevertRange]
	@fromTransactionId BIGINT,
	@toTransactionId BIGINT,
	@tenantId BIGINT = NULL,
	@context VARCHAR(128) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	IF ( @context IS NULL )
	BEGIN
		IF ( @tenantId IS NULL )
		BEGIN
			SET @context = 'Reverting transaction range ' + CAST( @fromTransactionId AS VARCHAR( 20 ) ) + ' to ' + CAST( @toTransactionId AS VARCHAR( 20 ) )
		END
		ELSE
		BEGIN
			SET @context = 'Reverting transaction range ' + CAST( @fromTransactionId AS VARCHAR( 20 ) ) + ' to ' + CAST( @toTransactionId AS VARCHAR( 20 ) ) + ' for tenant ' + CAST( @tenantId AS VARCHAR( 20 ) )
		END
	END

	IF ( @context IS NOT NULL )
	BEGIN
		DECLARE @contextInfo VARBINARY(128) = CONVERT( VARBINARY(128), @context )
		SET CONTEXT_INFO @contextInfo
	END

	BEGIN TRANSACTION

	DECLARE @finalResults TABLE ( Name NVARCHAR( 100 ), Inserted INT, Deleted INT )
	DECLARE @results TABLE ( Name NVARCHAR( 100 ), Inserted INT, Deleted INT )

	DECLARE Transactions CURSOR FAST_FORWARD FOR
		SELECT
			TransactionId
		FROM
			Hist_Transaction
		WHERE
			TransactionId >= @fromTransactionId
			AND TransactionId <= @toTransactionId
			AND (
				@tenantId IS NULL
				OR TenantId = @tenantId
			)
		ORDER BY
			TransactionId DESC

	OPEN Transactions

	DECLARE @currentTransactionId BIGINT

	FETCH NEXT FROM Transactions into @currentTransactionId
	WHILE @@fetch_status = 0
	BEGIN
		INSERT @results EXEC spRevert @currentTransactionId, @tenantId, @context

		MERGE @finalResults AS target
		USING ( SELECT Name, Inserted, Deleted FROM @results ) AS source ( Name, Inserted, Deleted )
		ON ( target.Name = source.Name )
		WHEN MATCHED THEN
			UPDATE SET
				target.Inserted = source.Inserted + target.Inserted,
				target.Deleted = source.Deleted + target.Deleted
		WHEN NOT MATCHED THEN
			INSERT ( Name, Inserted, Deleted )
			VALUES ( source.Name, source.Inserted, source.Deleted );

		DELETE FROM @results

		FETCH NEXT FROM Transactions INTO @currentTransactionId
	END

	CLOSE Transactions
	DEALLOCATE Transactions

	COMMIT TRANSACTION

	SELECT
		Name,
		Inserted,
		Deleted
	FROM
		@finalResults
END