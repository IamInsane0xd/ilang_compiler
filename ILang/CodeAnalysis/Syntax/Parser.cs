using ILang.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace ILang.CodeAnalysis.Syntax;

internal sealed class Parser
{
	private readonly SourceText _text;
	private readonly ImmutableArray<SyntaxToken> _tokens;
	private readonly DiagnosticBag _diagnostics = new DiagnosticBag();

	private int _position;

	public Parser(SourceText text)
	{
		Lexer lexer = new Lexer(text);
		List<SyntaxToken> tokens = new List<SyntaxToken>();
		SyntaxToken token;

		do
		{
			token = lexer.Lex();

			if (token.Kind != SyntaxKind.WhiteSpaceToken && token.Kind != SyntaxKind.BadToken)
				tokens.Add(token);

		} while (token.Kind != SyntaxKind.EndOfFileToken);

		_text = text;
		_tokens = tokens.ToImmutableArray();
		_diagnostics.AddRange(lexer.Diagnostics);
	}

	public DiagnosticBag Diagnostics => _diagnostics;

	private SyntaxToken Peek(int offset)
	{
		int index = _position + offset;

		if (index >= _tokens.Length)
			return _tokens[_tokens.Length - 1];

		return _tokens[index];
	}

	private SyntaxToken Current => Peek(0);

	private SyntaxToken NextToken()
	{
		SyntaxToken current = Current;
		_position++;
		return current;
	}

	private SyntaxToken MatchToken(SyntaxKind kind)
	{
		if (Current.Kind == kind)
			return NextToken();

		_diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
		return new SyntaxToken(kind, Current.Position, null, null);
	}

	public CompilationUnitSyntax ParseCompilationUnit()
	{
		StatementSyntax statement = ParseStatement();
		SyntaxToken endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
		return new CompilationUnitSyntax(statement, endOfFileToken);
	}

	private StatementSyntax ParseStatement()
	{
		switch (Current.Kind)
		{
			case SyntaxKind.OpenBraceToken:
				return ParseBlockStatement();

			case SyntaxKind.LetKeyword:
			case SyntaxKind.VarKeyword:
				return ParseVariableDeclaration();

			case SyntaxKind.IfKeyword:
				return ParseIfStatement();

			case SyntaxKind.WhileKeyword:
				return ParseWhileStatement();

			case SyntaxKind.ForKeyword:
				return ParseForStatement();

			default:
				return ParseExpressionStatement();
		}
	}

	private BlockStatementSyntax ParseBlockStatement()
	{
		ImmutableArray<StatementSyntax>.Builder statements = ImmutableArray.CreateBuilder<StatementSyntax>();

		SyntaxToken openBraceToken = MatchToken(SyntaxKind.OpenBraceToken);

		while (Current.Kind != SyntaxKind.EndOfFileToken && Current.Kind != SyntaxKind.CloseBraceToken)
		{
			SyntaxToken startToken = Current;
			StatementSyntax statement = ParseStatement();

			statements.Add(statement);

			if (Current == startToken)
				NextToken();
		}

		SyntaxToken closeBraceToken = MatchToken(SyntaxKind.CloseBraceToken);

		return new BlockStatementSyntax(openBraceToken, statements.ToImmutable(), closeBraceToken);
	}

	private StatementSyntax ParseVariableDeclaration()
	{
		SyntaxKind expected = Current.Kind == SyntaxKind.LetKeyword ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword;
		SyntaxToken keyword = MatchToken(expected);
		SyntaxToken identifier = MatchToken(SyntaxKind.IdentifierToken);
		SyntaxToken equals = MatchToken(SyntaxKind.EqualsToken);
		ExpressionSyntax initializer = ParseExpression();

		return new VariableDeclarationSyntax(keyword, identifier, equals, initializer);
	}

	private StatementSyntax ParseIfStatement()
	{
		SyntaxToken keyword = MatchToken(SyntaxKind.IfKeyword);
		ExpressionSyntax condition = ParseExpression();
		StatementSyntax statement = ParseStatement();
		ElseClauseSyntax? elseClause = ParseElseClause();

		return new IfStatementSyntax(keyword, condition, statement, elseClause);
	}

