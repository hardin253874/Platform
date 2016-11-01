-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spDetermineCascade]
(
	@entityId bigint,
	@tenantId bigint
)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @batchGuid UNIQUEIDENTIFIER = NEWID()

	INSERT INTO
		Batch ( BatchGuid )
	VALUES
		( @batchGuid )

	DECLARE @batchId BIGINT = @@IDENTITY
	
	IF ( @entityId IS NOT NULL )
	BEGIN
		INSERT
			INTO EntityBatch
		VALUES
			(@batchId, @entityId)
			
		EXEC spDetermineCascadeBatch @batchId, @tenantId

		SELECT
			*
		FROM
			EntityBatch
		WHERE
			BatchId = @batchId

		DELETE FROM
			EntityBatch
		WHERE
			BatchId = @batchId

		DELETE FROM
			Batch
		WHERE
			BatchId = @batchId
	END
END

