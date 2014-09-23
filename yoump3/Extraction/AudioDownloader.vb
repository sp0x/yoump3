Imports System.IO
Imports System.Net
Namespace Extraction

    Public Class IOFinishedEventArgs
        Inherits EventArgs
        Public Property Path As String
        Public Property Stream As Stream
        Public Property Mode As IOMode = IOMode.File
    End Class
    Public Enum IOMode
        File
        Streaming
    End Enum

    ''' <summary>
    ''' Provides a method to download a video and extract its audio track.
    ''' </summary>
    Public Class AudioDownloader
        Inherits Downloader
        Private isCanceled As Boolean

        ''' <summary>
        ''' Initializes a new instance of the <see cref="AudioDownloader"/> class.
        ''' </summary>
        ''' <param name="video">The video to convert.</param>
        ''' <param name="savePath">The path to save the audio.</param>
        ''' /// <param name="bytesToDownload">An optional value to limit the number of bytes to download.</param>
        ''' <exception cref="ArgumentNullException"><paramref name="video"/> or <paramref name="savePath"/> is <c>null</c>.</exception>
        Public Sub New(video As VideoCodecInfo, savePath As String, Optional bytesToDownload As Int32 = Nothing)
            MyBase.New(video, savePath, bytesToDownload)
        End Sub

#Region "Events"
        ''' <summary>
        ''' Occurs when the progress of the audio extraction has changed.
        ''' </summary>
        Public Event AudioExtractionProgressChanged As EventHandler(Of ProgressEventArgs)
        ''' <summary>
        ''' Occurs when the download progress of the video file has changed.
        ''' </summary>
        Public Event DownloadProgressChanged As EventHandler(Of ProgressEventArgs)
#End Region

        ''' <summary>
        ''' Downloads the video from YouTube and then extracts the audio track out if it.
        ''' </summary>
        ''' <exception cref="IOException">
        ''' The temporary video file could not be created.
        ''' - or -
        ''' The audio file could not be created.
        ''' </exception>
        ''' <exception cref="AudioExtractionException">An error occured during audio extraction.</exception>
        ''' <exception cref="WebException">An error occured while downloading the video.</exception>
        Public Overrides Sub StartDownloading()
            Dim tempPath As String = Path.GetTempFileName()
            Me.DownloadVideo(tempPath)
            If Not Me.isCanceled Then
                Me.ExtractAudio(tempPath)
            End If

            Me.OnDownloadFinished(New IOFinishedEventArgs With {.Path = Me.AudioPath})
        End Sub

        Protected Sub OnDownloadFinished(e As IOFinishedEventArgs)
            MyBase.RaiseDownloadFinished(Me, e)
        End Sub
        ''' <summary>
        ''' Downloads the FLV.
        ''' </summary>
        ''' <param name="path">The path where it should be saved</param>
        ''' <remarks></remarks>
        Private Sub DownloadVideo(path As String)
            Dim videoDownloader = New VideoDownloader(Me.Video, path, Me.BytesToDownload)
            AddHandler videoDownloader.DownloadProgressChanged, _
                Sub(sender As Object, e As ProgressEventArgs)
                    RaiseEvent DownloadProgressChanged(Me, e)
                    Me.isCanceled = e.Cancel
                End Sub
            videoDownloader.StartDownloading()
        End Sub

        ''' <summary>
        ''' Extracts the audio stream from a given F
        ''' </summary>
        ''' <param name="path">The path to the downloaded Flv file.</param>
        ''' <remarks></remarks>
        Private Sub ExtractAudio(path As String)
            Using flvFile = New FlvFile(path, Me.AudioPath)
                AddHandler flvFile.ConversionProgressChanged, _
                    Sub(sender As Object, e As ProgressEventArgs)
                        RaiseEvent AudioExtractionProgressChanged(Me, New ProgressEventArgs(e.ProgressPercentage))
                    End Sub
                flvFile.ExtractStreams()
            End Using
        End Sub
    End Class



End Namespace