BEGIN TRY
    -- Step 1: Backup the table into ScheduledNotificationMTempBackup
    SELECT * INTO ScheduledNotificationMTempBackup FROM ScheduledNotificationM;

    -- Step 2: Truncate original table
    TRUNCATE TABLE ScheduledNotificationM;

    -- Step 3: Alter Columns to NOT NULL
    -- ScheduledTime
    IF EXISTS (
        SELECT 1 FROM sys.columns 
        WHERE Name = N'ScheduledTime' AND Object_ID = OBJECT_ID(N'dbo.ScheduledNotificationM')
    )
    BEGIN
        ALTER TABLE dbo.ScheduledNotificationM
        ALTER COLUMN ScheduledTime DATETIME NOT NULL;
    END;

    -- AllowRepeat
    IF EXISTS (
        SELECT 1 FROM sys.columns 
        WHERE Name = N'AllowRepeat' AND Object_ID = OBJECT_ID(N'dbo.ScheduledNotificationM')
    )
    BEGIN
        ALTER TABLE dbo.ScheduledNotificationM
        ALTER COLUMN AllowRepeat BIT NOT NULL;
    END;

    -- IsSent
    IF EXISTS (
        SELECT 1 FROM sys.columns 
        WHERE Name = N'IsSent' AND Object_ID = OBJECT_ID(N'dbo.ScheduledNotificationM')
    )
    BEGIN
        ALTER TABLE dbo.ScheduledNotificationM
        ALTER COLUMN IsSent BIT NOT NULL;
    END;

    -- CreatedBy
    IF EXISTS (
        SELECT 1 FROM sys.columns 
        WHERE Name = N'CreatedBy' AND Object_ID = OBJECT_ID(N'dbo.ScheduledNotificationM')
    )
    BEGIN
        ALTER TABLE dbo.ScheduledNotificationM
        ALTER COLUMN CreatedBy BIGINT NOT NULL;
    END;

    -- CreatedOn
    IF EXISTS (
        SELECT 1 FROM sys.columns 
        WHERE Name = N'CreatedOn' AND Object_ID = OBJECT_ID(N'dbo.ScheduledNotificationM')
    )
    BEGIN
        ALTER TABLE dbo.ScheduledNotificationM
        ALTER COLUMN CreatedOn DATETIME NOT NULL;
    END;

    -- IsActive
    IF EXISTS (
        SELECT 1 FROM sys.columns 
        WHERE Name = N'IsActive' AND Object_ID = OBJECT_ID(N'dbo.ScheduledNotificationM')
    )
    BEGIN
        ALTER TABLE dbo.ScheduledNotificationM
        ALTER COLUMN IsActive BIT NOT NULL;
    END;

    -- Step 4: Re-insert data from backup
    INSERT INTO ScheduledNotificationM (
         Title, Body, Topic, LandingScreen, TargetAudience, ScheduledTime, AllowRepeat,
         IsSent, CreatedBy, ModifiedBy, CreatedOn, ModifiedOn, ScheduledEndTime, IsActive,
         Image, ProductId, MobileNumber
    )
    SELECT 
        Title, Body, Topic, LandingScreen, TargetAudience, ScheduledTime, AllowRepeat,
        IsSent, CreatedBy, ModifiedBy, CreatedOn, ModifiedOn, ScheduledEndTime, ISNULL(IsActive, 0),
        Image, ProductId, MobileNumber
    FROM ScheduledNotificationMTempBackup
    ORDER BY Id;




    PRINT 'Table altered and data restored successfully.';
END TRY
BEGIN CATCH
    PRINT 'Error occurred!';
    PRINT ERROR_MESSAGE();
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS NVARCHAR);
    PRINT 'Severity: ' + CAST(ERROR_SEVERITY() AS NVARCHAR);
    PRINT 'State: ' + CAST(ERROR_STATE() AS NVARCHAR);
    PRINT 'Line: ' + CAST(ERROR_LINE() AS NVARCHAR);
    PRINT 'Procedure: ' + ISNULL(ERROR_PROCEDURE(), 'N/A');
    
    -- Optionally rollback if used in a transaction scope
    ROLLBACK TRANSACTION;
END CATCH
--- Modify by siva 12 June 2025 5:11 PM 
GO


    --Modified By Chaitanya 16 June 11:34PM -
