Imports ytDownloader
Imports ytDownloader.Extraction

Class MainWindow
   
    Private oldPnt As Double = 0

    Private Sub updateHandler(sender As Object, e As ProgressEventArgs)
        Dim tsAction As Action = _
            Sub()
                Dim str As String = Nothing
                If oldPnt.Equals(0D) Then
                    str = String.Format("{0}: {1}", e.Flag.ToString, e.ProgressPercentage)
                    Trace.WriteLine(str)
                    oldPnt = e.ProgressPercentage
                ElseIf Math.Abs(e.ProgressPercentage - oldPnt) > 2.0 Then ' get delta / update on every 2% 
                    str = String.Format("{0}: {1}", e.Flag.ToString, e.ProgressPercentage)
                    Trace.WriteLine(str)
                    oldPnt = e.ProgressPercentage
                End If
            End Sub
        Task.Factory.StartNew(tsAction)
    End Sub



    Public Sub Lded() Handles Me.Loaded
        Dim audiox As AudioDownloader = Downloader.Factory(Of AudioDownloader).Create("https://www.youtube.com/watch?v=Xz8aM8ZoAbA", "c:\test.mp3")
        AddHandler audiox.AudioExtractionProgressChanged, AddressOf updateHandler
        AddHandler audiox.DownloadProgressChanged, AddressOf updateHandler
        audiox.StartDownloadingThreaded()
        Return

        'Dim yt As New Youtube("dblooddrinker@gmail.com", "samcred0carding")
        'Dim ULU As ULong = ULong.MaxValue
        'Dim videos As List(Of Google.YouTube.Video) = yt.SearchVideo("how to make tiramisu")
        'For Each vid In videos
        '    vid = vid
        'Next



    End Sub
End Class
