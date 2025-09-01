--------------- Created by Guna Surya 04/03/2025 10:00:00 AM ---------------	

GO
alter PROCEDURE [dbo].[GetMyBucketContent] @mobileUserKey UNIQUEIDENTIFIER        
AS        
BEGIN        
    DECLARE @priorDaysInfo INT = CAST(        
                                 (        
                                     SELECT TOP 1 value FROM settings WHERE code = 'PRIORDAYSINFO'        
                                 ) AS INT) - 1,        
            @currentDate DATE = getDate()        
        
    SELECT p.Id,        
           p.Name,        
           b.Startdate,        
           b.Enddate,                          
           p.ListImage,        
           p.Price,            
           DATEDIFF(day, getdate(), b.enddate) as DaysToGo,        
           pc.name AS CategoryName,        
           cast((CASE        
                     WHEN pl.productid IS NOT NULL        
                          AND pl.IsDelete = 0 THEN        
                         1                             
                     ELSE        
                         0                            
                 END        
                ) AS BIT) AS IsHeart,        
           cast(CASE        
                    WHEN DATEDIFF(day, GETDATE(), b.enddate) <= @priorDaysInfo THEN        
                        1        
                    ELSE        
                        0        
                END AS BIT) AS ShowReminder,        
        
           -- buy button text                            
           CASE        
               WHEN cast(CASE        
                             WHEN DATEDIFF(day, GETDATE(), b.enddate) <= @priorDaysInfo THEN        
                                 1        
                             ELSE        
                                 0        
                         END AS BIT) = 0        
                    AND GETdATE()        
                    BETWEEN b.startDate AND b.endDate THEN        
                   'Purchased'        
             ELSE   
    CASE   
        WHEN p.isActive = 1    
            AND CAST(  
                CASE   
                    WHEN DATEDIFF(day, GETDATE(), b.enddate) < @priorDaysInfo THEN 1   
                    ELSE 0   
                END AS BIT) = 1   
            AND GETDATE() BETWEEN b.startDate AND b.endDate   
        THEN 'Renew'   
        ELSE   
            CASE   
                WHEN p.isActive = 0 THEN 'Disabled'   
                ELSE 'Renew'   
            END   
    END  
  
           END AS BuyButtonText,        
           Notification as NotificationEnabled        
    FROM MYBUCKETM b        
        INNER JOIN productsm p  ON p.id = b.productid        
        LEFT JOIN productcategoriesm pc  ON pc.id = p.categoryid        
        LEFT JOIN ProductLikesM pl  ON pl.CreatedBy = b.MobileUserKey  AND pl.likeid = 1 AND pl.productid = b.productid        
    WHERE b.mobileuserkey = @mobileUserKey        
          AND b.IsActive = 1      
    ORDER BY p.NAME        
END 

  
Go 
ALTER PROCEDURE [dbo].[GetCustomers]
    @IsPaging BIT = 1,
    @PageSize INT = 25,
    @PageNumber INT = 1,
    @SortExpression VARCHAR(50) = NULL,
    @SortOrder VARCHAR(50) = NULL,
    @RequestedBy VARCHAR(50) = NULL,
    @SearchText VARCHAR(100) = NULL,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @StrategyKey VARCHAR(50) = NULL,
    @SecondaryKey VARCHAR(50) = NULL,
    @TotalCount INT = 0 OUTPUT,
    @TotalAmount INT = 0 OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    SET @IsPaging = COALESCE(@IsPaging, 0);
    SET @PageSize = COALESCE(@PageSize, 20);
    SET @SortExpression = COALESCE(@SortExpression, 'callDate');
    SET @SortOrder = COALESCE(@SortOrder, 'desc');
    SET @SearchText = ISNULL(@SearchText, '');
    
    SELECT 
        @TotalCount = COUNT(1),
        @TotalAmount = ISNULL(SUM(po.PaidAmount), 0)
    FROM Leads l
    INNER JOIN PurchaseOrders po ON l.Id = po.LeadId
    INNER JOIN STATUS s ON po.STATUS = s.Id
    WHERE l.AssignedTo IS NOT NULL
        AND s.Name = 'Customer'
        AND po.CreatedOn = (
            SELECT MAX(CreatedOn)
            FROM PurchaseOrders
            WHERE LeadId = l.Id
            AND STATUS = s.Id
        )
        AND l.AssignedTo = ISNULL(@RequestedBy, l.AssignedTo)
        AND po.paymentDate BETWEEN 
            ISNULL(@FromDate, DATEADD(MONTH, -1, GETDATE())) AND 
            DATEADD(SECOND, -1, DATEADD(DAY, 1, ISNULL(@ToDate, GETDATE())))
        AND (l.FullName LIKE '%' + @SearchText + '%' 
            OR l.MobileNumber LIKE '%' + @SearchText + '%')
        AND ISNULL(po.IsExpired, 0) = 0
        AND (
            (@SecondaryKey IS NOT NULL AND po.ServiceId IN (
                SELECT CAST(value AS INT) 
                FROM STRING_SPLIT(@SecondaryKey, ',')
            )) 
            OR @SecondaryKey IS NULL
        );

    IF OBJECT_ID('tempdb..#tempCustomer') IS NOT NULL
        DROP TABLE #tempCustomer;

    WITH CustomerData AS (
        SELECT 
            ROW_NUMBER() OVER (ORDER BY l.Id DESC) AS slNo,
            l.Id,
            l.PublicKey AS LeadPublicKey,
            l.FullName,
            l.MobileNumber,
            l.EmailId,
            lt.Name AS LeadTypeKey,
            u.FirstName AS AssignedToName,
            ISNULL(po.StartDate, '') AS StartDate,
            ISNULL(po.EndDate, '') AS EndDate,
            po.PaidAmount,
            ISNULL(DATEDIFF(DAY, GETDATE(), po.EndDate), 0) AS DaysToGo,
            po.STATUS,
            s.Name AS PrStatusName,
            l.PurchaseOrderKey,
            po.City,
            po.Remark,
            po.ModeOfPayment,
            po.ServiceId,
            po.paymentDate,
            srv.Name AS ServiceName
        FROM Leads l
        INNER JOIN PurchaseOrders po ON l.Id = po.LeadId
        INNER JOIN Users u ON l.AssignedTo = u.PublicKey
        LEFT JOIN LeadTypes lt ON lt.PublicKey = l.LeadTypeKey
        INNER JOIN STATUS s ON s.Id = po.STATUS
        INNER JOIN Services srv ON po.ServiceId = srv.Id
        WHERE l.AssignedTo IS NOT NULL
            AND s.Name = 'Customer'
            AND po.CreatedOn = (
                SELECT MAX(CreatedOn)
                FROM PurchaseOrders
                WHERE LeadId = l.Id
                AND STATUS = s.Id
            )
            AND l.AssignedTo = ISNULL(@RequestedBy, l.AssignedTo)
            AND po.paymentDate BETWEEN 
                ISNULL(@FromDate, DATEADD(MONTH, -1, GETDATE())) AND 
                DATEADD(SECOND, -1, DATEADD(DAY, 1, ISNULL(@ToDate, GETDATE())))
            AND (l.FullName LIKE '%' + @SearchText + '%' 
                OR l.MobileNumber LIKE '%' + @SearchText + '%')
            AND ISNULL(po.IsExpired, 0) = 0
            AND (
                (@SecondaryKey IS NOT NULL AND po.ServiceId IN (
                    SELECT CAST(value AS INT) 
                    FROM STRING_SPLIT(@SecondaryKey, ',')
                )) 
                OR @SecondaryKey IS NULL
            )
    )
    SELECT *
    INTO #tempCustomer
    FROM CustomerData
    ORDER BY 
        CASE WHEN @SortOrder = 'asc' AND @SortExpression = 'CreatedOn' THEN paymentDate END ASC,
        CASE WHEN @SortOrder = 'asc' AND @SortExpression = 'leadName' THEN FullName END ASC,
        CASE WHEN @SortOrder = 'desc' AND @SortExpression = 'leadName' THEN FullName END DESC
    OFFSET IIF(@PageNumber <= 1, 0, (@PageNumber - 1)) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT (
        SELECT *
        FROM #tempCustomer
        FOR JSON AUTO
    ) AS JsonData;

    DROP TABLE IF EXISTS #tempCustomer;
END;

-- 5 March Ajith
GO
ALTER PROCEDURE GetProductCommunityMappings
    @SearchText NVARCHAR(100) = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @Status NVARCHAR(20) = NULL,  -- Accept status as a parameter
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Normalize @FromDate and @ToDate
    SET @FromDate = COALESCE(@FromDate, '1900-01-01');  -- Default to old date if NULL
    SET @ToDate = COALESCE(@ToDate, GETDATE());  -- Default to today if NULL
    SET @ToDate = DATEADD(DAY, 1, @ToDate);  -- Ensure the @ToDate includes the full day

    -- Count total records matching the criteria
    SELECT @TotalCount = COUNT(*)
    FROM ProductCommunityMappingM pcmm
        LEFT JOIN ProductsM pm ON pcmm.ProductId = pm.Id
        LEFT JOIN ProductCategoriesM pcm ON pm.CategoryID = pcm.Id
        LEFT JOIN Users u ON pcmm.ModifiedBy = u.Id
    WHERE 
        (@SearchText IS NULL OR
        pm.Name LIKE '%' + @SearchText + '%' OR
        pcm.Name LIKE '%' + @SearchText + '%')
        AND pcmm.CreatedDate >= @FromDate
        AND pcmm.CreatedDate < @ToDate
        AND (
            @Status IS NULL OR  -- If Status is NULL, fetch all records
            (@Status = 'ACTIVE' AND pcmm.IsActive = 1 AND pcmm.IsDeleted = 0) OR
            (@Status = 'INACTIVE' AND pcmm.IsActive = 0 AND pcmm.IsDeleted = 0) OR
            (@Status = 'DELETED' AND pcmm.IsDeleted = 1)
        );

    -- Fetch paginated results
    SELECT
        pcmm.Id,
        pcmm.CommunityId,
        pcmm.ProductId,
        pm.Name AS ProductName,
        pcm.Name AS CommunityName,
        pcmm.IsActive,
        pcmm.IsDeleted,
        pcmm.CreatedDate,
        u.FirstName + ' ' + u.LastName AS ModifiedBy,
        pcmm.ModifiedDate,
        u.FirstName + ' ' + u.LastName AS CreatedBy
    FROM ProductCommunityMappingM pcmm
        LEFT JOIN ProductsM pm ON pcmm.ProductId = pm.Id
        LEFT JOIN ProductCategoriesM pcm ON pm.CategoryID = pcm.Id
        LEFT JOIN Users u ON pcmm.ModifiedBy = u.Id
    WHERE 
        (@SearchText IS NULL OR
        pm.Name LIKE '%' + @SearchText + '%' OR
        pcm.Name LIKE '%' + @SearchText + '%')
        AND pcmm.CreatedDate >= @FromDate
        AND pcmm.CreatedDate < @ToDate
        AND (
            @Status IS NULL OR
            (@Status = 'ACTIVE' AND pcmm.IsActive = 1 AND pcmm.IsDeleted = 0) OR
            (@Status = 'INACTIVE' AND pcmm.IsActive = 0 AND pcmm.IsDeleted = 0) OR
            (@Status = 'DELETED' AND pcmm.IsDeleted = 1)
        )
    ORDER BY pcmm.ModifiedDate DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS   
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO



--   getSubscriptionDetails 5 March --Siva
GO
ALTER PROCEDURE GetSubscriptionDetails                    
  @PageSize INT,   
  @PageNumber INT,  
  @SearchText NVARCHAR(255) = NULL,  
  @SubscriptionPlanId INT = NULL,   
  @SubscriptionDurationId INT = NULL,   
 @TotalCount INT OUTPUT                                       
AS                    
BEGIN                    
    SET NOCOUNT ON;                    
                    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;                    
                    
    SELECT                  
        ROW_NUMBER() OVER (ORDER BY ISNULL(sm.ModifiedOn, sm.CreatedOn) DESC) AS SlNo,                 
        sm.Id AS MappingId,                
        p.Id AS ProductId,                     
        p.Name AS ProductName,                 
        sd.Id AS DurationId,                
        sd.Name AS DurationName,                     
        sd.Months AS DurationMonths,                     
        sm.DiscountPercentage,                     
        p.Price AS ProductPrice,                   
        p.Price * sd.Months AS ProductPriceWithDuration,                        
        sd.IsActive AS DurationStatus,                
        sp.Id AS PlanId,                
        sp.Name AS PlanName,                  
        sp.IsActive AS PlanStatus,                    
        sm.IsActive AS MappingStatus,        
  sm.createdon as createdOn,      
   (u.FirstName + ' ' + u.LastName) as  ModifiedBy,        
        ((p.Price * sd.Months) * (1 - (sm.DiscountPercentage / 100.0))) AS DiscountPrice                    
    FROM                     
        SubscriptionMappingM AS sm                    
    INNER JOIN                     
        ProductsM AS p ON sm.ProductId = p.Id                    
    INNER JOIN                     
        SubscriptionPlanM AS sp ON sm.SubscriptionPlanId = sp.Id                    
    INNER JOIN                     
        SubscriptionDurationM AS sd ON sm.SubscriptionDurationId = sd.Id          
 left join users as u on u.PublicKey = sm.ModifiedBy      
    WHERE                     
       (@SearchText IS NULL  
    OR p.Name LIKE '%' + @SearchText + '%' )   
    AND (@SubscriptionPlanId IS NULL OR sp.Id = @SubscriptionPlanId)   
   AND (@SubscriptionDurationId IS NULL OR sd.Id = @SubscriptionDurationId)   
  AND (p.IsDeleted = 0)               
                    
    ORDER BY ISNULL(sm.ModifiedOn, sm.CreatedOn) DESC                    
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;                    
                    
    SELECT @TotalCount = COUNT(1)                       
    FROM                     
        SubscriptionMappingM AS sm                    
    INNER JOIN                     
        ProductsM AS p ON sm.ProductId = p.Id                    
    INNER JOIN                     
        SubscriptionPlanM AS sp ON sm.SubscriptionPlanId = sp.Id                    
    INNER JOIN                     
        SubscriptionDurationM AS sd ON sm.SubscriptionDurationId = sd.Id                    
    WHERE                     
          (@SearchText IS NULL  
    OR p.Name LIKE '%' + @SearchText + '%')  
    AND (@SubscriptionPlanId IS NULL OR sp.Id = @SubscriptionPlanId)   
 AND (@SubscriptionDurationId IS NULL OR sd.Id = @SubscriptionDurationId)               
END; 

-- Ajith 6 March 11:20 AM
GO
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'AdvertisementImageM' AND COLUMN_NAME = 'ProductId')
BEGIN
    ALTER TABLE AdvertisementImageM ADD ProductId INT NULL;
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'AdvertisementImageM' AND COLUMN_NAME = 'ProductName')
BEGIN
    ALTER TABLE AdvertisementImageM ADD ProductName VARCHAR(100) NULL;
END
GO

Go
-- exec [GetCompaniesM] '2a113a88-9ba5-ef11-b32e-82a57052bbcc',1,null, null,null,null 
  
ALTER PROCEDURE [dbo].[GetCompaniesM]   
    @LoggedInUser UNIQUEIDENTIFIER, 
    @BasketId INT, 
    @SearchText VARCHAR(100), 
    @PrimaryKey VARCHAR(100), 
    @SecondaryKey VARCHAR(100), 
    @UserHasResearch BIT = NULL OUTPUT 
AS 
BEGIN 
 
    declare @userHasPurchasedResearch bit = 0 
 
    IF EXISTS 
    ( 
        SELECT 1 
        FROM MyBucketM 
        WHERE MobileUserKey = @LoggedInUser 
              AND ProductId = 
              ( 
                  SELECT Id FROM ProductsM WHERE Code = 'research' 
              ) 
    ) 
    BEGIN 
        SET @userHasPurchasedResearch = 1 
    END 
 
    IF EXISTS 
    ( 
        SELECT 1 
        FROM MyBucketM b 
            JOIN ProductsM p 
                ON b.ProductId = p.Id 
        WHERE b.MobileUserKey = @LoggedInUser 
              AND p.Code = 'research' 
              AND b.EndDate > GETDATE() 
    ) 
    BEGIN 
        SET @UserHasResearch = 1; 
    END 
    ELSE 
    BEGIN 
        SET @UserHasResearch = 0; 
 
    END 
 
 
 
 
 
    SELECT BasketId, 
           p.Id AS ProductId, 
           p.Name AS Productname, 
           pc.GroupName, 
           p.Price, 
           case 
               when IsFree = 1 then 
                   'Read' 
               when @userHasPurchasedResearch = 1 
                    and @UserHasResearch = 1 then 
                   'Read' 
               when @userHasPurchasedResearch = 0 
                    and @UserHasResearch = 0 then 
                   'Unlock' 
               when @userHasPurchasedResearch = 1 
                    and @UserHasResearch = 0 then 
                   'Renew' 
           end as CompanyStatus, 
           CASE 
               WHEN (@UserHasResearch = 1) 
                     THEN 
                   'Buy' 
				    WHEN    (isFree = 1) THEN 'Read' 
					WHEN @userHasPurchasedResearch = 1 
                    and @UserHasResearch = 0 THEN 'Renew' 
               ELSE 
                   'Purchased' 
           END AS BuyButtonText, 
           p.Description AS ProductDescription, 
           ( 
               SELECT ISNULL((CAST(AVG(ISNULL(Rating, 0)) AS DECIMAL(18, 2))), 0) 
               FROM ProductsRatingM 
               WHERE ProductId = p.id 
           ) AS OverallRating, 
           ( 
               SELECT COUNT(1) 
               FROM ProductLikesM 
               WHERE LikeId = 1 
                     AND ProductId = P.ID 
                     AND IsDelete = 0 
           ) AS HeartsCount, 
           ISNULL(prm.Rating, 0.0) AS UserRating, 
           CAST(((CASE 
                      WHEN ISNULL(plm.LikeId, 0) = 1 
                           AND plm.IsDelete = 0 THEN 
                          1 
                      ELSE 
                          0 
                  END 
                 ) 
                ) AS BIT) AS UserHasHeart, 
           CAST((CASE 
                     WHEN ISNULL(MB.Id, 0) > 0 THEN 
                         1 
                     ELSE 
                         0 
                 END 
                ) AS BIT) AS IsInMyBucket, 
           CAST(((CASE 
                      WHEN MB.Id IS NOT NULL 
                           AND MB.MobileUserKey = @LoggedInUser 
                           AND (GETDATE() 
                           BETWEEN MB.startDate AND MB.Enddate 
                               ) THEN 
                          1 
                      ELSE 
                          0 
                  END 
                 ) 
                ) AS BIT) AS IsInValidity, 
           p.ListImage, 
           IsFree, 
           CASE 
               WHEN (@UserHasResearch = 1) 
                    OR (isFree = 1) THEN 
                   c.name 
               ELSE 
                   'Unlock the name' 
           END AS Name, 
           ChartImageUrl, 
           OtherImage, 
           WebsiteUrl, 
           c.CreatedOn, 
           MarketCap, 
           CASE 
             WHEN (@UserHasResearch = 1) 
                    OR (isFree = 1) THEN 
                   ShortSummary 
               ELSE 
                   TrialDescription 
           END AS ShortSummary, 
           pe, 
           c.PublishDate 
    FROM CompanyDetailM c 
        INNER JOIN ProductsM p 
            ON p.Id = 
            ( 
                SELECT id FROM ProductsM WHERE Code = 'RESEARCH' 
            ) 
        LEFT JOIN Categories pc 
            ON pc.Id = p.CategoryID 
        LEFT JOIN ProductsRatingM AS prm 
            ON p.Id = prm.productId 
               and prm.CreatedBy = @LoggedInUser 
        LEFT JOIN ProductLikesM AS plm 
            ON plm.ProductId = p.Id 
               and plm.CreatedBy = @LoggedInUser 
               AND plm.LikeId = 1 
               AND plm.IsDelete = 0 
        LEFT JOIN MYBucketM AS MB 
            ON mb.ProductId = p.Id 
               and mb.MobileUserKey = @LoggedInUser 
               and mb.IsExpired = 0 
    WHERE BasketId = @BasketId AND c.IsActive = 1
          AND IsPublished = 1 
          AND ( 
                  @SearchText IS NULL 
                  OR @SearchText = '' 
                  OR c.Name LIKE '%' + @SearchText + '%' 
              ) 
          AND ( 
                  ( 
                      @PrimaryKey IS NULL 
                      OR @PrimaryKey = '' 
                  ) 
                  OR ( 
                         @PrimaryKey = 'GREATERTHAN500' 
                         AND MarketCap >= 500 
                     ) 
                  OR ( 
                         @PrimaryKey = 'LESSTHAN500' 
                         AND MarketCap < 500 
                     ) 
              ) 
          AND ( 
                  ( 
                      @SecondaryKey IS NULL 
                      OR @SecondaryKey = '' 
                  ) 
                  OR ( 
                         @SecondaryKey = 'LESSTHAN40' 
                         AND PE < 40 
                     ) 
                  OR ( 
                         @SecondaryKey = 'GREATERTHAN40' 
                         AND PE >= 40 
                     ) 
              ) 
    ORDER BY isfree DESC, 
             c.Createdon DESC 
