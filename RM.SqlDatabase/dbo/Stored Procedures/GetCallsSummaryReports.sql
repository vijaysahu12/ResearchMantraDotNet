--EXEC GetCallsSummaryReports '2023-03-01 19:05:26.067' , '2023-06-01 19:05:26.067' , ''
CREATE PROCEDURE [dbo].[GetCallsSummaryReports]
	@StartDate Date = NULL,
	@EndDate Date = null ,
	@CallBy varchar(50) = null-- '8DD53404-F2CD-ED11-8110-00155D23D79C'
AS BEGIN

--DECLARE 
--	@StartDate datetime = null,
--	@EndDate datetime = '2023-04-14',
--	@CallBy varchar(50) = null-- '8DD53404-F2CD-ED11-8110-00155D23D79C'

IF (@CallBy = '')
BEGIN
	set @CallBy  = null
END

DROP TABLE if EXISTS #filterCallPerformance
	 
SELECT * INTO #filterCallPerformance 
FROM CallPerformance where CallDate BETWEEN ISNULL( @StartDate , (DATEADD(MONTH,-20,GETDATE())))   AND ISNULL(@EndDate, GETDATE()) 
AND IsCallActivate = 1 and CallByKey = ISNULL(@CallBy, CallByKey)
AND ISNULL(isDelete , 0) = 0

SET @StartDate = ISNULL(@StartDate, CAST( dateadd(day,-300,GetDate() )  AS date) )
--SELECT @StartDate  --CAST( dateadd(day,-30,GetDate() )  AS date)
SET @EndDate = ISNULL(@EndDate, CAST(GetDate()  AS date) )

DECLARE @TotalCalls int = 0 , @SuccessBuyCalls int= 0, @SuccessSellCalls int= 0, @ClosedCalls int= 0, @StopLossBuyCalls int= 0 ,
@StopLossSellCalls int = 0, @Gar int = 0 , @OpenCalls int = 0 , @NotActive int = 0 , @c2c int = 0 

SELECT @TotalCalls  = COUNT(1) FROM #filterCallPerformance --where CallDate BETWEEN @StartDate and @EndDate and IsCallActivate = 1 and CallByKey = ISNULL(@CallBy, CallByKey)
SELECT @ClosedCalls = COUNT(1) FROM #filterCallPerformance where CallStatus = 'Closed' -- CallDate BETWEEN @StartDate and @EndDate and IsCallActivate = 1 and  and CallByKey = ISNULL(@CallBy, CallByKey)
SELECT @OpenCalls   = Count(1) FROM #filterCallPerformance WHERE  CallStatus = 'Open' --CallDate BETWEEN @StartDate and @EndDate and IsCallActivate = 1 and and  CallByKey = ISNULL(@CallBy, CallByKey)

SELECT @SuccessBuyCalls = count(1)  FROM #filterCallPerformance 
where  CallStatus = 'closed' and TradeType = 'Buy' and EntryPrice < ExitPrice 
--and  CallDate between @StartDate and @EndDate and  CallByKey = ISNULL(@CallBy, CallByKey) and IsCallActivate = 1
GROUP BY TradeType


SELECT @SuccessSellCalls =  count(1)  FROM #filterCallPerformance 
WHERE CallStatus = 'closed' and TradeType = 'Sell'  and   EntryPrice > ExitPrice  
--and CallDate BETWEEN @StartDate and @EndDate 
--and CallByKey = ISNULL(@CallBy, CallByKey)
--and IsCallActivate = 1
GROUP BY TradeType



SELECT  @StopLossBuyCalls =  count(1)  FROM #filterCallPerformance 
where CallStatus = 'closed' and  EntryPrice > ExitPrice and TradeType = 'Buy' -- and CallByKey = ISNULL(@CallBy, CallByKey)
--CallDate between @StartDate and @EndDate AND  and IsCallActivate = 1 AND ISNULL(cp.IsDelete, 0) = 0 
GROUP BY TradeType  

SELECT @StopLossSellCalls =  count(1) FROM #filterCallPerformance 
WHERE CallStatus = 'closed' and  EntryPrice < ExitPrice and TradeType = 'Sell'  
--and  CallDate BETWEEN @StartDate AND @EndDate AND   CallByKey = ISNULL(@CallBy, CallByKey)
--and IsCallActivate = 1 AND ISNULL(cp.IsDelete, 0) = 0 
GROUP BY TradeType  
 
