Namespace Extraction
    Public Class ProgressEventArgs
        Inherits EventArgs
        Public Sub New(progressPercentage As Double)
            Me.ProgressPercentage = progressPercentage
        End Sub

        ''' <summary>
        ''' Gets or sets a token whether the operation that reports the progress should be canceled.
        ''' </summary>
        Public Property Cancel() As Boolean

        ''' <summary>
        ''' Gets the progress percentage in a range from 0.0 to 100.0.
        ''' </summary>
        Public Property ProgressPercentage() As Double
    End Class


End Namespace