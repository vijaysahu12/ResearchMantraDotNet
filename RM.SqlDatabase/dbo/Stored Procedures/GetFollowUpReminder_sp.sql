CREATE PROCEDURE GetFollowUpReminder 
	@Param1 varchar(100)
AS BEGIN

SELECT follow.Id , CAST( le.AssignedTo  as uniqueidentifier) as AssignedTo , le.FullName ,   follow.Comments, follow.NotifyDate, follow.IsActive  
FROM FollowUPReminders as follow 
INNER JOIN Leads as le on le.PublicKey = follow.LeadKey
INNER JOIN Users as us on le.assignedTo = us.PublicKey and le.assignedTo is not null 
WHERE ISNULL(IsActive,0) = 1 AND CAST(NotifyDate  as date) = cast( GETDATE() AS DATE)
END 