GO
USE [KingresearchTest]
GO
/****** Object:  StoredProcedure [dbo].[GetMobileUserBucketDetails]    Script Date: 16-06-2025 09:36:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[GetMobileUserBucketDetails] 
    @IsPaging BIT = 1,
    @PageSize INT = 10,
    @PageNumber INT = 1,
    @SearchText VARCHAR(100) = NULL,
    @DaysToGo INT = NULL,
	@ProductId INT = NULL,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

	    -- Temp table to hold the filtered results before pagination
		IF OBJECT_ID('tempdb..#FilteredResults') IS NOT NULL
			DROP TABLE #FilteredResults;

		Declare @Islessthan5days bit = iif(@DaysToGo=5,1,0)

        SELECT 
            ROW_NUMBER() OVER (ORDER BY DATEDIFF(DAY, GETDATE(), MB.EndDate)) AS SNo,
            MU.FullName,
            MU.Mobile AS MobileNumber,
            MU.EmailId AS EmailID,
            MB.ProductName,
            MB.ProductID,
            MB.StartDate,
            MB.EndDate,
            DATEDIFF(DAY, GETDATE(), MB.EndDate) AS DaysToGo,
            MU.FirebaseFcmToken,
            MU.PublicKey,
            CASE
                WHEN MU.DeviceVersion IS NULL THEN 1
                WHEN MU.DeviceType = 'iOS' AND
                     TRY_CAST(PARSENAME(MU.DeviceVersion, 3) AS INT) = 1 AND
                     TRY_CAST(PARSENAME(MU.DeviceVersion, 2) AS INT) = 0 AND
                     TRY_CAST(PARSENAME(MU.DeviceVersion, 1) AS INT) < 15 THEN 1
                WHEN MU.DeviceType = 'Android' AND
                     TRY_CAST(PARSENAME(MU.DeviceVersion, 3) AS INT) = 1 AND
                     TRY_CAST(PARSENAME(MU.DeviceVersion, 2) AS INT) = 0 AND
                     TRY_CAST(PARSENAME(MU.DeviceVersion, 1) AS INT) < 57 THEN 1
                ELSE 0
            END AS OldDevice,
            0 AS Notification
		INTO #FilteredResults
        FROM MobileUsers MU
        LEFT JOIN MYBucketM MB ON MU.PublicKey = MB.MobileUserKey
        WHERE 
            MU.IsActive = 1
            AND ISNULL(MB.IsExpired, 0) = 0
            AND MB.EndDate > DATEADD(DAY, 1, GETDATE())
			AND (@ProductID IS NULL OR MB.ProductId = @ProductId)
            AND (
                @SearchText IS NULL
                OR MU.FullName LIKE '%' + @SearchText + '%'
                OR MU.Mobile LIKE '%' + @SearchText + '%'
                OR MU.EmailId LIKE '%' + @SearchText + '%'
            )
            AND (
                @DaysToGo IS NULL 
                OR (DATEDIFF(DAY, GETDATE(), MB.EndDate) = @DaysToGo AND @Islessthan5days =0)
				OR (DATEDIFF(DAY, GETDATE(), MB.EndDate) <= @DaysToGo-1 AND @Islessthan5days =1)
            )


    -- Output total count
    SELECT @TotalCount = COUNT(*) FROM #FilteredResults;

    -- Apply pagination
    SELECT *
    FROM #FilteredResults
    ORDER BY DaysToGo
    OFFSET CASE WHEN @IsPaging = 1 THEN (@PageNumber - 1) * @PageSize ELSE 0 END ROWS
    FETCH NEXT CASE WHEN @IsPaging = 1 THEN @PageSize ELSE 1000000 END ROWS ONLY;
END


    -------------Ajith 13-06-2025 4:47 PM

GO
--EXEC GetUserHistory @PageNumber = 1, @PageSize = 5000, @PublicKey = 'ba0c9cce-6a8f-ef11-819c-00155d23d79c';         
       
ALTER PROCEDURE GetUserHistory   
   @PageNumber INT = 1,   
   -- Page number (default is 1)      
   @PageSize INT = 20,   
   -- Number of records per page (default is 20)      
   @PublicKey VARCHAR(50)   
-- Mobile number for filtering      
AS   
BEGIN   
   SET NOCOUNT ON;   
   
   -- Common Table Expression (CTE) for Mobile Users Data      
   WITH   
      CTE_MobileUsers   
      AS   
      (   
         SELECT   
            MU.FullName,   
            MU.EmailId,   
            MU.PublicKey,   
            MU.LastLoginDate,   
            MU.Mobile,
            MU.DeviceVersion,   
            -- Determine ticket status      
            CASE      
                WHEN EXISTS (SELECT 1   
            FROM TicketM T   
            WHERE T.CreatedBy = MU.PublicKey AND LOWER(T.Status) LIKE '%o%' AND T.IsActive = 1 AND T.IsDelete = 0) THEN 'Open'      
                ELSE 'Close'      
            END AS TicketOpenStatus,   
            MU.FirebaseFcmToken,   
            CASE      
                WHEN LOWER(MU.DeviceType) LIKE '%and%' THEN 'and'      
                WHEN LOWER(MU.DeviceType) LIKE '%ios%' THEN 'ios'      
                ELSE 'unknown'      
            END AS DeviceType,   
            MU.IsActive,   
            MU.Gender,   
            MU.CanCommunityPost,   
            MU.RegistrationDate,   
            MU.ModifiedOn,   
   
            -- Bucket data aggregation      
            COUNT(DISTINCT MB.Id) AS BucketCount,   
            (      
    SELECT   
               MU.PublicKey AS MobileUserPublicKey,   
               MB.ID,   
               MB.ProductId AS BucketProductId,   
               P.Name As ProductName,   
               MB.CreatedDate,   
               MB.EndDate,   
               MB.StartDate,   
               MB.ModifiedDate AS BucketModifiedDate,   
               CASE     
        WHEN GETDATE() < MB.StartDate THEN DATEDIFF(DAY, MB.StartDate, MB.EndDate) -- Future Start Date     
        ELSE DATEDIFF(DAY, GETDATE(), MB.EndDate) -- Remaining days till EndDate     
    END AS ValidityInDays,   
               --   DATEDIFF(DAY, GETDATE(), MB.EndDate) AS ValidityInDays,      
               -- Include count of ReasonChangeLog entries for each BucketId      
               (SELECT COUNT(*)   
               FROM ReasonChangeLog RCL   
               WHERE RCL.MyBucketId = MB.ID) AS ReasonChangeLogCount   
            FROM MYBucketM MB   
            LEFT JOIN ProductSM P ON MB.ProductId = P.Id  
            WHERE MB.MobileUserKey = @PublicKey   
            ORDER BY MB.ModifiedDate DESC   
            FOR JSON PATH      
) AS BucketData,   
   
   
            -- Purchase data aggregation      
            COUNT(DISTINCT PO.Id) AS PurchaseCount,   
            (      
                SELECT   
               MU.PublicKey,   
               PO.ID,   
               PO.ProductId AS PurchaseProductId,   
               P.Name AS ProductName,   
               PO.PaidAmount,   
               PO.NetAmount,   
               PO.StartDate,   
               PO.EndDate,   
               PO.PaymentDate,   
               PO.TransactionId   
            FROM PurchaseOrdersM PO   
               LEFT JOIN ProductsM P ON PO.ProductId = P.Id   
            WHERE PO.ActionBy = MU.PublicKey and PO.IsActive = 1   
            FOR JSON PATH      
            ) AS PurchaseData,   
   
            -- Free trial data aggregation      
            COUNT(DISTINCT FT.Id) AS FreeTrialCount,   
            (      
                SELECT   
 FT.EndDate,   
               FT.StartDate,   
               STRING_AGG(P.Name, ', ') AS ProductNames   
            FROM FreeTrialBasketDetailM FTBD   
               JOIN ProductsM P ON FTBD.ProductId = P.Id   
            WHERE FTBD.FreeTrialBasketId IN (SELECT FreeTrialBasketId   
            FROM FreeTrialM   
            WHERE MobileUserKey = MU.PublicKey)   
            FOR JSON PATH      
            ) AS FreeTrailProductNames,   
   
            CASE       
             WHEN FT.MobileUserKey IS NULL THEN 'Not Activated'       
        WHEN DATEDIFF(DAY, FT.EndDate, GETDATE()) < 0 THEN 'Active'       
        ELSE 'Expired'       
    END       
AS FreeTrailStatus,   
            (      
                SELECT   
               FT.EndDate AS FreeTrialEndDate,   
               FT.StartDate AS FreeTrialStartDate,   
               P.Name AS ProductName   
            FROM FreeTrialM FT   
               LEFT JOIN FreeTrialBasketDetailM FTBD ON FT.FreeTrialBasketId = FTBD.FreeTrialBasketId   
               LEFT JOIN ProductsM P ON FTBD.ProductId = P.Id   
            WHERE FT.MobileUserKey = MU.PublicKey   
            FOR JSON PATH      
            ) AS FreeTrailData   
   
         FROM MobileUsers MU   
            LEFT JOIN MYBucketM MB ON MU.PublicKey = MB.MobileUserKey   
            LEFT JOIN PurchaseOrdersM PO ON MU.PublicKey = PO.ActionBy   
            LEFT JOIN FreeTrialM FT ON MU.PublicKey = FT.MobileUserKey   
            LEFT JOIN TicketM T ON MU.PublicKey = T.CreatedBy   
         GROUP BY      
            MU.FullName,      
            MU.EmailId,      
            MU.Mobile,      
            MU.FirebaseFcmToken,      
            MU.DeviceType,      
            MU.IsActive,      
            MU.Gender,      
            MU.CanCommunityPost,      
            MU.RegistrationDate,      
            MU.PublicKey,      
            FT.IsActive,      
            FT.StartDate,      
            FT.EndDate,      
            MU.ModifiedOn,      
            MU.LastLoginDate,     
            FT.MobileUserKey,
            MU.DeviceVersion   
      )   
   -- Fetch paginated data      
   SELECT *   
   FROM CTE_MobileUsers   
   WHERE PublicKey = @PublicKey   
   ORDER BY FullName      
    OFFSET (@PageNumber - 1) * @PageSize ROWS      
    FETCH NEXT @PageSize ROWS ONLY;   
END;      

GO
 --exec [GetActivityLogs] 100,1,'null','2025-05-14' ,'2025-06-13', null  ,null              
ALTER PROCEDURE GetActivityLogs       
 @PageSize int = 10,                     
 @PageNumber int = 1,     
 @PrimaryKey varchar(50) = null,        
 @FromDate datetime = null,                    
 @ToDate datetime = null,                    
 @SearchText varchar(100) = null,                 
 @TotalCount INT OUTPUT              
AS  
BEGIN      
    -- Set default search text to empty string 
    SET @SearchText = ISNULL(@SearchText, '') 
 
    -- Select total count for pagination purposes 
    SELECT @TotalCount = COUNT(1) 
    FROM LeadActivity as la   
    INNER JOIN ActivityType as aty ON la.ActivityType = aty.Id   
    INNER JOIN Leads le ON la.LeadKey = le.PublicKey   
    INNER JOIN Users as us ON la.CreatedBy = us.PublicKey   
    LEFT JOIN Users as lsource ON lsource.PublicKey = la.Source  
    LEFT JOIN Users as ldestination ON ldestination.PublicKey = la.Destination   
    WHERE  la.CreatedBy = ISNULL(@PrimaryKey, la.CreatedBy)     
    AND ( 
        le.FullName LIKE '%' + @SearchText + '%' OR 
        le.MobileNumber LIKE '%' + @SearchText + '%' 
    ) 
    AND ( 
        (@FromDate IS NULL AND @ToDate IS NULL) OR 
        (CAST(la.CreatedOn AS DATE) BETWEEN CAST(ISNULL(@FromDate, DATEADD(MONTH, -1, GETDATE())) AS DATE) 
        AND CAST(ISNULL(@ToDate, GETDATE()) AS DATETIME)) 
    ) 
 
    -- Main data query with pagination 
    SELECT   
        ROW_NUMBER() OVER (ORDER BY la.CreatedOn DESC) AS SlNo,  
        le.FullName AS LeadName,     
        aty.ActivityMessage AS ActivityType,      
        CASE     
            WHEN aty.ActivityMessage = 'Lead allocated'     
            THEN aty.ActivityMessage + ' from ' + ISNULL(lsource.FirstName, 'no_one') + ' to ' + ldestination.FirstName    
            ELSE aty.ActivityMessage      
        END AS Message,    
        le.MobileNumber,   
        la.CreatedOn,   
        us.FirstName AS CreatedBy   
    FROM LeadActivity AS la   
    INNER JOIN ActivityType AS aty ON la.ActivityType = aty.Id   
    INNER JOIN Leads le ON la.LeadKey = le.PublicKey   
    INNER JOIN Users as us ON la.CreatedBy = us.PublicKey   
    LEFT JOIN Users as lsource ON lsource.PublicKey = la.Source  
    LEFT JOIN Users as ldestination ON ldestination.PublicKey = la.Destination   
    WHERE la.CreatedBy = ISNULL(@PrimaryKey, la.CreatedBy)   
    AND ( 
        le.FullName LIKE '%' + @SearchText + '%' OR 
        le.MobileNumber LIKE '%' + @SearchText + '%' 
    ) 
    AND ( 
        (@FromDate IS NULL AND @ToDate IS NULL) OR 
        (CAST(la.CreatedOn AS DATE) BETWEEN CAST(ISNULL(@FromDate, DATEADD(MONTH, -1, GETDATE())) AS DATE) 
        AND CAST(ISNULL(@ToDate, GETDATE()) AS DATETIME)) 
    ) 
    ORDER BY la.CreatedOn DESC    
    OFFSET (@PageNumber - 1) * @PageSize ROWS   
    FETCH NEXT @PageSize ROWS ONLY;   
END 

Go 
--Modified By Chaitanya 13 June 05:55PM -

--DECLARE @TotalCount INT;
--DECLARE @LTCCount INT;
--EXEC Sp_Get_Leads 
--    @IsPaging = 0,@PageSize = 10,@PageNumber = 1, @SortExpression = 'CreatedOn',@SortOrder = 'desc',@FromDate = '2020-06-13', @ToDate = '2025-06-13',@PrimaryKey = NULL,@SecondaryKey = NULL,@ThirdKey = NULL,@FourthKey = NULL,@FifthKey = NULL,@CreatedBy = NULL,@AssignedTo = '1F1CCA06-1535-EE11-811E-00155D23D79C',@LoggedInUser = '1F1CCA06-1535-EE11-811E-00155D23D79C',@RoleKey = '45309f6e-f124-41f0-b094-b53f047c8603',@SearchText = NULL, 
--    @TotalCount = @TotalCount OUTPUT,       
--    @LTCCount = @LTCCount OUTPUT;

-- Modify By Siva 13 June 2025 1:14 PM -----------
GO
ALTER PROCEDURE [dbo].[GetAnalysts]
    @Category VARCHAR(50) = 'analyst'
AS
BEGIN
    SET NOCOUNT ON;

    DROP TABLE IF EXISTS #TEMPANALYSTS;
    DROP TABLE IF EXISTS #TEMPANALYSTS2;

    IF (@Category = 'analyst' OR @Category = 'bde' OR @Category = 'Sales Lead')
    BEGIN
        SELECT u.id, u.firstName, u.lastName, u.publicKey
        INTO #TEMPANALYSTS2
        FROM Users AS u
        INNER JOIN UserMappings AS r ON u.PublicKey = r.UserKey
        WHERE r.UserType = @Category
          AND ISNULL(u.IsDelete, 0) = 0
          AND ISNULL(r.IsActive, 1) = 1;

        -- RETURN rows directly, not JSON
        SELECT * FROM #TEMPANALYSTS2 ORDER BY firstName;

        DROP TABLE IF EXISTS #TEMPANALYSTS2;
    END
    ELSE IF (ISNUMERIC(@Category) = 1)
    BEGIN
        SELECT u.id, u.firstName, u.lastName, u.publicKey
        INTO #TEMPANALYSTS
        FROM Users AS u
        INNER JOIN UserMappings AS r ON u.PublicKey = r.UserKey
        WHERE r.UserType = 'bde'
          AND ISNULL(u.IsDelete, 0) = 0
          AND ISNULL(r.IsActive, 1) = 1
          AND (u.SupervisorId = @Category OR u.Id = @Category);

        --  RETURN rows directly, not JSON
        SELECT * FROM #TEMPANALYSTS ORDER BY firstName;

        DROP TABLE IF EXISTS #TEMPANALYSTS;
    END
    ELSE
    BEGIN
        -- Safe fallback
        SELECT CAST(NULL AS NVARCHAR(MAX)) AS firstName;
    END
END
--- Modify By Siva 13 June 2025 5:57PM
GO


ALTER PROCEDURE [dbo].[GetFilterUsersBy]                 
    @userType VARCHAR(50),
    @loginUserId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @loginUserId IS  NULL OR @loginUserId = 83
    BEGIN
        -- Admin or super user
        SELECT u.Id, u.PublicKey, u.FirstName, u.LastName, um.UserType
        FROM Users u
        INNER JOIN UserMappings um ON u.PublicKey = um.UserKey
        WHERE um.UserType = @userType
          AND ISNULL(u.IsDisabled, 0) = 0
          AND um.IsActive = 1
        ORDER BY u.FirstName;
    END
    ELSE
    BEGIN
        -- Sales lead: get only subordinates
        SELECT u.Id, u.PublicKey, u.FirstName, u.LastName, um.UserType
        FROM Users u
        INNER JOIN UserMappings um ON u.PublicKey = um.UserKey
        WHERE um.UserType = @userType
          AND ISNULL(u.IsDisabled, 0) = 0
          AND um.IsActive = 1
          AND (u.SupervisorId = @loginUserId OR  u.Id=@loginUserId)
        ORDER BY u.FirstName;
    END
END

--EXEC GetFilterUsersBy @userType = 'BDE', @loginUserId = 62 



---- MODIFY BY SIVA 17 JUNE 2025 10:46 AM
GO
ALTER PROCEDURE [dbo].[Sp_GetEnquiries]          
 @IsPaging INT = 0,                          
 @PageSize INT = 5,                          
 @PageNumber INT = 1,                          
 @SortExpression VARCHAR(50),                          
 @SortOrder VARCHAR(50),                          
 @FromDate VARCHAR(50) = NULL,                          
 @ToDate VARCHAR(50) = NULL,                                    
 @LoggedInUser VARCHAR(50) = NULL,                              
 @SearchText VARCHAR(250) = NULL,                          
 @TotalCount INT = 0 OUTPUT                 
AS   
BEGIN      
    -- Log the call  
    INSERT INTO LOGS VALUES (@LoggedInUser, 'sp_GetEnquiries', GETDATE())    
  
    -- If the logged-in user is admin, set @LoggedInUser to NULL to allow all records  
  DECLARE @LoggedInUserId INT;    
    DECLARE @RoleName VARCHAR(50)    
    
    -- Get logged in user and role    
    SELECT     
        @LoggedInUserId = u.Id,    
        @RoleName = r.Name    
    FROM Users u    
    INNER JOIN Roles r ON r.PublicKey = u.RoleKey    
    WHERE u.PublicKey = @LoggedInUser;    
 BEGIN    
 IF exists (SELECT 1 fROM Users  as us     
 INNER JOIN Roles as ro on us.RoleKey = ro.PublicKey    
 WHERE us.PublicKey = @LoggedInUser AND ro.Name = 'admin')    
 BEGIN    
  SET @LoggedInUser = null    
 END    
END  
    -- Temporary table for results  
    DROP TABLE IF EXISTS #tempLeads          
  
    SELECT             
        en.Id,   
        en.Details,    
        le.FullName AS LeadName,   
        en.ReferenceKey,   
        u.FirstName + ' ' + u.LastName AS CreatedBy,   
        en.PublickEy,   
        en.CreatedOn,  
        le.MobileNumber,            
        ISNULL(le.Favourite , 0) AS Favourite            
    INTO #tempLeads          
    FROM Enquiries AS en             
    INNER JOIN Leads AS le ON en.ReferenceKey = le.PublicKey             
    INNER JOIN Users AS u ON u.PublicKey = en.CreatedBy            
    WHERE           
    (  
        le.MobileNumber LIKE '%' + @SearchText + '%' OR  
        le.AlternateMobileNumber LIKE '%' + @SearchText + '%' OR  
        le.EmailId LIKE '%' + @SearchText + '%' OR  
        le.FullName LIKE '%' + @SearchText + '%' OR  
        @SearchText IS NULL  
    )           
    AND (( @LoggedInUser IS NULL)  
  
        OR(@RoleName='Sales Lead' AND  en.CreatedBy IN (  
            SELECT PublicKey   
            FROM Users   
            WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser  
        )  
  )  
  OR
  (
    @RoleName NOT IN ('Sales Lead', 'admin') 
    AND en.CreatedBy = @LoggedInUser
  )
    )  
    AND u.IsDisabled = 0  
    ORDER BY le.Favourite DESC            
  
    -- Total count  
    SELECT @TotalCount = COUNT(1) FROM #tempLeads           
  
    -- Paged and sorted results  
    SELECT *   
    FROM #tempLeads                          
    ORDER BY    
        CreatedOn DESC,      
        Favourite DESC,                    
        CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN CreatedOn END ASC,                          
        CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN CreatedOn END DESC,                          
        CASE WHEN @SortExpression = 'Favourite' AND @SortOrder = 'ASC' THEN Favourite END ASC,                          
        CASE WHEN @SortExpression = 'Favourite' AND @SortOrder = 'DESC' THEN Favourite END DESC,                          
        CASE WHEN @SortExpression = 'Name' AND @SortOrder = 'ASC' THEN LeadName END ASC,                          
        CASE WHEN @SortExpression = 'Name' AND @SortOrder = 'DESC' THEN LeadName END DESC                     
    OFFSET (IIF(@PageNumber <= 1, 0, (@PageNumber - 1))) * @PageSize ROWS   
    FETCH NEXT @PageSize ROWS ONLY           
  
    DROP TABLE IF EXISTS #tempLeads          
END  ---MODIFY BY SIVA 16 JUNE 2025 5:26 PM ------------
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
      DECLARE @LoggedInUserId INT;  
    DECLARE @RoleName VARCHAR(50)  
  
    -- Get logged in user and role  
    SELECT   
        @LoggedInUserId = u.Id,  
        @RoleName = r.Name  
    FROM Users u  
    INNER JOIN Roles r ON r.PublicKey = u.RoleKey  
    WHERE u.PublicKey = @LoggedInUser;  
         
            
                 
 SELECT @TotalCount = count(*) ,@TotalAmount = ISNULL(sum(purchaseOrders.PaidAmount),0)            
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
 AND (	(@RoleName = 'admin' AND @RequestedBy IS NULL )
	 OR	(@RoleName = 'Sales Lead' AND @RequestedBy IS NULL AND (Leads.AssignedTo = @LoggedInUser OR Leads.AssignedTo IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId )))
	 OR	(Leads.AssignedTo = @RequestedBy)
	 )
                
   AND CAST(purchaseOrders.paymentDate AS DATE) BETWEEN CAST(ISNULL(@FROMDATE, (DATEADD(MONTH, - 1, getdate()))) AS DATE)                 
    AND CAST(ISNULL(@TODATE, GETDATE()) AS DATETIME)                 
   AND (                 
    leads.FullName LIKE '%' + @SearchText + '%'                 
    or leads.mobilenumber LIKE '%' + @SearchText + '%')                 
  WHERE ISNULL(purchaseOrders.IsExpired, 0) = 0             
     AND((  purchaseOrders.ServiceId in (SELECT CAST(value AS INT)  FROM STRING_SPLIT(@SecondaryKey, ',')  )  and @SecondaryKey is not null )  
	 or( purchaseOrders.ServiceId = purchaseOrders.ServiceId and @SecondaryKey is null)    )                   
         
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
AND ((@RoleName = 'admin' AND @RequestedBy IS NULL)
    OR (@RoleName = 'Sales Lead' AND @RequestedBy IS NULL AND(leads.AssignedTo = @LoggedInUser OR leads.AssignedTo IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId)))
    OR ( leads.AssignedTo = @RequestedBy)
	)
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
---Modify By SIVA 17 JUNE 2025 1:30 PM-------
GO
 --exec [GetActivityLogs] 100,1,'null','2025-05-14' ,'2025-06-13', null  ,null                
ALTER PROCEDURE GetActivityLogs         
 @PageSize int = 10,                       
 @PageNumber int = 1,       
 @PrimaryKey varchar(50) = null,          
 @FromDate datetime = null,                      
 @ToDate datetime = null,
 @LoggedInUser VARCHAR(50) = NULL,  
 @SearchText varchar(100) = null,                   
 @TotalCount INT OUTPUT                
AS    
BEGIN        
    -- Set default search text to empty string   
    SET @SearchText = ISNULL(@SearchText, '')   
   
    DECLARE @LoggedInUserId INT;  
    DECLARE @RoleName VARCHAR(50)  
  
    -- Get logged in user and role  
    SELECT   
        @LoggedInUserId = u.Id,  
        @RoleName = r.Name  
    FROM Users u  
    INNER JOIN Roles r ON r.PublicKey = u.RoleKey  
    WHERE u.PublicKey = @LoggedInUser;  
	BEGIN  
 IF exists (SELECT 1 fROM Users  as us   
 INNER JOIN Roles as ro on us.RoleKey = ro.PublicKey  
 WHERE us.PublicKey = @LoggedInUser AND ro.Name = 'admin')  
 BEGIN  
  SET @LoggedInUser = null  
 END  
END
    -- Select total count for pagination purposes   
    SELECT @TotalCount = COUNT(1)   
    FROM LeadActivity as la     
    INNER JOIN ActivityType as aty ON la.ActivityType = aty.Id     
    INNER JOIN Leads le ON la.LeadKey = le.PublicKey     
    INNER JOIN Users as us ON la.CreatedBy = us.PublicKey     
    LEFT JOIN Users as lsource ON lsource.PublicKey = la.Source    
    LEFT JOIN Users as ldestination ON ldestination.PublicKey = la.Destination     
    WHERE ((@RoleName='admin' AND @LoggedInUser IS NULL)

        OR(@RoleName='Sales Lead' AND  la.CreatedBy IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser))
		
     )  
	 AND la.CreatedBy = ISNULL(@PrimaryKey, la.CreatedBy)            
    AND (   
        le.FullName LIKE '%' + @SearchText + '%' OR   
        le.MobileNumber LIKE '%' + @SearchText + '%'   
    )   
    AND (   
        (@FromDate IS NULL AND @ToDate IS NULL) OR   
        (CAST(la.CreatedOn AS DATE) BETWEEN CAST(ISNULL(@FromDate, DATEADD(MONTH, -1, GETDATE())) AS DATE)   
        AND CAST(ISNULL(@ToDate, GETDATE()) AS DATETIME))   
    )   
   
    -- Main data query with pagination   
    SELECT     
        ROW_NUMBER() OVER (ORDER BY la.CreatedOn DESC) AS SlNo,    
        le.FullName AS LeadName,       
        aty.ActivityMessage AS ActivityType,        
        CASE       
            WHEN aty.ActivityMessage = 'Lead allocated'       
            THEN aty.ActivityMessage + ' from ' + ISNULL(lsource.FirstName, 'no_one') + ' to ' + ldestination.FirstName      
            ELSE aty.ActivityMessage        
        END AS Message,      
        le.MobileNumber,     
        la.CreatedOn,     
        us.FirstName AS CreatedBy     
    FROM LeadActivity AS la     
    INNER JOIN ActivityType AS aty ON la.ActivityType = aty.Id     
    INNER JOIN Leads le ON la.LeadKey = le.PublicKey     
    INNER JOIN Users as us ON la.CreatedBy = us.PublicKey     
    LEFT JOIN Users as lsource ON lsource.PublicKey = la.Source    
    LEFT JOIN Users as ldestination ON ldestination.PublicKey = la.Destination     
    WHERE ((@RoleName='admin' AND @LoggedInUser IS NULL)

        OR(@RoleName='Sales Lead' AND  la.CreatedBy IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser))
		
     )   
	 AND la.CreatedBy = ISNULL(@PrimaryKey, la.CreatedBy)    
    AND (   
        le.FullName LIKE '%' + @SearchText + '%' OR   
        le.MobileNumber LIKE '%' + @SearchText + '%'   
    )   
    AND (   
        (@FromDate IS NULL AND @ToDate IS NULL) OR   
        (CAST(la.CreatedOn AS DATE) BETWEEN CAST(ISNULL(@FromDate, DATEADD(MONTH, -1, GETDATE())) AS DATE)   
        AND CAST(ISNULL(@ToDate, GETDATE()) AS DATETIME))   
    )   
    ORDER BY la.CreatedOn DESC      
    OFFSET (@PageNumber - 1) * @PageSize ROWS     
    FETCH NEXT @PageSize ROWS ONLY;     
END 
---- Modify By Siva 17 June 2025 5:44 PM
GO
--select * from useractivity ua left join ActivityType at on ua.ActivityType = at.id  where cast(createdon as date) = cast(getDate() as date)         
   
   
-- EXEC  GetUserActivity 10,1,null,'9718304558',null,null,'' ,null           
   
   
--Purpose:   To Get the activities based on Sales Team or based on Activities on given LeadId               
-- PrimaryKey : we have to filter based on Sales Team publicKey (primaryKey)               
-- SecondaryKey: we have to filter based on (secondaryKey)  which is leadKey               
-- Note: either filter based on PrimaryKey = Sales Team PublicKey and SecondaryKey = null                
-- Note: or filter based on PrimaryKey is null and SecondaryKey = LeadKey                
   
ALTER PROCEDURE [dbo].[GetUserActivity]   
(   
    @PageSize int = 10,   
    @PageNumber int = 1,   
    @PrimaryKey uniqueidentifier = null,   
    @SecondaryKey varchar(50) = null,   
    @FromDate datetime = null,   
    @ToDate datetime = null,  
	@LoggedInUser VARCHAR(50) = NULL,  
	@SearchText varchar(100) = null,   
    @TotalCount INT OUTPUT   
)   
AS                   
BEGIN   
    SET @SearchText = ISNULL(@SearchText, '')   
  
   DECLARE @LoggedInUserId INT;  
    DECLARE @RoleName VARCHAR(50)  
  
    -- Get logged in user and role  
    SELECT   
        @LoggedInUserId = u.Id,  
        @RoleName = r.Name  
    FROM Users u  
    INNER JOIN Roles r ON r.PublicKey = u.RoleKey  
    WHERE u.PublicKey = @LoggedInUser;  
	BEGIN  
 IF exists (SELECT 1 fROM Users  as us   
 INNER JOIN Roles as ro on us.RoleKey = ro.PublicKey  
 WHERE us.PublicKey = @LoggedInUser AND ro.Name = 'admin')  
 BEGIN  
  SET @LoggedInUser = null  
 END  
END

    DECLARE @leadKeyTemp uniqueidentifier   
  
    SELECT @leadKeyTemp = publickey   
    FROM leads   
    WHERE mobilenumber = @SecondaryKey   
      AND EmailId IS NOT NULL   
      OR @SecondaryKey IS NULL   
  
    PRINT('@leadKeyTemp')   
  
    SELECT   
        ROW_NUMBER() OVER (ORDER BY ua.CreatedOn DESC) AS SlNo,   
        users.FirstName,   
        users.LastName,   
        ua.PublicKey,   
        at.activitymessage,   
        ua.CreatedOn,   
        users.MobileNumber,   
        CASE   
            WHEN at.id IN (22, 23)   
            THEN activitymessage + ' for ' + cd.Name   
            ELSE (activitymessage + ' for ' + leads.fullname)   
        END AS message   
    INTO #tempTable   
    FROM UserActivity as ua   
        LEFT JOIN Users as users ON ua.publickey = users.PublicKey   
        LEFT JOIN ActivityType as at ON at.id = ua.activitytype   
        LEFT JOIN leads ON ua.leadkey = leads.publickey   
        LEFT JOIN CompanyDetailM cd   
            ON cd.Id = TRY_CAST(   
                CASE    
                    WHEN PATINDEX('%.%', ua.Description) > 0    
                    THEN LEFT(ua.Description, PATINDEX('%.%', ua.Description) - 1)    
                    ELSE ua.Description    
                END AS INT)   
  
    WHERE                     
        (users.FirstName LIKE '%' + @SearchText +'%'   
        OR users.LastName LIKE '%' + @SearchText +'%'   
        OR users.mobilenumber LIKE '%' + @SearchText +'%')   
        AND ((@RoleName='admin' AND @LoggedInUser IS NULL)

        OR(@RoleName='Sales Lead' AND  users.PublicKey IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser))
		
     )  
	 AND users.PublicKey = ISNULL(@PrimaryKey, users.PublicKey)   
        AND (ua.LeadKey =  @leadKeyTemp OR @SecondaryKey IS NULL)   
        AND (  
            @FromDate IS NULL AND @ToDate IS NULL   
            OR   
            CAST(ua.CreatedOn AS DATE)   
            BETWEEN CAST(ISNULL(@FromDate, DATEADD(MONTH, -12, GETDATE())) AS DATE)   
            AND CAST(ISNULL(@ToDate, GETDATE()) AS DATE)  
        )  
  
    -- Correct @TotalCount calculation  
    SELECT @TotalCount = COUNT(1) FROM #tempTable   
  
    -- Correct ordering for pagination  
    SELECT   
        SlNo,   
        FirstName,   
        LastName,   
        PublicKey,   
        activitymessage,   
        CreatedOn,   
        MobileNumber,   
        message   
    FROM #tempTable   
    ORDER BY SlNo  -- Ensures correct Sl.No sequence  
    OFFSET (@PageNumber - 1) * @PageSize ROWS   
    FETCH NEXT @PageSize ROWS ONLY   
  
    DROP TABLE IF EXISTS #tempTable   
END   

---- MODIFY BY SIVA 18 JUNE 2025 3:05 PM--------------
GO
ALTER PROCEDURE  [dbo].[GetJunkLeads]                                        
(                                        
--declare                                        
 @IsPaging int =0,                                        
 @PageSize int =2500,                                        
 @PageNumber int =1,                                        
 @SortOrder varchar(50) = 'desc',                                        
 @SortExpression varchar(50) = 'CreatedBy',                                        
 @FromDate  DateTime= NULL,                                        
 @ToDate  DateTime= NULL,                                        
 @PrimaryKey varchar(50) = null,                                        
 @SecondaryKey varchar(50) = null, -- Services Dropdown Filter                                         
 @ThirdKey varchar(50) =null,                                        
 @FourthKey varchar(50) =null, -- lead Source                                        
 @FifthKey varchar(50) =NULL,                                        
 @CreatedBy varchar(50) =null,
 @LoggedInUser VARCHAR(50) = NULL,  
 @AssignedTo varchar(50) = null,                                        
 @SearchText varchar(100) =  null,                                        
 @TotalCount INT output                                        
)                                        
AS                                        
BEGIN                                         
                                        
 --SET @FromDate  = ISNULL(CAST(@FromDate AS DATE), dateadd(day,-30, GetDate()))                                        
 --SET @ToDate  = ISNULL(CAST(@ToDate AS DATE),   GetDate())                                        
 set @PrimaryKey = IIF(@PrimaryKey = '' , null , @PrimaryKey)                                        
 set @SecondaryKey = IIF(@SecondaryKey = '' , null , @SecondaryKey)                                        
 set @ThirdKey = IIF(@ThirdKey = '' , null , @ThirdKey)                                        
 set @FourthKey = IIF(@FourthKey = '' , null , @FourthKey)                                        
                                            
 set @FifthKey = IIF(@FifthKey = null  , '' , @FifthKey)                                        
                                        
 DECLARE @StartIndex int =0                                        
 DECLARE @EndIndex int =0                                        
 DROP TABLE IF EXISTS #tempCustomer                                        
                                        
 set @StartIndex = (@PageNumber-1) * @PageSize                                         
 set @EndIndex = @StartIndex + @PageSize                                         
     DECLARE @LoggedInUserId INT;  
    DECLARE @RoleName VARCHAR(50)  
  
    -- Get logged in user and role  
    SELECT   
        @LoggedInUserId = u.Id,  
        @RoleName = r.Name  
    FROM Users u  
    INNER JOIN Roles r ON r.PublicKey = u.RoleKey  
    WHERE u.PublicKey = @LoggedInUser;  
	BEGIN  
 IF exists (SELECT 1 fROM Users  as us   
 INNER JOIN Roles as ro on us.RoleKey = ro.PublicKey  
 WHERE us.PublicKey = @LoggedInUser AND ro.Name = 'admin')  
 BEGIN  
  SET @LoggedInUser = null  
 END  
END                                     
                                        
 SELECT @TotalCount = count(1)                                        
 From Leads as Leads                                        
 LEFT JOIN Services as Services on Leads.ServiceKey = Services.PublicKey                                        
 LEFT JOIN LeadTypes as LeadTypes on LeadTypes.PublicKey = Leads.LeadTypeKey                                        
 LEFT JOIN LeadSources as LeadSource on LeadSource.PublicKey = Leads.LeadSourceKey                                        
 LEFT JOIN Users AS U1 on U1.PublicKey = Leads.AssignedTo            
  LEFT JOIN Users AS U2 on U2.PublicKey = Leads.CreatedBy                                        
 LEFT JOIN Status as st on st.id = Leads.StatusId                                        
 Left join PurchaseOrders as po on po.LeadId = Leads.Id and Status <>  (select id From status where Code = 'com')   --COM NO NEED TO FETCH EXPIRED SERVICE               
 LEFT JOIN Status as st1 on st1.id = po.Status                                     
 WHERE                                        
 ISNULL(leads.IsDelete,0) = 0                                         
 and (                                        
 Leads.MobileNumber LIKE '%' + @SearchText +'%' OR @SearchText IS NULL OR                       
  Leads.AlternateMobileNumber LIKE '%' + @SearchText +'%' OR                         
                   
 Leads.EmailId LIKE '%'+ @SearchText +'%' OR                                         
 Leads.FullName LIKE '%'+ @SearchText +'%'                      
 )                                        
  AND (
    (@RoleName = 'admin' AND @LoggedInUser IS NULL)
    OR
    (@RoleName = 'Sales Lead' AND Leads.AssignedTo IN (
        SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser
    ))
)
AND (@AssignedTo IS NULL OR Leads.AssignedTo = @AssignedTo)                          
-- AND (                                
--     (po.status <> 24 AND ISNULL(po.IsExpired, 0) = 0) -- not customer and not expired                                
--     OR                                
--     (po.status = 24 AND ISNULL(po.IsExpired, 0) = 1) -- is customer and expired                             
--  OR                                
--     (po.status = 4 AND ISNULL(po.IsExpired, 0) = 1) -- is not customer and expired                      
--     OR                                
--     (po.status IS NULL) -- no po status i.e., no PR's yet                                
-- ) We have commented the code because we want all the data with currect status       
AND (
    (@FifthKey = 'assigned' AND Leads.AssignedTo IS NOT NULL)
    OR (@FifthKey = 'unassigned' AND Leads.AssignedTo IS NULL)
    OR (ISNULL(@FifthKey, '') = '')
)
                                
                                        
 AND ISNULL(Leads.ServiceKey , '00000000-0000-0000-0000-000000000000') = ISNULL( @SecondaryKey ,isnull(Leads.ServiceKey,'00000000-0000-0000-0000-000000000000') ) --OR Coalesce(@SecondaryKey,'') = '')                            
 AND ISNULL(Leads.LeadSourceKey, '00000000-0000-0000-0000-000000000000') = ISNULL( @FourthKey , Leads.LeadSourceKey)  --OR Coalesce(@FourthKey,'') = '')                                        
 AND ISNULL(Leads.LeadTypeKey, '00000000-0000-0000-0000-000000000000') = ISNULL( @ThirdKey , ISNULL(Leads.LeadTypeKey,'00000000-0000-0000-0000-000000000000')) --OR Coalesce(@ThirdKey,'') = '')                                        
 AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN cast( isnull(@FromDate , leads.ModifiedOn) as DATE ) AND                                  
 CAST(ISNULL(@ToDate , leads.ModifiedOn) as date))                                        
                                          
 SELECT                                         
 ROW_NUMBER() OVER (ORDER BY Leads.ModifiedOn DESC) AS RowNumber ,                                        
 Leads.[Id],                                        
 Leads.[FullName],                                        
 Leads.[MobileNumber],                                        
 Leads.[EmailId],                                 
 ISNULL(Services.Name , '') as  [ServiceKey],                                         
 ISNULL(LeadTypes.Name , '') as [LeadTypeKey],                                        
 ISNULL(LeadSource.Name, '') as [LeadSourceKey],                                        
 ISNULL(Leads.[Remarks], '' ) as Remarks ,                                        
 Leads.[IsSpam],                                        
 Leads.[IsWon],                                        
 Leads.[IsDisabled],                                        
 Leads.[IsDelete],                                        
 Leads.[PublicKey],                                        
 Leads.[CreatedOn],                                         
 U2.FirstName as  [CreatedBy],                                        
 Leads.[ModifiedOn],                                        
 Leads.[ModifiedBy],                                         
 U1.FirstName as [AssignedTo],                                         
 Leads.StatusId as [StatusId] ,                                        
 st.Name as StatusName,                                        
 st1.name as poStatus,                                        
 Leads.PurchaseOrderKey                                       
  INTO #tempCustomer                                      
  From Leads as Leads                                        
 LEFT JOIN Services as Services on Leads.ServiceKey = Services.PublicKey                                        
 LEFT JOIN LeadTypes as LeadTypes on Leads.LeadTypeKey = LeadTypes.PublicKey                                     
LEFT JOIN LeadSources as LeadSource on Leads.LeadSourceKey = LeadSource.PublicKey                                       
 LEFT JOIN Users AS U1 on U1.PublicKey = Leads.AssignedTo            
  LEFT JOIN Users AS U2 on U2.PublicKey = Leads.CreatedBy                                        
 LEFT JOIN Status as st on Leads.StatusId = st.id                                   
 Left join PurchaseOrders as po on po.LeadId = Leads.Id and Status <>  (select id From status where Code = 'com')   --COM NO NEED TO FETCH EXPIRED SERVICE               
 LEFT JOIN Status as st1 on st1.id = po.Status                                        
 WHERE                                        
 ISNULL(leads.IsDelete,0) = 0                                         
 and (                                        
 Leads.MobileNumber LIKE '%' + @SearchText +'%' OR                      
  Leads.AlternateMobileNumber LIKE '%' + @SearchText +'%'                       
 OR @SearchText IS NULL OR              
 Leads.EmailId LIKE '%'+ @SearchText +'%' OR                                         
 Leads.FullName LIKE '%'+ @SearchText +'%'                                        
 )                                        
   AND (
    (@RoleName = 'admin' AND @LoggedInUser IS NULL)
    OR
    (@RoleName = 'Sales Lead' AND Leads.AssignedTo IN (
        SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser
    ))
)
AND (@AssignedTo IS NULL OR Leads.AssignedTo = @AssignedTo)
                      
-- AND (                                
--     (po.status <> 24 AND ISNULL(po.IsExpired, 0) = 0) -- not customer and not expired                                
--     OR                                
--     (po.status = 24 AND ISNULL(po.IsExpired, 0) = 1) -- is customer and expired                                       
--  OR                                
--     (po.status = 4 AND ISNULL(po.IsExpired, 0) = 1) -- is not customer and expired                         
--  OR                     
--     (po.status IS NULL)-- no po status i.e., no PR's yet                                
                     
-- )   We are comment code becose we want all the data withb currect statrus                                
 AND (
    (@FifthKey = 'assigned' AND Leads.AssignedTo IS NOT NULL)
    OR (@FifthKey = 'unassigned' AND Leads.AssignedTo IS NULL)
    OR (ISNULL(@FifthKey, '') = '')
)
                                        
 --AND (@CreatedBy IS NULL OR Leads.AssignedTo = @CreatedBy)                                      
 --AND ISNULL(Leads.ServiceKey , '00000000-0000-0000-0000-000000000000') = ISNULL( @SecondaryKey ,isnull(Leads.ServiceKey,'00000000-0000-0000-0000-000000000000') ) --OR Coalesce(@SecondaryKey,'') = '')                                        
 --AND ISNULL(Leads.LeadSourceKey, '00000000-0000-0000-0000-000000000000') = ISNULL( @FourthKey , ISNULL(Leads.LeadSourceKey,'00000000-0000-0000-0000-000000000000'))  --OR Coalesce(@FourthKey,'') = '')                                        
 --AND ISNULL(Leads.LeadTypeKey, '00000000-0000-0000-0000-000000000000') = ISNULL( @ThirdKey , Leads.LeadTypeKey) --OR Coalesce(@ThirdKey,'') = '')                  
                 
 AND (                 
         @SecondaryKey IS NULL OR                 
         ISNULL(Leads.ServiceKey, '00000000-0000-0000-0000-000000000000') = @SecondaryKey                 
    )                
    AND (                 
         @FourthKey IS NULL OR                 
         ISNULL(Leads.LeadSourceKey, '00000000-0000-0000-0000-000000000000') = @FourthKey                
    )                
    AND (                 
         @ThirdKey IS NULL OR                 
         ISNULL(Leads.LeadTypeKey, '00000000-0000-0000-0000-000000000000') = @ThirdKey                
    )                
                
                
 AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN cast( isnull(@FromDate , leads.ModifiedOn) as DATE )                                      
 AND CAST(ISNULL(@ToDate , leads.ModifiedOn) as date))         
 ORDER BY Leads.ModifiedOn DESC                                        
                                        
                                           
                               
 OFFSET (IIF(@PageNumber <= 1 , 0 ,(@PageNumber-1))) * @PageSize ROWS                                        
 FETCH NEXT @PageSize ROWS ONLY                                        
 SELECT (SELECT * FROM #tempCustomer FOR JSON AUTO) as JsonData --, @TotalCount as Total                         
END           
---MODIFY BY SIVA 18 JUNE 2025 3:06 PM--------------------
GO
  
    
ALTER PROCEDURE [dbo].[GetPurchaseOrders]          
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
 @SecondaryKey varchar(50) = null,          
 @ThirdKey varchar(50) = null,          
 @FourthKey varchar(100) = null,           
 @FifthKey varchar(100) = null,  
  @LoggedInUser VARCHAR(50) = NULL,    
-- New parameter added        
 @TotalCount INT OUTPUT,          
 @TotalSales INT OUTPUT          
AS          
BEGIN          
    -- Handle null or empty inputs          
    IF @SearchText = '' SET @SearchText = null          
    IF @RequestedBy = '' SET @RequestedBy = null          
    IF @FifthKey = '' SET @FifthKey = null -- Handle empty FifthKey   
  DECLARE @LoggedInUserId INT;    
    DECLARE @RoleName VARCHAR(50)    
    
    -- Get logged in user and role    
    SELECT     
        @LoggedInUserId = u.Id,    
        @RoleName = r.Name    
    FROM Users u    
    INNER JOIN Roles r ON r.PublicKey = u.RoleKey    
    WHERE u.PublicKey = @LoggedInUser;    
 BEGIN    
 IF exists (SELECT 1 fROM Users  as us     
 INNER JOIN Roles as ro on us.RoleKey = ro.PublicKey    
 WHERE us.PublicKey = @LoggedInUser AND ro.Name = 'admin')    
 BEGIN    
  SET @LoggedInUser = null    
 END    
END  
          
    SELECT           
        @TotalCount = COUNT(1),          
        @TotalSales = SUM(PaidAmount)          
    FROM PurchaseOrders AS PO          
    LEFT JOIN Leads AS le ON PO.LeadId = le.Id          
    LEFT JOIN LeadSources ON le.LeadSourceKey = LeadSources.PublicKey          
    LEFT JOIN Users AS CreatedUser ON PO.CreatedBy = CreatedUser.PublicKey          
    LEFT JOIN PaymentModes AS pm ON PO.ModeOfPayment = pm.Id          
    LEFT JOIN Services AS ser ON ser.Id = PO.ServiceId          
    LEFT JOIN Status AS st ON st.Id = PO.Status          
    LEFT JOIN Users AS u ON u.PublicKey = le.AssignedTo          
    WHERE          
        (          
            PO.Mobile LIKE '%' + @SearchText + '%' OR          
            @SearchText IS NULL OR          
            PO.Email LIKE '%' + @SearchText + '%' OR          
            PO.ClientName LIKE '%' + @SearchText + '%'          
        )          
        AND (          
            (PO.ServiceId IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@PrimaryKey, ',')) AND @PrimaryKey IS NOT NULL)           
            OR (@PrimaryKey IS NULL)          
        )          
        AND (st.Code = ISNULL(@SecondaryKey, st.Code) OR COALESCE(@SecondaryKey, '') = '')    
  AND ((@RoleName='admin' AND @LoggedInUser IS NULL)  
  
        OR(@RoleName='Sales Lead' AND  le.AssignedTo IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser))  
     OR (
        @RoleName NOT IN ('admin', 'Sales Lead') AND le.AssignedTo = @LoggedInUser
    )
     )   
        AND (le.AssignedTo = ISNULL(@ThirdKey, le.AssignedTo) OR COALESCE(@ThirdKey, '') = '')          
        AND ISNULL(le.LeadSourceKey, '00000000-0000-0000-0000-000000000000') = ISNULL(@FourthKey, le.LeadSourceKey)                  
        AND (CreatedUser.PublicKey = ISNULL(@FifthKey, CreatedUser.PublicKey) OR COALESCE(@FifthKey, '') = '') -- Filter for FifthKey        
        AND CAST(PO.PaymentDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)          
          
    SELECT           
        PO.Id,          
        PO.LeadId,          
        PO.ClientName,          
        PO.Mobile,          
        PO.Email,          
        PO.DOB,          
        ISNULL(PO.Remark, '') AS Remark,          
        CAST(PO.PaymentDate AS smalldatetime) AS PaymentDate,          
        pm.Name as ModeOfPayment,    
  po.ModeOfPayment as ModeOfPaymentId,          
        ISNULL(PO.BankName, '') AS BankName,          
        ISNULL(PO.Pan, '') AS Pan,          
        ISNULL(PO.State, '') AS State,          
     PO.City,          
        ISNULL(PO.TransactionRecipt, '') AS TransactionRecipt,          
        ISNULL(PO.TransasctionReference, '') AS TransasctionReference,          
        PO.ServiceId,          
        ISNULL(PO.Service, '') AS Service,          
        ISNULL(PO.NetAmount, 0) AS NetAmount,          
        PO.PaidAmount,          
        PO.Status,          
        CAST(PO.CreatedOn AS smalldatetime) AS CreatedOn,          
        CAST(PO.ActionBy AS varchar(50)) AS ActionBy,          
        CreatedUser.FirstName + ' ' + CreatedUser.Lastname AS CreatedBy,          
        st.Name AS StatusName,          
        ser.Name AS ServiceName,          
        PO.StartDate AS StartDate,          
        PO.EndDate AS EndDate,          
        PO.PublicKey,          
        CreatedUser.FirstName AS firstname,          
        PO.CountryCode,          
        U.FirstName + ' ' + u.LastName AS AssignedTo,          
        LeadSources.Name AS LeadSourceName ,      
        le.PublicKey AS leadPublickKey         
    FROM PurchaseOrders AS PO          
    LEFT JOIN Leads AS le ON PO.LeadId = le.Id          
    LEFT JOIN LeadSources ON le.LeadSourceKey = LeadSources.PublicKey          
    LEFT JOIN Users AS CreatedUser ON PO.CreatedBy = CreatedUser.PublicKey          
    LEFT JOIN PaymentModes AS pm ON PO.ModeOfPayment = pm.Id          
    LEFT JOIN Services AS ser ON ser.Id = PO.ServiceId          
    LEFT JOIN Status AS st ON st.Id = PO.Status          
    LEFT JOIN Users AS u ON u.PublicKey = le.AssignedTo          
    WHERE          
        (          
            PO.Mobile LIKE '%' + @SearchText + '%' OR          
            @SearchText IS NULL OR          
            PO.Email LIKE '%' + @SearchText + '%' OR          
            PO.ClientName LIKE '%' + @SearchText + '%'          
        )          
        AND (          
            (PO.ServiceId IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@PrimaryKey, ',')) AND @PrimaryKey IS NOT NULL)           
            OR (@PrimaryKey IS NULL)          
        )          
        AND (st.Code = ISNULL(@SecondaryKey, st.Code) OR COALESCE(@SecondaryKey, '') = '')   
  AND ((@RoleName='admin' AND @LoggedInUser IS NULL)  
  
        OR(@RoleName='Sales Lead' AND  le.AssignedTo IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser))  
     OR (
        @RoleName NOT IN ('admin', 'Sales Lead') AND le.AssignedTo = @LoggedInUser
    )
     )   
        AND (le.AssignedTo = ISNULL(@ThirdKey, le.AssignedTo) OR COALESCE(@ThirdKey, '') = '')          
        AND ISNULL(le.LeadSourceKey, '00000000-0000-0000-0000-000000000000') = ISNULL(@FourthKey, le.LeadSourceKey)                  
        AND (CreatedUser.PublicKey = ISNULL(@FifthKey, CreatedUser.PublicKey) OR COALESCE(@FifthKey, '') = '') -- Filter for FifthKey        
        AND CAST(PO.PaymentDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)          
    ORDER BY PO.Status, PO.ModifiedOn DESC          
    OFFSET (IIF(@PageNumber <= 1, 0, (@PageNumber - 1))) * @PageSize ROWS          
    FETCH NEXT @PageSize ROWS ONLY          
END ---- MODIFY BY SIVA 18 JUNE 2025 4:04 PM--------------
GO
ALTER PROCEDURE GetLeadFreeTrials      
    @PageSize INT = 10,      
    @PageNumber INT = 1,      
    @PrimaryKey VARCHAR(100) = NULL,      
    @FromDate DATETIME = NULL,      
    @ToDate DATETIME = NULL,
	@LoggedInUser VARCHAR(50)=NULL,
    @SearchText VARCHAR(100) = NULL,      
    @TotalCount INT OUTPUT      
AS      
BEGIN      
    SET NOCOUNT ON;      
    SET @PrimaryKey = IIF(@PrimaryKey = '', NULL, @PrimaryKey);      
      
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;  
	 DECLARE @LoggedInUserId INT;  
    DECLARE @RoleName VARCHAR(50)  
  
    -- Get logged in user and role  
    SELECT   
        @LoggedInUserId = u.Id,  
        @RoleName = r.Name  
    FROM Users u  
    INNER JOIN Roles r ON r.PublicKey = u.RoleKey  
    WHERE u.PublicKey = @LoggedInUser;  
	BEGIN  
 IF exists (SELECT 1 fROM Users  as us   
 INNER JOIN Roles as ro on us.RoleKey = ro.PublicKey  
 WHERE us.PublicKey = @LoggedInUser AND ro.Name = 'admin')  
 BEGIN  
  SET @LoggedInUser = null  
 END  
END
      
    SELECT @TotalCount = COUNT(1)      
    FROM LeadFreeTrial lt      
    inner JOIN Leads Lead ON lt.LeadKey = Lead.PublicKey      
    left JOIN Services service ON lt.ServiceKey = service.PublicKey      
    left JOIN Users u ON lt.CreatedBy = u.PublicKey      
  WHERE (@FromDate IS NULL AND @ToDate IS NULL)    
   OR (CAST(lt.CreatedOn AS DATE) >= CAST(@FromDate AS DATE)    
       AND CAST(lt.CreatedOn AS DATE) <= CAST(@ToDate AS DATE))  
	    AND ((@RoleName='admin' AND @LoggedInUser IS NULL)

        OR(@RoleName='Sales Lead' AND  lt.CreatedBy IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser))
		
     ) 
        AND ISNULL(service.PublicKey, '00000000-0000-0000-0000-000000000000') = ISNULL(@PrimaryKey, ISNULL(service.PublicKey, '00000000-0000-0000-0000-000000000000'))      
        AND (@SearchText IS NULL   
      Or lead.MobileNumber like '%' + @SearchText + '%'  
   or lead.EmailId like '%'+ @SearchText + '%'  
            OR Lead.FullName LIKE '%' + @SearchText + '%');      
      
    SELECT lt.Id,      
           Lead.FullName AS LeadName,      
           Lead.MobileNumber AS LeadNumber,  
     lead.EmailId as leadEmail,  
           lt.LeadKey,      
           lt.ServiceKey,      
           service.Name AS ServiceName,      
           lt.StartDate,      
           lt.EndDate,      
           lt.IsActive AS Status,      
           lt.CreatedOn,      
           CONCAT(u.FirstName, ' ', u.LastName) AS CreatedBy,      
           CONCAT(u.FirstName, ' ', u.LastName) AS ModifiedBy,      
           lt.ModifiedOn,      
          CASE                WHEN CAST(GETDATE() AS DATE) < CAST(lt.StartDate AS DATE)             THEN DATEDIFF(DAY, lt.StartDate, lt.EndDate)              ELSE DATEDIFF(DAY, CAST(GETDATE() AS DATE), lt.EndDate)         END AS Validity,      
           (SELECT COUNT(*) FROM LeadFreeTrailReasonLog WHERE LeadFreeTrialId = lt.Id) AS ReasonLogCount,      
           CASE      
               WHEN CAST(lt.EndDate AS DATE) < CAST(GETDATE() AS DATE) THEN 'Expired'                    WHEN lt.IsActive = 1 THEN 'Active'      
               ELSE 'Inactive' END AS StatusText      
    FROM LeadFreeTrial lt      
    inner JOIN Leads Lead ON lt.LeadKey = Lead.PublicKey      
    left JOIN Services service ON lt.ServiceKey = service.PublicKey      
    left JOIN Users u ON lt.CreatedBy = u.PublicKey      
   WHERE (@FromDate IS NULL AND @ToDate IS NULL)    
   OR (CAST(lt.CreatedOn AS DATE) >= CAST(@FromDate AS DATE)    
       AND CAST(lt.CreatedOn AS DATE) <= CAST(@ToDate AS DATE)) 
	    AND ((@RoleName='admin' AND @LoggedInUser IS NULL)

        OR(@RoleName='Sales Lead' AND  lt.CreatedBy IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser))
		
     ) 
        AND ISNULL(service.PublicKey, '00000000-0000-0000-0000-000000000000') = ISNULL(@PrimaryKey, ISNULL(service.PublicKey, '00000000-0000-0000-0000-000000000000'))      
        AND (@SearchText IS NULL      
            OR Lead.FullName LIKE '%' + @SearchText + '%'  
   Or lead.MobileNumber like '%' + @SearchText + '%'  
   or lead.EmailId like '%'+ @SearchText + '%'  
   )      
    ORDER BY ISNULL(lt.ModifiedOn, lt.CreatedOn) DESC      
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;      
END;      
      
--DECLARE @TotalRecords INT;      
--EXEC GetLeadFreeTrials      
--    @FromDate = '1970-01-01',        
--    @ToDate = '2025-12-31',        
--    @PageNumber = 1,        
--    @PrimaryKey = NULL,        
--    @SearchText = NULL,        
--    @PageSize = 25,       
--    @TotalCount = @TotalRecords OUTPUT;        
--SELECT @TotalRecords AS TotalCount; 
---MODIFY BY SIVA 19 JUNE 2025 2:25 PM---
GO
--  exec GetSalesDashboardReport '1F1CCA06-1535-EE11-811E-00155D23D79C', '2023-06-01' , '2025-06-30'                 
--  exec GetSalesDashboardReport 'BB74D26F-AA28-EB11-BEE5-00155D53687A', '2024-07-01', '2025-06-30'             
ALTER PROCEDURE [dbo].[GetSalesDashboardReport]    
    @LoggedInUser VARCHAR(50),    
    @StartDate DATETIME,    
    @EndDate DATETIME    
AS    
BEGIN    
    SET NOCOUNT ON;    
  DECLARE @LoggedInUserId INT;    
    DECLARE @RoleName VARCHAR(50)    
    
    -- Get logged in user and role    
    SELECT     
        @LoggedInUserId = u.Id,    
        @RoleName = r.Name    
    FROM Users u    
    INNER JOIN Roles r ON r.PublicKey = u.RoleKey    
    WHERE u.PublicKey = @LoggedInUser;    
 BEGIN    
 IF exists (SELECT 1 fROM Users  as us     
 INNER JOIN Roles as ro on us.RoleKey = ro.PublicKey    
 WHERE us.PublicKey = @LoggedInUser AND ro.Name = 'admin')    
 BEGIN    
  SET @LoggedInUser = null    
 END    
END  
    
    -- Log the report request    
    INSERT INTO Logs    
    VALUES (    
        CONVERT(VARCHAR(20), @StartDate, 120) + ' to ' + CONVERT(VARCHAR(20), @EndDate, 120),    
        'exec GetSalesDashboardReport',    
        GETDATE()    
    );    
    
    /*     
       If the logged-in user is an admin (role id = 1),    
       then set @LoggedInUser to NULL so that data is not filtered by user.    
    */    
     
    
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
   @FreshLeadsRevenue NVARCHAR(MAX);    
    
    -- Consolidate revenue aggregations into one query to reduce table scans.    
    SELECT     
        @TotalRevenue = SUM(CASE WHEN st.code IN ('app','cus','pen') THEN po.PaidAmount ELSE 0 END),    
        @TotalApprovedRevenue = SUM(CASE WHEN st.code IN ('app','cus') THEN po.PaidAmount ELSE 0 END),    
        @TotalPendingRevenue = SUM(CASE WHEN st.code = 'pen' THEN po.PaidAmount ELSE 0 END)    
    FROM PurchaseOrders AS po    
    INNER JOIN STATUS AS st ON po.STATUS = st.Id    
    WHERE po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE)   
 AND ((@RoleName='admin' AND @LoggedInUser IS NULL)  
  
        OR(@RoleName='Sales Lead' AND  po.CreatedBy IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser))  
    
     )    
    
    -- Total Sales Count (only active orders and excluding 'pen' and 'rej' statuses)    
    SELECT @TotalSalesInCount = COUNT(1)    
    FROM PurchaseOrders AS po    
    WHERE po.IsActive = 1    
      AND po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE)   
   AND ((@RoleName='admin' AND @LoggedInUser IS NULL)  
  
        OR(@RoleName='Sales Lead' AND  po.CreatedBy IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser))  
    
     )   
      AND po.STATUS IN (SELECT id FROM STATUS WHERE code NOT IN ('pen','rej'));    
    
    -- Sales Per Person as JSON    
    SELECT @TotalSalesPerPerson = (    
        SELECT     
            MAX(um.UserKey) AS UserKey,    
            MAX(us.FirstName) AS FirstName,    
            SUM(ISNULL(po.PaidAmount, 0.0)) AS PaidAmount    
        FROM UserMappings AS um    
        LEFT JOIN Users AS us ON um.UserKey = us.Publickey    
        LEFT JOIN PurchaseOrders AS po ON po.CreatedBy = um.UserKey    
        WHERE po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE)   
  AND ((@RoleName='admin' AND @LoggedInUser IS NULL)  
  
        OR(@RoleName='Sales Lead' AND  po.CreatedBy IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser))  
    
     )   
          AND po.STATUS IN (SELECT id FROM STATUS WHERE code NOT IN ('pen','rej'))    
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
   AND ((@RoleName='admin' AND @LoggedInUser IS NULL)  
  
        OR(@RoleName='Sales Lead' AND  po.CreatedBy IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser))  
    
     )   
      AND po.STATUS IN (SELECT id FROM STATUS WHERE code NOT IN ('pen','rej'));    
    
    -- Top 5 Deals as JSON    
    SELECT @TopFiveDealJson = (    
        SELECT TOP 5     
            po.LeadId,    
            po.ClientName,    
            po.PaymentDate,    
            po.PaidAmount    
        FROM PurchaseOrders AS po    
        INNER JOIN STATUS AS st ON po.STATUS = st.Id    
        WHERE po.IsActive = 1    
          AND CAST(po.PaymentDate AS DATE) BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE)    
    AND ((@RoleName='admin' AND @LoggedInUser IS NULL)  
  
        OR(@RoleName='Sales Lead' AND  po.CreatedBy IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser))  
    
     )   
          AND po.STATUS IN (SELECT id FROM STATUS WHERE code NOT IN ('pen','rej'))    
        ORDER BY po.PaidAmount DESC    
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
    AND ((@RoleName='admin' AND @LoggedInUser IS NULL)  
  
        OR(@RoleName='Sales Lead' AND  po.CreatedBy IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser))  
    
     )   
          AND po.STATUS IN (SELECT id FROM STATUS WHERE code NOT IN ('pen','rej'))    
        GROUP BY po.PaymentDate    
        FOR JSON AUTO    
    );    
    
    -- Every Day Performance (last 30 days relative to @EndDate) as JSON    
    ;WITH DateRange AS (    
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
   AND ((@RoleName='admin' AND @LoggedInUser IS NULL)  
  
        OR(@RoleName='Sales Lead' AND  po.CreatedBy IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser))  
    
     )   
           AND po.STATUS IN (SELECT id FROM STATUS WHERE code NOT IN ('pen','rej'))    
        GROUP BY dr.[Date]   
        FOR JSON AUTO    
    );    
    
    -- Monthly Sales Report as JSON (aggregated by month)    
    ;WITH MonthRange AS (    
        SELECT CAST(@StartDate AS DATE) AS MonthDate    
        UNION ALL    
        SELECT DATEADD(MONTH, 1, MonthDate)    
        FROM MonthRange    
        WHERE DATEADD(MONTH, 1, MonthDate) <= CAST(@EndDate AS DATE)    
    ),    
    GetMonthlyReport AS (    
        SELECT     
            DATEFROMPARTS(YEAR(po.PaymentDate), MONTH(po.PaymentDate), 1) AS SalesMonth,    
            SUM(po.PaidAmount) AS TotalSales    
        FROM PurchaseOrders AS po    
        WHERE  ((@RoleName='admin' AND @LoggedInUser IS NULL)  
  
        OR(@RoleName='Sales Lead' AND  po.CreatedBy IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser))  
    
     )   
          AND po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE)    
          AND po.STATUS IN (SELECT id FROM STATUS WHERE code NOT IN ('pen','rej'))    
        GROUP BY DATEFROMPARTS(YEAR(po.PaymentDate), MONTH(po.PaymentDate), 1)    
    )    
    SELECT @MonthlySalesReport = (    
        SELECT     
            FORMAT(mr.SalesMonth, 'yyyy-MM') AS [Date],    
            ISNULL(mr.TotalSales, 0.0) AS TotalSales    
        FROM MonthRange AS dr    
        LEFT JOIN GetMonthlyReport AS mr ON dr.MonthDate = mr.SalesMonth    
        ORDER BY dr.MonthDate DESC    
        FOR JSON AUTO    
    );    
    
    -- Total Sales Per Service as JSON    
    SELECT @TotalSalesPerService = (    
        SELECT     
            se.Id,    
            se.Name,    
            SUM(ISNULL(po.PaidAmount, 0)) AS TotalPayment    
        FROM Services AS se    
        LEFT JOIN PurchaseOrders AS po ON se.Id = po.ServiceId    
        WHERE     
   ((@RoleName='admin' AND @LoggedInUser IS NULL)  
  
        OR(@RoleName='Sales Lead' AND  po.CreatedBy IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser))  
    
     )   
          AND po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE)    
          AND po.STATUS IN (SELECT id FROM STATUS WHERE code NOT IN ('pen','rej'))    
        GROUP BY se.Id, se.Name    
        ORDER BY TotalPayment DESC    
        FOR JSON AUTO    
    );    
    
    
  ----Fresh Leads Revenue    
  -- WITH PaidSums AS (    
  --  SELECT     
  --   St.Name,    
  --   SUM(ISNULL(Po.PaidAmount, 0)) AS PaidAmount    
  --  FROM Leads L    
  --  INNER JOIN PurchaseOrders Po ON Po.LeadId = L.Id AND L.PurchaseOrderKey = Po.PublicKey    
  --  INNER JOIN STATUS St ON St.Id = Po.Status    
  --  WHERE     
  --   L.LeadTypeKey = '4f516913-c4b8-ed11-810f-00155d23d79c'    
  --   AND po.CreatedBy = ISNULL(@LoggedInUser, po.CreatedBy)    
  --     AND po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE)    
  --   AND Po.Status IN (3, 10, 11)    
  --  GROUP BY St.Name    
  -- ),     
  -- TotalSum AS (    
  --  SELECT SUM(PaidAmount) AS TotalFreshLeadRevenue FROM PaidSums    
  -- )    
  -- SELECT @FreshLeadsRevenue =     
  --  CONCAT(    
  --   '{',    
  --   STRING_AGG('"' + Name + '":' + CAST(PaidAmount AS VARCHAR), ','),    
  --   ',"TotalFreshLeadRevenue":' + CAST((SELECT TotalFreshLeadRevenue FROM TotalSum) AS VARCHAR),    
  --   '}'    
  --  )    
  -- FROM PaidSums;    
    
    -- Final report output with all aggregated data    
    SELECT     
        5000000 AS CurrentTargets,    
        ISNULL(@TotalRevenue, 0) AS TotalSales,    
        ISNULL(@TotalApprovedRevenue, 0) AS TotalApprovedAmount,    
        ISNULL(@TotalPendingRevenue, 0) AS TotalPendingAmount,    
        @TotalSalesInCount AS TotalSalesInCount,    
        @TopFiveDealJson AS TopFiveDealJson,    
        @TotalSalesPerDay AS TotalSalesPerDayJson,    
        @EveryDayPerformance AS EveryDayPerformanceJson,    
        @TotalSalesPerPerson AS TotalSalesPerPersonJson,    
  @MonthlySalesReport AS MonthlySalesReportJson,    
        @TotalSalesPerService AS TotalSalesPerService,    
  @FreshLeadsRevenue AS FreshLeadsRevenue    
