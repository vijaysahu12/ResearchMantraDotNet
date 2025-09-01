/****** Object:  Table [dbo].[PromotionM]    Script Date: 7/1/2025 6:04:29 PM ******/
GO

CREATE TABLE [dbo].[PromotionM](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](200) NULL,
	[Subtitle] [nvarchar](200) NULL,
	[Description] [nvarchar](max) NULL,
	[MediaUrl] [nvarchar](max) NULL,
	[MediaType] [nvarchar](50) NULL,
	[BadgeText] [nvarchar](100) NULL,
	[ValidityText] [datetime] NULL,
	[ButtonText] [nvarchar](MAX) NULL,
	[SecondaryButtonText] [nvarchar](100) NULL,
	[ActionUrl] [nvarchar](500) NULL,
	[DownloadUrl] [nvarchar](500) NULL,
	[ShowDownloadButton] [bit] NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[IsActive] [bit] NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[IsDelete] [bit] NULL,
	[ModifiedBy] [uniqueidentifier] NULL,
	[ModifiedOn] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[PromotionM] ADD  DEFAULT (getdate()) FOR [CreatedOn]
GO

ALTER TABLE [dbo].[PromotionM] ADD  DEFAULT ((0)) FOR [IsDelete]
GO

IF COL_LENGTH('PromotionM', 'MaxDisplayCount') IS NULL
BEGIN
    ALTER TABLE PromotionM ADD MaxDisplayCount INT NULL
END

IF COL_LENGTH('PromotionM', 'DisplayFrequency') IS NULL
BEGIN
    ALTER TABLE PromotionM ADD DisplayFrequency INT NULL
END

IF COL_LENGTH('PromotionM', 'LastShownAt') IS NULL
BEGIN
    ALTER TABLE PromotionM ADD LastShownAt DATETIME NULL
END

IF COL_LENGTH('PromotionM', 'GlobalButtonAction') IS NULL
BEGIN
    ALTER TABLE PromotionM ADD GlobalButtonAction BIT NULL
END

IF COL_LENGTH('PromotionM', 'Target') IS NULL
BEGIN
    ALTER TABLE PromotionM ADD Target NVARCHAR(100) NULL
END

IF COL_LENGTH('PromotionM', 'ProductName') IS NULL
BEGIN
    ALTER TABLE PromotionM ADD ProductName NVARCHAR(200) NULL
END

IF COL_LENGTH('PromotionM', 'ProductId') IS NULL
BEGIN
    ALTER TABLE PromotionM ADD ProductId INT NULL
END
IF COL_LENGTH('PromotionM', 'Title') IS NOT NULL
BEGIN
    ALTER TABLE PromotionM DROP COLUMN Title
END

IF COL_LENGTH('PromotionM', 'Subtitle') IS NOT NULL
BEGIN
    ALTER TABLE PromotionM DROP COLUMN Subtitle
END

IF COL_LENGTH('PromotionM', 'Description') IS NOT NULL
BEGIN
    ALTER TABLE PromotionM DROP COLUMN Description
END

IF COL_LENGTH('PromotionM', 'BadgeText') IS NOT NULL
BEGIN
    ALTER TABLE PromotionM DROP COLUMN BadgeText
END

IF COL_LENGTH('PromotionM', 'ValidityText') IS NOT NULL
BEGIN
    ALTER TABLE PromotionM DROP COLUMN ValidityText
END
--Created By Siva 9 July 2025 1:57 PM ---
IF COL_LENGTH('PromotionM', 'ShouldDisplay') IS NULL
BEGIN
    ALTER TABLE PromotionM ADD ShouldDisplay BIT NULL
END


---- MODIFY BY SIVA 18 JULY 2015 5:06 PM -----------------
GO
ALTER PROCEDURE [dbo].[GetEmployeeWorkStatus]         
    @SuperVisorId INT = NULL,        
    @StartDate DATE = NULL,        
    @EndDate DATE = NULL        
AS        
BEGIN        
    DECLARE @isAdmin BIT = 0,        
            @NumberOfDaysUntouched INT = 15;        
        
    IF EXISTS (        
        SELECT 1        
        FROM Users        
        WHERE id = @SuperVisorId        
          AND rolekey = (SELECT publickey FROM Roles WHERE name = 'admin')        
    )        
    BEGIN        
        SET @isAdmin = 1;        
    END        
        
    DECLARE @FollowUpLeadType UNIQUEIDENTIFIER;        
        
    SET @FollowUpLeadType = (        
        SELECT TOP 1 PublicKey        
        FROM LeadTypes        
        WHERE Id = 2 -- Follow Up        
    );        
        
    SELECT 
        us.Id,        
        MAX(us.FirstName) + ' ' + MAX(us.LastName) AS EmpName,        
        COUNT(DISTINCT le.id) AS LeadCount,        
        SUM(CASE WHEN DATEDIFF(day, le.modifiedon, GETDATE()) > @NumberOfDaysUntouched THEN 1 ELSE 0 END) AS UntouchedLeads,        
        SUM(CASE WHEN le.LeadTypeKey = @FollowUpLeadType THEN 1 ELSE 0 END) AS FollowUpLeads,
        ISNULL(poData.TotalPurchaseOrders, 0) AS TotalPurchaseOrders,        
        ISNULL(poData.TotalPRPayment, 0.0) AS TotalPRPayment,        
        SUM(CASE WHEN po.STATUS = 10 THEN ISNULL(po.PaidAmount, 0.0) ELSE 0.0 END) AS ApprovedPr,        
        SUM(CASE WHEN po.STATUS = 24 THEN ISNULL(po.PaidAmount, 0.0) ELSE 0.0 END) AS Ltc,        
        SUM(CASE WHEN po.STATUS = 11 THEN 1 ELSE 0 END) AS RejectedPrCount,        
        SUM(CASE WHEN po.STATUS = 11 THEN ISNULL(po.PaidAmount, 0.0) ELSE 0.0 END) AS RejectedPr,        
        SUM(CASE WHEN po.id IS NOT NULL AND po.STATUS = 3 THEN 1 ELSE 0 END) AS PendingPrCount,        
        SUM(CASE WHEN po.id IS NOT NULL AND po.STATUS = 3 THEN ISNULL(po.PaidAmount, 0.0) ELSE 0.0 END) AS PendingPr,        
        MAX(LE.ModifiedOn) AS LastLeadActivityDate        
     
    FROM Users AS us        
    LEFT JOIN Leads AS le ON us.PublicKey = le.AssignedTo        
    LEFT JOIN PurchaseOrders AS po ON le.Id = po.LeadId        
    -- OUTER APPLY for CreatedBy-based PurchaseOrder values
    OUTER APPLY (
        SELECT 
            COUNT(*) AS TotalPurchaseOrders,
            SUM(ISNULL(po2.PaidAmount, 0.0)) AS TotalPRPayment
        FROM PurchaseOrders po2
        WHERE po2.CreatedBy = us.PublicKey
          AND ISNULL(po2.PaymentDate, CAST(GETDATE() AS DATE)) 
              BETWEEN ISNULL(@StartDate, CAST(GETDATE() AS DATE)) AND ISNULL(@EndDate, CAST(GETDATE() AS DATE))
    ) AS poData

    WHERE ISNULL(us.IsDisabled, 0) = 0        
      AND ISNULL(us.IsDelete, 0) = 0        
      AND ISNULL(le.ModifiedOn, CAST(GETDATE() AS DATE)) 
            BETWEEN ISNULL(@StartDate, CAST(GETDATE() AS DATE)) AND ISNULL(@EndDate, CAST(GETDATE() AS DATE))        
      AND (      
            (@isAdmin = 1 AND us.roleKey IN (
                SELECT PublicKey FROM Roles WHERE name IN ('BDE', 'Sales Lead')
            ))      
            OR (@isAdmin = 0 AND us.SupervisorId = @SuperVisorId)      
            OR us.Id = @SuperVisorId      
        )      
    GROUP BY us.Id, poData.TotalPurchaseOrders, poData.TotalPRPayment
    ORDER BY MAX(ISNULL(le.ModifiedOn, le.CreatedOn)) DESC;
   END
---------------------------------Ajith 21 July 2025 12:47 PM -------------------

-- Add [Title] column if it doesn't exist
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'PromotionM'
      AND COLUMN_NAME = 'Title'
)
BEGIN
    ALTER TABLE PromotionM
    ADD [Title] VARCHAR(100) NULL;
END

-- Add [Description] column if it doesn't exist
IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'PromotionM'
      AND COLUMN_NAME = 'Description'
)
BEGIN
    ALTER TABLE PromotionM
    ADD [Description] VARCHAR(MAX) NULL;
END


------------Ajith 
GO
ALTER PROCEDURE GetBasketsM
    @MobileUserKey UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ResearchProductId INT;
    DECLARE @IsResearchActive BIT;
    DECLARE @UserHasValidPurchase BIT;

    -- Get RESEARCH product info
    SELECT 
        @ResearchProductId = Id,
        @IsResearchActive = CASE WHEN IsActive = 1 THEN 1 ELSE 0 END
    FROM ProductSM
    WHERE Code = 'RESEARCH';

    -- Default value
    SET @UserHasValidPurchase = 0;

    -- If RESEARCH product is inactive, check MyBucket for valid purchase
    IF @ResearchProductId IS NOT NULL AND @IsResearchActive = 0
    BEGIN
        SELECT @UserHasValidPurchase = CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END
        FROM MyBucketM
        WHERE ProductId = @ResearchProductId
          AND MobileUserKey = @MobileUserKey
          AND ISNULL(EndDate, GETDATE()) >= CAST(GETDATE() AS DATE);
        
        -- If not purchased or expired, return empty result set
        IF @UserHasValidPurchase = 0
        BEGIN
            SELECT 
                CAST(NULL AS INT) AS Id,
                CAST(NULL AS VARCHAR(200)) AS Title,
                CAST(NULL AS VARCHAR(MAX)) AS Description,
                CAST(NULL AS BIT) AS IsFree,
                CAST(NULL AS BIT) AS IsActive,
                CAST(NULL AS BIT) AS IsDelete,
                CAST(NULL AS INT) AS SortOrder,
                CAST(NULL AS INT) AS CompanyCount
            WHERE 1 = 0;
            RETURN;
        END
    END

    -- Return basket list with company count
    ;WITH CompanyCount AS      
    (
        SELECT BasketId, COUNT(BasketId) AS CompanyCount      
        FROM CompanyDetailM      
        WHERE IsPublished = 1 AND IsActive = 1     
        GROUP BY BasketId      
    )      
    SELECT    
        b.Id,    
        b.Title,    
        b.Description,    
        b.IsFree,    
        b.IsActive,    
        b.IsDelete,   
        b.SortOrder,    
        ISNULL(cc.CompanyCount, 0) AS CompanyCount   
    FROM BasketsM b      
    LEFT JOIN CompanyCount cc ON b.Id = cc.BasketId      
    WHERE b.IsActive = 1 AND b.IsDelete = 0 
END
---MODIFY BY SIVA 24 JULY 2025 6:12 PM ---
GO


ALTER PROCEDURE [dbo].[GetPartnerAccountsFlat]  
  --declare   
 @IsPaging BIT = 1,        
    @PageSize INT = 20,        
    @PageNumber INT = 1,        
    @FromDate DATE = NULL,        
    @ToDate DATE = NULL,        
    @SearchText VARCHAR(200) = NULL,        
    @SortExpression VARCHAR(100) = '',        
    @PartnerWith VARCHAR(100) = NULL,        
    @StatusType INT = NULL,        
    @AssignedTo INT = 0,        
    @product_count INT OUTPUT  
AS  
BEGIN  
    SET NOCOUNT ON;  
  
    -- 1. Calculate the total count of unique rows for paging  
    SELECT @product_count = COUNT(*)  
    FROM PartnerAccounts pa  
    INNER JOIN PartnerAccountDetails pad ON pa.Id = pad.PartnerAccountId  
    WHERE  
        ISNULL(pa.[Status], 0) = (CASE WHEN ISNULL(@StatusType, -1) = -1 THEN ISNULL(pa.[Status], 0) ELSE @StatusType END)  
        AND ISNULL(pa.[AssignedTo], 0) = IIF(@AssignedTo = 0, ISNULL(pa.[AssignedTo], 0), @AssignedTo)  
        AND (  
            pa.FullName LIKE '%' + ISNULL(@SearchText, pa.FullName) + '%'        
         OR pad.PartnerCId LIKE '%' + ISNULL(@SearchText, pad.PartnerCId) + '%'        
         OR pa.MobileNumber LIKE '%' + ISNULL(@SearchText, pa.MobileNumber) + '%'        
         OR pa.EmailId LIKE '%' + ISNULL(@SearchText, pa.EmailId) + '%'                   
        )  
        AND (  
            ISNULL(@PartnerWith, '') = ''  
         OR (LOWER(@PartnerWith) = LOWER(ISNULL(pad.PartnerCode, '')))  
        )  
        AND ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, pa.ModifiedOn)), ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999))  
            BETWEEN ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999)  
                AND ISNULL(@ToDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))  
        AND ISNULL(pa.isdelete, 0) = 0;  
  
    -- 2. Return paginated records  
    ;WITH CTE AS (  
        SELECT  
			ROW_NUMBER() OVER (ORDER BY pad.ModifiedOn DESC) AS SlNo,          
			pa.Id,          
			pa.PublicKey,   
			pa.FullName,          
			pa.MobileNumber,          
			pa.EmailId,          
			CASE                     
				WHEN pad.[StatusId] = 0 THEN 'Fresh'                     
				WHEN pad.[StatusId] = 1 THEN 'Pending'                     
				WHEN pad.[StatusId] = 2 THEN 'Accepted'                     
				WHEN pad.[StatusId] = 3 THEN 'Rejected'                     
				WHEN pad.[StatusId] = 4 THEN 'Followup'                     
				WHEN pad.[StatusId] = 5 THEN 'NotInterested'                     
				WHEN pad.[StatusId] = 6 THEN 'NPC'         
			 WHEN pad.[StatusId] = 7 THEN 'Linked to Jarvis'                         
  
				ELSE 'Fresh'                     
			END AS StatusType,          
			CAST(pa.[Status] AS VARCHAR) AS STATUS,          
			pa.Remarks,          
			'' AS Details,          
			pa.City,          
			pad.CreatedOn,     
			us.FirstName + us.LastName AS ModifiedBy,  
			'' AS CreatedBy,  
			pad.ModifiedOn,          
			pa.TelegramId,          
			pad.PartnerCode + ':' + pad.PartnerCId AS PartnerWith,          
			ISNULL(pa.Brokerage, 0.0) AS Brokerage,          
			ISNULL(pa.Source, 'kr') AS Source,          
			ISNULL(pa.AssignedTo, 0) AS AssignedTo,    
			us.FirstName + ' ' + us.LastName AS AssignedToName    
       
	   
	   FROM PartnerAccounts pa  
        INNER JOIN PartnerAccountDetails pad ON pa.Id = pad.PartnerAccountId  
        LEFT JOIN Users us ON us.Id = pa.AssignedTo  
        WHERE  
            ISNULL(pa.[Status], 0) = (CASE WHEN ISNULL(@StatusType, -1) = -1 THEN ISNULL(pa.[Status], 0) ELSE @StatusType END)  
            AND ISNULL(pa.[AssignedTo], 0) = IIF(@AssignedTo = 0, ISNULL(pa.[AssignedTo], 0), @AssignedTo)  
            AND (  
                pa.FullName LIKE '%' + ISNULL(@SearchText, pa.FullName) + '%'        
             OR pad.PartnerCId LIKE '%' + ISNULL(@SearchText, pad.PartnerCId) + '%'        
             OR pa.MobileNumber LIKE '%' + ISNULL(@SearchText, pa.MobileNumber) + '%'        
             OR pa.EmailId LIKE '%' + ISNULL(@SearchText, pa.EmailId) + '%'                   
            )  
            AND (  
                ISNULL(@PartnerWith, '') = ''  
             OR (LOWER(@PartnerWith) = LOWER(ISNULL(pad.PartnerCode, '')))  
            )  
            AND ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, pa.ModifiedOn)), ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999))  
                BETWEEN ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999)  
                    AND ISNULL(@ToDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))  
            AND ISNULL(pa.isdelete, 0) = 0  
    )  
    SELECT *  
    FROM CTE  
    WHERE  
        (@IsPaging = 0)  
        OR (SlNo > ((@PageNumber - 1) * @PageSize) AND SlNo <= (@PageNumber * @PageSize))  
    ORDER BY SlNo;  
