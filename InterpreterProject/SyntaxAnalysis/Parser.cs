using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.Errors;
using InterpreterProject.LexicalAnalysis;

namespace InterpreterProject.SyntaxAnalysis
{
    public class Parser
    {
        ParseTable table;
        Nonterminal start;
        Terminal syncTerm;

        public Parser(ParseTable table, Nonterminal start, Terminal syncTerm)
        {
            this.table = table;
            this.start = start;
            this.syncTerm = syncTerm;
        }

        public ParseTree Parse(IEnumerable<Token> tokenSource)
        {
            Stack<ISymbol> symbolStack = new Stack<ISymbol>();
            symbolStack.Push(Terminal.EOF);
            symbolStack.Push(start);

            Tree root = new Tree(start);
            Stack<INode> treeStack = new Stack<INode>();
            treeStack.Push(new Leaf(Terminal.EOF));
            treeStack.Push(root);

            ParseTree parseTree = new ParseTree(root);

            IEnumerator<Token> tokenStream = tokenSource.GetEnumerator();

            tokenStream.MoveNext();

            while (symbolStack.Count > 0)
            {
                Console.WriteLine("=========================================================");
                Console.WriteLine("PARSE: Stack " + SymbolsToString(symbolStack));
                Console.WriteLine("PARSE: expecting " + symbolStack.Peek());
                Console.WriteLine("PARSE: token " + tokenStream.Current);

                if (tokenStream.Current.tokenType == TokenType.ERROR)
                {
                    Console.WriteLine("PARSE: skipping error token");
                    parseTree.errors.Add(new LexicalError(tokenStream.Current));
                    tokenStream.MoveNext();
                    continue;
                }

                if (symbolStack.Peek() is Terminal)
                {
                    Terminal term = symbolStack.Peek() as Terminal;
                    Leaf leaf = treeStack.Peek() as Leaf;

                    if (term == Terminal.EPSILON)
                    {
                        Console.WriteLine("PARSE: ignore epsilon");
                        symbolStack.Pop();
                        treeStack.Pop();
                    }
                    else if (term.Matches(tokenStream.Current))
                    {
                        leaf.token = tokenStream.Current;
                        Console.WriteLine("PARSE: Terminal match");
                        tokenStream.MoveNext();
                        symbolStack.Pop();
                        treeStack.Pop();
                    }
                    else
                    {
                        Console.WriteLine("PARSE: Terminal mismatch");
                        Console.WriteLine("PARSE: Error");
                        Console.WriteLine("PARSE: Panic mode");
                        parseTree.errors.Add(new SyntaxError(tokenStream.Current));
                        Synchronize(symbolStack, tokenStream);     
                    }
                }
                else // top of stack is a nonterminal
                {
                    Nonterminal var = symbolStack.Pop() as Nonterminal;
                    Tree subtree = treeStack.Pop() as Tree;

                    ISymbol[] production = table.Get(var, tokenStream.Current);

                    if (production == null)
                    {
                        Console.WriteLine("PARSE: No such production");
                        Console.WriteLine("PARSE: Error");
                        Console.WriteLine("PARSE: Panic mode");
                        parseTree.errors.Add(new SyntaxError(tokenStream.Current));
                        Synchronize(symbolStack, tokenStream);
                    }
                    else
                    {
                        Console.WriteLine("PARSE: Using production " + SymbolsToString(production));
                        for (int i = production.Length - 1; i >= 0; i--)
                        {
                            INode treeChild;
                            if (production[i] is Terminal)
                                treeChild = new Leaf(production[i] as Terminal);
                            else
                                treeChild = new Tree(production[i] as Nonterminal);
                            subtree.children.Insert(0, treeChild);
                            treeStack.Push(treeChild);
                            symbolStack.Push(production[i]);
                        }
                    }
                }
            }

            Console.WriteLine(root);

            return parseTree;
        }

        private void Synchronize(Stack<ISymbol> symbolStack, IEnumerator<Token> tokenStream)
        {            
            while (!syncTerm.Matches(tokenStream.Current))
            {
                Console.WriteLine("PARSE: Discarding token " + tokenStream.Current);
                if (!tokenStream.MoveNext())
                    break; // no more token for us?
            }
            Console.WriteLine(syncTerm);
            while (symbolStack.Count > 0 && symbolStack.Peek() != syncTerm)
            {
                Console.WriteLine("PARSE: Discarding symbol " + symbolStack.Peek());
                symbolStack.Pop();
            }             
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

            public List<IError> errors = new List<IError>(); 

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

            public override string ToString()
            {
                return root.ToString();
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
