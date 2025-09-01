--exec GetCallPerformance 1,25,1,'','',null,null,'28-04-2023','08-05-2023',null
CREATE PROCEDURE [dbo].[GetCallPerformanceExcel]
 @IsPaging bit = 1,   
 @PageSize int =25,   
 @PageNumber int =1,
 @SortExpression varchar(50) = '',
 @SortOrder varchar(50) = '',  
 @RequestedBy varchar(50)= null,
 @SearchText varchar(100)= null,
 @FromDate DateTime= null,
 @ToDate DateTime= null,
 @StrategyKey varchar(50) = null,
 @TotalCount INT = 0 OUTPUT
AS BEGIN


SELECT @TotalCount = Count(1)
FROM CallPerformance as cp 
INNER JOIN Segments as seg  on cp.SegmentKey = seg.PublicKey
INNER JOIN Users as us on cp.CallByKey = us.PublicKey
INNER JOIN Stocks as sto on sto.PublicKey = cp.StockKey
INNER JOIN Strategies as stra on stra.PublicKey = cp.StrategyKey
WHERE ISNULL(cp.IsDelete, 0) = 0 
AND cp.CallByKey = ISNULL(@RequestedBy, cp.CallByKey) 
AND (sto.Name LIKE '%' +    ISNULL(@SearchText, sto.Name) +'%' OR Coalesce(@SearchText,'') = '')
AND cp.StrategyKey = ISNULL(@StrategyKey, cp.StrategyKey)
and (cp.CallDate >= cast(@FromDate as smalldatetime)  AND cp.CallDate <= cast(@ToDate as smalldatetime))
AND (us.FirstName = @RequestedBy OR Coalesce(@RequestedBy,'') = '') 

SELECT  
Cast(ROW_NUMBER() OVER(ORDER BY cp.CreatedOn desc) as int) AS Id
,cp.TradeType
,cast(cp.CallDate as smalldatetime) as CallDate
,stra.Name as StrategyName
,sto.Name as StockName
,cp.LotSize
,seg.Name as SegmentName
,isnull(cp.OptionValue , '') as OptionValue
,cp.EntryPrice
,cp.StopLossPrice
,ISNULL(cp.Target1Price, 0.0) AS Target1Price
,ISNULL(cp.Target2Price,0.0 ) AS Target2Price
,ISNULL(cp.Target3Price,0.0) AS Target3Price
,cp.CallStatus
,isnull(cp.ExitPrice ,0.0) as ExitPrice
,isnull(cp.ResultHigh , 0.0) as ResultHigh
,(

	CASE 
		WHEN CP.TradeType = 'BUY' THEN ISNULL(((ISNULL(CP.ExitPrice , CP.EntryPrice ) - CP.EntryPrice ) * cp.lotSize),0) 
		WHEN CP.TradeType = 'SELL' THEN ISNULL(((CP.EntryPrice - CP.ExitPrice ) * cp.lotSize),0) 
		ELSE 'OK'
	END

) AS Pnl
, (

	CASE
		WHEN CP.TradeType = 'BUY' AND ((CP.ExitPrice )) = CP.EntryPrice THEN '1:checkedGreen'
		WHEN CP.TradeType = 'BUY' AND ((CP.ExitPrice )) BETWEEN CP.EntryPrice AND CP.Target1Price THEN '2:checkedGreen'
		WHEN CP.TradeType = 'BUY' AND ((CP.ExitPrice )) BETWEEN CP.Target1Price AND CP.Target2Price THEN '3:checkedGreen'
		WHEN CP.TradeType = 'BUY' AND ((CP.ExitPrice  )) BETWEEN CP.Target2Price AND CP.Target3Price THEN '4:checkedGreen'
		WHEN CP.TradeType = 'BUY' AND ((CP.ExitPrice )) > CP.Target3Price THEN '5:v'
		WHEN CP.TradeType = 'BUY' AND ((CP.ExitPrice )) < CP.EntryPrice THEN '1:checkedRed'
		

		WHEN CP.TradeType = 'SELL' AND ((CP.ExitPrice )) = CP.EntryPrice THEN '1:checkedGreen'
		WHEN CP.TradeType = 'SELL' AND ((CP.ExitPrice )) BETWEEN CP.EntryPrice AND CP.Target1Price THEN '2:checkedGreen'
		WHEN CP.TradeType = 'SELL' AND ((CP.ExitPrice )) BETWEEN CP.Target1Price AND CP.Target2Price THEN '3:checkedGreen'
		WHEN CP.TradeType = 'SELL' AND ((CP.ExitPrice  )) BETWEEN CP.Target2Price AND CP.Target3Price THEN '4:checkedGreen'
		WHEN CP.TradeType = 'SELL' AND ((CP.ExitPrice )) < CP.Target3Price THEN '5:checkedGreen'
		WHEN CP.TradeType = 'SELL' AND ((CP.ExitPrice )) > CP.EntryPrice THEN '1:checkedRed'
		


		ELSE '1:GREEN'
	END

) as   Raiting
,cp.Remarks
,us.FirstName as CreatedBy
,isnull(cp.IsPublic,0) as IsPublic
,isnull(cp.ResultTypeKey , '') as ResultTypeKey 
,cp.StrategyKey
,ISNULL(cp.IsIntraday,0) AS IsIntraday
,cp.StockKey
,cp.SegmentKey
,cp.ExpiryKey 
,cp.CallByKey
,cp.PublicKey
,cp.CreatedOn
FROM CallPerformance as cp 
INNER JOIN Segments as seg  on cp.SegmentKey = seg.PublicKey
INNER JOIN Users as us on cp.CallByKey = us.PublicKey
INNER JOIN Stocks as sto on sto.PublicKey = cp.StockKey
INNER JOIN Strategies as stra on stra.PublicKey = cp.StrategyKey
WHERE 
(sto.Name LIKE '%' +    ISNULL(@SearchText, sto.Name) +'%' OR Coalesce(@SearchText,'') = '')
and cp.StrategyKey = ISNULL(@StrategyKey, cp.StrategyKey)
and (  CONVERT(date,  cp.CallDate) >=  CONVERT(date, @FromDate) AND  CONVERT(date, cp.CallDate) <=  CONVERT(date,  @ToDate))
AND cp.CallByKey   = isnull( @RequestedBy , cp.CallByKey) --OR Coalesce(@RequestedBy,'') = '') 
AND ISNULL(cp.IsDelete, 0) = 0 
ORDER BY 
	case
        when @SortOrder <> 'asc' then cast(null as date)
        when @SortExpression = 'CallDate' then CP.CallDate
        end ASC
,   case
        when @SortOrder <> 'asc' then ''
        when @SortExpression = 'TradeType' then CP.TradeType
        end ASC
,	case when @sortOrder <> 'desc' then ''
         when @SortExpression = 'TradeType' then CP.TradeType
         end DESC
,	case
        when @SortOrder <> 'desc' then cast(null as date)
        when @SortExpression = 'CallDate' then CP

.CallDate
        end DESC
,   case
        when @SortOrder <> 'asc' then ''
        when @SortExpression = 'CreatedOn' then CP.CreatedOn
        end ASC
,	case when @sortOrder <> 'desc' then ''
         when @SortExpression = 'CreatedOn' then CP.CreatedOn
         end DESC



END 
 