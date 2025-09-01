CREATE PROC [dbo].[Sp_GetServices] 
--@ServiceId int
AS
Begin
--Select * from services 
Select a.ID, a.Name,a.Description, a.ServiceCost, b.Name as ServiceTypeKey, c.Name as ServiceCategoryKey, a.IsDisabled
           ,a.IsDelete
           ,a.PublicKey
           ,a.CreatedOn
           ,a.CreatedBy
           ,a.ModifiedOn
           ,a.ModifiedBy 
           from Services a , ServiceTypes b, ServiceCategories c where a.ServiceTypeKey = b.PublicKey and a.ServiceCategoryKey = c.PublicKey and a.IsDelete = 0 
End



