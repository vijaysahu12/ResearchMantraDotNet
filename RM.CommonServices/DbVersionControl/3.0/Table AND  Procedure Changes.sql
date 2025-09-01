use KingResearchUAT
go


 

-- Date 02-Feb-2025 - by vijay sahu - 
-- Check and add 'GenerateToken' column if it does not exist in 'partnerAccountDetails'
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'partnerAccountDetails' AND COLUMN_NAME = 'GenerateToken'
)
BEGIN
    ALTER TABLE partnerAccountDetails  
    ADD GenerateToken VARCHAR(500);
END;

-- Check and add 'YesterdayPrice' column if it does not exist in 'CompanyDetailM'
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'CompanyDetailM' AND COLUMN_NAME = 'YesterdayPrice'
)
BEGIN
    ALTER TABLE CompanyDetailM
    ADD YesterdayPrice DECIMAL(18,2);
END;


IF NOT EXISTS (SELECT 1 FROM sys.columns 
WHERE name = 'IsActive' AND object_id = OBJECT_ID('ScheduledNotificationM'))
BEGIN
ALTER TABLE ScheduledNotificationM
ADD IsActive bit NULL; -- Or appropriate data type
END


IF NOT EXISTS (SELECT 1 FROM sys.columns 
WHERE name = 'IsActive' AND object_id = OBJECT_ID('ScannerPerformanceM'))
BEGIN
ALTER TABLE ScannerPerformanceM
ADD IsActive bit NULL; -- Or appropriate data type
END



IF NOT EXISTS (SELECT 1 FROM sys.columns 
WHERE name = 'IsDelete' AND object_id = OBJECT_ID('ScannerPerformanceM'))
BEGIN
ALTER TABLE ScannerPerformanceM
ADD IsDelete bit NULL; -- Or appropriate data type
END


GO 


IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = 'CreatedOn' AND object_id = OBJECT_ID('dbo.Complaints'))
BEGIN
    ALTER TABLE dbo.Complaints
    ADD CreatedOn DATETIME CONSTRAINT DF_Complaints_CreatedOn DEFAULT GETDATE() NOT NULL;
END;
GO 




GO
alter PROCEDURE GetTicketsM               
    @status VARCHAR(10) = NULL,               
    @ticketType VARCHAR(100) = NULL,               
    @priority VARCHAR(100) = NULL,               
    @startDate DATETIME,               
    @endDate DATETIME,               
    @PageNumber INT = 1,          
    @SearchText NVARCHAR(100) = NULL,              
    @TotalCount INT OUTPUT,               
    @TypeCount INT OUTPUT,               
    @StatusCount INT OUTPUT,               
    @PriorityCount INT OUTPUT               