END 

------------------Ajith 16-06-2025 3:10 PM
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
		pcm.Title, 
		pcm.Description, 
		pcm.ThumbnailImage, 
		pcm.ListImage, 
		pcm.AttachmentType, 
		pcm.Attachment, 
		pcm.Language, 
        pcm.ScreenshotJson,
		@TotalVideoDuration AS AllVideoDuration,
		@TotalVideoCount AS TotalVideoCount,
		@TotalChapters AS TotalChapters
	FROM ProductsContentM pcm
	WHERE pcm.ProductId = @productId 
		AND ISNULL(pcm.IsDeleted, 0) = 0 
		AND ISNULL(pcm.IsActive, 1) = 1 
	ORDER BY pcm.ModifiedOn DESC;
END; 

IF NOT EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'ProductsContentM'
      AND COLUMN_NAME = 'ScreenshotJson'
)
BEGIN
    ALTER TABLE ProductsContentM ADD ScreenshotJson NVARCHAR(MAX);
END

-----------------------Ajith 17-06-2025 5:30PM
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
        LEFT JOIN PaymentRequestStatusM AS PR ON PR.TransactionId = PO.TransasctionReference              
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
    LEFT JOIN PaymentRequestStatusM AS PR ON PR.TransactionId = PO.TransasctionReference              
    LEFT JOIN PhonePePaymentResponseM AS PP ON PR.TransactionId = PP.MerchanttransactionId               
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
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.TransactionId               
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
                LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.TransactionId             
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
----     REPLACE(PRS.Status, 'PAYMENT_SUCCESS', 'SUCCESS') AS Status,                     
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



