using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.LexicalAnalysis;

namespace InterpreterProject.SyntaxAnalysis
{
    public class ParseTable
    {
        Dictionary<Nonterminal, Dictionary<Terminal, ISymbol[]>> table = new Dictionary<Nonterminal, Dictionary<Terminal, ISymbol[]>>();

        public ParseTable()
        {

        }

        public ISymbol[] Get(Nonterminal var, Terminal term)
        {
            if (!table.ContainsKey(var))
                return null;
            if (!table[var].ContainsKey(term))
                return null;
            return table[var][term];
        }

        public ISymbol[] Get(Nonterminal var, Token token)
        {
            Dictionary<Terminal, ISymbol[]> tableRow = table[var];

            foreach (Terminal term in tableRow.Keys)
                if (term.Matches(token))
                    return tableRow[term];

            return null;
        }

        public void Add(Nonterminal var, Terminal term, ISymbol[] production)
        {
            if (!table.ContainsKey(var))
                table[var] = new Dictionary<Terminal, ISymbol[]>();
            table[var][term] = production;
        }
    }
}
