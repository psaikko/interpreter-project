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
    public abstract class Expression
    {
        public Token token;
        public abstract Value Evaluate(MiniPL.Runnable context);
        public abstract ValueType Type(MiniPL.Runnable context);
        public abstract void TypeCheck(MiniPL.Runnable context);

        Expression(Token token) { this.token = token; }

        public static Expression FromTreeNode(INode<Parser.IParseValue> ASTNode,
            Dictionary<String, Terminal> terms,
            Dictionary<String, Nonterminal> vars)
        {
            if (ASTNode is Tree<Parser.IParseValue>)
            {
                Tree<Parser.IParseValue> subtree = ASTNode as Tree<Parser.IParseValue>;
                Parser.IParseValue val = subtree.GetValue();
                Parser.NonterminalValue nt = val as Parser.NonterminalValue;
                if (nt == null) throw new Exception("EXPECTED NONTERMINAL NODE");

                if (nt.var != vars["expression"]) throw new Exception("EXPECTED EXPRESSION NODE");

                switch (subtree.children.Count)
                {
                    case 1:
                        INode<Parser.IParseValue> child = subtree.children[0];
                        return ExprFromLeaf(child as Leaf<Parser.IParseValue>);
                    case 2:
                        {
                            INode<Parser.IParseValue> op = subtree.children[0];
                            Leaf<Parser.IParseValue> opLeaf = op as Leaf<Parser.IParseValue>;
                            if (opLeaf == null) throw new Exception("MALFORMED AST");
                            Parser.TerminalValue opTerm = opLeaf.GetValue() as Parser.TerminalValue;
                            if (opTerm == null) throw new Exception("MALFORMED AST");

                            INode<Parser.IParseValue> opnd = subtree.children[1];
                            Expression baseExpr;
                            if (opnd is Leaf<Parser.IParseValue>)
                                baseExpr = ExprFromLeaf(opnd as Leaf<Parser.IParseValue>);
                            else
                                baseExpr = FromTreeNode(opnd as Tree<Parser.IParseValue>, terms, vars);
                            return new UnaryOp(opTerm.token.lexeme[0], baseExpr, opTerm.token);
                        }
                    case 3:
                        {
                            INode<Parser.IParseValue> op = subtree.children[1];
                            Leaf<Parser.IParseValue> opLeaf = op as Leaf<Parser.IParseValue>;
                            if (opLeaf == null) throw new Exception("MALFORMED AST");
                            Parser.TerminalValue opTerm = opLeaf.GetValue() as Parser.TerminalValue;
                            if (opTerm == null) throw new Exception("MALFORMED AST");

                            INode<Parser.IParseValue> lhs = subtree.children[0];
                            INode<Parser.IParseValue> rhs = subtree.children[2];
                            Expression lhsExpr, rhsExpr;
                            if (lhs is Leaf<Parser.IParseValue>) lhsExpr = ExprFromLeaf(lhs as Leaf<Parser.IParseValue>);
                            else lhsExpr = FromTreeNode(lhs as Tree<Parser.IParseValue>, terms, vars);
                            if (rhs is Leaf<Parser.IParseValue>) rhsExpr = ExprFromLeaf(rhs as Leaf<Parser.IParseValue>);
                            else rhsExpr = FromTreeNode(rhs as Tree<Parser.IParseValue>, terms, vars);
                            return new BinaryOp(lhsExpr, opTerm.token.lexeme[0], rhsExpr, opTerm.token);
                        }
                    default:
                        throw new Exception("MALFORMED AST");
                }
            }
            else throw new Exception("EXPECTED LEAF NODE");
        }

        private static Expression ExprFromLeaf(Leaf<Parser.IParseValue> leaf)
        {
            if (leaf == null) throw new Exception("MALFORMED AST");
            Parser.TerminalValue term = leaf.GetValue() as Parser.TerminalValue;
            if (term == null) throw new Exception("MALFORMED AST");
            if (term.token.tokenType.name == "identifier")
                return new IdentifierExpr(term.token.lexeme, term.token);
            else
                return new ValueExpr(new Value(term.token.tokenType.name, term.token.lexeme), term.token);
        }

        public class ValueExpr : Expression
        {
            public Value value;

            public ValueExpr(Value value, Token token) : base(token) 
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

        public class IdentifierExpr : Expression
        {
            public string identifier;

            public IdentifierExpr(string identifier, Token token) : base(token)  
            { 
                this.identifier = identifier;
            }

            override public Value Evaluate(MiniPL.Runnable context)
            {
                return context.values[identifier];
            }

            override public ValueType Type(MiniPL.Runnable context)
            {
                return context.declarations[identifier].type;
            }

            override public void TypeCheck(MiniPL.Runnable context)
            {
                // nothing to check
            }
        }

        public class UnaryOp : Expression
        {
            public char op;
            public Expression expr;

            public UnaryOp(char op, Expression expr, Token token) : base(token) 
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
                    context.errors.Add(new SemanticError(token, "bad type for '!' operation"));
            }
        }

        public class BinaryOp : Expression
        {
            public Expression lhs;
            public char op;
            public Expression rhs;

            public BinaryOp(Expression lhs, char op, Expression rhs, Token token) : base(token) 
            {
                this.op = op; 
                this.lhs = lhs;
                this.rhs = rhs;
            }

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
                        return new Value(lhs.Evaluate(context).IntValue() / rhs.Evaluate(context).IntValue());
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

            override public void TypeCheck(MiniPL.Runnable context)
            {
                rhs.TypeCheck(context);
                lhs.TypeCheck(context);
                switch (op)
                {
                    case '+':
                        if (!((rhs.Type(context) == ValueType.Integer && lhs.Type(context) == ValueType.Integer) ||
                              (rhs.Type(context) == ValueType.String && lhs.Type(context) == ValueType.String)))
                            context.errors.Add(new SemanticError(token, "bad operand types for '+' operation"));
                        break;
                    case '-':
                        if (!(rhs.Type(context) == ValueType.Integer && lhs.Type(context) == ValueType.Integer))
                            context.errors.Add(new SemanticError(token, "bad operand types for '-' operation"));
                        break;
                    case '*':
                        if (!(rhs.Type(context) == ValueType.Integer && lhs.Type(context) == ValueType.Integer))
                            context.errors.Add(new SemanticError(token, "bad operand types for '*' operation"));
                        break;
                    case '/':
                        if (!(rhs.Type(context) == ValueType.Integer && lhs.Type(context) == ValueType.Integer))
                            context.errors.Add(new SemanticError(token, "bad operand types for '/' operation"));
                        break;
                    case '<':
                        if (!(rhs.Type(context) == ValueType.Integer && lhs.Type(context) == ValueType.Integer))
                            context.errors.Add(new SemanticError(token, "bad operand types for '<' operation"));
                        break;
                    case '=':
                        if (!(rhs.Type(context) == lhs.Type(context)))
                            context.errors.Add(new SemanticError(token, "bad operand types for '=' operation"));
                        break;
                    case '&':
                        if (rhs.Type(context) == ValueType.String || lhs.Type(context) == ValueType.String)
                            context.errors.Add(new SemanticError(token, "bad operand types for '&' operation"));
                        break;
                }
            }
        }
    }
}
