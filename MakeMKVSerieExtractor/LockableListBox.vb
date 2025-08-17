Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Public Class LockableListBox
    Inherits ListBox

    Public Property UserInteractionLocked As Boolean = False

    Protected Overrides Sub WndProc(ByRef m As Message)
        Const WM_LBUTTONDOWN As Integer = &H201
        Const WM_LBUTTONDBLCLK As Integer = &H203
        Const WM_RBUTTONDOWN As Integer = &H204
        Const WM_RBUTTONDBLCLK As Integer = &H206
        Const WM_MOUSEWHEEL As Integer = &H20A
        Const WM_CONTEXTMENU As Integer = &H7B

        If UserInteractionLocked AndAlso
        (m.Msg = WM_LBUTTONDOWN OrElse m.Msg = WM_LBUTTONDBLCLK OrElse
         m.Msg = WM_RBUTTONDOWN OrElse m.Msg = WM_RBUTTONDBLCLK OrElse
         m.Msg = WM_MOUSEWHEEL OrElse m.Msg = WM_CONTEXTMENU) Then
            Return ' Ignore mouse input and context menu
        End If

        MyBase.WndProc(m)
    End Sub

    Protected Overrides Sub OnGotFocus(e As EventArgs)
        If UserInteractionLocked Then
            ' Redirect focus to parent or another control
            If Me.Parent IsNot Nothing Then
                Me.Parent.Select()
            End If
            Return
        End If
        MyBase.OnGotFocus(e)
    End Sub
End Class