using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;
using InterpreterProject.Errors;

namespace InterpreterProject.Languages
{
    // Contains Mini-PL language definition and semantic checks
    public class MiniPL
    {
        static MiniPL instance;

        // singleton class, no sense making many instaces of the language
        public static MiniPL GetInstance()
        {
            if (instance == null)
                instance = new MiniPL();
            return instance;
        }

        Dictionary<String, TokenType> tokenTypes = new Dictionary<string, TokenType>();
        public Dictionary<String, TokenType> TokenTypes
        {
            get { return tokenTypes; }
        }

        Dictionary<String, Terminal> terminals = new Dictionary<string, Terminal>();
        public Dictionary<String, Terminal> Terminals
        {
            get { return terminals; }
        }

        Dictionary<String, Nonterminal> nonterminals = new Dictionary<string, Nonterminal>();
        public Dictionary<String, Nonterminal> Nonterminals
        {
            get { return nonterminals; }
        }

        CFG grammar;
        public CFG Grammar
        {
            get { return grammar; }
        }

        Parser parser;
        public Parser Parser
        {
            get { return parser; }
        }


        Scanner scanner;
        public Scanner Scanner
        {
            get { return scanner; }
        }

        private MiniPL()
        {
            // Create NFA-type things for tokens using regular operations
            Regex reBlockCommentStart = Regex.Concat("/*");
            Regex reBlockCommentEnd = Regex.Concat("*/");

            Regex reLineComment = Regex.Concat("//").Concat(Regex.Not('\n').Star());

            Regex reWhitespace = Regex.Union(" \t\r\n").Star().Union(reLineComment);

            Regex reString = Regex.Char('"').Concat(Regex.Char('\\').Concat(Regex.Any()).Union(Regex.Not('"', '\\')).Star()).Concat(Regex.Char('"'));
            Regex reBinaryOperator = Regex.Union("+-*/<=&");
            Regex reUnaryOperator = Regex.Char('!');
            Regex reKeyword = Regex.Union(Regex.Concat("var"), Regex.Concat("for"), Regex.Concat("end"), Regex.Concat("in"),
                                          Regex.Concat("do"), Regex.Concat("read"), Regex.Concat("print"), Regex.Concat("assert"));
            Regex reType = Regex.Union(Regex.Concat("bool"), Regex.Concat("int"), Regex.Concat("string"));

            Regex reParenRight = Regex.Char(')'),
                  reParenLeft = Regex.Char('('),
                  reColon = Regex.Char(':'),
                  reSemicolon = Regex.Char(';'),
                  reAssignment = Regex.Concat(":="),
                  reDots = Regex.Concat("..");

            Regex reIdentifier = Regex.Union(Regex.Range('A', 'Z'), Regex.Range('a', 'z'))
                                      .Concat(Regex.Union(Regex.Range('A', 'Z'), Regex.Range('a', 'z'), Regex.Range('0', '9'), Regex.Char('_')).Star());
            Regex reInteger = Regex.Range('0', '9').Plus();

            // Define token types
            tokenTypes["block_comment_start"] = new TokenType("block_comment_start", reBlockCommentStart);
            tokenTypes["block_comment_end"] = new TokenType("block_comment_end", reBlockCommentEnd);
            tokenTypes["int"] = new TokenType("int", reInteger);
            tokenTypes["whitespace"] = new TokenType("whitespace", reWhitespace, priority: TokenType.Priority.Whitespace);
            tokenTypes["string"] = new TokenType("string", reString);
            tokenTypes["binary_op"] = new TokenType("binary op", reBinaryOperator);
            tokenTypes["unary_op"] = new TokenType("unary op", reUnaryOperator);
            tokenTypes["keyword"] = new TokenType("keyword", reKeyword, priority: TokenType.Priority.Keyword);
            tokenTypes["type"] = new TokenType("type", reType, priority: TokenType.Priority.Keyword);
            tokenTypes["left_paren"] = new TokenType("left paren", reParenLeft);
            tokenTypes["right_paren"] = new TokenType("right paren", reParenRight);
            tokenTypes["colon"] = new TokenType("colon", reColon);
            tokenTypes["semicolon"] = new TokenType("semicolon", reSemicolon);
            tokenTypes["assignment"] = new TokenType("assignment", reAssignment);
            tokenTypes["dots"] = new TokenType("dots", reDots);
            tokenTypes["identifier"] = new TokenType("identifier", reIdentifier);

            // create combined automaton and scanner object
            TokenAutomaton automaton = TokenType.CombinedAutomaton(tokenTypes.Values.ToArray());
            scanner = new Scanner(automaton, tokenTypes["block_comment_start"], tokenTypes["block_comment_end"]);

            // Define nonterminal variables of CFG
            nonterminals["program"] = new Nonterminal("PROG");
            nonterminals["statements"] = new Nonterminal("STMTS");
            nonterminals["statements_head"] = new Nonterminal("STMTS_HEAD");
            nonterminals["statements_tail"] = new Nonterminal("STMTS_TAIL");
            nonterminals["statement"] = new Nonterminal("STMT");
            nonterminals["declaration"] = new Nonterminal("DECL");
            nonterminals["declaration_assignment"] = new Nonterminal("DECL_ASSIGN");
            nonterminals["expression"] = new Nonterminal("EXPR");
            nonterminals["unary_operation"] = new Nonterminal("UNARY_OP");
            nonterminals["binary_operation"] = new Nonterminal("BINARY_OP");
            nonterminals["operand"] = new Nonterminal("OPND");

            // Define terminal variables of CFG
            terminals["identifier"] = new Terminal(tokenTypes["identifier"]);
            terminals["assert"] = new Terminal("assert");
            terminals["print"] = new Terminal("print");
            terminals["read"] = new Terminal("read");
            terminals["for"] = new Terminal("for");
            terminals["in"] = new Terminal("in");
            terminals["end"] = new Terminal("end");
            terminals["do"] = new Terminal("do");
            terminals["var"] = new Terminal("var");
            terminals["type"] = new Terminal(tokenTypes["type"]);
            terminals["string"] = new Terminal(tokenTypes["string"]);
            terminals["int"] = new Terminal(tokenTypes["int"]);
            terminals[")"] = new Terminal(")");
            terminals["("] = new Terminal("(");
            terminals[".."] = new Terminal("..");
            terminals[":="] = new Terminal(":=");
            terminals[":"] = new Terminal(":");
            terminals[";"] = new Terminal(";");
            terminals["binary_operator"] = new Terminal(tokenTypes["binary_op"]);
            terminals["unary_operator"] = new Terminal(tokenTypes["unary_op"]);

            // Create the Mini-PL grammar
            grammar = new CFG(nonterminals["program"], terminals.Values, nonterminals.Values);

            // define production rules for the grammar
            grammar.AddProductionRule(nonterminals["program"], new ISymbol[] { nonterminals["statements"] });
            grammar.AddProductionRule(nonterminals["statements"], new ISymbol[] { nonterminals["statements_head"], nonterminals["statements_tail"] });
            grammar.AddProductionRule(nonterminals["statements_head"], new ISymbol[] { nonterminals["statement"], terminals[";"] });
            grammar.AddProductionRule(nonterminals["statements_tail"], new ISymbol[] { nonterminals["statements_head"], nonterminals["statements_tail"] });
            grammar.AddProductionRule(nonterminals["statements_tail"], new ISymbol[] { Terminal.EPSILON });

            grammar.AddProductionRule(nonterminals["statement"], new ISymbol[] { nonterminals["declaration"] });
            grammar.AddProductionRule(nonterminals["statement"], new ISymbol[] { terminals["identifier"], terminals[":="], nonterminals["expression"] });
            grammar.AddProductionRule(nonterminals["statement"], new ISymbol[] { terminals["for"], terminals["identifier"], terminals["in"], nonterminals["expression"], terminals[".."], nonterminals["expression"], terminals["do"],
                                                                                   nonterminals["statements"], terminals["end"], terminals["for"] });
            grammar.AddProductionRule(nonterminals["statement"], new ISymbol[] { terminals["read"], terminals["identifier"] });
            grammar.AddProductionRule(nonterminals["statement"], new ISymbol[] { terminals["print"], nonterminals["expression"] });
            grammar.AddProductionRule(nonterminals["statement"], new ISymbol[] { terminals["assert"], terminals["("], nonterminals["expression"], terminals[")"] });

            grammar.AddProductionRule(nonterminals["declaration"], new ISymbol[] { terminals["var"], terminals["identifier"], terminals[":"], terminals["type"], nonterminals["declaration_assignment"] });
            grammar.AddProductionRule(nonterminals["declaration_assignment"], new ISymbol[] { terminals[":="], nonterminals["expression"] });
            grammar.AddProductionRule(nonterminals["declaration_assignment"], new ISymbol[] { Terminal.EPSILON });

            grammar.AddProductionRule(nonterminals["expression"], new ISymbol[] { nonterminals["unary_operation"] });
            grammar.AddProductionRule(nonterminals["expression"], new ISymbol[] { nonterminals["operand"], nonterminals["binary_operation"] });

            grammar.AddProductionRule(nonterminals["unary_operation"], new ISymbol[] { terminals["unary_operator"], nonterminals["operand"] });

            grammar.AddProductionRule(nonterminals["binary_operation"], new ISymbol[] { terminals["binary_operator"], nonterminals["operand"] });
            grammar.AddProductionRule(nonterminals["binary_operation"], new ISymbol[] { Terminal.EPSILON });

            grammar.AddProductionRule(nonterminals["operand"], new ISymbol[] { terminals["int"] });
            grammar.AddProductionRule(nonterminals["operand"], new ISymbol[] { terminals["string"] });
            grammar.AddProductionRule(nonterminals["operand"], new ISymbol[] { terminals["identifier"] });
            grammar.AddProductionRule(nonterminals["operand"], new ISymbol[] { terminals["("], nonterminals["expression"], terminals[")"] });

            // use ; as synchronizing token for Mini-PL
            parser = new Parser(grammar, terminals[";"]);
        }

