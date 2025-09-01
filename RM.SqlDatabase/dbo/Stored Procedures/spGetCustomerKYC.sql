--exec spGetCustomerKYC null

CREATE PROCEDURE [dbo].[spGetCustomerKYC]
--@status AS INT,
@SearchText as varchar(100)
AS BEGIN
	SELECT 
	 kyc.Id 
	,LeadKey 
	,l.[PublicKey]   
	,FullName 
	,MobileNumber 
	,EmailId 
	,PAN 
	,PANURL 
	,kyc.Remarks 
	,l.ProfileImage 
	,kyc.Status 
	,kyc.Verified 
	,kyc.CreatedOn
	,kyc.ModifiedOn 
	FROM [dbo].[CustomerKYC] AS kyc 
	INNER JOIN Leads AS l 
		ON kyc.LeadKey = kyc.LeadKey
	WHERE 
	--kyc.Verified = @status and 
	l.FullName like '%' + isnull(@SearchText, l.FullName)+'%'
END 
