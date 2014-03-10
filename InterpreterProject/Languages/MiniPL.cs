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
    public class MiniPL
    {
        static MiniPL instance;

        Dictionary<String, TokenType> tts = new Dictionary<string, TokenType>();
        Dictionary<String, Terminal> terms = new Dictionary<string, Terminal>();
        Dictionary<String, Nonterminal> vars = new Dictionary<string, Nonterminal>();
        CFG grammar;

        ParseTable parseTable;

        public static MiniPL GetInstance()
        {
            if (instance == null)
                instance = new MiniPL();
            return instance;
        }

        private MiniPL()
        {
            // Define regexes for tokens
            Regex reBlockComment = Regex.Concat("/*").Concat(Regex.Not('*').Union(Regex.Char('*').Plus().Concat(Regex.Not('*', '/'))).Star())
                                                     .Concat(Regex.Char('*').Plus().Concat(Regex.Char('/')));
            Regex reLineComment = Regex.Concat("//").Concat(Regex.Not('\n').Star());

            Regex reWhitespace = Regex.Union(" \t\n").Star().Union(reBlockComment).Union(reLineComment);

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
            tts["int"] = new TokenType("int", reInteger);
            tts["whitespace"] = new TokenType("whitespace", reWhitespace, priority: TokenType.Priority.Whitespace);
            tts["string"] = new TokenType("string", reString);
            tts["binary_op"] = new TokenType("binary op", reBinaryOperator);
            tts["unary_op"] = new TokenType("unary op", reUnaryOperator);
            tts["keyword"] = new TokenType("keyword", reKeyword, priority: TokenType.Priority.Keyword);
            tts["type"] = new TokenType("type", reType, priority: TokenType.Priority.Keyword);
            tts["left_paren"] = new TokenType("left paren", reParenLeft);
            tts["right_paren"] = new TokenType("right paren", reParenRight);
            tts["colon"] = new TokenType("colon", reColon);
            tts["semicolon"] = new TokenType("semicolon", reSemicolon);
            tts["assignment"] = new TokenType("assignment", reAssignment);
            tts["dots"] = new TokenType("dots", reDots);
            tts["identifier"] = new TokenType("identifier", reIdentifier);

            // Define nonterminal variables of CFG
            vars["program"] = new Nonterminal("PROG");
            vars["statements"] = new Nonterminal("STMTS");
            vars["statements_head"] = new Nonterminal("STMTS_HEAD");
            vars["statements_tail"] = new Nonterminal("STMTS_TAIL");
            vars["statement"] = new Nonterminal("STMT");
            vars["declaration"] = new Nonterminal("DECL");
            vars["declaration_assignment"] = new Nonterminal("DECL_ASSIGN");
            vars["expression"] = new Nonterminal("EXPR");
            vars["unary_operation"] = new Nonterminal("UNARY_OP");
            vars["binary_operation"] = new Nonterminal("BINARY_OP");
            vars["operand"] = new Nonterminal("OPND");

            // Define terminal variables of CFG
            terms["identifier"] = new Terminal(tts["identifier"]);
            terms["assert"] = new Terminal("assert");
            terms["print"] = new Terminal("print");
            terms["read"] = new Terminal("read");
            terms["for"] = new Terminal("for");
            terms["in"] = new Terminal("in");
            terms["end"] = new Terminal("end");
            terms["do"] = new Terminal("do");
            terms["var"] = new Terminal("var");
            terms["type"] = new Terminal(tts["type"]);
            terms["string"] = new Terminal(tts["string"]);
            terms["int"] = new Terminal(tts["int"]);
            terms[")"] = new Terminal(")");
            terms["("] = new Terminal("(");
            terms[".."] = new Terminal("..");
            terms[":="] = new Terminal(":=");
            terms[":"] = new Terminal(":");
            terms[";"] = new Terminal(";");
            terms["binary_operator"] = new Terminal(tts["binary_op"]);
            terms["unary_operator"] = new Terminal(tts["unary_op"]);

            grammar = new CFG(vars["program"], terms.Values, vars.Values);

            grammar.AddProductionRule(vars["program"], new ISymbol[] { vars["statements"] });
            grammar.AddProductionRule(vars["statements"], new ISymbol[] { vars["statements_head"], vars["statements_tail"] });
            grammar.AddProductionRule(vars["statements_head"], new ISymbol[] { vars["statement"], terms[";"] });
            grammar.AddProductionRule(vars["statements_tail"], new ISymbol[] { vars["statements_head"], vars["statements_tail"] });
            grammar.AddProductionRule(vars["statements_tail"], new ISymbol[] { Terminal.EPSILON });

            grammar.AddProductionRule(vars["statement"], new ISymbol[] { vars["declaration"] });
            grammar.AddProductionRule(vars["statement"], new ISymbol[] { terms["identifier"], terms[":="], vars["expression"] });
            grammar.AddProductionRule(vars["statement"], new ISymbol[] { terms["for"], terms["identifier"], terms["in"], vars["expression"], terms[".."], vars["expression"], terms["do"],
                                                                                   vars["statements"], terms["end"], terms["for"] });
            grammar.AddProductionRule(vars["statement"], new ISymbol[] { terms["read"], terms["identifier"] });
            grammar.AddProductionRule(vars["statement"], new ISymbol[] { terms["print"], vars["expression"] });
            grammar.AddProductionRule(vars["statement"], new ISymbol[] { terms["assert"], terms["("], vars["expression"], terms[")"] });

            grammar.AddProductionRule(vars["declaration"], new ISymbol[] { terms["var"], terms["identifier"], terms[":"], terms["type"], vars["declaration_assignment"] });
            grammar.AddProductionRule(vars["declaration_assignment"], new ISymbol[] { terms[":="], vars["expression"] });
            grammar.AddProductionRule(vars["declaration_assignment"], new ISymbol[] { Terminal.EPSILON });

            grammar.AddProductionRule(vars["expression"], new ISymbol[] { vars["unary_operation"] });
            grammar.AddProductionRule(vars["expression"], new ISymbol[] { vars["operand"], vars["binary_operation"] });

            grammar.AddProductionRule(vars["unary_operation"], new ISymbol[] { terms["unary_operator"], vars["operand"] });

            grammar.AddProductionRule(vars["binary_operation"], new ISymbol[] { terms["binary_operator"], vars["operand"] });
            grammar.AddProductionRule(vars["binary_operation"], new ISymbol[] { Terminal.EPSILON });

            grammar.AddProductionRule(vars["operand"], new ISymbol[] { terms["int"] });
            grammar.AddProductionRule(vars["operand"], new ISymbol[] { terms["string"] });
            grammar.AddProductionRule(vars["operand"], new ISymbol[] { terms["identifier"] });
            grammar.AddProductionRule(vars["operand"], new ISymbol[] { terms["("], vars["expression"], terms[")"] });

            parseTable = grammar.CreateLL1ParseTable();
        }

        public Dictionary<string, TokenType> GetTokenTypes()
        {
            return tts;
        }

        public Dictionary<string, Nonterminal> GetGrammarNonterminals()
        {
            return vars;
        }

        public Dictionary<string, Terminal> GetGrammarTerminals()
        {
            return terms;
        }

        public Scanner GetScanner()
        {
            TokenAutomaton automaton = TokenType.CombinedAutomaton(tts.Values.ToArray());
            return new Scanner(automaton); 
        }

        public Parser GetParser()
        {
            return new Parser(parseTable, vars["program"], terms[";"]);
        }

        public CFG GetGrammar()
        {
            return grammar;
        }

        public Runnable ProcessParseTree(Tree<Parser.IParseValue> parseTree)
        {
            // first remove unnecessary symbols ; : .. ( ) := and epsilons
            String[] pruneTokens = {"(",")",";",":","..",":=","var","in","for","end","do"};

            Predicate<Parser.IParseValue> isUnnecessaryTerminal =
                v => (v is Parser.TerminalValue) ? (v as Parser.TerminalValue).token == null || pruneTokens.Contains((v as Parser.TerminalValue).token.lexeme) : false;

            parseTree.RemoveNodesByValue(isUnnecessaryTerminal);

            // remove any tree nodes with no children
            Predicate<INode<Parser.IParseValue>> isEmptyNonterminal =
                v => (v is Tree<Parser.IParseValue>) ? (v as Tree<Parser.IParseValue>).children.Count == 0 : false;

            parseTree.RemoveNodes(isEmptyNonterminal);


            // refactor
            // STMTS->STMTS_HEAD STMTS_TAIL to STMTS->(STMT)+
            // DECL->"var" <IDENT> ":" <TYPE> ASSIGN to  DECL->"var" <IDENT> ":" <TYPE> [":=" <EXPR>] 
            // EXPR->UNARY|OPND BINARY to EXPR-> unary_op OPND | OPND | OPND binary_op OPND
            // OPND-><INT>|<STRING>|<IDENT>|<EXPR> -> just <INT>|<STRING>|<IDENT>|<EXPR>
            Nonterminal[] pruneVariables = new Nonterminal[] { 
                vars["statements_head"], vars["statements_tail"], vars["unary_operation"], 
                vars["binary_operation"], vars["declaration_assignment"], vars["operand"] };

            Predicate<Parser.IParseValue> isUnnecessaryNonterminal =
                v => (v is Parser.NonterminalValue) ? pruneVariables.Contains((v as Parser.NonterminalValue).var) : false;

            parseTree.RemoveNodesByValue(isUnnecessaryNonterminal);

            Runnable prog = new Runnable();

            Console.WriteLine(parseTree);

            // find declarations, produce errors if identifier declared multiple times         
            foreach (INode<Parser.IParseValue> node in parseTree.Nodes())
            {
                if (node is Tree<Parser.IParseValue>)
                {
                    Tree<Parser.IParseValue> subtree = node as Tree<Parser.IParseValue>;
                    Parser.NonterminalValue subtreeValue = node.GetValue() as Parser.NonterminalValue;
                    if (subtreeValue.var == vars["declaration"])
                    {
                        Leaf<Parser.IParseValue> idLeaf = (subtree.children[0] as Leaf<Parser.IParseValue>);
                        Leaf<Parser.IParseValue> typeLeaf = (subtree.children[1] as Leaf<Parser.IParseValue>);
                        Token idToken = (idLeaf.GetValue() as Parser.TerminalValue).token;
                        Token typeToken = (typeLeaf.GetValue() as Parser.TerminalValue).token;

                        string identifier = idToken.lexeme;
                        ValueType type = Value.TypeFromString(typeToken.lexeme);

                        Statement.DeclarationStmt declaration;
                        switch (subtree.children.Count)
                        {
                            case 2: // simple declaration
                                declaration = new Statement.DeclarationStmt(identifier, type, idToken);
                                break;
                            case 3: // declaration with assignment
                                Leaf<Parser.IParseValue> valueLeaf = (subtree.children[2] as Leaf<Parser.IParseValue>);
                                Expression expr = Expression.FromTreeNode(subtree.children[2], terms, vars);
                                declaration = new Statement.DeclarationStmt(identifier, type, idToken, expr);
                                break;
                            default:
                                throw new Exception("BAD AST STRUCTURE");
                        }

                        if (prog.declarations.ContainsKey(identifier))
                            prog.errors.Add(new SemanticError(idToken, identifier+" multiply defined"));
                        else
                            prog.declarations[identifier] = declaration;                                     
                    }
                }
            }

            // check that variables are defined before use
            foreach (INode<Parser.IParseValue> node in parseTree.Nodes())
            {
                if (node is Leaf<Parser.IParseValue>)
                {
                    Leaf<Parser.IParseValue> leaf = node as Leaf<Parser.IParseValue>;
                    Parser.TerminalValue leafValue = node.GetValue() as Parser.TerminalValue;
                    Token leafToken = leafValue.token;
                    if (leafToken.tokenType == tts["identifier"])
                    {
                        string identifier = leafToken.lexeme;
                        Position idPosition = leafToken.pos;

                        if (!prog.declarations.ContainsKey(identifier))
                            prog.errors.Add(new SemanticError(leafToken, identifier + " never defined"));
                        else if (idPosition.CompareTo(prog.declarations[identifier].token.pos) < 0)  
                            prog.errors.Add(new SemanticError(leafToken, identifier + " not defined before use"));
                    }
                }
            }

            // for-loop control variables not assigned to inside for loop



            // add statements to runnable

            Tree<Parser.IParseValue> statementListNode = parseTree.children[0] as Tree<Parser.IParseValue>;
            foreach (INode<Parser.IParseValue> statementNode in statementListNode.children)
                prog.statements.Add(Statement.FromTreeNode(statementNode, terms, vars));

            return prog;
        }        

        public class Runnable
        {
            public List<Statement> statements = new List<Statement>();
            public Dictionary<string, Statement.DeclarationStmt> declarations = new Dictionary<string, Statement.DeclarationStmt>();
            public Dictionary<string, Value> values = new Dictionary<string, Value>();
            public List<IError> errors = new List<IError>();    
    
            public bool Execute(TextReader stdin, TextWriter stdout)
            {

                if (errors.Count > 0)
                {
                    foreach (IError err in errors)
                        stdout.WriteLine(err.GetMessage());
                    return false;
                }

                Console.WriteLine("Start execution");

                foreach (Statement stmt in statements)
                {                                    
                    RuntimeError err = stmt.Execute(this, stdin, stdout);
                    if (err != null)
                    {
                        stdout.WriteLine(err.GetMessage());
                        return false;
                    }
                }

                Console.WriteLine("\nStop execution");

                return true;
            }
        }   
    }
}
