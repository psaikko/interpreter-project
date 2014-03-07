using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;

namespace InterpreterProject.Errors
{
    public class SyntaxError : IError
    {
        public Token t;

        public SyntaxError(Token t)
        {
            this.t = t;
        }

        public string GetMessage()
        {
            return String.Format("Syntax error: unexpected token \"{0}\" at row {1} position {2}", t.lexeme, t.row, t.col);
        }
    }
}
