CREATE TABLE [dbo].[ExceptionLogs] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [ExceptionType] NVARCHAR (MAX) NULL,
    [ErrorMessage]  NVARCHAR (MAX) NULL,
    [StackTrace]    NVARCHAR (MAX) NULL,
    [Description]   NVARCHAR (MAX) NULL,
    [Notes]         NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_ExceptionLogs] PRIMARY KEY CLUSTERED ([Id] ASC)
);

