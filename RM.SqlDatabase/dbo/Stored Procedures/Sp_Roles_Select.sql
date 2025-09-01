CREATE PROC [dbo].[Sp_Roles_Select]
	--@Id int
AS

SELECT	Id,
	 [Name]
      ,[Description]
      ,[IsDisabled]
      ,[IsDelete]
      ,[PublicKey]
      ,[CreatedOn]
      ,[CreatedBy]
      ,[ModifiedOn]
      ,[ModifiedBy]
FROM	Roles
--WHERE 	Id = @Id



