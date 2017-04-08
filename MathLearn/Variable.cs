using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLearn
{
    class Variable : Expression
    {
        public float Multiplier { get; set; }
        public Variable(string body, float mult, Postfixer.ElementKind type = Postfixer.ElementKind.Variable) : base(body, type)
        {
            ExpressionBody = body;
            ExpressionType = type;
            Multiplier = mult;
        }
    }
}