select @NotActive = Count(1) From #filterCallPerformance -- where CallDate BETWEEN @StartDate AND @EndDate  and IsCallActivate = 1 
select @c2c = count(1) from #filterCallPerformance where exitPrice = EntryPrice 
--CallDate BETWEEN @StartDate AND @EndDate  and IsCallActivate = 1 and 


 --SELECT 
	--@TotalCalls as TotalCalls , 
	--(ISNULL(@SuccessBuyCalls,0) + ISNULL(@SuccessSellCalls,0)) as SuccessCalls , @ClosedCalls as ClosedCalls, 
	--(ISNULL(@StopLossBuyCalls,0) + ISNULL(@StopLossSellCalls,0)) as StopLossCalls , @OpenCalls as OpenCalls , @NotActive  as NotActive, 
	--@Gar as Gar , 0 AS TargetRange , @c2c as C2C
	--into #tempCustomer
	--SELECT (SELECT * FROM #tempCustomer	FOR JSON AUTO) as JsonData

DROP TABLE IF EXISTS #tempTargets

;with cte as(
SELECT 
	 Id,
	-- TradeType, CallDate , EntryPrice, 
	--Target1Price,Target2Price,Target3Price,
	--StopLossPrice	,EntryTime	,TriggerTime	,ExitPrice	,ResultHigh , CallStatus ,
	( 
		CASE 
			WHEN  TradeType = 'Buy' and ExitPrice <= StopLossPrice then 'SL'
			WHEN  ExitPrice = EntryPrice then 'C2C'
			WHEN  TradeType = 'Buy' and ExitPrice < EntryPrice and ExitPrice > StopLossPrice  THEN 'Loss'
			WHEN  TradeType = 'Buy' and ExitPrice > EntryPrice AND ExitPrice <  Target1Price	THEN 'ProfitWithNoTarget'
			WHEN  TradeType = 'Buy' AND ExitPrice >= Target1Price and ExitPrice < isnull(Target2Price ,Target1Price +1)   THEN 'T1'
			WHEN  TradeType = 'Buy' AND ExitPrice >= Target1Price and ExitPrice >= Target2Price  AND ExitPrice < Target3Price  THEN 'T2'
			WHEN  TradeType = 'Buy' AND ExitPrice >= Target1Price and ExitPrice >= Target2Price  and ExitPrice >= Target3Price  THEN 'T3'
			WHEN  TradeType = 'Buy' AND Target3Price IS NOT NULL and ExitPrice >= Target3Price  THEN 'T3'
			WHEN  TradeType = 'Buy' AND Target2Price IS NOT NULL and ExitPrice >= Target2Price AND ExitPrice < ISNULL(Target3Price, Target2Price + 1) THEN 'T2'
		 

			WHEN  TradeType = 'SELL' and ExitPrice >= StopLossPrice then 'SL'
			WHEN  TradeType = 'SELL' and ExitPrice > EntryPrice and ExitPrice < StopLossPrice  THEN 'Loss'
			WHEN  TradeType = 'Sell' and EntryPrice > ExitPrice AND ExitPrice >  Target1Price	THEN 'ProfitWithNoTarget'
			WHEN  TradeType = 'Sell' AND ExitPrice <= Target1Price AND ExitPrice > ISNULL(Target2Price ,Target1Price -1)  THEN 'T1'
			WHEN  TradeType = 'Sell' AND ExitPrice <= Target2Price AND ExitPrice > ISNULL(Target3Price,Target2Price -1)  THEN 'T2'
			WHEN  TradeType = 'Sell' AND Target3Price IS NOT NULL AND ExitPrice <= Target3Price THEN 'T3'

			ELSE 'INVALID'
		END
	) AS Targets
	
FROM #filterCallPerformance  --and TradeType = 'Buy'
)
SELECT Targets ,  count(1) as Counts INTO #tempTargets From cte Group By Targets ORDER BY Targets 

DROP TABLE IF EXISTS #tempFinalJson

--SELECT ( 
	SELECT 
		@TotalCalls as TotalCalls , 
		(ISNULL(@SuccessBuyCalls,0) + ISNULL(@SuccessSellCalls,0)) as SuccessCalls , @ClosedCalls as ClosedCalls, 
		(ISNULL(@StopLossBuyCalls,0) + ISNULL(@StopLossSellCalls,0)) as StopLossCalls , @OpenCalls as OpenCalls , @NotActive  as NotActive, 
		@Gar as Gar , 0 AS TargetRange , ISNULL(@c2c,0) as C2C ,  ISNULL(Invalid,0) AS Invalid, ISNULL(Loss,0) AS Loss, ISNULL(ProfitWithNoTarget,0) AS ProfitWithNoTarget , ISNULL(SL,0) AS SL, ISNULL(T1,0) AS T1, ISNULL(T2,0) AS T2, ISNULL(T3,0) AS T3
	INTO #tempFinalJson
	FROM #tempTargets
	PIVOT
	(  
	  AVG(COUNTS) FOR Targets IN ( Invalid, Loss,ProfitWithNoTarget,SL,T1,T2,T3 )
	) AS PivotTable                                   
--) AS JsonData

SELECT ( SELECT * FROM #tempFinalJson FOR JSON AUTO ) as JsonData

END  