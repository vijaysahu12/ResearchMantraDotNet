CREATE PROCEDURE [dbo].[GetPartnerAccounts]
	@IsPaging bit = 0, 
	@PageSize int =100, 
	@PageNumber int = 1, 
	@FromDate date = null, 
	@ToDate date = null, 
	@SearchText varchar(200) = null,   
	@SortExpression varchar(100) = '', 
	@PartnerWith varchar(100) = null, 
	@StatusType  int  = null,
	@AssignedTo int = 0,
	@product_count INT OUTPUT

AS 
BEGIN 


--declare 
--@IsPaging bit = 0, 
--	@PageSize int =100, 
--	@PageNumber int = 1, 
--	@FromDate date = null, 
--	@ToDate date = null, 
--	@SearchText varchar(200) = null,   
--	@SortExpression varchar(100) = '', 
--	@PartnerWith varchar(100) = null, 
--	@StatusType  int  = null,
--	@AssignedTo int = 0,
--	@product_count int = 0



SET @SearchText = case when @SearchText = '' then null else @SearchText  END; 
SET @PartnerWith = case when @PartnerWith = '' then null else @PartnerWith  END; 


	SELECT  @product_count = COUNT(0)
	FROM PartnerAccounts
	WHERE 
	[Status] =     ( CASE WHEN ISNULL(@StatusType , -1) = -1 then [Status] else  @StatusType end ) AND 
	 isnull([AssignedTo],0) =  IIF(@AssignedTo = 0, ISNULL([AssignedTo],0) , @AssignedTo ) 
 AND
	(
		FullName LIKE '%' +    ISNULL(@SearchText, FullName) +'%' OR 
		ZerodhaCId LIKE '%'+   ISNULL(@SearchText, ZerodhaCId) +'%' OR 
		AngelCId like '%'+     ISNULL(@SearchText, AngelCId) +'%' OR 
		AliceBlueCId like '%'+ ISNULL(@SearchText, AliceBlueCId) +'%' OR 
		EdelweissCId like '%'+ ISNULL(@SearchText, EdelweissCId) +'%' OR 
		FyersCId like '%'+     ISNULL(@SearchText, FyersCId) +'%' OR 
		MobileNumber like '%'+ ISNULL(@SearchText, MobileNumber) +'%' OR 
		EmailId like '%'+      ISNULL(@SearchText, EmailId) +'%'
		)
	AND 
	 (
		ISNULL(@PartnerWith, '') = '' or 
		(
			(lower(@PartnerWith) =  'zerodha' AND isnull(ZerodhaCId, '') != '' ) OR
			(lower(@PartnerWith) =  'alice' AND isnull(AliceBlueCId, '') != '' ) OR 
			(lower(@PartnerWith) =  'angel' AND isnull(AngelCId, '') != '' ) OR
			(lower(@PartnerWith) =  'edelweiss' AND isnull(EdelweissCId, '') != '' ) OR
			(lower(@PartnerWith) =  'fyers' AND isnull(FyersCId, '') != '')    OR  
			(lower(@PartnerWith) =  'motilal' AND isnull(MotilalCId, '') != '')     OR
			(lower(@PartnerWith) =  'dhan' AND isnull(DhanCId, '') != '')    
		)
	)

	AND ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, ModifiedOn)), isnull(@FromDate , DATEADD(dd, 0, DATEDIFF(dd, 0, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))) - 9999 ) ) 
	BETWEEN isnull(DATEADD(dd, 0, DATEDIFF(dd, 0, @FromDate )), DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999 ) 
	AND ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, @ToDate)),DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))

	 
