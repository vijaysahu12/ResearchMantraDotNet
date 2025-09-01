CREATE PROCEDURE [dbo].[UnTochedLeadsAssignedToNull]
AS BEGIN 
	 
	DECLARE @GracePeriod int = 15
	;WITH  PurchaseOrderss as (
		select LeadId, max(StartDate)as StartDate , (
		select top 1 EndDate from PurchaseOrders as p where p.LeadId = pp.LeadId order by p.EndDate desc ) as EndDate 
		from PurchaseOrders  as pp
		Group by LeadId
	), AllActiveService as ( 
		SELECT 
			le.Id, le.FullName,  
			(
				CASE WHEN CAST( po.EndDate AS DATE) > CAST( le.ModifiedOn AS DATE)
					 THEN CAST( po.EndDate AS DATE)
					 ELSE CAST( le.ModifiedOn AS DATE)
				END
 			) as 
			FinalDate,
			(DATEDIFF(day,    GETDATE() , isnull( po.EndDate , le.ModifiedOn)) + @GracePeriod ) as DateDifference
		FROM Leads as le 
		INNER JOIN PurchaseOrderss as po on le.Id = po.LeadId
		WHERE  le.AssignedTo is not null and 
		po.EndDate IS NOT NULL 
		--and (DATEDIFF(day,    GETDATE() , isnull( po.EndDate , le.ModifiedOn)) + @GracePeriod )  = 0
	), AllNonActiveService as (
		SELECT 
			le.Id, le.FullName,  CAST( le.ModifiedOn AS DATE) as FinalDate ,
			(DATEDIFF(day,    GETDATE() , isnull( po.EndDate , le.ModifiedOn)) + @GracePeriod ) as DateDifference
		FROM Leads as le 
		INNER JOIN PurchaseOrderss as po on le.Id = po.LeadId
		WHERE le.AssignedTo is not null and (DATEDIFF(day,    GETDATE() , isnull( po.EndDate , le.ModifiedOn)) + @GracePeriod )  = 0
	) , FinalList as (
		SELECT * FROM AllNonActiveService 
		UNION 
		SELECT * FROM AllActiveService 
	) SELECT * FROM FinalList 
	 WHERE DateDifference = 0 

 END 