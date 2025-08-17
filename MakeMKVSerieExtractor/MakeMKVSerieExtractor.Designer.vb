<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MakeMKVSerieExtractor
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MakeMKVSerieExtractor))
        lblInput = New Label()
        btnSelectInputFolder = New Button()
        btnSelectOutputFolder = New Button()
        lblOutput = New Label()
        btnSelectTitleFile = New Button()
        lblTitleFile = New Label()
        btnStart = New Button()
        progressBar = New ProgressBar()
        txtLog = New RichTextBox()
        lstDVDMaps = New LockableListBox()
        copyToolStripMenuItem = New ContextMenuStrip(components)
        CopyToolStripMenuItem1 = New ToolStripMenuItem()
        lstTitles = New LockableListBox()
        lblStatus = New Label()
        txtInputPath = New TextBox()
        txtOutputPath = New TextBox()
        txtTitleFile = New TextBox()
        btnStop = New Button()
        txtMakeMKVPath = New TextBox()
        btnBrowseMakeMKV = New Button()
        lblMakeMKVpath = New Label()
        txtSeriesName = New TextBox()
        lblSerieName = New Label()
        lblFolders = New Label()
        lblTitles = New Label()
        lblExpectedDuration = New Label()
        nudExpectedDuration = New NumericUpDown()
        ToolTip = New ToolTip(components)
        btnAnalyseTracks = New Button()
        btnReset = New Button()
        cbxGenerateTitles = New CheckBox()
        ColorDialog1 = New ColorDialog()
        lblAutogentitle = New Label()
        copyToolStripMenuItem.SuspendLayout()
        CType(nudExpectedDuration, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' lblInput
        ' 
        lblInput.AutoSize = True
        lblInput.Location = New Point(13, 48)
        lblInput.Name = "lblInput"
        lblInput.Size = New Size(71, 15)
        lblInput.TabIndex = 0
        lblInput.Text = "Input Folder"
        ' 
        ' btnSelectInputFolder
        ' 
        btnSelectInputFolder.Location = New Point(559, 51)
        btnSelectInputFolder.Name = "btnSelectInputFolder"
        btnSelectInputFolder.Size = New Size(75, 23)
        btnSelectInputFolder.TabIndex = 3
        btnSelectInputFolder.Text = "📂 Browse"
        ToolTip.SetToolTip(btnSelectInputFolder, "Browse your computer to select the input folder containing DVD files")
        btnSelectInputFolder.UseVisualStyleBackColor = True
        ' 
        ' btnSelectOutputFolder
        ' 
        btnSelectOutputFolder.Location = New Point(559, 80)
        btnSelectOutputFolder.Name = "btnSelectOutputFolder"
        btnSelectOutputFolder.Size = New Size(75, 23)
        btnSelectOutputFolder.TabIndex = 4
        btnSelectOutputFolder.Text = "📂 Browse"
        ToolTip.SetToolTip(btnSelectOutputFolder, "Browse your computer to select the output folder for extracted episodes")
        btnSelectOutputFolder.UseVisualStyleBackColor = True
        ' 
        ' lblOutput
        ' 
        lblOutput.AutoSize = True
        lblOutput.Location = New Point(5, 84)
        lblOutput.Name = "lblOutput"
        lblOutput.Size = New Size(79, 15)
        lblOutput.TabIndex = 3
        lblOutput.Text = "Output folder"
        ' 
        ' btnSelectTitleFile
        ' 
        btnSelectTitleFile.Location = New Point(559, 109)
        btnSelectTitleFile.Name = "btnSelectTitleFile"
        btnSelectTitleFile.Size = New Size(75, 23)
        btnSelectTitleFile.TabIndex = 5
        btnSelectTitleFile.Text = "📂 Browse"
        ToolTip.SetToolTip(btnSelectTitleFile, "Browse to select a text file containing episode titles")
        btnSelectTitleFile.UseVisualStyleBackColor = True
        ' 
        ' lblTitleFile
        ' 
        lblTitleFile.AutoSize = True
        lblTitleFile.Location = New Point(17, 109)
        lblTitleFile.Name = "lblTitleFile"
        lblTitleFile.Size = New Size(67, 15)
        lblTitleFile.TabIndex = 6
        lblTitleFile.Text = "Title list file"
        ' 
        ' btnStart
        ' 
        btnStart.Location = New Point(48, 599)
        btnStart.Name = "btnStart"
        btnStart.Size = New Size(75, 23)
        btnStart.TabIndex = 90
        btnStart.Text = "▶ Start"
        ToolTip.SetToolTip(btnStart, "Begin the extraction process using the selected settings and files")
        btnStart.UseVisualStyleBackColor = True
        ' 
        ' progressBar
        ' 
        progressBar.Location = New Point(8, 570)
        progressBar.Name = "progressBar"
        progressBar.Size = New Size(626, 23)
        progressBar.TabIndex = 12
        ' 
        ' txtLog
        ' 
        txtLog.Location = New Point(8, 454)
        txtLog.Name = "txtLog"
        txtLog.Size = New Size(622, 96)
        txtLog.TabIndex = 80
        txtLog.Text = ""
        ToolTip.SetToolTip(txtLog, "Displays progress, warnings, and status messages during extraction")
        ' 
        ' lstDVDMaps
        ' 
        lstDVDMaps.ContextMenuStrip = copyToolStripMenuItem
        lstDVDMaps.FormattingEnabled = True
        lstDVDMaps.HorizontalScrollbar = True
        lstDVDMaps.ItemHeight = 15
        lstDVDMaps.Location = New Point(90, 167)
        lstDVDMaps.Name = "lstDVDMaps"
        lstDVDMaps.SelectionMode = SelectionMode.MultiExtended
        lstDVDMaps.Size = New Size(463, 124)
        lstDVDMaps.TabIndex = 14
        ToolTip.SetToolTip(lstDVDMaps, "Displays mapped DVD folders. Click to view or select a folder")
        lstDVDMaps.UserInteractionLocked = False
        ' 
        ' copyToolStripMenuItem
        ' 
        copyToolStripMenuItem.ImageScalingSize = New Size(20, 20)
        copyToolStripMenuItem.Items.AddRange(New ToolStripItem() {CopyToolStripMenuItem1})
        copyToolStripMenuItem.Name = "ContextMenuStrip1"
        copyToolStripMenuItem.Size = New Size(103, 26)
        ' 
        ' CopyToolStripMenuItem1
        ' 
        CopyToolStripMenuItem1.Name = "CopyToolStripMenuItem1"
        CopyToolStripMenuItem1.Size = New Size(102, 22)
        CopyToolStripMenuItem1.Text = "Copy"
        ' 
        ' lstTitles
        ' 
        lstTitles.ContextMenuStrip = copyToolStripMenuItem
        lstTitles.FormattingEnabled = True
        lstTitles.HorizontalScrollbar = True
        lstTitles.ItemHeight = 15
        lstTitles.Location = New Point(90, 297)
        lstTitles.Name = "lstTitles"
        lstTitles.SelectionMode = SelectionMode.MultiExtended
        lstTitles.Size = New Size(463, 124)
        lstTitles.TabIndex = 15
        ToolTip.SetToolTip(lstTitles, "Shows available titles/tracks from the selected DVD. Used for extraction")
        lstTitles.UserInteractionLocked = False
        ' 
        ' lblStatus
        ' 
        lblStatus.AutoSize = True
        lblStatus.Location = New Point(8, 552)
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(0, 15)
        lblStatus.TabIndex = 16
        ' 
        ' txtInputPath
        ' 
        txtInputPath.Location = New Point(90, 48)
        txtInputPath.Name = "txtInputPath"
        txtInputPath.Size = New Size(463, 23)
        txtInputPath.TabIndex = 2
        ToolTip.SetToolTip(txtInputPath, "Select the folder containing your DVD ISO or structure files")
        ' 
        ' txtOutputPath
        ' 
        txtOutputPath.Location = New Point(90, 80)
        txtOutputPath.Name = "txtOutputPath"
        txtOutputPath.Size = New Size(463, 23)
        txtOutputPath.TabIndex = 4
        ToolTip.SetToolTip(txtOutputPath, "Choose where the extracted episodes will be saved")
        ' 
        ' txtTitleFile
        ' 
        txtTitleFile.Location = New Point(90, 110)
        txtTitleFile.Name = "txtTitleFile"
        txtTitleFile.Size = New Size(463, 23)
        txtTitleFile.TabIndex = 5
        ToolTip.SetToolTip(txtTitleFile, "Optional: Load a predefined list of episode titles for better naming accuracy.")
        ' 
        ' btnStop
        ' 
        btnStop.Location = New Point(469, 599)
        btnStop.Name = "btnStop"
        btnStop.Size = New Size(75, 23)
        btnStop.TabIndex = 100
        btnStop.Text = "❌ Quit"
        ToolTip.SetToolTip(btnStop, "Close the application")
        btnStop.UseVisualStyleBackColor = True
        ' 
        ' txtMakeMKVPath
        ' 
        txtMakeMKVPath.Location = New Point(90, 139)
        txtMakeMKVPath.Name = "txtMakeMKVPath"
        txtMakeMKVPath.Size = New Size(463, 23)
        txtMakeMKVPath.TabIndex = 6
        ToolTip.SetToolTip(txtMakeMKVPath, "Select the MakeMKV-generated file that contains track and metadata info")
        ' 
        ' btnBrowseMakeMKV
        ' 
        btnBrowseMakeMKV.Location = New Point(559, 138)
        btnBrowseMakeMKV.Name = "btnBrowseMakeMKV"
        btnBrowseMakeMKV.Size = New Size(75, 23)
        btnBrowseMakeMKV.TabIndex = 7
        btnBrowseMakeMKV.Text = "📂 Browse"
        ToolTip.SetToolTip(btnBrowseMakeMKV, "Browse to select the MakeMKV file (.info or .xml) for parsing")
        btnBrowseMakeMKV.UseVisualStyleBackColor = True
        ' 
        ' lblMakeMKVpath
        ' 
        lblMakeMKVpath.AutoSize = True
        lblMakeMKVpath.Location = New Point(2, 138)
        lblMakeMKVpath.Name = "lblMakeMKVpath"
        lblMakeMKVpath.Size = New Size(82, 15)
        lblMakeMKVpath.TabIndex = 22
        lblMakeMKVpath.Text = "MakeMKV File"
        ' 
        ' txtSeriesName
        ' 
        txtSeriesName.Location = New Point(90, 19)
        txtSeriesName.Name = "txtSeriesName"
        txtSeriesName.Size = New Size(463, 23)
        txtSeriesName.TabIndex = 1
        ToolTip.SetToolTip(txtSeriesName, "Enter the name of the TV series. This will be used for naming output files")
        ' 
        ' lblSerieName
        ' 
        lblSerieName.AutoSize = True
        lblSerieName.Location = New Point(17, 19)
        lblSerieName.Name = "lblSerieName"
        lblSerieName.Size = New Size(67, 15)
        lblSerieName.TabIndex = 25
        lblSerieName.Text = "Serie Name"
        ' 
        ' lblFolders
        ' 
        lblFolders.AutoSize = True
        lblFolders.Location = New Point(39, 167)
        lblFolders.Name = "lblFolders"
        lblFolders.Size = New Size(45, 15)
        lblFolders.TabIndex = 27
        lblFolders.Text = "Folders"
        ' 
        ' lblTitles
        ' 
        lblTitles.AutoSize = True
        lblTitles.Location = New Point(49, 297)
        lblTitles.Name = "lblTitles"
        lblTitles.Size = New Size(35, 15)
        lblTitles.TabIndex = 28
        lblTitles.Text = "Titles"
        ' 
        ' lblExpectedDuration
        ' 
        lblExpectedDuration.AutoSize = True
        lblExpectedDuration.Location = New Point(226, 429)
        lblExpectedDuration.Name = "lblExpectedDuration"
        lblExpectedDuration.Size = New Size(201, 15)
        lblExpectedDuration.TabIndex = 29
        lblExpectedDuration.Text = "Expected Episode Duration (minutes)"
        ' 
        ' nudExpectedDuration
        ' 
        nudExpectedDuration.Location = New Point(433, 427)
        nudExpectedDuration.Maximum = New Decimal(New Integer() {90, 0, 0, 0})
        nudExpectedDuration.Name = "nudExpectedDuration"
        nudExpectedDuration.Size = New Size(120, 23)
        nudExpectedDuration.TabIndex = 17
        ToolTip.SetToolTip(nudExpectedDuration, "Set the typical length of one episode. For sitcoms, use ~22. For dramas, use ~44.")
        ' 
        ' btnAnalyseTracks
        ' 
        btnAnalyseTracks.Location = New Point(559, 425)
        btnAnalyseTracks.Name = "btnAnalyseTracks"
        btnAnalyseTracks.Size = New Size(75, 23)
        btnAnalyseTracks.TabIndex = 18
        btnAnalyseTracks.Text = "📌Duration"
        ToolTip.SetToolTip(btnAnalyseTracks, "Review episode durations and choose a reference group to set the average episode duration")
        btnAnalyseTracks.UseVisualStyleBackColor = True
        ' 
        ' btnReset
        ' 
        btnReset.Location = New Point(276, 599)
        btnReset.Name = "btnReset"
        btnReset.Size = New Size(75, 23)
        btnReset.TabIndex = 95
        btnReset.Text = "🔁 Reset"
        ToolTip.SetToolTip(btnReset, "Reset form to default")
        btnReset.UseVisualStyleBackColor = True
        ' 
        ' cbxGenerateTitles
        ' 
        cbxGenerateTitles.AutoSize = True
        cbxGenerateTitles.Location = New Point(559, 347)
        cbxGenerateTitles.Name = "cbxGenerateTitles"
        cbxGenerateTitles.Size = New Size(15, 14)
        cbxGenerateTitles.TabIndex = 16
        ToolTip.SetToolTip(cbxGenerateTitles, "Use this option to skip manual title entry, titles will be generated automatically based on folder and file names.")
        cbxGenerateTitles.UseVisualStyleBackColor = True
        ' 
        ' lblAutogentitle
        ' 
        lblAutogentitle.Location = New Point(560, 313)
        lblAutogentitle.Name = "lblAutogentitle"
        lblAutogentitle.Size = New Size(78, 79)
        lblAutogentitle.TabIndex = 102
        lblAutogentitle.Text = "Automaticcly Generate Titles"
        lblAutogentitle.TextAlign = ContentAlignment.MiddleCenter
        ' 
        ' MakeMKVSerieExtractor
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(650, 636)
        Controls.Add(btnReset)
        Controls.Add(cbxGenerateTitles)
        Controls.Add(btnAnalyseTracks)
        Controls.Add(nudExpectedDuration)
        Controls.Add(lblExpectedDuration)
        Controls.Add(lblTitles)
        Controls.Add(lblFolders)
        Controls.Add(txtSeriesName)
        Controls.Add(lblSerieName)
        Controls.Add(txtMakeMKVPath)
        Controls.Add(btnBrowseMakeMKV)
        Controls.Add(lblMakeMKVpath)
        Controls.Add(btnStop)
        Controls.Add(txtTitleFile)
        Controls.Add(txtOutputPath)
        Controls.Add(txtInputPath)
        Controls.Add(lblStatus)
        Controls.Add(lstDVDMaps)
        Controls.Add(txtLog)
        Controls.Add(progressBar)
        Controls.Add(btnStart)
        Controls.Add(btnSelectTitleFile)
        Controls.Add(lblTitleFile)
        Controls.Add(btnSelectOutputFolder)
        Controls.Add(lblOutput)
        Controls.Add(btnSelectInputFolder)
        Controls.Add(lblInput)
        Controls.Add(lstTitles)
        Controls.Add(lblAutogentitle)
        FormBorderStyle = FormBorderStyle.FixedSingle
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Name = "MakeMKVSerieExtractor"
        Text = "DVD Series Extractor"
        copyToolStripMenuItem.ResumeLayout(False)
        CType(nudExpectedDuration, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents lblInput As Label
    Friend WithEvents btnSelectInputFolder As Button
    Friend WithEvents btnSelectOutputFolder As Button
    Friend WithEvents lblOutput As Label
    Friend WithEvents btnSelectTitleFile As Button
    Friend WithEvents lblTitleFile As Label
    Friend WithEvents btnStart As Button
    Friend WithEvents progressBar As ProgressBar
    Friend WithEvents txtLog As RichTextBox
    Friend WithEvents lstDVDMaps As LockableListBox
    Friend WithEvents lstTitles As LockableListBox
    Friend WithEvents lblStatus As Label
    Friend WithEvents txtInputPath As TextBox
    Friend WithEvents txtOutputPath As TextBox
    Friend WithEvents txtTitleFile As TextBox
    Friend WithEvents btnStop As Button
    Friend WithEvents txtMakeMKVPath As TextBox
    Friend WithEvents btnBrowseMakeMKV As Button
    Friend WithEvents lblMakeMKVpath As Label
    Friend WithEvents copyToolStripMenuItem As ContextMenuStrip
    Friend WithEvents CopyToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents txtSeriesName As TextBox
    Friend WithEvents lblSerieName As Label
    Friend WithEvents lblFolders As Label
    Friend WithEvents lblTitles As Label
    Friend WithEvents lblExpectedDuration As Label
    Friend WithEvents nudExpectedDuration As NumericUpDown
    Friend WithEvents ToolTip As ToolTip
    Friend WithEvents ColorDialog1 As ColorDialog
    Friend WithEvents btnAnalyseTracks As Button
    Friend WithEvents cbxGenerateTitles As CheckBox
    Friend WithEvents lblAutogentitle As Label
    Friend WithEvents btnReset As Button

End Class
