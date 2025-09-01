USE [KingResearchDev]
GO
/****** Object:  StoredProcedure [dbo].[GetSalesDashboardReport]    Script Date: 28-08-2023 11:59:30 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--exec GetSalesDashboardReport '', '2023-08-1', '2023-08-30'
CREATE PROCEDURE [dbo].[GetSalesDashboardReport]
	@LoggedInUser varchar(50),
	@StartDate datetime,
	@EndDate datetime 
AS BEGIN

DECLARE 
	@TotalRevenue decimal= 0.0,
	@TotalSalesInCount int = 0.0 ,
	@TotalSalesPerPerson nvarchar(max) = '',
	@HighestSalesMaker decimal = 0.0,
	@TodaysSales decimal = 0.0,
	@TopFiveDealJson nvarchar(max),
	@TotalSalesPerDay nvarchar(max),
	@EveryDayPerformance nvarchar(max)
	--SET @StartDate = '2022-04-10 10:42:02.500' SET @EndDate = getdate()
--DECLARE @StartDate datetime = '2023-04-10 10:42:02.500', @EndDate DateTime = getdate()

	set @LoggedInUser = null 

	SELECT @TotalRevenue = SUM(PaidAmount) 
	FROM PurchaseOrders as po 
	INNER JOIN Status as st on po.Status = st.Id
	WHERE st.Code = 'cus' and IsActive = 1  and PaymentDate between CAST(@StartDate AS DATE) and CAST(@EndDate AS DATE)
	AND PO.CreatedBy = ISNULL(@LoggedInUser,PO.CreatedBy)
	-- Total Sales In Number 
	SELECT @TotalSalesInCount= COUNT(PaidAmount) 
	FROM PurchaseOrders as po 
	INNER JOIN Status as st on po.Status = st.Id
	WHERE st.Code = 'cus' AND IsActive = 1  AND PaymentDate BETWEEN CAST(@StartDate AS DATE) and CAST(@EndDate AS DATE)
	AND PO.CreatedBy = ISNULL(@LoggedInUser,PO.CreatedBy)

	SELECT @TotalSalesPerPerson = (
	SELECT 
	MAX(um.UserKey) as UserKey, MAX(us.FirstName ) as FirstName , SUM(ISNULL(po.PaidAmount,0.0)) AS PaidAmount 
	from UserMappings as um 
	LEFT JOIN Users as us on um.UserKey = us.Publickey
	left JOIN PurchaseOrders as po on po.CreatedBy = um.UserKey 
	WHERE um.UserType = 'bde' and PO.CreatedBy = ISNULL(@LoggedInUser,PO.CreatedBy) GROUP BY po.CreatedBy
	ORDER BY SUM(paidAmount) DESC 
	FOR JSON AUTO
	)

  
	-- Todays Total Sales 
	SELECT @TodaysSales = SUM(PaidAmount) FROM PurchaseOrders as po 
	INNER JOIN Status as st on po.Status = st.Id
	INNER JOIN Users as us on us.PublicKey = po.CreatedBy
	WHERE st.Code = 'cus' AND IsActive = 1  AND PaymentDate BETWEEN CAST(@StartDate AS DATE) and CAST(@EndDate AS DATE)
	AND PO.CreatedBy = ISNULL(@LoggedInUser,PO.CreatedBy)



	--Top 5 deal of the month (date durations)
	select @TopFiveDealJson = (
	SELECT top 5 LeadId, ClientName, PaymentDate, PaidAmount FROM PurchaseOrders as po 
	INNER JOIN Status as st on po.Status = st.Id
	INNER JOIN Users as us on us.PublicKey = po.CreatedBy
	WHERE st.Code = 'cus' AND IsActive = 1  AND PaymentDate BETWEEN CAST(@StartDate AS DATE) and CAST(@EndDate AS DATE) AND PO.CreatedBy = ISNULL(@LoggedInUser,PO.CreatedBy)
	ORDER BY PaidAmount desc FOR JSON AUTO
	 ) 
	 --select @TopFiveDealJson as TopFiveDeal
 


	--Total Sales Per Day between two dates 
	SELECT @TotalSalesPerDay = (
	SELECT FORMAT( PaymentDate, 'dd-MMM')  as PaymentDate  , sum(PaidAmount) as PerDaySales , Count(1) as PerDaySalesCount FROM PurchaseOrders as po 
	INNER JOIN Status as st on po.Status = st.Id
	INNER JOIN Users as us on us.PublicKey = po.CreatedBy
	WHERE st.Code = 'cus' AND IsActive = 1  AND PaymentDate BETWEEN CAST(@StartDate AS DATE) and CAST(@EndDate AS DATE) AND PO.CreatedBy = ISNULL(@LoggedInUser,PO.CreatedBy)
	GROUP BY PaymentDate FOR JSON AUTO
	)  

	;WITH DateRange AS (
		SELECT cast( DATEADD(DAY,  -30, @EndDate) as date) AS Date
		UNION ALL
		SELECT DATEADD(DAY, 1, Date)
		FROM DateRange
		WHERE Date < @EndDate
	)
	select @EveryDayPerformance = (
	SELECT FORMAT( Date  , 'dd-MMM') as Date , SUM(ISNULL(PaidAmount,0.0)) AS PaidAmount 
	FROM DateRange dr 
	LEFT JOIN PurchaseOrders as pr on dr.Date = CAST(PaymentDate as date) AND pr.CreatedBy = ISNULL(@LoggedInUser,pr.CreatedBy)
	GROUP BY DATE FOR JSON AUTO
	) 
	declare @MonthlySalesReport nvarchar(max)
	DECLARE @MonthStartDate DATE = DATEADD(MONTH,  (-1 * month(DATEADD(DAY, 1 - 1, DATEFROMPARTS(YEAR(GETDATE()), 8, 1)))) + 1     , DATEADD(DAY, 1 - 1, DATEFROMPARTS(YEAR(GETDATE()), 8, 1)))
	DECLARE @MonthEndDate DATE =  DATEADD(DAY, 0, DATEADD(DAY, 1 - 1, DATEFROMPARTS(YEAR(GETDATE()), 8, 1)));
	;WITH DateRange AS (
	SELECT @StartDate AS Date
	UNION ALL
	SELECT DATEADD(month, 1, Date)
	FROM DateRange
	WHERE DATEADD(month, 1, Date) <= @EndDate
	), GetMonthlyReport as (
	SELECT MONTH(PaymentDate) as Month, sum(PaidAmount) as TotalSales , DATEADD(DAY, 1 - 1, DATEFROMPARTS(YEAR(GETDATE()), 8, 1)) as SalesMonth  FROM PurchaseOrders 
	GROUP BY   MONTH(PaymentDate)
	)
	select @MonthlySalesReport  =(SELECT CAST(Date as date) as Date , ISNULL(gmr.TotalSales,0.0) as TotalSales FROM DateRange as dr left join GetMonthlyReport as gmr on cast(dr.Date as date) = cast( gmr.SalesMonth as date) FOR JSON AUTO)


	--;WITH DateRange AS (
	--	SELECT @MonthStartDate AS Date
	--	UNION ALL
	--	SELECT DATEADD(month, 1, Date)
	--	FROM DateRange
	--	WHERE DATEADD(month, 1, Date) <= @MonthEndDate
	--	), GetMonthlyReport as (
	--	SELECT MONTH(PaymentDate) as Month, sum(PaidAmount) as TotalSales , DATEADD(DAY, 1 - 1, DATEFROMPARTS(YEAR(GETDATE()), 8, 1)) as SalesInMonth  FROM PurchaseOrders 
	--	GROUP BY   MONTH(PaymentDate)
	--)
	--select @MonthlySalesReport  = (SELECT SalesInMonth , ISNULL(gmr.TotalSales,0.0) as TotalSales FROM DateRange as dr left join GetMonthlyReport as gmr on cast(dr.Date as date) = cast( gmr.SalesInMonth as date)
	--FOR JSON AUTO)


	SELECT 
		1000000 as CurrentTargets , ISNULL(@TotalRevenue, 0) as TotalSales, 
		@TotalSalesInCount as TotalSalesInCount , @TopFiveDealJson as TopFiveDealJson, 
		@TotalSalesPerDay as TotalSalesPerDayJson , @EveryDayPerformance as EveryDayPerformanceJson , 
		@TotalSalesPerPerson as TotalSalesPerPersonJson , @MonthlySalesReport as MonthlySalesReportJson
END

  