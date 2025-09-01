CREATE PROCEDURE [dbo].[GetExpiredServices] 
@ExpiredInDays varchar(100) = '1,2,4'
as begin



DROP TABLE IF EXISTS  #TempEmailTemplate	 
SELECT 
		 leads.FullName, leads.EmailId as Receiver, cast( users.PublicKey AS uniqueidentifier) as userkey, '' as CC , service.Name as SubscriptionName , purchaseOrders.PaidAmount , purchaseOrders.StartDate, purchaseOrders.EndDate , 
		 ISNULL(DATEDIFF(DAY, CAST(GETDATE() AS DATE) , CAST(purchaseOrders.EndDate AS DATE)),0) as DaysToGo , emailTe.Name, emailTe.Body, emailTe.Subject  
INTO #TempEmailTemplate		 
FROM Leads AS leads 
INNER JOIN Users as users on leads.AssignedTo = users.PublicKey
LEFT JOIN PurchaseOrders AS purchaseOrders ON leads.Id = purchaseOrders.LeadId
LEFT JOIN LeadTypes as LeadTypes on LeadTypes.PublicKey = Leads.LeadTypeKey
INNER JOIN Status as status on status.Id = purchaseOrders.Status
INNER JOIN Services as service on purchaseOrders.ServiceId = service.Id
CROSS JOIN EmailTemplates as emailTe
WHERE leads.AssignedTo IS NOT NULL 
AND emailTe.Name = 'SubscriptionExpired'
AND purchaseOrders.Status = (select Id from Status where name = 'CUSTOMER') 
AND 
(
	ISNULL(DATEDIFF(DAY, CAST(GETDATE() AS DATE) , CAST(purchaseOrders.EndDate AS DATE)),0) in (SELECT VALUE FROM string_split('1,3,5' ,','))
)
and ISNULL(DATEDIFF(DAY, CAST(GETDATE() AS DATE) , CAST(purchaseOrders.EndDate AS DATE)),0) >= 0
 
 

UPDATE #TempEmailTemplate SET Body = REPLACE(cast(Body as varchar(max)), '{{RECEIVER}}', FullName)  
UPDATE #TempEmailTemplate SET Body = REPLACE(cast(Body as varchar(max)), '{{subscriptionName}}', SubscriptionName)  
UPDATE #TempEmailTemplate SET Body = REPLACE(cast(Body as varchar(max)), '{{days}}', DaysToGo)  
select * From #TempEmailTemplate 
DROP TABLE IF EXISTS  #TempEmailTemplate	 

END
 