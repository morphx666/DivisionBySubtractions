Module ModuleMain
    Sub Main(arg() As String)
        Dim dividend As Double
        Dim divisor As Double
        Dim precision As Integer = 24
        Dim round As Boolean = False
        Dim removeTrailingZeros As Boolean = False

        ' 915.27  37768.2313358 32

        If arg.Length >= 2 Then
            For i As Integer = 0 To arg.Length - 1
                Select Case i
                    Case 0
                        If Not Double.TryParse(arg(i), dividend) Then
                            ShowMessage($"Invalid dividend value: {dividend}", ConsoleColor.Red)
                            Exit Sub
                        End If
                    Case 1
                        If Not Double.TryParse(arg(i), divisor) OrElse divisor = 0 Then
                            ShowMessage($"Invalid divisor value: {divisor}", ConsoleColor.Red)
                            Exit Sub
                        End If
                    Case Else
                        If Char.IsNumber(arg(i)(0)) Then
                            If Not Integer.TryParse(arg(i), precision) OrElse precision <= 0 Then
                                ShowMessage($"Invalid precision value: {precision}", ConsoleColor.Red)
                                Exit Sub
                            End If
                        ElseIf arg(i) = "z" Then
                            removeTrailingZeros = True
                        ElseIf arg(i) = "r" Then
                            round = True
                        End If
                End Select
            Next
        Else
            ShowMessage($"{My.Application.Info.AssemblyName} {My.Application.Info.Version}", ConsoleColor.White)
            Console.WriteLine()
            ShowMessage("Usage", ConsoleColor.Green)
            ShowMessage($"  {My.Application.Info.AssemblyName} dividend divisor [precision] [r] [z]")
            Console.WriteLine()
            ShowMessage("Arguments", ConsoleColor.Green)
            ShowMessage($"  dividend: a number to be divided by another number; the numerator on a division.")
            ShowMessage($"  divisor: a number by which another number is to be divided; the denominator.")
            ShowMessage($"  precision: optional parameter to specify the number of decimal digits to calculate.")
            ShowMessage($"  r: optional parameter to force rounding on the last digit.")
            ShowMessage($"  z: optional parameter to force the removal of trailing zeros.")
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
        res = Divide(dividend, divisor, precision, round, removeTrailingZeros)
        tmr.Stop()
        ShowMessage($"Algorithm: {tmr.ElapsedMilliseconds:N0}ms", ConsoleColor.Yellow)
        ShowMessage($"{dividend} / {divisor} = {res}", ConsoleColor.White)
        Console.WriteLine()

#If DEBUG Then
        Console.ReadKey()
#End If
    End Sub

    Private Function Divide(dividend As Double, divisor As Double, precision As Integer, round As Boolean, removeTrailingZeros As Boolean) As String
        Dim dividentSign As Integer = Math.Sign(dividend)
        Dim divisorSign As Integer = Math.Sign(divisor)

        dividend = Math.Abs(dividend)
        divisor = Math.Abs(divisor)
        If round Then precision += 1 ' This is to be able to round the last digit

        Dim lastDigitIndex = Function(n As Double) As Integer
                                 Dim ns As String = n.ToString()
                                 If ns.Contains(".") Then
                                     Return ns.Split("."c)(1).Length
                                 Else
                                     Return 0
                                 End If
                             End Function

        Dim dividendMult As Integer = lastDigitIndex(dividend)
        dividend *= 10 ^ dividendMult
        Dim divisorMult As Integer = lastDigitIndex(divisor)
        divisor *= 10 ^ divisorMult

        Dim tmpDividend As Long = CType(dividend, Long)
        Dim tmpDivisor As Long = CType(divisor, Long)

        Dim intCounter As ULong = 0
        Dim decCounter As ULong = 0
        Dim isDecimal As Boolean
        Dim decPart As String = "0"
        Dim p As Integer
        Dim tmp As Char

        If divisor = 0 Then
            If dividend = 0 Then Return "Undefined"
            Return "Infinity"
        End If

        If tmpDividend > 0 Then
            Do
                If tmpDividend < tmpDivisor Then
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
                        tmpDividend *= 10
                        If tmpDividend >= tmpDivisor Then Exit Do
                        decPart += "0"
                    Loop
                End If
                tmpDividend -= tmpDivisor

                If isDecimal Then
                    decCounter += 1UL
                Else
                    intCounter += 1UL
                End If
            Loop Until tmpDividend = 0

            If isDecimal Then decPart += decCounter.ToString()
        Else
            dividentSign = divisorSign
        End If

        If decPart.Length >= precision Then
            If round Then
                p = precision
                Dim n As Integer
                Dim dp() As Char = decPart.ToCharArray()

                If Integer.Parse(dp(p - 1)) >= 5 Then
                    p -= 2
                    For p = p To 0 Step -1
                        n = Integer.Parse(dp(p)) + 1
                        If n = 10 Then ' I miss the left-hand-side MID$ function from VB6 ;)
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

        Dim result As String = $"{intCounter}.{decPart.PadRight(precision, "0"c)}"
        Dim offset As Integer = divisorMult - dividendMult

        If offset <> 0 Then
            Dim pp As Integer = result.IndexOf("."c)
            Dim s As Integer = If(offset > 0, 1, -1)
            Dim b() As Char = result.ToCharArray()

            Do
                tmp = b(pp)
                b(pp) = b(pp + s)
                b(pp + s) = tmp
                pp += s
                offset -= s

                If pp + s < 0 Then
                    ReDim Preserve b(b.Length)
                    Array.Copy(b, 0, b, 1, b.Length - 1)
                    b(0) = "0"c
                    pp += 1
                End If
            Loop Until offset = 0

            result = New String(b)

            Dim tokens() As String = result.Split("."c)
            intCounter = ULong.Parse(tokens(0))
            decPart = If(tokens(1).Length > precision, tokens(1).Substring(0, precision - 1), tokens(1))
            result = $"{intCounter}.{decPart.PadRight(precision, "0"c)}"
        End If

        If removeTrailingZeros Then
            While result.Last() = "0"c AndAlso Not result(result.Length - 2) = "."c
                result = result.Substring(0, result.Length - 1)
            End While
        End If

        Return $"{If(dividentSign <> divisorSign, "-", "")}{result}"
    End Function

    Private Sub ShowMessage(msg As String, Optional c As ConsoleColor = ConsoleColor.Gray)
        Dim oc As ConsoleColor = Console.ForegroundColor
        Console.ForegroundColor = c
        Console.WriteLine(msg)
        Console.ForegroundColor = oc
    End Sub
End Module