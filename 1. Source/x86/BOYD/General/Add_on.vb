Option Explicit On
Option Strict Off
Imports System.Windows.Forms
Imports System.Data.SqlClient
Imports System.Data
Imports System.Threading

Public Class Add_on
    Private WithEvents SBO_Application As SAPbouiCOM.Application
    Dim oCompany As SAPbobsCOM.Company
    Dim oCreateBP As HoldingCreateBP
    Dim oSalesOrder As BranchSalesOrder
    Dim oBranchARInvoice As BranchARInvoice
    Dim oBranchARCredit As BranchARCredit
    Dim oHoldingARInvoice As HoldingARInvoice
#Region "Initial"
    Public Sub New()
        MyBase.New()
        Class_Init()
    End Sub
    Public Sub SetApplication()
        Dim sbogui As SAPbouiCOM.SboGuiApi
        Dim oconnection As String
        sbogui = New SAPbouiCOM.SboGuiApi
        If Environment.GetCommandLineArgs().Length = 1 Then
            oconnection = "0030002C0030002C00530041005000420044005F00440061007400650076002C0050004C006F006D0056004900490056"
        Else
            oconnection = Environment.GetCommandLineArgs.GetValue(1)
        End If

        Try
            sbogui.Connect(oconnection)
        Catch ex As Exception
            MsgBox("No SAP Application Running")
            End
        End Try
        SBO_Application = sbogui.GetApplication(-1)
    End Sub
    Private Function SetConnectionContext() As Integer
        Dim sCookie As String
        Dim sConnectionContext As String
        Try
            oCompany = New SAPbobsCOM.Company
            sCookie = oCompany.GetContextCookie
            sConnectionContext = SBO_Application.Company.GetConnectionContext(sCookie)
            If oCompany.Connected = True Then
                oCompany.Disconnect()
            End If
            SetConnectionContext = oCompany.SetSboLoginContext(sConnectionContext)
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Function
    Private Function ConnectToCompany() As Integer
        oCompany.UseTrusted = False
        ConnectToCompany = oCompany.Connect
        If oCompany.GetLastErrorDescription.Length > 0 Then
            SBO_Application.MessageBox(oCompany.GetLastErrorDescription)
        End If


    End Function
    Private Sub Class_Init()
        SetApplication()
        If Not SetConnectionContext() = 0 Then
            SBO_Application.MessageBox("Failed setting a connection to DI API")
            End ' Terminating the Add-On Application
        End If
        If Not ConnectToCompany() = 0 Then
            SBO_Application.MessageBox("Failed connecting to the company's Database")
            End ' Terminating the Add-On Application
        End If

        Dim functions As Functions
        functions = New Functions
        Dim adminInfo As SAPbobsCOM.AdminInfo = oCompany.GetCompanyService.GetAdminInfo
        functions.BranchCode = adminInfo.AdditionalIdNumber

        oCreateBP = New HoldingCreateBP(oCompany, SBO_Application)
        oSalesOrder = New BranchSalesOrder(oCompany, SBO_Application)
        oBranchARInvoice = New BranchARInvoice(oCompany, SBO_Application)
        oBranchARCredit = New BranchARCredit(oCompany, SBO_Application)
        oHoldingARInvoice = New HoldingARInvoice(oCompany, SBO_Application)
        CreateUDFSaleOrder()
        SBO_Application.SetStatusBarMessage("Add-on is loaded", , False)
    End Sub
    Private Sub CreateUDFSaleOrder()
        CreateUDF("ORDR", "AE_ClubCode", "Club Code", SAPbobsCOM.BoFieldTypes.db_Alpha, SAPbobsCOM.BoFldSubTypes.st_None, 50)
        CreateUDF("ORDR", "AE_ClubName", "Club Name", SAPbobsCOM.BoFieldTypes.db_Alpha, SAPbobsCOM.BoFldSubTypes.st_None, 200)
        CreateUDF("ORDR", "AE_ClubContactPer", "Club Contact Person", SAPbobsCOM.BoFieldTypes.db_Alpha, SAPbobsCOM.BoFldSubTypes.st_None, 200)
        CreateUDF("ORDR", "AE_ClubAddress", "Club Address", SAPbobsCOM.BoFieldTypes.db_Alpha, SAPbobsCOM.BoFldSubTypes.st_Address, 200)
    End Sub
    Private Sub CreateUDF(ByVal strTableName As String, ByVal strFieldName As String, ByVal strFieldDescription As String,
                          ByVal fieldType As SAPbobsCOM.BoFieldTypes, ByVal fieldSubType As SAPbobsCOM.BoFldSubTypes,
                          ByVal editSize As Integer,
                          Optional validValues() As String = Nothing,
                          Optional validValuesDescription() As String = Nothing,
                          Optional strDefaultValue As String = Nothing)
        Dim validValuesMD As SAPbobsCOM.ValidValues
        Dim udtMD As SAPbobsCOM.UserTablesMD
        Dim udfMD As SAPbobsCOM.UserFieldsMD
        Try
            If strTableName.StartsWith("@") Then
                udtMD = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables)
                Dim bTableExisted As Boolean = udtMD.GetByKey(strTableName.Replace("@", ""))
                udtMD = Nothing

                If (Not bTableExisted) Then
                    SBO_Application.MessageBox(String.Format("Table {0} doesn't exist", strTableName))
                    Return
                End If
            End If
            udfMD = oCompany.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields)
            udfMD.TableName = strTableName
            udfMD.Name = strFieldName
            udfMD.Description = strFieldDescription
            udfMD.Type = fieldType
            udfMD.SubType = fieldSubType
            udfMD.EditSize = editSize

            If (Not strDefaultValue Is Nothing) Then
                udfMD.DefaultValue = strDefaultValue
            End If

            If (Not validValues Is Nothing) Then
                Dim nValueCount As Integer = validValues.Count

                validValuesMD = udfMD.ValidValues
                For iValue As Integer = 0 To nValueCount - 1
                    validValuesMD.Value = validValues(iValue)
                    If (Not validValuesDescription Is Nothing) Then
                        If (iValue < validValuesDescription.Length) Then
                            validValuesMD.Description = validValuesDescription(iValue)
                        End If
                        validValuesMD.Add()
                    End If
                Next
            End If
            Dim nErrorCode As Integer = udfMD.Add()
            If (nErrorCode <> 0) Then
                If (nErrorCode <> -2035 And nErrorCode <> -1120) Then
                    SBO_Application.SetStatusBarMessage("Failed to add UDF.", SAPbouiCOM.BoMessageTime.bmt_Short, True)
                End If
            End If

        Catch ex As Exception
            SBO_Application.MessageBox(ex.Message)
        Finally
            udtMD = Nothing
            udfMD = Nothing
            validValuesMD = Nothing
            GC.Collect()
        End Try

    End Sub
