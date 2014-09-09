
Namespace Extraction
    Public Interface IAudioExtractor
        Inherits IDisposable

        ReadOnly Property VideoPath As String

        '''<exception cref="AudioExtractionException">An error occured while writing the chunk.</exception>
        Sub WriteChunk(chunk As Byte(), timeStamp As UInt32)
    end interface
End Namespace