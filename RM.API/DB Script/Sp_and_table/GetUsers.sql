CREATE PROCEDURE [dbo].[GetUsers]
	 @SearchText varchar(100) = null
AS BEGIN

	declare @TotalCount int 

	SET @SearchText = ISNULL(@SearchText,'')
	SELECT @TotalCount = count(1) FROM users as us LEFT JOIN roles as r on r.publickey = us.rolekey 
	WHERE us.firstname LIKE '%' + @SearchText +'%' OR 
	us.lastname LIKE '%' + @SearchText +'%' OR 
	us.emailid LIKE '%' + @SearchText +'%' OR 
	us.mobilenumber LIKE '%' + @SearchText +'%'
		
	SELECT (
		SELECT  
			ROW_NUMBER() OVER ( ORDER BY r.name ) SlNo, 
			FirstName, LastName, 
			MobileNumber, EmailId, us.PublicKey, 
			isnull(r.name, '') as RoleName, us.IsDisabled
		FROM Users as us 
		LEFT JOIN Roles as r on r.Publickey = us.rolekey 
		WHERE 
		us.firstname	LIKE '%' + @SearchText +'%' OR 
		us.lastname		LIKE '%' + @SearchText +'%' OR 
		us.emailid		LIKE '%' + @SearchText +'%' OR 
		us.mobilenumber LIKE '%' + @SearchText +'%' 
		ORDER BY r.name FOR JSON AUTO
	) AS JsonData , isnull(@TotalCount,0)  as TotalCount
END
