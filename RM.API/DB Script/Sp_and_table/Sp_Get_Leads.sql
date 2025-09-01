ALTER PROCEDURE [dbo].[Sp_Get_Leads]
(
--declare
	@IsPaging int =0,
	@PageSize int =5,
	@PageNumber int =1,
	@SortOrder varchar(50),
	@SortExpression varchar(50),
	@FromDate varchar(50) =null,
	@ToDate varchar(50) =null,
	@PrimaryKey varchar(50) = null,
	@SecondaryKey varchar(50) = null, -- Services Dropdown Filter 
	@ThirdKey varchar(50) =null,
	@FourthKey varchar(50) =null, -- lead Source
	@FifthKey varchar(50) =null,
	@CreatedBy varchar(50) =null,
	@AssignedTo varchar(50) = null,
	@LoggedInUser varchar(50) = null ,
	@RoleKey varchar(50) = '',
	@SearchText varchar(250) =  null,
	@TotalCount INT = 0 OUTPUT
)
AS
BEGIN	

	set @SecondaryKey  = iif(@SecondaryKey = '0' , null, @SecondaryKey)
	set @SecondaryKey  = iif(@SecondaryKey = '' , null, @SecondaryKey)
	
	
	set @FourthKey = iif(@FourthKey= '0' , null, @FourthKey)
	set @FourthKey = iif(@FourthKey= '' , null, @FourthKey)
	
	
	declare @StartIndex int =0
	declare @EndIndex int =0
	
	set @StartIndex = (@PageNumber-1) * @PageSize 
	set @EndIndex = @StartIndex + @PageSize 

	SET @FromDate  = ISNULL(CAST(@FromDate AS DATE), dateadd(day,-30, GetDate()))
	SET @ToDate  = ISNULL(CAST(@ToDate AS DATE),   GetDate())
 	
	select @TotalCount =Count(*) From Leads as Leads
	LEFT JOIN Services as Services on Leads.ServiceKey = Services.PublicKey
	LEFT JOIN LeadTypes as LeadTypes on LeadTypes.PublicKey = Leads.LeadTypeKey
	LEFT JOIN LeadSources as LeadSource on LeadSource.PublicKey = Leads.LeadSourceKey
	LEFT JOIN Users AS Users on Users.PublicKey = Leads.CreatedBy
	LEFT JOIN Status as st on st.id = Leads.StatusId
	Left join PurchaseOrders as po on po.LeadId = Leads.Id
	LEFT JOIN Status as st1 on st1.id = po.Status
	WHERE
	(
		Leads.MobileNumber LIKE '%' + @SearchText +'%' OR
		@SearchText IS NULL OR
		Leads.EmailId LIKE '%'+ @SearchText +'%' OR 
		Leads.FullName LIKE '%'+ @SearchText +'%'
	) 
	AND ISNULL(po.Status,0) != (SELECT ID FROM Status where code = 'cus')  
	AND (Leads.ServiceKey = ISNULL(@SecondaryKey,Leads.ServiceKey) OR Coalesce(@SecondaryKey,'') = '')
	AND (Leads.LeadSourceKey = ISNULL(@FourthKey , Leads.LeadSourceKey) OR Coalesce(@FourthKey,'') = '')
	AND (Leads.LeadTypeKey = ISNULL(@ThirdKey, Leads.LeadTypeKey) OR Coalesce(@ThirdKey,'') = '')
	AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN cast(@FromDate as date) AND cast(@ToDate as date))
	AND leads.AssignedTo = @LoggedInUser

	DROP TABLE IF EXISTS #tempLeads

	SELECT 
		Leads.[Id],	Leads.[FullName],Leads.[MobileNumber],Leads.[AlternateMobileNumber],Leads.[EmailId]
		, ISNULL(po.City, ISNULL(Leads.City,'')) as City , po.PaymentDate,
		ISNULL(Leads.Favourite, 0) as Favourite ,Services.Name as  [ServiceKey]
		, Services.Id as ServiceId,
		LeadTypes.Name as [LeadTypeKey], LeadTypes.Id as LeadTypesId,LeadSource.Id as LeadSourcesId,
		LeadSource.Name as [LeadSourceKey], isnull(po.[Remark], '' ) as Remark 
		,Leads.[IsSpam],Leads.[IsWon],Leads.[IsDisabled],Leads.[IsDelete]
		,Leads.[PublicKey],Leads.[CreatedOn], 
		Users.FirstName as  [CreatedBy],Leads.[ModifiedOn],
		Leads.[ModifiedBy], 
		'' as [AssignedTo], ISNULL(Leads.StatusId , (select id from status where Code = 'fresh')) as [StatusId] , isnull(st.Name , 'New') as StatusName
		,ISNULL(Leads.PurchaseOrderKey,CAST(CAST(0 AS BINARY) AS UNIQUEIDENTIFIER)) AS PurchaseOrderKey ,
		ISNULL(st1.Name,'New') as PurchaseOrderStatus  , isnull(po.ModeOfPayment,-1) as ModeOfPayment ,
		ISNULL(po.PaidAmount, 0.0) as PaidAmount , 	isnull(po.NetAmount, 0.0) as  NetAmount,
		ISNULL(po.TransactionRecipt ,'')AS TransactionRecipt, 
		ISNULL(po.TransasctionReference,'')  AS TransasctionReference ,
		(DATEDIFF(DAY,    GETDATE(), DATEADD(day, 15, ISNULL(leads.ModifiedOn,GETDATE())))) AS DaysToGo 
	INTO #tempLeads	
	From Leads as Leads
	LEFT JOIN Services as Services on Leads.ServiceKey = Services.PublicKey
	LEFT JOIN LeadTypes as LeadTypes on LeadTypes.PublicKey = Leads.LeadTypeKey
	LEFT JOIN LeadSources as LeadSource on LeadSource.PublicKey = Leads.LeadSourceKey
	LEFT JOIN Users AS Users on Users.PublicKey = Leads.CreatedBy
	LEFT JOIN Status as st on st.id = Leads.StatusId
	Left join PurchaseOrders as po on po.LeadId = Leads.Id 
	LEFT JOIN Status as st1 on st1.id = po.Status
	LEFT JOIN PaymentModes as pm on po.ModeOfPayment = pm.Id

	WHERE
	(
		Leads.MobileNumber LIKE '%' + @SearchText +'%' OR
		@SearchText IS NULL OR
		Leads.EmailId LIKE '%'+ @SearchText +'%' OR 
		Leads.FullName LIKE '%'+ @SearchText +'%'
	) 
	--AND (Leads.MobileNumber = ISNULL(@SearchText, Leads.MobileNumber) OR Coalesce(@SearchText,'') = '')
	--AND ISNULL(Leads.StatusId,0) != 24
	AND ISNULL(po.Status,0) != 24  
	AND (Leads.ServiceKey = ISNULL(@SecondaryKey,Leads.ServiceKey) OR Coalesce(@SecondaryKey,'') = '')
	AND (Leads.LeadSourceKey = ISNULL(@FourthKey , Leads.LeadSourceKey) OR Coalesce(@FourthKey,'') = '')
	AND (Leads.LeadTypeKey = ISNULL(@ThirdKey, Leads.LeadTypeKey) OR Coalesce(@ThirdKey,'') = '')
	--ANd (Leads.CreatedOn between cast(@FromDate as smalldatetime) AND cast(@ToDate as smalldatetime))
	AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN cast(@FromDate as date) AND cast(@ToDate as date))
	AND leads.AssignedTo = @LoggedInUser
   ORDER BY 
   Favourite DESC, 
   CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'ASC' THEN Leads.CreatedOn END ASC,
   CASE WHEN @SortExpression = 'CreatedOn' AND @SortOrder = 'DESC' THEN Leads.CreatedOn END DESC,
   CASE WHEN @SortExpression = 'Favourite' AND @SortOrder = 'ASC' THEN Leads.Favourite END ASC,
   CASE WHEN @SortExpression = 'Favourite' AND @SortOrder = 'DESC' THEN Leads.Favourite END DESC,
   CASE WHEN @SortExpression = 'Name' AND @SortOrder = 'ASC' THEN Leads.FullName END ASC,
   CASE WHEN @SortExpression = 'Name' AND @SortOrder = 'DESC' THEN Leads.FullName END DESC
	
   OFFSET (IIF(@PageNumber <= 1 , 0 ,(@PageNumber-1))) * @PageSize ROWS
   FETCH NEXT @PageSize ROWS ONLY


   SELECT (SELECT * FROM #tempLeads	FOR JSON AUTO) as JsonData

END
 