Imports System.Collections.Generic
Imports System.IO


Namespace Extraction

    Friend Class Mp3AudioExtractor
        Implements IAudioExtractor

#Region "Variables"
        Private ReadOnly chunkBuffer As List(Of Byte())
        Private ReadOnly fileStream As FileStream
        Private ReadOnly frameOffsets As List(Of UInteger)
        Private ls_warnings As List(Of String)
        Private p_channelMode As Integer
        Private delayWrite As Boolean
        Private p_BitRate As Integer
        Private p_firstFrameHeader As UInteger
        Private hasVbrHeader As Boolean
        Private isVbr As Boolean
        Private p_mpegVersion As Integer
        Private p_sampleRate As Integer
        Private totalFrameLength As UInteger
        Private doWriteVbrHeader As Boolean
        Private strVideoPath As String
#End Region

#Region "Construction"
        Public Sub New(path As String)
            Me.VideoPath = path
            Me.fileStream = New FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 64 * 1024)
            Me.ls_warnings = New List(Of String)()
            Me.chunkBuffer = New List(Of Byte())()
            Me.frameOffsets = New List(Of UInteger)()
            Me.delayWrite = True
        End Sub
#End Region

#Region "Props"
        Public Property VideoPath As String Implements IAudioExtractor.VideoPath
            Get
                Return strVideoPath
            End Get
            Private Set(val As String)
                strVideoPath = val
            End Set
        End Property


        Public ReadOnly Property Warnings() As IEnumerable(Of String)
            Get
                Return Me.ls_warnings
            End Get
        End Property
#End Region

#Region "Freame writer"

        Public Sub WriteChunk(chunk As Byte(), timeStamp As UInteger) Implements IAudioExtractor.WriteChunk
            Me.chunkBuffer.Add(chunk)
            Me.ParseMp3Frames(chunk)

            If Me.delayWrite AndAlso Me.totalFrameLength >= 65536 Then
                Me.delayWrite = False
            End If

            If Not Me.delayWrite Then
                Me.Flush()
            End If
        End Sub
#End Region

#Region "Disposition"

        Public Sub Dispose() Implements IDisposable.Dispose
            Me.Flush()

            If Me.doWriteVbrHeader Then
                Me.fileStream.Seek(0, SeekOrigin.Begin)
                Me.WriteVbrHeader(False)
            End If

            Me.fileStream.Dispose()
        End Sub
#End Region


        Private Sub Flush()
            For Each chunk As Byte() In chunkBuffer
                Me.fileStream.Write(chunk, 0, chunk.Length)
            Next

            Me.chunkBuffer.Clear()
        End Sub
        Shared mpeg1BitRate As Integer() = New Int32() {0, 32, 40, 48, 56, 64, _
                80, 96, 112, 128, 160, 192, _
                224, 256, 320, 0}
        Shared mpeg2XBitRate As Integer() = New Int32() {0, 8, 16, 24, 32, 40, _
                48, 56, 64, 80, 96, 112, _
                128, 144, 160, 0}
        Shared mpeg1SampleRate As Integer() = New Int32() {44100, 48000, 32000, 0}
        Shared mpeg20SampleRate As Integer() = New Int32() {22050, 24000, 16000, 0}
        Shared mpeg25SampleRate As Integer() = New Int32() {11025, 12000, 8000, 0}

#Region "Helpers"
        Private Shared Function GetFrameDataOffset(mpegVersion As Integer, channelMode As Integer) As Integer
            Return 4 + (If(mpegVersion = 3, (If(channelMode = 3, 17, 32)), (If(channelMode = 3, 9, 17))))
        End Function

        Private Shared Function GetFrameLength(mpegVersion As Integer, bitRate As Integer, sampleRate As Integer, padding As Integer) As Integer
            Dim vFlag As Int32 = If(mpegVersion = 3, 144, 72)
            bitRate = vFlag * bitRate
            Return Integer.Parse(Math.Truncate(bitRate / sampleRate) + padding)
        End Function
        ''' <summary>
        ''' Parses the main information from the frame
        ''' </summary>
        ''' <param name="header">The header flag</param>
        ''' <param name="mpegVersion"></param>
        ''' <param name="layer"></param>
        ''' <param name="bitrate"></param>
        ''' <param name="padding"></param>
        ''' <param name="channelMode"></param>
        ''' <param name="sampleRate"></param>
        ''' <remarks></remarks>
        Private Sub getMp3FrameInfo(ByRef header As ULong, ByRef mpegVersion As Int32, ByRef layer As Int32, ByRef bitrate As Int32, ByRef padding As Int32, _
                                    ByRef channelMode As Int32, ByRef sampleRate As Int32)
            mpegVersion = BitHelper.Read(header, 2)
            layer = BitHelper.Read(header, 2)
            BitHelper.Read(header, 1)
            bitrate = BitHelper.Read(header, 4)
            sampleRate = BitHelper.Read(header, 2)
            padding = BitHelper.Read(header, 1)
            BitHelper.Read(header, 1)
            channelMode = BitHelper.Read(header, 2)
        End Sub

        ''' <summary>
        ''' Gets ssamplerate, based on the version of MPEG
        ''' </summary>
        ''' <param name="mpgVersion"></param>
        ''' <param name="sampleRate"></param>
        ''' <remarks></remarks>
        Private Shared Sub calcSampleRate(mpgVersion As Int32, ByRef sampleRate As Int32)
            Select Case mpgVersion
                Case 2
                    sampleRate = mpeg20SampleRate(sampleRate)
                Case 3
                    sampleRate = mpeg1SampleRate(sampleRate)
                Case Else
                    sampleRate = mpeg25SampleRate(sampleRate)
            End Select
        End Sub
        ''' <summary>
        ''' Checks if the given frame is a VBR Header frame.
        ''' </summary>
        ''' <param name="buffer">The buffer for the frame</param>
        ''' <param name="frameOffsets">The list of frame offsets</param>
        ''' <param name="offset">The current offset</param>
        ''' <param name="mpgVersion">MPEG Version</param>
        ''' <param name="chanMode">Channel count</param>
        ''' <param name="mp3Extractor">The extractor to modify</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function checkForVBRHeader(buffer As Byte(), frameOffsets As List(Of UInteger), offset As Int32, mpgVersion As Int32, chanMode As Int32 _
                                                  , ByRef mp3Extractor As Mp3AudioExtractor)
            Dim isVbrHeaderFrame As Boolean = False
            If frameOffsets.Count = 0 Then 'No frames have been found 
                ' Check for an existing VBR header just to be safe (I haven't seen any in FLVs)
                Dim hdrOffset As Integer = offset + GetFrameDataOffset(mpgVersion, chanMode)

                If BigEndianBitConverter.ToUInt32(buffer, hdrOffset) = &H58696E67 Then
                    ' "Xing"
                    isVbrHeaderFrame = True
                    mp3Extractor.delayWrite = False
                    mp3Extractor.hasVbrHeader = True
                End If
            End If
            Return isVbrHeaderFrame
        End Function