AS               
BEGIN        
    SET @SearchText = LTRIM(RTRIM(@SearchText));        
    DECLARE @RowsPerPage INT = 10;               
    DECLARE @Offset INT = (@PageNumber - 1) * @RowsPerPage;               
               
    -- Step 1: Create the temporary table to hold the filtered data            
    SELECT DISTINCT               
        t.Id,               
        mu.fullname AS CreatedBy,      
        mu.Mobile,  
        mu.PublicKey as UserPublicKey, -- Added PublicKey  
        mu.Id as UserId, -- Added UserId  
        t.tickettype,               
        t.Priority,               
        t.subject,               
        t.Description,               
        t.Comment,               
        t.isactive,               
        t.isdelete,               
        t.CreatedOn,               
        t.images,               
        COALESCE(u.FirstName, mu.fullname) AS modifiedby,               
        t.modifiedOn,               
        t.status,               
        (               
            SELECT tcm.Comment,               
                   tcm.CreatedOn,               
                   tcm.Images,               
                   mu1.fullname AS CommentMobileUserName,               
                   mu2.fullname AS CommentByCrmUserName               
            FROM TicketCommentsM tcm               
                LEFT JOIN mobileusers mu1               
                    ON mu1.Id = tcm.CommentMobileUserId               
                LEFT JOIN mobileusers mu2               
                    ON mu2.Id = tcm.CommentByCrmId               
            WHERE tcm.TicketId = t.Id               
                  AND tcm.IsDelete = 0               
            FOR JSON PATH               
        ) AS CommentsJson               
    INTO #tempRecords               
    FROM ticketm t               
        INNER JOIN mobileusers mu               
            ON mu.publickey = t.createdby               
        LEFT JOIN users u               
            ON u.publickey = t.modifiedby               
    WHERE t.IsActive = 1               
          AND t.isdelete = 0               
          AND CAST(t.CreatedOn AS DATE) BETWEEN @startDate AND @endDate;          
              
               
    -- Step 2: Count the total records in the temporary table            
    SELECT @TotalCount = COUNT(1)               
    FROM #tempRecords AS t               
    WHERE (               
              t.status = @status OR @status IS NULL               
          )               
          AND (               
              t.TicketType = @ticketType OR @ticketType IS NULL               
          )               
          AND (               
              t.Priority = @priority OR @priority IS NULL               
          )          
    AND (@SearchText IS NULL  OR t.Subject LIKE '%' + @SearchText + '%'   OR t.Description LIKE '%' + @SearchText + '%'  OR t.CreatedBy LIKE '%' + @SearchText + '%' OR t.Mobile LIKE '%' + @SearchText + '%')              
    ;               
               
    -- Step 3: Count the records by TicketType, Priority, and Status            
    SELECT @TypeCount = COUNT(1)               
    FROM #tempRecords AS t               
    WHERE (               
              t.tickettype = @ticketType OR @ticketType IS NULL               
          );               
               
    SELECT @PriorityCount = COUNT(1)               
    FROM #tempRecords AS t               
    WHERE (               
              t.Priority = @priority OR @priority IS NULL               
          );               
               
    SELECT @StatusCount = COUNT(1)               
    FROM #tempRecords AS t               
    WHERE (     
              t.status = @status OR @status IS NULL               
          );               
              
    -- Step 4: Select the paged records for the response            
    SELECT ROW_NUMBER() OVER (ORDER BY ISNULL(t.ModifiedOn,t.CreatedOn) DESC) AS SlNo,               
           t.Id,    
           t.UserPublicKey,  
           t.UserId,      
           t.CreatedBy,      
           t.Mobile,      
           t.tickettype,               
           t.Priority,               
           t.subject,               
           t.Description,               
           t.Comment,               
           t.isactive,               
           t.CreatedOn,               
           t.modifiedby,               
           t.modifiedOn,               
           t.status,               
           t.Images,               
           t.CommentsJson               
    FROM #tempRecords AS t               
    WHERE t.IsActive = 1               
          AND t.isdelete = 0               
          AND (               
           t.status = @status OR @status IS NULL               
              )               
          AND (               
                  t.TicketType = @ticketType OR @ticketType IS NULL               
              )               
          AND (               
                  t.Priority = @priority OR @priority IS NULL               
              )          
      AND (@SearchText IS NULL  OR t.Subject LIKE '%' + @SearchText + '%'   OR t.Description LIKE '%' + @SearchText + '%'  OR t.CreatedBy LIKE '%' + @SearchText + '%' OR t.Mobile LIKE '%' + @SearchText + '%')              
          
    ORDER BY ISNULL(t.CreatedOn,t.ModifiedOn) DESC               
    OFFSET @Offset ROWS               
    FETCH NEXT @RowsPerPage ROWS ONLY;               
               
    -- Step 5: Drop the temporary table            
    DROP TABLE #tempRecords;               
END;   
GO

 
-----  05-Feb-2025 - by Guna Surya -    

IF OBJECT_ID('DashboardServiceM') IS NULL  -- Check if the table exists
BEGIN
    CREATE TABLE DashboardServiceM (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(255) NOT NULL,
        Subtitle NVARCHAR(MAX) NOT NULL,
        ImageUrl NVARCHAR(500) NOT NULL,
        Badge NVARCHAR(100) NULL
    );

    PRINT 'Table DashboardServiceM created successfully.'; -- Optional: Print a message
END
ELSE
BEGIN
    PRINT 'Table DashboardServiceM already exists.'; -- Optional: Print a message
END;


