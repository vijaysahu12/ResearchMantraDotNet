CREATE PROCEDURE [dbo].[Sp_PullLeadEnquiries]
	@isLead  int =  1 ,
	@ReferenceKey varchar(50)
AS
BEGIN
	SELECT Id ,Details ,IsLead ,ReferenceKey ,IsAdmin ,PublicKey ,CreatedOn ,CreatedBy FROM Enquiries WHERE ReferenceKey =  @ReferenceKey
END


