

create procedure [dbo].[GetPushNotification]
(
	@PageSize int,
	@PageNumber int,
	@RequestedBy varchar(100) = null,
	@TotalUnreadCount int =0 output 
)
AS BEGIN
DECLARE @offset INT
DECLARE @newsize INT

IF(@PageNumber=0)
begin
   SET @offset = @PageNumber;
   SET @newsize = @PageSize
end
ELSE 
begin
    SET @offset = @PageNumber+1;
    SET @newsize = @PageSize-1
end
-- SET NOCOUNT ON added to prevent extra result sets from
SET NOCOUNT ON;
WITH OrderedSet AS
(
  SELECT Id, UserKey, [Message] , ReadDate, IsRead, IsActive, IsImportant,
      ROW_NUMBER() OVER (ORDER BY CreatedDate  DESC) AS 'Index'
  FROM [dbo].[PushNotifications] where Userkey = @RequestedBy
)
  SELECT (SELECT * 
   FROM OrderedSet 
   WHERE [Index] BETWEEN @offset AND (@offset + @newsize) FOR JSON AUTO) as JsonData

    set @TotalUnreadCount = (SELECT COUNT(*) FROM [dbo].[PushNotifications] where Userkey = @RequestedBy and isnull(IsRead, 0) = 0)
	--select @TotalUnreadCount
END