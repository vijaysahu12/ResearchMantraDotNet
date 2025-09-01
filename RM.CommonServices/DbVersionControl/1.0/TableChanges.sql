alter table CompanyDetailM drop column PERatio,MarketCapInCrores;
ALTER TABLE BasketsM 
ADD SortOrder INT NULL;


GO
INSERT INTO Settings values ('android,ios','enablePGFor' ,1 )


GO 
ALTER TABLE [dbo].[ScreenerStockDataM]
ALTER COLUMN [ScreenerId] INT NOT NULL;

ALTER TABLE [dbo].[ScreenerStockDataM]
ALTER COLUMN [SymbolId] INT NOT NULL;


---------------------------------------------------------------------------------------------------------------------------------------------------------------------
USE [KingresearchTest]
GO

/****** Object:  Table [dbo].[PaymentRequestStatusM]    Script Date: 27-11-2024 17:06:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PaymentRequestStatusM](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[TransactionId] [nvarchar](300) NOT NULL,
	[Amount] [decimal](18, 2) NULL,
	[SubcriptionModelId] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[Status] [varchar](50) NULL,
 CONSTRAINT [PK_PaymentRequestStatus] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[PaymentRequestStatusM] ADD  CONSTRAINT [DF_PaymentRequestStatus_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO

---------------------------------------------------------------------------------------------------------------------------------------------------------------------
ALTER TABLE ProductsContentM
ADD IsActive bit DEFAULT 1,
    IsDeleted bit DEFAULT 0;