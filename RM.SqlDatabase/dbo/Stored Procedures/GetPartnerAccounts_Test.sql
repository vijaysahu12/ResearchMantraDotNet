--[GetPartnerAccounts_Test] 0,100,1,null,null,'7041782559','',null, 0

CREATE PROCEDURE [dbo].[GetPartnerAccounts_Test]
	@IsPaging bit = 0, 
	@PageSize int =100, 
	@PageNumber int = 1, 
	@FromDate date = null, 
	@ToDate date = null, 
	@SearchText varchar(200) = null,   
	@SortExpression varchar(100) = '', 
	@PartnerWith varchar(100) = null, 
	@StatusType  int  = null,
	@product_count INT OUTPUT

AS 
BEGIN 


SET @SearchText = case when @SearchText = '' then null else @SearchText  end; 
SET @PartnerWith = case when @PartnerWith = '' then null else @PartnerWith  end; 


	SELECT  @product_count = COUNT(0)
	FROM PartnerAccounts
	WHERE 
	[Status] = ( CASE WHEN ISNULL(@StatusType , -1) = -1 then [Status] else  @StatusType end )
 
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
			(lower(@PartnerWith) =  'fyers' AND isnull(FyersCId, '') != '')     
		)
	)

	AND ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, ModifiedOn)), isnull(@FromDate , DATEADD(dd, 0, DATEDIFF(dd, 0, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))) - 9999 ) ) 
	BETWEEN isnull(DATEADD(dd, 0, DATEDIFF(dd, 0, @FromDate )), DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999 ) 
	AND ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, @ToDate)),DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))

	 
SELECT 
		Id,
		FullName, 
		MobileNumber, 
		EmailId, 
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
		) as PartnerWith , 
		'' as ClientId , 
		'' as  Details , 
		Remarks,  
		City, 
		CreatedOn, 
		ModifiedOn, 
		TelegramId, 
		ZerodhaCId, 
		AngelCId, 
		AliceBlueCId ,
		EdelweissCId,
		FyersCId,
		'' as BrokerageAmount
	FROM PartnerAccounts
	WHERE 
	[Status] = ( CASE WHEN ISNULL(@StatusType , -1) = -1 THEN [Status] ELSE  @StatusType END )
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
			(lower(@PartnerWith) =  'fyers' AND isnull(FyersCId, '') != '')     
		)
	)

	AND ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, ModifiedOn)), isnull(@FromDate , DATEADD(dd, 0, DATEDIFF(dd, 0, DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))) - 9999 ) ) 
	BETWEEN isnull(DATEADD(dd, 0, DATEDIFF(dd, 0, @FromDate )), DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) - 9999 ) 
	AND ISNULL(DATEADD(dd, 0, DATEDIFF(dd, 0, @ToDate)),DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())))
	ORDER BY Id DESC
	OFFSET (IIF(@PageNumber <= 1 , 0 ,(@PageNumber-1))) * @PageSize ROWS
	FETCH NEXT @PageSize ROWS ONLY
END 



--DECLARE @PageNumber AS INT
--DECLARE @RowsOfPage AS INT
--        DECLARE @MaxTablePage  AS FLOAT 
--        SET @PageNumber=4
--        SET @RowsOfPage=10
--SELECT 
--		Id,
--		FullName, 
--		MobileNumber, 
--		EmailId, 
--		ModifiedOn, 
--		City, 
--		'' as PartnerWith , 
--		'' as ClientId , 
--		'' as  Details , 
--		Remarks,  
--	 CAST(	Status AS VARCHAR) AS Status , 
--		TelegramId, 
--		ZerodhaCId, 
--		AngelCId, 
--		AliceBlueCId 
--	FROM PartnerAccounts
--	order by Id desc
----	--OFFSET 5 ROWS FETCH NEXT 6 ROWS ONLY
--	OFFSET (@PageNumber-1)*@RowsOfPage ROWS
--	FETCH NEXT @RowsOfPage ROWS ONLY

 





