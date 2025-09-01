CREATE PROCEDURE [dbo].[GetCustomers]
--declare 
 @IsPaging bit = 1,   
 @PageSize int =25, 
 @PageNumber int =1,
 @SortExpression varchar(50) = NULL,
 @SortOrder varchar(50) = NULL,  
 @RequestedBy varchar(50)= null,
 @SearchText varchar(100)= null,
 @FromDate DateTime= null,
 @ToDate DateTime= null,
 @StrategyKey varchar(50) = null,
 @TotalCount INT = 0  OUTPUT
AS BEGIN 


set @IsPaging=0 
set @PageSize=20 
--set @PageNumber =1 
set @SortExpression='callDate' set @SortOrder='desc' 
--set @RequestedBy=NULL 
set @SearchText=NULL 
--set @FromDate='2023-03-31 11:00:31.333' 
--set @ToDate='2023-05-30 11:00:31.333' 
set @StrategyKey=NULL 
set @TotalCount = 0



SELECT @TotalCount = count(1)
FROM Leads AS leads 
INNER JOIN Users as users on leads.AssignedTo = users.PublicKey
LEFT JOIN PurchaseOrders AS purchaseOrders ON leads.Id = purchaseOrders.LeadId
LEFT JOIN LeadTypes as LeadTypes on LeadTypes.PublicKey = Leads.LeadTypeKey
INNER JOIN Status as status on status.Id = purchaseOrders.Status
WHERE leads.AssignedTo IS NOT NULL 
AND purchaseOrders.Status = (select Id from Status where name = 'customer') 
--and cast(GETDATE() as date) BETWEEN CAST(ISNULL(purchaseOrders.StartDate, '2020-01-01') as date) and CAST(ISNULL(purchaseOrders.EndDate, '2020-01-01') as date)


DROP TABLE if exists #tempCustomer

SELECT 
	leads.Id,leads.PublicKey AS LeadPublicKey, leads.FullName, leads.MobileNumber,leads.EmailId, LeadTypes.Name as [LeadTypeKey],
	users.FirstName as AssignedToName , isnull(purchaseOrders.StartDate,'') as StartDate, isnull(purchaseOrders.EndDate,'') as EndDate, purchaseOrders.PaidAmount, 
	IsNull(DATEDIFF(DAY, GETDATE() ,purchaseOrders.EndDate),0) as DaysToGo , purchaseOrders.Status ,
	status.Name as PrStatusName,leads.PurchaseOrderKey,purchaseOrders.City, purchaseOrders.Remark , 
	purchaseOrders.ModeOfPayment , purchaseOrders.ServiceId

INTO #tempCustomer
FROM Leads AS leads 
INNER JOIN Users as users on leads.AssignedTo = users.PublicKey
LEFT JOIN PurchaseOrders AS purchaseOrders ON leads.Id = purchaseOrders.LeadId
LEFT JOIN LeadTypes as LeadTypes on LeadTypes.PublicKey = Leads.LeadTypeKey
INNER JOIN Status as status on status.Id = purchaseOrders.Status
WHERE leads.AssignedTo IS NOT NULL 
AND purchaseOrders.Status = (SELECT Id FROM STATUS where name = 'customer') 
AND purchaseOrders.CreatedBy = isnull(@RequestedBy,purchaseOrders.CreatedBy)
--and cast(GETDATE() as date) BETWEEN CAST(ISNULL(purchaseOrders.StartDate, '2020-01-01') as date) and CAST(ISNULL(purchaseOrders.EndDate, '2020-01-01') as date)
--
--AND (  CONVERT(date,  leads.CreatedOn) >=  CONVERT(date, @FromDate) AND  CONVERT(date, leads.CreatedOn) <=  CONVERT(date,  @ToDate))
ORDER BY 
	CASE
		WHEN @SortOrder <> 'asc' then cast(null as date)
		WHEN @SortExpression = 'CreatedOn' then leads.CreatedOn
	END ASC ,   
	CASE
		WHEN @SortOrder <> 'asc' then ''
		WHEN @SortExpression = 'leadName' then leads.FullName
	end ASC ,	
	case 
		when @sortOrder <> 'desc' then ''
		when @SortExpression = 'leadName' then leads.FullName
	end DESC
OFFSET (IIF(@PageNumber <= 1 , 0 ,(@PageNumber-1))) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY

SELECT (SELECT * FROM #tempCustomer	FOR JSON AUTO) as JsonData

END 