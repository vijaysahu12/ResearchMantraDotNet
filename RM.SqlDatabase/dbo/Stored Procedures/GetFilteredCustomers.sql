CREATE PROCEDURE   [dbo].[GetPurchaseOrders] 
	@IsPaging bit = 1, 
	@PageSize int= 10,
	@PageNumber int = 1,
	@SortExpression varchar(50),
	@SortOrder varchar(20),
	@RequestedBy varchar(100) = null,
	@SearchText varchar(100) = null,
	@FromDate datetime = null,
	@ToDate datetime =null,
	@ServiceId int = null,
	@PrimaryKey int = null
AS
BEGIN
	
	declare @POStatus int =cast(isnull( @PrimaryKey ,null) AS INT ) 

	If @searchText = '' set @searchText = null
	If @RequestedBy = '' set @RequestedBy = null
	If @ServiceId = '0' set @RequestedBy = null
	

	SELECT 
		PO.Id
		,PO.LeadId
		,PO.ClientName
		,PO.Mobile
		,PO.Email
		,PO.DOB
		,ISNULL(PO.Remark,'') AS Remark
		,CAST(PO.PaymentDate as smalldatetime ) as PaymentDate
		,PO.ModeOfPayment
		,ISNULL(PO.BankName,'') AS BankName
		,ISNULL(PO.Pan,'')AS Pan
		,ISNULL(PO.State,'') State
		,PO.City
		,ISNULL(PO.TransactionRecipt,'') AS TransactionRecipt
		,ISNULL(PO.TransasctionReference, '') AS TransasctionReference
		,PO.ServiceId
		,ISNULL(PO.Service,'') AS Service
		,isnull(PO.NetAmount,0) as NetAmount
		,PO.PaidAmount
		,PO.Status
		,CAST(PO.PaymentDate as smalldatetime ) as CreatedOn
		,cast( PO.ActionBy  as varchar(50)) as ActionBy
		,CreatedUser.FirstName as  CreatedBy  
		,st.Name as StatusName
		,ser.Name as ServiceName
		,PO.StartDate as StartDate
		,PO.EndDate as EndDate
		,PO.PublicKey
		
		 
	FROM PurchaseOrders  as PO 
	INNER JOIN Users as CreatedUser on PO.CreatedBy = CreatedUser.PublicKey
	INNER JOIN PaymentModes as pm on po.ModeOfPayment = pm.Id
	INNER JOIN Services as ser on ser.id = po.ServiceId
	INNER JOIN Status as st on st.id = po.Status
		WHERE
	(
		PO.Mobile LIKE '%' + @SearchText +'%' OR
		@SearchText IS NULL OR
		PO.Email LIKE '%'+ @SearchText +'%' OR 
		PO.ClientName LIKE '%'+ @SearchText +'%'
	) 
		AND (PO.Status = ISNULL(@POStatus,PO.Status) OR Coalesce(@POStatus,'') = '')

	AND (PO.ServiceId = ISNULL(@ServiceId,PO.ServiceId) OR Coalesce(@ServiceId,'') = '')
	AND cast(PO.CreatedOn as date)  BETWEEN cast(@FromDate as date)   AND  cast(@ToDate as date)
ORDER BY
PO.Status
--   CASE
--        WHEN @SortOrder <> 'asc' then cast(NULL as date)
--        WHEN @SortExpression = 'CreatedOn' then PO.CreatedOn
--        END ASC
--,	CASE WHEN @sortOrder <> 'desc' then cast(null as date)
--         WHEN @SortExpression = 'CreatedOn' then PO.CreatedOn
--         END DESC
END



