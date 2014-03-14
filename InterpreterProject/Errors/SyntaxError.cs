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
        Token t;

        public SyntaxError(Token t)
        {
            this.t = t;
        }

        public string GetMessage()
        {
            return String.Format("Syntax error: unexpected {2} token \"{0}\" at {1}", t.Lexeme, t.TextPosition, t.Type.Name);
        }
    }
}
