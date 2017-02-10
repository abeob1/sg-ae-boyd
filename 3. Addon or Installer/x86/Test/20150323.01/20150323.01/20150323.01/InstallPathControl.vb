Namespace AddOnInstaller

  ' Windows form asking for the installation path
  Public Class InstallPathControl
    Inherits BaseControl
    Friend WithEvents Label2 As System.Windows.Forms.Label

    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents eAddOnPath As System.Windows.Forms.TextBox
    Friend WithEvents BrowseButton As System.Windows.Forms.Button
    Friend WithEvents FolderBrowserDialog1 As System.Windows.Forms.FolderBrowserDialog

    Public Sub New()
      InitializeComponent()
    End Sub


    Private Sub InitializeComponent()
      Me.Label1 = New System.Windows.Forms.Label
      Me.Label2 = New System.Windows.Forms.Label
      Me.eAddOnPath = New System.Windows.Forms.TextBox
      Me.BrowseButton = New System.Windows.Forms.Button
      Me.FolderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog
      Me.SuspendLayout()
      '
      'Label1
      '
      Me.Label1.Location = New System.Drawing.Point(16, 40)
      Me.Label1.Name = "Label1"
      Me.Label1.Size = New System.Drawing.Size(328, 16)
      Me.Label1.TabIndex = 0
      Me.Label1.Text = "To change the installation folder click Browse."
      '
      'Label2
      '
      Me.Label2.Location = New System.Drawing.Point(16, 168)
      Me.Label2.Name = "Label2"
      Me.Label2.Size = New System.Drawing.Size(328, 16)
      Me.Label2.TabIndex = 1
      Me.Label2.Text = "To continue please click Next."
      '
      'eAddOnPath
      '
      Me.eAddOnPath.Location = New System.Drawing.Point(16, 64)
      Me.eAddOnPath.Name = "eAddOnPath"
      Me.eAddOnPath.Size = New System.Drawing.Size(280, 20)
      Me.eAddOnPath.TabIndex = 2
      Me.eAddOnPath.Text = ""
      '
      'BrowseButton
      '
      Me.BrowseButton.Location = New System.Drawing.Point(304, 64)
      Me.BrowseButton.Name = "BrowseButton"
      Me.BrowseButton.Size = New System.Drawing.Size(88, 24)
      Me.BrowseButton.TabIndex = 3
      Me.BrowseButton.Text = "Browse"
      '
      'InstallPathControl
      '
      Me.Controls.Add(Me.BrowseButton)
      Me.Controls.Add(Me.eAddOnPath)
      Me.Controls.Add(Me.Label2)
      Me.Controls.Add(Me.Label1)
      Me.Name = "InstallPathControl"
      Me.ResumeLayout(False)

    End Sub

    Public Overrides Sub UpdateInfo(ByVal info As AddOnInstallInfo)

      info.StrAddOnInstallPath = eAddOnPath.Text

    End Sub

    Private Sub BrowseButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BrowseButton.Click
      'FolderBrowserDialog1.RootFolder = strAddOnPath
      If FolderBrowserDialog1.ShowDialog() = DialogResult.OK Then
        eAddOnPath.Text = FolderBrowserDialog1.SelectedPath
      End If
    End Sub
  End Class
End Namespace