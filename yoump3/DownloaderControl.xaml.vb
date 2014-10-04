Imports System.ComponentModel
Imports ytDownloader.Extraction
Imports ytDownloader

Public Class DownloaderControl
#Region "update options"
    Private _updateDelta As Double = 0
    Public UpdateInterval As Double = 2D
    Public Timer As New Stopwatch
#End Region


    Public Sub New(dldr As Downloader)
        Downloader = dldr
        InitializeComponent()
        PgProgress.Minimum = 0
        PgProgress.MaxHeight = 100D
        AddHandler Downloader.DownloadProgressChanged, AddressOf OnDownloadProgressChanged
        AddHandler Downloader.ExtractionProgressChanged, AddressOf OnExtractionProgressChanged
        AddHandler Downloader.DownloadFinished, AddressOf OnDownloadFinished
        Text = dldr.VideoCodec.ToString
    End Sub


    Private Sub OnDownloadProgressChanged(sender As Object, e As ProgressEventArgs)
        UpdateProgress(e)
    End Sub
    Private Sub OnExtractionProgressChanged(sender As Object, e As ProgressEventArgs)
        UpdateProgress(e)
    End Sub
    Private Sub UpdateProgress(prg As ProgressEventArgs)
        Dispatcher.Invoke(Sub()

                              Progress = prg.ProgressPercentage
                              Text = prg.GetStatusString(Downloader.ToString())
                          End Sub)
    End Sub



    Private Sub OnDownloadFinished(sender As Object, e As IoFinishedEventArgs)
        Dispatcher.Invoke(Sub()
                              Progress = 100D
                              PgProgress.IsEnabled = False
                              PgProgress.Foreground = Brushes.DarkGray
                              Text = String.Format("Finished - {0}", Text)
                          End Sub)
    End Sub


    Public Property Downloader As Downloader
        Get
            Return DataContext
        End Get
        Set(value As Downloader)
            DataContext = value
        End Set
    End Property
    <Category("Custom"), Description("The control's image")> _
    Public Property Text As String
        Get
            Return TxtValue.Text
        End Get
        Set(value As String)
            TxtValue.Text = value
        End Set
    End Property
    <Category("Custom"), Description("Progress")> _
    Public Property Progress As Double
        Get
            Return PgProgress.Value
        End Get
        Set(value As Double)
            PgProgress.Value = value
        End Set
    End Property

End Class
