Imports System.Collections.Specialized

Public Class Settings
    Public Shared [Global] As New Settings
    Public Shared GDataKey As String = "AI39si73TgEcqUvjTlZXiooqbKXhgpWjUHXSSnJ359iZfxG6oRGxMSwbE01GpwBH9Jff0oKh332zOGI5FwdfWa7bDPfSYmitfw"
    Public Sub New()
        Me.Items = New NameValueCollection
    End Sub
    Public Overrides Function ToString() As String
        Dim out As String = ""
        For i As Int64 = 0 To Items.Count - 1
            out &= Items.GetKey(i) & "=|=" & Items(Items.GetKey(i))
            If i < Items.Count - 1 Then out &= "J?%#R?%#^" & vbCrLf
        Next
        Return out
    End Function
    Public Sub SetFromCol(ByVal settingsObj As String)
        Dim objs As String() = Split(settingsObj, "J?%#R?%#^" & vbCrLf)
        For i As Int64 = 0 To objs.Length - 1
            Me.Items.Add(Split(objs(i), "=|=")(0), Split(objs(i), "=|=")(1))
        Next
    End Sub
    Public Sub Add(ByVal name As String, ByVal val As Object)
        Me.Set(name, val)
    End Sub
    Public Items As NameValueCollection
    Public Function [Get](ByVal name As String, Optional ByVal toLower As Boolean = False) As String
        Dim itm As String = Me.Items.Get(name)
        If Not toLower Then Return itm
        If itm IsNot Nothing Then If toLower Then Return itm.ToLower
        Return Nothing
    End Function
    Public Sub [Set](ByVal name As String, ByVal val As Object)
        Me.Items.Set(name, val)
    End Sub
End Class
