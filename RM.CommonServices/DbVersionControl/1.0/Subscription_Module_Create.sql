
GO 
CREATE TABLE SubscriptionDurationM (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Months INT NOT NULL,
    IsActive BIT null,
    CreatedOn DATETIME DEFAULT GETDATE(),
    ModifiedOn DATETIME DEFAULT GETDATE()
);
--- To Get the different plans like Lite, Pro , Alpha , Super 
CREATE TABLE SubscriptionPlanM (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    IsActive BIT null,
    CreatedOn DATETIME DEFAULT GETDATE(),
    ModifiedOn DATETIME DEFAULT GETDATE()
);


CREATE TABLE SubscriptionMappingM  (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SubscriptionDurationId INT NOT NULL,
    DiscountPercentage DECIMAL(18, 2) NOT NULL,
    ProductId INT NOT NULL,
    SubscriptionPlanId INT NOT NULL,
    IsActive BIT NULL,
    CreatedOn DATETIME DEFAULT GETDATE(),
    ModifiedOn DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_SubscriptionPlan_SubscriptionDurationM FOREIGN KEY (SubscriptionDurationId) REFERENCES SubscriptionDurationM(Id),
    CONSTRAINT FK_SubscriptionPlan_Plan FOREIGN KEY (SubscriptionPlanId) REFERENCES SubscriptionPlanM(Id)
);
 
-- INSERT INTO SubscriptionDuration (Name, Months, IsActive, CreatedOn, ModifiedOn)
-- VALUES 
--     ('Yearly', 12, 1, GETDATE(), GETDATE()),
--     ('Quarterly', 3, 1, GETDATE(), GETDATE()),
--     ('Monthly', 1, 1, GETDATE(), GETDATE());

 
GO --exec GetSubscriptionPlanWithProduct NULL, 1 , '2DC32A9F-2F35-EE11-811E-00155D23D79C' , 'IOS'  
CREATE  PROCEDURE GetSubscriptionPlanWithProduct         
    @ProductId INT = 1,                     
    @SubscriptionPlanId int =25    ,              
    @MobileUserKey  uniqueidentifier = null,      
    @DeviceType  VARCHAR(15) = 'android'      
AS         
BEGIN         
      
SELECT         
    sp.Id AS SubscriptionPlanId,        
    sp.Name AS PlanName,        
    sp.Description AS PlanDescription  ,      
    sm.ProductId,        
    pm.Name as ProductName,        
    pm.Price as ActualPrice,        
    (((pm.Price * sm.DiscountPercentage)/ 100))  as DiscountPrice,        
    (pm.Price - ((pm.Price * sm.DiscountPercentage)/ 100))  as NetPayment,        
    'DIWALI2024' as CouponCode,        
    DATEADD(MONTH, sd.Months, GETDATE()) as ExpireOn,        
    -- sm.IsActive AS SubscriptionMappingActive,        
 sd.Id AS SubscriptionDurationId,        
      CAST(sd.Months as varchar) + iif(CAST(sd.Months as int) = 1,', Month',  ', Months') AS SubscriptionDurationName,        
    sd.Months,        
 CAST(0 as bit) IsRecommended,      
 CAST(FORMAT(((pm.Price - ((pm.Price * sm.DiscountPercentage)/ 100))/sd.Months), 'N2') AS VARCHAR) + '/m' as PerMonth,      
 sm.Id AS SubscriptionMappingId      
FROM         
    SubscriptionMappingM sm        
INNER JOIN         
    SubscriptionDurationM sd ON sm.SubscriptionDurationId = sd.Id        
INNER JOIN         
    SubscriptionPlanM sp ON sm.SubscriptionPlanId = sp.Id        
INNER JOIN         
    ProductsM as pm on pm.Id = sm.ProductId        
WHERE         
    ISNULL(sp.IsActive,1) = 1         
    AND ISNULL(sd.IsActive,1) = 1         
    AND ISNULL(sm.IsActive,1) = 1        
    AND (sm.SubscriptionPlanId = @SubscriptionPlanId OR @SubscriptionPlanId IS NULL)         
    AND (sm.ProductId = @ProductId OR @ProductId IS NULL)        
 AND LOWER(@DeviceType)= 'android'      
END      
      
   
    
 
 
GO 

------------------------------------------------------------------------------------------------Data insertion for the above table--------------------------------------------------------------------------------
 GO