-----------------------------Ajith 18-06-2025 02:40 PM

GO
ALTER PROCEDURE [dbo].[ManagePurchaseOrderSM]               
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
              @FinalValidityInDays = DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(DATEADD(MONTH, sd.Months, GETDATE()) AS DATE)) 
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
                       @EndDate = (CASE WHEN EndDate > @CurrentDate THEN DATEADD(DAY, @FinalValidityInDays, EndDate)                     
                                        ELSE DATEADD(DAY, @FinalValidityInDays, @CurrentDate) END)                     
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
    SET @CommunityEndDate = CASE WHEN @ExistingCommunityEndDate > @CurrentDate THEN DATEADD(DAY, @communitymappingdays , @ExistingCommunityEndDate)               
           ELSE DATEADD(DAY, @communitymappingdays, @CurrentDate) END;               
                 
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
            SET @BonusProductEndDate = CASE WHEN @ExistingBonusEndDate > @CurrentDate THEN DATEADD(DAY, @BonusProductDurationInDays, @ExistingBonusEndDate)         
                                            ELSE DATEADD(DAY, @BonusProductDurationInDays, @CurrentDate) END;         
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
        SELECT (SELECT p.name, p.code, CAST(@startDate AS DATE) AS StartDate, CAST(@endDate AS DATE) AS EndDate, (@FinalValidityInDays) AS ProductValidity,               
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
-- Modidify by Siva 24 June 2025 2:44 PM--------
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
     
	 DECLARE @LoggedInUserId INT;

	SELECT @LoggedInUserId = Id
	FROM Users
	WHERE PublicKey =CASE 
					WHEN @AssignedTo = @LoggedInUser THEN NULL
					ELSE ISNULL(@AssignedTo, @LoggedInUser)
				END

  -- Fast check for LTC-qualified leads     
    SELECT @LTCCount = COUNT(1)     
    FROM Leads AS L     
    INNER JOIN PurchaseOrders AS PO     
        ON PO.LeadId = L.Id AND L.PurchaseOrderKey = PO.PublicKey     
    WHERE  ISNULL(L.IsDelete,0) = 0     
      AND PO.Status = 10     
      AND L.AssignedTo = @LoggedInUser;     
      
-- If LTC exists, return those approved lead rows immediately      
IF @LTCCount > 0      
BEGIN      
 ;WITH LeadData AS (     
            SELECT     
                L.Id,     
                L.FullName,     
                L.MobileNumber,     
                L.AlternateMobileNumber,     
                L.EmailId,     
                ISNULL(PO.City, ISNULL(L.City, '')) AS City,     
                PO.PaymentDate,     
                ISNULL(L.Favourite, 0) AS Favourite,     
                COALESCE(LS.Name, PS.Name) AS ServiceKey,     
                COALESCE(LS.Id, PS.Id) AS ServiceId,     
                LT.Name AS LeadTypeKey,     
                LT.Id AS LeadTypesId,     
                LSRC.Id AS LeadSourcesId,     
                LSRC.Name AS LeadSourceKey,     
                ISNULL(L.Remarks, '') AS Remark,     
                L.IsSpam,     
                L.IsWon,     
                L.IsDisabled,     
                L.IsDelete,     
                L.PublicKey,     
                L.CreatedOn,     
                U.FirstName AS CreatedBy,     
                L.CreatedBy AS LeadCreatedBy,     
                L.ModifiedOn,     
                L.ModifiedBy,     
                L.AssignedTo,     
                ISNULL(L.StatusId, 1) AS StatusId,     
                ISNULL(ST.Name, 'New') AS StatusName,     
                ISNULL(L.PurchaseOrderKey, CAST('00000000-0000-0000-0000-000000000000' AS UNIQUEIDENTIFIER)) AS PurchaseOrderKey,     
                ISNULL(ST1.Name, 'New') AS PurchaseOrderStatus,     
                ISNULL(PO.ModeOfPayment, -1) AS ModeOfPayment,     
                ISNULL(PO.PaidAmount, 0.0) AS PaidAmount,     
                ISNULL(PO.NetAmount, 0.0) AS NetAmount,     
                ISNULL(PO.TransactionRecipt, '') AS TransactionRecipt,     
                ISNULL(PO.TransasctionReference, '') AS TransasctionReference,     
                CASE     
                    WHEN DATEDIFF(DAY, GETDATE(), DATEADD(DAY, 90, ISNULL(L.ModifiedOn, GETDATE()))) > 0     
                    THEN DATEDIFF(DAY, GETDATE(), DATEADD(DAY, 90, ISNULL(L.ModifiedOn, GETDATE())))     
                    ELSE 0     
                END AS DaysToGo,     
                L.CountryCode,     
                ROW_NUMBER() OVER (     
                    ORDER BY     
                        CASE WHEN @SortExpression   = 'FullName' AND @SortOrder = 'ASC' THEN L.FullName END ASC,     
                        CASE WHEN @SortExpression   = 'FullName' AND @SortOrder = 'DESC' THEN L.FullName END DESC,     
                        CASE WHEN @SortExpression   = 'ModifiedOn' AND @SortOrder = 'ASC' THEN L.ModifiedOn END ASC,     
                        CASE WHEN @SortExpression   = 'ModifiedOn' AND @SortOrder = 'DESC' THEN L.ModifiedOn END DESC     
      ) AS SlNo     
            FROM Leads AS L     
            INNER JOIN PurchaseOrders AS PO ON PO.LeadId = L.Id AND L.PurchaseOrderKey = PO.PublicKey     
            LEFT JOIN Services AS LS ON L.ServiceKey = LS.PublicKey     
            LEFT JOIN Services AS PS ON PO.ServiceId = PS.Id     
            LEFT JOIN LeadTypes AS LT ON L.LeadTypeKey = LT.PublicKey     
            LEFT JOIN LeadSources AS LSRC ON L.LeadSourceKey = LSRC.PublicKey     
            LEFT JOIN Users AS U ON L.CreatedBy = U.PublicKey     
            LEFT JOIN Status AS ST ON L.StatusId = ST.Id     
            LEFT JOIN Status AS ST1 ON PO.Status = ST1.Id     
            WHERE ISNULL(L.IsDelete,0) = 0      
              AND PO.Status = 10     
              AND L.AssignedTo = @LoggedInUser    
        )     
        SELECT *     
        FROM LeadData     
        WHERE SlNo BETWEEN ((@PageNumber - 1) * @PageSize + 1) AND (@PageNumber * @PageSize);     
     
        RETURN;     
    END     
          
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
	
AND ((@IsAdmin = 1 AND @AssignedTo IS NULL) OR(@AssignedTo IS NOT NULL AND Leads.AssignedTo = @AssignedTo)
    OR
    (@AssignedTo IS NULL AND Leads.AssignedTo = @LoggedInUser)
    OR
    (@AssignedTo IS NULL AND Leads.AssignedTo IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId))
)

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
        AND (     
             @FromDate IS NULL OR @ToDate IS NULL OR     
             CAST(Leads.ModifiedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)     
            )      
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
     
	  SELECT @TotalCount =  (SELECT Count(*) From #tempLeads)    
	  

		SELECT * FROM #tempLeads       
        WHERE SlNo BETWEEN ((@PageNumber - 1) * @PageSize) + 1 AND @PageSize * @PageNumber;       
      
    DROP TABLE #tempLeads;       
END 
--- Modify by Siva 26 June 2025 10:56 AM ------------------
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
          AND rolekey = (  
              SELECT publickey  
              FROM Roles  
              WHERE name = 'admin'  
          )  
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
  
    SELECT us.Id,  
           MAX(us.FirstName) + ' ' + MAX(us.LastName) AS EmpName,  
           COUNT(le.id) AS LeadCount,  
           SUM(CASE WHEN DATEDIFF(day, le.modifiedon, GETDATE()) > @NumberOfDaysUntouched THEN 1 ELSE 0 END) AS UntouchedLeads,  
           SUM(CASE WHEN le.LeadTypeKey = @FollowUpLeadType THEN 1 ELSE 0 END) AS FollowUpLeads,  
           COUNT(po.id) AS TotalPurchaseOrders,  
           SUM(ISNULL(po.PaidAmount, 0.0)) AS TotalPRPayment,  
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
    WHERE ISNULL(us.IsDisabled, 0) = 0  
      AND ISNULL(us.IsDelete, 0) = 0  
      AND ISNULL(le.ModifiedOn, CAST(GETDATE() AS DATE)) BETWEEN ISNULL(@StartDate, '2020-02-28') AND ISNULL(@EndDate, CAST(GETDATE() AS DATE))  
      AND (
        -- Admin sees Sales Leads + BDEs
        (@isAdmin = 1 AND us.roleKey IN (
            SELECT PublicKey FROM Roles WHERE name IN ('BDE', 'Sales Lead')
        ))

        -- Sales Lead sees self + BDEs under them
        OR (@isAdmin = 0 AND us.SupervisorId = @SuperVisorId)

        -- Include the login user himself always
        OR us.Id = @SuperVisorId
    )

    GROUP BY us.Id  
    ORDER BY MAX(LE.ModifiedOn) DESC; -- Order by the latest modified date  
END


-----------------------Ajith 2025-06-26 5:02 PM
GO
ALTER PROCEDURE [dbo].[GetFreeTrial]   
    @IsPaging BIT = 1,   
    @PageSize INT = 10,   
    @PageNumber INT = 1,   
    @SortExpression VARCHAR(50) = 'CreatedOn',   
    -- Default sorting column    
    @SortOrder VARCHAR(20) = 'DESC',   
    -- Default sorting order    
    @RequestedBy VARCHAR(100) = NULL,   
    @FromDate DATETIME = NULL,   
    @ToDate DATETIME = NULL,   
    @SearchText VARCHAR(100) = NULL,   
    @PrimaryKey VARCHAR(100) = NULL,   
    @TotalCount INT OUTPUT   
AS   
BEGIN   
    SET NOCOUNT ON;   
   
    -- Normalize NULL or empty input values    
    IF @FromDate = null SET @FromDate = DATEADD(DAY,-7, GETDATE());   
    IF @ToDate = '' SET @ToDate =    GETDATE() ;   
   
   
    IF @RequestedBy = '' SET @RequestedBy = NULL;   
    IF @RequestedBy = '' SET @RequestedBy = NULL;   
   
    -- Step 1: Create a temporary table to hold base data    
    CREATE TABLE #BaseData   
    (   
        FullName NVARCHAR(255),   
        PublicKey NVARCHAR(50),   
        Mobile NVARCHAR(50),   
        StartDate DATETIME,   
        EndDate DATETIME,   
        CreatedOn DATETIME,   
        ProductNames NVARCHAR(MAX),   
        Validity INT,   
        Status NVARCHAR(50)   
    );   
   
    -- Step 2: Insert the base dataset into the temporary table    
    INSERT INTO #BaseData   
        (FullName, Mobile, PublicKey, CreatedOn, StartDate, EndDate, ProductNames, Validity, Status)   
    SELECT   
        MU.FullName,   
        MU.Mobile,   
        MU.PublicKey,   
        MU.CreatedOn,   
        FM.StartDate,   
        FM.EndDate,   
        (    
            -- Subquery to avoid duplicates          
            SELECT STRING_AGG(P2.Name, ', ')   
        FROM (          
                SELECT DISTINCT P.Name   
            FROM FreeTrialBasketDetailM FTBD   
                JOIN ProductsM P ON FTBD.ProductId = P.Id   
            WHERE FTBD.FreeTrialBasketId = FM.FreeTrialBasketId   
			and p.isactive = 1 and isdeleted = 0 
            ) AS P2          
        ) AS ProductNames,   
        MIN(FTB.DaysInNumber) AS Validity,   
        CAST(        
            CASE         
                WHEN FM.MobileUserKey IS NULL THEN 'Not Activated'       
                WHEN DATEDIFF(DAY, FM.EndDate, GETDATE()) < 0 THEN 'Active'         
                ELSE 'Expired'         
            END         
        AS NVARCHAR(50)) AS Status   
    FROM MobileUsers MU   
        LEFT JOIN FreeTrialM FM ON MU.PublicKey = FM.MobileUserKey   
        LEFT JOIN FreeTrialBasketM FTB ON FM.FreeTrialBasketId = FTB.Id   
    WHERE    
    (
        @SearchText IS NULL
        OR MU.Mobile LIKE '%' + @SearchText + '%'
        OR MU.EmailId LIKE '%' + @SearchText + '%'
        OR MU.FullName LIKE '%' + @SearchText + '%'
    )
    AND (
        @FromDate IS NULL OR @ToDate IS NULL
        OR CAST(MU.RegistrationDate AS DATE)
           BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)
    )
   
    GROUP BY          
        MU.FullName,          
        MU.Mobile,       
        FM.MobileUserKey,      
        MU.PublicKey,          
        FM.StartDate,          
        FM.EndDate,          
        FM.IsActive,       
        MU.CreatedOn,        
        FM.FreeTrialBasketId;   
   
    -- Step 3: Calculate the total count after applying the status filter    
    SELECT @TotalCount = COUNT(1)   
    FROM #BaseData   
    WHERE (@PrimaryKey IS NULL OR Status = @PrimaryKey);   
   
    -- Step 4: Fetch paginated and sorted results from the temporary table    
    SELECT *   
    FROM #BaseData   
    WHERE (@PrimaryKey IS NULL OR Status = @PrimaryKey)   
    ORDER BY     
        CASE WHEN @SortExpression = 'StartDate' THEN StartDate END ASC,    
        CASE WHEN @SortExpression = 'EndDate' THEN EndDate END ASC,    
        CASE WHEN @SortExpression = 'CreatedOn' THEN CreatedOn END DESC,    
        CASE WHEN @SortExpression IS NULL THEN CreatedOn END DESC    
    OFFSET (IIF(@PageNumber <= 1, 0, (@PageNumber - 1))) * @PageSize ROWS               
    FETCH NEXT @PageSize ROWS ONLY;   
   
    -- Step 5: Drop the temporary table    
    DROP TABLE #BaseData;   
