--exec GetPurchaseOrdersByLead '4B19EE07-1A01-EE11-8119-00155D23D79C'
CREATE PROCEDURE GetPurchaseOrdersByLead
@LeadPublicKey uniqueidentifier ,
@TotalCount INT OUTPUT
AS BEGIN 
	
		DROP TABLE IF EXISTS #tempPoList
		
		SELECT le.PublicKey, p.PaymentDate, pm.Name as Mop , p.ServiceID, ser.Name as ServiceName, p.PaidAmount , p.StartDate , p.EndDate   
		INTO #tempPoList
		FROM PurchaseOrders as p 
		INNER JOIN Leads as le on le.Id = p.LeadId 
		Inner Join PaymentModes as pm on p.ModeOfPayment = pm.Id 
		Inner Join Services as ser on ser.Id = p.ServiceId 
		WHERE le.PublicKey = @LeadPublicKey 
		order by p.EndDate

		select @TotalCount = count(1) from #tempPoList 
		
		SELECT (SELECT * FROM #tempPoList FOR JSON AUTO) as JsonData


end