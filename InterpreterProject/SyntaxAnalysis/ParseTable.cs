using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.LexicalAnalysis;

namespace InterpreterProject.SyntaxAnalysis
{
    // Implementation of a parse table
    public class ParseTable
    {
        Dictionary<Nonterminal, Dictionary<Terminal, ISymbol[]>> table = new Dictionary<Nonterminal, Dictionary<Terminal, ISymbol[]>>();

        // can get production from nonterminal and terminal as usual
        public ISymbol[] Get(Nonterminal var, Terminal term)
        {
            if (!table.ContainsKey(var))
                return null;
            if (!table[var].ContainsKey(term))
                return null;
            return table[var][term];
        }

        // or from a nonterminal and a token, in which case we need to check
        // all the terminals for the given nonterminal and see if one matches the token
        public ISymbol[] Get(Nonterminal var, Token token)
        {
            Dictionary<Terminal, ISymbol[]> tableRow = table[var];

            foreach (Terminal term in tableRow.Keys)
                if (term.Matches(token))
                    return tableRow[term];

            return null;
        }

        // Add a production for some (nonterminal, terminal) pair to the table
        public void Add(Nonterminal var, Terminal term, ISymbol[] production)
        {
            if (!table.ContainsKey(var))
                table[var] = new Dictionary<Terminal, ISymbol[]>();
            table[var][term] = production;
        }
    }
}
