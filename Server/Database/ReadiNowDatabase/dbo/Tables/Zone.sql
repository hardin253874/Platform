-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[Zone] (
    [Zone_id]      INT          IDENTITY (1, 1) NOT NULL,
    [Country_code] CHAR (2)     NOT NULL,
    [Zone_name]    VARCHAR (35) NOT NULL,
    CONSTRAINT [PK_Zone] PRIMARY KEY CLUSTERED ([Zone_id] ASC)
);

