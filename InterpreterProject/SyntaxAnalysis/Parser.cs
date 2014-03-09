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
        List<IError> errors = new List<IError>();

        public Parser(ParseTable table, Nonterminal start, Terminal syncTerm)
        {
            this.table = table;
            this.start = start;
            this.syncTerm = syncTerm;
        }

        public Tree<IParseValue> Parse(IEnumerable<Token> tokenSource)
        {
            Stack<ISymbol> symbolStack = new Stack<ISymbol>();
            symbolStack.Push(Terminal.EOF);
            symbolStack.Push(start);

            Tree<IParseValue> parseTree = new Tree<IParseValue>(new NonterminalValue(start));
            Stack<INode<IParseValue>> treeStack = new Stack<INode<IParseValue>>();

            //INode<ISymbol> tmp = new TerminalNode(null, Terminal.EOF);

            treeStack.Push(new Leaf<IParseValue>(new TerminalValue(Terminal.EOF)));
            treeStack.Push(parseTree);

            //Tree<ISymbol> parseTree = new Tree<ISymbol>(root);

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
                    errors.Add(new LexicalError(tokenStream.Current));
                    tokenStream.MoveNext();
                    continue;
                }

                if (symbolStack.Peek() is Terminal)
                {
                    Terminal term = symbolStack.Peek() as Terminal;
                    Leaf<IParseValue> leaf = treeStack.Peek() as Leaf<IParseValue>;

                    if (term == Terminal.EPSILON)
                    {
                        Console.WriteLine("PARSE: ignore epsilon");
                        symbolStack.Pop();
                        treeStack.Pop();
                    }
                    else if (term.Matches(tokenStream.Current))
                    {
                        (leaf.GetValue() as TerminalValue).token = tokenStream.Current;
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
                        errors.Add(new SyntaxError(tokenStream.Current));
                        Synchronize(symbolStack, tokenStream);     
                    }
                }
                else // top of stack is a nonterminal
                {
                    Nonterminal var = symbolStack.Pop() as Nonterminal;
                    Tree<IParseValue> subtree = treeStack.Pop() as Tree<IParseValue>;

                    ISymbol[] production = table.Get(var, tokenStream.Current);

                    if (production == null)
                    {
                        Console.WriteLine("PARSE: No such production");
                        Console.WriteLine("PARSE: Error");
                        Console.WriteLine("PARSE: Panic mode");
                        errors.Add(new SyntaxError(tokenStream.Current));
                        Synchronize(symbolStack, tokenStream);
                    }
                    else
                    {
                        Console.WriteLine("PARSE: Using production " + SymbolsToString(production));
                        for (int i = production.Length - 1; i >= 0; i--)
                        {
                            INode<IParseValue> treeChild;
                            if (production[i] is Terminal)
                                treeChild = new Leaf<IParseValue>(new TerminalValue(production[i] as Terminal));
                            else
                                treeChild = new Tree<IParseValue>(new NonterminalValue(production[i] as Nonterminal));
                            subtree.children.Insert(0, treeChild);
                            treeStack.Push(treeChild);
                            symbolStack.Push(production[i]);
                        }
                    }
                }
            }

            Console.WriteLine(parseTree);

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

        public List<IError> GetErrors()
        {
            List<IError> tmp = errors;
            errors = new List<IError>();
            return tmp;
        }

        public interface IParseValue 
        {
            ISymbol GetSymbol();
        }

        public class TerminalValue : IParseValue
        {
            public Token token;
            public Terminal term;

            public TerminalValue(Terminal term)
            {
                this.term = term;
            }

            public override string ToString()
            {
                return (token == null) ? term.ToString() : token.ToString();
            }

            public ISymbol GetSymbol()
            {
                return term;
            }
        }

        public class NonterminalValue : IParseValue
        {
            public Nonterminal var;

            public NonterminalValue(Nonterminal var)
            {
                this.var = var;
            }

            public override string ToString()
            {
                return var.ToString();
            }

            public ISymbol GetSymbol()
            {
                return var;
            }
        }
    }
}
