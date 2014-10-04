Imports System.Collections.ObjectModel

Public Class VideoQuality
    Inherits ObservableCollection(Of String)
    Public Sub New()
        Add("Lowest")
        Add("144")
        Add("260")
        Add("320")
        Add("720")
        Add("1080")
        Add("Highest")
    End Sub
End Class