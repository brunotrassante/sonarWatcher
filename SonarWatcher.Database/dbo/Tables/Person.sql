CREATE TABLE [dbo].[Person] (
    [Id]    INT           IDENTITY (1, 1) NOT NULL,
    [Name]  VARCHAR (150) NOT NULL,
    [Email] VARCHAR (150) NOT NULL,
    [Active] INT NULL, 
    CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED ([Id] ASC)
);

