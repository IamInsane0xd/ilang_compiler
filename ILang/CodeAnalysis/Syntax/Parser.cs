using System.Collections.Immutable;

namespace ILang.CodeAnalysis.Syntax;

internal sealed class Parser
{
	private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
	private readonly ImmutableArray<SyntaxToken> _tokens;
	private int _position;

	public Parser(string text)
	{
		Lexer? lexer = new Lexer(text);
		List<SyntaxToken>? tokens = new List<SyntaxToken>();
		SyntaxToken token;

		do
		{
			token = lexer.Lex();

			if (token.Kind != SyntaxKind.WhiteSpaceToken && token.Kind != SyntaxKind.BadToken)
				tokens.Add(token);

		} while (token.Kind != SyntaxKind.EndOfFileToken);

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
		SyntaxToken? current = Current;
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

	public SyntaxTree Parse()
	{
		ExpressionSyntax? expression = ParseExpression();
		SyntaxToken? endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
		return new SyntaxTree(_diagnostics.ToImmutableArray(), expression, endOfFileToken);
	}

	private ExpressionSyntax ParseExpression() => ParseAssignmentExpression();

	private ExpressionSyntax ParseAssignmentExpression()
	{
		if (Current.Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.EqualsToken)
		{
			SyntaxToken? identifierToken = NextToken();
			SyntaxToken? operatorToken = NextToken();
			ExpressionSyntax? right = ParseAssignmentExpression();
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
			SyntaxToken? operatorToken = NextToken();
			ExpressionSyntax? operand = ParseBinaryExpression(unaryOperatorPrecedence);

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

			SyntaxToken? operatorToken = NextToken();
			ExpressionSyntax? right = ParseBinaryExpression(precedence);

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
		SyntaxToken? left = MatchToken(SyntaxKind.OpenParenthesisToken);
		ExpressionSyntax? expression = ParseExpression();
		SyntaxToken? right = MatchToken(SyntaxKind.CloseParenthesisToken);
		return new ParenthesizedExpressionSyntax(left, expression, right);
	}

	private ExpressionSyntax ParseBooleanLiteral()
	{
		bool isTrue = Current.Kind == SyntaxKind.TrueKeyword;
		SyntaxToken? keywordToken = isTrue ? MatchToken(SyntaxKind.TrueKeyword) : MatchToken(SyntaxKind.FalseKeyword);
		return new LiteralExpressionSyntax(keywordToken, isTrue);
	}

	private ExpressionSyntax ParseNumberLiteral()
	{
		SyntaxToken? numberToken = MatchToken(SyntaxKind.NumberToken);
		return new LiteralExpressionSyntax(numberToken);
	}

	private ExpressionSyntax ParseNameExpression()
	{
		SyntaxToken? identifierToken = MatchToken(SyntaxKind.IdentifierToken);
		return new NameExpressionSyntax(identifierToken);
	}
}
