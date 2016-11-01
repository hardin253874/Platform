-- Copyright 2011-2016 Global Software Innovation Pty Ltd
CREATE FULLTEXT INDEX ON [dbo].[Data_NVarChar]
    ([Data] LANGUAGE 1033)
    KEY INDEX [IDX_Data_NVarChar]
    ON [Data_NVarChar_Catalog];
