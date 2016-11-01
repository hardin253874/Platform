
-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spDeleteListDetails]
(
	@entityIds [dbo].[UniqueIdListType] READONLY,
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
	
	INSERT INTO
		EntityBatch
	SELECT
		@batchId,
		Id
	FROM
		@entityIds
			
	EXEC spDeleteBatchDetails @batchId, @tenantId
END