using InterpreterProject.Errors;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.Languages
{
    public abstract class Statement
    {
        public abstract RuntimeError Execute(MiniPL.Runnable context, TextReader stdin, TextWriter stdout);
        public abstract void TypeCheck(MiniPL.Runnable context);

        Token token;
        public Token Token
        {
            get { return token; }
            set { }
        }

        Statement(Token token) 
        { 
            this.token = token; 
        }

        public static Statement FromTreeNode(IParseNode ASTNode,
            Dictionary<String, Terminal> terms,
            Dictionary<String, Nonterminal> vars)
        {
            ParseTree subtree =
                ASTNode as ParseTree;
            if (subtree == null)
                throw new Exception("EXPECTED TREE NODE");
            if (subtree.Nonterminal != vars["statement"])
                throw new Exception("EXPECTED STATEMENT NODE");

            if (subtree.Children[0] is ParseTree) // ----------------------------------- Declaration
            {
                ParseTree declTree =
                    subtree.Children[0] as ParseTree;
                ParseLeaf idLeaf =
                    declTree.Children[0] as ParseLeaf;
                ParseLeaf typeLeaf =
                    declTree.Children[1] as ParseLeaf;
                if (idLeaf == null || typeLeaf == null)
                    throw new Exception("BAD AST STRUCTURE");
                Token idToken = idLeaf.Token;
                Token typeToken = typeLeaf.Token;

                string identifier = idToken.Lexeme;
                ValueType type = Value.TypeFromString(typeToken.Lexeme);

                switch (declTree.Children.Count)
                {
                    case 2: // ------------------------------------------------------------------------ simple declaration
                        return new Statement.DeclarationStmt(identifier, type, idToken);
                    case 3: // ------------------------------------------------------------------------ declaration with assignment
                        ParseLeaf valueLeaf =
                            declTree.Children[2] as ParseLeaf;
                        Expression expr =
                            Expression.FromTreeNode(declTree.Children[2], terms, vars);
                        return new Statement.DeclarationStmt(identifier, type, idToken, expr);
                    default:
                        throw new Exception("BAD AST STRUCTURE");
                }
            }
            else // Assignment or read or print or assert or for
            {
                ParseLeaf firstChild =
                    subtree.Children[0] as ParseLeaf;
                if (firstChild.Terminal.MatchedTokenType != null &&
                    firstChild.Terminal.MatchedTokenType.Name == "identifier") // ---------------------------- assignment or for
                {
                    if (subtree.Children.Count == 2) // ----------------------------------------------- assignment
                    {
                        return new AssignStmt(firstChild.Token.Lexeme,
                            Expression.FromTreeNode(subtree.Children[1], terms, vars),
                            firstChild.Token);
                    }
                    else if (subtree.Children.Count == 4) // ------------------------------------------ for
                    {
                        List<Statement> block = new List<Statement>();
                        ParseTree blockChild =
                            subtree.Children[3] as ParseTree;
                        foreach (IParseNode blockSubtree in blockChild.Children)
                            block.Add(Statement.FromTreeNode(blockSubtree, terms, vars));
                        if (blockChild == null)
                            throw new Exception("MALFORMED AST");
                        return new ForStmt(firstChild.Token.Lexeme,
                            Expression.FromTreeNode(subtree.Children[1], terms, vars),
                            Expression.FromTreeNode(subtree.Children[2], terms, vars),
                            block, firstChild.Token);
                    }
                    else throw new Exception("MALFORMED AST");
                }
                else
                {
                    if (subtree.Children.Count != 2)
                        throw new Exception("MALFORMED AST");
                    switch (firstChild.Token.Lexeme)
                    {
                        case "assert": // ------------------------------------------------------------- assert                        
                            return new AssertStmt(Expression.FromTreeNode(
                                subtree.Children[1],
                                terms, vars),
                                firstChild.Token);
                        case "print": // -------------------------------------------------------------- print
                            return new PrintStmt(Expression.FromTreeNode(
                                subtree.Children[1],
                                terms, vars),
                                firstChild.Token);
                        case "read": // --------------------------------------------------------------- read
                            ParseLeaf secondChild =
                                subtree.Children[1] as ParseLeaf;
                            if (secondChild == null)
                                throw new Exception("MALFORMED AST");
                            return new ReadStmt(secondChild.Token.Lexeme, firstChild.Token);
                        default:
                            throw new Exception("UNEXPECTED STATEMENT TYPE");
                    }
                }
            }
            throw new Exception("THIS SHOULD NOT HAPPEN WHAT DID YOU DO");
        }

        public class ReadStmt : Statement
        {
            string identifier;
            public string Identifier
            {
                get { return identifier; }
            }

            public ReadStmt(string identifier, Token token)
                : base(token)
            {
                this.identifier = identifier;
            }

            public override RuntimeError Execute(MiniPL.Runnable context, TextReader stdin, TextWriter stdout)
            {
                string input = "";
                char[] buf = new char[1];

                while (true)
                {
                    int read = stdin.Read(buf, 0, 1);
                    if (read == 0) break;
                    if (buf[0] == ' ' || buf[0] == '\t' || buf[0] == '\n') break;
                    input += buf[0];
                }

                if (context.declarations[identifier].Type == ValueType.Integer)
                {
                    int inputInt = 0;
                    if (!Int32.TryParse(input, out inputInt)) return new RuntimeError(Token, "expected to read integer");
                    context.values[identifier] = new Value(inputInt);
                }
                else if (context.declarations[identifier].Type == ValueType.String)
                {
                    context.values[identifier] = new Value(input);
                }
                else
                    throw new Exception("TYPE CHECKING FAILED");

                return null;
            }

            public override void TypeCheck(MiniPL.Runnable context)
            {
                if (context.declarations[identifier].Type == ValueType.Boolean)
                    context.errors.Add(new SemanticError(Token, "cannot read a boolean value"));
            }
        }

        public class AssertStmt : Statement
        {
            Expression expr;

            public AssertStmt(Expression expr, Token token)
                : base(token)
            { 
                this.expr = expr; 
            }

            public override RuntimeError Execute(MiniPL.Runnable context, TextReader stdin, TextWriter stdout)
            {
                if (expr.Type(context) == ValueType.String)
                    throw new Exception("TYPE CHECKING FAILED");
                if (expr.Evaluate(context).BooleanValue() == false)
                    return new RuntimeError(Token, "assertion failed");
                return null;
            }

            public override void TypeCheck(MiniPL.Runnable context)
            {
                expr.TypeCheck(context);
                if (expr.Type(context) == ValueType.String)
                    context.errors.Add(new SemanticError(Token, "cannot assert a string value"));
            }
        }

        public class PrintStmt : Statement
        {
            Expression expr;

            public PrintStmt(Expression expr, Token token)
                : base(token)
            {
                this.expr = expr;
            }

            public override RuntimeError Execute(MiniPL.Runnable context, TextReader stdin, TextWriter stdout)
            {
                switch (expr.Type(context))
                {
                    case ValueType.Integer:
                        stdout.Write(expr.Evaluate(context).IntValue());
                        break;
                    case ValueType.String:
                        stdout.Write(expr.Evaluate(context).StringValue());
                        break;
                    default:
                        throw new Exception("TYPE CHECK FAILED");
                }
                return null;
            }

            public override void TypeCheck(MiniPL.Runnable context)
            {
                expr.TypeCheck(context);
                if (expr.Type(context) == ValueType.Boolean)
                    context.errors.Add(new SemanticError(Token, "cannot print a boolean value"));
            }
        }

        public class AssignStmt : Statement
        {
            Expression expr;

            string identifier;
            public string Identifier
            {
                get { return identifier; }
            }
           
            public AssignStmt(string identifier, Expression expr, Token token)
                : base(token)
            {
                this.identifier = identifier;
                this.expr = expr;
            }

            public override RuntimeError Execute(MiniPL.Runnable context, TextReader stdin, TextWriter stdout)
            {
                context.values[identifier] = expr.Evaluate(context);
                return null;
            }

            public override void TypeCheck(MiniPL.Runnable context)
            {
                expr.TypeCheck(context);
                switch (context.declarations[identifier].Type)
                {
                    case ValueType.Boolean:
                        if (expr.Type(context) == ValueType.String)
                            context.errors.Add(new SemanticError(Token, "cannot assign string value to boolean " + identifier));
                        break;
                    case ValueType.Integer:
                        if (expr.Type(context) != ValueType.Integer)
                            context.errors.Add(new SemanticError(Token, "expected integer type value for " + identifier));
                        break;
                    case ValueType.String:
                        if (expr.Type(context) != ValueType.String)
                            context.errors.Add(new SemanticError(Token, "expected string type value for " + identifier));
                        break;
                }
            }
        }

        public class ForStmt : Statement
        {                      
            Expression startVal;
            Expression endVal;
            
            private string identifier;
            public string Identifier
            {
                get { return identifier; }
            }

            private IEnumerable<Statement> block;
            public IEnumerable<Statement> Block
            {
                get { return block; }
            }

            public ForStmt(string identifier, Expression startVal, Expression endVal, IEnumerable<Statement> block, Token token)
                : base(token)
            {
                this.identifier = identifier;
                this.startVal = startVal;
                this.endVal = endVal;
                this.block = block;
            }

            public override RuntimeError Execute(MiniPL.Runnable context, TextReader stdin, TextWriter stdout)
            {
                int start = startVal.Evaluate(context).IntValue();
                int end = endVal.Evaluate(context).IntValue();

                for (int i = start; i <= end; i++)
                {
                    context.values[identifier] = new Value(i);
                    foreach (Statement stmt in block)
                    {
                        stmt.Execute(context, stdin, stdout);
                    }
                }
                context.values[identifier] = new Value(end + 1);
                return null;
            }

            public override void TypeCheck(MiniPL.Runnable context)
            {
                startVal.TypeCheck(context);
                endVal.TypeCheck(context);
                if (context.declarations[identifier].Type != ValueType.Integer)
                    context.errors.Add(new SemanticError(Token, "bad for-loop control type"));
                if (startVal.Type(context) != ValueType.Integer)
                    context.errors.Add(new SemanticError(startVal.Token, "bad for-loop start value type"));
                if (endVal.Type(context) != ValueType.Integer)
                    context.errors.Add(new SemanticError(startVal.Token, "bad for-loop end value type"));
                foreach (Statement stmt in block)
                {
                    stmt.TypeCheck(context);
                }
            }
        }

        public class DeclarationStmt : Statement
        {         
            Expression initialValue;

            string identifier;
            public string Identifier
            {
                get { return identifier; }
            }

            ValueType type;
            public ValueType Type
            {
                get { return type; }
            }

            public DeclarationStmt(string identifier, ValueType type, Token token, Expression initialValue)
                : this(identifier, type, token)
            {
                this.initialValue = initialValue;
            }

            public DeclarationStmt(string identifier, ValueType type, Token token)
                : base(token)
            {
                this.identifier = identifier;
                this.type = type;
            }

            public override RuntimeError Execute(MiniPL.Runnable context, TextReader stdin, TextWriter stdout)
            {
                context.values[identifier] = (initialValue == null) ? new Value(type) : initialValue.Evaluate(context);
                return null;
            }

            public override void TypeCheck(MiniPL.Runnable context)
            {
                if (initialValue != null)
                {
                    initialValue.TypeCheck(context);
                    switch (type)
                    {
                        case ValueType.Boolean:
                            if (initialValue.Type(context) == ValueType.String)
                                context.errors.Add(new SemanticError(Token, "cannot assign string value to boolean " + identifier));
                            break;
                        case ValueType.Integer:
                            if (initialValue.Type(context) != ValueType.Integer)
                                context.errors.Add(new SemanticError(Token, "expected integer type value for " + identifier));
                            break;
                        case ValueType.String:
                            if (initialValue.Type(context) != ValueType.String)
                                context.errors.Add(new SemanticError(Token, "expected string type value for " + identifier));
                            break;
                    }
                }
            }
        }
    }
}
