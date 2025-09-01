CREATE TABLE [dbo].[MobileUsers] (
    [MobileUserId]    BIGINT           IDENTITY (1, 1) NOT NULL,
    [LeadKey]         UNIQUEIDENTIFIER NOT NULL,
    [OneTimePassword] VARCHAR (6)      NULL,
    [IsOtpVerified]   BIT              NOT NULL,
    [MobileToken]     VARCHAR (200)    NOT NULL,
    [DeviceType]      VARCHAR (10)     NOT NULL,
    [IMEI]            VARCHAR (100)    NULL,
    [StockNature]     VARCHAR (50)     NULL,
    [AgreeToTerms]    BIT              NULL,
    [SameForWhatsApp] BIT              NULL,
    [IsActive]        BIT              NULL,
    [IsDelete]        BIT              NULL,
    [CreatedOn]       DATETIME         NULL,
    [ModifiedOn]      DATETIME         NULL,
    [ProfileImage]    NVARCHAR (MAX)   NULL,
    [About]           VARCHAR (200)    NULL,
    CONSTRAINT [PK_MobileUsers] PRIMARY KEY CLUSTERED ([MobileUserId] ASC)
);

