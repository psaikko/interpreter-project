using InterpreterProject.Errors;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.Languages
{
    // Representation of a Mini-PL expression
    public abstract class Expression
    {
        // used to keep track of relevant text position
        Token token;
        public Token Token
        {
            get { return token; }
        }

        public abstract Value Evaluate(MiniPL.Runnable context);
        public abstract ValueType Type(MiniPL.Runnable context);
        public abstract void TypeCheck(MiniPL.Runnable context);

        // Every expression contains token to identify location in code
        Expression(Token token) 
        { 
            this.token = token; 
        }

        // creates an appropriate Expression object from an abstract syntax tree node
        public static Expression FromTreeNode(IParseNode ASTNode,
            Dictionary<String, Terminal> terms,
            Dictionary<String, Nonterminal> vars)
        {
            if (ASTNode is ParseTree)
            {
                ParseTree subtree = ASTNode as ParseTree;
                if (subtree.Nonterminal != vars["expression"]) throw new Exception("EXPECTED EXPRESSION NODE");

                switch (subtree.Children.Count)
                {
                    case 1:
                        IParseNode child = subtree.Children[0];
                        if (child is ParseLeaf) // identifier or literal
                            return ExprFromLeaf(child as ParseLeaf);
                        else // another expr
                            return FromTreeNode(child, terms, vars);
                    case 2: // unary operation
                        {
                            IParseNode op = subtree.Children[0];
                            ParseLeaf opLeaf = op as ParseLeaf;
                            if (opLeaf == null) throw new Exception("MALFORMED AST");

                            IParseNode opnd = subtree.Children[1];
                            Expression baseExpr;
                            if (opnd is ParseLeaf)
                                baseExpr = ExprFromLeaf(opnd as ParseLeaf);
                            else
                                baseExpr = FromTreeNode(opnd as ParseTree, terms, vars);
                            return new UnaryOp(opLeaf.Token.Lexeme[0], baseExpr, opLeaf.Token);
                        }
                    case 3: // binary operation
                        {
                            IParseNode op = subtree.Children[1];
                            ParseLeaf opLeaf = op as ParseLeaf;
                            if (opLeaf == null) throw new Exception("MALFORMED AST");

                            IParseNode lhs = subtree.Children[0];
                            IParseNode rhs = subtree.Children[2];
                            Expression lhsExpr, rhsExpr;
                            if (lhs is ParseLeaf) lhsExpr = ExprFromLeaf(lhs as ParseLeaf);
                            else lhsExpr = FromTreeNode(lhs as ParseTree, terms, vars);
                            if (rhs is ParseLeaf) rhsExpr = ExprFromLeaf(rhs as ParseLeaf);
                            else rhsExpr = FromTreeNode(rhs as ParseTree, terms, vars);
                            return new BinaryOp(lhsExpr, opLeaf.Token.Lexeme[0], rhsExpr, opLeaf.Token);
                        }
                    default:
                        throw new Exception("MALFORMED AST");
                }
            }
            else throw new Exception("EXPECTED LEAF NODE");
        }

        // Creates an indentifier or value expression from an abstract syntax tree leaf
        private static Expression ExprFromLeaf(ParseLeaf leaf)
        {
            if (leaf == null) throw new Exception("MALFORMED AST");
            if (leaf.Token.Type.Name == "identifier")
                return new IdentifierExpr(leaf.Token.Lexeme, leaf.Token);
            else
                return new ValueExpr(new Value(leaf.Token.Type.Name, leaf.Token.Lexeme), leaf.Token);
        }

        // Literal value
        public class ValueExpr : Expression
        {
            Value value;

            public ValueExpr(Value value, Token token)
                : base(token)
            {
                this.value = value;
            }

            override public Value Evaluate(MiniPL.Runnable context)
            {
                return value;
            }

            override public ValueType Type(MiniPL.Runnable context)
            {
                return value.Type();
            }

            override public void TypeCheck(MiniPL.Runnable context)
            {
                // nothing to check
            }
        }

        // variable identifier
        public class IdentifierExpr : Expression
        {
            string identifier;

            public IdentifierExpr(string identifier, Token token)
                : base(token)
            {
                this.identifier = identifier;
            }

            override public Value Evaluate(MiniPL.Runnable context)
            {
                return context.values[identifier];
            }

            override public ValueType Type(MiniPL.Runnable context)
            {
                return context.declarations[identifier].Type;
            }

            override public void TypeCheck(MiniPL.Runnable context)
            {
                // nothing to check
            }
        }

        // unary operation (!)
        public class UnaryOp : Expression
        {
            public char op;
            public Expression expr;

            public UnaryOp(char op, Expression expr, Token token)
                : base(token)
            {
                this.op = op; this.expr = expr;
            }

            override public Value Evaluate(MiniPL.Runnable context)
            {
                if (op == '!')
                {
                    if (expr.Type(context) == ValueType.Boolean)
                        return new Value(!expr.Evaluate(context).BooleanValue());
                    if (expr.Type(context) == ValueType.Integer)
                        return new Value(expr.Evaluate(context).IntValue() == 0);
                    throw new Exception("TYPE CHECKING FAILED");
                }
                throw new Exception("UNEXPECTED OPERATION " + op);
            }

            override public ValueType Type(MiniPL.Runnable context)
            {
                return ValueType.Boolean;
            }

            override public void TypeCheck(MiniPL.Runnable context)
            {
                expr.TypeCheck(context);
                if (expr.Type(context) != ValueType.Boolean &&
                    expr.Type(context) != ValueType.Integer)
                    context.errors.Add(new SemanticError(Token, "bad type for '!' operation"));
            }
        }

        // binary arithmetic or logical operation (+-/*=&<)
        public class BinaryOp : Expression
        {
            Expression lhs;
            char op;
            Expression rhs;

            public BinaryOp(Expression lhs, char op, Expression rhs, Token token)
                : base(token)
            {
                this.op = op;
                this.lhs = lhs;
                this.rhs = rhs;
            }

            // apply the appropriate operation on operands
            override public Value Evaluate(MiniPL.Runnable context)
            {
                switch (op)
                {
                    case '+':
                        if (rhs.Type(context) == ValueType.Integer && lhs.Type(context) == ValueType.Integer)
                            return new Value(lhs.Evaluate(context).IntValue() + rhs.Evaluate(context).IntValue());
                        if (rhs.Type(context) == ValueType.String && lhs.Type(context) == ValueType.String)
                            return new Value(lhs.Evaluate(context).StringValue() + rhs.Evaluate(context).StringValue());
                        throw new Exception("TYPECHECKING FAILED");
                    case '-':
                        if (rhs.Type(context) != ValueType.Integer || lhs.Type(context) != ValueType.Integer)
                            throw new Exception("TYPECHECKING FAILED");
                        return new Value(lhs.Evaluate(context).IntValue() - rhs.Evaluate(context).IntValue());
                    case '*':
                        if (rhs.Type(context) != ValueType.Integer || lhs.Type(context) != ValueType.Integer)
                            throw new Exception("TYPECHECKING FAILED");
                        return new Value(lhs.Evaluate(context).IntValue() * rhs.Evaluate(context).IntValue());
                    case '/':
                        if (rhs.Type(context) != ValueType.Integer || lhs.Type(context) != ValueType.Integer)
                            throw new Exception("TYPECHECKING FAILED");
                        int denominator = rhs.Evaluate(context).IntValue();
                        if (denominator == 0)
                            throw new MiniPL_DivideByZeroException(new RuntimeError(Token, "divide by zero"));
                        return new Value(lhs.Evaluate(context).IntValue() / denominator);
                    case '<':
                        if (rhs.Type(context) != ValueType.Integer || lhs.Type(context) != ValueType.Integer)
                            throw new Exception("TYPECHECKING FAILED");
                        return new Value(lhs.Evaluate(context).IntValue() < rhs.Evaluate(context).IntValue());
                    case '=':
                        if (rhs.Type(context) != lhs.Type(context))
                            throw new Exception("TYPECHECKING FAILED");
                        switch (rhs.Type(context))
                        {
                            case ValueType.Boolean:
                                return new Value(rhs.Evaluate(context).BooleanValue() == lhs.Evaluate(context).BooleanValue());
                            case ValueType.Integer:
                                return new Value(rhs.Evaluate(context).IntValue() == lhs.Evaluate(context).IntValue());
                            case ValueType.String:
                                return new Value(rhs.Evaluate(context).StringValue() == lhs.Evaluate(context).StringValue());
                        }
                        break;
                    case '&':
                        if (rhs.Type(context) == ValueType.String ||
                            lhs.Type(context) == ValueType.String)
                            throw new Exception("TYPECHECKING FAILED");
                        return new Value(rhs.Evaluate(context).BooleanValue() && lhs.Evaluate(context).BooleanValue());
                }
                throw new Exception("UNEXPECTED OPERATOR " + op);
            }

            // the expected return type of the operation
            override public ValueType Type(MiniPL.Runnable context)
            {
                switch (op)
                {
                    case '+':
                        ValueType t = lhs.Type(context);
                        if (t == ValueType.String) return t;
                        return ValueType.Integer;
                    case '-':
                        return ValueType.Integer;
                    case '*':
                        return ValueType.Integer;
                    case '/':
                        return ValueType.Integer;
                    case '<':
                        return ValueType.Boolean;
                    case '=':
                        return ValueType.Boolean;
                    case '&':
                        return ValueType.Boolean;
                    default:
                        throw new Exception("UNEXPECTER OPERATOR " + op);
                }
            }

            // recursively check operands and check that their types are compatible with operation
            override public void TypeCheck(MiniPL.Runnable context)
            {
                rhs.TypeCheck(context);
                lhs.TypeCheck(context);
                switch (op)
                {
                    case '+':
                        if (!((rhs.Type(context) == ValueType.Integer && lhs.Type(context) == ValueType.Integer) ||
                              (rhs.Type(context) == ValueType.String && lhs.Type(context) == ValueType.String)))
                            context.errors.Add(new SemanticError(Token, "bad operand types for '+' operation"));
                        break;
                    case '-':
                        if (!(rhs.Type(context) == ValueType.Integer && lhs.Type(context) == ValueType.Integer))
                            context.errors.Add(new SemanticError(Token, "bad operand types for '-' operation"));
                        break;
                    case '*':
                        if (!(rhs.Type(context) == ValueType.Integer && lhs.Type(context) == ValueType.Integer))
                            context.errors.Add(new SemanticError(Token, "bad operand types for '*' operation"));
                        break;
                    case '/':
                        if (!(rhs.Type(context) == ValueType.Integer && lhs.Type(context) == ValueType.Integer))
                            context.errors.Add(new SemanticError(Token, "bad operand types for '/' operation"));
                        break;
                    case '<':
                        if (!(rhs.Type(context) == ValueType.Integer && lhs.Type(context) == ValueType.Integer))
                            context.errors.Add(new SemanticError(Token, "bad operand types for '<' operation"));
                        break;
                    case '=':
                        if (!(rhs.Type(context) == lhs.Type(context)))
                            context.errors.Add(new SemanticError(Token, "bad operand types for '=' operation"));
                        break;
                    case '&':
                        if (rhs.Type(context) == ValueType.String || lhs.Type(context) == ValueType.String)
                            context.errors.Add(new SemanticError(Token, "bad operand types for '&' operation"));
                        break;
                }
            }
        }
    }
}
