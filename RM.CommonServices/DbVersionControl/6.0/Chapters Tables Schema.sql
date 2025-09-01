-- Check if 'Chapters' table exists and drop it
IF OBJECT_ID('dbo.Chapters', 'U') IS NOT NULL
    DROP TABLE dbo.Chapters;

-- Check if 'SubChapters' table exists and drop it
IF OBJECT_ID('dbo.SubChapters', 'U') IS NOT NULL
    DROP TABLE dbo.SubChapters;

-- Create Chapters Table
CREATE TABLE Chapters (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL, -- Relates to a Course or Product
    ChapterTitle NVARCHAR(255) NOT NULL,
    IsActive BIT DEFAULT 1,
    IsDelete BIT DEFAULT 0, -- Soft delete flag
    CreatedOn DATETIME DEFAULT GETDATE(),
    CreatedBy BIGINT NOT NULL, -- User ID who created the record
    ModifiedOn DATETIME NULL, -- Nullable for updates
    ModifiedBy BIGINT NULL -- Nullable for updates
);

-- Create SubChapters Table
CREATE TABLE SubChapters (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ChapterId INT NOT NULL, -- Foreign Key Reference to Chapters
    Title NVARCHAR(255) NOT NULL,
    Link NVARCHAR(MAX) NULL, -- Stores Video or Document Links
    Description NVARCHAR(MAX) NULL,
    Language NVARCHAR(50) NULL,
    IsVisible BIT DEFAULT 1,
    VideoDuration INT DEFAULT 0, -- In Seconds
    IsActive BIT DEFAULT 1,
    IsDelete BIT DEFAULT 0, -- Soft delete flag
    CreatedOn DATETIME DEFAULT GETDATE(),
    CreatedBy BIGINT NOT NULL, -- User ID who created the record
    ModifiedOn DATETIME NULL, -- Nullable for updates
    ModifiedBy BIGINT NULL, -- Nullable for updates
    CONSTRAINT FK_SubChapter_Chapter FOREIGN KEY (ChapterId) REFERENCES Chapters(Id) ON DELETE CASCADE
);

-- Create Indexes for Performance
CREATE INDEX IX_Chapter_ProductId ON Chapters(ProductId);
CREATE INDEX IX_Chapter_IsDelete ON Chapters(IsDelete);
CREATE INDEX IX_SubChapter_ChapterId ON SubChapters(ChapterId);
CREATE INDEX IX_SubChapter_IsDelete ON SubChapters(IsDelete);

-- Insert a Sample Chapter
INSERT INTO Chapters (ProductId, ChapterTitle, IsActive, IsDelete, CreatedOn, CreatedBy)
VALUES (1, 'Introduction to SQL', 1, 0, GETDATE(), 101),
(2, 'Data Structures Basics', 1, 0, GETDATE(), 102),
(3, 'Advanced SQL Concepts', 1, 0, GETDATE(), 101);

-- Insert a Sample SubChapter
INSERT INTO SubChapters (ChapterId, Title, Link, Description, Language, IsVisible, VideoDuration, IsActive, IsDelete, CreatedOn, CreatedBy)
VALUES 
(1, 'SQL Basics', 'https://example.com/video1', 'Introduction to SQL Basics', 'English', 1, 300, 1, 0, GETDATE(), 101);


-- Insert Multiple SubChapters for Chapter 1 (SQL Introduction)
INSERT INTO SubChapters (ChapterId, Title, Link, Description, Language, IsVisible, VideoDuration, IsActive, IsDelete, CreatedOn, CreatedBy)
VALUES 
(1, 'SQL Basics', 'https://example.com/sql_basics', 'Understanding SQL Basics', 'English', 1, 300, 1, 0, GETDATE(), 101),
(1, 'SQL Joins', 'https://example.com/sql_joins', 'Learn about Inner, Outer, Left, and Right Joins', 'English', 1, 600, 1, 0, GETDATE(), 101),
(1, 'SQL Indexing', 'https://example.com/sql_indexing', 'Boost Query Performance with Indexing', 'English', 1, 450, 1, 0, GETDATE(), 101),
(1, 'SQL Transactions', 'https://example.com/sql_transactions', 'ACID Properties and Transactions in SQL', 'English', 1, 500, 1, 0, GETDATE(), 101);

-- Insert Multiple SubChapters for Chapter 2 (Advanced SQL)
INSERT INTO SubChapters (ChapterId, Title, Link, Description, Language, IsVisible, VideoDuration, IsActive, IsDelete, CreatedOn, CreatedBy)
VALUES 
(2, 'Stored Procedures', 'https://example.com/stored_procedures', 'Using Stored Procedures in SQL Server', 'English', 1, 700, 1, 0, GETDATE(), 102),
(2, 'Triggers in SQL', 'https://example.com/sql_triggers', 'Automating tasks with SQL Triggers', 'English', 1, 650, 1, 0, GETDATE(), 102),
(2, 'Common Table Expressions (CTE)', 'https://example.com/sql_cte', 'Writing Recursive Queries with CTE', 'English', 1, 400, 1, 0, GETDATE(), 102);

-- Insert Multiple SubChapters for Chapter 3 (Data Structures Basics)
INSERT INTO SubChapters (ChapterId, Title, Link, Description, Language, IsVisible, VideoDuration, IsActive, IsDelete, CreatedOn, CreatedBy)
VALUES 
(3, 'Arrays and Linked Lists', 'https://example.com/data_structures_1', 'Fundamentals of Arrays and Linked Lists', 'English', 1, 500, 1, 0, GETDATE(), 103),
(3, 'Stacks and Queues', 'https://example.com/data_structures_2', 'Understanding Stacks and Queues', 'English', 1, 550, 1, 0, GETDATE(), 103),
(3, 'Binary Trees and Graphs', 'https://example.com/data_structures_3', 'Exploring Trees and Graph Structures', 'English', 1, 600, 1, 0, GETDATE(), 103);
-- Soft Delete a Chapter (Instead of Deleting)
UPDATE Chapters 
SET IsDelete = 1, ModifiedOn = GETDATE(), ModifiedBy = 102
WHERE Id = 1;

-- Retrieve Only Active & Non-Deleted Chapters with SubChapters
SELECT 
    c.Id AS ChapterId, c.ChapterTitle, c.IsActive, c.CreatedOn, c.CreatedBy, c.ModifiedOn, c.ModifiedBy,
    sc.Id AS SubChapterId, sc.Title, sc.Link, sc.Description, sc.Language, sc.IsVisible, sc.VideoDuration, 
    sc.IsActive, sc.CreatedOn, sc.CreatedBy, sc.ModifiedOn, sc.ModifiedBy
FROM Chapters c
LEFT JOIN SubChapters sc ON c.Id = sc.ChapterId
WHERE c.IsDelete = 0 AND (sc.IsDelete = 0 OR sc.Id IS NULL)
ORDER BY c.Id, sc.Id;


GO 



-- Check if the 'Description' column exists in the 'Chapters' table
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Chapters' AND COLUMN_NAME = 'Description'
)
BEGIN
    -- Add the 'Description' column
    ALTER TABLE Chapters ADD Description NVARCHAR(MAX) NULL;
    PRINT 'Column Description added to Chapters table.';
END
ELSE
BEGIN
    PRINT 'Column Description already exists in Chapters table.';
END;


GO

-- Check if the stored procedure exists before dropping it
IF OBJECT_ID('dbo.GetPlayList', 'P') IS NOT NULL
    DROP PROCEDURE dbo.GetPlayList;
 
GO
-- Exec GetPlayList 1,20685 
CREATE PROCEDURE GetPlayList  
    @ProductId int ,  
    @MobileUserId BIGINT 
AS BEGIN 
 
 
declare @MobileUserKey uniqueidentifier; 
 
SELECT @MobileUserKey = PublicKey from MobileUsers where Id = @MobileUserId 
 
SELECT  
    c.Id AS ChapterId, c.ChapterTitle, c.Description AS ChapterDescription, 
    sc.Id AS SubChapterId, sc.Title AS SubChapterTitle, sc.Link AS SubChapterLink,  
    sc.Description AS SubChapterDescription, sc.Language AS SubChapterLanguage, sc.VideoDuration, sc.IsVisible
    
FROM 
MYBucketM as mb  
INNER JOIN Chapters c on mb.ProductId = c.productId and ISNULL(mb.IsActive,1) = 1 AND ISNULL(mb.IsExpired,0) = 0  
LEFT JOIN SubChapters sc ON c.Id = sc.ChapterId 
WHERE c.IsDelete = 0 AND (sc.IsDelete = 0 OR sc.Id IS NULL) 
and mb.ProductId = @ProductId and mb.MobileUserKey = @MobileUserKey 
ORDER BY c.Id, sc.Id; 
 
END  
-----Modify By Siva 31-03-2025 
--sp_helptext GetAllComplaints
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
        (@SearchText IS NULL OR firstName LIKE '%' + @SearchText + '%' OR @SearchText IS NULL OR mobile LIKE '%' + @SearchText + '%'  );    
    
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
           (@SearchText IS NULL OR firstName LIKE '%' + @SearchText + '%' OR @SearchText IS NULL OR mobile LIKE '%' + @SearchText + '%'  )
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
           (@SearchText IS NULL OR firstName LIKE '%' + @SearchText + '%' OR @SearchText IS NULL OR mobile LIKE '%' + @SearchText + '%'  )
        ORDER BY createdOn DESC;      
    END      
END;    
    
    -- Ajith 07-04-2025 10:50
    Go
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

	FROM SubChapters
	WHERE ChapterId IN (SELECT Id
		FROM Chapters
		WHERE ProductId = @productId) AND IsActive = 1;

	SELECT @TotalChapters = COUNT(DISTINCT c.Id)
	FROM Chapters c
		JOIN SubChapters sc ON sc.ChapterId = c.Id AND sc.IsDelete = 0
	WHERE c.ProductId = @productId AND c.IsActive = 1 AND c.IsDelete = 0;

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

-- Ajith 08-04-2025 10:40

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
	FROM SubChapters sc
		JOIN Chapters c ON sc.ChapterId = c.Id
	WHERE sc.IsActive = 1 AND sc.IsDelete = 0 AND c.ProductId = @productId AND c.IsActive = 1 AND c.IsDelete = 0;

	SELECT @TotalChapters = COUNT(DISTINCT c.Id)
	FROM Chapters c JOIN SubChapters sc ON sc.ChapterId = c.Id AND sc.IsDelete = 0 AND sc.IsActive = 1
	WHERE c.ProductId = @productId AND c.IsActive = 1 AND c.IsDelete = 0;

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

--Ajith 09-04-2025 12:45
GO
-- Add Image column if not exists
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns 
    WHERE Name = N'Image' 
      AND Object_ID = Object_ID(N'ScheduledNotificationM')
)
BEGIN
    ALTER TABLE ScheduledNotificationM
    ADD Image NVARCHAR(MAX); -- or NVARCHAR(500), depending on usage
END

-- Add ProductId column if not exists
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns 
    WHERE Name = N'ProductId' 
      AND Object_ID = Object_ID(N'ScheduledNotificationM')
)
BEGIN
    ALTER TABLE ScheduledNotificationM
    ADD ProductId INT;
END

-- Add MobileNumber column if not exists
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns 
    WHERE Name = N'MobileNumber' 
      AND Object_ID = Object_ID(N'ScheduledNotificationM')
)
BEGIN
    ALTER TABLE ScheduledNotificationM
    ADD MobileNumber VARCHAR(20); -- adjust size as needed
END

GO
ALTER PROCEDURE [dbo].[GetScheduledNotifications]     
 @IsPaging int = 1,    
    @PageSize INT = NULL,         -- NULL means return all records   
    @PageNumber INT = NULL,       -- NULL means return all records   
    @SortExpression VARCHAR(50) = 'CreatedOn',            
    @SortOrder VARCHAR(4) = 'DESC',            
    @SearchText VARCHAR(250) = NULL,            
    @FromDate DATETIME = NULL,            
    @ToDate DATETIME = NULL,            
    @TotalCount INT OUTPUT            
AS            
BEGIN            
    SET NOCOUNT ON;     
    SET @TotalCount = 0;     
   
    -- Get total count   
    SELECT @TotalCount = COUNT(*)   
    FROM ScheduledNotificationM sn            
    LEFT JOIN Users u ON sn.CreatedBy = u.Id     
    WHERE     
       (sn.IsActive = 1 OR sn.IsActive IS NULL)     
AND (@SearchText IS NULL OR sn.Title LIKE '%' + @SearchText + '%')            
AND (@FromDate IS NULL OR CAST(sn.CreatedOn AS DATE) >= CAST(@FromDate AS DATE))     
AND (@ToDate IS NULL OR CAST(sn.CreatedOn AS DATE) <= CAST(@ToDate AS DATE)); 
 
   
    -- Fetch results with dynamic pagination   
    SELECT     
        sn.Id AS NotificationId,            
        sn.Title,            
        sn.Body,            
        sn.Topic,
        sn.image,
        sn.productId,
        sn.mobilenumber,            
        sn.TargetAudience,            
        sn.LandingScreen,          
        sn.ScheduledTime AS ScheduleDate,            
        sn.CreatedOn AS CreatedDate,          
        sn.ModifiedOn AS ModifiedDate,          
        u.FirstName + ' ' + u.LastName AS CreatedBy,        
        u.FirstName + ' ' + u.LastName AS ModifiedBy        
    FROM ScheduledNotificationM sn            
    LEFT JOIN Users u ON sn.CreatedBy = u.Id     
    WHERE     
        (sn.IsActive = 1 OR sn.IsActive IS NULL)     
AND (@SearchText IS NULL OR sn.Title LIKE '%' + @SearchText + '%')            
AND (@FromDate IS NULL OR CAST(sn.CreatedOn AS DATE) >= CAST(@FromDate AS DATE))     
AND (@ToDate IS NULL OR CAST(sn.CreatedOn AS DATE) <= CAST(@ToDate AS DATE)) 
 
    ORDER BY     
        CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN sn.CreatedOn END DESC,   
        CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN sn.CreatedOn END ASC,   
        CASE WHEN @SortExpression = 'ModifiedOn' AND @SortOrder = 'DESC' THEN sn.ModifiedOn END DESC,   
        CASE WHEN @SortExpression = 'ModifiedOn' AND @SortOrder = 'ASC' THEN sn.ModifiedOn END ASC   
    OFFSET CASE WHEN @PageSize IS NOT NULL AND @PageNumber IS NOT NULL THEN (@PageNumber - 1) * @PageSize ELSE 0 END ROWS     
    FETCH NEXT CASE WHEN @PageSize IS NOT NULL AND @PageNumber IS NOT NULL THEN @PageSize ELSE 100000 END ROWS ONLY;   
END;  
 

 
 -------- Guna surya  12-04-2025 12:45-------------------------------------------
 GO
 ALTER PROCEDURE [dbo].[GetProductsM]                 
 @SearchText VARCHAR(50) = NULL,                 
 @UserKey UNIQUEIDENTIFIER,                 
 @IsValidUser BIT OUTPUT                 
AS                 
BEGIN                 
    SET @IsValidUser = 1;                  
                    
    DECLARE @priorDaysInfo INT = CAST((           
        SELECT TOP 1 value           
        FROM settings           
        WHERE code = 'PRIORDAYSINFO'           
    ) AS INT);
        
    CREATE TABLE #ApprovedMobileNumbers (MobileNumber VARCHAR(20));
    
    -- Insert approved mobile numbers
    INSERT INTO #ApprovedMobileNumbers (MobileNumber) VALUES
        ('8309091570'),
        ('6309373318'),
        ('8317509340'),
        ('7013182931'),
        ('6281012562'),
        ('6306153124'),
        ('8978106799'),
        ('9411122233'),
        ('7799190322'),
        ('8121927016'),
        ('9840730173');
    
    -- Get current user's mobile number
    DECLARE @UserMobileNumber VARCHAR(20) = NULL;
    
    SELECT @UserMobileNumber = Mobile
    FROM MobileUsers
    WHERE PublicKey = @UserKey;
    
    -- Check if user's mobile number is in the approved list
    DECLARE @IsApprovedNumber BIT = 0;
    
    IF @UserMobileNumber IS NOT NULL AND EXISTS (
        SELECT 1 FROM #ApprovedMobileNumbers 
        WHERE MobileNumber = @UserMobileNumber
    )
    BEGIN
        SET @IsApprovedNumber = 1;
    END;
        
    -- Get the products using the original query structure
    ;WITH VideoCounts AS (                 
        SELECT pc.ProductId, COUNT(CASE WHEN pc.attachmenttype = 'video' THEN 1 ELSE NULL END) AS VideoCount                 
        FROM productscontentm AS pc                 
        GROUP BY pc.ProductId                 
    ),                 
    ContentCounts AS (                 
        SELECT ProductId, COUNT(1) as TotalContent 
        FROM ProductsContentM                   
        GROUP BY ProductId                   
    )                 
    SELECT                   
        p.ID,                
        p.Name,                
        p.Description,
        p.code,
        p.DescriptionTitle,            
        PCM.Name AS Category,                
        PCM.GroupName,                
        p.Price,          
        (SELECT ISNULL(CAST(AVG(ISNULL(Rating, 0)) AS DECIMAL(18, 2)), 0) 
         FROM ProductsRatingM WHERE ProductId = p.id) AS OverallRating,                
        ISNULL(prm.Rating, 0.0) AS UserRating,                
        CAST(CASE WHEN ISNULL(plm.LikeId, 0) = 1 THEN 1 ELSE 0 END AS BIT) AS Liked,                
        (SELECT COUNT(1) FROM ProductLikesM WHERE LikeId = 1 AND ProductId = P.ID AND IsDelete = 0) AS HeartsCount,                
        (SELECT COUNT(1) FROM ProductLikesM WHERE LikeId = 2 AND ProductId = P.ID AND IsDelete = 0) AS ThumbsUpCount,                
        CAST(CASE WHEN ISNULL(plm.LikeId, 0) = 1 AND plm.IsDelete = 0 THEN 1 ELSE 0 END AS BIT) AS UserHasHeart,                
        CAST(1 AS BIT) AS UserHasThumbsUp,                
        CAST(CASE WHEN ISNULL(MB.Id, 0) > 0 THEN 1 ELSE 0 END AS BIT) AS IsInMyBucket,                          
        CAST(CASE                  
            WHEN MB.Id IS NOT NULL                
             AND MB.MobileUserKey = @UserKey                
             AND (GETDATE() BETWEEN MB.startDate AND MB.Enddate)                
             THEN 1 ELSE 0 END                
        AS BIT) AS IsInValidity,                
        CAST(CASE 
            WHEN DATEDIFF(DAY, GETDATE(), ISNULL(mb.enddate, GETDATE())) > @priorDaysInfo THEN 0 
            ELSE 1 
        END AS BIT) AS ShowReminder,                
        p.ListImage,                
        CASE           
            WHEN MB.Id IS NULL THEN 'Buy'          
            ELSE 
                CASE           
                    WHEN DATEDIFF(DAY, GETDATE(), MB.enddate) <= @priorDaysInfo THEN 'Renew'          
                    ELSE 'Purchased'          
                END          
        END AS BuyButtonText,      
        (SELECT COUNT(1) FROM ProductsContentM WHERE ProductId = p.Id) AS ContentCount,                
        COALESCE(vc.VideoCount, 0) AS VideoCount,    
        '' AS IsIosCouponEnabled    
    INTO #FinalResult
    FROM ProductsM AS p                 
    LEFT JOIN ProductCategoriesM AS PCM ON p.CategoryID = PCM.Id                 
    LEFT JOIN ProductsRatingM AS prm ON p.Id = prm.productId AND prm.CreatedBy = @UserKey                 
    LEFT JOIN ProductLikesM AS plm ON plm.ProductId = p.Id AND plm.CreatedBy = @UserKey AND plm.LikeId = 1 AND plm.IsDelete = 0                
    LEFT JOIN MYBucketM AS MB ON mb.ProductId = p.Id AND mb.MobileUserKey = @UserKey AND mb.IsExpired = 0                 
    LEFT JOIN VideoCounts vc ON vc.ProductId = p.Id                       
    LEFT JOIN ContentCounts CC ON CC.ProductId = p.Id                        
    WHERE p.IsActive = 1 AND p.IsDeleted = 0;

    -- Filter results into #FilteredResult
    CREATE TABLE #FilteredResult (
    ID INT,
    Name VARCHAR(255),
    Description VARCHAR(MAX),
    Code VARCHAR(100),
    DescriptionTitle VARCHAR(255),
    Category VARCHAR(255),
    GroupName VARCHAR(255),
    Price DECIMAL(18, 2),
    OverallRating DECIMAL(18, 2),
    UserRating FLOAT,  
    Liked BIT,
    HeartsCount INT,
    ThumbsUpCount INT,
    UserHasHeart BIT,
    UserHasThumbsUp BIT,
    IsInMyBucket BIT,
    IsInValidity BIT,
    ShowReminder BIT,
    ListImage VARCHAR(255),
    BuyButtonText VARCHAR(50),
    ContentCount INT,
    VideoCount INT,
    IsIosCouponEnabled VARCHAR(50) NULL 
);

    -- Insert filtered results based on mobile number
    INSERT INTO #FilteredResult
    SELECT *
    FROM #FinalResult
    WHERE 
        (@IsApprovedNumber = 1 AND Code = 'BREAKFASTSTRATEGY')
        OR (Code <> 'BREAKFASTSTRATEGY');

    -- Final result
    SELECT * FROM #FilteredResult
    ORDER BY Name;

    -- Clean up
    DROP TABLE #ApprovedMobileNumbers;
    DROP TABLE #FinalResult;
    DROP TABLE #FilteredResult;
END
--Modifide by Siva Apr 15 2025
--GetFilterUsersBy  
  GO         
ALTER PROCEDURE [dbo].[GetFilterUsersBy]             
@userType VARCHAR(50)            
AS BEGIN            
 SELECT             
  users.Id, users.PublicKey, FirstName , users.LastName , UserMappings.UserType             
 FROM Users INNER JOIN UserMappings            
 ON users.PublicKey = UserMappings.UserKey where UserMappings.UserType = @userType  AND users.IsDisabled=0  and UserMappings.IsActive=1           
          
 ORDER BY users.FirstName ASC            
END 

--- MODIFIED BY SIVA 2025 APRIL 15
GO
CREATE PROCEDURE [dbo].[GetPhonePePaymentReportBarChart]    
    @FromDate VARCHAR(50) = NULL,      
    @ToDate VARCHAR(50) = NULL,       
    @ProductId INT = NULL, -- NEW: filter by ProductId  
    @DurationType NVARCHAR(20) = NULL -- NEW: 'Monthly', 'Quarterly', 'HalfYearly', 'Yearly'  
AS    
BEGIN    
    SET NOCOUNT ON;    
    
    -- Default dates if not provided  
    IF @FromDate IS NULL     
        SET @FromDate = CONVERT(VARCHAR(10), DATEADD(YEAR, -1, GETDATE()), 120);    
    IF @ToDate IS NULL     
        SET @ToDate = CONVERT(VARCHAR(10), GETDATE(), 120);    
 IF @DurationType IS NULL  
        SET @DurationType = 'Monthly';  
    
    IF @DurationType = 'Monthly'  
    BEGIN  
        SELECT      
            FORMAT(phonePe.CreatedOn, 'MMM-yyyy') AS Duration,  
            p.Name AS ProductName,  
            SUM(ISNULL(phonePe.Amount, 0)) AS TotalPaidAmount,  
            COUNT(mu.FullName) AS TotalUserCount  
        FROM PaymentRequestStatusM AS prs     
        INNER JOIN MobileUsers AS mu ON prs.CreatedBy = mu.PublicKey    
        INNER JOIN ProductsM AS p ON prs.ProductId = p.Id    
        INNER JOIN SubscriptionMappingM AS sm ON prs.SubscriptionMappingId = sm.Id    
        INNER JOIN SubscriptionDurationM AS sd ON sm.SubscriptionDurationId = sd.Id    
        INNER JOIN PhonePePaymentResponseM AS phonePe ON phonePe.MerchantTransactionId = prs.TransactionId    
        WHERE phonePe.Code = 'PAYMENT_SUCCESS'    
            AND phonePe.CreatedOn BETWEEN @FromDate AND @ToDate  
            AND (@ProductId IS NULL OR prs.ProductId = @ProductId)  
        GROUP BY FORMAT(phonePe.CreatedOn, 'MMM-yyyy'), p.Name  
        ORDER BY MIN(YEAR(phonePe.CreatedOn)), MIN(MONTH(phonePe.CreatedOn)), p.Name  
    END  
    ELSE IF @DurationType = 'Quarterly'  
    BEGIN  
        SELECT      
            CONCAT('Q', DATEPART(QUARTER, phonePe.CreatedOn), '-', YEAR(phonePe.CreatedOn)) AS Duration,  
            p.Name AS ProductName,  
            SUM(ISNULL(phonePe.Amount, 0)) AS TotalPaidAmount,  
            COUNT(mu.FullName) AS TotalUserCount  
        FROM PaymentRequestStatusM AS prs     
        INNER JOIN MobileUsers AS mu ON prs.CreatedBy = mu.PublicKey    
        INNER JOIN ProductsM AS p ON prs.ProductId = p.Id    
        INNER JOIN SubscriptionMappingM AS sm ON prs.SubscriptionMappingId = sm.Id    
        INNER JOIN SubscriptionDurationM AS sd ON sm.SubscriptionDurationId = sd.Id    
        INNER JOIN PhonePePaymentResponseM AS phonePe ON phonePe.MerchantTransactionId = prs.TransactionId    
        WHERE phonePe.Code = 'PAYMENT_SUCCESS'    
            AND phonePe.CreatedOn BETWEEN @FromDate AND @ToDate  
            AND (@ProductId IS NULL OR prs.ProductId = @ProductId)  
        GROUP BY DATEPART(QUARTER, phonePe.CreatedOn), YEAR(phonePe.CreatedOn), p.Name  
        ORDER BY MIN(YEAR(phonePe.CreatedOn)), MIN(DATEPART(QUARTER, phonePe.CreatedOn)), p.Name  
    END  
    ELSE IF @DurationType = 'HalfYearly'  
    BEGIN  
        SELECT      
            CASE   
                WHEN DATEPART(MONTH, phonePe.CreatedOn) BETWEEN 1 AND 6 THEN CONCAT('H1-', YEAR(phonePe.CreatedOn))  
                ELSE CONCAT('H2-', YEAR(phonePe.CreatedOn))  
            END AS Duration,  
            p.Name AS ProductName,  
            SUM(ISNULL(phonePe.Amount, 0)) AS TotalPaidAmount,  
            COUNT(mu.FullName) AS TotalUserCount  
        FROM PaymentRequestStatusM AS prs     
        INNER JOIN MobileUsers AS mu ON prs.CreatedBy = mu.PublicKey    
        INNER JOIN ProductsM AS p ON prs.ProductId = p.Id    
        INNER JOIN SubscriptionMappingM AS sm ON prs.SubscriptionMappingId = sm.Id    
        INNER JOIN SubscriptionDurationM AS sd ON sm.SubscriptionDurationId = sd.Id    
        INNER JOIN PhonePePaymentResponseM AS phonePe ON phonePe.MerchantTransactionId = prs.TransactionId    
        WHERE phonePe.Code = 'PAYMENT_SUCCESS'    
            AND phonePe.CreatedOn BETWEEN @FromDate AND @ToDate  
            AND (@ProductId IS NULL OR prs.ProductId = @ProductId)  
        GROUP BY   
            CASE   
                WHEN DATEPART(MONTH, phonePe.CreatedOn) BETWEEN 1 AND 6 THEN CONCAT('H1-', YEAR(phonePe.CreatedOn))  
                ELSE CONCAT('H2-', YEAR(phonePe.CreatedOn))  
            END, p.Name  
        ORDER BY MIN(YEAR(phonePe.CreatedOn)), MIN(CASE WHEN DATEPART(MONTH, phonePe.CreatedOn) BETWEEN 1 AND 6 THEN 1 ELSE 2 END), p.Name  
    END  
    ELSE IF @DurationType = 'Yearly'  
    BEGIN  
        SELECT      
            CAST(YEAR(phonePe.CreatedOn) AS VARCHAR(4)) AS Duration,  
            p.Name AS ProductName,  
            SUM(ISNULL(phonePe.Amount, 0)) AS TotalPaidAmount,  
            COUNT(mu.FullName) AS TotalUserCount  
        FROM PaymentRequestStatusM AS prs     
        INNER JOIN MobileUsers AS mu ON prs.CreatedBy = mu.PublicKey    
        INNER JOIN ProductsM AS p ON prs.ProductId = p.Id    
        INNER JOIN SubscriptionMappingM AS sm ON prs.SubscriptionMappingId = sm.Id    
        INNER JOIN SubscriptionDurationM AS sd ON sm.SubscriptionDurationId = sd.Id    
        INNER JOIN PhonePePaymentResponseM AS phonePe ON phonePe.MerchantTransactionId = prs.TransactionId    
        WHERE phonePe.Code = 'PAYMENT_SUCCESS'    
            AND phonePe.CreatedOn BETWEEN @FromDate AND @ToDate  
            AND (@ProductId IS NULL OR prs.ProductId = @ProductId)  
        GROUP BY YEAR(phonePe.CreatedOn), p.Name  
        ORDER BY MIN(YEAR(phonePe.CreatedOn)), p.Name  
    END  
END;  

--Ajith 15-04-2025 12:38 PM
GO
IF COL_LENGTH('dbo.ProductsM', 'LandscapeImage') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[ProductsM]
    ALTER COLUMN [LandscapeImage] VARCHAR(255) NULL;
END



-------------------------------Gunasurya 15-04-2025 2:48--------------------------------
--exec [GetProductById] 96,'d7f73b83-bee9-ef11-b38d-f7f49baa4b49'     
GO
alter PROCEDURE [dbo].[GetProductById]                                
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
                                
  SELECT @ContentCount = COUNT(pc.Id),                            
       @VideoContent =         
           COUNT(CASE                            
                     WHEN pc.attachmentType = 'video' THEN 1                            
                     ELSE NULL                            
                 END)                            
    +         
           (SELECT COUNT(CASE         
                             WHEN sc.Link IS NOT NULL THEN 1         
                             ELSE NULL         
                         END)        
            FROM SUBCHAPTERS sc        
            JOIN CHAPTERS c ON sc.ChapterId = c.id        
            WHERE c.ProductId = @ProductId        
                  AND c.IsActive = 1         
                  AND c.IsDelete = 0        
                  AND sc.IsActive = 1        
                  AND sc.IsDelete = 0)                            
