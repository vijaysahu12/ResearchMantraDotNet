CREATE TABLE UserRefreshTokenM (
    Id INT IDENTITY(1,1) PRIMARY KEY, -- Auto-incrementing primary key
    MobileUserKey UNIQUEIDENTIFIER NOT NULL, -- Foreign key referencing the Users table
    RefreshToken NVARCHAR(256) NOT NULL, -- Refresh token
    DeviceType NVARCHAR(256) NOT NULL, -- Device identifier
    IssuedAt DATETIME NOT NULL, -- Timestamp when the token was issued
    ExpiresAt DATETIME NOT NULL, -- Timestamp when the token expires
    IsRevoked BIT DEFAULT 0, -- Indicates whether the token has been revoked (0 = False, 1 = True)
	ModifiedOn DATETIME NULL
);


CREATE TABLE CodelineContactInfo (
    Id INT IDENTITY(1,1) PRIMARY KEY, -- Primary key with auto-increment
    FullName NVARCHAR(255)  NULL, -- Full Name column
    ContactNumber NVARCHAR(15)  NULL, -- Contact Number column
    Email NVARCHAR(255)  NULL, -- Email column
    Reason NVARCHAR(MAX) NULL, -- Reason column, allowing NULL values
    CreatedOn DATETIME DEFAULT GETDATE() -- Optional: Automatically set the creation timestamp
);


CREATE TABLE ScannerPerformanceM (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Ltp DECIMAL(10,2)  NULL,
	Message NVARCHAR(400) NULL,
    NetChange DECIMAL(10,2)  NULL,
    PercentChange DECIMAL(5,2)  NULL,
    TradingSymbol NVARCHAR(50)  NULL,
    ViewChart NVARCHAR(MAX) NULL,
    Topic varchar(100) NULL,
    CreatedOn DATETIME DEFAULT GETDATE(),
	SentAt DateTime default getDate() null
);


GO

ALTER TABLE [dbo].[LearningContentM]
ADD [ModifiedOn] DATETIME NULL;
GO


-- Add CouponCode to PaymentRequestStatusM
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PaymentRequestStatusM' AND COLUMN_NAME = 'CouponCode'
)
BEGIN
    ALTER TABLE PaymentRequestStatusM
    ADD CouponCode VARCHAR(50) NULL;
END
GO

-- Add SubscriptionMappingId to PurchaseOrdersM
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PurchaseOrdersM' AND COLUMN_NAME = 'SubscriptionMappingId'
)
BEGIN
    ALTER TABLE PurchaseOrdersM
    ADD SubscriptionMappingId INT;
END
GO

-- Add SubscriptionMappingId to PaymentRequestStatusM
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PaymentRequestStatusM' AND COLUMN_NAME = 'SubscriptionMappingId'
)
BEGIN
    ALTER TABLE [dbo].[PaymentRequestStatusM]
    ADD SubscriptionMappingId INT NULL;
END
GO

-- Add TransactionId to PurchaseOrdersM
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PurchaseOrdersM' AND COLUMN_NAME = 'TransactionId'
)
BEGIN
    ALTER TABLE [dbo].[PurchaseOrdersM]
    ADD [TransactionId] VARCHAR(100) NULL;
END
GO


-- Check and add the Def column if it does not exist
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'CouponsM' AND COLUMN_NAME = 'Def'
)
BEGIN
    ALTER TABLE [dbo].[CouponsM]
    ADD [Def] BIT DEFAULT ((0)) NOT NULL;
END;




-- Check and add the IsActive column if it does not exist
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'ProductsContentM' AND COLUMN_NAME = 'IsActive'
)
BEGIN
    ALTER TABLE [dbo].[ProductsContentM]
    ADD [IsActive] BIT DEFAULT ((1)) NOT NULL;
END;

-- Check and add the IsDeleted column if it does not exist
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'ProductsContentM' AND COLUMN_NAME = 'IsDeleted'
)
BEGIN
    ALTER TABLE [dbo].[ProductsContentM]
    ADD [IsDeleted] BIT DEFAULT ((0)) NOT NULL;
END;


------CRM NEW TABLE FOR COMPLAINTS -----
CREATE TABLE Complaints (
    Id INT IDENTITY(1,1) PRIMARY KEY,  -- Auto-incremented UserID
    FirstName VARCHAR(50) NOT NULL,         -- First Name
    LastName VARCHAR(50) NOT NULL,          -- Last Name
    Email VARCHAR(100) NOT NULL,            -- Email Address
    Mobile VARCHAR(15) NOT NULL,            -- Mobile Number
    Images NVARCHAR(MAX),                   -- JSON array of image filenames
    Message TEXT NOT NULL                   -- Message content
);


----END-----