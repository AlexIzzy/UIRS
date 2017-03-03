using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MathLearn
{
    static class Postfixer
    {
        private static List<string> _functionList = new List<string>
        {
            "sin",
            "cos",
            "tan",
            "sqrt",
            "asin",
            "acos",
            "atan"
        };
        /// <summary>
        /// Основной метод класса.
        /// </summary>
        /// <param name="input">Выражение в инфиксной записи</param>
        /// <returns></returns>
        public static double CalculateExpression(string input)
        {
            var output = ConvertRPN(input); //Преобразование к ОПЗ
            //output = OptimizeExpression(output); //Оптимизация выражения
            double result = Evaluate(output); //Вычисление
            return result;
        }
        /// <summary>
        /// Метод для конвертации в ОПЗ.
        /// </summary>
        /// <param name="input">Выражение, записаннное в инфиксной нотации</param>
        /// <returns>Выражение, преобразованное к ОПЗ. Лексемы и операторы разделены пробелами</returns>
        public static List<string> ConvertRPN(string input)
        {
            input = input.Replace(" ", "");
            var output = new List<string>();
            var operatorStack = new Stack<char>();
            string lexem = string.Empty;

            for (int i = 0; i < input.Length; i++)
            {
                // Проверка на наличие унарного минуса перед лексемой
                if (input[i] == '-' && (i == 0 || input[i - 1] == '('))
                {
                    lexem += input[i];
                    continue;
                }
                // Если символ - цифра или часть числа, считываем до конца и добавляем в выходную строку
                if (char.IsDigit(input[i]))
                {
                    while (!IsOperator(input[i]))
                    {
                        char temp = input[i];
                        IsDecimalDelimiter(ref temp);
                        lexem += temp;
                        i++;
                        if (i == input.Length) break;
                    }
                    output.Add(lexem);
                    lexem = string.Empty;
                    i--;
                    continue;
                }
                // Если входной символ - не цифра, то он либо оператор, либо функция.
                // Проверка на функцию
                if (char.IsLetter(input[i]))
                {
                    // Если входной символ - буква, считать до первого оператора 
                    // (в случае функции это будет открывающая скобка, в случае переменной - все остальное)
                    while (!IsOperator(input[i]))
                    {
                        char temp = input[i];
                        lexem += temp;
                        i++;
                        if (i == input.Length) break;
                    }
                    if (IsFunction(lexem))
                    {
                        i++;
                        int bracketsCount = 1;
                        string temp = "";
                        while (input[i] != ')' || bracketsCount != 1)
                        {
                            if (input[i] == '(') bracketsCount++;
                            if (input[i] == ')') bracketsCount--;
                            temp += input[i];
                            i++;
                        }
                        i++;
                        var inFuncExpression = Evaluate(ConvertRPN(temp));
                        output.Add(inFuncExpression.ToString());
                    }
                    output.Add(lexem);
                    lexem = string.Empty;
                    i--;
                    continue;
                }
                // Особого внимания заслуживают только скобки. В остальных случаях определяется приоритет и выполняются
                // соответсвующие алгоритму действия.
                switch (input[i])
                {
                    case '(':
                        operatorStack.Push(input[i]);
                        break;
                    case ')':
                        char s = operatorStack.Pop();
                        while (s != '(')
                        {
                            output.Add(s.ToString());
                            s = operatorStack.Pop();
                        }
                        break;
                    default:
                        while (operatorStack.Count > 0 && (GetPriority(input[i]) <= GetPriority(operatorStack.Peek())))
                            output.Add(operatorStack.Pop().ToString());
                        operatorStack.Push(input[i]);
                        break;
                }
            }
            // После прохода по всей входной строке выталкиваем оставшиеся операторы из стека в выходную строку
            while (operatorStack.Count > 0)
                output.Add(operatorStack.Pop().ToString());
            return output;
        }

        private static string OptimizeExpression(string input)
        {
            string output = "";
            return output;
        }
        /// <summary>
        /// Метод для вычисления значения выражения, преобразованного (и оптимизированного) к ОПЗ
        /// </summary>
        /// <param name="input">Выражение в ОПЗ</param>
        /// <returns></returns>
        private static double Evaluate(IEnumerable<string> input)
        {
            double result = 0;
            var lexemStack = new Stack<string>();
            foreach (string t in input)
            {
                double tempd;
                if (double.TryParse(t, out tempd))
                    lexemStack.Push(t);
                else if (IsFunction(t))
                {
                    double a = double.Parse(lexemStack.Pop());
                    switch (t)
                    {
                        case "sin":
                            result = Math.Sin(a * Math.PI / 180);
                            break;
                        case "cos":
                            result = Math.Cos(a * Math.PI / 180);
                            break;
                        case "sqrt":
                            result = Math.Sqrt(a);
                            break;
                        case "tan":
                            result = Math.Tan(a * Math.PI / 180);
                            break;
                        case "asin":
                            result = Math.Asin(a);
                            break;
                        case "acos":
                            result = Math.Acos(a);
                            break;
                        case "atan":
                            result = Math.Atan(a);
                            break;
                        default:
                            MessageBox.Show("Ошибка при обработке функции: функция неизвестна!");
                            return double.NaN;
                    }
                    lexemStack.Push(result.ToString());
                }

                else if (IsOperator(char.Parse(t))) //Если оператор, вытащить из стека                                               
                {                              //две последних лексемы и выполнить соответствующее действие
                    double a = double.Parse(lexemStack.Pop());
                    double b = double.Parse(lexemStack.Pop());
                    switch (char.Parse(t))
                    {
                        case '+':
                            result = b + a;
                            break;
                        case '-':
                            result = b - a;
                            break;
                        case '*':
                            result = b * a;
                            break;
                        case '/':
                            result = b / a;
                            break;
                        case '^':
                            result = Math.Pow(b, a);
                            break;
                    }
                    lexemStack.Push(result.ToString()); //отправить результат операции в стек
                }
            }
            return Math.Round(double.Parse(lexemStack.Peek()),3);
        }

        /// <summary>
        /// Метод для преобразования десятичной запятой в точку.
        /// </summary>
        /// <param name="c">Символ десятичной запятой</param>
        private static void IsDecimalDelimiter(ref char c)
        {
            if (".".IndexOf(c) == 0)
                c = ',';
        }
        /// <summary>
        /// Метод, определяющий, является ли текущий символ разделителем (пробелом или знаком "=")
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool IsDelimiter(char c)
        {
            return " =".IndexOf(c) != -1;
        }
        /// <summary>
        /// Метод, определяющий, является ли текущий символ оператором.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool IsOperator(char c)
        {
            return "+-/*()^".IndexOf(c) != -1;
        }
        /// <summary>
        /// Метод, определяющий приоритет математической операции, выражаемой текущим оператором
        /// </summary>
        /// <param name="oper"></param>
        /// <returns></returns>
        private static byte GetPriority(char oper)
        {
            switch (oper)
            {
                case '(': return 1;
                case ')': return 1;
                case '+': return 2;
                case '-': return 2;
                case '*': return 3;
                case '/': return 3;
                case '^': return 4;
                default: return 5;
            }
        }

        private static bool IsFunction(string func)
        {
            return _functionList.Contains(func);
        }
    }
}