END
Go


GO
ALTER PROCEDURE [dbo].[GetAdImagesm] @type VARCHAR(50)     
 ,@searchText VARCHAR(100)     
AS     
BEGIN     
 SELECT id     
  ,name     
  ,IsActive     
  ,IsDelete     
  ,Url     
  ,Type    
  ,ExpireOn,
  productId,
  productName   
 FROM AdvertisementImageM     
 WHERE (     
   @type IS NULL     
   OR type = @type     
   )     
  AND (     
   @searchText IS NULL     
   OR @searchText = ''     
   OR name LIKE '%' + @searchText + '%'     
   )     
  AND IsDelete = 0     
 ORDER BY createdon     
  ,IsActive DESC     
END 

exec GetAdImagesm 'DASHBOARD', null
GO

GO
ALTER PROCEDURE [dbo].[GetBasketsM] 
AS 
BEGIN 
 
    ;with 
        CompanyCount 
        as 
        ( 
            select basketId, COUNT(basketId) as CompanyCount 
            from CompanyDetailM 
            where IsPublished = 1 and IsActive = 1
            group by BasketId 
        ) 
    select b.Id, b.Title, b.Description, b.IsFree, b.IsActive, b.IsDelete,b.SortOrder, cc.CompanyCount 
    from BasketsM b 
        inner join CompanyCount cc on b.Id = cc.BasketId 
    WHERE  b.IsActive = 1 and b.IsDelete = 0 
END 
GO


--Created By Siva 17 March 2025 ----2:30PM
GO
ALTER PROCEDURE GetAllComplaints    
@IsPaging INT = 1,    
@PageSize INT = 10,                         
@PageNumber INT = 1,    
@TotalCount INT OUTPUT,    
@SearchText VARCHAR(250) = NULL,    
@FromDate VARCHAR(50) = NULL,                            
@ToDate VARCHAR(50) = NULL    
AS    
BEGIN    
    SET NOCOUNT ON;    
  
    -- Ensure PageSize is valid (greater than 0)  
    IF @PageSize <= 0    
    BEGIN    
        SET @PageSize = 10;  -- Default to 10 if invalid  
    END  
  
    -- Get total count of complaints    
    SELECT @TotalCount = COUNT(*)     
    FROM Complaints    
    WHERE     
        (@SearchText IS NULL OR firstName LIKE '%' + @SearchText + '%');  
  
    -- Fetch complaints with pagination    
    IF @IsPaging = 1    
    BEGIN    
        SELECT id,    
            firstName,    
            lastName,    
            email,    
            mobile,    
            images,    
            message,
			status,
			ModifiedOn,
			ModifiedBy,
            createdOn    
        FROM Complaints    
        WHERE     
            (@SearchText IS NULL OR firstName LIKE '%' + @SearchText + '%')   
        ORDER BY createdOn DESC    
        OFFSET ((@PageNumber - 1) * @PageSize) ROWS    
        FETCH NEXT @PageSize ROWS ONLY;    
    END    
    ELSE    
    BEGIN    
        -- Return all records if paging is disabled  
        SELECT id,    
            firstName,    
            lastName,    
            email,    
            mobile,    
            images,    
            message,
			status,
			ModifiedOn,
			ModifiedBy,
            createdOn    
        FROM Complaints    
        WHERE     
            (@SearchText IS NULL OR firstName LIKE '%' + @SearchText + '%')   
        ORDER BY createdOn DESC;    
    END    
END;  
 

--------- created by Guna Surya 12-March-25-------------------------    
GO
alter PROCEDURE SendWhatsappTemplateMessages        
    @TemplateId VARCHAR(50),        
    @TargetType VARCHAR(50),         
    @TemplateName VARCHAR(MAX),        
    @MobileNumbers VARCHAR(MAX) = NULL         
AS        
BEGIN        
    SET NOCOUNT ON;        
            
    CREATE TABLE #tempTable (        
        TemplateBody NVARCHAR(MAX),        
        MobileNumber VARCHAR(15),        
        Name NVARCHAR(255)        
    );        
        DECLARE @LatestAndroidVersion NVARCHAR(50), @LatestIosVersion NVARCHAR(50);

    SELECT @LatestIosVersion = Value FROM Settings WHERE Code = 'IosCurrentVersion';
    SELECT @LatestAndroidVersion = Value FROM Settings WHERE Code = 'AndroidCurrentVersion';
    IF @TemplateName = 'paymentlink'        
    BEGIN        
         IF UPPER(@TargetType) = 'ALL'       
        BEGIN        
            INSERT INTO #tempTable (MobileNumber, Name)        
            SELECT Mobile, FullName        
            FROM MobileUsers        
        END        
        ELSE IF UPPER(@TargetType) = 'MOBILE'        
        BEGIN        
            INSERT INTO #tempTable (MobileNumber, Name)        
            SELECT Mobile, FullName        
            FROM MobileUsers        
            WHERE Mobile IN (SELECT value FROM STRING_SPLIT(@MobileNumbers, ','));        
        END        
            
        UPDATE #tempTable         
        SET TemplateBody = '{"id":"' + @TemplateId + '","params":["' + (SELECT Value FROM Settings WHERE code = 'TelegramLink') + '"]}';        
            
        SELECT TemplateBody, MobileNumber FROM #tempTable;        
    END        
    ELSE IF UPPER(@TemplateName) IN ('welcome_new','welcome_test','zomm_meeting_invitation_oct_1st','zoom_meeting_for_event_customers','example_template','demo_tes','mumbai','no_brainer_master','app')        
    BEGIN        
        IF UPPER(@TargetType) = 'ALL'       
        BEGIN        
            INSERT INTO #tempTable (MobileNumber, Name)        
            SELECT Mobile, FullName        
            FROM MobileUsers        
        END        
        ELSE IF UPPER(@TargetType) = 'MOBILE'        
        BEGIN        
            INSERT INTO #tempTable (MobileNumber, Name)        
            SELECT Mobile, FullName        
            FROM MobileUsers        
            WHERE Mobile IN (SELECT value FROM STRING_SPLIT(@MobileNumbers, ','));        
        END        
            
        UPDATE #tempTable         
        SET TemplateBody = '{"id":"' + @TemplateId + '","params":["' + Name + '","' + (SELECT Value FROM Settings WHERE code = 'TelegramLink') + '"]}';        
            
        SELECT TemplateBody, MobileNumber FROM #tempTable;        
    END        
    ELSE IF  UPPER(@TemplateName) IN( 'update_app_2025','app_update_march')      
    BEGIN        
       IF UPPER(@TargetType) = 'ALL'         
        BEGIN        
            INSERT INTO #tempTable (MobileNumber, Name)        
            SELECT Mobile, 'user'        
            FROM MobileUsers        
        END        
        ELSE IF UPPER(@TargetType) = 'MOBILE'    
        BEGIN        
            INSERT INTO #tempTable (MobileNumber, Name)        
            SELECT Mobile, 'user'        
            FROM MobileUsers        
            WHERE Mobile IN (SELECT value FROM STRING_SPLIT(@MobileNumbers, ','));        
        END       
       ELSE IF UPPER(@TargetType) = 'UNMATCHEDDEVICE'        
        BEGIN        
            INSERT INTO #tempTable (MobileNumber, Name)        
        SELECT Mobile, 'user'        
        FROM MobileUsers        
        WHERE 
            (DeviceType LIKE 'Android:%' AND DeviceVersion <> @LatestAndroidVersion)
            OR 
            (DeviceType LIKE 'IOS%' OR DeviceType LIKE 'IosId:%' AND DeviceVersion <> @LatestIosVersion)
            OR 
            (IsOtpVerified = 0);  -- Unverified users        
        END       
   ELSE          
   BEGIN                   
   INSERT INTO #tempTable (MobileNumber, Name)                  
   SELECT Mobile, 'user'                   
   FROM MobileUsers  WHERE mobile IN (SELECT value FROM STRING_SPLIT(@MobileNumbers, ','));           
   END        
            
        UPDATE #tempTable         
       SET TemplateBody = '{"id":"' + @TemplateId + '","params":["' +   
                   CASE   
                       WHEN Name IS NULL OR Name = '' THEN 'Username'   
                       ELSE Name   
                   END + '"]}';  
       
            
        SELECT TemplateBody, MobileNumber FROM #tempTable;        
    END        
        
    DROP TABLE #tempTable;        
END  


GO
ALTER PROCEDURE GetTargetAudianceListForPushNotification                  
    @AudianceCategory VARCHAR(50),                  
    @topic VARCHAR(50),                  
    @mobile VARCHAR(MAX) -- Use MAX to handle multiple numbers                      
AS                  
BEGIN                
        
    DECLARE @MobileUsers TABLE                  
    (                  
        FirebaseFcmToken VARCHAR(200),                  
        PublicKey UNIQUEIDENTIFIER,                  
        FullName VARCHAR(50),          
        OldDevice BIT,  
        Notification BIT 
    );                
          
    DECLARE @MobileList TABLE (Mobile VARCHAR(20));                  
          
    -- Parse the comma-separated @mobile input into individual rows                      
    IF @mobile IS NOT NULL                  
    BEGIN                  
        INSERT INTO @MobileList (Mobile)                  
        SELECT value FROM STRING_SPLIT(@mobile, ',');                  
    END            
     
    -- Fetch latest app versions    
    DECLARE @LatestAndroidVersion NVARCHAR(50), @LatestIosVersion NVARCHAR(50);    
    SELECT @LatestIosVersion = Value FROM Settings WHERE Code = 'IosCurrentVersion';    
    SELECT @LatestAndroidVersion = Value FROM Settings WHERE Code = 'AndroidCurrentVersion';    
    
          
    -- Handle different audience categories          
    IF UPPER(TRIM(@AudianceCategory)) = 'ALL'                  
    BEGIN                  
        INSERT INTO @MobileUsers                  
        SELECT           
            mu.FirebaseFcmToken,                  
            mu.PublicKey,                  
            mu.FullName,          
            CASE           
                WHEN mu.DeviceVersion IS NULL THEN 1  -- If NULL, mark as old          
                WHEN mu.DeviceType = 'iOS'           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 15 THEN 1 -- Old iOS device          
                          
                WHEN mu.DeviceType = 'Android'           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 57 THEN 1 -- Old Android device          
                          
                ELSE 0           
            END AS OldDevice,
            NULL AS Notification
        FROM MobileUsers mu                  
        WHERE ISNULL(mu.isActive, 1) = 1                  
              AND ISNULL(mu.IsDelete, 0) = 0;          
    END                  
    ELSE IF UPPER(TRIM(@AudianceCategory)) = 'PURCHASEDANY'                  
    BEGIN        
       INSERT INTO @MobileUsers                  
       SELECT          
           mu.FirebaseFcmToken,                  
           mu.PublicKey,                  
           mu.FullName,          
           CASE           
               WHEN mu.DeviceVersion IS NULL THEN 1            
               WHEN mu.DeviceType = 'iOS'           
                    AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1           
                    AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0           
                    AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 15 THEN 1           
           
               WHEN mu.DeviceType = 'Android'           
                    AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1           
                    AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0           
                    AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 57 THEN 1           
               ELSE 0           
           END AS OldDevice,
           NULL AS Notification
       FROM MobileUsers mu                  
       WHERE ISNULL(mu.isActive, 1) = 1                  
           AND ISNULL(mu.IsDelete, 0) = 0                  
           AND EXISTS (    
               SELECT 1 FROM PurchaseOrdersM pom     
               WHERE pom.ActionBy = mu.PublicKey     
               AND pom.TransasctionReference <> 'WITHOUTPAYMENT'    
           );    
    END        
    
    ELSE IF UPPER(TRIM(@AudianceCategory)) = 'NOTPURCHASEDANY'                  
    BEGIN        
       INSERT INTO @MobileUsers                  
       SELECT          
           mu.FirebaseFcmToken,                  
           mu.PublicKey,                  
           mu.FullName,          
           CASE           
               WHEN mu.DeviceVersion IS NULL THEN 1            
               WHEN mu.DeviceType = 'iOS'           
                    AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1           
                    AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0           
                    AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 15 THEN 1           
           
               WHEN mu.DeviceType = 'Android'           
                    AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1           
                    AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0           
                    AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 57 THEN 1           
               ELSE 0           
           END AS OldDevice,
           NULL AS Notification
       FROM MobileUsers mu                  
       WHERE ISNULL(mu.isActive, 1) = 1                  
           AND ISNULL(mu.IsDelete, 0) = 0                  
           AND NOT EXISTS (    
               SELECT 1 FROM PurchaseOrdersM pom     
               WHERE pom.ActionBy = mu.PublicKey     
               AND pom.TransasctionReference <> 'WITHOUTPAYMENT'    
           );                
    END        
     
    ELSE IF UPPER(TRIM(@AudianceCategory)) = 'MOBILE'                  
    BEGIN         
        INSERT INTO @MobileUsers                  
        SELECT           
            mu.FirebaseFcmToken,                  
            mu.PublicKey,                
            mu.FullName,          
            CASE           
                WHEN mu.DeviceVersion IS NULL THEN 1            
                WHEN mu.DeviceType = 'iOS'           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 15 THEN 1           
          
                WHEN mu.DeviceType = 'Android'           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 57 THEN 1           
                ELSE 0           
            END AS OldDevice,
            NULL AS Notification
        FROM MobileUsers mu                  
        WHERE ISNULL(mu.isActive, 1) = 1                  
              AND ISNULL(mu.IsDelete, 0) = 0                  
              AND EXISTS (                  
                  SELECT 1 FROM @MobileList ml WHERE ml.Mobile = mu.Mobile                  
              );                  
    END                  
    ELSE IF UPPER(TRIM(@AudianceCategory)) = 'PURCHASEDKALKIBAATAAJ'        
    BEGIN        
        INSERT INTO @MobileUsers                  
        SELECT           
            mu.FirebaseFcmToken,                  
            mu.PublicKey,                  
            mu.FullName,          
            CASE           
                WHEN mu.DeviceVersion IS NULL THEN 1            
                WHEN mu.DeviceType = 'iOS'           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 15 THEN 1           
          
                WHEN mu.DeviceType = 'Android'           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 57 THEN 1           
                ELSE 0           
            END AS OldDevice,
            mb.Notification
        FROM MobileUsers mu            
        INNER JOIN MYBucketM AS mb ON mu.PublicKey = mb.MobileUserKey AND ISNULL(mb.IsActive, 1) = 1        
            AND CAST(mb.EndDate AS DATE) >= CAST(GETDATE() AS DATE)        
        INNER JOIN ProductsM AS p ON p.id = mb.ProductId AND p.Code = 'KALKIBAATAAJ'        
        WHERE ISNULL(mu.isActive, 1) = 1                  
              AND ISNULL(mu.IsDelete, 0) = 0;               
    END        
    ELSE IF UPPER(TRIM(@AudianceCategory)) = 'PURCHASEDBREAKFAST'        
    BEGIN        
        INSERT INTO @MobileUsers                  
        SELECT           
            mu.FirebaseFcmToken,                  
            mu.PublicKey,                  
            mu.FullName,          
            CASE           
                WHEN mu.DeviceVersion IS NULL THEN 1            
                WHEN mu.DeviceType = 'iOS'           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 15 THEN 1           
          
                WHEN mu.DeviceType = 'Android'           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 57 THEN 1           
                ELSE 0           
            END AS OldDevice,
            mb.Notification
        FROM MobileUsers mu            
        INNER JOIN MYBucketM AS mb ON mu.PublicKey = mb.MobileUserKey AND ISNULL(mb.IsActive, 1) = 1        
            AND CAST(mb.EndDate AS DATE) >= CAST(GETDATE() AS DATE)        
        INNER JOIN ProductsM AS p ON p.id = mb.ProductId AND p.Code = 'BREAKFAST'        
        WHERE ISNULL(mu.isActive, 1) = 1                  
              AND ISNULL(mu.IsDelete, 0) = 0;               
    END        
    ELSE IF UPPER(TRIM(@AudianceCategory)) = 'PURCHASEDBREAKOUT'        
    BEGIN        
        INSERT INTO @MobileUsers                  
        SELECT           
            mu.FirebaseFcmToken,                  
            mu.PublicKey,                  
            mu.FullName,          
            CASE           
                WHEN mu.DeviceVersion IS NULL THEN 1            
                WHEN mu.DeviceType = 'iOS'           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 15 THEN 1           
          
                WHEN mu.DeviceType = 'Android'           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 57 THEN 1           
                ELSE 0           
            END AS OldDevice,
            mb.Notification
        FROM MobileUsers mu            
        INNER JOIN MYBucketM AS mb ON mu.PublicKey = mb.MobileUserKey AND ISNULL(mb.IsActive, 1) = 1        
            AND CAST(mb.EndDate AS DATE) >= CAST(GETDATE() AS DATE)        
        INNER JOIN ProductsM AS p ON p.id = mb.ProductId AND p.Code = 'BREAKOUT'        
        WHERE ISNULL(mu.isActive, 1) = 1                  
              AND ISNULL(mu.IsDelete, 0) = 0;               
    END        
    ELSE IF UPPER(TRIM(@AudianceCategory)) = 'PURCHASEDMORNINGSHORT'        
    BEGIN        
        INSERT INTO @MobileUsers                  
        SELECT           
            mu.FirebaseFcmToken,                  
            mu.PublicKey,                  
            mu.FullName,          
            CASE           
                WHEN mu.DeviceVersion IS NULL THEN 1            
                WHEN mu.DeviceType = 'iOS'           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 15 THEN 1           
          
                WHEN mu.DeviceType = 'Android'           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1                                
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0           
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 57 THEN 1           
                ELSE 0           
            END AS OldDevice,
            mb.Notification
        FROM MobileUsers mu            
        INNER JOIN MYBucketM AS mb ON mu.PublicKey = mb.MobileUserKey AND ISNULL(mb.IsActive, 1) = 1        
            AND CAST(mb.EndDate AS DATE) >= CAST(GETDATE() AS DATE)        
        INNER JOIN ProductsM AS p ON p.id = mb.ProductId AND p.Code = 'MORNINGSHORT'        
        WHERE ISNULL(mu.isActive, 1) = 1                  
              AND ISNULL(mu.IsDelete, 0) = 0;               
    END        
    ELSE IF UPPER(TRIM(@AudianceCategory)) = 'UNMATCHEDDEVICE'      
    BEGIN      
        INSERT INTO @MobileUsers                
        SELECT         
            mu.FirebaseFcmToken,                
            mu.PublicKey,                
            mu.FullName,        
            CASE         
                WHEN mu.DeviceVersion IS NULL THEN 1  -- If NULL, mark as old        
                WHEN mu.DeviceType = 'iOS'         
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1         
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0         
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 15 THEN 1     
                        
                WHEN mu.DeviceType = 'Android'         
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1         
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0         
                     AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 57 THEN 1    
                ELSE 0         
            END AS OldDevice,
            NULL AS Notification
        FROM MobileUsers mu                
        WHERE ISNULL(mu.isActive, 1) = 1                
              AND ISNULL(mu.IsDelete, 0) = 0    
              AND (  
                  (mu.DeviceType LIKE 'Android:%' AND mu.DeviceVersion <> @LatestAndroidVersion)    
                  OR (mu.DeviceType LIKE 'IOS%' OR mu.DeviceType LIKE 'IosId:%' AND mu.DeviceVersion <> @LatestIosVersion)    
                  OR mu.IsOtpVerified = 0    
              );             
    END  
  
    SELECT * FROM @MobileUsers;                  
END;
        

