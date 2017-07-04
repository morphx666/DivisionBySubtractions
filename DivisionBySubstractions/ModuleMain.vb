Module ModuleMain
    Sub Main(arg() As String)
        Dim dividend As Long
        Dim divisor As Long
        Dim precision As Integer = 24
        Dim round As Boolean = False

        If arg.Length >= 2 Then
            If Not Long.TryParse(arg(0), dividend) Then
                ShowMessage($"Invalid dividend value: {dividend}", ConsoleColor.Red)
                Exit Sub
            End If
            If Not Long.TryParse(arg(1), divisor) OrElse divisor = 0 Then
                ShowMessage($"Invalid divisor value: {divisor}", ConsoleColor.Red)
                Exit Sub
            End If
            If (arg.Length > 2 AndAlso Not Integer.TryParse(arg(2), precision)) OrElse precision <= 0 Then
                ShowMessage($"Invalid precision value: {precision}", ConsoleColor.Red)
                Exit Sub
            End If
            round = arg.Length > 3 AndAlso arg(3) = "r"
        Else
            ShowMessage($"{My.Application.Info.AssemblyName} {My.Application.Info.Version}", ConsoleColor.White)
            Console.WriteLine()
            ShowMessage("Usage", ConsoleColor.Green)
            ShowMessage($"  {My.Application.Info.AssemblyName} dividend divisor [precision] [r]")
            Console.WriteLine()
            ShowMessage("Example", ConsoleColor.Green)
            ShowMessage($"  {My.Application.Info.AssemblyName} 1 3 24")
            Console.WriteLine()

            dividend = 1
            divisor = 3
            precision = 24
        End If

        ' ---------------------------------------------------------------------

        Dim tmr As New Stopwatch()
        Dim res As String

        tmr.Start()
        res = (dividend / divisor).ToString()
        ShowMessage($"Internal:  {tmr.ElapsedMilliseconds:N0}ms", ConsoleColor.Yellow)
        ShowMessage($"{dividend} / {divisor} = {res}", ConsoleColor.White)
        Console.WriteLine()

        tmr.Restart()
        res = Divide(dividend, divisor, precision, round)
        tmr.Stop()
        ShowMessage($"Algorithm: {tmr.ElapsedMilliseconds:N0}ms", ConsoleColor.Yellow)
        ShowMessage($"{dividend} / {divisor} = {res}", ConsoleColor.White)
        Console.WriteLine()

#If DEBUG Then
        Console.ReadKey()
#End If
    End Sub

    Private Function Divide(dividend As Long, divisor As Long, precision As Integer, round As Boolean) As String
        Dim dividentSign As Integer = Math.Sign(dividend)
        Dim divisorSign As Integer = Math.Sign(divisor)

        dividend = Math.Abs(dividend)
        divisor = Math.Abs(divisor)
        If round Then precision += 1 ' This is to be able to round the last digit

        Dim tmpDivident As Long = dividend

        Dim intCounter As ULong = 0
        Dim decCounter As ULong = 0
        Dim isDecimal As Boolean
        Dim decPart As String = "0"

        If divisor = 0 Then
            If dividend = 0 Then Return "Undefined"
            Return "Infinity"
        End If

        If dividend > 0 Then
            Do
                If tmpDivident < divisor Then
                    If isDecimal Then
                        If decPart.Length >= precision Then
                            isDecimal = False
                            Exit Do
                        End If
                        decPart += decCounter.ToString()
                    Else
                        decPart = ""
                        isDecimal = True
                    End If
                    decCounter = 0

                    Do
                        tmpDivident *= 10
                        If tmpDivident >= divisor Then Exit Do
                        decPart += "0"
                    Loop
                End If
                tmpDivident -= divisor

                If isDecimal Then
                    decCounter += 1UL
                Else
                    intCounter += 1UL
                End If
            Loop Until tmpDivident = 0

            If isDecimal Then decPart += decCounter.ToString()
        Else
            dividentSign = divisorSign
        End If

        If decPart.Length > 0 Then
            If round Then
                Dim p As Integer = precision
                Dim n As Integer
                Dim dp() As Char = decPart.ToCharArray()

                If Integer.Parse(dp(p - 1)) >= 5 Then
                    p -= 2
                    For p = p To 0 Step -1
                        n = Integer.Parse(dp(p)) + 1
                        If n = 10 Then ' I miss the MID$ function ;)
                            dp(p) = "0"c
                        Else
                            dp(p) = Convert.ToChar(n + 48)
                            Exit For
                        End If
                    Next
                End If
                decPart = New String(dp)
                If p = -1 Then intCounter += 1UL
                precision -= 1 ' Restore the original precision
            End If

            If decPart.Length > precision Then decPart = decPart.Substring(0, precision)
        End If

        Return String.Format("{0}{1}.{2}", If(dividentSign <> divisorSign, "-", ""),
                                       intCounter,
                                       decPart.PadRight(precision, "0"c))
    End Function

    Private Sub ShowMessage(msg As String, Optional c As ConsoleColor = ConsoleColor.Gray)
        Console.ForegroundColor = c
        Console.WriteLine(msg)
        Console.ForegroundColor = ConsoleColor.Gray
    End Sub
End Module
