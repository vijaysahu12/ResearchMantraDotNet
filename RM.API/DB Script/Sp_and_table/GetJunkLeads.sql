CREATE PROCEDURE [dbo].[GetJunkLeads]
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
	LEFT JOIN Users AS Users on Users.PublicKey = Leads.CreatedBy
	LEFT JOIN Status as st on st.id = Leads.StatusId
	Left join PurchaseOrders as po on po.LeadId = Leads.Id
	LEFT JOIN Status as st1 on st1.id = po.Status
	where (
		Leads.MobileNumber LIKE '%' + @SearchText +'%' OR @SearchText IS NULL OR
		--Leads.EmailId LIKE '%'+ @SearchText +'%' OR 
		Leads.FullName LIKE '%'+ @SearchText +'%'
	) 
	AND 
		(	@FifthKey = 'assigned'   AND leads.AssignedTo IS NOT NULL	  OR  
	 		@FifthKey = 'unassigned' AND leads.AssignedTo IS NULL OR 
			ISNULL(@FifthKey,'') = ''
		) 

	AND ISNULL(Leads.ServiceKey , '') = ISNULL( @SecondaryKey , ISNULL(Leads.ServiceKey , '')) --OR Coalesce(@SecondaryKey,'') = '')
	AND Leads.LeadSourceKey = ISNULL( @FourthKey , Leads.LeadSourceKey)  --OR Coalesce(@FourthKey,'') = '')
	AND Leads.LeadTypeKey =ISNULL(  @ThirdKey , Leads.LeadTypeKey ) --OR Coalesce(@ThirdKey,'') = '')
	AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN cast( isnull(@FromDate , leads.ModifiedOn) as DATE ) AND 
	cast(isnull(@ToDate , leads.ModifiedOn) as date))


	 
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
	userAssignedTo.FirstName as [AssignedTo], 
	Leads.StatusId as [StatusId] ,
	st.Name as StatusName
	,Leads.PurchaseOrderKey 
		INTO #tempCustomer
	From Leads as Leads
	LEFT JOIN Services as Services on Leads.ServiceKey = Services.PublicKey
	LEFT JOIN LeadTypes as LeadTypes on LeadTypes.PublicKey = Leads.LeadTypeKey
	LEFT JOIN LeadSources as LeadSource on LeadSource.PublicKey = Leads.LeadSourceKey
	LEFT JOIN Users AS Users on Users.PublicKey = Leads.CreatedBy
	LEFT JOIN Users as userAssignedTo on userAssignedTo.PublicKey = Leads.AssignedTo
	LEFT JOIN Status as st on st.id = Leads.StatusId
	Left join PurchaseOrders as po on po.LeadId = Leads.Id
	LEFT JOIN Status as st1 on st1.id = po.Status
		where (
		Leads.MobileNumber LIKE '%' + @SearchText +'%' OR @SearchText IS NULL OR
		--Leads.EmailId LIKE '%'+ @SearchText +'%' OR 
		Leads.FullName LIKE '%'+ @SearchText +'%'
	) 
	AND 
		(	@FifthKey = 'assigned'   AND leads.AssignedTo IS NOT NULL OR  
	 		@FifthKey = 'unassigned' AND leads.AssignedTo IS NULL OR 
			ISNULL(@FifthKey,'') = ''
		) 

	AND Leads.ServiceKey = ISNULL( @SecondaryKey ,Leads.ServiceKey ) --OR Coalesce(@SecondaryKey,'') = '')
	AND Leads.LeadSourceKey = ISNULL( @FourthKey , Leads.LeadSourceKey)  --OR Coalesce(@FourthKey,'') = '')
	AND Leads.LeadTypeKey =ISNULL(  @ThirdKey , Leads.LeadTypeKey ) --OR Coalesce(@ThirdKey,'') = '')
	AND (CAST(Leads.ModifiedOn AS DATE) BETWEEN cast( isnull(@FromDate , leads.ModifiedOn) as DATE ) AND 
	cast(isnull(@ToDate , leads.ModifiedOn) as date))

	ORDER BY Leads.ModifiedOn DESC
	 

	OFFSET (IIF(@PageNumber <= 1 , 0 ,(@PageNumber-1))) * @PageSize ROWS
	FETCH NEXT @PageSize ROWS ONLY
	SELECT (SELECT * FROM #tempCustomer	FOR JSON AUTO) as JsonData --, @TotalCount as Total
	--select * from #tempCustomer
END 
 