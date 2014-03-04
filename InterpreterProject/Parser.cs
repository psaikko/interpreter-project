using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class Parser
    {
        Dictionary<CFG.Variable, Dictionary<CFG.Terminal, CFG.ISymbol[]>> table;
        CFG.Variable start;

        public Parser(Dictionary<CFG.Variable, Dictionary<CFG.Terminal, CFG.ISymbol[]>> parseTable, CFG.Variable start)
        {
            this.table = parseTable;
            this.start = start;
        }

        public bool Parse(IEnumerable<Token> tokens)
        {
            Stack<CFG.ISymbol> s = new Stack<CFG.ISymbol>();
            s.Push(CFG.Terminal.EOF);
            s.Push(start);

            IEnumerator<Token> ts = tokens.GetEnumerator();

            ts.MoveNext();

            while (s.Count > 0)
            {
                Console.WriteLine("=========================================================");
                Console.WriteLine("PARSE: Stack " + SymbolsToString(s));
                Console.WriteLine("PARSE: expecting " + s.Peek());
                Console.WriteLine("PARSE: token " + ts.Current);

                if (s.Peek() is CFG.Terminal)
                {
                    CFG.Terminal term = s.Pop() as CFG.Terminal;
                    if (term == CFG.Terminal.epsilon)
                    {
                        Console.WriteLine("PARSE: disregard epsilon");
                        continue;
                    }
                    if (term.Matches(ts.Current))
                    {
                        Console.WriteLine("PARSE: Terminal match");
                        ts.MoveNext();                        
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("PARSE: Terminal mismatch");
                        goto Error;
                    }
                }
                else // top of stack is a nonterminal
                {
                    CFG.Variable var = s.Pop() as CFG.Variable;

                    CFG.ISymbol[] production = tableGet(var, ts.Current);

                    if (production == null)
                    {
                        Console.WriteLine("PARSE: No such production");
                        goto Error;
                    }
                    else
                    {
                        Console.WriteLine("PARSE: Using production " + SymbolsToString(production));
                        for (int i = production.Length - 1; i >= 0; i--)
                            s.Push(production[i]);
                    }
                }
            }

            return true;
        Error:
            Console.WriteLine("Parse error");
            return false;
        }

        private CFG.ISymbol[] tableGet(CFG.Variable var, Token token)
        {
            Dictionary<CFG.Terminal, CFG.ISymbol[]> tableRow = table[var];

            foreach (CFG.Terminal term in tableRow.Keys)
                if (term.Matches(token))
                    return tableRow[term];

            return null;
        }

        private string SymbolsToString(IEnumerable<CFG.ISymbol> production)
        {
            if (production == null) return "null";

            string s = "";
            foreach (CFG.ISymbol symbol in production)
            {
                s += symbol + " ";
            }
            return s;
        }
    }
}
