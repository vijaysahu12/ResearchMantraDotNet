CREATE TABLE [dbo].[Services] (
    [Id]                 INT              IDENTITY (1, 1) NOT NULL,
    [Name]               VARCHAR (250)    NULL,
    [Description]        VARCHAR (MAX)    NULL,
    [ServiceCost]        DECIMAL (12, 2)  NULL,
    [ServiceTypeKey]     VARCHAR (100)    NULL,
    [ServiceCategoryKey] VARCHAR (100)    NULL,
    [IsDisabled]         TINYINT          DEFAULT ((0)) NULL,
    [IsDelete]           TINYINT          DEFAULT ((0)) NULL,
    [PublicKey]          UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NULL,
    [CreatedOn]          DATETIME         DEFAULT (getdate()) NULL,
    [CreatedBy]          VARCHAR (100)    NULL,
    [ModifiedOn]         DATETIME         DEFAULT (NULL) NULL,
    [ModifiedBy]         VARCHAR (100)    NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

