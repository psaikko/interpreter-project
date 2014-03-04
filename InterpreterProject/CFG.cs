using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class CFG
    {
        Dictionary<Variable, List<ISymbol[]>> productions = new Dictionary<Variable, List<ISymbol[]>>();
        Variable startSymbol;
        ICollection<Terminal> terminals;
        ICollection<Variable> nonterminals;

        Dictionary<ISymbol, ISet<Terminal>> first = new Dictionary<ISymbol, ISet<Terminal>>();
        Dictionary<Variable, ISet<Terminal>> follow = new Dictionary<Variable, ISet<Terminal>>();
 
        public CFG(Variable start, ICollection<Terminal> terminals, ICollection<Variable> nonterminals)
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

        public void AddProductionRule(Variable v, ISymbol[] result)
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

        public List<ISymbol[]> GetProductionRules(Variable v)
        {
            List<ISymbol[]> productionRules= null;
            productions.TryGetValue(v, out productionRules);
            return productionRules;
        }

        public Dictionary<Variable, Dictionary<Terminal, ISymbol[]>> CreateLL1ParseTable()
        {
            ComputeFirstSets();
            ComputeFollowSets();

            Console.WriteLine("=========================================================");

            Dictionary<Variable, Dictionary<Terminal, ISymbol[]>> LL1ParseTable =
                new Dictionary<Variable, Dictionary<Terminal, ISymbol[]>>();

            foreach (Variable var in productions.Keys)
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

            foreach (Variable var in nonterminals)
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
            foreach (Variable var in nonterminals)
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
                foreach (Variable var in nonterminals)
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
            foreach (Variable A in nonterminals)
            {
                follow[A] = new HashSet<Terminal>();
                if (A == startSymbol)
                    follow[A].Add(Terminal.EOF);
                foreach (Variable B in nonterminals)
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
                foreach (Variable B in nonterminals)
                {
                    foreach (ISymbol[] prod in productions[B])
                    {
                        for (int i = 0; i < prod.Length; i++)
                        {
                            if (prod[i] is Variable)
                            {
                                Variable A = prod[i] as Variable;
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
                foreach (Variable A in nonterminals)
                    Console.WriteLine("CFG: Follow " + A + " = " + SymbolsToString(follow[A]));
            }

            Console.WriteLine("CFG: follow sets converged");
        }

        public ISet<Terminal> Follow(Variable startVar)
        {
            return follow[startVar];
        }

        public interface ISymbol { }

        public class Terminal : ISymbol
        {
            public static readonly Terminal epsilon = new Terminal("");
            public static readonly Terminal EOF = new Terminal(TokenType.EOF); 

            public readonly TokenType tokenType = null;
            public readonly string lexeme = null;

            public Terminal(TokenType tokenType)
            {
                this.tokenType = tokenType;
            }

            public Terminal(string lexeme)
            {
                this.lexeme = lexeme;
            }

            public bool Matches(Token t)
            {
                if (tokenType != null)
                    return t.tokenType == tokenType;
                else
                    return t.lexeme == lexeme;
            }

            public override string ToString()
            {
                if (tokenType != null)
                    return "<" + tokenType.name + ">";
                else
                    return "\"" + lexeme + "\"";
            }
        }

        public class Variable : ISymbol
        {
            public readonly string name;

            public Variable(string name)
            {
                this.name = name;
            }

            public override string ToString()
            {
                return name;
            }
        }
    }
}
