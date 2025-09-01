CREATE PROCEDURE [dbo].[Sp_Get_Leads_v]
(
	@IsPaging int =0,
	@PageSize int =0,
	@PageNumber int =30,
	@SortOrder varchar(50) ='DESC',
	@SortExpression varchar(50) ='Id',	
	@FromDate varchar(50) ='0',
	@ToDate varchar(50) ='0',		
	@Id INT =0,
	@PrimaryKey varchar(50) ='0',
	@SecondaryKey varchar(50) ='0',
	@ThirdKey varchar(50) ='0',
	@FourthKey varchar(50) ='0',
	@FifthKey varchar(50) ='0',
	@CreatedBy varchar(50) ='0',
	@AssignedTo varchar(50) ='0',
	@LoggedInUser varchar(50) ='0',
	@RoleKey varchar(50) ='0',
	@SearchText varchar(250) =''
)
AS
BEGIN	

If @Id is null set @Id = 0
If @SortOrder is null set @SortOrder = 'DESC'
If @SortExpression is null set @SortExpression = 'Id'
If @PrimaryKey is null set @PrimaryKey = '0'
If @SecondaryKey is null set @SecondaryKey = '0'
If @ThirdKey is null set @ThirdKey = '0'
If @FourthKey is null set @FourthKey = '0'
If @FifthKey is null set @FifthKey = '0'
If @CreatedBy is null set @CreatedBy = '0'
If @CreatedBy = '' set @CreatedBy = '0'
If @AssignedTo = '' set @AssignedTo = '0'
If @AssignedTo is null set @AssignedTo = '0'
If @SearchText is null set @SearchText = ''
If @FromDate is null set @FromDate = '2000-01-01'
If @ToDate is null set @ToDate = '2100-01-01'
If @FromDate = '' set @FromDate = '2000-01-01'
If @ToDate = '' set @ToDate = '2100-01-01'

	print '@PrimaryKey' + @PrimaryKey
			print @SecondaryKey
			print @ThirdKey
			print @FourthKey
			print '@@CreatedBy' + @CreatedBy
			print @AssignedTo
			print @SearchText
			print @FromDate
			print @ToDate
		

Update Leads set AssignedTo = CreatedBy where (AssignedTo is null)


	declare @StartIndex int =0
	declare @EndIndex int =0
	declare @RoleId int =0

	Select @RoleId = Id from Roles where PublicKey = @RoleKey
	
	set @StartIndex = (@PageSize-1) * @PageNumber 
	set @EndIndex = @StartIndex + @PageNumber 

	DECLARE @vTotalRecordCount INT  
	DECLARE @vSortExpression varchar (100)
	SET @vSortExpression = @SortExpression + ' ' + @SortOrder
	
	print @StartIndex
			print @EndIndex
			
	
	
	print ' Role Id  '
	print @RoleId
	
	if (@RoleId = 1)
	Begin 
	
	
----	SELECT a.[Id],a.[FullName],a.[MobileNumber],a.[EmailId],b.Name as  [ServiceKey], c.Name as [LeadTypeKey],d.Name as [LeadSourceKey],a.[Remarks],a.[IsStudent],a.[IsCustomer],a.[IsSpam],a.[IsWon],a.[IsDisabled],a.[IsDelete],a.[PublicKey],a.[CreatedOn], cu.FirstName as  [CreatedBy],a.[ModifiedOn],a.[ModifiedBy],au.FirstName as [AssignedTo] 
----	FROM Leads a , Services b, LeadTypes c, LeadSources d, Users cu, Users au
----	WHERE (
----	 (a.[ServiceKey] = CONVERT(VARCHAR(100), b.PublicKey))
----	AND  (a.[LeadTypeKey] = CONVERT(VARCHAR(100), c.PublicKey))
----	AND  (a.[LeadSourceKey] = CONVERT(VARCHAR(100), d.PublicKey))
----	AND  (a.[CreatedBy] = CONVERT(VARCHAR(100), cu.PublicKey))
----	AND  (a.[AssignedTo] = CONVERT(VARCHAR(100), au.PublicKey))
----	AND(@PrimaryKey ='0' OR CONVERT(VARCHAR(100), a.PublicKey)=@PrimaryKey) 
----	AND (@SecondaryKey ='0' or @SecondaryKey=[ServiceKey]) 
----	AND (@ThirdKey ='0' or @ThirdKey=[LeadTypeKey]) 
----	AND (@FourthKey ='0' or @FourthKey=[LeadSourceKey]) 
----	AND (@CreatedBy ='0' or @CreatedBy=a.[CreatedBy]) 
----	AND (@AssignedTo ='0' or @AssignedTo=a.[AssignedTo])
----	AND ((a.FullName like '%'+@SearchText+'%')or (a.MobileNumber like '%'+@SearchText+'%') or (a.EmailId like '%'+@SearchText+'%'))
----AND (a.CreatedOn >  Convert(Datetime,@FromDate)) 
----AND (a.CreatedOn <  Convert(Datetime,@ToDate)) 	
----	)    


