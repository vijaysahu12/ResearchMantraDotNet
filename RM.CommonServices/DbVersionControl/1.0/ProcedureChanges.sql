
          
        
GO   
-- EXEC oTPlOGIN '8895804280',null,null,null,  '91', null                                                         

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
        
    -- Fetch minute difference for retry attempts                            
    SELECT TOP 1        
        @MinuteDifference = DATEDIFF(MINUTE, ModifiedOn, GETDATE())        
    FROM MobileUsers        
    WHERE Mobile = @MobileNumber        
          AND IsDelete = 0        
          AND ISNULL(SelfDeleteRequest, 0) = 0;        
        
    -- Check OTP Limit                            
    IF (        
           @MinuteDifference < 30        
           AND EXISTS        
    (        
        SELECT 1        
        FROM MobileUsers        
        WHERE Mobile = @MobileNumber        
              AND RetryAttempt > 2        
              and IsActive = 1        
              and IsDelete = 0        
    )        
       )        
    BEGIN        
        SET @Result = 'OTPLIMITREACHED';        
        SELECT @Result AS Result,        
               FullName,        
               EmailId,        
               'Otp Limit Reached. Try again in ' + CAST(30 - @MinuteDifference AS VARCHAR) + ' minutes' AS Message,        
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
        RETURN;        
    END        
    -- Handle Existing Users                            
    IF EXISTS        
    (        
        SELECT 1        
        FROM MobileUsers        
        WHERE Mobile = @MobileNumber        
              AND IsDelete = 0        
              AND IsActive = 1        
              AND (        
                      SelfDeleteRequestDate IS NULL        
                      OR cast(SelfDeleteRequestDate as date) >= CAST(GETDATE() AS DATE)        
                  )        
    )        
    BEGIN        
        IF EXISTS        
        (        
            SELECT 1        
            FROM MobileUsers        
            WHERE Mobile = @MobileNumber        
                  AND IsDelete = 0        
                  AND IsActive = 1        
                  AND (        
                          SelfDeleteRequestDate IS NULL        
                          OR cast(SelfDeleteRequestDate as date) >= CAST(GETDATE() AS DATE)        
                      )        
                  and SelfDeleteRequest = 1        
        )        
        BEGIN        
            UPDATE MobileUsers        
            set SelfDeleteRequest = null,        
                SelfDeleteRequestDate = null        
            WHERE Mobile = @MobileNumber        
                  AND IsDelete = 0        
                  AND IsActive = 1        
                  AND (        
                          SelfDeleteRequestDate IS NULL        
                          OR cast(SelfDeleteRequestDate as date) >= CAST(GETDATE() AS DATE)        
      )        
                  and SelfDeleteRequest = 1        
        END        
        UPDATE MobileUsers        
        SET OneTimePassword = @Otp,        
            ModifiedOn = GETDATE(),        
   RetryAttempt = CASE        
                               WHEN @MinuteDifference > 30 THEN        
                                   1        
                               ELSE        
                                   RetryAttempt + 1        
                           END        
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
        RETURN;        
    END        
    ELSE IF EXISTS        
    (        
        SELECT 1        
        FROM MobileUsers        
        WHERE Mobile = @MobileNumber        
              AND IsDelete = 0        
              AND IsActive = 1        
              and ISNULL(SelfDeleteRequest, 0) = 0        
    )        
    BEGIN        
        -- Insert lead                    
        INSERT INTO Leads        
        (        
            FullName,        
            MobileNumber,        
            CreatedBy,        
            CreatedOn,        
            ModifiedOn,        
            ModifiedBy,        
            Remarks        
        )        
        VALUES        
        (   '',        
            @MobileNumber,        
            (        
                SELECT Value FROM Settings WHERE Code = 'admin'        
            ),        
            GETDATE(),        
            GETDATE(),        
            (        
                SELECT Value FROM Settings WHERE Code = 'admin'        
            ),        
            'Mobile app user'        
        );        
        -- Insert New User if not exists                            
        INSERT INTO MobileUsers        
        (        
            FullName,        
            LeadKey,        
            Password,        
            OneTimePassword,        
            IsOtpVerified,        
            Mobile,        
            DeviceType,        
            CreatedOn,        
            ModifiedOn,        
            MobileToken,        
            IMEI,        
            StockNature,        
            AgreeToTerms,        
            SameForWhatsApp,        
            IsActive,        
            IsDelete,        
            ProfileImage,        
            About,        
            RegistrationDate,        
            LastLoginDate,        
            EmailId,        
            CountryCode,        
            RetryAttempt        
        )        
        VALUES        
        (   '',        
            (        
                select publickey from leads where id = @@IDENTITY        
            ),        
            '123456',        
            @Otp,        
            0,        
            @MobileNumber,        
            ISNULL(@DeviceType, ''),        
            GETDATE(),        
            GETDATE(),        
            'REGISTERED',        
            '',        
            '',        
            1,        
            1,        
            1,        
            0,        
            NULL,        
            '213',        
            GETDATE(),        
            GETDATE(),        
            '',        
            @CountryCode,        
            1        
        );        
        
        
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
        RETURN;        
    END        
        
    -- Handle Self-Deleted Users                            
    IF EXISTS        
    (        
        SELECT 1        
        FROM MobileUsers        
        WHERE Mobile = @MobileNumber        
              AND IsActive = 1        
              AND SelfDeleteRequest = 1        
              AND cast(SelfDeleteRequestDate as Date) <= cast(GETDATE() as Date)        
    )        
    BEGIN        
        -- Mark user as deleted                            
        --UPDATE MobileUsers        
        --SET IsDelete = 1,        
        --    FullName = '',IsOtpVerified = 0        
        --WHERE Mobile = @MobileNumber        
        --      AND IsActive = 1        
        --      AND SelfDeleteRequest = 1        
        --      AND cast(SelfDeleteRequestDate as Date) <= cast(GETDATE() as Date)    
         
  DELETE from MobileUsers    
  WHERE Mobile = @MobileNumber        
              AND IsActive = 1        
              AND SelfDeleteRequest = 1        
              AND cast(SelfDeleteRequestDate as Date) <= cast(GETDATE() as Date)       
        
        -- Insert Lead if not exists                            
        
        INSERT INTO Leads        
        (        
            FullName,        
            MobileNumber,        
            CreatedBy,        
            CreatedOn,        
            ModifiedOn,        
            ModifiedBy,        
            Remarks        
        )        
        VALUES        
        (   '',        
            @MobileNumber,        
            (        
                SELECT Value FROM Settings WHERE Code = 'admin'        
            ),        
            GETDATE(),        
            GETDATE(),        
            (        
                SELECT Value FROM Settings WHERE Code = 'admin'        
            ),        
            'Mobile app user'        
        );        
        
        -- Insert New User                            
        INSERT INTO MobileUsers        
        (        
            FullName,        
            LeadKey,        
            Password,        
            OneTimePassword,        
            IsOtpVerified,        
            Mobile,        
            DeviceType,        
            CreatedOn,        
            ModifiedOn,        
            MobileToken,        
            IMEI,        
            StockNature,        
            AgreeToTerms,        
            SameForWhatsApp,        
            IsActive,        
            IsDelete,        
            ProfileImage,        
            About,        
            RegistrationDate,        
            LastLoginDate,        
            EmailId,        
            CountryCode,        
            RetryAttempt        
        )        
        VALUES        
        (   '',        
            (        
                select publickey from leads where id = @@IDENTITY        
            ),        
            '123456',        
            @Otp,        
            0,        
            @MobileNumber,        
            ISNULL(@DeviceType, ''),        
            GETDATE(),        
            GETDATE(),        
            'REGISTERED',        
            '',        
            '',        
            1,        
            1,        
            1,        
            0,        
            NULL,        
            '213',        
            GETDATE(),        
            GETDATE(),        
            '',        
 @CountryCode,        
            1        
        );        
        
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
        RETURN;        
    END        
        
    IF NOT EXISTS (SELECT 1 FROM MobileUsers WHERE Mobile = @MobileNumber)        
    BEGIN        
        -- Insert Lead if not exists                            
        
        INSERT INTO Leads        
        (        
            FullName,        
            MobileNumber,        
            CreatedBy,        
            CreatedOn,        
            ModifiedOn,        
            ModifiedBy,        
            Remarks        
        )        
        VALUES        
        (   '',        
            @MobileNumber,        
            (        
                SELECT Value FROM Settings WHERE Code = 'admin'        
            ),        
            GETDATE(),        
            GETDATE(),        
            (        
                SELECT Value FROM Settings WHERE Code = 'admin'        
            ),        
            'Mobile app user'        
        );        
        
        -- Insert New User                            
        INSERT INTO MobileUsers        
        (        
            FullName,        
            LeadKey,        
            Password,        
            OneTimePassword,        
            IsOtpVerified,        
            Mobile,        
            DeviceType,        
            CreatedOn,        
            ModifiedOn,        
            MobileToken,        
            IMEI,        
            StockNature,        
            AgreeToTerms,        
            SameForWhatsApp,        
            IsActive,        
            IsDelete,        
            ProfileImage,        
            About,        
            RegistrationDate,        
            LastLoginDate,        
            EmailId,        
            CountryCode,        
            RetryAttempt        
        )        
        VALUES        
        (   '',        
            (        
                select publickey from leads where id = @@IDENTITY        
            ),        
            '123456',        
            @Otp,        
            0,        
            @MobileNumber,        
            ISNULL(@DeviceType, ''),        
            GETDATE(),        
            GETDATE(),        
            'REGISTERED',        
            '',        
            '',        
          1,        
            1,        
            1,        
            0,        
            NULL,        
            '213',        
            GETDATE(),        
            GETDATE(),        
            '',        
            @CountryCode,        
            1        
        );        
    print('oputside')      
      
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
        RETURN;        
    END        
        
        