FROM PRODUCTSCONTENTM pc                 
WHERE pc.ProductId = @ProductId                            
      AND pc.isActive = 1                            
      AND pc.isDeleted = 0;          
        
                                
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
        p.code,  
        p.Description,                        
        p.DescriptionTitle,                              
        p.CategoryID,  
  pcmm.CommunityId,
  pc.name as communityname,
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
  (SELECT TOP 1 Attachment     
   FROM ProductsContentM     
   WHERE productId = @ProductId AND title LIKE '%intro%') AS LandscapeVideoUrl,    
    
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
                pb.ProductId = p.id AND pb.IsActive = 1 and pb.IsDeleted = 0 AND p2.IsActive = 1 AND p2.IsDeleted = 0                    
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
	  left join ProductCommunityMappingM pcmm on p.Id = pcmm.ProductId  and pcmm.IsActive = 1 and pcmm.IsDeleted = 0
	  LEFT JOIN ProductsM pc ON pcmm.CommunityId = pc.Id and pc.IsActive = 1 and pc.IsDeleted = 0
    WHERE p.id = @ProductId                                
    ORDER BY POM.CreatedOn DESC                                
END    


GO
-- This procedure only getting call from Mobile Flutter while doing any payment using payment gateway                                   
-- Not:  Don't use this for CRM or any where elese to get the subscription in my bucket.                                    
-- @paidAmount means the final amount which has been paid by the client                                  
-- exec ManagePurchaseOrderSm 'BA0C9CCE-6A8F-EF11-819C-00155D23D79C', 1  , 1,'TRANSACT12312313', 1  , null                                     
ALTER PROCEDURE [dbo].[ManagePurchaseOrderSM]          
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
DECLARE @IsOldDevice BIT = 0;  
  
SELECT @IsOldDevice = CASE  
    WHEN mu.DeviceVersion IS NULL THEN 1 -- If NULL, mark as old  
  
    -- Old iOS devices  
    WHEN (mu.DeviceType LIKE 'iOS%' OR mu.DeviceType LIKE 'IosId:%')  
         AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1  
         AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0  
         AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 16 THEN 1  
  
    -- Old Android devices  
    WHEN mu.DeviceType LIKE 'Android:%'  
         AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1  
         AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0  
         AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 61 THEN 1  
  
    ELSE 0  
END  
FROM MobileUsers AS mu  
WHERE PublicKey = @mobileUserKey;  
  
      
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
    SELECT (SELECT     
          p.name,    
          p.code,    
          CAST(@startDate as date) AS StartDate,    
          CAST(@endDate as date) as EndDate,    
          (@FinalValidityInDays + 1) AS ProductValidity,    
          @BonusProductName as BonusProduct,    
          @BonusProductDurationInDays as BonusProductValidity,    
          (SELECT TOP 1     
                 pc.Name    
           FROM ProductCommunityMappingM pcm    
           JOIN ProductsM pc ON pc.Id = pcm.CommunityId    
           WHERE pcm.ProductId = @ProductId    
           AND pcm.IsActive = 1    
           AND pc.IsActive = 1    
           AND pc.IsDeleted = 0) as Community,    
          (SELECT TOP 1     
                 cat.Name    
           FROM ProductCategoriesM cat    
           WHERE cat.Id = p.CategoryId    
           AND cat.IsActive = 1    
           AND cat.IsDelete = 0) as ProductCategory    
        FROM ProductsM p    
        WHERE p.id = @productId    
        FOR JSON AUTO) AS JsonData    
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
--exec GetTargetAudianceListForPushNotification 'PURCHASEDKALKIBAATAAJ','testing','',null  
  
alter PROCEDURE GetTargetAudianceListForPushNotification                      
    @AudianceCategory VARCHAR(50),                      
    @topic VARCHAR(50),                      
    @mobile VARCHAR(MAX), -- Use MAX to handle multiple numbers          
 @ProductId int = 0  
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
       AND (
           EXISTS (  
               SELECT 1 FROM PurchaseOrdersM pom   
               WHERE pom.ActionBy = mu.PublicKey   
               AND pom.TransasctionReference <> 'WITHOUTPAYMENT'  
           )
           OR EXISTS (
               SELECT 1 FROM MYBucketM mb
               WHERE mb.MobileUserKey = mu.PublicKey
           )
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
       )
       AND NOT EXISTS (
           SELECT 1 FROM MYBucketM mb
           WHERE mb.MobileUserKey = mu.PublicKey
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
  
  
   IF @ProductId IS NOT NULL  
BEGIN  
    UPDATE @MobileUsers  
    SET notification = 0  
    WHERE PublicKey IN (  
        SELECT mb.MobileUserKey   
        FROM MYBucketM mb  
        WHERE mb.ProductId = @ProductId   
        AND mb.Notification = 0  
    );  
END  
  
      
    SELECT * FROM @MobileUsers;                      
END;  
  
--Ajith 16-04-2025 11:54 AM
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ProductCommunityMappingM' 
      AND COLUMN_NAME = 'DurationInDays'
)
BEGIN
    ALTER TABLE ProductCommunityMappingM
    ADD DurationInDays INT NULL;
END
  
  
 GO
  ALTER PROCEDURE GetProductCommunityMappings 
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
 
    -- Normalize @FromDate and @ToDate 
    SET @FromDate = COALESCE(@FromDate, '1900-01-01'); -- Default to old date if NULL 
    SET @ToDate = COALESCE(@ToDate, GETDATE()); -- Default to today if NULL 
    SET @ToDate = DATEADD(DAY, 1, @ToDate); -- Ensure the @ToDate includes the full day 
 
    -- Count total records matching the criteria 
    SELECT @TotalCount = COUNT(*) 
    FROM ProductCommunityMappingM pcmm 
        LEFT JOIN ProductsM pm1 ON pcmm.ProductId = pm1.Id 
        LEFT JOIN ProductsM pm2 ON pcmm.CommunityId = pm2.Id 
        LEFT JOIN Users u1 ON pcmm.CreatedBy = u1.Id 
        LEFT JOIN Users u2 ON pcmm.ModifiedBy = u2.Id 
    WHERE   
        (@SearchText IS NULL OR 
         pm1.Name LIKE '%' + @SearchText + '%' OR  
         pm2.Name LIKE '%' + @SearchText + '%')  
        AND pcmm.CreatedDate >= @FromDate 
        AND pcmm.CreatedDate < @ToDate 
        AND (  
            @Status IS NULL OR  
            (@Status = 'ACTIVE' AND pcmm.IsActive = 1 AND pcmm.IsDeleted = 0) OR 
            (@Status = 'INACTIVE' AND pcmm.IsActive = 0 AND pcmm.IsDeleted = 0) OR 
            (@Status = 'DELETED' AND pcmm.IsDeleted = 1) 
        ); 
 
    -- Fetch paginated results 
    SELECT 
        pcmm.Id, 
        pcmm.ProductId, 
        pm1.Name AS ProductName, -- Fetching Product Name 
        pcmm.CommunityId, 
        pm2.Name AS CommunityName, -- Fetching Community Name 
        pcmm.IsActive, 
        pcmm.IsDeleted, 
        pcmm.CreatedDate, 
        pcmm.ModifiedDate, 
        pcmm.DurationInDays,
        u1.FirstName + ' ' + u1.LastName AS CreatedBy, 
        u2.FirstName + ' ' + u2.LastName AS ModifiedBy 
    FROM ProductCommunityMappingM pcmm 
        -- Join for Product Name 
        LEFT JOIN ProductsM pm1 ON pcmm.ProductId = pm1.Id 
        -- Join for Community Name 
        LEFT JOIN ProductsM pm2 ON pcmm.CommunityId = pm2.Id 
        -- Join Users table for CreatedBy 
        LEFT JOIN Users u1 ON pcmm.CreatedBy = u1.Id 
        -- Join Users table for ModifiedBy 
        LEFT JOIN Users u2 ON pcmm.ModifiedBy = u2.Id 
    WHERE 
        (@SearchText IS NULL OR  
         pm1.Name LIKE '%' + @SearchText + '%' OR  
         pm2.Name LIKE '%' + @SearchText + '%')  
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
-- This procedure only getting call from Mobile Flutter while doing any payment using payment gateway                                     
-- Not:  Don't use this for CRM or any where elese to get the subscription in my bucket.                                      
-- @paidAmount means the final amount which has been paid by the client                                    
-- exec ManagePurchaseOrderSm '783da9b3-9016-f011-b3cf-c3c5a7518e81', 4 , 105,'TRANSACT12312313','TRANSACT1231231311, 1 , null                                       
ALTER PROCEDURE [dbo].[ManagePurchaseOrderSM]
    @mobileUserKey UNIQUEIDENTIFIER,
    @productId INT,
    @SubscriptionMappingId int,
    @MerchantTransactionId VARCHAR(100),
    @TransactionId VARCHAR(100),
    @paidAmount DECIMAL(18, 2),
    @couponCode VARCHAR(20) = NULL
-- using coupon name here                                                                           
AS
BEGIN
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
        SELECT @MobileNumber = Mobile,
        @NewLeadKey = LeadKey
    FROM MobileUsers
    WHERE PublicKey = @MobileUserKey; -- Check if LeadKey is invalid or NULL             
        IF @NewLeadKey IS NULL
        OR NOT EXISTS (   SELECT 1
        FROM Leads
        WHERE PublicKey = @NewLeadKey)            
        BEGIN
        SELECT TOP 1
            @NewLeadKey = PublicKey
        FROM Leads
        WHERE MobileNumber = @MobileNumber;
        IF @NewLeadKey IS NOT NULL            
            BEGIN
            UPDATE MobileUsers            
                   SET LeadKey = @NewLeadKey            
                 WHERE PublicKey = @MobileUserKey;
        END            
            ELSE            
            BEGIN
            SELECT @NewLeadKey = NEWID()
            INSERT INTO Leads
                (SupervisorId,PublicKey, FullName,
                Gender,MobileNumber,AlternateMobileNumber,EmailId,
                ProfileImage,PriorityStatus,AssignedTo,ServiceKey,
                LeadTypeKey,LeadSourceKey,Remarks,IsDisabled,
                IsDelete,CreatedOn,CreatedBy,IsSpam,IsWon,ModifiedOn,
                ModifiedBy,City,PinCode,StatusId,
                PurchaseOrderKey,Favourite,CountryCode)
            SELECT NULL,
                @NewLeadKey, FullName,
                (CASE            
                             WHEN Gender = 'male' then 'm'            
                             else 'f' end),
                Mobile, NULL, EmailId, NULL, 1, NULL,
                'C11DAA9F-F125-EE11-811D-00155D23D79C', --admin        
                NULL, @leadsourcekey, 'Reg. via mobile app', 0,
                0, @CurrentDate, @AdminKey, 0,
                0, @CurrentDate, @AdminKey, City, '', 1, NULL, 1, CountryCode
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
        SELECT @couponHasProductValidity = IIF(ProductValidityInDays IS NULL, 0, 1),
        @couponValidityDays = ProductValidityInDays,
        @discountPercent = IIF(DiscountInPercentage IS NULL, 0, 1)
    FROM CouponsM c
    WHERE publickey = @couponkey;                                           
        IF (@couponValidityDays > @FinalValidityInDays)            
        BEGIN
        SET @FinalValidityInDays = @couponValidityDays
    END            
        IF EXISTS (   SELECT 1
    FROM myBucketM
    where MobileUserKey = @MobileUserKey AND ProductId     = @ProductId)            
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
        INSERT myBucketM
            (MobileUserKey,ProductId,ProductName,StartDate,EndDate,Status,
            CreatedBy,CreatedDate,ModifiedBy,ModifiedDate,
            IsActive,IsExpired)
        VALUES
            (@MobileUserKey, @ProductId, @ProductName, @startDate, @endDate,
                1, @MobileUserKey, @CurrentDate, NULL, NULL, 1, 0);
    END               
DECLARE @IsOldDevice BIT = 0;    
    
SELECT @IsOldDevice = CASE    
    WHEN mu.DeviceVersion IS NULL THEN 1 -- If NULL, mark as old    
    
    -- Old iOS devices    
    WHEN (mu.DeviceType LIKE 'iOS%' OR mu.DeviceType LIKE 'IosId:%')
            AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1
            AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0
            AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 16 THEN 1    
    
    -- Old Android devices    
    WHEN mu.DeviceType LIKE 'Android:%'
            AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1
            AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0
            AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 61 THEN 1    
    
    ELSE 0    
END
    FROM MobileUsers AS mu
    WHERE PublicKey = @mobileUserKey;    
   
  IF (@IsOldDevice = 0)         
