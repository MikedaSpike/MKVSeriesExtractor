Imports System.IO
Imports System.Reflection
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Microsoft.Win32
<Assembly: AssemblyVersion("25.08.17")>

Public Class MakeMKVSerieExtractor
    Private activeListBox As ListBox
    Private exeName As String = Path.GetFileNameWithoutExtension(Application.ExecutablePath)
    Private iniPath As String = Path.Combine(Application.StartupPath, exeName & ".ini")
    Private logFile As String = Path.Combine(Application.StartupPath, Application.ProductName & ".log")
    Private Extractor As ExtractMKVdvd = Nothing
    Private IniFile As DracLabs.IniFile = Nothing
    Private LoadTitleStatus As Boolean = True
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        If Not File.Exists(iniPath) Then
            File.WriteAllText(iniPath, "[Settings]" & vbCrLf &
                  "MakeMKVPath=" & vbCrLf &
                  "InputFolder=" & vbCrLf &
                  "OutputFolder=" & vbCrLf &
                  "TitleList=" & vbCrLf &
                  "ShowName=" & vbCrLf &
                  "EpisodeDuration=44")
        End If
        IniFile = New DracLabs.IniFile
        IniFile.Load(iniPath)
        txtMakeMKVPath.Text = IniFile.GetKeyValue("Settings", "MakeMKVPath")
        If String.IsNullOrWhiteSpace(txtMakeMKVPath.Text) Then
            Dim mkvPath = GetMakeMKVPathFromRegistry()
            If mkvPath IsNot Nothing Then
                txtMakeMKVPath.Text = mkvPath
                AppendTxtToScreen($"[🧠] MakeMKV found via registry: {mkvPath}")
                txtMakeMKVPath_Leave(txtMakeMKVPath, EventArgs.Empty)
            Else
                AppendTxtToScreen("[❌] MakeMKV not found in registry. Please use the Browse button to manually select the correct executable.")
            End If

        End If
        txtInputPath.Text = IniFile.GetKeyValue("Settings", "InputFolder")
        txtOutputPath.Text = IniFile.GetKeyValue("Settings", "OutputFolder")
        txtTitleFile.Text = IniFile.GetKeyValue("Settings", "TitleList")
        If String.IsNullOrWhiteSpace(txtTitleFile.Text) Then
            cbxGenerateTitles.Checked = False
        End If
        txtSeriesName.Text = IniFile.GetKeyValue("Settings", "ShowName")

        Dim durationStr = IniFile.GetKeyValue("Settings", "EpisodeDuration")
        Dim durationVal As Decimal = nudExpectedDuration.Value
        If Decimal.TryParse(durationStr, durationVal) Then
            nudExpectedDuration.Value = Math.Min(Math.Max(durationVal, nudExpectedDuration.Minimum), nudExpectedDuration.Maximum)
        Else
            nudExpectedDuration.Value = 0
        End If

        AppendTxtToScreen($"LOG:")
        File.WriteAllText(logFile, $"Program {My.Application.Info.Title} {My.Application.Info.Version} started.{Environment.NewLine}")

        If File.Exists(txtTitleFile.Text) Then
            lblStatus.Text = "Loading titles. Please wait..."
            Me.Show()
            Me.Refresh()
            LoadTitles(txtTitleFile.Text)
            lblStatus.Text = ""
        End If

        If Directory.Exists(txtInputPath.Text) Then
            LoadDVDDirectories()
        End If

    End Sub
