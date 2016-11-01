-- Copyright 2011-2016 Global Software Innovation Pty Ltd
/*
Ensure a trackable identity is set for every database on creation
so that platforms may be managed and reconciled across multiple
instances.
*/

IF (NOT EXISTS (SELECT 1 FROM [RNDB]))
BEGIN
	INSERT INTO [RNDB] DEFAULT VALUES
END