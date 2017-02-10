Imports Microsoft.Win32


Namespace AddOnInstaller

  '/// <summary>
  '/// Main class of the AddOnInstaller project.
  '/// </summary>
  Public Class AddOnInstallManager

    '/// <summary>
    '/// Path of the AddOnInstallAPI.dll.
    '/// </summary>
    Protected strDllPath As String

    '/// <summary>
    '/// .
    '/// </summary>
    Protected keyValue As String
    Protected subKeyAddOnInstallDirValue As String
    Protected subKeyDllPathValue As String

    '/// <summary>
    '/// Reference to the class containing all the AddOn information needed by the intaller.
    '/// </summary>
    Protected installInfo As AddOnInstallInfo

    ' Declaring the functions inside "AddOnInstallAPI.dll"

    'SetAddOnFolder - Use it if you want to change the installation folder.
    Declare Function SetAddOnFolder Lib "AddOnInstallAPI.dll" (ByVal strPath As String) As Int32

    'RestartNeeded - Use it if your installation requires a restart, it will cause
    'the SBO application to close itself after the installation is complete.
    Declare Function RestartNeeded Lib "AddOnInstallAPI.dll" () As Int32

#If Version = "2005" Then
    'EndInstallEx - Signals B1 that the installation is complete
    Declare Function EndInstallEx Lib "AddOnInstallAPI.dll" (ByVal strPath As String, ByVal isSucceed As Boolean) As Int32

    'EndUninstall - Signals B1 that the uninstallation is complete
    Declare Function EndUninstall Lib "AddOnInstallAPI.dll" (ByVal strPath As String, ByVal isSucceed As Boolean) As Int32

    'B1Info - Gets B1 Version information
    Declare Function B1Info Lib "AddOnInstallAPI.dll" (ByVal strB1Info As String, ByVal maxLen As Int32) As Int32

#Else
    'EndInstall - Signals B1 that the installation is complete
    Declare Function EndInstall Lib "AddOnInstallAPI.dll" () As Int32
