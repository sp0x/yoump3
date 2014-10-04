Imports System.Windows.Forms
Imports ytDownloader
Imports ytDownloader.Extraction

Class MainWindow
   
    

    Public Sub Lded() Handles Me.Loaded
        'Dim audiox As AudioDownloader = Downloader.Factory(Of AudioDownloader).Create("https://www.youtube.com/watch?v=Xz8aM8ZoAbA", "c:\test.mp3")
        'AddHandler audiox.AudioExtractionProgressChanged, AddressOf updateHandler
        'AddHandler audiox.DownloadProgressChanged, AddressOf updateHandler
        'audiox.StartDownloadingThreaded()
        'Return

        'Dim yt As New Youtube("dblooddrinker@gmail.com", "samcred0carding")
        'Dim ULU As ULong = ULong.MaxValue
        'Dim videos As List(Of Google.YouTube.Video) = yt.SearchVideo("how to make tiramisu")
        'For Each vid In videos
        '    vid = vid
        'Next
    End Sub

    Public Sub LaunchDownload(dldr As Downloader)
        Dispatcher.InvokeAsync(Sub()
                                   Dim ctlDldr As New DownloaderControl(dldr)
                                   SpDownloaders.Children.Add(ctlDldr)
                                   ctlDldr.Downloader.StartAsync()
                               End Sub)
    End Sub

    Public Sub HandleArguments(ops As Options)
        Dim executor As Action(Of Downloader) = AddressOf LaunchDownload
        Dim tmpQuality As Int32
        If ops.Quality = "Highest" Then
            tmpQuality = Int32.MaxValue
        ElseIf ops.Quality = "Lowest" Then
            tmpQuality = -1
        Else
            tmpQuality = CInt(ops.Quality)
        End If

        ' ReSharper disable once VBWarnings::BC42358
        Downloader.Factory.CreateListAsync(ops.Link, ops.OutputPath, ops.OnlyVideo, ops.Format, tmpQuality, _
                                  executor)

    End Sub

    Private Sub onKeyPressed(sender As Object, e As System.Windows.Input.KeyEventArgs) Handles Me.KeyDown
        If e.Key = Key.V And Keyboard.IsKeyDown(Key.LeftCtrl) Then
            Dim url As String = Clipboard.GetText
            Dim ops As New Options
            ops.OutputPath = TxtOutputPath.Text
            ops.OnlyVideo = ChkOnlyVideo.IsChecked
            If RbMp3.IsChecked Then ops.Format = "mp3"
            If RbMp4.IsChecked Then ops.Format = "mp4"
            If RbAac.IsChecked Then ops.Format = "aac"
            If Rb3Gp.IsChecked Then ops.Format = "3gp"
            If RbFlv.IsChecked Then ops.Format = "flv"
            If RbWebm.IsChecked Then ops.Format = "webm"
            If RbNone.IsChecked Then ops.Format = Nothing
            ops.Link = url
            ops.Quality = CbQuality.SelectedValue
            HandleArguments(ops)
        End If
    End Sub

    Private Sub btnBrowseOutputPath_Click(sender As Object, e As RoutedEventArgs) Handles btnBrowseOutputPath.Click
        Dim fop As folderbrowserdialog
        If fop.ShowDialog = Forms.DialogResult.OK Then
            txtOutputPath.Text = fop.SelectedPath
        End If
    End Sub



    Class Options
        Public Property Format As String
        Public Property Quality As String
        Public Property OnlyVideo As Boolean
        Public Property Link As String
        Public Property OutputPath As String

    End Class
End Class
