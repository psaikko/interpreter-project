using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.LexicalAnalysis;

namespace InterpreterProject.SyntaxAnalysis
{
    public class Parser
    {
        ParseTable table;
        Nonterminal start;

        public Parser(ParseTable table, Nonterminal start)
        {
            this.table = table;
            this.start = start;
        }

        public ParseTree Parse(IEnumerable<Token> tokens)
        {
            Stack<ISymbol> s = new Stack<ISymbol>();
            s.Push(Terminal.EOF);
            s.Push(start);

            Tree root = new Tree(start);
            Stack<INode> treeStack = new Stack<INode>();
            treeStack.Push(new Leaf(Terminal.EOF));
            treeStack.Push(root);

            IEnumerator<Token> ts = tokens.GetEnumerator();

            ts.MoveNext();

            while (s.Count > 0)
            {
                Console.WriteLine("=========================================================");
                Console.WriteLine("PARSE: Stack " + SymbolsToString(s));
                Console.WriteLine("PARSE: expecting " + s.Peek());
                Console.WriteLine("PARSE: token " + ts.Current);

                if (s.Peek() is Terminal)
                {
                    Terminal term = s.Pop() as Terminal;
                    Leaf leaf = treeStack.Pop() as Leaf;

                    if (term == Terminal.EPSILON)
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
                        // TODO: add error
                        Console.WriteLine("PARSE: Terminal mismatch");
                        Console.WriteLine("PARSE: Error");
                        return null;
                    }
                }
                else // top of stack is a nonterminal
                {
                    Nonterminal var = s.Pop() as Nonterminal;
                    Tree subtree = treeStack.Pop() as Tree;

                    ISymbol[] production = table.Get(var, ts.Current);

                    if (production == null)
                    {
                        // TODO: add error
                        Console.WriteLine("PARSE: No such production");
                        Console.WriteLine("PARSE: Error");
                        return null;
                    }
                    else
                    {
                        Console.WriteLine("PARSE: Using production " + SymbolsToString(production));
                        for (int i = production.Length - 1; i >= 0; i--)
                        {
                            INode treeChild;
                            if (production[i] is Terminal)
                            {
                                treeChild = new Leaf(production[i] as Terminal);
                            }
                            else
                            {
                                treeChild = new Tree(production[i] as Nonterminal);
                            }
                            subtree.children.Insert(0, treeChild);
                            treeStack.Push(treeChild);
                            s.Push(production[i]);
                        }
                    }
                }
            }

            Console.WriteLine(root);

            return new ParseTree(root);
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


        public interface INode 
        { 
            ISymbol GetSymbol();
        }

        public class ParseTree
        {
            public Tree root;

            public ParseTree(Tree root)
            {
                this.root = root;
            }

            public int SymbolCount(Predicate<ISymbol> pred)
            {
                int count = 0;
                Stack<INode> nodeStack = new Stack<INode>();
                nodeStack.Push(root);
                while (nodeStack.Count > 0)
                {
                    INode current = nodeStack.Pop();
                    if (pred(current.GetSymbol()))
                        count++;
                    if (current is Tree)
                    {
                        Tree subtree = current as Tree;
                        foreach (INode child in subtree.children)
                            nodeStack.Push(child);
                    }
                }
                return count;
            }

            public bool DepthContains(int depth, Predicate<ISymbol> pred)
            {
                Stack<INode> nodeStack = new Stack<INode>();
                Stack<int> depthStack = new Stack<int>();
                nodeStack.Push(root);
                depthStack.Push(0);
                while (nodeStack.Count > 0)
                {
                    INode currentNode = nodeStack.Pop();
                    int currentDepth = depthStack.Pop();
                    if (currentDepth < depth)
                    {
                        if (currentNode is Tree)
                        {
                            Tree subtree = currentNode as Tree;
                            foreach (INode child in subtree.children)
                            {
                                nodeStack.Push(child);
                                depthStack.Push(currentDepth + 1);
                            }
                        }
                    }
                    else if (currentDepth == depth)
                        if (pred(currentNode.GetSymbol()))
                            return true;
                }
                return false;
            }
        }

        public class Tree : INode
        {
            public List<INode> children = new List<INode>();
            public Nonterminal var;
            public Tree(Nonterminal var) { this.var = var; }
            public ISymbol GetSymbol() { return var; }

            public override string ToString()
            {
                String s = "";

                Stack<INode> nodeStack = new Stack<INode>();
                Stack<int> depthStack = new Stack<int>();
                nodeStack.Push(this);
                depthStack.Push(0);

                while (nodeStack.Count > 0)
                {
                    INode node = nodeStack.Pop();
                    int depth = depthStack.Pop();
                    string indent = "";
                    for (int i = 0; i < depth; i++) indent += "  ";
                    if (node is Leaf)
                    {
                        s += indent + node.ToString() + '\n';
                    }
                    else
                    {
                        Tree t = node as Tree;
                        s += indent + t.var + '\n';
                        for (int i = t.children.Count - 1; i >= 0; i--)
                        {
                            INode next = t.children[i];
                            nodeStack.Push(next);
                            depthStack.Push(depth + 1);
                        }
                    }
                }

                return s;
            }
        }

        public class Leaf : INode
        {
            public Terminal term;
            public Token token;
            public Leaf(Terminal term) { this.term = term; }
            public ISymbol GetSymbol() { return term; }

            public override string ToString()
            {
                return (token == null ? term.ToString() : token.ToString());
            }
        }
    }
}