        public Runnable ProcessParseTree(ParseTree parseTree, IEnumerable<IError> parseErrors)
        {
            Runnable prog = new Runnable();
            bool isValidParseTree = true;
            foreach (IError err in parseErrors)
            {
                prog.errors.Add(err);
                if (err is SyntaxError)
                    isValidParseTree = false;
            }

            // can't construct AST if parse tree is bad
            if (!isValidParseTree)
                return prog;

            // first remove unnecessary symbols ; : .. ( ) := and epsilons
            String[] pruneTokens = { "(", ")", ";", ":", "..", ":=", "var", "in", "for", "end", "do" };

            Predicate<IParseNode> isUnnecessaryTerminal =
                n => (n is ParseLeaf) ? (n as ParseLeaf).Token == null || pruneTokens.Contains((n as ParseLeaf).Token.Lexeme) : false;

            parseTree.RemoveNodes(isUnnecessaryTerminal);

            // remove any tree nodes with no children
            Predicate<IParseNode> isEmptyNonterminal =
                v => (v is ParseTree) ? (v as ParseTree).Children.Count == 0 : false;

            parseTree.RemoveNodes(isEmptyNonterminal);

            // refactor
            // STMTS->STMTS_HEAD STMTS_TAIL to STMTS->(STMT)+
            // DECL->"var" <IDENT> ":" <TYPE> ASSIGN to  DECL->"var" <IDENT> ":" <TYPE> [":=" <EXPR>] 
            // EXPR->UNARY|OPND BINARY to EXPR-> unary_op OPND | OPND | OPND binary_op OPND
            // OPND-><INT>|<STRING>|<IDENT>|<EXPR> to just <INT>|<STRING>|<IDENT>|<EXPR>
            Nonterminal[] pruneVariables = new Nonterminal[] { 
                nonterminals["statements_head"], nonterminals["statements_tail"], nonterminals["unary_operation"], 
                nonterminals["binary_operation"], nonterminals["declaration_assignment"], nonterminals["operand"] };

            Predicate<IParseNode> isUnnecessaryNonterminal =
                n => (n is ParseTree) ? pruneVariables.Contains((n as ParseTree).Nonterminal) : false;

            parseTree.RemoveNodes(isUnnecessaryNonterminal);

            if (Program.debug) Console.WriteLine(parseTree);

            // AST is formed at this point, so do semantic checks

            // find declarations, produce errors if identifier declared multiple times         
            foreach (IParseNode node in parseTree.Nodes())
            {
                if (node is ParseTree)
                {
                    ParseTree subtree = node as ParseTree;
                    if (subtree.Nonterminal == nonterminals["declaration"])
                    {
                        ParseLeaf idLeaf = (subtree.Children[0] as ParseLeaf);
                        ParseLeaf typeLeaf = (subtree.Children[1] as ParseLeaf);
                        Token idToken = idLeaf.Token;
                        Token typeToken = typeLeaf.Token;

                        string identifier = idToken.Lexeme;
                        ValueType type = Value.TypeFromString(typeToken.Lexeme);

                        Statement.DeclarationStmt declaration;
                        switch (subtree.Children.Count)
                        {
                            case 2: // simple declaration
                                declaration = new Statement.DeclarationStmt(identifier, type, idToken);
                                break;
                            case 3: // declaration with assignment
                                ParseLeaf valueLeaf = (subtree.Children[2] as ParseLeaf);
                                Expression expr = Expression.FromTreeNode(subtree.Children[2], terminals, nonterminals);
                                declaration = new Statement.DeclarationStmt(identifier, type, idToken, expr);
                                break;
                            default:
                                throw new Exception("BAD AST STRUCTURE");
                        }

                        if (prog.declarations.ContainsKey(identifier))
                            prog.errors.Add(new SemanticError(idToken, identifier + " multiply defined"));
                        else
                            prog.declarations[identifier] = declaration;
                    }
                }
            }

            // check that variables are defined before use
            foreach (IParseNode node in parseTree.Nodes())
            {
                if (node is ParseLeaf)
                {
                    ParseLeaf leaf = node as ParseLeaf;
                    Token leafToken = leaf.Token;
                    if (leafToken.Type == tokenTypes["identifier"])
                    {
                        string identifier = leafToken.Lexeme;
                        Position idPosition = leafToken.TextPosition;

                        if (!prog.declarations.ContainsKey(identifier))
                            prog.errors.Add(new SemanticError(leafToken, identifier + " never defined"));
                        else if (idPosition.CompareTo(prog.declarations[identifier].Token.TextPosition) < 0)
                            prog.errors.Add(new SemanticError(leafToken, identifier + " not defined before use"));
                    }
                }
            }

            // add statements to runnable
            ParseTree statementListNode = parseTree.Children[0] as ParseTree;
            foreach (IParseNode statementNode in statementListNode.Children)
                prog.statements.Add(Statement.FromTreeNode(statementNode, terminals, nonterminals));

            // check that for-loop control variables are not modified inside the for-loop
            foreach (Statement stmt in prog.statements)
            {
                if (stmt is Statement.ForStmt)
                {
                    Statement.ForStmt forStmt = stmt as Statement.ForStmt;
                    Stack<Statement> stmtStack = new Stack<Statement>();

                    foreach (Statement substmt in forStmt.Block)
                        stmtStack.Push(substmt);

                    while (stmtStack.Count != 0)
                    {
                        Statement s = stmtStack.Pop();
                        if (s is Statement.AssignStmt)
                        {
                            Statement.AssignStmt assignment = s as Statement.AssignStmt;
                            if (assignment.Identifier == forStmt.Identifier)
                                prog.errors.Add(new SemanticError(assignment.Token, forStmt.Identifier + " cannot be modified inside for-loop"));
                        }
                        else if (s is Statement.DeclarationStmt)
                        {
                            Statement.DeclarationStmt declaration = s as Statement.DeclarationStmt;
                            if (declaration.Identifier == forStmt.Identifier)
                                prog.errors.Add(new SemanticError(declaration.Token, forStmt.Identifier + " cannot be modified inside for-loop"));
                        }
                        else if (s is Statement.ForStmt)
                        {
                            Statement.ForStmt nestedFor = s as Statement.ForStmt;
                            if (nestedFor.Identifier == forStmt.Identifier)
                                prog.errors.Add(new SemanticError(nestedFor.Token, forStmt.Identifier + " cannot be modified inside for-loop"));
                            foreach (Statement substmt in nestedFor.Block)
                                stmtStack.Push(substmt);
                        }
                    }
                }
            }

            // typecheck each statement
            foreach (Statement stmt in prog.statements)
                stmt.TypeCheck(prog);

            return prog;
        }

        // Representation of Mini-PL program as a list of executable statements
        // with variable and declaration information
        public class Runnable
        {
            public List<Statement> statements = new List<Statement>();
            public Dictionary<string, Statement.DeclarationStmt> declarations = new Dictionary<string, Statement.DeclarationStmt>();
            public Dictionary<string, Value> values = new Dictionary<string, Value>();
            public List<IError> errors = new List<IError>();

            public bool Execute(TextReader stdin, TextWriter stdout)
            {
                // just print errors if there are any
                if (errors.Count > 0)
                {
                    foreach (IError err in errors)
                        stdout.WriteLine(err.GetMessage());
                    return false;
                }

                if (Program.debug) Console.WriteLine("Start execution");

                // execute each statement in order, stopping if runtime error detected
                foreach (Statement stmt in statements)
                {
                    RuntimeError err;
                    try
                    {
                        err = stmt.Execute(this, stdin, stdout);
                    }
                    catch (MiniPL_DivideByZeroException ex) // ugly but it works
                    {
                        err = ex.Error;
                    }
                    if (err != null)
                    {
                        stdout.WriteLine(err.GetMessage());
                        return false;
                    }
                }

                if (Program.debug) Console.WriteLine("Stop execution");

                return true;
            }
        }
    }
}
