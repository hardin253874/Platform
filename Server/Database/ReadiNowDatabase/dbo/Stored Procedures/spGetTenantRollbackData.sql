
-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE PROCEDURE [dbo].[spGetTenantRollbackData]
	@days INT,
	@tenantId BIGINT

AS

BEGIN

	DECLARE @time DATE
	DECLARE @dateTime DATETIME = NULL

	IF ( @days > 0 )
	BEGIN
		SET @time = DATEADD( dd, -@days, GETUTCDATE( ) )
		SET @dateTime = CAST( @time AS DATETIME )
	END

	DECLARE @lastSystemUpdate BIGINT

	SELECT
		@lastSystemUpdate = TransactionId
	FROM
		Hist_Transaction
	WHERE
		TenantId = @tenantId
		AND SystemUpgrade = 1
	ORDER BY
		TransactionId ASC

	SELECT
		Year = DATEPART(yy, Timestamp),
		Month = DATEPART(mm, Timestamp),
		Day = DATEPART(dd, Timestamp),
		Hour = DATEPART(hh, Timestamp),
		Minute = DATEPART(mi, Timestamp) / 10 * 10
	FROM
		Hist_Transaction
	WHERE
		TenantId = @tenantId
		AND ( @lastSystemUpdate IS NULL OR TransactionId > @lastSystemUpdate )
		AND ( @dateTime IS NULL OR [Timestamp] > @dateTime )
		AND [UserDefined] = 0
	GROUP BY
		DATEPART(yy, Timestamp),
		DATEPART(mm, Timestamp),
		DATEPART(dd, Timestamp),
		DATEPART(hh, Timestamp),
		DATEPART(mi, Timestamp) / 10
	ORDER BY
		DATEPART(yy, Timestamp),
		DATEPART(mm, Timestamp),
		DATEPART(dd, Timestamp),
		DATEPART(hh, Timestamp),
		DATEPART(mi, Timestamp) / 10

	SELECT
		Timestamp,
		Context
	FROM
		Hist_Transaction
	WHERE
		TenantId = @tenantId
		AND ( @lastSystemUpdate IS NULL OR TransactionId > @lastSystemUpdate )
		AND ( @dateTime IS NULL OR [Timestamp] > @dateTime )
		AND UserDefined = 1

	SELECT
		Date = a.[Timestamp],
		RollbackDate = b.[Timestamp],
		Username =
			CASE WHEN
				a.UserId = 0
			THEN
				'System Administrator'
			ELSE
				ISNULL( dbo.fnName( a.UserId ), 'Unknown' )
			END
	FROM
		Hist_Transaction a
	LEFT JOIN
		Hist_Transaction b ON
			a.RevertTo = b.TransactionId
	WHERE
		a.TenantId = @tenantId
		AND a.RevertTo IS NOT NULL
	ORDER BY
		a.[Timestamp]

END