BEGIN
        DECLARE @communitymappingdays INT;
        -- Use INT for storing days difference   

        -- Calculate the end date based on the ProductCommunityMappingM table   
        SELECT @communitymappingdays = pcm.DurationInDays
        FROM ProductCommunityMappingM pcm
        WHERE pcm.ProductId = @ProductId
            AND pcm.IsActive = 1;

        -- Set the EndDate based on the mapping duration for insertion   
        IF @communitymappingdays IS NOT NULL   
    BEGIN
            SET @endDate = DATEADD(DAY, @communitymappingdays, GETDATE());
        -- Calculate end date based on days for insert   
        END

        -- Perform the MERGE operation   
        MERGE INTO myBucketM AS target            
    USING (   
        SELECT pcm.CommunityId, pc.Name AS CommunityName, sd.Months
        -- Include Months for update scenario   
        FROM ProductCommunityMappingM pcm
            JOIN productsm pc ON pc.Id = pcm.CommunityId
            JOIN SubscriptionMappingM sm ON sm.ProductId = pcm.ProductId and sm.id = @SubscriptionMappingId
            JOIN SubscriptionDurationM sd ON sm.SubscriptionDurationId = sd.Id
        WHERE pcm.ProductId = @ProductId
            AND pcm.IsActive = 1
            AND pc.isactive = 1
            AND pc.isdeleted = 0   
    ) AS source            
    ON target.MobileUserKey = @MobileUserKey
            AND target.ProductId = source.CommunityId            
   WHEN MATCHED THEN    
       UPDATE SET target.ProductName = source.CommunityName,            
                  target.EndDate = DATEADD(MONTH, source.Months, GETDATE()), -- Use Months for updating EndDate   
                  target.ModifiedBy = @MobileUserKey,   
                  target.ModifiedDate = @CurrentDate,    
                  target.IsActive = 1,    
                  target.IsExpired = 0            
   WHEN NOT MATCHED THEN            
       INSERT (MobileUserKey, ProductId, ProductName,            
               StartDate, EndDate, Status,            
               CreatedBy, CreatedDate, ModifiedBy,            
               ModifiedDate, IsActive, IsExpired)            
       VALUES (@MobileUserKey, source.CommunityId, source.CommunityName,            
               @CurrentDate, @endDate, 1, @MobileUserKey, @CurrentDate, NULL, NULL, 1, 0);
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

            INSERT INTO myBucketM
                (MobileUserKey,
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
            VALUES
                (@MobileUserKey,
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
        IF EXISTS ( SELECT 1
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
        INSERT INTO [dbo].[PurchaseOrdersM]
            ([LeadId],[ClientName],[Mobile],[Email],
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
        SELECT (SELECT Id
            FROM Leads
            WHERE PublicKey = LeadKey),
            fullname, mobile, emailid, Dob, NULL, @CurrentDate,
            (CASE            
                         WHEN @paidAmount = 0.00 THEN @couponMOP            
                         ELSE @razorMOP END) AS ModeOfPayment,
            NULL, NULL, NULL, City, @MerchantTransactionId,
            @ProductId, @ProductName, @price, @paidAmount,
            @couponkey, @discountAmount, @discountPercent, 1,
            @MobileUserKey, 1, @CurrentDate, @CurrentDate, @MobileUserKey,
            NULL, NULL, @CurrentDate, @endDate, 1, 1, NULL, @SubscriptionMappingId, @TransactionId
        FROM #TempTable;
    END;            
            
        UPDATE CouponsM            
           SET TotalRedeems = TotalRedeems + 1            
         WHERE publickey = @couponkey;            
        UPDATE MobileUsers            
           SET CancommunityPost = 1            
         WHERE publickey = @MobileUserKey;            
        DROP TABLE #TempTable            
    SELECT (SELECT
            p.name,
            p.code,
            CAST(@startDate as date) AS StartDate,
            CAST(@endDate as date) as EndDate,
            (@FinalValidityInDays + 1) AS ProductValidity,
            @BonusProductName as BonusProduct,
            @BonusProductDurationInDays as BonusProductValidity,
            (SELECT TOP 1
                pc.Name
            FROM ProductCommunityMappingM pcm
                JOIN ProductsM pc ON pc.Id = pcm.CommunityId
            WHERE pcm.ProductId = @ProductId
                AND pcm.IsActive = 1
                AND pc.IsActive = 1
                AND pc.IsDeleted = 0) as Community,
            (SELECT TOP 1
                cat.Name
            FROM ProductCategoriesM cat
            WHERE cat.Id = p.CategoryId
                AND cat.IsActive = 1
                AND cat.IsDelete = 0) as ProductCategory
        FROM ProductsM p
        WHERE p.id = @productId
        FOR JSON AUTO) AS JsonData      
        COMMIT TRANSACTION;            
    END TRY            
    BEGIN CATCH            
        IF @@TRANCOUNT > 0            
            ROLLBACK TRANSACTION;            
        SELECT ERROR_MESSAGE() as JsonData,
        ERROR_LINE() AS lINE, ERROR_NUMBER() AS NUMBER            
            
        INSERT INTO lOGS
        (Description,
        Source,
        CreatedDate)
    VALUES
        (ERROR_MESSAGE() + ' :: ' + CAST(@productId AS VARCHAR) + ', ' + @MerchantTransactionId + ', '            
                + CAST(@paidAmount AS VARCHAR) + ', ' + @couponCode,
            'ManagePurchaseOrderSM',
            @CurrentDate)            
    END CATCH
END 




---------- Guna Surya  21-04-2025 11:54 AM  --------------------------------------------------------------------
GO
alter PROCEDURE [dbo].[GetFilterUsersBy]               
@userType VARCHAR(50)              
AS BEGIN              
 SELECT               
  users.Id, users.PublicKey, FirstName , users.LastName , UserMappings.UserType               
 FROM Users INNER JOIN UserMappings              
 ON users.PublicKey = UserMappings.UserKey where UserMappings.UserType = @userType  AND isnull(IsDisabled,0)=0  and UserMappings.IsActive=1             
      
 ORDER BY users.FirstName ASC              
END   
  
---- Ajith 21-04-2025m2:38 PM 

IF COL_LENGTH('ProductsM', 'IsQueryFormEnabled') IS NULL
BEGIN
    ALTER TABLE ProductsM
    ADD IsQueryFormEnabled BIT NOT NULL DEFAULT 0;
END

GO
--exec [GetProductById] 96,'d7f73b83-bee9-ef11-b38d-f7f49baa4b49'                           
ALTER PROCEDURE [dbo].[GetProductById]
    @ProductId INT,
    @MobileUserKey UNIQUEIDENTIFIER
AS
BEGIN
    --declare @ProductId int = 5                                                                   
    --,@MobileUserKey UNIQUEIDENTIFIER = '294A46B7-E918-EF11-B260-A630D7B5F938'                                                                 
    DECLARE @priorDaysInfo INT = CAST((SELECT TOP 1
        value
    FROM settings
    WHERE code = 'PRIORDAYSINFO') AS INT)
    DECLARE @ContentCount INT = 0, 
            @VideoContent INT = 0

    SELECT @ContentCount = COUNT(pc.Id),
        @VideoContent = COUNT(CASE 
                                      WHEN pc.attachmentType = 'video' THEN 1 
                                      ELSE NULL END) + (   SELECT COUNT(CASE 
                                                                             WHEN sc.Link IS NOT NULL THEN 1 
                                                                             ELSE NULL END)
        FROM SUBCHAPTERS sc
            JOIN CHAPTERS c
            ON sc.ChapterId = c.id
        WHERE c.ProductId = @ProductId
            AND c.IsActive  = 1
            AND c.IsDelete  = 0
            AND sc.IsActive = 1
            AND sc.IsDelete = 0)
    FROM PRODUCTSCONTENTM pc
    WHERE pc.ProductId = @ProductId
        AND pc.isActive  = 1
        AND pc.isDeleted = 0;


    DROP TABLE IF EXISTS #tempBenefits
    SELECT ben.Id,
        ben.GiftProductId,
        sub.NAME AS Names,
        ben.Months,
        pro.NAME,
        pro.Description
    INTO   #tempBenefits
    FROM ExtraBenefitsM AS ben
        INNER JOIN ProductsM AS pro
        ON ben.GiftProductId       = pro.Id
        INNER JOIN SubscriptionDurationM AS sub
        ON sub.Id                  = ben.SubscriptionId
            AND isnull(sub.isActive, 1) = 1
    WHERE ben.ProductId           = @ProductId
        AND ISNULL(pro.IsActive, 1) = 1

    DECLARE @extraBenefits NVARCHAR(MAX) = (SELECT *
    FROM #tempBenefits
    FOR JSON AUTO)
    DECLARE @CurrentDate DATE = cast(getdate() AS DATE)

    DECLARE @IsOutOfSubscription VARCHAR(300) = (   SELECT TOP 1
        mobileUserKey
    FROM MYBucketM b
    WHERE productId            = @ProductId
        AND mobileUserKey        = @MobileUserKey
        AND ISNULL(IsActive, 1)  = 1
        AND isnull(IsExpired, 0) = 0
        AND @currentDate         >= cast(b.StartDate AS DATE)
        AND @currentDate         <= cast(b.endDate AS DATE))
    DECLARE @isExpired NVARCHAR(max) = (   SELECT TOP 1
        IsExpired
    FROM MYBucketM
    WHERE productId            = @ProductId
        AND mobileUserKey        = @MobileUserKey
        AND ISNULL(IsACtive, 1)  = 1
        AND isnull(IsExpired, 0) = 0)

    SELECT TOP 1
        p.id,
        p.NAME,
        p.code,
        p.Description,
        p.DescriptionTitle,
        p.CategoryID,
        P.IsQueryFormEnabled,
        CASE 
                WHEN (   p.isActive = 1
            AND mb.id IS NOT NULL)
            OR (   p.isActive = 0
            AND mb.id IS NOT NULL) THEN pcmm.CommunityId 
                ELSE NULL END AS CommunityId,
        CASE 
                WHEN (   p.isActive = 1
            AND mb.id IS NOT NULL)
            OR (   p.isActive = 0
            AND mb.id IS NOT NULL) THEN pc.name 
                ELSE NULL END AS communityname,
        pcm.NAME AS Category,
        CAST(p.Price AS DECIMAL(16, 4)) AS Price,
        cast(pom.CouponKey AS VARCHAR(200)) AS CouponCode,
        isnull(pom.PaidAmount, 0.0) AS PaidAmount,
        CAST(ISNULL(pom.CouponDiscountAmount, 0.0) AS VARCHAR) AS Discount,
        CAST(ISNULL(PR.Rating, 0) AS VARCHAR) AS UserRating,
        '' AS Liked,
        '' AS EnableSubscription,
        (   SELECT top 1
            sv.DurationName
        FROM SubscriptionView sv
        WHERE ProductId         = @ProductId
            and sv.DurationName   != 'free'
            AND sv.DurationActive = 1) AS SubscriptionData,
        CAST(ISNULL(pl.LikeId, 0) AS BIT) AS IsHeart,
        CAST(0 AS BIT) AS IsThumbsUp,
        @extraBenefits AS ExtraBenefits,
        CAST(CASE 
                     WHEN DATEDIFF(day, GETDATE(), mb.enddate) <= @priorDaysInfo THEN 1 
                     ELSE 0 END AS BIT) AS ShowReminder,
        CAST(IIF(@isExpired IS NOT NULL, 1, 0) AS BIT) AS IsInMyBucket,
        (   SELECT TOP 1
            Attachment
        FROM ProductsContentM
        WHERE productId = @ProductId
            AND title LIKE '%intro%') AS LandscapeVideoUrl,
        P.LandscapeImage AS LandscapeImage,
        CAST(IIF(@IsOutOfSubscription IS NULL, 0, 1) AS BIT) AS IsInValidity,
        (SELECT *
        FROM ProductsContentM
        WHERE productId = @ProductId
        FOR JSON AUTO) AS Content,
        ( 
           --buy button text                                                                         
           CASE 
                -- Case for Active Products                             
                WHEN p.isActive = 1 THEN 
                    CASE 
                         WHEN mb.id IS NULL THEN 'Buy' 
                         ELSE CASE 
                                   WHEN DATEDIFF(day, GETDATE(), mb.enddate) < @priorDaysInfo THEN 'Renew' 
                                   ELSE 'Purchased' END END 
 
                -- Case for Inactive Products                             
                WHEN p.isActive = 0 THEN CASE 
                                              WHEN mb.id IS NOT NULL THEN 'Purchased' 
                                              WHEN mb.id IS NULL THEN '' END END) AS BuyButtonText,
        @ContentCount AS ContentCount,
        @VideoContent AS VideoCount,
        DATEDIFF(day, getdate(), ISNULL(mb.enddate, GETDATE())) AS DaysToGo,
        (CASE 
                 WHEN mb.id IS NOT NULL
            AND DATEDIFF(day, GETDATE(), mb.enddate) >= @priorDaysInfo THEN '[]' 
                 ELSE (   SELECT *
        FROM (                   SELECT pb.BonusProductId AS Id,
                    p2.NAME AS BonusProductName,
                    pb.DurationInDays AS Validity,
                    (   SELECT s.Value
                    FROM Settings s
                    WHERE s.Code     = 'BonusMessage'
                        AND s.IsActive = 1) AS BonusMessage
                FROM ProductBonusMappingM pb
                    INNER JOIN ProductsM p2
                    ON pb.BonusProductId = p2.Id
                    LEFT JOIN ProductCategoriesM pcm2
                    ON p2.CategoryID     = pcm2.Id
                WHERE pb.ProductId = p.id
                    AND pb.IsActive  = 1
                    and pb.IsDeleted = 0
                    AND p2.IsActive  = 1
                    AND p2.IsDeleted = 0
            UNION ALL
                SELECT pcm.CommunityId,
                    max(pTemp.Name) as BonusProductName,
                    min(sd.Months * 30) as Validity,
                    (   SELECT s.Value
                    FROM Settings s
                    WHERE s.Code     = 'BonusMessage'
                        AND s.IsActive = 1) AS BonusMessage
                From ProductCommunityMappingM as pcm
                    inner join SubscriptionMappingM as sm
                    on pcm.CommunityId           = sm.ProductId
                    inner join SubscriptionDurationM as sd
                    on sm.SubscriptionDurationId = sd.Id
                    inner join ProductsM as pTemp
                    on pTemp.Id                  = pcm.CommunityId
                where pcm.ProductId              = p.Id
                    and ISNULL(pTemp.IsActive, 1)  = 1
                    and ISNULL(pTemp.IsDeleted, 0) = 0
                    and ISNULL(pcm.IsActive, 1)    = 1
                    and ISNULL(pcm.IsDeleted, 0)   = 0
                    and ISNULL(sm.IsActive, 1)     = 1
                    and ISNULL(sd.IsActive, 1)     = 1
                group by pcm.CommunityId) AS UnionedResults
        FOR JSON PATH) END) AS BonusProducts
    FROM ProductsM AS P
        INNER JOIN ProductCategoriesM AS pcm
        ON p.CategoryID     = pcm.Id
        LEFT JOIN PurchaseOrdersM AS POM
        ON POM.ProductId    = p.Id
            AND pom.ProductId    = @ProductId
            AND pom.ActionBy     = @MobileUserKey
        LEFT JOIN ProductsRatingM AS PR
        ON PR.ProductId     = P.Id
            AND PR.CreatedBy     = @MobileUserKey
        LEFT JOIN ProductLikesM AS pl
        ON pl.ProductId     = p.Id
            AND pl.LikeId        = 1
            AND pl.CreatedBy     = @MobileUserKey
            AND pl.IsDelete      = 0
        LEFT JOIN subscriptiondurationm s
        ON s.Id             = p.SubscriptionId
        LEFT JOIN MYBucketM AS Mb
        ON p.id             = mb.ProductId
            AND mb.mobileuserkey = @MobileUserkey
        left join ProductCommunityMappingM pcmm
        on p.Id             = pcmm.ProductId
            and pcmm.IsActive    = 1
            and pcmm.IsDeleted   = 0
        LEFT JOIN ProductsM pc
        ON pcmm.CommunityId = pc.Id
            and pc.IsActive      = 1
            and pc.IsDeleted     = 0
    WHERE p.id = @ProductId
    ORDER BY POM.CreatedOn DESC
END



--Siva 23 April 2025
--GetPhonePePaymentReportBarChart
GO
CREATE PROCEDURE [dbo].[GetPhonePePaymentReportBarChart]      
    @FromDate VARCHAR(50) = NULL,        
    @ToDate VARCHAR(50) = NULL,         
    @ProductId INT = NULL, -- NEW: filter by ProductId    
    @DurationType NVARCHAR(20) = NULL -- NEW: 'Monthly', 'Quarterly', 'HalfYearly', 'Yearly'    
AS      
BEGIN      
    SET NOCOUNT ON;      
      
    -- Default dates if not provided    
    IF @FromDate IS NULL       
        SET @FromDate = CONVERT(VARCHAR(10), DATEADD(YEAR, -1, GETDATE()), 120);      
    IF @ToDate IS NULL       
        SET @ToDate = CONVERT(VARCHAR(10), GETDATE(), 120);      
 IF @DurationType IS NULL    
        SET @DurationType = 'Monthly';    
      
    IF @DurationType = 'Monthly'    
    BEGIN    
        SELECT        
            FORMAT(phonePe.CreatedOn, 'MMM-yyyy') AS Duration,    
            p.Name AS ProductName,    
            SUM(ISNULL(phonePe.Amount, 0)) AS TotalPaidAmount,    
            COUNT(mu.FullName) AS TotalUserCount    
        FROM PaymentRequestStatusM AS prs       
        INNER JOIN MobileUsers AS mu ON prs.CreatedBy = mu.PublicKey      
        INNER JOIN ProductsM AS p ON prs.ProductId = p.Id      
        INNER JOIN SubscriptionMappingM AS sm ON prs.SubscriptionMappingId = sm.Id      
        INNER JOIN SubscriptionDurationM AS sd ON sm.SubscriptionDurationId = sd.Id      
        INNER JOIN PhonePePaymentResponseM AS phonePe ON phonePe.MerchantTransactionId = prs.TransactionId      
             
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(phonePe.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))    
            AND (@ProductId IS NULL OR prs.ProductId = @ProductId)    
        GROUP BY FORMAT(phonePe.CreatedOn, 'MMM-yyyy'), p.Name    
        ORDER BY MIN(YEAR(phonePe.CreatedOn)), MIN(MONTH(phonePe.CreatedOn)), p.Name    
    END    
    ELSE IF @DurationType = 'Quarterly'    
    BEGIN    
        SELECT        
            CONCAT('Q', DATEPART(QUARTER, phonePe.CreatedOn), '-', YEAR(phonePe.CreatedOn)) AS Duration,    
            p.Name AS ProductName,    
            SUM(ISNULL(phonePe.Amount, 0)) AS TotalPaidAmount,    
            COUNT(mu.FullName) AS TotalUserCount    
        FROM PaymentRequestStatusM AS prs       
        INNER JOIN MobileUsers AS mu ON prs.CreatedBy = mu.PublicKey      
        INNER JOIN ProductsM AS p ON prs.ProductId = p.Id      
        INNER JOIN SubscriptionMappingM AS sm ON prs.SubscriptionMappingId = sm.Id      
        INNER JOIN SubscriptionDurationM AS sd ON sm.SubscriptionDurationId = sd.Id      
        INNER JOIN PhonePePaymentResponseM AS phonePe ON phonePe.MerchantTransactionId = prs.TransactionId      
             
            AND(@FromDate IS NULL OR @ToDate IS NULL OR CAST(phonePe.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))    
            AND (@ProductId IS NULL OR prs.ProductId = @ProductId)    
        GROUP BY DATEPART(QUARTER, phonePe.CreatedOn), YEAR(phonePe.CreatedOn), p.Name    
        ORDER BY MIN(YEAR(phonePe.CreatedOn)), MIN(DATEPART(QUARTER, phonePe.CreatedOn)), p.Name    
    END    
    ELSE IF @DurationType = 'HalfYearly'    
    BEGIN    
        SELECT        
            CASE     
                WHEN DATEPART(MONTH, phonePe.CreatedOn) BETWEEN 1 AND 6 THEN CONCAT('H1-', YEAR(phonePe.CreatedOn))    
                ELSE CONCAT('H2-', YEAR(phonePe.CreatedOn))    
            END AS Duration,    
            p.Name AS ProductName,    
            SUM(ISNULL(phonePe.Amount, 0)) AS TotalPaidAmount,    
            COUNT(mu.FullName) AS TotalUserCount    
        FROM PaymentRequestStatusM AS prs       
        INNER JOIN MobileUsers AS mu ON prs.CreatedBy = mu.PublicKey      
        INNER JOIN ProductsM AS p ON prs.ProductId = p.Id      
        INNER JOIN SubscriptionMappingM AS sm ON prs.SubscriptionMappingId = sm.Id      
        INNER JOIN SubscriptionDurationM AS sd ON sm.SubscriptionDurationId = sd.Id      
        INNER JOIN PhonePePaymentResponseM AS phonePe ON phonePe.MerchantTransactionId = prs.TransactionId      
          
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(phonePe.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))   
            AND (@ProductId IS NULL OR prs.ProductId = @ProductId)    
        GROUP BY     
            CASE     
                WHEN DATEPART(MONTH, phonePe.CreatedOn) BETWEEN 1 AND 6 THEN CONCAT('H1-', YEAR(phonePe.CreatedOn))    
                ELSE CONCAT('H2-', YEAR(phonePe.CreatedOn))    
            END, p.Name    
        ORDER BY MIN(YEAR(phonePe.CreatedOn)), MIN(CASE WHEN DATEPART(MONTH, phonePe.CreatedOn) BETWEEN 1 AND 6 THEN 1 ELSE 2 END), p.Name    
    END    
    ELSE IF @DurationType = 'Yearly'    
    BEGIN    
        SELECT        
            CAST(YEAR(phonePe.CreatedOn) AS VARCHAR(4)) AS Duration,    
            p.Name AS ProductName,    
            SUM(ISNULL(phonePe.Amount, 0)) AS TotalPaidAmount,    
            COUNT(mu.FullName) AS TotalUserCount    
        FROM PaymentRequestStatusM AS prs       
        INNER JOIN MobileUsers AS mu ON prs.CreatedBy = mu.PublicKey      
        INNER JOIN ProductsM AS p ON prs.ProductId = p.Id      
        INNER JOIN SubscriptionMappingM AS sm ON prs.SubscriptionMappingId = sm.Id      
        INNER JOIN SubscriptionDurationM AS sd ON sm.SubscriptionDurationId = sd.Id      
        INNER JOIN PhonePePaymentResponseM AS phonePe ON phonePe.MerchantTransactionId = prs.TransactionId      
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(phonePe.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))  
            AND (@ProductId IS NULL OR prs.ProductId = @ProductId)    
        GROUP BY YEAR(phonePe.CreatedOn), p.Name    
        ORDER BY MIN(YEAR(phonePe.CreatedOn)), p.Name    
    END    
END; 


-------------------Guna Surya  23-04-2025 12:00 PM  ------------------------------
GO
alter PROCEDURE [dbo].[ManagePurchaseOrderSM]            
    @mobileUserKey UNIQUEIDENTIFIER,            
    @productId INT,            
    @SubscriptionMappingId INT,            
    @MerchantTransactionId VARCHAR(100),            
    @TransactionId VARCHAR(100),            
    @paidAmount DECIMAL(18, 2),            
    @couponCode VARCHAR(20) = NULL            
AS            
BEGIN            
    DECLARE @CurrentDate DATETIME = GETDATE(),                        
            @leadsourcekey UNIQUEIDENTIFIER,                        
            @AdminKey UNIQUEIDENTIFIER            
    SELECT @leadsourcekey = publickey            
    FROM LeadSources            
    WHERE name = 'mobileapp'            
    SELECT @AdminKey = us.publickey            
    FROM Users AS us            
        INNER JOIN Roles AS ro ON us.RoleKey = ro.PublicKey            
    WHERE ro.Name = 'Admin'            
        AND us.PublicKey = '3CA214D0-8CB8-EB11-AAF2-00155D53687A'            
      
    BEGIN TRANSACTION;            
    BEGIN TRY                        
        DECLARE @NewLeadKey UNIQUEIDENTIFIER,                        
                @MobileNumber VARCHAR(20);                              
        SELECT @MobileNumber = Mobile,            
               @NewLeadKey = LeadKey            
        FROM MobileUsers            
        WHERE PublicKey = @mobileUserKey;            
          
        IF @NewLeadKey IS NULL OR NOT EXISTS (SELECT 1 FROM Leads WHERE PublicKey = @NewLeadKey)                        
        BEGIN            
            SELECT TOP 1 @NewLeadKey = PublicKey            
            FROM Leads            
            WHERE MobileNumber = @MobileNumber;            
            IF @NewLeadKey IS NOT NULL                        
            BEGIN            
                UPDATE MobileUsers                        
                SET LeadKey = @NewLeadKey                        
                WHERE PublicKey = @mobileUserKey;            
            END                        
            ELSE                        
            BEGIN            
                SELECT @NewLeadKey = NEWID()            
                INSERT INTO Leads (SupervisorId, PublicKey, FullName, Gender, MobileNumber, AlternateMobileNumber, EmailId,            
                    ProfileImage, PriorityStatus, AssignedTo, ServiceKey, LeadTypeKey, LeadSourceKey, Remarks, IsDisabled,            
                    IsDelete, CreatedOn, CreatedBy, IsSpam, IsWon, ModifiedOn, ModifiedBy, City, PinCode, StatusId,            
                    PurchaseOrderKey, Favourite, CountryCode)            
                SELECT NULL, @NewLeadKey, FullName, (CASE WHEN Gender = 'male' THEN 'm' ELSE 'f' END), Mobile, NULL, EmailId, NULL, 1, NULL,            
                    'C11DAA9F-F125-EE11-811D-00155D23D79C', NULL, @leadsourcekey, 'Reg. via mobile app', 0, 0, @CurrentDate, @AdminKey, 0,            
                    0, @CurrentDate, @AdminKey, City, '', 1, NULL, 1, CountryCode            
                FROM MobileUsers            
                WHERE PublicKey = @mobileUserKey;            
                UPDATE MobileUsers                   
                SET LeadKey = @NewLeadKey                        
                WHERE PublicKey = @mobileUserKey            
            END            
        END                        
          
        DECLARE @couponMOP INT,                        
                @razorMOP INT                        
        SELECT @couponMOP = Id FROM PaymentModes WHERE name = 'Coupon';                        
        SELECT @razorMOP = Id FROM PaymentModes WHERE name = 'Razor';                        
        DROP TABLE IF EXISTS #TempTable                        
        SELECT * INTO #TempTable FROM MobileUsers WHERE publickey = @mobileUserKey                        
                        
        DECLARE @couponCodeExists BIT,                        
                @discountAmount DECIMAL(18, 2),                        
                @discountPercent INT,                        
                @price DECIMAL(18, 2),                        
                @couponkey UNIQUEIDENTIFIER,                        
                @startDate DATETIME = @currentDate,                        
                @endDate DATETIME,                        
                @couponHasProductValidity BIT,                        
                @couponValidityDays INT = 0,                        
                @FinalValidityInDays INT = 0,                        
                @ProductName VARCHAR(100) = '',                        
                @validity INT = NULL;                        
                        
        SELECT @couponkey = publickey, @validity = ProductValidityInDays            
        FROM CouponsM            
        WHERE Name = @couponCode AND IsActive = 1 AND IsDelete = 0 AND RedeemLimit > TotalRedeems                                                               
        SELECT @discountPercent = sm.DiscountPercentage, @price = p.Price, @discountAmount = ((p.Price * (sm.DiscountPercentage) / 100)),            
               @couponValidityDays = 0, @endDate = DATEADD(MONTH, sd.Months, GETDATE()), @ProductName = p.Name,            
               @FinalValidityInDays = DATEDIFF(DAY, GETDATE(), DATEADD(MONTH, sd.Months, GETDATE())) - 1            
        FROM SubscriptionMappingM AS sm            
        INNER JOIN SubscriptionDurationM AS sd ON sm.SubscriptionDurationId = sd.Id            
        INNER JOIN ProductsM AS p ON sm.ProductId = p.Id            
        WHERE sm.Id = @SubscriptionMappingId AND ProductId = @productId AND sm.IsActive = 1                                            
        SELECT @couponHasProductValidity = IIF(ProductValidityInDays IS NULL, 0, 1), @couponValidityDays = ProductValidityInDays,            
               @discountPercent = IIF(DiscountInPercentage IS NULL, 0, 1)            
        FROM CouponsM c WHERE publickey = @couponkey;                                                       
          
        IF (@couponValidityDays > @FinalValidityInDays)                        
        BEGIN            
            SET @FinalValidityInDays = @couponValidityDays            
        END                        
          
        -- Check if the main product is active before adding to myBucketM  
        IF EXISTS (SELECT 1 FROM ProductsM WHERE Id = @productId AND IsActive = 1 AND IsDeleted = 0)  
        BEGIN  
            IF EXISTS (SELECT 1 FROM MYBucketM WHERE MobileUserKey = @MobileUserKey AND ProductId = @ProductId)            
            BEGIN            
                SELECT @startDate = CASE WHEN EndDate > @CurrentDate THEN StartDate ELSE @CurrentDate END,                  
                       @EndDate = (CASE WHEN EndDate > @CurrentDate THEN DATEADD(DAY, @FinalValidityInDays + 1, EndDate)                  
                                        ELSE DATEADD(DAY, @FinalValidityInDays + 1, @CurrentDate) END)                  
                FROM MYBucketM WHERE MobileUserKey = @MobileUserKey AND ProductId = @ProductId                 
            
                UPDATE MYBucketM            
                SET ProductName = @ProductName, StartDate = @startDate, EndDate = @endDate, ModifiedBy = @mobileUserKey,            
                    ModifiedDate = @CurrentDate, IsActive = 1, IsExpired = 0, Status = 1            
                WHERE MobileUserKey = @MobileUserKey AND ProductId = @ProductId;            
            END            
            ELSE                        
            BEGIN            
                INSERT INTO myBucketM (MobileUserKey, ProductId, ProductName, StartDate, EndDate, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, IsActive, IsExpired)            
                VALUES (@MobileUserKey, @ProductId, @ProductName, @startDate, @endDate, 1, @MobileUserKey, @CurrentDate, NULL, NULL, 1, 0);            
            END                           
        END  
          
        DECLARE @IsOldDevice BIT = 0;                
      SELECT @IsOldDevice = CASE                
            WHEN mu.DeviceVersion IS NULL THEN 1                
            WHEN (mu.DeviceType LIKE 'iOS%' OR mu.DeviceType LIKE 'IosId:%') AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1            
                 AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0 AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 16 THEN 1                
            WHEN mu.DeviceType LIKE 'Android:%' AND TRY_CAST(PARSENAME(mu.DeviceVersion, 3) AS INT) = 1            
                 AND TRY_CAST(PARSENAME(mu.DeviceVersion, 2) AS INT) = 0 AND TRY_CAST(PARSENAME(mu.DeviceVersion, 1) AS INT) < 61 THEN 1                
            ELSE 0 END            
        FROM MobileUsers AS mu WHERE PublicKey = @mobileUserKey;                
               
        IF (@IsOldDevice = 0)          
        BEGIN          
            DECLARE @communitymappingdays INT, @ExistingCommunityStartDate DATE, @ExistingCommunityEndDate DATE, @CommunityStartDate DATE,            
                    @CommunityEndDate DATE, @CommunityId INT, @CommunityName VARCHAR(100);            
            SELECT @communitymappingdays = pcm.DurationInDays FROM ProductCommunityMappingM pcm WHERE pcm.ProductId = @ProductId AND pcm.IsActive = 1;            
            SELECT TOP 1 @CommunityId = pcm.CommunityId, @CommunityName = pc.Name            
            FROM ProductCommunityMappingM pcm JOIN ProductsM pc ON pc.Id = pcm.CommunityId            
            WHERE pcm.ProductId = @ProductId AND pcm.IsActive = 1 AND pc.IsActive = 1 AND pc.IsDeleted = 0;            
            SELECT @ExistingCommunityStartDate = StartDate, @ExistingCommunityEndDate = EndDate            
            FROM myBucketM WHERE MobileUserKey = @MobileUserKey AND ProductId = @CommunityId;            
            SET @CommunityStartDate = CASE WHEN @ExistingCommunityEndDate > @CurrentDate THEN @ExistingCommunityStartDate ELSE @CurrentDate END;            
            SET @CommunityEndDate = CASE WHEN @ExistingCommunityEndDate > @CurrentDate THEN DATEADD(DAY, @communitymappingdays + 1, @ExistingCommunityEndDate)            
                                         ELSE DATEADD(DAY, @communitymappingdays + 1, @CurrentDate) END;            
              
            -- Check if the community product is active before adding to myBucketM  
            IF EXISTS (SELECT 1 FROM ProductsM WHERE Id = @productId AND IsActive = 1 AND IsDeleted = 0)  
            BEGIN  
                IF @ExistingCommunityEndDate IS NOT NULL  
                BEGIN  
                    UPDATE myBucketM SET StartDate = @CommunityStartDate, EndDate = @CommunityEndDate, ModifiedBy = @MobileUserKey, ModifiedDate = @CurrentDate,            
                                        IsExpired = 0, IsActive = 1, Status = 1, ProductName = @CommunityName            
                    WHERE MobileUserKey = @MobileUserKey AND ProductId = @CommunityId;            
                END  
                ELSE  
                BEGIN  
                    INSERT INTO myBucketM (MobileUserKey, ProductId, ProductName, StartDate, EndDate, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, IsActive, IsExpired)            
                    VALUES (@MobileUserKey, @CommunityId, @CommunityName, @CommunityStartDate, @CommunityEndDate, 1, @MobileUserKey, @CurrentDate, NULL, NULL, 1, 0);            
                END  
            END  
        END          
          
        DECLARE @BonusProductId INT, @BonusProductName VARCHAR(100), @BonusProductDurationInDays INT, @BonusProductStartDate DATETIME,            
                @BonusProductEndDate DATETIME, @ExistingBonusStartDate DATETIME, @ExistingBonusEndDate DATETIME;      
        SELECT @BonusProductId = pbm.BonusProductId, @BonusProductName = pm.Name, @BonusProductDurationInDays = pbm.DurationInDays      
        FROM ProductBonusMappingM pbm JOIN ProductsM pm ON pm.Id = pbm.BonusProductId      
        WHERE pbm.ProductId = @ProductId AND pbm.IsActive = 1 AND pm.IsActive = 1 AND pm.IsDeleted = 0;      
          
        -- Check if the bonus product is active before adding to myBucketM  
        IF @BonusProductId IS NOT NULL AND EXISTS (SELECT 1 FROM ProductsM WHERE Id = @productId AND IsActive = 1 AND IsDeleted = 0)                        
        BEGIN      
            SELECT @ExistingBonusStartDate = StartDate, @ExistingBonusEndDate = EndDate      
            FROM myBucketM WHERE MobileUserKey = @MobileUserKey AND ProductId = @BonusProductId;      
            SET @BonusProductStartDate = CASE WHEN @ExistingBonusEndDate > @CurrentDate THEN @ExistingBonusStartDate ELSE @CurrentDate END;      
            SET @BonusProductEndDate = CASE WHEN @ExistingBonusEndDate > @CurrentDate THEN DATEADD(DAY, @BonusProductDurationInDays + 1, @ExistingBonusEndDate)      
                                            ELSE DATEADD(DAY, @BonusProductDurationInDays + 1, @CurrentDate) END;      
            IF @ExistingBonusEndDate IS NOT NULL      
            BEGIN      
                UPDATE myBucketM SET StartDate = @BonusProductStartDate, EndDate = @BonusProductEndDate, ModifiedBy = @MobileUserKey, ModifiedDate = @CurrentDate,            
                                    IsExpired = 0, IsActive = 1, Status = 1, ProductName = @BonusProductName      
                WHERE MobileUserKey = @MobileUserKey AND ProductId = @BonusProductId;      
            END      
            ELSE      
            BEGIN      
                INSERT INTO myBucketM (MobileUserKey, ProductId, ProductName, StartDate, EndDate, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, IsActive, IsExpired)      
                VALUES (@MobileUserKey, @BonusProductId, @BonusProductName, @BonusProductStartDate, @BonusProductEndDate, 1, @MobileUserKey, @CurrentDate, NULL, NULL, 1, 0);      
            END      
        END      
          
        IF EXISTS (SELECT 1 FROM [dbo].[PurchaseOrdersM] WHERE TransasctionReference = @MerchantTransactionId)                        
        BEGIN            
            UPDATE [dbo].[PurchaseOrdersM]                        
            SET StartDate = @CurrentDate, EndDate = @endDate, Product = @ProductName, ProductId = @ProductId, CouponKey = @couponCode,                        
                NetAmount = @price, PaidAmount = @paidAmount, ModifiedOn = @CurrentDate, Remark = Remark + ': ' + 'Re executed the procedures'                        
            WHERE TransasctionReference = @MerchantTransactionId;            
        END                        
        ELSE                        
        BEGIN            
            INSERT INTO [dbo].[PurchaseOrdersM] ([LeadId], [ClientName], [Mobile], [Email], [DOB], [Remark], [PaymentDate], [ModeOfPayment],            
                [BankName], [Pan], [State], [City], [TransasctionReference], [ProductId], [Product], [NetAmount], [PaidAmount], [CouponKey],            
                [CouponDiscountAmount], [CouponDiscountPercent], [Status], [ActionBy], [PaymentStatusId], [PaymentActionDate], [CreatedOn], [CreatedBy],            
                [ModifiedOn], [ModifiedBy], [StartDate], [EndDate], [IsActive], [KycApproved], [KycApprovedDate], SubscriptionMappingId, TransactionId)            
            SELECT (SELECT Id FROM Leads WHERE PublicKey = LeadKey), fullname, mobile, emailid, Dob, NULL, @CurrentDate,            
                   (CASE WHEN @paidAmount = 0.00 THEN @couponMOP ELSE @razorMOP END) AS ModeOfPayment, NULL, NULL, NULL, City, @MerchantTransactionId,            
                   @ProductId, @ProductName, @price, @paidAmount, @couponkey, @discountAmount, @discountPercent, 1, @MobileUserKey, 1, @CurrentDate, @CurrentDate,            
                   @MobileUserKey, NULL, NULL, @CurrentDate, @endDate, 1, 1, NULL, @SubscriptionMappingId, @TransactionId            
            FROM #TempTable;            
        END;                        
                        
        UPDATE CouponsM SET TotalRedeems = TotalRedeems + 1 WHERE publickey = @couponkey;                        
        UPDATE MobileUsers SET CancommunityPost = 1 WHERE publickey = @MobileUserKey;                        
        DROP TABLE #TempTable                        
        SELECT (SELECT p.name, p.code, CAST(@startDate AS DATE) AS StartDate, CAST(@endDate AS DATE) AS EndDate, (@FinalValidityInDays + 1) AS ProductValidity,            
                       @BonusProductName AS BonusProduct, @BonusProductDurationInDays AS BonusProductValidity,            
                       (SELECT TOP 1 pc.Name FROM ProductCommunityMappingM pcm JOIN ProductsM pc ON pc.Id = pcm.CommunityId            
                        WHERE pcm.ProductId = @ProductId AND pcm.IsActive = 1 AND pc.IsActive = 1 AND pc.IsDeleted = 0) AS Community,            
                       (SELECT TOP 1 cat.Name FROM ProductCategoriesM cat WHERE cat.Id = p.CategoryId AND cat.IsActive = 1 AND cat.IsDelete = 0) AS ProductCategory            
                FROM ProductsM p WHERE p.id = @productId FOR JSON AUTO) AS JsonData                  
        COMMIT TRANSACTION;                        
    END TRY                        
    BEGIN CATCH                        
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;                        
        SELECT ERROR_MESSAGE() AS JsonData, ERROR_LINE() AS lINE, ERROR_NUMBER() AS NUMBER                        
        INSERT INTO lOGS (Description, Source, CreatedDate)            
        VALUES (ERROR_MESSAGE() + ' :: ' + CAST(@productId AS VARCHAR) + ', ' + @MerchantTransactionId + ', ' + CAST(@paidAmount AS VARCHAR) + ', ' + @couponCode,            
                'ManagePurchaseOrderSM', @CurrentDate)                        
    END CATCH            
END



-------------------Guna Surya  24-04-2025 12:00 PM  ------------------------------
GO
alter PROCEDURE [dbo].[ManagePurchaseOrderSM]            
    @mobileUserKey UNIQUEIDENTIFIER,            
    @productId INT,            
    @SubscriptionMappingId INT,            
    @MerchantTransactionId VARCHAR(100),            
    @TransactionId VARCHAR(100),            
    @paidAmount DECIMAL(18, 2),            
    @couponCode VARCHAR(20) = NULL            
AS            
BEGIN            
    DECLARE @CurrentDate DATETIME = GETDATE(),                        
            @leadsourcekey UNIQUEIDENTIFIER,                        
            @AdminKey UNIQUEIDENTIFIER            
    SELECT @leadsourcekey = publickey            
    FROM LeadSources            
    WHERE name = 'mobileapp'            
    SELECT @AdminKey = us.publickey            
    FROM Users AS us            
        INNER JOIN Roles AS ro ON us.RoleKey = ro.PublicKey            
    WHERE ro.Name = 'Admin'            
        AND us.PublicKey = '3CA214D0-8CB8-EB11-AAF2-00155D53687A'            
      
    BEGIN TRANSACTION;            
    BEGIN TRY                        
        DECLARE @NewLeadKey UNIQUEIDENTIFIER,                        
                @MobileNumber VARCHAR(20);                              
        SELECT @MobileNumber = Mobile,            
               @NewLeadKey = LeadKey            
        FROM MobileUsers            
        WHERE PublicKey = @mobileUserKey;            
          
        IF @NewLeadKey IS NULL OR NOT EXISTS (SELECT 1 FROM Leads WHERE PublicKey = @NewLeadKey)                        
        BEGIN            
            SELECT TOP 1 @NewLeadKey = PublicKey            
            FROM Leads            
            WHERE MobileNumber = @MobileNumber;            
            IF @NewLeadKey IS NOT NULL                        
            BEGIN            
                UPDATE MobileUsers                        
                SET LeadKey = @NewLeadKey                        
                WHERE PublicKey = @mobileUserKey;            
            END                        
            ELSE                        
            BEGIN            
                SELECT @NewLeadKey = NEWID()            
                INSERT INTO Leads (SupervisorId, PublicKey, FullName, Gender, MobileNumber, AlternateMobileNumber, EmailId,            
                    ProfileImage, PriorityStatus, AssignedTo, ServiceKey, LeadTypeKey, LeadSourceKey, Remarks, IsDisabled,            
                    IsDelete, CreatedOn, CreatedBy, IsSpam, IsWon, ModifiedOn, ModifiedBy, City, PinCode, StatusId,            
                    PurchaseOrderKey, Favourite, CountryCode)            
                SELECT NULL, @NewLeadKey, FullName, (CASE WHEN Gender = 'male' THEN 'm' ELSE 'f' END), Mobile, NULL, EmailId, NULL, 1, NULL,            
                    'C11DAA9F-F125-EE11-811D-00155D23D79C', NULL, @leadsourcekey, 'Reg. via mobile app', 0, 0, @CurrentDate, @AdminKey, 0,            
                    0, @CurrentDate, @AdminKey, City, '', 1, NULL, 1, CountryCode            
                FROM MobileUsers            
                WHERE PublicKey = @mobileUserKey;            
                UPDATE MobileUsers                   
                SET LeadKey = @NewLeadKey                        
                WHERE PublicKey = @mobileUserKey            
            END            
        END                        
          
        DECLARE @couponMOP INT,                        
                @razorMOP INT                        
        SELECT @couponMOP = Id FROM PaymentModes WHERE name = 'Coupon';                        
        SELECT @razorMOP = Id FROM PaymentModes WHERE name = 'Razor';                        
        DROP TABLE IF EXISTS #TempTable                        
        SELECT * INTO #TempTable FROM MobileUsers WHERE publickey = @mobileUserKey                        
                        
        DECLARE @couponCodeExists BIT,                        
                @discountAmount DECIMAL(18, 2),                        
                @discountPercent INT,                        
                @price DECIMAL(18, 2),                        
                @couponkey UNIQUEIDENTIFIER,                        
                @startDate DATETIME = @currentDate,                        
                @endDate DATETIME,                        
                @couponHasProductValidity BIT,                        
                @couponValidityDays INT = 0,                        
                @FinalValidityInDays INT = 0,                        
                @ProductName VARCHAR(100) = '',                        
                @validity INT = NULL;                        
                        
        SELECT @couponkey = publickey, @validity = ProductValidityInDays            
        FROM CouponsM            
        WHERE Name = @couponCode AND IsActive = 1 AND IsDelete = 0 AND RedeemLimit > TotalRedeems                                                               
        SELECT @discountPercent = sm.DiscountPercentage, @price = p.Price, @discountAmount = ((p.Price * (sm.DiscountPercentage) / 100)),            
               @couponValidityDays = 0, @endDate = DATEADD(MONTH, sd.Months, GETDATE()), @ProductName = p.Name,            
               @FinalValidityInDays = DATEDIFF(DAY, GETDATE(), DATEADD(MONTH, sd.Months, GETDATE())) - 1            
        FROM SubscriptionMappingM AS sm            
        INNER JOIN SubscriptionDurationM AS sd ON sm.SubscriptionDurationId = sd.Id            
        INNER JOIN ProductsM AS p ON sm.ProductId = p.Id            
        WHERE sm.Id = @SubscriptionMappingId AND ProductId = @productId AND sm.IsActive = 1                                            
        SELECT @couponHasProductValidity = IIF(ProductValidityInDays IS NULL, 0, 1), @couponValidityDays = ProductValidityInDays,            
               @discountPercent = IIF(DiscountInPercentage IS NULL, 0, 1)            
        FROM CouponsM c WHERE publickey = @couponkey;                                                       
          
        IF (@couponValidityDays > @FinalValidityInDays)                        
        BEGIN            
            SET @FinalValidityInDays = @couponValidityDays            
        END                        
          
        -- Check if the main product is active before adding to myBucketM  
        IF EXISTS (SELECT 1 FROM ProductsM WHERE Id = @productId AND IsActive = 1 AND IsDeleted = 0)  
        BEGIN  
            IF EXISTS (SELECT 1 FROM MYBucketM WHERE MobileUserKey = @MobileUserKey AND ProductId = @ProductId)            
            BEGIN            
                SELECT @startDate = CASE WHEN EndDate > @CurrentDate THEN StartDate ELSE @CurrentDate END,                  
                       @EndDate = (CASE WHEN EndDate > @CurrentDate THEN DATEADD(DAY, @FinalValidityInDays + 1, EndDate)                  
                                        ELSE DATEADD(DAY, @FinalValidityInDays + 1, @CurrentDate) END)                  
                FROM MYBucketM WHERE MobileUserKey = @MobileUserKey AND ProductId = @ProductId                 
            
                UPDATE MYBucketM            
                SET ProductName = @ProductName, StartDate = @startDate, EndDate = @endDate, ModifiedBy = @mobileUserKey,            
                    ModifiedDate = @CurrentDate, IsActive = 1, IsExpired = 0, Status = 1            
                WHERE MobileUserKey = @MobileUserKey AND ProductId = @ProductId;            
            END            
            ELSE                        
            BEGIN            
                INSERT INTO myBucketM (MobileUserKey, ProductId, ProductName, StartDate, EndDate, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, IsActive, IsExpired)            
                VALUES (@MobileUserKey, @ProductId, @ProductName, @startDate, @endDate, 1, @MobileUserKey, @CurrentDate, NULL, NULL, 1, 0);            
            END                           
        END  
          
            --#region manage community mapping  
            DECLARE @communitymappingdays INT, @ExistingCommunityStartDate DATE, @ExistingCommunityEndDate DATE, @CommunityStartDate DATE,            
                    @CommunityEndDate DATE, @CommunityId INT, @CommunityName VARCHAR(100);            
            SELECT @communitymappingdays = pcm.DurationInDays FROM ProductCommunityMappingM pcm WHERE pcm.ProductId = @ProductId AND pcm.IsActive = 1;            
          
			SELECT TOP 1 @CommunityId = pcm.CommunityId, @CommunityName = pc.Name            
            FROM ProductCommunityMappingM pcm JOIN ProductsM pc ON pc.Id = pcm.CommunityId            
            WHERE pcm.ProductId = @ProductId AND pcm.IsActive = 1 AND pc.IsActive = 1 AND pc.IsDeleted = 0;
			
			IF (@CommunityId IS NOT NULL)
			BEGIN
				SELECT @ExistingCommunityStartDate = StartDate, @ExistingCommunityEndDate = EndDate            
				FROM myBucketM WHERE MobileUserKey = @MobileUserKey AND ProductId = @CommunityId;            
           
				SET @CommunityStartDate = CASE WHEN @ExistingCommunityEndDate > @CurrentDate THEN @ExistingCommunityStartDate ELSE @CurrentDate END;            
				SET @CommunityEndDate = CASE WHEN @ExistingCommunityEndDate > @CurrentDate THEN DATEADD(DAY, @communitymappingdays + 1, @ExistingCommunityEndDate)            
											ELSE DATEADD(DAY, @communitymappingdays + 1, @CurrentDate) END;            
              
				-- Check if the community product is active before adding to myBucketM  
				IF EXISTS (SELECT 1 FROM ProductsM WHERE Id = @productId AND IsActive = 1 AND IsDeleted = 0)  
				BEGIN  
				
				IF @ExistingCommunityEndDate IS NOT NULL  
				BEGIN  
					UPDATE myBucketM SET StartDate = @CommunityStartDate, EndDate = @CommunityEndDate, ModifiedBy = @MobileUserKey, ModifiedDate = @CurrentDate,            
										IsExpired = 0, IsActive = 1, Status = 1, ProductName = @CommunityName            
					WHERE MobileUserKey = @MobileUserKey AND ProductId = @CommunityId;            
				END  
				ELSE  
				BEGIN  
					INSERT INTO myBucketM (MobileUserKey, ProductId, ProductName, StartDate, EndDate, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, IsActive, IsExpired)            
					VALUES (@MobileUserKey, @CommunityId, @CommunityName, @CommunityStartDate, @CommunityEndDate, 1, @MobileUserKey, @CurrentDate, NULL, NULL, 1, 0);            
				END  
				END  
			END  -- Check if the bonus product is active before adding to myBucketM  
           --endRegion manage community mapping 
		            
          
     --#region Manage Bonus Product 
	    DECLARE @BonusProductId INT, @BonusProductName VARCHAR(100), @BonusProductDurationInDays INT, @BonusProductStartDate DATETIME,            
                @BonusProductEndDate DATETIME, @ExistingBonusStartDate DATETIME, @ExistingBonusEndDate DATETIME;      
        SELECT @BonusProductId = pbm.BonusProductId, @BonusProductName = pm.Name, @BonusProductDurationInDays = pbm.DurationInDays      
        FROM ProductBonusMappingM pbm JOIN ProductsM pm ON pm.Id = pbm.BonusProductId      
        WHERE pbm.ProductId = @ProductId AND pbm.IsActive = 1 AND pm.IsActive = 1 AND pm.IsDeleted = 0;      
          
        -- Check if the bonus product is active before adding to myBucketM  
        IF @BonusProductId IS NOT NULL AND EXISTS (SELECT 1 FROM ProductsM WHERE Id = @productId AND IsActive = 1 AND IsDeleted = 0)                        
        BEGIN      
            SELECT @ExistingBonusStartDate = StartDate, @ExistingBonusEndDate = EndDate      
            FROM myBucketM WHERE MobileUserKey = @MobileUserKey AND ProductId = @BonusProductId;      
            SET @BonusProductStartDate = CASE WHEN @ExistingBonusEndDate > @CurrentDate THEN @ExistingBonusStartDate ELSE @CurrentDate END;      
            SET @BonusProductEndDate = CASE WHEN @ExistingBonusEndDate > @CurrentDate THEN DATEADD(DAY, @BonusProductDurationInDays + 1, @ExistingBonusEndDate)      
                                            ELSE DATEADD(DAY, @BonusProductDurationInDays + 1, @CurrentDate) END;      
            IF @ExistingBonusEndDate IS NOT NULL      
            BEGIN      
                UPDATE myBucketM SET StartDate = @BonusProductStartDate, EndDate = @BonusProductEndDate, ModifiedBy = @MobileUserKey, ModifiedDate = @CurrentDate,            
                                    IsExpired = 0, IsActive = 1, Status = 1, ProductName = @BonusProductName      
                WHERE MobileUserKey = @MobileUserKey AND ProductId = @BonusProductId;      
            END      
            ELSE      
            BEGIN      
                INSERT INTO myBucketM (MobileUserKey, ProductId, ProductName, StartDate, EndDate, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, IsActive, IsExpired)      
                VALUES (@MobileUserKey, @BonusProductId, @BonusProductName, @BonusProductStartDate, @BonusProductEndDate, 1, @MobileUserKey, @CurrentDate, NULL, NULL, 1, 0);      
            END      
        END 
	 --#endRegion Manage Bonus Product
          
        IF EXISTS (SELECT 1 FROM [dbo].[PurchaseOrdersM] WHERE TransasctionReference = @MerchantTransactionId)                        
        BEGIN            
            UPDATE [dbo].[PurchaseOrdersM]                        
            SET StartDate = @CurrentDate, EndDate = @endDate, Product = @ProductName, ProductId = @ProductId, CouponKey = @couponkey,                        
                NetAmount = @price, PaidAmount = @paidAmount, ModifiedOn = @CurrentDate, Remark = Remark + ': ' + 'Re executed the procedures'                        
            WHERE TransasctionReference = @MerchantTransactionId;            
        END                        
        ELSE                        
        BEGIN            
            INSERT INTO [dbo].[PurchaseOrdersM] ([LeadId], [ClientName], [Mobile], [Email], [DOB], [Remark], [PaymentDate], [ModeOfPayment],            
                [BankName], [Pan], [State], [City], [TransasctionReference], [ProductId], [Product], [NetAmount], [PaidAmount], [CouponKey],            
                [CouponDiscountAmount], [CouponDiscountPercent], [Status], [ActionBy], [PaymentStatusId], [PaymentActionDate], [CreatedOn], [CreatedBy],            
                [ModifiedOn], [ModifiedBy], [StartDate], [EndDate], [IsActive], [KycApproved], [KycApprovedDate], SubscriptionMappingId, TransactionId)            
            SELECT (SELECT Id FROM Leads WHERE PublicKey = LeadKey), fullname, mobile, emailid, Dob, NULL, @CurrentDate,            
                   (CASE WHEN @paidAmount = 0.00 THEN @couponMOP ELSE @razorMOP END) AS ModeOfPayment, NULL, NULL, NULL, City, @MerchantTransactionId,            
                   @ProductId, @ProductName, @price, @paidAmount, @couponkey, @discountAmount, @discountPercent, 1, @MobileUserKey, 1, @CurrentDate, @CurrentDate,            
                   @MobileUserKey, NULL, NULL, @CurrentDate, @endDate, 1, 1, NULL, @SubscriptionMappingId, @TransactionId            
            FROM #TempTable;            
        END;                        
                        
        UPDATE CouponsM SET TotalRedeems = TotalRedeems + 1 WHERE publickey = @couponkey;                        
        --UPDATE MobileUsers SET CancommunityPost = 1 WHERE publickey = @MobileUserKey;                        
        DROP TABLE #TempTable                        
        SELECT (SELECT p.name, p.code, CAST(@startDate AS DATE) AS StartDate, CAST(@endDate AS DATE) AS EndDate, (@FinalValidityInDays + 1) AS ProductValidity,            
                       @BonusProductName AS BonusProduct, @BonusProductDurationInDays AS BonusProductValidity,            
                       (SELECT TOP 1 pc.Name FROM ProductCommunityMappingM pcm JOIN ProductsM pc ON pc.Id = pcm.CommunityId            
                        WHERE pcm.ProductId = @ProductId AND pcm.IsActive = 1 AND pc.IsActive = 1 AND pc.IsDeleted = 0) AS Community,            
                       (SELECT TOP 1 cat.Name FROM ProductCategoriesM cat WHERE cat.Id = p.CategoryId AND cat.IsActive = 1 AND cat.IsDelete = 0) AS ProductCategory            
                FROM ProductsM p WHERE p.id = @productId FOR JSON AUTO) AS JsonData                  
        COMMIT TRANSACTION;                        
    END TRY                        
    BEGIN CATCH                        
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;                        
        SELECT ERROR_MESSAGE() AS JsonData, ERROR_LINE() AS lINE, ERROR_NUMBER() AS NUMBER                        
        INSERT INTO lOGS (Description, Source, CreatedDate)            
       VALUES (ERROR_MESSAGE() + ' :: ' + CAST(@productId AS VARCHAR) + ', ' + @MerchantTransactionId + ', '          
                + CAST(@paidAmount AS VARCHAR) + ', ' + @couponCode + ' error_line: ' + cast(ERROR_LINE() as varchar), 'ManagePurchaseOrderSM',          
                @CurrentDate)                               
    END CATCH            
END


--Ajith 23-04-2025 3:02 PM
ALTER TABLE QueryFormM
ALTER COLUMN QueryRelatedTo INT NULL;

GO
--sp_helptext GetPartnerAccountsSummaryReport 
ALTER PROCEDURE [dbo].[GetPartnerAccountsSummaryReport]
    @PartnerAccountName VARCHAR(50) = NULL,
    @AssignedTo INT = 0,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL,
    @BenchMark INT OUTPUT
AS
BEGIN
    SET @BenchMark = 10;

    SET @FromDate = ISNULL(@FromDate, DATEADD(DD, -60, GETDATE()));
    SET @ToDate = ISNULL(@ToDate, GETDATE());

    SET @ToDate = DATEADD(DAY, 1, @ToDate);

    ;WITH
        CTE
        AS
        (
            SELECT TOP (DATEDIFF(DAY, @FromDate, @ToDate) + 1)
                Date = DATEADD(DAY, ROW_NUMBER() OVER(ORDER BY a.object_id) - 1, @FromDate)
            FROM sys.all_objects a CROSS JOIN sys.all_objects b
        ),
        GetTotalRegistration
        AS
        (
            SELECT CAST(d.DateOn AS DATE) AS DateOn, COUNT(1) AS TotalRegistration
            FROM (
                            SELECT Id, CAST(CreatedOn AS DATE) AS DateOn
                    FROM PartnerAccounts
                    WHERE CAST(CreatedOn AS DATE) BETWEEN @FromDate AND DATEADD(DAY, -1, @ToDate)
                        AND IsDelete = 0
                        AND (@AssignedTo = 0 OR AssignedTo = @AssignedTo)

                UNION

                    SELECT Id, CAST(ModifiedOn AS DATE) AS DateOn
                    FROM PartnerAccounts
                    WHERE CAST(ModifiedOn AS DATE) BETWEEN @FromDate AND DATEADD(DAY, -1, @ToDate)
                        AND Status = 0
                        AND IsDelete = 0
                        AND (@AssignedTo = 0 OR AssignedTo = @AssignedTo)
    ) d
            GROUP BY d.DateOn
        ),
        GetTotalConversion
        AS
        (
            SELECT CAST(ModifiedOn AS DATE) AS DateOn, COUNT(1) AS TotalConversion
            FROM PartnerAccounts
            WHERE ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, ModifiedOn)), ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999))       
            BETWEEN ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999)       
            AND ISNULL(@ToDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))
                AND STATUS = 2
                AND ISNULL(isdelete, 0) = 0
                AND (@AssignedTo = 0 OR AssignedTo = @AssignedTo)
            GROUP BY CAST(ModifiedOn AS DATE)
        )

    SELECT
        CAST(CTE.Date AS VARCHAR) AS Date,
        ISNULL(reg.TotalRegistration, 0) AS TotalRegistration,
        ISNULL(con.TotalConversion, 0) AS TotalConversion
    FROM CTE
        LEFT JOIN (SELECT DateOn, SUM(TotalRegistration) AS TotalRegistration
        FROM GetTotalRegistration
        GROUP BY DateOn) reg ON CTE.Date = reg.DateOn
        LEFT JOIN (SELECT DateOn, SUM(TotalConversion) AS TotalConversion
        FROM GetTotalConversion
        GROUP BY DateOn) con ON CTE.Date = con.DateOn
    WHERE CTE.Date < @ToDate
    ORDER BY CAST(CTE.Date AS DATE) ASC;
