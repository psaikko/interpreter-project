using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    class Program
    {
        static void Main(string[] args)
        {
            // Define token regexes
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
            TokenType ttInteger = new TokenType("integer", reInteger),
                      ttWhitespace = new TokenType("whitespace", reWhitespace, priority: TokenType.Priority.Whitespace),
                      ttString = new TokenType("string", reString),
                      ttBinaryOperator = new TokenType("binary op", reBinaryOperator),
                      ttUnaryOperator = new TokenType("unary op", reUnaryOperator),
                      ttKeyword = new TokenType("keyword", reKeyword, priority: TokenType.Priority.Keyword),
                      ttType = new TokenType("type", reType, priority: TokenType.Priority.Keyword),
                      ttParenLeft = new TokenType("left paren", reParenLeft),
                      ttParenRight = new TokenType("right paren", reParenRight),
                      ttColon = new TokenType("colon", reColon),
                      ttSemicolon = new TokenType("semicolon", reSemicolon),
                      ttAssignment = new TokenType("assignment", reAssignment),
                      ttDots = new TokenType("dots", reDots),
                      ttIdentifier = new TokenType("identifier", reIdentifier);

            // Construct automaton
            TokenAutomaton automaton = TokenType.CombinedAutomaton(
                ttWhitespace, ttString, ttBinaryOperator, ttUnaryOperator, ttKeyword, ttType, ttParenRight, ttParenLeft,
                ttColon, ttSemicolon, ttAssignment, ttDots, ttIdentifier, ttInteger);
            Scanner sc = new Scanner(automaton);

            // Define nonterminal variables of CFG
            CFG.Variable varProgram = new CFG.Variable("PROG"),
                         varStatements = new CFG.Variable("STMTS"),
                         varStatementsHead = new CFG.Variable("STMTS_HEAD"),
                         varStatementsTail = new CFG.Variable("STMTS_TAIL"),
                         varStatement = new CFG.Variable("STMT"),
                         varDeclaration = new CFG.Variable("DECL"),
                         varDeclarationAssign = new CFG.Variable("DECL_ASSIGN"),
                         varExpression = new CFG.Variable("EXPR"),
                         varUnaryOperation = new CFG.Variable("UNARY_OP"),
                         varBinaryOperation = new CFG.Variable("BINARY_OP"),
                         varOperand = new CFG.Variable("OPND");

            // Define terminal variables of CFG
            CFG.Terminal termIdentifier = new CFG.Terminal(ttIdentifier),
                         termAssert = new CFG.Terminal("assert"),
                         termPrint = new CFG.Terminal("print"),
                         termRead = new CFG.Terminal("read"),
                         termFor = new CFG.Terminal("for"),
                         termIn = new CFG.Terminal("in"),
                         termEnd = new CFG.Terminal("end"),
                         termDo = new CFG.Terminal("do"),
                         termVar = new CFG.Terminal("var"),
                         termType = new CFG.Terminal(ttType),
                         termString = new CFG.Terminal(ttString),
                         termInteger = new CFG.Terminal(ttInteger),
                         termParenRight = new CFG.Terminal(")"),
                         termParenLeft = new CFG.Terminal("("),
                         termDots = new CFG.Terminal(".."),
                         termAssignment = new CFG.Terminal(":="),
                         termColon = new CFG.Terminal(":"),
                         termSemicolon = new CFG.Terminal(";"),
                         termBinaryOperator = new CFG.Terminal(ttBinaryOperator),
                         termUnaryOperator = new CFG.Terminal(ttUnaryOperator);
            CFG.Terminal[] terminals = { termIdentifier, termAssert, termPrint, termRead, termFor, termIn, termEnd,
                                         termDo, termVar, termType, termString, termInteger, termParenRight, termParenLeft,
                                         termDots, termAssignment, termColon, termSemicolon, termBinaryOperator, termUnaryOperator};

            CFG miniPLGrammar = new CFG(varProgram, terminals);

            miniPLGrammar.AddProductionRule(varProgram, new CFG.ISymbol[] { varStatements });
            miniPLGrammar.AddProductionRule(varStatements, new CFG.ISymbol[] { varStatementsHead, varStatementsTail });
            miniPLGrammar.AddProductionRule(varStatementsHead, new CFG.ISymbol[] { varStatement, termSemicolon });
            miniPLGrammar.AddProductionRule(varStatementsTail, new CFG.ISymbol[] { varStatementsHead, varStatementsTail });
            miniPLGrammar.AddProductionRule(varStatementsTail, new CFG.ISymbol[] { CFG.Terminal.epsilon });

            miniPLGrammar.AddProductionRule(varStatement, new CFG.ISymbol[] { varDeclaration });
            miniPLGrammar.AddProductionRule(varStatement, new CFG.ISymbol[] { termIdentifier, termAssignment, varExpression });
            miniPLGrammar.AddProductionRule(varStatement, new CFG.ISymbol[] { termFor, termIdentifier, termIn, varExpression, termDots, varExpression, termDo,
                                                                              varStatements, termEnd, termFor });
            miniPLGrammar.AddProductionRule(varStatement, new CFG.ISymbol[] { termRead, termIdentifier });
            miniPLGrammar.AddProductionRule(varStatement, new CFG.ISymbol[] { termPrint, varExpression });
            miniPLGrammar.AddProductionRule(varStatement, new CFG.ISymbol[] { termAssert, termParenLeft, varExpression, termParenRight });

            miniPLGrammar.AddProductionRule(varDeclaration, new CFG.ISymbol[] { termVar, termIdentifier, termColon, termType, varDeclarationAssign });
            miniPLGrammar.AddProductionRule(varDeclarationAssign, new CFG.ISymbol[] { termAssignment, varExpression });
            miniPLGrammar.AddProductionRule(varDeclarationAssign, new CFG.ISymbol[] { CFG.Terminal.epsilon });

            miniPLGrammar.AddProductionRule(varExpression, new CFG.ISymbol[] { varUnaryOperation });
            miniPLGrammar.AddProductionRule(varExpression, new CFG.ISymbol[] { varOperand, varBinaryOperation });

            miniPLGrammar.AddProductionRule(varUnaryOperation, new CFG.ISymbol[] { termUnaryOperator, varOperand });

            miniPLGrammar.AddProductionRule(varBinaryOperation, new CFG.ISymbol[] { termBinaryOperator, varOperand });
            miniPLGrammar.AddProductionRule(varBinaryOperation, new CFG.ISymbol[] { CFG.Terminal.epsilon });

            miniPLGrammar.AddProductionRule(varOperand, new CFG.ISymbol[] { termInteger });
            miniPLGrammar.AddProductionRule(varOperand, new CFG.ISymbol[] { termString });
            miniPLGrammar.AddProductionRule(varOperand, new CFG.ISymbol[] { termIdentifier });
            miniPLGrammar.AddProductionRule(varOperand, new CFG.ISymbol[] { termParenLeft, varExpression, termParenRight });

            miniPLGrammar.CreateLL1ParseTable();

            Console.ReadLine();
        }
    }
}