SELECT @vTotalRecordCount=COUNT(ID)FROM Leads a  where (
	 (@PrimaryKey ='0' OR CONVERT(VARCHAR(100), a.PublicKey)=@PrimaryKey) 
	AND (@SecondaryKey ='0' or @SecondaryKey=[ServiceKey]) 
	AND (@ThirdKey ='0' or @ThirdKey=[LeadTypeKey]) 
	AND (@FourthKey ='0' or @FourthKey=[LeadSourceKey]) 
	AND (@CreatedBy ='0' or @CreatedBy=a.[CreatedBy]) 
	AND (@AssignedTo ='0' or @AssignedTo=a.[AssignedTo])
	AND	((a.FullName like '%'+@SearchText+'%')or (a.MobileNumber like '%'+@SearchText+'%') or (a.EmailId like '%'+@SearchText+'%'))
	AND (a.CreatedOn >  Convert(Datetime,@FromDate)) 
	AND (a.CreatedOn <  Convert(Datetime,@ToDate)) 
			)
			
	print @vTotalRecordCount
	
		SELECT *,@vTotalRecordCount as TotalRecordCount
		FROM (SELECT ROW_NUMBER() OVER (ORDER BY
			CASE WHEN @vSortExpression='Id DESC' THEN a.[Id] END DESC,
			CASE WHEN @vSortExpression='Id ASC' THEN a.[Id] END ASC
			) AS 
			ROW_ID,a.[Id],a.[FullName],a.[MobileNumber],a.[EmailId],b.Name as  [ServiceKey], c.Name as [LeadTypeKey],d.Name as [LeadSourceKey],a.[Remarks],a.[IsSpam],a.[IsWon],a.[IsDisabled],a.[IsDelete],a.[PublicKey],a.[CreatedOn], cu.FirstName as  [CreatedBy],a.[ModifiedOn],a.[ModifiedBy],au.FirstName as [AssignedTo] 
	FROM Leads a , Services b, LeadTypes c, LeadSources d, Users cu, Users au
		WHERE (
		 (a.[ServiceKey] = CONVERT(VARCHAR(100), b.PublicKey))
	AND  (a.[LeadTypeKey] = CONVERT(VARCHAR(100), c.PublicKey))
	AND  (a.[LeadSourceKey] = CONVERT(VARCHAR(100), d.PublicKey))
	AND  (a.[CreatedBy] = CONVERT(VARCHAR(100), cu.PublicKey))
	AND  (a.[AssignedTo] = CONVERT(VARCHAR(100), au.PublicKey))
	AND(@PrimaryKey ='0' OR CONVERT(VARCHAR(100), a.PublicKey)=@PrimaryKey) 
	AND (@SecondaryKey ='0' or @SecondaryKey=[ServiceKey]) 
	AND (@ThirdKey ='0' or @ThirdKey=[LeadTypeKey]) 
	AND (@FourthKey ='0' or @FourthKey=[LeadSourceKey]) 
	AND (@CreatedBy ='0' or @CreatedBy=a.[CreatedBy]) 
	AND (@AssignedTo ='0' or @AssignedTo=a.[AssignedTo])
	AND ((a.FullName like '%'+@SearchText+'%')or (a.MobileNumber like '%'+@SearchText+'%') or (a.EmailId like '%'+@SearchText+'%'))
AND (a.CreatedOn >  Convert(Datetime,@FromDate)) 
AND (a.CreatedOn <  Convert(Datetime,@ToDate)) 
			)
			) ld     
		WHERE 
			ROW_ID BETWEEN @StartIndex AND @EndIndex
	End
	ELSE
	BEGIN
	
	set @AssignedTo = @LoggedInUser	
	
	
	
	----SELECT a.[Id],a.[FullName],a.[MobileNumber],a.[EmailId],b.Name as  [ServiceKey], c.Name as [LeadTypeKey],d.Name as [LeadSourceKey],a.[Remarks],a.[IsStudent],a.[IsCustomer],a.[IsSpam],a.[IsWon],a.[IsDisabled],a.[IsDelete],a.[PublicKey],a.[CreatedOn], cu.FirstName as  [CreatedBy],a.[ModifiedOn],a.[ModifiedBy],au.FirstName as [AssignedTo] 
	----FROM Leads a , Services b, LeadTypes c, LeadSources d, Users cu, Users au
	----WHERE (
	---- (a.[ServiceKey] = CONVERT(VARCHAR(100), b.PublicKey))
	----AND  (a.[LeadTypeKey] = CONVERT(VARCHAR(100), c.PublicKey))
	----AND  (a.[LeadSourceKey] = CONVERT(VARCHAR(100), d.PublicKey))
	----AND  (a.[CreatedBy] = CONVERT(VARCHAR(100), cu.PublicKey))
	----AND  (a.[AssignedTo] = CONVERT(VARCHAR(100), au.PublicKey))
	----AND  (@PrimaryKey ='0' OR CONVERT(VARCHAR(100), a.PublicKey)=@PrimaryKey) 
	----AND  (@SecondaryKey ='0' or @SecondaryKey=[ServiceKey]) 
	----AND  (@ThirdKey ='0' or @ThirdKey=[LeadTypeKey]) 
	----AND  (@FourthKey ='0' or @FourthKey=[LeadSourceKey]) 
	----AND (@CreatedBy ='0' or @CreatedBy=a.[CreatedBy]) 
	----AND (@AssignedTo ='0' or @LoggedInUser=a.[AssignedTo])
	----AND ((a.FullName like '%'+@SearchText+'%')or (a.MobileNumber like '%'+@SearchText+'%') or (a.EmailId like '%'+@SearchText+'%'))
 ---- AND (a.CreatedOn >  Convert(Datetime,@FromDate)) 
 ---- AND (a.CreatedOn <  Convert(Datetime,@ToDate)) 	
	----)   
	
	
	SELECT @vTotalRecordCount=COUNT(ID)FROM Leads a  where (
	 (@PrimaryKey ='0' OR CONVERT(VARCHAR(100), a.PublicKey)=@PrimaryKey) 
	AND (@SecondaryKey ='0' or @SecondaryKey=[ServiceKey]) 
	AND (@ThirdKey ='0' or @ThirdKey=[LeadTypeKey]) 
	AND (@FourthKey ='0' or @FourthKey=[LeadSourceKey]) 
	--AND (@CreatedBy ='0' or @CreatedBy=a.[CreatedBy]) 
	AND (@AssignedTo ='0' or @LoggedInUser=a.[AssignedTo])
	AND	((a.FullName like '%'+@SearchText+'%')or (a.MobileNumber like '%'+@SearchText+'%') or (a.EmailId like '%'+@SearchText+'%'))
	AND (a.CreatedOn >  Convert(Datetime,@FromDate)) 
	AND (a.CreatedOn <  Convert(Datetime,@ToDate)) 
			)
			
			print @vTotalRecordCount
	
		SELECT *,@vTotalRecordCount as TotalRecordCount
		FROM (SELECT ROW_NUMBER() OVER (ORDER BY
			CASE WHEN @vSortExpression='Id DESC' THEN a.[Id] END DESC,
			CASE WHEN @vSortExpression='Id ASC' THEN a.[Id] END ASC
			) AS 
			ROW_ID,a.[Id],a.[FullName],a.[MobileNumber],a.[EmailId],b.Name as  [ServiceKey], c.Name as [LeadTypeKey],d.Name as [LeadSourceKey],a.[Remarks],a.[IsSpam],a.[IsWon],a.[IsDisabled],a.[IsDelete],a.[PublicKey],a.[CreatedOn], cu.FirstName as  [CreatedBy],a.[ModifiedOn],a.[ModifiedBy],au.FirstName as [AssignedTo] 
	FROM Leads a , Services b, LeadTypes c, LeadSources d, Users cu, Users au
		WHERE (
		 (a.[ServiceKey] = CONVERT(VARCHAR(100), b.PublicKey))
	AND  (a.[LeadTypeKey] = CONVERT(VARCHAR(100), c.PublicKey))
	AND  (a.[LeadSourceKey] = CONVERT(VARCHAR(100), d.PublicKey))
	AND  (a.[CreatedBy] = CONVERT(VARCHAR(100), cu.PublicKey))
	AND  (a.[AssignedTo] = CONVERT(VARCHAR(100), au.PublicKey))
	AND(@PrimaryKey ='0' OR CONVERT(VARCHAR(100), a.PublicKey)=@PrimaryKey) 
	AND (@SecondaryKey ='0' or @SecondaryKey=[ServiceKey]) 
	AND (@ThirdKey ='0' or @ThirdKey=[LeadTypeKey]) 
	AND (@FourthKey ='0' or @FourthKey=[LeadSourceKey]) 
	--AND (@CreatedBy ='0' or @CreatedBy=a.[CreatedBy]) 
	AND (@AssignedTo ='0' or @LoggedInUser=a.[AssignedTo])
	AND ((a.FullName like '%'+@SearchText+'%')or (a.MobileNumber like '%'+@SearchText+'%') or (a.EmailId like '%'+@SearchText+'%'))
AND (a.CreatedOn >  Convert(Datetime,@FromDate)) 
AND (a.CreatedOn <  Convert(Datetime,@ToDate)) 
			)
			) ld     
		WHERE 
			ROW_ID BETWEEN @StartIndex AND @EndIndex
	
	
	End
	
	
	 
	
END


