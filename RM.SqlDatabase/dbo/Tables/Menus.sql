CREATE TABLE [dbo].[Menus] (
    [Id]          INT              IDENTITY (1, 1) NOT NULL,
    [Name]        VARCHAR (100)    NULL,
    [Description] VARCHAR (250)    NULL,
    [Url]         VARCHAR (250)    NULL,
    [ParentId]    INT              DEFAULT ((0)) NULL,
    [SortOrder]   INT              DEFAULT ((100)) NULL,
    [IsDisabled]  TINYINT          DEFAULT ((0)) NULL,
    [IsDelete]    TINYINT          DEFAULT ((0)) NULL,
    [PublicKey]   UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NULL,
    [CreatedOn]   DATETIME         DEFAULT (getdate()) NULL,
    [CreatedBy]   VARCHAR (100)    NULL,
    [ModifiedOn]  DATETIME         DEFAULT (NULL) NULL,
    [ModifiedBy]  VARCHAR (100)    NULL,
    [IsLHS]       INT              NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

