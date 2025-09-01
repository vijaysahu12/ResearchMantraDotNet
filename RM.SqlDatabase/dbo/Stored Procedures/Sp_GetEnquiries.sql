CREATE PROCEDURE Sp_GetEnquiries
	@RequestedBy varchar(50)
AS BEGIN 
	SELECT 
		en.Id, en.Details ,  le.FullName as LeadName, en.ReferenceKey , u.FirstName + ' ' + u.LastName as CreatedBy , en.PublickEy, en.CreatedOn ,
		ISNULL(le.Favourite , 0) AS Favourite
		FROM Enquiries as en 
	INNER JOIN Leads as le on en.ReferenceKey = le.publickEy 
	INNER JOIN Users as u on u.PublicKey = en.CreatedBy
	where le.CreatedBy = @RequestedBy
	ORDER BY le.Favourite desc 
 END