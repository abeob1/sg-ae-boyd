/****** Object:  StoredProcedure [dbo].[sp_AE_GenerateXML]    Script Date: 10/24/2013 15:23:27 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_AE_GenerateXML]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_AE_GenerateXML]
GO
/****** Object:  StoredProcedure [dbo].[sp_AE_GenerateXML]    Script Date: 10/24/2013 15:23:27 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_AE_GenerateXML]
@DocEntry numeric(18,0),
@value  nvarchar(max) OUTPUT
as
Declare @taxes as table(paymentTotalLocal numeric(10,2), paymentTotalVAT numeric(10,2) , paymentTotal numeric(10,2))
insert into @taxes select 0.00, 0.00, 0.00

Declare @XML as xml;

with invoice as
(
	select Top 1
		invoiceNumber 
		,invDate = CONVERT(char(10), invDate,126)
		,invCoveredFrom = CONVERT(char(10), invCoveredFrom,126)
		,invCoveredTo = CONVERT(char(10), invCoveredTo,126)
		,invFinalInvoice
		,invArchiveDate = CONVERT(char(10), GetDate(),126)
	from 
		AE_ARInvoiceXmlData
	where
		DocEntry = @DocEntry
),
address as
(
	select Top 1
		invtoLine1
		,invtoLine2
		,invtoLine3
		,invtoLine4
		,memLine1
		,memLine2
		,memLine3
		,memLine4	
		,invfromLine1
		,invfromLine2
		,invfromLine3
		,invfromLine4
		,payeeLine1
		,payeeLine2
		,payeeLine3
		,payeeLine4
		,bankLine1
		,bankLine2
		,bankLine3
		,bankLine4					
	from 
		AE_ARInvoiceXmlData
	where
		DocEntry = @DocEntry
),
invoiceTo as
(
	select Top 1
		invtoName
		,invtoID 
		,invtoTo 
		,invtoContactName
		,invtoEmail
		,invtoClubReference 
		,invtoAlias
		,invtoVATNumber 
	from 
		AE_ARInvoiceXmlData
	where
		DocEntry = @DocEntry
),
member as 
(
	select Top 1
		memberName 
		,memVATNumber		
	from 
		AE_ARInvoiceXmlData
	where
		DocEntry = @DocEntry
),
invoiceFrom as
(
	select top 1
		invfromName
		,invfromContactName = 'PAULINE/MEGA'
		,invfromEmail
		,invfromTelephone
		,invfromFaxNo
		,invfromClubReference 	
	from 
		AE_ARInvoiceXmlData
	where
		DocEntry = @DocEntry
),
invoiceInformation as 
(
	select Top 1
		invinfoName
		,invinfoVoyageNumber 
		,invinfoIncidentDate = CONVERT(char(10), invinfoIncidentDate,126)
		,invinfoDischargePort
		,invinfoDescription
		,invinfoCurrency	
	from 
		AE_ARInvoiceXmlData
	where
		DocEntry = @DocEntry
),
payee as
(
	select top 1
		payeeName 
		,payeeRegistration
		,payeeVATNumber		
	from 
		AE_ARInvoiceXmlData
	where
		DocEntry = @DocEntry
),
bank as
(
	select Top 1
		bankName
	from 
		AE_ARInvoiceXmlData
	where
		DocEntry = @DocEntry
),
account as
(
	select Top 1
		AccountName
		,Number
		,SortCode
		,SwiftCode
	from 
		AE_ARInvoiceXmlData
	where
		DocEntry = @DocEntry	
),
PreFees as
(
	select
		paymentsFeeDate = CONVERT(char(10), MAX(paymentsFeeDate),126)
		,paymentsContractor =MAX(paymentsContractor)
		,paymentsUnit = '-'--Convert(Decimal(10,2),(Round(paymentsUnit,2)))
		,paymentsCost = Convert(Decimal(10,2),(Round(SUM(paymentsCost),2)))
		,paymentsQuantity = 1--Convert(Decimal(10,2),(Round(SUM(paymentsQuantity),2)))
		,paymentsAmount = Convert(Decimal(10,2),(Round(SUM(paymentsAmount),2)))
		,paymentsWork = MAX(CAST(paymentsWork as nvarchar(max)))
		,thirdpartypaymentsFeeDate = CONVERT(char(10), MAX(thirdpartypaymentsFeeDate),126)
		,thirdpartypaymentsContractor = MAX(thirdpartypaymentsContractor)
		,thirdpartypaymentsUnit = '-'--Convert(Decimal(10,2),(Round(thirdpartypaymentsUnit,2)))
		,thirdpartypaymentsCost = Convert(Decimal(10,2),(Round(SUM(thirdpartypaymentsCost),2)))
		,thirdpartypaymentsQuantity = 1--Convert(Decimal(10,2),(Round(SUM(thirdpartypaymentsQuantity),2)))
		,thirdpartypaymentsAmount = Convert(Decimal(10,2),(Round(SUM(thirdpartypaymentsAmount),2)))	
		,thirdpartypaymentsWork = MAX(CAST(thirdpartypaymentsWork as nvarchar(max)))		 
		,U_AE_BillAs
	from 
		AE_ARInvoiceXmlData	
	where
		DocEntry = @DocEntry
	Group by
		U_AE_BillAs
),
fee as 
(
	select
		paymentsFeeDate 
		,paymentsContractor 
		,paymentsUnit
		,paymentsCost
		,paymentsQuantity
		,paymentsAmount
		,paymentsWork
		
	from 
		PreFees			
	where
		U_AE_BillAs = 'S'
	--Group by
		--ItemCode, paymentsContractor, paymentsFeeDate,CAST(paymentsWork as nvarchar(max))
),
feethirdparty as
(
	select
		thirdpartypaymentsFeeDate
		,thirdpartypaymentsContractor
		,thirdpartypaymentsUnit  
		,thirdpartypaymentsCost 
		,thirdpartypaymentsQuantity 
		,thirdpartypaymentsAmount 
		,thirdpartypaymentsWork
	from 
		PreFees			
	where
		U_AE_BillAs = 'T'
	--Group by thirdpartypaymentsFeeDate, ItemCode, thirdpartypaymentsContractor, CAST(thirdpartypaymentsWork as nvarchar(max))
),
disbursements as 
(
	select 
		paymentsDisbursementType
		,paymentsDisbursementComments 
		,paymentsDisbursementAmount = Convert(Decimal(10,2),(Round(paymentsDisbursementAmount,2)))	
	from 
		AE_ARInvoiceXmlData			
	where
		DocEntry = @DocEntry
		and U_AE_BillAs = 'D'			
),
taxes as 
(
	select
		paymentTotalLocal = Convert(Decimal(10,2),(Round(SUM(paymentTotalLocal),2)))
		,paymentTotalVAT = Convert(Decimal(10,2),(Round(SUM(paymentTotalVAT),2)))
		,paymentTotal = Convert(Decimal(10,2),(Round(SUM(paymentTotal),2)))		
	from 
		AE_ARInvoiceXmlData			
	where
		DocEntry = @DocEntry
		And ItemCode = 'ITM199'
),
invoiceSummary as
(
	select
		invsumTotalFee =  Convert(Decimal(10,2),(select CASE WHEN (select COUNT(*) from fee) > 0 then (Round(SUM(paymentsAmount ),2)) else 0.00 end from fee)) 
		,invsumTotalDisbursements = Convert(Decimal(10,2),(select CASE WHEN (select COUNT(*) from disbursements) > 0 then (Round(SUM( paymentsDisbursementAmount ),2)) else 0.00 end from disbursements))
		,invsumTotalTaxes = Convert(Decimal(10,2),(select CASE WHEN (select COUNT(*) from taxes) > 0 then IsNull((Round(SUM(paymentTotalVAT ),2)),0.00) else 0.00 end from taxes))
		,invsumTotalThirdPartyFee  = Convert(Decimal(10,2),IsNull((select CASE WHEN (select COUNT(*) from feethirdparty) > 0 then (Round(SUM(thirdpartypaymentsAmount ),2)) else 0.00 end from feethirdparty),0))
		,invsumTotalThirdPartyDisbursements= 0.00--Convert(Decimal(10,2),(Round(SUM(invsumTotalThirdPartyDisbursements),2)))
		,invsumTotalThirdPartyTaxes  = 0.00--Convert(Decimal(10,2),(Round(SUM(invsumTotalThirdPartyTaxes),2)))
		,invsumAmountPayable = Convert(Decimal(10,2),(Round(MAX(invsumAmountPayable),2)))	
	from 
		AE_ARInvoiceXmlData
	where
		DocEntry = @DocEntry
	Group by DocEntry		
)
/*----------------GENERATE XML STRING---------------*/
Select @XML = 
(
SELECT '2' as '@version', invoiceNumber AS '@invoiceNumber', invDate AS '@date', invCoveredFrom AS '@coveredFrom', invCoveredTo AS '@coveredTo', 
	   case invFinalInvoice when 'Y' then 'true' else 'false' end AS '@finalInvoice', invArchiveDate AS '@archiveDate', 
-- header ==========================================================================================================================================
       (select (select invtoName AS '@name', invtoID AS '@ID', invtoTo AS '@to', invtoContactName	AS '@contactName', invtoEmail AS '@email', 
									  invtoClubReference AS '@clubReference', invtoAlias AS '@alias', invtoVATNumber AS '@VATNumber',
									  (select invtoLine1 AS '@line1', invtoLine2 AS '@line2', invtoLine3 AS '@line3', invtoLine4 AS '@line4' 
										 from address for xml path('address'), type),
						      (select memberName AS '@name', memVATNumber AS '@VATNumber', 
									  (select memLine1 AS '@line1', memLine2 AS '@line2', memLine3 AS '@line3', memLine4 AS '@line4' 
									     from address for xml path('address'), type)
							      from member for xml path('member'), type)
					   from invoiceTo
					for xml path('invoiceTo'), type), -- invoiceTo
			(select invfromName AS '@name', invfromContactName	AS '@contactName', invfromEmail AS '@email', invfromTelephone AS '@telephone', invfromFaxNo AS '@faxNo', 
							   invfromClubReference AS '@clubReference',
							   (select invfromLine1 AS '@line1', invfromLine2 AS '@line2', invfromLine3 AS '@line3', invfromLine4 AS '@line4' 
								  from address for xml path('address'), type)
						from  invoiceFrom
						for xml path('invoiceFrom'), type), -- invoiceFrom
			(select invinfoName as 'name', invinfoVoyageNumber as 'voyageNumber', invinfoIncidentDate as 'incidentDate' ,
					invinfoDischargePort as 'dischargePort', invinfoDescription as 'description' , invinfoCurrency as 'currency'
				from invoiceInformation for xml auto, type) -- invoiceInformation
	for xml path('header'), type), -- header	
--===================================================================================================================================================
-- payeeDetails =====================================================================================================================================
   (select (select payeeName AS '@name', payeeRegistration	AS '@registration', payeeVATNumber AS '@VATNumber', 
										    (select payeeLine1 AS '@line1', payeeLine2 AS '@line2', payeeLine3 AS '@line3', payeeLine4 AS '@line4' 
											   from address for xml path('address'), type)
									   from payee
									for xml path('payee'), type), -- payee
									(select (select bankName AS '@name',
												    (select bankLine1 AS '@line1', bankLine2 AS '@line2', bankLine3 AS '@line3', bankLine4 AS '@line4' 
													   from address for xml path('address'), type)	
											   from bank for xml path('bank'), type),
											(select AccountName AS '@name', Number AS '@number', SortCode AS '@sortCode', SwiftCode AS '@swiftCode'
											   from account for xml path('account'), type)
									for xml path('remittanceInstructions'), type) -- remittanceInstructions
			for xml path('payeeDetails'), type), 
--===================================================================================================================================================
-- invoiceSummary ===================================================================================================================================
		(select invsumTotalFee as 'totalFee', invsumTotalDisbursements as 'totalDisbursements' , invsumTotalTaxes as 'totalTaxes',
				invsumTotalThirdPartyFee as 'totalThirdPartyFee', invsumTotalThirdPartyDisbursements as 'totalThirdPartyDisbursements',
				invsumTotalThirdPartyTaxes as 'totalThirdPartyTaxes', invsumAmountPayable as 'amountPayable'
			from invoiceSummary for  xml auto, type), 
--===================================================================================================================================================
-- payments =========================================================================================================================================
		(select CASE WHEN (select COUNT(*) from fee) > 0 
				then
					(select paymentsFeeDate AS '@date', paymentsContractor AS '@contractor', paymentsUnit AS '@unit', paymentsCost AS '@cost', 
								paymentsQuantity AS '@quantity', paymentsAmount AS '@amount', paymentsWork AS '@work'  from fee for  xml path('fee'), root('fees'), type)
				else
				   (select null for  xml path('fees'),  type)
				End,
				
				CASE WHEN (select COUNT(*) from disbursements) > 0 
				then
					(select paymentsDisbursementType AS '@type', paymentsDisbursementComments AS '@comments', paymentsDisbursementAmount as '@amount' from disbursements for  xml path('disbursement'), root('disbursements'), type)
				else
					(select null for  xml path('disbursements'), type)
				End,
				
				CASE WHEN (select COUNT(*) from taxes) > 0 
				then
					(select IsNull(paymentTotalLocal,0.00) as 'totalLocal', IsNull(paymentTotalVAT,0.00) as 'totalVAT', IsNull(paymentTotal,0.00) as 'total' from taxes for  xml auto, type)
				else	
					(select paymentTotalLocal as '@totalLocal', paymentTotalVAT as '@totalVAT', paymentTotal as '@total' from @taxes for  xml path('taxes'), type)
				End
			for xml path('payments'), type), 
--===================================================================================================================================================
-- thirdpartypayments ===============================================================================================================================
		(select CASE WHEN (select COUNT(*) from feethirdparty) > 0 
				then 
					(select  thirdpartypaymentsContractor AS '@contractor', thirdpartypaymentsUnit AS '@unit', thirdpartypaymentsCost AS '@cost', 
								thirdpartypaymentsQuantity AS '@quantity', thirdpartypaymentsAmount AS '@amount', thirdpartypaymentsWork AS '@work' ,thirdpartypaymentsFeeDate AS '@date' from feethirdparty for  xml path('fee'), root('fees'), type)
				else
					(select null for  xml path('fees'), type)
				End,
			  (select null for xml path('disbursements'), type),
					(select paymentTotal as '@totalLocal', paymentTotalVAT as '@totalVAT', paymentTotalLocal as '@total' from @taxes for  xml path ('taxes'), type)
		for xml path('thirdPartyPayments'), type),
--===================================================================================================================================================
-- Elements empty ===================================================================================================================================
		(select (select 'Spica Serial No.:' + IsNull(T.U_AE_SpicaSN,'') from OINV T where T.DocEntry = @DocEntry) AS otherInformation, (select null for xml path('AttachedFileDetails'), type) 
					  for xml path(''), type)
--===================================================================================================================================================

 FROM invoice FOR XML PATH('invoice'), ELEMENTS XSINIL 
)
select @value = CONVERT(nvarchar(max),@XML)				  



GO