END  

----------------------Ajith 28-07-2025 1:20PM
GO
-- exec GetMobileUserDetails 'DC5F1249-9719-EF11-B261-88B133B31C8F'    

ALTER PROCEDURE GetMobileUserDetails @MobileUserKey UNIQUEIDENTIFIER             
AS             
BEGIN             
 DECLARE @ActiveProductExists BIT = 0             
             
    -- Check for Specific user only
    IF @MobileUserKey IN (
        '594f9019-fc1f-ef11-b261-88b133b31c8f', --Vijay sir
        'fa8f798d-de1c-ef11-b261-88b133b31c8f', --Krithika personal
        '43a79d43-6335-f011-b3f7-a0b5f4cdec09', --Krithika crm
        '90b3d104-9b96-ef11-b30e-ff98edb1bde3', --Harry sir,
        '1194d99e-c419-ef11-b261-88b133b31c8f' --Sushmita mam
    )
    BEGIN
        SET @ActiveProductExists = 1;
    END        
             
 SELECT Fullname             
  ,ProfileImage             
  ,PublicKey             
  ,Gender,           
  --,CAST(case when CanCommunityPost = 0 then 0 else @ActiveProductExists end as bit) as HasActiveProduct             
  --,CAST(case when CanCommunityPost is null then @ActiveProductExists else CanCommunityPost end as bit) as HasActiveProduct   
  @ActiveProductExists as HasActiveProduct       
 FROM mobileusers             
 WHERE publickey = @MobileUserKey             
END 
             
-- CREATE PROCEDURE GetMobileUserDetails @MobileUserKey UNIQUEIDENTIFIER             
-- AS             
-- BEGIN             
--  DECLARE @ActiveProductExists BIT = 0             
             
--  IF EXISTS (             
--    SELECT 1             
--    FROM MYBucketM             
--    WHERE mobileuserkey = @MobileUserKey             
--     AND GETDATE() BETWEEN StartDate             
--      AND enddate          
--   AND ProductId != 11      
--    )             
--  BEGIN             
--   SET @ActiveProductExists = 1             
--  END             
--  ELSE             
--  BEGIN             
--   SET @ActiveProductExists = 0             
--  END             
             
--  SELECT Fullname             
--   ,ProfileImage             
--   ,PublicKey             
--   ,Gender           
--   --,CAST(case when CanCommunityPost = 0 then 0 else @ActiveProductExists end as bit) as HasActiveProduct             
--   ,CAST(case when CanCommunityPost is null then @ActiveProductExists else CanCommunityPost end as bit) as HasActiveProduct             
--  FROM mobileusers             
--  WHERE publickey = @MobileUserKey             
-- END 

---MODIFY BY SIVA 29 JULY 2025 6:10PM------------
IF COL_LENGTH('PromotionM', 'ScheduleDate') IS NULL
BEGIN
    ALTER TABLE PromotionM ADD ScheduleDate DATETIME NULL
END
IF COL_LENGTH('PromotionM', 'IsNotification') IS NULL
BEGIN
    ALTER TABLE PromotionM ADD IsNotification BIT NULL
END

---MODIFY BY SIVA 4 AUGUST 2025 12:22 PM -------------
IF COL_LENGTH('PartnerAccountDetails', 'AssignedTo') IS NULL
BEGIN
    ALTER TABLE PartnerAccountDetails ADD AssignedTo INT NULL
END
--- MODIFY BY SIVA 4 AUGUST 2025 3:09 PM ----

	 UPDATE pad
SET pad.AssignedTo = pa.AssignedTo
--select pad.AssignedTo,pa.AssignedTo
FROM PartnerAccountDetails pad
INNER JOIN PartnerAccounts pa ON pad.PartnerAccountId = pa.Id
WHERE pad.AssignedTo IS NULL OR pad.AssignedTo <> pa.AssignedTo;
;

----------- CHANGES BY RAGESH for MOBILE and SALES DASHBOARD ENHANCEMENTS---------------------------------------------

--SALES DASHBOARD
----------------------------------------------------------------------------------------------------------------------------------------------------------------------     
--  EXEC GetSalesDashboardReport 'bb74d26f-aa28-eb11-bee5-00155d53687a', '2025-06-01 00:00:00.000', '2025-06-30 23:59:59.100', 'months', 'e616912b-a987-eb11-94db-00155d53687a'     
----------------------------------------------------------------------------------------------------------------------------------------------------------------------     
 GO
ALTER PROCEDURE GetSalesDashboardReport
    @LoggedInUser VARCHAR(50), 
    @StartDate DATETIME, 
    @EndDate DATETIME, 
    @ThreeMonthPerformaceChartPeriodType VARCHAR(10) = 'months', 
    -- 'months' | 'quarters' | 'years'     
    @SalesPersonPublicKey NVARCHAR(36) = NULL, 
    @ActiveUserPeriodType NVARCHAR(10) = 'months' 
