Imports System.Data.SqlClient
Imports SAPbobsCOM

Public Class Functions
    Private Shared branchcodeVal As String
    Shared errMessage As String



    Public Shared ReadOnly Property ErrorMessage() As String
        Get
            Return errMessage
        End Get
    End Property
    Public Shared Property BranchCode() As String
        Get
            Return branchcodeVal
        End Get
        Set(ByVal value As String)
            branchcodeVal = value
        End Set
    End Property

    Public Shared Sub WriteLog(ByVal Str As String)
        Dim oWrite As IO.StreamWriter
        Dim FilePath As String
        FilePath = Application.StartupPath + "\logfile.txt"

        If IO.File.Exists(FilePath) Then
            oWrite = IO.File.AppendText(FilePath)
        Else
            oWrite = IO.File.CreateText(FilePath)
        End If
        oWrite.Write(Now.ToString() + ":" + Str + vbCrLf)
        oWrite.Close()
    End Sub

    Public Shared Function CheckHO() As Boolean
        If BranchCode = "HDG" Then
            Return True
        Else
            Return False
        End If

    End Function
    Public Shared Function ConnectToCompany(ByVal Connection As Array, ByVal company As SAPbobsCOM.Company) As Integer
        Dim sErrMsg As String = ""
        Dim sErrCode As Integer
        If company.Connected Then
            company.Disconnect()
        End If
        company.CompanyDB = Connection(0).ToString()
        company.UserName = Connection(1).ToString()
        company.Password = Connection(2).ToString()
        company.Server = Connection(3).ToString()
        company.DbUserName = Connection(4).ToString()
        company.DbPassword = Connection(5).ToString()
        company.LicenseServer = Connection(6)
        If Connection(7) = "2008" Then
            company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2008
        ElseIf Connection(7) = "2005" Then
            company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2005
        ElseIf Connection(7) = "2012" Then
            company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2012
        End If


        'company.DbServerType = SAPbobsCOM.BoDataServerTypes.dst_MSSQL2008
        If company.Connect <> 0 Then
            company.GetLastError(sErrCode, sErrMsg)
            errMessage = sErrMsg
        End If
        Return sErrCode
    End Function
    Public Shared Sub DrawTabSO(ByVal oForm As SAPbouiCOM.Form, ByVal SBO_Application As SAPbouiCOM.Application, ByVal tableName As String)
        Try
            Dim oFolder As SAPbouiCOM.Folder = Nothing
            Dim oItem As SAPbouiCOM.Item
            Dim oItemRef As SAPbouiCOM.Item = Nothing
            'Dim iMaxPane As Integer = 0
            Dim otxt As SAPbouiCOM.EditText

            oItemRef = oForm.Items.Item("1320002137")
            oItem = oForm.Items.Add("ClubFld", SAPbouiCOM.BoFormItemTypes.it_FOLDER)
            oItem.Top = oItemRef.Top
            oItem.Height = oItemRef.Height
            oItem.Left = oItemRef.Left + oItemRef.Width
            oItem.Width = oItemRef.Width
            oItem.Visible = True


            oFolder = oItem.Specific
            oFolder.Caption = "Club"
            oFolder.GroupWith(oItemRef.UniqueID)
            oFolder.Pane = 99


            Dim chooseFromLists As SAPbouiCOM.ChooseFromListCollection = Nothing
            chooseFromLists = oForm.ChooseFromLists

            Dim chooseFromList As SAPbouiCOM.ChooseFromList = Nothing
            Dim chooseFromListCreationParams As SAPbouiCOM.ChooseFromListCreationParams = SBO_Application.CreateObject(SAPbouiCOM.BoCreatableObjectType.cot_ChooseFromListCreationParams)

            chooseFromListCreationParams.MultiSelection = False
            chooseFromListCreationParams.ObjectType = "2"
            chooseFromListCreationParams.UniqueID = "CFL1"


            chooseFromList = chooseFromLists.Add(chooseFromListCreationParams)
            Dim Conditions As SAPbouiCOM.Conditions = chooseFromLists.Item("CFL1").GetConditions()
            Dim Condition As SAPbouiCOM.Condition = Conditions.Add()
            Condition.Alias = "CardType"
            Condition.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL
            Condition.CondVal = "C"
            chooseFromLists.Item("CFL1").SetConditions(Conditions)

            'chooseFromListCreationParams.MultiSelection = False
            'chooseFromListCreationParams.ObjectType = "2"
            'chooseFromListCreationParams.UniqueID = "CFL2"

            'chooseFromList = chooseFromLists.Add(chooseFromListCreationParams)

            chooseFromListCreationParams.MultiSelection = False
            chooseFromListCreationParams.ObjectType = "11"
            chooseFromListCreationParams.UniqueID = "CFL3"

            chooseFromList = chooseFromLists.Add(chooseFromListCreationParams)

            'Club code textbox
            oItem = oForm.Items.Add("staic1", SAPbouiCOM.BoFormItemTypes.it_STATIC)
            oItem.FromPane = 99
            oItem.ToPane = 99
            oItemRef = oForm.Items.Item("19")
            oItem.Top = oItemRef.Top
            oItem.Left = 10
            oItem.LinkTo = "ClubCode"
            Dim oLbl As SAPbouiCOM.StaticText = oItem.Specific
            oLbl.Caption = "Code"
            oItem = oForm.Items.Add("ClubCode", SAPbouiCOM.BoFormItemTypes.it_EDIT)
            oItem.FromPane = 99
            oItem.ToPane = 99
            oItem.Width = 150
            oItemRef = oForm.Items.Item("staic1")
            oItem.Top = oItemRef.Top
            oItem.Left = oItemRef.Left + oItemRef.Width + 5
            otxt = oItem.Specific
            otxt.DataBind.SetBound(True, tableName, "U_AE_ClubCode")
            otxt.ChooseFromListUID = "CFL1"
            otxt.ChooseFromListAlias = "CardCode"

            'Club name textbox
            oItem = oForm.Items.Add("staic2", SAPbouiCOM.BoFormItemTypes.it_STATIC)
            oItem.FromPane = 99
            oItem.ToPane = 99
            oItemRef = oForm.Items.Item("staic1")
            oItem.Top = oItemRef.Top + oItem.Height + 5
            oItem.Left = 10
            oLbl = oItem.Specific
            oLbl.Item.LinkTo = "ClubName"
            oLbl.Caption = "Name"
            oItem = oForm.Items.Add("ClubName", SAPbouiCOM.BoFormItemTypes.it_EDIT)
            oItem.FromPane = 99
            oItem.ToPane = 99
            oItem.Width = 150
            oItemRef = oForm.Items.Item("staic2")
            oItem.Top = oItemRef.Top
            oItem.Left = oItemRef.Left + oItemRef.Width + 5
            otxt = oItem.Specific
            otxt.Item.LinkTo = "static2"
            otxt.DataBind.SetBound(True, tableName, "U_AE_ClubName")
            'otxt.ChooseFromListUID = "CFL2"
            'otxt.ChooseFromListAlias = "CardName"

            'Club Contact Person
            oItem = oForm.Items.Add("staic3", SAPbouiCOM.BoFormItemTypes.it_STATIC)
            oItem.FromPane = 99
            oItem.ToPane = 99
            oItemRef = oForm.Items.Item("staic2")
            oItem.Top = oItemRef.Top + oItem.Height + 5
            oItem.Left = 10
            oLbl = oItem.Specific
            oLbl.Item.LinkTo = "ClubCntct"
            oLbl.Caption = "Contact Person"
            oItem = oForm.Items.Add("ClubCntct", SAPbouiCOM.BoFormItemTypes.it_EDIT)
            oItem.FromPane = 99
            oItem.ToPane = 99
            oItem.Width = 150
            oItemRef = oForm.Items.Item("staic3")
            oItem.Top = oItemRef.Top
            oItem.Left = oItemRef.Left + oItemRef.Width + 5
            otxt = oItem.Specific
            'otxt.Item.LinkTo = "static2"
            otxt.DataBind.SetBound(True, tableName, "U_AE_ClubContactPer")
            otxt.ChooseFromListUID = "CFL3"
            otxt.ChooseFromListAlias = "Name"

            'Club Contact Person
            oItem = oForm.Items.Add("staic4", SAPbouiCOM.BoFormItemTypes.it_STATIC)
            oItem.FromPane = 99
            oItem.ToPane = 99
            oItemRef = oForm.Items.Item("staic3")
            oItem.Top = oItemRef.Top + oItem.Height + 5
            oItem.Left = 10
            oLbl = oItem.Specific
            oLbl.Item.LinkTo = "ClubAddr"
            oLbl.Caption = "Address"
            oItem = oForm.Items.Add("ClubAddr", SAPbouiCOM.BoFormItemTypes.it_EXTEDIT)
            oItem.FromPane = 99
            oItem.ToPane = 99
            oItem.Width = 150
            oItem.Height = 50
            oItemRef = oForm.Items.Item("staic4")
            oItem.Top = oItemRef.Top
            oItem.Left = oItemRef.Left + oItemRef.Width + 5
            otxt = oItem.Specific
            otxt.DataBind.SetBound(True, tableName, "U_AE_ClubAddress")
        Catch ex As Exception
            SBO_Application.SetStatusBarMessage(ex.ToString)
        End Try
    End Sub
    Public Shared Function GetAddress(ByVal CardCode As String, ByVal oCompany As SAPbobsCOM.Company) As String
        Dim ors As SAPbobsCOM.Recordset = oCompany.GetBusinessObject(BoObjectTypes.BoRecordset)
        Dim sql As String = "select top 1 format = Replace(t2.format,' ','$*'), street= isnull(t0.street,''), City = isnull(t0.City,''), ZipCode = isnull(t0.ZipCode,''), County = Isnull(t0.County,''), state = isnull(t0.[state],''), Country = isnull(t1.Name,''), Block  = Isnull(t0.Block,''), streetno = IsNull(t0.streetno,'') from crd1 t0 inner join ocry t1 on t0.country = t1.Code inner join oadf t2 on t1.addrformat = t2.code where t0.cardcode = '{0}' and AdresType = 'B'"
        sql = String.Format(sql, CardCode)
        ors.DoQuery(sql)
        If ors.RecordCount = 0 Then
            'not found
            Return ""
        End If
        Dim addrformat As String = ors.Fields.Item(0).Value
        addrformat.Replace("    ", "$*")
        Dim arr As Array = addrformat.Split("$")
        Dim address As String = ""
        For i As Integer = 0 To arr.Length - 1

            Dim UpperCase As Boolean = InStr(arr(i).ToString(), "[U]", CompareMethod.Text)
            Dim Lowercase As Boolean = InStr(arr(i).ToString(), "[L]", CompareMethod.Text)
            Dim Capitalite As Boolean = InStr(arr(i).ToString(), "[C]", CompareMethod.Text)
            Dim Description As Boolean = InStr(arr(i).ToString(), "[D]", CompareMethod.Text)
            Description = InStr(arr(i).ToString(), "[UD]", CompareMethod.Text)
            Description = InStr(arr(i).ToString(), "[LD]", CompareMethod.Text)
            Description = InStr(arr(i).ToString(), "[CD]", CompareMethod.Text)

            Select Case Left(arr(i), 1)
                Case "*"
                    address += " "
                Case "N"
                    address += vbNewLine
                Case "1", "2", "3", "7", "T"
                    Dim s As String
                    If Left(arr(i), 1) = "T" Then
                        s = ors.Fields.Item("streetno").Value
                    Else
                        Dim index As Integer = Convert.ToInt32(Left(arr(i), 1))
                        s = ors.Fields.Item(index).Value
                    End If
                    If UpperCase Then
                        address += s.ToUpper()
                    ElseIf Lowercase Then
                        address += s.ToLower()
                    ElseIf Capitalite Then
                        address += Left(s, 1).ToUpper() + s.Substring(1)
                    Else
                        address += s
                    End If
                Case "4"
                    Dim index As Integer = Convert.ToInt32(Left(arr(i), 1))
                    Dim s As String = ors.Fields.Item(index).Value
                    'If Description Then
                    '    'get county description here
                    'Else
                    '    address += s
                    'End If
                    address += s
                Case "5"
                    Dim index As Integer = Convert.ToInt32(Left(arr(i), 1))
                    Dim s As String = ors.Fields.Item(index).Value
                    'If Description Then
                    '    'get state description here
                    'Else

                    'End If
                    address += s
                Case "6"
                    Dim index As Integer = Convert.ToInt32(Left(arr(i), 1))
                    Dim s As String = ors.Fields.Item(index).Value
                    'If Description Then
                    '    'get Country description here
                    'Else
                    '    address += " " + s
                    'End If
                    address += s
            End Select
        Next

        Return address
    
    End Function
End Class
