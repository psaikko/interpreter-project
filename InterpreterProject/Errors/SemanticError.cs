using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.LexicalAnalysis;

namespace InterpreterProject.Errors
{
    public class SemanticError : Error
    {
        string description;

        public SemanticError(Token t, string description) 
            : base(t)
        {
            this.description = description;
        }

        override public string GetMessage()
        {
            return String.Format("Semantic error: {0} at {1}", description, t.TextPosition);
        }
    }
}
