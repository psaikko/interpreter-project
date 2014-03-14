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
        CFG grammar;
        List<IError> errors = new List<IError>();

        public Parser(CFG grammar, Terminal syncTerm)
        {
            this.grammar = grammar;
            this.table = grammar.CreateLL1ParseTable();
            this.start = grammar.StartSymbol;
            this.syncTerm = syncTerm;

            if (table == null)
                throw new Exception("GRAMMAR NOT LL(1)");
        }

        public Tree<IParseValue> Parse(IEnumerable<Token> tokenSource)
        {
            Stack<ISymbol> symbolStack = new Stack<ISymbol>();
            symbolStack.Push(Terminal.EOF);
            symbolStack.Push(start);

            Tree<IParseValue> parseTree = new Tree<IParseValue>(new NonterminalValue(start));
            Stack<INode<IParseValue>> treeStack = new Stack<INode<IParseValue>>();

            treeStack.Push(new Leaf<IParseValue>(new TerminalValue(Terminal.EOF)));
            treeStack.Push(parseTree);

            IEnumerator<Token> tokenStream = tokenSource.GetEnumerator();

            tokenStream.MoveNext();

            while (symbolStack.Count > 0)
            {
                if (Program.debug)
                {
                    Console.WriteLine("=========================================================");
                    Console.WriteLine("  PARSE: Stack " + SymbolsToString(symbolStack));
                    Console.WriteLine("  PARSE: expecting " + symbolStack.Peek());
                    Console.WriteLine("  PARSE: token " + tokenStream.Current);
                }

                if (tokenStream.Current.tokenType == TokenType.ERROR)
                {
                    if (Program.debug) 
                        Console.WriteLine("  PARSE: skipping error token");
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
                        if (Program.debug) 
                            Console.WriteLine("  PARSE: ignore epsilon");
                        symbolStack.Pop();
                        treeStack.Pop();
                    }
                    else if (term.Matches(tokenStream.Current))
                    {
                        (leaf.GetValue() as TerminalValue).token = tokenStream.Current;
                        if (Program.debug) 
                            Console.WriteLine("  PARSE: Terminal match");
                        tokenStream.MoveNext();
                        symbolStack.Pop();
                        treeStack.Pop();
                    }
                    else
                    {
                        if (Program.debug)
                            Console.WriteLine("  PARSE: Error, Terminal mismatch");
                       
                        errors.Add(new SyntaxError(tokenStream.Current));
                        Synchronize(symbolStack, treeStack, tokenStream);     
                    }
                }
                else // top of stack is a nonterminal
                {
                    Nonterminal var = symbolStack.Pop() as Nonterminal;
                    INode<IParseValue> popped = treeStack.Pop();
                    Tree<IParseValue> subtree = popped as Tree<IParseValue>;

                    ISymbol[] production = table.Get(var, tokenStream.Current);

                    if (production == null)
                    {
                        symbolStack.Push(var);
                        treeStack.Push(popped);

                        if (Program.debug)
                            Console.WriteLine("  PARSE: Error, No such production");

                        errors.Add(new SyntaxError(tokenStream.Current));
                        Synchronize(symbolStack, treeStack, tokenStream);
                    }
                    else
                    {
                        if (Program.debug) 
                            Console.WriteLine("  PARSE: Using production " + SymbolsToString(production));
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

            if (Program.debug) 
                Console.WriteLine(parseTree);

            return parseTree;
        }

        private void Synchronize(Stack<ISymbol> symbolStack, Stack<INode<IParseValue>> treeStack, IEnumerator<Token> tokenStream)
        {
            sync: while (symbolStack.Count > 0)
            {
                if (Program.debug)
                    Console.WriteLine("  PARSE: Synchronize token " + tokenStream.Current + " symbol " + symbolStack.Peek());

                if (tokenStream.Current.tokenType == TokenType.EOF)
                {
                    while (symbolStack.Count > 0)
                    {
                        symbolStack.Pop();
                        treeStack.Pop();
                    }                        
                    if (Program.debug)
                        Console.WriteLine("PARSE: Unexpected EOF");
                    return;
                }

                if (symbolStack.Peek() is Terminal)
                {                   
                    Terminal t = symbolStack.Peek() as Terminal;
                    if (t.Matches(tokenStream.Current))
                    {
                        if (Program.debug)
                            Console.WriteLine("  PARSE: Token matches");
                        return;
                    }

                    if (syncTerm.Matches(tokenStream.Current))
                    {
                        if (Program.debug)
                            Console.WriteLine("  PARSE: Discarding symbol " + symbolStack.Peek());
                        symbolStack.Pop();
                        treeStack.Pop();
                        continue;
                    }

                    if (Program.debug)
                        Console.WriteLine("  PARSE: Discard token");
                    if (!tokenStream.MoveNext())
                        throw new Exception("OUT OF TOKENS");
                    continue;
                }
                else
                {
                    Nonterminal v = symbolStack.Peek() as Nonterminal;
                    if (table.Get(v, tokenStream.Current) != null)
                    {
                        if (Program.debug)
                            Console.WriteLine("  PARSE: Valid production exists");
                        return;
                    }
                        
                    foreach (ISymbol sym in grammar.Follow(v))
                    {
                        if (sym is Terminal)
                        {
                            Terminal followTerm = sym as Terminal;
                            if (followTerm.Matches(tokenStream.Current))
                            {
                                if (Program.debug)
                                    Console.WriteLine("  PARSE: Discarding symbol " + symbolStack.Peek());
                                symbolStack.Pop();
                                treeStack.Pop();
                                goto sync;
                            }
                        }
                        if (sym is Nonterminal)
                        {
                            Nonterminal followVar = sym as Nonterminal;
                            if (table.Get(followVar, tokenStream.Current) != null)
                            {
                                if (Program.debug)
                                    Console.WriteLine("  PARSE: Discarding symbol " + symbolStack.Peek());
                                symbolStack.Pop();
                                treeStack.Pop();
                                goto sync;
                            }
                        }
                    }

                    if (Program.debug)
                        Console.WriteLine("  PARSE: Discard token");
                    if (!tokenStream.MoveNext())
                        throw new Exception("OUT OF TOKENS");
                }
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
