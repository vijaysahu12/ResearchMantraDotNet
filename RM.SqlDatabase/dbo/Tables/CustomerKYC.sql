CREATE TABLE [dbo].[CustomerKYC] (
    [Id]         BIGINT         IDENTITY (1, 1) NOT NULL,
    [LeadKey]    VARCHAR (36)   NOT NULL,
    [PANURL]     NVARCHAR (50)  NULL,
    [PAN]        VARCHAR (10)   NULL,
    [Verified]   BIT            NULL,
    [CreatedOn]  DATETIME       NOT NULL,
    [ModifiedOn] DATETIME       NULL,
    [ModifiedBy] VARCHAR (36)   NULL,
    [IsDelete]   BIT            NULL,
    [Status]     VARCHAR (50)   NULL,
    [Remarks]    NVARCHAR (200) NULL,
    [JsonData]   NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_CustomerKYC] PRIMARY KEY CLUSTERED ([Id] ASC)
);

