using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;

namespace InterpreterProject.Errors
{
    public class SemanticError : IError
    {
        Token t;
        string description;

        public SemanticError(Token t, string description)
        {
            this.t = t;
            this.description = description;
        }

        public string GetMessage()
        {
            return String.Format("Semantic error: {0} at {1}", description, t.pos);
        }
    }
}
