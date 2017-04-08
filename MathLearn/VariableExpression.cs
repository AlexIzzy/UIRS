using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLearn
{
    class VariableExpression : Expression
    {
        public List<Expression> ExpressionsList { get; set; }
        public VariableExpression(string body, List<Expression> expList, Postfixer.ElementKind type = Postfixer.ElementKind.VariableExpression)
            : base(body, type)
        {
            ExpressionBody = body;
            ExpressionType = type;
            ExpressionsList = expList;
        }
    }
}