MERGE INTO DashboardServiceM AS target
USING (VALUES
    ('SCANNERS', 'Quickly spot stocks that are moving fast so you can take advantage of trading opportunities.', 'scanner.jpg', 'Exclusive'),
    ('STRATEGIES', 'Learn simple but powerful trading strategies to improve your skills, whether you''re a beginner or an experienced trader.', 'statergies.jpg', NULL),
    ('SCREENER', 'Easily find stocks that are quickly increasing in price or breaking through important resistance levels.', 'screeners.jpg', NULL),
    ('STOCK MARKET BASICS', 'Learn about chart patterns and how to analyze stocks step-by-step, from basic to advanced', 'learning-material.jpg', NULL),
    ('MARKET REPORT', 'Get insights into the market before trading starts to give yourself a better chance of success.', 'marketreport.jpg', NULL),
    ('RESEARCH', 'Use reports based on data to find the best stocks to invest in, considering both their technical and financial strength.', 'research.jpg', NULL)
) AS source (Title, Subtitle, ImageUrl, Badge)
ON target.Title = source.Title -- Prevents duplicates by checking Title
WHEN NOT MATCHED THEN
    INSERT (Title, Subtitle, ImageUrl, Badge)
    VALUES (source.Title, source.Subtitle, source.ImageUrl, source.Badge);


GO 