SELECT 
		PA.Id,
		FullName, 
		PA.MobileNumber, 
		PA.EmailId, 
		(
		CASE WHEN  [Status] = 0 then 'Fresh'
			 WHEN  [Status] = 1 then 'Pending'
			 WHEN  [Status] = 2 then 'Accepted'
			 WHEN  [Status] = 3 then 'Rejected'
			 WHEN  [Status] = 4 then 'Followup'
		ELSE 'Fresh'
		END 
		) as StatusType,
		CAST(Status AS VARCHAR) AS Status , 
		(
			  IIF(ISNULL(ZerodhaCId,'') != '' , 'Zerodha: ', ' ') + ISNULL(UPPER(ZerodhaCId),' ') 
			+ IIF(ISNULL(AngelCId,'') != '' , ' | Angel: ', ' ') + isnull(UPPER(AngelCId),' ') 
			+ IIF(ISNULL(AliceBlueCId,'') != ''  , ' | AliceBlueCId: ' , ' ') + isnull(UPPER(AliceBlueCId),' ') 
			+ IIF(ISNULL(EdelweissCId,'') != '' , ' | EdelweissCid: ' , ' ') + ISNULL(UPPER(EdelweissCid),' ') 
			+ IIF(ISNULL(FyersCId,'') != '' , ' | FyersCId: ' , ' ') + isnull(UPPER(FyersCId),' ') 
			+ IIF(ISNULL(MotilalCId,'') != '' , ' | MotilalCId: ' , ' ') + isnull(UPPER(MotilalCId),' ') 
			+ IIF(ISNULL(DhanCId,'') != '' , ' | DhanCId: ' , ' ') + isnull(UPPER(DhanCId),' ') 
		) as PartnerWith , 
		'' as ClientId , 
		'' as  Details , 
		Remarks,  
		City, 
		PA.CreatedOn, 
		PA.ModifiedOn, 
		TelegramId, 
		ZerodhaCId, 
		AngelCId, 
		AliceBlueCId ,
		EdelweissCId,
		FyersCId,
		MotilalCId,
		DhanCId,
		isnull(Brokerage,0.0) as Brokerage,
		Source,
		ISNULL(AssignedTo,0) as AssignedTo,
		(US.FirstName + ' ' + US.LastName ) AS AssignedToName
	
	FROM PartnerAccounts PA 
	LEFT JOIN Users AS US ON PA.AssignedTo = US.Id
	WHERE 
	[Status] =		( CASE WHEN ISNULL(@StatusType , -1) = -1 THEN [Status] ELSE  @StatusType END )  
	AND isnull([AssignedTo],0) =  IIF(@AssignedTo = 0, ISNULL([AssignedTo],0) , @AssignedTo ) 
	AND 
	(
		FullName LIKE '%' +    ISNULL(@SearchText, FullName) +'%' OR 
		ZerodhaCId LIKE '%'+   ISNULL(@SearchText, ZerodhaCId) +'%' OR 
		AngelCId like '%'+     ISNULL(@SearchText, AngelCId) +'%' OR 
		AliceBlueCId like '%'+ ISNULL(@SearchText, AliceBlueCId) +'%' OR 
		EdelweissCId like '%'+ ISNULL(@SearchText, EdelweissCId) +'%' OR 
		FyersCId like '%'+     ISNULL(@SearchText, FyersCId) +'%' OR 
		MotilalCId like '%'+     ISNULL(@SearchText, MotilalCId) +'%' OR 
		DhanCId like '%'+     ISNULL(@SearchText, DhanCId) +'%' OR 
		PA.MobileNumber like '%'+ ISNULL(@SearchText, PA.MobileNumber) +'%' OR 
		PA.EmailId like '%'+      ISNULL(@SearchText, PA.EmailId) +'%'
		)
	AND 
	 (
		ISNULL(@PartnerWith, '') = '' or 
		(
			(lower(@PartnerWith) =  'zerodha' AND isnull(ZerodhaCId, '') != '' ) OR
			(lower(@PartnerWith) =  'alice' AND isnull(AliceBlueCId, '') != '' ) OR 
			(lower(@PartnerWith) =  'angel' AND isnull(AngelCId, '') != '' ) OR
			(lower(@PartnerWith) =  'edelweiss' AND isnull(EdelweissCId, '') != '' ) OR
			(lower(@PartnerWith) =  'fyers' AND isnull(FyersCId, '') != '')      OR
			(lower(@PartnerWith) =  'motilal' AND isnull(MotilalCId, '') != '')  OR
			(lower(@PartnerWith) =  'dhan' AND isnull(DhanCId, '') != '')     
		)
	)

	AND ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, PA.ModifiedOn)), isnull(@FromDate , DATEADD(dd, 0, DATEDIFF(dd, 0, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))) - 9999 ) ) 
	BETWEEN isnull(DATEADD(dd, 0, DATEDIFF(dd, 0, @FromDate )), DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999 ) 
	AND ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, @ToDate)),DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))
	ORDER BY Id DESC
	OFFSET (IIF(@PageNumber <= 1 , 0 ,(@PageNumber-1))) * @PageSize ROWS
	FETCH NEXT @PageSize ROWS ONLY
END  
