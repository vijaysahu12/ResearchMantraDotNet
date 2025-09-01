CREATE TABLE [dbo].[CommunicationTemplates] (
    [Id]         INT              IDENTITY (1, 1) NOT NULL,
    [Name]       VARCHAR (100)    NULL,
    [ModeKey]    VARCHAR (100)    NULL,
    [Template]   NVARCHAR (MAX)   NULL,
    [IsDisabled] TINYINT          DEFAULT ((0)) NULL,
    [IsDelete]   TINYINT          DEFAULT ((0)) NULL,
    [PublicKey]  UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NULL,
    [CreatedOn]  DATETIME         DEFAULT (getdate()) NULL,
    [CreatedBy]  VARCHAR (100)    NULL,
    [ModifiedOn] DATETIME         DEFAULT (NULL) NULL,
    [ModifiedBy] VARCHAR (100)    NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

