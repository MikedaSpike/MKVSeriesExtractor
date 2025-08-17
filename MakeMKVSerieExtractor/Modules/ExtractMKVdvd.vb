Option Strict On
Imports System.IO
Imports System.Text.RegularExpressions

Public Module TrackTableFields
    Public Const TrackIndex As String = "TrackIndex"
    Public Const DVDTitleNumber As String = "DVDTitleNumber"
    Public Const Duration As String = "Duration"
    Public Const Filename As String = "Filename"
    Public Const SortCode As String = "SortCode"
    Public Const SizeBytes As String = "SizeBytes"
    Public Const VideoCodec As String = "VideoCodec"
    Public Const AudioStreams As String = "AudioStreams"
    Public Const SubtitleStreams As String = "SubtitleStreams"
    Public Const ChapterLayout As String = "ChapterLayout"
End Module

Public Module AnalyseTracks
    Public Const IsPlayAll As String = "IsPlayAll"
    Public Const GroupIndex As String = "GroupIndex"
End Module

'MakeMKV TINFO Field Reference

'Field ID | Description
'---------|-------------------------------
'2        | Disc title
'8        | Number of chapters
'9        | Duration (hh:mm : ss)
'10       | File size (GB)
'11       | File size (bytes)
'16       | MPLS playlist file name
'24       | DVD title number
'25       | Number of segments
'26       | Chapter layout
'27       | Output filename
'28       | Audio short code
'29       | Audio long code
'30       | Summary (chapters + size + code)
'31       | HTML-formatted title info
'33       | Unknown / internal flag
'49       | Sort code (e.g. C1, D2)
Public Class ExtractMKVdvd
    Public CancelRequested As Boolean = False
    Private Const ExtractTimeoutSeconds As Integer = 360
    Private Const OutlierThresholdSeconds As Integer = 600

    ''' <summary>
    ''' Runs MakeMKV in info mode to retrieve metadata from a source file.
    ''' Captures output with optional logging and timeout support.
    ''' </summary>
    ''' <param name="makeMKVPath">Path to the MakeMKV executable.</param>
    ''' <param name="sourcePath">Path to the source file.</param>
    ''' <param name="timeoutSeconds">Optional timeout in seconds (default: 120).</param>
    ''' <param name="logAction">Optional logging callback for status messages.</param>
    ''' <returns>Combined output from MakeMKV, or Nothing if failed, timed out, or cancelled.</returns>
    Public Function GetMakeMKVInfo(makeMKVPath As String, sourcePath As String, Optional timeoutSeconds As Integer = 120, Optional logAction As Action(Of String) = Nothing) As String
        Dim makeMKVInfoArgs As String = $"-r info file:""{sourcePath}"""
        Dim outputBuilder As New Text.StringBuilder()

        Try
            Using procInfo As New Process()
                procInfo.StartInfo = New ProcessStartInfo With {
                .FileName = makeMKVPath,
                .Arguments = makeMKVInfoArgs,
                .UseShellExecute = False,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .CreateNoWindow = True
            }

                ' Capture output asynchronously
                AddHandler procInfo.OutputDataReceived, Sub(sender, e)
                                                            If e.Data IsNot Nothing Then
                                                                outputBuilder.AppendLine(e.Data)
                                                            End If
                                                        End Sub

                AddHandler procInfo.ErrorDataReceived, Sub(sender, e)
                                                           If e.Data IsNot Nothing Then
                                                               outputBuilder.AppendLine("[stderr] " & e.Data)
                                                           End If
                                                       End Sub

                procInfo.Start()
                procInfo.BeginOutputReadLine()
                procInfo.BeginErrorReadLine()

                Dim startTime As DateTime = DateTime.Now

                While Not procInfo.HasExited
                    Application.DoEvents()

                    If (DateTime.Now - startTime).TotalSeconds > timeoutSeconds Then
                        Try
                            procInfo.Kill()
                            logAction?.Invoke("[⏱️] Timeout: MakeMKV info process killed.")
                        Catch
                            logAction?.Invoke("❌ Could not kill MakeMKV info process.")
                        End Try
                        Return Nothing
                    End If

                    If CancelRequested Then
                        Try
                            procInfo.Kill()
                            logAction?.Invoke("🛑 MakeMKV info process cancelled.")
                        Catch
                            logAction?.Invoke("❌ Could not cancel MakeMKV info process.")
                        End Try
                        Return Nothing
                    End If

                    Threading.Thread.Sleep(200)
                End While

                Return outputBuilder.ToString()
            End Using
        Catch ex As Exception
            logAction?.Invoke($"[❌] Error running MakeMKV info: {ex.Message}")
            Return Nothing
        End Try
    End Function
    ''' <summary>
    ''' Parses track data to identify and exclude 'Play All' tracks, filter outliers, and return sorted track indices.
    ''' </summary>
    ''' <param name="trackTable">DataTable containing track metadata.</param>
    ''' <param name="expectedDurationMin">Expected minimum duration for valid tracks.</param>
    ''' <param name="logAction">Callback for logging progress and decisions.</param>
    ''' <returns>List of track indices after filtering and sorting.</returns>
    Public Function ParseTrackNumbers(trackTable As DataTable, expectedDurationMin As Double, logAction As Action(Of String)) As List(Of Integer)
        Dim trackDurations As New Dictionary(Of Integer, TimeSpan)
        Dim longestTrackDuration As TimeSpan = TimeSpan.Zero
        Dim possiblePlayAllIndex As Integer = -1
        Dim otherTracksTotalDuration As TimeSpan = TimeSpan.Zero
        Dim playAllTrackIndex As Integer? = Nothing
        Dim chapterLayouts As New Dictionary(Of Integer, String)

        ' Step 1: Build duration map and find longest track
        For Each row As DataRow In trackTable.Rows
            If row.IsNull(TrackTableFields.TrackIndex) OrElse row.IsNull(TrackTableFields.Duration) Then Continue For
            Dim trackIndex As Integer = CType(row(TrackTableFields.TrackIndex), Integer)
            Dim duration As TimeSpan = CType(row(TrackTableFields.Duration), TimeSpan)
            trackDurations(trackIndex) = duration
            If duration > longestTrackDuration Then
                longestTrackDuration = duration
                possiblePlayAllIndex = trackIndex
            End If
        Next

        ' Step 2: Compare longest track to sum of others
        For Each kvp In trackDurations
            If kvp.Key <> possiblePlayAllIndex Then
                otherTracksTotalDuration += kvp.Value
            End If
        Next

        ' Step 3: Detect and remove PlayAll
        If trackDurations.Count < 3 Then
            logAction?.Invoke($"[ℹ️] Skipping 'Play All' detection — only {trackDurations.Count} tracks found.")
        ElseIf Math.Abs((longestTrackDuration - otherTracksTotalDuration).TotalSeconds) <= 360 Then
            playAllTrackIndex = possiblePlayAllIndex
            trackDurations.Remove(possiblePlayAllIndex)
            logAction?.Invoke($"[🔍] TrackIndex #{possiblePlayAllIndex} marked as 'Play All' based on duration match.")
        Else
            ' Step 3b: Detect Play All by chapter layout
            For Each row As DataRow In trackTable.Rows
                If row.IsNull(TrackTableFields.TrackIndex) OrElse row.IsNull(TrackTableFields.ChapterLayout) Then Continue For
                Dim trackIndex = CType(row(TrackTableFields.TrackIndex), Integer)
                If Not trackDurations.ContainsKey(trackIndex) Then Continue For
                Dim layout = row(TrackTableFields.ChapterLayout).ToString()
                Dim chapterGroups = layout.Split(","c).Count(Function(s) s.Contains("-"))
                If chapterGroups >= 2 Then
                    playAllTrackIndex = trackIndex
                    trackDurations.Remove(trackIndex)
                    logAction?.Invoke($"[🔍] TrackIndex #{trackIndex} marked as 'Play All' based on chapter layout.")
                    Exit For
                End If
            Next
        End If

        ' Remove from trackTable
        If playAllTrackIndex.HasValue Then
            For i = trackTable.Rows.Count - 1 To 0 Step -1
                Dim row = trackTable.Rows(i)
                If Not row.IsNull(TrackTableFields.TrackIndex) AndAlso
           CType(row(TrackTableFields.TrackIndex), Integer) = playAllTrackIndex.Value Then
                    logAction?.Invoke($"[🧹] TrackIndex #{playAllTrackIndex.Value} removed from trackTable (reason: Play All).")
                    trackTable.Rows.RemoveAt(i)
                End If
            Next
        End If

        ' Step 4: Hybrid Outlier Detection
        Dim doubleEpisodeKeys As New HashSet(Of Integer)
        Dim filteredTrackTable = FilterValidTracks(trackTable, expectedDurationMin, logAction, doubleEpisodeKeys)

        ' Step 5: Filter by chapter count
        FilterByChapterCount(filteredTrackTable, doubleEpisodeKeys, logAction)


        ' Step 6: Sort by SortCode (TINFO:49)
        Dim trackSortMap As New Dictionary(Of Integer, String)
        For Each row As DataRow In trackTable.Rows
            If row.IsNull(TrackTableFields.DVDTitleNumber) OrElse row.IsNull(TrackTableFields.SortCode) Then Continue For
            Dim trackNum As Integer = CType(row(TrackTableFields.DVDTitleNumber), Integer)
            If trackDurations.ContainsKey(trackNum) Then
                trackSortMap(trackNum) = row(TrackTableFields.SortCode).ToString()
            End If
        Next

        'Dim sortedTitles = titleSortMap.OrderBy(Function(kvp) kvp.Value).Select(Function(kvp) kvp.Key).ToList()
        'Return sortedTitles
        Dim sortedTrackIndices = filteredTrackTable.AsEnumerable().
                                    OrderBy(Function(row) row(TrackTableFields.DVDTitleNumber).ToString()).'OrderBy(Function(row) row(TrackTableFields.SortCode).ToString()).
                                    Select(Function(row) CType(row(TrackTableFields.TrackIndex), Integer)).
                                    ToList()

        Return sortedTrackIndices

    End Function
    ''' <summary>
    ''' Returns a copy of the track table with annotations for 'Play All' detection and duration-based grouping.
    ''' </summary>
    ''' <param name="trackTable">Original DataTable containing track metadata.</param>
    ''' <param name="logAction">Callback for logging analysis steps and decisions.</param>
    ''' <returns>Annotated DataTable with added columns for PlayAll flag and GroupIndex.</returns>
    Public Function GetAnnotatedTrackTable(trackTable As DataTable, logAction As Action(Of String)) As DataTable
        Dim annotatedTable = trackTable.Copy()
        annotatedTable.Columns.Add(AnalyseTracks.IsPlayAll, GetType(Boolean))
        annotatedTable.Columns.Add(AnalyseTracks.GroupIndex, GetType(Integer))

        Dim trackDurations As New Dictionary(Of Integer, TimeSpan)
        Dim longestTrackDuration As TimeSpan = TimeSpan.Zero
        Dim possiblePlayAllIndex As Integer = -1
        Dim otherTracksTotalDuration As TimeSpan = TimeSpan.Zero
        Dim playAllTrackIndex As Integer? = Nothing

        ' Step 1: Build duration map and find longest track
        For Each row As DataRow In annotatedTable.Rows
            If row.IsNull(TrackTableFields.TrackIndex) OrElse row.IsNull(TrackTableFields.Duration) Then Continue For
            Dim trackIndex As Integer = CType(row(TrackTableFields.TrackIndex), Integer)
            Dim duration As TimeSpan = CType(row(TrackTableFields.Duration), TimeSpan)
            trackDurations(trackIndex) = duration
            If duration > longestTrackDuration Then
                longestTrackDuration = duration
                possiblePlayAllIndex = trackIndex
            End If
        Next

        ' Step 2: Compare longest track to sum of others
        For Each kvp In trackDurations
            If kvp.Key <> possiblePlayAllIndex Then
                otherTracksTotalDuration += kvp.Value
            End If
        Next

        ' Step 3: Detect PlayAll
        If trackDurations.Count < 3 Then
            logAction?.Invoke($"[ℹ️] Skipping 'Play All' detection — only {trackDurations.Count} tracks found.")
        ElseIf Math.Abs((longestTrackDuration - otherTracksTotalDuration).TotalSeconds) <= 360 Then
            playAllTrackIndex = possiblePlayAllIndex
            'logAction?.Invoke($"[🔍] TrackIndex #{possiblePlayAllIndex} marked as 'Play All' based on duration match.")
        Else
            For Each row As DataRow In annotatedTable.Rows
                If row.IsNull(TrackTableFields.TrackIndex) OrElse row.IsNull(TrackTableFields.ChapterLayout) Then Continue For
                Dim trackIndex = CType(row(TrackTableFields.TrackIndex), Integer)
                If Not trackDurations.ContainsKey(trackIndex) Then Continue For
                Dim layout = row(TrackTableFields.ChapterLayout).ToString()
                Dim chapterGroups = layout.Split(","c).Count(Function(s) s.Contains("-"))
                If chapterGroups >= 2 Then
                    playAllTrackIndex = trackIndex
                    'logAction?.Invoke($"[🔍] TrackIndex #{trackIndex} marked as 'Play All' based on chapter layout.")
                    Exit For
                End If
            Next
        End If

        ' Step 4: Mark PlayAll
        If playAllTrackIndex.HasValue Then
            For Each row As DataRow In annotatedTable.Rows
                If Not row.IsNull(TrackTableFields.TrackIndex) AndAlso
               CType(row(TrackTableFields.TrackIndex), Integer) = playAllTrackIndex.Value Then
                    row(AnalyseTracks.IsPlayAll) = True
                End If
            Next
        End If

        ' Step 5: Group durations
        Dim tolerance = 0.1 ' ±10%
        Dim groups As New List(Of List(Of Integer)) ' Each group is a list of track indices

        For Each kvp In trackDurations
            Dim trackIndex = kvp.Key
            Dim duration = kvp.Value.TotalSeconds
            Dim matchedGroup As List(Of Integer) = Nothing

            For Each group In groups
                Dim referenceDuration = trackDurations(group(0)).TotalSeconds
                If Math.Abs(duration - referenceDuration) / referenceDuration <= tolerance Then
                    matchedGroup = group
                    Exit For
                End If
            Next

            If matchedGroup IsNot Nothing Then
                matchedGroup.Add(trackIndex)
            Else
                groups.Add(New List(Of Integer) From {trackIndex})
            End If
        Next

        ' Step 6: Assign GroupIndex
        For i = 0 To groups.Count - 1
            For Each TrackIndex As Integer In groups(i)
                For Each row As DataRow In annotatedTable.Rows
                    If Not row.IsNull(TrackTableFields.TrackIndex) AndAlso
               CType(row(TrackTableFields.TrackIndex), Integer) = TrackIndex Then
                        row(AnalyseTracks.GroupIndex) = i
                    End If
                Next
            Next
        Next

        'logAction?.Invoke($"[📊] Grouped {trackDurations.Count} tracks into {groups.Count} duration clusters.")
        Return annotatedTable
    End Function
    ''' <summary>
    ''' Parses MakeMKV info output and builds a DataTable containing track metadata and stream details.
    ''' </summary>
    ''' <param name="infoOutput">Raw text output from MakeMKV's info mode.</param>
    ''' <returns>DataTable with track-level metadata including audio and subtitle streams.</returns>
    Public Function BuildTrackDataTable(infoOutput As String) As DataTable
        Dim dt As New DataTable()
        dt.Columns.Add(TrackTableFields.TrackIndex, GetType(Integer))
        dt.Columns.Add(TrackTableFields.DVDTitleNumber, GetType(Integer))
        dt.Columns.Add(TrackTableFields.Duration, GetType(TimeSpan))
        dt.Columns.Add(TrackTableFields.Filename, GetType(String))
        dt.Columns.Add(TrackTableFields.SortCode, GetType(String))
        dt.Columns.Add(TrackTableFields.SizeBytes, GetType(Long))
        dt.Columns.Add(TrackTableFields.VideoCodec, GetType(String))
        dt.Columns.Add(TrackTableFields.AudioStreams, GetType(List(Of String)))
        dt.Columns.Add(TrackTableFields.SubtitleStreams, GetType(List(Of String)))
        dt.Columns.Add(TrackTableFields.ChapterLayout, GetType(String))

        Dim trackRows As New Dictionary(Of Integer, DataRow)
        Dim streamFields As New Dictionary(Of Integer, Dictionary(Of Integer, Dictionary(Of Integer, String)))()

        For Each line As String In infoOutput.Split({vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
            ' TINFO parsing
            Dim tinfoMatch As Match = Regex.Match(line, "TINFO:(\d+),(\d+),0,""([^""]*)""")
            If tinfoMatch.Success Then
                Dim trackIndex As Integer
                Dim fieldId As Integer
                If Not Integer.TryParse(tinfoMatch.Groups(1).Value, trackIndex) Then Continue For
                If Not Integer.TryParse(tinfoMatch.Groups(2).Value, fieldId) Then Continue For
                Dim value As String = tinfoMatch.Groups(3).Value

                If Not trackRows.ContainsKey(trackIndex) Then
                    Dim row As DataRow = dt.NewRow()
                    row(TrackTableFields.TrackIndex) = trackIndex
                    row(TrackTableFields.AudioStreams) = New List(Of String)()
                    row(TrackTableFields.SubtitleStreams) = New List(Of String)()
                    trackRows(trackIndex) = row
                End If

                Dim r As DataRow = trackRows(trackIndex)
                Select Case fieldId
                    Case 4, 8
                        r(TrackTableFields.VideoCodec) = value

                    Case 9
                        Dim ts As TimeSpan
                        If TimeSpan.TryParse(value, ts) Then
                            r(TrackTableFields.Duration) = ts
                        End If

                    Case 11
                        Dim sizeValue As Long
                        If Long.TryParse(value, sizeValue) Then
                            r(TrackTableFields.SizeBytes) = sizeValue
                        End If

                    Case 24
                        Dim dvdTitleNumber As Integer
                        If Integer.TryParse(value, dvdTitleNumber) Then
                            r(TrackTableFields.DVDTitleNumber) = dvdTitleNumber
                        End If

                    Case 26
                        r(TrackTableFields.ChapterLayout) = value

                    Case 27
                        r(TrackTableFields.Filename) = value

                    Case 49
                        r(TrackTableFields.SortCode) = value
                End Select
            End If

            ' SINFO parsing
            Dim sinfoMatch As Match = Regex.Match(line, "SINFO:(\d+),(\d+),(\d+),[^,]*,""([^""]*)""")
            If sinfoMatch.Success Then
                Dim trackIndex As Integer
                Dim streamIndex As Integer
                Dim fieldId As Integer
                If Not Integer.TryParse(sinfoMatch.Groups(1).Value, trackIndex) Then Continue For
                If Not Integer.TryParse(sinfoMatch.Groups(2).Value, streamIndex) Then Continue For
                If Not Integer.TryParse(sinfoMatch.Groups(3).Value, fieldId) Then Continue For
                Dim value As String = sinfoMatch.Groups(4).Value.Trim()

                If Not streamFields.ContainsKey(trackIndex) Then streamFields(trackIndex) = New Dictionary(Of Integer, Dictionary(Of Integer, String))()
                If Not streamFields(trackIndex).ContainsKey(streamIndex) Then streamFields(trackIndex)(streamIndex) = New Dictionary(Of Integer, String)()

                streamFields(trackIndex)(streamIndex)(fieldId) = value
            End If
        Next

        ' Aggregate stream info
        For Each trackIndex As Integer In streamFields.Keys
            If Not trackRows.ContainsKey(trackIndex) Then Continue For
            Dim r As DataRow = trackRows(trackIndex)

            For Each streamIndex As Integer In streamFields(trackIndex).Keys
                Dim fields As Dictionary(Of Integer, String) = streamFields(trackIndex)(streamIndex)

                Dim streamType As String = If(fields.ContainsKey(1), fields(1), "")
                Dim codec As String = If(fields.ContainsKey(5), fields(5), "")
                Dim lang As String = If(fields.ContainsKey(4), fields(4), "")
                Dim name As String = If(fields.ContainsKey(7), fields(7), "")
                If String.IsNullOrWhiteSpace(name) AndAlso fields.ContainsKey(30) Then name = fields(30)

                Dim shortLang As String = If(fields.ContainsKey(3), fields(3), "")
                Dim longLang As String = If(fields.ContainsKey(4), fields(4), "")
                Dim bitrate As String = If(fields.ContainsKey(13), fields(13), "")
                Dim channels As String = If(fields.ContainsKey(14), fields(14), "")

                Dim label As String = $"{shortLang} - {longLang}"

                If streamType.Equals("Audio", StringComparison.OrdinalIgnoreCase) Then
                    Dim details As New List(Of String)
                    If Not String.IsNullOrWhiteSpace(name) Then details.Add(name)
                    If Not String.IsNullOrWhiteSpace(channels) Then details.Add("Stereo") ' or $"{channels}ch" if you prefer
                    If Not String.IsNullOrWhiteSpace(bitrate) Then details.Add(bitrate)
                    If details.Count > 0 Then label &= $" ({String.Join(", ", details)})"
                    CType(r(TrackTableFields.AudioStreams), List(Of String)).Add(label)

                ElseIf streamType.Equals("Subtitles", StringComparison.OrdinalIgnoreCase) Then
                    CType(r(TrackTableFields.SubtitleStreams), List(Of String)).Add(label)
                End If

                ' If Not String.IsNullOrWhiteSpace(lang) Then label &= $" ({lang})"

                If streamType.Equals("Audio", StringComparison.OrdinalIgnoreCase) Then
                    CType(r(TrackTableFields.AudioStreams), List(Of String)).Add(label)
                ElseIf streamType.Equals("Subtitles", StringComparison.OrdinalIgnoreCase) Then
                    CType(r(TrackTableFields.SubtitleStreams), List(Of String)).Add(label)
                End If
            Next
        Next

        ' Finalize rows
        For Each row As DataRow In trackRows.Values
            dt.Rows.Add(row)
        Next

        Return dt
    End Function

    ''' <summary>
    ''' Counts the total number of unique chapters from a chapter layout string.
    ''' </summary>
    ''' <param name="layout">Comma-separated chapter layout (e.g., "1-3,4,5-7").</param>
    ''' <returns>Total number of distinct chapters.</returns>
    Private Function CountChapters(layout As String) As Integer
        If String.IsNullOrWhiteSpace(layout) Then Return 0
        Return layout.Split(","c).SelectMany(Function(part)
                                                 If part.Contains("-"c) Then
                                                     Dim range = part.Split("-"c)
                                                     Return Enumerable.Range(CInt(range(0)), CInt(range(1)) - CInt(range(0)) + 1)
                                                 Else
                                                     Return {CInt(part)}
                                                 End If
                                             End Function
                ).Distinct().Count()
    End Function

    ''' <summary>
    ''' Filters out tracks with chapter counts that deviate significantly from the modal range, excluding known double episodes.
    ''' </summary>
    ''' <param name="trackTable">Reference to the DataTable containing track metadata.</param>
    ''' <param name="doubleEpisodeKeys">Set of track numbers representing double episodes to exclude from filtering.</param>
    ''' <param name="logAction">Callback for logging filtering decisions.</param>
    Private Sub FilterByChapterCount(ByRef trackTable As DataTable, doubleEpisodeKeys As HashSet(Of Integer), logAction As Action(Of String))
        Dim chapterLayouts = trackTable.AsEnumerable().
        Where(Function(row) Not row.IsNull(TrackTableFields.DVDTitleNumber) AndAlso Not row.IsNull(TrackTableFields.ChapterLayout)).
        ToDictionary(Function(row) CType(row(TrackTableFields.DVDTitleNumber), Integer),
                     Function(row) row(TrackTableFields.ChapterLayout).ToString())

        Dim dynamicThreshold = Math.Max(3, CInt(chapterLayouts.Count * 0.3))

        If chapterLayouts.Count <= dynamicThreshold Then
            Dim chapterCounts = chapterLayouts.Select(Function(kvp) New With {.Key = kvp.Key, .Count = CountChapters(kvp.Value)}).ToList()

            If chapterCounts.Any() Then
                Dim modeCount = chapterCounts.GroupBy(Function(x) x.Count).
                                          OrderByDescending(Function(g) g.Count()).
                                          First().Key

                Dim minAcceptable = Math.Max(1, modeCount - 2)
                Dim maxAcceptable = modeCount + 2

                For i = trackTable.Rows.Count - 1 To 0 Step -1
                    Dim row = trackTable.Rows(i)
                    If row.IsNull(TrackTableFields.DVDTitleNumber) Then Continue For
                    Dim trackNum = CType(row(TrackTableFields.DVDTitleNumber), Integer)

                    ' 🛡️ Skip double episodes
                    If doubleEpisodeKeys.Contains(trackNum) Then Continue For

                    Dim entry = chapterCounts.FirstOrDefault(Function(x) x.Key = trackNum)
                    If entry IsNot Nothing AndAlso (entry.Count < minAcceptable OrElse entry.Count > maxAcceptable) Then
                        logAction?.Invoke($"[🧹] Track #{entry.Key} rejected: Chapter count {entry.Count} outside dynamic range {minAcceptable}-{maxAcceptable} (mode = {modeCount})")
                        trackTable.Rows.RemoveAt(i)
                    End If
                Next
            End If
        End If
    End Sub

    ''' <summary>
    ''' Filters tracks based on duration clustering and expected length, identifying valid episodes and double-length outliers.
    ''' </summary>
    ''' <param name="trackTable">DataTable containing track metadata.</param>
    ''' <param name="expectedDurationMin">Expected duration of a single episode in minutes.</param>
    ''' <param name="logAction">Callback for logging filtering decisions and analysis.</param>
    ''' <param name="doubleEpisodeKeys">Output set of track numbers identified as double episodes.</param>
    ''' <returns>Filtered DataTable containing only valid and double-length tracks.</returns>
    Public Function FilterValidTracks(trackTable As DataTable, expectedDurationMin As Double, logAction As Action(Of String), ByRef doubleEpisodeKeys As HashSet(Of Integer)) As DataTable
        doubleEpisodeKeys = New HashSet(Of Integer)()



        Dim bucketSizeMinutes As Integer = 10
        Dim buckets As New Dictionary(Of Integer, List(Of Integer))
        Dim trackDurations As New Dictionary(Of Integer, TimeSpan)

        For Each row As DataRow In trackTable.Rows
            If row.IsNull(TrackTableFields.DVDTitleNumber) OrElse row.IsNull(TrackTableFields.Duration) Then Continue For
            Dim trackNum As Integer = CType(row(TrackTableFields.DVDTitleNumber), Integer)
            Dim duration As TimeSpan = CType(row(TrackTableFields.Duration), TimeSpan)
            trackDurations(trackNum) = duration
        Next

        ' Fill buckets
        For Each kvp In trackDurations
            Dim minutes = CInt(kvp.Value.TotalMinutes)
            Dim bucketKey = (minutes \ bucketSizeMinutes) * bucketSizeMinutes
            If Not buckets.ContainsKey(bucketKey) Then buckets(bucketKey) = New List(Of Integer)
            buckets(bucketKey).Add(kvp.Key)
        Next

        ' Evaluate buckets
        Dim validBuckets = buckets.
            Select(Function(b)
                       Dim avg = b.Value.Select(Function(i) trackDurations(i).TotalMinutes).Average()
                       Return New With {.Key = b.Key, .TrackKeys = b.Value, .Average = avg}
                   End Function).
             Where(Function(b) b.Average >= expectedDurationMin * 0.5).ToList() ' ' Reject short clusters

        If validBuckets.Count = 0 Then
            logAction?.Invoke("[⚠️] No valid duration clusters found.")
            Return New DataTable()
        End If

        ' Prefer clusters near expectedDurationMin (+/- 6 min)
        Dim preferredBuckets = validBuckets.
            Where(Function(b) Math.Abs(b.Average - expectedDurationMin) <= 6).
            OrderByDescending(Function(b) b.TrackKeys.Count).
            ToList()

        Dim selectedBucket = If(preferredBuckets.FirstOrDefault(), validBuckets.OrderByDescending(Function(b) b.TrackKeys.Count).First())

        logAction?.Invoke($"[📊] Selected duration cluster: {selectedBucket.Key}-{selectedBucket.Key + bucketSizeMinutes - 1} min " &
                          $"(Avg: {TimeSpan.FromMinutes(selectedBucket.Average):hh\:mm\:ss}, Count: {selectedBucket.TrackKeys.Count})")

        Dim finalKeepKeys As New List(Of Integer)(selectedBucket.TrackKeys)

        ' Add double episodes
        For Each kvp In trackDurations
            Dim key = kvp.Key
            Dim durationMin = kvp.Value.TotalMinutes
            If Not finalKeepKeys.Contains(key) Then
                Dim ratio = durationMin / expectedDurationMin
                Dim avgTimeSpan = TimeSpan.FromMinutes(expectedDurationMin)
                Dim doubleTimeSpan = TimeSpan.FromMinutes(expectedDurationMin * 2)

                If Math.Abs(ratio - 2) < 0.2 Then
                    logAction?.Invoke($"[📺] Track #{key} ({kvp.Value}) kept as double episode. " &
                                      $"Track duration: {kvp.Value:hh\:mm\:ss}, Average: {avgTimeSpan:hh\:mm\:ss}, Expected double: {doubleTimeSpan:hh\:mm\:ss}")
                    finalKeepKeys.Add(key)
                    doubleEpisodeKeys.Add(key)
                Else
                    logAction?.Invoke($"[🧹] Track #{key} ({kvp.Value}) removed as outlier. " &
                                      $"Track duration: {kvp.Value:hh\:mm\:ss}, Average: {avgTimeSpan:hh\:mm\:ss}, Expected double: {doubleTimeSpan:hh\:mm\:ss}")
                End If
            End If
        Next
        ' Build filtered DataTable
        Dim filteredTable = trackTable.Clone()
        For Each row As DataRow In trackTable.Rows
            If row.IsNull(TrackTableFields.DVDTitleNumber) Then Continue For
            Dim trackNum = CType(row(TrackTableFields.DVDTitleNumber), Integer)
            If finalKeepKeys.Contains(trackNum) Then
                filteredTable.ImportRow(row)
            End If
        Next

        Return filteredTable

    End Function
    ''' <summary>
    ''' Extracts a single track from a source using MakeMKV and returns the path to the resulting MKV file.
    ''' </summary>
    ''' <param name="makeMKVPath">Path to the MakeMKV executable.</param>
    ''' <param name="sourcePath">Path to the source file (e.g., disc image).</param>
    ''' <param name="TrackIndex">Index of the track to extract.</param>
    ''' <param name="tempExtractFolder">Folder where the extracted MKV file will be saved.</param>
    ''' <param name="logAction">Callback for logging extraction progress and errors.</param>
    ''' <param name="timeoutSeconds">Optional timeout in seconds (default: ExtractTimeoutSeconds).</param>
    ''' <returns>Path to the extracted MKV file, or Nothing if extraction fails or is cancelled.</returns>
    Public Function ExtractSingleTrack(makeMKVPath As String, sourcePath As String, TrackIndex As Integer, tempExtractFolder As String, logAction As Action(Of String), Optional timeoutSeconds As Integer = ExtractTimeoutSeconds) As String
        Dim mkvArgs As String = $"-r mkv file:""{sourcePath}"" {TrackIndex} ""{tempExtractFolder}"""
        logAction?.Invoke($"[🔄] Extracting track {TrackIndex} started...")

        Try
            Using procExtract As New Process()
                procExtract.StartInfo = New ProcessStartInfo With {
                .FileName = makeMKVPath,
                .Arguments = mkvArgs,
                .UseShellExecute = False,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .CreateNoWindow = True
            }

                procExtract.Start()

                ' Read standard output and error asynchronously
                Dim stdOutBuilder As New Text.StringBuilder()
                Dim stdErrBuilder As New Text.StringBuilder()

                Dim stdOutTask As Threading.Tasks.Task = Threading.Tasks.Task.Run(Sub()
                                                                                      While Not procExtract.StandardOutput.EndOfStream
                                                                                          Dim line As String = procExtract.StandardOutput.ReadLine()
                                                                                          stdOutBuilder.AppendLine(line)
                                                                                      End While
                                                                                  End Sub)

                Dim stdErrTask As Threading.Tasks.Task = Threading.Tasks.Task.Run(Sub()
                                                                                      While Not procExtract.StandardError.EndOfStream
                                                                                          Dim errLine As String = procExtract.StandardError.ReadLine()
                                                                                          stdErrBuilder.AppendLine(errLine)
                                                                                          logAction?.Invoke("[MakeMKV-ERR] " & errLine)
                                                                                      End While
                                                                                  End Sub)

                Dim startTime As DateTime = DateTime.Now

                While Not procExtract.HasExited
                    System.Windows.Forms.Application.DoEvents()

                    If (DateTime.Now - startTime).TotalSeconds > timeoutSeconds Then
                        Try
                            procExtract.Kill()
                            logAction?.Invoke("[⏱️] Timeout: process stopped after " & (timeoutSeconds \ 60) & " minutes.")
                        Catch
                            logAction?.Invoke("❌ Could not stop process.")
                        End Try
                        Exit While
                    End If

                    If CancelRequested Then
                        Try
                            procExtract.Kill()
                        Catch
                            logAction?.Invoke("❌ Could not stop process.")
                        End Try
                        Exit While
                    End If

                    System.Windows.Forms.Application.DoEvents()
                    Threading.Thread.Sleep(500)
                End While

                ' Wait for output/error reading to finish
                stdOutTask.Wait(1000)
                stdErrTask.Wait(1000)

                If procExtract.HasExited Then
                    If procExtract.ExitCode = 0 Then
                        logAction?.Invoke("✅ Extraction completed successfully.")
                        Dim extractedMKV As String = Directory.GetFiles(tempExtractFolder, "*.mkv").OrderByDescending(Function(f) File.GetLastWriteTime(f)).FirstOrDefault()
                        If extractedMKV IsNot Nothing Then
                            Return extractedMKV
                        Else
                            logAction?.Invoke("[⚠️] No MKV file found after extraction.")
                            Return Nothing
                        End If
                    Else
                        logAction?.Invoke($"❌ Extraction failed. ExitCode: {procExtract.ExitCode}")
                        Return Nothing
                    End If
                End If
            End Using
        Catch ex As Exception
            logAction?.Invoke($"[❌] Error during extraction: {ex.Message}{Environment.NewLine}{ex.StackTrace}")
        End Try

        Return Nothing
    End Function
    ''' <summary>
    ''' Renames and moves an extracted MKV file into a season-based folder using series and episode metadata.
    ''' </summary>
    ''' <param name="ExtractedMKV">Path to the extracted MKV file.</param>
    ''' <param name="outputPath">Base output directory for organizing episodes.</param>
    ''' <param name="seriesName">Name of the series used in the filename.</param>
    ''' <param name="titleLine">Title line containing season and episode info (e.g., "S01E02").</param>
    ''' <param name="logAction">Callback for logging file operations and errors.</param>
    Public Sub MoveAndRenameMkv(ExtractedMKV As String, outputPath As String, seriesName As String, titleLine As String, logAction As Action(Of String))
        Dim sourceExtension As String = Path.GetExtension(ExtractedMKV)
        Dim pattern As String = "S(\d{2})E(\d{2})"
        Dim match As Match = Regex.Match(titleLine, pattern)
        If match.Success Then
            Dim season As String = match.Groups(1).Value
            Dim episode As String = match.Groups(2).Value
            Dim cleanTitle As String = Regex.Replace(titleLine, pattern & "[:\- ]*", "")
            cleanTitle = Regex.Replace(cleanTitle, "[^a-zA-Z0-9\s]", "").Trim()
            Dim seasonFolder As String = Path.Combine(outputPath, $"Season {season}")
            Try
                Directory.CreateDirectory(seasonFolder)
            Catch ex As Exception
                logAction?.Invoke($"[❌] Could not create folder: {seasonFolder}: {ex.Message}")
                Return
            End Try
            Dim newName As String = $"{seriesName} - S{season}E{episode} - {cleanTitle}{sourceExtension}"
            Dim newPath As String = Path.Combine(seasonFolder, newName)
            Try
                File.Move(ExtractedMKV, newPath)
                logAction?.Invoke($"[✅] Renamed: {Path.GetFileName(ExtractedMKV)} → {newPath}")
            Catch ex As Exception
                logAction?.Invoke($"[❌] Error moving file: [{ExtractedMKV}] to [{newPath}]: {ex.Message}")
            End Try
        Else
            logAction?.Invoke($"[⚠️] No SxxExx found in: {titleLine}")
        End If
    End Sub
    ''' <summary>
    ''' Moves and renames an extracted MKV file to the specified output directory using the provided filename.
    ''' </summary>
    ''' <param name="extractedMKV">Path to the extracted MKV file.</param>
    ''' <param name="outputPath">Destination folder for the renamed file.</param>
    ''' <param name="finalFileName">Desired filename (extension added if missing).</param>
    ''' <param name="logAction">Callback for logging file operations and errors.</param>
    Public Sub MoveMkvToOutput(extractedMKV As String, outputPath As String, finalFileName As String, logAction As Action(Of String))
        ' Get extension from source
        Dim sourceExtension As String = Path.GetExtension(extractedMKV)

        ' Check if finalFileName already has an extension
        If String.IsNullOrWhiteSpace(Path.GetExtension(finalFileName)) Then
            finalFileName &= sourceExtension
        End If

        Dim newPath As String = Path.Combine(outputPath, finalFileName)

        Try
            File.Move(extractedMKV, newPath)
            logAction?.Invoke($"[✅] Renamed: {Path.GetFileName(extractedMKV)} → {newPath}")
        Catch ex As Exception
            logAction?.Invoke($"[❌] Error moving file: [{extractedMKV}] to [{newPath}]: {ex.Message}")
        End Try
    End Sub

End Class