#End If

    '/// <summary>
    '/// Reads the command line arguments given by the B1 application at installation time.
    '/// Prepares information needed for installation.
    '/// Call uninstall when parameter is /x
    '/// </summary>
    Public Function InitParams() As Boolean
      Dim strCmdLine As String
      Dim NumOfParams As Integer 'The number of parameters in the command line (should be 2)
      Dim strCmdLineElements(2) As String

      ' The command line parameter contains 2 parameters seperated by '|' 
      NumOfParams = Environment.GetCommandLineArgs.Length
      If NumOfParams = 2 Then
        strCmdLine = Environment.GetCommandLineArgs.GetValue(1)

        installInfo = New AddOnInstallInfo

        ' Initialize key names
        InitRegistryKeyValues()

        ' Uninstall command
        If (strCmdLine = "/x") Then
          ' Uninstall the addon
          UninstallAddOn()
          Return False

          ' Install the addon
        Else
          If (strCmdLine.IndexOf("|") = -1) Then
            MessageBox.Show("This installer must be run from Sap Business One", _
                            "Incorrect command line arguments", _
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Return False
          End If

          strCmdLineElements = strCmdLine.Split("|")

          ' Get the proposed AddOn Installation destination Folder
          installInfo.StrAddOnInstallPath = strCmdLineElements.GetValue(0)

          ' Get the "AddOnInstallAPI.dll" path
          strDllPath = strCmdLineElements.GetValue(1)
          strDllPath = strDllPath.Remove((strDllPath.Length - 19), 19) ' Only the path is needed

          Return True
        End If
      Else ' The setup must always be called with 2 command line parameters
        MessageBox.Show("This installer must be run from Sap Business One ", _
                        "Incorrect command line arguments", _
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        Return False
      End If
    End Function

    ' Init key names 
    Protected Sub InitRegistryKeyValues()

      keyValue = "SOFTWARE\\SAP\\SAP Manage\\SAP Business One\\AddOnInstaller" _
        + "\\" + installInfo.PartnerName + "\\" + installInfo.AddOnName
      subKeyAddOnInstallDirValue = "AddOnInstallDir"
      subKeyDllPathValue = "DllPath"

    End Sub

    '/// <summary>
    '/// Does the addon uninstallation actions.
    '/// </summary>
    Protected Sub UninstallAddOn()

      Try

        Dim addOnLocation As String

        ' Finds addon location by reading the information stored at installation time
        addOnLocation = ReadRegisteredAddOnInfo()

        ' Removes all addon files 
        RemoveAllAddOnFiles(addOnLocation)

        ' Removes all addon registry information
        UnregisterAddOnInfo()


#If Version = "2005" Then
        ' Call EndUnistall function to be able to inform B1 when uninstall is finished
        ' New function from 2005 version (no equivalent in 2004 version)

        Environment.CurrentDirectory = strDllPath ' For Dll function calls will work

        Dim ret As Int32
        ret = EndUninstall(addOnLocation, True)
        If ret <> 0 Then
          MessageBox.Show("EndUninstall returned " + ret.ToString(), _
                          "Error while trying to uninstall the AddOn", _
                          MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End If
#End If

      Catch ex As Exception
        MessageBox.Show("Error: " + ex.Message, _
                        "Error while trying to uninstall the AddOn", _
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
#If Version = "2005" Then

        ' Call EnUninstall to inform B1 addon uninstall had problems
        ' New function from 2005 version (no equivalent in 2004 version)
        Dim ret As Int32
        Environment.CurrentDirectory = strDllPath ' For Dll function calls will work
        ret = EndUninstall("", False)
        If ret <> 0 Then
          MessageBox.Show("EndUninstall returned " + ret.ToString(), _
                          "Error while trying to uninstall the AddOn", _
                          MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End If
#End If

      End Try

    End Sub

    '/// <summary>
    '/// Launchs the addon installation process.
    '/// </summary>
    Public Sub Install()

      Try
        Dim strAddOnPath As String = installInfo.StrAddOnInstallPath

        If (IO.Directory.Exists(strAddOnPath)) Then
          ' Clear the directory if it already exists
          DeleteFiles(strAddOnPath)
        Else
          ' Create installation folder
          IO.Directory.CreateDirectory(strAddOnPath)
        End If

        ' Copy files to addOnPath directory
        ExtractFilesToAddOnPath(strAddOnPath)

        ' Save addon installation information into a registry for deinstallation
        RegisterAddOnInfo(strAddOnPath, strDllPath)

        ' Call AddOnInstallAPI.dll functions to inform B1 about the installation
        CallAddOnInstallAPI(strAddOnPath)

      Catch ex As Exception
        Dim ret As Int32
        MessageBox.Show("Error: " + ex.Message, _
                        "Error while trying to install the AddOn", _
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
#If Version = "2005" Then

        ' Call EnInstallEx to alert B1 there was an error during installation
        ' New function from 2005 version (no equivalent on 2004 version)
        Environment.CurrentDirectory = strDllPath ' For Dll function calls will work
        ret = EndInstallEx("", False)
      If ((installInfo.RestartNeeded And ret <> -1) Or _
          (Not installInfo.RestartNeeded And ret <> 0)) Then
          MessageBox.Show("EndInstallEx returned " + ret.ToString(), _
                          "Error while trying to install the AddOn", _
                          MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End If
#End If

      End Try

    End Sub


    '/// <summary>
    '/// Copies all the addon files into the path given as parameter.
    '/// </summary>
    '/// <param name="addOnPath">Path where the files should be copied</param>
    Protected Sub ExtractFilesToAddOnPath(ByVal addOnPath As String)

      Dim count As Integer

      ' Extra files
      For count = 0 To installInfo.ExtraFiles.Length - 1
        ExtractFile(installInfo.ExtraFiles.GetValue(count), _
                    installInfo.ExtraDirectories(count), _
                    addOnPath)
      Next count

      ' Interop files
      If (installInfo.DIFile <> "") Then
        ExtractFile(installInfo.DIFile, "", addOnPath)
      End If
      If (installInfo.UIFile <> "") Then
        ExtractFile(installInfo.UIFile, "", addOnPath)
      End If

      ' B1WizardBase file
      If (installInfo.B1WizardBaseFile <> "") Then
        ExtractFile(installInfo.B1WizardBaseFile, "", addOnPath)
      End If

      ' AddOn Exe file
      If (installInfo.ExeFile <> "") Then
        ExtractFile(installInfo.ExeFile, "", addOnPath)
      End If

    End Sub


    '/// <summary>
    '/// Copies a specific file into the path given as parameter.
    '/// </summary>
    '/// <param name="fileName">Name of the file to be copied</param>
    '/// <param name="folder">Directory inside the path where the file should be located</param>
    '/// <param name="addOnPath">Path where the file should be copied</param>
    Protected Sub ExtractFile(ByVal fileName As String, _
                              ByVal folder As String, _
                              ByVal addOnPath As String)

      Dim AddonFile As IO.FileStream
      Dim thisExe As System.Reflection.Assembly
      Dim file As System.IO.Stream
      Dim fileNameWOExt As String
      Dim buffer() As Byte

      ' Create specific folder if needed
      If (folder <> "") Then
        addOnPath += "\" + folder

        If Not (IO.Directory.Exists(addOnPath)) Then
          IO.Directory.CreateDirectory(addOnPath)
        End If
      End If

      ' Extract file name without extension
      fileNameWOExt = fileName.Substring(0, fileName.LastIndexOf("."))

      ' Obtain assembly of the addon install .exe
      thisExe = System.Reflection.Assembly.GetExecutingAssembly()

      ' Obtain stream containing the information of the file to copy
      file = thisExe.GetManifestResourceStream("AddOnInstaller." + fileName)
      If (file Is Nothing) Then
        MessageBox.Show(fileName + " file not found inside your installer.exe file!!!", _
          "Installer Error")
      End If

      ' Create a tmp file first, after file is extracted change to the real extension
      AddonFile = IO.File.Create(addOnPath & "\" & fileNameWOExt & ".tmp")
      ReDim buffer(file.Length)

      ' Read information stocked on the file
      file.Read(buffer, 0, file.Length)
      ' Write information into the tmp file
      AddonFile.Write(buffer, 0, file.Length)
      AddonFile.Close()
      ' Change file extension to exe
      IO.File.Move(addOnPath & "\" & fileNameWOExt & ".tmp", addOnPath & "\" & fileName)

    End Sub

    '/// <summary>
    '/// Calls AddOnInstallAPI.dll functions needed to complete the installation.
    '/// </summary>
    '/// <param name="addOnPath">Path where the addon should be installed</param>
    Protected Sub CallAddOnInstallAPI(ByVal addOnPath As String)

      Dim ret As Int32

      Environment.CurrentDirectory = strDllPath ' For Dll function calls will work

      ' Tell B1 where the exe file of the addon is located
      ret = SetAddOnFolder(addOnPath)
      If ret <> 0 Then
        MessageBox.Show("SetAddOnFolder(" + addOnPath + ") returned " + ret.ToString(), _
                        "Error while trying to install the AddOn", _
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      End If

      ' Tell B1 that PC must restart after addon installation
      If installInfo.RestartNeeded Then
        ret = RestartNeeded() ' Inform SBO the restart is needed
        If ret <> 0 Then
          MessageBox.Show("RestartNeeded() returned " + ret.ToString(), _
                          "Error while trying to install the AddOn", _
                          MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End If
      End If

      ' Inform SBO the installation ended
#If Version = "2005" Then
      ' Call EndInstallEx with isSucceeded = true
      ' Replaces EndInstall function of 2004 version
      ret = EndInstallEx(addOnPath, True)
      If ((installInfo.RestartNeeded And ret <> -1) Or _
          (Not installInfo.RestartNeeded And ret <> 0)) Then
        MessageBox.Show("EndInstallEx returned " + ret.ToString(), _
                        "Error while trying to install the AddOn", _
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      End If
#Else
      ' Call EndInstall (no parameters)
      ret = EndInstall()
      If ret <> 0 Then
        MessageBox.Show("EndInstall returned " + ret.ToString(), _
                        "Error while trying to install the AddOn", _
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
      End If
#End If
    End Sub

    '/// <summary>
    '/// Removes a directory given its path.
    '/// </summary>
    '/// <param name="addOnPath">Path</param>
    Protected Sub RemoveAllAddOnFiles(ByVal addOnPath As String)
      Try
        IO.Directory.Delete(addOnPath, True)
      Catch ex As Exception
      End Try
    End Sub

    '/// <summary>
    '/// Remove all files inside the path given as parameter.
    '/// </summary>
    '/// <param name="path"></param>
    Protected Sub DeleteFiles(ByVal path As String)
      Dim strFiles, strDirs As String()
      Dim file, dir As String

      strFiles = IO.Directory.GetFiles(path)

      If (strFiles.Length >= 1) Then
        For Each file In strFiles
          IO.File.Delete(file)
        Next
      End If

      strDirs = IO.Directory.GetDirectories(path)

      If (strDirs.Length >= 1) Then
        For Each dir In strDirs
          IO.Directory.Delete(dir, True)
        Next
      End If

    End Sub
    '/// <summary>
    '/// Saves the addOn location path and AddOnInstallAPIdll path into registry values.
    '/// Called during installation.
    '/// This information is needed during uninstall of the addon.
    '/// </summary>
    '/// <param name="addOnLocation">Path where the addon should be installed</param>
    '/// <param name="dllPath">Path where the AddOnInstallAPIdll is located</param>
    Protected Sub RegisterAddOnInfo(ByVal addOnLocation As String, ByVal dllPath As String)

      Dim regParam As RegistryKey

      regParam = Registry.CurrentUser.OpenSubKey(keyValue, True)
      If regParam Is Nothing Then
        ' Key doesn't exist; create it.
        regParam = Registry.CurrentUser.CreateSubKey(keyValue)
      End If

      If (Not regParam Is Nothing) Then
        regParam.SetValue(subKeyAddOnInstallDirValue, addOnLocation)
        regParam.SetValue(subKeyDllPathValue, dllPath)
        regParam.Close()
      End If

    End Sub

    '/// <summary>
    '/// Reads the addOn location path and AddOnInstallAPIdll path from registry values.
    '/// Called during uninstall.
    '/// </summary>
    '/// <returns>Returns the addOn path</returns>
    Protected Function ReadRegisteredAddOnInfo() As String

      Dim regParam As RegistryKey
      Dim addOnLocation As String = ""

      regParam = Registry.CurrentUser.OpenSubKey(keyValue, True)
      If (Not regParam Is Nothing) Then
        addOnLocation = regParam.GetValue(subKeyAddOnInstallDirValue)
        strDllPath = regParam.GetValue(subKeyDllPathValue)
        regParam.Close()
      End If

      Return addOnLocation

    End Function

    '/// <summary>
    '/// Removes the addOn location path and AddOnInstallAPIdll path from the registry values.
    '/// </summary>
    Private Sub UnregisterAddOnInfo()

      Dim regParam As RegistryKey

      regParam = _
        Registry.CurrentUser.OpenSubKey( _
          "SOFTWARE\\SAP\\SAP Manage\\SAP Business One\\AddOnInstaller" _
          + "\\" + installInfo.PartnerName, True)
      If (Not regParam Is Nothing) Then
        regParam.DeleteSubKey(installInfo.AddOnName, True)
        regParam.Close()
      End If

    End Sub

    '/// <summary>
    '/// Returns the addOn path where the addon should be installed.
    '/// </summary>
    '/// <returns>AddOn path</returns>
    Public Function GetAddOnPath() As String
      Return installInfo.StrAddOnInstallPath
    End Function

    '/// <summary>
    '/// Returns the AddOnInstallInfo of the AddOn to be installed by this installer.
    '/// </summary>
    '/// <returns>AddOnInstallInfo reference</returns>
    Public Function GetAddOnInstallInfo() As AddOnInstallInfo
      Return installInfo
    End Function

  End Class
End Namespace
