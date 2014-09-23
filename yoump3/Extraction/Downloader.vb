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
            If video Is Nothing Then   Throw New ArgumentNullException("video")
            If savePath Is Nothing Then Throw New ArgumentNullException("savePath")
         
            Me.Video = video
            Me.AudioPath = savePath
            Me.BytesToDownload = bytesToDownload
        End Sub

        Protected Sub New(url As String, savePath As String, Optional bytesToDownload As Nullable(Of Int32) = Nothing)
            If String.IsNullOrEmpty(url) Then Throw New ArgumentNullException("url")
            Dim ytUrl As New YtUrlDecoder()
            Dim videoModes As IEnumerable(Of VideoCodecInfo) = ytUrl.GetDownloadUrls(url)
            Me.Video = (From xVideo In videoModes Select xVideo Where xVideo.CanExtractAudio Order By xVideo.AudioBitrate Take 1).FirstOrDefault
            Me.AudioPath = savePath
            Me.BytesToDownload = bytesToDownload
            StartDownloading()
        End Sub

        ''' <summary>
        ''' Occurs when the download finished.
        ''' </summary>
        Public Event DownloadFinished As EventHandler(Of IOFinishedEventArgs)

        ''' <summary>
        ''' Occurs when the download is starts.
        ''' </summary>
        Public Event DownloadStarted As EventHandler
        Friend Sub RaiseDownloadFinished(sender As Object, e As IOFinishedEventArgs)
            RaiseEvent DownloadFinished(sender, e)
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
        Public Property AudioPath() As String

        ''' <summary>
        ''' Gets the video to download/convert.
        ''' </summary>
        Public Property Video() As VideoCodecInfo

        ''' <summary>
        ''' Starts the work of the <see cref="Downloader"/>.
        ''' </summary>
        Public MustOverride Sub StartDownloading()


    End Class


End Namespace