END 



GO
ALTER PROCEDURE GetLeadFreeTrials       
    @PageSize INT = 10,       
    @PageNumber INT = 1,       
    @PrimaryKey VARCHAR(100) = NULL,       
    @FromDate DATETIME = NULL,       
    @ToDate DATETIME = NULL, 
	@LoggedInUser VARCHAR(50)=NULL, 
    @SearchText VARCHAR(100) = NULL,       
    @TotalCount INT OUTPUT       
AS       
BEGIN       
    SET NOCOUNT ON;       
    SET @PrimaryKey = IIF(@PrimaryKey = '', NULL, @PrimaryKey);       
       
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;   
	 DECLARE @LoggedInUserId INT;   
    DECLARE @RoleName VARCHAR(50)   
   
    -- Get logged in user and role   
    SELECT    
        @LoggedInUserId = u.Id,   
        @RoleName = r.Name   
    FROM Users u   
    INNER JOIN Roles r ON r.PublicKey = u.RoleKey   
    WHERE u.PublicKey = @LoggedInUser;   
	BEGIN   
 IF exists (SELECT 1 fROM Users  as us    
 INNER JOIN Roles as ro on us.RoleKey = ro.PublicKey   
 WHERE us.PublicKey = @LoggedInUser AND ro.Name = 'admin')   
 BEGIN   
  SET @LoggedInUser = null   
 END   
