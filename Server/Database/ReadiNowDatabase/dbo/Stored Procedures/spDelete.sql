-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spDelete]
(
	@entityId BIGINT,
	@tenantId BIGINT
)
AS
BEGIN

	SET NOCOUNT ON;

	DECLARE @batchGuid UNIQUEIDENTIFIER = NEWID( )

	INSERT INTO
		Batch ( BatchGuid )
	VALUES
		( @batchGuid )

	DECLARE @batchId BIGINT = @@IDENTITY
	
	IF ( @entityId IS NOT NULL )
	BEGIN
		INSERT INTO
			EntityBatch
		VALUES
			( @batchId, @entityId )
			
		EXEC spDeleteBatch @batchId, @tenantId
	END
END