-- Ajith 18/03/2025 12:10

GO
ALTER PROCEDURE [dbo].[GetCustomers]            
 --declare                 
 @IsPaging BIT = 1            
 ,@PageSize INT = 25            
 ,@PageNumber INT = 1            
 ,@SortExpression VARCHAR(50) = NULL            
 ,@SortOrder VARCHAR(50) = NULL            
 ,@RequestedBy VARCHAR(50) = NULL            
 ,@SearchText VARCHAR(100) = NULL            
 ,@FromDate DATETIME = NULL            
 ,@ToDate DATETIME = NULL            
 ,@StrategyKey VARCHAR(50) = NULL      
 ,@SecondaryKey varchar(50) = null    
 ,@LoggedInUser VARCHAR(50) = NULL                                   
 ,@TotalCount INT = 0 OUTPUT          
 ,@TotalAmount INT =0 OUTPUT        
AS            
BEGIN            
 SET @IsPaging = 0            
  SET @PageSize = COALESCE(@PageSize, 20);            
 --set @PageNumber =1                 
 SET @SortExpression = 'callDate'            
 SET @SortOrder = 'desc'            
 --set @RequestedBy=NULL                 
 --set @FromDate='2023-03-31 11:00:31.333'                 
 --set @ToDate='2023-05-30 11:00:31.333'                 
 SET @StrategyKey = NULL            
 SET @TotalCount = 0            
 SET @SearchText = ISNULL(@SearchText, '')        
 
 IF EXISTS (select 1 from Users as us      
INNER JOIN Roles  as ro on us.RoleKey = ro.PublicKey     
WHERE us.PublicKey = @LoggedInUser and ro.Name <> 'admin')     
BEGIN -- if it's a bde      
  SET @RequestedBy = @LoggedInUser             
END       
            
 SELECT @TotalCount = count(1) ,@TotalAmount = ISNULL(sum(purchaseOrders.PaidAmount),0)       
  FROM Leads AS leads            
  INNER JOIN PurchaseOrders AS purchaseOrders ON leads.Id = purchaseOrders.LeadId            
   AND leads.AssignedTo IS NOT NULL            
   AND purchaseOrders.STATUS = (            
    SELECT Id            
    FROM STATUS            
    WHERE name = 'customer'            
    )            
  INNER JOIN Users AS users ON leads.AssignedTo = users.PublicKey            
  LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = Leads.LeadTypeKey            
  INNER JOIN STATUS AS STATUS ON STATUS.Id = purchaseOrders.STATUS            
   AND leads.AssignedTo = ISNULL(@RequestedBy, leads.AssignedTo)            
   AND CAST(purchaseOrders.paymentDate AS DATE) BETWEEN CAST(ISNULL(@FROMDATE, (DATEADD(MONTH, - 1, getdate()))) AS DATE)            
    AND CAST(ISNULL(@TODATE, GETDATE()) AS DATETIME)            
   AND (            
    leads.FullName LIKE '%' + @SearchText + '%'            
    or leads.mobilenumber LIKE '%' + @SearchText + '%')            
  WHERE ISNULL(purchaseOrders.IsExpired, 0) = 0        
     AND((  purchaseOrders.ServiceId in (SELECT CAST(value AS INT)  FROM STRING_SPLIT(@SecondaryKey, ',')  )  and @SecondaryKey is not null )  or( purchaseOrders.ServiceId = purchaseOrders.ServiceId and @SecondaryKey is null)    )              
    
 --FROM Leads AS leads            
 --INNER JOIN PurchaseOrders AS purchaseOrders ON leads.Id = purchaseOrders.LeadId            
 -- AND leads.AssignedTo IS NOT NULL            
 -- AND purchaseOrders.STATUS = (            
 --  SELECT Id            
 --  FROM STATUS            
 --  WHERE name = 'customer'            
 --  )            
 --INNER JOIN Users AS users ON leads.AssignedTo = users.PublicKey            
 --LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = Leads.LeadTypeKey            
 --INNER JOIN STATUS AS STATUS ON STATUS.Id = purchaseOrders.STATUS            
 -- AND leads.AssignedTo = ISNULL(@RequestedBy, leads.AssignedTo)            
 -- AND CAST(purchaseOrders.paymentDate AS DATE) BETWEEN CAST(ISNULL(@FROMDATE, (DATEADD(MONTH, - 1, getdate()))) AS DATE)            
 --  AND CAST(ISNULL(@TODATE, GETDATE()) AS DATETIME)            
 -- AND (            
 --   leads.FullName LIKE '%' + @SearchText + '%'            
 --   or leads.mobilenumber LIKE '%' + @SearchText + '%')            
 --WHERE ISNULL(purchaseOrders.IsExpired, 0) = 0            
            
 --and cast(GETDATE() as date) BETWEEN CAST(ISNULL(purchaseOrders.StartDate, '2020-01-01') as date) and CAST(ISNULL(purchaseOrders.EndDate, '2020-01-01') as date)                
 DROP TABLE            
            
 IF EXISTS #tempCustomer            
  SELECT Cast(ROW_NUMBER() OVER (            
     ORDER BY leads.id DESC            
     ) AS INT) AS slNo            
   ,leads.Id            
   ,leads.PublicKey AS LeadPublicKey            
  ,leads.FullName            
   ,leads.MobileNumber            
   ,leads.EmailId            
   ,LeadTypes.Name AS [LeadTypeKey]            
   ,users.FirstName AS AssignedToName            
   ,isnull(purchaseOrders.StartDate, '') AS StartDate            
   ,isnull(purchaseOrders.EndDate, '') AS EndDate            
   ,purchaseOrders.PaidAmount            
   ,IsNull(DATEDIFF(DAY, GETDATE(), purchaseOrders.EndDate), 0) AS DaysToGo            
   ,purchaseOrders.STATUS            
   ,STATUS.Name AS PrStatusName            
   ,leads.PurchaseOrderKey            
   ,purchaseOrders.City            
   ,purchaseOrders.Remark            
   ,purchaseOrders.ModeOfPayment            
   ,purchaseOrders.ServiceId            
   ,purchaseorders.paymentDate       
   ,s.name as ServiceName    
  INTO #tempCustomer            
  FROM Leads AS leads            
  INNER JOIN PurchaseOrders AS purchaseOrders ON leads.Id = purchaseOrders.LeadId            
  AND leads.AssignedTo IS NOT NULL            
   AND purchaseOrders.STATUS = (            
    SELECT Id            
    FROM STATUS            
    WHERE name = 'customer'            
    )            
  INNER JOIN Users AS users ON leads.AssignedTo = users.PublicKey            
  LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = Leads.LeadTypeKey            
  INNER JOIN STATUS AS STATUS ON STATUS.Id = purchaseOrders.STATUS      
  AND (@RequestedBy IS NULL OR (Leads.AssignedTo IS NOT NULL AND Leads.AssignedTo = @RequestedBy))        
  -- AND leads.AssignedTo = ISNULL(@RequestedBy, leads.AssignedTo)            
   AND CAST(purchaseOrders.paymentDate AS DATE) BETWEEN CAST(ISNULL(@FROMDATE, (DATEADD(MONTH, - 1, getdate()))) AS DATE)            
    AND CAST(ISNULL(@TODATE, GETDATE()) AS DATETIME)            
   AND (            
    leads.FullName LIKE '%' + @SearchText + '%'            
    or leads.mobilenumber LIKE '%' + @SearchText + '%')      
  inner join Services s  on purchaseOrders.Serviceid =s.id    
  WHERE ISNULL(purchaseOrders.IsExpired, 0) = 0         
   AND((  purchaseOrders.ServiceId in (SELECT CAST(value AS INT)  FROM STRING_SPLIT(@SecondaryKey, ',')  )  and @SecondaryKey is not null )  or( purchaseOrders.ServiceId = purchaseOrders.ServiceId and @SecondaryKey is null)    )              
    
  ORDER BY CASE             
    WHEN @SortOrder <> 'asc'            
     THEN cast(NULL AS DATE)            
    WHEN @SortExpression = 'CreatedOn'            
     THEN leads.CreatedOn            
    END ASC            
   ,CASE             
    WHEN @SortOrder <> 'asc'            
     THEN ''            
    WHEN @SortExpression = 'leadName'            
     THEN leads.FullName            
    END ASC            
   ,CASE             
    WHEN @sortOrder <> 'desc'            
     THEN ''            
    WHEN @SortExpression = 'leadName'            
     THEN leads.FullName            
    END DESC OFFSET(IIF(@PageNumber <= 1, 0, (@PageNumber - 1))) * @PageSize ROWS            
            
 FETCH NEXT @PageSize ROWS ONLY            
            
 SELECT (            
   SELECT *            
   FROM #tempCustomer            
   FOR JSON AUTO            
   ) AS JsonData            
END     
GO




-- Guna Surya  18/03/2025


GO
alter PROCEDURE [dbo].[Sp_Get_Leads] (                
 --declare                                                  
 @IsPaging INT = 0            
 ,@PageSize INT = 5                                    
 ,@PageNumber INT = 1                                    
 ,@SortExpression VARCHAR(50)                                    
 ,@SortOrder VARCHAR(50)                                    
 ,@FromDate VARCHAR(50) = NULL                                    
 ,@ToDate VARCHAR(50) = NULL                                    
 ,@PrimaryKey VARCHAR(50) = NULL                                    
 ,@SecondaryKey VARCHAR(600) = NULL                                    
 ,-- Services Dropdown Filter                                                   
 @ThirdKey VARCHAR(50) = NULL                                    
 ,@FourthKey VARCHAR(50) = NULL                                    
 ,-- lead Source                                                  
 @FifthKey VARCHAR(50) = NULL                                    
 ,-- po status                                                  
 @CreatedBy VARCHAR(50) = NULL                                    
 ,@AssignedTo VARCHAR(50) = NULL                                    
 ,@LoggedInUser VARCHAR(50) = NULL                                    
 ,@RoleKey VARCHAR(50) = ''                                    
 ,@SearchText VARCHAR(250) = NULL                                    
 ,@TotalCount INT = 0 OUTPUT                                    
 )                                    
AS                                    
BEGIN                                
 DECLARE @StatusId int                               
 SELECT @StatusId = Id FROM STATUS WHERE Code = 'fresh'                              
      
      
-- select @IsPaging=0,@PageSize=20,@PageNumber =1,@SortExpression='CreatedOn',@SortOrder='desc',      
-- @FromDate='2000-01-01',@ToDate='9999-01-01',@PrimaryKey=NULL,      
-- @SecondaryKey=NULL,@ThirdKey=NULL,@FourthKey=NULL,@FifthKey=NULL,      
-- @CreatedBy=NULL,@AssignedTo=NULL,      
-- @LoggedInUser='0526f991-40c9-42ef-9921-c7f93d62574a',      
-- @RoleKey='cecf471a-9276-eb11-9fe9-00155d53687a'       
      
      
      
IF EXISTS (select 1 from Users as us       
INNER JOIN Roles  as ro on us.RoleKey = ro.PublicKey      
WHERE us.PublicKey = @LoggedInUser and ro.Name NOT IN ('admin', 'DM-Ashok') )      
BEGIN    
  SET @AssignedTo    = @LoggedInUser              
END      
      
SELECT @TotalCount = count(1)                                    
 FROM Leads AS Leads                                
  LEFT JOIN PurchaseOrders AS po ON po.LeadId = Leads.Id AND (Leads.PurchaseOrderKey IS NOT NULL AND Leads.PurchaseOrderKey = po.PublicKey)               
 LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = CAST(Leads.LeadTypeKey  as uniqueidentifier)                                     
 LEFT JOIN LeadSources AS LeadSource ON LeadSource.PublicKey = CAST( Leads.LeadSourceKey as uniqueidentifier) and isnull(LeadSource.IsDelete,0) = 0            
 LEFT JOIN STATUS AS st ON st.id = Leads.StatusId                                     
LEFT JOIN Users AS Users ON Users.PublicKey = TRY_CAST(Leads.CreatedBy AS UNIQUEIDENTIFIER)                                   
 --LEFT JOIN Services AS LeadService ON cast(leads.ServiceKey   as uniqueidentifier) = LeadService.PublicKey and LeadService.IsDelete = 0 and LeadService.IsDisabled = 0              
 LEFT JOIN Services AS LeadService ON ( leads.ServiceKey IS NOT NULL AND TRY_CAST(leads.ServiceKey AS UNIQUEIDENTIFIER) = LeadService.PublicKey )           
 LEFT JOIN Services AS PoService ON po.serviceid = PoService.id AND ISNULL(po.isExpired, 0) <> 1                                   
 LEFT JOIN STATUS AS st1 ON st1.id = po.STATUS                                     
 LEFT JOIN PaymentModes AS pm ON po.ModeOfPayment = pm.Id                                   
 WHERE ISNULL(leads.IsDelete,0) = 0 and             
(@AssignedTo IS NULL OR (Leads.AssignedTo IS NOT NULL AND Leads.AssignedTo = @AssignedTo))                                   
  AND (                         
    (po.status <> 24 AND ISNULL(po.IsExpired, 0) = 0)              
    OR                             
    (po.status = 24 AND ISNULL(po.IsExpired, 0) = 1)                             
 OR                             
    (po.status IS NULL)                          
 OR                         
 (po.status = 4)                         
)                         
     AND((  leads.ServiceKey in (SELECT CAST(value AS uniqueidentifier)  FROM STRING_SPLIT(@SecondaryKey, ',')  )  and @SecondaryKey is not null )  or( leads.ServiceKey = leads.ServiceKey and @SecondaryKey is null)    )                                   
                    
 AND (ISNULL(Leads.LeadSourceKey, '1') = ISNULL(@FourthKey, ISNULL(Leads.LeadSourceKey, '1')))                                     
 AND (ISNULL(Leads.LeadTypeKey, '1') = ISNULL(@ThirdKey, ISNULL(Leads.LeadTypeKey, '1')))                                     
 AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN cast(@FromDate AS DATE) AND cast(@ToDate AS DATE))           
 AND (                                     
 Leads.MobileNumber LIKE '%' + @SearchText + '%'                                     
 OR Leads.AlternateMobileNumber LIKE '%' + @SearchText + '%'                                     
 OR @SearchText IS NULL                                     
 OR Leads.EmailId LIKE '%' + @SearchText + '%'                                     
 OR Leads.FullName LIKE '%' + @SearchText + '%'                                     
 )                                     
 AND (@FifthKey IS NULL OR (@FifthKey = 'New' AND ISNULL(st1.Name, 'New') = 'New')                                     
 OR (@FifthKey <> 'New' AND ISNULL(st1.Name, 'New') = @FifthKey))                            
                      
 DROP TABLE IF EXISTS #tempLeads                                    
 SELECT Cast(ROW_NUMBER() OVER (                  
   ORDER BY                    
   Leads.ModifiedOn   desc ,Favourite DESC,                  
       CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN Leads.CreatedOn END ASC,                  
        CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN Leads.CreatedOn END DESC,                  
        CASE WHEN @SortExpression = 'Favourite' AND @SortOrder = 'ASC' THEN Favourite END ASC,                  
        CASE WHEN @SortExpression = 'Favourite' AND @SortOrder = 'DESC' THEN Favourite END DESC,                  
        CASE WHEN @SortExpression = 'Name' AND @SortOrder = 'ASC' THEN FullName END ASC,                  
        CASE WHEN @SortExpression = 'Name' AND @SortOrder = 'DESC' THEN FullName END DESC                  
                   
 --ORDER BY Leads.id DESC                   
                   
 ) AS INT) AS SlNo                                     
 ,Leads.[Id]                                    
 ,Leads.[FullName]                                    
 ,Leads.[MobileNumber]                                    
 ,Leads.[AlternateMobileNumber]                                    
 ,Leads.[EmailId]                                    
 ,ISNULL(po.City, ISNULL(Leads.City, '')) AS City                                    
 ,po.PaymentDate                                    
 ,ISNULL(Leads.Favourite, 0) AS Favourite                                    
 ,COALESCE(LeadService.Name, PoService.Name) AS [ServiceKey]              
 ,COALESCE(LeadService.Id, PoService.Id) AS ServiceId                                   
 ,LeadTypes.Name AS [LeadTypeKey]                                    
 ,LeadTypes.Id AS LeadTypesId                                    
 ,LeadSource.Id AS LeadSourcesId                                    
 ,LeadSource.Name AS [LeadSourceKey]                                    
 ,ISNULL(Leads.[Remarks], '') AS Remark               
 ,Leads.[IsSpam]                                    
 ,Leads.[IsWon]                                    
 ,Leads.[IsDisabled]                                    
 ,Leads.[IsDelete]                                    
 ,Leads.[PublicKey]                                    
 ,Leads.[CreatedOn]                                    
 ,Users.FirstName AS [CreatedBy]                                    
 ,Leads.[ModifiedOn]                                    
 ,Leads.[ModifiedBy]                     
 ,'' AS [AssignedTo]                                    
 ,ISNULL(Leads.StatusId, ( 1)) AS [StatusId]        
 ,isnull(st.Name, 'New') AS StatusName                                    
 ,Isnull(Leads.PurchaseOrderKey, CAST('00000000-0000-0000-0000-000000000000'  AS UNIQUEIDENTIFIER)) AS PurchaseOrderKey                                    
 ,isnull(st1.Name, 'New') AS PurchaseOrderStatus                                    
 ,isnull(po.ModeOfPayment, - 1) AS ModeOfPayment                                    
 ,isnull(po.PaidAmount, 0.0) AS PaidAmount                                    
 ,isnull(po.NetAmount, 0.0) AS NetAmount                                    
 ,ISNULL(po.TransactionRecipt, '') AS TransactionRecipt                                    
 ,ISNULL(po.TransasctionReference, '') AS TransasctionReference                                    
 ,(case when DATEDIFF(DAY, GETDATE(), DATEADD(day, 90, ISNULL(leads.ModifiedOn, GETDATE()))) > 0 then DATEDIFF(DAY, GETDATE(), DATEADD(day, 90, ISNULL(leads.ModifiedOn, GETDATE()))) else 0 end) AS DaysToGo                                    
 ,Leads.CountryCode                  
 INTO #tempLeads                          
 FROM Leads AS Leads              
 LEFT JOIN PurchaseOrders AS po ON po.LeadId = Leads.Id AND (Leads.PurchaseOrderKey IS NOT NULL AND Leads.PurchaseOrderKey = po.PublicKey)              
 LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = CAST(Leads.LeadTypeKey  as uniqueidentifier)                                    
 LEFT JOIN LeadSources AS LeadSource ON LeadSource.PublicKey = CAST( Leads.LeadSourceKey as uniqueidentifier) and isnull(LeadSource.IsDelete,0) = 0           
 LEFT JOIN STATUS AS st ON st.id = Leads.StatusId                                    
