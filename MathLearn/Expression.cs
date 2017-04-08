using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLearn
{
    public class Expression
    {
        public string ExpressionBody { get; set; }

        public Postfixer.ElementKind ExpressionType { get; set; }

        public Expression(string body, Postfixer.ElementKind type)
        {
            ExpressionType = type;
            ExpressionBody = body;
        }
    }
}
