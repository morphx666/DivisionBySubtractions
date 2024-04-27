using System.Diagnostics;

// Example:
// ./dbs 17.27 7285.1957 5000 z

namespace DivisionBySubtractions {
    public class DivisionBySubtractions {
        public static void Main(string[] args) {
            double dividend = 0;
            double divisor = 0;
            int precision = 24;
            bool round = false;
            bool removeTrailingZeros = false;

            if(args.Length >= 2) {
                for(int i = 0; i < args.Length; i++) {
                    switch(i) {
                        case 0:
                            if(!double.TryParse(args[i], out dividend)) {
                                ShowMessage($"Invalid dividend value: {dividend}", ConsoleColor.Red);
                                return;
                            }
                            break;
                        case 1:
                            if(!double.TryParse(args[i], out divisor) || divisor == 0) {
                                ShowMessage($"Invalid divisor value: {divisor}", ConsoleColor.Red);
                                return;
                            }
                            break;
                        default:
                            if(char.IsNumber(args[i][0])) {
                                if(!int.TryParse(args[i], out precision) || precision <= 0) {
                                    ShowMessage($"Invalid precision value: {precision}", ConsoleColor.Red);
                                    return;
                                }
                            } else if(args[i] == "z") {
                                removeTrailingZeros = true;
                            } else if(args[i] == "r") {
                                round = true;
                            }
                            break;
                    }
                }
            } else {
                string asmName = typeof(DivisionBySubtractions).Assembly.GetName().Name;
                string asmVersion = typeof(DivisionBySubtractions).Assembly.GetName().Version.ToString();

                ShowMessage($"{asmName} {asmVersion}\n", ConsoleColor.White);
                ShowMessage("Usage", ConsoleColor.Green);
                ShowMessage($"  {asmName} dividend divisor [precision] [r] [z]\n");
                ShowMessage("Arguments", ConsoleColor.Green);
                ShowMessage($"  dividend: a number to be divided by another number; the numerator on a division.");
                ShowMessage($"  divisor: a number by which another number is to be divided; the denominator.");
                ShowMessage($"  precision: optional parameter to specify the number of decimal digits to calculate.");
                ShowMessage($"  r: optional parameter to force rounding on the last digit.");
                ShowMessage($"  z: optional parameter to force the removal of trailing zeros.\n");
                ShowMessage("Example", ConsoleColor.Green);
                ShowMessage($"  {asmName} 1 3 24\n");

                //dividend = 1;
                //divisor = 3;
                //precision = 24;

                //dividend = 77;
                //divisor = 1923;
                //precision = 13;
                //round = true;

                //dividend = 99977099456547945;
                //divisor = 1912584907252377.128;
                //precision = 320;
                //round = true;

                dividend = 17.27;
                divisor = 7285.1957;
                precision = Console.WindowWidth - 25;
                removeTrailingZeros = true;
            }

            string res;

            Stopwatch tmr = Stopwatch.StartNew();
            res = (dividend / divisor).ToString();
            ShowMessage($"Native:    {tmr.ElapsedMilliseconds:N0}ms", ConsoleColor.Yellow);
            ShowMessage($"{dividend} / {divisor} = {res}\n", ConsoleColor.White);

            tmr.Restart();
            res = Divide(dividend, divisor, precision, round, removeTrailingZeros);
            ShowMessage($"Algorithm: {tmr.ElapsedMilliseconds:N0}ms", ConsoleColor.Yellow);
            ShowMessage($"{dividend} / {divisor} = {res}\n", ConsoleColor.White);
        }

        private static string Divide(double dividend, double divisor, int precision, bool round, bool removeTrailingZeros) {
            int dividendSign = Math.Sign(dividend);
            int divisorSign = Math.Sign(divisor);

            dividend = Math.Abs(dividend);
            divisor = Math.Abs(divisor);
            if(round) precision += 1; // This is to be able to round the last digit

            Func<double, int> lastDigitIndex = (double n) => {
                string ns = n.ToString();
                return ns.Contains('.') ? ns.Split('.')[1].Length : 0;
            };

            int dividendMult = lastDigitIndex(dividend);
            dividend *= Math.Pow(10, dividendMult);
            int divisorMult = lastDigitIndex(divisor);
            divisor *= Math.Pow(10, divisorMult);

            ulong tmpDividend = (ulong)dividend;
            ulong tmpDivisor = (ulong)divisor;

            ulong intCounter = 0;
            ulong decCounter = 0;
            bool isDecimal = false;
            string decPart = "0";

            if(tmpDivisor == 0) {
                if(tmpDividend == 0) return "Undefined";
                return "Infinity";
            }

            if(tmpDividend > 0) {
                do { // Main division by subtractions
                    if(tmpDividend < tmpDivisor) {
                        if(isDecimal) {
                            if(decPart.Length >= precision) {
                                isDecimal = false;
                                break;
                            }
                            decPart += decCounter;
                        } else {
                            decPart = "";
                            isDecimal = true;
                        }
                        decCounter = 0;

                        while(true) {
                            tmpDividend *= 10;
                            if(tmpDividend >= tmpDivisor) break;
                            decPart += '0';
                        }
                    }
                    tmpDividend -= tmpDivisor;

                    if(isDecimal) {
                        decCounter += 1;
                    } else {
                        intCounter += 1;
                    }
                } while(tmpDividend != 0);

                if(isDecimal) decPart += decCounter;
            } else {
                dividendSign = divisorSign;
            }

            if(decPart.Length >= precision) {
                if(round) {
                    int p = precision;
                    int n;
                    char[] dp = decPart.ToCharArray();

                    if((dp[p - 1] - 48) >= 5) {
                        p -= 2;
                        for(; p >= 0; p--) {
                            n = (dp[p] - 48) + 1;
                            if(n == 10) {
                                dp[p] = '0';
                            } else {
                                dp[p] = (char)(n + 48);
                                break;
                            }
                        }
                    }
                    decPart = string.Join("", dp);
                    if(p == -1) intCounter += 1;
                    precision -= 1; // Restore the original precision
                }

                if(decPart.Length > precision) decPart = decPart[..precision];
            }

            string result = $"{intCounter}.{decPart.PadRight(precision, '0')}";
            int offset = divisorMult - dividendMult;

            if(offset != 0) {
                int pp = result.IndexOf('.');
                int s = offset > 0 ? 1 : -1;
                char[] b = result.ToCharArray();
                int bl = b.Length - 1;

                do {
                    (b[pp], b[pp + s]) = (b[pp + s], b[pp]);
                    pp += s;
                    offset -= s;

                    if(pp + s < 0) {
                        b = b[..bl];
                        b[0] = '0';
                        pp += 1;
                    }
                } while(offset != 0);

                result = new string(b);

                string[] tokens = result.Split('.');
                intCounter = ulong.Parse(tokens[0]);
                decPart = tokens[1].Length > precision ? tokens[1][..(precision - 1)] : tokens[1];
                result = $"{intCounter}.{decPart.PadRight(precision, '0')}";
            }

            if(removeTrailingZeros) {
                while(result[^1] == '0' && result[^2] != '.') {
                    result = result[..^1];
                }
            }

            return $"{(dividendSign != divisorSign ? "-" : "")}{result}";
        }

        private static void ShowMessage(string msg, ConsoleColor c = ConsoleColor.Gray) {
            ConsoleColor oc = Console.ForegroundColor;
            Console.ForegroundColor = c;
            Console.WriteLine(msg);
            Console.ForegroundColor = oc;
        }
    }
}