LEFT JOIN Users AS Users ON Users.PublicKey = TRY_CAST(Leads.CreatedBy AS UNIQUEIDENTIFIER)                                 
 --LEFT JOIN Services AS LeadService ON cast(leads.ServiceKey   as uniqueidentifier) = LeadService.PublicKey and LeadService.IsDelete = 0 and LeadService.IsDisabled = 0             
 LEFT JOIN Services AS LeadService ON ( leads.ServiceKey IS NOT NULL AND TRY_CAST(leads.ServiceKey AS UNIQUEIDENTIFIER) = LeadService.PublicKey )          
 LEFT JOIN Services AS PoService ON po.serviceid = PoService.id AND ISNULL(po.isExpired, 0) <> 1                                  
 LEFT JOIN STATUS AS st1 ON st1.id = po.STATUS                                    
 LEFT JOIN PaymentModes AS pm ON po.ModeOfPayment = pm.Id                    
 WHERE ISNULL(leads.IsDelete,0) = 0 and            
    (@AssignedTo IS NULL OR (Leads.AssignedTo IS NOT NULL AND Leads.AssignedTo = @AssignedTo))                                              
 AND (                    
    (po.status <> 24 AND ISNULL(po.IsExpired, 0) = 0)                              
    OR                            
    (po.status = 24 AND ISNULL(po.IsExpired, 0) = 1)                            
 OR                            
    (po.status IS NULL)                         
 OR                        
 (po.status = 4)                        
)                            
------                            
 --AND (ISNULL(Leads.ServiceKey, '1') = ISNULL(@SecondaryKey, ISNULL(Leads.ServiceKey, '1')))                           
    AND(( leads.ServiceKey in (SELECT CAST(value AS uniqueidentifier)  FROM STRING_SPLIT(@SecondaryKey, ',')  )  and @SecondaryKey is not null )  or( leads.ServiceKey = leads.ServiceKey and @SecondaryKey is null)    )                                  
                      
 AND (ISNULL(Leads.LeadSourceKey, '1') = ISNULL(@FourthKey, ISNULL(Leads.LeadSourceKey, '1')))                                    
 AND (ISNULL(Leads.LeadTypeKey, '1') = ISNULL(@ThirdKey, ISNULL(Leads.LeadTypeKey, '1')))                     
 AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN cast(@FromDate AS DATE) AND cast(@ToDate AS DATE))                                    
 AND (                           
 Leads.MobileNumber LIKE '%' + @SearchText + '%'                                    
 OR Leads.AlternateMobileNumber LIKE '%' + @SearchText + '%'                                    
 OR @SearchText IS NULL                                    
 OR Leads.EmailId LIKE '%' + @SearchText + '%'                                    
 OR Leads.FullName LIKE '%' + @SearchText + '%')                                    
 AND (@FifthKey IS NULL OR (@FifthKey = 'New' AND ISNULL(st1.Name, 'New') = 'New')                                    
 OR (@FifthKey <> 'New' AND ISNULL(st1.Name, 'New') = @FifthKey))                               
 ORDER BY ISNULL(Leads.ModifiedOn, LEADS.CREATEDON)   desc, Favourite DESC ,                  
 CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN Leads.CreatedOn END ASC,                  
 CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN Leads.CreatedOn END DESC,                  
 CASE WHEN @SortExpression = 'Favourite' AND @SortOrder = 'ASC' THEN Favourite END ASC,                  
 CASE WHEN @SortExpression = 'Favourite' AND @SortOrder = 'DESC' THEN Favourite END DESC,                  
 CASE WHEN @SortExpression = 'Name' AND @SortOrder = 'ASC' THEN FullName END ASC,                  
 CASE WHEN @SortExpression = 'Name' AND @SortOrder = 'DESC' THEN FullName END DESC                  
                 
 OFFSET (IIF(@PageNumber <= 1, 0, (@PageNumber - 1))) * @PageSize ROWS                  
 FETCH NEXT @PageSize ROWS ONLY                                                           
           
 SELECT * FROM #tempLeads       
           
END    
 

 GO
-- This procedure only getting call from Mobile Flutter while doing any payment using payment gateway                             
-- Not:  Don't use this for CRM or any where elese to get the subscription in my bucket.                              
-- @paidAmount means the final amount which has been paid by the client                            
-- exec ManagePurchaseOrderSm 'BA0C9CCE-6A8F-EF11-819C-00155D23D79C', 1  , 1,'TRANSACT12312313', 1  , null                               
Alter PROCEDURE [dbo].[ManagePurchaseOrderSM]    
    @mobileUserKey UNIQUEIDENTIFIER,    
    @productId INT,    
    @SubscriptionMappingId int,    
    @MerchantTransactionId VARCHAR(100),    
    @TransactionId VARCHAR(100),    
    @paidAmount DECIMAL(18, 2),    
    @couponCode VARCHAR(20) = NULL -- using coupon name here                                                                   
AS    
BEGIN    
    --SELECT * fROM Services                      
    --SELECT @mobileUserKey = '0d9c40b8-289b-ef11-b318-c2aedf6978f6',                          
    --   @productId = 42,                          
    --   @SubscriptionMappingId = 58,                          
    --   @MerchantTransactionId = 'TRANSACTION02012025110520',                          
    --   @TransactionId = 'T2501021105446938436023',                          
    --   @paidAmount = 600,                          
    --   @couponCode = NULL;                          
    DECLARE @CurrentDate   DATETIME = getdate(),    
            @leadsourcekey uniqueidentifier,    
            @AdminKey      uniqueidentifier    
    SELECT @leadsourcekey = publickey    
      from LeadSources    
     where name = 'mobileapp'    
    SELECT @AdminKey = us.publickey    
      from Users as us    
     inner join Roles as ro    
        on us.RoleKey = ro.PublicKey    
     WHERE ro.Name      = 'Admin'    
       and us.PublicKey = '3CA214D0-8CB8-EB11-AAF2-00155D53687A'    
    
    BEGIN TRANSACTION;    
    BEGIN TRY    
    
    
        DECLARE @NewLeadKey   UNIQUEIDENTIFIER,    
                @MobileNumber VARCHAR(20);    
        -- Get the MobileNumber and existing LeadKey     
        SELECT @MobileNumber = Mobile,    
               @NewLeadKey = LeadKey    
          FROM MobileUsers    
         WHERE PublicKey = @MobileUserKey; -- Check if LeadKey is invalid or NULL     
        IF @NewLeadKey IS NULL    
        OR NOT EXISTS (   SELECT 1    
                            FROM Leads    
                           WHERE PublicKey = @NewLeadKey)    
        BEGIN -- Find the LeadKey using MobileNumber       
            SELECT TOP 1 @NewLeadKey = PublicKey    
              FROM Leads    
             WHERE MobileNumber = @MobileNumber; -- Update MobileUser if a valid LeadKey is found       
            IF @NewLeadKey IS NOT NULL    
            BEGIN    
                UPDATE MobileUsers    
                   SET LeadKey = @NewLeadKey    
                 WHERE PublicKey = @MobileUserKey;    
            END    
            ELSE    
            BEGIN    
                SELECT @NewLeadKey = NEWID()    
                INSERT INTO Leads (SupervisorId,    
                                   PublicKey,    
                                   FullName,    
                                   Gender,    
                                   MobileNumber,    
                                   AlternateMobileNumber,    
                                   EmailId,    
                                   ProfileImage,    
                                   PriorityStatus,    
                                   AssignedTo,    
                                   ServiceKey,    
                                   LeadTypeKey,    
                                   LeadSourceKey,    
                                   Remarks,    
                                   IsDisabled,    
                                   IsDelete,    
                                   CreatedOn,    
                                   CreatedBy,    
                                   IsSpam,    
                                  IsWon,    
                                   ModifiedOn,    
                                   ModifiedBy,    
         City,    
                                   PinCode,    
                                   StatusId,    
                                   PurchaseOrderKey,    
                                   Favourite,    
                                   CountryCode)    
                SELECT NULL,    
                       @NewLeadKey,    
                       FullName,    
                       (CASE    
                             WHEN Gender = 'male' then 'm'    
                             else 'f' end),    
                       Mobile,    
                       NULL,    
                       EmailId,    
                       NULL,    
                       1,    
                       NULL,    
                       'C11DAA9F-F125-EE11-811D-00155D23D79C',    
                       NULL,    
                       @leadsourcekey,    
                       'Reg. via mobile app',    
                       0,    
                       0,    
                       @CurrentDate,    
                       @AdminKey,    
                       0,    
                       0,    
                       @CurrentDate,    
                       @AdminKey,    
                       City,    
                       '',    
                       1,    
                       NULL,    
                       1,    
                       CountryCode    
                  FROM MobileUsers    
                 where PublicKey = @mobileUserKey;    
                UPDATE MobileUsers    
                   set LeadKey = @NewLeadKey    
                 where PublicKey = @mobileUserKey    
            END    
        END    
        DECLARE @couponMOP INT,    
                @razorMOP  INT    
        SELECT @couponMOP = Id    
          FROM PaymentModes    
         WHERE name = 'Coupon';    
        SELECT @razorMOP = Id    
          FROM PaymentModes    
         WHERE name = 'Razor';    
        DROP TABLE IF EXISTS #TempTable    
        SELECT *    
        INTO   #TempTable    
          FROM MobileUsers    
         WHERE publickey = @MobileUserKey --'FE05087D-6AA6-EF11-B32F-852B0D908F81'                                    
    
        DECLARE @couponCodeExists         BIT,    
                @discountAmount           DECIMAL(18, 2),    
                @discountPercent          INT,    
                @price                    DECIMAL(18, 2),    
                @couponkey                UNIQUEIDENTIFIER,    
                @startDate                DATETIME        = @currentDate,    
                @endDate                  datetime,    
                @couponHasProductValidity bit,    
                @couponValidityDays       int             = 0,    
                @FinalValidityInDays      int             = 0,    
                @ProductName              varchar(100)    = '',    
                @validity                 int             = null;    
    
        SELECT @couponkey = publickey,    
               @validity = ProductValidityInDays    
          FROM CouponsM    
         WHERE Name        = @couponCode    
           AND IsActive    = 1    
           AND IsDelete    = 0    
           AND RedeemLimit > TotalRedeems    
    
        -- Calculate the NetAmount ,DiscountAmount based on SubscriptionMappingId                                  
        -- Because each product coming under atleast 1 plan , and price may very based on Plan or Durations.                                   
        SELECT @discountPercent = sm.DiscountPercentage,    
               @price = p.Price,    
               @discountAmount = ((p.Price * (sm.DiscountPercentage) / 100)),    
               @couponValidityDays = 0,    
               @endDate = DATEADD(month, sd.Months, GETDATE()),    
               @ProductName = p.Name,    
               @FinalValidityInDays = DATEDIFF(DAY, (@currentDate), (DATEADD(month, sd.Months, @currentDate))) - 1    
          FROM SubscriptionMappingM as sm    
         INNER JOIN SubscriptionDurationM as sd    
            on sm.SubscriptionDurationId = sd.Id    
         INNER JOIN ProductsM as p    
            on sm.ProductId              = p.Id    
         WHERE sm.Id       = @SubscriptionMappingId    
           and ProductId   = @productId    
           and sm.IsActive = 1    
    
        -- SET @paidAmount = @price - isnull(@discountAmount,0);                  
        SELECT @couponHasProductValidity = IIF(ProductValidityInDays IS NULL, 0, 1),    
               @couponValidityDays = ProductValidityInDays,    
               @discountPercent = IIF(DiscountInPercentage IS NULL, 0, 1)    
          FROM CouponsM c    
         WHERE publickey = @couponkey;    
    
        -- Now change the final Paid Amount after applying the coupon code on it.                                  
        -- set @paidAmount = @paidAmount - (@paidAmount * (@discountPercent / 100))                       
    
        --Validity days change if coupon validity is greater then Subscription Validity in days                             
        IF (@couponValidityDays > @FinalValidityInDays)    
        BEGIN    
            SET @FinalValidityInDays = @couponValidityDays    
        END    
    
        IF EXISTS (   SELECT 1    
                        FROM myBucketM    
                       where MobileUserKey = @MobileUserKey    
                         AND ProductId     = @ProductId)    
        BEGIN    
    
            SELECT @startDate = CASE    
                                     WHEN EndDate > @CurrentDate THEN StartDate    
                                     ELSE @CurrentDate END,    
                   @EndDate = (CASE    
                                    WHEN EndDate > @CurrentDate THEN DATEADD(DAY, @FinalValidityInDays, EndDate)    
                                    ELSE DATEADD(DAY, @FinalValidityInDays + 1, @CurrentDate) END)    
              FROM MYBucketM    
             WHERE MobileUserKey = @MobileUserKey    
               AND ProductId     = @ProductId    
    
            UPDATE MYBucketM    
               SET ProductName = @ProductName,    
                   StartDate = CASE    
                                    WHEN EndDate > @CurrentDate THEN StartDate    
                                    ELSE @CurrentDate END,    
                   EndDate = @EndDate,    
                   ModifiedBy = @mobileUserKey,    
                   ModifiedDate = @CurrentDate,    
                   IsActive = 1,    
                   IsExpired = 0,    
                   Status = 1    
             WHERE MobileUserKey = @MobileUserKey    
               AND Productid     = @ProductId    
        END    
        ELSE    
        BEGIN    
            INSERT myBucketM (MobileUserKey,    
                              ProductId,    
                              ProductName,    
                              StartDate,    
                              EndDate,    
                              Status,    
                              CreatedBy,    
                              CreatedDate,    
                              ModifiedBy,    
                              ModifiedDate,    
                              IsActive,    
                              IsExpired)    
            VALUES (@MobileUserKey,    
                    @ProductId,    
                    @ProductName,    
                    @startDate,    
                    @endDate,    
                    1,    
                    @MobileUserKey,    
                    @CurrentDate,    
                    NULL,    
                    NULL,    
                    1,    
                    0);    
    
        END    
    
    
  -- Optimized with MERGE statement MERGE INTO myBucketM AS target      
  MERGE INTO myBucketM AS target    
  USING (   SELECT pcm.CommunityId,    
                   pc.Name AS CommunityName    
              FROM ProductCommunityMappingM pcm    
              JOIN productsm pc    
                ON pc.Id = pcm.CommunityId    
             WHERE pcm.ProductId = @ProductId    
               AND pcm.IsActive  = 1    
				and pc.isactive = 1    
				and  pc.isdeleted = 0    
      ) AS source    
     ON target.MobileUserKey = @MobileUserKey    
    AND target.ProductId = source.CommunityId    
   WHEN MATCHED THEN UPDATE SET target.ProductName = source.CommunityName,    
                                target.StartDate = @startDate,    
                                target.EndDate = @endDate,    
                                target.ModifiedBy = @MobileUserKey,    
                                target.ModifiedDate = @CurrentDate,    
                                target.IsActive = 1,    
                                target.IsExpired = 0    
   WHEN NOT MATCHED THEN    
      INSERT (MobileUserKey,    
              ProductId,    
              ProductName,    
              StartDate,    
              EndDate,    
              Status,    
              CreatedBy,    
              CreatedDate,    
              ModifiedBy,    
              ModifiedDate,    
              IsActive,    
              IsExpired)    
      VALUES (@MobileUserKey,    
              source.CommunityId,    
              source.CommunityName,    
              @startDate,    
              @endDate,    
              1,    
              @MobileUserKey,    
              @CurrentDate,    
              NULL,    
              NULL,    
              1,    
              0);    
    
    
    
        -- Check if the purchased product has an associated bonus product                
        DECLARE @BonusProductId             INT,    
                @BonusProductName           VARCHAR(100),    
                @BonusProductDurationInDays INT,    
                @BonusProductStartDate      DATETIME,    
                @BonusProductEndDate        DATETIME,    
                @ExistingBonusEndDate       DATETIME;    
    
        -- Retrieve Bonus Product details from ProductBonusMappingM                
		   SELECT @BonusProductId = pbm.BonusProductId,    
		   @BonusProductName = pm.Name,    
		   @BonusProductDurationInDays = pbm.DurationInDays    
		   FROM ProductBonusMappingM pbm  
		   JOIN ProductsM pm ON pm.Id = pbm.BonusProductId  
		   WHERE pbm.ProductId = @ProductId    
		   AND pbm.IsActive = 1  
		   AND pm.IsActive = 1  
		   AND pm.IsDeleted = 0;  
  
    
        -- If a bonus product exists, proceed with logic                
        IF @BonusProductId IS NOT NULL    
        BEGIN    
            -- Check if the user already has this bonus product in myBucketM                
            SELECT @ExistingBonusEndDate = EndDate    
              FROM myBucketM    
             WHERE MobileUserKey = @MobileUserKey    
               AND ProductId     = @BonusProductId;    
    
            -- Determine the start and end dates for the bonus product                
            IF @ExistingBonusEndDate IS NOT NULL    
            BEGIN    
                -- If the current bonus is still active, extend its validity                
                IF @ExistingBonusEndDate > @CurrentDate    
                BEGIN    
                    SET @BonusProductStartDate = @ExistingBonusEndDate;    
                    SET @BonusProductEndDate = DATEADD(DAY, @BonusProductDurationInDays, @ExistingBonusEndDate);    
                END    
                ELSE    
                BEGIN    
                    -- If the bonus product is expired, reset its validity                
                    SET @BonusProductStartDate = @CurrentDate;    
                    SET @BonusProductEndDate = DATEADD(DAY, @BonusProductDurationInDays, @CurrentDate);    
                END    
    
                -- Update the existing bonus product in myBucketM                
                UPDATE myBucketM    
                   SET EndDate = @BonusProductEndDate,    
                       ModifiedBy = @MobileUserKey,    
                       ModifiedDate = @CurrentDate,    
                       IsExpired = 0,    
                       IsActive = 1    
          WHERE MobileUserKey = @MobileUserKey    
                   AND ProductId     = @BonusProductId;    
            END    
            ELSE    
            BEGIN    
                -- If the bonus product does not exist, insert a new record                
                SET @BonusProductStartDate = @CurrentDate;    
                SET @BonusProductEndDate = DATEADD(DAY, @BonusProductDurationInDays, @CurrentDate);    
    
                INSERT INTO myBucketM (MobileUserKey,    
                                       ProductId,    
                                       ProductName,    
                                       StartDate,    
                                       EndDate,    
      Status,    
                                       CreatedBy,    
                                       CreatedDate,    
                                       ModifiedBy,    
                                       ModifiedDate,    
                                       IsActive,    
                                       IsExpired)    
                VALUES (@MobileUserKey,    
                        @BonusProductId,    
                        @BonusProductName,    
                        @BonusProductStartDate,    
                        @BonusProductEndDate,    
                        1,    
                        @MobileUserKey,    
                        @CurrentDate,    
                        NULL,    
                        NULL,    
                        1,    
                        0);    
            END    
        END;    
    
    
    
        IF EXISTS (   SELECT 1    
                        FROM [dbo].[PurchaseOrdersM]    
                       WHERE TransasctionReference = @MerchantTransactionId)    
        BEGIN    
            -- Update the existing record                          
            UPDATE [dbo].[PurchaseOrdersM]    
               SET StartDate = @CurrentDate,    
                   EndDate = @endDate,    
                   Product = @ProductName,    
                   ProductId = @ProductId,    
                   CouponKey = @couponCode,    
                   NetAmount = @price,    
                   PaidAmount = @paidAmount,    
                   ModifiedOn = @CurrentDate,    
                   Remark = Remark + ': ' + 'Re executed the procedures'    
             WHERE TransasctionReference = @MerchantTransactionId;    
        END    
        ELSE    
        BEGIN    
            INSERT INTO [dbo].[PurchaseOrdersM] ([LeadId],    
                                                 [ClientName],    
                                                 [Mobile],    
                                                 [Email],    
                                                 [DOB],    
                                                 [Remark],    
                                                 [PaymentDate],    
                                                 [ModeOfPayment],    
                                                 [BankName],    
                                                 [Pan],    
                                                 [State],    
                                                 [City],    
                                                 [TransasctionReference],    
                                                 [ProductId],    
                                                 [Product],    
                                                 [NetAmount],    
                                                 [PaidAmount],    
                                                 [CouponKey],    
                                                 [CouponDiscountAmount],    
                                                 [CouponDiscountPercent],    
                                                 [Status],    
                                                 [ActionBy],    
                                                 [PaymentStatusId],    
                                                 [PaymentActionDate],    
                                              [CreatedOn],    
                                                 [CreatedBy],    
                                                 [ModifiedOn],    
                                                 [ModifiedBy],    
                                                 [StartDate],    
                                                 [EndDate],    
                                                 [IsActive],    
                                                 [KycApproved],    
                                                 [KycApprovedDate],    
                                                 SubscriptionMappingId,    
                                                 TransactionId)    
            SELECT (SELECT Id FROM Leads WHERE PublicKey = LeadKey),    
                   fullname,    
                   mobile,    
                   emailid,    
                   Dob,    
                   NULL,    
                   @CurrentDate,    
                   (CASE    
                         WHEN @paidAmount = 0.00 THEN @couponMOP    
                         ELSE @razorMOP END) AS ModeOfPayment,    
                   NULL,    
                   NULL,    
                   NULL,    
                   City,    
                   @MerchantTransactionId,    
                   @ProductId,    
                   @ProductName,    
                   @price,    
                   @paidAmount,    
                   @couponkey,    
                   @discountAmount,    
                   @discountPercent,    
                   1,    
                   @MobileUserKey,    
                   1,    
                   @CurrentDate,    
                   @CurrentDate,    
                   @MobileUserKey,    
                   NULL,    
                   NULL,    
                   @CurrentDate,    
                   @endDate,    
                   1,    
                   1,    
                   NULL,    
                   @SubscriptionMappingId,    
                   @TransactionId    
              FROM #TempTable;    
        END;    
    
        UPDATE CouponsM    
           SET TotalRedeems = TotalRedeems + 1    
         WHERE publickey = @couponkey;    
        UPDATE MobileUsers    
           SET CancommunityPost = 1    
         WHERE publickey = @MobileUserKey;    
    
    
        DROP TABLE #TempTable    
        SELECT (   SELECT name,    
                          code,    
                          CAST(@startDate as date) AS StartDate,    
                          CAST(@endDate as date) as EndDate,    
                          (@FinalValidityInDays + 1) AS ProductValidity,    
                          @BonusProductName as BonusProduct,    
                          @BonusProductDurationInDays as BonusProductValidity    
                     FROM ProductsM    
                    WHERE id = @productId    
                   FOR JSON auto) AS JsonData    
        COMMIT TRANSACTION;    
    END TRY    
    BEGIN CATCH    
        IF @@TRANCOUNT > 0    
            ROLLBACK TRANSACTION;    
        SELECT ERROR_MESSAGE() as JsonData,    
               ERROR_LINE() AS lINE,    
               ERROR_NUMBER() AS NUMBER    
    
        INSERT INTO lOGS (Description,    
                          Source,    
                          CreatedDate)    
        VALUES (ERROR_MESSAGE() + ' :: ' + CAST(@productId AS VARCHAR) + ', ' + @MerchantTransactionId + ', '    
                + CAST(@paidAmount AS VARCHAR) + ', ' + @couponCode,    
                'ManagePurchaseOrderSM',    
                @CurrentDate)    
    END CATCH    
