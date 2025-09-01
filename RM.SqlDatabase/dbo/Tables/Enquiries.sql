CREATE TABLE [dbo].[Enquiries] (
    [Id]           INT              IDENTITY (1, 1) NOT NULL,
    [Details]      VARCHAR (1000)   NULL,
    [IsLead]       TINYINT          DEFAULT ((1)) NULL,
    [ReferenceKey] VARCHAR (100)    NULL,
    [IsDisabled]   TINYINT          DEFAULT ((0)) NULL,
    [IsDelete]     TINYINT          DEFAULT ((0)) NULL,
    [PublicKey]    UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NULL,
    [CreatedOn]    DATETIME         DEFAULT (getdate()) NULL,
    [CreatedBy]    VARCHAR (100)    NULL,
    [ModifiedOn]   DATETIME         DEFAULT (NULL) NULL,
    [ModifiedBy]   VARCHAR (100)    NULL,
    [IsAdmin]      TINYINT          DEFAULT ((1)) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

