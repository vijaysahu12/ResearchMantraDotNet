CREATE PROCEDURE [dbo].[GetCallPerformance]
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
WHERE (sto.Name LIKE '%' +    ISNULL(@SearchText, sto.Name) +'%' OR Coalesce(@SearchText,'') = '')
and cp.StrategyKey = ISNULL(@StrategyKey, cp.StrategyKey)
and (  CONVERT(date,  cp.CallDate) >=  CONVERT(date, @FromDate) AND  CONVERT(date, cp.CallDate) <=  CONVERT(date,  @ToDate))
AND cp.CallByKey   = isnull( @RequestedBy , cp.CallByKey) --OR Coalesce(@RequestedBy,'') = '') 
AND ISNULL(cp.IsDelete, 0) = 0 

;with cte as (

SELECT  
Cast(ROW_NUMBER() OVER(ORDER BY cp.CreatedOn desc) as int) AS Id
,cp.TradeType
,cast(cp.CallDate as smalldatetime) as CallDate
,stra.Name as StrategyName
,sto.Name as StockName
,cp.LotSize
,seg.Name as SegmentName
,ISNULL(cp.OptionValue , '') as OptionValue
,ISNULL(cp.Target2Price,0.0 ) AS Target2Price
,ISNULL(cp.Target3Price,0.0) AS Target3Price
,cp.CallStatus
,isnull(cp.StopLossPrice, 0.0) as StopLossPrice
,cp.EntryPrice
,ISNULL(cp.ExitPrice ,0.0) AS ExitPrice
,ISNULL(cp.Target1Price, 0.0) AS Target1Price
,IIF(cp.TradeType = 'BUY',  
	(IIF( cp.ExitPrice > cp.Target1Price , cp.Target1Price ,(cp.ExitPrice  ))), 
	(IIF(
			cp.ExitPrice < cp.Target1Price, cp.Target1Price,(
				 cp.ExitPrice )
			)
		
	)
	) AS FinalExit 
,ISNULL(cp.ResultHigh , 0.0) as ResultHigh
,cp.Remarks
,us.FirstName as CreatedBy
,isnull(cp.IsPublic,0) as IsPublic
,isnull(cp.ResultTypeKey , '') as ResultTypeKey 
,cp.StrategyKey
,ISNULL(cp.IsIntraday,0) AS IsIntraday
,cp.StockKey
,cp.SegmentKey
,isnull(cp.ExpiryKey , '') as ExpiryKey
,isnull(cp.CallByKey , '')  as CallByKey
,cp.PublicKey
,cp.CreatedOn
FROM CallPerformanceTemp as cp 
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

)SELECT  
 Id
,cp.TradeType
,CallDate
,StrategyName
,StockName
,LotSize
,SegmentName
,OptionValue
,cp.EntryPrice
,StopLossPrice
,Target1Price
,Target2Price
,Target3Price
,cp.CallStatus
,ExitPrice
--,FinalExit
,ResultHigh
,(

	CASE 
		WHEN TradeType = 'BUY' THEN ISNULL(((ISNULL(CP.FinalExit , CP.EntryPrice ) - CP.EntryPrice ) * cp.lotSize),0) 
		WHEN TradeType = 'SELL' THEN ISNULL(((CP.EntryPrice - FinalExit ) * cp.lotSize),0) 
		ELSE 'OK'
	END

)AS Pnl ,(
	CASE
		WHEN CP.TradeType = 'BUY' AND  ((CP.FinalExit )) = CP.EntryPrice THEN '1:checkedGreen'
		WHEN CP.TradeType = 'BUY' AND  ((CP.FinalExit )) BETWEEN CP.EntryPrice AND CP.Target1Price THEN '2:checkedGreen'
		WHEN CP.TradeType = 'BUY' AND  ((CP.FinalExit )) BETWEEN CP.Target1Price AND CP.Target2Price THEN '3:checkedGreen'
		WHEN CP.TradeType = 'BUY' AND  ((CP.FinalExit )) BETWEEN CP.Target2Price AND CP.Target3Price THEN '4:checkedGreen'
		WHEN CP.TradeType = 'BUY' AND  ((CP.FinalExit )) > CP.Target3Price THEN '5:v'
		WHEN CP.TradeType = 'BUY' AND  ((CP.FinalExit )) < CP.EntryPrice THEN '1:checkedRed'
		WHEN CP.TradeType = 'SELL' AND ((CP.FinalExit )) = CP.EntryPrice THEN '1:checkedGreen'
		WHEN CP.TradeType = 'SELL' AND ((CP.FinalExit )) BETWEEN CP.EntryPrice AND CP.Target1Price THEN '2:checkedGreen'
		WHEN CP.TradeType = 'SELL' AND ((CP.FinalExit )) BETWEEN CP.Target1Price AND CP.Target2Price THEN '3:checkedGreen'
		WHEN CP.TradeType = 'SELL' AND ((CP.FinalExit )) BETWEEN CP.Target2Price AND CP.Target3Price THEN '4:checkedGreen'
		WHEN CP.TradeType = 'SELL' AND ((CP.FinalExit )) < CP.Target3Price THEN '5:checkedGreen'
		WHEN CP.TradeType = 'SELL' AND ((CP.FinalExit )) > CP.EntryPrice THEN '1:checkedRed'
		ELSE '1:GREEN'
	END
)AS Raiting 
,cp.Remarks
,CreatedBy
,IsPublic
,ResultTypeKey 
,cp.StrategyKey
,IsIntraday
,cp.StockKey
,cp.SegmentKey
,ExpiryKey
,cp.CallByKey
,cp.PublicKey
,cp.CreatedOn
 FROM cte as cp 

