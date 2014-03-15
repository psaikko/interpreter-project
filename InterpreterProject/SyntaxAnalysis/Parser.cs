using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.Errors;
using InterpreterProject.LexicalAnalysis;

namespace InterpreterProject.SyntaxAnalysis
{
    /*
     * Implements a predictive parser for any given LL(1) grammar.
     * Uses phrase-level error recovery which is overridden if a special
     * synchronising token (like ;) is encountered.
     */
    public class Parser
    {
        ParseTable table;
        Nonterminal start;
        Terminal syncTerm;
        CFG grammar;

        List<IError> errors;

        public List<IError> Errors
        {
            get { return errors; }
        }

        public Parser(CFG grammar, Terminal syncTerm)
        {
            this.grammar = grammar;
            this.table = grammar.CreateLL1ParseTable();
            this.start = grammar.StartSymbol;
            this.syncTerm = syncTerm;

            if (table == null)
                throw new Exception("GRAMMAR NOT LL(1)");
        }

        // Parse the given stream of tokens
        public ParseTree Parse(IEnumerable<Token> tokenSource)
        {
            errors = new List<IError>();

            Stack<ISymbol> symbolStack = new Stack<ISymbol>();
            symbolStack.Push(Terminal.EOF);
            symbolStack.Push(start);

            ParseTree parseTree = new ParseTree(start);
            Stack<IParseNode> treeStack = new Stack<IParseNode>();

            treeStack.Push(new ParseLeaf(Terminal.EOF));
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

                // ignore error tokens
                if (tokenStream.Current.Type == TokenType.ERROR)
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
                    ParseLeaf leaf = treeStack.Peek() as ParseLeaf;

                    if (term == Terminal.EPSILON)
                    {
                        // epsilon production was used, exclude from parse tree
                        if (Program.debug)
                            Console.WriteLine("  PARSE: ignore epsilon");
                        symbolStack.Pop();
                        treeStack.Pop();
                    }
                    else if (term.Matches(tokenStream.Current))
                    {
                        // current token matches the top of the parse stack, add it to parse tree                        
                        if (Program.debug)
                            Console.WriteLine("  PARSE: Terminal match");
                        leaf.Token = tokenStream.Current;
                        tokenStream.MoveNext();
                        symbolStack.Pop();
                        treeStack.Pop();
                    }
                    else
                    {
                        // current token does no match, recover from error
                        if (Program.debug)
                            Console.WriteLine("  PARSE: Error, Terminal mismatch");
                        errors.Add(new SyntaxError(tokenStream.Current));
                        Synchronize(symbolStack, treeStack, tokenStream);
                    }
                }
                else // top of stack is a nonterminal
                {
                    Nonterminal var = symbolStack.Pop() as Nonterminal;
                    IParseNode popped = treeStack.Pop();
                    ParseTree subtree = popped as ParseTree;

                    ISymbol[] production = table.Get(var, tokenStream.Current);

                    if (production == null)
                    {
                        // cannot derive the current token from the nonterminal at the top of the stack
                        if (Program.debug)
                            Console.WriteLine("  PARSE: Error, No such production");

                        symbolStack.Push(var);
                        treeStack.Push(popped);
                                                                                          
                        errors.Add(new SyntaxError(tokenStream.Current));
                        Synchronize(symbolStack, treeStack, tokenStream);
                    }
                    else
                    {
                        // use the production specified by the parse table, add node to parse tree
                        if (Program.debug)
                            Console.WriteLine("  PARSE: Using production " + SymbolsToString(production));

                        for (int i = production.Length - 1; i >= 0; i--)
                        {
                            IParseNode treeChild;
                            if (production[i] is Terminal)
                                treeChild = new ParseLeaf(production[i] as Terminal);
                            else
                                treeChild = new ParseTree(production[i] as Nonterminal);
                            subtree.Children.Insert(0, treeChild);
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

        // Uses phrase-level error recovery with First and Follow sets to recover from bad state
        private void Synchronize(Stack<ISymbol> symbolStack, Stack<IParseNode> treeStack, IEnumerator<Token> tokenStream)
        {
            // Loop until good state found (return statements) or no more symbols on stack.
        sync: while (symbolStack.Count > 0)
            {
                if (Program.debug)
                    Console.WriteLine("  PARSE: Synchronize token " + tokenStream.Current + " symbol " + symbolStack.Peek());

                // no recovery from end of file, empty the parse stack
                if (tokenStream.Current.Type == TokenType.EOF)
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
                        // good state reached
                        if (Program.debug)
                            Console.WriteLine("  PARSE: Token matches");
                        return;
                    }

                    if (syncTerm.Matches(tokenStream.Current))
                    {
                        // special case: we have a synchronising token like ';' that we do not want to skip over
                        // so remove symbol from parse stack
                        if (Program.debug)
                            Console.WriteLine("  PARSE: Discarding symbol " + symbolStack.Peek());
                        symbolStack.Pop();
                        treeStack.Pop();
                        continue;
                    }

                    // normal case: discard token and continue with recovery
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
                        // good state reached (current token in First set of symbol at top of stack)
                        if (Program.debug)
                            Console.WriteLine("  PARSE: Valid production exists");
                        return;
                    }

                    // if current terminal could be matched by something in Follow set of 
                    // symbol at top of stack, then skip the current symbol
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

                    // default action: skip current terminal
                    if (Program.debug)
                        Console.WriteLine("  PARSE: Discard token");
                    if (!tokenStream.MoveNext())
                        throw new Exception("OUT OF TOKENS");
                }
            }
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
    }
}
