-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spMergeInt]
	@data dbo.Data_IntType READONLY
AS
BEGIN

	SET NOCOUNT ON;

	MERGE
		dbo.Data_Int dest
	USING
		@data AS src
	ON
		dest.EntityId = src.EntityId AND
		dest.TenantId = src.TenantId AND
		dest.FieldId = src.FieldId
	WHEN NOT MATCHED THEN
		INSERT (
			EntityId,
			TenantId,
			FieldId,
			Data )
		VALUES (
			src.EntityId,
			src.TenantId,
			src.FieldId,
			src.Data )
	WHEN MATCHED AND EXISTS (
		SELECT
			dest.Data
		EXCEPT
		SELECT
			src.Data ) THEN
		UPDATE
			SET dest.Data = src.Data;
END