END 
       
    SELECT @TotalCount = COUNT(1)       
    FROM LeadFreeTrial lt       
    inner JOIN Leads Lead ON lt.LeadKey = Lead.PublicKey       
    left JOIN Services service ON lt.ServiceKey = service.PublicKey       
    left JOIN Users u ON lt.CreatedBy = u.PublicKey       
  WHERE (@FromDate IS NULL AND @ToDate IS NULL)     
   OR (CAST(lt.CreatedOn AS DATE) >= CAST(@FromDate AS DATE)     
       AND CAST(lt.CreatedOn AS DATE) <= CAST(@ToDate AS DATE))   
	    AND ((@RoleName='admin' AND @LoggedInUser IS NULL) 
 
        OR(@RoleName='Sales Lead' AND  lt.CreatedBy IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser)) 
		 
     )  
        AND ISNULL(service.PublicKey, '00000000-0000-0000-0000-000000000000') = ISNULL(@PrimaryKey, ISNULL(service.PublicKey, '00000000-0000-0000-0000-000000000000'))       
        AND (@SearchText IS NULL    
      Or lead.MobileNumber like '%' + @SearchText + '%'   
   or lead.EmailId like '%'+ @SearchText + '%'   
            OR Lead.FullName LIKE '%' + @SearchText + '%');       
       
    SELECT lt.Id,       
           Lead.FullName AS LeadName,       
           Lead.MobileNumber AS LeadNumber,   
     lead.EmailId as leadEmail,   
           lt.LeadKey,       
           lt.ServiceKey,       
           service.Name AS ServiceName,       
           lt.StartDate,       
           lt.EndDate,       
           lt.IsActive AS Status,       
           lt.CreatedOn,       
              CONCAT(cu.FirstName, ' ', cu.LastName) AS CreatedBy,       
                CONCAT(
                    ISNULL(NULLIF(mu.FirstName, ''), cu.FirstName),
                    ' ',
                    ISNULL(NULLIF(mu.LastName, ''), cu.LastName)
                ) AS ModifiedBy,       
           lt.ModifiedOn,       
          CASE                WHEN CAST(GETDATE() AS DATE) < CAST(lt.StartDate AS DATE)             THEN DATEDIFF(DAY, lt.StartDate, lt.EndDate)              ELSE DATEDIFF(DAY, CAST(GETDATE() AS DATE), lt.EndDate)         END AS Validity,       
           (SELECT COUNT(*) FROM LeadFreeTrailReasonLog WHERE LeadFreeTrialId = lt.Id) AS ReasonLogCount,       
           CASE       
               WHEN CAST(lt.EndDate AS DATE) < CAST(GETDATE() AS DATE) THEN 'Expired'                    WHEN lt.IsActive = 1 THEN 'Active'       
               ELSE 'Inactive' END AS StatusText       
    FROM LeadFreeTrial lt       
    inner JOIN Leads Lead ON lt.LeadKey = Lead.PublicKey       
    left JOIN Services service ON lt.ServiceKey = service.PublicKey       
     LEFT JOIN Users cu ON lt.CreatedBy = cu.PublicKey       
    LEFT JOIN Users mu ON lt.ModifiedBy = mu.PublicKey       
   WHERE (@FromDate IS NULL AND @ToDate IS NULL)     
   OR (CAST(lt.CreatedOn AS DATE) >= CAST(@FromDate AS DATE)     
       AND CAST(lt.CreatedOn AS DATE) <= CAST(@ToDate AS DATE))  
	    AND ((@RoleName='admin' AND @LoggedInUser IS NULL) 
 
        OR(@RoleName='Sales Lead' AND  lt.CreatedBy IN (SELECT PublicKey FROM Users WHERE SupervisorId = @LoggedInUserId OR PublicKey = @LoggedInUser)) 
		 
     )  
        AND ISNULL(service.PublicKey, '00000000-0000-0000-0000-000000000000') = ISNULL(@PrimaryKey, ISNULL(service.PublicKey, '00000000-0000-0000-0000-000000000000'))       
        AND (@SearchText IS NULL       
            OR Lead.FullName LIKE '%' + @SearchText + '%'   
   Or lead.MobileNumber like '%' + @SearchText + '%'   
   or lead.EmailId like '%'+ @SearchText + '%'   
   )       
    ORDER BY ISNULL(lt.ModifiedOn, lt.CreatedOn) DESC       
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;       
END;       
       