END


-- Ajith 18/03/2025 12:10

GO
ALTER PROCEDURE [dbo].[GetCustomers]            
 --declare                 
 @IsPaging BIT = 1            
 ,@PageSize INT = 25            
 ,@PageNumber INT = 1            
 ,@SortExpression VARCHAR(50) = NULL            
 ,@SortOrder VARCHAR(50) = NULL            
 ,@RequestedBy VARCHAR(50) = NULL            
 ,@SearchText VARCHAR(100) = NULL            
 ,@FromDate DATETIME = NULL            
 ,@ToDate DATETIME = NULL            
 ,@StrategyKey VARCHAR(50) = NULL      
 ,@SecondaryKey varchar(50) = null    
 ,@LoggedInUser VARCHAR(50) = NULL                                   
 ,@TotalCount INT = 0 OUTPUT          
 ,@TotalAmount INT =0 OUTPUT        
AS            
BEGIN            
 SET @IsPaging = 0            
  SET @PageSize = COALESCE(@PageSize, 20);            
 --set @PageNumber =1                 
 SET @SortExpression = 'callDate'            
 SET @SortOrder = 'desc'            
 --set @RequestedBy=NULL                 
 --set @FromDate='2023-03-31 11:00:31.333'                 
 --set @ToDate='2023-05-30 11:00:31.333'                 
 SET @StrategyKey = NULL            
 SET @TotalCount = 0            
 SET @SearchText = ISNULL(@SearchText, '')        
 
 IF EXISTS (select 1 from Users as us      
INNER JOIN Roles  as ro on us.RoleKey = ro.PublicKey     
WHERE us.PublicKey = @LoggedInUser and ro.Name <> 'admin')     
BEGIN -- if it's a bde      
  SET @RequestedBy = @LoggedInUser             
END       
            
 SELECT @TotalCount = count(1) ,@TotalAmount = ISNULL(sum(purchaseOrders.PaidAmount),0)       
  FROM Leads AS leads            
  INNER JOIN PurchaseOrders AS purchaseOrders ON leads.Id = purchaseOrders.LeadId            
   AND leads.AssignedTo IS NOT NULL            
   AND purchaseOrders.STATUS = (            
    SELECT Id            
    FROM STATUS            
    WHERE name = 'customer'            
    )            
  INNER JOIN Users AS users ON leads.AssignedTo = users.PublicKey            
  LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = Leads.LeadTypeKey            
  INNER JOIN STATUS AS STATUS ON STATUS.Id = purchaseOrders.STATUS            
   AND leads.AssignedTo = ISNULL(@RequestedBy, leads.AssignedTo)            
   AND CAST(purchaseOrders.paymentDate AS DATE) BETWEEN CAST(ISNULL(@FROMDATE, (DATEADD(MONTH, - 1, getdate()))) AS DATE)            
    AND CAST(ISNULL(@TODATE, GETDATE()) AS DATETIME)            
   AND (            
    leads.FullName LIKE '%' + @SearchText + '%'            
    or leads.mobilenumber LIKE '%' + @SearchText + '%')            
  WHERE ISNULL(purchaseOrders.IsExpired, 0) = 0        
     AND((  purchaseOrders.ServiceId in (SELECT CAST(value AS INT)  FROM STRING_SPLIT(@SecondaryKey, ',')  )  and @SecondaryKey is not null )  or( purchaseOrders.ServiceId = purchaseOrders.ServiceId and @SecondaryKey is null)    )              
    
 --FROM Leads AS leads            
 --INNER JOIN PurchaseOrders AS purchaseOrders ON leads.Id = purchaseOrders.LeadId            
 -- AND leads.AssignedTo IS NOT NULL            
 -- AND purchaseOrders.STATUS = (            
 --  SELECT Id            
 --  FROM STATUS            
 --  WHERE name = 'customer'            
 --  )            
 --INNER JOIN Users AS users ON leads.AssignedTo = users.PublicKey            
 --LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = Leads.LeadTypeKey            
 --INNER JOIN STATUS AS STATUS ON STATUS.Id = purchaseOrders.STATUS            
 -- AND leads.AssignedTo = ISNULL(@RequestedBy, leads.AssignedTo)            
 -- AND CAST(purchaseOrders.paymentDate AS DATE) BETWEEN CAST(ISNULL(@FROMDATE, (DATEADD(MONTH, - 1, getdate()))) AS DATE)            
 --  AND CAST(ISNULL(@TODATE, GETDATE()) AS DATETIME)            
 -- AND (            
 --   leads.FullName LIKE '%' + @SearchText + '%'            
 --   or leads.mobilenumber LIKE '%' + @SearchText + '%')            
 --WHERE ISNULL(purchaseOrders.IsExpired, 0) = 0            
            
 --and cast(GETDATE() as date) BETWEEN CAST(ISNULL(purchaseOrders.StartDate, '2020-01-01') as date) and CAST(ISNULL(purchaseOrders.EndDate, '2020-01-01') as date)                
 DROP TABLE            
            
 IF EXISTS #tempCustomer            
  SELECT Cast(ROW_NUMBER() OVER (            
     ORDER BY leads.id DESC            
     ) AS INT) AS slNo            
   ,leads.Id            
   ,leads.PublicKey AS LeadPublicKey            
  ,leads.FullName            
   ,leads.MobileNumber            
   ,leads.EmailId            
   ,LeadTypes.Name AS [LeadTypeKey]            
   ,users.FirstName AS AssignedToName            
   ,isnull(purchaseOrders.StartDate, '') AS StartDate            
   ,isnull(purchaseOrders.EndDate, '') AS EndDate            
   ,purchaseOrders.PaidAmount            
   ,IsNull(DATEDIFF(DAY, GETDATE(), purchaseOrders.EndDate), 0) AS DaysToGo            
   ,purchaseOrders.STATUS            
   ,STATUS.Name AS PrStatusName            
   ,leads.PurchaseOrderKey            
   ,purchaseOrders.City            
   ,purchaseOrders.Remark            
   ,purchaseOrders.ModeOfPayment            
   ,purchaseOrders.ServiceId            
   ,purchaseorders.paymentDate       
   ,s.name as ServiceName    
  INTO #tempCustomer            
  FROM Leads AS leads            
  INNER JOIN PurchaseOrders AS purchaseOrders ON leads.Id = purchaseOrders.LeadId            
  AND leads.AssignedTo IS NOT NULL            
   AND purchaseOrders.STATUS = (            
    SELECT Id            
    FROM STATUS            
    WHERE name = 'customer'            
    )            
  INNER JOIN Users AS users ON leads.AssignedTo = users.PublicKey            
  LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = Leads.LeadTypeKey            
  INNER JOIN STATUS AS STATUS ON STATUS.Id = purchaseOrders.STATUS      
  AND (@RequestedBy IS NULL OR (Leads.AssignedTo IS NOT NULL AND Leads.AssignedTo = @RequestedBy))        
  -- AND leads.AssignedTo = ISNULL(@RequestedBy, leads.AssignedTo)            
   AND CAST(purchaseOrders.paymentDate AS DATE) BETWEEN CAST(ISNULL(@FROMDATE, (DATEADD(MONTH, - 1, getdate()))) AS DATE)            
    AND CAST(ISNULL(@TODATE, GETDATE()) AS DATETIME)            
   AND (            
    leads.FullName LIKE '%' + @SearchText + '%'            
    or leads.mobilenumber LIKE '%' + @SearchText + '%')      
  inner join Services s  on purchaseOrders.Serviceid =s.id    
  WHERE ISNULL(purchaseOrders.IsExpired, 0) = 0         
   AND((  purchaseOrders.ServiceId in (SELECT CAST(value AS INT)  FROM STRING_SPLIT(@SecondaryKey, ',')  )  and @SecondaryKey is not null )  or( purchaseOrders.ServiceId = purchaseOrders.ServiceId and @SecondaryKey is null)    )              
    
  ORDER BY CASE             
    WHEN @SortOrder <> 'asc'            
     THEN cast(NULL AS DATE)            
    WHEN @SortExpression = 'CreatedOn'            
     THEN leads.CreatedOn            
    END ASC            
   ,CASE             
    WHEN @SortOrder <> 'asc'            
     THEN ''            
    WHEN @SortExpression = 'leadName'            
     THEN leads.FullName            
    END ASC            
   ,CASE             
    WHEN @sortOrder <> 'desc'            
     THEN ''            
    WHEN @SortExpression = 'leadName'            
     THEN leads.FullName            
    END DESC OFFSET(IIF(@PageNumber <= 1, 0, (@PageNumber - 1))) * @PageSize ROWS            
            
 FETCH NEXT @PageSize ROWS ONLY            
            
 SELECT (            
   SELECT *            
   FROM #tempCustomer            
   FOR JSON AUTO            
   ) AS JsonData            
END     
GO


GO
DROP PROCEDURE IF EXISTS [dbo].[ManagePurchaseOrderSM]
GO
-- vijay sahu 18/March/2025 16:55

-- This procedure only getting call from Mobile Flutter while doing any payment using payment gateway                             
-- Not:  Don't use this for CRM or any where elese to get the subscription in my bucket.                              
-- @paidAmount means the final amount which has been paid by the client                            
-- exec ManagePurchaseOrderSm 'BA0C9CCE-6A8F-EF11-819C-00155D23D79C', 1  , 1,'TRANSACT12312313', 1  , null                               
create PROCEDURE [dbo].[ManagePurchaseOrderSM]    
    @mobileUserKey UNIQUEIDENTIFIER,    
    @productId INT,    
    @SubscriptionMappingId int,    
    @MerchantTransactionId VARCHAR(100),    
    @TransactionId VARCHAR(100),    
    @paidAmount DECIMAL(18, 2),    
    @couponCode VARCHAR(20) = NULL -- using coupon name here                                                                   
AS    
BEGIN    
    --SELECT * fROM Services                      
    --SELECT @mobileUserKey = '0d9c40b8-289b-ef11-b318-c2aedf6978f6',                          
    --   @productId = 42,                          
    --   @SubscriptionMappingId = 58,                          
    --   @MerchantTransactionId = 'TRANSACTION02012025110520',                          
    --   @TransactionId = 'T2501021105446938436023',                          
    --   @paidAmount = 600,                          
    --   @couponCode = NULL;                          
    DECLARE @CurrentDate   DATETIME = getdate(),    
            @leadsourcekey uniqueidentifier,    
            @AdminKey      uniqueidentifier    
    SELECT @leadsourcekey = publickey    
      from LeadSources    
     where name = 'mobileapp'    
    SELECT @AdminKey = us.publickey    
      from Users as us    
     inner join Roles as ro    
        on us.RoleKey = ro.PublicKey    
     WHERE ro.Name      = 'Admin'    
       and us.PublicKey = '3CA214D0-8CB8-EB11-AAF2-00155D53687A'    
    
    BEGIN TRANSACTION;    
    BEGIN TRY    
    
    
        DECLARE @NewLeadKey   UNIQUEIDENTIFIER,    
                @MobileNumber VARCHAR(20);    
        -- Get the MobileNumber and existing LeadKey     
        SELECT @MobileNumber = Mobile,    
               @NewLeadKey = LeadKey    
          FROM MobileUsers    
         WHERE PublicKey = @MobileUserKey; -- Check if LeadKey is invalid or NULL     
        IF @NewLeadKey IS NULL    
        OR NOT EXISTS (   SELECT 1    
                            FROM Leads    
                           WHERE PublicKey = @NewLeadKey)    
        BEGIN -- Find the LeadKey using MobileNumber       
            SELECT TOP 1 @NewLeadKey = PublicKey    
              FROM Leads    
             WHERE MobileNumber = @MobileNumber; -- Update MobileUser if a valid LeadKey is found       
            IF @NewLeadKey IS NOT NULL    
            BEGIN    
                UPDATE MobileUsers    
                   SET LeadKey = @NewLeadKey    
                 WHERE PublicKey = @MobileUserKey;    
            END    
            ELSE    
            BEGIN    
                SELECT @NewLeadKey = NEWID()    
                INSERT INTO Leads (SupervisorId,PublicKey,    FullName,    
                                   Gender,MobileNumber,AlternateMobileNumber,EmailId,    
                                   ProfileImage,PriorityStatus,AssignedTo,ServiceKey,    
                                   LeadTypeKey,LeadSourceKey,Remarks,IsDisabled,    
                                   IsDelete,CreatedOn,CreatedBy,IsSpam,IsWon,ModifiedOn,    
                                   ModifiedBy,City,PinCode,StatusId,    
                                   PurchaseOrderKey,Favourite,CountryCode)    
                SELECT NULL,    
                       @NewLeadKey,FullName,    
                       (CASE    
                             WHEN Gender = 'male' then 'm'    
                             else 'f' end),    
                       Mobile,NULL,EmailId,NULL,1,NULL,    
                       'C11DAA9F-F125-EE11-811D-00155D23D79C', --admin
                       NULL,@leadsourcekey,'Reg. via mobile app',0,    
                       0,@CurrentDate,@AdminKey,0,    
                       0,@CurrentDate,@AdminKey,City,'',1,NULL,1,CountryCode    
                  FROM MobileUsers    
                 where PublicKey = @mobileUserKey;    
                UPDATE MobileUsers    
                   set LeadKey = @NewLeadKey    
                 where PublicKey = @mobileUserKey    
            END    
        END    
        DECLARE @couponMOP INT,    
                @razorMOP  INT    
        SELECT @couponMOP = Id    
          FROM PaymentModes    
         WHERE name = 'Coupon';    
        SELECT @razorMOP = Id    
          FROM PaymentModes    
         WHERE name = 'Razor';    
        DROP TABLE IF EXISTS #TempTable    
        SELECT *    
        INTO   #TempTable    
          FROM MobileUsers    
         WHERE publickey = @MobileUserKey --'FE05087D-6AA6-EF11-B32F-852B0D908F81'                                    
    
        DECLARE @couponCodeExists         BIT,    
                @discountAmount           DECIMAL(18, 2),    
                @discountPercent          INT,    
                @price                    DECIMAL(18, 2),    
                @couponkey                UNIQUEIDENTIFIER,    
                @startDate                DATETIME        = @currentDate,    
                @endDate                  datetime,    
                @couponHasProductValidity bit,    
                @couponValidityDays       int             = 0,    
                @FinalValidityInDays      int             = 0,    
                @ProductName              varchar(100)    = '',    
                @validity                 int             = null;    
    
        SELECT @couponkey = publickey,    
               @validity = ProductValidityInDays    
          FROM CouponsM    
         WHERE Name        = @couponCode    
           AND IsActive    = 1    
           AND IsDelete    = 0    
           AND RedeemLimit > TotalRedeems    
    
        -- Calculate the NetAmount ,DiscountAmount based on SubscriptionMappingId                                  
        -- Because each product coming under atleast 1 plan , and price may very based on Plan or Durations.                                   
        SELECT @discountPercent = sm.DiscountPercentage,    
               @price = p.Price,    
               @discountAmount = ((p.Price * (sm.DiscountPercentage) / 100)),    
               @couponValidityDays = 0,    
               @endDate = DATEADD(month, sd.Months, GETDATE()),    
               @ProductName = p.Name,    
               @FinalValidityInDays = DATEDIFF(DAY, (@currentDate), (DATEADD(month, sd.Months, @currentDate))) - 1    
          FROM SubscriptionMappingM as sm    
         INNER JOIN SubscriptionDurationM as sd    
            on sm.SubscriptionDurationId = sd.Id    
         INNER JOIN ProductsM as p    
            on sm.ProductId              = p.Id    
         WHERE sm.Id       = @SubscriptionMappingId    
           and ProductId   = @productId    
           and sm.IsActive = 1    
    
        -- SET @paidAmount = @price - isnull(@discountAmount,0);                  
        SELECT @couponHasProductValidity = IIF(ProductValidityInDays IS NULL, 0, 1),    
               @couponValidityDays = ProductValidityInDays,    
               @discountPercent = IIF(DiscountInPercentage IS NULL, 0, 1)    
          FROM CouponsM c    
         WHERE publickey = @couponkey;    
    
        -- Now change the final Paid Amount after applying the coupon code on it.                                  
        -- set @paidAmount = @paidAmount - (@paidAmount * (@discountPercent / 100))                       
    
        --Validity days change if coupon validity is greater then Subscription Validity in days                             
        IF (@couponValidityDays > @FinalValidityInDays)    
        BEGIN    
            SET @FinalValidityInDays = @couponValidityDays    
        END    
    
        IF EXISTS (   SELECT 1 FROM myBucketM where MobileUserKey = @MobileUserKey AND ProductId     = @ProductId)    
        BEGIN    
    
            SELECT @startDate = CASE    
                                     WHEN EndDate > @CurrentDate THEN StartDate    
                                     ELSE @CurrentDate END,    
                   @EndDate = (CASE    
                                    WHEN EndDate > @CurrentDate THEN DATEADD(DAY, @FinalValidityInDays, EndDate)    
                                    ELSE DATEADD(DAY, @FinalValidityInDays + 1, @CurrentDate) END)    
              FROM MYBucketM    
             WHERE MobileUserKey = @MobileUserKey    
               AND ProductId     = @ProductId    
    
            UPDATE MYBucketM    
               SET ProductName = @ProductName,    
                   StartDate = CASE    
                                    WHEN EndDate > @CurrentDate THEN StartDate    
                                    ELSE @CurrentDate END,    
                   EndDate = @EndDate,    
                   ModifiedBy = @mobileUserKey,    
                   ModifiedDate = @CurrentDate,    
                   IsActive = 1,    
                   IsExpired = 0,    
                   Status = 1    
             WHERE MobileUserKey = @MobileUserKey    
               AND Productid     = @ProductId    
        END    
        ELSE    
        BEGIN    
            INSERT myBucketM (MobileUserKey,ProductId,ProductName,StartDate,EndDate,Status,    
                              CreatedBy,CreatedDate,ModifiedBy,ModifiedDate,    
                              IsActive,IsExpired)    
            VALUES (@MobileUserKey,@ProductId,@ProductName,@startDate,@endDate,    
                    1,@MobileUserKey,@CurrentDate,NULL,NULL,1,0);    
        END    
    
    
  -- Optimized with MERGE statement to insert if the product has mapped with any Community
  declare @IsOldDevice bit = 0   
  select  @IsOldDevice = CASE 
      WHEN mu.DeviceVersion IS NULL THEN 1  -- If NULL, mark as old
      WHEN mu.DeviceType = 'iOS' 
           AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1 
           AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0 
           AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 16 THEN 1 -- Old iOS device
      
      WHEN mu.DeviceType = 'Android' 
           AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1 
           AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0 
           AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 61 THEN 1 -- Old Android device
      
      ELSE 0 
  END
  FROM MobileUsers AS mu WHERE PublicKey = @mobileUserKey

  IF(@IsOldDevice =0)
  BEGIN
  MERGE INTO myBucketM AS target    
  USING (   SELECT pcm.CommunityId,    
                   pc.Name AS CommunityName    
              FROM ProductCommunityMappingM pcm    
              JOIN productsm pc    
                ON pc.Id = pcm.CommunityId    
             WHERE pcm.ProductId = @ProductId    
               AND pcm.IsActive  = 1    
      AND pc.isactive = 1    
      AND  pc.isdeleted = 0    
      ) AS source    
     ON target.MobileUserKey = @MobileUserKey    
    AND target.ProductId = source.CommunityId    
   WHEN MATCHED THEN UPDATE SET target.ProductName = source.CommunityName,    
                                target.StartDate = @startDate,    
                                target.EndDate = @endDate,    
                                target.ModifiedBy = @MobileUserKey,    
                                target.ModifiedDate = @CurrentDate,    
                                target.IsActive = 1,    
                                target.IsExpired = 0    
   WHEN NOT MATCHED THEN    
      INSERT (MobileUserKey,ProductId,ProductName,    
              StartDate,EndDate,Status,    
              CreatedBy,CreatedDate,ModifiedBy,    
              ModifiedDate,IsActive,IsExpired)    
      VALUES (@MobileUserKey,source.CommunityId,source.CommunityName,    
              @startDate,@endDate,1,@MobileUserKey,@CurrentDate,NULL,NULL,1,0);    
    
  END
  
    
    
        -- Check if the purchased product has an associated bonus product                
        DECLARE @BonusProductId             INT,    
                @BonusProductName           VARCHAR(100),    
                @BonusProductDurationInDays INT,    
                @BonusProductStartDate      DATETIME,    
                @BonusProductEndDate        DATETIME,    
                @ExistingBonusEndDate       DATETIME;    
    
        -- Retrieve Bonus Product details from ProductBonusMappingM                
		SELECT @BonusProductId = pbm.BonusProductId,    
		@BonusProductName = pm.Name,    
		@BonusProductDurationInDays = pbm.DurationInDays    
		FROM ProductBonusMappingM pbm  
		JOIN ProductsM pm ON pm.Id = pbm.BonusProductId  
		WHERE pbm.ProductId = @ProductId    
		AND pbm.IsActive = 1  
		AND pm.IsActive = 1  
		AND pm.IsDeleted = 0;  
  
    
        -- If a bonus product exists, proceed with logic                
        IF @BonusProductId IS NOT NULL    
        BEGIN    
            -- Check if the user already has this bonus product in myBucketM                
            SELECT @ExistingBonusEndDate = EndDate    
              FROM myBucketM    
             WHERE MobileUserKey = @MobileUserKey    
               AND ProductId     = @BonusProductId;    
    
            -- Determine the start and end dates for the bonus product                
            IF @ExistingBonusEndDate IS NOT NULL    
            BEGIN    
                -- If the current bonus is still active, extend its validity                
                IF @ExistingBonusEndDate > @CurrentDate    
                BEGIN    
                    SET @BonusProductStartDate = @ExistingBonusEndDate;    
                    SET @BonusProductEndDate = DATEADD(DAY, @BonusProductDurationInDays, @ExistingBonusEndDate);    
                END    
                ELSE    
                BEGIN    
                    -- If the bonus product is expired, reset its validity                
                    SET @BonusProductStartDate = @CurrentDate;    
                    SET @BonusProductEndDate = DATEADD(DAY, @BonusProductDurationInDays, @CurrentDate);    
                END    
    
                -- Update the existing bonus product in myBucketM                
                UPDATE myBucketM    
                   SET EndDate = @BonusProductEndDate,    
                       ModifiedBy = @MobileUserKey,    
                       ModifiedDate = @CurrentDate,    
                       IsExpired = 0,    
                       IsActive = 1    
          WHERE MobileUserKey = @MobileUserKey    
                   AND ProductId     = @BonusProductId;    
            END    
            ELSE    
            BEGIN    
                -- If the bonus product does not exist, insert a new record                
                SET @BonusProductStartDate = @CurrentDate;    
                SET @BonusProductEndDate = DATEADD(DAY, @BonusProductDurationInDays, @CurrentDate);    
    
                INSERT INTO myBucketM (MobileUserKey,    
                                       ProductId,    
                                       ProductName,    
                                       StartDate,    
                                       EndDate,    
      Status,    
                                       CreatedBy,    
                                       CreatedDate,    
                                       ModifiedBy,    
                                       ModifiedDate,    
                                       IsActive,    
                                       IsExpired)    
                VALUES (@MobileUserKey,    
                        @BonusProductId,    
                        @BonusProductName,    
                        @BonusProductStartDate,    
                        @BonusProductEndDate,    
                        1,    
                        @MobileUserKey,    
                        @CurrentDate,    
                        NULL,    
                        NULL,    
                        1,    
                        0);    
            END    
        END;    
    
    
    
        IF EXISTS (   SELECT 1    
                        FROM [dbo].[PurchaseOrdersM]    
                       WHERE TransasctionReference = @MerchantTransactionId)    
        BEGIN    
            -- Update the existing record                          
            UPDATE [dbo].[PurchaseOrdersM]    
               SET StartDate = @CurrentDate,    
                   EndDate = @endDate,    
                   Product = @ProductName,    
                   ProductId = @ProductId,    
                   CouponKey = @couponCode,    
                   NetAmount = @price,    
                   PaidAmount = @paidAmount,    
                   ModifiedOn = @CurrentDate,    
                   Remark = Remark + ': ' + 'Re executed the procedures'    
             WHERE TransasctionReference = @MerchantTransactionId;    
        END    
        ELSE    
        BEGIN    
            INSERT INTO [dbo].[PurchaseOrdersM] ([LeadId],[ClientName],[Mobile],[Email],    
                                                 [DOB],[Remark],[PaymentDate],[ModeOfPayment],    
                                                 [BankName],[Pan],[State],[City],    
                                                 [TransasctionReference],[ProductId],    
                                                 [Product],[NetAmount],[PaidAmount],[CouponKey],    
                                                 [CouponDiscountAmount],[CouponDiscountPercent],    
                                                 [Status],[ActionBy],[PaymentStatusId],    
                                                 [PaymentActionDate],[CreatedOn],[CreatedBy],    
                                                 [ModifiedOn],[ModifiedBy],[StartDate],[EndDate],    
                                                 [IsActive],[KycApproved],[KycApprovedDate],    
                                                 SubscriptionMappingId,TransactionId)    
            SELECT (SELECT Id FROM Leads WHERE PublicKey = LeadKey),    
                   fullname,mobile,emailid,Dob,NULL,@CurrentDate,    
                   (CASE    
                         WHEN @paidAmount = 0.00 THEN @couponMOP    
                         ELSE @razorMOP END) AS ModeOfPayment,    
                   NULL,NULL,NULL,City,@MerchantTransactionId,    
                   @ProductId,@ProductName,@price,@paidAmount,    
                   @couponkey,@discountAmount,@discountPercent,1,    
                   @MobileUserKey,1,@CurrentDate,@CurrentDate,@MobileUserKey,    
                   NULL,NULL,@CurrentDate,@endDate,1,1,NULL,@SubscriptionMappingId,@TransactionId    
              FROM #TempTable;    
        END;    
    
        UPDATE CouponsM    
           SET TotalRedeems = TotalRedeems + 1    
         WHERE publickey = @couponkey;    
        UPDATE MobileUsers    
           SET CancommunityPost = 1    
         WHERE publickey = @MobileUserKey;    
    
    
        DROP TABLE #TempTable    
        SELECT (   SELECT name,    
                          code,    
                          CAST(@startDate as date) AS StartDate,    
                          CAST(@endDate as date) as EndDate,    
                          (@FinalValidityInDays + 1) AS ProductValidity,    
                          @BonusProductName as BonusProduct,    
                          @BonusProductDurationInDays as BonusProductValidity    
                     FROM ProductsM    
                    WHERE id = @productId    
                   FOR JSON auto) AS JsonData    
        COMMIT TRANSACTION;    
    END TRY    
    BEGIN CATCH    
        IF @@TRANCOUNT > 0    
            ROLLBACK TRANSACTION;    
        SELECT ERROR_MESSAGE() as JsonData,    
               ERROR_LINE() AS lINE,    
               ERROR_NUMBER() AS NUMBER    
    
        INSERT INTO lOGS (Description,    
                          Source,    
                          CreatedDate)    
        VALUES (ERROR_MESSAGE() + ' :: ' + CAST(@productId AS VARCHAR) + ', ' + @MerchantTransactionId + ', '    
                + CAST(@paidAmount AS VARCHAR) + ', ' + @couponCode,    
                'ManagePurchaseOrderSM',    
                @CurrentDate)    
    END CATCH    
