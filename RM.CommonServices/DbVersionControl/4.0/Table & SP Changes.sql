ALTER PROCEDURE GetSubscriptionDetails          
    @PageSize INT,          
    @PageNumber INT,          
    @SearchText NVARCHAR(255) = NULL,          
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
        ((p.Price * sd.Months) * (1 - (sm.DiscountPercentage / 100.0))) AS DiscountPrice          
    FROM           
        SubscriptionMappingM AS sm
    INNER JOIN           
        ProductsM AS p ON sm.ProductId = p.Id          
    INNER JOIN           
        SubscriptionPlanM AS sp ON sm.SubscriptionPlanId = sp.Id          
    INNER JOIN           
        SubscriptionDurationM AS sd ON sm.SubscriptionDurationId = sd.Id          
    WHERE           
        (@SearchText IS NULL OR p.Name LIKE '%' + @SearchText + '%')       
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
        (@SearchText IS NULL OR p.Name LIKE '%' + @SearchText + '%')       
        AND (p.IsDeleted = 0);        
END; 


GO



----------- Created By Guna surya on 2025-02-19 ----------- 

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubscriptionMappingM' AND COLUMN_NAME = 'ModifiedBy')
BEGIN
    ALTER TABLE SubscriptionMappingM ADD ModifiedBy UNIQUEIDENTIFIER NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'SubscriptionPlanM' AND COLUMN_NAME = 'ModifiedBy')
BEGIN
    ALTER TABLE SubscriptionPlanM ADD ModifiedBy UNIQUEIDENTIFIER NULL;
END


---------- Modified By Vijay Sahu on 2025-02-19 

GO
alter PROCEDURE GetSubscriptionDetails              
    @PageSize INT,              
    @PageNumber INT,              
    @SearchText NVARCHAR(255) = NULL,              
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
        (@SearchText IS NULL OR p.Name LIKE '%' + @SearchText + '%')           
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
        (@SearchText IS NULL OR p.Name LIKE '%' + @SearchText + '%')           
        AND (p.IsDeleted = 0);            
END; 

GO


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
 @FifthKey varchar(50) ='',                          
 @CreatedBy varchar(50) =null,                          
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
                          
                          
 SELECT @TotalCount = count(1)                          
 From Leads as Leads                          
 LEFT JOIN Services as Services on Leads.ServiceKey = Services.PublicKey                          
 LEFT JOIN LeadTypes as LeadTypes on LeadTypes.PublicKey = Leads.LeadTypeKey                          
 LEFT JOIN LeadSources as LeadSource on LeadSource.PublicKey = Leads.LeadSourceKey                          
 LEFT JOIN Users AS Users on Users.PublicKey = Leads.AssignedTo                          
 LEFT JOIN Status as st on st.id = Leads.StatusId                          
 Left join PurchaseOrders as po on po.LeadId = Leads.Id    and Status <>  (select id From status where Code = 'com')   --COM NO NEED TO FETCH EXPIRED                       
 LEFT JOIN Status as st1 on st1.id = po.Status                          
 WHERE                          
 ISNULL(leads.IsDelete,0) = 0                           
 and (                          
 Leads.MobileNumber LIKE '%' + @SearchText +'%' OR @SearchText IS NULL OR           
  Leads.AlternateMobileNumber LIKE '%' + @SearchText +'%' OR           
        
 Leads.EmailId LIKE '%'+ @SearchText +'%' OR                           
 Leads.FullName LIKE '%'+ @SearchText +'%'                          
 )                          
   AND (@CreatedBy IS NULL OR Leads.AssignedTo = @CreatedBy)                        
