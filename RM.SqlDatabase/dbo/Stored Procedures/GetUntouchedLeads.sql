CREATE PROCEDURE [dbo].[GetUntouchedLeads]
AS BEGIN

DECLARE @ExpiryDays int , @AdminRole varchar(50) = (SELECT value from Settings where Code = 'admin') 

SELECT @ExpiryDays =  value from Settings where Code = 'untouched'

	SELECT 
		CAST([PublicKey] AS uniqueidentifier) AS PublicKey
		,[FullName]
		,Cast(AssignedTo AS uniqueidentifier) AS AssignedTo 
		,cast((DATEDIFF(day, ModifiedOn, GETDATE())) AS INT) AS DateDiff
		, 'Kindly follow up with ' + FullName as Comments
		, Cast(@AdminRole AS uniqueidentifier)as AdminRole
	FROM [KingResearch].[dbo].[Leads]
	WHERE 
	DATEDIFF(day, ModifiedOn, GETDATE()) > @ExpiryDays AND
	 AssignedTo IS NOT NULL 
	AND ISNULL(IsDisabled ,0 ) = 0  
	AND ISNULL(IsDelete ,0 ) = 0 

	--UPDATE 
	--	Leads 
	--SET 
	--AssignedTo = null , 
	--ModifiedOn = GETDATE(),
	--ModifiedBy = (SELECT Value FROM  Settings Where Code = 'admin')
	--WHERE DATEDIFF(day, ModifiedOn, GETDATE()) > @ExpiryDays
END