END 

-- Ajith 24-04-2025 12:10 AM

GO
ALTER PROCEDURE GetQueryForms
    @PageSize INT = 10,
    @PageNumber INT = 1,
    @PrimaryKey VARCHAR(100) = NULL,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL,
    @SearchText VARCHAR(100) = NULL,
    @TotalCount INT OUTPUT
AS      
BEGIN
    SET NOCOUNT ON;
    SET @PrimaryKey = IIF(@PrimaryKey = '', NULL, @PrimaryKey);

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Get total count for paging 
    SELECT @TotalCount = COUNT(1)
    FROM QueryFormM qf
        LEFT JOIN ProductsM p ON qf.ProductId = p.Id
        LEFT JOIN MobileUsers mu ON qf.MobileUserId = mu.PublicKey
    WHERE 
        qf.IsDeleted = 0
        AND (
            (@PrimaryKey IS NULL OR ISNULL(qf.IsActive, 0) = CAST(@PrimaryKey AS INT))
        )
        AND (
            (@FromDate IS NULL AND @ToDate IS NULL)
            OR (CAST(qf.CreatedOn AS DATE) >= CAST(@FromDate AS DATE) AND CAST(qf.CreatedOn AS DATE) <= CAST(@ToDate AS DATE))
        )
        AND (
            @SearchText IS NULL
            OR qf.Questions LIKE '%' + @SearchText + '%'
            OR qf.QueryRelatedTo LIKE '%' + @SearchText + '%'
            OR mu.Mobile LIKE '%' + @SearchText + '%'
            OR mu.FullName LIKE '%' + @SearchText + '%'
            OR p.Name LIKE '%' + @SearchText + '%'
        );

    -- Select query with pagination 
    SELECT qf.Id,
        mu.FullName as Name,
        qf.MobileUserId,
        qf.ProductId,
        p.Name AS ProductName,
        qf.Questions,
        qf.ScreenshotUrl,
        qf.QueryRelatedTo,
        qf.CreatedOn,
        mu.FullName AS CreatedBy,
        qf.ModifiedOn,
        u.FirstName + ' ' + u.LastName AS ModifiedBy,
        mu.Mobile,
        CASE WHEN qf.IsActive = 1 THEN 1 ELSE 0 END AS IsActive,
        (SELECT COUNT(*)
        FROM QueryFormRemarks qfr
        WHERE qfr.QueryId = qf.Id
            AND qfr.IsDeleted = 0) AS RemarksCount
    FROM QueryFormM qf
        LEFT JOIN ProductsM p ON qf.ProductId = p.Id
        LEFT JOIN MobileUsers mu ON qf.MobileUserId = mu.PublicKey
        LEFT JOIN Users u ON u.Id = qf.ModifiedBy
    WHERE 
        qf.IsDeleted = 0
        AND (
            (@PrimaryKey IS NULL OR ISNULL(qf.IsActive, 0) = CAST(@PrimaryKey AS INT))
        )
        AND (
            (@FromDate IS NULL AND @ToDate IS NULL)
            OR (CAST(qf.CreatedOn AS DATE) >= CAST(@FromDate AS DATE) AND CAST(qf.CreatedOn AS DATE) <= CAST(@ToDate AS DATE))
        )
        AND (
            @SearchText IS NULL
            OR qf.Questions LIKE '%' + @SearchText + '%'
            OR qf.QueryRelatedTo LIKE '%' + @SearchText + '%'
            OR mu.Mobile LIKE '%' + @SearchText + '%'
            OR mu.FullName LIKE '%' + @SearchText + '%'
            OR p.Name LIKE '%' + @SearchText + '%'
        )
    ORDER BY ISNULL(qf.ModifiedOn, qf.CreatedOn) DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END

-----Modify By Siva 24 April 2025 
GO
ALTER PROCEDURE [dbo].[GetPhonePePaymentReportBarChart]      
    @FromDate VARCHAR(50) = NULL,        
    @ToDate VARCHAR(50) = NULL,         
    @ProductId INT = NULL, -- NEW: filter by ProductId    
    @DurationType NVARCHAR(20) = NULL -- NEW: 'Monthly', 'Quarterly', 'HalfYearly', 'Yearly'    
AS      
BEGIN      
    SET NOCOUNT ON;      
      
    -- Default dates if not provided    
    IF @FromDate IS NULL       
        SET @FromDate = CONVERT(VARCHAR(10), DATEADD(YEAR, -1, GETDATE()), 120);      
    IF @ToDate IS NULL       
        SET @ToDate = CONVERT(VARCHAR(10), GETDATE(), 120);      
 IF @DurationType IS NULL    
        SET @DurationType = 'Monthly';    
      
    IF @DurationType = 'Monthly'    
    BEGIN    
        SELECT        
            FORMAT(PPR.CreatedOn, 'MMM-yyyy') AS Duration,    
            P.Name AS ProductName,    
            SUM(ISNULL(PPR.Amount, 0)) AS TotalPaidAmount,    
            COUNT(MU.FullName) AS TotalUserCount    
        FROM PaymentRequestStatusM PRS          
        INNER JOIN SubscriptionMappingM as sm on prs.SubscriptionMappingId = sm.Id          
        INNER JOIN SubscriptionDurationM as sd on sm.SubscriptionDurationId = sd.Id          
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId          
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey          
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id     
    WHERE PPR.Code IN('PAYMENT_SUCCESS', 'SUCCESS')
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PPR.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))    
            AND (@ProductId IS NULL OR PRS.ProductId = @ProductId)    
        GROUP BY FORMAT(PPR.CreatedOn, 'MMM-yyyy'), P.Name    
        ORDER BY MIN(YEAR(PPR.CreatedOn)), MIN(MONTH(PPR.CreatedOn)), P.Name    
    END    
    ELSE IF @DurationType = 'Quarterly'    
    BEGIN    
        SELECT        
            CONCAT('Q', DATEPART(QUARTER, PPR.CreatedOn), '-', YEAR(PPR.CreatedOn)) AS Duration,    
            P.Name AS ProductName,    
            SUM(ISNULL(PPR.Amount, 0)) AS TotalPaidAmount,    
            COUNT(mu.FullName) AS TotalUserCount    
        FROM PaymentRequestStatusM PRS          
        INNER JOIN SubscriptionMappingM as sm on prs.SubscriptionMappingId = sm.Id          
        INNER JOIN SubscriptionDurationM as sd on sm.SubscriptionDurationId = sd.Id          
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId          
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey          
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id     
    WHERE PPR.Code IN('PAYMENT_SUCCESS', 'SUCCESS')     
             
            AND(@FromDate IS NULL OR @ToDate IS NULL OR CAST(PPR.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))    
            AND (@ProductId IS NULL OR PRS.ProductId = @ProductId)    
        GROUP BY DATEPART(QUARTER, PPR.CreatedOn), YEAR(PPR.CreatedOn), P.Name    
        ORDER BY MIN(YEAR(PPR.CreatedOn)), MIN(DATEPART(QUARTER, PPR.CreatedOn)), P.Name    
    END    
    ELSE IF @DurationType = 'HalfYearly'    
    BEGIN    
        SELECT        
            CASE     
                WHEN DATEPART(MONTH, PPR.CreatedOn) BETWEEN 1 AND 6 THEN CONCAT('H1-', YEAR(PPR.CreatedOn))    
                ELSE CONCAT('H2-', YEAR(PPR.CreatedOn))    
            END AS Duration,    
            P.Name AS ProductName,    
            SUM(ISNULL(PPR.Amount, 0)) AS TotalPaidAmount,    
            COUNT(mu.FullName) AS TotalUserCount    
        FROM PaymentRequestStatusM PRS          
        INNER JOIN SubscriptionMappingM as sm on prs.SubscriptionMappingId = sm.Id          
        INNER JOIN SubscriptionDurationM as sd on sm.SubscriptionDurationId = sd.Id          
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId          
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey          
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id     
    WHERE PPR.Code IN('PAYMENT_SUCCESS', 'SUCCESS')
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PPR.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))   
            AND (@ProductId IS NULL OR PRS.ProductId = @ProductId)    
        GROUP BY     
            CASE     
                WHEN DATEPART(MONTH, PPR.CreatedOn) BETWEEN 1 AND 6 THEN CONCAT('H1-', YEAR(PPR.CreatedOn))    
                ELSE CONCAT('H2-', YEAR(PPR.CreatedOn))    
            END, p.Name    
        ORDER BY MIN(YEAR(PPR.CreatedOn)), MIN(CASE WHEN DATEPART(MONTH, PPR.CreatedOn) BETWEEN 1 AND 6 THEN 1 ELSE 2 END), P.Name    
    END    
    ELSE IF @DurationType = 'Yearly'    
    BEGIN    
        SELECT        
            CAST(YEAR(PPR.CreatedOn) AS VARCHAR(4)) AS Duration,    
            P.Name AS ProductName,    
            SUM(ISNULL(PPR.Amount, 0)) AS TotalPaidAmount,    
            COUNT(mu.FullName) AS TotalUserCount    
        FROM PaymentRequestStatusM PRS          
        INNER JOIN SubscriptionMappingM as sm on prs.SubscriptionMappingId = sm.Id          
        INNER JOIN SubscriptionDurationM as sd on sm.SubscriptionDurationId = sd.Id          
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId          
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey          
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id     
    WHERE PPR.Code IN('PAYMENT_SUCCESS', 'SUCCESS')     
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PPR.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))  
            AND (@ProductId IS NULL OR PRS.ProductId = @ProductId)    
        GROUP BY YEAR(PPR.CreatedOn), P.Name    
        ORDER BY MIN(YEAR(PPR.CreatedOn)), P.Name    
    END    
END; 

---Modify By Siva 24 April 2025 
  GO  
ALTER PROCEDURE [dbo].[GetPhonePePaymentReportChart]      
    @FromDate VARCHAR(50) = NULL,        
    @ToDate VARCHAR(50) = NULL         
AS      
BEGIN      
    SET NOCOUNT ON;      
      
    ---- If no dates are provided, set default to last 1 year (FromDate) and today (ToDate)      
    --IF @FromDate IS NULL       
    --    SET @FromDate = CONVERT(VARCHAR(10), DATEADD(YEAR, -1, GETDATE()), 120); -- Default to 1 year ago      
    --IF @ToDate IS NULL       
    --    SET @ToDate = CONVERT(VARCHAR(10), GETDATE(), 120); -- Default to today      
      
 select        
        
   --sm.SubscriptionDurationId , sd.Name, *        
    MAX(P.Name) AS ProductName,      
    SUM(isnull(PPR.Amount,0)) AS TotalPaidAmount,      
   COUNT(MU.FullName) AS TotalUserCount,      
           
   COUNT(CASE WHEN SD.Name = 'Quaterly' THEN 1 END) AS ThreeMonthsCount,  -- Count of 3 Months      
  COUNT(CASE WHEN SD.Name = 'Half-Yearly' THEN 1 END) AS SixMonthsCount,    -- Count of 6 Months      
  COUNT(CASE WHEN SD.Name = 'Yearly' THEN 1 END) AS TwelveMonthsCount ,-- Count of 12 Months      
  COUNT(CASE WHEN SD.Name = 'Free' THEN 1 END) AS FreeMonthsCount, -- Count of 12 Months      
  COUNT(CASE WHEN SD.Name = 'Month' THEN 1 END) AS OneMonthsCount -- Count of 12 Months      
      
      
  FROM PaymentRequestStatusM PRS          
        INNER JOIN SubscriptionMappingM as sm on prs.SubscriptionMappingId = sm.Id          
        INNER JOIN SubscriptionDurationM as sd on sm.SubscriptionDurationId = sd.Id          
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId          
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey          
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id     
    WHERE PPR.Code IN('PAYMENT_SUCCESS', 'SUCCESS') 
  and (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PPR.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))        
 GROUP BY PRS.ProductId      
END; 



-------------------Guna Surya  24-04-2025 5:07 PM  ------------------------------
--exec [GetProductById] 96,'d7f73b83-bee9-ef11-b38d-f7f49baa4b49' 
GO
CREATE PROCEDURE [dbo].[GetProductById]      
    @ProductId INT,      
    @MobileUserKey UNIQUEIDENTIFIER      
AS      
BEGIN      
    --declare @ProductId int = 5                                                                         
    --,@MobileUserKey UNIQUEIDENTIFIER = '294A46B7-E918-EF11-B260-A630D7B5F938'    
     
    DECLARE @priorDaysInfo INT = CAST((SELECT TOP 1      
        value      
    FROM settings      
    WHERE code = 'PRIORDAYSINFO') AS INT)      
    DECLARE @ContentCount INT = 0,       
            @VideoContent INT = 0      
      
    SELECT @ContentCount = COUNT(pc.Id),      
        @VideoContent = COUNT(CASE       
                                      WHEN pc.attachmentType = 'video' THEN 1       
                                      ELSE NULL END) + (   SELECT COUNT(CASE       
                                                                             WHEN sc.Link IS NOT NULL THEN 1       
                                                                             ELSE NULL END)      
        FROM SUBCHAPTERS sc      
            JOIN CHAPTERS c      
            ON sc.ChapterId = c.id      
        WHERE c.ProductId = @ProductId      
            AND c.IsActive  = 1      
            AND c.IsDelete  = 0      
            AND sc.IsActive = 1      
            AND sc.IsDelete = 0)      
    FROM PRODUCTSCONTENTM pc      
    WHERE pc.ProductId = @ProductId      
        AND pc.isActive  = 1      
        AND pc.isDeleted = 0;      
      
      
    DROP TABLE IF EXISTS #tempBenefits      
    SELECT ben.Id,      
        ben.GiftProductId,      
        sub.NAME AS Names,      
        ben.Months,      
        pro.NAME,      
        pro.Description      
    INTO   #tempBenefits      
    FROM ExtraBenefitsM AS ben      
        INNER JOIN ProductsM AS pro      
        ON ben.GiftProductId       = pro.Id      
        INNER JOIN SubscriptionDurationM AS sub      
        ON sub.Id                  = ben.SubscriptionId      
            AND isnull(sub.isActive, 1) = 1      
    WHERE ben.ProductId           = @ProductId      
        AND ISNULL(pro.IsActive, 1) = 1      
      
    DECLARE @extraBenefits NVARCHAR(MAX) = (SELECT *      
    FROM #tempBenefits      
    FOR JSON AUTO)      
    DECLARE @CurrentDate DATE = cast(getdate() AS DATE)      
      
    DECLARE @IsOutOfSubscription VARCHAR(300) = (   SELECT TOP 1      
        mobileUserKey      
    FROM MYBucketM b      
    WHERE productId            = @ProductId      
        AND mobileUserKey        = @MobileUserKey      
        AND ISNULL(IsActive, 1)  = 1      
        AND isnull(IsExpired, 0) = 0      
        AND @currentDate         >= cast(b.StartDate AS DATE)      
        AND @currentDate         <= cast(b.endDate AS DATE))      
    DECLARE @isExpired NVARCHAR(max) = (   SELECT TOP 1      
        IsExpired      
    FROM MYBucketM      
    WHERE productId            = @ProductId      
        AND mobileUserKey        = @MobileUserKey      
        AND ISNULL(IsACtive, 1)  = 1      
        AND isnull(IsExpired, 0) = 0)      
    
    
      DECLARE @accessToScanner BIT = 0;    
  IF EXISTS (    
    SELECT 1    
    FROM checkScannerWithStrategyExistsView AS viewTemp    
    WHERE viewTemp.ProductId = @ProductId    
      AND viewTemp.MobileUserKey = @MobileUserKey    
      AND CAST(ISNULL(viewTemp.EndDate, GETDATE()) AS DATE) >= CAST(GETDATE() AS DATE)    
)    
    
    BEGIN    
        SET @accessToScanner = 1;    
    END    
      
    SELECT TOP 1      
        p.id,      
        p.NAME,      
        p.code,      
        p.Description,      
        p.DescriptionTitle,      
        p.CategoryID,      
        P.IsQueryFormEnabled,      
        @accessToScanner AS accessToScanner,    
        CASE       
                WHEN (   p.isActive = 1      
            AND mb.id IS NOT NULL)      
            OR (   p.isActive = 0      
            AND mb.id IS NOT NULL) THEN pcmm.CommunityId       
      ELSE NULL END AS CommunityId,      
        CASE       
                WHEN (   p.isActive = 1      
            AND mb.id IS NOT NULL)      
            OR (   p.isActive = 0      
            AND mb.id IS NOT NULL) THEN pc.name       
                ELSE NULL END AS communityname,      
        pcm.NAME AS Category,      
        CAST(p.Price AS DECIMAL(16, 4)) AS Price,      
        cast(pom.CouponKey AS VARCHAR(200)) AS CouponCode,      
        isnull(pom.PaidAmount, 0.0) AS PaidAmount,      
        CAST(ISNULL(pom.CouponDiscountAmount, 0.0) AS VARCHAR) AS Discount,      
        CAST(ISNULL(PR.Rating, 0) AS VARCHAR) AS UserRating,      
        '' AS Liked,      
        '' AS EnableSubscription,      
        (   SELECT top 1      
            sv.DurationName      
        FROM SubscriptionView sv      
        WHERE ProductId         = @ProductId      
            and sv.DurationName   != 'free'      
            AND sv.DurationActive = 1) AS SubscriptionData,      
        CAST(ISNULL(pl.LikeId, 0) AS BIT) AS IsHeart,      
        CAST(0 AS BIT) AS IsThumbsUp,      
        @extraBenefits AS ExtraBenefits,      
        CAST(CASE       
                     WHEN DATEDIFF(day, GETDATE(), mb.enddate) <= @priorDaysInfo THEN 1       
                     ELSE 0 END AS BIT) AS ShowReminder,      
        CAST(IIF(@isExpired IS NOT NULL, 1, 0) AS BIT) AS IsInMyBucket,      
        (   SELECT TOP 1      
            Attachment      
        FROM ProductsContentM      
        WHERE productId = @ProductId      
            AND title LIKE '%intro%') AS LandscapeVideoUrl,      
        P.LandscapeImage AS LandscapeImage,      
        CAST(IIF(@IsOutOfSubscription IS NULL, 0, 1) AS BIT) AS IsInValidity,      
        (SELECT *      
        FROM ProductsContentM      
        WHERE productId = @ProductId      
        FOR JSON AUTO) AS Content,      
        (       
           --buy button text                                                                               
           CASE       
                -- Case for Active Products                                   
                WHEN p.isActive = 1 THEN       
                    CASE       
                         WHEN mb.id IS NULL THEN 'Buy'       
                         ELSE CASE       
                                   WHEN DATEDIFF(day, GETDATE(), mb.enddate) < @priorDaysInfo THEN 'Renew'       
                                   ELSE 'Purchased' END END       
       
                -- Case for Inactive Products                                   
                WHEN p.isActive = 0 THEN CASE       
                                              WHEN mb.id IS NOT NULL THEN 'Purchased'       
                                              WHEN mb.id IS NULL THEN '' END END) AS BuyButtonText,      
        @ContentCount AS ContentCount,      
        @VideoContent AS VideoCount,      
        DATEDIFF(day, getdate(), ISNULL(mb.enddate, GETDATE())) AS DaysToGo,      
        (CASE       
                 WHEN mb.id IS NOT NULL      
            AND DATEDIFF(day, GETDATE(), mb.enddate) >= @priorDaysInfo THEN '[]'       
                 ELSE (   SELECT *      
        FROM (                   SELECT pb.BonusProductId AS Id,      
                    p2.NAME AS BonusProductName,      
                    pb.DurationInDays AS Validity,      
                    (   SELECT s.Value      
                    FROM Settings s      
                    WHERE s.Code     = 'BonusMessage'      
                        AND s.IsActive = 1) AS BonusMessage      
                FROM ProductBonusMappingM pb      
                    INNER JOIN ProductsM p2      
                    ON pb.BonusProductId = p2.Id      
                    LEFT JOIN ProductCategoriesM pcm2      
                    ON p2.CategoryID     = pcm2.Id      
                WHERE pb.ProductId = p.id      
                    AND pb.IsActive  = 1      
                    and pb.IsDeleted = 0      
                    AND p2.IsActive  = 1      
                    AND p2.IsDeleted = 0      
            UNION ALL      
                SELECT pcm.CommunityId,      
                    max(pTemp.Name) as BonusProductName,      
                    min(pcm.DurationInDays) as Validity,      
                    (   SELECT s.Value      
                    FROM Settings s      
                    WHERE s.Code     = 'BonusMessage'      
     AND s.IsActive = 1) AS BonusMessage      
                From ProductCommunityMappingM as pcm      
                    inner join SubscriptionMappingM as sm      
                    on pcm.CommunityId           = sm.ProductId      
                    inner join SubscriptionDurationM as sd      
                    on sm.SubscriptionDurationId = sd.Id      
                    inner join ProductsM as pTemp      
                    on pTemp.Id                  = pcm.CommunityId      
                where pcm.ProductId              = p.Id      
                    and ISNULL(pTemp.IsActive, 1)  = 1      
                    and ISNULL(pTemp.IsDeleted, 0) = 0      
                    and ISNULL(pcm.IsActive, 1)    = 1      
   and ISNULL(pcm.IsDeleted, 0)   = 0      
                    and ISNULL(sm.IsActive, 1)     = 1      
                    and ISNULL(sd.IsActive, 1)     = 1      
                group by pcm.CommunityId) AS UnionedResults      
        FOR JSON PATH) END) AS BonusProducts      
    FROM ProductsM AS P      
        INNER JOIN ProductCategoriesM AS pcm  ON p.CategoryID     = pcm.Id      
        LEFT JOIN PurchaseOrdersM AS POM ON POM.ProductId    = p.Id      
            AND pom.ProductId    = @ProductId      
            AND pom.ActionBy     = @MobileUserKey      
        LEFT JOIN ProductsRatingM AS PR      
        ON PR.ProductId     = P.Id      
            AND PR.CreatedBy     = @MobileUserKey      
        LEFT JOIN ProductLikesM AS pl      
        ON pl.ProductId     = p.Id      
            AND pl.LikeId        = 1      
            AND pl.CreatedBy     = @MobileUserKey      
            AND pl.IsDelete      = 0      
        LEFT JOIN subscriptiondurationm s      
        ON s.Id             = p.SubscriptionId      
        LEFT JOIN MYBucketM AS Mb      
        ON p.id             = mb.ProductId      
            AND mb.mobileuserkey = @MobileUserkey      
        left join ProductCommunityMappingM pcmm      
        on p.Id             = pcmm.ProductId      
            and pcmm.IsActive    = 1      
            and pcmm.IsDeleted   = 0      
        LEFT JOIN ProductsM pc      
        ON pcmm.CommunityId = pc.Id      
            and pc.IsActive      = 1      
            and pc.IsDeleted     = 0      
    WHERE p.id = @ProductId      
    ORDER BY POM.CreatedOn DESC      
