USE [KingResearch]
GO
/****** Object:  Table [dbo].[FollowUpReminders]    Script Date: 15-06-2023 19:43:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FollowUpReminders](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LeadKey] [uniqueidentifier] NOT NULL,
	[Comments] [varchar](500) NULL,
	[NotifyDate] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_FollowUpReminder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
