CREATE TABLE [dbo].[CustomerTags] (
    [Id]          INT              IDENTITY (1, 1) NOT NULL,
    [CustomerKey] NVARCHAR (100)   NULL,
    [TagKey]      NVARCHAR (100)   NULL,
    [IsDelete]    TINYINT          DEFAULT ((0)) NULL,
    [PublicKey]   UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NULL,
    [CreatedOn]   DATETIME         DEFAULT (getdate()) NULL,
    [CreatedBy]   VARCHAR (100)    NULL,
    [ModifiedOn]  DATETIME         DEFAULT (NULL) NULL,
    [ModifiedBy]  VARCHAR (100)    NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