SET IDENTITY_INSERT [dbo].[SubscriptionPlanM] ON 
GO
INSERT [dbo].[SubscriptionPlanm] ([Id], [Name], [Description], [IsActive], [CreatedOn], [ModifiedOn]) VALUES (1, N'Basic', N'Basic Plan', 1, CAST(N'2024-11-20T13:28:38.163' AS DateTime), CAST(N'2024-11-20T13:28:38.163' AS DateTime))
GO
SET IDENTITY_INSERT [dbo].[SubscriptionPlanM] OFF
GO
SET IDENTITY_INSERT [dbo].[SubscriptionDurationM] ON 
GO
INSERT [dbo].[SubscriptionDurationM] ([Id], [Name], [Months], [IsActive], [CreatedOn], [ModifiedOn]) VALUES (1, N'Yearly', 12, 1, CAST(N'2024-11-20T12:41:14.463' AS DateTime), CAST(N'2024-11-20T12:41:14.463' AS DateTime))
GO
INSERT [dbo].[SubscriptionDurationM] ([Id], [Name], [Months], [IsActive], [CreatedOn], [ModifiedOn]) VALUES (2, N'Quarterly', 3, 1, CAST(N'2024-11-20T12:41:14.463' AS DateTime), CAST(N'2024-11-20T12:41:14.463' AS DateTime))
GO
INSERT [dbo].[SubscriptionDurationM] ([Id], [Name], [Months], [IsActive], [CreatedOn], [ModifiedOn]) VALUES (3, N'Free', 1, 1, CAST(N'2024-11-20T13:35:40.763' AS DateTime), CAST(N'2024-11-20T13:35:40.763' AS DateTime))
GO



--Create subscription plans 


 
GO
DECLARE @Code NVARCHAR(50), @DurationId INT, @DurationId2 INT,@DurationId3 INT, @DiscountPrice DECIMAL(10, 2) = 100, @ProductId INT;
DECLARE @SQL NVARCHAR(MAX);

SELECT @DurationId = Id FROM SubscriptionDurationM WHERE Name = 'Free';
SELECT @DurationId2 = Id FROM SubscriptionDurationM WHERE Name = 'Quarterly';
SELECT @DurationId3 = Id FROM SubscriptionDurationM WHERE Name = 'Yearly';


DECLARE ProductCursor CURSOR FOR
SELECT Code FROM ProductsM 
--WHERE Code IN ('BREAKOUT', 'ST', 'KALKIBAATAAJ', 'MORNINGSHORT', 'BREAKFAST''SWING', 'HARMONIC', 'MULTIBAGGER', 'TOPBOTTOM', 'RESEARCH');

OPEN ProductCursor;

-- Fetch the first Code
FETCH NEXT FROM ProductCursor INTO @Code;

-- Loop through each Code
WHILE @@FETCH_STATUS = 0
BEGIN
    -- Get the ProductId for the current Code
    SELECT @ProductId = Id FROM ProductsM WHERE Code = @Code;

	IF NOT EXISTS(
		SELECT 1 
        FROM SubscriptionMappingM
        WHERE ProductId = @ProductId AND SubscriptionDurationId = @DurationId AND SubscriptionPlanId = 1
    )
	BEGIN
		SET @SQL = '
		INSERT INTO SubscriptionMappingM
			(SubscriptionDurationId, DiscountPercentage, ProductId, SubscriptionPlanId, IsActive, CreatedOn, ModifiedOn)
		SELECT ' + CAST(@DurationId AS NVARCHAR) + ', ' + CAST(@DiscountPrice AS NVARCHAR) + ', ' + CAST(@ProductId AS NVARCHAR) + 
		', 1, 1, GETDATE(), GETDATE();';
		EXEC sp_executesql @SQL;
	END


	IF NOT EXISTS(
		SELECT 1 
        FROM SubscriptionMappingM
        WHERE ProductId = @ProductId AND SubscriptionDurationId = @DurationId2 AND SubscriptionPlanId = 1
    )
	BEGIN
		SET @SQL = '
		INSERT INTO SubscriptionMappingM
			(SubscriptionDurationId, DiscountPercentage, ProductId, SubscriptionPlanId, IsActive, CreatedOn, ModifiedOn)
		SELECT ' + CAST(@DurationId2 AS NVARCHAR) + ', ' + CAST(@DiscountPrice AS NVARCHAR) + ', ' + CAST(@ProductId AS NVARCHAR) + 
		', 1, 1, GETDATE(), GETDATE();';
		EXEC sp_executesql @SQL;
	END

	IF NOT EXISTS(
		SELECT 1 
        FROM SubscriptionMappingM
        WHERE ProductId = @ProductId AND SubscriptionDurationId = @DurationId3 AND SubscriptionPlanId = 1
    )
	BEGIN
		SET @SQL = '
		INSERT INTO SubscriptionMappingM
			(SubscriptionDurationId, DiscountPercentage, ProductId, SubscriptionPlanId, IsActive, CreatedOn, ModifiedOn)
		SELECT ' + CAST(@DurationId3 AS NVARCHAR) + ', ' + CAST(@DiscountPrice AS NVARCHAR) + ', ' + CAST(@ProductId AS NVARCHAR) + 
		', 1, 1, GETDATE(), GETDATE();';
		EXEC sp_executesql @SQL;
	END

    -- Fetch the next Code
    FETCH NEXT FROM ProductCursor INTO @Code;
END

-- Close and deallocate the cursor
CLOSE ProductCursor;
DEALLOCATE ProductCursor;
GO