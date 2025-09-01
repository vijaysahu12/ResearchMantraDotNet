Create PROC [dbo].[Sp_GetCustomerTags]
@CustomerKey varchar(100)
AS
Begin

Select a.Id, a.CustomerKey,b.Name as TagKey
           ,a.IsDelete
           ,a.PublicKey
           ,a.CreatedOn
           ,a.CreatedBy
           ,a.ModifiedOn
           ,a.ModifiedBy 
           from CustomerTags a , 
           Tags b
           where 
           a.CustomerKey = @CustomerKey and 
           a.IsDelete = 0 and 
           a.TagKey = b.PublicKey
           
--Select * from CustomerServices  
--Select * from Services
End


