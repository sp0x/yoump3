Public Class STD
    Public Shared Function sprintf(pattern As String, ParamArray vals As String())
        Return String.Format(pattern, vals)
    End Function
    Public Shared Function inlineHelper(Of T)(ByRef target As T, ByVal value As T) As T
        target = value
        Return value
    End Function
End Class
