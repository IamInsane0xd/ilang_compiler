namespace ILang.CodeAnalysis.Syntax
{
	public enum SyntaxKind
	{
		// Tokens
		BadToken,
		EndOfFileToken,
		WhiteSpaceToken,
		NumberToken,
		PlusToken,
		MinusToken,
		StarToken,
		SlashToken,
		BangToken,
		EqualsToken,
		AmpersandAmpersandToken,
		PipePipeToken,
		EqualsEqualsToken,
		BangEqualsToken,
		OpenParenthesisToken,
		CloseParenthesisToken,
		IdentifierToken,

		// Keywords
		TrueKeyword,
		FalseKeyword,

		// Expressions
		LiteralExpression,
		NameExpression,
		UnaryExpression,
		BinaryExpression,
		ParenthesizedExpression,
		AssignmentExpression,
	}
}
