using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;

namespace InterpreterProject.Errors
{
    // Interface for different errors that can occur during interpretation process
    public interface IError
    {
        string GetMessage();
    }
}
