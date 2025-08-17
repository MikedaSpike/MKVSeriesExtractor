Public Class frmTrackGroupViewer
    Public Property SelectedAverageDuration As Double

    Public Sub New(trackTable As DataTable)
        InitializeComponent()

        dgvTracks.DataSource = trackTable
        dgvTracks.ReadOnly = True
        dgvTracks.SelectionMode = DataGridViewSelectionMode.CellSelect
        dgvTracks.MultiSelect = False
        Dim visibleColumns = New List(Of String) From {
                                                            TrackTableFields.TrackIndex,
                                                            TrackTableFields.Duration,
                                                            AnalyseTracks.IsPlayAll,
                                                            AnalyseTracks.GroupIndex
                                                        }
        dgvTracks.Columns(TrackTableFields.Duration).DefaultCellStyle.Format = "hh\:mm\:ss"
        dgvTracks.AllowUserToAddRows = False
        dgvTracks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
        For Each column As DataGridViewColumn In dgvTracks.Columns
            column.Visible = visibleColumns.Contains(column.Name)
        Next

    End Sub
    Private Sub dgvTracks_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvTracks.CellClick
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Exit Sub

        Dim clickedRow = dgvTracks.Rows(e.RowIndex)
        Dim groupIndexObj = clickedRow.Cells(AnalyseTracks.GroupIndex).Value
        If groupIndexObj Is Nothing OrElse IsDBNull(groupIndexObj) Then Exit Sub

        Dim selectedGroupIndex = Convert.ToInt32(groupIndexObj)

        ' Highlight all rows in the same group
        For Each row As DataGridViewRow In dgvTracks.Rows
            Dim rowGroupObj = row.Cells(AnalyseTracks.GroupIndex).Value
            Dim rowGroupIndex = If(IsDBNull(rowGroupObj), -1, Convert.ToInt32(rowGroupObj))

            row.DefaultCellStyle.BackColor = If(rowGroupIndex = selectedGroupIndex, Color.LightBlue, Color.White)
        Next

        ' Select the Duration cell of the clicked row
        dgvTracks.CurrentCell = clickedRow.Cells(TrackTableFields.Duration)

        ' Update label with average duration of selected group
        Dim durations = dgvTracks.Rows.Cast(Of DataGridViewRow).
                                            Where(Function(r)
                                                      Dim groupObj = r.Cells(AnalyseTracks.GroupIndex).Value
                                                      Return Not IsDBNull(groupObj) AndAlso Convert.ToInt32(groupObj) = selectedGroupIndex
                                                  End Function).
                                            Select(Function(r)
                                                       Dim durationObj = r.Cells(TrackTableFields.Duration).Value
                                                       If durationObj IsNot Nothing AndAlso Not IsDBNull(durationObj) AndAlso TypeOf durationObj Is TimeSpan Then
                                                           Return CType(durationObj, TimeSpan).TotalMinutes
                                                       Else
                                                           Return CType(Nothing, Nullable(Of Double))
                                                       End If
                                                   End Function).
                                            Where(Function(d) d.HasValue).
                                            Select(Function(d) d.Value).
                                            ToList()

        If durations.Count > 0 Then
            lblSelectedDuration.Text = $"Group {selectedGroupIndex}: {durations.Count} track(s), Avg Duration = {Math.Round(durations.Average())} min"
        Else
            lblSelectedDuration.Text = ""
        End If


    End Sub
    'Private Sub dgvTracks_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvTracks.CellClick
    '    If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Exit Sub

    '    Dim clickedColumn = dgvTracks.Columns(e.ColumnIndex).Name
    '    If clickedColumn <> TrackTableFields.Duration Then Exit Sub

    '    Dim clickedRow = dgvTracks.Rows(e.RowIndex)
    '    Dim groupIndexObj = clickedRow.Cells(AnalyseTracks.GroupIndex).Value
    '    If groupIndexObj Is Nothing OrElse IsDBNull(groupIndexObj) Then Exit Sub

    '    Dim selectedGroupIndex = Convert.ToInt32(groupIndexObj)

    '    ' Highlight all rows with the same GroupIndex
    '    For Each row As DataGridViewRow In dgvTracks.Rows
    '        Dim rowGroupObj = row.Cells(AnalyseTracks.GroupIndex).Value
    '        Dim rowGroupIndex = If(IsDBNull(rowGroupObj), -1, Convert.ToInt32(rowGroupObj))

    '        row.DefaultCellStyle.BackColor = If(rowGroupIndex = selectedGroupIndex, Color.LightBlue, Color.White)
    '    Next
    'End Sub
    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Dim durations As New List(Of Double)
        Dim playAllSelected As Boolean = False

        For Each row As DataGridViewRow In dgvTracks.Rows
            If row.IsNewRow Then Continue For

            If row.DefaultCellStyle.BackColor = Color.LightBlue Then
                Dim duration = CType(row.Cells(TrackTableFields.Duration).Value, TimeSpan)
                durations.Add(duration.TotalMinutes)

                If Not IsDBNull(row.Cells(AnalyseTracks.IsPlayAll).Value) AndAlso CBool(row.Cells(AnalyseTracks.IsPlayAll).Value) Then
                    playAllSelected = True
                End If
            End If
        Next

        If durations.Count = 0 Then
            MessageBox.Show("No tracks selected. Click a cell to select a group.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        If playAllSelected Then
            Dim result = MessageBox.Show("This group includes a track marked as 'Play All'. Are you sure you want to use it?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
            If result = DialogResult.No Then Return
        End If

        ' Check if group is unusually small
        Dim largestGroupSize = dgvTracks.Rows.Cast(Of DataGridViewRow).
             Where(Function(r) Not r.IsNewRow).
             GroupBy(Function(r) Convert.ToInt32(r.Cells(AnalyseTracks.GroupIndex).Value)).
             Max(Function(g) g.Count())

        If durations.Count < largestGroupSize Then
            Dim result = MessageBox.Show("This group has fewer tracks than other groups. Are you sure it's the correct one?", "Possible Outlier", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If result = DialogResult.No Then Return
        End If

        SelectedAverageDuration = durations.Average()
        DialogResult = DialogResult.OK
    End Sub
End Class