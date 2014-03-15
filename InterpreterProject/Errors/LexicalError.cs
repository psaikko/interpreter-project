using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;

namespace InterpreterProject.Errors
{
    public class LexicalError : Error
    {
        public LexicalError(Token t) : base(t) { }

        override public string GetMessage()
        {
            return String.Format("Lexical error: unexpected character '{0}' at row {1}", t.Lexeme, t.TextPosition);
        }
    }
}
