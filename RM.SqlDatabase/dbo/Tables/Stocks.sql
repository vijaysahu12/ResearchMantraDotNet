CREATE TABLE [dbo].[Stocks] (
    [Id]          INT              IDENTITY (1, 1) NOT NULL,
    [Name]        VARCHAR (100)    NULL,
    [Description] VARCHAR (300)    NULL,
    [LotSize]     INT              DEFAULT ((1)) NULL,
    [IsDisabled]  TINYINT          DEFAULT ((0)) NULL,
    [IsDelete]    TINYINT          DEFAULT ((0)) NULL,
    [PublicKey]   UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [CreatedOn]   DATETIME         DEFAULT (getdate()) NULL,
    [CreatedBy]   VARCHAR (100)    NULL,
    [ModifiedOn]  DATETIME         DEFAULT (NULL) NULL,
    [ModifiedBy]  VARCHAR (100)    NULL,
    PRIMARY KEY CLUSTERED ([PublicKey] ASC)
);