#Region "Control Handlers"
    Private Sub btnSelectOutputFolder_Click(sender As Object, e As EventArgs) Handles btnSelectOutputFolder.Click
        Using folderDialog As New FolderBrowserDialog()
            folderDialog.Description = "Select the output folder"
            If Directory.Exists(txtOutputPath.Text) Then
                folderDialog.SelectedPath = txtOutputPath.Text
            End If
            If folderDialog.ShowDialog() = DialogResult.OK Then
                txtOutputPath.Text = folderDialog.SelectedPath
                IniFile.SetKeyValue("Settings", "OutputFolder", folderDialog.SelectedPath)
                IniFile.Save(iniPath)
            End If
        End Using
    End Sub

    Private Sub btnSelectInputFolder_Click(sender As Object, e As EventArgs) Handles btnSelectInputFolder.Click
        Using folderDialog As New FolderBrowserDialog()
            folderDialog.Description = "Select the input folder"
            If Directory.Exists(txtInputPath.Text) Then
                folderDialog.SelectedPath = txtInputPath.Text
            End If
            If folderDialog.ShowDialog() = DialogResult.OK Then
                txtInputPath.Text = folderDialog.SelectedPath
                LoadDVDDirectories()
                IniFile.SetKeyValue("Settings", "InputFolder", folderDialog.SelectedPath)
                IniFile.Save(iniPath)
            End If
        End Using
    End Sub

    Private Sub btnSelectTitleFilee_Click(sender As Object, e As EventArgs) Handles btnSelectTitleFile.Click
        Using openFileDialog As New OpenFileDialog()
            openFileDialog.Filter = "Text files (*.txt)|*.txt"
            openFileDialog.Title = "Select title list"
            If File.Exists(txtTitleFile.Text) Then
                openFileDialog.InitialDirectory = Path.GetDirectoryName(txtTitleFile.Text)
                openFileDialog.FileName = Path.GetFileName(txtTitleFile.Text)
            End If
            If openFileDialog.ShowDialog() = DialogResult.OK Then
                txtTitleFile.Text = openFileDialog.FileName
                LoadTitles(openFileDialog.FileName)
                IniFile.SetKeyValue("Settings", "TitleList", openFileDialog.FileName)
                IniFile.Save(iniPath)
            End If
        End Using
    End Sub

    Private Sub btnBrowseMakeMKV_Click(sender As Object, e As EventArgs) Handles btnBrowseMakeMKV.Click
        Using ofd As New OpenFileDialog()
            ofd.Title = "Select MakeMKV executable"
            ofd.Filter = "EXE files (*.exe)|*.exe"
            If ofd.ShowDialog() = DialogResult.OK Then
                txtMakeMKVPath.Text = ofd.FileName
                IniFile.SetKeyValue("Settings", "MakeMKVPath", ofd.FileName)
                IniFile.Save(iniPath)
            End If
        End Using
    End Sub

    Private Sub nudExpectedDuration_ValueChanged(sender As Object, e As EventArgs) Handles nudExpectedDuration.ValueChanged
        If IniFile IsNot Nothing Then
            IniFile.SetKeyValue("Settings", "EpisodeDuration", nudExpectedDuration.Value.ToString())
            IniFile.Save(iniPath)
        End If
    End Sub
    Private Sub txtInputPath_Leave(sender As Object, e As EventArgs) Handles txtInputPath.Leave
        If IniFile IsNot Nothing Then
            If Directory.Exists(txtInputPath.Text) Then
                LoadDVDDirectories()
            Else
                lstDVDMaps.Items.Clear()
                If cbxGenerateTitles.Checked Then
                    GenerateTitlesFromDVDMaps()
                End If
            End If
            IniFile.SetKeyValue("Settings", "InputFolder", txtInputPath.Text)
            IniFile.Save(iniPath)
        End If
    End Sub

    Private Sub txtOutputPath_Leave(sender As Object, e As EventArgs) Handles txtOutputPath.Leave
        If IniFile IsNot Nothing Then
            IniFile.SetKeyValue("Settings", "OutputFolder", txtOutputPath.Text)
            IniFile.Save(iniPath)
        End If
    End Sub

    Private Sub txtTitleFile_Leave(sender As Object, e As EventArgs) Handles txtTitleFile.Leave
        If IniFile IsNot Nothing Then
            If File.Exists(txtTitleFile.Text) Then
                LoadTitles(txtTitleFile.Text)
            Else
                lstTitles.Items.Clear()
            End If
            IniFile.SetKeyValue("Settings", "TitleList", txtTitleFile.Text)
            IniFile.Save(iniPath)
        End If
    End Sub

    Private Sub txtMakeMKVPath_Leave(sender As Object, e As EventArgs) Handles txtMakeMKVPath.Leave
        If IniFile IsNot Nothing Then
            IniFile.SetKeyValue("Settings", "MakeMKVPath", txtMakeMKVPath.Text)
            IniFile.Save(iniPath)
        End If
    End Sub

    Private Sub txtSeriesName_Leave(sender As Object, e As EventArgs) Handles txtSeriesName.Leave
        If IniFile IsNot Nothing Then
            IniFile.SetKeyValue("Settings", "ShowName", txtSeriesName.Text)
            IniFile.Save(iniPath)
        End If
    End Sub

    Private Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        btnStop.Text = "⏹️ Stop"
        ToolTip.SetToolTip(btnStop, "Stop the current operation. Interrupting the process may cause an error.")
        btnStart.Enabled = False
        Dim outputPath As String = txtOutputPath.Text
        Dim makeMKVPath As String = txtMakeMKVPath.Text
        Dim seriesName As String = txtSeriesName.Text.Trim()
        Try
            SetControlsDuringOperation(True)
            If String.IsNullOrWhiteSpace(seriesName) Then
                MessageBox.Show("Please enter a series name before starting.", "Missing input", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Extractor = New ExtractMKVdvd()
            Extractor.CancelRequested = False

            If Not File.Exists(makeMKVPath) Then
                MessageBox.Show("The specified path to MakeMKV is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If
            If Not Directory.Exists(outputPath) Then
                MessageBox.Show("The output folder does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            Dim totalItems As Integer = lstDVDMaps.Items.Count
            If totalItems = 0 Then
                MessageBox.Show("No DVD folders found.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            If nudExpectedDuration.Value <= 1 Then
                MessageBox.Show("Please set a valid expected episode duration.", "Invalid Duration", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            progressBar.Value = 0
            progressBar.Maximum = CInt(Math.Round(lstDVDMaps.Items.Count + lstTitles.Items.Count + 2)) 'For the mapping Step
            txtLog.Clear()

            Dim mappedLetter As String = ""
            Dim uncRoot As String = ""
            If lstDVDMaps.Items.Cast(Of String).Any(Function(p) p.StartsWith("\\")) Then
                Try
                    lblStatus.Text = "Mapping network share..."
                    lblStatus.Refresh()
                    uncRoot = String.Join("\", lstDVDMaps.Items(0).ToString().Split("\"c).Take(4))
                    mappedLetter = UNCMapper.GetFreeDriveLetter()
                    If Not UNCMapper.MapDrive(uncRoot, mappedLetter) Then
                        MessageBox.Show($"Mapping to {mappedLetter}: to {uncRoot} failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Return
                    End If
                    progressBar.Value += 1
                    AppendTxtToScreen($"[🌐] UNC mapped as {mappedLetter}:\ to {uncRoot}")
                    Application.DoEvents()
                Catch ex As Exception
                    MessageBox.Show($"❌ Mapping failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return
                End Try
            End If
            Dim episodeIndex As Integer = 0

            For i As Integer = 0 To totalItems - 1
                lblStatus.Text = $"Preparing extraction for item {i + 1} of {totalItems}..."
                Dim selectedTracks As List(Of Integer)
                lstDVDMaps.ClearSelected()
                If i < lstDVDMaps.Items.Count Then lstDVDMaps.SelectedIndex = i
                If Extractor.CancelRequested Then
                    AppendTxtToScreen("[🛑] Extraction cancelled by user.")
                    lblStatus.Text = "Extraction cancelled."
                    Exit For
                End If

                Dim sourcePath As String = lstDVDMaps.Items(i).ToString()
                Dim rawTitle As String = If(episodeIndex < lstTitles.Items.Count, lstTitles.Items(episodeIndex).ToString(), $"Title_{episodeIndex + 1}")
                Dim tempExtractFolder As String = Path.Combine(outputPath, $"temp_extract")
                Directory.CreateDirectory(tempExtractFolder)

                Dim existingMKVs = Directory.GetFiles(tempExtractFolder, "*.mkv")
                If existingMKVs.Any() Then
                    If i = 0 Then
                        AppendTxtToScreen("[⚠️] Temp folder already contains an MKV file. Please remove it before continuing.")
                        Dim result As DialogResult = MessageBox.Show(
                                                                    "The temporary folder already contains an MKV file." & vbCrLf &
                                                                    "Do you want to open the folder to clean it?",
                                                                    "Error",
                                                                    MessageBoxButtons.YesNo,
                                                                    MessageBoxIcon.Error
                                                                    )
                        If result = DialogResult.Yes Then
                            Process.Start("explorer.exe", tempExtractFolder)
                        End If
                        Exit For
                    Else
                        AppendTxtToScreen("❌ Unexpected MKV file found in temp folder during batch. Aborting to avoid overwrite.")
                        MessageBox.Show("An MKV file was found in the temp folder during batch processing. This may indicate a failed cleanup or overwrite risk.", "Critical Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Exit For
                    End If
                End If

                Dim dvdTitle = Path.GetFileName(sourcePath)
                lblStatus.Text = $"Reading DVD: {dvdTitle}"
                lblStatus.Refresh()
                AppendTxtToScreen($"[📀] Reading DVD {dvdTitle} started...")

                Dim infoOutput As String = ""

                Try
                    infoOutput = Extractor.GetMakeMKVInfo(makeMKVPath, sourcePath, 120)
                    lblStatus.Text = $"Info collected from DVD: {dvdTitle}"
                Catch ex As TimeoutException
                    AppendTxtToScreen("[⏱️] Timeout: retrieving info took too long. Batch will be stopped.")
                    Extractor.CancelRequested = True
                    Exit Sub
                End Try

                If Extractor.CancelRequested Then Exit Sub

                progressBar.Value += 1
                progressBar.Refresh()

                Dim MakeMKVTable As DataTable = Extractor.BuildTrackDataTable(infoOutput)
                AppendTxtToScreen($"[📊] Parsed {MakeMKVTable.Rows.Count} title(s) into DataTable.")
                LogDataTable(MakeMKVTable, Sub(msg) AppendToLog(msg))

                lblStatus.Text = "Selecting tracks for extraction."
                selectedTracks = Extractor.ParseTrackNumbers(MakeMKVTable, nudExpectedDuration.Value, AddressOf AppendToLog)

                Dim previewIndex As Integer = episodeIndex
                Dim episodeCounter As Integer = 1

                For Each track As Integer In selectedTracks
                    Dim titleLine As String
                    If Not cbxGenerateTitles.Checked Then
                        If previewIndex < lstTitles.Items.Count Then
                            titleLine = lstTitles.Items(previewIndex).ToString()
                        Else
                            titleLine = $"Title_{previewIndex + 1}"
                            AppendTxtToScreen($"[ℹ️] No title found for index {previewIndex}, using placeholder: {titleLine}")
                        End If
                        Dim pattern As String = "S(\d{2})E(\d{2})"
                        Dim match As Match = Regex.Match(titleLine, pattern)
                        If match.Success Then
                            Dim season As String = match.Groups(1).Value
                            Dim episode As String = match.Groups(2).Value
                            Dim cleanTitle As String = Regex.Replace(titleLine, pattern & "[:\- ]*", "")
                            cleanTitle = Regex.Replace(cleanTitle, "[^a-zA-Z0-9\s]", "").Trim()
                            Dim newName As String = $"{seriesName} - S{season}E{episode} - {cleanTitle}.mkv"
                            AppendTxtToScreen($"[✅] Track #{track} will be extracted as: {newName}")
                        Else
                            AppendTxtToScreen($"[⚠️] Track #{track} selected, but no SxxExx pattern found in: {titleLine}")
                        End If
                        previewIndex += 1
                    Else
                        lstTitles.SelectedIndex = lstDVDMaps.SelectedIndex
                        Dim currentDvdTitle As String = lstTitles.SelectedItem.ToString()
                        titleLine = $"{currentDvdTitle}_E{episodeCounter:D2}"
                        Dim newName As String = $"{titleLine}.mkv"
                        AppendTxtToScreen($"[✅] Track #{track} will be extracted as: {newName}")
                        episodeCounter += 1
                    End If

                Next

                episodeCounter = 1
                For Each track As Integer In selectedTracks
                    If Extractor.CancelRequested Then Exit For
                    lstTitles.ClearSelected()
                    Dim Episodetitle As String

                    If cbxGenerateTitles.Checked Then
                        If episodeIndex < lstTitles.Items.Count Then
                            lstTitles.SelectedIndex = episodeIndex
                            Episodetitle = lstTitles.Items(episodeIndex).ToString()
                        Else
                            MessageBox.Show($"No title found for episode index {episodeIndex}. Please check your title file.", "Missing Title", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                            AppendTxtToScreen($"[❌] Missing title for episode index {episodeIndex}. Stopping batch.")
                            Exit For
                        End If
                    Else
                        lstTitles.SelectedIndex = lstDVDMaps.SelectedIndex
                        Dim currentDvdTitle As String = lstTitles.SelectedItem.ToString()
                        Episodetitle = $"{currentDvdTitle}_E{episodeCounter:D2}"
                        episodeCounter += 1
                    End If

                    lblStatus.Text = $"Extracting track #{track} ({Episodetitle})..."
                    lblStatus.Refresh()
                    Application.DoEvents()

                    Dim extractedMKV As String = Extractor.ExtractSingleTrack(makeMKVPath, sourcePath, track, tempExtractFolder, AddressOf AppendTxtToScreen)
                    If String.IsNullOrWhiteSpace(extractedMKV) OrElse Not File.Exists(extractedMKV) Then
                        AppendTxtToScreen($"❌ Extraction failed for track #{track}. File not found.")
                        MessageBox.Show($"Extraction failed for track #{track}. Stopping batch.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        Exit For
                    End If

                    If Not cbxGenerateTitles.Checked Then
                        Extractor.MoveAndRenameMkv(extractedMKV, outputPath, seriesName, Episodetitle, AddressOf AppendTxtToScreen)
                    Else
                        Extractor.MoveMkvToOutput(extractedMKV, outputPath, Episodetitle, AddressOf AppendTxtToScreen)
                    End If

                    episodeIndex += 1
                    progressBar.Value += 1
                    progressBar.Refresh()
                    Application.DoEvents()
                Next

                If Extractor.CancelRequested Then Exit For
            Next

            If mappedLetter <> "" Then
                UNCMapper.UnmapDrive(mappedLetter)
                progressBar.Value += 1
                progressBar.Refresh()
                Application.DoEvents()
                AppendTxtToScreen($"[📤] Mapping {mappedLetter}:\ removed.")
            End If
            If Not Extractor.CancelRequested Then
                lblStatus.Text = "Done!"
                progressBar.Value = progressBar.Maximum
                Me.Refresh()
                Application.DoEvents()
                Thread.Sleep(1000)
            Else
                AppendTxtToScreen("[🛑] Cancel request received: process stopped.")
            End If

        Catch ex As Exception
            AppendTxtToScreen($"❌ Something went wrong: {ex.Message}")
        Finally
            If Extractor IsNot Nothing Then Extractor = Nothing
            lstDVDMaps.ClearSelected()
            lstTitles.ClearSelected()
            btnStart.Enabled = True
            btnStop.Enabled = True
            progressBar.Value = 0
            btnStop.Text = "❌ Quit"
            ToolTip.SetToolTip(btnStop, "Close the application")
            SetControlsDuringOperation(False)
        End Try
    End Sub

    Private Sub lstDVDMaps_MouseDown(sender As Object, e As MouseEventArgs) Handles lstDVDMaps.MouseDown
        If e.Button = MouseButtons.Right Then
            activeListBox = CType(sender, ListBox)
        End If
    End Sub

    Private Sub lstTitles_MouseDown(sender As Object, e As MouseEventArgs) Handles lstTitles.MouseDown
        If e.Button = MouseButtons.Right Then
            activeListBox = CType(sender, ListBox)
        End If
    End Sub

    Private Sub cbxGenerateTitles_CheckedChanged(sender As Object, e As EventArgs) Handles cbxGenerateTitles.CheckedChanged
        If lstDVDMaps.Items.Count > 0 Then
            txtTitleFile.Enabled = Not cbxGenerateTitles.Checked
            btnSelectTitleFile.Enabled = Not cbxGenerateTitles.Checked
            lstTitles.Enabled = Not cbxGenerateTitles.Checked
            If Not cbxGenerateTitles.Checked Then
                If File.Exists(txtTitleFile.Text) Then
                    LoadTitles(txtTitleFile.Text)
                Else
                    lstTitles.Items.Clear()
                    AppendTxtToScreen("[ℹ️] No title file found, please select one.")
                End If
            Else
                lstTitles.Items.Clear()
                GenerateTitlesFromDVDMaps()
            End If
        Else
            lstTitles.Items.Clear()
        End If
    End Sub

    Private Sub copyToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles copyToolStripMenuItem.Click
        If btnStart.Enabled AndAlso activeListBox IsNot Nothing AndAlso activeListBox.SelectedItems.Count > 0 Then
            Dim copiedText As String = String.Join(Environment.NewLine, activeListBox.SelectedItems.Cast(Of String))
            Clipboard.SetText(copiedText)
        End If
    End Sub

    Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        If Extractor IsNot Nothing Then
            Extractor.CancelRequested = True
            AppendTxtToScreen("[🛑] STOP requested by user...")

            Dim processPath = txtMakeMKVPath.Text
            Dim processName = Path.GetFileNameWithoutExtension(processPath)

            For Each p In Process.GetProcessesByName(processName)
                Try
                    p.Kill()
                    AppendTxtToScreen("[✅] MakeMKV process [{processName}] terminated.")
                Catch ex As Exception
                    AppendTxtToScreen($"[❌] Could not stop MakeMKV: {ex.Message}")
                End Try
            Next
            progressBar.Value = 0
        ElseIf btnStop.Text = "❌ Quit" Then
            Dim result As DialogResult = MessageBox.Show("Do you want to exit the program?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If result = DialogResult.Yes Then
                AppendTxtToScreen("[✅] Program will exit...")
                Application.Exit()
            Else
                AppendTxtToScreen("[ℹ️] Program will remain active.")
            End If

        End If
    End Sub
    Private Sub btnAnalyseTracks_Click(sender As Object, e As EventArgs) Handles btnAnalyseTracks.Click
        Try

            Dim makeMKVPath As String = txtMakeMKVPath.Text
            Dim selectedEntry As String = String.Empty
            Dim infoOutput As String = ""

            SetControlsDuringOperation(True)
            btnStop.Text = "⏹️ Stop"
            ToolTip.SetToolTip(btnStop, "Stop the current operation. Interrupting the process may cause an error.")
            If lstDVDMaps.SelectedItems.Count <> 1 Then
                MessageBox.Show("Please select exactly one DVD folder.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            Else
                selectedEntry = lstDVDMaps.SelectedItem.ToString
            End If

            Extractor = New ExtractMKVdvd()
            Try
                lblStatus.Text = $"Please wait while collecting data from {Path.GetFileName(selectedEntry)}"
                lblStatus.Refresh()
                infoOutput = Extractor.GetMakeMKVInfo(makeMKVPath, selectedEntry, 120)
            Catch ex As TimeoutException
                AppendTxtToScreen("[⏱️] Timeout: retrieving info took too long. Not able to show user .")
                Exit Sub
            End Try
            If Extractor.CancelRequested Then Exit Sub
            btnStop.Enabled = False
            lblStatus.Text = "Data collected, opening new form"
            lblStatus.Refresh()
            Dim rawTrackTable As DataTable = Extractor.BuildTrackDataTable(infoOutput)
            Dim annotatedTable = Extractor.GetAnnotatedTrackTable(rawTrackTable, AddressOf AppendTxtToScreen)

            Dim viewer As New frmTrackGroupViewer(annotatedTable)
            If viewer.ShowDialog() = DialogResult.OK Then
                Dim avgDuration As Integer = CInt(Math.Round(viewer.SelectedAverageDuration))
                Debug.WriteLine("avgDuration: " & avgDuration)
                Debug.WriteLine("Minimum: " & nudExpectedDuration.Minimum)
                Debug.WriteLine(avgDuration >= nudExpectedDuration.Minimum)
                Debug.WriteLine(avgDuration <= nudExpectedDuration.Maximum)
                If avgDuration >= nudExpectedDuration.Minimum AndAlso avgDuration <= nudExpectedDuration.Maximum Then
                    nudExpectedDuration.Value = CDec(avgDuration)
                Else
                    MessageBox.Show("Selected group duration is outside the allowed range. Likely a movie or unsupported format.", "Invalid Duration", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                End If
            End If
        Catch ex As Exception
            AppendTxtToScreen($"❌ Something went wrong: {ex.Message}")
        Finally
            SetControlsDuringOperation(False)
            If Extractor IsNot Nothing Then Extractor = Nothing
            btnStop.Text = "❌ Quit"
            ToolTip.SetToolTip(btnStop, "Close the application")
            btnStop.Enabled = True
        End Try

    End Sub

    Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        Dim result As DialogResult = MessageBox.Show("Are you sure you want to reset the form to its default values?", "Confirm Reset", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If result = DialogResult.Yes Then
            ResetForm()
        End If
    End Sub
#End Region
    ''' <summary>
    ''' Updates control states based on whether an operation is in progress.
    ''' </summary>
    ''' <param name="isLocked">True to lock user interaction; False to enable controls.</param>
    Private Sub SetControlsDuringOperation(isLocked As Boolean)
        For Each ctrl As Control In Me.Controls
            If ctrl Is btnStop OrElse ctrl Is txtLog OrElse ctrl Is lblStatus Then
                ctrl.Enabled = True
            ElseIf ctrl Is lstDVDMaps OrElse ctrl Is lstTitles Then
                ctrl.Enabled = True
                ctrl.TabStop = Not isLocked
            Else
                ctrl.Enabled = Not isLocked
            End If
        Next

        lstDVDMaps.Enabled = True
        lstTitles.Enabled = True
        lstDVDMaps.TabStop = Not isLocked
        lstTitles.TabStop = Not isLocked
        lstDVDMaps.UserInteractionLocked = isLocked
        lstTitles.UserInteractionLocked = isLocked
    End Sub
    ''' <summary>
    ''' Generates display titles from DVD map paths, appending instance numbers for duplicates.
    ''' </summary>
    Private Sub GenerateTitlesFromDVDMaps()
        lstTitles.Items.Clear()

        Dim dvdCountMap As New Dictionary(Of String, Integer)

        ' First pass: count occurrences of clean DVD names
        For Each fullPath As String In lstDVDMaps.Items
            Dim dvdName As String = Path.GetFileNameWithoutExtension(fullPath)

            If dvdCountMap.ContainsKey(dvdName) Then
                dvdCountMap(dvdName) += 1
            Else
                dvdCountMap(dvdName) = 1
            End If
        Next

        Dim dvdInstanceMap As New Dictionary(Of String, Integer)
        For Each fullPath As String In lstDVDMaps.Items
            Dim dvdName As String = Path.GetFileNameWithoutExtension(fullPath)
            Dim finalName As String

            If dvdCountMap(dvdName) > 1 Then
                If Not dvdInstanceMap.ContainsKey(dvdName) Then
                    dvdInstanceMap(dvdName) = 1
                Else
                    dvdInstanceMap(dvdName) += 1
                End If
                finalName = $"{dvdName}_DVD{dvdInstanceMap(dvdName):D2}"
            Else
                finalName = dvdName
            End If

            lstTitles.Items.Add(finalName)
        Next
    End Sub
    ''' <summary>
    ''' Loads titles from a file into the list and validates them for uniqueness and correctness.
    ''' </summary>
    ''' <param name="filePath">Path to the file containing title entries.</param>
    Private Sub LoadTitles(filePath As String)
        If File.Exists(filePath) Then
            Dim titles As String() = File.ReadAllLines(filePath)
            lstTitles.Items.Clear()
            lstTitles.Items.AddRange(titles)
            If ValidateTitles() Then
                AppendTxtToScreen("[✅] Validate Titles : All titles are valid and unique.")
            Else
                MessageBox.Show("Errors detected during import titles", "Validate Titles Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        End If
    End Sub
    ''' <summary>
    ''' Validates title entries for correct episode format and uniqueness.
    ''' </summary>
    ''' <returns>True if all titles are valid and unique; otherwise, False.</returns>
    Private Function ValidateTitles() As Boolean
        Dim pattern As String = "S(\d{2})E(\d{2})"
        Dim seenEpisodes As New HashSet(Of String)
        Dim hasErrors As Boolean = False

        For Each item As String In lstTitles.Items
            Dim titleLine As String = item.ToString()
            Dim match As Match = Regex.Match(titleLine, pattern)

            If Not match.Success Then
                AppendTxtToScreen($"[⚠️] Validate Titles : Invalid format (missing SxxExx): {titleLine}")
                hasErrors = True
            Else
                Dim episodeKey As String = match.Value ' e.g., "S01E02"
                If seenEpisodes.Contains(episodeKey) Then
                    AppendTxtToScreen($"[⚠️] Validate Titles : Duplicate episode detected: {episodeKey} in title '{titleLine}'")
                    hasErrors = True
                Else
                    seenEpisodes.Add(episodeKey)
                End If
            End If
        Next

        If Not hasErrors Then

            Return True
        Else

            Return False
        End If
    End Function
    ''' <remarks>
    ''' Adds folders containing VIDEO_TS or ISO files to the list.  
    ''' Automatically generates titles if the corresponding checkbox is checked.
    ''' </remarks>
    Private Sub LoadDVDDirectories()
        Dim inputPath As String = txtInputPath.Text
        If Not Directory.Exists(inputPath) Then
            MessageBox.Show("Input folder does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        lstDVDMaps.Items.Clear()

        Dim allFolders As New List(Of String)
        allFolders.Add(inputPath)
        allFolders.AddRange(Directory.GetDirectories(inputPath, "*", SearchOption.AllDirectories))

        Dim dvdCandidates As New List(Of String)

        For Each folder As String In allFolders
            Dim hasVideoTS As Boolean = Directory.Exists(Path.Combine(folder, "VIDEO_TS"))
            Dim isoFiles As String() = Directory.GetFiles(folder, "*.iso", SearchOption.TopDirectoryOnly)

            If hasVideoTS Then
                dvdCandidates.Add(folder) ' DVD folder found
            ElseIf isoFiles.Length >= 1 Then
                dvdCandidates.AddRange(isoFiles) ' At least one ISO found
            ElseIf isoFiles.Length > 1 Then
                AppendTxtToScreen($"Note: multiple ISO files found in: {folder}")
            End If
        Next

        For Each item As String In dvdCandidates.OrderBy(Function(x) x, StringComparer.CurrentCultureIgnoreCase)
            lstDVDMaps.Items.Add(item)
        Next
        If cbxGenerateTitles.Checked Then
            GenerateTitlesFromDVDMaps()
        End If

    End Sub

    ''' <summary>
    ''' Appends a message to the log display and internal log, ensuring UI updates and scroll position.
    ''' </summary>
    ''' <param name="message">The message to append to the log.</param>
    Public Sub AppendTxtToScreen(message As String)
        txtLog.AppendText(message & vbCrLf)
        txtLog.SelectionStart = txtLog.Text.Length
        txtLog.ScrollToCaret()
        txtInputPath.Refresh()
        Application.DoEvents()
        AppendToLog(message)
    End Sub

    ''' <summary>
    ''' Appends a timestamped message to the log file.
    ''' </summary>
    ''' <param name="message">The message to log.</param>
    Public Sub AppendToLog(message As String)
        Dim timestampedText As String = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}"
        File.AppendAllText(logFile, timestampedText & Environment.NewLine)
    End Sub
    ''' <summary>
    ''' Logs the contents of a <see cref="DataTable"/> row by row, including column headers.
    ''' </summary>
    ''' <param name="table">The <see cref="DataTable"/> to log.</param>
    ''' <param name="logAction">
    ''' Optional logging delegate to handle output. If not provided, no logging occurs.
    ''' </param>
    ''' <remarks>
    ''' - Empty or null tables are reported with a placeholder message.
    ''' - Handles various data types including strings, IEnumerable(Of String), and other IEnumerable types.
    ''' - Null and DBNull values are logged as empty strings.
    ''' </remarks>
    Public Sub LogDataTable(table As DataTable, Optional logAction As Action(Of String) = Nothing)
        If table Is Nothing OrElse table.Rows.Count = 0 Then
            logAction?.Invoke("[📊] DataTable is empty.")
            Return
        End If

        Dim headers = String.Join(" | ", table.Columns.Cast(Of DataColumn).Select(Function(col) col.ColumnName))
        logAction?.Invoke($"[📊] Columns: {headers}")

        For i As Integer = 0 To table.Rows.Count - 1
            Dim rowIndex = i
            Dim rowValues = String.Join(" | ", table.Columns.Cast(Of DataColumn).Select(Function(col)
                                                                                            Dim value = table.Rows(rowIndex)(col)

                                                                                            If value Is Nothing OrElse DBNull.Value.Equals(value) Then
                                                                                                Return ""
                                                                                            End If

                                                                                            ' Prevent treating strings as IEnumerable
                                                                                            If TypeOf value Is String Then
                                                                                                Return value.ToString()
                                                                                            End If

                                                                                            ' Handle List(Of String)
                                                                                            If TypeOf value Is IEnumerable(Of String) Then
                                                                                                Return String.Join(",", CType(value, IEnumerable(Of String)))
                                                                                            End If

                                                                                            ' Handle other IEnumerable types (fallback)
                                                                                            If TypeOf value Is IEnumerable Then
                                                                                                Dim items = CType(value, IEnumerable).Cast(Of Object).Select(Function(obj) obj?.ToString()).ToList()
                                                                                                Return String.Join(",", items)
                                                                                            End If

                                                                                            Return value.ToString()
                                                                                        End Function))

            logAction?.Invoke($"[📄] Row {i + 1}: {rowValues}")
        Next
    End Sub

    ''' <summary>
    ''' Attempts to locate the MakeMKV executable by scanning the Windows registry for uninstall entries.
    ''' </summary>
    ''' <returns>
    ''' The full path to <c>makemkvcon64.exe</c> or <c>makemkvcon.exe</c> if found; otherwise, <c>Nothing</c>.
    ''' </returns>
    Private Function GetMakeMKVPathFromRegistry() As String
        Try

            Dim uninstallKeys As String() = {
                "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                "SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            }

            For Each rootKey In uninstallKeys
                Using key As RegistryKey = Registry.LocalMachine.OpenSubKey(rootKey)
                    If key Is Nothing Then Continue For

                    For Each subKeyName In key.GetSubKeyNames()
                        Using subKey As RegistryKey = key.OpenSubKey(subKeyName)
                            If subKey Is Nothing Then Continue For

                            Dim displayName = subKey.GetValue("DisplayName")?.ToString()
                            If displayName IsNot Nothing AndAlso displayName.ToLower().Contains("makemkv") Then
                                Dim uninstallString = subKey.GetValue("UninstallString")?.ToString()
                                If uninstallString IsNot Nothing Then
                                    Dim installDir = Path.GetDirectoryName(uninstallString)
                                    Dim possibleExe = Path.Combine(installDir, "makemkvcon64.exe")
                                    If File.Exists(possibleExe) Then Return possibleExe

                                    possibleExe = Path.Combine(installDir, "makemkvcon.exe")
                                    If File.Exists(possibleExe) Then Return possibleExe
                                End If
                            End If
                        End Using
                    Next
                End Using
            Next

        Catch ex As Exception

        Finally

        End Try
        Return Nothing

    End Function
    ''' <summary>
    ''' Resets all form controls to their default values, except for txtMakeMKVPath.
    ''' - Clears TextBoxes
    ''' - Checks CheckBoxes
    ''' - Clears ListBoxes
    ''' - Sets NumericUpDowns to their minimum value
    ''' - Refocuses on txtSeriesName
    ''' </summary>
    Private Sub ResetForm()
        For Each ctrl As Control In Me.Controls
            If ctrl.Name = "txtMakeMKVPath" Then
                Continue For
            End If

            If TypeOf ctrl Is TextBox Then
                CType(ctrl, TextBox).Text = ""
            ElseIf TypeOf ctrl Is CheckBox Then
                CType(ctrl, CheckBox).Checked = True
            ElseIf TypeOf ctrl Is ListBox Then
                CType(ctrl, ListBox).Items.Clear()
            ElseIf TypeOf ctrl Is NumericUpDown Then
                CType(ctrl, NumericUpDown).Value = CType(ctrl, NumericUpDown).Minimum
            End If
        Next
        txtSeriesName.Focus()
    End Sub
End Class


