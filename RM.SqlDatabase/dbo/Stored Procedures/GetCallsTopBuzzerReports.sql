 
---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
--exec GetCallsTopBuzzerReports 

CREATE PROCEDURE [dbo].[GetCallsTopBuzzerReports]
	@StartDate dATE = NULL,
	@EndDate DATE = null ,
	@CallBy varchar(50) = null-- '8DD53404-F2CD-ED11-8110-00155D23D79C'
AS BEGIN
	--DECLARE 
	--	@StartDate datetime = null,
	--	@EndDate datetime = null,
	--	@CallBy varchar(50) = null-- '8DD53404-F2CD-ED11-8110-00155D23D79C'
	 
 
	 IF(@CallBy = '')
	 BEGIN
		SET @CallBy = null
	 END

	SET @StartDate = ISNULL(@StartDate, CAST( dateadd(day,-30,GetDate() )  AS date) )
	--SELECT @StartDate  --CAST( dateadd(day,-30,GetDate() )  AS date)
	SET @EndDate = ISNULL(@EndDate, CAST(GetDate()  AS date) )
	
	Declare @BenchMark decimal = 50000;
	
	SELECT top 1 @BenchMark = pnl + @BenchMark FROM CallPerformance where CallStatus = 'closed' and ExitPrice is not null and IsCallActivate = 1
	order by Pnl desc 

	select StrategyKey ,max(st.Name) as Strategyname  , sum(pnl) as Pnl , cast(sum((pnl * 100) / @BenchMark) as Decimal(10,2)) as Percentage  From CallPerformance as cp 
	inner join Strategies as st on  cp.StrategyKey = st.PublicKey
	where cast(CallDate as date) between CAST(@StartDate as date) and CAST(@EndDate as date) and cp.IsCallActivate = 1
	and  CallStatus = 'closed'  and CallByKey = ISNULL(@CallBy, CallByKey)

	Group by StrategyKey
	order by Pnl desc 

END  