	private ElseClauseSyntax? ParseElseClause()
	{
		if (Current.Kind != SyntaxKind.ElseKeyword)
			return null;

		SyntaxToken keyword = NextToken();
		StatementSyntax statement = ParseStatement();

		return new ElseClauseSyntax(keyword, statement);
	}

	private StatementSyntax ParseWhileStatement()
	{
		SyntaxToken keyword = MatchToken(SyntaxKind.WhileKeyword);
		ExpressionSyntax condition = ParseExpression();
		StatementSyntax body = ParseStatement();

		return new WhileStatementSyntax(keyword, condition, body);
	}

	private StatementSyntax ParseForStatement()
	{
		SyntaxToken keyword = MatchToken(SyntaxKind.ForKeyword);
		SyntaxToken identifier = MatchToken(SyntaxKind.IdentifierToken);
		SyntaxToken equalsToken = MatchToken(SyntaxKind.EqualsToken);
		ExpressionSyntax lowerBound = ParseExpression();
		SyntaxToken toKeyword = MatchToken(SyntaxKind.ToKeyword);
		ExpressionSyntax upperBound = ParseExpression();
		StatementSyntax body = ParseStatement();

		return new ForStatementSyntax(keyword, identifier, equalsToken, lowerBound, toKeyword, upperBound, body);
	}

	private ExpressionStatementSyntax ParseExpressionStatement()
	{
		var expression = ParseExpression();
		return new ExpressionStatementSyntax(expression);
	}

	private ExpressionSyntax ParseExpression() => ParseAssignmentExpression();

	private ExpressionSyntax ParseAssignmentExpression()
	{
		if (Current.Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.EqualsToken)
		{
			SyntaxToken identifierToken = NextToken();
			SyntaxToken operatorToken = NextToken();
			ExpressionSyntax right = ParseAssignmentExpression();
			return new AssignmentExpressionSyntax(identifierToken, operatorToken, right);
		}

		return ParseBinaryExpression();
	}

	private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0)
	{
		ExpressionSyntax left;
		int unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();

		if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
		{
			SyntaxToken operatorToken = NextToken();
			ExpressionSyntax operand = ParseBinaryExpression(unaryOperatorPrecedence);

			left = new UnaryExpressionSyntax(operatorToken, operand);
		}

		else
		{
			left = ParsePrimaryExpression();
		}

		while (true)
		{
			int precedence = Current.Kind.GetBinaryOperatorPrecedence();

			if (precedence == 0 || precedence <= parentPrecedence)
				break;

			SyntaxToken operatorToken = NextToken();
			ExpressionSyntax right = ParseBinaryExpression(precedence);

			left = new BinaryExpressionSyntax(left, operatorToken, right);
		}

		return left;
	}

	private ExpressionSyntax ParsePrimaryExpression()
	{
		switch (Current.Kind)
		{
			case SyntaxKind.OpenParenthesisToken:
				return ParseParenthesizedExpression();

			case SyntaxKind.TrueKeyword:
			case SyntaxKind.FalseKeyword:
				return ParseBooleanLiteral();

			case SyntaxKind.NumberToken:
				return ParseNumberLiteral();

			default:
				return ParseNameExpression();
		}
	}

	private ExpressionSyntax ParseParenthesizedExpression()
	{
		SyntaxToken left = MatchToken(SyntaxKind.OpenParenthesisToken);
		ExpressionSyntax expression = ParseExpression();
		SyntaxToken right = MatchToken(SyntaxKind.CloseParenthesisToken);
		return new ParenthesizedExpressionSyntax(left, expression, right);
	}

	private ExpressionSyntax ParseBooleanLiteral()
	{
		bool isTrue = Current.Kind == SyntaxKind.TrueKeyword;
		SyntaxToken keywordToken = isTrue ? MatchToken(SyntaxKind.TrueKeyword) : MatchToken(SyntaxKind.FalseKeyword);
		return new LiteralExpressionSyntax(keywordToken, isTrue);
	}

	private ExpressionSyntax ParseNumberLiteral()
	{
		SyntaxToken numberToken = MatchToken(SyntaxKind.NumberToken);
		return new LiteralExpressionSyntax(numberToken);
	}

	private ExpressionSyntax ParseNameExpression()
	{
		SyntaxToken identifierToken = MatchToken(SyntaxKind.IdentifierToken);
		return new NameExpressionSyntax(identifierToken);
	}
}