AND (                  
    (po.status <> 24 AND ISNULL(po.IsExpired, 0) = 0) -- not customer and not expired                  
    OR                  
    (po.status = 24 AND ISNULL(po.IsExpired, 0) = 1) -- is customer and expired               
 OR                  
    (po.status = 4 AND ISNULL(po.IsExpired, 0) = 1) -- is not customer and expired        
    OR                  
    (po.status IS NULL) -- no po status i.e., no PR's yet                  
)                    
 AND                           
 ( @FifthKey = 'assigned'   AND leads.AssignedTo IS NOT NULL OR                            
   @FifthKey = 'unassigned' AND leads.AssignedTo IS NULL OR                    
  ISNULL(@FifthKey,'') = ''                          
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
 Users.FirstName as  [CreatedBy],                          
 Leads.[ModifiedOn],                          
 Leads.[ModifiedBy],                           
 Users.FirstName as [AssignedTo],                           
 Leads.StatusId as [StatusId] ,                          
 st.Name as StatusName,                          
 st1.name as poStatus,                          
 Leads.PurchaseOrderKey                         
  INTO #tempCustomer                          
  From Leads as Leads                          
 LEFT JOIN Services as Services on Leads.ServiceKey = Services.PublicKey                          
 LEFT JOIN LeadTypes as LeadTypes on LeadTypes.PublicKey = Leads.LeadTypeKey                          
 LEFT JOIN LeadSources as LeadSource on LeadSource.PublicKey = Leads.LeadSourceKey                          
 LEFT JOIN Users AS Users on Users.PublicKey = Leads.AssignedTo                          
 LEFT JOIN Status as st on st.id = Leads.StatusId                          
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
   AND (@CreatedBy IS NULL OR Leads.AssignedTo = @CreatedBy)                          
AND (                  
    (po.status <> 24 AND ISNULL(po.IsExpired, 0) = 0) -- not customer and not expired                  
    OR                  
    (po.status = 24 AND ISNULL(po.IsExpired, 0) = 1) -- is customer and expired                         
 OR                  
    (po.status = 4 AND ISNULL(po.IsExpired, 0) = 1) -- is not customer and expired           
 OR       
    (po.status IS NULL)-- no po status i.e., no PR's yet                  
       
)                    
 AND                                                   
 ( @FifthKey = 'assigned'   AND leads.AssignedTo IS NOT NULL OR                            
   @FifthKey = 'unassigned' AND leads.AssignedTo IS NULL OR                           
  ISNULL(@FifthKey,'') = ''                          
 )                           
 AND (@CreatedBy IS NULL OR Leads.AssignedTo = @CreatedBy)                        
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
 --select * from #tempCustomer                          
END  



----------- Created By Guna surya on 2025-02-20 -----------
GO
alter PROCEDURE GetExpiredServiceFromMyBucket          
AS          
BEGIN          
    SET NOCOUNT ON;          
      
    SELECT       
     STRING_AGG(CAST(mb.Id AS NVARCHAR), ', ') AS MyBucketIds,       
        mb.MobileUserKey,        
        STRING_AGG(p.Code, ', ') AS Topics,          
        STRING_AGG(mb.ProductName, ', ') AS ProductNames,          
        MAX(mu.FirebaseFcmToken) AS FirebaseFcmToken,          
        'Expiring Soon' AS Status        
    FROM MYBucketM AS mb          
    INNER JOIN MobileUsers AS mu ON mb.MobileUserKey = mu.Publickey          
    INNER JOIN ProductsM AS p ON mb.productId = p.Id          
    WHERE          
        mb.IsActive = 1  
  and p.IsActive = 1  
        AND DATEDIFF(DAY, GETDATE(), mb.EndDate) BETWEEN 0 AND 4          
    GROUP BY mb.MobileUserKey      
    UNION ALL      
      
    SELECT          
  STRING_AGG(CAST(mb.Id AS NVARCHAR), ', ') AS MyBucketIds,       
        mb.MobileUserKey,        
        STRING_AGG(p.Code, ', ') AS Topics,          
        STRING_AGG(mb.ProductName, ', ') AS ProductNames,          
        MAX(mu.FirebaseFcmToken) AS FirebaseFcmToken,          
        'Expired' AS Status        
    FROM MYBucketM AS mb          
    INNER JOIN MobileUsers AS mu ON mb.MobileUserKey = mu.Publickey          
    INNER JOIN ProductsM AS p ON mb.productId = p.Id          
    WHERE          
        mb.IsActive = 1  
  and p.IsActive = 1    
        AND DATEDIFF(DAY, GETDATE(), mb.EndDate) <= -1          
    GROUP BY mb.MobileUserKey;      
END; 
GO  

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
                WHEN p.isActive = 0 THEN ''   
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
GO


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
      
    DECLARE @extraBenefits NVARCHAR(MAX) = (      
                                               SELECT * FROM #tempBenefits FOR JSON AUTO      
                                           )      
    DECLARE @CurrentDate DATE = cast(getdate() AS DATE)      
    DECLARE @IsOutOfSubscription VARCHAR(300) = (      
                                                    SELECT TOP 1      
                                                        mobileUserKey      
                                                    FROM MYBucketM b      
                                                    WHERE productId = @ProductId      
                                                          AND mobileUserKey = @MobileUserKey      
                                                          AND ISNULL(IsACtive, 1) = 1      
                                                          AND isnull(IsExpired, 0) = 0      
                                                          AND @currentDate >= cast(b.StartDate AS DATE)      
                                                          AND @currentDate <= cast(b.endDate AS DATE)      
                                                )      
    DECLARE @isExpired NVARCHAR(max) = (      
                                           SELECT TOP 1      
                                               IsExpired      
                                           FROM MYBucketM      
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
            SELECT top 1      
                sv.DurationName      
            FROM SubscriptionView sv      
            WHERE ProductId = @ProductId      
                  and sv.DurationName != 'free'      
                  AND sv.DurationActive = 1      
        ) AS SubscriptionData,      
        CAST(ISNULL(pl.LikeId, 0) AS BIT) AS IsHeart,      
        CAST(0 AS BIT) AS IsThumbsUp,      
        @extraBenefits AS ExtraBenefits,      
        CAST(CASE      
                 WHEN DATEDIFF(day, GETDATE(), mb.enddate) <= @priorDaysInfo THEN      
                     1      
                 ELSE      
                     0      
             END AS BIT) AS ShowReminder,      
        CAST(IIF(@isExpired IS NOT NULL, 1, 0) AS BIT) AS IsInMyBucket,      
        P.LandscapeImage AS LandscapeImage,      
        CAST(IIF(@IsOutOfSubscription IS NULL, 0, 1) AS BIT) AS IsInValidity,      
        (      
            SELECT * FROM ProductsContentM WHERE productId = @ProductId FOR JSON AUTO      
        ) AS Content,      
        (      
        --buy button text                                            
      CASE 
    -- Case for Active Products
			WHEN p.isActive = 1 THEN 
				CASE 
					WHEN mb.id IS NULL THEN 'Buy'  
					ELSE 
						CASE 
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
		END) AS BuyButtonText,

        @ContentCount AS ContentCount,      
        @VideoContent AS VideoCount,      
        DATEDIFF(day, getdate(), ISNULL(mb.enddate, GETDATE())) AS DaysToGo,    
    (           CASE               WHEN mb.id IS NOT NULL AND DATEDIFF(day, GETDATE(), mb.enddate) >= @priorDaysInfo               THEN '[]'               ELSE                   (                       SELECT pb.BonusProductId AS Id,                      
  
        p2.NAME AS BonusProductName,                              pb.DurationInDays AS Validity,                                
  (SELECT s.Value FROM Settings s WHERE s.Code = 'BonusMessage' AND s.IsActive = 1) AS BonusMessage  FROM ProductBonusMappingM pb    
  INNER JOIN ProductsM p2 ON pb.BonusProductId = p2.Id LEFT JOIN ProductCategoriesM pcm2 ON p2.CategoryID = pcm2.Id   
  WHERE pb.ProductId = p.Id AND pb.IsActive = 1  FOR JSON PATH ) END       ) AS BonusProducts      
    FROM ProductsM AS P      
        INNER JOIN ProductCategoriesM AS pcm      
            ON p.CategoryID = pcm.Id      
        LEFT JOIN PurchaseOrdersM AS POM      
            ON POM.ProductId = p.Id      
               AND pom.ProductId = @ProductId      
               AND pom.ActionBy = @MobileUserKey      
        LEFT JOIN ProductsRatingM AS PR      
            ON PR.ProductId = P.Id      
               AND PR.CreatedBy = @MobileUserKey      
        LEFT JOIN ProductLikesM AS pl      
            ON pl.ProductId = p.Id      
               AND pl.LikeId = 1      
               AND pl.CreatedBy = @MobileUserKey      
               AND pl.IsDelete = 0      
        LEFT JOIN subscriptiondurationm s      
            ON s.Id = p.SubscriptionId      
        LEFT JOIN MYBucketM AS Mb      
            ON p.id = mb.ProductId      
               AND mb.mobileuserkey = @MobileUserkey      
    WHERE p.id = @ProductId      
    ORDER BY POM.CreatedOn DESC      
END


----------- Created By Vijay Sahu on 2025-02-24 ----------- 
GO
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'MobileUsers' AND COLUMN_NAME = 'DeviceVersion')
BEGIN
    ALTER TABLE MobileUsers ADD DeviceVersion UNIQUEIDENTIFIER NULL;
END

GO
--select * From ScheduledNotificationM       
--select * From Settings      
--EXEC GetTargetAudianceListForPushNotification 'PURCHASEDANY', '',''    
ALTER PROCEDURE GetTargetAudianceListForPushNotification        
    @AudianceCategory VARCHAR(20),        
    @topic VARCHAR(50),        
    @mobile VARCHAR(MAX) -- Use MAX to handle multiple numbers            
AS        
BEGIN       
    DECLARE @MobileUsers TABLE        
    (        
        FirebaseFcmToken VARCHAR(200),        
        PublicKey UNIQUEIDENTIFIER,        
        FullName VARCHAR(50),
        OldDevice BIT
    );      

    DECLARE @MobileList TABLE (Mobile VARCHAR(20));        

    -- Parse the comma-separated @mobile input into individual rows            
    IF @mobile IS NOT NULL        
    BEGIN        
        INSERT INTO @MobileList (Mobile)        
        SELECT value FROM STRING_SPLIT(@mobile, ',');        
    END        

    -- Handle different audience categories
    IF UPPER(@AudianceCategory) = 'ALL'        
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
            END AS OldDevice        
        FROM MobileUsers mu        
        WHERE ISNULL(mu.isActive, 1) = 1        
              AND ISNULL(mu.IsDelete, 0) = 0;
    END        
    ELSE IF UPPER(@AudianceCategory) = 'PURCHASEDANY'        
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
            END AS OldDevice        
        FROM MobileUsers mu        
        INNER JOIN PurchaseOrdersM AS pom ON mu.PublicKey = pom.ActionBy        
        WHERE ISNULL(mu.isActive, 1) = 1        
              AND ISNULL(mu.IsDelete, 0) = 0        
              AND pom.TransasctionReference <> 'WITHOUTPAYMENT';        
    END        
    ELSE IF UPPER(@AudianceCategory) = 'MOBILE'        
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
            END AS OldDevice        
        FROM MobileUsers mu        
        WHERE ISNULL(mu.isActive, 1) = 1        
              AND ISNULL(mu.IsDelete, 0) = 0        
              AND EXISTS        
              (        
                  SELECT 1 FROM @MobileList ml WHERE ml.Mobile = mu.Mobile        
              );        
    END        

    SELECT * FROM @MobileUsers;        
