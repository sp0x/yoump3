Imports yoump3.Extraction
Namespace Extraction
    ''' <summary>
    ''' Provides the base class for the <see cref="AudioDownloader"/> and <see cref="VideoDownloader"/> class.
    ''' </summary>
    Public MustInherit Class Downloader
        ''' <summary>
        ''' Initializes a new instance of the <see cref="Downloader"/> class.
        ''' </summary>
        ''' <param name="video">The video to download/convert.</param>
        ''' <param name="savePath">The path to save the video/audio.</param>
        ''' /// <param name="bytesToDownload">An optional value to limit the number of bytes to download.</param>
        ''' <exception cref="ArgumentNullException"><paramref name="video"/> or <paramref name="savePath"/> is <c>null</c>.</exception>
        Protected Sub New(video As VideoCodecInfo, savePath As String, Optional bytesToDownload As System.Nullable(Of Integer) = Nothing)
            If video Is Nothing Then
                Throw New ArgumentNullException("video")
            End If

            If savePath Is Nothing Then
                Throw New ArgumentNullException("savePath")
            End If

            Me.Video = video
            Me.SavePath = savePath
            Me.BytesToDownload = bytesToDownload
        End Sub

        ''' <summary>
        ''' Occurs when the download finished.
        ''' </summary>
        Public Event DownloadFinished As EventHandler

        ''' <summary>
        ''' Occurs when the download is starts.
        ''' </summary>
        Public Event DownloadStarted As EventHandler
        Friend Sub RaiseDownloadFinished(sender As Object, e As EventArgs)
            RaiseEvent DownloadStarted(sender, e)
        End Sub
        Protected Sub RaiseDownloadStarted(sender As Object, e As EventArgs)
            RaiseEvent DownloadStarted(sender, e)
        End Sub


        ''' <summary>
        ''' Gets the number of bytes to download. <c>null</c>, if everything is downloaded.
        ''' </summary>
        Public Property BytesToDownload As System.Nullable(Of Integer) = Nothing

        ''' <summary>
        ''' Gets the path to save the video/audio.
        ''' </summary>
        Public Property SavePath() As String

        ''' <summary>
        ''' Gets the video to download/convert.
        ''' </summary>
        Public Property Video() As VideoCodecInfo

        ''' <summary>
        ''' Starts the work of the <see cref="Downloader"/>.
        ''' </summary>
        Public MustOverride Sub Execute()

       
    End Class


End Namespace