END 
-- Modify By Siva 25 April 2025
    GO
ALTER PROCEDURE [dbo].[GetPhonePePaymentReportChart]      
    @FromDate VARCHAR(50) = NULL,        
    @ToDate VARCHAR(50) = NULL         
AS      
BEGIN      
    SET NOCOUNT ON;      
      
    ---- If no dates are provided, set default to last 1 year (FromDate) and today (ToDate)      
    --IF @FromDate IS NULL       
    --    SET @FromDate = CONVERT(VARCHAR(10), DATEADD(YEAR, -1, GETDATE()), 120); -- Default to 1 year ago      
    --IF @ToDate IS NULL       
    --    SET @ToDate = CONVERT(VARCHAR(10), GETDATE(), 120); -- Default to today      
      
 select        
        
   --sm.SubscriptionDurationId , sd.Name, *        
    MAX(P.Name) AS ProductName,      
    SUM(isnull(PPR.Amount,0)) AS TotalPaidAmount,      
   COUNT(PRS.Status) AS TotalUserCount,      
           
   COUNT(CASE WHEN SD.Name = 'Quaterly' THEN 1 END) AS ThreeMonthsCount,  -- Count of 3 Months      
  COUNT(CASE WHEN SD.Name = 'Half-Yearly' THEN 1 END) AS SixMonthsCount,    -- Count of 6 Months      
  COUNT(CASE WHEN SD.Name = 'Yearly' THEN 1 END) AS TwelveMonthsCount ,-- Count of 12 Months      
  COUNT(CASE WHEN SD.Name = 'Free' THEN 1 END) AS FreeMonthsCount, -- Count of 12 Months      
  COUNT(CASE WHEN SD.Name = 'Month' THEN 1 END) AS OneMonthsCount -- Count of 12 Months      
      
      
  FROM PaymentRequestStatusM PRS          
        INNER JOIN SubscriptionMappingM as sm on prs.SubscriptionMappingId = sm.Id          
        INNER JOIN SubscriptionDurationM as sd on sm.SubscriptionDurationId = sd.Id          
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId          
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey          
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id     
    WHERE PRS.Status IN('PAYMENT_SUCCESS', 'SUCCESS') 
  and (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))        
 GROUP BY PRS.ProductId      
END; 


----- Modify By Siva 25 April 2025

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
    @FromDate VARCHAR(50) = NULL,          
    @ToDate VARCHAR(50) = NULL,          
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
        @TotalPaidAmount = SUM(ISNULL(PPR.Amount, 0))          
    FROM PaymentRequestStatusM PRS          
        INNER JOIN SubscriptionMappingM as sm on prs.SubscriptionMappingId = sm.Id          
        INNER JOIN SubscriptionDurationM as sd on sm.SubscriptionDurationId = sd.Id          
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId          
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey          
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id          
    WHERE                   
            (@SearchText IS NULL OR MU.FullName LIKE '%' + @SearchText + '%' OR CAST(MU.Mobile AS VARCHAR) = @SearchText)          
        AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))          
      AND ( @PrimaryKey IS NULL OR (@PrimaryKey IN ('PAYMENT_SUCCESS', 'SUCCESS') AND PRS.Status IN ('PAYMENT_SUCCESS', 'SUCCESS')) OR PRS.Status = @PrimaryKey)          
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
                PPR.TransactionId,          
                PRS.CreatedOn,          
                REPLACE(PRS.Status, 'PAYMENT_SUCCESS', 'SUCCESS') AS Status,          
                ISNULL(PPR.Amount, 0) AS PaidAmount,          
                sd.Name AS Duration, -- Static placeholder              
    ISNULL(NULLIF(LTRIM(RTRIM(prs.CouponCode)), ''), 'N/A') AS CouponCode,  
                PPR.PaymentInstrumentType,          
                PPR.MerchantTransactionId,          
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
                LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId          
     LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey          
                LEFT JOIN ProductsM P ON PRS.ProductId = P.Id          
            WHERE                   
            (@SearchText IS NULL OR MU.FullName LIKE '%' + @SearchText + '%' OR CAST(MU.Mobile AS VARCHAR) = @SearchText)          
                AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))          
      AND ( @PrimaryKey IS NULL OR (@PrimaryKey IN ('PAYMENT_SUCCESS', 'SUCCESS') AND PRS.Status IN ('PAYMENT_SUCCESS', 'SUCCESS')) OR PRS.Status = @PrimaryKey)          
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
           
           
           
           
----ALTER PROCEDURE [dbo].[GetPhonePe]                    
----    @IsPaging INT = 0,                   
----    @PageSize INT = 5,                    
----    @PageNumber INT = 1,                   
----    @SortExpression VARCHAR(50) = 'CreatedOn', -- Default sort column                  
----    @SortOrder VARCHAR(50) = 'DESC', -- Default sort order                 
----    @RequestedBy VARCHAR(50) = NULL,                    
----    @FromDate VARCHAR(50) = NULL,                    
----    @ToDate VARCHAR(50) = NULL,                    
----    @SearchText VARCHAR(250) = NULL,                    
----    @PrimaryKey VARCHAR(50) = NULL, -- Used to filter by Status                 
----    @SecondaryKey INT = NULL, -- Used to filter by Product ID             
----    @TotalCount INT = 0 OUTPUT, -- Total number of records             
----    @TotalPaidAmount DECIMAL(18, 2) = 0 OUTPUT -- Total PaidAmount             
----AS                    
----BEGIN                    
----    -- Initialize TotalCount and TotalPaidAmount variables             
----    SET @TotalCount = 0;                  
----    SET @TotalPaidAmount = 0;             
             
----    -- Step 1: Calculate the total count of records and total PaidAmount             
----    SELECT              
----        @TotalCount = COUNT(1),             
----        @TotalPaidAmount = SUM(ISNULL(PPR.Amount, 0))             
----    FROM PaymentRequestStatusM PRS                  
----    LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId                   
----    LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey             
----    LEFT JOIN ProductsM P ON PRS.ProductId = P.Id             
----    WHERE                   
----        (@SearchText IS NULL                 
----         OR MU.FullName LIKE '%' + @SearchText + '%'                 
----         OR CAST(MU.Mobile AS VARCHAR) = @SearchText)                  
----        AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))                 
----        AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey)             
----        AND (@SecondaryKey IS NULL OR PRS.ProductId = @SecondaryKey); -- Filter by Product ID             
                  
----    -- Step 2: Query to fetch the data with dynamic sorting and pagination                  
----    IF @IsPaging = 1                 
----    BEGIN                
----        SELECT                     
----            MU.FullName,                  
----            P.Name AS ProductName,               
----            MU.Mobile,                  
----            PRS.Amount AS RequestAmount,                 
----            PPR.TransactionId,                
----            PRS.CreatedOn,                  
----            REPLACE(PRS.Status, 'PAYMENT_SUCCESS', 'SUCCESS') AS Status,                  
----            ISNULL(PPR.Amount, 0) AS PaidAmount,                  
----            PPR.PaymentInstrumentType,                  
----            PPR.MerchantTransactionId,      
----            MU.PublicKey,                
----            ROW_NUMBER() OVER (                  
----                ORDER BY                   
----                    CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN PRS.CreatedOn END ASC,                  
----                    CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN PRS.CreatedOn END DESC,                  
----                    CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'ASC' THEN MU.FullName END ASC,                  
----                    CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'DESC' THEN MU.FullName END DESC                  
----            ) AS RowNum                  
----        INTO #TempPhonePe                  
----        FROM PaymentRequestStatusM PRS                  
----        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId                   
----        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey                
----        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id                 
----        WHERE                 
----            (@SearchText IS NULL                 
----             OR MU.FullName LIKE '%' + @SearchText + '%'                 
----             OR CAST(MU.Mobile AS VARCHAR) = @SearchText)                  
----            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))                 
----            AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey)             
----            AND (@SecondaryKey IS NULL OR PRS.ProductId = @SecondaryKey); -- Filter by Product ID             
                  
----        -- Step 3: Return paginated and sorted results                  
----        SELECT                     
----            FullName,                  
----            ProductName,               
----            Mobile,                  
----            RequestAmount,               
----            TransactionId,                
----            CreatedOn,                  
----            Status,                  
----            PaidAmount,              
----   'vj' Duration,           
----   'vj' CouponCode,               
----            PaymentInstrumentType,                  
----            MerchantTransactionId,              
----            PublicKey                
----        FROM #TempPhonePe                  
----        WHERE RowNum BETWEEN (@PageNumber - 1) * @PageSize + 1 AND @PageNumber * @PageSize                  
----        ORDER BY              
----            CASE WHEN @SortExpression = 'CreatedOn' THEN CreatedOn END DESC; -- Default ordering                  
                  
----        -- Clean up the temporary table                  
----        DROP TABLE IF EXISTS #TempPhonePe;                  
----    END                
----    ELSE                
----    BEGIN                
----        -- Return all data without pagination                
----        SELECT                     
----            MU.FullName,                
----            P.Name AS ProductName,               
----            MU.Mobile,                  
----            PRS.Amount AS RequestAmount,                 
----            PPR.TransactionId,                
----            PRS.CreatedOn,                  
----            REPLACE(PRS.Status, 'PAYMENT_SUCCESS', 'SUCCESS') AS Status,                  
----            ISNULL(PPR.Amount, 0) AS PaidAmount,               
----   '' Duration,           
----   '' CouponCode,           
----            PPR.PaymentInstrumentType,                  
----            PPR.MerchantTransactionId,              
----            MU.PublicKey               
----        FROM PaymentRequestStatusM PRS                  
----        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId                   
----        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey               
----        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id                 
----        WHERE                   
----            (@SearchText IS NULL               
----             OR MU.FullName LIKE '%' + @SearchText + '%'                 
----             OR CAST(MU.Mobile AS VARCHAR) = @SearchText)                  
----            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))                 
----            AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey)             
----            AND (@SecondaryKey IS NULL OR PRS.ProductId = @SecondaryKey) -- Filter by Product ID             
----        ORDER BY CreatedOn DESC;               
----    END 


----- Modify By Siva 25 April 2025
GO

ALTER PROCEDURE [dbo].[GetPhonePePaymentReportBarChart]      
    @FromDate VARCHAR(50) = NULL,        
    @ToDate VARCHAR(50) = NULL,         
    @ProductId INT = NULL, -- NEW: filter by ProductId    
    @DurationType NVARCHAR(20) = NULL -- NEW: 'Monthly', 'Quarterly', 'HalfYearly', 'Yearly'    
AS      
BEGIN      
    SET NOCOUNT ON;      
      
    -- Default dates if not provided    
    IF @FromDate IS NULL       
        SET @FromDate = CONVERT(VARCHAR(10), DATEADD(YEAR, -1, GETDATE()), 120);      
    IF @ToDate IS NULL       
        SET @ToDate = CONVERT(VARCHAR(10), GETDATE(), 120);      
 IF @DurationType IS NULL    
        SET @DurationType = 'Monthly';    
      
    IF @DurationType = 'Monthly'    
    BEGIN    
        SELECT        
            FORMAT(PPR.CreatedOn, 'MMM-yyyy') AS Duration,    
            P.Name AS ProductName,    
            SUM(ISNULL(PPR.Amount, 0)) AS TotalPaidAmount,    
            COUNT(PRS.Status) AS TotalUserCount    
        FROM PaymentRequestStatusM PRS          
        INNER JOIN SubscriptionMappingM as sm on prs.SubscriptionMappingId = sm.Id          
        INNER JOIN SubscriptionDurationM as sd on sm.SubscriptionDurationId = sd.Id          
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId          
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey          
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id     
    WHERE PRS.Status IN('PAYMENT_SUCCESS', 'SUCCESS')
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PPR.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))    
            AND (@ProductId IS NULL OR PRS.ProductId = @ProductId)    
        GROUP BY FORMAT(PPR.CreatedOn, 'MMM-yyyy'), P.Name    
        ORDER BY MIN(YEAR(PPR.CreatedOn)), MIN(MONTH(PPR.CreatedOn)), P.Name    
    END    
    ELSE IF @DurationType = 'Quarterly'    
    BEGIN    
        SELECT        
            CONCAT('Q', DATEPART(QUARTER, PPR.CreatedOn), '-', YEAR(PPR.CreatedOn)) AS Duration,    
            P.Name AS ProductName,    
            SUM(ISNULL(PPR.Amount, 0)) AS TotalPaidAmount,    
            COUNT(PRS.Status) AS TotalUserCount    
        FROM PaymentRequestStatusM PRS          
        INNER JOIN SubscriptionMappingM as sm on prs.SubscriptionMappingId = sm.Id          
        INNER JOIN SubscriptionDurationM as sd on sm.SubscriptionDurationId = sd.Id          
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId          
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey          
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id     
    WHERE PRS.Status IN('PAYMENT_SUCCESS', 'SUCCESS')     
             
            AND(@FromDate IS NULL OR @ToDate IS NULL OR CAST(PPR.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))    
            AND (@ProductId IS NULL OR PRS.ProductId = @ProductId)    
        GROUP BY DATEPART(QUARTER, PPR.CreatedOn), YEAR(PPR.CreatedOn), P.Name    
        ORDER BY MIN(YEAR(PPR.CreatedOn)), MIN(DATEPART(QUARTER, PPR.CreatedOn)), P.Name    
    END    
    ELSE IF @DurationType = 'HalfYearly'    
    BEGIN    
        SELECT        
            CASE     
                WHEN DATEPART(MONTH, PPR.CreatedOn) BETWEEN 1 AND 6 THEN CONCAT('H1-', YEAR(PPR.CreatedOn))    
                ELSE CONCAT('H2-', YEAR(PPR.CreatedOn))    
            END AS Duration,    
            P.Name AS ProductName,    
            SUM(ISNULL(PPR.Amount, 0)) AS TotalPaidAmount,    
            COUNT(PRS.Status) AS TotalUserCount    
        FROM PaymentRequestStatusM PRS          
        INNER JOIN SubscriptionMappingM as sm on prs.SubscriptionMappingId = sm.Id          
        INNER JOIN SubscriptionDurationM as sd on sm.SubscriptionDurationId = sd.Id          
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId          
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey          
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id     
    WHERE PRS.Status IN('PAYMENT_SUCCESS', 'SUCCESS')
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PPR.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))   
            AND (@ProductId IS NULL OR PRS.ProductId = @ProductId)    
        GROUP BY     
            CASE     
                WHEN DATEPART(MONTH, PPR.CreatedOn) BETWEEN 1 AND 6 THEN CONCAT('H1-', YEAR(PPR.CreatedOn))    
                ELSE CONCAT('H2-', YEAR(PPR.CreatedOn))    
            END, p.Name    
        ORDER BY MIN(YEAR(PPR.CreatedOn)), MIN(CASE WHEN DATEPART(MONTH, PPR.CreatedOn) BETWEEN 1 AND 6 THEN 1 ELSE 2 END), P.Name    
    END    
    ELSE IF @DurationType = 'Yearly'    
    BEGIN    
        SELECT        
            CAST(YEAR(PPR.CreatedOn) AS VARCHAR(4)) AS Duration,    
            P.Name AS ProductName,    
            SUM(ISNULL(PPR.Amount, 0)) AS TotalPaidAmount,    
            COUNT(PRS.Status) AS TotalUserCount    
        FROM PaymentRequestStatusM PRS          
        INNER JOIN SubscriptionMappingM as sm on prs.SubscriptionMappingId = sm.Id          
        INNER JOIN SubscriptionDurationM as sd on sm.SubscriptionDurationId = sd.Id          
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId          
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey          
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id     
    WHERE PRS.Status IN('PAYMENT_SUCCESS', 'SUCCESS')     
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PPR.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))  
            AND (@ProductId IS NULL OR PRS.ProductId = @ProductId)    
        GROUP BY YEAR(PPR.CreatedOn), P.Name    
        ORDER BY MIN(YEAR(PPR.CreatedOn)), P.Name    
    END    
END; 

---------------------Ajith 28-04-2025 10:40 AM -------------------------------------
Go
ALTER PROCEDURE GetSubscriptionPlanWithProduct                   
    @ProductId INT = 1,                   
    @SubscriptionPlanId int = 25,                   
    @MobileUserKey uniqueidentifier = null,                   
    @DeviceType VARCHAR(15) = 'android'                   
AS                   
BEGIN                   
                   
    SELECT sp.Id AS SubscriptionPlanId,                   
           sp.Name AS PlanName,                   
           sp.Description AS PlanDescription,                   
           sm.ProductId,                   
           pm.Name as ProductName,                   
           (pm.Price * SD.Months) as ActualPrice,                   
          ROUND((((pm.Price * SD.Months) * sm.DiscountPercentage) / 100),0) as DiscountPrice,                  
          ROUND((pm.Price * SD.Months) - (((pm.Price * SD.Months) * sm.DiscountPercentage) / 100), 0) as NetPayment,                    
           '' as CouponCode,                   
           DATEADD(MONTH, sd.Months, GETDATE()) as ExpireOn,                   
           -- sm.IsActive AS SubscriptionMappingActive,                                    
           sd.Id AS SubscriptionDurationId,                   
           CAST(sd.Months as varchar) + iif(CAST(sd.Months as int) = 1, ' Month', ' Months') AS SubscriptionDurationName,                   
           sd.Months,                   
           CAST(0 as bit) IsRecommended,                   
          -- CAST(FORMAT(((pm.Price - ((pm.Price * sm.DiscountPercentage) / 100)) / sd.Months), 'N2') AS VARCHAR) + '/m' as PerMonth,                  
  CAST(CEILING(
    (ROUND((pm.Price * SD.Months) - (((pm.Price * SD.Months) * sm.DiscountPercentage) / 100), 0))/ sd.Months) AS VARCHAR) + '/m' AS PerMonth,                 
                 
           sm.Id AS SubscriptionMappingId                   
    FROM SubscriptionMappingM sm                   
        INNER JOIN SubscriptionDurationM sd                   
            ON sm.SubscriptionDurationId = sd.Id                   
        INNER JOIN SubscriptionPlanM sp                   
            ON sm.SubscriptionPlanId = sp.Id                   
        INNER JOIN ProductsM as pm                   
            on pm.Id = sm.ProductId                   
    WHERE ISNULL(sp.IsActive, 1) = 1                   
          AND ISNULL(sd.IsActive, 1) = 1                   
          AND ISNULL(sm.IsActive, 1) = 1                   
          AND (                   
                  sm.SubscriptionPlanId = @SubscriptionPlanId                   
                  OR @SubscriptionPlanId IS NULL                   
              )                   
          AND (                   
                  sm.ProductId = @ProductId                   
                  OR @ProductId IS NULL                   
              )                   
          AND LOWER(@DeviceType) IN (SELECT VALUE FROM string_split('android,ios', ','))                   
                   
         ORDER BY SD.Months                    
END

-----------------------Ajith 29-04-2025 3:40 PM ------------------------------
Go
CREATE PROCEDURE [dbo].[GetTradeJournalsByMobileUserKey]
    @MobileUserKey UNIQUEIDENTIFIER,
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Total Count
    SELECT @TotalCount = COUNT(1)
    FROM TradingJournal
    WHERE MobileUserKey = @MobileUserKey AND IsDeleted = 0;

    -- Paged Result
    SELECT 
        Id,
        CONVERT(VARCHAR(10), StartDate, 120) AS StartDate,
        MobileUserKey,
        Symbol,
        BuySellButton,
        CapitalAmount,
        RiskPercentage,
        RiskAmount,
        EntryPrice,
        StopLoss,
        Target1,
        Target2,
        PositionSize,
        ActualExitPrice,
        ProfitLoss,
        RiskReward,
        Notes,
        CreatedBy,
        ModifiedBy,
        IsActive,
        IsDeleted,
        CreatedOn,
        ModifiedOn
    FROM TradingJournal
    WHERE MobileUserKey = @MobileUserKey AND IsDeleted = 0
    ORDER BY CreatedOn DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END

---------------Ajith 02-05-2025 3:29 PM ------------
GO
ALTER PROCEDURE [dbo].[Sp_Get_Leads]
    @IsPaging INT = 0,
    @PageSize INT = 5,
    @PageNumber INT = 1,
    @SortExpression VARCHAR(50),
    @SortOrder VARCHAR(50),
    @FromDate VARCHAR(50) = NULL,
    @ToDate VARCHAR(50) = NULL,
    @PrimaryKey VARCHAR(50) = NULL,
    @SecondaryKey VARCHAR(600) = NULL,
    @ThirdKey VARCHAR(50) = NULL,
    @FourthKey VARCHAR(50) = NULL,
    @FifthKey VARCHAR(50) = NULL,
    @CreatedBy VARCHAR(50) = NULL,
    @AssignedTo VARCHAR(50) = NULL,
    @LoggedInUser VARCHAR(50) = NULL,
    @RoleKey VARCHAR(50) = '',
    @SearchText VARCHAR(250) = NULL,
    @TotalCount INT = 0 OUTPUT,
    @LTCCount INT = 0 OUTPUT
