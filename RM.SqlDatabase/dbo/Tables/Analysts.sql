CREATE TABLE [dbo].[Analysts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserKey] [uniqueidentifier] NOT NULL,
	[UserType] [varchar](50) NOT NULL,
	[IsActive] [bit] NULL,
	[IsDelete] [bit] NULL,
 CONSTRAINT [PK_Analysts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO


