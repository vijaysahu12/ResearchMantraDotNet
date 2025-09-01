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
    WHERE BasketId = @BasketId
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
GO


GO 
-- This procedure only getting call from Mobile Flutter while doing any payment using payment gateway 
-- Not:  Don't use this for CRM or any where elese to get the subscription in my bucket.  
-- @paidAmount means the final amount which has been paid by the client
--    exec ManagePurchaseOrderSm '49baf3e9-ee65-ef11-8178-00155d23d79c', 1  , 1,'TRANSACT12312313', 1  , null   
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
    BEGIN TRANSACTION;  
    BEGIN TRY  
        DECLARE @CurrentDate DATETIME =  getdate(); 
  
        INSERT INTO lOGS  
        VALUES  
        (CAST(@productId AS VARCHAR) + ', ' + @MerchantTransactionId + ', ' + CAST(@paidAmount AS VARCHAR) + ', '  
         +  @couponCode,  
         'ManagePurchaseOrderSM',  
         @CurrentDate  
        )  
  
        DROP TABLE IF EXISTS #TempTable  
  
        DECLARE @couponMOP INT,  
                @razorMOP INT;  
  
        SELECT @couponMOP = Id FROM PaymentModes WHERE name = 'Coupon';  
        SELECT @razorMOP = Id  FROM PaymentModes  WHERE name = 'Razor';  
  
        SELECT TOP 1 * INTO #TempTable FROM MobileUsers WHERE publickey = @MobileUserKey --'FE05087D-6AA6-EF11-B32F-852B0D908F81'            
  
        --select * From MobileUsers where LeadKey is null      
  
        IF NOT EXISTS (SELECT 1 FROM #TempTable WHERE LeadKey IS NOT NULL)  
        BEGIN  
            -- Check if MobileNumber exists in Leads     
            IF NOT EXISTS  
            (  
                SELECT 1  
                FROM Leads  
                WHERE MobileNumber =  
                (  
                    SELECT TOP 1 Mobile FROM #TempTable  
                )  
            )  
            BEGIN  
                --select '-- Insert a new row into Leads for the missing MobileNumber'     
  
               INSERT INTO Leads (SupervisorId, PublicKey, FullName, Gender, MobileNumber, AlternateMobileNumber, EmailId, ProfileImage, PriorityStatus, AssignedTo, ServiceKey, LeadTypeKey, 
               LeadSourceKey, Remarks, IsDisabled, IsDelete, CreatedOn, CreatedBy, IsSpam, IsWon, ModifiedOn, ModifiedBy, City, PinCode, StatusId, PurchaseOrderKey, Favourite, CountryCode) 
               SELECT NULL, NEWID(), FullName, Gender, Mobile, NULL, EmailId, NULL, 1, NULL, NULL, NULL, NULL, 'Reg. via mobile app', 0, 0, GETDATE(), 'Admin', 0, 0, GETDATE(), 'Admin', City, '', 1, NULL, 1, CountryCode FROM #TempTable; 
 
            END  
  
            -- Update MobileUsers with the LeadKey from Leads     
            UPDATE MobileUsers  
            SET LeadKey =  
                (  
                    SELECT TOP 1  
                        PublicKey  
                    FROM Leads  
                    WHERE MobileNumber =  
                    (  
                        SELECT TOP 1 Mobile FROM #TempTable  
                    )  
                )  
            WHERE Mobile =  
            (  
                SELECT TOP 1 Mobile FROM #TempTable  
            );  
  
            -- Update LeadKey in the temporary table     
            UPDATE #TempTable  
            SET LeadKey =  
                (  
                    SELECT TOP 1  
                        PublicKey  
                    FROM Leads  
                    WHERE MobileNumber =  
                    (  
                        SELECT TOP 1 Mobile FROM #TempTable  
                    )  
                );  
        END -- By this line of code.. we have fixed if leadKey from leads table is missing for mobileUser table 
  
        DECLARE @couponCodeExists BIT,  
                @discountAmount DECIMAL(18, 2),  
                @discountPercent INT,  
                @price DECIMAL(18, 2),  
                @couponkey UNIQUEIDENTIFIER,  
                @startDate DATETIME = @currentDate,  
                @endDate datetime,  
                @couponHasProductValidity bit,  
                @couponValidityDays int = 0,  
                @FinalValidityInDays int = 0,  
                @ProductName varchar(100) = '',  
                @validity int = null;  
  
        SELECT @couponkey = publickey, @validity = ProductValidityInDays  
        FROM CouponsM WHERE Name = @couponCode  AND IsActive = 1 AND IsDelete = 0 AND RedeemLimit > TotalRedeems  
  
        -- Calculate the NetAmount ,DiscountAmount based on SubscriptionMappingId      
        -- Because each product coming under atleast 1 plan , and price may very based on Plan or Durations.       
        SELECT @discountPercent = sm.DiscountPercentage,  
               @price = p.Price,  
               @discountAmount = ((p.Price * (sm.DiscountPercentage) / 100)),  
               @couponValidityDays = 0,  
               @endDate = DATEADD(month, sd.Months, GETDATE()),  
               @ProductName = p.Name , 
               @FinalValidityInDays = DATEDIFF(DAY, (@currentDate), (DATEADD(month, sd.Months, @currentDate))) - 1 
        FROM SubscriptionMappingM as sm  
            INNER JOIN SubscriptionDurationM as sd  
                on sm.SubscriptionDurationId = sd.Id  
            INNER JOIN ProductsM as p  
                on sm.ProductId = p.Id  
        WHERE sm.Id = @SubscriptionMappingId 
              and ProductId = 1  
              and sm.IsActive = 1  
  
        -- SET @paidAmount = @price - isnull(@discountAmount,0);  
  
        SELECT @couponHasProductValidity = IIF(ProductValidityInDays IS NULL, 0, 1),  
                @couponValidityDays = ProductValidityInDays, 
               @discountPercent = IIF(DiscountInPercentage IS NULL, 0, 1)  
        FROM CouponsM c WHERE publickey = @couponkey;  
  
        -- Now change the final Paid Amount after applying the coupon code on it.      
        -- set @paidAmount = @paidAmount - (@paidAmount * (@discountPercent / 100))  
 
        --Validity days change if coupon validity is greater then Subscription Validity in days 
        IF(@couponValidityDays > @FinalValidityInDays) 
        BEGIN 
            SET @FinalValidityInDays = @couponValidityDays 
        END 
   
        IF EXISTS  
        (  
            SELECT 1  
            FROM myBucketM  
            where MobileUserKey = @MobileUserKey  
                  AND ProductId = @ProductId  
        )  
        BEGIN  
  
            SELECT  
            @startDate = CASE  
                                WHEN EndDate > @CurrentDate THEN  
                                    StartDate  
                                ELSE  
                                    @CurrentDate  
                            END, 
 
            @EndDate = (CASE  
                                   WHEN EndDate > @CurrentDate THEN  
                                       DATEADD(DAY, @FinalValidityInDays, EndDate)  
                                   ELSE  
                                       DATEADD(DAY, @FinalValidityInDays , @CurrentDate)  
                               END  
                              )  
            from MYBucketM WHERE MobileUserKey = @MobileUserKey AND ProductId = @ProductId  
 
            UPDATE MYBucketM  
            SET ProductName = @ProductName,  
                StartDate = CASE  
                                WHEN EndDate > @CurrentDate THEN  
                                    StartDate  
                                ELSE  
                                    @CurrentDate  
                            END,  
                EndDate = @EndDate ,  
                ModifiedBy = @mobileUserKey,        
                ModifiedDate = @CurrentDate,  
                IsActive = 1,  
                IsExpired = 0  
            WHERE MobileUserKey = @MobileUserKey AND Productid = @ProductId  
        END  
        ELSE  
        BEGIN  
            INSERT myBucketM  
            (  
                MobileUserKey, ProductId, ProductName, StartDate, EndDate, Status, CreatedBy,CreatedDate,ModifiedBy,ModifiedDate, IsActive,IsExpired  
            )  
            VALUES  
            (   @MobileUserKey, @ProductId,@ProductName, @startDate, @endDate, 1, @MobileUserKey, @CurrentDate, NULL, NULL, 1, 0  
            );  
  
        END  
  
       INSERT INTO [dbo].[PurchaseOrdersM] ([LeadId], [ClientName], [Mobile], [Email], [DOB], [Remark], [PaymentDate], [ModeOfPayment], [BankName], [Pan],
        [State], [City], [TransasctionReference], [ProductId], [Product], [NetAmount], [PaidAmount], [CouponKey], [CouponDiscountAmount], [CouponDiscountPercent], [Status], [ActionBy], 
        [PaymentStatusId], [PaymentActionDate], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [StartDate], [EndDate], [IsActive], [KycApproved], [KycApprovedDate],SubscriptionMappingId, TransactionId)  
       SELECT (SELECT Id FROM Leads WHERE PublicKey = LeadKey), fullname, mobile, emailid, Dob, NULL, @CurrentDate, (CASE WHEN @paidAmount = 0.00 THEN @couponMOP ELSE @razorMOP END) AS ModeOfPayment, 
       NULL, NULL, NULL, City, @MerchantTransactionId, @ProductId, @ProductName, @price, @paidAmount, @couponkey, @discountAmount, @discountPercent, 1, 
       @MobileUserKey, 1, @CurrentDate, @CurrentDate, @MobileUserKey, NULL, NULL, @CurrentDate, @endDate, 1, 1, NULL , @SubscriptionMappingId , @TransactionId
       FROM #TempTable; 
 
 
  
        -- INSERT INTO [dbo].[LeadActivity]  
        -- (  
        --     [LeadKey],[ActivityType], [Message], [Description],  [ProductId], [CreatedOn]  
        -- )  
        -- VALUES  
        -- (  
        --     (  
        --         select top 1 leadkey from #TempTable  
        --     ),  
        --     NULL, NULL, 2, @productId, @CurrentDate  
        -- )  
  
        UPDATE CouponsM SET TotalRedeems = TotalRedeems + 1 WHERE publickey = @couponkey;  
        UPDATE MobileUsers SET CancommunityPost = 0 WHERE publickey = @MobileUserKey and  Mobile != '9411122233';  
  
  
        DROP TABLE #TempTable  
        SELECT  
            (  
                SELECT name,  
                       code,  
                       CAST(@startDate as date) AS startDate,  
                       CAST(@endDate as date) as enddate,  
                       @FinalValidityInDays AS ProductValidity  
                FROM ProductsM  
                WHERE id = @productId  
                FOR JSON auto  
            ) AS JsonData  
        COMMIT TRANSACTION;  
    END TRY  
    BEGIN CATCH  
        IF @@TRANCOUNT > 0  
            ROLLBACK TRANSACTION;  
  
        SELECT ERROR_MESSAGE() as JsonData  
    END CATCH  
  
END


 GO
-- exec ValidateCouponM 'DD09B51A-6A64-EF11-8174-00155D23D79C', 1, 'kratest'   , 1
          
ALTER PROCEDURE [dbo].[ValidateCouponM]           
    @mobileUserKey UNIQUEIDENTIFIER,          
    @productId INT,          
    @coupon VARCHAR(100), -- not coupon, we using name instead  
	@subscriptionDurationId int
AS          
BEGIN       
  
    
    DECLARE @RESULT VARCHAR(50),          
            @deductedPrice DECIMAL(10, 2),          
            @discountPrice DECIMAL(10, 2),          
            @discountPercentage int  ,          
            @productPrice DECIMAL(10, 2),          
            @couponId int = 0    ,
			@subscriptionDurationPrice decimal (18,2)

			

          
    -- Fetch product price first to return it if the coupon is invalid             
    SELECT @productPrice = Price          
    FROM productsm          
    WHERE id = @productId;          
          
    -- Check if coupon exists               
    IF NOT EXISTS          
    (          
        SELECT 1          
        FROM CouponsM c          
            inner JOIN CouponProductmappingM cp          
                on cp.CouponID = c.Id          
                   and ISNULL(cp.ProductID, @productId) = @productId          
            inner JOIN CouponUserMappingM cu          
                on cu.couponId = c.Id          
                   and ISNULL(cu.mobileuserkey, @mobileUserKey) = @mobileUserKey          
        WHERE Name = @coupon          
              AND c.IsActive = 1          
              AND c.IsDelete = 0          
              AND RedeemLimit > TotalRedeems          
    )          
    BEGIN          
        SET @RESULT = 'COUPONINVALID';          
        SET @deductedPrice = @productPrice;          
        SELECT @RESULT AS Result,          
               @deductedPrice AS DeductedPrice;          
        RETURN;          
    END; -- Return invalid coupon            
    ELSE          
    BEGIN          
        -- Start operation for coupons            
          
        DECLARE @couponkey UNIQUEIDENTIFIER;   
		
		select @subscriptionDurationPrice = CAST(( (p.Price - ((p.Price * sm.DiscountPercentage) / 100))  * ( c.DiscountInPercentage) / 100.0)  AS DECIMAL(18, 2)) 
 
			from couponsm c 
			inner JOIN SubscriptionMappingM sm  
            ON sm.SubscriptionDurationId = @subscriptionDurationId  
            inner JOIN ProductsM p  
            ON p.Id = @productId  


        SELECT @couponkey = c.publickey          
        FROM CouponsM c          
            inner JOIN CouponProductmappingM cp          
                ON cp.CouponID = c.Id          
            inner JOIN CouponUserMappingM cu          
                ON cu.CouponId = c.Id          
                   AND ISNULL(cu.mobileuserkey, @mobileUserKey) = @mobileUserKey          
        WHERE c.Name = @coupon          
              AND c.IsActive = 1          
              AND c.IsDelete = 0          
              AND c.RedeemLimit > c.TotalRedeems          
              AND isnull(cp.ProductID,@productid) = @productId;          
  
          
        SELECT @couponId = Id, @discountPrice = DiscountInPrice, @discountPercentage = DiscountInPercentage FROM CouponsM WHERE name = @coupon and IsActive =1 and IsDelete = 0 ;            
          
          
          
        -- Check if user has already used the coupon            
        IF EXISTS          
        (          
            select 1          
            from purchaseordersm          
            where ActionBy = @mobileUserKey          
                  and CouponKey   = @couponkey          
                  and productId = @productId          
        )          
        BEGIN      
  
            SET @RESULT = 'COUPONINVALID';          
            SET @deductedPrice = @productPrice;          
        END -- User has used the same coupon before now return invalid            
        ELSE          
        BEGIN          
            -- Coupon is valid return deducted price            
  
          
            SET @RESULT = 'COUPONVALID';            
          
          
   SET @deductedPrice = CASE                         
                             WHEN @discountPercentage is not NULL AND @discountPrice is   NULL THEN (@subscriptionDurationPrice * (@discountPercentage / 100.0))                      
                             WHEN @discountPercentage is NULL AND @discountPrice is not NULL THEN  @subscriptionDurationPrice                       
                             ELSE @subscriptionDurationPrice        
             END            
       
            --SET @deductedPrice = 0          
          
          
        END -- coupon is valid return Deducted price            
                
    END -- Whole coupon operation            
          
          
          
          
    -- Return result and deducted price                
    SELECT @RESULT AS Result,          
           CASE          
               WHEN @deductedPrice < 0 THEN          
                   0          
               ELSE          
                   @deductedPrice          
           END AS DeductedPrice;          
END


go
ALTER PROCEDURE [dbo].[GetFilteredMobileProducts]   
    @SearchText VARCHAR(100) = NULL,    
    @CategoryId INT = NULL  
AS      
BEGIN      
    SELECT   
        MAX(p.id) AS Id,      
        p.Name,      
        p.Code,      
        p.Description,      
        p.DescriptionTitle,      
        p.ListImage,      
        p.LandscapeImage,      
        p.Price,      
        p.DiscountAmount AS DiscountAmount,      
        p.DiscountPercent AS DiscountPercent,      
        p.CategoryId,      
        MAX(pcm.Name) AS Category,      
        p.CreatedDate,      
        p.IsActive,      
        p.imageUrl,      
        MAX(p.subscriptionId) AS SubscriptionId,      
        CAST(MAX(ISNULL(p.modifiedDate, p.createdDate)) AS DATETIME) AS LastModified,      
COUNT(DISTINCT CASE WHEN pc.IsDeleted = 0 THEN pc.id END) AS ContentCount,
        (      
            SELECT STUFF((      
                SELECT DISTINCT ', ' + pcm.AttachmentType      
                FROM ProductsContentM pcm      
                WHERE pcm.ProductId = MAX(p.id)      
                FOR XML PATH(''), TYPE      
            ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS DistinctAttachmentTypes      
        ) AS DistinctAttachmentTypes      
    FROM productsm p      
    LEFT JOIN ProductsContentM pc ON pc.productId = p.id      
    LEFT JOIN ProductCategoriesM pcm ON pcm.id = p.CategoryId      
    WHERE   
        p.code <> 'FREE'      
        AND p.IsDeleted = 0    
        AND (@SearchText IS NULL OR p.Name LIKE '%' + @SearchText + '%')      
        AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)  
    GROUP BY   
        p.name,      
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
    ORDER BY Id      
END;
GO




go
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
 LEFT JOIN Users AS Users on Users.PublicKey = Leads.AssignedTo                      
 LEFT JOIN Status as st on st.id = Leads.StatusId                      
 Left join PurchaseOrders as po on po.LeadId = Leads.Id                      
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
 AND Leads.LeadTypeKey =ISNULL(  @ThirdKey , Leads.LeadTypeKey ) --OR Coalesce(@ThirdKey,'') = '')                      
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
 Left join PurchaseOrders as po on po.LeadId = Leads.Id                      
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
                      
 AND ISNULL(Leads.ServiceKey , '00000000-0000-0000-0000-000000000000') = ISNULL( @SecondaryKey ,isnull(Leads.ServiceKey,'00000000-0000-0000-0000-000000000000') ) --OR Coalesce(@SecondaryKey,'') = '')                      
 AND ISNULL(Leads.LeadSourceKey, '00000000-0000-0000-0000-000000000000') = ISNULL( @FourthKey , Leads.LeadSourceKey)  --OR Coalesce(@FourthKey,'') = '')                      
 AND Leads.LeadTypeKey =ISNULL(  @ThirdKey , Leads.LeadTypeKey ) --OR Coalesce(@ThirdKey,'') = '')                      
 AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN cast( isnull(@FromDate , leads.ModifiedOn) as DATE ) AND                
 CAST(ISNULL(@ToDate , leads.ModifiedOn) as date))         
                  
              
 AND                       
 ( @FifthKey = 'assigned'   AND leads.AssignedTo IS NOT NULL OR                        
   @FifthKey = 'unassigned' AND leads.AssignedTo IS NULL OR                       
  ISNULL(@FifthKey,'') = ''                      
 )                       
AND (@CreatedBy IS NULL OR Leads.AssignedTo = @CreatedBy)                    
 AND ISNULL(Leads.ServiceKey , '00000000-0000-0000-0000-000000000000') = ISNULL( @SecondaryKey ,isnull(Leads.ServiceKey,'00000000-0000-0000-0000-000000000000') ) --OR Coalesce(@SecondaryKey,'') = '')                      
 AND ISNULL(Leads.LeadSourceKey, '00000000-0000-0000-0000-000000000000') = ISNULL( @FourthKey , Leads.LeadSourceKey)  --OR Coalesce(@FourthKey,'') = '')                      
 AND Leads.LeadTypeKey =ISNULL(  @ThirdKey , Leads.LeadTypeKey ) --OR Coalesce(@ThirdKey,'') = '')                      
 AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN cast( isnull(@FromDate , leads.ModifiedOn) as DATE )                    
 AND CAST(ISNULL(@ToDate , leads.ModifiedOn) as date))                      
 ORDER BY Leads.ModifiedOn DESC                      
                      
                         
                      
 OFFSET (IIF(@PageNumber <= 1 , 0 ,(@PageNumber-1))) * @PageSize ROWS                      
 FETCH NEXT @PageSize ROWS ONLY                      
 SELECT (SELECT * FROM #tempCustomer FOR JSON AUTO) as JsonData --, @TotalCount as Total                      
 --select * from #tempCustomer                      
END
GO


go

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
    @TotalSales INT OUTPUT        
AS        
BEGIN        
    IF @SearchText = '' SET @SearchText = null;        
    IF @RequestedBy = '' SET @RequestedBy = null;        
      
    SELECT        
        @TotalCount = COUNT(DISTINCT PO.Id),        
        @TotalSales = SUM(PO.PaidAmount)        
    FROM PurchaseOrdersM as PO        
    LEFT JOIN Users as CreatedUser ON  CreatedUser.PublicKey = PO.CreatedBy         
    LEFT JOIN PaymentModes as pm ON pm.Id = PO.ModeOfPayment       
    LEFT JOIN ProductsM as pd ON pd.Id = PO.ProductId      
    LEFT JOIN Status as st ON st.Id = PO.Status        
    left JOIN PaymentRequestStatusM AS PR ON  PR.ProductId  = po.productId       
     JOIN PhonePePaymentResponseM AS PP ON  PP.MerchanttransactionId  = PR.TransactionId        
    WHERE        
        (PO.Mobile LIKE '%' + @SearchText + '%' OR        
        @SearchText IS NULL OR        
        PO.Email LIKE '%' + @SearchText + '%' OR        
        PO.ClientName LIKE '%' + @SearchText + '%')        
        AND ((PO.ProductId IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@PrimaryKey, ',') ) AND @PrimaryKey IS NOT NULL)        
        OR (PO.ProductId = PO.ProductId AND @PrimaryKey IS NULL))        
        AND CAST(PO.PaymentDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE);      
      
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
    left JOIN PaymentRequestStatusM AS PR ON  PR.ProductId  = po.productId       
     JOIN PhonePePaymentResponseM AS PP ON PR.TransactionId = PP.MerchanttransactionId        
    WHERE        
        (PO.Mobile LIKE '%' + @SearchText + '%' OR        
        @SearchText IS NULL OR        
        PO.Email LIKE '%' + @SearchText + '%' OR        
        PO.ClientName LIKE '%' + @SearchText + '%')        
        AND ((PO.ProductId IN (SELECT CAST(value AS INT) FROM STRING_SPLIT(@PrimaryKey, ',') ) AND @PrimaryKey IS NOT NULL)        
        OR (PO.ProductId = PO.ProductId AND @PrimaryKey IS NULL))        
        AND CAST(PO.PaymentDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)        
    ORDER BY Status, PaymentDate DESC        
    OFFSET (CASE WHEN @PageNumber <= 1 THEN 0 ELSE (@PageNumber - 1) * @PageSize END) ROWS        
    FETCH NEXT @PageSize ROWS ONLY;        
END;
GO



--exec GetSubscriptionPlanWithProduct  40, 1 , 'd753127a-9bac-ef11-b337-952e85e58b7e' , 'android'         
ALTER PROCEDURE GetSubscriptionPlanWithProduct                
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
 AND LOWER(@DeviceType) IN  ( SELECT VALUE FROM string_split('android,ios',',') )             
END
GO

go 
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
    
    ORDER BY ISNULL(t.ModifiedOn,t.CreatedOn) DESC         
    OFFSET @Offset ROWS         
    FETCH NEXT @RowsPerPage ROWS ONLY;         
         
    -- Step 5: Drop the temporary table      
    DROP TABLE #tempRecords;         
END;
GO




-- exec DeleteMobileUserData '8895804280'
CREATE PROCEDURE DeleteMobileUserData
    @Mobile NVARCHAR(20)
AS
BEGIN
    DECLARE @CurrentDatabaseName NVARCHAR(100);
    
    SET @CurrentDatabaseName = DB_NAME();

    IF @CurrentDatabaseName NOT IN ('KingResearchTest', 'KingResearchUAT')
    BEGIN
        -- Raise an error if the database is not one of the allowed databases
        RAISERROR('Procedure can only be executed on KingResearchTest or KingResearchUAT databases', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @mobileUserKey UNIQUEIDENTIFIER, 
                @CurrentDate DATETIME = GETDATE();

        SELECT @mobileUserKey = PublicKey 
        FROM MobileUsers 
        WHERE Mobile = @Mobile;

        DELETE FROM MYBucketM 
        WHERE MobileUserKey = @mobileUserKey;

        DELETE FROM PurchaseOrdersM 
        WHERE actionBy = @mobileUserKey;

        UPDATE MobileUsers 
        SET CanCommunityPost = 1
        WHERE PublicKey = @mobileUserKey;


        COMMIT TRANSACTION;
        
        SELECT 'Execute successfully' AS Result;
    END TRY
    BEGIN CATCH
        -- If an error occurs, rollback the transaction
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
			SELECT 'Execute successfully'

        -- Return the error message
        SELECT 'Exception Occurred: ' + ERROR_MESSAGE() AS Result;
    END CATCH
END;
GO


-- exec GetCouponsM '0E5705F3-36A7-EF11-B330-AB74B4CC6D1C',1,2            
    
CREATE PROCEDURE GetCouponsM    
(    
    @userKey UNIQUEIDENTIFIER,    
    @productId INT,    
    @subscriptionDurationId INT    
)    
AS    
BEGIN    
               
    DECLARE @productPrice DECIMAL(18, 2) = (    
                                               SELECT TOP 1 Price FROM ProductsM WHERE Id = @productId    
                                           )     
    
    --select top 1    
    --    c.Name as CouponName,    
    --    DiscountInPercentage as DiscountPercentage,    
    --    c.Description as Description,    
    --    CAST(((p.Price - ((p.Price * sm.DiscountPercentage) / 100)) * (c.DiscountInPercentage) / 100.0) AS DECIMAL(18, 2)) AS DiscountAmount    
    --from couponsm c    
    --    left JOIN ProductsM p    
    --        ON p.Id = @productId    
    --    INNER JOIN SubscriptionMappingM sm    
    --        ON sm.SubscriptionDurationId = @subscriptionDurationId    
    --where def = 1    
    --      and c.IsActive = 1    
    --      and c.IsDelete = 0    
    --union all    
    --Result table to store coupon details            
--    SELECT MAX(c.Name) AS CouponName,    
--           max(c.DiscountInPercentage) AS DiscountPercentage,    
--           max(c.Description) as Description,    
--           CAST((max((p.Price - ((p.Price * sm.DiscountPercentage) / 100))) * (MAX(c.DiscountInPercentage) / 100.0)) AS DECIMAL(18, 2)) AS DiscountAmount    
--   from couponsm c    
--    left JOIN SubscriptionMappingM sm    
--        ON sm.SubscriptionDurationId = @subscriptionDurationId    
--           and ProductId = @productId    
--    left JOIN CouponProductMappingM cpm    
--        ON cpm.CouponId = c.id    
--    left join CouponUserMappingM cu    
--        on cu.CouponId = c.id    
--           and cu.MobileUserKey = @userKey    
--    left JOIN ProductsM p    
--        ON p.Id = cpm.ProductID    
--           and p.id = @productId    
--where 1 = 1    
--      and (    
--              cu.MobileUserKey = @userKey    
--              OR cu.MobileUserKey IS NULL    
--          )    
--      AND cu.Id IS NOT NULL    
--      AND c.IsActive = 1    
--      AND c.IsDelete = 0    
-- AND (cpm.ProductID = @productId   or cpm.ProductID is null) and cpm.Id is not null    
--          and c.def = 0  
;with discountPercentage as (  
 SELECT  c.id, c.Name  AS CouponName,  
           c.DiscountInPercentage AS DiscountPercentage,  
          c.Description  as Description,  
       sm.id as smId  
         --  (p.Price - ((p.Price * sm.DiscountPercentage) / 100) ) * ( c.DiscountInPercentage  / 100.0)    AS DiscountAmount  
       
   from couponsm c  
    left JOIN SubscriptionMappingM sm  
        ON sm.SubscriptionDurationId = @subscriptionDurationId  
           and ProductId = @productId  
    left JOIN CouponProductMappingM cpm  
        ON cpm.CouponId = c.id  
    left join CouponUserMappingM cu  
        on cu.CouponId = c.id  
           and cu.MobileUserKey = @userKey  
    left JOIN ProductsM p  
        ON p.Id = cpm.ProductID  
           and p.id = @productId    
where   1=1   
and (  
              cu.MobileUserKey = @userKey  
              OR cu.MobileUserKey IS NULL  
          )  
      AND cu.Id IS NOT NULL  
      AND c.IsActive = 1  
     AND c.IsDelete = 0  
 AND (cpm.ProductID = @productId   or cpm.ProductID is null) and cpm.Id is not null  
          and c.def = 0  
   ),   
     netPayment as (  
  select    
  sm.id as smId,  
       (pm.Price - ((pm.Price * sm.DiscountPercentage)/ 100))  as NetPayment   
      
    from  
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
     AND (sm.ProductId = @ProductId OR @ProductId IS NULL)   
       
  
   ) select CouponName ,DiscountPercentage,Description,   
   (NetPayment * ( DiscountPercentage  / 100.0))  AS DiscountAmount  
   from netPayment np inner join discountPercentage dp on dp.smId = np.smId  
  
  
   union all   
   select top 1    
        c.Name as CouponName,    
        DiscountInPercentage as DiscountPercentage,    
        c.Description as Description,    
        CAST(((p.Price - ((p.Price * sm.DiscountPercentage) / 100)) * (c.DiscountInPercentage) / 100.0) AS DECIMAL(18, 2))  AS DiscountAmount    
    from couponsm c    
        left JOIN ProductsM p    
            ON p.Id = @productId    
        INNER JOIN SubscriptionMappingM sm    
            ON sm.SubscriptionDurationId = @subscriptionDurationId    
    where def = 1    
          and c.IsActive = 1    
          and c.IsDelete = 0    
    
    
    
    
    
END
----------------------------------------------------------------------------------------------------------------------------------------
GO

CREATE PROCEDURE GetFreeTrial 
 @IsPaging BIT = 1,                                 
 @PageSize INT = 10,                                 
 @PageNumber INT = 1,                                 
 @SortExpression VARCHAR(50) = 'ModifiedOn', -- Default sort column                                 
 @SortOrder VARCHAR(20) = 'DESC', -- Default sort order                                 
 @RequestedBy VARCHAR(100) = NULL,                                 
 @FromDate DATETIME = NULL,                                 
 @ToDate DATETIME = NULL,                                 
 @SearchText VARCHAR(100) = NULL,                                 
 @PrimaryKey VARCHAR(100) = NULL,                                 
 @Status VARCHAR(50) = NULL, -- New parameter for status filter
 @TotalCount INT OUTPUT
AS         
BEGIN                                 
    SET NOCOUNT ON; -- Prevents extra result sets from interfering with SELECT queries

    -- Normalize NULL or empty input values
    IF @SearchText = '' SET @SearchText = NULL;                                 
    IF @RequestedBy = '' SET @RequestedBy = NULL;     

    -- Step 1: Create a temporary table to hold base data
    CREATE TABLE #BaseData
    (
        FullName NVARCHAR(255),
        Mobile NVARCHAR(50),
        StartDate DATETIME,
        EndDate DATETIME,
        ProductNames NVARCHAR(MAX),
        Validity INT,
        Status NVARCHAR(50)
    );

    -- Step 2: Insert the base dataset into the temporary table
    INSERT INTO #BaseData (FullName, Mobile, StartDate, EndDate, ProductNames, Validity, Status)
    SELECT  
        MU.FullName,
        MU.Mobile,
        FM.StartDate,
        FM.EndDate,
        STRING_AGG(P.Name, ', ') AS ProductNames, 
        MIN(FTB.DaysInNumber) AS Validity,
        CAST(CASE WHEN FM.IsActive = 1 THEN 'Active' ELSE 'InActive' END AS NVARCHAR(50)) AS Status
    FROM MobileUsers MU
    LEFT JOIN FreeTrialM FM ON MU.PublicKey = FM.MobileUserKey
    LEFT JOIN FreeTrialBasketDetailM FTBD ON FTBD.FreeTrialBasketId = FM.FreeTrialBasketId
    LEFT JOIN FreeTrialBasketM FTB ON FM.FreeTrialBasketId = FTB.Id
    LEFT JOIN ProductsM P ON FTBD.ProductId = P.Id
    WHERE                                 
        (                                 
            (@SearchText IS NULL OR MU.Mobile LIKE '%' + @SearchText + '%') OR                                 
            (@SearchText IS NULL OR MU.EmailId LIKE '%' + @SearchText + '%') OR                                 
            (@SearchText IS NULL OR MU.FullName LIKE '%' + @SearchText + '%')                                 
        )        
        AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(MU.RegistrationDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))   
        AND (@PrimaryKey IS NULL OR LOWER(MU.DeviceType) LIKE '%' + LOWER(@PrimaryKey) + '%') 
    GROUP BY
        MU.FullName,
        MU.Mobile,
        FM.StartDate,
        FM.EndDate,
        FM.IsActive;

    -- Step 3: Calculate the total count after applying @Status filter
    SELECT @TotalCount = COUNT(*)
    FROM #BaseData
    WHERE (@Status IS NULL OR Status = @Status);

    -- Step 4: Fetch paginated and sorted results from the temporary table
    SELECT *
    FROM #BaseData
    WHERE (@Status IS NULL OR Status = @Status)
    ORDER BY   
        CASE WHEN @SortOrder = 'ASC' THEN 
            CASE @SortExpression 
                WHEN 'StartDate' THEN StartDate
                WHEN 'EndDate' THEN EndDate
                ELSE StartDate -- Default sorting
            END 
        END ASC,
        CASE WHEN @SortOrder = 'DESC' THEN 
            CASE @SortExpression 
                WHEN 'StartDate' THEN StartDate
                WHEN 'EndDate' THEN EndDate
                ELSE StartDate -- Default sorting
            END 
        END DESC
    OFFSET (IIF(@PageNumber <= 1, 0, (@PageNumber - 1))) * @PageSize ROWS     
    FETCH NEXT @PageSize ROWS ONLY; 

    -- Step 5: Drop the temporary table
    DROP TABLE #BaseData;
END
GO


go
--CreatedBy: Vijay Sahu                   
--Purpose: For OPT Verification and send the response with all the products he has purchased                  
--EXEC OtpLoginVerification '12129121291212', null,'android','91'                
CREATE PROCEDURE [dbo].[OtpLoginVerification]                  
@Mobile varchar(20),                  
@FirebaseFcmToken VARCHAR(500)  ,                
@DeviceType varchar(10),                
@CountryCode varchar(5)        
AS BEGIN                  
                 
 DECLARE @MobileUserKey uniqueidentifier, @MobileTemp varchar(20) , @OtpTemp varchar(6), @EmailTemp varchar(50),                  
 @Id int , @FullName varchar(80),@LeadKey varchar(50),                  
 @ProductCodes varchar(500),@Result varchar(50) , @IsOtpVerified  bit,@ProfileImage varchar(200),@CurrentDate datetime;                  
                  
                
   set @CurrentDate = cast(GETDATE() as DateTime)                
                
                
 SELECT                   
 @MobileUserKey = PublicKey, @MobileTemp = Mobile , @OtpTemp = OneTimePassword ,                  
 @EmailTemp = EmailId , @Id = Id , @FullName = FullName ,  @LeadKey = LeadKey                 
 FROM MobileUsers WHERE Mobile = @Mobile                  
                 
 set @LeadKey = (select publickey from leads where mobileNumber = @Mobile)                
                   
  IF(@MobileUserKey IS NOT NULL )                  
 BEGIN                  
                 
 UPDATE  mobileusers                 
  SET FirebaseFcmToken = @FirebaseFcmToken,IsOtpVerified =1,                  
  ModifiedOn = CAST(GETDATE() AS DATE)                  
 WHERE mobile = @Mobile                   
                 
 SET @Result = 'VERIFIEDUSER'                
 SET @ProfileImage =   (select profileImage from mobileusers where PublicKey = @MobileUserKey)                
                  
  SELECT @ProductCodes = STUFF((                  
 SELECT ', ' + CONVERT(VARCHAR(50), P.Code)                  
 FROM MYBucketM B                  
 INNER JOIN ProductsM P ON B.ProductId = P.Id                  
  WHERE MobileUserKey = @MobileUserKey                   
  AND GETDATE() BETWEEN CAST(StartDate AS DATE) and CAST(EndDate as date)                  
  FOR XML PATH('')), 1, 2, '');                  
  END                  
 ELSE if(@MobileUserKey is null AND @LeadKey is null )                
 BEGIN                 
   INSERT INTO Leads (Fullname,mobileNumber,CreatedBy,CreatedOn,ModifiedOn,ModifiedBy)                         
   values('',cast(@Mobile as varchar),'3CA214D0-8CB8-EB11-AAF2-00155D53687A',@CurrentDate,@CurrentDate,'3CA214D0-8CB8-EB11-AAF2-00155D53687A')                 
                       
  INSERT INTO MobileUsers (          
  FullName          
  ,LeadKey          
  ,Password          
  ,IsOtpVerified          
  ,Mobile          
  ,DeviceType          
  ,CreatedOn          
  ,ModifiedOn          
  ,MobileToken          
  ,IMEI          
  ,StockNature          
  ,AgreeToTerms          
  ,SameForWhatsApp          
  ,IsActive          
  ,IsDelete          
  ,ProfileImage          
  ,About          
  ,RegistrationDate          
  ,LastLoginDate          
  ,CountryCode          
  ,FirebaseFcmToken        
  )                         
   values(          
   @FullName          
   ,( select publickey from leads where mobileNumber = @Mobile)           
   ,''           
   ,1          
   ,cast(@Mobile  as varchar)          
   ,@DeviceType          
   ,@CurrentDate          
   ,@CurrentDate          
   ,''          
   ,''          
   ,''          
   ,1          
   ,1          
   ,1          
   ,0          
   ,'1212'          
   ,'213'          
   ,@CurrentDate          
   ,@CurrentDate          
   ,@CountryCode          
   ,@FirebaseFcmToken        
   )                      
                 
   set @MobileUserKey = (select publickey from mobileusers where mobile = @Mobile)              
   set @ProfileImage = (select ProfileImage from mobileusers where mobile = @Mobile)              
   set @FullName = (select FullName from mobileusers where mobile = @Mobile)              
      set @LeadKey = (select LeadKey from mobileusers where mobile = @Mobile)          
     
   
  
INSERT INTO [dbo].[MYBucketM]  
           ([MobileUserKey]  
           ,[ProductId]  
           ,[ProductName]  
           ,[StartDate]  
           ,[EndDate]  
           ,[Status]  
           ,[CreatedBy]  
           ,[CreatedDate]  
           ,[ModifiedBy]  
           ,[ModifiedDate]  
           ,[IsActive]  
           ,[IsExpired])  
     VALUES  
           (@MobileUserKey  
           ,12  
           ,'FREE'  
           ,'2023-12-08 15:34:53.783'  
           ,'2023-12-28 15:34:53.783'  
           ,1  
           ,'BB74D26F-AA28-EB11-BEE5-00155D53687A'  
           ,getDate()  
           ,null  
           ,null  
           ,1  
           ,0)  
  
  
  
              
 set @Result = 'REGISTRATIONSUCCESSFULL'                
    set @FullName = (select FullName from mobileusers where mobile = @Mobile)              
              
                
 END                
                
 ELSE IF(@MobileUserKey is not null AND @LeadKey is null )                
 BEGIN                 
   INSERT INTO Leads (Fullname,mobileNumber,CreatedBy,CreatedOn,ModifiedOn,ModifiedBy)                         
   values('sandeep',cast(@Mobile as varchar),'3CA214D0-8CB8-EB11-AAF2-00155D53687A',@CurrentDate,@CurrentDate,'3CA214D0-8CB8-EB11-AAF2-00155D53687A')                 
                       
    update MobileUsers                 
    SET FullName = @FullName,                
    LeadKey = ( select publickey from leads where mobileNumber = @Mobile),                
    Password = '123456',                
    IsOtpVerified = 1,                
    Mobile =  cast(@Mobile  as varchar),                
    DeviceType = 'android',                
    CreatedOn = @CurrentDate,                
    ModifiedOn = @CurrentDate,                
    MobileToken = '12121231',                    
    IMEI = '',                
    StockNature = '',                
    AgreeToTerms =1,                
    SameForWhatsApp =1,                
    IsActive =1,                
    IsDelete = 0,                
    ProfileImage = '1212',                
    About = '213',                
    RegistrationDate = @CurrentDate,                
    LastLoginDate = @CurrentDate,                
    CountryCode = @CountryCode                        
   where publickey = @MobileUserKey    
     
  
     
INSERT INTO [dbo].[MYBucketM]  
           ([MobileUserKey]  
           ,[ProductId]  
           ,[ProductName]  
           ,[StartDate]  
           ,[EndDate]  
           ,[Status]  
           ,[CreatedBy]  
           ,[CreatedDate]  
           ,[ModifiedBy]  
           ,[ModifiedDate]  
           ,[IsActive]  
           ,[IsExpired])  
     VALUES  
           (@MobileUserKey  
           ,12  
           ,'FREE'  
           ,'2023-12-08 15:34:53.783'  
           ,'2023-12-28 15:34:53.783'  
           ,1  
           ,'BB74D26F-AA28-EB11-BEE5-00155D53687A'  
           ,getDate()  
           ,null  
           ,null  
           ,1  
           ,0)  
                       
    set @Result = 'REGISTRATIONSUCCESSFULL'                
                
                 
 END                
 ELSE IF (@MobileUserKey is  null AND @LeadKey is NOT null )                
 BEGIN                
                
  INSERT INTO MobileUsers (FullName          
  ,LeadKey          
  ,Password          
  ,IsOtpVerified          
  ,Mobile          
  ,DeviceType          
  ,CreatedOn          
  ,ModifiedOn          
  ,MobileToken          
  ,IMEI          
  ,StockNature          
  ,AgreeToTerms          
  ,SameForWhatsApp          
  ,IsActive          
  ,IsDelete          
  ,ProfileImage          
  ,About          
  ,RegistrationDate          
  ,LastLoginDate          
  ,CountryCode        
  )                         
   values(@FullName          
   ,( select publickey from leads where mobileNumber = @Mobile)          
   ,'123456'          
   ,1       
   ,cast(@Mobile  as varchar)          
   ,'android'          
   ,@CurrentDate          
   ,@CurrentDate          
   ,'12121231'          
   ,''          
   ,''          
   ,1          
   ,1          
   ,1          
   ,0          
   ,'1212'          
   ,'213'          
   ,@CurrentDate          
   ,@CurrentDate          
   ,@CountryCode        
   )          
            
              
                      
                 
   UPDATE Leads set Fullname = 'sandeep',                
   mobileNumber = cast(@Mobile as varchar),                
   CreatedBy = '3CA214D0-8CB8-EB11-AAF2-00155D53687A',                
   CreatedOn = @CurrentDate,                
   ModifiedOn = @CurrentDate,                
   ModifiedBy = '3CA214D0-8CB8-EB11-AAF2-00155D53687A'           
   WHERE mobilenumber = @Mobile                
                    
set @MobileUserKey = (select publickey from mobileusers where mobile = @Mobile)              
   set @ProfileImage = (select ProfileImage from mobileusers where mobile = @Mobile)              
   set @FullName = (select FullName from mobileusers where mobile = @Mobile)              
      set @LeadKey = (select LeadKey from mobileusers where mobile = @Mobile)     
     
  
     
INSERT INTO [dbo].[MYBucketM]  
           ([MobileUserKey]  
           ,[ProductId]  
           ,[ProductName]  
           ,[StartDate]  
           ,[EndDate]  
           ,[Status]  
           ,[CreatedBy]  
           ,[CreatedDate]  
           ,[ModifiedBy]  
           ,[ModifiedDate]  
           ,[IsActive]  
           ,[IsExpired])  
     VALUES  
           (@MobileUserKey  
           ,12  
           ,'FREE'  
           ,'2023-12-08 15:34:53.783'  
           ,'2023-12-28 15:34:53.783'  
           ,1  
           ,'BB74D26F-AA28-EB11-BEE5-00155D53687A'  
           ,getDate()  
           ,null  
           ,null  
           ,1  
           ,1)  
              
 set @Result = 'REGISTRATIONSUCCESSFULL'                
    set @FullName = (select FullName from mobileusers where mobile = @Mobile)                  
 END                
                
                
                
                
                  
  SELECT                   
  @Result as Result,                   
  '' as Message,                  
  @MobileUserKey as MobileUserKey ,                   
  @ProfileImage as ProfileImage,                
  @FullName as FullName ,                   
  @EmailTemp as EmailTemp ,                   
  @Id as Id ,                   
  @LeadKey as LeadKey,                  
  @ProductCodes as EventSubscription                  
END
GO

--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
--sp_helptext ManageCouponM  
  
  
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
        IF @mobileNumbers IS NULL          
        BEGIN          
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
           
        -- check if coupon name is changed and it has 0 Total Redeems            
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
        ELSE          
        BEGIN          
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
          
   select @newCouponId = id from couponsm where PublicKey = @couponKey          
                   
          
            -- insert into coupon product mapping                  
            IF @productIds IS NULL          
            BEGIN          
                IF NOT EXISTS          
                (          
                    SELECT 1          
                    FROM CouponProductMappingM          
                    WHERE CouponId = @newCouponId          
                          AND ProductId IS NULL          
                )          
                BEGIN          
                    INSERT INTO CouponProductMappingM          
                    (          
                        CouponId,          
                        ProductId          
                    )          
                    VALUES          
                    (@newCouponId, NULL)          
                END          
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
                FROM string_split(@productIds, ',') AS product          
                WHERE NOT EXISTS          
                (          
                    SELECT 1          
                    FROM CouponProductMappingM          
                    WHERE CouponId = @newCouponId          
                          AND ProductId = product.value          
                )          
            END          
          
          
            -- insert into coupon user mapping            
            IF @mobileNumbers IS NULL          
            BEGIN          
                IF NOT EXISTS          
                (          
                    SELECT 1          
                    FROM CouponUserMappingM          
                    WHERE CouponId = @newCouponId          
                          AND MobileUserKey IS NULL          
                )          
                BEGIN          
                    INSERT INTO CouponUserMappingM          
                    (          
                        CouponId,          
                        MobileUserKey          
                    )          
                    VALUES          
                    (@newCouponId, NULL);          
                END          
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
                ON MU.mobile = LTRIM(RTRIM(ss.value))          
                WHERE NOT EXISTS          
                (          
                    SELECT 1          
                    FROM CouponUserMappingM          
                    WHERE CouponId = @newCouponId          
                          AND MobileUserKey = MU.PublicKey          
                )          
            END          
          
   set @result = 'UPDATED'          
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


  ALTER TABLE CouponsM
ADD IsVisible BIT NOT NULL DEFAULT 1


-----------------------------------------------------------------------------------------------------------------------------------------
GO

--GetPurchaseOrdersM

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
    LEFT JOIN PaymentRequestStatusM AS PR ON PR.ProductId = PO.ProductId         
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
CREATE PROCEDURE [dbo].[GetPhonePe]   
    @IsPaging INT = 0,  
    @PageSize INT = 5,   
    @PageNumber INT = 1,  
    @SortExpression VARCHAR(50),   
    @SortOrder VARCHAR(50),   
    @RequestedBy VARCHAR(50) = NULL,   
    @FromDate VARCHAR(50) = NULL,   
    @ToDate VARCHAR(50) = NULL,   
    @SearchText VARCHAR(250) = NULL,   
    @PrimaryKey VARCHAR(50) = NULL,   
    @TotalCount INT = 0 OUTPUT 
AS   
BEGIN   
    -- Initialize TotalCount variable 
    SET @TotalCount = 0; 
 
    -- Step 1: Calculate the total count of records 
    SELECT @TotalCount = COUNT(*)  
    FROM PaymentRequestStatusM PRS 
    LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId  
    LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey 
    WHERE  
        (@SearchText IS NULL OR MU.FullName LIKE '%' + @SearchText + '%')  
        AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)); 
 
    -- Step 2: Query to fetch the data with dynamic sorting and pagination 
    SELECT    
        MU.FullName, 
        MU.Mobile, 
        PRS.Amount AS RequestAmount, 
        PRS.CreatedOn, 
        REPLACE(PRS.Status, 'PAYMENT_SUCCESS', 'SUCCESS') AS Status, 
        ISNULL(PPR.Amount, 0) AS PaidAmount, 
        PPR.PaymentInstrumentType, 
        PPR.MerchantTransactionId, 
        ROW_NUMBER() OVER ( 
            ORDER BY  
                CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN PRS.CreatedOn END ASC, 
                CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN PRS.CreatedOn END DESC, 
                CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'ASC' THEN MU.FullName END ASC, 
                CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'DESC' THEN MU.FullName END DESC 
        ) AS RowNum 
    INTO #TempPhonePe 
    FROM PaymentRequestStatusM PRS 
    LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId  
    LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey 
    WHERE  
        (@SearchText IS NULL OR MU.FullName LIKE '%' + @SearchText + '%')  
        AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)); 
 
    -- Step 3: Return paginated and sorted results 
    SELECT    
        FullName, 
        Mobile, 
        RequestAmount, 
        CreatedOn, 
        Status, 
        PaidAmount, 
        PaymentInstrumentType, 
        MerchantTransactionId 
    FROM #TempPhonePe 
    WHERE RowNum BETWEEN (@PageNumber - 1) * @PageSize + 1 AND @PageNumber * @PageSize 
    ORDER BY RowNum; 
 
    -- Clean up the temporary table 
    DROP TABLE IF EXISTS #TempPhonePe; 
END; 

GO

GO
ALTER PROCEDURE GetMobileUsers   
    @IsPaging BIT = 1,                                  
    @PageSize INT = 10,                                  
    @PageNumber INT = 1,                                  
    @SortExpression VARCHAR(50) = '',                                  
    @SortOrder VARCHAR(20) = '',                                  
    @RequestedBy VARCHAR(100) = NULL,                                  
    @FromDate DATETIME = NULL,                                  
    @ToDate DATETIME = NULL,                                  
    @SearchText VARCHAR(100) = NULL,                                  
    @PrimaryKey VARCHAR(100) = NULL,                                  
    @TotalCount INT OUTPUT                                  
AS          
BEGIN 
    SET NOCOUNT ON; 
 
    -- Set default NULL values for empty input 
    IF @SearchText = '' SET @SearchText = NULL;                                  
    IF @RequestedBy = '' SET @RequestedBy = NULL;      
 
    -- Count total matching records for paging 
    SELECT @TotalCount = COUNT(1)                               
    FROM MobileUsers MU 
    WHERE                                  
        ( 
            (MU.Mobile LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)  
            OR (MU.EmailId LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)  
            OR (MU.FullName LIKE '%' + @SearchText + '%' OR @SearchText IS NULL) 
        ) 
        AND (CAST(MU.RegistrationDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)) 
        AND (@PrimaryKey IS NULL OR LOWER(MU.DeviceType) LIKE '%' + LOWER(@PrimaryKey) + '%');  
 
    -- Main query to retrieve user data 
    SELECT   
        MU.Id,  
        MU.FullName,  
        MU.EmailId,  
        MU.Mobile,  
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
        COUNT(MB.MobileUserKey) AS BucketCount, 
        ( 
            SELECT  
                MB.ProductName,  
                MB.CreatedDate,  
                MB.EndDate, 
                MB.ProductId, 
                MB.StartDate, 
                DATEDIFF(DAY, GETDATE(), MB.EndDate) AS ValidityInDays 
            FROM MYBucketM MB 
            WHERE MB.MobileUserKey = MU.PublicKey 
            FOR JSON PATH 
        ) AS BucketData 
    FROM MobileUsers MU 
    LEFT JOIN MYBucketM MB  
        ON MU.PublicKey = MB.MobileUserKey 
    WHERE 
        ( 
            (MU.Mobile LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)  
            OR (MU.EmailId LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)  
            OR (MU.FullName LIKE '%' + @SearchText + '%' OR @SearchText IS NULL) 
        ) 
        AND (CAST(MU.RegistrationDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)) 
        AND (@PrimaryKey IS NULL OR LOWER(MU.DeviceType) LIKE '%' + LOWER(@PrimaryKey) + '%') 
    GROUP BY  
        MU.Id,  
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
        MU.ModifiedOn 
ORDER BY    
        CASE  
            WHEN @SortExpression = 'FullName' THEN MU.FullName 
            WHEN @SortExpression = 'EmailId' THEN MU.EmailId 
            ELSE MU.ModifiedOn 
        END  
        OFFSET IIF(@PageNumber <= 1, 0, (@PageNumber - 1)) * @PageSize ROWS      
        FETCH NEXT @PageSize ROWS ONLY;          
 
    SET NOCOUNT OFF; 
END; 
GO

GO
ALTER PROCEDURE [dbo].[GetPhonePe]   
    @IsPaging INT = 0,  
    @PageSize INT = 5,   
    @PageNumber INT = 1,  
    @SortExpression VARCHAR(50),   
    @SortOrder VARCHAR(50),   
    @RequestedBy VARCHAR(50) = NULL,   
    @FromDate VARCHAR(50) = NULL,   
    @ToDate VARCHAR(50) = NULL,   
    @SearchText VARCHAR(250) = NULL,   
    @PrimaryKey VARCHAR(50) = NULL, -- Used to filter by Status
    @TotalCount INT = 0 OUTPUT 
AS   
BEGIN   
    -- Initialize TotalCount variable 
    SET @TotalCount = 0; 
 
    -- Step 1: Calculate the total count of records 
    SELECT @TotalCount = COUNT(*)  
    FROM PaymentRequestStatusM PRS 
    LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId  
    LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey 
    WHERE  
        (@SearchText IS NULL OR MU.FullName LIKE '%' + @SearchText + '%')  
        AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))
        AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey); 
 
    -- Step 2: Query to fetch the data with dynamic sorting and pagination 
    SELECT    
        MU.FullName, 
        MU.Mobile, 
        PRS.Amount AS RequestAmount, 
        PRS.CreatedOn, 
        REPLACE(PRS.Status, 'PAYMENT_SUCCESS', 'SUCCESS') AS Status, 
        ISNULL(PPR.Amount, 0) AS PaidAmount, 
        PPR.PaymentInstrumentType, 
        PPR.MerchantTransactionId, 
        ROW_NUMBER() OVER ( 
            ORDER BY  
                CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN PRS.CreatedOn END ASC, 
                CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN PRS.CreatedOn END DESC, 
                CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'ASC' THEN MU.FullName END ASC, 
                CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'DESC' THEN MU.FullName END DESC 
        ) AS RowNum 
    INTO #TempPhonePe 
    FROM PaymentRequestStatusM PRS 
    LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId  
    LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey 
    WHERE  
        (@SearchText IS NULL OR MU.FullName LIKE '%' + @SearchText + '%')  
        AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))
        AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey); 
 
    -- Step 3: Return paginated and sorted results 
    SELECT    
        FullName, 
        Mobile, 
        RequestAmount, 
        CreatedOn, 
        Status, 
        PaidAmount, 
        PaymentInstrumentType, 
        MerchantTransactionId 
    FROM #TempPhonePe 
    WHERE RowNum BETWEEN (@PageNumber - 1) * @PageSize + 1 AND @PageNumber * @PageSize 
    ORDER BY RowNum; 
 
    -- Clean up the temporary table 
    DROP TABLE IF EXISTS #TempPhonePe; 
END; 
GO

GO
ALTER PROCEDURE GetMobileUsers      
    @IsPaging BIT = 1,                                     
    @PageSize INT = 10,                                     
    @PageNumber INT = 1,                                     
    @SortExpression VARCHAR(50) = '',                                     
    @SortOrder VARCHAR(20) = '',                                     
    @RequestedBy VARCHAR(100) = NULL,                                     
    @FromDate DATETIME = NULL,                                     
    @ToDate DATETIME = NULL,                                     
    @SearchText VARCHAR(100) = NULL,                                     
    @PrimaryKey VARCHAR(100) = NULL,                                     
    @TotalCount INT OUTPUT                                     
AS             
BEGIN    
    SET NOCOUNT ON;    
    
    -- Set default NULL values for empty input    
    IF @SearchText = '' SET @SearchText = NULL;                                     
    IF @RequestedBy = '' SET @RequestedBy = NULL;         
    
    -- Count total matching records for paging    
    SELECT @TotalCount = COUNT(1)                                  
    FROM MobileUsers MU    
    WHERE                                     
        (    
            (MU.Mobile LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)     
            OR (MU.EmailId LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)     
            OR (MU.FullName LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)    
        )    
        AND (CAST(MU.RegistrationDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))    
        AND (@PrimaryKey IS NULL OR LOWER(MU.DeviceType) LIKE '%' + LOWER(@PrimaryKey) + '%');     
    
    -- Main query to retrieve user data    
    SELECT      
        MU.Id,     
        MU.FullName,     
        MU.EmailId,     
        MU.Mobile,     
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
        COUNT(MB.MobileUserKey) AS BucketCount,    
        (    
            SELECT     
                MB.Id,   
                MB.ProductName,     
                MB.CreatedDate,     
                MB.EndDate,    
                MB.ProductId,    
                MB.StartDate,    
               DATEDIFF(DAY, MB.StartDate, MB.EndDate) AS ValidityInDays    
            FROM MYBucketM MB    
            WHERE MB.MobileUserKey = MU.PublicKey    
            FOR JSON PATH    
        ) AS BucketData    
    FROM MobileUsers MU    
    LEFT JOIN MYBucketM MB     
        ON MU.PublicKey = MB.MobileUserKey    
    WHERE    
        (    
            (MU.Mobile LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)     
            OR (MU.EmailId LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)     
            OR (MU.FullName LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)    
        )    
        AND (CAST(MU.RegistrationDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))    
        AND (@PrimaryKey IS NULL OR LOWER(MU.DeviceType) LIKE '%' + LOWER(@PrimaryKey) + '%')    
    GROUP BY     
        MU.Id,     
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
        MU.ModifiedOn    
ORDER BY ISNULL(mu.ModifiedOn,mu.RegistrationDate) desc 
        OFFSET IIF(@PageNumber <= 1, 0, (@PageNumber - 1)) * @PageSize ROWS         
        FETCH NEXT @PageSize ROWS ONLY;             
    
    SET NOCOUNT OFF;    
END; 
GO

GO
ALTER PROCEDURE [dbo].[GetPhonePe]    
    @IsPaging INT = 0,   
    @PageSize INT = 5,    
    @PageNumber INT = 1,   
    @SortExpression VARCHAR(50),    
    @SortOrder VARCHAR(50),    
    @RequestedBy VARCHAR(50) = NULL,    
    @FromDate VARCHAR(50) = NULL,    
    @ToDate VARCHAR(50) = NULL,    
    @SearchText VARCHAR(250) = NULL,    
    @PrimaryKey VARCHAR(50) = NULL, -- Used to filter by Status 
    @TotalCount INT = 0 OUTPUT  
AS    
BEGIN    
    -- Initialize TotalCount variable  
    SET @TotalCount = 0;  
  
    -- Step 1: Calculate the total count of records  
    SELECT @TotalCount = COUNT(*)   
    FROM PaymentRequestStatusM PRS  
    LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId   
    LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey  
    WHERE   
        (@SearchText IS NULL 
         OR MU.FullName LIKE '%' + @SearchText + '%' 
         OR CAST(MU.Mobile AS VARCHAR) = @SearchText)  
        AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)) 
        AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey);  
  
    -- Step 2: Query to fetch the data with dynamic sorting and pagination  
    IF @IsPaging = 1 
    BEGIN
        SELECT     
            MU.FullName,  
            MU.Mobile,  
            PRS.Amount AS RequestAmount,  
            PRS.CreatedOn,  
            REPLACE(PRS.Status, 'PAYMENT_SUCCESS', 'SUCCESS') AS Status,  
            ISNULL(PPR.Amount, 0) AS PaidAmount,  
            PPR.PaymentInstrumentType,  
            PPR.MerchantTransactionId,  
            ROW_NUMBER() OVER (  
                ORDER BY   
                    CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN PRS.CreatedOn END ASC,  
                    CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN PRS.CreatedOn END DESC,  
                    CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'ASC' THEN MU.FullName END ASC,  
                    CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'DESC' THEN MU.FullName END DESC  
            ) AS RowNum  
        INTO #TempPhonePe  
        FROM PaymentRequestStatusM PRS  
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId   
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey  
        WHERE   
            (@SearchText IS NULL 
             OR MU.FullName LIKE '%' + @SearchText + '%' 
             OR CAST(MU.Mobile AS VARCHAR) = @SearchText)  
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)) 
            AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey);  
  
        -- Step 3: Return paginated and sorted results  
        SELECT     
            FullName,  
            Mobile,  
            RequestAmount,  
            CreatedOn,  
            Status,  
            PaidAmount,  
            PaymentInstrumentType,  
            MerchantTransactionId  
        FROM #TempPhonePe  
        WHERE RowNum BETWEEN (@PageNumber - 1) * @PageSize + 1 AND @PageNumber * @PageSize  
        ORDER BY RowNum;  
  
        -- Clean up the temporary table  
        DROP TABLE IF EXISTS #TempPhonePe;  
    END
    ELSE
    BEGIN
        -- Return all data without pagination
        SELECT     
            MU.FullName,  
            MU.Mobile,  
            PRS.Amount AS RequestAmount,  
            PRS.CreatedOn,  
            REPLACE(PRS.Status, 'PAYMENT_SUCCESS', 'SUCCESS') AS Status,  
            ISNULL(PPR.Amount, 0) AS PaidAmount,  
            PPR.PaymentInstrumentType,  
            PPR.MerchantTransactionId
        FROM PaymentRequestStatusM PRS  
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId   
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey  
        WHERE   
            (@SearchText IS NULL 
             OR MU.FullName LIKE '%' + @SearchText + '%' 
             OR CAST(MU.Mobile AS VARCHAR) = @SearchText)  
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)) 
            AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey);  
    END
END;  
GO

GO
ALTER PROCEDURE [dbo].[GetPhonePe]    
    @IsPaging INT = 0,   
    @PageSize INT = 5,    
    @PageNumber INT = 1,   
    @SortExpression VARCHAR(50),    
    @SortOrder VARCHAR(50),    
    @RequestedBy VARCHAR(50) = NULL,    
    @FromDate VARCHAR(50) = NULL,    
    @ToDate VARCHAR(50) = NULL,    
    @SearchText VARCHAR(250) = NULL,    
    @PrimaryKey VARCHAR(50) = NULL, -- Used to filter by Status 
    @TotalCount INT = 0 OUTPUT  
AS    
BEGIN    
    -- Initialize TotalCount variable  
    SET @TotalCount = 0;  
  
    -- Step 1: Calculate the total count of records  
    SELECT @TotalCount = COUNT(*)   
    FROM PaymentRequestStatusM PRS  
    LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId   
    LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey  
    WHERE   
        (@SearchText IS NULL 
         OR MU.FullName LIKE '%' + @SearchText + '%' 
         OR CAST(MU.Mobile AS VARCHAR) = @SearchText)  
        AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)) 
        AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey);  
  
    -- Step 2: Query to fetch the data with dynamic sorting and pagination  
    IF @IsPaging = 1 
    BEGIN
        SELECT     
            MU.FullName,  
            MU.Mobile,  
            PRS.Amount AS RequestAmount,  
            PRS.CreatedOn,  
            REPLACE(PRS.Status, 'PAYMENT_SUCCESS', 'SUCCESS') AS Status,  
            ISNULL(PPR.Amount, 0) AS PaidAmount,  
            PPR.PaymentInstrumentType,  
            PPR.MerchantTransactionId,  
            ROW_NUMBER() OVER (  
                ORDER BY   
                    CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN PRS.CreatedOn END ASC,  
                    CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN PRS.CreatedOn END DESC,  
                    CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'ASC' THEN MU.FullName END ASC,  
                    CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'DESC' THEN MU.FullName END DESC  
            ) AS RowNum  
        INTO #TempPhonePe  
        FROM PaymentRequestStatusM PRS  
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId   
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey  
        WHERE   
            (@SearchText IS NULL 
             OR MU.FullName LIKE '%' + @SearchText + '%' 
             OR CAST(MU.Mobile AS VARCHAR) = @SearchText)  
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)) 
            AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey);  
  
        -- Step 3: Return paginated and sorted results  
        SELECT     
            FullName,  
            Mobile,  
            RequestAmount,  
            CreatedOn,  
            Status,  
            PaidAmount,  
            PaymentInstrumentType,  
            MerchantTransactionId  
        FROM #TempPhonePe  
        WHERE RowNum BETWEEN (@PageNumber - 1) * @PageSize + 1 AND @PageNumber * @PageSize  
        ORDER BY RowNum;  
  
        -- Clean up the temporary table  
        DROP TABLE IF EXISTS #TempPhonePe;  
    END
    ELSE
    BEGIN
        -- Return all data without pagination
        SELECT     
            MU.FullName,  
            MU.Mobile,  
            PRS.Amount AS RequestAmount,  
            PRS.CreatedOn,  
            REPLACE(PRS.Status, 'PAYMENT_SUCCESS', 'SUCCESS') AS Status,  
            ISNULL(PPR.Amount, 0) AS PaidAmount,  
            PPR.PaymentInstrumentType,  
            PPR.MerchantTransactionId
        FROM PaymentRequestStatusM PRS  
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId   
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey  
        WHERE   
            (@SearchText IS NULL 
             OR MU.FullName LIKE '%' + @SearchText + '%' 
             OR CAST(MU.Mobile AS VARCHAR) = @SearchText)  
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE)) 
            AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey);  
    END
END;  
GO



    
-- exec GetProductContentM 'E551010E-9795-EE11-812A-00155D23D79C' , 1, null , null    
    
ALTER PROCEDURE [dbo].[GetProductContentM]     
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
AND IsDeleted = 0
AND IsActive = 1
    
     
END    




GO
CREATE PROCEDURE [dbo].[GetProductById] @ProductId INT    
 ,@MobileUserKey UNIQUEIDENTIFIER    
AS    
BEGIN --declare @ProductId int = 5                                
 --,@MobileUserKey UNIQUEIDENTIFIER = '294A46B7-E918-EF11-B260-A630D7B5F938'                              
 DECLARE @priorDaysInfo INT = CAST((    
    SELECT TOP 1 value    
    FROM settings    
    WHERE code = 'PRIORDAYSINFO'    
    ) AS INT)    
 DECLARE @ContentCount INT = 0    
  ,@VideoContent INT = 0    
    
SELECT      @ContentCount = COUNT(Id),       @VideoContent = COUNT(CASE            WHEN attachmentType = 'video' THEN 1           ELSE NULL       END)   FROM PRODUCTSCONTENTM   
WHERE      ProductId = @ProductId     AND isActive = 1     AND isDeleted = 0; 
 
    
 DROP TABLE    
    
 IF EXISTS #tempBenefits    
  SELECT ben.Id    
   ,ben.GiftProductId    
   ,sub.NAME AS Names    
   ,ben.Months    
   ,pro.NAME    
   ,pro.Description    
  INTO #tempBenefits    
  FROM ExtraBenefitsM AS ben    
  INNER JOIN ProductsM AS pro ON ben.GiftProductId = pro.Id    
  INNER JOIN SubscriptionDurationM AS sub ON sub.Id = ben.SubscriptionId    
   AND isnull(sub.isActive, 1) = 1    
  WHERE ben.ProductId = @ProductId    
   AND ISNULL(pro.IsActive, 1) = 1    
    
 DECLARE @extraBenefits NVARCHAR(MAX) = (    
   SELECT * FROM #tempBenefits    
   FOR JSON AUTO    
   )    
 DECLARE @CurrentDate DATE = cast(getdate() AS DATE)    
 DECLARE @IsOutOfSubscription VARCHAR(300) = (    
   SELECT TOP 1 mobileUserKey    
   FROM MYBucketM b    
   WHERE productId = @ProductId    
    AND mobileUserKey = @MobileUserKey    
    AND ISNULL(IsACtive, 1) = 1    
    AND isnull(IsExpired, 0) = 0    
    AND @currentDate >= cast(b.StartDate AS DATE)    
    AND @currentDate <= cast(b.endDate AS DATE)    
   )    
 DECLARE @isExpired NVARCHAR(max) = (    
   SELECT TOP 1 IsExpired    
   FROM MYBucketM    
   WHERE productId = @ProductId    
    AND mobileUserKey = @MobileUserKey    
    AND ISNULL(IsACtive, 1) = 1    
    AND isnull(IsExpired, 0) = 0    
   )    
    
 SELECT TOP 1 p.id    
  ,p.NAME    
  ,p.Description    
  ,p.DescriptionTitle    
  ,p.CategoryID    
  ,pcm.NAME AS Category    
  ,    
  CAST(p.Price AS DECIMAL(16, 4)) AS Price    
  ,cast(pom.CouponKey AS VARCHAR(200)) AS CouponCode    
  ,isnull(pom.PaidAmount, 0.0) AS PaidAmount    
  ,CAST(ISNULL(pom.CouponDiscountAmount, 0.0) AS VARCHAR) AS Discount    
  ,CAST(ISNULL(PR.Rating, 0) AS VARCHAR) AS UserRating    
  ,'' AS Liked    
  ,'' AS EnableSubscription    
  ,( SELECT top 1 sv.DurationName FROM SubscriptionView sv WHERE ProductId = @ProductId and sv.DurationName != 'free' AND sv.DurationActive = 1   ) AS SubscriptionData    
  ,CAST(ISNULL(pl.LikeId, 0) AS BIT) AS IsHeart    
  ,CAST(0 AS BIT) AS IsThumbsUp    
  ,@extraBenefits AS ExtraBenefits    
  ,CAST(CASE     
    WHEN DATEDIFF(day, GETDATE(), mb.enddate) <= @priorDaysInfo    
     THEN 1    
    ELSE 0    
    END AS BIT) AS ShowReminder    
  ,CAST(IIF(@isExpired IS NOT NULL, 1, 0) AS BIT) AS IsInMyBucket    
  ,P.LandscapeImage AS LandscapeImage    
  ,CAST(IIF(@IsOutOfSubscription IS NULL, 0, 1) AS BIT) AS IsInValidity    
  ,(    
   SELECT *    
   FROM ProductsContentM    
   WHERE productId = @ProductId    
   FOR JSON AUTO    
   ) AS Content    
  ,(    
   --buy button text                                      
   CASE     
    WHEN mb.id IS NULL    
     THEN 'Buy'    
    ELSE CASE     
      WHEN cast(CASE     
         WHEN DATEDIFF(day, GETDATE(), mb.enddate) < @priorDaysInfo    
          THEN 1    
         ELSE 0    
         END AS BIT) = 1    
       THEN 'Renew'    
      ELSE 'Purchased'    
      END    
    END    
   ) AS BuyButtonText    
  ,@ContentCount AS ContentCount    
  ,@VideoContent AS VideoCount    
  ,DATEDIFF(day, getdate(), ISNULL(mb.enddate, GETDATE())) AS DaysToGo    
 FROM ProductsM AS P    
 INNER JOIN ProductCategoriesM AS pcm ON p.CategoryID = pcm.Id    
 LEFT JOIN PurchaseOrdersM AS POM ON POM.ProductId = p.Id AND pom.ProductId = @ProductId AND pom.ActionBy = @MobileUserKey    
 LEFT JOIN ProductsRatingM AS PR ON PR.ProductId = P.Id AND PR.CreatedBy = @MobileUserKey    
 LEFT JOIN ProductLikesM AS pl ON pl.ProductId = p.Id AND pl.LikeId = 1 AND pl.CreatedBy = @MobileUserKey AND pl.IsDelete = 0    
 LEFT JOIN subscriptiondurationm s ON s.Id = p.SubscriptionId     
 LEFT JOIN MYBucketM AS Mb ON p.id = mb.ProductId AND mb.mobileuserkey = @MobileUserkey     
 WHERE p.id = @ProductId    
 ORDER BY POM.CreatedOn DESC    
END 
GO

GO
ALTER PROCEDURE [dbo].[GetPhonePe]     
    @IsPaging INT = 0,    
    @PageSize INT = 5,     
    @PageNumber INT = 1,    
    @SortExpression VARCHAR(50) = 'CreatedOn', -- Default sort column   
    @SortOrder VARCHAR(50) = 'DESC', -- Default sort order  
    @RequestedBy VARCHAR(50) = NULL,     
    @FromDate VARCHAR(50) = NULL,     
    @ToDate VARCHAR(50) = NULL,     
    @SearchText VARCHAR(250) = NULL,     
    @PrimaryKey VARCHAR(50) = NULL, -- Used to filter by Status  
    @TotalCount INT = 0 OUTPUT   
AS     
BEGIN     
    -- Initialize TotalCount variable   
    SET @TotalCount = 0;   
   
    -- Step 1: Calculate the total count of records   
    SELECT @TotalCount = COUNT(*)    
    FROM PaymentRequestStatusM PRS   
    LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId    
    LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey   
    WHERE    
        (@SearchText IS NULL  
         OR MU.FullName LIKE '%' + @SearchText + '%'  
         OR CAST(MU.Mobile AS VARCHAR) = @SearchText)   
        AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))  
        AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey);   
   
    -- Step 2: Query to fetch the data with dynamic sorting and pagination   
    IF @IsPaging = 1  
    BEGIN 
        SELECT      
            MU.FullName,   
            P.Name AS ProductName,
            MU.Mobile,   
            PRS.Amount AS RequestAmount,  
            PPR.TransactionId, 
            PRS.CreatedOn,   
            REPLACE(PRS.Status, 'PAYMENT_SUCCESS', 'SUCCESS') AS Status,   
            ISNULL(PPR.Amount, 0) AS PaidAmount,   
            PPR.PaymentInstrumentType,   
            PPR.MerchantTransactionId,   
            ROW_NUMBER() OVER (   
                ORDER BY    
                    CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN PRS.CreatedOn END ASC,   
                    CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN PRS.CreatedOn END DESC,   
                    CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'ASC' THEN MU.FullName END ASC,   
                    CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'DESC' THEN MU.FullName END DESC   
            ) AS RowNum   
        INTO #TempPhonePe   
        FROM PaymentRequestStatusM PRS   
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId    
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey 
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id  
        WHERE    
            (@SearchText IS NULL  
             OR MU.FullName LIKE '%' + @SearchText + '%'  
             OR CAST(MU.Mobile AS VARCHAR) = @SearchText)   
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))  
            AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey);   
   
        -- Step 3: Return paginated and sorted results   
        SELECT      
            FullName,   
            ProductName, -- Include ProductName in the final output
            Mobile,   
            RequestAmount,
            TransactionId, -- Include TransactionId in the paginated output
            CreatedOn,   
            Status,   
            PaidAmount,   
            PaymentInstrumentType,   
            MerchantTransactionId   
        FROM #TempPhonePe   
        WHERE RowNum BETWEEN (@PageNumber - 1) * @PageSize + 1 AND @PageNumber * @PageSize   
        ORDER BY CreatedOn DESC; -- Default ordering by CreatedOn DESC   
   
        -- Clean up the temporary table   
        DROP TABLE IF EXISTS #TempPhonePe;   
    END 
    ELSE 
    BEGIN 
        -- Return all data without pagination 
        SELECT      
            MU.FullName, 
            P.Name AS ProductName, -- Include ProductName in the non-paginated results
            MU.Mobile,   
            PRS.Amount AS RequestAmount,  
            PPR.TransactionId, -- Include TransactionId in the non-paginated results
            PRS.CreatedOn,   
            REPLACE(PRS.Status, 'PAYMENT_SUCCESS', 'SUCCESS') AS Status,   
            ISNULL(PPR.Amount, 0) AS PaidAmount,   
            PPR.PaymentInstrumentType,   
            PPR.MerchantTransactionId 
        FROM PaymentRequestStatusM PRS   
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId    
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id  
        WHERE    
            (@SearchText IS NULL  
             OR MU.FullName LIKE '%' + @SearchText + '%'  
             OR CAST(MU.Mobile AS VARCHAR) = @SearchText)   
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))  
            AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey) 
        ORDER BY CreatedOn DESC; -- Default ordering by CreatedOn DESC
    END 
END;
GO

GO
ALTER PROCEDURE GetMobileUsers      
    @IsPaging BIT = 1,                                     
    @PageSize INT = 10,                                     
    @PageNumber INT = 1,                                     
    @SortExpression VARCHAR(50) = '',                                     
    @SortOrder VARCHAR(20) = '',                                     
    @RequestedBy VARCHAR(100) = NULL,                                     
    @FromDate DATETIME = NULL,                                     
    @ToDate DATETIME = NULL,                                     
    @SearchText VARCHAR(100) = NULL,                                     
    @PrimaryKey VARCHAR(100) = NULL,                                     
    @TotalCount INT OUTPUT                                     
AS             
BEGIN    
    SET NOCOUNT ON;    
    
    -- Set default NULL values for empty input    
    IF @SearchText = '' SET @SearchText = NULL;                                     
    IF @RequestedBy = '' SET @RequestedBy = NULL;         
    
    -- Count total matching records for paging    
    SELECT @TotalCount = COUNT(1)                                  
    FROM MobileUsers MU    
    WHERE                                     
        (    
            (MU.Mobile LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)     
            OR (MU.EmailId LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)     
            OR (MU.FullName LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)    
        )    
        AND (CAST(MU.RegistrationDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))    
        AND (@PrimaryKey IS NULL OR LOWER(MU.DeviceType) LIKE '%' + LOWER(@PrimaryKey) + '%');     
    
    -- Main query to retrieve user data    
    SELECT      
        MU.Id,     
        MU.FullName,
        MU.PublicKey,     
        MU.EmailId,     
        MU.Mobile,     
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
        COUNT(MB.MobileUserKey) AS BucketCount,    
        (    
            SELECT     
                MB.Id,   
                MB.ProductName,     
                MB.CreatedDate,     
                MB.EndDate,    
                MB.ProductId,    
                MB.StartDate,    
               DATEDIFF(DAY, MB.StartDate, MB.EndDate) AS ValidityInDays    
            FROM MYBucketM MB    
            WHERE MB.MobileUserKey = MU.PublicKey    
            FOR JSON PATH    
        ) AS BucketData    
    FROM MobileUsers MU    
    LEFT JOIN MYBucketM MB     
        ON MU.PublicKey = MB.MobileUserKey    
    WHERE    
        (    
            (MU.Mobile LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)     
            OR (MU.EmailId LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)     
            OR (MU.FullName LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)    
        )    
        AND (CAST(MU.RegistrationDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))    
        AND (@PrimaryKey IS NULL OR LOWER(MU.DeviceType) LIKE '%' + LOWER(@PrimaryKey) + '%')    
    GROUP BY     
        MU.Id,     
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
        MU.ModifiedOn    
ORDER BY ISNULL(mu.ModifiedOn,mu.RegistrationDate) desc 
        OFFSET IIF(@PageNumber <= 1, 0, (@PageNumber - 1)) * @PageSize ROWS         
        FETCH NEXT @PageSize ROWS ONLY;             
    
    SET NOCOUNT OFF;    
END; 
GO

GO

ALTER PROCEDURE GetUserHistory
    @PageNumber INT = 1,          -- Page number (default is 1)
    @PageSize INT = 20,           -- Number of records per page (default is 20)
    @PublicKey VARCHAR(50)        -- Mobile number for filtering
AS
BEGIN
    SET NOCOUNT ON;

    WITH CTE_MobileUsers AS
    (
        SELECT         
            MU.FullName,     
            MU.EmailId, 
            MU.PublicKey,
            MU.LastLoginDate,    
            MU.Mobile,
            -- Aggregating ticket status to determine if there's at least one 'Open' ticket
            CASE
                WHEN COUNT(CASE WHEN LOWER(T.[Status]) LIKE '%o%' THEN 1 END) > 0 THEN 'Open'
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
            COUNT(DISTINCT MB.Id) AS BucketCount,    
            (
                SELECT     
                    MB.ProductId AS BucketProductId,
                    MB.ProductName,     
                    MB.CreatedDate,     
                    MB.EndDate,    
                    MB.StartDate,    
                    DATEDIFF(DAY, MB.StartDate, MB.EndDate) AS ValidityInDays    
                FROM MYBucketM MB    
                WHERE MB.MobileUserKey = @PublicKey
                FOR JSON PATH    
            ) AS BucketData,

            COUNT(DISTINCT PO.Id) AS PurchaseCount,    
            (
                SELECT     
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
                WHERE PO.PublicKey = @PublicKey
                FOR JSON PATH    
            ) AS PurchaseData,

            COUNT(DISTINCT FT.Id) AS FreeTrailCount,    
            (
                SELECT     
                    FT.EndDate,
                    FT.StartDate,
                    STRING_AGG(P.Name, ', ') AS ProductNames  
                FROM FreeTrialBasketDetailM FTBD 
                JOIN ProductsM P ON FTBD.ProductId = P.Id 
                FOR JSON PATH
            ) AS FreeTrailProductNames,

            CAST(CASE WHEN FT.IsActive = 1 THEN 'Active' ELSE 'Inactive' END AS NVARCHAR(50)) AS FreeTrailStatus,

            (
                SELECT     
                    FT.EndDate AS FreeTrailEndDate,
                    MU.FullName AS UserName,
                    FT.StartDate AS FreeTrailStartDate,
                    P.Name
                FROM FreeTrialM FT
                LEFT JOIN  FreeTrialBasketDetailM FTBD 
                ON FT.FreeTrialBasketId = FTBD.FreeTrialBasketId
                LEFT JOIN ProductsM P ON FTBD.ProductId = P.Id
                WHERE FT.MobileUserKey = MU.PublicKey    
                FOR JSON PATH    
            ) AS FreeTrailData

        FROM MobileUsers MU    
        LEFT JOIN MYBucketM MB ON MU.PublicKey = MB.MobileUserKey
        LEFT JOIN PurchaseOrdersM PO ON MU.Mobile = PO.Mobile 
        LEFT JOIN FreeTrialM FT ON MU.PublicKey = FT.MobileUserKey 
        LEFT JOIN FreeTrialBasketM FTB ON FT.FreeTrialBasketId = FTB.Id  
        LEFT JOIN TicketM T ON MU.PublicKey = T.CreatedBy AND T.IsActive = 1 AND T.IsDelete = 0
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
            FT.FreeTrialBasketId,
            FT.EndDate,
            FT.IsActive,
            MU.ModifiedOn,
            FT.StartDate,
            FT.EndDate,
            MU.LastLoginDate
    )
    SELECT *
    FROM CTE_MobileUsers
    WHERE PublicKey = @PublicKey
    ORDER BY FullName
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO

GO
ALTER PROCEDURE [dbo].[GetPhonePe]     
    @IsPaging INT = 0,    
    @PageSize INT = 5,     
    @PageNumber INT = 1,    
    @SortExpression VARCHAR(50) = 'CreatedOn', -- Default sort column   
    @SortOrder VARCHAR(50) = 'DESC', -- Default sort order  
    @RequestedBy VARCHAR(50) = NULL,     
    @FromDate VARCHAR(50) = NULL,     
    @ToDate VARCHAR(50) = NULL,     
    @SearchText VARCHAR(250) = NULL,     
    @PrimaryKey VARCHAR(50) = NULL, -- Used to filter by Status  
    @TotalCount INT = 0 OUTPUT   
AS     
BEGIN     
    -- Initialize TotalCount variable   
    SET @TotalCount = 0;   
   
    -- Step 1: Calculate the total count of records   
    SELECT @TotalCount = COUNT(*)    
    FROM PaymentRequestStatusM PRS   
    LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId    
    LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey   
    WHERE    
        (@SearchText IS NULL  
         OR MU.FullName LIKE '%' + @SearchText + '%'  
         OR CAST(MU.Mobile AS VARCHAR) = @SearchText)   
        AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))  
        AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey);   
   
    -- Step 2: Query to fetch the data with dynamic sorting and pagination   
    IF @IsPaging = 1  
    BEGIN 
        SELECT      
            MU.FullName,   
            P.Name AS ProductName,
            MU.Mobile,   
            PRS.Amount AS RequestAmount,  
            PPR.TransactionId, 
            PRS.CreatedOn,   
            REPLACE(PRS.Status, 'PAYMENT_SUCCESS', 'SUCCESS') AS Status,   
            ISNULL(PPR.Amount, 0) AS PaidAmount,   
            PPR.PaymentInstrumentType,   
            PPR.MerchantTransactionId,   
            ROW_NUMBER() OVER (   
                ORDER BY    
                    CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN PRS.CreatedOn END ASC,   
                    CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN PRS.CreatedOn END DESC,   
                    CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'ASC' THEN MU.FullName END ASC,   
                    CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'DESC' THEN MU.FullName END DESC   
            ) AS RowNum   
        INTO #TempPhonePe   
        FROM PaymentRequestStatusM PRS   
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId    
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey 
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id  
        WHERE    
            (@SearchText IS NULL  
             OR MU.FullName LIKE '%' + @SearchText + '%'  
             OR CAST(MU.Mobile AS VARCHAR) = @SearchText)   
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))  
            AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey);   
   
        -- Step 3: Return paginated and sorted results   
        SELECT      
            FullName,   
            ProductName, -- Include ProductName in the final output
            Mobile,   
            RequestAmount,
            TransactionId, -- Include TransactionId in the paginated output
            CreatedOn,   
            Status,   
            PaidAmount,   
            PaymentInstrumentType,   
            MerchantTransactionId   
        FROM #TempPhonePe   
        WHERE RowNum BETWEEN (@PageNumber - 1) * @PageSize + 1 AND @PageNumber * @PageSize   
        ORDER BY CreatedOn DESC; -- Default ordering by CreatedOn DESC   
   
        -- Clean up the temporary table   
        DROP TABLE IF EXISTS #TempPhonePe;   
    END 
    ELSE 
    BEGIN 
        -- Return all data without pagination 
        SELECT      
            MU.FullName, 
            P.Name AS ProductName, -- Include ProductName in the non-paginated results
            MU.Mobile,   
            PRS.Amount AS RequestAmount,  
            PPR.TransactionId, -- Include TransactionId in the non-paginated results
            PRS.CreatedOn,   
            REPLACE(PRS.Status, 'PAYMENT_SUCCESS', 'SUCCESS') AS Status,   
            ISNULL(PPR.Amount, 0) AS PaidAmount,   
            PPR.PaymentInstrumentType,   
            PPR.MerchantTransactionId 
        FROM PaymentRequestStatusM PRS   
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId    
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id  
        WHERE    
            (@SearchText IS NULL  
             OR MU.FullName LIKE '%' + @SearchText + '%'  
             OR CAST(MU.Mobile AS VARCHAR) = @SearchText)   
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))  
            AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey) 
        ORDER BY CreatedOn DESC; -- Default ordering by CreatedOn DESC
    END 
END;
GO

GO
ALTER PROCEDURE GetMobileUsers      
    @IsPaging BIT = 1,                                     
    @PageSize INT = 10,                                     
    @PageNumber INT = 1,                                     
    @SortExpression VARCHAR(50) = '',                                     
    @SortOrder VARCHAR(20) = '',                                     
    @RequestedBy VARCHAR(100) = NULL,                                     
    @FromDate DATETIME = NULL,                                     
    @ToDate DATETIME = NULL,                                     
    @SearchText VARCHAR(100) = NULL,                                     
    @PrimaryKey VARCHAR(100) = NULL,                                     
    @TotalCount INT OUTPUT                                     
AS             
BEGIN    
    SET NOCOUNT ON;    
    
    -- Set default NULL values for empty input    
    IF @SearchText = '' SET @SearchText = NULL;                                     
    IF @RequestedBy = '' SET @RequestedBy = NULL;         
    
    -- Count total matching records for paging    
    SELECT @TotalCount = COUNT(1)                                  
    FROM MobileUsers MU    
    WHERE                                     
        (    
            (MU.Mobile LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)     
            OR (MU.EmailId LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)     
            OR (MU.FullName LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)    
        )    
        AND (CAST(MU.RegistrationDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))    
        AND (@PrimaryKey IS NULL OR LOWER(MU.DeviceType) LIKE '%' + LOWER(@PrimaryKey) + '%');     
    
    -- Main query to retrieve user data    
    SELECT      
        MU.Id,     
        MU.FullName,
        MU.PublicKey,     
        MU.EmailId,     
        MU.Mobile,     
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
        COUNT(MB.MobileUserKey) AS BucketCount,    
        (    
            SELECT     
                MB.Id,   
                MB.ProductName,     
                MB.CreatedDate,     
                MB.EndDate,    
                MB.ProductId,    
                MB.StartDate,    
               DATEDIFF(DAY, MB.StartDate, MB.EndDate) AS ValidityInDays    
            FROM MYBucketM MB    
            WHERE MB.MobileUserKey = MU.PublicKey    
            FOR JSON PATH    
        ) AS BucketData    
    FROM MobileUsers MU    
    LEFT JOIN MYBucketM MB     
        ON MU.PublicKey = MB.MobileUserKey    
    WHERE    
        (    
            (MU.Mobile LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)     
            OR (MU.EmailId LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)     
            OR (MU.FullName LIKE '%' + @SearchText + '%' OR @SearchText IS NULL)    
        )    
        AND (CAST(MU.RegistrationDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))    
        AND (@PrimaryKey IS NULL OR LOWER(MU.DeviceType) LIKE '%' + LOWER(@PrimaryKey) + '%')    
    GROUP BY     
        MU.Id,     
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
        MU.ModifiedOn    
ORDER BY ISNULL(mu.ModifiedOn,mu.RegistrationDate) desc 
        OFFSET IIF(@PageNumber <= 1, 0, (@PageNumber - 1)) * @PageSize ROWS         
        FETCH NEXT @PageSize ROWS ONLY;             
    
    SET NOCOUNT OFF;    
END; 
GO

GO

ALTER PROCEDURE GetUserHistory
    @PageNumber INT = 1,          -- Page number (default is 1)
    @PageSize INT = 20,           -- Number of records per page (default is 20)
    @PublicKey VARCHAR(50)        -- Mobile number for filtering
AS
BEGIN
    SET NOCOUNT ON;

    WITH CTE_MobileUsers AS
    (
        SELECT         
            MU.FullName,     
            MU.EmailId, 
            MU.PublicKey,
            MU.LastLoginDate,    
            MU.Mobile,
            -- Aggregating ticket status to determine if there's at least one 'Open' ticket
            CASE
                WHEN COUNT(CASE WHEN LOWER(T.[Status]) LIKE '%o%' THEN 1 END) > 0 THEN 'Open'
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
            COUNT(DISTINCT MB.Id) AS BucketCount,    
            (
                SELECT     
                    MB.ProductId AS BucketProductId,
                    MB.ProductName,     
                    MB.CreatedDate,     
                    MB.EndDate,    
                    MB.StartDate,    
                    DATEDIFF(DAY, MB.StartDate, MB.EndDate) AS ValidityInDays    
                FROM MYBucketM MB    
                WHERE MB.MobileUserKey = @PublicKey
                FOR JSON PATH    
            ) AS BucketData,

            COUNT(DISTINCT PO.Id) AS PurchaseCount,    
            (
                SELECT     
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
                WHERE PO.PublicKey = @PublicKey
                FOR JSON PATH    
            ) AS PurchaseData,

            COUNT(DISTINCT FT.Id) AS FreeTrailCount,    
            (
                SELECT     
                    FT.EndDate,
                    FT.StartDate,
                    STRING_AGG(P.Name, ', ') AS ProductNames  
                FROM FreeTrialBasketDetailM FTBD 
                JOIN ProductsM P ON FTBD.ProductId = P.Id 
                FOR JSON PATH
            ) AS FreeTrailProductNames,

            CAST(CASE WHEN FT.IsActive = 1 THEN 'Active' ELSE 'Inactive' END AS NVARCHAR(50)) AS FreeTrailStatus,

            (
                SELECT     
                    FT.EndDate AS FreeTrailEndDate,
                    MU.FullName AS UserName,
                    FT.StartDate AS FreeTrailStartDate,
                    P.Name
                FROM FreeTrialM FT
                LEFT JOIN  FreeTrialBasketDetailM FTBD 
                ON FT.FreeTrialBasketId = FTBD.FreeTrialBasketId
                LEFT JOIN ProductsM P ON FTBD.ProductId = P.Id
                WHERE FT.MobileUserKey = MU.PublicKey    
                FOR JSON PATH    
            ) AS FreeTrailData

        FROM MobileUsers MU    
        LEFT JOIN MYBucketM MB ON MU.PublicKey = MB.MobileUserKey
        LEFT JOIN PurchaseOrdersM PO ON MU.Mobile = PO.Mobile 
        LEFT JOIN FreeTrialM FT ON MU.PublicKey = FT.MobileUserKey 
        LEFT JOIN FreeTrialBasketM FTB ON FT.FreeTrialBasketId = FTB.Id  
        LEFT JOIN TicketM T ON MU.PublicKey = T.CreatedBy AND T.IsActive = 1 AND T.IsDelete = 0
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
            FT.FreeTrialBasketId,
            FT.EndDate,
            FT.IsActive,
            MU.ModifiedOn,
            FT.StartDate,
            FT.EndDate,
            MU.LastLoginDate
    )
    SELECT *
    FROM CTE_MobileUsers
    WHERE PublicKey = @PublicKey
    ORDER BY FullName
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END;
GO



Go 

alter PROCEDURE [dbo].[CheckLeadDuplicateByNumber] 
    @MobileNumber VARCHAR(12),    
    @LeadKey VARCHAR(50) = NULL    
AS    
BEGIN    
    DECLARE @DuplicateByNumber TABLE (    
        Result NVARCHAR(200),    
        LeadName NVARCHAR(50),    
        Remarks NVARCHAR(500),    
        MobileNumber NVARCHAR(50),    
        AlternateMobileNumber NVARCHAR(50),    
        EmailId NVARCHAR(50),    
        PublicKey UNIQUEIDENTIFIER,    
        AssignedTo NVARCHAR(50),    
        ModifiedOn VARCHAR(50) NULL    
    )    
    
    INSERT INTO @DuplicateByNumber    
    SELECT TOP 1    
        (    
            CASE     
                WHEN le.AssignedTo IS NULL    
                    THEN 'Unassigned'    
                WHEN le.AssignedTo IS NOT NULL AND DATEDIFF(DAY, le.ModifiedOn, GETDATE()) > 15 AND po.id IS NULL    
                    THEN 'AssignedUntouched'    
                ELSE 'Assigned'    
            END    
        ) AS Result,    
        FullName AS LeadName,    
        Remarks,    
        le.MobileNumber,    
        le.AlternateMobileNumber,    
        le.EmailId,    
        le.PublicKey,    
        us.FirstName AS AssignedTo,    
        le.ModifiedOn    
    FROM Leads AS le    
    LEFT JOIN Users AS us ON le.AssignedTo = us.PublicKey    
    LEFT JOIN PurchaseOrders po ON po.leadid = le.id AND po.leadid IS NOT NULL   
    WHERE ((le.MobileNumber = @MobileNumber OR le.AlternateMobileNumber = @MobileNumber)    
            AND ISNULL(@LeadKey, '00000000-0000-0000-0000-000000000000') = '00000000-0000-0000-0000-000000000000')    
        OR (le.MobileNumber = @MobileNumber AND le.PublicKey <> ISNULL(@LeadKey, le.PublicKey))    
    
    IF EXISTS (
        SELECT 1    
        FROM Leads AS le    
        WHERE (le.MobileNumber = @MobileNumber OR le.AlternateMobileNumber = @MobileNumber)    
          AND le.IsDelete = 1    
    )    
    BEGIN    
        SELECT 'Not Exists' AS Result,    
               '' AS LeadName,    
               '' AS Remarks,    
               '' AS MobileNumber,    
               '' AS AlternateMobileNumber,    
               '' AS EmailId,    
               NULL AS PublicKey,    
               '' AS AssignedTo,    
               NULL AS ModifiedOn    
    END    
    ELSE IF NOT EXISTS (
        SELECT TOP 1 1    
        FROM @DuplicateByNumber    
    )    
    BEGIN    
        SELECT 'Not Exists' AS Result,    
               '' AS LeadName,    
               '' AS Remarks,    
               '' AS MobileNumber,    
               '' AS AlternateMobileNumber,    
               '' AS EmailId,    
               NULL AS PublicKey,    
               '' AS AssignedTo,    
               NULL AS ModifiedOn    
    END    
    ELSE    
    BEGIN    
        SELECT *    
        FROM @DuplicateByNumber    
    END    
END
Go

GO
ALTER PROCEDURE [dbo].[GetFreeTrial]   
    @IsPaging BIT = 1,  
    @PageSize INT = 10,   
    @PageNumber INT = 1,  
    @SortExpression VARCHAR(50) = 'StartDate', -- Default sort column   
    @SortOrder VARCHAR(20) = 'DESC', -- Default sort order   
    @RequestedBy VARCHAR(100) = NULL,  
    @FromDate DATETIME = NULL,  
    @ToDate DATETIME = NULL,  
    @SearchText VARCHAR(100) = NULL,  
    @PrimaryKey VARCHAR(100) = NULL,  
    @TotalCount INT OUTPUT  
AS           
BEGIN                                   
    SET NOCOUNT ON; -- Prevents extra result sets from interfering with SELECT queries  
  
    -- Normalize NULL or empty input values  
    IF @SearchText = '' SET @SearchText = NULL;  
    IF @RequestedBy = '' SET @RequestedBy = NULL;  
  
    -- Step 1: Create a temporary table to hold base data  
    CREATE TABLE #BaseData  
    (  
        FullName NVARCHAR(255), 
        PublicKey  NVARCHAR(50), 
        Mobile NVARCHAR(50),  
        StartDate DATETIME,  
        EndDate DATETIME,  
        ProductNames NVARCHAR(MAX),  
        Validity INT,  
        Status NVARCHAR(50)  
    );  
  
    -- Step 2: Insert the base dataset into the temporary table  
    INSERT INTO #BaseData (FullName, Mobile, PublicKey, StartDate, EndDate, ProductNames, Validity, Status)  
    SELECT    
        MU.FullName,  
        MU.Mobile,  
        MU.PublicKey,  
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
            ) AS P2  
        ) AS ProductNames,  
        MIN(FTB.DaysInNumber) AS Validity,  
CAST(
    CASE 
        WHEN DATEDIFF(DAY, FM.EndDate, GETDATE()) < 0 THEN 'Active' 
        ELSE 'Expired' 
    END 
AS NVARCHAR(50)) AS Status
    FROM MobileUsers MU  
    LEFT JOIN FreeTrialM FM ON MU.PublicKey = FM.MobileUserKey  
    LEFT JOIN FreeTrialBasketM FTB ON FM.FreeTrialBasketId = FTB.Id  
    WHERE                                   
        (                                   
            (@SearchText IS NULL OR MU.Mobile LIKE '%' + @SearchText + '%') OR   
            (@SearchText IS NULL OR MU.EmailId LIKE '%' + @SearchText + '%') OR   
            (@SearchText IS NULL OR MU.FullName LIKE '%' + @SearchText + '%')   
        )          
        AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(MU.RegistrationDate AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))  
    GROUP BY  
        MU.FullName,  
        MU.Mobile, 
        MU.PublicKey,  
        FM.StartDate,  
        FM.EndDate,  
        FM.IsActive,  
        FM.FreeTrialBasketId;  
  
    -- Step 3: Calculate the total count after applying @PrimaryKeytatus filter  
    SELECT @TotalCount = COUNT(*)  
    FROM #BaseData  
    WHERE (@PrimaryKey IS NULL OR Status = @PrimaryKey);  
  
    -- Step 4: Fetch paginated and sorted results from the temporary table  
    SELECT *  
    FROM #BaseData  
    WHERE (@PrimaryKey IS NULL OR Status = @PrimaryKey)  
    ORDER BY     
        CASE WHEN @SortOrder = 'ASC' THEN   
            CASE @SortExpression   
                WHEN 'StartDate' THEN StartDate  
                WHEN 'EndDate' THEN EndDate  
                ELSE StartDate -- Default sorting  
            END   
        END ASC,  
        CASE WHEN @SortOrder = 'DESC' THEN   
            CASE @SortExpression   
                WHEN 'StartDate' THEN StartDate  
                WHEN 'EndDate' THEN EndDate  
                ELSE StartDate -- Default sorting  
            END   
        END DESC  
    OFFSET (IIF(@PageNumber <= 1, 0, (@PageNumber - 1))) * @PageSize ROWS       
    FETCH NEXT @PageSize ROWS ONLY;   
  
    -- Step 5: Drop the temporary table  
    DROP TABLE #BaseData;  
END 
GO

GO
ALTER PROCEDURE GetUserHistory 
    @PageNumber INT = 1,          -- Page number (default is 1) 
    @PageSize INT = 20,           -- Number of records per page (default is 20) 
    @PublicKey VARCHAR(50)        -- Mobile number for filtering 
AS 
BEGIN 
    SET NOCOUNT ON; 
 
    WITH CTE_MobileUsers AS 
    ( 
        SELECT          
            MU.FullName,      
            MU.EmailId,  
            MU.PublicKey, 
            MU.LastLoginDate,     
            MU.Mobile, 
            -- Aggregating ticket status to determine if there's at least one 'Open' ticket 
            CASE 
                WHEN COUNT(CASE WHEN LOWER(T.[Status]) LIKE '%o%' THEN 1 END) > 0 THEN 'Open' 
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
            COUNT(DISTINCT MB.Id) AS BucketCount,     
            ( 
                SELECT      
                    MB.ProductId AS BucketProductId, 
                    MB.ProductName,      
                    MB.CreatedDate,      
                    MB.EndDate,     
                    MB.StartDate,     
                    DATEDIFF(DAY, MB.StartDate, MB.EndDate) AS ValidityInDays     
                FROM MYBucketM MB     
                WHERE MB.MobileUserKey = @PublicKey 
                FOR JSON PATH     
            ) AS BucketData, 
 
            COUNT(DISTINCT PO.Id) AS PurchaseCount,     
            ( 
                SELECT      
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
                WHERE PO.ActionBy = @PublicKey 
                FOR JSON PATH     
            ) AS PurchaseData, 
 
            COUNT(DISTINCT FT.Id) AS FreeTrailCount,     
            ( 
                SELECT      
                    FT.EndDate, 
                    FT.StartDate, 
                    STRING_AGG(P.Name, ', ') AS ProductNames   
                FROM FreeTrialBasketDetailM FTBD  
                JOIN ProductsM P ON FTBD.ProductId = P.Id  
                FOR JSON PATH 
            ) AS FreeTrailProductNames, 
 
            CAST(CASE WHEN FT.IsActive = 1 THEN 'Active' ELSE 'Inactive' END AS NVARCHAR(50)) AS FreeTrailStatus, 
 
            ( 
                SELECT      
                    FT.EndDate AS FreeTrailEndDate, 
                    MU.FullName AS UserName, 
                    FT.StartDate AS FreeTrailStartDate, 
                    P.Name 
                FROM FreeTrialM FT 
                LEFT JOIN  FreeTrialBasketDetailM FTBD  
                ON FT.FreeTrialBasketId = FTBD.FreeTrialBasketId 
                LEFT JOIN ProductsM P ON FTBD.ProductId = P.Id 
                WHERE FT.MobileUserKey = MU.PublicKey     
                FOR JSON PATH     
            ) AS FreeTrailData 
 
        FROM MobileUsers MU     
        LEFT JOIN MYBucketM MB ON MU.PublicKey = MB.MobileUserKey 
        LEFT JOIN PurchaseOrdersM PO ON MU.PublicKey = PO.ActionBy  
        LEFT JOIN FreeTrialM FT ON MU.PublicKey = FT.MobileUserKey  
        LEFT JOIN FreeTrialBasketM FTB ON FT.FreeTrialBasketId = FTB.Id   
        LEFT JOIN TicketM T ON MU.PublicKey = T.CreatedBy AND T.IsActive = 1 AND T.IsDelete = 0 
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
            FT.FreeTrialBasketId, 
            FT.EndDate, 
            FT.IsActive, 
            MU.ModifiedOn, 
            FT.StartDate, 
            FT.EndDate, 
            MU.LastLoginDate 
    ) 
    SELECT * 
    FROM CTE_MobileUsers 
    WHERE PublicKey = @PublicKey 
    ORDER BY FullName 
    OFFSET (@PageNumber - 1) * @PageSize ROWS 
    FETCH NEXT @PageSize ROWS ONLY; 
END; 
GO

GO
ALTER PROCEDURE [dbo].[GetPhonePe]      
    @IsPaging INT = 0,     
    @PageSize INT = 5,      
    @PageNumber INT = 1,     
    @SortExpression VARCHAR(50) = 'CreatedOn', -- Default sort column    
    @SortOrder VARCHAR(50) = 'DESC', -- Default sort order   
    @RequestedBy VARCHAR(50) = NULL,      
    @FromDate VARCHAR(50) = NULL,      
    @ToDate VARCHAR(50) = NULL,      
    @SearchText VARCHAR(250) = NULL,      
    @PrimaryKey VARCHAR(50) = NULL, -- Used to filter by Status   
    @TotalCount INT = 0 OUTPUT    
AS      
BEGIN      
    -- Initialize TotalCount variable    
    SET @TotalCount = 0;    
    
    -- Step 1: Calculate the total count of records    
    SELECT @TotalCount = COUNT(*)     
    FROM PaymentRequestStatusM PRS    
    LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId     
    LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey    
    WHERE     
        (@SearchText IS NULL   
         OR MU.FullName LIKE '%' + @SearchText + '%'   
         OR CAST(MU.Mobile AS VARCHAR) = @SearchText)    
        AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))   
        AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey);    
    
    -- Step 2: Query to fetch the data with dynamic sorting and pagination    
    IF @IsPaging = 1   
    BEGIN  
        SELECT       
            MU.FullName,    
            P.Name AS ProductName, 
            MU.Mobile,    
            PRS.Amount AS RequestAmount,   
            PPR.TransactionId,  
            PRS.CreatedOn,    
            REPLACE(PRS.Status, 'PAYMENT_SUCCESS', 'SUCCESS') AS Status,    
            ISNULL(PPR.Amount, 0) AS PaidAmount,    
            PPR.PaymentInstrumentType,    
            PPR.MerchantTransactionId,    
            MU.PublicKey,   -- Added PublicKey here
            ROW_NUMBER() OVER (    
                ORDER BY     
                    CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN PRS.CreatedOn END ASC,    
                    CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN PRS.CreatedOn END DESC,    
                    CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'ASC' THEN MU.FullName END ASC,    
                    CASE WHEN @SortExpression = 'FullName' AND @SortOrder = 'DESC' THEN MU.FullName END DESC    
            ) AS RowNum    
        INTO #TempPhonePe    
        FROM PaymentRequestStatusM PRS    
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId     
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey  
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id   
        WHERE     
            (@SearchText IS NULL   
             OR MU.FullName LIKE '%' + @SearchText + '%'   
             OR CAST(MU.Mobile AS VARCHAR) = @SearchText)    
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))   
            AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey);    
    
        -- Step 3: Return paginated and sorted results    
        SELECT       
            FullName,    
            ProductName, -- Include ProductName in the final output 
            Mobile,    
            RequestAmount, 
            TransactionId, -- Include TransactionId in the paginated output 
            CreatedOn,    
            Status,    
            PaidAmount,    
            PaymentInstrumentType,    
            MerchantTransactionId,
            PublicKey   -- Added PublicKey here
        FROM #TempPhonePe    
        WHERE RowNum BETWEEN (@PageNumber - 1) * @PageSize + 1 AND @PageNumber * @PageSize    
        ORDER BY CreatedOn DESC; -- Default ordering by CreatedOn DESC    
    
        -- Clean up the temporary table    
        DROP TABLE IF EXISTS #TempPhonePe;    
    END  
    ELSE  
    BEGIN  
        -- Return all data without pagination  
        SELECT       
            MU.FullName,  
            P.Name AS ProductName, -- Include ProductName in the non-paginated results 
            MU.Mobile,    
            PRS.Amount AS RequestAmount,   
            PPR.TransactionId, -- Include TransactionId in the non-paginated results 
            PRS.CreatedOn,    
            REPLACE(PRS.Status, 'PAYMENT_SUCCESS', 'SUCCESS') AS Status,    
            ISNULL(PPR.Amount, 0) AS PaidAmount,    
            PPR.PaymentInstrumentType,    
            PPR.MerchantTransactionId,
            MU.PublicKey  -- Added PublicKey here
        FROM PaymentRequestStatusM PRS    
        LEFT JOIN PhonePePaymentResponseM PPR ON PRS.TransactionId = PPR.MerchantTransactionId     
        LEFT JOIN MobileUsers MU ON PRS.CreatedBy = MU.PublicKey 
        LEFT JOIN ProductsM P ON PRS.ProductId = P.Id   
        WHERE     
            (@SearchText IS NULL   
             OR MU.FullName LIKE '%' + @SearchText + '%'   
             OR CAST(MU.Mobile AS VARCHAR) = @SearchText)    
            AND (@FromDate IS NULL OR @ToDate IS NULL OR CAST(PRS.CreatedOn AS DATE) BETWEEN CAST(@FromDate AS DATE) AND CAST(@ToDate AS DATE))   
            AND (@PrimaryKey IS NULL OR PRS.Status = @PrimaryKey)  
        ORDER BY CreatedOn DESC; -- Default ordering by CreatedOn DESC 
    END  
END;
GO

GO
ALTER PROCEDURE GetPartnerDematAccounts     
    @PageSize INT = 100,         
    @PageNumber INT = 1,         
    @FromDate DATE = NULL,         
    @ToDate DATE = NULL,     
    @Status INT = 0,   
    @SearchText NVARCHAR(100) = NULL,   
    @TotalCount INT OUTPUT     
AS     
BEGIN     
    DECLARE @StartRow INT = (@PageNumber - 1) * @PageSize;     
     
    SELECT      
        CAST(ROW_NUMBER() OVER (ORDER BY pa.CreatedOn DESC) AS INT) AS SlNo,     
        mu.EmailId,     
        mu.FullName,
        MU.PublicKey,
        MU.Id AS UserId,     
        mu.Mobile,     
        pa.Id,     
        pa.PartnerId,     
        pa.PartnerName,     
        pa.API,     
        pa.SecretKey,     
        pa.CreatedOn, 
		  pa.ModifiedOn, 
   COALESCE(modifier_mobile.FullName,  
             CONCAT(modifier_user.FirstName, ' ', modifier_user.LastName)) AS ModifiedBy 
    FROM PartnerAccountsM pa     
    LEFT JOIN MobileUsers mu ON pa.CreatedBy = mu.PublicKey 
	LEFT JOIN MobileUsers modifier_mobile ON pa.ModifiedBy = modifier_mobile.PublicKey 
    LEFT JOIN Users modifier_user ON pa.ModifiedBy = modifier_user.PublicKey 
    WHERE      
        ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, pa.CreatedOn)), ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999))     
        BETWEEN ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999)     
        AND ISNULL(@ToDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))     
        AND (@Status = -1 OR pa.IsActive = @Status)   
   AND (@SearchText IS NULL          OR mu.FullName LIKE '%' + @SearchText + '%'         OR mu.Mobile LIKE '%' + @SearchText + '%'         OR mu.EmailId LIKE '%' + @SearchText + '%')   
   
    ORDER BY ISNULL(pa.ModifiedOn,pa.CreatedOn) DESC      
    OFFSET @StartRow ROWS FETCH NEXT @PageSize ROWS ONLY;     
     
    SELECT @TotalCount = COUNT(1)     
    FROM PartnerAccountsM pa     
    LEFT JOIN MobileUsers mu ON pa.CreatedBy = mu.PublicKey  
	 
    WHERE      
        ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, pa.CreatedOn)), ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999))     
        BETWEEN ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999)     
        AND ISNULL(@ToDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))     
        AND (@Status = -1 OR pa.IsActive = @Status)   
  AND (@SearchText IS NULL          OR mu.FullName LIKE '%' + @SearchText + '%'         OR mu.Mobile LIKE '%' + @SearchText + '%'         OR mu.EmailId LIKE '%' + @SearchText + '%');     
END 
GO

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
        
    ORDER BY ISNULL(t.ModifiedOn,t.CreatedOn) DESC             
    OFFSET @Offset ROWS             
    FETCH NEXT @RowsPerPage ROWS ONLY;             
             
    -- Step 5: Drop the temporary table          
    DROP TABLE #tempRecords;             
END; 
GO

GO
ALTER PROCEDURE GetUserHistory 
    @PageNumber INT = 1,          -- Page number (default is 1) 
    @PageSize INT = 20,           -- Number of records per page (default is 20) 
    @PublicKey VARCHAR(50)        -- Mobile number for filtering 
AS 
BEGIN 
    SET NOCOUNT ON; 
 
    WITH CTE_MobileUsers AS 
    ( 
        SELECT          
            MU.FullName,      
            MU.EmailId,  
            MU.PublicKey, 
            MU.LastLoginDate,     
            MU.Mobile, 
            -- Aggregating ticket status to determine if there's at least one 'Open' ticket 
            CASE 
                WHEN COUNT(CASE WHEN LOWER(T.[Status]) LIKE '%o%' THEN 1 END) > 0 THEN 'Open' 
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
            COUNT(DISTINCT MB.Id) AS BucketCount,     
            ( 
                SELECT 
                    MB.ID,
                    MB.ProductId AS BucketProductId, 
                    MB.ProductName,      
                    MB.CreatedDate,      
                    MB.EndDate,     
                    MB.StartDate,     
                    DATEDIFF(DAY, MB.StartDate, MB.EndDate) AS ValidityInDays     
                FROM MYBucketM MB     
                WHERE MB.MobileUserKey = @PublicKey 
                FOR JSON PATH     
            ) AS BucketData, 
 
            COUNT(DISTINCT PO.Id) AS PurchaseCount,     
            ( 
                SELECT      
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
                WHERE PO.ActionBy = @PublicKey 
                FOR JSON PATH     
            ) AS PurchaseData, 
 
            COUNT(DISTINCT FT.Id) AS FreeTrailCount,     
            ( 
                SELECT      
                    FT.EndDate, 
                    FT.StartDate, 
                    STRING_AGG(P.Name, ', ') AS ProductNames   
                FROM FreeTrialBasketDetailM FTBD  
                JOIN ProductsM P ON FTBD.ProductId = P.Id  
                FOR JSON PATH 
            ) AS FreeTrailProductNames, 
 
            CAST(CASE WHEN FT.IsActive = 1 THEN 'Active' ELSE 'Inactive' END AS NVARCHAR(50)) AS FreeTrailStatus, 
 
            ( 
                SELECT      
                    FT.EndDate AS FreeTrailEndDate, 
                    MU.FullName AS UserName, 
                    FT.StartDate AS FreeTrailStartDate, 
                    P.Name 
                FROM FreeTrialM FT 
                LEFT JOIN  FreeTrialBasketDetailM FTBD  
                ON FT.FreeTrialBasketId = FTBD.FreeTrialBasketId 
                LEFT JOIN ProductsM P ON FTBD.ProductId = P.Id 
                WHERE FT.MobileUserKey = MU.PublicKey     
                FOR JSON PATH     
            ) AS FreeTrailData 
 
        FROM MobileUsers MU     
        LEFT JOIN MYBucketM MB ON MU.PublicKey = MB.MobileUserKey 
        LEFT JOIN PurchaseOrdersM PO ON MU.PublicKey = PO.ActionBy  
        LEFT JOIN FreeTrialM FT ON MU.PublicKey = FT.MobileUserKey  
        LEFT JOIN FreeTrialBasketM FTB ON FT.FreeTrialBasketId = FTB.Id   
        LEFT JOIN TicketM T ON MU.PublicKey = T.CreatedBy AND T.IsActive = 1 AND T.IsDelete = 0 
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
            FT.FreeTrialBasketId, 
            FT.EndDate, 
            FT.IsActive, 
            MU.ModifiedOn, 
            FT.StartDate, 
            FT.EndDate, 
            MU.LastLoginDate 
    ) 
    SELECT * 
    FROM CTE_MobileUsers 
    WHERE PublicKey = @PublicKey 
    ORDER BY FullName 
    OFFSET (@PageNumber - 1) * @PageSize ROWS 
    FETCH NEXT @PageSize ROWS ONLY; 
END; 
GO

GO
ALTER PROCEDURE [dbo].[GetPOReport]  
    @StartDate DATE = NULL,   
    @EndDate DATE = NULL,   
    @StatusId INT,  
    @LeadSourceId INT,  
    @PageNumber INT = 1,  -- Page number parameter (input)
    @PageSize INT = 10,   -- Page size parameter (input)
    @TotalSalesAmount DECIMAL(18, 4) OUTPUT,
    @TotalCount INT OUTPUT
AS   
BEGIN   
    -- Drop the temporary table if it exists
    DROP TABLE IF EXISTS #TempGetPOReportTable;

    -- Step 1: Create the temporary table to hold the filtered data
    SELECT 
        po.ClientName,   
        po.Mobile,   
        s.Name AS StatusName,   
        po.Remark,   
        po.PaymentDate,   
        po.PaidAmount,   
        pm.Name AS ModeOfPayment,   
        po.TransasctionReference,   
        po.StartDate,   
        po.EndDate,   
        LS.Name AS LeadSource   
    INTO #TempGetPOReportTable   
    FROM PurchaseOrders AS po   
    INNER JOIN Leads AS LE ON PO.LeadId = LE.Id   
    INNER JOIN LeadSources AS LS ON LS.PublicKey = LE.LeadSourceKey  
        AND LS.Id = ISNULL(@LeadSourceId, LS.ID)   
    INNER JOIN PaymentModes AS pm ON po.ModeOfPayment = pm.Id   
    INNER JOIN STATUS AS s ON po.STATUS = s.id   
    INNER JOIN Services AS ser ON po.ServiceID = ser.Id   
    WHERE CAST(PaymentDate AS DATE) BETWEEN @StartDate AND @EndDate   
      AND po.STATUS = ISNULL(@StatusId, po.STATUS);

    -- Step 2: Calculate the total sales amount
    SELECT @TotalSalesAmount = SUM(PaidAmount)   
    FROM #TempGetPOReportTable;

    -- Step 3: Calculate the total count
    SELECT @TotalCount = COUNT(1)   
    FROM #TempGetPOReportTable;

    -- Step 4: Return paginated results using OFFSET and FETCH
    SELECT *   
    FROM #TempGetPOReportTable   
    ORDER BY PaymentDate DESC   -- Modify this ORDER BY as per your requirement (e.g., sort by date or other column)
    OFFSET (@PageNumber - 1) * @PageSize ROWS   
    FETCH NEXT @PageSize ROWS ONLY;   

    -- Step 5: Drop the temporary table
    DROP TABLE IF EXISTS #TempGetPOReportTable;  
END;

GO


Go
alter PROCEDURE ManageCouponM            
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
        IF @mobileNumbers IS NULL            
        BEGIN            
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
             
        -- check if coupon name is changed and it has 0 Total Redeems              
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
        ELSE            
        BEGIN            
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
            
   select @newCouponId = id from couponsm where PublicKey = @couponKey            
                     
            
            -- insert into coupon product mapping                    
            IF @productIds IS NULL            
            BEGIN            
                IF NOT EXISTS            
                (            
                    SELECT 1            
                    FROM CouponProductMappingM            
                    WHERE CouponId = @newCouponId            
                          AND ProductId IS NULL            
                )            
                BEGIN            
                    INSERT INTO CouponProductMappingM            
                    (            
                        CouponId,            
                        ProductId            
                    )            
                    VALUES            
                    (@newCouponId, NULL)            
                END            
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
                FROM string_split(@productIds, ',') AS product            
                WHERE NOT EXISTS            
                (            
                    SELECT 1            
                    FROM CouponProductMappingM            
                    WHERE CouponId = @newCouponId            
                          AND ProductId = product.value            
                )            
            END            
            
            
            -- insert into coupon user mapping              
		-- If @mobileNumbers is NULL or empty string
		IF @mobileNumbers IS NULL
		BEGIN
			-- Remove all mappings for the given CouponId
			DELETE FROM CouponUserMappingM
			WHERE CouponId = @newCouponId;
			 BEGIN            
                IF NOT EXISTS            
                (            
                    SELECT 1            
                    FROM CouponUserMappingM            
                    WHERE CouponId = @newCouponId            
                          AND MobileUserKey IS NULL            
                )            
                BEGIN           
                    INSERT INTO CouponUserMappingM            
                    (            
                        CouponId,            
                        MobileUserKey            
                    )            
                    VALUES            
                    (@newCouponId, NULL);            
                END            
            END            
		END
		ELSE
		BEGIN
		   IF EXISTS            
			(            
				SELECT 1            
				FROM CouponUserMappingM            
				WHERE CouponId = @newCouponId            
					  AND MobileUserKey IS NULL            
			)
			BEGIN
				-- Delete the record with MobileUserKey IS NULL
				DELETE FROM CouponUserMappingM
				WHERE CouponId = @newCouponId
				  AND MobileUserKey IS NULL;
			END			   
			-- Remove mappings for users not in the provided list
			DELETE FROM CouponUserMappingM
			WHERE CouponId = @newCouponId
			  AND MobileUserKey NOT IN (
				  SELECT MU.PublicKey
				  FROM string_split(@mobileNumbers, ',') AS ss 
				  INNER JOIN MobileUsers MU
				  ON MU.mobile = LTRIM(RTRIM(ss.value))  
				  where isActive = 1 and IsDelete = 0  
			  );

			-- Add mappings for new users in the provided list
			INSERT INTO CouponUserMappingM
			(CouponId, MobileUserKey)
			SELECT @newCouponId, MU.PublicKey
			FROM string_split(@mobileNumbers, ',') AS ss
			INNER JOIN MobileUsers MU
			ON MU.mobile = LTRIM(RTRIM(ss.value))  where isActive = 1 and IsDelete = 0      
			and NOT EXISTS (
				SELECT 1
				FROM CouponUserMappingM
				WHERE CouponId = @newCouponId
				  AND MobileUserKey = MU.PublicKey
			);
		END  
   set @result = 'UPDATED'            
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
Go

Go
alter PROCEDURE GetSubscriptionPlanWithProduct    
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
           pm.Price as ActualPrice,    
           (((pm.Price * sm.DiscountPercentage) / 100)) as DiscountPrice,    
           (pm.Price - ((pm.Price * sm.DiscountPercentage) / 100)) as NetPayment,    
           'DIWALI2024' as CouponCode,    
           DATEADD(MONTH, sd.Months, GETDATE()) as ExpireOn,    
           -- sm.IsActive AS SubscriptionMappingActive,                     
           sd.Id AS SubscriptionDurationId,    
           CAST(sd.Months as varchar) + iif(CAST(sd.Months as int) = 1, ' Month', ' Months') AS SubscriptionDurationName,    
           sd.Months,    
           CAST(0 as bit) IsRecommended,    
          -- CAST(FORMAT(((pm.Price - ((pm.Price * sm.DiscountPercentage) / 100)) / sd.Months), 'N2') AS VARCHAR) + '/m' as PerMonth,   
    CAST(CEILING((pm.Price - ((pm.Price * sm.DiscountPercentage) / 100)) / sd.Months) AS VARCHAR) + '/m' AS PerMonth,  
  
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
          AND LOWER(@DeviceType) IN (    
                                        SELECT VALUE FROM string_split('android,ios', ',')    
                                    )    
    
         ORDER BY SD.Months     
END
GO


Go
Create  PROCEDURE GetSubscriptionDetails    
 @PageSize INT ,    
@PageNumber INT,    
@SearchText NVARCHAR(255) = NULL,    
@TotalCount INT OUTPUT       
AS    
BEGIN    
    SET NOCOUNT ON;    
    
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;    
    
    SELECT  
 ROW_NUMBER() OVER ( ORDER BY isnull(sm.ModifiedOn, sm.CreatedOn) desc)  AS SlNo, 

 sm.id as MappingId,
        p.id AS ProductId,     
        p.Name AS ProductName, 
		sd.Id as DurationId,
        sd.Name AS DurationName,     
        sd.Months AS DurationMonths,     
        sm.DiscountPercentage,     
        sd.IsActive AS DurationStatus,
		sp.Id as PlanId,
  sp.Name as PlanName,  
        sp.IsActive AS PlanStatus,    
  sm.IsActive as MappingStatus    
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
		and 
		(p.IsDeleted =  0 )
    ORDER BY isnull(sm.ModifiedOn, sm.CreatedOn) desc    
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
		and 
		(p.IsDeleted =  0 );  
END;  
Go