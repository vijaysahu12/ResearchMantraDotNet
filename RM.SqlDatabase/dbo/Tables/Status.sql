CREATE TABLE [dbo].[Status] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [Name]        VARCHAR (50)  NOT NULL,
    [Category]    VARCHAR (50)  NULL,
    [Description] VARCHAR (200) NULL,
    [Code]        VARCHAR (5)   NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

