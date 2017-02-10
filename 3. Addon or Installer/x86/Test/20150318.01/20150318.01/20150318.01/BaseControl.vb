Namespace AddOnInstaller

  '/// Base class for all installer Windows forms
  '/// Manages the update of the information stored on each window
  '/// to be accessible from the AddOnInstallManager
  Public Class BaseControl
    Inherits System.Windows.Forms.UserControl


    Public Title As String

    Public Sub New()
      InitializeComponent()
    End Sub
    Private Sub InitializeComponent()
      '
      'BaseControl
      '
      Me.Name = "BaseControl"
      Me.Size = New System.Drawing.Size(400, 256)

    End Sub

    Public Overridable Sub UpdateInfo(ByVal info As AddOnInstallInfo)

    End Sub



  End Class
End Namespace
