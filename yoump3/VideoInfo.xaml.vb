Imports System.Text
Imports ytDownloader
Imports yoump3.ImagingExtensions

Public Class VideoInfo
    Public Property Downloaders As List(Of Downloader)
        Get
            If DataContext Is Nothing Then DataContext = New List(Of Downloader)
            Return DataContext
        End Get
        Set(value As List(Of Downloader))
            DataContext = value
        End Set
    End Property
    Public Shared ReadOnly VideoImageProperty As DependencyProperty = _
    DependencyProperty.Register("VideoImage", GetType(ImageSource), GetType(VideoInfo))
    Public Shared ReadOnly VideoInfoProperty As DependencyProperty = _
    DependencyProperty.Register("VideoInfo", GetType(String), GetType(VideoInfo), New UIPropertyMetadata(String.Empty))
   

    Public Property VideoImage As ImageSource
        Get
            Return GetValue(VideoImageProperty)
        End Get
        Set(value As ImageSource)
            SetValue(VideoImageProperty, value)
        End Set
    End Property
    Public Property VideoInfo As String
        Get
            Return GetValue(VideoInfoProperty)
        End Get
        Set(value As String)
            SetValue(VideoInfoProperty, value)
        End Set
    End Property
    Public Sub New(dlds As List(Of Downloader))
        InitializeComponent()
        Downloaders.AddRange(dlds)
        Dim isPlaylist As Boolean = dlds.Count > 1
        Dim plInfo As String = ""
        If isPlaylist Then plInfo = String.Format(" - {0} Videos", dlds.Count)

        Dim imgBytes As Byte() = dlds.FirstOrDefault.GetVideoImageBytes()
        VideoImage = imgBytes.ToImage
        Title = String.Format("Video info: {0}", dlds.FirstOrDefault.VideoCodec.Title)
        Dim bldInfo As New StringBuilder
        bldInfo.Append(dlds.FirstOrDefault.VideoCodec.Title & Environment.NewLine)
        bldInfo.AppendFormat("Url: {0}{1}", dlds.FirstOrDefault.InputUrl, Environment.NewLine)
        bldInfo.AppendFormat("Resolution: {0}{1}", dlds.FirstOrDefault.VideoCodec.Resolution, Environment.NewLine)
        bldInfo.AppendFormat("Type: {0}{1}", dlds.FirstOrDefault.VideoCodec.VideoType, Environment.NewLine)
        bldInfo.AppendFormat("Is Playlist: {0}{1}", isPlaylist, plInfo + Environment.NewLine)

        VideoInfo = bldInfo.ToString()
    End Sub

    Public Sub New(ByVal dldr As Downloader)
        Me.New(New List(Of Downloader)({dldr}))
    End Sub

    Private Sub BtnOk_Click(sender As Object, e As RoutedEventArgs) Handles BtnOk.Click
        DialogResult = True
        Me.Close()
    End Sub
End Class
