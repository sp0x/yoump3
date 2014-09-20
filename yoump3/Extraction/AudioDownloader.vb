Imports System.IO
Imports System.Net
Namespace Extraction


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

        ''' <summary>
        ''' Occurs when the progress of the audio extraction has changed.
        ''' </summary>
        Public Event AudioExtractionProgressChanged As EventHandler(Of ProgressEventArgs)

        ''' <summary>
        ''' Occurs when the download progress of the video file has changed.
        ''' </summary>
        Public Event DownloadProgressChanged As EventHandler(Of ProgressEventArgs)

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
        Public Overrides Sub Execute()
            Dim tempPath As String = Path.GetTempFileName()

            Me.DownloadVideo(tempPath)

            If Not Me.isCanceled Then
                Me.ExtractAudio(tempPath)
            End If

            Me.OnDownloadFinished(EventArgs.Empty)
        End Sub
       
        Protected Sub OnDownloadFinished(e As EventArgs)
            MyBase.RaiseDownloadFinished(Me, e)
        End Sub

        Private Sub DownloadVideo(path As String)
            Dim videoDownloader = New VideoDownloader(Me.Video, path, Me.BytesToDownload)
            AddHandler videoDownloader.DownloadProgressChanged, _
                Sub(sender As Object, e As ProgressEventArgs)
                    RaiseEvent DownloadProgressChanged(Me, e)
                    Me.isCanceled = e.Cancel
                End Sub
            videoDownloader.Execute()
        End Sub

        Private Sub ExtractAudio(path As String)
            Using flvFile = New FlvFile(path, Me.SavePath)
                AddHandler flvFile.ConversionProgressChanged, _
                    Sub(sender As Object, e As ProgressEventArgs)
                        RaiseEvent AudioExtractionProgressChanged(Me, New ProgressEventArgs(e.ProgressPercentage))
                    End Sub
                flvFile.ExtractStreams()
            End Using
        End Sub
    End Class



End Namespace