END
GO


--vijay sahu 19-03-2025 10:35 
GO
ALTER PROCEDURE [dbo].[GetUserTopics]                         
 @mobileUserKey UNIQUEIDENTIFIER                            
AS                            
BEGIN                            
  DECLARE                      
  @quote VARCHAR(1000)  = ''                    
 ,@author VARCHAR(100)  = ''                    
 ,@DiscountName VARCHAR(100) = ''                     
 ,@DiscountStatus BIT = 0                    
          
  SELECT TOP 1                     
  @quote = QuoteText,         
  @author = Author,         
  @DiscountStatus = CAST(1 AS BIT),         
  @DiscountName  = ''                     
  FROM QuotesM                            
  ORDER BY NEWID();                          
                      
  DECLARE @FreeTrialBasketId INT                 
          
  SELECT TOP 1         
  @DiscountName = Name,         
  @FreeTrialBasketId = Id         
  FROM FreeTrialBasketM         
  WHERE IsExpired = 0         
  AND IsActive = 1          
  AND GETDATE() BETWEEN StartDate AND EndDate;                    
                    
  IF (ISNULL(@FreeTrialBasketId, 0) = 0)              
  BEGIN              
    SET @DiscountStatus = 0;             
  END              
  ELSE              
  BEGIN              
            
    SET @DiscountStatus = CASE   
 WHEN (SELECT COUNT(DISTINCT ProductId)          
 FROM MYBucketM         
 WHERE MobileUserKey = @mobileUserKey           
 AND ProductId IN (1,3,5)) = 3 THEN 0         
              
      WHEN EXISTS (        
        SELECT 1             
        FROM FreeTrialM         
        WHERE MobileUserKey = @mobileUserKey            
          AND FreeTrialBasketId = @FreeTrialBasketId              
          AND IsActive = 1           
      ) THEN 0         
              
      ELSE 1        
    END;               
  END;                
                 
  SELECT                    
  @quote AS Quote,        
  @author AS Author,        
  @DiscountName AS DiscountName,         
  @DiscountStatus AS DiscountStatus,                   
  'Activate' AS ButtonText,         
  'diwali2024joiningbonus.jpg' AS ImageUrl,     
  'https://youtube.com/shorts/N5JZONp8WIc?feature=share' as promotionUrl,    
  'https://www.youtube.com/shorts/jiD8aY9B7zY' as AppIntroductionUrl,
  'click' AS ActionUrl;                  
END;
Go

-- Ajith 20-03-2025 5:10PM

GO
ALTER PROCEDURE GetBonusProductMappings 
    @SearchText NVARCHAR(100) = NULL, 
    @PageNumber INT = 1, 
    @PageSize INT = 10, 
    @FromDate DATETIME = NULL, 
    @ToDate DATETIME = NULL, 
    @Status NVARCHAR(20) = NULL, 
    @TotalCount INT OUTPUT 
AS  
BEGIN 
    SET NOCOUNT ON; 

    -- Ensure valid date range
    SET @FromDate = COALESCE(@FromDate, '1900-01-01');  -- Default to old date if NULL 
    SET @ToDate = COALESCE(@ToDate, GETDATE());  -- Default to today if NULL 
    SET @ToDate = DATEADD(SECOND, -1, DATEADD(DAY, 1, @ToDate));  -- Ensure full day is included

    -- Count total matching records
    SELECT @TotalCount = COUNT(*) 
    FROM ProductBonusMappingM pbm 
        LEFT JOIN ProductsM p1 ON pbm.ProductId = p1.Id 
        LEFT JOIN ProductsM p2 ON pbm.BonusProductId = p2.Id 
        LEFT JOIN Users u1 ON pbm.CreatedBy = u1.Id 
        LEFT JOIN Users u2 ON pbm.ModifiedBy = u2.Id 
    WHERE 
        (@SearchText IS NULL OR 
         p1.Name LIKE '%' + @SearchText + '%' OR  
         p2.Name LIKE '%' + @SearchText + '%')  
        AND pbm.CreatedOn >= @FromDate 
        AND pbm.CreatedOn <= @ToDate 
        AND (  
            @Status IS NULL OR  
            (@Status = 'ACTIVE' AND pbm.IsActive = 1 AND pbm.IsDeleted = 0) OR 
            (@Status = 'INACTIVE' AND pbm.IsActive = 0 AND pbm.IsDeleted = 0) OR 
            (@Status = 'DELETED' AND pbm.IsDeleted = 1) 
        ); 

    -- Fetch paginated results
    SELECT 
        pbm.Id, 
        pbm.ProductId, 
        pbm.DurationInDays,
        p1.Name AS ProductName, 
        pbm.BonusProductId, 
        p2.Name AS BonusProductName, 
        pbm.IsActive, 
        pbm.IsDeleted, 
        pbm.CreatedOn, 
        pbm.ModifiedOn, 
        u1.FirstName + ' ' + u1.LastName AS CreatedBy, 
        u2.FirstName + ' ' + u2.LastName AS ModifiedBy 
    FROM ProductBonusMappingM pbm 
        LEFT JOIN ProductsM p1 ON pbm.ProductId = p1.Id 
        LEFT JOIN ProductsM p2 ON pbm.BonusProductId = p2.Id 
        LEFT JOIN Users u1 ON pbm.CreatedBy = u1.Id 
        LEFT JOIN Users u2 ON pbm.ModifiedBy = u2.Id 
    WHERE 
        (@SearchText IS NULL OR  
         p1.Name LIKE '%' + @SearchText + '%' OR  
         p2.Name LIKE '%' + @SearchText + '%')  
        AND pbm.CreatedOn >= @FromDate  
        AND pbm.CreatedOn <= @ToDate  
        AND (  
            @Status IS NULL OR 
            (@Status = 'ACTIVE' AND pbm.IsActive = 1 AND pbm.IsDeleted = 0) OR 
            (@Status = 'INACTIVE' AND pbm.IsActive = 0 AND pbm.IsDeleted = 0) OR 
            (@Status = 'DELETED' AND pbm.IsDeleted = 1) 
        ) 
    ORDER BY pbm.ModifiedOn DESC  
    OFFSET (@PageNumber - 1) * @PageSize ROWS 
    FETCH NEXT @PageSize ROWS ONLY; 
END;

GO

-- Add IsDeleted if not exists
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'ProductBonusMappingM' AND COLUMN_NAME = 'IsDeleted')
BEGIN
    ALTER TABLE ProductBonusMappingM ADD IsDeleted BIT NOT NULL DEFAULT 0;
END;

-- Add ModifiedBy if not exists
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'ProductBonusMappingM' AND COLUMN_NAME = 'ModifiedBy')
BEGIN
    ALTER TABLE ProductBonusMappingM ADD ModifiedBy INT NULL;
END;

-- Add CreatedBy with a default value to avoid errors
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'ProductBonusMappingM' AND COLUMN_NAME = 'CreatedBy')
BEGIN
    ALTER TABLE ProductBonusMappingM ADD CreatedBy INT NULL;
END;

GO
ALTER PROCEDURE [dbo].[GetPurchaseOrdersM]             
    @IsPaging bit = 1,             
    @PageSize int = 10,             
    @PageNumber int = 1,             
    @SortExpression varchar(50),             
    @SortOrder varchar(20),             
    @RequestedBy varchar(100) = null,             
    @FromDate datetime = null,             
    @ToDate datetime = null,             
    @SearchText varchar(100) = null,             
    @PrimaryKey varchar(100) = null,             
    @TotalCount INT OUTPUT,             
    @TotalSales DECIMAL(18, 2) OUTPUT             
AS             
BEGIN             
    IF @SearchText = '' SET @SearchText = null;             
    IF @RequestedBy = '' SET @RequestedBy = null;             
    WITH DistinctPO AS (   
        SELECT DISTINCT    
            PO.Id,    
         PO.PaidAmount AS PaidAmount   
        FROM PurchaseOrdersM AS PO             
        LEFT JOIN Users AS CreatedUser ON CreatedUser.PublicKey = PO.CreatedBy              
        LEFT JOIN PaymentModes AS pm ON pm.Id = PO.ModeOfPayment            
        LEFT JOIN ProductsM AS pd ON pd.Id = PO.ProductId           
        LEFT JOIN Status AS st ON st.Id = PO.Status             
        LEFT JOIN PaymentRequestStatusM AS PR ON PR.ProductId = PO.ProductId            
        JOIN PhonePePaymentResponseM AS PP ON PP.MerchanttransactionId = PR.TransactionId             
        WHERE    
            (PO.Mobile LIKE '%' + @SearchText + '%' OR             
             @SearchText IS NULL OR             
             PO.Email LIKE '%' + @SearchText + '%' OR             
             PO.ClientName LIKE '%' + @SearchText + '%')             
            AND ((PO.ProductId IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@PrimaryKey, ',') ) AND @PrimaryKey IS NOT NULL)             
            OR (PO.ProductId = PO.ProductId AND @PrimaryKey IS NULL))   
            AND PO.IsActive <> 0     
            AND CAST(PO.PaymentDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)   
    )   
    SELECT    
        @TotalCount = COUNT(DISTINCT Id),   
        @TotalSales = SUM(PaidAmount)   
    FROM DistinctPO;   
   
    SELECT DISTINCT       
        PO.Id,             
        PO.LeadId,             
        PO.ClientName,             
        PO.Mobile,             
        PO.Email,             
        PO.DOB,             
        ISNULL(PO.Remark, '') AS Remark,             
        CAST(PO.PaymentDate AS smalldatetime) AS PaymentDate,             
        pm.Name AS ModeOfPayment,             
        ISNULL(PO.BankName, '') AS BankName,             
        ISNULL(PO.Pan, '') AS Pan,             
        ISNULL(PO.State, '') AS State,             
        PO.City,             
        ISNULL(PO.TransasctionReference, '') AS TransasctionReference,             
        PO.ProductId,             
        ISNULL(PO.Product, '') AS Product,             
        ISNULL(PO.NetAmount, 0) AS NetAmount,             
        PO.PaidAmount,             
        PO.Status,             
        CAST(PO.CreatedOn AS smalldatetime) AS CreatedOn,             
        CAST(PO.ActionBy AS varchar(50)) AS ActionBy,             
        CreatedUser.FirstName + ' ' + CreatedUser.Lastname AS CreatedBy,             
        st.Name AS StatusName,             
        pd.Name AS ProductName,             
        PO.StartDate,             
        PO.EndDate,             
        PO.PublicKey,             
        CreatedUser.FirstName AS FirstName,             
        DATEDIFF(day, ISNULL(PO.StartDate, GETDATE()), ISNULL(PO.EndDate, GETDATE())) AS DaysToGo,             
        PP.State AS PaymentStatus             
    FROM PurchaseOrdersM AS PO             
    LEFT JOIN Users AS CreatedUser ON PO.CreatedBy = CreatedUser.PublicKey             
    LEFT JOIN PaymentModes AS pm ON PO.ModeOfPayment = pm.Id             
    LEFT JOIN ProductsM AS pd ON pd.Id = PO.ProductId             
    LEFT JOIN Status AS st ON st.Id = PO.Status             
  LEFT JOIN (  
    -- Selecting the latest TransactionId per ProductId  
    SELECT ProductId, MAX(TransactionId) AS TransactionId  
    FROM PaymentRequestStatusM  
    GROUP BY ProductId  
) AS PR ON PR.ProductId = PO.ProductId              
    JOIN PhonePePaymentResponseM AS PP ON PR.TransactionId = PP.MerchanttransactionId             
    WHERE    
        (PO.Mobile LIKE '%' + @SearchText + '%' OR             
         @SearchText IS NULL OR             
         PO.Email LIKE '%' + @SearchText + '%' OR             
         PO.ClientName LIKE '%' + @SearchText + '%')             
        AND ((PO.ProductId IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@PrimaryKey, ',') ) AND @PrimaryKey IS NOT NULL)             
        OR (PO.ProductId = PO.ProductId AND @PrimaryKey IS NULL))      
        AND PO.IsActive <> 0     
        AND CAST(PO.PaymentDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)             
    ORDER BY Status, PaymentDate DESC             
    OFFSET (CASE WHEN @PageNumber <= 1 THEN 0 ELSE (@PageNumber - 1) * @PageSize END) ROWS             
    FETCH NEXT @PageSize ROWS ONLY;             
