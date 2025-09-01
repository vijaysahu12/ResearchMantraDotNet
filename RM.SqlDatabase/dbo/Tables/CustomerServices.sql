CREATE TABLE [dbo].[CustomerServices] (
    [Id]                BIGINT           IDENTITY (1, 1) NOT NULL,
    [CustomerKey]       VARCHAR (100)    NULL,
    [ServiceKey]        VARCHAR (100)    NULL,
    [PaymentModeKey]    VARCHAR (100)    NULL,
    [ActualCost]        DECIMAL (12, 2)  DEFAULT ((0)) NULL,
    [AmountPaid]        DECIMAL (12, 2)  DEFAULT ((0)) NULL,
    [AmountOutstanding] DECIMAL (12, 2)  DEFAULT ((0)) NULL,
    [Discount]          DECIMAL (12, 2)  DEFAULT ((0)) NULL,
    [IsWon]             TINYINT          NULL,
    [OrderId]           VARCHAR (300)    NULL,
    [Remarks]           VARCHAR (500)    NULL,
    [IsDisabled]        TINYINT          DEFAULT ((0)) NULL,
    [IsDelete]          TINYINT          DEFAULT ((0)) NULL,
    [PublicKey]         UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NULL,
    [CreatedOn]         DATETIME         DEFAULT (getdate()) NULL,
    [CreatedBy]         NVARCHAR (100)   DEFAULT ((1)) NULL,
    [ModifiedOn]        DATETIME         DEFAULT (NULL) NULL,
    [ModifiedBy]        VARCHAR (100)    NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