END;
GO



------------------------- Created By Guna Surya on 2025-02-24 ----------- 
GO
alter PROCEDURE GetLeadFreeTrials    
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
    
    SELECT @TotalCount = COUNT(1)    
    FROM LeadFreeTrial lt    
    inner JOIN Leads Lead ON lt.LeadKey = Lead.PublicKey    
    left JOIN Services service ON lt.ServiceKey = service.PublicKey    
    left JOIN Users u ON lt.CreatedBy = u.PublicKey    
  WHERE (@FromDate IS NULL AND @ToDate IS NULL)  
   OR (CAST(lt.CreatedOn AS DATE) >= CAST(@FromDate AS DATE)  
       AND CAST(lt.CreatedOn AS DATE) <= CAST(@ToDate AS DATE))  
        AND ISNULL(service.PublicKey, '00000000-0000-0000-0000-000000000000') = ISNULL(@PrimaryKey, ISNULL(service.PublicKey, '00000000-0000-0000-0000-000000000000'))    
        AND (@SearchText IS NULL 
		    Or lead.MobileNumber like '%' + @SearchText + '%'
            OR Lead.FullName LIKE '%' + @SearchText + '%');    
    
    SELECT lt.Id,    
           Lead.FullName AS LeadName,    
           Lead.MobileNumber AS LeadNumber,    
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
          CASE    
           WHEN CAST(GETDATE() AS DATE) < CAST(lt.StartDate AS DATE) 
           THEN DATEDIFF(DAY, lt.StartDate, lt.EndDate)  
           ELSE DATEDIFF(DAY, CAST(GETDATE() AS DATE), lt.EndDate) 
       END AS Validity,    
           (SELECT COUNT(*) FROM LeadFreeTrailReasonLog WHERE LeadFreeTrialId = lt.Id) AS ReasonLogCount,    
           CASE    
               WHEN CAST(lt.EndDate AS DATE) < CAST(GETDATE() AS DATE) THEN 'Expired'    
               WHEN lt.IsActive = 1 THEN 'Active'    
               ELSE 'Inactive' END AS StatusText    
    FROM LeadFreeTrial lt    
    inner JOIN Leads Lead ON lt.LeadKey = Lead.PublicKey    
    left JOIN Services service ON lt.ServiceKey = service.PublicKey    
    left JOIN Users u ON lt.CreatedBy = u.PublicKey    
   WHERE (@FromDate IS NULL AND @ToDate IS NULL)  
   OR (CAST(lt.CreatedOn AS DATE) >= CAST(@FromDate AS DATE)  
       AND CAST(lt.CreatedOn AS DATE) <= CAST(@ToDate AS DATE))  
        AND ISNULL(service.PublicKey, '00000000-0000-0000-0000-000000000000') = ISNULL(@PrimaryKey, ISNULL(service.PublicKey, '00000000-0000-0000-0000-000000000000'))    
        AND (@SearchText IS NULL    
            OR Lead.FullName LIKE '%' + @SearchText + '%'
			Or lead.MobileNumber like '%' + @SearchText + '%'
			)    
    ORDER BY ISNULL(lt.ModifiedOn, lt.CreatedOn) DESC    
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;    
END;    

