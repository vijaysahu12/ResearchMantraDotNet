CREATE TABLE [dbo].[PartnerAccountActivities] (
    [Id]        INT            IDENTITY (1,1) NOT NULL,
    [PartnerAccountDetailId] INT NOT NULL,
    [Comments]  NVARCHAR (MAX) NOT NULL,
    [CreatedOn] DATETIME NULL,
    [CreatedBy] INT NULL,
    CONSTRAINT [PK_PartnerAccountActivities] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_PartnerAccountActivities_PartnerAccountDetails] FOREIGN KEY ([PartnerAccountDetailId])
        REFERENCES [dbo].[PartnerAccountDetails] ([Id])
);