IF OBJECT_ID('dbo.GetExpiredServiceFromMyBucket', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetExpiredServiceFromMyBucket;
GO

CREATE PROCEDURE dbo.GetExpiredServiceFromMyBucket  
AS  
BEGIN  
    SET NOCOUNT ON;  

    SELECT  
        mb.Id,  
        mb.MobileUserKey,
        p.Code AS Topic,  
        p.CategoryID,
        mb.ProductName,  
        mu.FirebaseFcmToken,  
        mb.StartDate,  
        mb.EndDate,  
        DATEDIFF(DAY, GETDATE(), mb.EndDate) AS DateDifference  
    FROM MYBucketM AS mb  
    INNER JOIN MobileUsers AS mu ON mb.MobileUserKey = mu.Publickey  
    INNER JOIN ProductsM AS p ON mb.productId = p.Id  
    WHERE  
        mb.IsActive = 1  
        AND mb.IsExpired != 1  
        AND (DATEDIFF(DAY, GETDATE(), mb.EndDate) BETWEEN 1 AND 5  
             OR DATEDIFF(DAY, GETDATE(), mb.EndDate) <= -1);
END;
GO

IF OBJECT_ID('dbo.GetProductContentM', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetProductContentM;
GO


GO
-- exec GetProductContentM 'E551010E-9795-EE11-812A-00155D23D79C' , 1, null , null      
CREATE PROCEDURE [dbo].[GetProductContentM]       
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
      
SELECT Title, Description, ThumbnailImage,ListImage, AttachmentType, Attachment   FROM ProductsContentM       
WHERE ProductId = @productId    
AND ISNULL(IsDeleted,0) = 0  
AND ISNULL(IsActive,1) = 1  
order by ModifiedOn desc
END
GO



--IF OBJECT_ID('[dbo].[ProductBonusMappingM]') IS NULL  -- Check if the table exists
--BEGIN
--ALTER TABLE ScheduledNotificationM
--ADD ScheduledEndTime DATETIME2 NULL; -- Or appropriate data type
--END
--    CREATE TABLE [dbo].[ProductBonusMappingM] (
--        [Id]             INT      IDENTITY (1, 1) NOT NULL,
--        [ProductId]      INT      NOT NULL,
--        [BonusProductId] INT      NOT NULL,
--        [DurationInDays] INT      NULL,
--        [IsActive]       BIT      DEFAULT ((1)) NOT NULL,
--        [CreatedOn]      DATETIME DEFAULT (getdate()) NULL,
--        [ModifiedOn]     DATETIME NULL,
--        PRIMARY KEY CLUSTERED ([Id] ASC),
--        CONSTRAINT [FK_ProductBonusMappingM_BonusProduct] FOREIGN KEY ([BonusProductId]) REFERENCES [dbo].[ProductsM] ([Id]),
--        CONSTRAINT [FK_ProductBonusMappingM_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[ProductsM] ([Id])
--    );

--    PRINT 'Table ProductBonusMappingM created successfully.';
--END
--ELSE
--BEGIN
--    PRINT 'Table ProductBonusMappingM already exists.';
--END;
--GO



IF OBJECT_ID('dbo.GetPerformance', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetPerformance;
GO


CREATE PROCEDURE GetPerformance
    @Topic NVARCHAR(100),
    @Symbol NVARCHAR(50),
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @TotalCount INT OUTPUT
-- Declare TotalCount as OUTPUT
AS
BEGIN
    -- Calculate total count after grouping logic
    WITH
        GroupedNotifications
        AS
        (
            SELECT
                SentAt,
                TradingSymbol,
                ROW_NUMBER() OVER (PARTITION BY CAST(SentAt AS DATE), TradingSymbol ORDER BY CreatedOn) AS RowAsc
            FROM ScannerPerformanceM
            WHERE TradingSymbol IS NOT NULL AND TradingSymbol != ''
            AND IsActive = 1
                AND (@Topic IS NULL OR Topic = @Topic)
                AND (@Symbol IS NULL OR TradingSymbol = @Symbol)
                AND (@FromDate IS NULL OR SentAt >= @FromDate)
                AND (@ToDate IS NULL OR SentAt <= @ToDate)
        )
    SELECT @TotalCount = COUNT(*)
    FROM GroupedNotifications
    WHERE RowAsc = 1;
    -- First row in each group

    -- Common Table Expression (CTE) to filter and group data
    WITH
        GroupedNotifications
        AS
        (
            SELECT
                Id,
                SentAt,
                TradingSymbol,
                CreatedOn,
                Ltp,
                ROW_NUMBER() OVER (PARTITION BY CAST(SentAt AS DATE), TradingSymbol ORDER BY CreatedOn) AS RowAsc,
                ROW_NUMBER() OVER (PARTITION BY CAST(SentAt AS DATE), TradingSymbol ORDER BY CreatedOn DESC) AS RowDesc
            FROM ScannerPerformanceM
            WHERE TradingSymbol IS NOT NULL AND TradingSymbol != ''
            AND IsActive = 1
                AND (@Topic IS NULL OR Topic = @Topic)
                AND (@Symbol IS NULL OR TradingSymbol = @Symbol)
                AND (@FromDate IS NULL OR SentAt >= @FromDate)
                AND (@ToDate IS NULL OR SentAt <= @ToDate)
        )

    -- Select and format data with pagination
    SELECT
        CAST(FirstNotification.SentAt AS DATE) AS Date,
        FirstNotification.TradingSymbol AS Symbol,
        NULL AS Cmp,
        'N/A' AS Duration,
        CASE 
            WHEN FirstNotification.Ltp > 0 AND LastNotification.Ltp IS NOT NULL THEN 
                CONCAT('Captured ', 
                       ROUND(LastNotification.Ltp - FirstNotification.Ltp, 2), 
                       CASE WHEN ROUND(LastNotification.Ltp - FirstNotification.Ltp, 2) > 0 THEN ' levels' ELSE ' level' END)
            ELSE 'Not applicable for this.'
        END AS InvestmentMessage,
        CASE 
            WHEN FirstNotification.Ltp != 0 AND LastNotification.Ltp IS NOT NULL THEN
                ROUND(((LastNotification.Ltp - FirstNotification.Ltp) / FirstNotification.Ltp) * 100, 2)
            ELSE 0
        END AS Roi,
        'Open' AS Status,
        FirstNotification.Ltp AS EntryPrice,
        LastNotification.Ltp AS ExitPrice,
        FirstNotification.CreatedOn,
        FirstNotification.Ltp,
        FirstNotification.SentAt,
        FirstNotification.TradingSymbol,
        FirstNotification.Id
    FROM
        GroupedNotifications FirstNotification
        JOIN
        GroupedNotifications LastNotification
        ON FirstNotification.TradingSymbol = LastNotification.TradingSymbol
            AND FirstNotification.SentAt = LastNotification.SentAt
            AND FirstNotification.RowAsc = 1 -- First notification of the group
            AND LastNotification.RowDesc = 1
    -- Last notification of the group
    ORDER BY 
        CAST(FirstNotification.SentAt AS DATE) DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

-- 2025-02-06 by -Ajith
GO
CREATE PROCEDURE GetPaginationCallPerformanceM   
    @Code VARCHAR(50),  
    @PageNumber INT,   -- New: Page Number  
    @PageSize INT,     -- New: Page Size  
    @TotalTrades INT OUTPUT,  
    @TotalProfitable INT OUTPUT,  
    @TotalLoss INT OUTPUT,  
    @TradeClosed INT OUTPUT,  
    @Balance NVARCHAR(100) OUTPUT,  
    @TradeOpen INT OUTPUT  
AS  
BEGIN  
    SET NOCOUNT ON;  
  
    DECLARE @InvestmentAmount INT = 5000;  
  
    IF @Code = 'ShortInvestment'               
    BEGIN  
        -- Summary Stats  
        SELECT  
            @TotalTrades = COUNT(*),  
            @TotalProfitable = CAST(SUM(CASE WHEN Pnl > 0 AND CallStatus = 'Closed' THEN 1 ELSE 0 END) AS INT),  
            @TotalLoss = CAST(SUM(CASE WHEN Pnl < 0 AND CallStatus = 'Closed' THEN 1 ELSE 0 END) AS INT),  
            @TradeClosed = CAST(SUM(CASE WHEN CallStatus = 'Closed' THEN 1 ELSE 0 END) AS INT),  
            @TradeOpen = CAST(SUM(CASE WHEN CallStatus = 'Open' THEN 1 ELSE 0 END) AS INT),  
            @Balance = N'₹' + FORMAT(SUM(CASE WHEN CallStatus = 'Closed' THEN Pnl ELSE 0 END), 'N0', 'en-IN')  
        FROM callperformance  
        WHERE StrategyKey IN (SELECT PublicKey FROM Strategies WHERE Name = 'Harmonic')  
          AND IsDelete = 0  
          AND Pnl IS NOT NULL  
          AND Pnl != 0;  
  
        -- Data Records with Pagination  
        SELECT  
            ROW_NUMBER() OVER (ORDER BY CAST(ROI AS DECIMAL(18,2)) DESC) AS ID,  
            StockKey AS Symbol,  
            CAST(ROI AS VARCHAR(100)) AS Roi,  
            N'₹' + FORMAT(EntryPrice, 'N0', 'en-IN') AS EntryPrice,  
            CallStatus AS Status,  
            'N/A' AS Duration,  
            N'₹' + FORMAT(ExitPrice, 'N0', 'en-IN') AS ExitPrice,  
            CAST(NULL AS INT) AS Cmp,    
            CONCAT(N'Invest ₹', @InvestmentAmount, N' turned into ₹', CAST(@InvestmentAmount * (1 + ROI / 100.0) AS INT)) AS InvestmentMessage  
        FROM callperformance  
        WHERE StrategyKey IN (SELECT PublicKey FROM Strategies WHERE Name = 'Harmonic')  
          AND IsDelete = 0  
          AND Pnl IS NOT NULL  
          AND Pnl != 0  
        ORDER BY CAST(ROI AS DECIMAL(18,2)) DESC  
        OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;  -- Pagination added  
    END               
  
    ELSE IF @Code = 'Multibagger'               
    BEGIN  
        -- Summary Stats  
        SELECT  
            @TotalTrades = COUNT(*),  
            @TotalProfitable = CAST(SUM(CASE WHEN Pnl > 0 THEN 1 ELSE 0 END) AS INT),  
            @TotalLoss = CAST(SUM(CASE WHEN Pnl < 0 THEN 1 ELSE 0 END) AS INT),  
            @TradeClosed = 0,  
            @TradeOpen = @TotalTrades,  
            @Balance = N'₹' + FORMAT(SUM(Pnl), 'N0', 'en-IN')  
        FROM xl_harryportfolio;  
  
        -- Data Records with Pagination  
        WITH hf_CTE AS (  
            SELECT *,  
                CAST(  
                    CASE   
                        WHEN entryPrice = 0 OR entryPrice IS NULL THEN 0               
                        ELSE ROUND(((ltp - entryPrice) / entryPrice) * 100.0, 2)               
                    END   
                AS VARCHAR(100)) AS Roi  
            FROM xl_harryportfolio  
        )  
        SELECT   
            ROW_NUMBER() OVER (ORDER BY CAST(Roi AS DECIMAL(18,2)) DESC) AS ID,  
            instrument AS Symbol,  
            CAST(Roi AS VARCHAR(100)) AS Roi,  
            N'₹' + FORMAT(EntryPrice, 'N0', 'en-IN') AS EntryPrice,  
            'Open' AS Status,  
            'N/A' AS Duration,  
            '1' AS ExitPrice,  
            CAST(ltp AS INT) AS Cmp,    
            CONCAT(N'Invest ₹', @InvestmentAmount, N' turned into ₹', CAST(@InvestmentAmount * (1 + Roi / 100.0) AS INT)) AS InvestmentMessage  
        FROM hf_CTE  
        ORDER BY CAST(Roi AS DECIMAL(18,2)) DESC  
        OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;  -- Pagination added  
    END  
END;
GO


-------------------------------- 07-Feb-2025 - by Guna Surya--------------------------------
--exec GetProductsM null   , 'DB7E2AA9-9F19-EF11-B261-88B133B31C8F' , null                                           
alter PROCEDURE [dbo].[GetProductsM]                  
 @SearchText VARCHAR(50) = NULL,                 
 @UserKey UNIQUEIDENTIFIER,                 
 @IsValidUser BIT OUTPUT                 
AS                 
BEGIN                 
 --DECLARE @UserKey UNIQUEIDENTIFIER = '1194D99E-C419-EF11-B261-88B133B31C8F'                 
 SET @IsValidUser = 1;                  
                   
                 
                 
 DECLARE @priorDaysInfo INT = CAST((           
    SELECT TOP 1 value           
    FROM settings           
    WHERE code = 'PRIORDAYSINFO'           
    ) AS INT)  ;WITH VideoCounts AS (                 
   SELECT pc.ProductId ,COUNT(CASE WHEN pc.attachmenttype = 'video' THEN 1 ELSE NULL END) AS VideoCount                 
   FROM productscontentm AS pc                 
   GROUP BY pc.ProductId                 
   ),                 
   ContentCounts AS (                 
    SELECT ProductId, COUNT(1) as TotalContent FROM ProductsContentM                   
    GROUP BY ProductId                   
   )                 
 SELECT                   
  p.ID                 
  ,p.Name AS Name                 
  ,p.Description                 
  ,p.DescriptionTitle             
  ,PCM.Name AS Category                 
  ,PCM.GroupName                 
  ,p.Price,           
  --ISNULL(prm.Rating,0) as Raiting ,                  
  (SELECT ISNULL((CAST(AVG(ISNULL(Rating, 0)) AS DECIMAL(18, 2))),0) FROM ProductsRatingM WHERE ProductId = p.id) AS OverallRating,                 
  ISNULL(prm.Rating,0.0) AS UserRating,                 
  CAST((CASE WHEN ISNULL(plm.LikeId,0) = 1 THEN 1  ELSE 0 END ) AS BIT) AS Liked,                 
  (SELECT COUNT(1) FROM ProductLikesM WHERE LikeId = 1 AND ProductId = P.ID AND IsDelete = 0) AS HeartsCount,                 
  (SELECT COUNT(1) FROM ProductLikesM WHERE LikeId = 2 AND ProductId = P.ID AND IsDelete = 0) AS ThumbsUpCount,                 
  CAST(((CASE WHEN ISNULL(plm.LikeId,0) = 1 and plm.IsDelete = 0 THEN 1  ELSE 0 END )) AS bit) AS UserHasHeart,                 
  CAST(1 AS bit) AS UserHasThumbsUp,                 
  CAST((CASE WHEN ISNULL(MB.Id,0) > 0  THEN 1 ELSE 0 END ) AS bit) AS IsInMyBucket,                 
                   
  CAST(((CASE                  
      WHEN MB.Id IS NOT NULL                 
       AND MB.MobileUserKey = @UserKey                 
       AND ( GETDATE() BETWEEN MB.startDate AND MB.Enddate)                 
       THEN 1 ELSE 0 END))                 
  AS BIT) AS IsInValidity,                 
  (CAST(CASE WHEN DATEDIFF(day, GETDATE(), cast((isnull(mb.enddate,getDate())) AS DATE)) > @priorDaysInfo                 
   THEN 0 ELSE 1 END AS BIT )) as ShowReminder,                 
  p.ListImage as ListImage,                 
  --(                  
  --CASE WHEN mb.id IS NULL THEN 'BUY'                  
  --  WHEN DATEDIFF(day, GETDATE(), (mb.enddate)) <= @priorDaysInfo THEN 'Renew'                  
  --  WHEN GETDATE() BETWEEN mb.StartDate AND mb.EndDate THEN 'Purchased'                  
  --ELSE 'BUY' END ) AS BuyButtonText,        
         
    (--buy button text          
        
       
       
       
   CASE            
    WHEN mb.id IS NULL           
     THEN 'Buy'           
    ELSE CASE            
      WHEN cast(CASE            
         WHEN DATEDIFF(day, GETDATE(), mb.enddate) <= @priorDaysInfo           
          THEN 1           
         ELSE 0           
         END AS BIT) = 1           
       THEN 'Renew'           
      ELSE 'Purchased'           
      END           
    END) AS BuyButtonText,       
  (SELECT COUNT(1) FROM ProductsContentM where ProductId = p.Id) as ContentCount,                 
  COALESCE(vc.VideoCount, 0) AS VideoCount    ,     
  '' as IsIosCouponEnabled,
   (
      SELECT 
          pb.BonusProductId AS Id,
          p2.NAME AS BonusProductName,
          pb.DurationInDays AS Validity,
          LEFT(p2.Description, 150) AS Description,
          pcm2.NAME AS CategoryName
      FROM ProductBonusMappingM pb
      INNER JOIN ProductsM p2 ON pb.BonusProductId = p2.Id
      LEFT JOIN ProductCategoriesM pcm2 ON p2.CategoryID = pcm2.Id
      WHERE pb.ProductId = p.Id
        AND pb.IsActive = 1
      FOR JSON PATH
  ) AS BonusProducts
 FROM ProductsM AS p                 
 LEFT JOIN ProductCategoriesM AS PCM ON p.CategoryID = PCM.Id                 
 LEFT JOIN ProductsRatingM AS prm ON p.Id = prm.productId and prm.CreatedBy = @UserKey                 
 LEFT JOIN ProductLikesM AS plm ON plm.ProductId = p.Id and plm.CreatedBy = @UserKey    AND plm.LikeId = 1 AND plm.IsDelete = 0                
 LEFT JOIN MYBucketM AS MB ON mb.ProductId = p.Id and mb.MobileUserKey = @UserKey and mb.IsExpired = 0                 
 LEFT JOIN VideoCounts vc ON vc.ProductId = p.Id -- Join with the video count CTE                         
 LEFT JOIN ContentCounts CC ON CC.ProductId = p.Id -- Join with the video count CTE                         
 WHERE p.IsActive = 1   and p.IsDeleted = 0               
 ORDER BY p.name                 
 END                 
                 
-------------------------------- 13-Feb-2025 - by Guna Surya--------------------------------


IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'LeadFreeTrailReasonLog')
BEGIN
    CREATE TABLE LeadFreeTrailReasonLog (
        LogId INT IDENTITY(1,1) PRIMARY KEY,
        LeadKey UNIQUEIDENTIFIER NOT NULL,
        ServiceKey UNIQUEIDENTIFIER NOT NULL,
        Reason NVARCHAR(MAX) NOT NULL,
        ServiceName NVARCHAR(255) NOT NULL,
        FreeTrailStartDate DATETIME NOT NULL,
        FreeTrailEndDate DATETIME NOT NULL,
        CreatedBy UNIQUEIDENTIFIER NOT NULL,
        CreatedDate DATETIME NOT NULL
    );
END
