 
--EXEC GetCallsTopPerformersReports 
CREATE PROCEDURE [dbo].[GetCallsTopPerformersReports]
@StartDate Date = null,
@EndDate Date = null,
@CallBy varchar(50) = null-- '8DD53404-F2CD-ED11-8110-00155D23D79C'
as begin

	
	 IF(@CallBy = '')
	 BEGIN
		SET @CallBy = null
	 END

	SET @StartDate = ISNULL(@StartDate, CAST( dateadd(day,-30,GetDate() )  AS date) )
	SET @EndDate =   ISNULL(@EndDate, CAST(GetDate()  AS date))

	;with cte as (
	
	select 
	users.publickey as UserKey, CP.StrategyKey, (users.FirstName + ' ' + users.LastName ) as Name ,
	ISNULL(ROI,0) AS ROI , '' as Accuracy, 
	(
		CASE 
			WHEN CP.TradeType = 'BUY' THEN ISNULL(((ISNULL(CP.ExitPrice , CP.EntryPrice ) - CP.EntryPrice ) * cp.lotSize),0) 
			WHEN CP.TradeType = 'SELL' THEN ISNULL(((CP.EntryPrice - CP.ExitPrice ) * cp.lotSize),0) 
			ELSE 'OK'
		END
	) AS Pnl , 1 as TotalCalls , CP.PlottedCapital ,'' as Counts 
	From Users as users 
	INNER JOIN CallPerformance as CP on users.PublicKey = CP.CallByKey AND
    CAST(CallDate AS DATE) BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE) and IsCallActivate = 1 and ISNULL(cp.IsDelete,0) = 0
	AND CallStatus = 'Closed'


	), GetTotalCalls as (
		SELECT cte.UserKey, max(Name) as Name, count(TotalCalls) as TotalCalls from cte Group By cte.UserKey 
	), GetTotalRoi as (
		SELECT cte.UserKey, max(Name) AS Name, sum(ROI) AS TotalRoi FROM cte Group By cte.UserKey  
	), GetStrategyCountTemp as (
		SELECT distinct cte.UserKey, cte.Name , StrategyKey  from cte  
	), GetStrategyCount  as (
		select UserKey, Name, Count(StrategyKey) as StrategyCount From GetStrategyCountTemp 
		group by UserKey, Name
	), GetPnl as (
		SELECT 
		CP.CallByKey,
		sum(pnl) AS Pnl From CallPerformance as CP 
		where CallDate between @StartDate and @EndDate and CallStatus = 'Closed'  and IsCallActivate = 1
		group by cp.CallByKey
	), TotalCapitalDeploy as (
		SELECT cte.UserKey, max(Name) AS Name, SUM(PlottedCapital) AS TotalCapital FROM cte Group By cte.UserKey  
	), AverageCapitalDeploy as (
		SELECT cte.UserKey, max(Name) AS Name, AVG(PlottedCapital) AS AverageCapital FROM cte Group By cte.UserKey  
	), TotalProfitCalls as (
		SELECT CallByKey , Count(1) as TotalProfitCalls  FROM CallPerformance	
		WHERE CallDate BETWEEN @StartDate AND @EndDate AND CallStatus = 'Closed' and IsCallActivate = 1 AND CallByKey = ISNULL(@CallBy, CallByKey)
		AND ( (TradeType = 'Buy'  and EntryPrice < ExitPrice) OR (TradeType = 'Sell' AND EntryPrice > ExitPrice ))
		GROUP BY CallByKey 
	) 

	SELECT GTC.*, GTR.TotalRoi, GSC.StrategyCount , GP.PNL , TCD.TotalCapital , ACD.AverageCapital ,
	cast(((TPC.TotalProfitCalls * 100) / GTC.TotalCalls) as decimal) as Accuracy 
	FROM  GetTotalCalls as GTC
	INNER JOIN GetTotalRoi as GTR on GTC.UserKey = GTR.UserKey
	INNER JOIN GetStrategyCount as GSC on GTC.UserKey = GSC.UserKey
	INNER JOIN GetPnl as GP on GP.CallByKey = GTC.UserKey
	inner join TotalProfitCalls TPC on TPC.CallByKey = GTC.UserKey
	inner join TotalCapitalDeploy TCD on TCD.UserKey = GTC.UserKey
	inner join AverageCapitalDeploy ACD on ACD.UserKey = GTC.UserKey
	order by Accuracy desc 
 




------For Testing Purpose  
------DECLARE 
------	@StartDate datetime = '2023-04-1',
------	@EndDate datetime = '2023-04-14',
------	@UserKey varchar(50) = '91EE36E5-2BD6-ED11-8111-00155D23D79C'

--------SELECT TradeType, CallStatus , EntryPrice, ExitPrice, StopLossPrice  FROM CallPerformance 
--------where TradeType = 'Buy' and  CallStatus = 'closed' and EntryPrice > ExitPrice 
--------and CallDate between @StartDate and @EndDate


------SELECT TradeType, EntryPrice, ExitPrice, Pnl, LotSize, *  FROM 
------CallPerformance 
------WHERE CallDate between @StartDate and @EndDate and CallStatus = 'Closed' and CallByKey = @UserKey


--------SELECT sum(Pnl) FROM 
--------CallPerformance 
--------WHERE CallDate between @StartDate and @EndDate and CallStatus = 'Closed' and CallByKey = @UserKey
--------Group By CallByKey


------SELECT count(1) as TotalCalls  FROM 
------CallPerformance 
------WHERE CallDate between @StartDate and @EndDate and CallStatus = 'Closed' and CallByKey = @UserKey


------SELECT sum(roi) as ROI   FROM 
------CallPerformance 
------WHERE CallDate between @StartDate and @EndDate and CallStatus = 'Closed' and CallByKey = @UserKey


------SELECT count(distinct StrategyKey) as StrategyKey    FROM 
------CallPerformance 
------WHERE CallDate between @StartDate and @EndDate and CallStatus = 'Closed' and CallByKey = @UserKey


------SELECT sum(Pnl) as Pnl    FROM 
------CallPerformance 
------WHERE CallDate between @StartDate and @EndDate and CallStatus = 'Closed' and CallByKey = @UserKey

end 
 