CREATE TABLE [dbo].[Pincodes] (
    [Id]         BIGINT           IDENTITY (1, 1) NOT NULL,
    [Pincode]    VARCHAR (50)     NULL,
    [Area]       VARCHAR (150)    NULL,
    [Division]   VARCHAR (150)    NULL,
    [District]   VARCHAR (150)    NULL,
    [State]      VARCHAR (150)    NULL,
    [IsDisabled] TINYINT          DEFAULT ((0)) NULL,
    [IsDelete]   TINYINT          DEFAULT ((0)) NULL,
    [PublicKey]  UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NULL,
    [CreatedOn]  DATETIME         DEFAULT (getdate()) NULL,
    [CreatedBy]  VARCHAR (100)    NULL,
    [ModifiedOn] DATETIME         DEFAULT (NULL) NULL,
    [ModifiedBy] VARCHAR (100)    NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

