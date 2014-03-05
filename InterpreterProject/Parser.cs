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

            INode root = new Tree(start);
            Stack<INode> treeStack = new Stack<INode>();
            treeStack.Push(new Leaf(CFG.Terminal.EOF));
            treeStack.Push(root);

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
                    Leaf leaf = treeStack.Pop() as Leaf;

                    if (term == CFG.Terminal.epsilon)
                    {
                        Console.WriteLine("PARSE: disregard epsilon");
                        continue;
                    }
                    if (term.Matches(ts.Current))
                    {
                        leaf.token = ts.Current;
                        Console.WriteLine("PARSE: Terminal match");
                        ts.MoveNext();                        
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("PARSE: Terminal mismatch");
                        Console.WriteLine("PARSE: Error");
                        return false;
                    }
                }
                else // top of stack is a nonterminal
                {
                    CFG.Variable var = s.Pop() as CFG.Variable;
                    Tree subtree = treeStack.Pop() as Tree;

                    CFG.ISymbol[] production = tableGet(var, ts.Current);

                    if (production == null)
                    {
                        Console.WriteLine("PARSE: No such production");
                        Console.WriteLine("PARSE: Error");
                        return false;
                    }
                    else
                    {
                        // add leaves to tree
                        Console.WriteLine("PARSE: Using production " + SymbolsToString(production));
                        for (int i = production.Length - 1; i >= 0; i--)
                        {                  
                            INode treeChild;
                            if (production[i] is CFG.Terminal)
                            {
                                treeChild = new Leaf(production[i] as CFG.Terminal);
                            } 
                            else 
                            {
                                treeChild = new Tree(production[i] as CFG.Variable);
                            }
                            subtree.children.Insert(0, treeChild);

                            treeStack.Push(treeChild); 
                            s.Push(production[i]);
                        }
                    }
                }
            }

            // TODO: debug print tree

            Stack<INode> q = new Stack<INode>();
            Stack<int> dq = new Stack<int>();
            q.Push(root);
            dq.Push(0);

            while (q.Count > 0)
            {
                INode node = q.Pop();
                int depth = dq.Pop();
                string indent = "";
                for (int i = 0; i < depth; i++) indent += "\t";
                Console.WriteLine(indent + node.ToString());
                if (node is Tree)
                {
                    Tree t = node as Tree;
                    foreach(INode next in t.children)
                    {
                        q.Push(next);
                        dq.Push(depth + 1);
                    }
                }
            }

            return true;
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


        private interface INode 
        { 
            CFG.ISymbol GetSymbol();
        }

        private class Tree : INode
        {
            public List<INode> children = new List<INode>();
            public CFG.Variable var;
            public Tree(CFG.Variable var) { this.var = var; }
            public CFG.ISymbol GetSymbol() { return var; }

            public override string ToString()
            {
                return var.ToString();
            }
        }

        private class Leaf : INode
        {
            public CFG.Terminal term;
            public Token token;
            public Leaf(CFG.Terminal term) { this.term = term; }
            public CFG.ISymbol GetSymbol() { return term; }

            public override string ToString()
            {
                return term.ToString() + (token == null ? "" : " " + token.ToString());
            }
        }
    }
}