ORDER BY 
	case
        when @SortOrder <> 'asc' then cast(null as date)
        when @SortExpression = 'CallDate' then CP.CallDate
        end ASC
,   case
        when @SortOrder <> 'asc' then ''
        when @SortExpression = 'TradeType' 
		then CP.TradeType end ASC
,	case when @sortOrder <> 'desc' then ''
         when @SortExpression = 'TradeType' then CP.TradeType
         end DESC
,	case
        when @SortOrder <> 'desc' then cast(null as date)
        when @SortExpression = 'CallDate' 
		then CP.CallDate
        end DESC
,   case
        when @SortOrder <> 'asc' then ''
        when @SortExpression = 'CreatedOn' then CP.CreatedOn
        end ASC
,	case when @sortOrder <> 'desc' then ''
         when @SortExpression = 'CreatedOn' 
		 then CP.CreatedOn end DESC


OFFSET (IIF(@PageNumber <= 1 , 0 ,(@PageNumber-1))) * @PageSize ROWS
FETCH NEXT @PageSize ROWS ONLY
END

--pnl cacluate based on target 1 
------;with cte as (


------SELECT  
------ cp.Id
------,cp.TradeType
------,cast(cp.CallDate as smalldatetime) as CallDate
------,stra.Name as StrategyName
------,sto.Name as StockName
------,cp.LotSize
------,seg.Name as SegmentName
------,ISNULL(cp.OptionValue , '') as OptionValue
------,ISNULL(cp.Target2Price,0.0 ) AS Target2Price
------,ISNULL(cp.Target3Price,0.0) AS Target3Price
------,cp.CallStatus
------,isnull(cp.StopLossPrice, 0.0) as StopLossPrice
------,cp.EntryPrice
------,ISNULL(cp.ExitPrice ,0.0) AS ExitPrice
------,ISNULL(cp.Target1Price, 0.0) AS Target1Price
------,IIF(cp.TradeType = 'BUY',  
------	(IIF( cp.ExitPrice > cp.Target1Price , cp.Target1Price ,(cp.ExitPrice  ))), 
------	(IIF(
------			cp.ExitPrice < cp.Target1Price, cp.Target1Price,(
------				 cp.ExitPrice )
------			)
		
------	)
------	) AS FinalExit 
------,ISNULL(cp.ResultHigh , 0.0) as ResultHigh
------,cp.Remarks
------,us.FirstName as CreatedBy
------,isnull(cp.IsPublic,0) as IsPublic
------,isnull(cp.ResultTypeKey , '') as ResultTypeKey 
------,cp.StrategyKey
------,ISNULL(cp.IsIntraday,0) AS IsIntraday
------,cp.StockKey
------,cp.SegmentKey
------,isnull(cp.ExpiryKey , '') as ExpiryKey
------,cp.CallByKey
------,cp.PublicKey
------,cp.CreatedOn
------FROM CallPerformanceTemp as cp 
------INNER JOIN Segments as seg  on cp.SegmentKey = seg.PublicKey
------INNER JOIN Users as us on cp.CallByKey = us.PublicKey
------INNER JOIN Stocks as sto on sto.PublicKey = cp.StockKey
------INNER JOIN Strategies as stra on stra.PublicKey = cp.StrategyKey


------)SELECT  
------ Id
------,cp.TradeType
------,CallDate
------,StrategyName
------,StockName
------,LotSize
------,SegmentName
------,OptionValue
------,cp.EntryPrice
------,StopLossPrice
------,Target1Price
------,Target2Price
------,Target3Price
------,cp.CallStatus
------,ExitPrice
------,FinalExit
------,ResultHigh
------,(

------	CASE 
------		WHEN TradeType = 'BUY' THEN ISNULL(((ISNULL(CP.FinalExit , CP.EntryPrice ) - CP.EntryPrice ) * cp.lotSize),0) 
------		WHEN TradeType = 'SELL' THEN ISNULL(((CP.EntryPrice - FinalExit ) * cp.lotSize),0) 
------		ELSE 'OK'
------	END

------)AS Pnl ,(
------	CASE
------		WHEN CP.TradeType = 'BUY' AND  ((CP.FinalExit )) = CP.EntryPrice THEN '1:checkedGreen'
------		WHEN CP.TradeType = 'BUY' AND  ((CP.FinalExit )) BETWEEN CP.EntryPrice AND CP.Target1Price THEN '2:checkedGreen'
------		WHEN CP.TradeType = 'BUY' AND  ((CP.FinalExit )) BETWEEN CP.Target1Price AND CP.Target2Price THEN '3:checkedGreen'
------		WHEN CP.TradeType = 'BUY' AND  ((CP.FinalExit )) BETWEEN CP.Target2Price AND CP.Target3Price THEN '4:checkedGreen'
------		WHEN CP.TradeType = 'BUY' AND  ((CP.FinalExit )) > CP.Target3Price THEN '5:v'
------		WHEN CP.TradeType = 'BUY' AND  ((CP.FinalExit )) < CP.EntryPrice THEN '1:checkedRed'
------		WHEN CP.TradeType = 'SELL' AND ((CP.FinalExit )) = CP.EntryPrice THEN '1:checkedGreen'
------		WHEN CP.TradeType = 'SELL' AND ((CP.FinalExit )) BETWEEN CP.EntryPrice AND CP.Target1Price THEN '2:checkedGreen'
------		WHEN CP.TradeType = 'SELL' AND ((CP.FinalExit )) BETWEEN CP.Target1Price AND CP.Target2Price THEN '3:checkedGreen'
------		WHEN CP.TradeType = 'SELL' AND ((CP.FinalExit )) BETWEEN CP.Target2Price AND CP.Target3Price THEN '4:checkedGreen'
------		WHEN CP.TradeType = 'SELL' AND ((CP.FinalExit )) < CP.Target3Price THEN '5:checkedGreen'
------		WHEN CP.TradeType = 'SELL' AND ((CP.FinalExit )) > CP.EntryPrice THEN '1:checkedRed'
------		ELSE '1:GREEN'
------	END
------)AS Raiting 
------,cp.Remarks
------,CreatedBy
------,IsPublic
------,ResultTypeKey 
------,cp.StrategyKey
------,IsIntraday
------,cp.StockKey
------,cp.SegmentKey
------,ExpiryKey
------,cp.CallByKey
------,cp.PublicKey
------,cp.CreatedOn
------into #tempTableVijay
------FROM cte as cp 


 
 

-- declare @p13 int
--set @p13=37
--exec sp_executesql N'EXEC GetCallPerformance @IsPaging ,@PageSize, @PageNumber  , @SortExpression , @SortOrder  , @RequestedBy , @SearchText , @FromDate, @ToDate , @StrategyKey , @TotalCount OUTPUT
--',N'@IsPaging bit,@PageSize int,@PageNumber  int,@SortExpression varchar(50),@SortOrder varchar(50),@RequestedBy varchar(100),
--@SearchText varchar(100),@FromDate datetime,@ToDate datetime,@StrategyKey varchar(120),@TotalCount int output',@IsPaging=0,@PageSize=20,
-- @PageNumber =1,@SortExpression=NULL,@SortOrder=NULL,@RequestedBy='cc083981-2bd6-ed11-8111-00155d23d79c',@SearchText=NULL,@FromDate='2023-07-01 00:00:00',@ToDate='2023-08-03 00:00:00',@StrategyKey=NULL,@TotalCount=@p13 output
--select @p13

--select 
--(19.20 -
--16.00) * 1250


 