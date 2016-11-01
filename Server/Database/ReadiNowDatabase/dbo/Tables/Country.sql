-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Country] (
    [Country_code] CHAR (2)     NOT NULL,
    [Country_name] VARCHAR (45) NOT NULL,
	CONSTRAINT [PK_Country] PRIMARY KEY CLUSTERED ([Country_code] ASC)
);

