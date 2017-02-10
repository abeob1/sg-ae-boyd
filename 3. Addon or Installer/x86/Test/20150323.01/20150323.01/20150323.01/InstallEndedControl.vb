Namespace AddOnInstaller

  '/// Windows Form shown at the end of the installation
  Public Class InstallEndedControl
    Inherits BaseControl
    Friend WithEvents Label1 As System.Windows.Forms.Label


    Public Sub New()
      InitializeComponent()
    End Sub

    Private Sub InitializeComponent()
      Me.Label1 = New System.Windows.Forms.Label
      Me.SuspendLayout()
      '
      'Label1
      '
      Me.Label1.Location = New System.Drawing.Point(64, 80)
      Me.Label1.Name = "Label1"
      Me.Label1.Size = New System.Drawing.Size(184, 32)
      Me.Label1.TabIndex = 0
      Me.Label1.Text = "Installation has finished"
      '
      'InstallEndedControl
      '
      Me.Controls.Add(Me.Label1)
      Me.Name = "InstallEndedControl"
      Me.ResumeLayout(False)

    End Sub
  End Class
End Namespace
