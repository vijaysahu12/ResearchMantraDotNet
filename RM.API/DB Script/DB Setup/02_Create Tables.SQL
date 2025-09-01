USE [KingResearch]
GO
/****** Object:  Table [dbo].[CallPerformance]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CallPerformance](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TradeType] [varchar](30) NOT NULL,
	[CallDate] [datetime] NOT NULL,
	[CallByKey] [varchar](50) NULL,
	[StrategyKey] [varchar](50) NOT NULL,
	[StockKey] [varchar](50) NOT NULL,
	[SegmentKey] [varchar](50) NULL,
	[LotSize] [int] NULL,
	[OptionValue] [varchar](500) NULL,
	[ExpiryKey] [varchar](50) NULL,
	[EntryPrice] [decimal](12, 2) NULL,
	[Target1Price] [decimal](12, 2) NULL,
	[Target2Price] [decimal](12, 2) NULL,
	[Target3Price] [decimal](12, 2) NULL,
	[StopLossPrice] [decimal](12, 2) NULL,
	[EntryTime] [varchar](20) NULL,
	[TriggerTime] [varchar](20) NULL,
	[ExitPrice] [decimal](12, 2) NULL,
	[ResultHigh] [decimal](12, 2) NULL,
	[PlottedCapital] [decimal](12, 2) NULL,
	[Pnl] [decimal](12, 2) NULL,
	[Roi] [decimal](12, 2) NULL,
	[Remarks] [varchar](500) NULL,
	[CallStatus] [varchar](50) NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[IsIntraday] [bit] NULL,
	[ResultTypeKey] [varchar](50) NULL,
	[IsPublic] [bit] NULL,
	[IsDisabled] [bit] NULL,
	[IsDelete] [bit] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [varchar](100) NOT NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
 CONSTRAINT [PK__CallPerf__3214EC0768916350] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CommunicationModes]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommunicationModes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL,
	[Description] [varchar](300) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CommunicationTemplates]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommunicationTemplates](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL,
	[ModeKey] [varchar](100) NULL,
	[Template] [nvarchar](max) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CustomerKYC]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerKYC](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[LeadKey] [varchar](36) NOT NULL,
	[PANURL] [nvarchar](50) NULL,
	[PAN] [varchar](10) NULL,
	[Verified] [bit] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](36) NULL,
	[IsDelete] [bit] NULL,
	[Status] [varchar](50) NULL,
	[Remarks] [nvarchar](200) NULL,
	[JsonData] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_CustomerKYC] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Customers]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Customers](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomerTypeKey] [varchar](100) NULL,
	[SegmentKey] [varchar](100) NULL,
	[TotalPurchases] [decimal](12, 2) NULL,
	[Remarks] [varchar](300) NULL,
	[LeadKey] [varchar](100) NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CustomerServices]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerServices](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CustomerKey] [varchar](100) NULL,
	[ServiceKey] [varchar](100) NULL,
	[PaymentModeKey] [varchar](100) NULL,
	[ActualCost] [decimal](12, 2) NULL,
	[AmountPaid] [decimal](12, 2) NULL,
	[AmountOutstanding] [decimal](12, 2) NULL,
	[Discount] [decimal](12, 2) NULL,
	[IsWon] [tinyint] NULL,
	[OrderId] [varchar](300) NULL,
	[Remarks] [varchar](500) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [nvarchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CustomerTags]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerTags](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerKey] [nvarchar](100) NULL,
	[TagKey] [nvarchar](100) NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CustomerTypes]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL,
	[Description] [varchar](300) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Enquiries]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Enquiries](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Details] [varchar](1000) NULL,
	[IsLead] [tinyint] NULL,
	[ReferenceKey] [varchar](100) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
	[IsAdmin] [tinyint] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExceptionLogs]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExceptionLogs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ExceptionType] [nvarchar](max) NULL,
	[ErrorMessage] [nvarchar](max) NULL,
	[StackTrace] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[Notes] [nvarchar](max) NULL,
 CONSTRAINT [PK_ExceptionLogs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Expirys]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Expirys](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL,
	[Description] [varchar](300) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[PublicKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Leads]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Leads](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[PublicKey] [uniqueidentifier] NOT NULL,
	[FullName] [varchar](150) NULL,
	[Gender] [char](1) NULL,
	[MobileNumber] [varchar](100) NOT NULL,
	[EmailId] [varchar](100) NULL,
	[ProfileImage] [nvarchar](max) NULL,
	[PriorityStatus] [varchar](20) NULL,
	[AssignedTo] [varchar](100) NULL,
	[ServiceKey] [varchar](100) NULL,
	[LeadTypeKey] [varchar](100) NULL,
	[LeadSourceKey] [varchar](100) NULL,
	[Remarks] [varchar](300) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [varchar](100) NOT NULL,
	[IsSpam] [tinyint] NULL,
	[IsWon] [tinyint] NULL,
	[ModifiedOn] [datetime] NOT NULL,
	[ModifiedBy] [varchar](100) NOT NULL,
	[City] [varchar](100) NULL,
	[PinCode] [varchar](15) NULL,
	[PurchaseOrderKey] [uniqueidentifier] NULL,
	[StatusId] [int] NULL,
 CONSTRAINT [PK__Leads__3214EC07DF7B12F4] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Leads_shadow]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Leads_shadow](
	[AuditId] [bigint] IDENTITY(1,1) NOT NULL,
	[Id] [bigint] NOT NULL,
	[FullName] [varchar](150) NULL,
	[MobileNumber] [varchar](100) NULL,
	[EmailId] [varchar](100) NULL,
	[ServiceKey] [varchar](100) NULL,
	[LeadTypeKey] [varchar](100) NULL,
	[LeadSourceKey] [varchar](100) NULL,
	[Remarks] [varchar](300) NULL,
	[IsStudent] [tinyint] NULL,
	[IsCustomer] [tinyint] NULL,
	[IsSpam] [tinyint] NULL,
	[IsWon] [tinyint] NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
	[AssignedTo] [varchar](100) NULL,
	[AuditAction] [char](1) NOT NULL,
	[AuditDate] [datetime] NOT NULL,
	[AuditUser] [varchar](50) NOT NULL,
	[AuditApp] [varchar](128) NULL,
 CONSTRAINT [PK_Leads_shadow] PRIMARY KEY CLUSTERED 
(
	[AuditId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LeadSources]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LeadSources](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL,
	[Description] [varchar](300) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LeadTypes]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LeadTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL,
	[Description] [varchar](300) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Menus]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Menus](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL,
	[Description] [varchar](250) NULL,
	[Url] [varchar](250) NULL,
	[ParentId] [int] NULL,
	[SortOrder] [int] NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
	[IsLHS] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MobileUsers]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MobileUsers](
	[MobileUserId] [bigint] IDENTITY(1,1) NOT NULL,
	[LeadKey] [uniqueidentifier] NOT NULL,
	[OneTimePassword] [varchar](6) NULL,
	[IsOtpVerified] [bit] NOT NULL,
	[MobileToken] [varchar](200) NOT NULL,
	[DeviceType] [varchar](10) NOT NULL,
	[IMEI] [varchar](100) NULL,
	[StockNature] [varchar](50) NULL,
	[AgreeToTerms] [bit] NULL,
	[SameForWhatsApp] [bit] NULL,
	[IsActive] [bit] NULL,
	[IsDelete] [bit] NULL,
	[CreatedOn] [datetime] NULL,
	[ModifiedOn] [datetime] NULL,
	[ProfileImage] [nvarchar](max) NULL,
	[About] [varchar](200) NULL,
 CONSTRAINT [PK_MobileUsers] PRIMARY KEY CLUSTERED 
(
	[MobileUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PartnerAccounts]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PartnerAccounts](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[FullName] [varchar](150) NULL,
	[MobileNumber] [varchar](100) NULL,
	[EmailId] [varchar](100) NULL,
	[City] [varchar](100) NULL,
	[Source] [varchar](50) NULL,
	[AssignedTo] [int] NULL,
	[TelegramId] [varchar](500) NULL,
	[ZerodhaCId] [varchar](100) NULL,
	[AngelCId] [varchar](100) NULL,
	[AliceBlueCId] [varchar](100) NULL,
	[EdelweissCId] [varchar](100) NULL,
	[FyersCId] [varchar](100) NULL,
	[MotilalCId] [varchar](100) NULL,
	[Brokerage] [float] NULL,
	[LeadKey] [varchar](100) NULL,
	[CustomerKey] [varchar](100) NULL,
	[Remarks] [varchar](300) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedIpAddress] [varchar](100) NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
	[Status] [int] NULL,
 CONSTRAINT [PK__PartnerA__3214EC0705BC839F] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PartnerAccountActivities]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PartnerAccountActivities](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [PartnerAccountDetailId] [int] NOT NULL,
        [Comments] [nvarchar](max) NOT NULL,
        [CreatedOn] [datetime] NULL,
        [CreatedBy] [int] NULL,
CONSTRAINT [PK_PartnerAccountActivities] PRIMARY KEY CLUSTERED
(
        [Id] ASC
 ),
 CONSTRAINT [FK_PartnerAccountActivities_PartnerAccountDetails] FOREIGN KEY ([PartnerAccountDetailId]) REFERENCES [dbo].[PartnerAccountDetails] ([Id])
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PartnerAccountActivities]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PartnerAccountActivities](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [PartnerAccountDetailId] [int] NOT NULL,
        [Comments] [nvarchar](max) NOT NULL,
        [CreatedOn] [datetime] NULL,
        [CreatedBy] [int] NULL,
 CONSTRAINT [PK_PartnerAccountActivities] PRIMARY KEY CLUSTERED
(
        [Id] ASC
 ),
 CONSTRAINT [FK_PartnerAccountActivities_PartnerAccountDetails] FOREIGN KEY ([PartnerAccountDetailId]) REFERENCES [dbo].[PartnerAccountDetails] ([Id])
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentModes]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentModes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL,
	[Description] [varchar](300) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PerformanceResults]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PerformanceResults](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL,
	[Description] [varchar](300) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[PublicKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Pincodes]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Pincodes](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Pincode] [varchar](50) NULL,
	[Area] [varchar](150) NULL,
	[Division] [varchar](150) NULL,
	[District] [varchar](150) NULL,
	[State] [varchar](150) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PurchaseOrders]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PurchaseOrders](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[LeadId] [bigint] NULL,
	[ClientName] [varchar](50) NULL,
	[Mobile] [varchar](50) NULL,
	[Email] [varchar](50) NULL,
	[DOB] [varchar](50) NULL,
	[Remark] [varchar](200) NULL,
	[PaymentDate] [datetime] NULL,
	[ModeOfPayment] [int] NULL,
	[BankName] [varchar](100) NULL,
	[Pan] [varchar](20) NULL,
	[State] [varchar](20) NULL,
	[City] [varchar](50) NULL,
	[TransactionRecipt] [varchar](500) NULL,
	[TransasctionReference] [varchar](100) NULL,
	[ServiceId] [int] NULL,
	[Service] [varchar](50) NULL,
	[NetAmount] [decimal](10, 2) NULL,
	[PaidAmount] [decimal](10, 2) NULL,
	[PaymentStatusId] [int] NULL,
	[PaymentActionDate] [datetime] NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[LTCDate] [datetime] NULL,
	[IsExpired] [bit] NULL,
	[Status] [int] NULL,
	[ActionBy] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [uniqueidentifier] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK__Purchase__3214EC0739F97C3D] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RoleMenus]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoleMenus](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleKey] [varchar](100) NULL,
	[MenuKey] [varchar](100) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Roles]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Roles](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL,
	[Description] [varchar](300) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Segments]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Segments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL,
	[Description] [varchar](300) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ServiceCategories]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ServiceCategories](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL,
	[Description] [varchar](300) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Services]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Services](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](250) NULL,
	[Description] [varchar](max) NULL,
	[ServiceCost] [decimal](12, 2) NULL,
	[ServiceTypeKey] [varchar](100) NULL,
	[ServiceCategoryKey] [varchar](100) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ServiceTypes]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ServiceTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL,
	[Description] [varchar](300) NULL,
	[IsRecurring] [tinyint] NULL,
	[RecurringTime] [int] NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Status]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Status](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Category] [varchar](50) NULL,
	[Description] [varchar](200) NULL,
	[Code] [varchar](5) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Stocks]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Stocks](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL,
	[Description] [varchar](300) NULL,
	[LotSize] [int] NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[PublicKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Strategies]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Strategies](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL,
	[Description] [varchar](300) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[PublicKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Tags]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Tags](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL,
	[Description] [varchar](300) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [varchar](100) NULL,
	[LastName] [varchar](100) NULL,
	[MobileNumber] [varchar](100) NULL,
	[PasswordHash] [varbinary](500) NULL,
	[PasswordSalt] [varbinary](300) NULL,
	[EmailId] [varchar](100) NULL,
	[DOJ] [varchar](100) NULL,
	[Address] [varchar](300) NULL,
	[RoleKey] [varchar](100) NULL,
	[Gender] [varchar](100) NULL,
	[UserImage] [varchar](300) NULL,
	[IsDisabled] [tinyint] NULL,
	[IsDelete] [tinyint] NULL,
	[PublicKey] [uniqueidentifier] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [varchar](100) NULL,
	[ModifiedOn] [datetime] NULL,
	[ModifiedBy] [varchar](100) NULL,
	[Password] [varchar](100) NULL,
	[PasswordKey] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserTypes]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserTypes](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[xl_Partners]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[xl_Partners](
	[Client Id] [float] NULL,
	[Branch Code New] [nvarchar](255) NULL,
	[Remeshire Group] [nvarchar](255) NULL,
	[Day of UCC Response Date] [nvarchar](255) NULL,
	[Day of First Trade Date] [nvarchar](255) NULL,
	[Day of Last trade date] [nvarchar](255) NULL,
	[Day of Derivative UCC Response Date] [nvarchar](255) NULL,
	[Day of Derivative first trade date] [nvarchar](255) NULL,
	[Day of Derivative Last trade date] [nvarchar](255) NULL,
	[Margin] [nvarchar](255) NULL,
	[dd] [float] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[xl_StockList]    Script Date: 14-05-2023 20:27:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[xl_StockList](
	[Description] [nvarchar](255) NULL,
	[Name] [nvarchar](255) NULL,
	[LotSize] [float] NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CallPerformance] ADD  CONSTRAINT [DF_CallPerformance_PublicKey]  DEFAULT (newid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[CommunicationModes] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[CommunicationModes] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[CommunicationModes] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[CommunicationModes] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[CommunicationModes] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[CommunicationTemplates] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[CommunicationTemplates] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[CommunicationTemplates] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[CommunicationTemplates] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[CommunicationTemplates] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Customers] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[Customers] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[Customers] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Customers] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[CustomerServices] ADD  DEFAULT ((0)) FOR [ActualCost]
GO
ALTER TABLE [dbo].[CustomerServices] ADD  DEFAULT ((0)) FOR [AmountPaid]
GO
ALTER TABLE [dbo].[CustomerServices] ADD  DEFAULT ((0)) FOR [AmountOutstanding]
GO
ALTER TABLE [dbo].[CustomerServices] ADD  DEFAULT ((0)) FOR [Discount]
GO
ALTER TABLE [dbo].[CustomerServices] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[CustomerServices] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[CustomerServices] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[CustomerServices] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[CustomerServices] ADD  DEFAULT ((1)) FOR [CreatedBy]
GO
ALTER TABLE [dbo].[CustomerServices] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[CustomerTags] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[CustomerTags] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[CustomerTags] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[CustomerTags] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[CustomerTypes] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[CustomerTypes] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[CustomerTypes] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[CustomerTypes] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[CustomerTypes] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Enquiries] ADD  DEFAULT ((1)) FOR [IsLead]
GO
ALTER TABLE [dbo].[Enquiries] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[Enquiries] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[Enquiries] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[Enquiries] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Enquiries] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Enquiries] ADD  DEFAULT ((1)) FOR [IsAdmin]
GO
ALTER TABLE [dbo].[Expirys] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[Expirys] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[Expirys] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[Expirys] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Expirys] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Leads] ADD  CONSTRAINT [DF__Leads__PublicKey__07C12930]  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[Leads] ADD  CONSTRAINT [DF__Leads__AssignedT__0A9D95DB]  DEFAULT ('BB74D26F-AA28-EB11-BEE5-00155D53687A') FOR [AssignedTo]
GO
ALTER TABLE [dbo].[Leads] ADD  CONSTRAINT [DF__Leads__LeadTypeK__00200768]  DEFAULT ('4C98BD0B-6F08-EB11-BFA7-00155D53687A') FOR [LeadTypeKey]
GO
ALTER TABLE [dbo].[Leads] ADD  CONSTRAINT [DF__Leads__LeadSourc__01142BA1]  DEFAULT ('EB82ED26-6708-EB11-BFA7-00155D53687A') FOR [LeadSourceKey]
GO
ALTER TABLE [dbo].[Leads] ADD  CONSTRAINT [DF__Leads__IsDisable__05D8E0BE]  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[Leads] ADD  CONSTRAINT [DF__Leads__IsDelete__06CD04F7]  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[Leads] ADD  CONSTRAINT [DF__Leads__CreatedOn__08B54D69]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Leads] ADD  CONSTRAINT [DF__Leads__IsSpam__03F0984C]  DEFAULT ((0)) FOR [IsSpam]
GO
ALTER TABLE [dbo].[Leads] ADD  CONSTRAINT [DF__Leads__IsWon__04E4BC85]  DEFAULT ((0)) FOR [IsWon]
GO
ALTER TABLE [dbo].[Leads] ADD  CONSTRAINT [DF__Leads__ModifiedO__09A971A2]  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Leads_shadow] ADD  CONSTRAINT [DF_Leads_shadow_AuditDate]  DEFAULT (getdate()) FOR [AuditDate]
GO
ALTER TABLE [dbo].[Leads_shadow] ADD  CONSTRAINT [DF_Leads_shadow_AuditUser]  DEFAULT (suser_sname()) FOR [AuditUser]
GO
ALTER TABLE [dbo].[Leads_shadow] ADD  CONSTRAINT [DF_Leads_shadow_AuditApp]  DEFAULT (('App=('+rtrim(isnull(app_name(),'')))+') ') FOR [AuditApp]
GO
ALTER TABLE [dbo].[LeadSources] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[LeadSources] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[LeadSources] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[LeadSources] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[LeadSources] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[LeadTypes] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[LeadTypes] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[LeadTypes] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[LeadTypes] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[LeadTypes] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Menus] ADD  DEFAULT ((0)) FOR [ParentId]
GO
ALTER TABLE [dbo].[Menus] ADD  DEFAULT ((100)) FOR [SortOrder]
GO
ALTER TABLE [dbo].[Menus] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[Menus] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[Menus] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[Menus] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Menus] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[PartnerAccounts] ADD  CONSTRAINT [DF__PartnerAc__IsDis__1CBC4616]  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[PartnerAccounts] ADD  CONSTRAINT [DF__PartnerAc__IsDel__1DB06A4F]  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[PartnerAccounts] ADD  CONSTRAINT [DF__PartnerAc__Publi__1EA48E88]  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[PartnerAccounts] ADD  CONSTRAINT [DF__PartnerAc__Creat__1F98B2C1]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[PartnerAccounts] ADD  CONSTRAINT [DF__PartnerAc__Modif__208CD6FA]  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[PaymentModes] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[PaymentModes] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[PaymentModes] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[PaymentModes] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[PaymentModes] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[PerformanceResults] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[PerformanceResults] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[PerformanceResults] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[PerformanceResults] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[PerformanceResults] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Pincodes] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[Pincodes] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[Pincodes] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[Pincodes] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Pincodes] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[PurchaseOrders] ADD  CONSTRAINT [DF_PurchaseOrders_PublicKey]  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[RoleMenus] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[RoleMenus] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[RoleMenus] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[RoleMenus] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[RoleMenus] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Roles] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[Roles] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[Roles] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[Roles] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Roles] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Segments] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[Segments] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[Segments] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[Segments] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Segments] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[ServiceCategories] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[ServiceCategories] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[ServiceCategories] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[ServiceCategories] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[ServiceCategories] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Services] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[Services] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[Services] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[Services] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Services] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[ServiceTypes] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[ServiceTypes] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[ServiceTypes] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[ServiceTypes] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[ServiceTypes] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Stocks] ADD  DEFAULT ((1)) FOR [LotSize]
GO
ALTER TABLE [dbo].[Stocks] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[Stocks] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[Stocks] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[Stocks] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Stocks] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Strategies] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[Strategies] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[Strategies] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[Strategies] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Strategies] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Tags] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[Tags] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[Tags] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[Tags] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Tags] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((0)) FOR [IsDisabled]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((0)) FOR [IsDelete]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (newsequentialid()) FOR [PublicKey]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (NULL) FOR [ModifiedOn]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (NULL) FOR [Password]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (NULL) FOR [PasswordKey]
GO
