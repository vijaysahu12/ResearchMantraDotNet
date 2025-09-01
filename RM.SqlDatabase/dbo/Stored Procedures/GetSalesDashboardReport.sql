--exec GetSalesReportForDashboard '', '2023-08-10 18:35:16.180', '2023-08-10 18:35:16.180'
CREATE PROCEDURE GetSalesDashboardReport
	@LoggedInUser varchar(50),
	@StartDate datetime,
	@EndDate datetime
AS BEGIN

declare 
@TotalRevenue decimal= 0.0,
@TotalSalesInCount int = 0.0 ,
@TotalSalesPerPerson nvarchar(max) = '',
@HighestSalesMaker decimal = 0.0,
@TodaysSales decimal = 0.0,
@TopFiveDealJson nvarchar(max),
@TotalSalesPerDay nvarchar(max),
@EveryDayPerformance nvarchar(max)
SET @StartDate = '2023-04-10 10:42:02.500' SET @EndDate = getdate()
--DECLARE @StartDate datetime = '2023-04-10 10:42:02.500', @EndDate DateTime = getdate()


SELECT @TotalRevenue = SUM(PaidAmount) 
FROM PurchaseOrders as po 
INNER JOIN Status as st on po.Status = st.Id
WHERE st.Code = 'cus' and IsActive = 1  and PaymentDate between @StartDate and @EndDate


-- Total Sales In Number 
SELECT @TotalSalesInCount= COUNT(PaidAmount) 
FROM PurchaseOrders as po 
INNER JOIN Status as st on po.Status = st.Id
WHERE st.Code = 'cus' AND IsActive = 1  AND PaymentDate BETWEEN @StartDate AND @EndDate



select @TotalSalesPerPerson = (
SELECT 
MAX(um.UserKey) as UserKey, MAX(us.FirstName ) as FirstName , SUM(ISNULL(po.PaidAmount,0.0)) AS PaidAmount 
from UserMappings as um 
LEFT JOIN Users as us on um.UserKey = us.Publickey
left JOIN PurchaseOrders as po on po.CreatedBy = um.UserKey 
WHERE um.UserType = 'bde' GROUP BY po.CreatedBy
ORDER BY SUM(paidAmount) DESC 
FOR JSON AUTO
)

  
-- Todays Total Sales 
SELECT @TodaysSales = SUM(PaidAmount) FROM PurchaseOrders as po 
INNER JOIN Status as st on po.Status = st.Id
INNER JOIN Users as us on us.PublicKey = po.CreatedBy
WHERE st.Code = 'cus' AND IsActive = 1  AND PaymentDate BETWEEN @StartDate AND @EndDate




--Top 5 deal of the month (date durations)
select @TopFiveDealJson = (
SELECT top 5 LeadId, ClientName, PaymentDate, PaidAmount FROM PurchaseOrders as po 
INNER JOIN Status as st on po.Status = st.Id
INNER JOIN Users as us on us.PublicKey = po.CreatedBy
WHERE st.Code = 'cus' AND IsActive = 1  AND PaymentDate BETWEEN @StartDate AND @EndDate
ORDER BY PaidAmount desc FOR JSON AUTO
 ) 
 --select @TopFiveDealJson as TopFiveDeal


--Total Sales Per Day between two dates 
SELECT @TotalSalesPerDay = (
SELECT PaymentDate, sum(PaidAmount) as PerDaySales , Count(1) as PerDaySalesCount FROM PurchaseOrders as po 
INNER JOIN Status as st on po.Status = st.Id
INNER JOIN Users as us on us.PublicKey = po.CreatedBy
WHERE st.Code = 'cus' AND IsActive = 1  AND PaymentDate BETWEEN @StartDate AND @EndDate
GROUP BY PaymentDate FOR JSON AUTO
)  
;WITH DateRange AS (
    SELECT cast( DATEADD(DAY, -30, @EndDate) as date) AS Date
    UNION ALL
    SELECT DATEADD(DAY, 1, Date)
    FROM DateRange
    WHERE Date < @EndDate
)
select @EveryDayPerformance = (
SELECT Date , SUM(ISNULL(PaidAmount,0.0)) AS PaidAmount 
FROM DateRange dr 
LEFT JOIN PurchaseOrders as pr on dr.Date = CAST(PaymentDate as date)
GROUP BY DATE FOR JSON AUTO
) 


SELECT 
	1000000 as CurrentTargets , @TotalRevenue as TotalSales, 
	@TotalSalesInCount as TotalSalesInCount , @TopFiveDealJson as TopFiveDealJson, 
	@TotalSalesPerDay as TotalSalesPerDayJson , @EveryDayPerformance as EveryDayPerformanceJson , 
	@TotalSalesPerPerson as TotalSalesPerPersonJson

	
END



