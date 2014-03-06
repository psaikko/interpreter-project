using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
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

        public Dictionary<Nonterminal, Dictionary<Terminal, ISymbol[]>> CreateLL1ParseTable()
        {
            ComputeFirstSets();
            ComputeFollowSets();

            Console.WriteLine("=========================================================");

            Dictionary<Nonterminal, Dictionary<Terminal, ISymbol[]>> LL1ParseTable =
                new Dictionary<Nonterminal, Dictionary<Terminal, ISymbol[]>>();

            foreach (Nonterminal var in productions.Keys)
            {
                LL1ParseTable.Add(var, new Dictionary<Terminal, ISymbol[]>());
                foreach (Terminal t in terminals) LL1ParseTable[var].Add(t, null);

                foreach (ISymbol[] varProduction in productions[var])
                {
                    //if (varProduction[0] != Terminal.epsilon)
                    {
                        Console.WriteLine("CFG: LL(1) table gen, var = " + var + " prod = " + SymbolsToString(varProduction));

                        ISet<Terminal> productionFirst = First(varProduction);

                        foreach (Terminal term in productionFirst)
                        {
                            if (term != Terminal.epsilon)
                            {
                                if (LL1ParseTable[var][term] != null) // LL(1) violation
                                {
                                    Console.WriteLine("CFG: LL(1) violation at (First) [" + var + "," + term + "]");
                                    Console.WriteLine("\tWas: " + SymbolsToString(LL1ParseTable[var][term]));
                                    Console.WriteLine("\tNew: " + SymbolsToString(varProduction));
                                    return null;
                                }
                                else
                                {
                                    LL1ParseTable[var][term] = varProduction;
                                }
                            }
                        }
                        if (productionFirst.Contains(Terminal.epsilon))
                        {
                            foreach (Terminal term in Follow(var))
                            {
                                if (LL1ParseTable[var][term] != null) // LL(1) violation
                                {
                                    Console.WriteLine("CFG: LL(1) violation at (Follow) [" + var + "," + term + "]");
                                    Console.WriteLine("\tWas: " + SymbolsToString(LL1ParseTable[var][term]));
                                    Console.WriteLine("\tNew: " + SymbolsToString(varProduction));
                                    return null;
                                }
                                else
                                {
                                    LL1ParseTable[var][term] = varProduction;
                                }
                            }
                        }
                    }                    
                }
            }

            foreach (Nonterminal var in nonterminals)
            {
                foreach (Terminal term in terminals)
                {
                    if (LL1ParseTable[var][term] != null)
                        Console.WriteLine("CFG: LL(1) Table[" + var + "][" + term + "] = " + SymbolsToString(LL1ParseTable[var][term]));
                }
            }

            return LL1ParseTable;
        }

        private void ComputeFirstSets()
        {
            Console.WriteLine("=========================================================");
            Console.WriteLine("CFG: Computing first sets");
            foreach (Nonterminal var in nonterminals)
            {
                first[var] = new HashSet<Terminal>();
                foreach (ISymbol[] production in productions[var])
                    if (production[0] is Terminal)
                        first[var].Add(production[0] as Terminal);
                Console.WriteLine("CFG: First " + var + " = " + SymbolsToString(first[var]));
            }

            Console.WriteLine("CFG: Initial step done");

            bool converged = false;
            while (!converged)
            {
                converged = true;
                Console.WriteLine("--iteration--");
                foreach (Nonterminal var in nonterminals)
                {
                    foreach (ISymbol[] production in productions[var])
                    {
                        int tmp = first[var].Count;
                        first[var].UnionWith(First(production));
                        if (first[var].Count > tmp)
                        {
                            Console.WriteLine("CFG: First " + var + " = " + SymbolsToString(first[var]));
                            converged = false;
                        } 
                    }                 
                }

            }
            Console.WriteLine("CFG: first sets converged");
        }

        public ISet<Terminal> First(params ISymbol[] production)
        {
            ISet<Terminal> firstSet = new HashSet<Terminal>();
            ISymbol x = production[0];
            if (x == Terminal.epsilon)
                firstSet.Add(Terminal.epsilon);
            else if (x is Terminal)
                firstSet.Add(x as Terminal);
            else if (production.Length == 1)
                firstSet.UnionWith(first[x]);
            else if (!first[x].Contains(Terminal.epsilon))
                firstSet.UnionWith(first[x]);
            else // epsilon in FIRST(x)
            {
                firstSet.UnionWith(first[x]);
                firstSet.Remove(Terminal.epsilon);
                firstSet.UnionWith(First(production.Skip(1).ToArray()));
            }

            return firstSet;
        }

        private void ComputeFollowSets()
        {
            Console.WriteLine("=========================================================");
            Console.WriteLine("CFG: Computing follow sets");
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
                Console.WriteLine("CFG: Follow " + A + " = " + SymbolsToString(follow[A]));
            }

            Console.WriteLine("CFG: Initial step done");

            bool converged = false;

            while (!converged)
            {
                converged = true;
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
                                    if (fw.Contains(Terminal.epsilon))
                                    {
                                        follow[A].UnionWith(follow[B]);
                                        fw.Remove(Terminal.epsilon);
                                    }
                                    follow[A].UnionWith(fw);
                                }
                                if (follow[A].Count > tmp)
                                {
                                    Console.WriteLine("CFG: used rule "+B+" -> "+SymbolsToString(prod)+"for "+A);
                                    Console.WriteLine("CFG: Follow " + A + " = " + SymbolsToString(follow[A]));
                                    Console.WriteLine();
                                    converged = false;
                                }
                            }
                        }
                    }  
                }
                foreach (Nonterminal A in nonterminals)
                    Console.WriteLine("CFG: Follow " + A + " = " + SymbolsToString(follow[A]));
            }

            Console.WriteLine("CFG: follow sets converged");
        }

        public ISet<Terminal> Follow(Nonterminal startVar)
        {
            return follow[startVar];
        }

    }
}
