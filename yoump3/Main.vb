﻿Imports ytDownloader
Imports ytDownloader.Extraction
Imports CommandLine

Public Module Main
    Public Wl As Action(Of String) = AddressOf Console.WriteLine
    Public Wr As Action(Of String) = AddressOf Console.Write
    Private _updateDelta As Double = 0
    Public UpdateInterval As Double = 2D
    Public Timer As New Stopwatch

    Private Sub updateHandler(sender As Object, e As ProgressEventArgs)
        Dim tsAction As Action = _
            Sub()
                Dim str As String = Nothing
                Dim shouldPrint As Boolean = False
                Dim delta As Double = Math.Abs(e.ProgressPercentage - _updateDelta)
                If _updateDelta.Equals(0D) Or _updateDelta.Equals(100D) Then
                    shouldPrint = True
                    _updateDelta = e.ProgressPercentage
                ElseIf delta > UpdateInterval Then ' get delta 
                    shouldPrint = True
                    _updateDelta = e.ProgressPercentage
                End If

                If shouldPrint Then
                    Dim eta As TimeSpan = Timer.Elapsed
                    Wr(String.Format("[{0}:{1}] ", eta.Minutes, eta.Seconds))

                    str = String.Format("{0}: ", e.Flag.ToString)
                    Console.ForegroundColor = ConsoleColor.Green
                    Wr(str)
                    str = String.Format("{0:F2}", e.ProgressPercentage)
                    Console.ForegroundColor = ConsoleColor.Red
                    Wl(str)
                    Console.ForegroundColor = ConsoleColor.White
                End If
            End Sub
        Task.Factory.StartNew(tsAction)
    End Sub

    Private Sub DownloadFinished(sender As Object, e As IoFinishedEventArgs)
        Timer.Stop()
        Select Case e.Mode
            Case IoMode.File
                Console.ForegroundColor = ConsoleColor.Green
                Wl(String.Format("Finished [in {0}] downloading and saved to {1}", Timer.Elapsed, e.Path))
                Console.ForegroundColor = ConsoleColor.White
            Case IoMode.Streaming

        End Select
    End Sub

    Private Sub PrintStatement(a As String, b As String)
        Dim str As String = String.Format("{0}: ", a)
        Console.ForegroundColor = ConsoleColor.Green
        Wr(str)
        str = String.Format("{0}", b)
        Console.ForegroundColor = ConsoleColor.Red
        Wl(str)
        Console.ForegroundColor = ConsoleColor.White
    End Sub
    
    Public Function HandleArguments(ops As Options)
        Dim dldr As Downloader = Nothing
        Dim dldOps As New DownloadOptionsBuilder(ops.Link, ops.OutputPath, ops.OnlyVideo, ops.Format, ops.Quality)
        dldr = dldOps.GetDownloader
        If TypeOf dldr Is AudioDownloader Then
            PrintStatement("Downloading audio: ", ops.Link)
        Else
            PrintStatement("Downloading video: ", ops.Link)
        End If

        AddHandler dldr.ExtractionProgressChanged, AddressOf updateHandler
        AddHandler dldr.DownloadProgressChanged, AddressOf updateHandler
        AddHandler dldr.DownloadFinished, AddressOf DownloadFinished
        PrintStatement("To: ", dldOps.Output)

        Return dldr
    End Function

    Public Sub Main(args As String())
        Dim ops As Options = New Options
        If CommandLine.Parser.Default.ParseArguments(args, ops) Then
            If String.IsNullOrEmpty(ops.OutputPath) Or String.IsNullOrEmpty(ops.Link) Then Return
            Dim dldr As Downloader = HandleArguments(ops)
            
            Try
                Timer.Start()
                dldr.StartDownloading()
                Catch ex As Exception
                Wl(ex.Message)
            End Try
        Else
            Wl("Usage:")
            Wl("yoump3 http://youtubeUrl <-q [quality]> <-f [format:mp3,aac,flv,mp4,webm,3gp]>  outputFile <-*audio|video>")
        End If
        Wl("Press any key to exit:") : Console.ReadLine()

    End Sub

    Class Options
        <OptionAttribute("f", "format", Required:=False, HelpText:="The format of the audio.")> _
        Public Property Format As String

        <OptionAttribute("q", "quality", Required:=False, HelpText:="The quality of the video.")> _
        Public Property Quality As String


        <OptionAttribute("v", "video", DefaultValue:=False, Required:=False)>
        Public Property OnlyVideo As Boolean

        <ValueOption(0)> _
        Public Property Link As String

        <ValueOption(1)> _
        Public Property OutputPath As String

    End Class




End Module