END;   
GO


--------- Guna Surya Goud 21-03-2025 3:44PM -------------------      
--exec [GetProductById] 1,'294A46B7-E918-EF11-B260-A630D7B5F938'            
CREATE PROCEDURE [dbo].[GetProductById]                    
    @ProductId INT,                    
    @MobileUserKey UNIQUEIDENTIFIER                    
AS                    
BEGIN --declare @ProductId int = 5                                                    
    --,@MobileUserKey UNIQUEIDENTIFIER = '294A46B7-E918-EF11-B260-A630D7B5F938'                                                  
    DECLARE @priorDaysInfo INT = CAST(                    
                                 (                    
                                     SELECT TOP 1 value FROM settings WHERE code = 'PRIORDAYSINFO'                    
                                 ) AS INT)                    
    DECLARE @ContentCount INT = 0,                    
            @VideoContent INT = 0                    
                    
    SELECT @ContentCount = COUNT(Id),                    
           @VideoContent = COUNT(   CASE                    
                                        WHEN attachmentType = 'video' THEN                    
                                            1                    
                                        ELSE                    
                                            NULL                    
                                    END                    
                                )                    
    FROM PRODUCTSCONTENTM         
    WHERE ProductId = @ProductId                    
          AND isActive = 1                    
          AND isDeleted = 0;                    
                    
                    
    DROP TABLE IF EXISTS #tempBenefits                    
    SELECT ben.Id,                    
           ben.GiftProductId,                    
           sub.NAME AS Names,                    
           ben.Months,                    
           pro.NAME,                    
           pro.Description                    
    INTO #tempBenefits                    
    FROM ExtraBenefitsM AS ben                    
        INNER JOIN ProductsM AS pro                    
            ON ben.GiftProductId = pro.Id                    
        INNER JOIN SubscriptionDurationM AS sub                    
            ON sub.Id = ben.SubscriptionId                    
               AND isnull(sub.isActive, 1) = 1                    
    WHERE ben.ProductId = @ProductId                    
          AND ISNULL(pro.IsActive, 1) = 1                    
                    
    DECLARE @extraBenefits NVARCHAR(MAX) = ( SELECT * FROM #tempBenefits FOR JSON AUTO)                    
    DECLARE @CurrentDate DATE = cast(getdate() AS DATE)             
         
    DECLARE @IsOutOfSubscription VARCHAR(300) = (                    
                                                    SELECT TOP 1 mobileUserKey                    
                                                    FROM MYBucketM b                    
                                                    WHERE productId = @ProductId                    
                                                          AND mobileUserKey = @MobileUserKey                    
                                                          AND ISNULL(IsActive, 1) = 1                    
                                                          AND isnull(IsExpired, 0) = 0                    
                                                          AND @currentDate >= cast(b.StartDate AS DATE)                    
                                                          AND @currentDate <= cast(b.endDate AS DATE)                    
                                                )                    
    DECLARE @isExpired NVARCHAR(max) = (                    
                                           SELECT TOP 1 IsExpired FROM MYBucketM                    
                                           WHERE productId = @ProductId                    
                                                 AND mobileUserKey = @MobileUserKey                 
                                                 AND ISNULL(IsACtive, 1) = 1                    
                                   AND isnull(IsExpired, 0) = 0                    
                                       )              
          
    SELECT TOP 1                    
        p.id,                    
        p.NAME,                    
        p.Description,            
        p.DescriptionTitle,                  
        p.CategoryID,                    
        pcm.NAME AS Category,                    
        CAST(p.Price AS DECIMAL(16, 4)) AS Price,                    
        cast(pom.CouponKey AS VARCHAR(200)) AS CouponCode,                    
        isnull(pom.PaidAmount, 0.0) AS PaidAmount,                    
        CAST(ISNULL(pom.CouponDiscountAmount, 0.0) AS VARCHAR) AS Discount,                    
        CAST(ISNULL(PR.Rating, 0) AS VARCHAR) AS UserRating,                    
        '' AS Liked,                    
        '' AS EnableSubscription,                    
  (                    
            SELECT top 1 sv.DurationName                    
            FROM SubscriptionView sv                    
            WHERE ProductId = @ProductId                    
                  and sv.DurationName != 'free' AND sv.DurationActive = 1                    
        ) AS SubscriptionData,                    
        CAST(ISNULL(pl.LikeId, 0) AS BIT) AS IsHeart,                    
        CAST(0 AS BIT) AS IsThumbsUp,                    
        @extraBenefits AS ExtraBenefits,                    
        CAST(CASE                    
                 WHEN DATEDIFF(day, GETDATE(), mb.enddate) <= @priorDaysInfo THEN 1                    
                 ELSE 0                    
             END AS BIT) AS ShowReminder,                    
        CAST(IIF(@isExpired IS NOT NULL, 1, 0) AS BIT) AS IsInMyBucket,                    
        P.LandscapeImage AS LandscapeImage,                    
        CAST(IIF(@IsOutOfSubscription IS NULL, 0, 1) AS BIT) AS IsInValidity,                    
        (SELECT * FROM ProductsContentM WHERE productId = @ProductId FOR JSON AUTO) AS Content,                    
        (                    
   --buy button text                                                          
   CASE               
   -- Case for Active Products              
   WHEN p.isActive = 1 THEN               
    CASE WHEN mb.id IS NULL THEN 'Buy'                
    ELSE CASE               
    WHEN DATEDIFF(day, GETDATE(), mb.enddate) < @priorDaysInfo THEN 'Renew'                
    ELSE 'Purchased'                
    END               
   END                
              
   -- Case for Inactive Products              
   WHEN p.isActive = 0 THEN               
   CASE               
   WHEN mb.id IS NOT NULL THEN 'Purchased'                
   WHEN mb.id IS NULL THEN ''                
   END                
   END        
  ) AS BuyButtonText,              
        @ContentCount AS ContentCount,                    
        @VideoContent AS VideoCount,                    
        DATEDIFF(day, getdate(), ISNULL(mb.enddate, GETDATE())) AS DaysToGo,                  
    (        
  CASE  WHEN mb.id IS NOT NULL AND DATEDIFF(day, GETDATE(), mb.enddate) >= @priorDaysInfo   THEN '[]'            
  ELSE             
  (  SELECT * FROM        
        (        
            SELECT        
                pb.BonusProductId AS Id,        
                p2.NAME AS BonusProductName,        
                pb.DurationInDays AS Validity,        
                (SELECT s.Value FROM Settings s WHERE s.Code = 'BonusMessage' AND s.IsActive = 1) AS BonusMessage        
            FROM        
                ProductBonusMappingM pb        
            INNER JOIN        
                ProductsM p2 ON pb.BonusProductId = p2.Id        
            LEFT JOIN        
                ProductCategoriesM pcm2 ON p2.CategoryID = pcm2.Id        
            WHERE        
                pb.ProductId = p.id AND pb.IsActive = 1 AND p2.IsActive = 1 AND p2.IsDeleted = 0        
            UNION ALL        
   SELECT         
   pcm.CommunityId,max(pTemp.Name) as BonusProductName,          
   min(sd.Months * 30) as Validity, (SELECT s.Value FROM Settings s WHERE s.Code = 'BonusMessage' AND s.IsActive = 1) AS BonusMessage          
   From ProductCommunityMappingM as pcm         
   inner join SubscriptionMappingM as sm on pcm.CommunityId = sm.ProductId        
   inner join SubscriptionDurationM as sd on sm.SubscriptionDurationId = sd.Id        
   inner join ProductsM as pTemp on pTemp.Id = pcm.CommunityId        
   where pcm.ProductId = p.Id and ISNULL(pTemp.IsActive,1) = 1 and ISNULL(pTemp.IsDeleted,0) = 0       
  and ISNULL(pcm.IsActive,1) = 1 and ISNULL(pcm.IsDeleted,0) = 0       
   and ISNULL(sm.IsActive,1) = 1 and ISNULL(sd.IsActive,1) = 1        
   group by pcm.CommunityId         
        ) AS UnionedResults        
        FOR JSON PATH)            
  END         
          
 ) AS BonusProducts                    
    FROM ProductsM AS P                    
        INNER JOIN ProductCategoriesM AS pcm ON p.CategoryID = pcm.Id                    
        LEFT JOIN PurchaseOrdersM AS POM ON POM.ProductId = p.Id                    
               AND pom.ProductId = @ProductId AND pom.ActionBy = @MobileUserKey                    
        LEFT JOIN ProductsRatingM AS PR ON PR.ProductId = P.Id AND PR.CreatedBy = @MobileUserKey                    
        LEFT JOIN ProductLikesM AS pl ON pl.ProductId = p.Id                    
               AND pl.LikeId = 1 AND pl.CreatedBy = @MobileUserKey AND pl.IsDelete = 0                    
        LEFT JOIN subscriptiondurationm s ON s.Id = p.SubscriptionId                    
        LEFT JOIN MYBucketM AS Mb ON p.id = mb.ProductId AND mb.mobileuserkey = @MobileUserkey                    
    WHERE p.id = @ProductId                    
    ORDER BY POM.CreatedOn DESC                    
END



------- Created by Guna Surya on 24-March-2025 -----------------------------------
Go
--exec GetCommunityDetails 20838   
alter PROCEDURE GetCommunityDetails       
@MobileUserId bigint       
AS BEGIN      
      
SELECT   mb.ProductId  , pm.Name as ProductName , pm.code as ProductCode         
FROM MyBucketM mb     
JOIN MobileUsers mu ON mb.MobileUserKey = mu.PublicKey   
JOIN ProductsM pm ON mb.ProductId = pm.Id      
JOIN ProductCategoriesM pcm ON pm.CategoryID = pcm.Id and pcm.Code = 'comm'      
WHERE mb.IsActive = 1  
AND pm.IsDeleted = 0 
AND (
    pm.IsActive = 1  
    OR (pm.IsActive = 0 
        AND CAST(mb.StartDate AS DATE) <= CAST(GETDATE() AS DATE) 
        AND CAST(mb.EndDate AS DATE) >= CAST(GETDATE() AS DATE)
    )
)

AND  mb.MobileUserKey  =  (select Publickey from MobileUsers where id =  @MobileUserId    )  
    
END 

--------- Created by Siva on 24-March-2025 -----------------------------------
GO


ALTER PROCEDURE [dbo].[Sp_Get_Leads] (                  
 --declare                                                    
 @IsPaging INT = 0              
 ,@PageSize INT = 5                                      
 ,@PageNumber INT = 1                                      
 ,@SortExpression VARCHAR(50)                                      
 ,@SortOrder VARCHAR(50)                                      
 ,@FromDate VARCHAR(50) = NULL                                      
 ,@ToDate VARCHAR(50) = NULL                                      
 ,@PrimaryKey VARCHAR(50) = NULL                                      
 ,@SecondaryKey VARCHAR(600) = NULL                                      
 ,-- Services Dropdown Filter                                                     
 @ThirdKey VARCHAR(50) = NULL                                      
 ,@FourthKey VARCHAR(50) = NULL                                      
 ,-- lead Source                                                    
 @FifthKey VARCHAR(50) = NULL                                      
 ,-- po status                                                    
 @CreatedBy VARCHAR(50) = NULL                                      
 ,@AssignedTo VARCHAR(50) = NULL                                      
 ,@LoggedInUser VARCHAR(50) = NULL                                      
 ,@RoleKey VARCHAR(50) = ''                                      
 ,@SearchText VARCHAR(250) = NULL                                      
 ,@TotalCount INT = 0 OUTPUT                                      
 )                                      
AS                                      
BEGIN                                  
 DECLARE @StatusId int                                 
 SELECT @StatusId = Id FROM STATUS WHERE Code = 'fresh'                                
        
        
-- select @IsPaging=0,@PageSize=20,@PageNumber =1,@SortExpression='CreatedOn',@SortOrder='desc',        
-- @FromDate='2000-01-01',@ToDate='9999-01-01',@PrimaryKey=NULL,        
-- @SecondaryKey=NULL,@ThirdKey=NULL,@FourthKey=NULL,@FifthKey=NULL,        
-- @CreatedBy=NULL,@AssignedTo=NULL,        
-- @LoggedInUser='0526f991-40c9-42ef-9921-c7f93d62574a',        
-- @RoleKey='cecf471a-9276-eb11-9fe9-00155d53687a'         
        
        
        
IF EXISTS (select 1 from Users as us         
INNER JOIN Roles  as ro on us.RoleKey = ro.PublicKey        
WHERE us.PublicKey = @LoggedInUser and ro.Name NOT IN ('admin', 'DM-Ashok') )        
BEGIN      
  SET @AssignedTo    = @LoggedInUser                
END        
        
SELECT @TotalCount = count(1)                                      
 FROM Leads AS Leads                                  
  LEFT JOIN PurchaseOrders AS po ON po.LeadId = Leads.Id AND (Leads.PurchaseOrderKey IS NOT NULL AND Leads.PurchaseOrderKey = po.PublicKey)                 
 LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = CAST(Leads.LeadTypeKey  as uniqueidentifier)                                       
 LEFT JOIN LeadSources AS LeadSource ON LeadSource.PublicKey = CAST( Leads.LeadSourceKey as uniqueidentifier) and isnull(LeadSource.IsDelete,0) = 0              
 LEFT JOIN STATUS AS st ON st.id = Leads.StatusId                                       
LEFT JOIN Users AS Users ON Users.PublicKey = TRY_CAST(Leads.CreatedBy AS UNIQUEIDENTIFIER)                                     
 --LEFT JOIN Services AS LeadService ON cast(leads.ServiceKey   as uniqueidentifier) = LeadService.PublicKey and LeadService.IsDelete = 0 and LeadService.IsDisabled = 0                
 LEFT JOIN Services AS LeadService ON ( leads.ServiceKey IS NOT NULL AND TRY_CAST(leads.ServiceKey AS UNIQUEIDENTIFIER) = LeadService.PublicKey )             
 LEFT JOIN Services AS PoService ON po.serviceid = PoService.id AND ISNULL(po.isExpired, 0) <> 1                                     
 LEFT JOIN STATUS AS st1 ON st1.id = po.STATUS                                       
 LEFT JOIN PaymentModes AS pm ON po.ModeOfPayment = pm.Id                                     
 WHERE ISNULL(leads.IsDelete,0) = 0 and               
(@AssignedTo IS NULL OR (Leads.AssignedTo IS NOT NULL AND Leads.AssignedTo = @AssignedTo))                                     
  AND (                           
    (po.status <> 24 AND ISNULL(po.IsExpired, 0) = 0)                
    OR                               
    (po.status = 24 AND ISNULL(po.IsExpired, 0) = 1)                               
 OR                               
    (po.status IS NULL)                            
 OR                           
 (po.status = 4)                           
)                           
     AND((  leads.ServiceKey in (SELECT CAST(value AS uniqueidentifier)  FROM STRING_SPLIT(@SecondaryKey, ',')  )  and @SecondaryKey is not null )  or( leads.ServiceKey = leads.ServiceKey and @SecondaryKey is null)    )                                    
                      
 AND (ISNULL(Leads.LeadSourceKey, '1') = ISNULL(@FourthKey, ISNULL(Leads.LeadSourceKey, '1')))                                       
 AND (ISNULL(Leads.LeadTypeKey, '1') = ISNULL(@ThirdKey, ISNULL(Leads.LeadTypeKey, '1')))                                       
 AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN cast(@FromDate AS DATE) AND cast(@ToDate AS DATE))             
 AND (                                       
 Leads.MobileNumber LIKE '%' + @SearchText + '%'                                       
 OR Leads.AlternateMobileNumber LIKE '%' + @SearchText + '%'                                       
 OR @SearchText IS NULL                                       
 OR Leads.EmailId LIKE '%' + @SearchText + '%'                                       
 OR Leads.FullName LIKE '%' + @SearchText + '%'                                       
 )                                       
 AND (@FifthKey IS NULL OR (@FifthKey = 'New' AND ISNULL(st1.Name, 'New') = 'New')                                       
 OR (@FifthKey <> 'New' AND ISNULL(st1.Name, 'New') = @FifthKey))                              
                        
 DROP TABLE IF EXISTS #tempLeads                                      
 SELECT Cast(ROW_NUMBER() OVER (                    
   ORDER BY                      
   Leads.createdOn   desc ,Favourite DESC,                    
       CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN Leads.CreatedOn END ASC,                    
        CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN Leads.CreatedOn END DESC,                    
        CASE WHEN @SortExpression = 'Favourite' AND @SortOrder = 'ASC' THEN Favourite END ASC,                    
        CASE WHEN @SortExpression = 'Favourite' AND @SortOrder = 'DESC' THEN Favourite END DESC,                    
        CASE WHEN @SortExpression = 'Name' AND @SortOrder = 'ASC' THEN FullName END ASC,                    
        CASE WHEN @SortExpression = 'Name' AND @SortOrder = 'DESC' THEN FullName END DESC                    
                     
 --ORDER BY Leads.id DESC                     
                     
 ) AS INT) AS SlNo                                       
 ,Leads.[Id]                                      
 ,Leads.[FullName]                                      
 ,Leads.[MobileNumber]                                      
 ,Leads.[AlternateMobileNumber]                                      
 ,Leads.[EmailId]                                      
 ,ISNULL(po.City, ISNULL(Leads.City, '')) AS City                                      
 ,po.PaymentDate                                      
 ,ISNULL(Leads.Favourite, 0) AS Favourite                                      
 ,COALESCE(LeadService.Name, PoService.Name) AS [ServiceKey]                
 ,COALESCE(LeadService.Id, PoService.Id) AS ServiceId                                     
 ,LeadTypes.Name AS [LeadTypeKey]                                      
 ,LeadTypes.Id AS LeadTypesId                                      
 ,LeadSource.Id AS LeadSourcesId                                      
 ,LeadSource.Name AS [LeadSourceKey]                                      
 ,ISNULL(Leads.[Remarks], '') AS Remark                 
 ,Leads.[IsSpam]                                      
 ,Leads.[IsWon]                                      
 ,Leads.[IsDisabled]                                      
 ,Leads.[IsDelete]                                      
 ,Leads.[PublicKey]                                      
 ,Leads.[CreatedOn]                                      
 ,Users.FirstName AS [CreatedBy]                                      
 ,Leads.[ModifiedOn]                                      
 ,Leads.[ModifiedBy]                       
 ,'' AS [AssignedTo]                                      
 ,ISNULL(Leads.StatusId, ( 1)) AS [StatusId]          
 ,isnull(st.Name, 'New') AS StatusName                                      
 ,Isnull(Leads.PurchaseOrderKey, CAST('00000000-0000-0000-0000-000000000000'  AS UNIQUEIDENTIFIER)) AS PurchaseOrderKey                                      
 ,isnull(st1.Name, 'New') AS PurchaseOrderStatus                                      
 ,isnull(po.ModeOfPayment, - 1) AS ModeOfPayment                                      
 ,isnull(po.PaidAmount, 0.0) AS PaidAmount                                      
 ,isnull(po.NetAmount, 0.0) AS NetAmount                                      
 ,ISNULL(po.TransactionRecipt, '') AS TransactionRecipt                                      
 ,ISNULL(po.TransasctionReference, '') AS TransasctionReference                                      
 ,(case when DATEDIFF(DAY, GETDATE(), DATEADD(day, 90, ISNULL(leads.ModifiedOn, GETDATE()))) > 0 then DATEDIFF(DAY, GETDATE(), DATEADD(day, 90, ISNULL(leads.ModifiedOn, GETDATE()))) else 0 end) AS DaysToGo                                      
 ,Leads.CountryCode                    
 INTO #tempLeads                            
 FROM Leads AS Leads                
 LEFT JOIN PurchaseOrders AS po ON po.LeadId = Leads.Id AND (Leads.PurchaseOrderKey IS NOT NULL AND Leads.PurchaseOrderKey = po.PublicKey)                
 LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = CAST(Leads.LeadTypeKey  as uniqueidentifier)                                      
 LEFT JOIN LeadSources AS LeadSource ON LeadSource.PublicKey = CAST( Leads.LeadSourceKey as uniqueidentifier) and isnull(LeadSource.IsDelete,0) = 0             
 LEFT JOIN STATUS AS st ON st.id = Leads.StatusId                                      
