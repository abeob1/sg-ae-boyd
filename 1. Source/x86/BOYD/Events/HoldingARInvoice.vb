Imports System.Xml

Public Class HoldingARInvoice
    Dim WithEvents SBO_Application As SAPbouiCOM.Application
    Dim oCompany As SAPbobsCOM.Company
    Dim DocNum As String = ""
    Dim oForm As SAPbouiCOM.Form = Nothing

    Sub New(ByVal ocompany1 As SAPbobsCOM.Company, ByVal sbo_application1 As SAPbouiCOM.Application)
        SBO_Application = sbo_application1
        oCompany = ocompany1
    End Sub
    Private Sub SBO_Application_FormDataEvent(ByRef BusinessObjectInfo As SAPbouiCOM.BusinessObjectInfo, ByRef BubbleEvent As Boolean) Handles SBO_Application.FormDataEvent
        'Dim oForm As SAPbouiCOM.Form = Nothing
        If BusinessObjectInfo.BeforeAction = False Then
            Select Case BusinessObjectInfo.FormTypeEx
                Case "133" 'AR Invoice
                    Select Case BusinessObjectInfo.EventType
                        Case SAPbouiCOM.BoEventTypes.et_FORM_DATA_ADD To SAPbouiCOM.BoEventTypes.et_FORM_DATA_UPDATE
                            If Functions.CheckHO Then
                                If BusinessObjectInfo.ActionSuccess Then
                                    oForm = SBO_Application.Forms.Item(BusinessObjectInfo.FormUID)
                                    GenerateXML(oForm.DataSources.DBDataSources.Item(0).GetValue("DocEntry", 0),
                                                oForm.DataSources.DBDataSources.Item(0).GetValue("DocNum", 0),
                                                oForm.DataSources.DBDataSources.Item(0).GetValue("CardCode", 0))
                                End If
                            End If
                    End Select
            End Select
        End If
    End Sub
    Private Sub GenerateXML(ByVal DocEntry As Integer, ByVal DocNum As String, ByVal CardCode As String)
        Dim oRc As SAPbobsCOM.Recordset = Nothing
        Dim doc As XmlDocument = New XmlDocument()
        Dim stringXML As String = ""
        Dim query As String = ""
        Dim fileSystem As FileIO.FileSystem
        Try
            oRc = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset)
            query = String.Format("Select COUNT(*) From OCRD where CardCode = '{0}' and IsNull(U_AE_InvType,'') = 'F' ", CardCode.Trim)
            oRc.DoQuery(query)
            If oRc.Fields.Item(0).Value > 0 Then
                fileSystem = New FileIO.FileSystem
                query = String.Format(My.Resources.GetXMLString.ToString(), DocEntry)

                oRc.DoQuery(query)
                If oRc.RecordCount > 0 Then
                    oRc.MoveFirst()
                    While Not oRc.EoF
                        stringXML += oRc.Fields.Item(0).Value
                        oRc.MoveNext()
                    End While
                    Dim XML As String = Replace(stringXML, "¿", " ")

                    doc.LoadXml(XML)
                    If Not fileSystem.DirectoryExists("C:\Invoice") Then
                        fileSystem.CreateDirectory("C:\Invoice")
                    End If
                    If Not fileSystem.FileExists("C:\Invoice\B" + DocNum + ".xml") Then
                        doc.Save("C:\Invoice\B" + DocNum + ".xml")
                    End If
                End If
            End If
        Catch ex As Exception
            SBO_Application.SetStatusBarMessage(ex.ToString)
        Finally
            oRc = Nothing
            GC.Collect()
        End Try
    End Sub
End Class
