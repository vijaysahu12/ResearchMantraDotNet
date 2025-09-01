CREATE PROCEDURE [dbo].[GetFilterUsersBy] 
@userType VARCHAR(50)
AS BEGIN
	SELECT 
		users.Id, users.PublicKey, FirstName , users.LastName , UserMappings.UserType 
	FROM Users INNER JOIN UserMappings
	ON users.PublicKey = UserMappings.UserKey where UserMappings.UserType = @userType
	ORDER BY FirstName ASC
END