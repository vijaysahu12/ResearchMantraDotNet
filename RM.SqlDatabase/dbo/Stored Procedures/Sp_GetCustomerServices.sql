
Create PROC [dbo].[Sp_GetCustomerServices] 
@CustomerKey varchar(100)
AS
Begin

Select a.ID, a.CustomerKey,b.Name as ServiceKey, b.ServiceCost as AmountPaid, c.Name as PaymentModeKey, a.IsWon,
a.OrderId,a.Remarks,
 a.IsDisabled
           ,a.IsDelete
           ,a.PublicKey
           ,a.CreatedOn
           ,a.CreatedBy
           ,a.ModifiedOn
           ,a.ModifiedBy 
           from CustomerServices a , 
           Services b, 
           PaymentModes c 
           where 
           a.CustomerKey = @CustomerKey and 
           a.IsDelete = 0 and 
           a.PaymentModeKey = c.PublicKey and 
           a.ServiceKey = b.PublicKey
           
--Select * from CustomerServices  
--Select * from Services
End


