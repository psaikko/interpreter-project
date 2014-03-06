using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class Nonterminal : ISymbol
    {
        public readonly string name;

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