--DECLARE @TotalRecords INT;       
--EXEC GetLeadFreeTrials       
--    @FromDate = '1970-01-01',         
--    @ToDate = '2025-12-31',         
--    @PageNumber = 1,         
--    @PrimaryKey = NULL,         
--    @SearchText = NULL,         
--    @PageSize = 25,        
--    @TotalCount = @TotalRecords OUTPUT;         
--SELECT @TotalRecords AS TotalCount; 


------------------------------------Ajith 27-06-2025 3:26 PM
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
        LEFT JOIN Users AS CreatedUser ON PO.CreatedBy = CreatedUser.PublicKey                  
    LEFT JOIN PaymentModes AS pm ON PO.ModeOfPayment = pm.Id                  
    LEFT JOIN ProductsM AS pd ON pd.Id = PO.ProductId                  
    LEFT JOIN Status AS st ON st.Id = PO.Status                  
    LEFT JOIN PaymentRequestStatusM AS PR ON PR.TransactionId = PO.TransasctionReference                 
    LEFT JOIN PhonePePaymentResponseM AS PP ON PR.TransactionId = PP.MerchanttransactionId                  
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
    LEFT JOIN PaymentRequestStatusM AS PR ON PR.TransactionId = PO.TransasctionReference                 
    LEFT JOIN PhonePePaymentResponseM AS PP ON PR.TransactionId = PP.MerchanttransactionId                  
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



----------------------Ajith 30-06-2025 12:02 PM
GO
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'LeadFreeTrial' 
      AND COLUMN_NAME = 'IsDeleted'
)
BEGIN
    ALTER TABLE LeadFreeTrial ADD IsDeleted BIT NOT NULL DEFAULT(0);
END
--- CREATED BY SIVA 1 JULY 2025 3:15 PM ------------------------
GO
ALTER PROCEDURE [dbo].[GetPmImagesM]           
 @searchText VARCHAR(100)           
AS           
BEGIN           
 SELECT Id,
		Title,
		Subtitle ,
		Description,
		MediaUrl ,
		MediaType, 
		BadgeText ,
		ValidityText, 
		ButtonText,    
        SecondaryButtonText,
	    ActionUrl,DownloadUrl,
	    ShowDownloadButton,
	    StartDate,
	    EndDate,
	    IsActive,
	    CreatedBy    
 FROM PromotionM           
 WHERE           
              
   (@searchText IS NULL           
   OR @searchText = ''           
   OR Title LIKE '%' + @searchText + '%'  ) AND IsDelete=0    
            
 ORDER BY CreatedOn,IsActive DESC           
END     


--- CREATED BY SIVA 2 JULY 2025 3:15PM ---------------------
IF COL_LENGTH('PartnerAccountDetails', 'IsVerified') IS NULL
BEGIN
    ALTER TABLE PartnerAccountDetails
    ADD IsVerified BIT
END
IF COL_LENGTH('PartnerAccountDetails', 'PartnerWith') IS NULL
BEGIN
    ALTER TABLE PartnerAccountDetails
    ADD PartnerWith BIT
END
---Created By Siva 8 July 2025 4:03 PM---------------
GO
USE [KingresearchTest]
GO