LEFT JOIN Users AS Users ON Users.PublicKey = TRY_CAST(Leads.CreatedBy AS UNIQUEIDENTIFIER)                                   
 --LEFT JOIN Services AS LeadService ON cast(leads.ServiceKey   as uniqueidentifier) = LeadService.PublicKey and LeadService.IsDelete = 0 and LeadService.IsDisabled = 0               
 LEFT JOIN Services AS LeadService ON ( leads.ServiceKey IS NOT NULL AND TRY_CAST(leads.ServiceKey AS UNIQUEIDENTIFIER) = LeadService.PublicKey )            
 LEFT JOIN Services AS PoService ON po.serviceid = PoService.id AND ISNULL(po.isExpired, 0) <> 1                                    
 LEFT JOIN STATUS AS st1 ON st1.id = po.STATUS                                      
 LEFT JOIN PaymentModes AS pm ON po.ModeOfPayment = pm.Id                      
 WHERE ISNULL(leads.IsDelete,0) = 0 and              
    (@AssignedTo IS NULL OR (Leads.AssignedTo IS NOT NULL AND Leads.AssignedTo = @AssignedTo))                                                
 AND (                      
    (po.status <> 24 AND ISNULL(po.IsExpired, 0) = 0)                                
    OR                              
    (po.status = 24 AND ISNULL(po.IsExpired, 0) = 1)                              
 OR                              
    (po.status IS NULL)                           
 OR                          
 (po.status = 4)                          
)                              
------                              
 --AND (ISNULL(Leads.ServiceKey, '1') = ISNULL(@SecondaryKey, ISNULL(Leads.ServiceKey, '1')))                             
    AND(( leads.ServiceKey in (SELECT CAST(value AS uniqueidentifier)  FROM STRING_SPLIT(@SecondaryKey, ',')  )  and @SecondaryKey is not null )  or( leads.ServiceKey = leads.ServiceKey and @SecondaryKey is null)    )                                    
                        
 AND (ISNULL(Leads.LeadSourceKey, '1') = ISNULL(@FourthKey, ISNULL(Leads.LeadSourceKey, '1')))                                      
 AND (ISNULL(Leads.LeadTypeKey, '1') = ISNULL(@ThirdKey, ISNULL(Leads.LeadTypeKey, '1')))                       
 AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN cast(@FromDate AS DATE) AND cast(@ToDate AS DATE))                                      
 AND (                             
 Leads.MobileNumber LIKE '%' + @SearchText + '%'                                      
 OR Leads.AlternateMobileNumber LIKE '%' + @SearchText + '%'                                      
 OR @SearchText IS NULL                                      
 OR Leads.EmailId LIKE '%' + @SearchText + '%'                                      
 OR Leads.FullName LIKE '%' + @SearchText + '%')                                      
 AND (@FifthKey IS NULL OR (@FifthKey = 'New' AND ISNULL(st1.Name, 'New') = 'New')                                      
 OR (@FifthKey <> 'New' AND ISNULL(st1.Name, 'New') = @FifthKey))                                 
 ORDER BY Leads.createdOn   desc, Favourite DESC ,                    
 CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN Leads.CreatedOn END ASC,                    
 CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN Leads.CreatedOn END DESC,                    
 CASE WHEN @SortExpression = 'Favourite' AND @SortOrder = 'ASC' THEN Favourite END ASC,                    
 CASE WHEN @SortExpression = 'Favourite' AND @SortOrder = 'DESC' THEN Favourite END DESC,                    
 CASE WHEN @SortExpression = 'Name' AND @SortOrder = 'ASC' THEN FullName END ASC,                    
 CASE WHEN @SortExpression = 'Name' AND @SortOrder = 'DESC' THEN FullName END DESC                    
                   
 OFFSET (IIF(@PageNumber <= 1, 0, (@PageNumber - 1))) * @PageSize ROWS                    
 FETCH NEXT @PageSize ROWS ONLY                                                             
             
 SELECT * FROM #tempLeads         
             
END      


------------- Created by Guna on 25-March-2025 -----------------------------------
  
  GO
  alter PROCEDURE [dbo].[Sp_Get_Leads] (                      
 --declare                                                        
 @IsPaging INT = 0                  
 ,@PageSize INT = 5                                          
 ,@PageNumber INT = 1                                          
 ,@SortExpression VARCHAR(50)                                          
 ,@SortOrder VARCHAR(50)                                          
 ,@FromDate VARCHAR(50) = NULL                                          
 ,@ToDate VARCHAR(50) = NULL                                          
 ,@PrimaryKey VARCHAR(50) = NULL                                          
 ,@SecondaryKey VARCHAR(600) = NULL                                          
 ,-- Services Dropdown Filter                                                         
 @ThirdKey VARCHAR(50) = NULL                                          
 ,@FourthKey VARCHAR(50) = NULL                                          
 ,-- lead Source                                                        
 @FifthKey VARCHAR(50) = NULL                                          
 ,-- po status                                                        
 @CreatedBy VARCHAR(50) = NULL                                          
 ,@AssignedTo VARCHAR(50) = NULL                                          
 ,@LoggedInUser VARCHAR(50) = NULL                                          
 ,@RoleKey VARCHAR(50) = ''                                          
 ,@SearchText VARCHAR(250) = NULL                                          
 ,@TotalCount INT = 0 OUTPUT                                          
 )                                          
AS                                          
BEGIN                                      
 DECLARE @StatusId int                                     
 SELECT @StatusId = Id FROM STATUS WHERE Code = 'fresh'                                    
            
            
-- select @IsPaging=0,@PageSize=20,@PageNumber =1,@SortExpression='CreatedOn',@SortOrder='desc',            
-- @FromDate='2000-01-01',@ToDate='9999-01-01',@PrimaryKey=NULL,            
-- @SecondaryKey=NULL,@ThirdKey=NULL,@FourthKey=NULL,@FifthKey=NULL,            
-- @CreatedBy=NULL,@AssignedTo=NULL,            
-- @LoggedInUser='0526f991-40c9-42ef-9921-c7f93d62574a',            
-- @RoleKey='cecf471a-9276-eb11-9fe9-00155d53687a'             
            
            
            
IF EXISTS (select 1 from Users as us             
INNER JOIN Roles  as ro on us.RoleKey = ro.PublicKey            
WHERE us.PublicKey = @LoggedInUser and ro.Name NOT IN ('admin', 'DM-Ashok') )            
BEGIN          
  SET @AssignedTo    = @LoggedInUser                    
END            
            
SELECT @TotalCount = count(1)                                          
 FROM Leads AS Leads                                      
  LEFT JOIN PurchaseOrders AS po ON po.LeadId = Leads.Id AND (Leads.PurchaseOrderKey IS NOT NULL AND Leads.PurchaseOrderKey = po.PublicKey)                     
 LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = CAST(Leads.LeadTypeKey  as uniqueidentifier)                                           
 LEFT JOIN LeadSources AS LeadSource ON LeadSource.PublicKey = CAST( Leads.LeadSourceKey as uniqueidentifier) and isnull(LeadSource.IsDelete,0) = 0                  
 LEFT JOIN STATUS AS st ON st.id = Leads.StatusId                                           
LEFT JOIN Users AS Users ON Users.PublicKey = TRY_CAST(Leads.CreatedBy AS UNIQUEIDENTIFIER)                                         
 --LEFT JOIN Services AS LeadService ON cast(leads.ServiceKey   as uniqueidentifier) = LeadService.PublicKey and LeadService.IsDelete = 0 and LeadService.IsDisabled = 0                    
 LEFT JOIN Services AS LeadService ON ( leads.ServiceKey IS NOT NULL AND TRY_CAST(leads.ServiceKey AS UNIQUEIDENTIFIER) = LeadService.PublicKey )                 
 LEFT JOIN Services AS PoService ON po.serviceid = PoService.id AND ISNULL(po.isExpired, 0) <> 1      
 LEFT JOIN STATUS AS st1 ON st1.id = po.STATUS                                           
 LEFT JOIN PaymentModes AS pm ON po.ModeOfPayment = pm.Id                                         
 WHERE ISNULL(leads.IsDelete,0) = 0 and                   
(@AssignedTo IS NULL OR (Leads.AssignedTo IS NOT NULL AND Leads.AssignedTo = @AssignedTo))                                         
  AND (                               
    (po.status <> 24 AND ISNULL(po.IsExpired, 0) = 0)                    
    OR                                   
    (po.status = 24 AND ISNULL(po.IsExpired, 0) = 1)                                   
 OR                                   
    (po.status IS NULL)                                
 OR                               
 (po.status = 4)                               
)                               
     AND((  leads.ServiceKey in (SELECT CAST(value AS uniqueidentifier)  FROM STRING_SPLIT(@SecondaryKey, ',')  )  and @SecondaryKey is not null )  or( leads.ServiceKey = leads.ServiceKey and @SecondaryKey is null)    )                                   
   
    
                          
 AND (ISNULL(Leads.LeadSourceKey, '1') = ISNULL(@FourthKey, ISNULL(Leads.LeadSourceKey, '1')))                                           
 AND (ISNULL(Leads.LeadTypeKey, '1') = ISNULL(@ThirdKey, ISNULL(Leads.LeadTypeKey, '1')))                                           
 AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN cast(@FromDate AS DATE) AND cast(@ToDate AS DATE))                 
 AND (                                           
 Leads.MobileNumber LIKE '%' + @SearchText + '%'                                           
 OR Leads.AlternateMobileNumber LIKE '%' + @SearchText + '%'                                           
 OR @SearchText IS NULL                                           
 OR Leads.EmailId LIKE '%' + @SearchText + '%'                                           
 OR Leads.FullName LIKE '%' + @SearchText + '%'                                           
 )                                           
 AND (@FifthKey IS NULL OR (@FifthKey = 'New' AND ISNULL(st1.Name, 'New') = 'New')                                           
 OR (@FifthKey <> 'New' AND ISNULL(st1.Name, 'New') = @FifthKey))                                  
                            
 DROP TABLE IF EXISTS #tempLeads                                          
SELECT
    CAST(ROW_NUMBER() OVER (
        ORDER BY
            Leads.ModifiedOn DESC, Favourite DESC,
            CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN Leads.CreatedOn END ASC,
            CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN Leads.CreatedOn END DESC,
            CASE WHEN @SortExpression = 'Favourite' AND @SortOrder = 'ASC' THEN Favourite END ASC,
            CASE WHEN @SortExpression = 'Favourite' AND @SortOrder = 'DESC' THEN Favourite END DESC,
            CASE WHEN @SortExpression = 'Name' AND @SortOrder = 'ASC' THEN FullName END ASC,
            CASE WHEN @SortExpression = 'Name' AND @SortOrder = 'DESC' THEN FullName END DESC
    ) AS INT) AS SlNo,
    Leads.[Id],
    Leads.[FullName],
    Leads.[MobileNumber],
    Leads.[AlternateMobileNumber],
    Leads.[EmailId],
    ISNULL(po.City, ISNULL(Leads.City, '')) AS City,
    po.PaymentDate,
    ISNULL(Leads.Favourite, 0) AS Favourite,
    COALESCE(LeadService.Name, PoService.Name) AS [ServiceKey],
    COALESCE(LeadService.Id, PoService.Id) AS ServiceId,
    LeadTypes.Name AS [LeadTypeKey],
    LeadTypes.Id AS LeadTypesId,
    LeadSource.Id AS LeadSourcesId,
    LeadSource.Name AS [LeadSourceKey],
    ISNULL(Leads.[Remarks], '') AS Remark,
    Leads.[IsSpam],
    Leads.[IsWon],
    Leads.[IsDisabled],
    Leads.[IsDelete],
    Leads.[PublicKey],
    Leads.[CreatedOn],
    Users.FirstName AS [CreatedBy],
    Leads.[ModifiedOn],
    Leads.[ModifiedBy],
    '' AS [AssignedTo],
    ISNULL(Leads.StatusId, 1) AS [StatusId],
    ISNULL(st.Name, 'New') AS StatusName,
    ISNULL(Leads.PurchaseOrderKey, CAST('00000000-0000-0000-0000-000000000000' AS UNIQUEIDENTIFIER)) AS PurchaseOrderKey,
    ISNULL(st1.Name, 'New') AS PurchaseOrderStatus,
    ISNULL(po.ModeOfPayment, -1) AS ModeOfPayment,
    ISNULL(po.PaidAmount, 0.0) AS PaidAmount,
    ISNULL(po.NetAmount, 0.0) AS NetAmount,
    ISNULL(po.TransactionRecipt, '') AS TransactionRecipt,
    ISNULL(po.TransasctionReference, '') AS TransasctionReference,
    (CASE WHEN DATEDIFF(DAY, GETDATE(), DATEADD(day, 90, ISNULL(Leads.ModifiedOn, GETDATE()))) > 0 
          THEN DATEDIFF(DAY, GETDATE(), DATEADD(day, 90, ISNULL(Leads.ModifiedOn, GETDATE()))) 
          ELSE 0 END) AS DaysToGo,
    Leads.CountryCode
INTO #tempLeads
FROM Leads AS Leads
LEFT JOIN PurchaseOrders AS po ON po.LeadId = Leads.Id 
    AND (Leads.PurchaseOrderKey IS NOT NULL AND Leads.PurchaseOrderKey = po.PublicKey)
LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = CAST(Leads.LeadTypeKey AS UNIQUEIDENTIFIER)
LEFT JOIN LeadSources AS LeadSource ON LeadSource.PublicKey = CAST(Leads.LeadSourceKey AS UNIQUEIDENTIFIER) 
    AND ISNULL(LeadSource.IsDelete, 0) = 0
LEFT JOIN STATUS AS st ON st.id = Leads.StatusId
LEFT JOIN Users AS Users ON Users.PublicKey = TRY_CAST(Leads.CreatedBy AS UNIQUEIDENTIFIER)
LEFT JOIN Services AS LeadService ON (Leads.ServiceKey IS NOT NULL 
    AND TRY_CAST(Leads.ServiceKey AS UNIQUEIDENTIFIER) = LeadService.PublicKey)
LEFT JOIN Services AS PoService ON po.serviceid = PoService.id AND ISNULL(po.isExpired, 0) <> 1
LEFT JOIN STATUS AS st1 ON st1.id = po.STATUS
LEFT JOIN PaymentModes AS pm ON po.ModeOfPayment = pm.Id
WHERE ISNULL(Leads.IsDelete, 0) = 0
    AND (@AssignedTo IS NULL OR (Leads.AssignedTo IS NOT NULL AND Leads.AssignedTo = @AssignedTo))
    AND (
        (po.status <> 24 AND ISNULL(po.IsExpired, 0) = 0)
        OR (po.status = 24 AND ISNULL(po.IsExpired, 0) = 1)
        OR (po.status IS NULL)
        OR (po.status = 4)
    )
    AND (
        (Leads.ServiceKey IN (SELECT CAST(value AS UNIQUEIDENTIFIER) FROM STRING_SPLIT(@SecondaryKey, ',')) 
            AND @SecondaryKey IS NOT NULL)
        OR (Leads.ServiceKey = Leads.ServiceKey AND @SecondaryKey IS NULL)
    )
    AND (ISNULL(Leads.LeadSourceKey, '1') = ISNULL(@FourthKey, ISNULL(Leads.LeadSourceKey, '1')))
    AND (ISNULL(Leads.LeadTypeKey, '1') = ISNULL(@ThirdKey, ISNULL(Leads.LeadTypeKey, '1')))
    AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))
    AND (
        Leads.MobileNumber LIKE '%' + @SearchText + '%'
        OR Leads.AlternateMobileNumber LIKE '%' + @SearchText + '%'
        OR @SearchText IS NULL
        OR Leads.EmailId LIKE '%' + @SearchText + '%'
        OR Leads.FullName LIKE '%' + @SearchText + '%'
    )
    AND (@FifthKey IS NULL 
        OR (@FifthKey = 'New' AND ISNULL(st1.Name, 'New') = 'New')
        OR (@FifthKey <> 'New' AND ISNULL(st1.Name, 'New') = @FifthKey));

SELECT *
FROM #tempLeads
ORDER BY SlNo
OFFSET (IIF(@PageNumber <= 1, 0, (@PageNumber - 1))) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY;        
                 
END          
         

Go
-- exec GetProductContentM 'E551010E-9795-EE11-812A-00155D23D79C' , 1, null , null       
ALTER PROCEDURE [dbo].[GetProductContentMV2]        
@userkey uniqueidentifier,       
@productId int,       
@IsInMyBucket bit OUTPUT,   
@IsInValidity bit OUTPUT   
AS        
BEGIN        
       
DECLARE @StartDate DATETIME, @EndDate DATETIME       
       
-- Check if the user has the product in the bucket       
IF EXISTS (SELECT id FROM mybucketm WHERE MobileUserKey = @userkey AND ProductId = @productId)       
BEGIN       
    SET @IsInMyBucket = 1       
       
    SELECT @StartDate = startDate, @EndDate = endDate        
    FROM mybucketm        
    WHERE MobileUserKey = @userkey AND ProductId = @productId       
       
    IF GETDATE() BETWEEN @StartDate AND @EndDate       
    BEGIN        
        SET @IsInValidity = 1       
    END       
    ELSE       
    BEGIN       
        SET @IsInValidity = 0       
    END       
END       
ELSE       
BEGIN       
    SET @IsInMyBucket = 0       
    SET @IsInValidity = 0       
END       
       
SELECT
Title, Description, ThumbnailImage,ListImage, AttachmentType, Attachment, Language, 
ISNULL(Duration, 0) AS AllVideoDuration,   
0 as TotalVideoCount
FROM ProductsContentM        
WHERE ProductId = @productId     
AND ISNULL(IsDeleted,0) = 0   
AND ISNULL(IsActive,1) = 1   
order by ModifiedOn desc 
END 

-- Ajith 28/03/2025 11:00 AM

GO
ALTER PROCEDURE [dbo].[GetProductContentMV2]
    @userkey UNIQUEIDENTIFIER,
    @productId INT,
    @IsInMyBucket BIT OUTPUT,
    @IsInValidity BIT OUTPUT
AS        
BEGIN
    DECLARE @StartDate DATETIME, @EndDate DATETIME;

    -- Check if the user has the product in their bucket       
    IF EXISTS (SELECT id
    FROM mybucketm
    WHERE MobileUserKey = @userkey AND ProductId = @productId)       
    BEGIN
        SET @IsInMyBucket = 1;

        SELECT @StartDate = startDate, @EndDate = endDate
        FROM mybucketm
        WHERE MobileUserKey = @userkey AND ProductId = @productId;

        IF GETDATE() BETWEEN @StartDate AND @EndDate       
            SET @IsInValidity = 1;       
        ELSE       
            SET @IsInValidity = 0;
    END       
    ELSE       
    BEGIN
        SET @IsInMyBucket = 0;
        SET @IsInValidity = 0;
    END;

    -- Calculate Total Video Count and Total Video Duration **per chapter**
    DECLARE @TotalVideoCount INT, @TotalVideoDuration INT, @TotalChapters INT;

    SELECT
        @TotalVideoCount = COUNT(*),
        @TotalVideoDuration = ISNULL(SUM(VideoDuration), 0)

    FROM SubChapters WHERE ChapterId IN (SELECT Id FROM Chapters WHERE ProductId = @productId) AND IsActive = 1;
    SELECT @TotalChapters = COUNT(*) FROM Chapters WHERE ProductId = @productId AND IsActive = 1;

    -- Fetch product content with calculated values
    SELECT
        Title,
        Description,
        ThumbnailImage,
        ListImage,
        AttachmentType,
        Attachment,
        Language,
        @TotalVideoDuration AS AllVideoDuration, -- Assign total duration
        @TotalVideoCount AS TotalVideoCount,
        @TotalChapters AS TotalChapters
    -- Assign total count
    FROM ProductsContentM
    WHERE ProductId = @productId
        AND ISNULL(IsDeleted, 0) = 0
        AND ISNULL(IsActive, 1) = 1
    ORDER BY ModifiedOn DESC;
END;

GO
-- Add Duration column if it doesn't exist
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ProductsContentM' 
    AND COLUMN_NAME = 'Duration'
)
BEGIN
    ALTER TABLE ProductsContentM ADD Duration INT NULL;
END

-- Add Language column if it doesn't exist
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ProductsContentM' 
    AND COLUMN_NAME = 'Language'
)
BEGIN
    ALTER TABLE ProductsContentM ADD Language NVARCHAR(255) NULL;
END

