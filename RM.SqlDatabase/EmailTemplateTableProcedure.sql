
CREATE TABLE [dbo].[EmailTemplates](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NULL,
	[Subject] [varchar](255) NULL,
	[Body] [text] NULL,
	[CreatedOn] [datetime] NULL,
	[UpdatedOn] [datetime] NULL,
	[ModifiedBy] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[EmailTemplates] ON 
GO
INSERT [dbo].[EmailTemplates] ([Id], [Name], [Subject], [Body], [CreatedOn], [UpdatedOn], [ModifiedBy]) VALUES (1, N'SubscriptionExpired', N'Subscription About To expired', N'Hello {{receiver}} , Your Subscription {{subscriptionName}} going to expire in {{days}}, Please renew to avoid the important updates.', CAST(N'2023-05-17T10:58:17.313' AS DateTime), CAST(N'2023-05-17T10:58:17.313' AS DateTime), N'BB74D26F-AA28-EB11-BEE5-00155D53687A')
GO
SET IDENTITY_INSERT [dbo].[EmailTemplates] OFF
GO


GO
--exec GetExpiredServices  
CREATE PROCEDURE GetExpiredServices 
@ExpiredInDays varchar(200) = '1,2,4'
as begin



DROP TABLE IF EXISTS  #TempEmailTemplate	 
SELECT 
		 leads.FullName, leads.EmailId as Receiver, '' as CC , service.Name as SubscriptionName , purchaseOrders.PaidAmount , purchaseOrders.StartDate, purchaseOrders.EndDate , 
		 ISNULL(DATEDIFF(DAY, CAST(purchaseOrders.StartDate AS DATE) , CAST(purchaseOrders.EndDate AS DATE)),0) as DaysToGo , emailTe.Name, emailTe.Body, emailTe.Subject  
into #TempEmailTemplate		 
FROM Leads AS leads 
INNER JOIN Users as users on leads.AssignedTo = users.PublicKey
LEFT JOIN PurchaseOrders AS purchaseOrders ON leads.Id = purchaseOrders.LeadId
LEFT JOIN LeadTypes as LeadTypes on LeadTypes.PublicKey = Leads.LeadTypeKey
INNER JOIN Status as status on status.Id = purchaseOrders.Status
inner join Services as service on purchaseOrders.ServiceId = service.Id
cross join EmailTemplates as emailTe
WHERE leads.AssignedTo IS NOT NULL 
AND emailTe.Name = 'SubscriptionExpired'
AND purchaseOrders.Status = (select Id from Status where name = 'CUSTOMER') 
AND 
(
	ISNULL(DATEDIFF(DAY, CAST(purchaseOrders.StartDate AS DATE) , CAST(purchaseOrders.EndDate AS DATE)),0) in (SELECT VALUE FROM string_split(@ExpiredInDays ,','))
)
 
 

UPDATE #TempEmailTemplate SET Body = REPLACE(cast(Body as varchar(max)), '{{RECEIVER}}', FullName)  
UPDATE #TempEmailTemplate SET Body = REPLACE(cast(Body as varchar(max)), '{{subscriptionName}}', '"'+ SubscriptionName + '"')  
UPDATE #TempEmailTemplate SET Body = REPLACE(cast(Body as varchar(max)), '{{days}}', DaysToGo)  
SELECT * FROM #TempEmailTemplate 
DROP TABLE IF EXISTS  #TempEmailTemplate	 

END



