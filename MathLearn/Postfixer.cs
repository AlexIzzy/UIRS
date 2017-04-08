using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MathLearn
{
    public static class Postfixer
    {
        public enum ElementKind
        {
            Constant = 1,
            Variable,
            Operator,
            Function,
            VariableExpression
        }
        private static List<string> _functionList = new List<string>
        {
            "sin",
            "cos",
            "tan",
            "sqrt",
            "asin",
            "acos",
            "atan",
            "^"
        };
        /// <summary>
        /// Основной метод класса.
        /// </summary>
        /// <param name="input">Выражение в инфиксной записи</param>
        /// <returns></returns>
        public static string CalculateExpression(string input)
        {
            var output = ConvertRPN(input); //Преобразование к ОПЗ
            var result = Evaluate(output);
            return result;
        }

        public static double SolveEquotation(IDictionary<string, ElementKind> input)
        {
            return 0;
        }


        /// <summary>
        /// Метод для конвертации в ОПЗ.
        /// </summary>
        /// <param name="input">Выражение, записаннное в инфиксной нотации</param>
        /// <returns>Выражение, преобразованное к ОПЗ. Лексемы и операторы разделены пробелами</returns>

        public static List<Expression> ConvertRPN(string input)
        {
            input = input.Replace(" ", "");
            var output = new List<Expression>();
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
                    output.Add(new Expression(lexem, ElementKind.Constant));
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
                        output.Add(new Expression(inFuncExpression, ElementKind.Constant));
                        output.Add(new Expression(lexem, ElementKind.Function));
                    }
                    else output.Add(new Variable(lexem, 1));
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
                            output.Add(new Expression(s.ToString(), ElementKind.Operator));
                            s = operatorStack.Pop();
                        }
                        break;
                    default:
                        while (operatorStack.Count > 0 && (GetPriority(input[i]) <= GetPriority(operatorStack.Peek())))
                            output.Add(new Expression(operatorStack.Pop().ToString(), ElementKind.Operator));
                        operatorStack.Push(input[i]);
                        break;
                }
            }
            // После прохода по всей входной строке выталкиваем оставшиеся операторы из стека в выходную строку
            while (operatorStack.Count > 0)
                output.Add(new Expression(operatorStack.Pop().ToString(), ElementKind.Operator));
            return output;
        }

        private static Stack<Expression> OptimizeExpression(IEnumerable<Expression> input)
        {
            var optimizedExpression = new Stack<Expression>();
            foreach (var value in input)
            {
                switch (value.ExpressionType)
                {
                    case ElementKind.Constant:
                        optimizedExpression.Push(value);
                        break;
                    case ElementKind.Variable:
                        optimizedExpression.Push(value);
                        break;
                    case ElementKind.Operator:
                        var operandA = optimizedExpression.Pop();
                        var operandB = optimizedExpression.Pop();
                        if (IsVarOrVarExp(operandA) || IsVarOrVarExp(operandB))
                        {
                            var operands = new[] { operandA, operandB };
                            var exprList = new List<Expression>();
                            var exprBody = $"{operandA.ExpressionBody} {value.ExpressionBody} {operandB.ExpressionBody}";
                            foreach (var expression in operands)
                                switch (expression.ExpressionType)
                                {
                                    case ElementKind.Variable:
                                        exprList.Add(expression);
                                        break;
                                    case ElementKind.VariableExpression:
                                        exprList.AddRange(((VariableExpression)expression).ExpressionsList);
                                        break;
                                }
                            CompressExpression(new VariableExpression(exprBody, exprList));
                        }
                        else optimizedExpression.Push(new Expression(
                            OperatorEval(value.ExpressionBody[0],
                                double.Parse(operandA.ExpressionBody), double.Parse(operandB.ExpressionBody)).ToString(),
                                    ElementKind.Constant));
                        break;
                    case ElementKind.Function:
                        double result;
                        var funcOperandA = double.Parse(optimizedExpression.Pop().ExpressionBody);
                        if (value.ExpressionBody == "^")
                        {
                            var powerBase = double.Parse(optimizedExpression.Pop().ExpressionBody);
                            result = FunctionEval(value.ExpressionBody, funcOperandA, powerBase);
                        }
                        else result = FunctionEval(value.ExpressionBody, funcOperandA);
                        optimizedExpression.Push(new Expression(result.ToString(), ElementKind.Constant));
                        break;
                    case ElementKind.VariableExpression:
                        CompressExpression(value as VariableExpression);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return optimizedExpression;
        }

        private static void CompressExpression(VariableExpression expression)
        {

        }

        private static string Evaluate(IEnumerable<Expression> input)
        {
            var lexemStack = new Stack<Expression>();
            foreach (var t in input)
            {
                double result;
                switch (t.ExpressionType)
                {
                    case ElementKind.Constant:
                        lexemStack.Push(t);
                        break;
                    case ElementKind.Function:
                        var a = double.Parse(lexemStack.Pop().ExpressionBody);
                        if (t.ExpressionBody == "^")
                        {
                            var powerBase = double.Parse(lexemStack.Pop().ExpressionBody);
                            result = FunctionEval(t.ExpressionBody, a, powerBase);
                        }
                        else result = FunctionEval(t.ExpressionBody, a);
                        lexemStack.Push(new Expression(result.ToString(), ElementKind.Constant));
                        break;
                    case ElementKind.Operator:
                        a = double.Parse(lexemStack.Pop().ExpressionBody);
                        var b = double.Parse(lexemStack.Pop().ExpressionBody);
                        result = OperatorEval(t.ExpressionBody[0], a, b);
                        lexemStack.Push(new Expression(result.ToString(), ElementKind.Constant));
                        break;
                }
            }
            return lexemStack.Peek().ExpressionBody;
        }

        private static double OperatorEval(char oper, double a, double b)
        {
            double result;
            switch (oper)
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
                default:
                    MessageBox.Show("Ошибка при обработке оператора: оператор неизвестен!");
                    return double.NaN;
            }
            return result;
        }

        private static double FunctionEval(string func, double a, double b = 0)
        {
            double result;
            switch (func)
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
                case "^":
                    result = Math.Pow(b, a);
                    break;
                default:
                    MessageBox.Show("Ошибка при обработке функции: функция неизвестна!");
                    return double.NaN;
            }
            return result;
        }

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

        private static bool IsVarOrVarExp(Expression operand)
        {
            return operand.ExpressionType == ElementKind.Variable ||
                operand.ExpressionType == ElementKind.VariableExpression;
        }
        /// <summary>
        /// Метод, определяющий, является ли текущий символ оператором.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool IsOperator(char c)
        {
            return "+-/*()".IndexOf(c) != -1;
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
                case '(':
                    return 1;
                case ')':
                    return 1;
                case '+':
                    return 2;
                case '-':
                    return 2;
                case '*':
                    return 3;
                case '/':
                    return 3;
                default:
                    return 4;
            }
        }

        private static bool OperatorOfGroup(char oper)
        {
            return "*/".IndexOf(oper) != -1;
        }

        private static bool IsFunction(string func)
        {
            return _functionList.Contains(func);
        }
    }
}