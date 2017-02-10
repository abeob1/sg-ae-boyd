CREATE PROCEDURE [dbo].[sp_AE_GenerateXML]
@DocEntry numeric(18,0)
as
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
		ARInvoiceXMLData
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
		ARInvoiceXMLData
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
		ARInvoiceXMLData
	where
		DocEntry = @DocEntry
),
member as 
(
	select Top 1
		memberName 
		,memVATNumber		
	from 
		ARInvoiceXMLData
	where
		DocEntry = @DocEntry
),
invoiceFrom as
(
	select top 1
		invfromName
		,invfromContactName
		,invfromEmail
		,invfromTelephone
		,invfromFaxNo
		,invfromClubReference 	
	from 
		ARInvoiceXMLData
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
		ARInvoiceXMLData
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
		ARInvoiceXMLData
	where
		DocEntry = @DocEntry
),
bank as
(
	select Top 1
		bankName
	from 
		ARInvoiceXMLData
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
		ARInvoiceXMLData
	where
		DocEntry = @DocEntry	
),
invoiceSummary as
(
	select
		invsumTotalFee = Convert(Decimal(10,2),(Round(SUM(invsumTotalFee),2)))
		,invsumTotalDisbursements = Convert(Decimal(10,2),(Round(SUM(invsumTotalDisbursements),2)))
		,invsumTotalTaxes = Convert(Decimal(10,2),(Round(SUM(invsumTotalTaxes),2)))
		,invsumTotalThirdPartyFee  = Convert(Decimal(10,2),(Round(SUM(invsumTotalThirdPartyFee),2)))
		,invsumTotalThirdPartyDisbursements= Convert(Decimal(10,2),(Round(SUM(invsumTotalThirdPartyDisbursements),2)))
		,invsumTotalThirdPartyTaxes  = Convert(Decimal(10,2),(Round(SUM(invsumTotalThirdPartyTaxes),2)))
		,invsumAmountPayable = Convert(Decimal(10,2),(Round(MAX(invsumAmountPayable),2)))	
	from 
		ARInvoiceXMLData
	where
		DocEntry = @DocEntry
	Group by DocEntry		
),
fee as 
(
	select
		paymentsFeeDate = CONVERT(char(10), paymentsFeeDate,126)
		,paymentsContractor
		,paymentsUnit = Convert(Decimal(10,2),(Round(paymentsUnit,2)))
		,paymentsCost = Convert(Decimal(10,2),(Round(paymentsCost,2)))
		,paymentsQuantity = Convert(Decimal(10,2),(Round(paymentsQuantity,2)))
		,paymentsAmount = Convert(Decimal(10,2),(Round(paymentsAmount,2)))
		,paymentsWork 
		,thirdpartypaymentsFeeDate = CONVERT(char(10), thirdpartypaymentsFeeDate,126)
		,thirdpartypaymentsContractor
		,thirdpartypaymentsUnit = Convert(Decimal(10,2),(Round(thirdpartypaymentsUnit,2)))
		,thirdpartypaymentsCost = Convert(Decimal(10,2),(Round(thirdpartypaymentsCost,2)))
		,thirdpartypaymentsQuantity = Convert(Decimal(10,2),(Round(thirdpartypaymentsQuantity,2)))
		,thirdpartypaymentsAmount = Convert(Decimal(10,2),(Round(thirdpartypaymentsAmount,2)))	
		,thirdpartypaymentsWork
	from 
		ARInvoiceXMLData			
	where
		DocEntry = @DocEntry			
),
disbursements as 
(
	select 
		paymentsDisbursementType
		,paymentsDisbursementComments 
		,paymentsDisbursementAmount = Convert(Decimal(10,2),(Round(paymentsDisbursementAmount,2)))	
	from 
		ARInvoiceXMLData			
	where
		DocEntry = @DocEntry			
),
taxes as 
(
	select
		paymentTotalLocal = Convert(Decimal(10,2),(Round(SUM(paymentTotalLocal),2)))
		,paymentTotalVAT = Convert(Decimal(10,2),(Round(SUM(paymentTotalVAT),2)))
		,paymentTotal = Convert(Decimal(10,2),(Round(SUM(paymentTotal),2)))		
	from 
		ARInvoiceXMLData			
	where
		DocEntry = @DocEntry
)
/*----------------GENERATE XML STRING---------------*/
SELECT '2' as '@version', invoiceNumber AS '@invoiceNumber', invDate AS '@date', invCoveredFrom AS '@coveredFrom', invCoveredTo AS '@coveredTo', 
	   case invFinalInvoice when 'Y' then 'True' else 'False' end AS '@finalInvoice', invArchiveDate AS '@archiveDate', 
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
		(select (select paymentsFeeDate AS '@Date', paymentsContractor AS '@contractor', paymentsUnit AS '@unit', paymentsCost AS '@cost', 
						   paymentsQuantity AS '@quantity', paymentsAmount AS '@amount', paymentsWork AS '@work'  from fee for  xml path('fee'), root('fees'), type),
						(select paymentsDisbursementType AS '@type', paymentsDisbursementComments AS '@comments', paymentsDisbursementAmount as '@amount' from disbursements for  xml path('disbursement'), root('disbursements'), type),
						(select paymentTotalLocal as 'totalLocal', paymentTotalVAT as 'totalVAT', paymentTotal as 'total' from taxes for  xml auto, type)
			for xml path('payments'), type), 
--===================================================================================================================================================
-- thirdpartypayments ===============================================================================================================================
		(select (select  null FOR XML PATH('fees'), type), 
								  (select null for xml path('disbursements'), type),
								  (select paymentTotalLocal as 'totalLocal', paymentTotalVAT as 'totalVAT', paymentTotal as 'total' from taxes for xml auto, type )  
		for xml path('thirdPartyPayments'), type),
--===================================================================================================================================================
-- Elements empty ===================================================================================================================================
		(select '' AS otherInformation, (select null for xml path('AttachedFileDetails'), type) 
					  for xml path(''), type)
--===================================================================================================================================================

 FROM invoice FOR XML PATH('Invoice'), ELEMENTS XSINIL 
				  


