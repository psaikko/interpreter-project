using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.SyntaxAnalysis
{
    public class Nonterminal : ISymbol
    {
        string name;
        public string Name
        {
            get { return name; }
        } 

        public Nonterminal(string name)
        {
            this.name = name;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