GO
INSERT INTO ActivityType (activitymessage)
SELECT 'FreeTrail Activated'
WHERE NOT EXISTS (SELECT 1 FROM ActivityType WHERE activitymessage = 'FreeTrail Activated')
UNION ALL
SELECT 'FreeTrail Deleted'
WHERE NOT EXISTS (SELECT 1 FROM ActivityType WHERE activitymessage = 'FreeTrail Deleted')
UNION ALL
SELECT 'FreeTrail Extended'
WHERE NOT EXISTS (SELECT 1 FROM ActivityType WHERE activitymessage = 'FreeTrail Extended');

GO

------------ Created By Guna Surya 25-feb-25-------------------------
GO
     
alter PROCEDURE  [dbo].[GetJunkLeads]                              
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
 @FifthKey varchar(50) ='',                              
 @CreatedBy varchar(50) =null,                              
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
   AND (@CreatedBy IS NULL OR Leads.AssignedTo = @CreatedBy)                            
AND (                      
    (po.status <> 24 AND ISNULL(po.IsExpired, 0) = 0) -- not customer and not expired                      
    OR                      
    (po.status = 24 AND ISNULL(po.IsExpired, 0) = 1) -- is customer and expired                   
 OR                      
    (po.status = 4 AND ISNULL(po.IsExpired, 0) = 1) -- is not customer and expired            
    OR                      
    (po.status IS NULL) -- no po status i.e., no PR's yet                      
)                        
 AND                               
 ( @FifthKey = 'assigned'   AND leads.AssignedTo IS NOT NULL OR                                
   @FifthKey = 'unassigned' AND leads.AssignedTo IS NULL OR                        
  ISNULL(@FifthKey,'') = ''                              
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
 Leads.MobileNumber LIKE '%' + @SearchText +'%' OR            
  Leads.AlternateMobileNumber LIKE '%' + @SearchText +'%'             
 OR @SearchText IS NULL OR    
 Leads.EmailId LIKE '%'+ @SearchText +'%' OR                               
 Leads.FullName LIKE '%'+ @SearchText +'%'                              
 )                              
   AND (@CreatedBy IS NULL OR Leads.AssignedTo = @CreatedBy)                            
AND (                      
    (po.status <> 24 AND ISNULL(po.IsExpired, 0) = 0) -- not customer and not expired                      
    OR                      
    (po.status = 24 AND ISNULL(po.IsExpired, 0) = 1) -- is customer and expired                             
 OR                      
    (po.status = 4 AND ISNULL(po.IsExpired, 0) = 1) -- is not customer and expired               
 OR           
    (po.status IS NULL)-- no po status i.e., no PR's yet                      
           
)                        
 AND                                                       
 ( @FifthKey = 'assigned'   AND leads.AssignedTo IS NOT NULL OR                                
   @FifthKey = 'unassigned' AND leads.AssignedTo IS NULL OR                               
  ISNULL(@FifthKey,'') = ''                              
 )                               
 AND (@CreatedBy IS NULL OR Leads.AssignedTo = @CreatedBy)                            
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
GO


------------ Created By Vijay Sahu 25-feb-25-------------------------
GO
--exec [dbo].[OtpLogin] '1234567895', null,null,null,'91','123456'
CREATE PROCEDURE [dbo].[OtpLogin]                
    @MobileNumber VARCHAR(20) = NULL,                
    @CreatedBy UNIQUEIDENTIFIER = NULL,                
    @ModifiedBy UNIQUEIDENTIFIER = NULL,                
    @DeviceType VARCHAR(10) = NULL,                
    @CountryCode VARCHAR(10) = NULL,        
    @Otp VARCHAR(6) OUTPUT                