END;
GO
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
GO
 -- exec GetProductById 7 , '8C81A1B9-88A1-EF11-B327-EE73E7D1DF4C'     

ALTER PROCEDURE [dbo].[GetProductById]  
    @ProductId INT,  
    @MobileUserKey UNIQUEIDENTIFIER  
AS  
BEGIN  
    --declare @ProductId int = 5                          
    --,@MobileUserKey UNIQUEIDENTIFIER = '294A46B7-E918-EF11-B260-A630D7B5F938'                        
  
    DECLARE @priorDaysInfo INT = CAST(  
                                 (  
                                     SELECT TOP 1 value FROM settings WHERE code = 'PRIORDAYSINFO'  
                                 ) AS INT)  
    DECLARE @ContentCount int = 0,  
            @VideoContent int = 0  
    SELECT @ContentCount = COUNT(Id),  
           @VideoContent = count(   CASE  
                                        WHEN attachmentType = 'video' THEN  
                                            1  
                                        ELSE  
                                            NULL  
                                    END  
                                )  
    FROM PRODUCTSCONTENTM  
    WHERE ProductId = @ProductId  
  
    DROP TABLE IF EXISTS #tempBenefits  
  
  
    SELECT ben.Id,  
           ben.GiftProductId,  
           sub.Name as Names,  
           ben.Months,  
           pro.Name,  
           pro.Description  
    INTO #tempBenefits  
    FROM ExtraBenefitsM AS ben  
        INNER JOIN ProductsM AS pro  
            ON ben.GiftProductId = pro.Id  
        INNER JOIN subscriptionduration AS sub  
            ON sub.Id = ben.SubscriptionId  
               AND isnull(sub.isActive, 1) = 1  
    WHERE ben.ProductId = @ProductId  
          AND ISNULL(pro.IsActive, 1) = 1  
  
    DECLARE @extraBenefits NVARCHAR(MAX) = (  
                                               SELECT * FROM #tempBenefits FOR JSON AUTO  
                                           )  
  
  
  
    DECLARE @CurrentDate date = cast(getdate() AS date)  
    DECLARE @IsOutOfSubscription VARCHAR(300) = (  
                                                    SELECT TOP 1  
                                                        mobileUserKey  
                                                    FROM MYBucketM b  
                                                    WHERE productId = @ProductId  
                                                          AND mobileUserKey = @MobileUserKey  
                                                          AND ISNULL(IsACtive, 1) = 1  
                                                          AND isnull(IsExpired, 0) = 0  
                                                         AND @currentDate >= cast(b.StartDate as date)  
                                                          AND @currentDate <= cast(b.endDate as date)  
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
  
    SELECT top 1  
        p.id,  
        p.name,  
        p.Description,  
        p.DescriptionTitle,  
        p.CategoryID,  
        pcm.Name as Category,  
        --CAST(ISNULL(pom.NetAmount,p.Price) as decimal(16,4)) as Price,                          
        CAST(p.Price as decimal(16, 4)) as Price,  
        cast(pom.CouponKey as varchar(200)) AS CouponCode,  
        isnull(pom.PaidAmount, 0.0) as PaidAmount,  
        CAST(ISNULL(pom.CouponDiscountAmount, 0.0) as VARCHAR) AS Discount,  
        CAST(ISNULL(PR.Rating, 0) AS varchar) AS UserRating,  
        '' AS Liked,  
        '' AS EnableSubscription,  
        (s.name) AS SubscriptionData,  
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
        ( --buy button text                                
        CASE  
            WHEN mb.id IS NULL THEN  
                'Buy'  
            ELSE   
                CASE  
                    WHEN cast(CASE  
                                  WHEN DATEDIFF(day, GETDATE(), mb.enddate) < @priorDaysInfo THEN  
                                      1  
                                  ELSE  
                                      0  
                              END AS BIT) = 1 THEN  
                        'Renew'  
                    ELSE  
                        'Purchased'  
                END  
        END  
        ) AS BuyButtonText,  
        @ContentCount AS ContentCount,  
        @VideoContent AS VideoCount,  
        DATEDIFF(day, getdate(), ISNULL(mb.enddate, GETDATE())) as DaysToGo  
    FROM ProductsM as P  
        INNER JOIN ProductCategoriesM as pcm  
            on p.CategoryID = pcm.Id  
        LEFT JOIN PurchaseOrdersM AS POM  
            on POM.ProductId = p.Id  
               and pom.ProductId = @ProductId  
               and pom.ActionBy = @MobileUserKey  
        LEFT JOIN ProductsRatingM AS PR  
            ON PR.ProductId = P.Id  
               AND PR.CreatedBy = @MobileUserKey  
        LEFT JOIN ProductLikesM AS pl  
            on pl.ProductId = p.Id  
               AND pl.LikeId = 1  
               AND pl.CreatedBy = @MobileUserKey  
               AND pl.IsDelete = 0  
        LEFT JOIN subscriptionduration s  
            ON s.Id = p.SubscriptionId  
        LEFT JOIN MYBucketM as Mb  
            on p.id = mb.ProductId  
               and mb.mobileuserkey = @MobileUserkey  
    WHERE p.id = @ProductId  
    order by POM.CreatedOn desc  
  
END  
GO 

---------------------------------------------------------------------------------------------------------------------------------------------------------------------


GO
    --exec GetScreenerDetails        
CREATE PROCEDURE GetScreenerDetails        
 @IsPaging bit = 1,              
 @PageSize int =25,              
 @PageNumber int =1,           
 @SortExpression varchar(50) = '',           
 @SortOrder varchar(50) = '',           
 @RequestedBy varchar(50)= null,           
 @SearchText varchar(100)= null,           
 @FromDate DateTime= null,           
 @ToDate DateTime= null,           
 @StrategyKey varchar(50) = null,           
 @TotalCount INT = 0 OUTPUT           
AS  
BEGIN  
    -- Fetch all screener details along with their category information  
    SELECT   
        S.Id AS ScreenerId,  
        S.Name AS ScreenerName,  
        S.Code,  
        S.Description AS ScreenerDescription,  
        S.IsActive AS ScreenerIsActive,   
		s.BackgroundColor,
		S.Icon as ScreenerIcon,
        SC.Id AS CategoryId,  
        SC.CategoryName,  
        SC.Description AS CategoryDescription,  
        SC.Image as CategoryImage,  
        SC.BackgroundColor as CategoryBackGroundColor,  
        SC.IsActive AS CategoryIsActive,  
        cast( 1 as bit) as SubscriptionStatus  
          
    FROM Screener S  
    INNER JOIN ScreenerCategorym SC ON S.CategoryId = SC.Id  
    WHERE S.IsActive = 1 AND SC.IsActive = 1 -- Fetch only active screeners and categories  
    ORDER BY SC.CategoryName, S.Name; -- Sort by category and screener name  
END;
GO

---------------------------------------------------------------------------------------------------------------------------------------------------------------------


 GO          
-- exec GetCallPerformanceM 'MB',null,null,null,null,null,null          
          
ALTER PROCEDURE GetCallPerformanceM          
    @Code VARCHAR(50),          
    @TotalTrades INT OUTPUT,          
    @TotalProfitable INT OUTPUT,          
    @TotalLoss INT OUTPUT,          
    @TradeClosed INT OUTPUT,          
    @Balance nvarchar(100) OUTPUT,          
 @TradeOpen INT OUTPUT          
AS          
BEGIN          
    SET NOCOUNT ON;          
          
          
 DECLARE @InvestmentAmount int = 5000          
          
    IF @code = 'SI'          
    BEGIN          
        SELECT          
            @TotalTrades = COUNT(*),          
            @TotalProfitable = CAST(SUM(CASE WHEN Pnl > 0 and CallStatus = 'Closed' THEN 1 ELSE 0 END) AS INT),          
            @TotalLoss = CAST(SUM(CASE WHEN Pnl < 0 and CallStatus = 'Closed' THEN 1 ELSE 0 END) AS INT),          
            @TradeClosed = CAST(SUM(CASE WHEN CallStatus = 'Closed' THEN 1 ELSE 0 END) AS INT),          
			@TradeOpen = CAST(SUM(CASE WHEN CallStatus = 'Open' THEN 1 ELSE 0 END) AS INT),          
            @Balance = N'₹' + FORMAT(SUM(CASE WHEN CallStatus = 'Closed' THEN Pnl ELSE 0 END), 'N0', 'en-IN')          
        FROM callperformance          
        WHERE StrategyKey IN (SELECT PublicKey FROM Strategies WHERE Name = 'Harmonic')          
          AND Pnl IS NOT NULL          
          AND Pnl != 0;          
          
        SELECT          
            StockKey AS Symbol,          
            cast(ROI as varchar(100)) as Roi,          
            N'₹' + FORMAT(EntryPrice, 'N0', 'en-IN') AS EntryPrice,          
            CallStatus AS Status,          
            'N/A' AS Duration,          
			N'₹' + FORMAT(ExitPrice, 'N0', 'en-IN') as ExitPrice ,           
            NULL AS Cmp,          
   CONCAT(                 N'Invest ₹', @InvestmentAmount, N' turned into ₹',                 CAST(@InvestmentAmount * (1 + ROI / 100.0) AS INT)             ) AS InvestmentMessage          
        FROM callperformance          
        WHERE StrategyKey IN (SELECT PublicKey FROM Strategies WHERE Name = 'Harmonic')          
          AND Pnl IS NOT NULL          
          AND Pnl != 0    
  ORDER BY  cast(ROI as decimal(18,2)) DESC    
        
    END          
    ELSE IF @code = 'MB'          
    BEGIN          
        
   SELECT          
            @TotalTrades = COUNT(*),          
            @TotalProfitable = CAST(SUM(CASE WHEN Pnl > 0 THEN 1 ELSE 0 END) AS INT),          
            @TotalLoss = CAST(SUM(CASE WHEN Pnl < 0 THEN 1 ELSE 0 END) AS INT),          
            @TradeClosed = 0,          
   @TradeOpen = @TotalTrades,          
            @Balance = N'₹' + FORMAT(SUM(Pnl), 'N0', 'en-IN')          
        FROM xl_harryportfolio          
                  
        
   ;WITH hf_CTE          
   as (select *,cast( CASE          
                  WHEN entryPrice = 0          
                       OR entryPrice IS NULL THEN          
                      0          
                  ELSE          
                      ROUND(((ltp - entryPrice) / entryPrice) * 100.0, 2)          
              END as varchar(100)) as Roi          
       from xl_harryportfolio          
      )          
  SELECT instrument as Symbol,          
         cast(Roi as varchar(100)) as Roi,          
         N'₹' + FORMAT(EntryPrice, 'N0', 'en-IN') AS EntryPrice,          
         'Open' as Status,          
         'N/A' as Duration,          
         '1' as ExitPrice,          
         cast(ltp as int) as Cmp,          
         CONCAT(          
                   N'Invest ₹',          
                   @InvestmentAmount,          
                   N' turned into ₹',          
                   CAST(@InvestmentAmount * (1 + Roi / 100.0) AS INT)          
               ) AS InvestmentMessage          
  FROM hf_CTE          
    END          
END;          
          
          
--avgprice = entry          
--ltp = cmp           
--exit = null as status is open          
--roi =  ltp - avg     
          
          
--avgprice = entry          
--ltp = cmp           
--exit = null as status is open          
--roi =  ltp - avg 
        
        
--avgprice = entry        
--ltp = cmp         
--exit = null as status is open        
--roi =  ltp - avg
GO


------------------------------------------------------------------------------------------------------------------------------------------------------------------
GO 
--  exec ActivateFreeTrialM '3B21B407-6D64-EF11-8175-00155D23D79C'                     
ALTER PROCEDURE [dbo].[ActivateFreeTrialM]        
 @mobileUserKey UNIQUEIDENTIFIER            
AS            
BEGIN            
        
        
BEGIN TRY        
--check if free trial not activated         
--check if in mybucket there is no product matching to current plan         
declare @TrialInDays int =0      
,@Result varchar(100) = ''     
,@Message varchar(500)        
    
--SELECT @MobileUserId  = Id From MobileUsers where PublicKey = @mobileUserKey         
--select * From FreeTrialM      
    
    
         
select @TrialInDays = DaysInNumber  FROM FreeTrialBasketM WHERE isnull(isActive,1) = 1 and isnull(isExpired,0) = 0         
        
--IF NOT EXISTS (SELECT  1 FROM FreeTrialM WHERE MobileUserKey = @mobileUserKey)        
--BEGIN        
-- print 'Activated'        
        
 --IF NOT EXISTS (SELECT top 1 1 FROM MYBucketM where MobileUserKey = @mobileUserKey  )        
 --BEGIN        
      
  --declare @mobileUserKey UNIQUEIDENTIFIER     = '1194D99E-C419-EF11-B261-88B133B31C8F'          
  ----INSERT INTO MYBucketM        
  ----select @mobileUserKey , pm.Id , pm.Name,getdate() as StartDate,DATEADD(day, @TrialInDays, GETDATE()) as EndDate , 1 Status,@mobileUserKey CreatedBy ,GETDATE() createdOn,        
  ----@mobileUserKey as ModifiedBy ,GETDATE() as ModifiedOn ,1 IsActive ,0 as IsExpired , 1 as Notification          
  ----From FreeTrialBasketM as b         
  ----INNER JOIN FreeTrialBasketDetailM as bd on bd.FreeTrialBasketId = b.Id        
  ----INNER JOIN ProductsM as pm on pm.Id = bd.ProductId        
  ----WHERE b.IsActive = 1 and IsExpired = 0 --and pm.id = 5         
        
         
 INSERT INTO MYBucketM        
 select @mobileUserKey , pm.Id , pm.Name,getdate() as StartDate,DATEADD(day, @TrialInDays, GETDATE()) as EndDate , 1 Status,@mobileUserKey CreatedBy ,GETDATE() createdOn,        
 @mobileUserKey as ModifiedBy ,GETDATE() as ModifiedOn ,1 IsActive ,0 as IsExpired , 1 as Notification          
 FROM FreeTrialBasketM as b        
 inner join FreeTrialBasketDetailM as bd on bd.FreeTrialBasketId = b.Id        
 INNER JOIN ProductsM as pm  on pm.Id = bd.ProductId        
 where pm.Id not in (select ProductId from MYBucketM where MobileUserKey = @mobileUserKey)    
 and ISNULL(b.IsActive,1) = 1         
 AND ISNULL(b.isExpired,0) = 0       
        
        
 INSERT INTO FreeTrialM         
 select @mobileUserKey, 1 , GETDATE() as StartDate ,  DATEADD(day, @TrialInDays, GETDATE()) as EndDate , 1 ,GETDATE() , GETDATE(), Id From     
 FreeTrialBasketM  where ISNULL(IsActive,1) = 1 and ISNULL(isExpired,0) = 0         
        
        
  SET @Result = 'FreeTrialActivated'         
  SET @Message = 'Enjoy ' + CAST(@TrialInDays as varchar) + ' days of free trial! Visit MyBucket to check your subscription validity'        
 --END        
 --ELSE BEGIN         
 -- SET @Result = 'alreadyAvailed'         
 -- SET @Message = 'You already availed the service.. please renew to continue the benefits.'        
 --END         
        
        
--END        
--ELSE BEGIN        
-- SET @Result = 'alreadyTrialExists'         
-- SET @Message = 'Your trial is active or expired... please renew the subscription to continue the benefits.'        
--END        
        
        
END TRY        
BEGIN CATCH        
    -- Statements to handle the error        
             
        set @Result= 'failedPleaseTryAgain'        
  set @Message = ERROR_MESSAGE()          
  INSERT INTO ExceptionLogs (ExceptionType, ErrorMessage, StackTrace , Description , Notes , CreatedDate)        
  VALUES ('ActivateFreeTrialM',ERROR_MESSAGE(), ERROR_SEVERITY() ,  ERROR_STATE() ,   ERROR_LINE()  , GETDATE())        
  rollback;        
        
END CATCH;        
        
         
select  @Result as Result , @Message as Message        
        
END        
        
GO 


---------------------------------------------------------------------------------------------------------------------------------------------------------------------


GO 
ALTER PROCEDURE [dbo].[GetCompaniesM] @LoggedInUser UNIQUEIDENTIFIER        
 ,@BasketId INT        
 ,@SearchText VARCHAR(100)        
 ,@PrimaryKey VARCHAR(100)        
 ,@SecondaryKey VARCHAR(100)        
 ,@UserHasResearch BIT = NULL OUTPUT        
AS        
BEGIN        
      
declare @userHasPurchasedResearch bit = 0      
       
 IF EXISTS (      
 SELECT 1       
FROM MyBucketM      
WHERE MobileUserKey = @LoggedInUser      
  AND ProductId = (SELECT Id FROM ProductsM WHERE Code = 'research')      
  )      
  BEGIN      
   SET @userHasPurchasedResearch = 1      
  END      
         
 IF EXISTS (        
   SELECT 1        
   FROM MyBucketM b        
   JOIN ProductsM p ON b.ProductId = p.Id        
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
      
       
      
      
        
 SELECT BasketId        
  ,p.Id AS ProductId        
  ,p.Name AS Productname        
  ,pc.GroupName        
  ,p.Price        
  ,case when IsFree =1 then 'Read'      
  when @userHasPurchasedResearch = 1 and @UserHasResearch = 1 then 'Read'        
  when @userHasPurchasedResearch = 0 and @UserHasResearch = 0 then 'Unlock'      
   when @userHasPurchasedResearch = 1 and @UserHasResearch = 0 then 'Renew' end as CompanyStatus      
  ,CASE         
   WHEN (@UserHasResearch = 1)        
    OR (isFree = 1)        
    THEN 'Buy'        
   ELSE 'Purchased'        
   END AS BuyButtonText        
  ,p.Description AS ProductDescription        
  ,(        
   SELECT ISNULL((CAST(AVG(ISNULL(Rating, 0)) AS DECIMAL(18, 2))), 0)        
   FROM ProductsRatingM        
   WHERE ProductId = p.id        
   ) AS OverallRating        
  ,(        
   SELECT COUNT(1)        
   FROM ProductLikesM        
   WHERE LikeId = 1        
    AND ProductId = P.ID        
    AND IsDelete = 0        
   ) AS HeartsCount        
  ,ISNULL(prm.Rating, 0.0) AS UserRating        
  ,CAST((        
    (        
     CASE         
      WHEN ISNULL(plm.LikeId, 0) = 1        
       AND plm.IsDelete = 0        
       THEN 1        
      ELSE 0        
      END        
     )        
    ) AS BIT) AS UserHasHeart        
  ,CAST((        
    CASE         
     WHEN ISNULL(MB.Id, 0) > 0        
      THEN 1        
     ELSE 0        
     END        
    ) AS BIT) AS IsInMyBucket        
  ,CAST((        
    (        
     CASE         
      WHEN MB.Id IS NOT NULL        
       AND MB.MobileUserKey = @LoggedInUser        
       AND (        
        GETDATE() BETWEEN MB.startDate        
         AND MB.Enddate        
        )        
       THEN 1        
      ELSE 0        
      END        
     )        
    ) AS BIT) AS IsInValidity        
  ,p.ListImage        
  ,IsFree        
  ,CASE         
   WHEN (@UserHasResearch = 1)        
    OR (isFree = 1)        
    THEN c.name        
   ELSE 'Unlock the name'        
   END AS Name        
  ,ChartImageUrl        
  ,OtherImage        
  ,WebsiteUrl        
  ,c.CreatedOn        
  ,MarketCap        
       
  ,CASE         
   WHEN (@UserHasResearch = 1)        
    OR (isFree = 1)        
    THEN ShortSummary        
   ELSE TrialDescription        
   END AS ShortSummary        
  ,pe     
  ,c.PublishDate  
 FROM CompanyDetailM c        
 INNER JOIN ProductsM p ON p.Id = (        
   SELECT id        
   FROM ProductsM        
   WHERE Code = 'RESEARCH'        
   )        
 LEFT JOIN Categories pc ON pc.Id = p.CategoryID        
  LEFT JOIN ProductsRatingM AS prm ON p.Id = prm.productId and prm.CreatedBy = @LoggedInUser               
   LEFT JOIN ProductLikesM AS plm ON plm.ProductId = p.Id and plm.CreatedBy = @LoggedInUser    AND plm.LikeId = 1 AND plm.IsDelete = 0            
    LEFT JOIN MYBucketM AS MB ON mb.ProductId = p.Id and mb.MobileUserKey = @LoggedInUser and mb.IsExpired = 0                    
        
        
        
 WHERE BasketId = @BasketId        
  AND IsPublished = 1        
  AND (        
   @SearchText IS NULL        
   OR @SearchText = ''        
   OR c.Name LIKE '%' + @SearchText + '%'        
   )        
  AND (        
   (        
    @PrimaryKey IS NULL          OR @PrimaryKey = ''        
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
 ORDER BY isfree DESC        
  ,c.Createdon DESC        
END
GO

---------------------------------------------------------------------------------------------------------------------------------------------------------------------


GO 
ALTER PROCEDURE [dbo].[GetBasketsM]        
AS        
BEGIN        
        
;with CompanyCount as (        
select basketId,COUNT(basketId) as CompanyCount from CompanyDetailM where IsPublished = 1  group by BasketId       
)select b.Id,b.Title,b.Description,b.IsFree,b.IsActive,b.IsDelete,cc.CompanyCount from BasketsM b inner join CompanyCount cc on b.Id = cc.BasketId   
  WHERE  b.IsActive = 1 and  b.IsDelete = 0  
           
END 
GO

---------------------------------------------------------------------------------------------------------------------------------------------------------------------

GO
--exec GetSubscriptionPlanWithProduct NULL, 1 , '2DC32A9F-2F35-EE11-811E-00155D23D79C' , 'IOS'
CREATE PROCEDURE GetSubscriptionPlanWithProduct   
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
    DATEADD(day, sd.Months, GETDATE()) as ExpireOn,  
    -- sm.IsActive AS SubscriptionMappingActive,  
    -- sd.Id AS SubscriptionDurationId,  
      CAST(sd.Months as varchar) + ', months' AS SubscriptionDurationName,  
    sd.Months,  
	CAST(0 as bit) IsRecommended,
	CAST(FORMAT(((pm.Price - ((pm.Price * sm.DiscountPercentage)/ 100))/sd.Months), 'N2') AS VARCHAR) + '/m' as PerMonth,
	sm.Id AS SubscriptionMappingId
FROM   
    SubscriptionMapping sm  
INNER JOIN   
    SubscriptionDuration sd ON sm.SubscriptionDurationId = sd.Id  
INNER JOIN   
    SubscriptionPlan sp ON sm.SubscriptionPlanId = sp.Id  
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


---------------------------------------------------------------------------------------------------------------------------------------------------------------------


GO 
-- exec [GetProductById]  10 , 'A04D2F94-1377-EF11-8187-00155D23D79C'   
ALTER PROCEDURE [dbo].[GetProductById]  
    @ProductId INT,  
    @MobileUserKey UNIQUEIDENTIFIER  
AS  
BEGIN  
    --declare @ProductId int = 5                          
    --,@MobileUserKey UNIQUEIDENTIFIER = '294A46B7-E918-EF11-B260-A630D7B5F938'                        
  
    DECLARE @priorDaysInfo INT = CAST(  
                                 (  
                                     SELECT TOP 1 value FROM settings WHERE code = 'PRIORDAYSINFO'  
                                 ) AS INT)  
    DECLARE @ContentCount int = 0,  
            @VideoContent int = 0  
    SELECT @ContentCount = COUNT(Id),  
           @VideoContent = count(   CASE  
                                        WHEN attachmentType = 'video' THEN  
                                            1  
                                        ELSE  
                                            NULL  
                                    END  
                                )  
    FROM PRODUCTSCONTENTM  
    WHERE ProductId = @ProductId  
  
    DROP TABLE IF EXISTS #tempBenefits  
  
  
    SELECT ben.Id,  
           ben.GiftProductId,  
           sub.Name as Names,  
           ben.Months,  
           pro.Name,  
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
  
  
  
    DECLARE @CurrentDate date = cast(getdate() AS date)  
    DECLARE @IsOutOfSubscription VARCHAR(300) = (  
                                                    SELECT TOP 1  
                                                        mobileUserKey  
                                                    FROM MYBucketM b  
                                                    WHERE productId = @ProductId  
                                                          AND mobileUserKey = @MobileUserKey  
                                                          AND ISNULL(IsACtive, 1) = 1  
                                                          AND isnull(IsExpired, 0) = 0  
                                                         AND @currentDate >= cast(b.StartDate as date)  
                                                          AND @currentDate <= cast(b.endDate as date)  
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
  
    SELECT top 1  
        p.id,  
        p.name,  
        p.Description,  
        p.DescriptionTitle,  
        p.CategoryID,  
        pcm.Name as Category,  
        --CAST(ISNULL(pom.NetAmount,p.Price) as decimal(16,4)) as Price,                          
        CAST(p.Price as decimal(16, 4)) as Price,  
        cast(pom.CouponKey as varchar(200)) AS CouponCode,  
        isnull(pom.PaidAmount, 0.0) as PaidAmount,  
        CAST(ISNULL(pom.CouponDiscountAmount, 0.0) as VARCHAR) AS Discount, 
        CAST(ISNULL(PR.Rating, 0) AS varchar) AS UserRating,  
        '' AS Liked,  
        '' AS EnableSubscription,  
        (s.name) AS SubscriptionData,  
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
        ( --buy button text                                
        CASE  
            WHEN mb.id IS NULL THEN  
                'Buy'  
            ELSE  
                CASE  
                    WHEN cast(CASE  
                                  WHEN DATEDIFF(day, GETDATE(), mb.enddate) < @priorDaysInfo THEN  
                                      1  
                                  ELSE  
                                      0  
                              END AS BIT) = 1 THEN  
                        'Renew'  
                    ELSE  
                        'Purchased'  
                END  
        END  
        ) AS BuyButtonText,  
        @ContentCount AS ContentCount,  
        @VideoContent AS VideoCount,  
        DATEDIFF(day, getdate(), ISNULL(mb.enddate, GETDATE())) as DaysToGo  
    FROM ProductsM as P  
        INNER JOIN ProductCategoriesM as pcm  
            on p.CategoryID = pcm.Id  
        LEFT JOIN PurchaseOrdersM AS POM  
            on POM.ProductId = p.Id  
               and pom.ProductId = @ProductId  
               and pom.ActionBy = @MobileUserKey  
        LEFT JOIN ProductsRatingM AS PR  
            ON PR.ProductId = P.Id  
               AND PR.CreatedBy = @MobileUserKey  
        LEFT JOIN ProductLikesM AS pl  
            on pl.ProductId = p.Id  
               AND pl.LikeId = 1  
               AND pl.CreatedBy = @MobileUserKey  
               AND pl.IsDelete = 0  
        LEFT JOIN subscriptiondurationm s  
            ON s.Id = p.SubscriptionId  
        LEFT JOIN MYBucketM as Mb  
            on p.id = mb.ProductId  
               and mb.mobileuserkey = @MobileUserkey  
    WHERE p.id = @ProductId  
    order by POM.CreatedOn desc  
  
END 
GO



---------------------------------------------------------------------------------------------------------------------------------------------------------------------
GO 

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
                    ON MU.mobile = LTRIM(RTRIM(ss.value))          
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

GO 

---------------------------------------------------------------------------------------------------------------------------------------------------------------------
GO 

alter PROCEDURE GetPartnerDematAccounts    
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

---------------------------------------------------------------------------------------------------------------------------------------------------------------------
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

---------------------------------------------------------------------------------------------------------------------------------------------------------------------
GO
alter PROCEDURE GetCoupons        
    @PageSize INT = 100,            
    @PageNumber INT = 1,            
    @FromDate DATE = NULL,            
    @ToDate DATE = NULL,        
    @Status INT = 0,        
    @SearchText NVARCHAR(100) = NULL,      
    @TotalCount INT OUTPUT        
AS        
BEGIN 
    SET @SearchText = LTRIM(RTRIM(@SearchText));  
    DECLARE @StartRow INT = (@PageNumber - 1) * @PageSize;        
        
    WITH CouponData AS (      
        SELECT       
            c.Id AS CouponId,      
            c.PublicKey,      
            c.Name,      
            c.Description,      
            c.DiscountInPercentage,      
            c.DiscountInPrice,      
            c.TotalRedeems,      
            c.RedeemLimit,      
            c.ProductValidityInDays,      
            c.IsActive,      
            c.IsDelete,      
            c.CreatedOn,      
   c.modifiedOn,      
            STUFF((      
                SELECT ',' + CAST(cp.ProductID AS VARCHAR(10))      
                FROM CouponProductMappingM cp      
                WHERE cp.CouponID = c.Id      
                FOR XML PATH('')      
            ), 1, 1, '') AS ProductIds,      
            STUFF((      
                SELECT ',' + mu.Mobile      
                FROM CouponUserMappingM cum      
                JOIN MobileUsers mu ON cum.MobileUserKey = mu.PublicKey      
                WHERE cum.CouponId = c.Id      
                FOR XML PATH('')      
            ), 1, 1, '') AS MobileNumbers      
        FROM CouponsM c      
        WHERE         
            ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, c.CreatedOn)), ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999))        
            BETWEEN ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999)        
            AND ISNULL(@ToDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))        
            AND(@Status = -1    OR (@Status = 1 AND c.IsActive = 1 AND c.IsDelete = 0)     OR (@Status = 0 AND c.IsActive = 0 AND c.IsDelete = 0)     OR (@Status = 2 AND c.IsDelete = 1))            
    AND (@SearchText IS NULL OR c.Name LIKE '%' + @SearchText + '%' OR c.Description LIKE '%' + @SearchText + '%'))
  SELECT *      
  FROM CouponData      
  ORDER BY       
        CASE WHEN IsDelete = 0 THEN 1 ELSE 0 END DESC,      
        IsActive DESC,      
            ISNULL(ModifiedOn,CreatedOn) DESC          
    OFFSET @StartRow ROWS FETCH NEXT @PageSize ROWS ONLY;        
        
    SELECT @TotalCount = COUNT(1)        
    FROM CouponsM c      
    WHERE         
        ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, c.CreatedOn)), ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999))        
        BETWEEN ISNULL(@FromDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999)        
        AND ISNULL(@ToDate, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))        
         AND(@Status = -1    OR (@Status = 1 AND c.IsActive = 1 AND c.IsDelete = 0)     OR (@Status = 0 AND c.IsActive = 0 AND c.IsDelete = 0)     OR (@Status = 2 AND c.IsDelete = 1))     
        AND (@SearchText IS NULL OR c.Name LIKE '%' + @SearchText + '%' OR c.Description LIKE '%' + @SearchText + '%');      
END 
GO
---------------------------------------------------------------------------------------------------------------------------------------------------------------------
GO
alter PROCEDURE [dbo].[GetAdImagesm] @type VARCHAR(50)  
 ,@searchText VARCHAR(100)  
AS  
BEGIN  
 SELECT id  
  ,name  
  ,IsActive  
  ,IsDelete  
  ,Url  
  ,Type 
  ,ExpireOn
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
GO
---------------------------------------------------------------------------------------------------------------------------------------------------------------------