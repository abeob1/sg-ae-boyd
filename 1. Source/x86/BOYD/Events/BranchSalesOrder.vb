Imports System.Xml

Public Class BranchSalesOrder
    Dim WithEvents SBO_Application As SAPbouiCOM.Application
    Dim oCompany As SAPbobsCOM.Company
    Dim oForm As SAPbouiCOM.Form = Nothing

    Sub New(ByVal ocompany1 As SAPbobsCOM.Company, ByVal sbo_application1 As SAPbouiCOM.Application)
        SBO_Application = sbo_application1
        oCompany = ocompany1
    End Sub
    Private Sub SBO_Application_ItemEvent(ByVal FormUID As String, ByRef pVal As SAPbouiCOM.ItemEvent, ByRef BubbleEvent As Boolean) Handles SBO_Application.ItemEvent
        Try
            If pVal.EventType <> SAPbouiCOM.BoEventTypes.et_FORM_UNLOAD And pVal.EventType <> SAPbouiCOM.BoEventTypes.et_FORM_DEACTIVATE Then
                oForm = SBO_Application.Forms.Item(FormUID)
            End If
            If pVal.BeforeAction = False Then
                Select Case pVal.FormType
                    Case "139" 'SALES ORDER
                        If Not Functions.CheckHO Then
                            Select Case pVal.EventType
                                Case SAPbouiCOM.BoEventTypes.et_FORM_LOAD
                                    Functions.DrawTabSO(oForm, SBO_Application, "ORDR")
                                Case SAPbouiCOM.BoEventTypes.et_ITEM_PRESSED
                                    If pVal.ItemUID = "ClubFld" Then
                                        oForm.PaneLevel = 99
                                    End If
                                Case SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST
                                    Dim chooseFromListEvent As SAPbouiCOM.IChooseFromListEvent
                                    chooseFromListEvent = pVal
                                    Dim strChooseFromListId As String = chooseFromListEvent.ChooseFromListUID
                                    Dim chooseFromList As SAPbouiCOM.ChooseFromList = oForm.ChooseFromLists.Item(strChooseFromListId)
                                    Dim editText As SAPbouiCOM.EditText

                                    If (chooseFromListEvent.Before_Action = False) Then
                                        Dim dataTable As SAPbouiCOM.DataTable = Nothing
                                        dataTable = chooseFromListEvent.SelectedObjects

                                        If Not dataTable Is Nothing Then
                                            Dim strValue As String

                                            If pVal.ItemUID = "ClubCode" Then
                                                strValue = System.Convert.ToString(dataTable.GetValue(0, 0))
                                                editText = oForm.Items.Item(pVal.ItemUID).Specific
                                                Try
                                                    editText.Value = strValue
                                                Catch
                                                End Try

                                                strValue = System.Convert.ToString(dataTable.GetValue(1, 0))
                                                editText = oForm.Items.Item("ClubName").Specific
                                                Try
                                                    editText.Value = strValue
                                                Catch
                                                End Try
                                                editText = oForm.Items.Item("ClubAddr").Specific
                                                Try
                                                    editText.Value = Functions.GetAddress(System.Convert.ToString(dataTable.GetValue(0, 0)), oCompany).Trim
                                                Catch
                                                End Try
                                                editText = oForm.Items.Item("ClubCntct").Specific
                                                Try
                                                    editText.Value = ""
                                                Catch
                                                End Try
                                                'ElseIf pVal.ItemUID = "ClubName" Then
                                                '    strValue = System.Convert.ToString(dataTable.GetValue(1, 0))
                                                '    editText = oForm.Items.Item(pVal.ItemUID).Specific
                                                '    Try
                                                '        editText.Value = strValue
                                                '    Catch
                                                '    End Try
                                                '    strValue = System.Convert.ToString(dataTable.GetValue(0, 0))
                                                '    editText = oForm.Items.Item("ClubCode").Specific
                                                '    Try
                                                '        editText.Value = strValue
                                                '    Catch
                                                '    End Try
                                            ElseIf pVal.ItemUID = "ClubCntct" Then
                                                strValue = System.Convert.ToString(dataTable.GetValue(2, 0))
                                                editText = oForm.Items.Item(pVal.ItemUID).Specific
                                                Try
                                                    editText.Value = strValue
                                                Catch
                                                End Try
                                            End If

                                        End If
                                    End If
                                Case SAPbouiCOM.BoEventTypes.et_VALIDATE
                                    'If pVal.ItemUID = "ClubCode" Then
                                    '    Dim editText As SAPbouiCOM.EditText = oForm.Items.Item("ClubCode").Specific
                                    '    If editText.Value Is Nothing Or editText.Value = "" Then
                                    '        oForm.Items.Item("ClubCntct").Enabled = False
                                    '    Else
                                    '        oForm.Items.Item("ClubCntct").Enabled = True
                                    '    End If
                                    '    oForm.Items.Item("ClubCntct").Refresh()
                                    'End If
                            End Select
                        End If
                End Select
            Else
                Select Case pVal.EventType
                    Case SAPbouiCOM.BoEventTypes.et_CHOOSE_FROM_LIST
                        If Not Functions.CheckHO Then
                            If pVal.ItemUID = "ClubCntct" Then
                                Dim editText As SAPbouiCOM.EditText = oForm.Items.Item("ClubCode").Specific

                                Dim oCFLs As SAPbouiCOM.ChooseFromListCollection
                                oCFLs = oForm.ChooseFromLists

                                Dim Conditions As SAPbouiCOM.Conditions = oCFLs.Item("CFL3").GetConditions()
                                If Conditions.Count = 1 Then
                                    Conditions.Item(0).CondVal = editText.Value
                                    oCFLs.Item("CFL3").SetConditions(Conditions)
                                Else
                                    Dim Condition As SAPbouiCOM.Condition = Conditions.Add()
                                    Condition.Alias = "CardCode"
                                    Condition.Operation = SAPbouiCOM.BoConditionOperation.co_EQUAL
                                    Condition.CondVal = editText.Value
                                    oCFLs.Item("CFL3").SetConditions(Conditions)
                                End If
                            End If
                        End If
                End Select
            End If
        Catch ex As Exception
            SBO_Application.SetStatusBarMessage(ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, True)
        End Try
    End Sub
End Class