AS                
BEGIN                
    SET NOCOUNT ON;                
             
    DECLARE @MinuteDifference INT,                
            @IsSelfDeleteRequested BIT = 0,                
            @Result VARCHAR(100),                
            @PublicKey VARCHAR(50);                
                
    -- Generate OTP                                    
    SET @Otp = 123456 -- Replace with random OTP generation logic if required                                  
                
   
	-- CHECK IF USER HAS ALREADY REQUESTED FOR SELF DELETE IF YES THEN RESET THE DATA FOR THE SAME IsDelete , SelfDelete
	UPDATE MobileUsers SET IsDelete = 0 , IsActive = 1 WHERE Mobile = @MobileNumber 
	
	-- Fetch minute difference for retry attempts                                    
	SELECT TOP 1 @MinuteDifference = DATEDIFF(MINUTE, ModifiedOn, GETDATE()) FROM MobileUsers WHERE Mobile = @MobileNumber  AND IsDelete = 0 AND ISNULL(SelfDeleteRequest, 0) = 0;                
	 
    -- Check OTP Limit                                    
    IF (  @MinuteDifference < 30 AND EXISTS ( SELECT 1 FROM MobileUsers WHERE Mobile = @MobileNumber AND RetryAttempt > 2  and IsActive = 1 and IsDelete = 0 ))                
    BEGIN 
	       
        SET @Result = 'OTPLIMITREACHED';                
        SELECT @Result AS Result,                
               FullName,                
               EmailId,                
               'Otp Limit Reached. Try again after ' + CAST(30 - @MinuteDifference AS VARCHAR) + ' minutes' AS Message,                
               '' AS Otp,                
               PublicKey,                
               OneTimePassword,                
               ProfileImage,                
               FirebaseFcmToken,                
               CAST(0 AS BIT) AS IsExistingUser,                
               @IsSelfDeleteRequested AS IsSelfDeleteRequested,                
               (                
                   SELECT TOP 1 PublicKey FROM Leads WHERE MobileNumber = @MobileNumber                
               ) AS LeadKey                
        FROM MobileUsers                
        WHERE Mobile = @MobileNumber and IsDelete = 0;                
    END  
	
    -- Handle Existing Users                                    
    IF EXISTS                
    (                
        SELECT 1                
        FROM MobileUsers                
        WHERE Mobile = @MobileNumber AND IsDelete = 0 AND IsActive = 1                
              AND (                
                      SelfDeleteRequestDate IS NULL                
                      OR cast(SelfDeleteRequestDate as date) >= CAST(GETDATE() AS DATE)                
                  )                
    )                
    BEGIN         
	 
		-- Update OTP and Retry Attempt
		UPDATE MobileUsers 
		SET OneTimePassword = @Otp, 
			ModifiedOn = GETDATE(), 
			LastLoginDate = GETDATE(),
			RetryAttempt = CASE WHEN @MinuteDifference > 30 THEN 1 ELSE RetryAttempt + 1 END
		WHERE Mobile = @MobileNumber;
		 
        -- Return OTP Sent Response                                    
		SELECT 'OTPSENT' AS Result,                
			FullName,                
			EmailId,                
			'OTP Sent Successfully.' AS Message,                
			'' AS Otp,                
			PublicKey,                
			OneTimePassword,                
			ProfileImage,                
			FirebaseFcmToken,                
			CAST(1 AS BIT) AS IsExistingUser,                
			@IsSelfDeleteRequested AS IsSelfDeleteRequested                
		FROM MobileUsers                
		WHERE Mobile = @MobileNumber                
			and IsActive = 1                
			and IsDelete = 0;     
			

		-- Reset Self-Delete Request if conditions are met
		IF EXISTS (
			SELECT 1 FROM MobileUsers 
			WHERE Mobile = @MobileNumber AND IsDelete = 0 AND IsActive = 1 
				  AND (SelfDeleteRequestDate IS NULL OR SelfDeleteRequestDate >= CAST(GETDATE() AS DATE)) 
				  AND SelfDeleteRequest = 1
		) 
		BEGIN
			UPDATE MobileUsers 
			SET SelfDeleteRequest = NULL, SelfDeleteRequestDate = NULL WHERE Mobile = @MobileNumber;
		END


                       
    END                
    ELSE IF EXISTS                
    (                
        SELECT 1 FROM MobileUsers WHERE Mobile = @MobileNumber AND IsDelete = 0 AND IsActive = 1 and ISNULL(SelfDeleteRequest, 0) = 0                
    )                
    BEGIN    
  
  --      -- Insert lead                            
		--INSERT INTO Leads (FullName, MobileNumber, CreatedBy, CreatedOn, ModifiedOn, ModifiedBy, Remarks) 
		--VALUES ('', @MobileNumber, (SELECT Value FROM Settings WHERE Code = 'admin'), GETDATE(), GETDATE(), (SELECT Value FROM Settings WHERE Code = 'admin'), 'Mobile app user');
		INSERT INTO MobileUsers (FullName, LeadKey, Password, OneTimePassword, IsOtpVerified, Mobile, DeviceType, CreatedOn, ModifiedOn, MobileToken, IMEI, StockNature, AgreeToTerms, SameForWhatsApp, IsActive, IsDelete, ProfileImage, About, RegistrationDate, LastLoginDate, EmailId, CountryCode, RetryAttempt) 
		VALUES ('', (SELECT publickey FROM leads WHERE id = @@IDENTITY), '123456', @Otp, 0, @MobileNumber, ISNULL(@DeviceType, ''), GETDATE(), GETDATE(), 'REGISTERED', '', '', 1, 1, 1, 0, NULL, '213', GETDATE(), GETDATE(), '', @CountryCode, 1);

                
        SET @Result = 'REGISTERED';                
        SELECT TOP 1                
			@Result AS Result,                
            FullName,                
            EmailId,                
            'Registration Successfull please enter OTP.' AS Message,                
            '' AS Otp,                
            PublicKey,                
            OneTimePassword,                
            ProfileImage,                
            FirebaseFcmToken,                
            cast(0 as bit) AS IsExistingUser,                
            @PublicKey AS LeadKey,                
            @IsSelfDeleteRequested AS IsSelfDeleteRequested                
        FROM MobileUsers                
        WHERE ID = SCOPE_IDENTITY()                
		print('inside')              
                        
    END                
                
    -- Handle Self-Deleted Users                                    
    IF EXISTS ( SELECT 1 FROM MobileUsers WHERE Mobile = @MobileNumber AND IsActive = 1 AND SelfDeleteRequest = 1 AND cast(SelfDeleteRequestDate as Date) <= cast(GETDATE() as Date))                
    BEGIN      
		DELETE from MobileUsers WHERE Mobile = @MobileNumber AND IsActive = 1  AND SelfDeleteRequest = 1 AND cast(SelfDeleteRequestDate as Date) <= cast(GETDATE() as Date)               
                
        -- Insert Lead if not exists                                    
                
		--INSERT INTO Leads (FullName, MobileNumber, CreatedBy, CreatedOn, ModifiedOn, ModifiedBy, Remarks) 
		--VALUES ('', @MobileNumber, (SELECT Value FROM Settings WHERE Code = 'admin'), GETDATE(), GETDATE(), (SELECT Value FROM Settings WHERE Code = 'admin'), 'Mobile app user');
      
                
        -- Insert New User                                    
        INSERT INTO MobileUsers                
        (                
            FullName,  LeadKey, Password,OneTimePassword,IsOtpVerified,Mobile,DeviceType,CreatedOn,ModifiedOn,MobileToken,IMEI,StockNature, AgreeToTerms,                
            SameForWhatsApp,IsActive,IsDelete, ProfileImage,About, RegistrationDate,LastLoginDate, EmailId,  CountryCode,RetryAttempt)                
        VALUES                
        ( '',                
            ( select publickey from leads where id = @@IDENTITY  ), '123456',@Otp,0,@MobileNumber,ISNULL(@DeviceType, ''),GETDATE(),GETDATE(),'REGISTERED','', '',                
            1, 1, 1, 0,NULL,'213',  GETDATE(), GETDATE(),'', @CountryCode, 1);                
                
        -- Return Success                                    
        SELECT TOP 1                
            'REGISTERED' AS Result,                
            FullName,                
            EmailId,                
            'Registration Successful. Please enter OTP.' AS Message,                
            '' AS Otp,                
            PublicKey,                
            OneTimePassword,                
            ProfileImage,                
            FirebaseFcmToken,                
            CAST(0 AS BIT) AS IsExistingUser,                
            PublicKey AS LeadKey,                
            @IsSelfDeleteRequested AS IsSelfDeleteRequested                
        FROM MobileUsers                
        WHERE Mobile = @MobileNumber and IsDelete = 0  ;                
                     
    END                
                
    IF NOT EXISTS (SELECT 1 FROM MobileUsers WHERE Mobile = @MobileNumber)                
    BEGIN                
        -- Insert Lead if not exists                                    
         
		--INSERT INTO Leads (FullName, MobileNumber, ServiceKey, CreatedBy, CreatedOn, ModifiedOn, ModifiedBy, Remarks) 
		--VALUES ('', @MobileNumber, (SELECT PublicKey FROM Services WHERE Name = 'OPTIONS HNI'), (SELECT Value FROM Settings WHERE Code = 'admin'), GETDATE(), GETDATE(), (SELECT Value FROM Settings WHERE Code = 'admin'), 'Mobile app user');
       
                
		INSERT INTO MobileUsers (FullName, LeadKey, Password, OneTimePassword, IsOtpVerified, Mobile, DeviceType, CreatedOn, ModifiedOn, MobileToken, IMEI, StockNature, AgreeToTerms, SameForWhatsApp, IsActive, IsDelete, ProfileImage, About, RegistrationDate, LastLoginDate, EmailId, CountryCode, RetryAttempt) 
		VALUES ('', (SELECT publickey FROM leads WHERE id = @@IDENTITY), '123456', @Otp, 0, @MobileNumber, ISNULL(@DeviceType, ''), GETDATE(), GETDATE(), 'REGISTERED', '', '', 1, 1, 1, 0, NULL, '213', GETDATE(), GETDATE(), '', @CountryCode, 1);

        -- Return Success                                    
        SELECT TOP 1                
            'REGISTERED' AS Result,                
            FullName,                
            EmailId,                
            'Registration Successful. Please enter OTP.' AS Message,                
            '' AS Otp,                
            PublicKey,                
            OneTimePassword,                
            ProfileImage,                
            FirebaseFcmToken,                
            CAST(0 AS BIT) AS IsExistingUser,                
            PublicKey AS LeadKey,                
            @IsSelfDeleteRequested AS IsSelfDeleteRequested                
        FROM MobileUsers                
        WHERE Mobile = @MobileNumber and IsDelete = 0;                
                      
    END                
             
                