AS 
BEGIN
    SET NOCOUNT ON;

    DECLARE @StatusId INT;
    SELECT @StatusId = Id
    FROM STATUS
    WHERE Code = 'fresh';

    IF EXISTS ( 
        SELECT 1
    FROM Users AS us
        INNER JOIN Roles AS ro ON us.RoleKey = ro.PublicKey
    WHERE us.PublicKey = @LoggedInUser AND ro.Name NOT IN ('admin', 'DM-Ashok') 
    ) 
    BEGIN
        SET @AssignedTo = @LoggedInUser;
    END

    -- Count leads with approved status (po.Status = 10)
    SELECT @LTCCount = COUNT(1)
    FROM Leads AS Leads
        LEFT JOIN PurchaseOrders AS po ON po.LeadId = Leads.Id AND Leads.PurchaseOrderKey = po.PublicKey
    WHERE ISNULL(Leads.IsDelete, 0) = 0
        AND po.Status = 10
        AND (@AssignedTo IS NULL OR Leads.AssignedTo = @AssignedTo);

    SELECT @TotalCount = COUNT(1)
    FROM Leads AS Leads
        LEFT JOIN PurchaseOrders AS po ON po.LeadId = Leads.Id AND Leads.PurchaseOrderKey = po.PublicKey
        LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = CAST(Leads.LeadTypeKey AS UNIQUEIDENTIFIER)
        LEFT JOIN LeadSources AS LeadSource ON LeadSource.PublicKey = CAST(Leads.LeadSourceKey AS UNIQUEIDENTIFIER) AND ISNULL(LeadSource.IsDelete, 0) = 0
        LEFT JOIN STATUS AS st ON st.Id = Leads.StatusId
        LEFT JOIN Users AS Users ON Users.PublicKey = TRY_CAST(Leads.CreatedBy AS UNIQUEIDENTIFIER)
        LEFT JOIN Services AS LeadService ON Leads.ServiceKey IS NOT NULL AND TRY_CAST(Leads.ServiceKey AS UNIQUEIDENTIFIER) = LeadService.PublicKey
        LEFT JOIN Services AS PoService ON po.ServiceId = PoService.Id AND ISNULL(po.IsExpired, 0) <> 1
        LEFT JOIN STATUS AS st1 ON st1.Id = po.Status
        LEFT JOIN PaymentModes AS pm ON po.ModeOfPayment = pm.Id
    WHERE ISNULL(Leads.IsDelete, 0) = 0
        AND (@AssignedTo IS NULL OR (Leads.AssignedTo = @AssignedTo))
        AND ( 
            (po.Status <> 24 AND ISNULL(po.IsExpired, 0) = 0)
        OR (po.Status = 24 AND ISNULL(po.IsExpired, 0) = 1)
        OR (po.Status IS NULL)
        OR (po.Status = 4) 
        )
        AND ( 
            (leads.ServiceKey IN (SELECT CAST(value AS uniqueidentifier)
        FROM STRING_SPLIT(@SecondaryKey, ',')) AND @SecondaryKey IS NOT NULL)
        OR (@SecondaryKey IS NULL) 
        )
        AND (ISNULL(Leads.LeadSourceKey, '1') = ISNULL(@FourthKey, ISNULL(Leads.LeadSourceKey, '1')))
        AND (ISNULL(Leads.LeadTypeKey, '1') = ISNULL(@ThirdKey, ISNULL(Leads.LeadTypeKey, '1')))
        AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))
        AND ( 
            @SearchText IS NULL
        OR Leads.MobileNumber LIKE '%' + @SearchText + '%'
        OR Leads.AlternateMobileNumber LIKE '%' + @SearchText + '%'
        OR Leads.EmailId LIKE '%' + @SearchText + '%'
        OR Leads.FullName LIKE '%' + @SearchText + '%' 
        )
        AND ( 
            @FifthKey IS NULL
        OR (@FifthKey = 'New' AND ISNULL(st1.Name, 'New') = 'New')
        OR (@FifthKey <> 'New' AND ISNULL(st1.Name, 'New') = @FifthKey) 
        );

    DROP TABLE IF EXISTS #tempLeads;

    SELECT
        ROW_NUMBER() OVER ( 
            ORDER BY 
                Leads.ModifiedOn DESC,  
                CASE  
                    WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN Leads.CreatedOn 
                    WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN NULL 
                END ASC, 
                CASE  
                    WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN Leads.CreatedOn 
                END DESC, 
                CASE  
                    WHEN @SortExpression = 'Favourite' AND @SortOrder = 'ASC' THEN ISNULL(Leads.Favourite, 0) 
                    WHEN @SortExpression = 'Favourite' AND @SortOrder = 'DESC' THEN NULL 
                END ASC, 
                CASE  
                    WHEN @SortExpression = 'Favourite' AND @SortOrder = 'DESC' THEN ISNULL(Leads.Favourite, 0) 
                END DESC, 
                CASE  
                    WHEN @SortExpression = 'Name' AND @SortOrder = 'ASC' THEN Leads.FullName 
                    WHEN @SortExpression = 'Name' AND @SortOrder = 'DESC' THEN NULL 
                END ASC, 
                CASE  
                    WHEN @SortExpression = 'Name' AND @SortOrder = 'DESC' THEN Leads.FullName 
                END DESC 
        ) AS SlNo,
        Leads.Id,
        Leads.FullName,
        Leads.MobileNumber,
        Leads.AlternateMobileNumber,
        Leads.EmailId,
        ISNULL(po.City, ISNULL(Leads.City, '')) AS City,
        po.PaymentDate,
        ISNULL(Leads.Favourite, 0) AS Favourite,
        COALESCE(LeadService.Name, PoService.Name) AS ServiceKey,
        COALESCE(LeadService.Id, PoService.Id) AS ServiceId,
        LeadTypes.Name AS LeadTypeKey,
        LeadTypes.Id AS LeadTypesId,
        LeadSource.Id AS LeadSourcesId,
        LeadSource.Name AS LeadSourceKey,
        ISNULL(Leads.Remarks, '') AS Remark,
        Leads.IsSpam,
        Leads.IsWon,
        Leads.IsDisabled,
        Leads.IsDelete,
        Leads.PublicKey,
        Leads.CreatedOn,
        Users.FirstName AS CreatedBy,
        leads.CreatedBy As LeadCreatedBy,
        Leads.ModifiedOn,
        Leads.ModifiedBy,
        '' AS AssignedTo,
        ISNULL(Leads.StatusId, 1) AS StatusId,
        ISNULL(st.Name, 'New') AS StatusName,
        ISNULL(Leads.PurchaseOrderKey, CAST('00000000-0000-0000-0000-000000000000' AS UNIQUEIDENTIFIER)) AS PurchaseOrderKey,
        ISNULL(st1.Name, 'New') AS PurchaseOrderStatus,
        ISNULL(po.ModeOfPayment, -1) AS ModeOfPayment,
        ISNULL(po.PaidAmount, 0.0) AS PaidAmount,
        ISNULL(po.NetAmount, 0.0) AS NetAmount,
        ISNULL(po.TransactionRecipt, '') AS TransactionRecipt,
        ISNULL(po.TransasctionReference, '') AS TransasctionReference,
        CASE  
            WHEN DATEDIFF(DAY, GETDATE(), DATEADD(DAY, 90, ISNULL(Leads.ModifiedOn, GETDATE()))) > 0 
            THEN DATEDIFF(DAY, GETDATE(), DATEADD(DAY, 90, ISNULL(Leads.ModifiedOn, GETDATE()))) 
            ELSE 0 
        END AS DaysToGo,
        Leads.CountryCode
    INTO #tempLeads
    FROM Leads
        LEFT JOIN PurchaseOrders AS po ON po.LeadId = Leads.Id AND Leads.PurchaseOrderKey = po.PublicKey
        LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = CAST(Leads.LeadTypeKey AS UNIQUEIDENTIFIER)
        LEFT JOIN LeadSources AS LeadSource ON LeadSource.PublicKey = CAST(Leads.LeadSourceKey AS UNIQUEIDENTIFIER) AND ISNULL(LeadSource.IsDelete, 0) = 0
        LEFT JOIN STATUS AS st ON st.Id = Leads.StatusId
        LEFT JOIN Users AS Users ON Users.PublicKey = TRY_CAST(Leads.CreatedBy AS UNIQUEIDENTIFIER)
        LEFT JOIN Services AS LeadService ON Leads.ServiceKey IS NOT NULL AND TRY_CAST(Leads.ServiceKey AS UNIQUEIDENTIFIER) = LeadService.PublicKey
        LEFT JOIN Services AS PoService ON po.ServiceId = PoService.Id AND ISNULL(po.IsExpired, 0) <> 1
        LEFT JOIN STATUS AS st1 ON st1.Id = po.Status
        LEFT JOIN PaymentModes AS pm ON po.ModeOfPayment = pm.Id
    WHERE ISNULL(Leads.IsDelete, 0) = 0
        AND (@AssignedTo IS NULL OR (Leads.AssignedTo = @AssignedTo))
        AND ( 
            (po.Status <> 24 AND ISNULL(po.IsExpired, 0) = 0)
        OR (po.Status = 24 AND ISNULL(po.IsExpired, 0) = 1)
        OR (po.Status IS NULL)
        OR (po.Status = 4) 
        )
        AND ( 
            (leads.ServiceKey IN (SELECT CAST(value AS uniqueidentifier)
        FROM STRING_SPLIT(@SecondaryKey, ',')) AND @SecondaryKey IS NOT NULL)
        OR (@SecondaryKey IS NULL) 
        )
        AND (ISNULL(Leads.LeadSourceKey, '1') = ISNULL(@FourthKey, ISNULL(Leads.LeadSourceKey, '1')))
        AND (ISNULL(Leads.LeadTypeKey, '1') = ISNULL(@ThirdKey, ISNULL(Leads.LeadTypeKey, '1')))
        AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))
        AND ( 
            @SearchText IS NULL
        OR Leads.MobileNumber LIKE '%' + @SearchText + '%'
        OR Leads.AlternateMobileNumber LIKE '%' + @SearchText + '%'
        OR Leads.EmailId LIKE '%' + @SearchText + '%'
        OR Leads.FullName LIKE '%' + @SearchText + '%' 
        )
        AND ( 
            @FifthKey IS NULL
        OR (@FifthKey = 'New' AND ISNULL(st1.Name, 'New') = 'New')
        OR (@FifthKey <> 'New' AND ISNULL(st1.Name, 'New') = @FifthKey) 
        );
 IF @LTCCount > 0
    BEGIN
        SELECT *
        FROM #tempLeads
        WHERE ISNULL(PurchaseOrderStatus, '') = 'Approved'
        ORDER BY SlNo
        OFFSET (IIF(@PageNumber <= 1, 0, (@PageNumber - 1)) * @PageSize) ROWS 
        FETCH NEXT @PageSize ROWS ONLY;
    END
    ELSE
    BEGIN
        SELECT *
        FROM #tempLeads
        WHERE ISNULL(PurchaseOrderStatus, '') <> 'Approved'
        ORDER BY SlNo
        OFFSET (IIF(@PageNumber <= 1, 0, (@PageNumber - 1)) * @PageSize) ROWS 
        FETCH NEXT @PageSize ROWS ONLY;
    END
END

------------------------Ajith 07-05-2025 12:00 PM ----------------------
GO
ALTER PROCEDURE [dbo].[Sp_Get_Leads] 
    @IsPaging INT = 0, 
    @PageSize INT = 5, 
    @PageNumber INT = 1, 
    @SortExpression VARCHAR(50), 
    @SortOrder VARCHAR(50), 
    @FromDate VARCHAR(50) = NULL, 
    @ToDate VARCHAR(50) = NULL, 
    @PrimaryKey VARCHAR(50) = NULL, 
    @SecondaryKey VARCHAR(600) = NULL, 
    @ThirdKey VARCHAR(50) = NULL, 
    @FourthKey VARCHAR(50) = NULL, 
    @FifthKey VARCHAR(50) = NULL, 
    @CreatedBy VARCHAR(50) = NULL, 
    @AssignedTo VARCHAR(50) = NULL, 
    @LoggedInUser VARCHAR(50) = NULL, 
    @RoleKey VARCHAR(50) = '', 
    @SearchText VARCHAR(250) = NULL, 
    @TotalCount INT = 0 OUTPUT, 
    @LTCCount INT = 0 OUTPUT 
AS  
BEGIN 
    SET NOCOUNT ON; 
 SET @AssignedTo = NULLIF(@AssignedTo, ''); 
  -- Check if the user is admin or not 
    DECLARE @IsAdmin BIT = 0; 
    IF @RoleKey = 'admin' 
        SET @IsAdmin = 1; 
 
    DECLARE @StatusId INT; 
    SELECT @StatusId = Id 
    FROM STATUS 
    WHERE Code = 'fresh'; 
 
 -- Check if the logged-in user has created any approved leads 
    IF EXISTS ( 
        SELECT 1 
        FROM Leads AS Leads 
        LEFT JOIN PurchaseOrders AS po  
            ON po.LeadId = Leads.Id AND Leads.PurchaseOrderKey = po.PublicKey 
        WHERE ISNULL(Leads.IsDelete, 0) = 0 
          AND po.Status = 10  -- Approved 
          AND Leads.CreatedBy = @LoggedInUser  -- ONLY if user CREATED the approved lead 
    ) 
    BEGIN 
        SET @LTCCount = 1 -- Trigger popup 
    END 
    ELSE 
    BEGIN 
        SET @LTCCount = 0 -- No popup, show all 
    END 
 
    -- Count leads with approved status (po.Status = 10) 
    SELECT @LTCCount = COUNT(1) 
    FROM Leads AS Leads 
        LEFT JOIN PurchaseOrders AS po ON po.LeadId = Leads.Id AND Leads.PurchaseOrderKey = po.PublicKey 
    WHERE ISNULL(Leads.IsDelete, 0) = 0 
        AND po.Status = 10 
        AND (@AssignedTo IS NULL OR Leads.AssignedTo = @AssignedTo); 
 
    SELECT @TotalCount = COUNT(1) 
     FROM Leads 
        LEFT JOIN PurchaseOrders AS po ON po.LeadId = Leads.Id AND Leads.PurchaseOrderKey = po.PublicKey 
        LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = CAST(Leads.LeadTypeKey AS UNIQUEIDENTIFIER) 
        LEFT JOIN LeadSources AS LeadSource ON LeadSource.PublicKey = CAST(Leads.LeadSourceKey AS UNIQUEIDENTIFIER) AND ISNULL(LeadSource.IsDelete, 0) = 0 
        LEFT JOIN STATUS AS st ON st.Id = Leads.StatusId 
        LEFT JOIN Users AS Users ON Users.PublicKey = TRY_CAST(Leads.CreatedBy AS UNIQUEIDENTIFIER) 
        LEFT JOIN Services AS LeadService ON Leads.ServiceKey IS NOT NULL AND TRY_CAST(Leads.ServiceKey AS UNIQUEIDENTIFIER) = LeadService.PublicKey 
        LEFT JOIN Services AS PoService ON po.ServiceId = PoService.Id AND ISNULL(po.IsExpired, 0) <> 1 
        LEFT JOIN STATUS AS st1 ON st1.Id = po.Status 
        LEFT JOIN PaymentModes AS pm ON po.ModeOfPayment = pm.Id 
    WHERE ISNULL(Leads.IsDelete, 0) = 0 
       AND (@AssignedTo IS NULL OR Leads.AssignedTo = @AssignedTo) 
        AND (  
            (po.Status <> 24 AND ISNULL(po.IsExpired, 0) = 0) 
        OR (po.Status = 24 AND ISNULL(po.IsExpired, 0) = 1) 
        OR (po.Status IS NULL) 
        OR (po.Status = 4)  
        ) 
        AND((  leads.ServiceKey in (SELECT CAST(value AS uniqueidentifier) 
        FROM STRING_SPLIT(@SecondaryKey, ',')  ) 
        and @SecondaryKey is not null ) or( leads.ServiceKey = leads.ServiceKey and @SecondaryKey is null)    ) 
 
        AND (ISNULL(Leads.LeadSourceKey, '1') = ISNULL(@FourthKey, ISNULL(Leads.LeadSourceKey, '1'))) 
        AND (ISNULL(Leads.LeadTypeKey, '1') = ISNULL(@ThirdKey, ISNULL(Leads.LeadTypeKey, '1'))) 
        AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)) 
        AND (  
            @SearchText IS NULL 
        OR Leads.MobileNumber LIKE '%' + @SearchText + '%' 
        OR Leads.AlternateMobileNumber LIKE '%' + @SearchText + '%' 
        OR Leads.EmailId LIKE '%' + @SearchText + '%' 
        OR Leads.FullName LIKE '%' + @SearchText + '%'  
        ) 
        AND (  
            @FifthKey IS NULL 
        OR (@FifthKey = 'New' AND ISNULL(st1.Name, 'New') = 'New') 
        OR (@FifthKey <> 'New' AND ISNULL(st1.Name, 'New') = @FifthKey)  
        ); 
 
    DROP TABLE IF EXISTS #tempLeads; 
 
    SELECT 
        ROW_NUMBER() OVER (  
            ORDER BY  
                Leads.ModifiedOn DESC,   
                CASE   
                    WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN Leads.CreatedOn  
                    WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN NULL  
                END ASC,  
                CASE   
                    WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN Leads.CreatedOn  
                END DESC,  
                CASE   
                    WHEN @SortExpression = 'Favourite' AND @SortOrder = 'ASC' THEN ISNULL(Leads.Favourite, 0)  
                    WHEN @SortExpression = 'Favourite' AND @SortOrder = 'DESC' THEN NULL  
                END ASC,  
                CASE   
                    WHEN @SortExpression = 'Favourite' AND @SortOrder = 'DESC' THEN ISNULL(Leads.Favourite, 0)  
                END DESC,  
                CASE   
                    WHEN @SortExpression = 'Name' AND @SortOrder = 'ASC' THEN Leads.FullName  
                    WHEN @SortExpression = 'Name' AND @SortOrder = 'DESC' THEN NULL  
                END ASC,  
                CASE   
                    WHEN @SortExpression = 'Name' AND @SortOrder = 'DESC' THEN Leads.FullName  
                END DESC  
        ) AS SlNo, 
        Leads.Id, 
        Leads.FullName, 
        Leads.MobileNumber, 
        Leads.AlternateMobileNumber, 
        Leads.EmailId, 
        ISNULL(po.City, ISNULL(Leads.City, '')) AS City, 
        po.PaymentDate, 
        ISNULL(Leads.Favourite, 0) AS Favourite, 
        COALESCE(LeadService.Name, PoService.Name) AS ServiceKey, 
        COALESCE(LeadService.Id, PoService.Id) AS ServiceId, 
        LeadTypes.Name AS LeadTypeKey, 
        LeadTypes.Id AS LeadTypesId, 
        LeadSource.Id AS LeadSourcesId, 
        LeadSource.Name AS LeadSourceKey, 
        ISNULL(Leads.Remarks, '') AS Remark, 
        Leads.IsSpam, 
        Leads.IsWon, 
        Leads.IsDisabled, 
        Leads.IsDelete, 
        Leads.PublicKey, 
        Leads.CreatedOn, 
        Users.FirstName AS CreatedBy, 
        leads.CreatedBy As LeadCreatedBy, 
        Leads.ModifiedOn, 
        Leads.ModifiedBy, 
        Leads.AssignedTo, 
        ISNULL(Leads.StatusId, 1) AS StatusId, 
        ISNULL(st.Name, 'New') AS StatusName, 
        ISNULL(Leads.PurchaseOrderKey, CAST('00000000-0000-0000-0000-000000000000' AS UNIQUEIDENTIFIER)) AS PurchaseOrderKey, 
        ISNULL(st1.Name, 'New') AS PurchaseOrderStatus, 
        ISNULL(po.ModeOfPayment, -1) AS ModeOfPayment, 
        ISNULL(po.PaidAmount, 0.0) AS PaidAmount, 
        ISNULL(po.NetAmount, 0.0) AS NetAmount, 
        ISNULL(po.TransactionRecipt, '') AS TransactionRecipt, 
        ISNULL(po.TransasctionReference, '') AS TransasctionReference, 
        CASE   
            WHEN DATEDIFF(DAY, GETDATE(), DATEADD(DAY, 90, ISNULL(Leads.ModifiedOn, GETDATE()))) > 0  
            THEN DATEDIFF(DAY, GETDATE(), DATEADD(DAY, 90, ISNULL(Leads.ModifiedOn, GETDATE())))  
            ELSE 0  
        END AS DaysToGo, 
        Leads.CountryCode 
    INTO #tempLeads 
    FROM Leads 
        LEFT JOIN PurchaseOrders AS po ON po.LeadId = Leads.Id AND Leads.PurchaseOrderKey = po.PublicKey 
        LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = CAST(Leads.LeadTypeKey AS UNIQUEIDENTIFIER) 
        LEFT JOIN LeadSources AS LeadSource ON LeadSource.PublicKey = CAST(Leads.LeadSourceKey AS UNIQUEIDENTIFIER) AND ISNULL(LeadSource.IsDelete, 0) = 0 
        LEFT JOIN STATUS AS st ON st.Id = Leads.StatusId 
        LEFT JOIN Users AS Users ON Users.PublicKey = TRY_CAST(Leads.CreatedBy AS UNIQUEIDENTIFIER) 
        LEFT JOIN Services AS LeadService ON Leads.ServiceKey IS NOT NULL AND TRY_CAST(Leads.ServiceKey AS UNIQUEIDENTIFIER) = LeadService.PublicKey 
        LEFT JOIN Services AS PoService ON po.ServiceId = PoService.Id AND ISNULL(po.IsExpired, 0) <> 1 
        LEFT JOIN STATUS AS st1 ON st1.Id = po.Status 
        LEFT JOIN PaymentModes AS pm ON po.ModeOfPayment = pm.Id 
    WHERE ISNULL(Leads.IsDelete, 0) = 0 
       AND (@AssignedTo IS NULL OR Leads.AssignedTo = @AssignedTo) 
        AND (  
            (po.Status <> 24 AND ISNULL(po.IsExpired, 0) = 0) 
        OR (po.Status = 24 AND ISNULL(po.IsExpired, 0) = 1) 
        OR (po.Status IS NULL) 
        OR (po.Status = 4)  
        ) 
        AND((  leads.ServiceKey in (SELECT CAST(value AS uniqueidentifier) 
        FROM STRING_SPLIT(@SecondaryKey, ',')  ) 
        and @SecondaryKey is not null ) or( leads.ServiceKey = leads.ServiceKey and @SecondaryKey is null)    ) 
 
        AND (ISNULL(Leads.LeadSourceKey, '1') = ISNULL(@FourthKey, ISNULL(Leads.LeadSourceKey, '1'))) 
        AND (ISNULL(Leads.LeadTypeKey, '1') = ISNULL(@ThirdKey, ISNULL(Leads.LeadTypeKey, '1'))) 
        AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)) 
        AND (  
            @SearchText IS NULL 
        OR Leads.MobileNumber LIKE '%' + @SearchText + '%' 
        OR Leads.AlternateMobileNumber LIKE '%' + @SearchText + '%' 
        OR Leads.EmailId LIKE '%' + @SearchText + '%' 
        OR Leads.FullName LIKE '%' + @SearchText + '%'  
        ) 
        AND (  
            @FifthKey IS NULL 
        OR (@FifthKey = 'New' AND ISNULL(st1.Name, 'New') = 'New') 
        OR (@FifthKey <> 'New' AND ISNULL(st1.Name, 'New') = @FifthKey)  
        ); 
    IF @LTCCount > 0 
    BEGIN 
        SELECT * 
        FROM #tempLeads 
        ORDER BY SlNo 
        OFFSET (IIF(@PageNumber <= 1, 0, (@PageNumber - 1)) * @PageSize) ROWS  
        FETCH NEXT @PageSize ROWS ONLY; 
    END 
    ELSE 
    BEGIN 
         SELECT * FROM #tempLeads 
        ORDER BY SlNo 
        OFFSET (IIF(@PageNumber <= 1, 0, (@PageNumber - 1)) * @PageSize) ROWS  
        FETCH NEXT @PageSize ROWS ONLY; 
    END 
END


ALTER TABLE TradingJournal
ALTER COLUMN RiskPercentage decimal(18,2);

----------Ajith 09-05-2025 12:00 AM ----------------

GO
ALTER PROCEDURE [dbo].[Sp_Get_Leads] 
    @IsPaging INT = 0, 
    @PageSize INT = 5, 
    @PageNumber INT = 1, 
    @SortExpression VARCHAR(50), 
    @SortOrder VARCHAR(50), 
    @FromDate VARCHAR(50) = NULL, 
    @ToDate VARCHAR(50) = NULL, 
    @PrimaryKey VARCHAR(50) = NULL, 
    @SecondaryKey VARCHAR(600) = NULL, 
    @ThirdKey VARCHAR(50) = NULL, 
    @FourthKey VARCHAR(50) = NULL, 
    @FifthKey VARCHAR(50) = NULL, 
    @CreatedBy VARCHAR(50) = NULL, 
    @AssignedTo VARCHAR(50) = NULL, 
    @LoggedInUser VARCHAR(50) = NULL, 
    @RoleKey VARCHAR(50) = '', 
    @SearchText VARCHAR(250) = NULL, 
    @TotalCount INT = 0 OUTPUT, 
    @LTCCount INT = 0 OUTPUT 
AS  
BEGIN 
    SET NOCOUNT ON; 
    SET @AssignedTo = NULLIF(@AssignedTo, ''); 
    
    -- Check if the user is admin or not 
    DECLARE @IsAdmin BIT = 0; 
    IF @RoleKey = 'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b' 
        SET @IsAdmin = 1; 
    
    DECLARE @StatusId INT; 
    SELECT @StatusId = Id 
    FROM STATUS 
    WHERE Code = 'fresh'; 
    
    -- -- 
    -- IF EXISTS ( 
    --     SELECT 1 
    --     FROM Leads AS Leads 
    --     LEFT JOIN PurchaseOrders AS po  
    --         ON po.LeadId = Leads.Id AND Leads.PurchaseOrderKey = po.PublicKey 
    --     WHERE ISNULL(Leads.IsDelete, 0) = 0 
    --       AND po.Status = 10  -- Approved 
    --       AND Leads.CreatedBy = @LoggedInUser  
    -- ) 
    -- BEGIN 
    --     SET @LTCCount = 1 -- Trigger popup 
    -- END 
    -- ELSE 
    -- BEGIN 
    --     SET @LTCCount = 0 -- No popup, show all 
    -- END 
    
   -- Check if the logged-in user got any Pr approved by finance admin and ready for LTC 
SELECT @LTCCount = COUNT(1)
FROM Leads AS Leads
LEFT JOIN PurchaseOrders AS po  
    ON po.LeadId = Leads.Id AND Leads.PurchaseOrderKey = po.PublicKey 
WHERE ISNULL(Leads.IsDelete, 0) = 0 
  AND po.Status = 10
  AND Leads.AssignedTo = @LoggedInUser;

-- If LTC exists, return those approved lead rows immediately
IF @LTCCount > 0
BEGIN
;WITH LeadData AS (
        SELECT
            ROW_NUMBER() OVER (  
                ORDER BY  
                    Leads.ModifiedOn DESC,   
                    CASE   
                        WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN Leads.CreatedOn  
                        WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN NULL  
                    END ASC,  
                    CASE   
                        WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN Leads.CreatedOn  
                    END DESC,  
                    CASE   
                        WHEN @SortExpression = 'Favourite' AND @SortOrder = 'ASC' THEN ISNULL(Leads.Favourite, 0)  
                        WHEN @SortExpression = 'Favourite' AND @SortOrder = 'DESC' THEN NULL  
                    END ASC,  
                    CASE   
                        WHEN @SortExpression = 'Favourite' AND @SortOrder = 'DESC' THEN ISNULL(Leads.Favourite, 0)  
                    END DESC  
            ) AS SlNo, 
            Leads.Id, 
            Leads.FullName, 
            Leads.MobileNumber, 
            Leads.AlternateMobileNumber, 
            Leads.EmailId, 
            ISNULL(po.City, ISNULL(Leads.City, '')) AS City, 
            po.PaymentDate, 
            ISNULL(Leads.Favourite, 0) AS Favourite, 
            COALESCE(LeadService.Name, PoService.Name) AS ServiceKey, 
            COALESCE(LeadService.Id, PoService.Id) AS ServiceId, 
            LeadTypes.Name AS LeadTypeKey, 
            LeadTypes.Id AS LeadTypesId, 
            LeadSource.Id AS LeadSourcesId, 
            LeadSource.Name AS LeadSourceKey, 
            ISNULL(Leads.Remarks, '') AS Remark, 
            Leads.IsSpam, 
            Leads.IsWon, 
            Leads.IsDisabled, 
            Leads.IsDelete, 
            Leads.PublicKey, 
            Leads.CreatedOn, 
            Users.FirstName AS CreatedBy, 
            Leads.CreatedBy As LeadCreatedBy, 
            Leads.ModifiedOn, 
            Leads.ModifiedBy, 
            Leads.AssignedTo, 
            ISNULL(Leads.StatusId, 1) AS StatusId, 
            ISNULL(st.Name, 'New') AS StatusName, 
            ISNULL(Leads.PurchaseOrderKey, CAST('00000000-0000-0000-0000-000000000000' AS UNIQUEIDENTIFIER)) AS PurchaseOrderKey, 
            ISNULL(st1.Name, 'New') AS PurchaseOrderStatus, 
            ISNULL(po.ModeOfPayment, -1) AS ModeOfPayment, 
            ISNULL(po.PaidAmount, 0.0) AS PaidAmount, 
            ISNULL(po.NetAmount, 0.0) AS NetAmount, 
            ISNULL(po.TransactionRecipt, '') AS TransactionRecipt, 
            ISNULL(po.TransasctionReference, '') AS TransasctionReference, 
            CASE   
                WHEN DATEDIFF(DAY, GETDATE(), DATEADD(DAY, 90, ISNULL(Leads.ModifiedOn, GETDATE()))) > 0  
                THEN DATEDIFF(DAY, GETDATE(), DATEADD(DAY, 90, ISNULL(Leads.ModifiedOn, GETDATE())))  
                ELSE 0  
            END AS DaysToGo,
            Leads.CountryCode 
        FROM Leads AS Leads
        LEFT JOIN PurchaseOrders AS po  
            ON po.LeadId = Leads.Id AND Leads.PurchaseOrderKey = po.PublicKey 
        LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = CAST(Leads.LeadTypeKey AS UNIQUEIDENTIFIER) 
        LEFT JOIN LeadSources AS LeadSource ON LeadSource.PublicKey = CAST(Leads.LeadSourceKey AS UNIQUEIDENTIFIER) AND ISNULL(LeadSource.IsDelete, 0) = 0 
        LEFT JOIN STATUS AS st ON st.Id = Leads.StatusId 
        LEFT JOIN Users AS Users ON Users.PublicKey = TRY_CAST(Leads.CreatedBy AS UNIQUEIDENTIFIER) 
        LEFT JOIN Services AS LeadService ON Leads.ServiceKey IS NOT NULL AND TRY_CAST(Leads.ServiceKey AS UNIQUEIDENTIFIER) = LeadService.PublicKey 
        LEFT JOIN Services AS PoService ON po.ServiceId = PoService.Id AND ISNULL(po.IsExpired, 0) <> 1 
        LEFT JOIN STATUS AS st1 ON st1.Id = po.Status 
        LEFT JOIN PaymentModes AS pm ON po.ModeOfPayment = pm.Id 
        WHERE ISNULL(Leads.IsDelete, 0) = 0 
          AND po.Status = 10
          AND (@IsAdmin = 1 OR Leads.AssignedTo = @LoggedInUser)
    )
    -- Now apply pagination on the result
    SELECT * FROM LeadData
    WHERE SlNo BETWEEN (@PageNumber - 1) * @PageSize + 1 AND @PageNumber * @PageSize;

    RETURN;
