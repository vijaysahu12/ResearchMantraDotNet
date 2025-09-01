CREATE TABLE [dbo].[Customers] (
    [Id]              BIGINT           IDENTITY (1, 1) NOT NULL,
    [CustomerTypeKey] VARCHAR (100)    NULL,
    [SegmentKey]      VARCHAR (100)    NULL,
    [TotalPurchases]  DECIMAL (12, 2)  NULL,
    [Remarks]         VARCHAR (300)    NULL,
    [LeadKey]         VARCHAR (100)    NULL,
    [IsDelete]        TINYINT          DEFAULT ((0)) NULL,
    [PublicKey]       UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NULL,
    [CreatedOn]       DATETIME         DEFAULT (getdate()) NULL,
    [CreatedBy]       VARCHAR (100)    NULL,
    [ModifiedOn]      DATETIME         DEFAULT (NULL) NULL,
    [ModifiedBy]      VARCHAR (100)    NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

