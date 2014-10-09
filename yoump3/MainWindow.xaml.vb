Imports System.Windows.Forms
Imports ytDownloader
Imports ytDownloader.Extraction

Class MainWindow
    Public Class LazyList(Of T)
        Inherits List(Of T)
        Private _pRetriever As Func(Of List(Of T))

        Public Sub New(retriever As Func(Of List(Of T)))
            _pRetriever = retriever
        End Sub

        Public Sub Initialize()
            AddRange(_pRetriever.Invoke())
        End Sub
        Public Function GetFirst() As T
            Return _pRetriever.Invoke.FirstOrDefault
        End Function
    End Class
    
#Region "Launchers"
    Public Sub Lded() Handles Me.Loaded

    End Sub

    Public Sub LaunchDownload(list As List(Of Downloader))
      Dim wndVideo As New VideoInfo(list)
        wndVideo.Owner = Me
        If wndVideo.ShowDialog Then
            Task.Factory.StartNew(Sub()
                                      For Each dldr In list
                                          Try
                                              dldr = Downloader.Initialize(dldr)
                                              Dim ctlDldr As DownloaderControl
                                              Dispatcher.Invoke( _
                                                  Sub()
                                                      ctlDldr = New DownloaderControl(dldr)
                                                      SpDownloaders.Children.Add(ctlDldr)
                                                      ctlDldr.Downloader.StartThreaded()
                                                  End Sub)
                                          Catch ex As Exception
                                              Trace.WriteLine(ex.Message)
                                          End Try
                                      Next
                                  End Sub)
        End If
    End Sub
    Public Sub LaunchDownload(dld As Downloader)
        LaunchDownload(New List(Of Downloader)({dld}))
    End Sub
#End Region


    Public Sub HandleArguments(ops As Options)
        Dim tmpQuality As Int32
        If ops.Quality = "Highest" Then
            tmpQuality = Int32.MaxValue
        ElseIf ops.Quality = "Lowest" Then
            tmpQuality = -1
        Else
            tmpQuality = CInt(ops.Quality)
        End If
        ' ReSharper disable once VBWarnings::BC42358
        Dim list = Downloader.Factory.CreateList(ops.Link, ops.OutputPath, ops.OnlyVideo, ops.Format, tmpQuality, True)
        LaunchDownload(list)

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

    Private Sub btnBrowseOutputPath_Click(sender As Object, e As RoutedEventArgs) Handles BtnBrowseOutputPath.Click
        Dim fop As FolderBrowserDialog
        If fop.ShowDialog = Forms.DialogResult.OK Then
            TxtOutputPath.Text = fop.SelectedPath
        End If
    End Sub



    Public Class Options
        Public Property Format As String
        Public Property Quality As String
        Public Property OnlyVideo As Boolean
        Public Property Link As String
        Public Property OutputPath As String

    End Class
End Class
