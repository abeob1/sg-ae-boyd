
Imports System.IO

Namespace AddOnInstaller

  ' Base form for the wizard
  ' Starts all installation process
  Public Class BaseForm
    Inherits System.Windows.Forms.Form

    Protected currentIndex As Integer

    Protected wizardPages(1) As BaseControl
    Protected endInstallPage As BaseControl

    Protected b1AddOnInstall As AddOnInstallManager


#Region " Windows Form Designer generated code "

    Public Sub New()
      MyBase.New()

      'This call is required by the Windows Form Designer.
      InitializeComponent()

      ' Create the AddOnInstall Manager instance
      b1AddOnInstall = New AddOnInstallManager

      ' Read the command line parameters
      ' Return true install addon
      ' Return false uninstall addon
      If (b1AddOnInstall.InitParams()) Then
        ' Install addon
        ' Initialize wizard pages for the addon installation
        InitPages()

        EnableButtons()
      Else
        ' Uninstall addon
        ' Show uninstall page at the end of uninstall
        ShowUninstallPage()
      End If

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
      If disposing Then
        If Not (components Is Nothing) Then
          components.Dispose()
        End If
      End If
      MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents BackButton As System.Windows.Forms.Button
    Friend WithEvents NextButton As System.Windows.Forms.Button
    Friend WithEvents CancelBut As System.Windows.Forms.Button
    Friend WithEvents MainLabel As System.Windows.Forms.Label
    Friend WithEvents WizardPanel As System.Windows.Forms.Panel
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
      Me.MainLabel = New System.Windows.Forms.Label
      Me.BackButton = New System.Windows.Forms.Button
      Me.NextButton = New System.Windows.Forms.Button
      Me.CancelBut = New System.Windows.Forms.Button
      Me.WizardPanel = New System.Windows.Forms.Panel
      Me.SuspendLayout()
      '
      'MainLabel
      '
      Me.MainLabel.Font = New System.Drawing.Font("Microsoft Sans Serif", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
      Me.MainLabel.Location = New System.Drawing.Point(24, 16)
      Me.MainLabel.Name = "MainLabel"
      Me.MainLabel.Size = New System.Drawing.Size(280, 24)
      Me.MainLabel.TabIndex = 1
      '
      'BackButton
      '
      Me.BackButton.Location = New System.Drawing.Point(232, 312)
      Me.BackButton.Name = "BackButton"
      Me.BackButton.Size = New System.Drawing.Size(88, 24)
      Me.BackButton.TabIndex = 2
      Me.BackButton.Text = "Back"
      '
      'NextButton
      '
      Me.NextButton.Location = New System.Drawing.Point(328, 312)
      Me.NextButton.Name = "NextButton"
      Me.NextButton.Size = New System.Drawing.Size(88, 24)
      Me.NextButton.TabIndex = 3
      Me.NextButton.Text = "Next"
      '
      'CancelBut
      '
      Me.CancelBut.Location = New System.Drawing.Point(8, 312)
      Me.CancelBut.Name = "CancelBut"
      Me.CancelBut.Size = New System.Drawing.Size(88, 24)
      Me.CancelBut.TabIndex = 4
      Me.CancelBut.Text = "Cancel"
      '
      'WizardPanel
      '
      Me.WizardPanel.Location = New System.Drawing.Point(8, 48)
      Me.WizardPanel.Name = "WizardPanel"
      Me.WizardPanel.Size = New System.Drawing.Size(400, 256)
      Me.WizardPanel.TabIndex = 5
      '
      'BaseForm
      '
      Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
      Me.BackColor = System.Drawing.SystemColors.Control
      Me.ClientSize = New System.Drawing.Size(424, 350)
      Me.Controls.Add(Me.WizardPanel)
      Me.Controls.Add(Me.CancelBut)
      Me.Controls.Add(Me.NextButton)
      Me.Controls.Add(Me.BackButton)
      Me.Controls.Add(Me.MainLabel)
      Me.Name = "BaseForm"
      Me.Text = "AddOn Installation Wizard"
      Me.ResumeLayout(False)

    End Sub

#End Region


    ' Creates the needed pages for the addon installation process
    Protected Sub InitPages()

      wizardPages(0) = New InstallMainControl
      wizardPages(1) = New InstallPathControl

      Dim ipc As InstallPathControl
      ipc = wizardPages(1)

      ipc.eAddOnPath.Text = b1AddOnInstall.GetAddOnPath()

      WizardPanel.Controls.Add(wizardPages(0))
      wizardPages(0).Visible = True
      WizardPanel.Controls.Add(wizardPages(1))
      wizardPages(1).Visible = False

      currentIndex = 0

    End Sub

    ' Enables/Disables the buttons depending on which step of installation we are
    Protected Sub EnableButtons()
      ' only one pane
      If (wizardPages.Length = 1) Then
        BackButton.Enabled = False
        NextButton.Text = "Install"
        NextButton.Enabled = True
      ElseIf (currentIndex = 0) Then
        BackButton.Enabled = False
        NextButton.Text = "Next"
        NextButton.Enabled = True
        ' last pane of a list of panes
      ElseIf (currentIndex = wizardPages.Length - 1) Then
        BackButton.Enabled = True
        NextButton.Text = "Install"
        NextButton.Enabled = True
        ' pane in the middle of a list of panes
      Else
        BackButton.Enabled = True
        NextButton.Text = "Next"
        NextButton.Enabled = True
      End If

    End Sub

    ' Show uninstall windows form at the end of the installation
    Protected Sub ShowUninstallPage()

      ' Show Message Installed
      wizardPages(0) = New UnInstallControl

      Dim ipc As UnInstallControl
      ipc = wizardPages(0)

      WizardPanel.Controls.Add(wizardPages(0))
      wizardPages(0).Visible = True

      currentIndex = 0

      ' Disable Buttons
      BackButton.Enabled = False
      NextButton.Enabled = False
      CancelBut.Text = "OK"
      CancelBut.Enabled = True


    End Sub

    Protected Sub NextButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NextButton.Click

      Dim page As BaseControl = Nothing

      If (wizardPages.Length = 1 Or currentIndex <= wizardPages.Length - 1) Then
        page = wizardPages(currentIndex)
      End If

      If (wizardPages.Length <> 1 And currentIndex < wizardPages.Length - 1) Then

        ' current pane invisible
        page.Visible = False
        page.UpdateInfo(b1AddOnInstall.GetAddOnInstallInfo())

        ' change current pane and make it visible
        currentIndex += 1
        page = wizardPages(currentIndex)
        page.Visible = True

        ' change title
        MainLabel.Text = page.Title

        ' change buttons
        EnableButtons()
      ElseIf (wizardPages.Length = 1 Or currentIndex = wizardPages.Length - 1) Then
        ' Update saved information
        page.UpdateInfo(b1AddOnInstall.GetAddOnInstallInfo())

        ' Call AddOnInstallAPI.dll functions
        b1AddOnInstall.Install()

        ' current pane invisible
        page.Visible = False

        currentIndex = wizardPages.Length

        endInstallPage = New InstallEndedControl
        endInstallPage.Visible = True

        BackButton.Visible = False
        NextButton.Text = "Finish"
        CancelBut.Visible = False

      Else
        Close()
      End If

    End Sub

    Protected Sub BackButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BackButton.Click
      Dim page As BaseControl

      ' current pane invisible
      page = wizardPages(currentIndex)
      page.Visible = False
      page.UpdateInfo(b1AddOnInstall.GetAddOnInstallInfo())

      ' change current pane and make it visible
      currentIndex -= 1
      page = wizardPages(currentIndex)
      page.Visible = True

      ' change title
      MainLabel.Text = page.Title

      ' change buttons
      EnableButtons()

    End Sub

    Private Sub CancelBut_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CancelBut.Click
      Close()
    End Sub
  End Class

End Namespace
