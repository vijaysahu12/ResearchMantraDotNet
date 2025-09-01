 CREATE PROCEDURE [dbo].[GetEmailTemplate] 
 @EmailTemplateName varchar(100),
 @Category varchar(50),
 @RequestedBy UniqueIdentifier,
 @PurchaseOrderKey UniqueIdentifier
 AS BEGIN
	DECLARE @Body nvarchar(max) = '', @Subject varchar(100) = ''
	
 	SELECT 
	@Body = Body ,@Subject = Subject
	FROM EmailTemplates WHERE NAME = ISNULL(@EmailTemplateName,'SubscriptionExpired')    
	 
	--DROP TABLE IF EXISTS #tempCustomers


	DECLARE @tempCustomers TABLE (FullName varchar(100), EmailId varchar(100) , StartDate datetime, EndDate datetime , DaysToGo int ,ServiceId int, ServiceName varchar(100)  , Body varchar(max), Subject varchar(100));


	IF(ISNULL(@EmailTemplateName,'SubscriptionExpired')    = 'SubscriptionExpired')
	BEGIN
		INSERT INTO @tempCustomers
		SELECT 
		leads.FullName, purchaseOrders.Email as EmailId ,  isnull(purchaseOrders.StartDate,'') as StartDate, isnull(purchaseOrders.EndDate,'') as EndDate,  
		ISNULL(DATEDIFF(DAY, GETDATE() ,purchaseOrders.EndDate),0) as DaysToGo ,services.id as ServiceId, services.Name as ServiceName , @Body as Body, @Subject as Subject 
		 
		FROM Leads AS leads 
		INNER JOIN Users as users on leads.AssignedTo = users.PublicKey
		LEFT JOIN PurchaseOrders AS purchaseOrders ON leads.Id = purchaseOrders.LeadId
		inner join Services as services on purchaseOrders.ServiceId = services.Id
		INNER JOIN Status as status on status.Id = purchaseOrders.Status
		WHERE leads.AssignedTo IS NOT NULL 


		UPDATE @tempCustomers  SET Body = REPLACE(Body, '{{receiver}}', FullName);
		UPDATE @tempCustomers  SET Body = REPLACE(Body, '{{subscriptionName}}', ServiceName);
		UPDATE @tempCustomers  SET Body = REPLACE(Body, '{{days}}', DaysToGo); 
	END
	ELSE IF (ISNULL(@EmailTemplateName,'')    = 'LTC') BEGIN 
		--DROP TABLE IF EXISTS #tempCustomers
		INSERT INTO @tempCustomers
		SELECT 
		leads.FullName LeadName, purchaseOrders.Email as EmailId,  isnull(purchaseOrders.StartDate,'') as StartDate, 
		ISNULL(purchaseOrders.EndDate,'') as EndDate,  
		ISNULL(DATEDIFF(DAY, GETDATE() ,purchaseOrders.EndDate),0) as DaysToGo ,
		services.id as ServiceId, services.Name as ServiceName , @Body as Body, @Subject as Subject 
 		FROM Leads AS leads 
		INNER JOIN Users as users on leads.AssignedTo = users.PublicKey
		LEFT JOIN PurchaseOrders AS purchaseOrders ON leads.Id = purchaseOrders.LeadId
		INNER JOIN Services as services on purchaseOrders.ServiceId = services.Id
		INNER JOIN Status as status on status.Id = purchaseOrders.Status
		WHERE purchaseOrders.PublicKey	= @PurchaseOrderKey
		AND isnull(purchaseOrders.Email , '') <> ''

		UPDATE @tempCustomers  SET Body = REPLACE(Body, '{{receiver}}', FullName);
		UPDATE @tempCustomers  SET Body = REPLACE(Body, '{{CustomerServiceName}}', ServiceName);
		UPDATE @tempCustomers  SET Body = REPLACE(Body, '{{StartDate}}', CAST(StartDate as date ) );
		UPDATE @tempCustomers  SET Body = REPLACE(Body, '{{EndDate}}', CAST(EndDate as date ));
	END
--	INSERT INTO exceptionlogs (ExceptionType
--,ErrorMessage
--,StackTrace
--,Description
--,Notes)
--	SELECT '','', (FullName + ' _ ' + isnull( EmailId , 'prkr@yopmail.com') + ' _ ' + Body + ' _ ' + Subject + ' _ ' + 'kingresearch@yopmail.com' ) , 
--	 '' , ''
--	FROM @tempCustomers 
	
	SELECT FullName , ISNULL( EmailId , 'prkr@yopmail.com') as EmailId , Body, Subject, 'kingresearch@yopmail.com' as Cc  FROM @tempCustomers 
 END

  
