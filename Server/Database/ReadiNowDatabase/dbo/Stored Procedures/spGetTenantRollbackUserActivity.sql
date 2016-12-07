-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spGetTenantRollbackUserActivity]
	@date DATETIME,
	@tenantId BIGINT

AS

BEGIN

	SELECT
		Username =
			CASE WHEN
				UserId = 0
			THEN
				'System Administrator'
			ELSE
				ISNULL( dbo.fnName( UserId ), 'Unknown' )
			END
	FROM
		Hist_Transaction
	WHERE
		[Timestamp] >= @date
		AND TenantId = @tenantId
	GROUP BY
		UserId

END