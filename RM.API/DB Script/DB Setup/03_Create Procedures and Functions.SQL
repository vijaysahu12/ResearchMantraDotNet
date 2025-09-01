use KingResearch

/****** Object:  StoredProcedure [dbo].[GetFilteredCustomers]    Script Date: 6/13/2021 10:53:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetFilteredCustomers]
		@CreatedBy int = null ,
		@Name varchar(100) = '',
		@TotalRecord INT = 0 OUTPUT
AS 
BEGIN

SELECT @TotalRecord = count(1) FROM Customers where CreatedBy = COALESCE(@CreatedBy, CreatedBy)

SELECT 
	Leads.FullName,
	Leads.MobileNumber,
	Leads.EmailId, 
	LeadTypes.Name as LeadType,  
	LeadSources.Name as LeadSource, 
	Leads.Remarks, 
	Leads.AssignedTo,
	Customers.CreatedBy   ,
	SUM(CustomerServices.AmountPaid) as TotalPurchase ,
	COUNT(Services.Name) as TotalService
From Customers 
INNER JOIN Leads 
	ON CONVERT(VARCHAR(36), Customers.LeadKey) = convert( varchar(36), Leads.PublicKey)
LEFT JOIN CustomerServices ON CONVERT(VARCHAR(36), Customers.PublicKey) = CONVERT(VARCHAR(36), CustomerServices.CustomerKey )
LEFT JOIN Services ON CustomerServices.ServiceKey = Services.PublicKey
INNER JOIN LeadSources ON Leads.LeadSourceKey =  LeadSources.PublicKey 
INNER JOIN LeadTypes on Leads.LeadTypeKey = LeadTypes.PublicKey
WHERE  Customers.IsDelete = 0 AND Leads.IsDelete = 0 
AND Customers.CreatedBy = Customers.CreatedBy
AND Customers.CreatedBy = coalesce(@CreatedBy ,Customers.CreatedBy)
AND (
	Leads.FullName 
		LIKE COALESCE('%'+ @Name + '%' , Leads.FullName) OR Leads.EmailId 
		LIKE COALESCE('%'+ @Name + '%' , Leads.EmailId)  OR Leads.MobileNumber 
		LIKE COALESCE('%'+ @Name + '%' , Leads.EmailId)
	)
GROUP BY CustomerServices.AmountPaid , Leads.FullName,
	Leads.MobileNumber,
	Leads.EmailId, 
	LeadTypes.Name, 
	LeadSources.Name,
	Leads.Remarks, 
	Leads.AssignedTo,
	Customers.CreatedBy,
	CustomerServices.ServiceKey,
	Services.Name
END

GO
/****** Object:  StoredProcedure [dbo].[GetPartnerAccounts]    Script Date: 6/13/2021 10:53:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPartnerAccounts]
(
	@IsPaging int =0,
	@PageSize int =0,
	@PageNumber int =30,
	@FromDate varchar(50) ='0',
	@ToDate varchar(50) ='0',	
	@SearchText varchar(250) =''	,
	@SortExpression varchar(50) ='Date'
)
AS
BEGIN	
 
	declare @StartIndex int =0
	declare @EndIndex int =0
	
	set @StartIndex = (@PageNumber-1) * @PageSize 
	set @EndIndex = @StartIndex + @PageSize 

	DECLARE @vTotalRecordCount INT  
	--DECLARE @vSortExpression varchar (100)
	--SET @vSortExpression = @SortExpression + ' ' + @SortOrder
	
	
	SELECT 
		ID, FullName, MobileNumber, Emailid, City , 
	(
		CASE WHEN COALESCE(ZerodhaCId , '' ) = ''
			THEN  
				(
					CASE WHEN COALESCE(AngelCId , '' ) = ''
					THEN  
					( 
						CASE WHEN COALESCE(AliceBlueCid , '' ) = ''
						THEN  ('') ELSE 'ALICE'
						END
					)
					ELSE 'ANGEL'
					END
				)
			ELSE 'ZERODHA'
			END

	) AS PartnerWith,
	(
		CASE WHEN COALESCE(ZerodhaCId , '' ) = ''
			THEN  
				(
					CASE WHEN COALESCE(AngelCId , '' ) = ''
					THEN  
					( 
						CASE WHEN COALESCE(AliceBlueCid , '' ) = ''
						THEN  ('') ELSE AliceBlueCid
						END
					)
					ELSE AngelCId
					END
				)
			ELSE ZerodhaCId
			END

	) AS ClientId,
	CONCAT(COALESCE(' Angel: ' + AngelCId + '  ', ''),COALESCE( '|| AliceBlue: ' + AliceBlueCid + '  ', ''), COALESCE( '|| Zerodha: ' +   ZerodhaCId + '  ', '') ) as Details  , Remarks , CreatedOn 
	FROM PartnerAccounts 
	WHERE COALESCE(MOBILENUMBER, '0') <> '0' 
	AND PartnerAccounts.FullName LIKE '%' + COALESCE(@SearchText, PartnerAccounts.FullName) + '%'
	OR PartnerAccounts.Emailid LIKE '%' + COALESCE(@SearchText, PartnerAccounts.Emailid) + '%'
	OR PartnerAccounts.ZerodhaCId LIKE '%' + COALESCE(@SearchText, PartnerAccounts.ZerodhaCId) + '%'
	OR PartnerAccounts.AliceBlueCid LIKE '%' + COALESCE(@SearchText, PartnerAccounts.AliceBlueCid) + '%'
	OR PartnerAccounts.AngelCId LIKE '%' + COALESCE(@SearchText, PartnerAccounts.AngelCId) + '%'

END

GO
/****** Object:  StoredProcedure [dbo].[PrintSQLResults]    Script Date: 6/13/2021 10:53:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create procedure [dbo].[PrintSQLResults] 
    @query nvarchar(MAX),
    @numberToDisplay int = 10,
    @padding int = 20
as

SET NOCOUNT ON;
SET ANSI_WARNINGS ON;

declare @cols nvarchar(MAX), 
        @displayCols nvarchar(MAX), 
        @sql nvarchar(MAX), 
        @printableResults nvarchar(MAX),
        @NewLineChar AS char(2) = char(13) + char(10),
        @Tab AS char(9) = char(9);

if exists (select * from tempdb.sys.tables where name = '##PrintSQLResultsTempTable') drop table ##PrintSQLResultsTempTable

set @query = REPLACE(@query, 'from', ' into ##PrintSQLResultsTempTable from');
--print @query
exec(@query);
select ROW_NUMBER() OVER (ORDER BY (select Null)) AS ID12345XYZ, * into #PrintSQLResultsTempTable 
from ##PrintSQLResultsTempTable
drop table ##PrintSQLResultsTempTable

select name
into #PrintSQLResultsTempTableColumns
from tempdb.sys.columns where object_id =
object_id('tempdb..#PrintSQLResultsTempTable');

select @cols =
stuff((
    (select ' + space(1) + (LEFT( (CAST([' + name + '] as nvarchar(max)) + space('+ CAST(@padding as nvarchar(4)) +')), '+CAST(@padding as nvarchar(4))+')) ' as [text()]
    FROM #PrintSQLResultsTempTableColumns
    where name != 'ID12345XYZ'
    FOR XML PATH(''), root('str'), type ).value('/str[1]','nvarchar(max)'))
,1,0,'''''');

select @displayCols =
stuff((
    (select space(1) + LEFT(name + space(@padding), @padding) as [text()]
    FROM #PrintSQLResultsTempTableColumns
    where name != 'ID12345XYZ'
    FOR XML PATH(''), root('str'), type ).value('/str[1]','nvarchar(max)'))
,1,0,'');

DECLARE 
    @tableCount int = (select count(*) from #PrintSQLResultsTempTable);
DECLARE 
    @i int = 1, 
    @ii int = case when @tableCount < @numberToDisplay then @tableCount else @numberToDisplay end;

print @displayCols -- header
While @i <= @ii
BEGIN
    set @sql = N'select @printableResults = ' + @cols + ' + @NewLineChar from #PrintSQLResultsTempTable where ID12345XYZ = ' + CAST(@i as varchar(3)) + '; print @printableResults;'
    --print @sql
    execute sp_executesql @sql, N'@NewLineChar char(2), @printableResults nvarchar(max) output', @NewLineChar = @NewLineChar, @printableResults = @printableResults output
    print @printableResults
    SET @i += 1;
END

GO
/****** Object:  StoredProcedure [dbo].[Sp_Get_Leads]    Script Date: 6/13/2021 10:53:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Sp_Get_Leads]
(
	@IsPaging int =0,
	@PageSize int =0,
	@PageNumber int =30,
	@SortOrder varchar(50) ='DESC',
	@SortExpression varchar(50) ='Date',	
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
If @PageSize = 0 set @PageSize = 30
If @PageNumber = 0 set @PageNumber = 1

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
declare @RoleId int =0
Select @RoleId = Id from Roles where CONVERT(VARCHAR(100), PublicKey) = @RoleKey


	declare @StartIndex int =0
	declare @EndIndex int =0
	
	set @StartIndex = (@PageNumber-1) * @PageSize 
	set @EndIndex = @StartIndex + @PageSize 

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
			CASE WHEN @vSortExpression='Date DESC' THEN a.[Id] END DESC,
			CASE WHEN @vSortExpression='Date ASC' THEN a.[Id] END ASC,
			CASE WHEN @vSortExpression='Name DESC' THEN a.[FullName] END DESC,
			CASE WHEN @vSortExpression='Name ASC' THEN a.[FullName] END ASC,
			CASE WHEN @vSortExpression='LeadType DESC' THEN c.Name END DESC,
			CASE WHEN @vSortExpression='LeadType ASC' THEN c.Name END ASC
			) AS 
			ROW_ID,a.[Id],a.[FullName],a.[MobileNumber],a.[EmailId],b.Name as  [ServiceKey], c.Name as [LeadTypeKey],d.Name as [LeadSourceKey],
			a.[Remarks],a.[IsSpam],a.[IsWon],
			a.[IsDisabled],a.[IsDelete],a.[PublicKey],a.[CreatedOn], cu.FirstName as  [CreatedBy],a.[ModifiedOn],a.[ModifiedBy],au.FirstName as [AssignedTo] 
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
			CASE WHEN @vSortExpression='Date DESC' THEN a.[Id] END DESC,
			CASE WHEN @vSortExpression='Date ASC' THEN a.[Id] END ASC,
			CASE WHEN @vSortExpression='Name DESC' THEN a.[FullName] END DESC,
			CASE WHEN @vSortExpression='Name ASC' THEN a.[FullName] END ASC,
			CASE WHEN @vSortExpression='LeadType DESC' THEN c.Name END DESC,
			CASE WHEN @vSortExpression='LeadType ASC' THEN c.Name END ASC
			) AS 
			ROW_ID,a.[Id],a.[FullName],a.[MobileNumber],a.[EmailId],b.Name as  [ServiceKey], c.Name as [LeadTypeKey],d.Name as [LeadSourceKey],a.[Remarks]
			,a.[IsSpam],a.[IsWon],a.[IsDisabled],a.[IsDelete],a.[PublicKey],a.[CreatedOn], cu.FirstName as  [CreatedBy],a.[ModifiedOn],a.[ModifiedBy],au.FirstName as [AssignedTo] 
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

GO
/****** Object:  StoredProcedure [dbo].[Sp_Get_Leads_v]    Script Date: 6/13/2021 10:53:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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

GO
/****** Object:  StoredProcedure [dbo].[Sp_GetCustomerServices]    Script Date: 6/13/2021 10:53:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create PROC [dbo].[Sp_GetCustomerServices] 
@CustomerKey varchar(100)
AS
Begin

Select a.ID, a.CustomerKey,b.Name as ServiceKey, b.ServiceCost as AmountPaid, c.Name as PaymentModeKey, a.IsWon,
a.OrderId,a.Remarks,
 a.IsDisabled
           ,a.IsDelete
           ,a.PublicKey
           ,a.CreatedOn
           ,a.CreatedBy
           ,a.ModifiedOn
           ,a.ModifiedBy 
           from CustomerServices a , 
           Services b, 
           PaymentModes c 
           where 
           a.CustomerKey = @CustomerKey and 
           a.IsDelete = 0 and 
           a.PaymentModeKey = c.PublicKey and 
           a.ServiceKey = b.PublicKey
           
--Select * from CustomerServices  
--Select * from Services
End

GO
/****** Object:  StoredProcedure [dbo].[Sp_GetCustomerTags]    Script Date: 6/13/2021 10:53:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROC [dbo].[Sp_GetCustomerTags]
@CustomerKey varchar(100)
AS
Begin

Select a.Id, a.CustomerKey,b.Name as TagKey
           ,a.IsDelete
           ,a.PublicKey
           ,a.CreatedOn
           ,a.CreatedBy
           ,a.ModifiedOn
           ,a.ModifiedBy 
           from CustomerTags a , 
           Tags b
           where 
           a.CustomerKey = @CustomerKey and 
           a.IsDelete = 0 and 
           a.TagKey = b.PublicKey
           
--Select * from CustomerServices  
--Select * from Services
End

GO
/****** Object:  StoredProcedure [dbo].[Sp_GetServices]    Script Date: 6/13/2021 10:53:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[Sp_GetServices] 
--@ServiceId int
AS
Begin
--Select * from services 
Select a.ID, a.Name,a.Description, a.ServiceCost, b.Name as ServiceTypeKey, c.Name as ServiceCategoryKey, a.IsDisabled
           ,a.IsDelete
           ,a.PublicKey
           ,a.CreatedOn
           ,a.CreatedBy
           ,a.ModifiedOn
           ,a.ModifiedBy 
           from Services a , ServiceTypes b, ServiceCategories c where a.ServiceTypeKey = b.PublicKey and a.ServiceCategoryKey = c.PublicKey and a.IsDelete = 0 
End


GO
/****** Object:  StoredProcedure [dbo].[Sp_Roles_Select]    Script Date: 6/13/2021 10:53:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [dbo].[Sp_Roles_Select]
	--@Id int
AS

SELECT	Id,
	 [Name]
      ,[Description]
      ,[IsDisabled]
      ,[IsDelete]
      ,[PublicKey]
      ,[CreatedOn]
      ,[CreatedBy]
      ,[ModifiedOn]
      ,[ModifiedBy]
FROM	Roles
--WHERE 	Id = @Id


GO
/****** Object:  StoredProcedure [dbo].[Sp_Sys_Auto_Select]    Script Date: 6/13/2021 10:53:48 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROC [dbo].[Sp_Sys_Auto_Select]
	@sTableName varchar(128),
	@bExecute bit = 0
AS

IF dbo.fnTableHasPrimaryKey(@sTableName) = 0
 BEGIN
	RAISERROR ('Procedure cannot be created on a table with no primary key.', 10, 1)
	RETURN
 END

DECLARE	@sProcText varchar(8000),
	@sKeyFields varchar(2000),
	@sSelectClause varchar(2000),
	@sWhereClause varchar(2000),
	@sColumnName varchar(128),
	@nColumnID smallint,
	@bPrimaryKeyColumn bit,
	@nAlternateType int,
	@nColumnLength int,
	@nColumnPrecision int,
	@nColumnScale int,
	@IsNullable bit, 
	@IsIdentity int,
	@sTypeName varchar(128),
	@sDefaultValue varchar(4000),
	@sCRLF char(2),
	@sTAB char(1)

SET	@sTAB = char(9)
SET 	@sCRLF = char(13) + char(10)

SET 	@sProcText = ''
SET 	@sKeyFields = ''
SET	@sSelectClause = ''
SET	@sWhereClause = ''

SET 	@sProcText = @sProcText + 'IF EXISTS(SELECT * FROM sysobjects WHERE name = ''Sp_' + @sTableName + '_Select'')' + @sCRLF
SET 	@sProcText = @sProcText + @sTAB + 'DROP PROC Sp_' + @sTableName + '_Select' + @sCRLF
IF @bExecute = 0
	SET 	@sProcText = @sProcText + 'GO' + @sCRLF

SET 	@sProcText = @sProcText + @sCRLF

PRINT @sProcText

IF @bExecute = 1 
	EXEC (@sProcText)

SET 	@sProcText = ''
SET 	@sProcText = @sProcText + '----------------------------------------------------------------------------' + @sCRLF
SET 	@sProcText = @sProcText + '-- Select a single record from ' + @sTableName + @sCRLF
SET 	@sProcText = @sProcText + '----------------------------------------------------------------------------' + @sCRLF
SET 	@sProcText = @sProcText + 'CREATE PROC Sp_' + @sTableName + '_Select' + @sCRLF

DECLARE crKeyFields cursor for
	SELECT	*
	FROM	dbo.fnTableColumnInfo(@sTableName)
	ORDER BY 2

OPEN crKeyFields

FETCH 	NEXT 
FROM 	crKeyFields 
INTO 	@sColumnName, @nColumnID, @bPrimaryKeyColumn, @nAlternateType, 
	@nColumnLength, @nColumnPrecision, @nColumnScale, @IsNullable, 
	@IsIdentity, @sTypeName, @sDefaultValue
				
WHILE (@@FETCH_STATUS = 0)
 BEGIN
	IF (@bPrimaryKeyColumn = 1)
	 BEGIN
		IF (@sKeyFields <> '')
			SET @sKeyFields = @sKeyFields + ',' + @sCRLF 
	
		SET @sKeyFields = @sKeyFields + @sTAB + '@' + @sColumnName + ' ' + @sTypeName
	
		IF (@nAlternateType = 2) --decimal, numeric
			SET @sKeyFields =  @sKeyFields + '(' + CAST(@nColumnPrecision AS varchar(3)) + ', ' 
					+ CAST(@nColumnScale AS varchar(3)) + ')'
	
		ELSE IF (@nAlternateType = 1) --character and binary
			SET @sKeyFields =  @sKeyFields + '(' + CAST(@nColumnLength AS varchar(4)) +  ')'

		IF (@sWhereClause = '')
			SET @sWhereClause = @sWhereClause + 'WHERE ' 
		ELSE
			SET @sWhereClause = @sWhereClause + ' AND ' 

		SET @sWhereClause = @sWhereClause + @sTAB + @sColumnName  + ' = @' + @sColumnName + @sCRLF 
	 END

	IF (@sSelectClause = '')
		SET @sSelectClause = @sSelectClause + 'SELECT'
	ELSE
		SET @sSelectClause = @sSelectClause + ',' + @sCRLF 

	SET @sSelectClause = @sSelectClause + @sTAB + @sColumnName 

	FETCH 	NEXT 
	FROM 	crKeyFields 
	INTO 	@sColumnName, @nColumnID, @bPrimaryKeyColumn, @nAlternateType, 
		@nColumnLength, @nColumnPrecision, @nColumnScale, @IsNullable, 
		@IsIdentity, @sTypeName, @sDefaultValue
 END

CLOSE crKeyFields
DEALLOCATE crKeyFields

SET 	@sSelectClause = @sSelectClause + @sCRLF

SET 	@sProcText = @sProcText + @sKeyFields + @sCRLF
SET 	@sProcText = @sProcText + 'AS' + @sCRLF
SET 	@sProcText = @sProcText + @sCRLF
SET 	@sProcText = @sProcText + @sSelectClause
SET 	@sProcText = @sProcText + 'FROM	' + @sTableName + @sCRLF
SET 	@sProcText = @sProcText + @sWhereClause
SET 	@sProcText = @sProcText + @sCRLF
IF @bExecute = 0
	SET 	@sProcText = @sProcText + 'GO' + @sCRLF


PRINT @sProcText

IF @bExecute = 1 
	EXEC (@sProcText)

GO
/****** Object:  StoredProcedure [dbo].[GetPartnerAccountsSummaryReport]    Script Date: 23-04-2023 12:29:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[GetPartnerAccountsSummaryReport] 
	@PartnerAccountName varchar(50),
	@AssignedTo varchar(50),
	@FromDate date,
	@ToDate date,
	@BenchMark INT output
AS 

BEGIN 

DECLARE @MinDate DATE = ISNULL(@FromDate,  DATEADD(DD,-(DAY(GETDATE() -1)), GETDATE())) ,
        @MaxDate DATE = ISNULL(@ToDate,DATEADD(DD,-(DAY(GETDATE())), DATEADD(MM, 1, GETDATE())));
set @BenchMark = 20

;with CTE as (
SELECT  TOP (DATEDIFF(DAY, @MinDate, @MaxDate) + 1)
        Date = DATEADD(DAY, ROW_NUMBER() OVER(ORDER BY a.object_id) - 1, @MinDate)
FROM    sys.all_objects a CROSS JOIN sys.all_objects b
), GetTotalRegistration as (
	SELECT  CAST(CreatedOn As Date) as DateOn , count(1) as TotalRegistration FROM PartnerAccounts 
	WHERE CreatedOn BETWEEN  @MinDate  and @MaxDate
	GROUP BY CAST(CreatedOn As Date)
), GetTotalConversion as (
	SELECT  
		CAST(ModifiedOn As Date) as DateOn, count(1) as TotalConversion  
	FROM PartnerAccounts 
	WHERE CreatedOn BETWEEN  @MinDate  and @MaxDate and STATUS = 2
	GROUP BY CAST(ModifiedOn As Date)
)
 SELECT 
	CTE.Date , 
	ISNULL(reg.TotalRegistration , 0) as TotalRegistration  , 
	ISNULL( con.TotalConversion,0) as TotalConversion  
 FROM Cte 
 LEFT JOIN GetTotalRegistration as reg on cte.Date = reg.DateOn
 LEFT JOIN GetTotalConversion as con on con.DateOn = reg.DateOn
 ORDER BY CAST(reg.DateOn As Date) asc

--SELECT * FROM PartnerAccounts ORDER BY ModifiedOn desc 
--SELECT  CAST(CreatedOn As Date) as DateOn , count(1) as TotalRegistration FROM PartnerAccounts 
--WHERE CreatedOn BETWEEN  @MinDate  and @MaxDate
--GROUP BY CAST(CreatedOn As Date)
--SELECT FullName, CreatedOn, Status FROM PartnerAccounts WHERE CAST(CreatedOn As Date)  = '2023-04-08'

--SELECT DATEADD(DD,-(DAY(GETDATE() -1)), GETDATE()) AS FirstDate
--SELECT DATEADD(DD,-(DAY(GETDATE())), DATEADD(MM, 1, GETDATE())) AS LastDate

--SELECT * FROM PartnerAccounts where CreatedOn between  DATEADD(DD,-(DAY(GETDATE() -1)), GETDATE()) and DATEADD(DD,-(DAY(GETDATE())), DATEADD(MM, 1, GETDATE()))
--select FullName, AssignedTo, Source, CreatedOn, ModifiedOn from PartnerAccounts  
--where Status = 2 
--order by CreatedOn desc 
END 
GO
USE [master]
GO
ALTER DATABASE [KingResearch] SET  READ_WRITE 
GO
