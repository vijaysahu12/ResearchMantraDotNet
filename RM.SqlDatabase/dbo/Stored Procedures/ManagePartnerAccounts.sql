CREATE PROCEDURE [dbo].[ManagePartnerAccounts]
	@PartnerAccountID  int = 0, 
	@FullName varchar(100)
	,@MobileNumber varchar(15) = null,@EmailId varchar(100)= null,@City varchar(100)= null,@TelegramId varchar(500)= null
	,@ZerodhaCId varchar(100)= null,@AngelCId varchar(100)= null,@AliceBlueCId varchar(100)= null,@EdelweissCId varchar(100)= null
	,@FyersCId varchar(100)= null 
	,@MotilalCId varchar(100) = null
	,@Comments varchar(300) = null
	,@CreatedBy varchar(50)= null
	,@CreatedIpAddress varchar(100)= null, @Status int  = 1 ,@Brokerage float
	,@Source varchar(50) 
	,@AssignedTo int = null
AS 
BEGIN
	 
	BEGIN TRY 
		
		IF EXISTS (select 1 from PartnerAccounts where MobileNumber = @MobileNumber)
		BEGIN
			print 1 
			select  @PartnerAccountID = Id from PartnerAccounts where MobileNumber = @MobileNumber
		end 
		IF(ISNULL(@PartnerAccountID,0) != 0  )
		BEGIN
	
			UPDATE PartnerAccounts 
			SET 
				ModifiedBy = @CreatedBy, 
				ModifiedOn = GetDate(),
				ZerodhaCId = ISNULL(@ZerodhaCId ,ZerodhaCId) ,
				AliceBlueCId = ISNULL(@AliceBlueCId , AliceBlueCId) ,
				AngelCId = ISNULL(@AngelCId , AngelCId) ,
				FyersCId = ISNULL(@FyersCId , FyersCId) ,
				EdelweissCId = ISNULL(@EdelweissCId , EdelweissCId),
				MotilalCId = ISNULL(@MotilalCId , MotilalCId),
				[Status] = @Status ,
				Brokerage = @Brokerage,
				Source = @Source	,
				AssignedTo = @AssignedTo
			WHERE Id = @PartnerAccountID 
			
			IF (ISNULL(@Comments,'') != '')
			Begin
			
                                INSERT INTO PartnerAccountActivities (PartnerAccountDetailId, Comments, CreatedOn , CreatedBy)
                                VALUES      (0, @Comments , GetDate(), @CreatedBy)
			END
		END ELSE 
		BEGIN 
			INSERT INTO PartnerAccounts (
			 FullName,MobileNumber,EmailId,City,TelegramId,ZerodhaCId,AngelCId,AliceBlueCId,EdelweissCId
			,FyersCId, MotilalCId ,Remarks, CreatedIpAddress,CreatedOn
			,CreatedBy,ModifiedOn,ModifiedBy,Status , Brokerage, Source,AssignedTo) values 
			(
				 @FullName, @MobileNumber, @EmailId, @City, @TelegramId, @ZerodhaCId, @AngelCId, @AliceBlueCId, @EdelweissCId
				,@FyersCId, @MotilalCId , @Comments, @CreatedIpAddress, GETDATE()
				,@CreatedBy,GETDATE(), @CreatedBy, 1 , isnull(@Brokerage ,0.0), @Source , @AssignedTo
			)
			
			SET @PartnerAccountID = SCOPE_IDENTITY()
			
			IF (ISNULL(@Comments,'') != '')
			Begin
                                INSERT INTO PartnerAccountActivities (PartnerAccountDetailId, Comments, CreatedOn , CreatedBy)
                                VALUES      (0, @Comments , GetDate(), @CreatedBy)
			END
			
		END
		SELECT 1 as Result
	END TRY 
	BEGIN CATCH 
		SELECT -1 as Result
	END CATCH 
END