/****** Object:  Table [dbo].[PromotionM]    Script Date: 7/1/2025 6:04:29 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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
	[ButtonText] [nvarchar](100) NULL,
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

----Created By Siva 9 July 2025 10:17 AM ------------------
go

CREATE PROCEDURE [dbo].[GetPromoImagesM]
    @searchText VARCHAR(100) = NULL
AS
BEGIN
    SELECT 
        Id,
        MediaUrl,
        MediaType,
        ButtonText,
        SecondaryButtonText,
        ActionUrl,
        DownloadUrl,
        ShowDownloadButton,
        StartDate,
        EndDate,
        IsActive,
        CreatedBy,
        MaxDisplayCount,
        DisplayFrequency,
        LastShownAt,
        GlobalButtonAction,
        Target,
        ProductName,
        ProductId
    FROM PromotionM
    WHERE 
        IsDelete = 0
        AND (
            @searchText IS NULL
            OR @searchText = ''
            OR ProductName LIKE '%' + @searchText + '%'
        )
    ORDER BY CreatedOn, IsActive DESC
END

---- MODIFY BY SIVA 3 JULY 2025 3:07 PM --------------------
GO
ALTER PROCEDURE [dbo].[GetPartnerAccounts]    
    @IsPaging BIT = 0,    
    @PageSize INT = 100,    
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
    -- Calculate the total count of records               
    SELECT @product_count = COUNT(distinct pa.Id)    
    FROM PartnerAccounts PA    
        LEFT JOIN PartnerAccountDetails pad ON pa.id = pad.partnerAccountId    
        LEFT JOIN Users US ON PA.AssignedTo = US.Id    
    WHERE               
        ISNULL([Status], 0) = (               
            CASE               
                WHEN ISNULL(@StatusType, -1) = -1 THEN ISNULL([Status], 0)               
                ELSE @StatusType               
            END               
        )    
        AND ISNULL([AssignedTo], 0) = IIF(@AssignedTo = 0, ISNULL([AssignedTo], 0), @AssignedTo)    
        AND (               
            FullName LIKE '%' + ISNULL(@SearchText, FullName) + '%'    
        OR pad.PartnerCId LIKE '%' + ISNULL(@SearchText, pad.PartnerCId) + '%'    
        OR PA.MobileNumber LIKE '%' + ISNULL(@SearchText, PA.MobileNumber) + '%'    
        OR PA.EmailId LIKE '%' + ISNULL(@SearchText, PA.EmailId) + '%'               
        )    
        AND (    
    ISNULL(@PartnerWith, '') = ''    
        OR (    
        (LOWER(@PartnerWith) = 'kite' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
            AND ISNULL(PartnerCode, '') = 'kite'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
        OR (LOWER(@PartnerWith) = 'ant' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
            AND ISNULL(PartnerCode, '') = 'ant'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
        OR (LOWER(@PartnerWith) = 'angel' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
            AND ISNULL(PartnerCode, '') = 'angel'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
        OR (LOWER(@PartnerWith) = 'edel' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
            AND ISNULL(PartnerCode, '') = 'edel'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
        OR (LOWER(@PartnerWith) = 'fyer' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
            AND ISNULL(PartnerCode, '') = 'fyer'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
        OR (LOWER(@PartnerWith) = 'moti' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
            AND ISNULL(PartnerCode, '') = 'moti'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
        OR (LOWER(@PartnerWith) = 'dhan' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
            AND ISNULL(PartnerCode, '') = 'dhan'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
        OR (LOWER(@PartnerWith) = 'delta' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
  AND ISNULL(PartnerCode, '') = 'delta'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
        OR (LOWER(@PartnerWith) = 'punch' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
            AND ISNULL(PartnerCode, '') = 'punch'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
    )    
)    
        AND ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, PA.ModifiedOn)), ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999))                
            BETWEEN ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999)                
            AND ISNULL(@ToDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))    
        AND ISNULL(PA.isdelete, 0) = 0;    
    
    -- Select the paginated records               
    SELECT    
        ROW_NUMBER() OVER (ORDER BY MAX(pad.ModifiedOn) DESC) AS SlNo,    
        pa.Id,    
        MAX(pa.PublicKey) AS PublicKey, MAX(pa.FullName) AS FullName,    
        MAX(pa.MobileNumber) AS MobileNumber,    
        MAX(pa.EmailId) AS EmailId,    
        CASE               
          WHEN MAX(pa.[Status]) = 0 THEN 'Fresh'               
            WHEN MAX(pa.[Status]) = 1 THEN 'Pending'               
            WHEN MAX(pa.[Status]) = 2 THEN 'Accepted'               
            WHEN MAX(pa.[Status]) = 3 THEN 'Rejected'               
            WHEN MAX(pa.[Status]) = 4 THEN 'Followup'               
            WHEN MAX(pa.[Status]) = 5 THEN 'NotInterested'               
            WHEN MAX(pa.[Status]) = 6 THEN 'NPC'               
            ELSE 'Fresh'               
        END AS StatusType,    
        CAST(MAX(pa.[Status]) AS VARCHAR) AS STATUS,    
        MAX(pa.Remarks) AS Remarks,    
        '' AS Details,    
        MAX(pa.City) AS City,    
        MAX(pa.CreatedOn) AS CreatedOn,    
        MAX(pa.ModifiedOn) AS ModifiedOn,    
        MAX(pa.TelegramId) AS TelegramId,    
        STUFF(               
            (SELECT '| ' + pad2.PartnerCode + ':' + pad2.PartnerCId    
        FROM PartnerAccountDetails pad2    
        WHERE pad2.PartnerAccountId = pa.Id    
        FOR XML PATH('')), 1, 2, '') AS PartnerWith,    
        ISNULL(MAX(pa.Brokerage), 0.0) AS Brokerage,    
        ISNULL(MAX(pa.Source), 'kr') AS Source,    
        ISNULL(MAX(pa.AssignedTo), 0) AS AssignedTo,   
		ISNULL(MAX(pa.PartnerWithAccount), 0) AS PartnerWithAccount,   
		CAST(ISNULL(MAX(CAST(pa.IsVerified AS INT)), 0) AS BIT) AS IsVerified,  
  
        MAX(US.FirstName + ' ' + US.LastName) AS AssignedToName    
    FROM    
        PartnerAccounts pa    
        LEFT JOIN Users us ON us.Id = pa.AssignedTo    
        LEFT JOIN PartnerAccountDetails pad ON pa.id = pad.partnerAccountId    
    WHERE               
        ISNULL([Status], 0) = (               
            CASE               
                WHEN ISNULL(@StatusType, -1) = -1 THEN ISNULL([Status], 0)               
                ELSE @StatusType               
            END               
        )    
        AND ISNULL([AssignedTo], 0) = IIF(@AssignedTo = 0, ISNULL([AssignedTo], 0), @AssignedTo)    
        AND (               
            FullName LIKE '%' + ISNULL(@SearchText, FullName) + '%'    
        OR pad.PartnerCId LIKE '%' + ISNULL(@SearchText, pad.PartnerCId) + '%'    
        OR PA.MobileNumber LIKE '%' + ISNULL(@SearchText, PA.MobileNumber) + '%'    
        OR PA.EmailId LIKE '%' + ISNULL(@SearchText, PA.EmailId) + '%'               
        )    
        AND (    
    ISNULL(@PartnerWith, '') = ''    
        OR (    
        (LOWER(@PartnerWith) = 'kite' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
            AND ISNULL(PartnerCode, '') = 'kite'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
        OR (LOWER(@PartnerWith) = 'ant' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
            AND ISNULL(PartnerCode, '') = 'ant'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
        OR (LOWER(@PartnerWith) = 'angel' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
            AND ISNULL(PartnerCode, '') = 'angel'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
        OR (LOWER(@PartnerWith) = 'edel' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
            AND ISNULL(PartnerCode, '') = 'edel'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
        OR (LOWER(@PartnerWith) = 'fyer' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
            AND ISNULL(PartnerCode, '') = 'fyer'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
        OR (LOWER(@PartnerWith) = 'moti' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
            AND ISNULL(PartnerCode, '') = 'moti'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
        OR (LOWER(@PartnerWith) = 'dhan' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
            AND ISNULL(PartnerCode, '') = 'dhan'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
        OR (LOWER(@PartnerWith) = 'delta' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
            AND ISNULL(PartnerCode, '') = 'delta'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
        OR (LOWER(@PartnerWith) = 'punch' AND EXISTS (    
            SELECT 1    
        FROM PartnerAccountDetails    
        WHERE PartnerAccountId = pa.Id    
            AND ISNULL(PartnerCode, '') = 'punch'    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCode, '')))) > 0    
            AND LEN(LTRIM(RTRIM(ISNULL(PartnerCId, '')))) > 0    
        ))    
    )    
)    
        AND ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, PA.ModifiedOn)), ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999))                
            BETWEEN ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999)                
            AND ISNULL(@ToDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))    
        AND ISNULL(PA.isdelete, 0) = 0    
    GROUP BY               
        pa.Id    
    ORDER BY               
        MAX(pa.ModifiedOn) DESC              
    OFFSET (IIF(@PageNumber <= 1, 0, (@PageNumber - 1)) * @PageSize) ROWS                
    FETCH NEXT @PageSize ROWS ONLY;    
END 

---------------------------------------------------------------------------------------------  
 
-- CRETED ON 04-07-2025 For Mobile Dashboard Screen -- Ragesh M  
 
---------------------------------------------------------------------------------------------
GO
CREATE PROCEDURE GetMobileDashboard 
AS 
BEGIN 
    DROP TABLE IF EXISTS #MobileUsersTemp; 
 
    DECLARE @MobileUsersCountJson NVARCHAR(MAX); 
 
    ;WITH androidUsers AS ( 
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
    iosUsers AS ( 
        SELECT DeviceVersion 
        FROM MobileUsers 
        WHERE ISNULL(IsDelete, 0) = 0 
            AND ISNULL(IsOtpVerified, 0) = 1 
            AND ISNULL(IsActive, 1) = 1 
            AND ISNULL(AgreeToTerms, 0) = 1 
            AND ISNULL(SelfDeleteRequest, 0) = 0 
            AND SelfDeleteReason IS NULL 
            AND LOWER(DeviceType) LIKE '%ios%' 
    ) 
 
    SELECT @MobileUsersCountJson = ( 
        SELECT 
            -- Total Counts 
            (SELECT COUNT(*) FROM androidUsers) AS AndroidUsers, 
            (SELECT COUNT(*) FROM iosUsers) AS IosUsers, 
            (SELECT COUNT(*) FROM ( 
                SELECT * FROM androidUsers 
                UNION ALL 
                SELECT * FROM iosUsers 
            ) AS combinedUsers) AS TotalUsers, 
 
            -- Grouped Versions 
            (SELECT top 4 
                DeviceVersion, COUNT(*) AS VersionCount 
             FROM androidUsers 
             GROUP BY DeviceVersion 
             ORDER BY DeviceVersion DESC 
             FOR JSON PATH) AS AndroidVersionBreakdown, 
 
            (SELECT top 4 
                DeviceVersion, COUNT(*) AS VersionCount 
             FROM iosUsers 
             GROUP BY DeviceVersion 
             ORDER BY DeviceVersion DESC 
             FOR JSON PATH) AS IosVersionBreakdown 
 
        FOR JSON PATH, WITHOUT_ARRAY_WRAPPER 
    ); 
 
    SELECT @MobileUsersCountJson AS MobileUserCount; 
END


------------------------------------------------------------------------------------------
--Modified by Raegsh M on 05 July 2025
------------------------------------------------------------------------------------------
--  exec GetSalesDashboardReport 'BBAD5AAE-7DFD-4A6C-BD9D-ED92AD8CAB2C', '2025-05-01' , '2025-05-30'                 
--  exec GetSalesDashboardReport '1f1cca06-1535-ee11-811e-00155d23d79c', '2025-06-01', '2025-06-30', 'years'       
GO
CREATE PROCEDURE [dbo].[GetSalesDashboardReport] 
    @LoggedInUser VARCHAR(50), 
    @StartDate DATETIME, 
    @EndDate DATETIME, 
    @ChartPeriodType VARCHAR(10) = 'months' 
-- 'months' | 'quarters' | 'years'  
AS    
BEGIN 
    SET NOCOUNT ON; 
 
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
 
    -- Check if role is admin and set @IsAdmin flag 
    DECLARE @IsAdmin BIT = 0; 
 
    IF (SELECT RoleKey 
    FROM Users 
    where PublicKey = @LoggedInUser) = 'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b'  -- Admin role 
BEGIN 
        SET @IsAdmin = 1; 
    END 
 
    -- If admin, don't restrict by user 
    IF @IsAdmin = 1 
BEGIN 
        SET @LoggedInUser = NULL; 
    END 
ELSE 
BEGIN 
        DECLARE @UserId INT; 
 
        SELECT @UserId = Id 
        FROM Users 
        WHERE PublicKey = @LoggedInUser; 
 
        IF EXISTS ( 
        SELECT 1 
        FROM Users 
        WHERE SupervisorId = @UserId 
    ) 
    BEGIN 
            -- Add subordinates and self 
            INSERT INTO #UsersIds 
                (Id) 
            SELECT PublicKey 
            FROM Users 
            WHERE SupervisorId = @UserId 
                OR Id = @UserId; 
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
            @TotalSalesPerService NVARCHAR(MAX); 
 
    -- Consolidate revenue aggregations into one query to reduce table scans.    
    SELECT 
        @TotalRevenue = SUM(CASE WHEN st.code IN ('app','cus','pen') THEN po.PaidAmount ELSE 0 END), 
        @TotalApprovedRevenue = SUM(CASE WHEN st.code IN ('app','cus') THEN po.PaidAmount ELSE 0 END), 
        @TotalPendingRevenue = SUM(CASE WHEN st.code = 'pen' THEN po.PaidAmount ELSE 0 END) 
    FROM PurchaseOrders AS po 
        INNER JOIN STATUS AS st ON po.STATUS = st.Id 
    WHERE po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
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
    WHERE po.IsActive = 1 
        AND po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
        -- AND po.CreatedBy = ISNULL(@LoggedInUser, po.CreatedBy)    
        AND (   
        NOT EXISTS (SELECT 1 
        FROM #UsersIds) -- Admin case: #UsersIds is empty, fetch all   
        OR po.CreatedBy IN (SELECT Id 
        FROM #UsersIds)  -- Leader or employee: filter by CreatedBy   
        ) 
        AND po.STATUS IN (SELECT id 
        FROM STATUS 
        WHERE code IN ('cus','app')); 
 
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
        SELECT DISTINCT 
            po.LeadId, 
            po.ClientName, 
            Cast(po.StartDate as date) as PaymentDate, 
            po.PaidAmount 
        FROM PurchaseOrders AS po 
            INNER JOIN STATUS AS st ON po.STATUS = st.Id 
        WHERE po.IsActive = 1 
            AND CAST(po.StartDate AS DATE) BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
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
        --GROUP BY po.LeadId, po.ClientName, po.PaymentDate, po.PaidAmount 
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
        NOT EXISTS (SELECT 1 
                FROM #UsersIds) -- Admin case: #UsersIds is empty, fetch all   
                OR po.CreatedBy IN (SELECT Id 
                FROM #UsersIds)  -- Leader or employee: filter by CreatedBy   
    ) 
                --    AND po.CreatedBy = ISNULL(@LoggedInUser, po.CreatedBy)    
                AND po.STATUS IN (SELECT id 
                FROM STATUS 
                WHERE code IN ('cus','app')) 
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
        -- po.CreatedBy = ISNULL(@LoggedInUser, po.CreatedBy)    
           (   
        NOT EXISTS (SELECT 1 
                FROM #UsersIds) -- Admin case: #UsersIds is empty, fetch all   
                OR po.CreatedBy IN (SELECT Id 
                FROM #UsersIds)  -- Leader or employee: filter by CreatedBy   
    ) 
                AND po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
                AND po.STATUS IN (SELECT id 
                FROM STATUS 
                WHERE code IN ('cus','app')) 
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
        WHERE (CAST(po.CreatedOn AS DATE) BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE)) AND (po.[Status] IN (24, 10))) 
 
    SELECT @LeadSoruceEarnings = (  
        SELECT 
            ls.Name, 
            SUM(po.PaidAmount) AS TotalEarnings 
        FROM Leads AS le 
            INNER JOIN LeadSources AS ls 
            ON ls.PublicKey = le.LeadSourceKey 
            INNER JOIN PurchaseOrders AS po 
            ON po.LeadId = le.Id 
        WHERE (CAST(po.CreatedOn AS DATE) BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE)) AND (po.[Status] IN (24, 10)) 
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
        @TotalLeads = COUNT(1), 
        @AllocatedLeads = SUM(CASE WHEN  LeadTypeKey = '4569c760-96e2-ee11-8142-00155d23d79c' THEN 1 ELSE 0 END), 
        @UnallocatedLeads = @TotalLeads - @AllocatedLeads 
    --FROM ( 
    --SELECT DISTINCT le.PublicKey, le.AssignedTo, le.LeadTypeKey 
    FROM Leads AS le 
    WHERE ( 
            NOT EXISTS (SELECT 1 
        FROM #UsersIds) 
        OR le.AssignedTo IN (SELECT Id 
        FROM #UsersIds) 
        ) 
        AND ISNULL(le.IsDelete, 0) = 0 
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
        WHERE ISNULL(le.IsDelete, 0) = 0 AND 
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
 
    IF @ChartPeriodType = 'months'  
        SET @ChartStartDate = DATEADD(MONTH, -2, DATEFROMPARTS(YEAR(@EndDate), MONTH(@EndDate), 1));  
    ELSE IF @ChartPeriodType = 'quarters'  
        SET @ChartStartDate = DATEADD(QUARTER, -2, DATEFROMPARTS(YEAR(@EndDate), ((DATEPART(QUARTER, @EndDate) - 1) * 3 + 1), 1));  
    ELSE IF @ChartPeriodType = 'years'  
        SET @ChartStartDate = DATEFROMPARTS(YEAR(@EndDate) - 2, 1, 1); 
    WITH 
        PeriodRange 
        AS 
        ( 
                            SELECT 
                    0 AS N, 
                    CAST(  
                CASE  
                    WHEN @ChartPeriodType = 'months' THEN DATEADD(MONTH, -2, DATEFROMPARTS(YEAR(@EndDate), MONTH(@EndDate), 1))  
                    WHEN @ChartPeriodType = 'quarters' THEN DATEADD(QUARTER, -2, DATEFROMPARTS(YEAR(@EndDate), ((DATEPART(QUARTER, @EndDate)-1)*3 + 1), 1))  
                    WHEN @ChartPeriodType = 'years' THEN DATEFROMPARTS(YEAR(@EndDate) - 2, 1, 1)  
                END AS DATE) AS PeriodStart 
            UNION ALL 
                SELECT 
                    N + 1, 
                    CAST(  
                CASE  
                    WHEN @ChartPeriodType = 'months' THEN DATEADD(MONTH, 1, PeriodStart)  
                    WHEN @ChartPeriodType = 'quarters' THEN DATEADD(QUARTER, 1, PeriodStart)  
                    WHEN @ChartPeriodType = 'years' THEN DATEADD(YEAR, 1, PeriodStart)  
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
                    WHEN @ChartPeriodType = 'months' THEN FORMAT(DATEFROMPARTS(YEAR(PaymentDate), MONTH(PaymentDate), 1), 'yyyy-MM')  
                    WHEN @ChartPeriodType = 'quarters' THEN CONCAT('Q', DATEPART(QUARTER, PaymentDate), '-', YEAR(PaymentDate))  
                    WHEN @ChartPeriodType = 'years' THEN FORMAT(PaymentDate, 'yyyy') -- Ensure this is also the FORMAT function for consistency  
                END AS PeriodKey, 
                SUM(ISNULL(PaidAmount, 0)) AS TotalPayment 
            FROM PurchaseOrders 
            WHERE  
                PaymentDate >= @ChartStartDate AND PaymentDate < DATEADD(DAY, 1, @EndDate) 
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
                    WHEN @ChartPeriodType = 'months' THEN FORMAT(DATEFROMPARTS(YEAR(PaymentDate), MONTH(PaymentDate), 1), 'yyyy-MM')  
                    WHEN @ChartPeriodType = 'quarters' THEN CONCAT('Q', DATEPART(QUARTER, PaymentDate), '-', YEAR(PaymentDate)) -- This was previously 'CONCAT(YEAR(PaymentDate), '-', DATEPART(QUARTER, PaymentDate))' - now corrected  
                    WHEN @ChartPeriodType = 'years' THEN FORMAT(PaymentDate, 'yyyy') -- This was previously 'CAST(YEAR(PaymentDate) AS VARCHAR(4))' - now corrected  
                END 
        ) 
    SELECT @threeMonthPerfOfLgdInUser = (  
        SELECT 
            CASE  
                WHEN @ChartPeriodType = 'months' THEN FORMAT(PeriodStart, 'yyyy-MM')  
                WHEN @ChartPeriodType = 'quarters' THEN CONCAT('Q', DATEPART(QUARTER, PeriodStart), '-', YEAR(PeriodStart))  
                WHEN @ChartPeriodType = 'years' THEN FORMAT(PeriodStart, 'yyyy')  
            END AS PaymentDate, 
            ISNULL(a.TotalPayment, 0) AS TotalPayment 
        FROM PeriodRange r 
            LEFT JOIN AggregatedData a ON  
            (  
                (@ChartPeriodType = 'months' AND FORMAT(r.PeriodStart, 'yyyy-MM') = a.PeriodKey) 
                OR (@ChartPeriodType = 'quarters' AND CONCAT('Q', DATEPART(QUARTER, r.PeriodStart), '-', YEAR(r.PeriodStart)) = a.PeriodKey) 
                OR (@ChartPeriodType = 'years' AND FORMAT(r.PeriodStart, 'yyyy') = a.PeriodKey)  
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
        AND (   
        NOT EXISTS (SELECT 1 
        FROM #UsersIds) 
        OR po.CreatedBy IN (SELECT Id 
        FROM #UsersIds)   
    ) 
        AND (po.IsExpired = 0 OR po.IsExpired IS NULL) 
        AND po.IsActive = 1 
        AND (us.IsDisabled = 0 OR us.IsDisabled IS NULL) 
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
            INNER JOIN PurchaseOrders AS po ON po.CreatedBy = u.PublicKey 
            INNER JOIN [Status] s ON po.[Status] = s.Id 
        WHERE    
            po.PaymentDate BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) 
            AND (   
                NOT EXISTS (SELECT 1 
            FROM #UsersIds) 
            OR po.CreatedBy IN (SELECT Id 
            FROM #UsersIds)   
            ) 
            AND (po.IsExpired = 0 OR po.IsExpired IS NULL) 
            AND po.IsActive = 1 
            AND (u.IsDisabled = 0 OR u.IsDisabled IS NULL) 
            AND (u.IsDelete = 0 OR u.IsDelete IS NULL) 
        ORDER BY en.CreatedOn DESC 
        FOR JSON PATH   
    ); 
 
   ;WITH LeadClassification AS ( 
    SELECT 
        po.LeadId, 
        CASE  
            WHEN COUNT(po.LeadId) > 1 THEN 'Returning' 
            ELSE 'New' 
        END AS CustomerType 
    FROM PurchaseOrders po 
    WHERE  
        po.IsActive = 1 
        AND po.STATUS IN (SELECT Id FROM STATUS WHERE code IN ('cus', 'app')) 
        -- Optional: consider only confirmed customers 
    GROUP BY po.LeadId 
), 
FilteredPurchaseOrders AS ( 
    SELECT 
        CAST(po.StartDate AS DATE) AS ServiceStartDate, 
        lc.CustomerType 
    FROM  LeadClassification lc 
    INNER JOIN PurchaseOrders po ON lc.LeadId = po.LeadId 
    WHERE  
        po.IsActive = 1 
        AND po.StartDate BETWEEN @StartDate AND @EndDate 
        AND po.STATUS IN (SELECT Id FROM STATUS WHERE code IN ('cus', 'app')) 
        AND ( 
            NOT EXISTS (SELECT 1 FROM #UsersIds) 
            OR po.CreatedBy IN (SELECT Id FROM #UsersIds) 
        ) 
), 
DailyCounts AS ( 
    SELECT 
        ServiceStartDate AS [Date], 
        COUNT(CASE WHEN CustomerType = 'New' THEN 1 END) AS [New], 
        COUNT(CASE WHEN CustomerType = 'Returning' THEN 1 END) AS [Returning] 
    FROM FilteredPurchaseOrders 
    GROUP BY ServiceStartDate 
) 
SELECT @MonthlyCustomerDetails = ( 
    SELECT 
        FORMAT([Date], 'yyyy-MM-dd') AS [Date], 
        [New], 
        [Returning] 
    FROM DailyCounts 
    WHERE ([New] + [Returning]) > 0  
    ORDER by [Date] 
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
        @LeadSoruceEarnings as LeadSoruceEarnings 
END 

----------------------------------------------------------------------------------------------------------------------------------------------------  
  
-- 01 July 2025 -- Ragesh M  -- Changes to get the total Partners  
  
----------------------------------------------------------------------------------------------------------------------------------------------------  
  GO
CREATE PROCEDURE [dbo].[GetDashboard]   
AS BEGIN   
    DROP TABLE IF EXISTS #PartnerAccountsTemp  
 
    SELECT *  
    INTO #PartnerAccountsTemp from  PartnerAccounts 
    Where ISNULL(IsDelete, 0) = 0 
    AND ISNULL(IsDisabled, 0) = 0 
   
	DECLARE @FreshPartners int, @PendingPartners int , @AcceptedPartners int, @RejectedPartners int, @totalPartners int    
   
	SELECT  @FreshPartners    = count(1) FROM #PartnerAccountsTemp WHERE Status NOT IN (1,2,3,4,5,6) or Status IS NULL -- Fresh    
	SELECT  @PendingPartners  = count(1) FROM #PartnerAccountsTemp WHERE Status = 1 -- Pending   
	SELECT  @AcceptedPartners = count(1) FROM #PartnerAccountsTemp WHERE Status = 2 -- Accepted    
	SELECT  @RejectedPartners = count(1) FROM #PartnerAccountsTemp WHERE Status = 3 -- Rejected  
    SELECT  @totalPartners    = count(1) FROM #PartnerAccountsTemp                  -- Total Partners  
    
   
	select @FreshPartners as FreshPartners   , @PendingPartners as PendingPartners , @AcceptedPartners as AcceptedPartners ,   
	@RejectedPartners as RejectedPartners , @totalPartners as TotalPartners  
   
   
END 
--Created By Siva 9 July 2025 1:57 PM ---
IF COL_LENGTH('PromotionM', 'ShouldDisplay') IS NULL
BEGIN
    ALTER TABLE PromotionM ADD ShouldDisplay BIT NULL
END


---------------------------------------------------------------------------------------------------------------------------------------

-- 01 July 2025 -- Ragesh M  -- Changes to get the total Partners  

---------------------------------------------------------------------------------------------------------------------------------------