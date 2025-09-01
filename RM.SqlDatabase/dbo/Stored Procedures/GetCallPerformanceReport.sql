CREATE PROCEDURE GetCallPerformanceReport
AS BEGIN
	
	--select distinct CallByKey From CallPerformance

SELECT us.FirstName +' '+ us.LastName as Name, count(1) as TotalCalls, sum(pnl) as Pnl ,  MAX(pnl) as MaxPnl, Min(pnl) as MinPnl, MAX(roi) as MaxRoi, Min(Roi) as MinRoi, AVG(roi) as AvgRoi 
FROM CallPerformance as cal
INNER JOIN Users as us on cal.CallByKey = us.PublicKey 
GROUP BY us.FirstName, us.LastName, cal.CallByKey

 

select sum(pnl) TotalProfit, sum(PlottedCapital) CaptialTurnover , max(pnl) as MaxPnl , min(pnl) as MinPnl, count(1) as TotalCalls   From CallPerformance


--select * From update CallPerformance set  where id = 1 
END