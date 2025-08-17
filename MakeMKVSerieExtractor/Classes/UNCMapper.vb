Option Strict On
Imports System.IO

Public Class UNCMapper

    ''' <summary>
    ''' Finds and returns the first available drive letter from Z: to D:.
    ''' </summary>
    ''' <returns>A free drive letter (e.g., "X:").</returns>
    ''' <exception cref="IOException">Thrown when no free drive letters are available.</exception>

    Public Shared Function GetFreeDriveLetter() As String
        Dim used As HashSet(Of String) = DriveInfo.GetDrives().Select(Function(d) d.Name.Substring(0, 2)).ToHashSet()
        For asciiCode As Integer = Asc("Z"c) To Asc("D"c) Step -1
            Dim letter As String = Chr(asciiCode) & ":"
            If Not used.Contains(letter) Then
                Return letter
            End If
        Next
        Throw New IOException("No free drive letters available.")
    End Function

    ''' <summary>
    ''' Maps a UNC path to a specified drive letter using the Windows "net use" command.
    ''' Returns True if the mapping succeeds; otherwise, throws an IOException with error details.
    ''' </summary>
    ''' <param name="uncPath">The UNC path to map (e.g., \\server\share).</param>
    ''' <param name="driveLetter">The drive letter to assign (e.g., Z:).</param>
    ''' <returns>True if the mapping is successful; otherwise, an exception is thrown.</returns>

    Public Shared Function MapDrive(uncPath As String, driveLetter As String) As Boolean
        Dim psi As New ProcessStartInfo("cmd.exe", $"/c net use {driveLetter} ""{uncPath}"" /persistent:no") With {
            .UseShellExecute = False,
            .CreateNoWindow = True,
            .RedirectStandardError = True
        }

        Using proc As Process = Process.Start(psi)
            proc.WaitForExit()
            If proc.ExitCode = 0 Then
                Return True
            Else
                Dim errorOutput As String = proc.StandardError.ReadToEnd()
                Throw New IOException($"Failed to map UNC path to {driveLetter}: {errorOutput}")
            End If
        End Using
    End Function

    ''' <summary>
    ''' Removes a mapped network drive using the Windows "net use" command.
    ''' </summary>
    ''' <param name="driveLetter">The drive letter to unmap (e.g., Z:).</param>

    Public Shared Sub UnmapDrive(driveLetter As String)
        Dim psi As New ProcessStartInfo("cmd.exe", $"/c net use {driveLetter} /delete /y") With {
            .UseShellExecute = False,
            .CreateNoWindow = True
        }
        Process.Start(psi).WaitForExit()
    End Sub

    ''' <summary>
    ''' Extracts the UNC share root (\\server\share) from a full UNC path.
    ''' </summary>
    ''' <param name="uncFullPath">The full UNC path (e.g., \\server\share\folder\file.txt).</param>
    ''' <returns>The root UNC share path (e.g., \\server\share).</returns>
    ''' <exception cref="ArgumentException">Thrown when the input is not a valid UNC path.</exception>

    Public Shared Function ExtractUncSharePath(uncFullPath As String) As String
        Dim parts As String() = uncFullPath.Split("\"c)
        If parts.Length >= 4 AndAlso parts(0) = "" AndAlso parts(1) <> "" AndAlso parts(2) <> "" Then
            Return $"\\{parts(1)}\{parts(2)}"
        Else
            Throw New ArgumentException("Invalid UNC path: " & uncFullPath)
        End If
    End Function
End Class