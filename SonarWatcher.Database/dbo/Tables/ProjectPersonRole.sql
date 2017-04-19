CREATE TABLE [dbo].[ProjectPersonRole] (
    [ProjectId] INT NOT NULL,
    [PersonId]  INT NOT NULL,
    [RoleId]    INT NULL,
    CONSTRAINT [FK_ProjectPerson_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person] ([Id]),
    CONSTRAINT [FK_ProjectPerson_Project] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Project] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_Projectperson]
    ON [dbo].[ProjectPersonRole]([ProjectId] ASC);

