<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmTrackGroupViewer
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        dgvTracks = New DataGridView()
        btnOK = New Button()
        lblSelectedDuration = New Label()
        lblInfo = New Label()
        ToolTip = New ToolTip(components)
        CType(dgvTracks, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' dgvTracks
        ' 
        dgvTracks.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvTracks.Location = New Point(12, 27)
        dgvTracks.Name = "dgvTracks"
        dgvTracks.Size = New Size(379, 227)
        dgvTracks.TabIndex = 0
        ' 
        ' btnOK
        ' 
        btnOK.Location = New Point(12, 260)
        btnOK.Name = "btnOK"
        btnOK.Size = New Size(113, 23)
        btnOK.TabIndex = 2
        btnOK.Text = "🕒Apply Duration"
        ToolTip.SetToolTip(btnOK, "Set the episode duration based on the selected group")
        btnOK.UseVisualStyleBackColor = True
        ' 
        ' lblSelectedDuration
        ' 
        lblSelectedDuration.AutoSize = True
        lblSelectedDuration.Location = New Point(131, 264)
        lblSelectedDuration.Name = "lblSelectedDuration"
        lblSelectedDuration.Size = New Size(99, 15)
        lblSelectedDuration.TabIndex = 3
        lblSelectedDuration.Text = "Selected duration"
        ' 
        ' lblInfo
        ' 
        lblInfo.AutoSize = True
        lblInfo.Location = New Point(73, 9)
        lblInfo.Name = "lblInfo"
        lblInfo.Size = New Size(275, 15)
        lblInfo.TabIndex = 4
        lblInfo.Text = "Pick a TV Show from Results – for average duration"
        ' 
        ' frmTrackGroupViewer
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(407, 295)
        Controls.Add(lblInfo)
        Controls.Add(lblSelectedDuration)
        Controls.Add(btnOK)
        Controls.Add(dgvTracks)
        FormBorderStyle = FormBorderStyle.FixedToolWindow
        Name = "frmTrackGroupViewer"
        StartPosition = FormStartPosition.CenterParent
        Text = "Pick a TV "
        CType(dgvTracks, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents dgvTracks As DataGridView
    Friend WithEvents btnOK As Button
    Friend WithEvents lblSelectedDuration As Label
    Friend WithEvents lblInfo As Label
    Friend WithEvents ToolTip As ToolTip
End Class
