-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE FUNCTION [dbo].[fnGenerateCSV]
(
	@entityId BIGINT,
	@relationshipTypeId BIGINT,
	@fieldId BIGINT,
	@tenantId BIGINT,
	@delimiter NVARCHAR( MAX ) = N', '
)
RETURNS NVARCHAR( MAX )
AS
BEGIN
	-----
	-- Use this scalar valued function to coalescse multiple string values
	-- into a single delimited string value.
	-- Usage:
	--		SELECT *, fnGenerateCSV( e.Id, <inheritsId>, <nameId>, <tenantId>, ', ' )
	--		FROM Entity e
	-----
	DECLARE @output NVARCHAR( MAX )
	
	SELECT
		@output = COALESCE( @output + @delimiter, '' ) + d.Data
	FROM
		Relationship r
	JOIN
		Data_NVarChar d ON
			r.ToId = d.EntityId AND
			d.TenantId = r.TenantId AND
			r.TenantId = @tenantId AND
			r.FromId = @entityId AND
			r.TypeId = @relationshipTypeId
	WHERE
		d.FieldId = @fieldId
	ORDER BY
		d.Data

	RETURN @output
END

