Imports System.Runtime.CompilerServices
Imports System.IO

Public Module ImagingExtensions

    <Extension>
    Public Function ToImage(bytes As Byte()) As ImageSource
        Dim biImg As New BitmapImage()
        Dim MS As New MemoryStream(bytes)
        biImg.BeginInit()
        biImg.StreamSource = MS
        biImg.EndInit()
        Return biImg
    End Function
End Module
