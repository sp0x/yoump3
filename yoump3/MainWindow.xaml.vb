Imports Google.YouTube
Imports System.IO
Imports System.Web
Imports yoump3.Extraction
Imports System.Text.RegularExpressions

Class MainWindow
    Public RNDURL As String = "https://www.youtube.com/watch?v=QjxScn7cKo8" ' "https://www.youtube.com/watch?v=f6NsZNp6Hhw"
    Public Class TimeCounter
        Public Property ExpireDate As Date
        Public Sub New(expDate As DateTime)
            If DateTime.Now > expDate Then
                Throw New InvalidOperationException("Expiration date must be in the future time!")
                Return
            End If
            ExpireDate = expDate
        End Sub


        Public ReadOnly Property TimeLeft As TimeSpan
            Get
                Dim diff As TimeSpan = ExpireDate - DateTime.Now
                Return diff
            End Get
        End Property
        Sub addUpdateMinutes(p1 As Integer)
            Throw New NotImplementedException
        End Sub

        Public Overrides Function ToString() As String
            Return TimeLeft.ToString
        End Function

    End Class
    Public Shared Function RemoveIllegalPathCharacters(path As String) As String
        Dim regexSearch As String = New String(System.IO.Path.GetInvalidFileNameChars()) + New String(System.IO.Path.GetInvalidPathChars())
        Dim r As New Regex(String.Format("[{0}]", Regex.Escape(regexSearch)))
        Return r.Replace(path, "")
    End Function

    ''' <summary>
    ''' The video information you got from the url.
    ''' </summary>
    ''' <param name="videoInfos"></param>
    ''' <exception cref="AudioExtractionException">Can't find a supported aduio format</exception>
    ''' <remarks></remarks>
    Public Shared Sub DownloadAudio(videoInfos As IEnumerable(Of VideoCodecInfo))
        Dim vid As VideoCodecInfo = (From vif In videoInfos Where vif.CanExtractAudio Order By vif.AudioBitrate).FirstOrDefault
        If vid Is Nothing Then
            Throw New AudioExtractionException("Can't find a supported audio format!")
        End If
        If vid.RequiresDecryption Then
            Dim ytDec As New YtUrlDecoder
            ytDec.DecryptDownloadUrl(vid)
        End If
        Dim audioDownloader As New AudioDownloader(vid, RemoveIllegalPathCharacters(vid.Title) + vid.AudioExtension)
        ''Register the progress events. We treat the download progress as 85% of the progress
        ''and the extraction progress only as 15% of the progress, because the download will
        ''take much longer than the audio extraction.
        AddHandler audioDownloader.DownloadProgressChanged, Sub(s, e)
                                                                Dim x = e.ProgressPercentage * 0.85
                                                                Trace.WriteLine(String.Format("video({0})", x))
                                                            End Sub
        AddHandler audioDownloader.AudioExtractionProgressChanged, Sub(s, e)
                                                                       Dim x = 85 + e.ProgressPercentage * 0.15
                                                                       Trace.WriteLine(String.Format("audio({0})", x))
                                                                   End Sub
        
        audioDownloader.StartDownloading()
    End Sub
   
    Public Sub lded() Handles Me.Loaded
        Dim somevar As UInt64 = &HFFFFFFFFFFFFFFFEUL
        Dim buff As Byte() = New Byte(1024) {}
        Dim ms As New MemoryStream(buff)
        Dim bwr As New BinaryWriter(ms)
        Dim gig = BitConverter.IsLittleEndian
        bwr.Write(somevar) ' this should pass into uint
        buff = ms.ToArray

        'Dim timx As New TimeCounter(DateTime.Now.AddDays(3))
        'timx.addUpdateMinutes(10)
        'Dim t As New task(Async Sub()
        '                      Do : Await Task.Delay(5000) : Console.WriteLine(timx.ToString) : Loop
        '                  End Sub)
        't.Start()

        '/*
        '   * Get the available video formats.
        '   * We'll work with them in the video and audio download examples.
        '   */
        Dim urldec As New YtUrlDecoder
        Dim videoInfos As IEnumerable(Of VideoCodecInfo) = urldec.GetDownloadUrls(RNDURL, False)
        DownloadAudio(videoInfos)
        Return

        Dim len As Int32 = 10
        Dim mask As ULong = CULng(&HFFFFFFFFFFFFFFFFUL >> 64 - len)
        Dim ULX As ULong = STD.RShift(&HFFFFFFFFFFFFFFFFUL, 64)

        Dim yt As New Youtube("dblooddrinker@gmail.com", "samcred0carding")
        Dim ULU As ULong = ULong.MaxValue
        Dim videos As List(Of Google.YouTube.Video) = yt.SearchVideo("how to make tiramisu")
        For Each vid In videos
            vid = vid
        Next



    End Sub
End Class