END;  



----------------------- Created By Guna Surya  28-feb-25-------------------------                
GO
alter PROCEDURE [dbo].[ActivateFreeTrialM]  
    @mobileUserKey UNIQUEIDENTIFIER  
AS  
BEGIN  
    BEGIN TRANSACTION;  
    BEGIN TRY  
        DECLARE @TrialInDays INT = 0,  
                @Result VARCHAR(100) = '',  
                @Message VARCHAR(500),  
                @InsertedCount INT = 0;  
  
        -- Get active and non-expired FreeTrialBasketM  
        SELECT @TrialInDays = DaysInNumber  
        FROM FreeTrialBasketM  
        WHERE ISNULL(isActive, 1) = 1  
          AND ISNULL(isExpired, 0) = 0;  
        
        -- Insert products into MYBucketM only if they are active and not deleted  
        INSERT INTO MYBucketM (MobileUserKey, ProductId, ProductName, StartDate, EndDate, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, IsActive, IsExpired, Notification)  
        SELECT @mobileUserKey,  
               ftbd.ProductId,  
               pm.Name,  
               GETDATE() AS StartDate,  
               DATEADD(day, @TrialInDays, GETDATE()) AS EndDate,  
               1 AS Status,  
               @mobileUserKey AS CreatedBy,  
               GETDATE() AS CreatedOn,  
               @mobileUserKey AS ModifiedBy,  
               GETDATE() AS ModifiedOn,  
               1 AS IsActive,  
               0 AS IsExpired,  
               1 AS Notification  
        FROM FreeTrialBasketDetailM ftbd  
        INNER JOIN ProductsM AS pm ON ftbd.ProductId = pm.Id  
        WHERE NOT EXISTS (SELECT 1  
                          FROM MYBucketM mb  
                          WHERE mb.ProductId = ftbd.ProductId  
                            AND MobileUserKey = @mobileUserKey)  
          AND pm.IsActive = 1   
          AND pm.IsDeleted = 0;  
  
        -- Count the number of inserted rows  
        SET @InsertedCount = @@ROWCOUNT;  
  
        -- Only register in FreeTrialM if at least one product was added to MYBucketM  
        IF @InsertedCount > 0  
        BEGIN  
            IF NOT EXISTS (SELECT 1 FROM FreeTrialM WHERE MobileUserKey = @mobileUserKey)  
            BEGIN  
                INSERT INTO FreeTrialM  
                SELECT @mobileUserKey,  
                       1,  
                       GETDATE() AS StartDate,  
                       DATEADD(day, @TrialInDays, GETDATE()) AS EndDate,  
                       1,  
                       GETDATE(),  
                       GETDATE(),  
                       Id  
                FROM FreeTrialBasketM  
                WHERE ISNULL(IsActive, 1) = 1  
                  AND ISNULL(isExpired, 0) = 0;  
            END  
  
            SET @Result = 'FreeTrialActivated';  
            SET @Message = 'Enjoy, your free trial activated! Visit MyBucket to check your subscription validity';  
        END  
        ELSE  
        BEGIN  
            SET @Result = 'NoEligibleProducts';  
            SET @Message = 'No active products available for a free trial.';  
        END  
  
        COMMIT TRANSACTION;  
    END TRY  
    BEGIN CATCH  
        ROLLBACK TRANSACTION;  
        SET @Result = 'failedPleaseTryAgain';  
        SET @Message = ERROR_MESSAGE();  
        INSERT INTO ExceptionLogs (ExceptionType, ErrorMessage, StackTrace, Description, Notes, CreatedDate)  
        VALUES ('ActivateFreeTrialM', ERROR_MESSAGE(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_LINE(), GETDATE());  
    END CATCH;  
  
    SELECT @Result AS Result,  
           @Message AS Message;  
END;


Go
alter PROCEDURE [dbo].[GetFreeTrial]  
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
        (@SearchText IS NULL OR MU.Mobile LIKE '%' + @SearchText + '%') OR  
        (@SearchText IS NULL OR MU.EmailId LIKE '%' + @SearchText + '%') OR  
        (@SearchText IS NULL OR MU.FullName LIKE '%' + @SearchText + '%')  
        AND (@FromDate IS NULL OR @ToDate IS NULL  
        OR CAST(MU.RegistrationDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)  
  
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

--28-02-2025 Ajith

GO
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
        p.CreatedDate,
        p.IsActive,
        p.imageUrl,
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
        p.IsActive,       
        p.DescriptionTitle
    ORDER BY MAX(p.id) DESC;
-- Ordering correctly

END;
GO
