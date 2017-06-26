CREATE TABLE [dbo].[ProjectPerson] (
    [ProjectId] INT NOT NULL,
    [PersonId]  INT NOT NULL,
    [RoleId]    INT NOT NULL
);






GO
CREATE NONCLUSTERED INDEX [IX_Projectperson]
    ON [dbo].[ProjectPerson]([ProjectId] ASC);

