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
        public string GetMessage()
        {
            throw new NotImplementedException();
        }
    }
}
