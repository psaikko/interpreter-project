﻿using System;
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
            this.terminals = terminals;
            this.nonterminals = nonterminals;
        }

        private string SymbolsToString(IEnumerable<ISymbol> production)
        {
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

            Dictionary<Variable, Dictionary<Terminal, ISymbol[]>> LL1ParseTable =
                new Dictionary<Variable, Dictionary<Terminal, ISymbol[]>>();

            foreach (Variable var in productions.Keys)
            {
                LL1ParseTable.Add(var, new Dictionary<Terminal, ISymbol[]>());
                foreach (Terminal t in terminals) LL1ParseTable[var].Add(t, null);

                foreach (ISymbol[] varProduction in productions[var])
                {
                    if (varProduction[0] != Terminal.epsilon)
                    {
                        Console.WriteLine("\nCFG: LL(1) table gen, var = " + var + " prod = " + SymbolsToString(varProduction));

                        ISet<Terminal> productionFirst = First(varProduction);

                        foreach (Terminal term in productionFirst)
                        {
                            if (term != Terminal.epsilon)
                            {
                                if (LL1ParseTable[var][term] != null) // LL(1) violation
                                {
                                    Console.WriteLine("First");
                                    Console.WriteLine("CFG: LL(1) violation at [" + var + "," + term + "]");
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
                                    Console.WriteLine("Follow");
                                    Console.WriteLine("CFG: LL(1) violation at [" + var + "," + term + "]");
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
            return LL1ParseTable;
        }

        private void ComputeFirstSets()
        {
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
                foreach (Variable var in nonterminals)
                {
                    foreach (ISymbol[] production in productions[var])
                    {
                        int before = first[var].Count;

                        first[var].UnionWith(First(production));

                        int after = first[var].Count;

                        if (after > before) converged = false;
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

            Console.WriteLine("CFG: First " + SymbolsToString(production) + "\n\t= " + SymbolsToString(firstSet));

            return firstSet;
        }

        private void ComputeFollowSets()
        {

        }

        public ISet<Terminal> Follow(Variable startVar)
        {
            ISet<Variable> visited = new HashSet<Variable>();
            Stack<Variable> followStack = new Stack<Variable>();
            ISet<Terminal> followSet = new HashSet<Terminal>();

            followStack.Push(startVar);
            visited.Add(startVar);

            while (followStack.Count > 0)
            {
                Variable v = followStack.Pop();
              
                ISet<Terminal> vFollowSet = new HashSet<Terminal>();
                if (v == startSymbol)
                {
                    vFollowSet.Add(Terminal.EOF);                    
                }
                else
                {
                    foreach (Variable var in productions.Keys)
                    {
                        foreach (ISymbol[] varProduction in productions[var])
                        {
                            for (int i = 0; i < varProduction.Length; i++)
                            {
                                if (varProduction[i] == v)
                                {
                                    if (i == varProduction.Length - 1)
                                    {
                                        if (!visited.Contains(var))
                                        {
                                            followStack.Push(var);
                                            visited.Add(var);
                                        }
                                    }
                                    else
                                    {
                                        vFollowSet.UnionWith(First(varProduction.Skip(i).ToArray())); 
                                    }
                                }
                            }
                        }
                    }
                }

                followSet.UnionWith(vFollowSet);
            }
            
            followSet.Remove(Terminal.epsilon);

            //Console.WriteLine("CFG: Follow " + startVar.name + " = " + SymbolsToString(followSet));

            return followSet;
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