END

    
    -- Total count of leads
    SELECT @TotalCount = COUNT(1)
    FROM Leads 
        LEFT JOIN PurchaseOrders AS po ON po.LeadId = Leads.Id AND Leads.PurchaseOrderKey = po.PublicKey 
        LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = CAST(Leads.LeadTypeKey AS UNIQUEIDENTIFIER) 
        LEFT JOIN LeadSources AS LeadSource ON LeadSource.PublicKey = CAST(Leads.LeadSourceKey AS UNIQUEIDENTIFIER) AND ISNULL(LeadSource.IsDelete, 0) = 0 
        LEFT JOIN STATUS AS st ON st.Id = Leads.StatusId 
        LEFT JOIN Users AS Users ON Users.PublicKey = TRY_CAST(Leads.CreatedBy AS UNIQUEIDENTIFIER) 
        LEFT JOIN Services AS LeadService ON Leads.ServiceKey IS NOT NULL AND TRY_CAST(Leads.ServiceKey AS UNIQUEIDENTIFIER) = LeadService.PublicKey 
        LEFT JOIN Services AS PoService ON po.ServiceId = PoService.Id AND ISNULL(po.IsExpired, 0) <> 1 
        LEFT JOIN STATUS AS st1 ON st1.Id = po.Status 
        LEFT JOIN PaymentModes AS pm ON po.ModeOfPayment = pm.Id 
    WHERE ISNULL(Leads.IsDelete, 0) = 0 
        AND (@AssignedTo IS NULL OR Leads.AssignedTo = @AssignedTo) 
        AND (  
            (po.Status <> 24 AND ISNULL(po.IsExpired, 0) = 0) 
        OR (po.Status = 24 AND ISNULL(po.IsExpired, 0) = 1) 
        OR (po.Status IS NULL) 
        OR (po.Status = 4)  
        ) 
        AND((leads.ServiceKey in (SELECT CAST(value AS uniqueidentifier) 
        FROM STRING_SPLIT(@SecondaryKey, ',') ) 
        and @SecondaryKey is not null ) or( leads.ServiceKey = leads.ServiceKey and @SecondaryKey is null)) 
        AND (ISNULL(Leads.LeadSourceKey, '1') = ISNULL(@FourthKey, ISNULL(Leads.LeadSourceKey, '1'))) 
        AND (ISNULL(Leads.LeadTypeKey, '1') = ISNULL(@ThirdKey, ISNULL(Leads.LeadTypeKey, '1'))) 
        AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)) 
        AND (  
            @SearchText IS NULL 
        OR Leads.MobileNumber LIKE '%' + @SearchText + '%' 
        OR Leads.AlternateMobileNumber LIKE '%' + @SearchText + '%' 
        OR Leads.EmailId LIKE '%' + @SearchText + '%' 
        OR Leads.FullName LIKE '%' + @SearchText + '%'  
        ) 
        AND (
    @FifthKey IS NULL
    OR ISNULL(LTRIM(RTRIM(st1.Name)), 'New') = LTRIM(RTRIM(@FifthKey))
); 
    
    -- Create temp table for paging 
    DROP TABLE IF EXISTS #tempLeads; 
    
    SELECT 
        ROW_NUMBER() OVER (  
            ORDER BY  
                Leads.ModifiedOn DESC,   
                CASE   
                    WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN Leads.CreatedOn  
                    WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN NULL  
                END ASC,  
                CASE   
                    WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN Leads.CreatedOn  
                END DESC,  
                CASE   
                    WHEN @SortExpression = 'Favourite' AND @SortOrder = 'ASC' THEN ISNULL(Leads.Favourite, 0)  
                    WHEN @SortExpression = 'Favourite' AND @SortOrder = 'DESC' THEN NULL  
                END ASC,  
                CASE   
                    WHEN @SortExpression = 'Favourite' AND @SortOrder = 'DESC' THEN ISNULL(Leads.Favourite, 0)  
                END DESC  
        ) AS SlNo, 
        Leads.Id, 
        Leads.FullName, 
        Leads.MobileNumber, 
        Leads.AlternateMobileNumber, 
        Leads.EmailId, 
        ISNULL(po.City, ISNULL(Leads.City, '')) AS City, 
        po.PaymentDate, 
        ISNULL(Leads.Favourite, 0) AS Favourite, 
        COALESCE(LeadService.Name, PoService.Name) AS ServiceKey, 
        COALESCE(LeadService.Id, PoService.Id) AS ServiceId, 
        LeadTypes.Name AS LeadTypeKey, 
        LeadTypes.Id AS LeadTypesId, 
        LeadSource.Id AS LeadSourcesId, 
        LeadSource.Name AS LeadSourceKey, 
        ISNULL(Leads.Remarks, '') AS Remark, 
        Leads.IsSpam, 
        Leads.IsWon, 
        Leads.IsDisabled, 
        Leads.IsDelete, 
        Leads.PublicKey, 
        Leads.CreatedOn, 
        Users.FirstName AS CreatedBy, 
        Leads.CreatedBy As LeadCreatedBy, 
        Leads.ModifiedOn, 
        Leads.ModifiedBy, 
        Leads.AssignedTo, 
        ISNULL(Leads.StatusId, 1) AS StatusId, 
        ISNULL(st.Name, 'New') AS StatusName, 
        ISNULL(Leads.PurchaseOrderKey, CAST('00000000-0000-0000-0000-000000000000' AS UNIQUEIDENTIFIER)) AS PurchaseOrderKey, 
        ISNULL(st1.Name, 'New') AS PurchaseOrderStatus, 
        ISNULL(po.ModeOfPayment, -1) AS ModeOfPayment, 
        ISNULL(po.PaidAmount, 0.0) AS PaidAmount, 
        ISNULL(po.NetAmount, 0.0) AS NetAmount, 
        ISNULL(po.TransactionRecipt, '') AS TransactionRecipt, 
        ISNULL(po.TransasctionReference, '') AS TransasctionReference, 
        CASE   
            WHEN DATEDIFF(DAY, GETDATE(), DATEADD(DAY, 90, ISNULL(Leads.ModifiedOn, GETDATE()))) > 0  
            THEN DATEDIFF(DAY, GETDATE(), DATEADD(DAY, 90, ISNULL(Leads.ModifiedOn, GETDATE())))  
            ELSE 0  
        END AS DaysToGo ,
        Leads.CountryCode 
    INTO #tempLeads
    FROM Leads 
        LEFT JOIN PurchaseOrders AS po ON po.LeadId = Leads.Id AND Leads.PurchaseOrderKey = po.PublicKey 
        LEFT JOIN LeadTypes AS LeadTypes ON LeadTypes.PublicKey = CAST(Leads.LeadTypeKey AS UNIQUEIDENTIFIER) 
        LEFT JOIN LeadSources AS LeadSource ON LeadSource.PublicKey = CAST(Leads.LeadSourceKey AS UNIQUEIDENTIFIER) AND ISNULL(LeadSource.IsDelete, 0) = 0 
        LEFT JOIN STATUS AS st ON st.Id = Leads.StatusId 
        LEFT JOIN Users AS Users ON Users.PublicKey = TRY_CAST(Leads.CreatedBy AS UNIQUEIDENTIFIER) 
        LEFT JOIN Services AS LeadService ON Leads.ServiceKey IS NOT NULL AND TRY_CAST(Leads.ServiceKey AS UNIQUEIDENTIFIER) = LeadService.PublicKey 
        LEFT JOIN Services AS PoService ON po.ServiceId = PoService.Id AND ISNULL(po.IsExpired, 0) <> 1 
        LEFT JOIN STATUS AS st1 ON st1.Id = po.Status 
        LEFT JOIN PaymentModes AS pm ON po.ModeOfPayment = pm.Id 
    WHERE ISNULL(Leads.IsDelete, 0) = 0 
        AND (@AssignedTo IS NULL OR Leads.AssignedTo = @AssignedTo) 
        AND (  
            (po.Status <> 24 AND ISNULL(po.IsExpired, 0) = 0) 
        OR (po.Status = 24 AND ISNULL(po.IsExpired, 0) = 1) 
        OR (po.Status IS NULL) 
        OR (po.Status = 4)  
        ) 
        AND((leads.ServiceKey in (SELECT CAST(value AS uniqueidentifier) 
        FROM STRING_SPLIT(@SecondaryKey, ',') ) 
        and @SecondaryKey is not null ) or( leads.ServiceKey = leads.ServiceKey and @SecondaryKey is null)) 
        AND (ISNULL(Leads.LeadSourceKey, '1') = ISNULL(@FourthKey, ISNULL(Leads.LeadSourceKey, '1'))) 
        AND (ISNULL(Leads.LeadTypeKey, '1') = ISNULL(@ThirdKey, ISNULL(Leads.LeadTypeKey, '1'))) 
        AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)) 
        AND (  
            @SearchText IS NULL 
        OR Leads.MobileNumber LIKE '%' + @SearchText + '%' 
        OR Leads.AlternateMobileNumber LIKE '%' + @SearchText + '%' 
        OR Leads.EmailId LIKE '%' + @SearchText + '%' 
        OR Leads.FullName LIKE '%' + @SearchText + '%'  
        ) 
        AND ( 
            @FifthKey IS NULL
        OR (@FifthKey = 'New' AND ISNULL(st1.Name, 'New') = 'New')
        OR (@FifthKey <> 'New' AND ISNULL(st1.Name, 'New') = @FifthKey) 
        )
    ORDER BY SlNo ASC; 
    
SELECT * 
        FROM #tempLeads 
        WHERE SlNo BETWEEN ((@PageNumber - 1) * @PageSize) + 1 AND @PageSize * @PageNumber; 

    DROP TABLE #tempLeads; 
END

GO
ALTER PROCEDURE [dbo].[GetTradeJournalsByMobileUserKey]  
    @MobileUserKey UNIQUEIDENTIFIER,  
    @PageNumber INT = NULL,  
    @PageSize INT = NULL,  
    @FromDate DATE = NULL, 
    @ToDate DATE = NULL,  
    @TotalCount INT OUTPUT  
AS  
BEGIN  
    SET NOCOUNT ON;  

    -- Total Count with optional date filter 
    SELECT @TotalCount = COUNT(1)  
    FROM TradingJournal  
    WHERE  
        MobileUserKey = @MobileUserKey  
        AND IsDeleted = 0  
        AND (@FromDate IS NULL OR CreatedOn >= @FromDate) 
        AND (@ToDate IS NULL OR CreatedOn <= @ToDate); 

    -- If pagination parameters are provided, apply paging
   IF @PageNumber IS NOT NULL AND @PageSize IS NOT NULL AND @PageSize > 0 AND @PageNumber > 0
    BEGIN
        DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;  

        -- Apply pagination with OFFSET and FETCH NEXT
        SELECT   
            Id,  
            CONVERT(VARCHAR(10), StartDate, 120) AS StartDate,  
            MobileUserKey,  
            Symbol,  
            BuySellButton,  
            CapitalAmount,  
            RiskPercentage,  
            RiskAmount,  
            EntryPrice,  
            StopLoss,  
            Target1,  
            Target2,  
            PositionSize,  
            ActualExitPrice,  
            ProfitLoss,  
            RiskReward,  
            Notes,  
            CreatedBy,  
            ModifiedBy,  
            IsActive,  
            IsDeleted,  
            CreatedOn,  
            ModifiedOn  
        FROM TradingJournal  
        WHERE  
            MobileUserKey = @MobileUserKey  
            AND IsDeleted = 0 
            AND (@FromDate IS NULL OR CreatedOn >= @FromDate) 
            AND (@ToDate IS NULL OR CreatedOn <= @ToDate) 
        ORDER BY CreatedOn DESC  
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;  
    END
    ELSE
    BEGIN
        -- No pagination: return all matching records
        SELECT   
            Id,  
            CONVERT(VARCHAR(10), StartDate, 120) AS StartDate,  
            MobileUserKey,  
            Symbol,  
            BuySellButton,  
            CapitalAmount,  
            RiskPercentage,  
            RiskAmount,  
            EntryPrice,  
            StopLoss,  
            Target1,  
            Target2,  
            PositionSize,  
            ActualExitPrice,  
            ProfitLoss,  
            RiskReward,  
            Notes,  
            CreatedBy,  
            ModifiedBy,  
            IsActive,  
            IsDeleted,  
            CreatedOn,  
            ModifiedOn  
        FROM TradingJournal  
        WHERE  
            MobileUserKey = @MobileUserKey  
            AND IsDeleted = 0 
            AND (@FromDate IS NULL OR CreatedOn >= @FromDate) 
            AND (@ToDate IS NULL OR CreatedOn <= @ToDate) 
        ORDER BY CreatedOn DESC;
    END
END

------------Ajith 13-05-2025 11:40 AM
-- Create IX_Leads_LedTypeKey if it does not exist
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('Leads') AND name = 'IX_Leads_LedTypeKey'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Leads_LedTypeKey
    ON Leads (LeadTypeKey);
END;

-- Create IX_Leads_LedSourceKey if it does not exist
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('Leads') AND name = 'IX_Leads_LedSourceKey'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Leads_LedSourceKey
    ON Leads (LeadSourceKey);
END;

-- Create IX_Leads_ServiceKey if it does not exist
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('Leads') AND name = 'IX_Leads_ServiceKey'
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Leads_ServiceKey
    ON Leads (ServiceKey);
END;

----------------Ajith 14-05-2025 2:20 PM
GO
ALTER PROCEDURE GetProductCommunityMappings  
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
  
    -- Normalize @FromDate and @ToDate  
    SET @FromDate = COALESCE(@FromDate, '1900-01-01'); -- Default to old date if NULL  
    SET @ToDate = COALESCE(@ToDate, GETDATE()); -- Default to today if NULL  
    SET @ToDate = DATEADD(DAY, 1, @ToDate); -- Ensure the @ToDate includes the full day  
  
    -- Count total records matching the criteria  
    SELECT @TotalCount = COUNT(*)  
    FROM ProductCommunityMappingM pcmm  
        LEFT JOIN ProductsM pm1 ON pcmm.ProductId = pm1.Id  
        LEFT JOIN ProductsM pm2 ON pcmm.CommunityId = pm2.Id  
        LEFT JOIN Users u1 ON pcmm.CreatedBy = u1.Id  
        LEFT JOIN Users u2 ON pcmm.ModifiedBy = u2.Id  
    WHERE    
        (@SearchText IS NULL OR  
         pm1.Name LIKE '%' + @SearchText + '%' OR   
         pm2.Name LIKE '%' + @SearchText + '%')   
        AND pcmm.CreatedDate >= @FromDate  
        AND pcmm.CreatedDate < @ToDate  
        AND (   
            @Status IS NULL OR   
            (@Status = 'ACTIVE' AND pcmm.IsActive = 1 AND pcmm.IsDeleted = 0) OR  
            (@Status = 'INACTIVE' AND pcmm.IsActive = 0 AND pcmm.IsDeleted = 0) 
        ) AND pcmm.IsDeleted = 0 ;  
  
    -- Fetch paginated results  
    SELECT  
        pcmm.Id,  
        pcmm.ProductId,  
        pm1.Name AS ProductName, -- Fetching Product Name  
        pcmm.CommunityId,  
        pm2.Name AS CommunityName, -- Fetching Community Name  
        pcmm.IsActive,  
        pcmm.IsDeleted,  
        pcmm.CreatedDate,  
        pcmm.ModifiedDate,  
        pcmm.DurationInDays, 
        u1.FirstName + ' ' + u1.LastName AS CreatedBy,  
        u2.FirstName + ' ' + u2.LastName AS ModifiedBy  
    FROM ProductCommunityMappingM pcmm  
        -- Join for Product Name  
        LEFT JOIN ProductsM pm1 ON pcmm.ProductId = pm1.Id  
        -- Join for Community Name  
        LEFT JOIN ProductsM pm2 ON pcmm.CommunityId = pm2.Id  
        -- Join Users table for CreatedBy  
        LEFT JOIN Users u1 ON pcmm.CreatedBy = u1.Id  
        -- Join Users table for ModifiedBy  
        LEFT JOIN Users u2 ON pcmm.ModifiedBy = u2.Id  
    WHERE  
        (@SearchText IS NULL OR   
         pm1.Name LIKE '%' + @SearchText + '%' OR   
         pm2.Name LIKE '%' + @SearchText + '%')   
        AND pcmm.CreatedDate >= @FromDate   
        AND pcmm.CreatedDate < @ToDate   
        AND (   
            @Status IS NULL OR  
            (@Status = 'ACTIVE' AND pcmm.IsActive = 1 AND pcmm.IsDeleted = 0) OR  
            (@Status = 'INACTIVE' AND pcmm.IsActive = 0 AND pcmm.IsDeleted = 0) 
        )
        AND pcmm.IsDeleted = 0  
    ORDER BY pcmm.ModifiedDate DESC   
    OFFSET (@PageNumber - 1) * @PageSize ROWS  
    FETCH NEXT @PageSize ROWS ONLY;  
END; 

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
            (@Status = 'INACTIVE' AND pbm.IsActive = 0 AND pbm.IsDeleted = 0)
        ) AND pbm.IsDeleted = 0;  
 
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
            (@Status = 'INACTIVE' AND pbm.IsActive = 0 AND pbm.IsDeleted = 0) 
        )  AND pbm.IsDeleted = 0
    ORDER BY pbm.ModifiedOn DESC  
    OFFSET (@PageNumber - 1) * @PageSize ROWS  
    FETCH NEXT @PageSize ROWS ONLY;  
END; 
---SIVA 16 MAY 2025---------------------------
GO
ALTER PROCEDURE GetProductCommunityMappings    
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
    
    -- Normalize @FromDate and @ToDate    
    SET @FromDate = COALESCE(@FromDate, '1900-01-01'); -- Default to old date if NULL    
    SET @ToDate = COALESCE(@ToDate, GETDATE()); -- Default to today if NULL    
    SET @ToDate = DATEADD(DAY, 1, @ToDate); -- Ensure the @ToDate includes the full day    
    
    -- Count total records matching the criteria    
    SELECT @TotalCount = COUNT(*)    
    FROM ProductCommunityMappingM pcmm    
        LEFT JOIN ProductsM pm1 ON pcmm.ProductId = pm1.Id    
        LEFT JOIN ProductsM pm2 ON pcmm.CommunityId = pm2.Id    
        LEFT JOIN Users u1 ON pcmm.CreatedBy = u1.Id    
        LEFT JOIN Users u2 ON pcmm.ModifiedBy = u2.Id    
    WHERE      
        (@SearchText IS NULL OR    
         pm1.Name LIKE '%' + @SearchText + '%' OR     
         pm2.Name LIKE '%' + @SearchText + '%')     
        AND(@FromDate IS NULL OR pcmm.CreatedDate >= @FromDate )   
        AND(@ToDate IS NULL OR pcmm.CreatedDate < @ToDate  )  
        AND (     
            @Status IS NULL OR     
            (@Status = 'ACTIVE' AND pcmm.IsActive = 1 AND pcmm.IsDeleted = 0) OR    
            (@Status = 'INACTIVE' AND pcmm.IsActive = 0 AND pcmm.IsDeleted = 0)   
        ) AND pcmm.IsDeleted = 0 ;    
    
    -- Fetch paginated results    
    SELECT    
        pcmm.Id,    
        pcmm.ProductId,    
        pm1.Name AS ProductName, -- Fetching Product Name    
        pcmm.CommunityId,    
        pm2.Name AS CommunityName, -- Fetching Community Name    
        pcmm.IsActive,    
        pcmm.IsDeleted,    
        pcmm.CreatedDate,    
        pcmm.ModifiedDate,    
        pcmm.DurationInDays,   
        u1.FirstName + ' ' + u1.LastName AS CreatedBy,    
        u2.FirstName + ' ' + u2.LastName AS ModifiedBy    
    FROM ProductCommunityMappingM pcmm    
        -- Join for Product Name    
        LEFT JOIN ProductsM pm1 ON pcmm.ProductId = pm1.Id    
        -- Join for Community Name    
        LEFT JOIN ProductsM pm2 ON pcmm.CommunityId = pm2.Id    
        -- Join Users table for CreatedBy    
        LEFT JOIN Users u1 ON pcmm.CreatedBy = u1.Id    
        -- Join Users table for ModifiedBy    
        LEFT JOIN Users u2 ON pcmm.ModifiedBy = u2.Id    
    WHERE    
        (@SearchText IS NULL OR     
         pm1.Name LIKE '%' + @SearchText + '%' OR     
         pm2.Name LIKE '%' + @SearchText + '%')     
         AND(@FromDate IS NULL OR pcmm.CreatedDate >= @FromDate )   
        AND(@ToDate IS NULL OR pcmm.CreatedDate < @ToDate  )      
        AND (     
            @Status IS NULL OR    
            (@Status = 'ACTIVE' AND pcmm.IsActive = 1 AND pcmm.IsDeleted = 0) OR    
            (@Status = 'INACTIVE' AND pcmm.IsActive = 0 AND pcmm.IsDeleted = 0)   
        )  
        AND pcmm.IsDeleted = 0    
    ORDER BY pcmm.ModifiedDate DESC     
    OFFSET (@PageNumber - 1) * @PageSize ROWS    
    FETCH NEXT @PageSize ROWS ONLY;    
END; 