AS     
BEGIN 
    SET NOCOUNT ON; 
 
    DECLARE @AnonyMouse bit 
 
    SELECT @AnonyMouse =     
                CASE     
                    WHEN [Code] = 'true' THEN 1    
                    ELSE 0    
                END 
    FROM Settings 
    WHERE [Value] = 'Anonymouse'; 
 
    -- Log the report request          
    INSERT INTO Logs 
    VALUES 
        ( 
            CONVERT(VARCHAR(20), @StartDate, 120) + ' to ' + CONVERT(VARCHAR(20), @EndDate, 120), 
            'exec GetSalesDashboardReport', 
            GETDATE()          
    ); 
 
    DROP TABLE IF EXISTS #UsersIds; 
 
    CREATE TABLE #UsersIds 
    ( 
        Id UNIQUEIDENTIFIER 
    ); 
 
    -- Check if role is admin     
    DECLARE @IsAdmin BIT = 0; 
 
    IF (SELECT RoleKey 
    FROM Users 
    WHERE PublicKey = @LoggedInUser) = 'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b'     
    BEGIN 
        SET @IsAdmin = 1; 
    END 
 
    -- Populate #UsersIds     
    IF @IsAdmin = 0     
    BEGIN 
        DECLARE @UserId INT; 
 
        SELECT @UserId = Id 
        FROM Users 
        WHERE PublicKey = @LoggedInUser; 
 
        IF EXISTS (SELECT 1 
        FROM Users 
        WHERE SupervisorId = @UserId)     
        BEGIN 
            -- Add subordinates and self     
            INSERT INTO #UsersIds 
                (Id) 
            SELECT PublicKey 
            FROM Users 
            WHERE SupervisorId = @UserId OR Id = @UserId; 
        END     
        ELSE     
        BEGIN 
            -- Only self     
            INSERT INTO #UsersIds 
                (Id) 
            SELECT PublicKey 
            FROM Users 
            WHERE Id = @UserId; 
        END 
    END     
    ELSE     
    BEGIN 
        -- Admin can see all users     
        INSERT INTO #UsersIds 
            (Id) 
        SELECT PublicKey 
        FROM Users; 
    END; 
 
 
    -- Declare variables with appropriate types/precisions          
    DECLARE @TotalRevenue DECIMAL(18,2) = 0.0,          
            @TotalApprovedRevenue DECIMAL(18,2) = 0.0,          
            @TotalPendingRevenue DECIMAL(18,2) = 0.0,          
            @TotalSalesInCount INT = 0,          
            @TotalSalesPerPerson NVARCHAR(MAX) = '',          
            @TodaysSales DECIMAL(18,2) = 0.0,          
            @TopFiveDealJson NVARCHAR(MAX),          
            @TotalSalesPerDay NVARCHAR(MAX),          
            @EveryDayPerformance NVARCHAR(MAX),          
            @MonthlySalesReport NVARCHAR(MAX),          
            @TotalSalesPerService NVARCHAR(MAX),     
            @ProductRevenueForSalesPersonJson NVARCHAR(MAX),     
            @ShowSalesPersonDropDownJson NVARCHAR(MAX),     
            @SubscriptionReportJson NVARCHAR(MAX),     
            @ActiveUsersReportJson NVARCHAR(MAX); 
 
    -- Consolidate revenue aggregations into one query to reduce table scans.          
    SELECT 
        @TotalRevenue = SUM(CASE WHEN st.code IN ('app','cus','pen') THEN po.PaidAmount ELSE 0 END), 
        @TotalApprovedRevenue = SUM(CASE WHEN st.code IN ('app','cus') THEN po.PaidAmount ELSE 0 END), 
        @TotalPendingRevenue = SUM(CASE WHEN st.code = 'pen' THEN po.PaidAmount ELSE 0 END) 
    FROM PurchaseOrders AS po 
        INNER JOIN STATUS AS st ON po.STATUS = st.Id 
    WHERE po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
        AND (PO.Anonymouse = @AnonyMouse) 
        AND (         
        NOT EXISTS (SELECT 1 
        FROM #UsersIds) -- Admin case: #UsersIds is empty, fetch all         
        OR po.CreatedBy IN (SELECT Id 
        FROM #UsersIds)  -- Leader or employee: filter by CreatedBy         
    ); 
    --AND po.CreatedBy = ISNULL(@LoggedInUser, po.CreatedBy);          
 
    -- Total Sales Count (only active orders and excluding 'pen' and 'rej' statuses)          
    SELECT @TotalSalesInCount = COUNT(1) 
    FROM PurchaseOrders AS po 
        inner join Leads as le on le.Id = po.LeadId 
    WHERE po.IsActive = 1 
        AND po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
        AND (PO.Anonymouse = @AnonyMouse) 
        -- AND po.CreatedBy = ISNULL(@LoggedInUser, po.CreatedBy)          
        AND (         
        NOT EXISTS (SELECT 1 
        FROM #UsersIds) -- Admin case: #UsersIds is empty, fetch all         
        OR po.CreatedBy IN (SELECT Id 
        FROM #UsersIds)  -- Leader or employee: filter by CreatedBy         
        ) 
        AND po.STATUS IN (SELECT id 
        FROM STATUS 
        WHERE code IN ('cus','app')) and le.IsSpam = 0; 
 
    -- Sales Per Person as JSON          
    SELECT @TotalSalesPerPerson = (          
        SELECT 
            MAX(um.UserKey) AS UserKey, 
            MAX(us.FirstName) AS FirstName, 
            SUM(ISNULL(po.PaidAmount, 0.0)) AS PaidAmount 
        FROM (         
            SELECT DISTINCT UserKey 
            FROM UserMappings 
            WHERE IsActive = 1 AND IsDelete = 0        
        ) AS um 
            LEFT JOIN Users AS us ON um.UserKey = us.Publickey 
            LEFT JOIN PurchaseOrders AS po ON po.CreatedBy = um.UserKey 
        WHERE po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
            AND (PO.Anonymouse = @AnonyMouse) 
            --   AND po.CreatedBy = ISNULL(@LoggedInUser, po.CreatedBy)          
            AND po.STATUS IN (SELECT id 
            FROM STATUS 
            WHERE code IN ('cus','app')) 
            AND (         
            NOT EXISTS (SELECT 1 
            FROM #UsersIds) -- Admin case: #UsersIds is empty, fetch all         
            OR po.CreatedBy IN (SELECT Id 
            FROM #UsersIds)  -- Leader or employee: filter by CreatedBy         
        ) 
        GROUP BY po.CreatedBy 
        ORDER BY SUM(po.PaidAmount) DESC 
        FOR JSON AUTO          
    ); 
 
    -- Today's Sales (or sales for the given date range)          
    SELECT @TodaysSales = SUM(po.PaidAmount) 
    FROM PurchaseOrders AS po 
        INNER JOIN STATUS AS st ON po.STATUS = st.Id 
    WHERE po.IsActive = 1 
        AND po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
 
        AND (PO.Anonymouse = @AnonyMouse) 
        --   AND po.CreatedBy = ISNULL(@LoggedInUser, po.CreatedBy)          
        AND (         
        NOT EXISTS (SELECT 1 
        FROM #UsersIds) -- Admin case: #UsersIds is empty, fetch all         
        OR po.CreatedBy IN (SELECT Id 
        FROM #UsersIds)  -- Leader or employee: filter by CreatedBy         
        ) 
        AND po.STATUS IN (SELECT id 
        FROM STATUS 
        WHERE code IN ('cus','app')); 
 
    -- Top 5 Deals as JSON          
    SELECT @TopFiveDealJson = (          
         SELECT 
            po.LeadId, 
            po.ClientName, 
            Cast(po.StartDate as date) as PaymentDate, 
            po.CreatedOn, 
            po.PaidAmount 
        FROM PurchaseOrders AS PO 
            LEFT JOIN Leads AS le ON PO.LeadId = le.Id 
            LEFT JOIN LeadSources ON le.LeadSourceKey = LeadSources.PublicKey 
            LEFT JOIN Users AS CreatedUser ON PO.CreatedBy = CreatedUser.PublicKey 
            LEFT JOIN PaymentModes AS pm ON PO.ModeOfPayment = pm.Id 
            LEFT JOIN Services AS ser ON ser.Id = PO.ServiceId 
            LEFT JOIN Status AS st ON st.Id = PO.Status 
            LEFT JOIN Users AS u ON u.PublicKey = le.AssignedTo 
        WHERE po.IsActive = 1 
            AND (PO.Anonymouse = @AnonyMouse) 
            AND CAST(po.PaymentDate AS DATE) BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
            AND po.STATUS IN (SELECT id 
            FROM STATUS 
            WHERE st.Code IN ('cus','app')) and le.IsSpam = 0 AND (         
            NOT EXISTS (SELECT 1 
            FROM #UsersIds) -- Admin case: #UsersIds is empty, fetch all         
            OR po.CreatedBy IN (SELECT Id 
            FROM #UsersIds)  -- Leader or employee: filter by CreatedBy         
        ) 
        ORDER BY PaymentDate DESC 
        FOR JSON AUTO          
    ); 
 
    -- Total Sales Per Day as JSON          
    SELECT @TotalSalesPerDay = (          
        SELECT 
            FORMAT(po.PaymentDate, 'dd-MMM') AS PaymentDate, 
            SUM(po.PaidAmount) AS PerDaySales, 
            COUNT(*) AS PerDaySalesCount 
        FROM PurchaseOrders AS po 
            INNER JOIN STATUS AS st ON po.STATUS = st.Id 
        WHERE po.IsActive = 1 
 
            AND po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
            --   AND po.CreatedBy = ISNULL(@LoggedInUser, po.CreatedBy)          
            AND (         
            NOT EXISTS (SELECT 1 
            FROM #UsersIds) -- Admin case: #UsersIds is empty, fetch all         
            OR po.CreatedBy IN (SELECT Id 
            FROM #UsersIds)  -- Leader or employee: filter by CreatedBy         
        ) 
            AND po.STATUS IN (SELECT id 
            FROM STATUS 
            WHERE code IN ('cus','app')) 
        GROUP BY po.PaymentDate 
        FOR JSON AUTO          
    ); 
 
    -- Every Day Performance (last 30 days relative to @EndDate) as JSON          
    ;WITH 
        DateRange 
        AS 
        ( 
                            SELECT CAST(DATEADD(DAY, -30, @EndDate) AS DATE) AS [Date] 
            UNION ALL 
                SELECT DATEADD(DAY, 1, [Date]) 
                FROM DateRange 
                WHERE [Date] < CAST(@EndDate AS DATE) 
        ) 
 
    SELECT @EveryDayPerformance = (   
            SELECT 
            FORMAT(dr.[Date], 'dd-MMM') AS [Date], 
            SUM(ISNULL(po.PaidAmount, 0.0)) AS PaidAmount 
        FROM DateRange dr 
            LEFT JOIN PurchaseOrders AS po 
            ON dr.[Date] = CAST(po.PaymentDate AS DATE) 
                AND (   
                    NOT EXISTS (   
                        SELECT 1 
                FROM #UsersIds   
                    ) -- Admin case: #UsersIds is empty, fetch all   
                OR po.CreatedBy IN (   
                        SELECT Id 
                FROM #UsersIds   
                    ) -- Leader or employee: filter by CreatedBy   
                ) 
                AND po.Status IN (   
                    SELECT id 
                FROM Status 
                WHERE Code IN ('cus', 'app')   
                ) 
        WHERE (PO.Anonymouse = @AnonyMouse) 
        GROUP BY dr.[Date] 
        FOR JSON AUTO   
        ); 
 
 
    -- Monthly Sales Report as JSON (aggregated by month)          
    ;WITH 
        MonthRange 
        AS 
        ( 
                            SELECT CAST(@StartDate AS DATE) AS MonthDate 
            UNION ALL 
                SELECT DATEADD(MONTH, 1, MonthDate) 
                FROM MonthRange 
                WHERE DATEADD(MONTH, 1, MonthDate) <= CAST(@EndDate AS DATE) 
        ), 
 
        GetMonthlyReport 
        AS 
        ( 
            SELECT 
                DATEFROMPARTS(YEAR(po.PaymentDate), MONTH(po.PaymentDate), 1) AS SalesMonth, 
                SUM(po.PaidAmount) AS TotalSales 
            FROM PurchaseOrders AS po 
            WHERE    
                    (   
                  NOT EXISTS (   
                            SELECT 1 
                FROM #UsersIds   
                        ) -- Admin case: #UsersIds is empty, fetch all    
                OR po.CreatedBy IN (   
                            SELECT Id 
                FROM #UsersIds   
                        ) -- Leader or employee: filter by CreatedBy   
                    ) 
                AND po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
                AND (PO.Anonymouse = @AnonyMouse) 
                AND po.Status IN (   
                        SELECT Id 
                FROM Status 
                WHERE Code IN ('cus', 'app')   
                    ) 
            GROUP BY DATEFROMPARTS(YEAR(po.PaymentDate), MONTH(po.PaymentDate), 1) 
        ) 
 
    -- Total Sales Per Service as JSON          
    SELECT @TotalSalesPerService = (          
        SELECT 
            se.Id, 
            se.Name, 
            SUM(ISNULL(po.PaidAmount, 0)) AS TotalPayment 
        FROM Services AS se 
            LEFT JOIN PurchaseOrders AS po ON se.Id = po.ServiceId 
        WHERE    (         
        NOT EXISTS (SELECT 1 
            FROM #UsersIds) -- Admin case: #UsersIds is empty, fetch all         
            OR po.CreatedBy IN (SELECT Id 
            FROM #UsersIds)  -- Leader or employee: filter by CreatedBy         
        ) --po.CreatedBy = ISNULL(@LoggedInUser, po.CreatedBy)          
            AND po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
            AND (PO.Anonymouse = @AnonyMouse) 
            AND po.STATUS IN (SELECT id 
            FROM STATUS 
            WHERE code IN ('cus','app')) 
        GROUP BY se.Id, se.Name 
        ORDER BY TotalPayment DESC 
        FOR JSON AUTO          
    ); 
 
    -- New: Daily Sales Report for Chart (retains existing monthly logic)         
    ;WITH 
        DayRange 
        AS 
        ( 
                            SELECT CAST(@StartDate AS DATE) AS SalesDate 
            UNION ALL 
                SELECT DATEADD(DAY, 1, SalesDate) 
                FROM DayRange 
                WHERE DATEADD(DAY, 1, SalesDate) <= CAST(@EndDate AS DATE) 
        ), 
        GetDailySalesReport 
        AS 
        ( 
            SELECT 
                CAST(po.PaymentDate AS DATE) AS SalesDate, 
                SUM(ISNULL(po.PaidAmount, 0.0)) AS TotalSales 
            FROM PurchaseOrders AS po 
            WHERE          
        po.PaymentDate BETWEEN @StartDate AND @EndDate 
                AND (PO.Anonymouse = @AnonyMouse) 
                AND po.STATUS IN (SELECT id 
                FROM STATUS 
                WHERE code IN ('cus','app')) 
                AND (         
            NOT EXISTS (SELECT 1 
                FROM #UsersIds) 
                OR po.CreatedBy IN (SELECT Id 
                FROM #UsersIds)         
        ) 
            GROUP BY CAST(po.PaymentDate AS DATE) 
        ) 
    SELECT @MonthlySalesReport = (         
    SELECT 
            FORMAT(dr.SalesDate, 'yyyy-MM-dd') AS [Date], 
            ISNULL(ds.TotalSales, 0.0) AS TotalSales 
        FROM DayRange dr 
            LEFT JOIN GetDailySalesReport ds ON dr.SalesDate = ds.SalesDate 
        ORDER BY dr.SalesDate 
        FOR JSON AUTO         
) 
    OPTION 
    (MAXRECURSION 
    1000); 
 
 
    DECLARE @LeadTypes NVARCHAR(MAX); 
    DECLARE @LeadStatus NVARCHAR(MAX); 
    DECLARE @UnallocatedLeads int; 
    DECLARE @AllocatedLeads int; 
    DECLARE @TotalLeads int; 
    DECLARE @EarningFromNewLeads INT; 
    DECLARE @LeadSoruceEarnings NVARCHAR(MAX); 
 
    SELECT @EarningFromNewLeads = (SELECT SUM(po.PaidAmount) AS TotalEarnings 
        FROM Leads AS le 
            INNER JOIN PurchaseOrders AS po ON po.LeadId = le.Id 
        WHERE (CAST(po.CreatedOn AS DATE) BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE)) AND (po.[Status] IN (24, 10)) 
            AND (PO.Anonymouse = @AnonyMouse) ) 
 
    SELECT @LeadSoruceEarnings = (        
        SELECT 
            ls.Name, 
            SUM(po.PaidAmount) AS TotalEarnings 
        FROM PurchaseOrders AS PO 
            LEFT JOIN Leads AS le ON PO.LeadId = le.Id 
            LEFT JOIN LeadSources AS ls ON le.LeadSourceKey = ls.PublicKey 
            LEFT JOIN Users AS CreatedUser ON PO.CreatedBy = CreatedUser.PublicKey 
            LEFT JOIN PaymentModes AS pm ON PO.ModeOfPayment = pm.Id 
            LEFT JOIN Services AS ser ON ser.Id = PO.ServiceId 
            LEFT JOIN Status AS st ON st.Id = PO.Status 
            LEFT JOIN Users AS u ON u.PublicKey = le.AssignedTo 
        where CAST(po.PaymentDate AS DATE) BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
            AND (PO.Anonymouse = @AnonyMouse) 
            and st.Code IN ('cus', 'app') 
        GROUP BY ls.Name 
        ORDER BY TotalEarnings DESC 
        FOR JSON AUTO        
    ) 
 
    SELECT @LeadTypes = ISNULL((         
    SELECT lt.Name , COUNT(1) AS Total 
        FROM Leads  AS le 
            LEFT JOIN LeadTypes AS lt ON lt.PublicKey = CAST(le.LeadTypeKey AS UNIQUEIDENTIFIER) 
        WHERE (         
        NOT EXISTS (SELECT 1 
            FROM #UsersIds) 
            OR le.CreatedBy IN (SELECT Id 
            FROM #UsersIds)       
    ) AND (         
        ISNULL(le.IsDelete, 0) = 0            
    ) 
        --AND(s.Category = 'Lead')        
        GROUP BY lt.Name 
        FOR JSON AUTO         
), '[]'); 
    -- fallback to empty JSON array        
    SELECT 
        @TotalLeads = COUNT(DISTINCT Leads.Id), 
        @AllocatedLeads = SUM(CASE WHEN Leads.LeadTypeKey = '4569c760-96e2-ee11-8142-00155d23d79c' THEN 1 ELSE 0 END) 
    FROM Leads 
        LEFT JOIN PurchaseOrders AS po 
        ON po.LeadId = Leads.Id 
            AND Leads.PurchaseOrderKey = po.PublicKey 
        LEFT JOIN LeadTypes AS LeadTypes 
        ON LeadTypes.PublicKey = CAST(Leads.LeadTypeKey AS UNIQUEIDENTIFIER) 
        LEFT JOIN LeadSources AS LeadSource 
        ON LeadSource.PublicKey = CAST(Leads.LeadSourceKey AS UNIQUEIDENTIFIER) 
            AND ISNULL(LeadSource.IsDelete, 0) = 0 
        LEFT JOIN STATUS AS st 
        ON st.Id = Leads.StatusId 
        LEFT JOIN Users AS Users 
        ON Users.PublicKey = TRY_CAST(Leads.CreatedBy AS UNIQUEIDENTIFIER) 
        LEFT JOIN Services AS LeadService 
        ON Leads.ServiceKey IS NOT NULL 
            AND TRY_CAST(Leads.ServiceKey AS UNIQUEIDENTIFIER) = LeadService.PublicKey 
        LEFT JOIN Services AS PoService 
        ON po.ServiceId = PoService.Id 
            AND ISNULL(po.IsExpired, 0) <> 1 
        LEFT JOIN STATUS AS st1 
        ON st1.Id = po.Status 
        LEFT JOIN PaymentModes AS pm 
        ON po.ModeOfPayment = pm.Id 
    WHERE  
        (       
            NOT EXISTS (SELECT 1 
        FROM #UsersIds) 
        OR Leads.AssignedTo IN (SELECT Id 
        FROM #UsersIds)       
        ) AND  
        (PO.Anonymouse = @AnonyMouse) AND  
        ISNULL(Leads.IsDelete, 0) = 0 
        SET @UnallocatedLeads = @TotalLeads - @AllocatedLeads; 
    --)       
    --AS DistinctLeads;       
    SELECT @LeadStatus = ISNULL((         
        SELECT 
            CASE         
                WHEN le.StatusId IS NULL or po.[Status] is null THEN 'New'         
                ELSE s.Name         
            END AS [Name], 
            COUNT(1) AS Total 
        FROM Leads AS le 
            LEFT JOIN PurchaseOrders AS po ON po.LeadId = le.Id AND le.PurchaseOrderKey = po.PublicKey 
            LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = CAST(le.LeadTypeKey AS UNIQUEIDENTIFIER) 
            LEFT JOIN LeadSources AS LeadSource ON LeadSource.PublicKey = CAST(le.LeadSourceKey AS UNIQUEIDENTIFIER) AND ISNULL(LeadSource.IsDelete, 0) = 0 
            LEFT JOIN STATUS AS st ON st.Id = le.StatusId 
            LEFT JOIN Users AS Users ON Users.PublicKey = TRY_CAST(le.CreatedBy AS UNIQUEIDENTIFIER) 
            LEFT JOIN Services AS LeadService ON le.ServiceKey IS NOT NULL AND TRY_CAST(le.ServiceKey AS UNIQUEIDENTIFIER) = LeadService.PublicKey 
            LEFT JOIN Services AS PoService ON po.ServiceId = PoService.Id AND ISNULL(po.IsExpired, 0) <> 1 
            LEFT JOIN STATUS AS s ON s.Id = po.Status 
            LEFT JOIN PaymentModes AS pm ON po.ModeOfPayment = pm.Id 
        WHERE ISNULL(le.IsDelete, 0) = 0 AND (PO.Anonymouse = @AnonyMouse) 
            AND 
            (         
                    NOT EXISTS (SELECT 1 
            FROM #UsersIds) 
            OR le.AssignedTo IN (SELECT Id 
            FROM #UsersIds)         
                ) 
            AND ((cast(le.CreatedOn as date) BETWEEN cast(@StartDate as date) AND cast(@EndDate as date))) 
            AND (s.Category = 'PO' OR le.StatusId is null or po.[Status] is null) 
        GROUP BY         
                CASE         
                    WHEN le.StatusId IS NULL OR po.[Status] is null THEN 'New'         
                    ELSE s.Name         
                END 
        FOR JSON AUTO          
    ), '[]'); 
 
 
    DECLARE @TotalCustomerCount INT = 0; 
 
    SELECT @TotalCustomerCount = COUNT(*) 
    FROM Leads AS leads 
        INNER JOIN PurchaseOrders AS po ON leads.Id = po.LeadId 
            AND leads.AssignedTo IS NOT NULL 
            AND po.STATUS = (SELECT Id 
            FROM STATUS 
            WHERE name = 'customer') 
        INNER JOIN Users AS users ON leads.AssignedTo = users.PublicKey 
        --LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = Leads.LeadTypeKey         
        INNER JOIN STATUS AS STATUS ON STATUS.Id = po.STATUS 
    WHERE ISNULL(po.IsExpired, 0) = 0 
        AND (PO.Anonymouse = @AnonyMouse) 
        AND (         
        NOT EXISTS (SELECT 1 
        FROM #UsersIds) -- Admin case: #UsersIds is empty, fetch all                 
        OR po.CreatedBy IN (SELECT Id 
        FROM #UsersIds)  -- Leader or employee: filter by CreatedBy         
    ) 
 
 
    DECLARE @mostSalesInCityJson NVARCHAR(MAX); 
 
    DECLARE @weeklyPerformanceJson NVARCHAR(MAX); 
    DECLARE @mostPaymentMethod NVARCHAR(MAX); 
    DECLARE @prCounts NVARCHAR(MAX); 
    DECLARE @threeMonthPerfOfLgdInUser NVARCHAR(MAX); 
    DECLARE @totalPerfOfLgdInUser INT; 
    DECLARE @recentFiveEnquires NVARCHAR(MAX); 
    DECLARE @MonthlyCustomerDetails NVARCHAR(MAX); 
    DECLARE @ChartStartDate DATE = @EndDate; 
    DECLARE @MonthSpan INT; 
    DECLARE @GroupByType VARCHAR(10); 
 
    SELECT @mostSalesInCityJson = (         
    SELECT po.City , SUM(ISNULL(PaidAmount, 0)) AS TotalPayment 
        FROM PurchaseOrders po 
        WHERE    (         
        NOT EXISTS (SELECT 1 
            FROM #UsersIds) -- Admin case: #UsersIds is empty, fetch all         
            OR po.CreatedBy IN (SELECT Id 
            FROM #UsersIds)  -- Leader or employee: filter by CreatedBy         
    ) --po.CreatedBy = ISNULL(@LoggedInUser, po.CreatedBy)          
            AND po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
            AND (PO.Anonymouse = @AnonyMouse) 
            AND po.STATUS IN (SELECT id 
            FROM STATUS 
            WHERE code IN ('cus','app')) 
            AND (po.IsExpired = 0 OR po.IsExpired IS NULL) 
        GROUP BY City 
        ORDER BY TotalPayment DESC 
        FOR JSON AUTO         
); 
 
    SELECT @mostPaymentMethod = (         
    SELECT pm.Name as [Name], SUM(ISNULL(PaidAmount, 0)) AS TotalPayment 
        FROM PurchaseOrders po 
            INNER JOIN PaymentModes pm ON po.ModeOfPayment = pm.Id 
        WHERE    (         
        NOT EXISTS (SELECT 1 
            FROM #UsersIds) -- Admin case: #UsersIds is empty, fetch all         
            OR po.CreatedBy IN (SELECT Id 
            FROM #UsersIds)  -- Leader or employee: filter by CreatedBy         
            ) --po.CreatedBy = ISNULL(@LoggedInUser, po.CreatedBy)          
            AND po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
            AND (PO.Anonymouse = @AnonyMouse) 
            AND po.STATUS IN (SELECT id 
            FROM STATUS 
            WHERE code IN ('cus','app')) 
            AND (po.IsExpired = 0 OR po.IsExpired IS NULL) 
        GROUP BY pm.Name 
        ORDER By TotalPayment DESC 
        FOR JSON AUTO         
    ); 
 
    SELECT @prCounts = (         
    SELECT s.Name, COUNT(*) AS Total 
        FROM PurchaseOrders po 
            INNER JOIN [Status] s ON po.[Status] = s.Id 
        WHERE (         
        NOT EXISTS (SELECT 1 
            FROM #UsersIds) 
            OR po.CreatedBy IN (SELECT Id 
            FROM #UsersIds)         
    ) 
            AND po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
            AND (PO.Anonymouse = @AnonyMouse) 
            AND ISNULL(po.IsExpired, 0) = 0 -- Only non-expired         
            AND po.Status IN (SELECT Id 
            FROM Status) 
        -- Optional but specific         
        GROUP BY s.Id, s.Name 
        FOR JSON AUTO         
); 
 
    WITH 
        WeeksInMonth 
        AS 
        ( 
            -- Anchor: Sunday before or equal to @StartDate        
                            SELECT 
                    CAST(DATEADD(DAY, -DATEPART(WEEKDAY, @StartDate) + 1, @StartDate) AS DATE) AS WeekStartDate, 
                    CAST(DATEADD(DAY, -DATEPART(WEEKDAY, @StartDate) + 7, @StartDate) AS DATE) AS WeekEndDate 
 
            UNION ALL 
 
                -- Recursive: Add 7 days per week        
                SELECT 
                    DATEADD(DAY, 7, WeekStartDate), 
                    DATEADD(DAY, 7, WeekEndDate) 
                FROM WeeksInMonth 
                WHERE DATEADD(DAY, 7, WeekStartDate) <= EOMONTH(@EndDate, 0) 
        ) 
    SELECT @weeklyPerformanceJson = (        
    SELECT 
            -- Format the week range as 'DD-Mon - DD-Mon' (e.g., '29-Dec - 04-Jan')        
            FORMAT(w.WeekStartDate, 'dd-MMM') + ' - ' + FORMAT(w.WeekEndDate, 'dd-MMM') AS PaymentDate, 
            -- Calculate the total payment for each week, defaulting to 0 if no payments are found.        
            ISNULL(SUM(po.PaidAmount), 0) AS TotalPayment 
        FROM WeeksInMonth w 
            LEFT JOIN PurchaseOrders po 
            -- Join PurchaseOrders where the payment date falls within the current week's range (Sunday to Saturday)        
            ON CAST(po.PaymentDate AS DATE) >= w.WeekStartDate 
                AND CAST(po.PaymentDate AS DATE) <= w.WeekEndDate 
                -- IMPORTANT: Also ensure the payment date is strictly within the overall @StartDate and @EndDate month range.        
                -- We compare po.PaymentDate (which is DATETIME) to @StartDate (DATETIME) directly.        
                -- For @EndDate, we use < DATEADD(DAY, 1, @EndDate) to include all payments on the @EndDate day, regardless of time.        
                AND po.PaymentDate >= @StartDate 
                AND (PO.Anonymouse = @AnonyMouse) 
                AND po.PaymentDate < DATEADD(DAY, 1, @EndDate) 
                -- Apply the existing user ID filter: either no specific users are filtered, or CreatedBy is in #UsersIds.        
                AND (        
            NOT EXISTS (SELECT 1 
                FROM #UsersIds) 
                OR po.CreatedBy IN (SELECT Id 
                FROM #UsersIds)        
        ) 
                -- Apply the existing status filter: only 'cus' or 'app' statuses.        
                AND po.STATUS IN (SELECT id 
                FROM STATUS 
                WHERE code IN ('cus','app')) 
                -- Apply the existing IsExpired filter.        
                AND ISNULL(po.IsExpired, 0) = 0 
        GROUP BY w.WeekStartDate, w.WeekEndDate 
        ORDER BY w.WeekStartDate 
        FOR JSON AUTO -- Output the result as a JSON array.        
) 
    OPTION 
    (MAXRECURSION 
    100); 
    -- Set MAXRECURSION to handle the number of weeks in a month, plus buffer.        
 
    -- CTE to generate past 3 months including current month         
 
    IF @ThreeMonthPerformaceChartPeriodType = 'months'        
        SET @ChartStartDate = DATEADD(MONTH, -2, DATEFROMPARTS(YEAR(@EndDate), MONTH(@EndDate), 1));        
    ELSE IF @ThreeMonthPerformaceChartPeriodType = 'quarters'        
        SET @ChartStartDate = DATEADD(QUARTER, -2, DATEFROMPARTS(YEAR(@EndDate), ((DATEPART(QUARTER, @EndDate) - 1) * 3 + 1), 1));        
    ELSE IF @ThreeMonthPerformaceChartPeriodType = 'years'        
        SET @ChartStartDate = DATEFROMPARTS(YEAR(@EndDate) - 2, 1, 1); 
    WITH 
        PeriodRange 
        AS 
        ( 
                            SELECT 
                    0 AS N, 
                    CAST(        
                CASE        
                    WHEN @ThreeMonthPerformaceChartPeriodType = 'months' THEN DATEADD(MONTH, -2, DATEFROMPARTS(YEAR(@EndDate), MONTH(@EndDate), 1))        
                    WHEN @ThreeMonthPerformaceChartPeriodType = 'quarters' THEN DATEADD(QUARTER, -2, DATEFROMPARTS(YEAR(@EndDate), ((DATEPART(QUARTER, @EndDate)-1)*3 + 1), 1))        
                    WHEN @ThreeMonthPerformaceChartPeriodType = 'years' THEN DATEFROMPARTS(YEAR(@EndDate) - 2, 1, 1)        
                END AS DATE) AS PeriodStart 
            UNION ALL 
                SELECT 
                    N + 1, 
                    CAST(        
                CASE        
                    WHEN @ThreeMonthPerformaceChartPeriodType = 'months' THEN DATEADD(MONTH, 1, PeriodStart)        
                    WHEN @ThreeMonthPerformaceChartPeriodType = 'quarters' THEN DATEADD(QUARTER, 1, PeriodStart)        
                    WHEN @ThreeMonthPerformaceChartPeriodType = 'years' THEN DATEADD(YEAR, 1, PeriodStart)        
                END AS DATE        
                ) 
                FROM PeriodRange 
                WHERE N < 2 
            -- To get 3 periods (0, 1, 2)        
        ), 
        AggregatedData 
        AS 
        ( 
            SELECT 
                -- This CASE expression must be identical to the one in GROUP BY        
                CASE        
                    WHEN @ThreeMonthPerformaceChartPeriodType = 'months' THEN FORMAT(DATEFROMPARTS(YEAR(PaymentDate), MONTH(PaymentDate), 1), 'yyyy-MM')        
                    WHEN @ThreeMonthPerformaceChartPeriodType = 'quarters' THEN CONCAT('Q', DATEPART(QUARTER, PaymentDate), '-', YEAR(PaymentDate))        
                    WHEN @ThreeMonthPerformaceChartPeriodType = 'years' THEN FORMAT(PaymentDate, 'yyyy') -- Ensure this is also the FORMAT function for consistency        
                END AS PeriodKey, 
                SUM(ISNULL(PaidAmount, 0)) AS TotalPayment 
            FROM PurchaseOrders 
            WHERE        
                PaymentDate >= @ChartStartDate AND PaymentDate < DATEADD(DAY, 1, @EndDate) 
                AND (Anonymouse = @AnonyMouse) 
                AND (        
                    NOT EXISTS (SELECT 1 
                FROM #UsersIds) 
                OR CreatedBy IN (SELECT Id 
                FROM #UsersIds)        
                ) 
                AND Status IN (SELECT Id 
                FROM STATUS 
                WHERE Code IN ('app', 'cus')) 
                AND ISNULL(IsExpired, 0) = 0 
            GROUP BY        
                -- TWEAK: Make sure this CASE expression is IDENTICAL to the one in the SELECT list for PeriodKey        
                CASE        
                    WHEN @ThreeMonthPerformaceChartPeriodType = 'months' THEN FORMAT(DATEFROMPARTS(YEAR(PaymentDate), MONTH(PaymentDate), 1), 'yyyy-MM')        
                    WHEN @ThreeMonthPerformaceChartPeriodType = 'quarters' THEN CONCAT('Q', DATEPART(QUARTER, PaymentDate), '-', YEAR(PaymentDate)) -- This was previously 'CONCAT(YEAR(PaymentDate), '-', DATEPART(QUARTER, PaymentDate))' - now corrected   
  
  
   
    
     
                    WHEN @ThreeMonthPerformaceChartPeriodType = 'years' THEN FORMAT(PaymentDate, 'yyyy') -- This was previously 'CAST(YEAR(PaymentDate) AS VARCHAR(4))' - now corrected        
END 
        ) 
    SELECT @threeMonthPerfOfLgdInUser = (        
        SELECT 
            CASE        
                WHEN @ThreeMonthPerformaceChartPeriodType = 'months' THEN FORMAT(PeriodStart, 'yyyy-MM')        
                WHEN @ThreeMonthPerformaceChartPeriodType = 'quarters' THEN CONCAT('Q', DATEPART(QUARTER, PeriodStart), '-', YEAR(PeriodStart))        
                WHEN @ThreeMonthPerformaceChartPeriodType = 'years' THEN FORMAT(PeriodStart, 'yyyy')        
            END AS PaymentDate, 
            ISNULL(a.TotalPayment, 0) AS TotalPayment 
        FROM PeriodRange r 
            LEFT JOIN AggregatedData a ON        
            (        
                (@ThreeMonthPerformaceChartPeriodType = 'months' AND FORMAT(r.PeriodStart, 'yyyy-MM') = a.PeriodKey) 
                OR (@ThreeMonthPerformaceChartPeriodType = 'quarters' AND CONCAT('Q', DATEPART(QUARTER, r.PeriodStart), '-', YEAR(r.PeriodStart)) = a.PeriodKey) 
                OR (@ThreeMonthPerformaceChartPeriodType = 'years' AND FORMAT(r.PeriodStart, 'yyyy') = a.PeriodKey)        
            ) 
        ORDER BY r.PeriodStart 
        FOR JSON AUTO        
    ) 
    OPTION 
    (MAXRECURSION 
    10);-- MAXRECURSION 10 is sufficient for 3 periods.        
 
 
    SELECT @totalPerfOfLgdInUser = (         
    SELECT 
            SUM(ISNULL(PaidAmount, 0)) AS TotalPayment 
        FROM PurchaseOrders 
        WHERE (         
        NOT EXISTS (SELECT 1 
            FROM #UsersIds) 
            OR CreatedBy IN (SELECT Id 
            FROM #UsersIds)         
    ) 
            AND STATUS IN (SELECT id 
            FROM STATUS 
            WHERE code IN ('app','cus')) 
            AND ISNULL(IsExpired, 0) = 0 
            AND (Anonymouse = @AnonyMouse)          
); 
 
 
    IF OBJECT_ID('tempdb..#UserPRSummary') IS NOT NULL         
    DROP TABLE #UserPRSummary; 
    SELECT 
        um.UserKey, 
        MAX(us.FirstName) AS FirstName, 
        SUM(ISNULL(po.PaidAmount, 0.0)) AS PaidAmount, 
        SUM(CASE WHEN s.Code = 'app' THEN 1 ELSE 0 END) AS Approved, 
        SUM(CASE WHEN s.Code = 'pen' THEN 1 ELSE 0 END) AS Pending, 
        SUM(CASE WHEN s.Code = 'rej' THEN 1 ELSE 0 END) AS Rejected 
    INTO #UserPRSummary 
    FROM (         
    SELECT DISTINCT UserKey 
        FROM UserMappings 
        WHERE IsActive = 1 AND IsDelete = 0         
) AS um 
        LEFT JOIN Users AS us ON um.UserKey = us.PublicKey 
        LEFT JOIN PurchaseOrders AS po ON po.CreatedBy = um.UserKey 
        LEFT JOIN [Status] s ON po.[Status] = s.Id 
    WHERE          
    po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
        AND (PO.Anonymouse = @AnonyMouse) 
        AND (         
        NOT EXISTS (SELECT 1 
        FROM #UsersIds) 
        OR po.CreatedBy IN (SELECT Id 
        FROM #UsersIds)         
    ) 
        AND (po.IsExpired = 0 OR po.IsExpired IS NULL) 
        AND po.IsActive = 1 AND (us.IsDisabled = 0 OR us.IsDisabled IS NULL) 
        AND (us.IsDelete = 0 OR us.IsDelete IS NULL) 
    GROUP BY um.UserKey 
    ORDER by UserKey; 
 
    -- Step 2: Assign the JSON result to a variable         
    DECLARE @salesPerPersonWithPr NVARCHAR(MAX); 
 
    SELECT @salesPerPersonWithPr = (         
    SELECT * 
        FROM #UserPRSummary 
        ORDER BY PaidAmount DESC 
        FOR JSON AUTO         
); 
 
    DECLARE @hourlyPerformanceJson NVARCHAR(MAX); 
 
    WITH 
        DayRange 
        AS 
        ( 
                            SELECT CAST(GETDATE() AS DATE) AS DayStart 
            UNION ALL 
                SELECT DATEADD(DAY, -1, DayStart) 
                FROM DayRange 
                WHERE DayStart > DATEADD(DAY, -6, CAST(GETDATE() AS DATE)) 
        ), 
        DailyTotals 
        AS 
        ( 
            SELECT 
                CAST(PaymentDate AS DATE) AS DayStart, 
                SUM(ISNULL(PaidAmount, 0)) AS TotalPayment 
FROM PurchaseOrders 
            WHERE (        
        NOT EXISTS (SELECT 1 
                FROM #UsersIds) 
                OR CreatedBy IN (SELECT Id 
                FROM #UsersIds)        
        ) 
                AND PaymentDate >= DATEADD(DAY, -6, CAST(GETDATE() AS DATE)) -- Changed to last 7 days     
                AND (Anonymouse = @AnonyMouse) 
                AND PaymentDate <= GETDATE() 
                AND STATUS IN (SELECT id 
                FROM STATUS 
                WHERE code NOT IN ('pen', 'rej')) 
                AND ISNULL(IsExpired, 0) = 0 
            GROUP BY CAST(PaymentDate AS DATE) 
        ) 
    SELECT @hourlyPerformanceJson = (        
    SELECT 
            FORMAT(dr.DayStart, 'yyyy-MM-dd') AS [Hour], -- Changed to 'Day' and format for date        
            ISNULL(dt.TotalPayment, 0) AS TotalPayment 
        FROM DayRange dr 
            LEFT JOIN DailyTotals dt ON dr.DayStart = dt.DayStart 
        ORDER BY dr.DayStart 
        FOR JSON AUTO        
) 
    OPTION 
    (MAXRECURSION 
    25); 
 
    SELECT @recentFiveEnquires = (         
       SELECT DISTINCT TOP 5 
            en.Id, 
            en.Details, 
            le.FullName AS LeadName, 
            en.ReferenceKey, 
            u.FirstName + ' ' + u.LastName AS CreatedBy, 
            en.PublickEy, 
            en.CreatedOn, 
            le.MobileNumber, 
            ISNULL(le.Favourite , 0) AS Favourite 
        FROM Enquiries AS en 
            INNER JOIN Leads AS le ON en.ReferenceKey = le.PublicKey 
            INNER JOIN Users AS u ON u.PublicKey = en.CreatedBy 
        WHERE u.IsDisabled = 0 
        ORDER BY EN.CreatedOn DESC 
        FOR JSON PATH         
    ); 
 
    ;WITH 
        CustomerOrders 
        AS 
        ( 
            SELECT 
                po.LeadId, 
                CAST(po.StartDate AS DATE) AS ServiceStartDate 
            FROM PurchaseOrders AS po 
                INNER JOIN Status AS st ON st.Id = po.Status 
            WHERE       
        po.IsActive = 1 
                AND (PO.Anonymouse = @AnonyMouse) 
                AND CAST(po.PaymentDate AS DATE) BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
                AND st.Code = 'cus' 
                AND (      
            NOT EXISTS (SELECT 1 
                FROM #UsersIds) 
                OR po.CreatedBy IN (SELECT Id 
                FROM #UsersIds)      
        ) 
        ), 
        LeadClassification 
        AS 
        ( 
            SELECT 
                LeadId, 
                CASE       
            WHEN COUNT(*) > 1 THEN 'Returning'      
            ELSE 'New'      
        END AS CustomerType 
            FROM CustomerOrders 
            GROUP BY LeadId 
        ), 
        ClassifiedOrders 
        AS 
        ( 
            SELECT 
                co.ServiceStartDate, 
                lc.CustomerType 
            FROM CustomerOrders co 
                INNER JOIN LeadClassification lc ON co.LeadId = lc.LeadId 
        ), 
        DailyCounts 
        AS 
        ( 
            SELECT 
                ServiceStartDate AS [Date], 
                COUNT(CASE WHEN CustomerType = 'New' THEN 1 END) AS [New], 
                COUNT(CASE WHEN CustomerType = 'Returning' THEN 1 END) AS [Returning] 
            FROM ClassifiedOrders 
            GROUP BY ServiceStartDate 
        ) 
    SELECT @MonthlyCustomerDetails = (      
    SELECT 
            FORMAT([Date], 'yyyy-MM-dd') AS [Date], 
            [New], 
            [Returning] 
        FROM DailyCounts 
        WHERE ([New] + [Returning]) > 0 
        ORDER BY [Date] 
        FOR JSON PATH      
    ); 
 
    -- Sales Person Dropdown     
    SELECT @ShowSalesPersonDropDownJson = (     
        SELECT 
            PublicKey, 
            CONCAT_WS(' ', FirstName, LastName) AS FullName 
        FROM Users 
        WHERE PublicKey IN (SELECT Id 
        FROM #UsersIds) 
        FOR JSON PATH     
    ); 
 
    SELECT @ProductRevenueForSalesPersonJson = (   
        SELECT 
            s.Name AS ServiceName, 
            SUM(po.PaidAmount) AS TotalPaidAmount 
        FROM PurchaseOrders AS po 
            INNER JOIN Services AS s ON s.Id = po.ServiceId 
        WHERE      
            ((     
                @SalesPersonPublicKey IS NOT NULL 
            AND po.CreatedBy = CAST(@SalesPersonPublicKey AS UNIQUEIDENTIFIER)     
            ) 
            OR 
            (     
                @SalesPersonPublicKey IS NULL 
            AND po.CreatedBy IN (SELECT Id 
            FROM #UsersIds)     
            )) 
            AND CAST(po.PaymentDate AS DATE) BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
            AND (PO.Anonymouse = @AnonyMouse) 
        GROUP BY s.Name 
        ORDER BY SUM(po.PaidAmount) DESC 
        FOR JSON PATH     
    ); 
 
    WITH 
        SubscriptionReport1 
        AS 
        ( 
            SELECT 
                po.Id, 
                s.Name, 
                po.LeadId, 
                po.ClientName, 
                po.IsActive, 
                po.IsExpired, 
                po.EndDate, 
                po.CreatedBy, 
                po.CreatedOn, 
                po.ModifiedOn, 
                po.PaymentDate 
            FROM PurchaseOrders AS po 
                LEFT JOIN Services AS s ON po.ServiceId = s.Id 
            WHERE (PO.Anonymouse = @AnonyMouse) 
                AND 
                NOT EXISTS (SELECT 1 
                FROM #UsersIds) 
                OR po.CreatedBy IN (SELECT Id 
                FROM #UsersIds) 
        ) 
 
    SELECT @SubscriptionReportJson = (     
        SELECT 
            COUNT( ClientName) AS TotalSubscription, 
            COUNT( CASE      
                      WHEN ISNULL(IsActive, 1) = 1 AND ISNULL(IsExpired, 0) = 0      
                      THEN ClientName      
                   END) AS ActiveSubscription, 
            COUNT( CASE      
                      WHEN (ISNULL(IsActive, 1) = 0 OR ISNULL(IsExpired, 0) = 1)       
                      THEN ClientName      
                   END) AS InactiveSubscription 
        FROM SubscriptionReport1 
        WHERE      
        (     
            CAST(PaymentDate AS DATE) BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE)      
        ) 
        FOR JSON PATH     
    ) 
    ;WITH 
        DateSeries 
        AS 
        ( 
            -- Days in a month     
                                                    SELECT DATEADD(DAY, v.number, DATEADD(MONTH, DATEDIFF(MONTH, 0, @EndDate), 0)) AS PeriodStart 
                FROM master..spt_values v 
                WHERE v.type = 'P' 
                    AND v.number < DAY(EOMONTH(@EndDate)) 
                    AND @ActiveUserPeriodType = 'months' 
 
            UNION ALL 
 
                -- Weeks in a quarter     
                SELECT DATEADD(WEEK, v.number, DATEADD(QUARTER, DATEDIFF(QUARTER, 0, @EndDate), 0)) 
                FROM master..spt_values v 
                WHERE v.type = 'P' 
                    AND v.number < 13 -- ~13 weeks in a quarter     
                    AND @ActiveUserPeriodType = 'quarters' 
 
            UNION ALL 
 
                -- Months in a half-year     
                SELECT DATEADD(MONTH, v.number, DATEADD(MONTH, DATEDIFF(MONTH, 0, DATEADD(MONTH, -5, @EndDate)), 0)) 
                FROM master..spt_values v 
                WHERE v.type = 'P' 
                    AND v.number < 6 
                    AND @ActiveUserPeriodType = 'halfyear' 
 
            UNION ALL 
 
                -- Months in a year     
                SELECT DATEADD(MONTH, v.number, DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(@EndDate), MONTH(@EndDate), 1))) 
                FROM master..spt_values v 
                WHERE v.type = 'P' 
                    AND v.number < 12 
                    AND @ActiveUserPeriodType = 'years' 
        ) 
    SELECT @ActiveUsersReportJson =      
    (     
        SELECT 
            ds.PeriodStart, 
            CASE      
   WHEN @ActiveUserPeriodType = 'months' THEN FORMAT(ds.PeriodStart, 'dd-MMM')     
        WHEN @ActiveUserPeriodType = 'quarters' THEN CONCAT('Week ', DATEPART(WEEK, ds.PeriodStart))     
        WHEN @ActiveUserPeriodType IN ('halfyear', 'years') THEN FORMAT(ds.PeriodStart, 'MMM-yy')     
        END AS PeriodLabel, 
            COUNT(u.Id) AS ActiveUsers 
        FROM DateSeries ds 
            LEFT JOIN Users u 
            ON u.CreatedOn <= ds.PeriodStart 
                AND ISNULL(u.IsDelete, 0) = 0 
                AND ISNULL(u.IsDisabled, 0) = 0 
        GROUP BY ds.PeriodStart 
        ORDER BY ds.PeriodStart 
        FOR JSON PATH     
    ); 
 
 
 
    -- Final report output with all aggregated data          
    SELECT 
        5000000 AS CurrentTargets, 
        ISNULL(@TotalRevenue, 0) AS TotalSales, 
        ISNULL(@TotalApprovedRevenue, 0) AS TotalApprovedAmount, 
        ISNULL(@TotalPendingRevenue, 0) AS TotalPendingAmount, 
        @TotalSalesInCount AS TotalSalesInCount, 
        @TopFiveDealJson AS TopFiveDealJson, 
        @TotalSalesPerDay AS TotalSalesPerDayJson, 
        @hourlyPerformanceJson AS EveryDayPerformanceJson, 
        @TotalSalesPerPerson AS TotalSalesPerPersonJson, 
        @MonthlySalesReport AS MonthlySalesReportJson, 
        @TotalSalesPerService AS TotalSalesPerService , 
        @LeadTypes as  LeadTypes , 
        @LeadStatus as  LeadStatus , 
        @TotalLeads as TotalLeads , 
        @AllocatedLeads as AllocatedLeads , 
        @UnallocatedLeads as UnallocatedLeads, 
        @TotalCustomerCount as TotalCustomerCount, 
        @mostsalesInCityJson as MostSalesInCityJson, 
        @mostPaymentMethod as MostPaymentMethodJson, 
        @prCounts as PrCounts, 
        @salesPerPersonWithPr as SalesPerPersonWithPr, 
        @weeklyPerformanceJson as WeeklyPerformanceJson, 
        @threeMonthPerfOfLgdInUser as ThreeMonthPerfOfLgdInUser, 
        @totalPerfOfLgdInUser as TotalPerfOfLgdInUser, 
        @recentFiveEnquires as RecentFiveEnquires, 
        @MonthlyCustomerDetails as MonthlyCustomerDetails, 
        @EarningFromNewLeads as EarningFromNewLeads, 
        @LeadSoruceEarnings as LeadSoruceEarnings, 
        @ProductRevenueForSalesPersonJson AS ProductRevenueForSalesPerson, 
        @ShowSalesPersonDropDownJson as ShowSalesPersonDropDown, 
        @SubscriptionReportJson AS SubscriptionReport, 
        @ActiveUsersReportJson AS ActiveUsersReportJson 
END      
     
     
     
--  EXEC GetSalesDashboardReport 'bb74d26f-aa28-eb11-bee5-00155d53687a', '2025-06-01 00:00:00.000', '2025-06-30 23:59:59.100', 'months', null, 'halfyear' -- admin     
     
     
--  EXEC GetSalesDashboardReport 'e9c25730-5253-f011-b3fd-edcd820905f7', '2025-06-01 00:00:00.000', '2025-06-30 23:59:59.100', 'months', 'E616912B-A987-EB11-94DB-00155D53687A' -- saleslead     
     
     
-- EXEC 



--MOBILE DASHBOARD 

---------------------------------------------------------------------------------------------         
 
-- CRETED ON 04-07-2025 For Mobile Dashboard Screen -- Ragesh M         
 
-- Altering on 21-07-2025 for enhancement in the mobile dashboard -- Ragesh M      
 
--  exec GetMobileDashboard 'bb74d26f-aa28-eb11-bee5-00155d53687a', '2025-06-01', '2025-06-30', 'months', 'months', 'months', 'months', 'months', 'months', 'months','months', null, 'months'--test           
 
 
--  exec GetMobileDashboard 'da717856-b3da-ee11-813e-00155d23d79c', '2025-06-01', '2025-06-30', 'years', 'months', 'years', 'months', 'years', 'years', 'months' --prod       
---------------------------------------------------------------------------------------------         
GO
ALTER PROCEDURE GetMobileDashboard 
    @LoggedInUser VARCHAR(50), 
    @StartDate DATETIME, 
    @EndDate DATETIME, 
    @ProductPurchasePeriodType VARCHAR(10) = 'months', 
    --primary key     
    @ClientRevenuePeriodType VARCHAR(10) = 'months', 
    --secondary key      
    @ProductRevenuePeriodType VARCHAR(10) = 'months', 
    --third key      
    @FreeTrialPeriodType VARCHAR(10) = 'months', 
    --fourth key     
    @PaymentGatewayPeriodType VARCHAR(10) = 'months', 
    --fifth key          
    @ProductPurchaseRevenuePeriodType VARCHAR(10) = 'months', 
    --sixth key     
    @CouponUsagePeriodType VARCHAR(10) = 'months', 
    --seventh key     
    @ProductRevenueGraphPeriodType VARCHAR(10) = 'months', 
    --eight key     
    @ProductPurchaseRevenueCategory VARCHAR(10) = '', 
    --ninth key      
    @ProductLikePeriodType VARCHAR(10) = 'months', 
    --tenth key     
    @UsersReoportPeriodType VARCHAR(10) = 'months', 
    --eleventh key     
    @RevenueByCityPeriodType VARCHAR(10) = 'months' 
--twelfth key     
-- 'months' | 'quarters' | 'HalfYearly' | 'years'       
AS 
BEGIN 
 
    set @ProductPurchaseRevenueCategory = IIF(@ProductPurchaseRevenueCategory = '', null , @ProductPurchaseRevenueCategory) 
 
    DECLARE @ProductRevenueChartStartDate DATE = @StartDate;-- default to passed StartDate       
    IF @ProductRevenuePeriodType = 'months'       
    SET @ProductRevenueChartStartDate = @StartDate       
    ELSE IF @ProductRevenuePeriodType = 'quarters'       
        SET @ProductRevenueChartStartDate = DATEADD(MONTH, -2, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @ProductRevenuePeriodType = 'HalfYearly'       
        SET @ProductRevenueChartStartDate = DATEADD(MONTH, -5, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @ProductRevenuePeriodType = 'years'       
    SET @ProductRevenueChartStartDate = DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1)); 
 
    DECLARE @ProductPurchaseChartStartDate DATE = @StartDate; 
    IF @ProductPurchasePeriodType = 'months'       
        SET @ProductPurchaseChartStartDate = @StartDate       
    ELSE IF @ProductPurchasePeriodType = 'quarters'       
        SET @ProductPurchaseChartStartDate = DATEADD(MONTH, -2, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @ProductPurchasePeriodType = 'HalfYearly'       
        SET @ProductPurchaseChartStartDate = DATEADD(MONTH, -5, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @ProductPurchasePeriodType = 'years'       
        SET @ProductPurchaseChartStartDate = DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1)); 
 
    DECLARE @ClientRevenueChartStartDate DATE = @StartDate; 
    IF @ClientRevenuePeriodType = 'months'       
        SET @ClientRevenueChartStartDate = @StartDate       
    ELSE IF @ClientRevenuePeriodType = 'quarters'       
        SET @ClientRevenueChartStartDate = DATEADD(MONTH, -2, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @ClientRevenuePeriodType = 'HalfYearly'       
        SET @ClientRevenueChartStartDate = DATEADD(MONTH, -5, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @ClientRevenuePeriodType = 'years'       
        SET @ClientRevenueChartStartDate = DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1)); 
 
    DECLARE @FreeTrialChartStartDate DATE = @StartDate; 
    IF @FreeTrialPeriodType = 'months'       
        SET @FreeTrialChartStartDate = @StartDate       
    ELSE IF @FreeTrialPeriodType = 'quarters'       
        SET @FreeTrialChartStartDate = DATEADD(MONTH, -2, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @FreeTrialPeriodType = 'HalfYearly'       
        SET @FreeTrialChartStartDate = DATEADD(MONTH, -5, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @FreeTrialPeriodType = 'years'       
        SET @FreeTrialChartStartDate = DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1)); 
 
    DECLARE @PaymentGatewayChartStartDate DATE = @StartDate; 
    IF @PaymentGatewayPeriodType = 'months'       
        SET @PaymentGatewayChartStartDate = @StartDate       
    ELSE IF @PaymentGatewayPeriodType = 'quarters'       
        SET @PaymentGatewayChartStartDate = DATEADD(MONTH, -2, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @PaymentGatewayPeriodType = 'HalfYearly'       
        SET @PaymentGatewayChartStartDate = DATEADD(MONTH, -5, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @PaymentGatewayPeriodType = 'years'       
        SET @PaymentGatewayChartStartDate = DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1)); 
 
    DECLARE @ProductPurchaseAmountChartStartDate DATE = @StartDate; 
    IF @ProductPurchaseRevenuePeriodType = 'months'       
        SET @ProductPurchaseAmountChartStartDate = @StartDate       
    ELSE IF @ProductPurchaseRevenuePeriodType = 'quarters'       
        SET @ProductPurchaseAmountChartStartDate = DATEADD(MONTH, -2, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @ProductPurchaseRevenuePeriodType = 'HalfYearly'       
        SET @ProductPurchaseAmountChartStartDate = DATEADD(MONTH, -5, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @ProductPurchaseRevenuePeriodType = 'years'       
        SET @ProductPurchaseAmountChartStartDate = DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1)); 
 
    DECLARE @CouponUsageChartStartDate DATE = @StartDate; 
    IF @CouponUsagePeriodType = 'months'       
        SET @CouponUsageChartStartDate = @StartDate       
    ELSE IF @CouponUsagePeriodType = 'quarters'       
        SET @CouponUsageChartStartDate = DATEADD(MONTH, -2, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @CouponUsagePeriodType = 'HalfYearly'       
        SET @CouponUsageChartStartDate = DATEADD(MONTH, -5, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @CouponUsagePeriodType = 'years'       
        SET @CouponUsageChartStartDate = DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1)); 
 
    DECLARE @ProductRevenueGraphChartStartDate DATE = @StartDate; 
    IF @ProductRevenueGraphPeriodType = 'months'       
        SET @ProductRevenueGraphChartStartDate = @StartDate       
    ELSE IF @ProductRevenueGraphPeriodType = 'quarters'       
        SET @ProductRevenueGraphChartStartDate = DATEADD(MONTH, -2, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @ProductRevenueGraphPeriodType = 'HalfYearly'       
        SET @ProductRevenueGraphChartStartDate = DATEADD(MONTH, -5, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @ProductRevenueGraphPeriodType = 'years'       
        SET @ProductRevenueGraphChartStartDate = DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1)); 
 
    DECLARE @ProductLikeStartDate DATE = @StartDate; 
    IF @ProductLikePeriodType = 'months'       
        SET @ProductLikeStartDate = @StartDate       
    ELSE IF @ProductLikePeriodType = 'quarters'       
        SET @ProductLikeStartDate = DATEADD(MONTH, -2, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @ProductLikePeriodType = 'HalfYearly'       
  SET @ProductLikeStartDate = DATEADD(MONTH, -5, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @ProductLikePeriodType = 'years'       
        SET @ProductLikeStartDate = DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1)); 
 
    DECLARE @UsersReoportStartDate DATE = @StartDate; 
    IF @UsersReoportPeriodType = 'months'       
        SET @UsersReoportStartDate = @StartDate       
    ELSE IF @UsersReoportPeriodType = 'quarters'       
        SET @UsersReoportStartDate = DATEADD(MONTH, -2, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @UsersReoportPeriodType = 'HalfYearly'       
        SET @UsersReoportStartDate = DATEADD(MONTH, -5, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @UsersReoportPeriodType = 'years'       
        SET @UsersReoportStartDate = DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1)); 
 
    DECLARE @RevenueByCityStartDate DATE = @StartDate; 
    IF @RevenueByCityPeriodType = 'months'       
        SET @RevenueByCityStartDate = @StartDate       
    ELSE IF @RevenueByCityPeriodType = 'quarters'       
        SET @RevenueByCityStartDate = DATEADD(MONTH, -2, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @RevenueByCityPeriodType = 'HalfYearly'       
        SET @RevenueByCityStartDate = DATEADD(MONTH, -5, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1));       
    ELSE IF @RevenueByCityPeriodType = 'years'       
        SET @RevenueByCityStartDate = DATEADD(MONTH, -11, DATEFROMPARTS(YEAR(@StartDate), MONTH(@StartDate), 1)); 
 
    SET NOCOUNT ON; 
    INSERT INTO Logs 
    VALUES 
        ( 
            CONVERT(VARCHAR(20), @StartDate, 120) + ' to ' + CONVERT(VARCHAR(20), @EndDate, 120), 
            'exec GetMobileDashboard', 
            GETDATE()           
        ); 
 
 
    DROP TABLE IF EXISTS #MobileUsersTemp; 
 
    DECLARE        
        @MobileUsersCountJson NVARCHAR(MAX),       
        @ProductPurchaseCountJson NVARCHAR(MAX),       
        @ClientWithHighestRevenueJson NVARCHAR(MAX),       
        @ProductWithHighestRevenueJson NVARCHAR(MAX),       
        @FreeTrialReportJson NVARCHAR(MAX),       
        @PaymentGatewayRevenueJson NVARCHAR(MAX),       
        @ProductPurchaseAmountJson NVARCHAR(MAX),      
        @CouponUsageReportJson NVARCHAR(MAX),      
        @ProductRevenueGraphreport NVARCHAR(MAX),     
        @ProductCategoreies NVARCHAR(MAX),     
        @ProductLikeReport NVARCHAR(MAX),     
        @LogsAndExceptionReport NVARCHAR(MAX),     
        @UsersReoport NVARCHAR(MAX),     
        @RevenueByCityReport NVARCHAR(MAX); 
 
    ;WITH 
        androidUsers 
        AS 
        ( 
            SELECT DeviceVersion 
            FROM MobileUsers 
            WHERE ISNULL(IsDelete, 0) = 0 
                AND ISNULL(IsOtpVerified, 0) = 1 
                AND ISNULL(IsActive, 1) = 1 
                AND ISNULL(AgreeToTerms, 0) = 1 
                AND ISNULL(SelfDeleteRequest, 0) = 0 
                AND SelfDeleteReason IS NULL 
                AND LOWER(DeviceType) LIKE '%android%' 
        ), 
        iosUsers 
        AS 
        ( 
            SELECT DeviceVersion 
            FROM MobileUsers 
            WHERE ISNULL(IsDelete, 0) = 0 
                AND ISNULL(IsOtpVerified, 0) = 1 
                AND ISNULL(IsActive, 0) = 1 
                AND ISNULL(AgreeToTerms, 0) = 1 
                AND ISNULL(SelfDeleteRequest, 0) = 0 
                AND SelfDeleteReason IS NULL 
                AND LOWER(DeviceType) LIKE '%ios%' 
        ) 
 
    SELECT @MobileUsersCountJson = (        
        SELECT 
            -- Total Counts        
            (SELECT COUNT(*) 
            FROM androidUsers) AS AndroidUsers, 
            (SELECT COUNT(*) 
            FROM iosUsers) AS IosUsers, 
            (SELECT COUNT(*) 
            FROM (        
                                                       SELECT * 
                    FROM androidUsers 
                UNION ALL 
                    SELECT * 
                    FROM iosUsers        
            ) AS combinedUsers) AS TotalUsers, 
 
            -- Grouped Versions        
            (SELECT top 5 
                DeviceVersion, COUNT(*) AS VersionCount 
            FROM androidUsers 
            GROUP BY DeviceVersion 
            ORDER BY DeviceVersion DESC 
            FOR JSON PATH) AS AndroidVersionBreakdown, 
 
            (SELECT top 5 
                DeviceVersion, COUNT(*) AS VersionCount 
            FROM iosUsers 
            GROUP BY DeviceVersion 
            ORDER BY DeviceVersion DESC 
            FOR JSON PATH) AS IosVersionBreakdown 
 
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER        
    ); 
    WITH 
        DistinctPO 
        AS 
        ( 
            SELECT 
                PO.Id, 
                PO.PaidAmount, 
                PO.Product, 
                PO.ProductId, 
                PO.StartDate, 
                PO.CreatedBy, 
                PO.ModeOfPayment, 
                PO.Status, 
                PO.TransasctionReference, 
                PO.IsActive 
            FROM PurchaseOrdersM AS PO 
                LEFT JOIN Users AS CreatedUser ON PO.CreatedBy = CreatedUser.PublicKey 
                LEFT JOIN PaymentModes AS pm ON PO.ModeOfPayment = pm.Id 
                LEFT JOIN ProductsM AS pd ON pd.Id = PO.ProductId 
                LEFT JOIN Status AS st ON st.Id = PO.Status 
                LEFT JOIN PaymentRequestStatusM AS PR ON PR.TransactionId = PO.TransasctionReference 
                LEFT JOIN PhonePePaymentResponseM AS PP ON PR.TransactionId = PP.MerchanttransactionId 
            WHERE  PO.IsActive <> 0 
                AND CAST(PO.PaymentDate AS DATE) BETWEEN CAST(@ProductPurchaseChartStartDate AS DATE) AND CAST(@EndDate AS DATE) 
        ) 
 
    SELECT @ProductPurchaseCountJson = ( 
    SELECT 
            dpo.ProductId, 
            p.Name as Product, 
            COUNT(dpo.ProductId) AS Count 
        FROM DistinctPO dpo 
        INNER JOIN ProductsM as p on dpo.ProductId = p.Id 
        GROUP BY dpo.ProductId, p.Name 
        ORDER BY Count DESC 
        FOR JSON PATH       
    ); 
 
    -- Active Purchase Orders in selected period     
    WITH 
        ActivePurchases 
        AS 
        ( 
            SELECT DISTINCT po.Mobile, po.Product 
            FROM PurchaseOrdersM po 
                INNER JOIN MobileUsers m on m.Mobile = po.Mobile 
            WHERE      
                po.PaymentDate BETWEEN CAST(@ProductPurchaseChartStartDate AS DATE) AND CAST(@EndDate AS DATE) 
                AND ISNULL(po.IsActive, 1) = 1 AND ISNULL(IsDelete, 0) = 0 AND ISNULL(IsOtpVerified, 0) = 1 
        ), 
        -- All Purchase Orders for reference (total users)     
        AllPurchases 
        AS 
        ( 
            SELECT DISTINCT po.Mobile, po.Product 
            FROM PurchaseOrdersM po 
                INNER JOIN MobileUsers m on m.Mobile = po.Mobile 
            WHERE      
                po.PaymentDate BETWEEN CAST(@ProductPurchaseChartStartDate AS DATE) AND CAST(@EndDate AS DATE) 
        ), 
        -- Revenue per Product in time range     
        RevenuePerProduct 
        AS 
        ( 
            SELECT 
                po.Product, 
                SUM(ISNULL(po.PaidAmount, 0)) AS TotalPaidAmount 
            FROM PurchaseOrdersM po 
            WHERE      
                po.PaymentDate BETWEEN CAST(@ProductPurchaseChartStartDate AS DATE) AND CAST(@EndDate AS DATE) 
                AND ISNULL(po.IsActive, 1) = 1 
            GROUP BY po.Product 
        ) , 
        LikesPerProduct 
        AS 
        ( 
            SELECT p.Name, COUNT(1)AS TotalLikes 
   FROM ProductLikesM pm 
                INNER JOIN ProductsM p on pm.ProductId = p.Id 
            WHERE      
        pm.CreatedOn BETWEEN @ProductPurchaseChartStartDate AND @EndDate 
            GROUP BY p.Name 
        ) 
 
 
    SELECT @ProductPurchaseAmountJson = (       
             
        SELECT 
            rpp.Product, 
            rpp.TotalPaidAmount, 
            COUNT(DISTINCT ap.Mobile) AS ActiveUsers, 
            COUNT(DISTINCT ap_all.Mobile) AS TotalUsers, 
            COUNT(DISTINCT ap_all.Mobile) - COUNT(DISTINCT ap.Mobile) AS InActiveUsers, 
            ISNULL(lpp.TotalLikes, 0) AS TotalLikes 
        FROM RevenuePerProduct rpp 
            LEFT JOIN ActivePurchases ap ON ap.Product = rpp.Product 
            LEFT JOIN AllPurchases ap_all ON ap_all.Product = rpp.Product 
            LEFT JOIN LikesPerProduct lpp ON lpp.Name = rpp.Product 
        GROUP BY rpp.Product, lpp.Name, rpp.TotalPaidAmount, lpp.TotalLikes 
        ORDER BY rpp.TotalPaidAmount DESC 
        FOR JSON PATH       
    ); 
 
    SELECT @ClientWithHighestRevenueJson = (       
        SELECT 
            po.ClientName, 
            MAX(po.PaymentDate) AS LatestPaymentDate, 
            SUM(ISNULL(po.PaidAmount, 0)) AS TotalPaidAmount 
        FROM PurchaseOrdersM po 
        WHERE          
            po.PaymentDate BETWEEN CAST(@ClientRevenueChartStartDate AS DATE) AND CAST(@EndDate AS DATE) 
            AND ISNULL(po.IsActive, 1) = 1      
        GROUP BY po.ClientName 
        ORDER BY TotalPaidAmount DESC 
        FOR JSON PATH       
    ); 
 
    SELECT @ProductWithHighestRevenueJson = (       
        SELECT 
            po.Product, 
            MAX(po.PaymentDate) AS LatestPaymentDate, 
            SUM(ISNULL(po.PaidAmount, 0)) AS TotalPaidAmount 
        FROM PurchaseOrdersM po 
        WHERE          
            po.PaymentDate BETWEEN CAST(@ProductRevenueChartStartDate AS DATE) AND CAST(@EndDate AS DATE) 
          AND ISNULL(po.IsActive, 1) = 1       
        GROUP BY po.Product 
        ORDER BY TotalPaidAmount DESC 
        FOR JSON PATH       
    ); 
 
    ;WITH 
        FreeTrialUsers 
        AS 
        ( 
            SELECT 
                MU.FullName, 
                COALESCE(FM.IsActive, 0) AS IsActive, 
                FM.EndDate 
            FROM MobileUsers MU 
                LEFT JOIN FreeTrialM FM ON MU.PublicKey = FM.MobileUserKey 
                LEFT JOIN FreeTrialBasketM FTB ON FM.FreeTrialBasketId = FTB.Id 
                LEFT JOIN PurchaseOrdersM AS po ON po.Mobile = MU.Mobile AND FM.CreatedOn = po.CreatedOn 
            WHERE          
            (MU.RegistrationDate BETWEEN CAST(@FreeTrialChartStartDate AS DATE) AND CAST(@EndDate AS DATE)) 
        ) 
    SELECT @FreeTrialReportJson = (       
        SELECT 
            COUNT(1) AS Total, 
            SUM(CASE        
                WHEN FM.MobileUserKey IS NULL THEN 1       
                ELSE 0       
            END) AS InActive, 
            SUM(CASE        
                WHEN FM.MobileUserKey IS NOT NULL AND DATEDIFF(DAY, FM.EndDate, GETDATE()) < 0 THEN 1       
                ELSE 0       
            END) AS Active, 
            SUM(CASE        
                WHEN FM.MobileUserKey IS NOT NULL AND DATEDIFF(DAY, FM.EndDate, GETDATE()) >= 0 THEN 1       
                ELSE 0       
            END) AS Expired 
        FROM MobileUsers MU 
            LEFT JOIN FreeTrialM FM ON MU.PublicKey = FM.MobileUserKey 
        WHERE (@StartDate IS NULL OR @EndDate IS NULL 
            OR CAST(MU.RegistrationDate AS DATE)        
             BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE)) 
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER       
    ); 
 
    SELECT @PaymentGatewayRevenueJson = (       
            SELECT 
            UPPER(ISNULL(PPR.PaymentInstrumentType, 'CASHFREE')) AS PaymentGateway, 
            SUM(       
                    COALESCE(       
                        CASE        
WHEN PPR.Code IN ('PAYMENT_SUCCESS', 'SUCCESS') AND PPR.Code <> 'PENDING' THEN PPR.Amount        
                            WHEN PRS.Status IN ('PAYMENT_SUCCESS', 'SUCCESS') AND PRS.Status <> 'PENDING' THEN PRS.Amount        
                            ELSE NULL        
                        END, 0)       
                ) AS Revenue 
        FROM PaymentRequestStatusM PRS 
            INNER JOIN SubscriptionMappingM as sm on prs.SubscriptionMappingId = sm.Id 
            INNER JOIN SubscriptionDurationM as sd on sm.SubscriptionDurationId = sd.Id 
            LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.TransactionId 
            LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey 
            LEFT JOIN ProductsM P ON PRS.ProductId = P.Id 
            LEFT JOIN PurchaseOrdersM as po on po.TransactionId = prs.TransactionId 
        WHERE        
                (       
                    (PPR.Code IN ('PAYMENT_SUCCESS', 'SUCCESS') AND PPR.Code <> 'PENDING') 
            OR 
            (PRS.Status IN ('PAYMENT_SUCCESS', 'SUCCESS') AND PRS.Status <> 'PENDING')       
                ) 
            AND PRS.CreatedOn BETWEEN CAST(@PaymentGatewayChartStartDate AS DATE) AND CAST(@EndDate AS DATE) 
            AND ISNULL(po.IsActive, 1) = 1       
        GROUP BY UPPER(ISNULL(PPR.PaymentInstrumentType, 'CASHFREE')) 
        FOR JSON PATH     
    ); 
 
    SELECT @CouponUsageReportJson = (      
        SELECT 
            cm.Name AS CouponName, 
            COUNT(DISTINCT pm.ActionBy) AS UniqueUserCount, 
            COUNT(pm.ActionBy) AS TotalUsageCount 
        FROM couponsm AS cm 
            LEFT JOIN purchaseordersm AS pm 
            ON pm.couponkey = cm.publickey 
        WHERE       
        ISNULL(cm.IsActive, 1) = 1 
            AND ISNULL(cm.IsDelete, 0) = 0 
            AND ISNULL(pm.IsActive, 1) = 1 
            AND ISNULL(pm.KycApproved, 0) = 1 
            AND pm.PaymentDate BETWEEN CAST(@CouponUsageChartStartDate AS DATE) AND CAST(@EndDate AS DATE) 
        GROUP BY cm.Name 
        FOR JSON AUTO      
    ); 
 
    SELECT @ProductRevenueGraphreport = (       
        SELECT 
            po.Product, 
            MAX(po.PaymentDate) AS LatestPaymentDate, 
            SUM(ISNULL(po.PaidAmount, 0)) AS TotalPaidAmount 
        FROM PurchaseOrdersM po 
        WHERE          
             po.PaymentDate BETWEEN CAST(@ProductRevenueGraphChartStartDate as DATE) AND CAST(@EndDate as DATE) 
             AND ISNULL(po.IsActive, 1) = 1       
        GROUP BY po.Product 
        ORDER BY TotalPaidAmount desc 
        FOR JSON PATH       
    ); 
 
    SELECT @ProductCategoreies = (     
        SELECT DISTINCT c.name 
        FROM ProductsM p 
            inner join ProductCategoriesM c on p.CategoryID = c.Id 
        FOR JSON AUTO     
    ) 
 
    SELECT @ProductLikeReport = (     
        SELECT pm.Name , COUNT(1) as Likes 
        FROM ProductsM as pm 
            inner join ProductLikesM as plm 
            on pm.Id = plm.ProductId 
        WHERE plm.CreatedOn BETWEEN CAST(@ProductLikeStartDate as DATE) AND CAST(@EndDate as DATE) and ISNULL(plm.IsDelete, 0) = 0 and ISNULL(pm.IsActive, 1) = 1 AND ISNULL(pm.IsDeleted , 0) = 0 
        GROUP BY pm.Name, pm.Id 
        order by likes desc 
        FOR JSON AUTO     
    ) 
 
    select @UsersReoport =     
    (     
        SELECT 
            COUNT(*) AS TotalUsers, 
            COUNT(CASE WHEN IsOtpVerified = 1 THEN 1 END) AS OtpVerifiedUsers, 
            COUNT(CASE WHEN ISNULL(IsOtpVerified, 0) = 0 THEN 1 END) AS OtpNotVerifiedUsers, 
            COUNT(CASE      
              WHEN SelfDeleteRequest IS NOT NULL 
                OR SelfDeleteRequestDate IS NOT NULL 
                OR SelfDeleteReason IS NOT NULL 
                OR ISNULL(IsDelete, 0) =1     
              THEN 1      
         END) AS SelfDeleteRequestedUsers 
        FROM MobileUsers 
        WHERE CreatedOn BETWEEN CAST(@UsersReoportStartDate as DATE) AND CAST(@EndDate as DATE) 
        FOR JSON AUTO     
    ) 
 
    SELECT @RevenueByCityReport=(     
        select City, COUNT(1) AS PurchasesCount, SUM(ISNULL(PaidAmount, 0)) AS TotalPaidAmount 
        from PurchaseOrdersM 
        WHERE PaymentDate BETWEEN CAST(@RevenueByCityStartDate as DATE) AND CAST(@EndDate as DATE) 
        GROUP BY City 
        ORDER BY PurchasesCount desc 
 
        FOR JSON AUTO     
    ) 
 
    SELECT 
        @MobileUsersCountJson AS MobileUserCount, 
        @ProductPurchaseCountJson AS ProductPurchaseCount, 
        @ClientWithHighestRevenueJson AS ClientWithHighestRevenue, 
        @ProductWithHighestRevenueJson AS ProductWithHighestRevenue, 
        @FreeTrialReportJson AS FreeTrialReport, 
        @PaymentGatewayRevenueJson AS PaymentGatewayRevenue, 
        @ProductPurchaseAmountJson AS ProductPurchaseRevenue, 
        @CouponUsageReportJson AS CouponUsageReport, 
        @ProductRevenueGraphreport AS ProductRevenueGraphreport, 
        @ProductCategoreies AS ProductCategoreies, 
        @ProductLikeReport AS ProductLikeReport, 
        @UsersReoport AS UsersReoport, 
        @RevenueByCityReport AS RevenueByCityReport 
END       
     
     
     
--  exec GetMobileDashboard 'bb74d26f-aa28-eb11-bee5-00155d53687a', '2025-04-01T00:00:00.000Z', '2025-06-30T00:00:00.000Z', 'months', 'months', 'months', 'months', 'months', 'months', 'months','months', null, 'years' --test 


--- Dashboard
----------------------------------------------------------------------------------------------------------------------------------------------------    
 
-- 01 July 2025 -- Ragesh M  -- Changes to get the total Partners    
 
----------------------------------------------------------------------------------------------------------------------------------------------------    
 GO
ALTER PROCEDURE [dbo].[GetDashboard] 
AS BEGIN 
    DROP TABLE IF EXISTS #PartnerAccountsTemp 
 
    SELECT pa.[Status] as paStatus, pad.StatusId as padStatus 
    INTO #PartnerAccountsTemp 
    from PartnerAccounts as pa 
        INNER JOIN 
        PartnerAccountDetails pad ON pad.PartnerAccountId = pa.Id 
        LEFT JOIN 
        Users us ON us.Id = pa.AssignedTo 
        LEFT JOIN 
        Users us1 ON us1.PublicKey = pad.CreatedBy 
        LEFT JOIN 
        Users us2 ON us2.PublicKey = pad.ModifiedBy 
    Where ISNULL(pa.IsDelete, 0) = 0 
        AND ISNULL(pa.IsDisabled, 0) = 0 
 
    DECLARE @FreshPartners int, @PendingPartners int , @AcceptedPartners int, @RejectedPartners int, @totalPartners int 
 
    SELECT @FreshPartners    = count(1) 
    FROM #PartnerAccountsTemp 
    WHERE COALESCE(padStatus, 0) = 0 -- Fresh      
    SELECT @PendingPartners  = count(1) 
    FROM #PartnerAccountsTemp 
    WHERE COALESCE(padStatus, 0)  = 1 
    -- Pending     
    SELECT @AcceptedPartners = count(1) 
    FROM #PartnerAccountsTemp 
    WHERE COALESCE(padStatus, 0)  = 2 
    -- Accepted      
    SELECT @RejectedPartners = count(1) 
    FROM #PartnerAccountsTemp 
    WHERE COALESCE(padStatus, 0)  = 3 
    -- Rejected    
    SELECT @totalPartners    = count(1) 
    FROM #PartnerAccountsTemp 
    -- Total Partners    
 
 
    select @FreshPartners as FreshPartners   , @PendingPartners as PendingPartners , @AcceptedPartners as AcceptedPartners , 
        @RejectedPartners as RejectedPartners , @totalPartners as TotalPartners 
 
 
END 




---- CHANGES BY AJITH
GO
ALTER PROCEDURE [dbo].[GetPhonePe]       
    @IsPaging INT = 0,       
    @PageSize INT = 5,       
    @PageNumber INT = 1,       
    @SortExpression VARCHAR(50) = 'CreatedOn',       
    -- Default sort column                              
    @SortOrder VARCHAR(4) = 'DESC',       
    -- Default sort order                             
    @RequestedBy VARCHAR(50) = NULL,       
    @FromDate DATETIME = NULL,       
    @ToDate DATETIME = NULL,       
    @SearchText VARCHAR(250) = NULL,       
    @PrimaryKey VARCHAR(50) = NULL,       
    -- Used to filter by Status                             
    @SecondaryKey INT = NULL,       
    -- Used to filter by Product ID                         
    @TotalCount INT = 0 OUTPUT,       
    -- Total number of records                         
    @TotalPaidAmount DECIMAL(18, 2) = 0 OUTPUT       
-- Total PaidAmount                         
AS                      
BEGIN       
    -- Step 1: Calculate total count and total paid amount                         
    SELECT       
        @TotalCount = COUNT(1),       
        @TotalPaidAmount = SUM(COALESCE(       
                    CASE WHEN PPR.Code IN ('PAYMENT_SUCCESS', 'SUCCESS','Credit') AND PPR.Code <> 'PENDING' THEN PPR.Amount ELSE NULL END,        
                    CASE WHEN PRS.Status IN ('PAYMENT_SUCCESS', 'SUCCESS', 'Credit') AND PRS.Status <> 'PENDING' THEN PRS.Amount ELSE NULL END,       
                    0       
                ))       
    FROM PaymentRequestStatusM PRS       
        INNER JOIN SubscriptionMappingM as sm on prs.SubscriptionMappingId = sm.Id       
        INNER JOIN SubscriptionDurationM as sd on sm.SubscriptionDurationId = sd.Id       
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.TransactionId       
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey       
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id       
        LEFT JOIN PurchaseOrdersM as po on po.TransactionId = prs.TransactionId      
    WHERE                               
            (@SearchText IS NULL OR MU.FullName LIKE '%' + @SearchText + '%' OR CAST(MU.Mobile AS VARCHAR) = @SearchText)       
        AND (@FromDate IS NULL OR PRS.CreatedOn >= @FromDate)       
        AND (@ToDate IS NULL OR PRS.CreatedOn <= @ToDate)       
        AND ( @PrimaryKey IS NULL OR (@PrimaryKey IN ('PAYMENT_SUCCESS', 'SUCCESS', 'Credit') AND PRS.Status IN ('PAYMENT_SUCCESS', 'SUCCESS', 'Credit')) OR PRS.Status = @PrimaryKey)       
        AND (@SecondaryKey IS NULL OR PRS.ProductId = @SecondaryKey);       
    -- Filter by Product ID                         
       
    -- Step 2: Query to fetch the data with pagination and sorting                              
    WITH       
        CTE_PhonePe       
        AS       
        (       
            SELECT       
                MU.FullName,       
                P.Name AS ProductName,       
                MU.Mobile,       
                PRS.Amount AS RequestAmount,       
                COALESCE(    
                    CASE WHEN PPR.Code IN ('PAYMENT_SUCCESS', 'SUCCESS', 'Credit') AND PPR.Code <> 'PENDING' THEN PPR.TransactionId ELSE NULL END,    
                    CASE WHEN PRS.Status IN ('PAYMENT_SUCCESS', 'SUCCESS', 'Credit') AND PRS.Status <> 'PENDING' THEN PRS.TransactionId ELSE NULL END,     
                    '') as TransactionId,       
                PRS.CreatedOn,       
                REPLACE(PRS.Status, 'PAYMENT_SUCCESS', 'SUCCESS') AS Status,       
                COALESCE(       
                    CASE WHEN PPR.Code IN ('PAYMENT_SUCCESS', 'SUCCESS', 'Credit') AND PPR.Code <> 'PENDING' THEN PPR.Amount ELSE NULL END,        
                    CASE WHEN PRS.Status IN ('PAYMENT_SUCCESS', 'SUCCESS', 'Credit') AND PRS.Status <> 'PENDING' THEN PRS.Amount ELSE NULL END,       
                    0       
                ) AS PaidAmount,       
                sd.Name AS Duration, -- Static placeholder             
                ISNULL(NULLIF(LTRIM(RTRIM(prs.CouponCode)), ''), 'N/A') AS CouponCode,       
                ISNULL(PPR.PaymentInstrumentType, 'CASHFREE') PaymentInstrumentType,       
                COALESCE(    
                    CASE WHEN PPR.Code IN ('PAYMENT_SUCCESS', 'SUCCESS', 'Credit') AND PPR.Code <> 'PENDING' THEN PPR.MerchantTransactionId ELSE NULL END,    
                    CASE WHEN PRS.Status IN ('PAYMENT_SUCCESS', 'SUCCESS', 'Credit') AND PRS.Status <> 'PENDING' THEN PRS.TransactionId ELSE NULL END,     
                    '') as MerchantTransactionId,      
                MU.PublicKey,       
                ROW_NUMBER() OVER (ORDER BY                        
                CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN PRS.CreatedOn END ASC,                              
                CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN PRS.CreatedOn END DESC,                              
                CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'ASC' THEN MU.FullName END ASC,                              
                CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'DESC' THEN MU.FullName END DESC                              
            ) AS RowNum       
            FROM PaymentRequestStatusM PRS       
                INNER JOIN SubscriptionMappingM as sm on prs.SubscriptionMappingId = sm.Id       
                INNER JOIN SubscriptionDurationM as sd on sm.SubscriptionDurationId = sd.Id       
                LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.TransactionId       
                LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey       
                LEFT JOIN ProductsM P ON PRS.ProductId = P.Id       
                LEFT JOIN PurchaseOrdersM as po on po.TransactionId = prs.TransactionId      
            WHERE                               
            (@SearchText IS NULL OR MU.FullName LIKE '%' + @SearchText + '%' OR CAST(MU.Mobile AS VARCHAR) = @SearchText)       
                AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))       
                AND ( @PrimaryKey IS NULL OR (@PrimaryKey IN ('PAYMENT_SUCCESS', 'SUCCESS', 'Credit') AND PRS.Status IN ('PAYMENT_SUCCESS', 'SUCCESS', 'Credit')) OR PRS.Status = @PrimaryKey)       
                AND (@SecondaryKey IS NULL OR PRS.ProductId = @SecondaryKey)       
            -- Filter by Product ID                         
        )       
       
    SELECT *       
    FROM CTE_PhonePe       
    WHERE @IsPaging = 0       
        OR RowNum BETWEEN (@PageNumber - 1) * @PageSize + 1 AND @PageNumber * @PageSize       
    ORDER BY                         
        CASE WHEN @SortExpression = 'CreatedOn' THEN CreatedOn END DESC;       
-- Default ordering                         
END;                       
                       
  
  --CREATED BY SIVA 12 AUGUST 2025 5:52 PM ---------
  GO
  CREATE TABLE [dbo].[ReasonChangePurchase] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [PurchaseOrderId] INT NOT NULL,
    [Reason] NVARCHAR(500) NULL,
    [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
    [CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [ProductId] INT NOT NULL,
    [Product] NVARCHAR(200) NOT NULL,
    [NetAmount] DECIMAL(18, 2) NOT NULL,
    [PaidAmount] DECIMAL(18, 2) NOT NULL,
    [StartDate] DATE NOT NULL,
    [EndDate] DATE NOT NULL,

    CONSTRAINT FK_ReasonChangePurchase_PurchaseOrdersM
        FOREIGN KEY (PurchaseOrderId) REFERENCES PurchaseOrdersM(Id)
);

  --CREATED BY SIVA 19 AUGUST 2025 6:52 PM ---------
GO
ALTER PROCEDURE Sp_GetPartnerAccountsWithComments
    @IsPaging BIT = 1,                
    @PageSize INT = 20,                
    @PageNumber INT = 1,
    @FromDate DATE = NULL,
    @ToDate   DATE = NULL,
    @SearchText VARCHAR(200) = NULL, 
    @product_count INT OUTPUT          
AS
BEGIN
    SET NOCOUNT ON;

    -- Count total records
    SELECT @product_count = COUNT(*)          
    FROM PartnerAccounts pa          
    INNER JOIN PartnerComments pc ON pa.Id = pc.PartnerId         
    WHERE  
        (@FromDate IS NULL OR CAST(pc.CreatedOn AS DATE) >= @FromDate)
        AND (@ToDate IS NULL OR CAST(pc.CreatedOn AS DATE) <= @ToDate)
        AND (@SearchText IS NULL OR pa.FullName LIKE '%' + @SearchText + '%' OR pa.MobileNumber LIKE '%' + @SearchText + '%');

     ;WITH CTE AS (          
        SELECT          
  ROW_NUMBER() OVER (ORDER BY  pc.CreatedOn DESC) AS SlNo, 
        pa.Id,
        pa.PublicKey,
        pa.FullName,
        pa.MobileNumber,
        pa.EmailId,
        pa.City,
        pc.Comments AS PartnerComment,
        pc.CreatedOn AS CommentCreatedOn,
        usCreated.FirstName + ' ' + usCreated.LastName AS CommentCreatedBy,
        pa.Remarks
    FROM PartnerAccounts pa
    INNER JOIN PartnerComments pc ON pa.Id = pc.PartnerId
    LEFT JOIN Users usCreated ON usCreated.Id = pc.CreatedBy
    WHERE 
        (@FromDate IS NULL OR CAST(pc.CreatedOn AS DATE) >= @FromDate)
        AND (@ToDate IS NULL OR CAST(pc.CreatedOn AS DATE) <= @ToDate)
        AND (@SearchText IS NULL OR pa.FullName LIKE '%' + @SearchText + '%' OR pa.MobileNumber LIKE '%' + @SearchText + '%')
		)
    SELECT *      
FROM CTE      
ORDER BY SlNo      
OFFSET (@PageNumber - 1) * @PageSize ROWS      
FETCH NEXT @PageSize ROWS ONLY;          
END 
  --CREATED BY SIVA 19 AUGUST 2025 6:52 PM ---------


GO
ALTER PROCEDURE GetPartnerAccountStatusCount                  
         
	@FromDate DATE = NULL,                  
    @ToDate DATE = NULL,  
    @AssignedTo INT = 0,         
    @PartnerName NVARCHAR(100) = NULL                  
AS                  
BEGIN                  
    SET NOCOUNT ON;                  
    
    --SET @FromDate = ISNULL(@FromDate, '1900-01-01');                  
    --SET @ToDate = ISNULL(@ToDate, GETDATE());    
    --SET @ToDate = DATEADD(DAY, 1, @ToDate); -- include end of day    
    
    SELECT                    
         '' AS Partner,    
    
        CASE                  
            WHEN pad.StatusId = 0 THEN 'Fresh'                  
            WHEN pad.StatusId = 1 THEN 'Pending'                  
            WHEN pad.StatusId = 2 THEN 'Accepted'                  
            WHEN pad.StatusId = 3 THEN 'Rejected'                  
            WHEN pad.StatusId= 4 THEN 'Followup'                  
            WHEN pad.StatusId= 5 THEN 'Not Interested'                  
            WHEN pad.StatusId = 6 THEN 'NPC'        
		    WHEN pad.StatusId = 7 THEN 'Linked to Jarvis'                                   
		    WHEN pad.StatusId = 8 THEN 'Insufficent Funds'                               
			 WHEN pad.StatusId = 9 THEN 'Signature Mismatch'                   
			WHEN pad.StatusId = 10 THEN 'Invalid API Keys'   
		   WHEN pad.StatusId = 11 THEN 'Link Shared'   
  
            ELSE 'Unknown'                    
        END AS StatusName,                  
    
        COUNT(*) AS StatusCount                  
    
    FROM PartnerAccounts pa            
    INNER JOIN PartnerAccountDetails pad ON pa.Id = pad.PartnerAccountId        
 LEFT JOIN Users us ON us.Id = pad.AssignedTo         
 LEFT JOIN             
 Users us2 ON us2.PublicKey = pad.ModifiedBy   
          
    WHERE     
           ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, pad.ModifiedOn)), ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999))            
            BETWEEN ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999)            
                AND ISNULL(@ToDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))     
 AND (ISNULL(@PartnerName, '') = '' OR (LOWER(@PartnerName) = LOWER(ISNULL(pad.PartnerCode, '')))) 
 AND ISNULL(pad.[AssignedTo], 0) = IIF(@AssignedTo = 0, ISNULL(pad.[AssignedTo], 0), @AssignedTo)            
    AND ISNULL(pa.isdelete, 0) = 0   
    
    GROUP BY     
             
        pad.StatusId               
    
    ORDER BY     
        pad.StatusId;                  
END 
--CREATED BY SIVA 19 AUGUST 2025 6:52 PM ---------

Go
ALTER PROCEDURE [dbo].[GetPartnerAccountsSummaryReport]            
    -- DECLARE       
    @PartnerName NVARCHAR(100) = NULL  ,                
    @AssignedTo INT = 0,           
    @FromDate DATE = NULL,           
    @ToDate DATE = NULL,           
    @BenchMark INT OUTPUT           
AS           
BEGIN           
    SET @BenchMark = 10;           
           
    SET @FromDate = ISNULL(@FromDate, DATEADD(DAY, -60, GETDATE()));           
    SET @ToDate = ISNULL(@ToDate, GETDATE());           
    SET @ToDate = DATEADD(DAY, 1, @ToDate);           
           
    ;WITH CTE AS (           
        SELECT TOP (DATEDIFF(DAY, @FromDate, @ToDate) + 1)           
            Date = DATEADD(DAY, ROW_NUMBER() OVER (ORDER BY a.object_id) - 1, @FromDate)           
        FROM sys.all_objects a CROSS JOIN sys.all_objects b           
    ),           
   GetTotalRegistration AS    
(    
    SELECT     
        CAST(pad.CreatedOn AS DATE) AS DateOn,    
        COUNT(1) AS TotalRegistration    
    FROM PartnerAccountDetails pad    
    INNER JOIN PartnerAccounts pa ON pad.PartnerAccountId = pa.Id    
    WHERE CAST(pad.CreatedOn AS DATE) BETWEEN @FromDate AND @ToDate    
      AND ISNULL(pa.IsDelete, 0) = 0  
	  AND (ISNULL(@PartnerName, '') = '' OR (LOWER(@PartnerName) = LOWER(ISNULL(pad.PartnerCode, '')))) 
      AND (@AssignedTo = 0 OR pa.AssignedTo = @AssignedTo)    
    GROUP BY CAST(pad.CreatedOn AS DATE)    
)    
,           
    GetTotalModification AS (           
        SELECT CAST(pad.ModifiedOn AS DATE) AS DateOn, COUNT(1) AS TotalModification           
        FROM PartnerAccountDetails pad           
        INNER JOIN PartnerAccounts pa ON pad.PartnerAccountId = pa.Id           
        WHERE ISNULL(DATEADD(DAY, 0, DATEDIFF(DAY, 0, pad.ModifiedOn)), @FromDate)            
              BETWEEN @FromDate AND @ToDate           
          AND ISNULL(pa.IsDelete, 0) = 0    
		  AND (ISNULL(@PartnerName, '') = '' OR (LOWER(@PartnerName) = LOWER(ISNULL(pad.PartnerCode, '')))) 
          AND (@AssignedTo = 0 OR pa.AssignedTo = @AssignedTo)           
        GROUP BY CAST(pad.ModifiedOn AS DATE)           
    )     ,  
  GetTotalConversion AS (           
        SELECT CAST(pad.ModifiedOn AS DATE) AS DateOn, COUNT(1) AS TotalConversion           
        FROM PartnerAccountDetails pad           
        INNER JOIN PartnerAccounts pa ON pad.PartnerAccountId = pa.Id           
        WHERE ISNULL(DATEADD(DAY, 0, DATEDIFF(DAY, 0, pad.ModifiedOn)), @FromDate)            
              BETWEEN @FromDate AND @ToDate           
          AND ISNULL(pa.IsDelete, 0) = 0     
		  AND (ISNULL(@PartnerName, '') = '' OR (LOWER(@PartnerName) = LOWER(ISNULL(pad.PartnerCode, '')))) 
    AND pad.StatusId=7  
          AND (@AssignedTo = 0 OR pa.AssignedTo = @AssignedTo)           
        GROUP BY CAST(pad.ModifiedOn AS DATE)           
    )        
           
    SELECT            
        CAST(CTE.Date AS VARCHAR) AS Date,           
        ISNULL(reg.TotalRegistration, 0) AS TotalRegistration,           
        ISNULL(con.TotalConversion, 0) AS TotalConversion ,  
  ISNULL(mod.TotalModification,0) as TotalModification  
    FROM CTE           
    LEFT JOIN (           
        SELECT DateOn, SUM(TotalRegistration) AS TotalRegistration           
        FROM GetTotalRegistration           
        GROUP BY DateOn           
    ) reg ON CTE.Date = reg.DateOn           
    LEFT JOIN (           
        SELECT DateOn, SUM(TotalConversion) AS TotalConversion           
        FROM GetTotalConversion           
        GROUP BY DateOn           
    ) con ON CTE.Date = con.DateOn     
  LEFT JOIN (           
        SELECT DateOn, SUM(TotalModification) AS TotalModification           
        FROM GetTotalModification           
        GROUP BY DateOn           
    ) mod ON CTE.Date = mod.DateOn     
    WHERE CTE.Date < @ToDate           
    ORDER BY CAST(CTE.Date AS DATE) ASC;           
END 