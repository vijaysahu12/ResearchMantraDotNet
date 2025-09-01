--exec [GetJunkLeads]

alter PROCEDURE [dbo].[GetJunkLeads]
(
--declare
	@IsPaging int =0,
	@PageSize int =25,
	@PageNumber int =1,
	@SortOrder varchar(50) = 'desc',
	@SortExpression varchar(50) = 'CreatedBy',
	@FromDate  DateTime= NULL,
	@ToDate  DateTime= NULL,
	@PrimaryKey varchar(50) = null,
	@SecondaryKey varchar(50) = null, -- Services Dropdown Filter 
	@ThirdKey varchar(50) =null,
	@FourthKey varchar(50) =null, -- lead Source
	@FifthKey varchar(50) =null,
	@CreatedBy varchar(50) =null,
	@AssignedTo varchar(50) = null,
	@SearchText varchar(100) =  null,
	@TotalCount INT output
)
AS
BEGIN	
	
	--set @IsPaging=0
	--set @PageSize=10 
	--set @PageNumber =1 
	--set @SortExpression = 'CreatedBy' 
	--set @SortOrder = 'desc' 
	--set @FromDate='2023-06-04 00:00:00' 
	--set @ToDate='2023-07-04 00:00:00' 
	--set @PrimaryKey=NULL 
	--set @SecondaryKey=NULL 
	--set @ThirdKey=NULL 
	--set @FourthKey=NULL 
	--set @FifthKey=NULL 
	--set @CreatedBy=NULL 
	--set @AssignedTo=NULL 
	--set @SearchText=NULL 


	declare @StartIndex int =0
	declare @EndIndex int =0
	DROP TABLE IF EXISTS #tempCustomer

	set @StartIndex = (@PageNumber-1) * @PageSize 
	set @EndIndex = @StartIndex + @PageSize 


	SELECT @TotalCount = count(1)
	From Leads as Leads
	LEFT JOIN Services as Services on Leads.ServiceKey = Services.PublicKey
	LEFT JOIN LeadTypes as LeadTypes on LeadTypes.PublicKey = Leads.LeadTypeKey
	LEFT JOIN LeadSources as LeadSource on LeadSource.PublicKey = Leads.LeadSourceKey
	LEFT JOIN Users AS Users on Users.PublicKey = Leads.CreatedBy
	LEFT JOIN Status as st on st.id = Leads.StatusId
	Left join PurchaseOrders as po on po.LeadId = Leads.Id
	LEFT JOIN Status as st1 on st1.id = po.Status
	WHERE
	(
		Leads.MobileNumber LIKE '%' + @SearchText +'%' OR @SearchText IS NULL OR
		Leads.EmailId LIKE '%'+ @SearchText +'%' OR 
		Leads.FullName LIKE '%'+ @SearchText +'%'
	) 
	AND leads.AssignedTo IS NULL
	AND @SecondaryKey IS NULL OR Leads.ServiceKey = @SecondaryKey  --OR Coalesce(@SecondaryKey,'') = '')
	AND @SecondaryKey IS NULL OR Leads.LeadSourceKey = @FourthKey  --OR Coalesce(@FourthKey,'') = '')
	AND @SecondaryKey IS NULL OR Leads.LeadTypeKey = @ThirdKey --OR Coalesce(@ThirdKey,'') = '')
	AND (CAST(Leads.CreatedOn AS DATE) BETWEEN cast(@FromDate as DATE ) AND cast(@ToDate as date))

	SELECT ROW_NUMBER() OVER (ORDER BY Leads.ModifiedOn DESC) AS RowNumber , Leads.[Id],	'' AS ROW_ID,Leads.[FullName],Leads.[MobileNumber],Leads.[EmailId],Services.Name as  [ServiceKey], 
	LeadTypes.Name as [LeadTypeKey],LeadSource.Name as [LeadSourceKey], Leads.[Remarks]
	,Leads.[IsSpam],Leads.[IsWon],Leads.[IsDisabled],Leads.[IsDelete],Leads.[PublicKey],Leads.[CreatedOn], Users.FirstName as  [CreatedBy],Leads.[ModifiedOn],Leads.[ModifiedBy], Leads.AssignedTo
	as [AssignedTo], Leads.StatusId as [StatusId] ,st.Name as StatusName
	,Leads.PurchaseOrderKey , 0 as Selected
	INTO #tempCustomer
	From Leads as Leads
	LEFT JOIN Services as Services on Leads.ServiceKey = Services.PublicKey
	LEFT JOIN LeadTypes as LeadTypes on LeadTypes.PublicKey = Leads.LeadTypeKey
	LEFT JOIN LeadSources as LeadSource on LeadSource.PublicKey = Leads.LeadSourceKey
	LEFT JOIN Users AS Users on Users.PublicKey = Leads.CreatedBy
	LEFT JOIN Status as st on st.id = Leads.StatusId
	Left join PurchaseOrders as po on po.LeadId = Leads.Id
	LEFT JOIN Status as st1 on st1.id = po.Status
	WHERE
	(
		Leads.MobileNumber LIKE '%' + @SearchText +'%' OR @SearchText IS NULL OR
		Leads.EmailId LIKE '%'+ @SearchText +'%' OR 
		Leads.FullName LIKE '%'+ @SearchText +'%'
	) 
	AND leads.AssignedTo IS NULL
	AND @SecondaryKey IS NULL OR Leads.ServiceKey = @SecondaryKey  --OR Coalesce(@SecondaryKey,'') = '')
	AND @SecondaryKey IS NULL OR Leads.LeadSourceKey = @FourthKey  --OR Coalesce(@FourthKey,'') = '')
	AND @SecondaryKey IS NULL OR Leads.LeadTypeKey = @ThirdKey --OR Coalesce(@ThirdKey,'') = '')
	AND (CAST(Leads.CreatedOn AS DATE) BETWEEN cast(@FromDate as DATE ) AND cast(@ToDate as date))
	ORDER BY Leads.ModifiedOn DESC
	 

	OFFSET (IIF(@PageNumber <= 1 , 0 ,(@PageNumber-1))) * @PageSize ROWS
	FETCH NEXT @PageSize ROWS ONLY
	SELECT (SELECT * FROM #tempCustomer	FOR JSON AUTO) as JsonData

	--select * from #tempCustomer
END 