#End Region


        Private Sub ParseMp3Frames(buffer As Byte())
            Dim offset As Integer = 0
            Dim length As Integer = buffer.Length

            While length >= 4
                Dim mpegVersion As Integer
                Dim sampleRate As Integer
                Dim channelMode As Integer
                Dim layer As Integer
                Dim bitRate As Integer
                Dim padding As Integer

                Dim header As ULong = CULng(BigEndianBitConverter.ToUInt32(buffer, offset)) << 32

                If BitHelper.Read(header, 11) <> &H7FF Then
                    Exit While
                End If

                getMp3FrameInfo(header, mpegVersion, layer, bitRate, padding, channelMode, sampleRate)
                If mpegVersion = 1 OrElse layer <> 1 OrElse bitRate = 0 OrElse bitRate = 15 OrElse sampleRate = 3 Then
                    Exit While
                End If
                bitRate = (If(mpegVersion = 3, mpeg1BitRate(bitRate), mpeg2XBitRate(bitRate))) * 1000
                calcSampleRate(mpegVersion, sampleRate)
                Dim frameLenght As Integer = GetFrameLength(mpegVersion, bitRate, sampleRate, padding)
                If frameLenght > length Then
                    Exit While
                End If

                Dim isVbrHeaderFrame As Boolean = checkForVBRHeader(buffer, frameOffsets, offset, mpegVersion, channelMode, Me)

                If Not isVbrHeaderFrame Then
                    If Me.p_BitRate = 0 Then
                        Me.p_BitRate = bitRate
                        Me.p_mpegVersion = mpegVersion
                        Me.p_sampleRate = sampleRate
                        Me.p_channelMode = channelMode
                        Me.p_firstFrameHeader = BigEndianBitConverter.ToUInt32(buffer, offset)
                    ElseIf Not Me.isVbr AndAlso bitRate <> Me.p_BitRate Then
                        Me.isVbr = True

                        If Not Me.hasVbrHeader Then
                            If Me.delayWrite Then
                                Me.WriteVbrHeader(True)
                                Me.doWriteVbrHeader = True
                                Me.delayWrite = False
                            Else
                                Me.ls_warnings.Add("Detected VBR too late, cannot add VBR header.")
                            End If
                        End If
                    End If
                End If

                Me.frameOffsets.Add(Me.totalFrameLength + offset)
                offset += frameLenght
                length -= frameLenght
            End While

            Me.totalFrameLength += buffer.Length
        End Sub

        Private Sub WriteVbrHeader(isPlaceholder As Boolean)
            Dim buffer As Byte() = New Byte(GetFrameLength(Me.p_mpegVersion, 64000, Me.p_sampleRate, 0)) {}

            If Not isPlaceholder Then
                Dim header As UInteger = Me.p_firstFrameHeader
                Dim dataOffset As Integer = GetFrameDataOffset(Me.p_mpegVersion, Me.p_channelMode)
                header = header And &HFFFE0DFFUI
                header = header Or (If(p_mpegVersion = 3, 5, 8)) << 12
                BitHelper.CopyBytes(buffer, 0, BigEndianBitConverter.GetBytes(header))
                BitHelper.CopyBytes(buffer, dataOffset, BigEndianBitConverter.GetBytes(&H58696E67))
                BitHelper.CopyBytes(buffer, dataOffset + 4, BigEndianBitConverter.GetBytes(&H7))
                BitHelper.CopyBytes(buffer, dataOffset + 8, BigEndianBitConverter.GetBytes(frameOffsets.Count))
                BitHelper.CopyBytes(buffer, dataOffset + 12, BigEndianBitConverter.GetBytes(totalFrameLength))

                Dim i As Integer = 0
                While i < 100
                    Dim frameIndex As Integer = ((i / 100.0) * Me.frameOffsets.Count)

                    buffer(dataOffset + 16 + i) = (Me.frameOffsets(frameIndex) / Me.totalFrameLength * 256.0)
                    System.Math.Max(System.Threading.Interlocked.Increment(i), i - 1)
                End While
            End If

            Me.fileStream.Write(buffer, 0, buffer.Length)
        End Sub
    End Class


End Namespace