---------------------Ajith 16-05-2025 4:23 PM
GO
--  exec getmobileproducts 'breakfast'                   
ALTER PROCEDURE  [dbo].[GetMobileProducts] @SearchText VARCHAR(100)     
AS     
BEGIN     
 SELECT max(p.id) AS Id     
  ,p.Name     
  ,p.Code     
  ,p.Description     
  ,p.DescriptionTitle     
  ,p.ListImage     
  ,p.LandscapeImage     
  ,p.Price     
  ,p.DiscountAmount AS DiscountAmount     
  ,p.DiscountPercent AS DiscountPercent     
  ,p.CategoryId     
  ,max(pcm.Name) AS Category   ,  
  max(pcm.code) as CategoryCode  
  ,p.CreatedDate     
  ,p.IsActive     
  ,p.imageUrl
  ,p.CanPost    
  ,max(p.subscriptionId) AS SubscriptionId     
  ,cast(max(isnull(p.modifiedDate, p.createdDate)) AS DATETIME) AS LastModified     
  ,COUNT(DISTINCT pc.id) AS ContentCount     
  ,(     
   SELECT STUFF((     
      SELECT DISTINCT ', ' + pcm.AttachmentType     
      FROM ProductsContentM pcm     
      WHERE pcm.ProductId = max(p.id)     
      FOR XML PATH('')     
       ,TYPE     
      ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS DistinctAttachmentTypes     
   ) AS DistinctAttachmentTypes     
 FROM productsm p     
 LEFT JOIN ProductsContentM pc ON pc.productId = p.id     
 LEFT JOIN ProductCategoriesM pcm ON pcm.id = p.CategoryId     
 WHERE p.code <> 'FREE'     
  AND (@SearchText is NULL)     
  and p.IsDeleted = 0   
  OR (p.Name LIKE '%' + @SearchText + '%')     
 GROUP BY p.name     
  ,p.Code     
  ,p.Description     
  ,p.Price     
  ,p.DiscountAmount     
  ,p.DiscountPercent     
  ,p.CategoryId     
  ,p.CreatedDate     
  ,p.ListImage     
  ,p.LandscapeImage     
  ,p.imageUrl
  ,p.CanPost     
  ,P.IsActive     
  ,p.DescriptionTitle     
 ORDER BY Id     
END; 

GO
--exec [GetFilteredMobileProducts] null,null,null  
  
ALTER PROCEDURE [dbo].[GetFilteredMobileProducts]      
    @SearchText VARCHAR(100) = NULL,      
    @CategoryId INT = NULL,      
    @Status INT = NULL      
AS             
BEGIN      
    SET NOCOUNT ON;      
      
    SELECT      
        MAX(p.id) AS Id,      
        p.Name,      
        p.Code,      
        p.Description,      
        p.DescriptionTitle,      
        p.ListImage,      
        p.LandscapeImage,      
        p.Price,      
        p.DiscountAmount,      
        p.DiscountPercent,      
        p.CategoryId,      
        MAX(pcm.Name) AS Category,     
          max(pcm.code) as CategoryCode,  
        p.CreatedDate,      
        p.IsActive,      
        p.imageUrl,
        p.CanPost,      
        MAX(p.subscriptionId) AS SubscriptionId,      
        CAST(MAX(ISNULL(p.modifiedDate, p.createdDate)) AS DATETIME) AS LastModified,      
        COUNT(DISTINCT CASE WHEN pc.IsDeleted = 0 THEN pc.id END) AS ContentCount,      
      
        -- Fetching distinct attachment types      
        (         
            SELECT STUFF((        
                SELECT DISTINCT ', ' + pcmi.AttachmentType      
            FROM ProductsContentM pcmi      
            WHERE pcmi.ProductId = p.id      
            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '')          
        ) AS DistinctAttachmentTypes      
      
    FROM productsm p      
        LEFT JOIN ProductsContentM pc ON pc.productId = p.id      
        LEFT JOIN ProductCategoriesM pcm ON pcm.id = p.CategoryId      
    WHERE          
        p.code <> 'FREE'      
        AND p.IsDeleted = 0      
        AND (@Status IS NULL OR p.IsActive = @Status)      
        AND (@SearchText IS NULL OR p.Name LIKE '%' + @SearchText + '%')      
        AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)      
    GROUP BY         
    p.Id,       
        p.Name,             
        p.Code,             
        p.Description,             
        p.Price,             
        p.DiscountAmount,             
        p.DiscountPercent,             
        p.CategoryId,             
        p.CreatedDate,             
        p.ListImage,             
        p.LandscapeImage,             
        p.imageUrl, 
        p.CanPost,            
        p.IsActive,             
        p.DescriptionTitle    
    ORDER BY MAX(p.id) DESC;      
-- Ordering correctly      
      
END;      
    
    
  
    
    
---- Step 1: Remove Default Constraint if it exists    
--DECLARE @constraintName NVARCHAR(255);    
    
--SELECT @constraintName = name     
--FROM sys.default_constraints     
--WHERE parent_object_id = OBJECT_ID('dbo.ScheduledNotificationM')     
--AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('dbo.ScheduledNotificationM'), 'ScheduledEndTime', 'ColumnId');    
    
--IF @constraintName IS NOT NULL    
--BEGIN    
--    EXEC('ALTER TABLE [dbo].[ScheduledNotificationM] DROP CONSTRAINT ' + @constraintName);    
--END    
    
---- Step 2: Alter Column Data Type    
--ALTER TABLE [dbo].[ScheduledNotificationM]     
--ALTER COLUMN [ScheduledEndTime] DATETIME2(7) NULL;    
    
    
    
---- Alter DeviceVersion column to VARCHAR(50)    
--ALTER TABLE [dbo].[MobileUsers]     
--ALTER COLUMN [DeviceVersion] VARCHAR(50) NULL;    
    
    
---- Step 1: Add Default Constraint for IsActive    
--ALTER TABLE [dbo].[ScannerPerformanceM]     
--ADD CONSTRAINT DF_ScannerPerformanceM_IsActive DEFAULT (1) FOR [IsActive];    
    
---- Step 2: Add Default Constraint for IsDelete    
--ALTER TABLE [dbo].[ScannerPerformanceM]     
--ADD CONSTRAINT DF_ScannerPerformanceM_IsDelete DEFAULT (0) FOR [IsDelete];    
    
 ----Siva 19-May 2025 
 GO
    CREATE PROCEDURE GetProductCommunityMappings      
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
      
    -- Normalize @FromDate and @ToDate      
    SET @FromDate = COALESCE(@FromDate, '1900-01-01'); -- Default to old date if NULL      
    SET @ToDate = COALESCE(@ToDate, GETDATE()); -- Default to today if NULL      
    SET @ToDate = DATEADD(DAY, 1, @ToDate); -- Ensure the @ToDate includes the full day      
      
    -- Count total records matching the criteria      
    SELECT @TotalCount = COUNT(*)      
    FROM ProductCommunityMappingM pcmm      
        LEFT JOIN ProductsM pm1 ON pcmm.ProductId = pm1.Id      
        LEFT JOIN ProductsM pm2 ON pcmm.CommunityId = pm2.Id      
        LEFT JOIN Users u1 ON pcmm.CreatedBy = u1.Id      
        LEFT JOIN Users u2 ON pcmm.ModifiedBy = u2.Id      
    WHERE        
        (@SearchText IS NULL OR      
         pm1.Name LIKE '%' + @SearchText + '%' OR       
         pm2.Name LIKE '%' + @SearchText + '%')       
        AND(@FromDate IS NULL OR pcmm.CreatedDate >= @FromDate )     
        AND(@ToDate IS NULL OR pcmm.CreatedDate < @ToDate  )    
        AND (       
            @Status IS NULL OR       
            (@Status = 'ACTIVE' AND pcmm.IsActive = 1 AND pcmm.IsDeleted = 0) OR      
            (@Status = 'INACTIVE' AND pcmm.IsActive = 0 AND pcmm.IsDeleted = 0)     
        ) AND pcmm.IsDeleted = 0 ;      
      
    -- Fetch paginated results      
    SELECT      
        pcmm.Id,      
        pcmm.ProductId,      
        pm1.Name AS ProductName, -- Fetching Product Name      
        pcmm.CommunityId,      
        pm2.Name AS CommunityName, -- Fetching Community Name      
        pcmm.IsActive,      
        pcmm.IsDeleted,      
        pcmm.CreatedDate,      
        pcmm.ModifiedDate,      
        pcmm.DurationInDays,     
        u1.FirstName + ' ' + u1.LastName AS CreatedBy,      
        u2.FirstName + ' ' + u2.LastName AS ModifiedBy      
    FROM ProductCommunityMappingM pcmm      
        -- Join for Product Name      
        LEFT JOIN ProductsM pm1 ON pcmm.ProductId = pm1.Id      
        -- Join for Community Name      
        LEFT JOIN ProductsM pm2 ON pcmm.CommunityId = pm2.Id      
        -- Join Users table for CreatedBy      
        LEFT JOIN Users u1 ON pcmm.CreatedBy = u1.Id      
        -- Join Users table for ModifiedBy      
        LEFT JOIN Users u2 ON pcmm.ModifiedBy = u2.Id      
    WHERE      
        (@SearchText IS NULL OR       
         pm1.Name LIKE '%' + @SearchText + '%' OR       
         pm2.Name LIKE '%' + @SearchText + '%')       
         AND(@FromDate IS NULL OR pcmm.CreatedDate >= @FromDate )     
        AND(@ToDate IS NULL OR pcmm.CreatedDate < @ToDate  )        
        AND (       
            @Status IS NULL OR      
            (@Status = 'ACTIVE' AND pcmm.IsActive = 1 AND pcmm.IsDeleted = 0) OR      
            (@Status = 'INACTIVE' AND pcmm.IsActive = 0 AND pcmm.IsDeleted = 0)     
        )    
        AND pcmm.IsDeleted = 0      
    ORDER BY pcmm.ModifiedDate DESC       
    OFFSET (@PageNumber - 1) * @PageSize ROWS      
    FETCH NEXT @PageSize ROWS ONLY;      
END; 

 -----------------------Ajith 19-05-2025 03:58 PM

    GO
    --exec [GetProductById] 96,'d7f73b83-bee9-ef11-b38d-f7f49baa4b49'                                
ALTER PROCEDURE [dbo].[GetProductById]     
    @ProductId INT,     
    @MobileUserKey UNIQUEIDENTIFIER     
AS     
BEGIN     
    --declare @ProductId int = 5                                                                        
    --,@MobileUserKey UNIQUEIDENTIFIER = '294A46B7-E918-EF11-B260-A630D7B5F938'   
    
    DECLARE @priorDaysInfo INT = CAST((SELECT TOP 1     
        value     
    FROM settings     
    WHERE code = 'PRIORDAYSINFO') AS INT)     
    DECLARE @ContentCount INT = 0,      
            @VideoContent INT = 0     
     
    SELECT @ContentCount = COUNT(pc.Id),     
        @VideoContent = COUNT(CASE      
                                      WHEN pc.attachmentType = 'video' THEN 1      
                                      ELSE NULL END) + (   SELECT COUNT(CASE      
                                                                             WHEN sc.Link IS NOT NULL THEN 1      
                                                                             ELSE NULL END)     
        FROM SUBCHAPTERS sc     
            JOIN CHAPTERS c     
            ON sc.ChapterId = c.id     
        WHERE c.ProductId = @ProductId     
            AND c.IsActive  = 1     
            AND c.IsDelete  = 0     
            AND sc.IsActive = 1     
            AND sc.IsDelete = 0)     
    FROM PRODUCTSCONTENTM pc     
    WHERE pc.ProductId = @ProductId     
        AND pc.isActive  = 1     
        AND pc.isDeleted = 0;     
     
     
    DROP TABLE IF EXISTS #tempBenefits     
    SELECT ben.Id,     
        ben.GiftProductId,     
        sub.NAME AS Names,     
        ben.Months,     
        pro.NAME,     
        pro.Description     
    INTO   #tempBenefits     
    FROM ExtraBenefitsM AS ben     
        INNER JOIN ProductsM AS pro     
        ON ben.GiftProductId       = pro.Id     
        INNER JOIN SubscriptionDurationM AS sub     
        ON sub.Id                  = ben.SubscriptionId     
            AND isnull(sub.isActive, 1) = 1     
    WHERE ben.ProductId           = @ProductId     
        AND ISNULL(pro.IsActive, 1) = 1     
     
    DECLARE @extraBenefits NVARCHAR(MAX) = (SELECT *     
    FROM #tempBenefits     
    FOR JSON AUTO)     
    DECLARE @CurrentDate DATE = cast(getdate() AS DATE)     
     
    DECLARE @IsOutOfSubscription VARCHAR(300) = (   SELECT TOP 1     
        mobileUserKey     
    FROM MYBucketM b     
    WHERE productId            = @ProductId     
        AND mobileUserKey        = @MobileUserKey     
        AND ISNULL(IsActive, 1)  = 1     
        AND isnull(IsExpired, 0) = 0     
        AND @currentDate         >= cast(b.StartDate AS DATE)     
        AND @currentDate         <= cast(b.endDate AS DATE))     
    DECLARE @isExpired NVARCHAR(max) = (   SELECT TOP 1     
        IsExpired     
    FROM MYBucketM     
    WHERE productId            = @ProductId     
        AND mobileUserKey        = @MobileUserKey     
        AND ISNULL(IsACtive, 1)  = 1     
        AND isnull(IsExpired, 0) = 0)     
   
   
      DECLARE @accessToScanner BIT = 0;   
  IF EXISTS (   
    SELECT 1   
    FROM checkScannerWithStrategyExistsView AS viewTemp   
    WHERE viewTemp.ProductId = @ProductId   
      AND viewTemp.MobileUserKey = @MobileUserKey   
      AND CAST(ISNULL(viewTemp.EndDate, GETDATE()) AS DATE) >= CAST(GETDATE() AS DATE)   
)   
   
    BEGIN   
        SET @accessToScanner = 1;   
    END   
     
    SELECT TOP 1     
        p.id,     
        p.NAME,     
        p.code,     
        p.Description,     
        p.DescriptionTitle,     
        p.CategoryID,     
        P.IsQueryFormEnabled,     
        @accessToScanner AS accessToScanner,   
        CASE      
                WHEN (   p.isActive = 1     
            AND mb.id IS NOT NULL)     
            OR (   p.isActive = 0     
            AND mb.id IS NOT NULL) THEN pcmm.CommunityId      
      ELSE NULL END AS CommunityId,     
        CASE      
                WHEN (   p.isActive = 1     
            AND mb.id IS NOT NULL)     
            OR (   p.isActive = 0     
            AND mb.id IS NOT NULL) THEN pc.name      
                ELSE NULL END AS communityname,     
        pcm.NAME AS Category,     
        CAST(p.Price AS DECIMAL(16, 4)) AS Price,     
        cast(pom.CouponKey AS VARCHAR(200)) AS CouponCode,     
        isnull(pom.PaidAmount, 0.0) AS PaidAmount,     
        CAST(ISNULL(pom.CouponDiscountAmount, 0.0) AS VARCHAR) AS Discount,     
        CAST(ISNULL(PR.Rating, 0) AS VARCHAR) AS UserRating,     
        '' AS Liked,     
        '' AS EnableSubscription,     
        (   SELECT top 1     
            sv.DurationName     
        FROM SubscriptionView sv     
        WHERE ProductId         = @ProductId     
            and sv.DurationName   != 'free'     
            AND sv.DurationActive = 1) AS SubscriptionData,     
        CAST(ISNULL(pl.LikeId, 0) AS BIT) AS IsHeart,     
        CAST(0 AS BIT) AS IsThumbsUp,     
        @extraBenefits AS ExtraBenefits,     
        CAST(CASE      
                     WHEN DATEDIFF(day, GETDATE(), mb.enddate) <= @priorDaysInfo THEN 1      
                     ELSE 0 END AS BIT) AS ShowReminder,     
        CAST(IIF(@isExpired IS NOT NULL, 1, 0) AS BIT) AS IsInMyBucket,     
        (   SELECT TOP 1     
            Attachment     
        FROM ProductsContentM     
        WHERE productId = @ProductId     
            AND title LIKE '%intro%') AS LandscapeVideoUrl,     
        P.LandscapeImage AS LandscapeImage,     
        CAST(IIF(@IsOutOfSubscription IS NULL, 0, 1) AS BIT) AS IsInValidity,     
        (SELECT *     
        FROM ProductsContentM     
        WHERE productId = @ProductId     
        FOR JSON AUTO) AS Content,     
        (      
           --buy button text                                                                              
           CASE      
                -- Case for Active Products                                  
                WHEN p.isActive = 1 THEN      
                    CASE      
                         WHEN mb.id IS NULL THEN 'Buy'      
                         ELSE CASE      
                                   WHEN DATEDIFF(day, GETDATE(), mb.enddate) < @priorDaysInfo THEN 'Renew'      
                                   ELSE 'Purchased' END END      
      
                -- Case for Inactive Products                                  
                WHEN p.isActive = 0 THEN CASE      
                                              WHEN mb.id IS NOT NULL THEN 'Purchased'      
                                              WHEN mb.id IS NULL THEN '' END END) AS BuyButtonText,     
        @ContentCount AS ContentCount,     
        @VideoContent AS VideoCount,     
        DATEDIFF(day, getdate(), ISNULL(mb.enddate, GETDATE())) AS DaysToGo,     
        (CASE      
                 WHEN mb.id IS NOT NULL     
            AND DATEDIFF(day, GETDATE(), mb.enddate) >= @priorDaysInfo THEN '[]'      
                 ELSE (   SELECT *     
        FROM (                   SELECT pb.BonusProductId AS Id,     
                    p2.NAME AS BonusProductName,     
                    pb.DurationInDays AS Validity,     
                    (   SELECT s.Value     
                    FROM Settings s     
                    WHERE s.Code     = 'BonusMessage'     
                        AND s.IsActive = 1) AS BonusMessage     
                FROM ProductBonusMappingM pb     
                    INNER JOIN ProductsM p2     
                    ON pb.BonusProductId = p2.Id     
                    LEFT JOIN ProductCategoriesM pcm2     
                    ON p2.CategoryID     = pcm2.Id     
                WHERE pb.ProductId = p.id     
                    AND pb.IsActive  = 1     
                    and pb.IsDeleted = 0     
                    AND p2.IsActive  = 1     
                    AND p2.IsDeleted = 0     
            UNION ALL     
                SELECT pcm.CommunityId,     
                    max(pTemp.Name) as BonusProductName,     
                    min(pcm.DurationInDays) as Validity,     
                    (   SELECT s.Value     
                    FROM Settings s     
                    WHERE s.Code     = 'BonusMessage'     
     AND s.IsActive = 1) AS BonusMessage     
                From ProductCommunityMappingM as pcm     
                    inner join SubscriptionMappingM as sm     
                    on pcm.CommunityId           = sm.ProductId     
                    inner join SubscriptionDurationM as sd     
                    on sm.SubscriptionDurationId = sd.Id     
                    inner join ProductsM as pTemp     
                    on pTemp.Id                  = pcm.CommunityId     
                where pcm.ProductId              = p.Id     
                    and ISNULL(pTemp.IsActive, 1)  = 1     
                    and ISNULL(pTemp.IsDeleted, 0) = 0     
                    and ISNULL(pcm.IsActive, 1)    = 1     
   and ISNULL(pcm.IsDeleted, 0)   = 0     
                    and ISNULL(sm.IsActive, 1)     = 1     
                    and ISNULL(sd.IsActive, 1)     = 1     
                group by pcm.CommunityId) AS UnionedResults     
        FOR JSON PATH) END) AS BonusProducts,
         (
    SELECT TOP 1 pb.BonusProductId 
    FROM ProductBonusMappingM pb
    INNER JOIN ProductsM p2 ON pb.BonusProductId = p2.Id
    WHERE pb.ProductId = p.id
      AND pb.IsActive = 1
      AND pb.IsDeleted = 0
      AND p2.IsActive = 1
      AND p2.IsDeleted = 0
) AS ScannerBonusProductId

    FROM ProductsM AS P     
        INNER JOIN ProductCategoriesM AS pcm  ON p.CategoryID     = pcm.Id     
        LEFT JOIN PurchaseOrdersM AS POM ON POM.ProductId    = p.Id     
            AND pom.ProductId    = @ProductId     
            AND pom.ActionBy     = @MobileUserKey     
        LEFT JOIN ProductsRatingM AS PR     
        ON PR.ProductId     = P.Id     
            AND PR.CreatedBy     = @MobileUserKey     
        LEFT JOIN ProductLikesM AS pl     
        ON pl.ProductId     = p.Id     
            AND pl.LikeId        = 1     
            AND pl.CreatedBy     = @MobileUserKey     
            AND pl.IsDelete      = 0     
        LEFT JOIN subscriptiondurationm s     
        ON s.Id             = p.SubscriptionId     
        LEFT JOIN MYBucketM AS Mb     
        ON p.id             = mb.ProductId     
            AND mb.mobileuserkey = @MobileUserkey     
        left join ProductCommunityMappingM pcmm     
        on p.Id             = pcmm.ProductId     
            and pcmm.IsActive    = 1     
            and pcmm.IsDeleted   = 0     
        LEFT JOIN ProductsM pc     
        ON pcmm.CommunityId = pc.Id     
            and pc.IsActive      = 1     
            and pc.IsDeleted     = 0     
    WHERE p.id = @ProductId     
    ORDER BY POM.CreatedOn DESC     
END 

----------------------Ajith 22-05-2025 3:10 PM 
 
 GO
 
-- exec GetCouponsM '1194d99e-c419-ef11-b261-88b133b31c8f', 1 ,2          
-- exec GetCouponsM 'ba0c9cce-6a8f-ef11-819c-00155d23d79c', 1, 2      
-- exec GetCouponsM '3B21B407-6D64-EF11-8175-00155D23D79C', 3, 2      
     
-- Finding the default subscriptioNmapping discount and then given coupon discount to get the final discount      
-- First get default discount (SubscriptionMapping)     
-- Then again substract the coupon disocunt from the price      
ALTER PROCEDURE GetCouponsM                
(                
    @userKey UNIQUEIDENTIFIER,                
    @productId INT,                
    @subscriptionDurationId INT                
)                
AS                
BEGIN         
       
-- select * From CouponsM  where id = 9        
-- SELECT * from CouponProductMappingM where CouponID = 9        
-- SELECT * from CouponUserMappingM where mobileUserKey = '0e5705f3-36a7-ef11-b330-ab74b4cc6d1c'       
-- select * From MobileUsers where mobile = '8309091570'   
-- declare  @productId INT =  2, @subscriptionDurationId int = 2 , @userKey UNIQUEIDENTIFIER = 'ba0c9cce-6a8f-ef11-819c-00155d23d79c'       
   
SELECT    
 c.Name as CouponName,        
    c.DiscountInPercentage AS discountPercentage,        
    sm.DiscountPercentage  AS discountPercentagesm,        
    c.[Description] as Description ,        
 CAST(
            (
                -- Step 1: Base price after subscription discount
                ((p.Price * sd.Months) * (1 - sm.DiscountPercentage / 100.0))
                -- Step 2: Apply coupon discount on the above result
                * (c.DiscountInPercentage / 100.0)
            ) 
        AS DECIMAL(10,2)) AS DiscountAmount
   
FROM ProductsM p         
INNER JOIN CouponProductMappingM as cpm on p.Id = ISNULL(cpm.ProductID , @productId) and p.id = @productId       
INNER JOIN CouponsM as c on c.Id = cpm.CouponID       
INNER JOIN SubscriptionMappingM AS SM ON p.Id = SM.ProductId and sm.ProductId = @productId 
INNER JOIN SubscriptionDurationM sd ON sm.SubscriptionDurationId = sd.Id      
WHERE ISNULL(p.IsActive,1) = 1 and  ISNULL(p.IsDeleted,0) = 0 AND        
ISNULL(c.IsVisible ,0) = 1 AND ISNULL( sm.IsActive,1) = 1        
AND ISNULL(c.IsActive ,1) = 1        
AND SM.SubscriptionDurationId = @subscriptionDurationId        
AND c.id in (        
        SELECT c.Id       
        FROM CouponsM c       
        LEFT JOIN CouponUserMappingM cu ON cu.CouponID = c.Id and cu.IsActive = 1        
        LEFT JOIN MobileUsers mu ON mu.PublicKey = cu.MobileUserKey       
        LEFT JOIN CouponProductMappingM cp ON cp.CouponID = c.Id       
        WHERE ISNULL(c.IsActive, 1) = 1       
        AND ISNULL(c.IsDelete, 0) = 0       
        AND c.RedeemLimit > c.TotalRedeems       
        AND ISNULL(c.IsVisible, 0) = 1       
        AND (       
        (cu.MobileUserKey IS NULL) -- Coupon not assigned to any user       
        OR (cu.MobileUserKey = @userKey) -- Coupon assigned to the specific user       
        )       
        AND (       
        (cp.ProductID IS NULL) -- Coupon not mapped to any product       
        OR (cp.ProductID = @productID) -- Coupon mapped to the specific product       
        )       
)        
       
END 

-----------------Ajith 30-05-2025 1:26 PM
GO
ALTER PROCEDURE GetSubscriptionPlanWithProduct                    
    @ProductId INT = 1,                    
    @SubscriptionPlanId int = 25,                    
    @MobileUserKey uniqueidentifier = null,                    
    @DeviceType VARCHAR(15) = 'android'                    
AS                    
BEGIN                    
                    
    SELECT sp.Id AS SubscriptionPlanId,                    
           sp.Name AS PlanName,                    
           sp.Description AS PlanDescription,                    
           sm.ProductId,                    
           pm.Name as ProductName,                    
           (pm.Price * SD.Months) as ActualPrice,                    
          ROUND((((pm.Price * SD.Months) * sm.DiscountPercentage) / 100),0) as DiscountPrice,                   
          ROUND((pm.Price * SD.Months) - (((pm.Price * SD.Months) * sm.DiscountPercentage) / 100), 0) as NetPayment,                     
           '' as CouponCode,                    
           DATEADD(MONTH, sd.Months, GETDATE()) as ExpireOn,                    
           -- sm.IsActive AS SubscriptionMappingActive,                                     
           sd.Id AS SubscriptionDurationId,                    
           CAST(sd.Months as varchar) + iif(CAST(sd.Months as int) = 1, ' Month', ' Months') AS SubscriptionDurationName,                    
           sd.Months,                    
           CAST(0 as bit) IsRecommended,                    
          -- CAST(FORMAT(((pm.Price - ((pm.Price * sm.DiscountPercentage) / 100)) / sd.Months), 'N2') AS VARCHAR) + '/m' as PerMonth,                   
  CAST(CEILING(
    ((pm.Price * SD.Months) - ((pm.Price * SD.Months) * sm.DiscountPercentage) / 100) / SD.Months
) AS VARCHAR) + '/m' AS PerMonth,                  
                  
           sm.Id AS SubscriptionMappingId                    
    FROM SubscriptionMappingM sm                    
        INNER JOIN SubscriptionDurationM sd                    
            ON sm.SubscriptionDurationId = sd.Id                    
        INNER JOIN SubscriptionPlanM sp                    
            ON sm.SubscriptionPlanId = sp.Id                    
        INNER JOIN ProductsM as pm                    
            on pm.Id = sm.ProductId                    
    WHERE ISNULL(sp.IsActive, 1) = 1                    
          AND ISNULL(sd.IsActive, 1) = 1                    
          AND ISNULL(sm.IsActive, 1) = 1                    
          AND (                    
                  sm.SubscriptionPlanId = @SubscriptionPlanId                    
                  OR @SubscriptionPlanId IS NULL                    
              )                    
          AND (                    
                  sm.ProductId = @ProductId                    
                  OR @ProductId IS NULL                    
              )                    
          AND LOWER(@DeviceType) IN (SELECT VALUE FROM string_split('android,ios', ','))                    
                    
         ORDER BY SD.Months                     
END

-------------Ajith 02-06-2025 5:10 PM
--exec GetTargetAudianceListForPushNotification 'PURCHASEDKALKIBAATAAJ','testing','',null   

Go
ALTER PROCEDURE GetTargetAudianceListForPushNotification
    @AudianceCategory VARCHAR(50),
    @topic VARCHAR(50),
    @mobile VARCHAR(MAX),
    -- Use MAX to handle multiple numbers           
    @ProductId int = 0 ,
    @FromDate DATE = NULL,
    @ToDate DATE = NULL
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
        INSERT INTO @MobileList
            (Mobile)
        SELECT value
        FROM STRING_SPLIT(@mobile, ',');
    END

    -- Fetch latest app versions         
    DECLARE @LatestAndroidVersion NVARCHAR(50), @LatestIosVersion NVARCHAR(50);
    SELECT @LatestIosVersion = Value
    FROM Settings
    WHERE Code = 'IosCurrentVersion';
    SELECT @LatestAndroidVersion = Value
    FROM Settings
    WHERE Code = 'AndroidCurrentVersion';


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
            AND ( 
           EXISTS (   
               SELECT 1
            FROM PurchaseOrdersM pom
            WHERE pom.ActionBy = mu.PublicKey
                AND pom.TransasctionReference <> 'WITHOUTPAYMENT'   
           )
            OR EXISTS ( 
               SELECT 1
            FROM MYBucketM mb
            WHERE mb.MobileUserKey = mu.PublicKey 
           ) 
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
           SELECT 1
            FROM PurchaseOrdersM pom
            WHERE pom.ActionBy = mu.PublicKey
                AND pom.TransasctionReference <> 'WITHOUTPAYMENT'   
       )
            AND NOT EXISTS ( 
           SELECT 1
            FROM MYBucketM mb
            WHERE mb.MobileUserKey = mu.PublicKey 
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
                  SELECT 1
            FROM @MobileList ml
            WHERE ml.Mobile = mu.Mobile                       
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
  ELSE IF UPPER(TRIM(@AudianceCategory)) = 'PAYMENTPENDINGUSERS'
BEGIN
        INSERT INTO @MobileUsers
        SELECT DISTINCT
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
            INNER JOIN PaymentRequestStatusM pr ON pr.CreatedBy = mu.PublicKey
        WHERE ISNULL(mu.isActive, 1) = 1
            AND ISNULL(mu.IsDelete, 0) = 0
            AND (@FromDate IS NULL OR CAST(pr.CreatedOn AS DATE) >= @FromDate)
            AND (@ToDate IS NULL OR CAST(pr.CreatedOn AS DATE) <= @ToDate)
            AND UPPER(pr.[Status]) IN ('FAILED', 'PENDING')
            AND (
            @mobile IS NULL
            OR mu.Mobile = @mobile
            OR EXISTS (
                SELECT 1
            FROM @MobileList ml
            WHERE ml.Mobile = mu.Mobile
            )
        );
    END


    IF @ProductId IS NOT NULL   
BEGIN
        UPDATE @MobileUsers   
    SET notification = 0   
    WHERE PublicKey IN (   
        SELECT mb.MobileUserKey
        FROM MYBucketM mb
        WHERE mb.ProductId = @ProductId
            AND mb.Notification = 0   
    );
    END


    SELECT *
    FROM @MobileUsers;
END;   

------ Siva 2 June 2025 ------------------------
GO
ALTER PROCEDURE ManageCouponM                  
    @couponName VARCHAR(100),                  
    @couponKey uniqueidentifier,                  
    @description NVARCHAR(500),                  
    @discountInPercentage int = NULL,                  
    @discountInPrice DECIMAL(10, 2) = NULL,                  
    @redeemLimit INT,                  
    @productValidityInDays INT null,                  
    @createdBy UNIQUEIDENTIFIER,                  
    @productIds NVARCHAR(MAX) = NULL,                  
    @mobileNumbers varchar(MAX) = NULL, -- csv mobileNumbers                      
    @action varchar(50),                  
    @override bit = 0                  
AS                  
BEGIN                  
                     
    DECLARE @newCouponId INT,                  
            @result varchar(100);                  
    IF UPPER(@action) = 'CREATE'                  
       OR UPPER(@action) = 'REGENERATE'                  
    BEGIN                  
        IF @override = 0                  
        BEGIN                  
            IF EXISTS                  
            (                  
                select 1                  
                from couponsm                  
                where                     
       Name = @couponName                  
            )                  
            BEGIN                  
                SELECT 'DUPLICATENAME' as Result;                  
                RETURN;                  
            END                  
        END                  
        ELSE                  
        BEGIN                  
            UPDATE couponsm                  
            SET IsActive = 0,                  
                IsDelete = 1                  
            WHERE name = @couponName                  
        END                  
        INSERT INTO [dbo].[CouponsM]                  
        (                  
            [Name],                  
            [PublicKey],                  
            [Description],                  
            [DiscountInPercentage],                  
            [DiscountInPrice],                  
            [TotalRedeems],                  
            [RedeemLimit],                  
            [ProductValidityInDays],                  
            [IsActive],                  
            [IsDelete],                  
            [CreatedOn],                  
            [CreatedBy]                  
        )                  
        VALUES                  
        (@couponName,                  
         newId(),                  
         @description,                  
         @discountInPercentage,                  
         @discountInPrice,                  
         0  ,                  
         @redeemLimit,                  
         @productValidityInDays,                  
         1  ,                  
         0  ,                  
         getDate(),                  
         @createdBy                  
        )                  
        SET @newCouponId = SCOPE_IDENTITY();                  
                  
        -- insert into coupon product mapping                          
        IF @productIds IS NULL                  
        BEGIN                  
            INSERT INTO CouponProductMappingM                  
            (                  
                CouponId,                  
                ProductId                  
            )                  
            values                  
            (@newCouponId, null)                  
        END                  
        ELSE                  
        BEGIN                  
            INSERT INTO CouponProductMappingM                  
            (                  
                CouponId,                  
                ProductId                  
            )                  
            SELECT @newCouponId,                  
                   value                  
            FROM string_split(@productIds, ',')                  
        END                  
        -- insert into coupon user mapping                          
        IF @mobileNumbers IS NULL                        BEGIN                  
            INSERT INTO CouponUserMappingM                  
   (                  
                CouponId,                  
                MobileUserKey                  
            )                  
            VALUES                  
            (@newCouponId, NULL);             
        END                  
        ELSE                  
        BEGIN                  
            INSERT INTO CouponUserMappingM                  
            (                  
                CouponId,                  
                MobileUserKey                  
            )                  
            SELECT @newCouponId,                  
                   MU.PublicKey                  
            FROM string_split(@mobileNumbers, ',') AS ss                  
                INNER JOIN MobileUsers MU                  
                    ON MU.mobile = LTRIM(RTRIM(ss.value))      where isActive = 1 and IsDelete = 0            
        END                  
        set @result = 'CREATESUCCESSFULL';                  
    END                  
    ELSE IF UPPER(@action) = 'EDIT'   
BEGIN   
    -- Check if coupon name is changed and it has 0 Total Redeems    
    IF EXISTS   
    (   
        Select 1   
        from couponsm   
        where PublicKey = @couponKey   
              and TotalRedeems > 0   
              and @couponName <> name   
    )   
    BEGIN   
        SET @result = 'CANNOTEDITNAME';   
        SELECT @result AS Result;   
        RETURN;   
    END   
     IF EXISTS (
            SELECT 1
            FROM CouponsM
            WHERE Name = @couponName
              AND PublicKey <> @couponKey
              AND IsDelete = 0
        )
        BEGIN
            SET @result = 'DUPLICATENAME';
            SELECT @result AS Result;
            RETURN;
        END   
    BEGIN   
        -- Update coupon details   
        UPDATE CouponsM   
        SET Description = @description,   
            DiscountInPercentage = @discountInPercentage,   
            DiscountInPrice = @discountInPrice,   
            RedeemLimit = @redeemLimit,   
            ProductValidityInDays = @productValidityInDays,   
            CreatedBy = @createdBy,   
            ModifiedOn = GETDATE(),   
            Name = @couponName   
        WHERE PublicKey = @couponKey   
   
        -- Get the new coupon ID   
        SELECT @newCouponId = id FROM CouponsM WHERE PublicKey = @couponKey   
   
        -- Delete unselected products (remove old mappings for products not in @productIds)   
        IF @productIds IS NOT NULL   
        BEGIN   
            -- Delete products that were previously associated with this coupon but are not in the new selection   
            DELETE FROM CouponProductMappingM   
            WHERE CouponId = @newCouponId   
                  AND ProductId NOT IN (SELECT value FROM string_split(@productIds, ','))   
        END   
   
        -- Insert new selected products into coupon product mapping   
        IF @productIds IS NOT NULL   
        BEGIN   
          DELETE FROM CouponProductMappingM   
            WHERE CouponId = @newCouponId   
               
            INSERT INTO CouponProductMappingM   
            (CouponId, ProductId)   
            SELECT @newCouponId, value   
            FROM string_split(@productIds, ',') AS product   
            WHERE NOT EXISTS   
            (   
                SELECT 1   
                FROM CouponProductMappingM   
                WHERE CouponId = @newCouponId   
                      AND ProductId = product.value   
            )   
        END   
         ELSE                 
        BEGIN    
         DELETE FROM CouponProductMappingM   
            WHERE CouponId = @newCouponId                 
            INSERT INTO CouponProductMappingM                  
            (                  
                CouponId,                  
                ProductId                  
            )                  
            values                  
            (@newCouponId, null)                  
        END                  
   
        -- If @mobileNumbers is NULL or empty string, remove all mappings for the given CouponId   
        IF @mobileNumbers IS NULL   
        BEGIN   
            DELETE FROM CouponUserMappingM   
            WHERE CouponId = @newCouponId;   
   
            -- If no mobile number is provided, insert a null mapping   
            IF NOT EXISTS   
            (   
                SELECT 1   
                FROM CouponUserMappingM   
                WHERE CouponId = @newCouponId   
                AND MobileUserKey IS NULL   
            )   
            BEGIN   
                INSERT INTO CouponUserMappingM   
                (CouponId, MobileUserKey)   
                VALUES (@newCouponId, NULL);   
            END   
        END   
        ELSE   
        BEGIN   
              DELETE FROM CouponUserMappingM   
                  WHERE CouponId = @newCouponId   
                  AND MobileUserKey is null  
                    
         -- If mobile numbers are provided, remove mappings for users not in the provided list   
            DELETE FROM CouponUserMappingM   
            WHERE CouponId = @newCouponId   
                  AND MobileUserKey NOT IN (   
                      SELECT MU.PublicKey   
                      FROM string_split(@mobileNumbers, ',') AS ss   
                      INNER JOIN MobileUsers MU   
                      ON MU.mobile = LTRIM(RTRIM(ss.value))   
                      WHERE isActive = 1 AND IsDelete = 0   
                  );   
   
            -- Add mappings for new users in the provided list   
            INSERT INTO CouponUserMappingM   
            (CouponId, MobileUserKey)   
            SELECT @newCouponId, MU.PublicKey   
            FROM string_split(@mobileNumbers, ',') AS ss   
            INNER JOIN MobileUsers MU   
            ON MU.mobile = LTRIM(RTRIM(ss.value))   
            WHERE isActive = 1 AND IsDelete = 0   
            AND NOT EXISTS   
            (   
                SELECT 1   
                FROM CouponUserMappingM   
                WHERE CouponId = @newCouponId   
                      AND MobileUserKey = MU.PublicKey   
            );   
        END   
   
        -- Return success message   
        SET @result = 'UPDATED';   
        SELECT @result AS Result;   
    END   
END                  
   ELSE IF UPPER(@action)='TOGGLE'          
            BEGIN          
             if exists (select 1 from couponsm where publickey=@couponKey)          
    BEGIN          
                 update couponsm          
                 set isactive=CASE WHEN isActive=1 THEN 0 ELSE 1 END, modifiedOn=GETDATE()          
                 where publickey=@couponKey          
                 set @result='UPDATED'          
             END          
    END          
    SELECT @result AS Result;          
 END 

 --Modify By Siva 3 June 2025 1:13 PM ---------------
 GO
 ALTER PROCEDURE GetTicketsM                    
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
                   mu2.FirstName+''+mu2.LastName AS CommentByCrmUserName                    
            FROM TicketCommentsM tcm                    
                LEFT JOIN mobileusers mu1                    
                    ON mu1.Id = tcm.CommentMobileUserId                    
                LEFT JOIN Users mu2                    
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
           AND (@startDate IS NULL OR CAST(t.CreatedOn AS DATE) >= @startDate)
AND (@endDate IS NULL OR CAST(t.CreatedOn AS DATE) <= @endDate)
               
                   
                    
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
    SELECT ROW_NUMBER() OVER (ORDER BY ISNULL(t.CreatedOn,t.modifiedOn) DESC) AS SlNo,                    
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
		   AND (@startDate IS NULL OR CAST(t.CreatedOn AS DATE) >= @startDate)
AND (@endDate IS NULL OR CAST(t.CreatedOn AS DATE) <= @endDate)
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
 -----------------Ajith 03-06-2025 5:00 PM
 GO
 ALTER PROCEDURE GetSubscriptionPlanWithProduct                     
    @ProductId INT = 1,                     
    @SubscriptionPlanId int = 25,                     
    @MobileUserKey uniqueidentifier = null,                     
    @DeviceType VARCHAR(15) = 'android'                     
AS                     
BEGIN                     
                     
    SELECT sp.Id AS SubscriptionPlanId,                     
           sp.Name AS PlanName,                     
           sp.Description AS PlanDescription,                     
           sm.ProductId,                     
           pm.Name as ProductName,                     
           (pm.Price * SD.Months) as ActualPrice,                     
          ROUND((((pm.Price * SD.Months) * sm.DiscountPercentage) / 100),0) as DiscountPrice,                    
          ROUND((pm.Price * SD.Months) - (((pm.Price * SD.Months) * sm.DiscountPercentage) / 100), 0) as NetPayment,                      
           '' as CouponCode,                     
           DATEADD(MONTH, sd.Months, GETDATE()) as ExpireOn,                     
           -- sm.IsActive AS SubscriptionMappingActive,                                      
           sd.Id AS SubscriptionDurationId,                     
           CAST(sd.Months as varchar) + iif(CAST(sd.Months as int) = 1, ' Month', ' Months') AS SubscriptionDurationName,                     
           sd.Months,                     
           CAST(0 as bit) IsRecommended,                     
          -- CAST(FORMAT(((pm.Price - ((pm.Price * sm.DiscountPercentage) / 100)) / sd.Months), 'N2') AS VARCHAR) + '/m' as PerMonth,                    
  CAST(FLOOR( 
    ((pm.Price * SD.Months) - ((pm.Price * SD.Months) * sm.DiscountPercentage) / 100) / SD.Months 
) AS VARCHAR) + '/m' AS PerMonth,                 
                   
           sm.Id AS SubscriptionMappingId                     
    FROM SubscriptionMappingM sm                     
        INNER JOIN SubscriptionDurationM sd                     
            ON sm.SubscriptionDurationId = sd.Id                     
        INNER JOIN SubscriptionPlanM sp                     
            ON sm.SubscriptionPlanId = sp.Id                     
        INNER JOIN ProductsM as pm                     
            on pm.Id = sm.ProductId                     
    WHERE ISNULL(sp.IsActive, 1) = 1                     
          AND ISNULL(sd.IsActive, 1) = 1                     
          AND ISNULL(sm.IsActive, 1) = 1                     
          AND (                     
                  sm.SubscriptionPlanId = @SubscriptionPlanId                     
                  OR @SubscriptionPlanId IS NULL                     
              )                     
          AND (                     
                  sm.ProductId = @ProductId                     
                  OR @ProductId IS NULL                     
              )                     
          AND LOWER(@DeviceType) IN (SELECT VALUE FROM string_split('android,ios', ','))                     
                     
         ORDER BY SD.Months                      
END