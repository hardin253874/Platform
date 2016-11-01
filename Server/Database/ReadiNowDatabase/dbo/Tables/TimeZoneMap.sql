-- Copyright 2011-2016 Global Software Innovation Pty Ltd

CREATE TABLE [dbo].[TimeZoneMap] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [Olson]     NVARCHAR (256) NULL,
    [Microsoft] NVARCHAR (256) NULL
);


GO
CREATE CLUSTERED INDEX [IX_TimeZoneMap]
    ON [dbo].[TimeZoneMap]([Id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TimeZoneMap_Microsoft]
    ON [dbo].[TimeZoneMap]([Microsoft] ASC)
    INCLUDE([Olson]);


GO
CREATE NONCLUSTERED INDEX [IX_TimeZoneMap_Olson]
    ON [dbo].[TimeZoneMap]([Olson] ASC)
    INCLUDE([Microsoft]);

