CREATE PROCEDURE [dbo].[GetDashboard]
AS BEGIN


	DECLARE @FreshPartners int, @PendingPartners int , @AcceptedPartners int, @RejectedPartners int 

	SELECT  @FreshPartners    = count(1) FROM PartnerAccounts WHERE Status = 0 -- Fresh 
	SELECT  @PendingPartners  = count(1) FROM PartnerAccounts WHERE Status = 1 -- Pending
	SELECT  @AcceptedPartners = count(1) FROM PartnerAccounts WHERE Status = 2 -- Accepted 
	SELECT  @RejectedPartners = count(1) FROM PartnerAccounts WHERE Status = 2 -- Rejected 
 

	select @FreshPartners as FreshPartners   , @PendingPartners as PendingPartners , @AcceptedPartners as AcceptedPartners ,
	@RejectedPartners as RejectedPartners 


END  

