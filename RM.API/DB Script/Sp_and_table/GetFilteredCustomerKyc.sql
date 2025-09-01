USE [KingResearchDev]
GO
/****** Object:  StoredProcedure [dbo].[spGetCustomerKYC]    Script Date: 05-09-2023 09:53:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--exec [GetFilteredCustomerKyc] null 
ALTER PROCEDURE [dbo].[GetFilteredCustomerKyc]
--@status AS INT,
 @IsPaging bit = 1,   
 @PageSize int =25, 
 @PageNumber int =1,
 @SortExpression varchar(50) = NULL,
 @SortOrder varchar(50) = NULL,  
 @RequestedBy varchar(50)= null,
 @SearchText varchar(100)= null,
 @FromDate DateTime= null,
 @ToDate DateTime= null,
 @StrategyKey varchar(50) = null,
 @TotalCount INT = 0  OUTPUT
AS BEGIN

SELECT @TotalCount =  COUNT(1) 
FROM PurchaseOrders  as po 
INNER JOIN Leads as le on po.LeadId = le.Id 
WHERE STATUS = (SELECT Id From Status where Code = 'app')

drop table if exists   #tempKycTable 

SELECT 
	ROW_NUMBER() OVER( order by po.Id) AS RowId,
	po.LeadID, po.ClientName, po.Email, po.Mobile,
	se.Name as ServiceName ,
	ISNULL(po.kycApproved , 0) as KycApproved , st.Name  as Status , po.KycApprovedDate 
	into  #tempKycTable
FROM PurchaseOrders  as po 
INNER JOIN Leads as le on po.LeadId = le.Id 
INNER JOIN Services as se on se.id = po.ServiceId
inner join Status as st on st.id = po.Status
WHERE STATUS = (SELECT Id From Status where Code = 'app')
and ISNULL(po.isactive , 0)  = 1 
AND ISNULL(PO.KycApproved , 0) = 0 

select ( SELECT * FROM 	#tempKycTable FOR JSON AUTO) as JsonData
drop table if exists   #tempKycTable 

END



