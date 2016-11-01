-- Copyright 2011-2016 Global Software Innovation Pty Ltd
/*
List the table names significant to the license metrics gathering.
*/

IF (NOT EXISTS (SELECT 1 FROM [Lic_Table_Name]))
BEGIN
	INSERT INTO [Lic_Table_Name]
	VALUES
    (0,'Entity'),
	(1,'Relationship'),
	(2,'Data_Alias'),
	(3,'Data_Bit'),
	(4,'Data_DateTime'),
	(5,'Data_Decimal'),
	(6,'Data_Guid'),
	(7,'Data_Int'),
	(8,'Data_NVarChar'),
	(9,'Data_Xml')
END