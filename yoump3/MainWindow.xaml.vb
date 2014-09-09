Imports Google.YouTube
Imports System.IO
Imports System.Web

Class MainWindow
    Public Sub lded() Handles Me.Loaded
        Dim yt As New Youtube("dblooddrinker@gmail.com", "samcred0carding")
        Dim videos As List(Of Google.YouTube.Video) = yt.SearchVideo("how to make tiramisu")
        For Each vid In videos
            vid = vid
        Next



    End Sub
End Class
