-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spMergeNVarChar]
	@data dbo.Data_NVarCharType READONLY
AS
BEGIN

	SET NOCOUNT ON;

	MERGE
		dbo.Data_NVarChar dest
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
			dest.Data COLLATE Latin1_General_CS_AS
		EXCEPT
		SELECT
			src.Data COLLATE Latin1_General_CS_AS ) THEN
		UPDATE
			SET dest.Data = src.Data;
END