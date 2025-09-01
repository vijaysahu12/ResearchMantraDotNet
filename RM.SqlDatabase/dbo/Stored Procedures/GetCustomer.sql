USE [Kingresearch]
GO
/****** Object:  StoredProcedure [dbo].[GetCustomers]    Script Date: 05/14/2023 6:04:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--EXEC GetCustomers 1,25,1,'CreatedOn','desc',null,null,'2023-05-01 05:03:34.000','2023-05-31 05:03:34.000',null
ALTER PROCEDURE [dbo].[GetCustomers]
@IsPaging bit = 1,   
 @PageSize int =25,   
 @PageNumber int =1,
 @SortExpression varchar(50) = NULL,
 @SortOrder varchar(50) = NULL,  
 @RequestedBy varchar(50)= NULL,
 @SearchText varchar(100)= NULL,
 @FromDate DateTime= NULL,
 @ToDate DateTime= NULL,
 @TotalCount INT = 0 OUTPUT
AS BEGIN 
	
	SELECT 
		leads.Id,leads.PublicKey AS LeadPublicKey, leads.FullName LeadName, leads.MobileNumber,leads.EmailId, 
		users.FirstName as AssignedToName , purchaseOrders.StartDate , purchaseOrders.EndDate, purchaseOrders.PaidAmount, 
		DATEDIFF(DAY, GETDATE() ,purchaseOrders.EndDate) as DaysToGo , purchaseOrders.Status , status.Name as PrStatusName,Leads.StatusId
	FROM Leads AS leads 
	INNER JOIN Users as users on leads.AssignedTo = users.PublicKey
	LEFT JOIN PurchaseOrders AS purchaseOrders ON leads.Id = purchaseOrders.LeadId
	INNER JOIN Status as status on status.Id = purchaseOrders.Status
	WHERE leads.AssignedTo IS NOT NULL and Leads.StatusId = 24
	AND (  CONVERT(date,  leads.CreatedOn) >=  CONVERT(date, @FromDate) AND  CONVERT(date, leads.CreatedOn) <=  CONVERT(date,  @ToDate))

ORDER BY 
	case
        when @SortOrder <> 'asc' then cast(null as date)
        when @SortExpression = 'CreatedOn' then leads.CreatedOn
        end ASC
,   case
        when @SortOrder <> 'asc' then ''
        when @SortExpression = 'leadName' then leads.FullName
        end ASC
,	case when @sortOrder <> 'desc' then ''
         when @SortExpression = 'leadName' then leads.FullName
         end DESC

OFFSET (IIF(@PageNumber <= 1 , 0 ,(@PageNumber-1))) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY

END 