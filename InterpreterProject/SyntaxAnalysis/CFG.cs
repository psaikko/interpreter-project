using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.SyntaxAnalysis
{
    // Representation of a context free grammar
    public class CFG
    {
        Dictionary<Nonterminal, List<ISymbol[]>> productions = new Dictionary<Nonterminal, List<ISymbol[]>>();

        Nonterminal startSymbol;
        public Nonterminal StartSymbol
        {
            get { return startSymbol; }
        }

        ICollection<Terminal> terminals;
        ICollection<Nonterminal> nonterminals;

        Dictionary<ISymbol, ISet<Terminal>> first = new Dictionary<ISymbol, ISet<Terminal>>();
        Dictionary<Nonterminal, ISet<Terminal>> follow = new Dictionary<Nonterminal, ISet<Terminal>>();

        // create a context free grammar object with some starting symbol and sets of terminals and nonterminals
        public CFG(Nonterminal start, ICollection<Terminal> terminals, ICollection<Nonterminal> nonterminals)
        {
            this.startSymbol = start;
            this.terminals = terminals.ToList();
            this.terminals.Add(Terminal.EOF);
            this.nonterminals = nonterminals;
        }

        // creates string representation of a production rule for debugging
        private string SymbolsToString(IEnumerable<ISymbol> production)
        {
            if (production == null) return "null";

            string s = "";
            foreach (ISymbol symbol in production)
            {
                s += symbol + " ";
            }
            return s;
        }

        public void AddProductionRule(Nonterminal v, ISymbol[] result)
        {
            if (productions.ContainsKey(v))
            {
                List<ISymbol[]> vProductions = productions[v];
                vProductions.Add(result);
            }
            else
            {
                List<ISymbol[]> vProductions = new List<ISymbol[]>();
                vProductions.Add(result);
                productions.Add(v, vProductions);
            }
        }

        public List<ISymbol[]> GetProductionRules(Nonterminal v)
        {
            List<ISymbol[]> productionRules = null;
            productions.TryGetValue(v, out productionRules);
            return productionRules;
        }

        // Builds an LL(1) parse table using the First and Follow sets for the grammar
        // returns null if CFG is not LL(1)
        public ParseTable CreateLL1ParseTable()
        {
            ComputeFirstSets();
            ComputeFollowSets();

            if (Program.debug)
                Console.WriteLine("=========================================================");

            ParseTable parseTable = new ParseTable();

            // for each nonterminal in the grammar and each production rule for the nonterminal
            foreach (Nonterminal var in productions.Keys)
            {
                foreach (ISymbol[] varProduction in productions[var])
                {
                    if (Program.debug)
                        Console.WriteLine("CFG: LL(1) table gen, var = " + var + " prod = " + SymbolsToString(varProduction));

                    ISet<Terminal> productionFirst = First(varProduction);
                    // add an entry to the parse table for each terminal in the first set of the production
                    foreach (Terminal term in productionFirst)
                    {
                        if (term != Terminal.EPSILON)
                        {
                            if (parseTable.Get(var, term) != null) // LL(1) violation
                            {
                                if (Program.debug)
                                {
                                    Console.WriteLine("CFG: LL(1) violation at (First) [" + var + "," + term + "]");
                                    Console.WriteLine("\tWas: " + SymbolsToString(parseTable.Get(var, term)));
                                    Console.WriteLine("\tNew: " + SymbolsToString(varProduction));
                                }
                                return null;
                            }
                            else
                            {
                                parseTable.Add(var, term, varProduction);
                            }
                        }
                    }
                    // if epsilon can be derived from the production
                    // add an entry to the parse table for each terminal in the follow set of the nonterminal as well
                    if (productionFirst.Contains(Terminal.EPSILON))
                    {
                        foreach (Terminal term in Follow(var))
                        {
                            if (parseTable.Get(var, term) != null) // LL(1) violation
                            {
                                if (Program.debug)
                                {
                                    Console.WriteLine("CFG: LL(1) violation at (Follow) [" + var + "," + term + "]");
                                    Console.WriteLine("\tWas: " + SymbolsToString(parseTable.Get(var, term)));
                                    Console.WriteLine("\tNew: " + SymbolsToString(varProduction));
                                }
                                return null;
                            }
                            else
                            {
                                parseTable.Add(var, term, varProduction);
                            }
                        }
                    }
                }
            }

            if (Program.debug)
                foreach (Nonterminal var in nonterminals)
                    foreach (Terminal term in terminals)
                        if (parseTable.Get(var, term) != null)
                            Console.WriteLine("CFG: LL(1) Table[" + var + "][" + term + "] = " + SymbolsToString(parseTable.Get(var, term)));

            return parseTable;
        }

        // iteratively compute the first set for each grammar nonterminal
        private void ComputeFirstSets()
        {
            if (Program.debug)
            {
                Console.WriteLine("=========================================================");
                Console.WriteLine("CFG: Computing first sets");
            }

            // initialize first sets
            foreach (Nonterminal var in nonterminals)
            {
                first[var] = new HashSet<Terminal>();
                foreach (ISymbol[] production in productions[var])
                    if (production[0] is Terminal)
                        first[var].Add(production[0] as Terminal);
                if (Program.debug)
                    Console.WriteLine("CFG: First " + var + " = " + SymbolsToString(first[var]));
            }

            if (Program.debug)
                Console.WriteLine("CFG: Initial step done");

            // loop until no changes to the first sets
            bool converged = false;
            while (!converged)
            {
                converged = true;
                if (Program.debug)
                    Console.WriteLine("--iteration--");
                // for each production or each grammar nonterminal
                foreach (Nonterminal var in nonterminals)
                {
                    foreach (ISymbol[] production in productions[var])
                    {
                        // combine first set of variable with first set of production
                        int tmp = first[var].Count;
                        first[var].UnionWith(First(production));
                        if (first[var].Count > tmp)
                        {
                            // first set was modified
                            if (Program.debug)
                                Console.WriteLine("CFG: First " + var + " = " + SymbolsToString(first[var]));
                            converged = false;
                        }
                    }
                }
            }
            if (Program.debug)
                Console.WriteLine("CFG: first sets converged");
        }

        // get the first set for a sequence of grammar symbols
        public ISet<Terminal> First(params ISymbol[] production)
        {
            ISet<Terminal> firstSet = new HashSet<Terminal>();
            ISymbol x = production[0];

            // production is empty
            if (x == Terminal.EPSILON)
                firstSet.Add(Terminal.EPSILON);
            // production starts with a terminal
            else if (x is Terminal)
                firstSet.Add(x as Terminal);
            // production consists of a single symbol
            else if (production.Length == 1)
                firstSet.UnionWith(first[x]);
            // epsilon cannot be derived from first symbol in production
            else if (!first[x].Contains(Terminal.EPSILON))
                firstSet.UnionWith(first[x]);
            // first symbol in production can disappear
            else 
            {
                firstSet.UnionWith(first[x]);
                firstSet.Remove(Terminal.EPSILON);
                // so we need to add First set of the tail of the production
                // to the first set for this symbol
                firstSet.UnionWith(First(production.Skip(1).ToArray()));
            }

            return firstSet;
        }

        // iteratively computer the follow set for each grammar nonterminal
        private void ComputeFollowSets()
        {
            if (Program.debug)
            {
                Console.WriteLine("=========================================================");
                Console.WriteLine("CFG: Computing follow sets");
            }

            // initialize follow sets for each nonterminal by going through every production
            // and finding terminals that come immediately after nonterminals in the productions
            foreach (Nonterminal A in nonterminals)
            {
                follow[A] = new HashSet<Terminal>();
                if (A == startSymbol)
                    follow[A].Add(Terminal.EOF);
                foreach (Nonterminal B in nonterminals)
                {
                    foreach (ISymbol[] prod in productions[B])
                    {
                        for (int i = 0; i < prod.Length - 1; i++)
                            if (prod[i] == A && prod[i + 1] is Terminal)
                                follow[A].Add(prod[i + 1] as Terminal);
                    }
                }
                if (Program.debug)
                    Console.WriteLine("CFG: Follow " + A + " = " + SymbolsToString(follow[A]));
            }
            if (Program.debug)
                Console.WriteLine("CFG: Initial step done");

            // loop until follow sets unchanged
            bool converged = false;
            while (!converged)
            {
                converged = true;
                if (Program.debug)
                    Console.WriteLine("--iteration--");
                // foreach nonterminal and for each of its productions
                foreach (Nonterminal B in nonterminals)
                {
                    foreach (ISymbol[] prod in productions[B])
                    {
                        // for every nonterminal in the production
                        for (int i = 0; i < prod.Length; i++)
                        {
                            if (prod[i] is Nonterminal)
                            {
                                // A is a nonterminal symbol in a production rule for B
                                Nonterminal A = prod[i] as Nonterminal;
                                int tmp = follow[A].Count;
                                // A is last symbol in a production of B, add Follow(B) to Follow(A)
                                if (i == prod.Length - 1)
                                {
                                    follow[A].UnionWith(follow[B]);
                                }
                                else
                                {
                                    ISymbol[] w = prod.Skip(i + 1).ToArray(); // rest of the production after A
                                    ISet<Terminal> fw = First(w);
                                    // if epsilon can be derived from symbols following A in the production
                                    // add Follow(B) to Follow(A)
                                    if (fw.Contains(Terminal.EPSILON))
                                    {
                                        follow[A].UnionWith(follow[B]);
                                        fw.Remove(Terminal.EPSILON);
                                    }
                                    // add first set of the symbols following A in the production to Follow(A) in any case
                                    follow[A].UnionWith(fw);
                                }
                                if (follow[A].Count > tmp)
                                {
                                    // a follow set was changed
                                    if (Program.debug)
                                    {
                                        Console.WriteLine("CFG: used rule " + B + " -> " + SymbolsToString(prod) + "for " + A);
                                        Console.WriteLine("CFG: Follow " + A + " = " + SymbolsToString(follow[A]));
                                        Console.WriteLine();
                                    }
                                    converged = false;
                                }
                            }
                        }
                    }
                }
                if (Program.debug)
                    foreach (Nonterminal A in nonterminals)
                        Console.WriteLine("CFG: Follow " + A + " = " + SymbolsToString(follow[A]));
            }
            if (Program.debug)
                Console.WriteLine("CFG: follow sets converged");
        }

        // get the follow set for a grammar nonterminal
        public ISet<Terminal> Follow(Nonterminal startVar)
        {
            return follow[startVar];
        }
    }
}
