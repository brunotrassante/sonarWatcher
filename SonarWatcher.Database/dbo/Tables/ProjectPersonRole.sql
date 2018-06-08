CREATE TABLE [dbo].[ProjectPersonRole] (
    [ProjectId] INT NOT NULL,
    [PersonId]  INT NOT NULL,
    [RoleId]    INT NULL,
    [Active]    BIT CONSTRAINT [DF_ProjectPersonRole_Active] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [FK_ProjectPerson_Person] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person] ([Id]),
    CONSTRAINT [FK_ProjectPerson_Project] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Project] ([Id]),
    CONSTRAINT [FK_ProjectPersonRole_Role] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Role] ([Id])
);




GO
CREATE NONCLUSTERED INDEX [IX_Projectperson]
    ON [dbo].[ProjectPersonRole]([ProjectId] ASC);

