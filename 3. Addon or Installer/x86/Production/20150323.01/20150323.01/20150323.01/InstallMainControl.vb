Namespace AddOnInstaller

  '/// Windows Form shown at the beginning of the installation
  Public Class InstallMainControl
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
      Me.Label1.Location = New System.Drawing.Point(56, 40)
      Me.Label1.Name = "Label1"
      Me.Label1.Size = New System.Drawing.Size(256, 48)
      Me.Label1.TabIndex = 0
      Me.Label1.Text = "This wizard will install your addon."
      '
      'InstallMainControl
      '
      Me.Controls.Add(Me.Label1)
      Me.Name = "InstallMainControl"
      Me.ResumeLayout(False)

    End Sub
  End Class
End Namespace