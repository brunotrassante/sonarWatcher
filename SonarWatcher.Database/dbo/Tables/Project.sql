CREATE TABLE [dbo].[Project] (
    [Id]       INT           IDENTITY (1, 1) NOT NULL,
    [Name]     VARCHAR (150) NOT NULL,
    [SonarKey] VARCHAR (150) NULL,
    [Active] INT NULL, 
    CONSTRAINT [PK_Project] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO



