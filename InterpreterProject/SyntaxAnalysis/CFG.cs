using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.SyntaxAnalysis
{
    public class CFG
    {
        Dictionary<Nonterminal, List<ISymbol[]>> productions = new Dictionary<Nonterminal, List<ISymbol[]>>();
        Nonterminal startSymbol;
        ICollection<Terminal> terminals;
        ICollection<Nonterminal> nonterminals;

        Dictionary<ISymbol, ISet<Terminal>> first = new Dictionary<ISymbol, ISet<Terminal>>();
        Dictionary<Nonterminal, ISet<Terminal>> follow = new Dictionary<Nonterminal, ISet<Terminal>>();
 
        public CFG(Nonterminal start, ICollection<Terminal> terminals, ICollection<Nonterminal> nonterminals)
        {
            this.startSymbol = start;
            this.terminals = terminals.ToList();
            this.terminals.Add(Terminal.EOF);
            this.nonterminals = nonterminals;
        }

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
            List<ISymbol[]> productionRules= null;
            productions.TryGetValue(v, out productionRules);
            return productionRules;
        }

        public ParseTable CreateLL1ParseTable()
        {
            ComputeFirstSets();
            ComputeFollowSets();

            if (Program.debug) 
                Console.WriteLine("=========================================================");

            ParseTable parseTable = new ParseTable();

            foreach (Nonterminal var in productions.Keys)
            {
                foreach (ISymbol[] varProduction in productions[var])
                {
                    //if (varProduction[0] != Terminal.epsilon)
                    {
                        if (Program.debug) 
                            Console.WriteLine("CFG: LL(1) table gen, var = " + var + " prod = " + SymbolsToString(varProduction));

                        ISet<Terminal> productionFirst = First(varProduction);

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
            }

            if (Program.debug)
            {
                foreach (Nonterminal var in nonterminals)
                {
                    foreach (Terminal term in terminals)
                    {
                        if (parseTable.Get(var, term) != null)
                            Console.WriteLine("CFG: LL(1) Table[" + var + "][" + term + "] = " + SymbolsToString(parseTable.Get(var, term)));
                    }
                }
            }
            
            return parseTable;
        }

        private void ComputeFirstSets()
        {
            if (Program.debug)
            {
                Console.WriteLine("=========================================================");
                Console.WriteLine("CFG: Computing first sets");
            }
            
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

            bool converged = false;
            while (!converged)
            {
                converged = true;
                if (Program.debug) 
                    Console.WriteLine("--iteration--");
                foreach (Nonterminal var in nonterminals)
                {
                    foreach (ISymbol[] production in productions[var])
                    {
                        int tmp = first[var].Count;
                        first[var].UnionWith(First(production));
                        if (first[var].Count > tmp)
                        {
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

        public ISet<Terminal> First(params ISymbol[] production)
        {
            ISet<Terminal> firstSet = new HashSet<Terminal>();
            ISymbol x = production[0];
            if (x == Terminal.EPSILON)
                firstSet.Add(Terminal.EPSILON);
            else if (x is Terminal)
                firstSet.Add(x as Terminal);
            else if (production.Length == 1)
                firstSet.UnionWith(first[x]);
            else if (!first[x].Contains(Terminal.EPSILON))
                firstSet.UnionWith(first[x]);
            else // epsilon in FIRST(x)
            {
                firstSet.UnionWith(first[x]);
                firstSet.Remove(Terminal.EPSILON);
                firstSet.UnionWith(First(production.Skip(1).ToArray()));
            }

            return firstSet;
        }

        private void ComputeFollowSets()
        {
            if (Program.debug)
            {
                Console.WriteLine("=========================================================");
                Console.WriteLine("CFG: Computing follow sets");
            }
            
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

            bool converged = false;

            while (!converged)
            {
                converged = true;
                if (Program.debug) 
                    Console.WriteLine("--iteration--");
                foreach (Nonterminal B in nonterminals)
                {
                    foreach (ISymbol[] prod in productions[B])
                    {
                        for (int i = 0; i < prod.Length; i++)
                        {
                            if (prod[i] is Nonterminal)
                            {
                                Nonterminal A = prod[i] as Nonterminal;
                                int tmp = follow[A].Count;
                                if (i == prod.Length - 1)
                                {                          
                                    follow[A].UnionWith(follow[B]);   
                                }
                                else
                                {
                                    ISymbol[] w = prod.Skip(i+1).ToArray();
                                    ISet<Terminal> fw = First(w);
                                    if (fw.Contains(Terminal.EPSILON))
                                    {
                                        follow[A].UnionWith(follow[B]);
                                        fw.Remove(Terminal.EPSILON);
                                    }
                                    follow[A].UnionWith(fw);
                                }
                                if (follow[A].Count > tmp)
                                {
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

        public ISet<Terminal> Follow(Nonterminal startVar)
        {
            return follow[startVar];
        }
    }
}
