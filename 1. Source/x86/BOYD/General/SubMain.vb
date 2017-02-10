Option Explicit On
Option Strict Off
Imports System.Windows.Forms
'Imports System.ServiceProcess

Module SubMain
    Public Sub Main()

        Dim Args As String() = Environment.GetCommandLineArgs()

        Dim obj As New Add_on
        Application.Run()


    End Sub
End Module
