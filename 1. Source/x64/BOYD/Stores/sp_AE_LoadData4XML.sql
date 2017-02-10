select 
	T0.DocEntry
--------invoice table-------------
	,invoiceNumber = IsNull(T0.DocNum,'')
	,invDate = T0.DocDate 
	,invCoveredFrom = IsNull(T0.U_AE_PDateFr ,'')
	,invCoveredTo = IsNull(T0.U_AE_PDateTo,'')
	,invFinalInvoice = IsNull(T0.U_AE_FinalInv ,'')
	,invArchiveDate = getdate()  --what is sent date?
--------invoice to table------------
	,invtoName = T0.CardName
	,invtoID = T0.CardCode
	,invtoTo = IsNull(T0.U_AE_Member ,'')
	,invtoContactName = IsNull(T1.Name ,'')
	,invtoEmail = IsNull(T1.E_MailL ,'')
	,invtoClubReference = IsNull(T0.U_AE_YourRef ,'')
	,invtoAlias = IsNull(T2.CardFName,'')
	,invtoVATNumber = IsNull(T2.LicTradNum ,'')
	--Address
	,invtoLine1 = IsNull(T3.Street ,'')
	,invtoLine2 = IsNull(T3.StreetNo ,'')
	,invtoLine3 = IsNull(T3.Block ,'')
	,invtoLine4 = IsNull(T3.City,'') +' '+ IsNull(T3.ZipCode,'') +' '+ IsNull(T3.Country,'')
-------member table-------------
	,memberName = IsNull(T0.U_AE_Member,'')
	,memVATNumber = ''
	--Address
	,memLine1 = IsNull(T0.U_AE_MAdd1,'')
	,memLine2 = IsNull(T0.U_AE_MAdd2,'')
	,memLine3 = IsNull(T0.U_AE_MAdd3,'')
	,memLine4 = IsNull(T0.U_AE_MAdd4,'')
-------invoice from-------------
	,invfromName = IsNull(T4.CompnyName,'')
	,invfromContactName = IsNull(T4.manager,'')
	,invfromEmail = IsNull(T4.e_mail,'')
	,invfromTelephone = IsNull(T4.Phone1,'')
	,invfromFaxNo =IsNull( T4.Fax,'')
	,invfromClubReference = IsNull(T0.U_AE_YourRef,'')
	--Address
	,invfromLine1 = IsNull(T5.Street,'')
	,invfromLine2 = IsNull(T5.StreetNo,'')
	,invfromLine3 = IsNull(T5.Block,'')
	,invfromLine4 = IsNull(T5.City,'') +' '+ IsNull(T5.ZipCode,'') +' '+ IsNull(T5.Country,'')
------invoiceInformation ------------
	,invinfoName = IsNull(T0.U_AE_VesselName,'') 
	,invinfoVoyageNumber = IsNull(T0.U_AE_VoyageNo,'')
	,invinfoIncidentDate = IsNull(T0.U_AE_DIncident ,'')
	,invinfoDischargePort = IsNull(T0.U_AE_PIncident ,'')
	,invinfoDescription = IsNull(T0.U_AE_Case ,'')
	,invinfoCurrency = IsNull(T0.DocCur ,'')
-----payee -------------------
	,payeeName = IsNull(T4.CompnyName,'')
	,payeeRegistration = IsNull(T4.RevOffice,'')
	,payeeVATNumber = IsNull(T4.TaxIdNum,'')
	--Address
	,payeeLine1 = IsNull(T5.Street,'')
	,payeeLine2 = IsNull(T5.StreetNo,'')
	,payeeLine3 = IsNull(T5.Block,'')
	,payeeLine4 = IsNull(T5.City,'') +' '+ IsNull(T5.ZipCode,'') +' '+ IsNull(T5.Country,'')
-----Bank -------------------
	,bankName = IsNull(T7.BankName,'')
	,bankLine1 = IsNull(T8.Street,'')
	,bankLine2 = IsNull(T8.StreetNo,'')
	,bankLine3 = IsNull(T8.Block,'')
	,bankLine4 = IsNull(T8.City,'')
-----Account -------------------	
	,AccountName = IsNull(T4.CompnyName,'')
	,Number = IsNull(T8.Account,'')
	,SortCode = '-'
	,SwiftCode = IsNull(T8.SwiftNum,'')
-----Invoice Summary -------------------		
	,invsumTotalFee = IsNull(T6.LineTotal,0)
	,invsumTotalDisbursements = IsNull(T6.LineTotal,0)
	,invsumTotalTaxes = IsNull(T6.VatSum,0)
	,invsumTotalThirdPartyFee  = IsNull(T6.LineTotal,0)
	,invsumTotalThirdPartyDisbursements = 0.0
	,invsumTotalThirdPartyTaxes  = IsNull(T6.VatSum,0)
	,invsumAmountPayable = IsNull(T0.DocTotal,0)
-----Payments -------------------			
	--Fees
	,paymentsFeeDate = T0.DocDate
	,paymentsContractor = IsNull(T6.Dscription,'')
	,paymentsUnit = IsNull(T9.InvntryUom,'')
	,paymentsCost = IsNull(T6.Price,0)
	,paymentsQuantity = IsNull(T6.Quantity,0)
	,paymentsAmount = IsNull(T6.LineTotal,0)
	,paymentsWork = IsNull(T6.Text,'')
	--Disbursements
	,paymentsDisbursementType =  IsNull(T6.Dscription,'')
	,paymentsDisbursementComments = IsNull(T6.Text,'')
	,paymentsDisbursementAmount = IsNull(T6.LineTotal ,0)
	--Taxes
	,paymentTotalLocal = IsNull(T6.VatSum,0)
	,paymentTotalVAT = IsNull(T6.VatSum,0)
	,paymentTotal = IsNull(T6.VatSum,0)
-----ThirdPartyPayments -------------------			
	--Fees
	,thirdpartypaymentsFeeDate = T0.DocDate
	,thirdpartypaymentsContractor = IsNull(T6.Dscription,'')
	,thirdpartypaymentsUnit = IsNull(T9.InvntryUom,'')
	,thirdpartypaymentsCost = IsNull(T6.Price,0)
	,thirdpartypaymentsQuantity = IsNull(T6.Quantity,0)
	,thirdpartypaymentsAmount = IsNull(T6.LineTotal,0)
	,thirdpartypaymentsWork = IsNull(T6.Text,'')
	--Taxes
	,thirdpartypaymentTotalLocal = IsNull(T6.VatSum,0)
	,thirdpartypaymentTotalVAT = IsNull(T6.VatSum,0)
	,thirdpartypaymentTotal = IsNull(T6.VatSum,0)
from 
	OINV T0
	left join OCPR T1 on T0.CntctCode=T1.CntctCode
	join OCRD T2 on T2.CardCode=T0.CardCode
	left join CRD1 T3 on T3.CardCode=T0.CardCode and T3.Address=T0.PayToCode
	join OADM T4 on 1=1
	join ADM1 T5 on 1=1
	left join INV1 T6 on T6.DocEntry = t0.DocEntry
	join (Select top 1 BankCode, BankName from ODSC) T7 on 1 = 1
	left join DSC1 T8 on T8.BankCode = T7.BankCode and T8.County = T0.DocCur
	left join OITM T9 on T9.ItemCode = T6.ItemCode
