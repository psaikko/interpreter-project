using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;

namespace InterpreterProject.Errors
{
    public class LexicalError : IError
    {
        Token t;

        public LexicalError(Token t)
        {
            this.t = t;
        }

        public string GetMessage()
        {
            return String.Format("Lexical error: unexpected character '{0}' at row {1}", t.lexeme, t.pos);
        }
    }
}
