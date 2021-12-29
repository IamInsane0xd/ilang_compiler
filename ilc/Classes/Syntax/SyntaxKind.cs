namespace ILang.Classes.Syntax
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
		OpenParenthesisToken,
		CloseParenthesisToken,
		IdentifierToken,

		// Keywords
		TrueKeyword,
		FalseKeyword,

		// Expressions
		LiteralExpression,
		UnaryExpression,
		BinaryExpression,
		ParenthesizedExpression
	}
}
