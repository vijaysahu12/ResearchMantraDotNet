CREATE TABLE [dbo].[PushNotifications](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserKey] [uniqueidentifier] NOT NULL,
	[Message] [varchar](500) NOT NULL,
	[ReadDate] [datetime] NULL,
	[IsRead] [bit] NULL,
	[IsActive] [bit] NULL,
	[IsImportant] [bit] NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[ModifiedDate] [datetime] NULL,
	[ModifiedBy] [uniqueidentifier] NULL,
	[Source] [varchar](100) NULL,
	[Destination] [varchar](50) NULL,
 CONSTRAINT [PK_PushNotification] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