#End Region
#Region "SAP Event"
    Private Sub SBO_Application_MenuEvent(ByRef pVal As SAPbouiCOM.MenuEvent, ByRef BubbleEvent As Boolean) Handles SBO_Application.MenuEvent
        If pVal.BeforeAction = False Then
            Select Case pVal.MenuUID

            End Select
        End If
    End Sub
    Private Sub SBO_Application__AppEvent(ByVal EventType As SAPbouiCOM.BoAppEventTypes) Handles SBO_Application.AppEvent
        Select Case EventType
            Case SAPbouiCOM.BoAppEventTypes.aet_CompanyChanged
                System.Windows.Forms.Application.Exit()
            Case SAPbouiCOM.BoAppEventTypes.aet_LanguageChanged
            Case SAPbouiCOM.BoAppEventTypes.aet_ServerTerminition
                System.Windows.Forms.Application.Exit()
            Case SAPbouiCOM.BoAppEventTypes.aet_ShutDown
                System.Windows.Forms.Application.Exit()
        End Select
    End Sub
    Private Sub SBO_Application_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles SBO_Application.ItemEvent
        If pVal.BeforeAction = False Then
            Dim oForm As SAPbouiCOM.Form = Nothing
            If pVal.EventType <> SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD And pVal.EventType <> SAPbouiCOM.BoEventTypes.et_FORM_DEACTIVATE Then
                oForm = SBO_Application.Forms.Item(FormUID)
            End If
            Select Case pVal.EventType
                Case SAPbouiCOM.BoEventTypes.et_FORM_LOAD
                    Select Case pVal.FormType
                    End Select
                Case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED
                    Select Case pVal.FormTypeEx
                        Case "134" 'BUSINESS PARTNER
                            'If Functions.CheckHO Then
                            '    If pVal.ItemUID = "1" Then
                            '        Dim oTextBox As SAPbouiCOM.EditText
                            '        oTextBox = oForm.Items.Item("5").Specific
                            '        If Not CreateBP.CreateBPtoAllBranches(oTextBox.Value, SBO_Application) Then
                            '            BubbleEvent = False
                            '        End If
                            '    End If
                            'End If
                    End Select
            End Select
        End If
    End Sub
#End Region

End Class
