-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[TimeZone] (
    [Zone_id]      INT         NOT NULL,
    [Abbreviation] VARCHAR (6) NOT NULL,
    [Time_start]   INT         NOT NULL,
    [Gmt_offset]   INT         NOT NULL,
    [Dst]          CHAR (1)    NOT NULL
);


GO
CREATE CLUSTERED INDEX [IX_TimeZone]
    ON [dbo].[TimeZone]([Zone_id] ASC);

