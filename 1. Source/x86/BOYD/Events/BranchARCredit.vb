Public Class BranchARCredit
    Dim WithEvents SBO_Application As SAPbouiCOM.Application
    Dim oCompany As SAPbobsCOM.Company
    Dim oForm As SAPbouiCOM.Form = Nothing
    Sub New(ByVal ocompany1 As SAPbobsCOM.Company, ByVal sbo_application1 As SAPbouiCOM.Application)
        SBO_Application = sbo_application1
        oCompany = ocompany1
    End Sub
    Private Sub SBO_Application_FormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean) Handles SBO_Application.FormDataEvent
        'Dim oForm As SAPbouiCOM.Form = Nothing
        If BusinessObjectInfo.BeforeAction = False Then
            Select Case BusinessObjectInfo.FormTypeEx
                Case "179" 'A/R Credit Memo
                    Select Case BusinessObjectInfo.EventType
                        Case SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD To SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE
                            If Not Functions.CheckHO Then
                                If BusinessObjectInfo.ActionSuccess Then
                                    oForm = SBO_Application.Forms.Item(BusinessObjectInfo.FormUID)
                                    If BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD Then
                                        Add_Update_Documents(oForm.DataSources.DBDataSources.Item(0).GetValue("DocEntry", 0), False)
                                    ElseIf BusinessObjectInfo.EventType = SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE Then
                                        Add_Update_Documents(oForm.DataSources.DBDataSources.Item(0).GetValue("DocEntry", 0), True)
                                    End If
                                End If
                            End If
                    End Select
            End Select
        End If
    End Sub
    Private Sub Add_Update_Documents(DocEntry As String, isUpdate As Boolean)
        Dim oARCredit As SAPbobsCOM.Documents
        Dim ors As SAPbobsCOM.Recordset = Nothing
        Dim errCode As Integer
        Dim errMess As String = ""
        Dim strConnection As Array

        Try
            oARCredit = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes)
            If oARCredit.GetByKey(DocEntry) Then
                Dim sErrMsg As String = ""
                Dim sErrCode As Integer = 0
                Dim isJPI As Boolean = oARCredit.UserFields.Fields.Item("U_AE_JPI").Value = "Y"

                oCompany.StartTransaction()
                Dim holdingCompany = New SAPbobsCOM.Company
                ors = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
                ors.DoQuery("Select * from [@HOLDINGS]")
                ors.MoveFirst()
                While Not ors.EoF
                    strConnection = ors.Fields.Item("U_AE_Connection").Value.ToString.Split(";")
                    If (Functions.ConnectToCompany(strConnection, holdingCompany) <> 0) Then
                        Throw New Exception(Functions.ErrorMessage)
                    End If
                    If holdingCompany.Connected Then
                        Try
                            holdingCompany.StartTransaction()
                            If isUpdate Then
                                oARCredit.UserFields.Fields.Item("U_AE_HDARCNNo").Value = ProcessSalesOrder(holdingCompany, oARCredit, ors, isUpdate)
                                oARCredit.UserFields.Fields.Item("U_AE_HDAPCNNo").Value = UpdateAPCredit(holdingCompany, oARCredit, ors)
                            Else
                                oARCredit.UserFields.Fields.Item("U_AE_HDARCNNo").Value = ProcessSalesOrder(holdingCompany, oARCredit, ors, isUpdate)
                                oARCredit.UserFields.Fields.Item("U_AE_HDAPCNNo").Value = CreateAPCredit(holdingCompany, oARCredit, ors)
                            End If
                            If oARCredit.Update() <> 0 Then
                                oCompany.GetLastError(errCode, errMess)
                                Throw New Exception(String.Format("Erro : {0} - {1}", errCode, errMess))
                            End If
                            If holdingCompany.InTransaction Then
                                holdingCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                            End If
                        Catch ex As Exception
                            If holdingCompany.InTransaction Then
                                holdingCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
                            End If
                            Throw New Exception(ex.Message)
                        End Try
                    End If
                    ors.MoveNext()
                End While
                Dim message As String
                If isUpdate Then
                    message = "Credit Note updated successfully in BOYD Holdings"
                Else
                    message = "Credit Note created successfully in BOYD Holdings"
                End If
                SBO_Application.MessageBox(message)
                If oCompany.InTransaction Then
                    oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit)
                End If
            Else
                SBO_Application.MessageBox("Error: Document not found!!!!")
            End If
        Catch ex As Exception
            If oCompany.InTransaction Then
                oCompany.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack)
            End If
            SBO_Application.SetStatusBarMessage(ex.ToString)
        Finally
            oARCredit = Nothing
            ors = Nothing
            GC.Collect()
        End Try

    End Sub
    Private Function CreateAPCredit(holdingCompany As SAPbobsCOM.Company, ARCredit As SAPbobsCOM.Documents, oRC As SAPbobsCOM.Recordset) As String
        Try
            Dim errCode As Integer
            Dim errMess As String = ""
            Dim BranchARDocEntry As Integer = 0
            Dim HoldingAPDocEntry As Integer = 0
            Dim BranchARDocNo As String = ""
            Dim HoldingAPDocNo As String = ""
            Dim APInvoice As SAPbobsCOM.Documents = Nothing
            Dim APCredit As SAPbobsCOM.Documents = Nothing
            BranchARDocEntry = ARCredit.Lines.BaseEntry
            oRC = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            If Not oRC Is Nothing Then
                oRC.DoQuery(String.Format("Select DocNum, Isnull(U_AE_HDAPInvNo,'') from OINV where DocEntry = {0} ", BranchARDocEntry))
                oRC.MoveFirst()
                BranchARDocNo = oRC.Fields.Item(0).Value
                HoldingAPDocNo = oRC.Fields.Item(1).Value
            End If
            oRC = holdingCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            oRC.DoQuery(String.Format("Select DocEntry from OPCH where DocNum = '{0}' and U_AE_BRARInvNo = '{1}'", HoldingAPDocNo, BranchARDocNo))
            oRC.MoveFirst()
            If oRC.RecordCount > 0 Then
                HoldingAPDocEntry = oRC.Fields.Item(0).Value
                APInvoice = holdingCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseInvoices)
                APCredit = holdingCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseCreditNotes)
                If APInvoice.GetByKey(HoldingAPDocEntry) Then
                    GetDocumentLine(HoldingAPDocEntry, "PCH1", oRC)
                    While Not oRC.EoF
                        If Not oRC.BoF Then
                            APCredit.Lines.Add()
                        End If
                        APCredit.Lines.BaseType = 18 'oRC.Fields.Item("ObjType").Value
                        APCredit.Lines.BaseEntry = oRC.Fields.Item("DocEntry").Value
                        APCredit.Lines.BaseLine = oRC.Fields.Item("LineNum").Value
                        oRC.MoveNext()
                    End While

                    APCredit.DocDate = ARCredit.DocDate
                    APCredit.DocDueDate = ARCredit.DocDueDate
                    APCredit.DiscountPercent = APInvoice.DiscountPercent
                    APCredit.CardCode = APInvoice.CardCode
                    ' APCredit.DiscountPercent = APInvoice.DiscountPercent
                    APCredit.UserFields.Fields.Item("U_AE_BRARCNNo").Value = ARCredit.DocNum.ToString()
                    If APCredit.Add = 0 Then
                        Dim docEntry As String = ""
                        holdingCompany.GetNewObjectCode(docEntry)
                        APCredit.GetByKey(docEntry)
                        Return APCredit.DocNum
                    Else
                        holdingCompany.GetLastError(errCode, errMess)
                        Throw New Exception(String.Format("Error : {0} - {1}", errCode, "Fail to Create AP Credit Note"))
                    End If
                End If
            Else
                Throw New Exception
            End If
        Catch ex As Exception
            Throw New Exception("Can not create AP Credit Memo")
        End Try
    End Function
    Private Function UpdateAPCredit(holdingCompany As SAPbobsCOM.Company, ARCredit As SAPbobsCOM.Documents, oRC As SAPbobsCOM.Recordset) As String
        Dim docnum As String = ""
        Dim errCode As Integer
        Dim errMess As String = ""
        Dim APCreditDocEntry As Integer = 0
        'Dim APInvoice As SAPbobsCOM.Documents = Nothing
        Dim APCredit As SAPbobsCOM.Documents = Nothing
        oRC = holdingCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oRC.DoQuery(String.Format("Select DocEntry from ORPC where DocNum = '{0}'", ARCredit.UserFields.Fields.Item("U_AE_HDAPCNNo").Value))
        oRC.MoveFirst()
        If oRC.RecordCount = 1 Then
            'APInvoice = holdingCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseInvoices)
            APCredit = holdingCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseCreditNotes)
            If APCredit.GetByKey(oRC.Fields.Item(0).Value) Then
                APCredit.NumAtCard = ARCredit.NumAtCard
                oRC.DoQuery(String.Format("SELECT COLUMN_NAME " +
                                        "FROM [{0}].INFORMATION_SCHEMA.COLUMNS " +
                                        "WHERE " +
                                        "   TABLE_NAME = 'OPCH' " +
                                        "   and COLUMN_NAME like 'U_AE%' " +
                                        "   and COLUMN_NAME not in( 'U_AE_BRARInvNo','U_AE_BRARCNNo','U_AE_HDAPInvNo', " +
                                        "   'U_AE_HDSONo','U_AE_HDAPCNNo','U_AE_HDARCNNo','U_AE_BRCode')", holdingCompany.CompanyDB))
                oRC.MoveFirst()
                If oRC.RecordCount Then
                    While Not oRC.EoF
                        APCredit.UserFields.Fields.Item(oRC.Fields.Item(0).Value).Value = ARCredit.UserFields.Fields.Item(oRC.Fields.Item(0).Value).Value
                        oRC.MoveNext()
                    End While
                End If
                APCredit.DocDate = ARCredit.DocDate
                APCredit.DocDueDate = ARCredit.DocDueDate
                If APCredit.Update <> 0 Then
                    holdingCompany.GetLastError(errCode, errMess)
                    Throw New Exception(String.Format("Error : {0} - {1}", errCode, "Failed to update AP Credit"))
                End If
                docnum = APCredit.DocNum
            End If
        Else
            docnum = CreateAPCredit(holdingCompany, ARCredit, oRC)
        End If
        Return docnum
    End Function
    Private Function GetDocumentLine(DocEntry As Integer, TableName As String, ors As SAPbobsCOM.Recordset)
        Try
            ors.DoQuery(String.Format("select ObjType, DocEntry, LineNum, ItemCode, Dscription, Text, Quantity, PriceBefDi, DiscPrcnt, VatGroup, AcctCode from {0} where DocEntry = {1}", TableName, DocEntry))
            ors.MoveFirst()
            Return ors
        Finally
            ors = Nothing
        End Try
    End Function
    Private Function ProcessSalesOrder(holdingCompany As SAPbobsCOM.Company, BranchARCredit As SAPbobsCOM.Documents, oRC As SAPbobsCOM.Recordset, isUpdate As Boolean) As String
        Dim errCode As Integer
        Dim errMess As String = ""
        Dim BranchARDocEntry As Integer = 0
        Dim HoldingSODocEntry As Integer = 0
        Dim BranchARDocNo As String = ""
        Dim HoldingSODocNo As String = ""
        Dim DocNum As String = ""
        Dim SaleOrder As SAPbobsCOM.Documents = Nothing
        Try
            BranchARDocEntry = BranchARCredit.Lines.BaseEntry
            oRC = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            If Not oRC Is Nothing Then
                oRC.DoQuery(String.Format("Select DocNum, Isnull(U_AE_HDSONo,'') from OINV where DocEntry = {0} ", BranchARDocEntry))
                oRC.MoveFirst()
                BranchARDocNo = oRC.Fields.Item(0).Value
                HoldingSODocNo = oRC.Fields.Item(1).Value
            End If
            oRC = holdingCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            oRC.DoQuery(String.Format("Select DocEntry from ORDR where DocNum = '{0}' and U_AE_BRARInvNo = '{1}'", HoldingSODocNo, BranchARDocNo))
            oRC.MoveFirst()
            If oRC.RecordCount > 0 Then
                HoldingSODocEntry = oRC.Fields.Item(0).Value
                SaleOrder = holdingCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders)

                If SaleOrder.GetByKey(HoldingSODocEntry) Then
                    If SaleOrder.DocumentStatus = SAPbobsCOM.BoStatus.bost_Open Then
                        SaleOrder.UserFields.Fields.Item("U_AE_BRARCNNo").Value = BranchARCredit.DocNum.ToString()
                        SaleOrder.CancelDate = BranchARCredit.DocDate
                        SaleOrder.Update()
                        If SaleOrder.Cancel() <> 0 Then
                            holdingCompany.GetLastError(errCode, errMess)
                            Throw New Exception(String.Format("Error : {0} - {1}", errCode, "Fail to Cancel Sale Order"))
                        End If
                    ElseIf SaleOrder.DocumentStatus = SAPbobsCOM.BoStatus.bost_Close Then
                        If isUpdate Then
                            DocNum = UpDateARCreditHolding(holdingCompany, BranchARCredit, SaleOrder.DocEntry, oRC)
                        Else
                            DocNum = CreateARCreditHolding(holdingCompany, SaleOrder.DocEntry, BranchARCredit, oRC)
                        End If
                    End If
                End If
                Return DocNum
            Else
                Throw New Exception("Can not find Sales Order")
            End If
        Catch ex As Exception
            Throw New Exception(ex.Message)
        End Try
    End Function
    Private Function CreateARCreditHolding(ByVal holdingCompany As SAPbobsCOM.Company, ByVal SOEntry As Integer, ByVal BranchARCredit As SAPbobsCOM.Documents, ByVal oRC As SAPbobsCOM.Recordset) As String
        Dim errCode As Integer
        Dim errMess As String = ""
        Dim ARInvoice As SAPbobsCOM.Documents = Nothing
        Dim ARCredit As SAPbobsCOM.Documents = Nothing
        Dim DocNum As String = ""
        oRC = holdingCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oRC.DoQuery(String.Format("Select Top 1 DocEntry from INV1 where BaseType = 17 and BaseEntry = {0}", SOEntry))
        If oRC.RecordCount = 1 Then
            ARInvoice = holdingCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices)
            ARCredit = holdingCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes)
            If ARInvoice.GetByKey(oRC.Fields.Item(0).Value) Then
                GetDocumentLine(ARInvoice.DocEntry, "INV1", oRC)
                While Not oRC.EoF
                    If Not oRC.BoF Then
                        ARCredit.Lines.Add()
                    End If
                    ARCredit.Lines.BaseType = 13 'oRC.Fields.Item("ObjType").Value
                    ARCredit.Lines.BaseEntry = oRC.Fields.Item("DocEntry").Value
                    ARCredit.Lines.BaseLine = oRC.Fields.Item("LineNum").Value
                    oRC.MoveNext()
                End While

                ARCredit.DocDate = BranchARCredit.DocDate
                ARCredit.DocDueDate = BranchARCredit.DocDueDate
                ' ARCredit.DiscountPercent = ARInvoice.DiscountPercent
                ARCredit.CardCode = ARInvoice.CardCode
                ARCredit.DiscountPercent = ARInvoice.DiscountPercent
                ARCredit.Rounding = ARInvoice.Rounding
                If ARInvoice.RoundingDiffAmountFC <> 0 Then
                    ARCredit.RoundingDiffAmount = ARInvoice.RoundingDiffAmountFC
                ElseIf ARInvoice.RoundingDiffAmountSC <> 0 Then
                    ARCredit.RoundingDiffAmount = ARInvoice.RoundingDiffAmountSC
                Else
                    ARCredit.RoundingDiffAmount = ARInvoice.RoundingDiffAmount
                End If

                ARCredit.UserFields.Fields.Item("U_AE_BRARCNNo").Value = BranchARCredit.DocNum.ToString
                If ARCredit.Add = 0 Then
                    Dim docEntry As String = ""
                    holdingCompany.GetNewObjectCode(docEntry)
                    ARCredit.GetByKey(docEntry)
                    DocNum = ARCredit.DocNum
                Else
                    holdingCompany.GetLastError(errCode, errMess)
                    Throw New Exception(String.Format("Error : {0} - {1}", errCode, "Fail to Create AP Credit Note"))
                End If
            End If
        Else
            Throw New Exception("Can not find AR Invoice to create AR Credit Note")
        End If
        Return DocNum
    End Function
    Private Function UpDateARCreditHolding(ByVal holdingCompany As SAPbobsCOM.Company, ByVal BranchARCredit As SAPbobsCOM.Documents, ByVal SOEntry As Integer, ByVal oRC As SAPbobsCOM.Recordset) As String
        Dim errCode As Integer
        Dim errMess As String = ""
        Dim DocNum As String = BranchARCredit.UserFields.Fields.Item("U_AE_HDARCNNo").Value
        Dim ARCredit As SAPbobsCOM.Documents = Nothing
        ARCredit = holdingCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes)
        oRC = holdingCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
        oRC.DoQuery(String.Format("select DocEntry from ORIN where DocNum = '{0}'", BranchARCredit.UserFields.Fields.Item("U_AE_HDARCNNo").Value))
        If oRC.RecordCount = 1 Then
            If ARCredit.GetByKey(oRC.Fields.Item(0).Value) Then
                ARCredit.NumAtCard = BranchARCredit.NumAtCard
                oRC.DoQuery(String.Format("SELECT COLUMN_NAME " +
                                        "FROM [{0}].INFORMATION_SCHEMA.COLUMNS " +
                                        "WHERE " +
                                        "   TABLE_NAME = 'OPCH' " +
                                        "   and COLUMN_NAME like 'U_AE%' " +
                                        "   and COLUMN_NAME not in( 'U_AE_BRARInvNo','U_AE_BRARCNNo','U_AE_HDAPInvNo', " +
                                        "   'U_AE_HDSONo','U_AE_HDAPCNNo','U_AE_HDARCNNo','U_AE_BRCode')", holdingCompany.CompanyDB))
                oRC.MoveFirst()
                If oRC.RecordCount Then
                    While Not oRC.EoF
                        ARCredit.UserFields.Fields.Item(oRC.Fields.Item(0).Value).Value = BranchARCredit.UserFields.Fields.Item(oRC.Fields.Item(0).Value).Value
                        oRC.MoveNext()
                    End While
                End If
                ARCredit.DocDate = BranchARCredit.DocDate
                ARCredit.DocDueDate = BranchARCredit.DocDueDate

                If ARCredit.Update <> 0 Then
                    holdingCompany.GetLastError(errCode, errMess)
                    Throw New Exception(String.Format("Error : {0} - {1}", errCode, "Failed to update AR Credit"))
                End If
            End If
        Else
            DocNum = CreateARCreditHolding(holdingCompany, SOEntry, BranchARCredit, oRC)
        End If
        Return DocNum
    End Function
End Class
