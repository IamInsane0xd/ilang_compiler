namespace ILang.CodeAnalysis.Syntax;

public static class SyntacFacts
{
	public static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.PlusToken or SyntaxKind.MinusToken or SyntaxKind.BangToken or SyntaxKind.TildeToken => 6,
			_ => 0,
		};
	}

	public static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.StarToken or SyntaxKind.SlashToken => 5,
			SyntaxKind.PlusToken or SyntaxKind.MinusToken => 4,
			SyntaxKind.EqualsEqualsToken or
			SyntaxKind.BangEqualsToken or
			SyntaxKind.LessToken or
			SyntaxKind.LessOrEqualsToken or
			SyntaxKind.GreaterToken or
			SyntaxKind.GreaterOrEqualsToken => 3,
			SyntaxKind.AmpersandToken or SyntaxKind.AmpersandAmpersandToken => 2,
			SyntaxKind.PipeToken or SyntaxKind.PipePipeToken or SyntaxKind.CaretToken => 1,
			_ => 0,
		};
	}

	public static SyntaxKind GetKeywordKind(string text)
	{
		return text switch
		{
			"true" => SyntaxKind.TrueKeyword,
			"false" => SyntaxKind.FalseKeyword,
			"let" => SyntaxKind.LetKeyword,
			"var" => SyntaxKind.VarKeyword,
			"to" => SyntaxKind.ToKeyword,
			"if" => SyntaxKind.IfKeyword,
			"else" => SyntaxKind.ElseKeyword,
			"while" => SyntaxKind.WhileKeyword,
			"for" => SyntaxKind.ForKeyword,
			_ => SyntaxKind.IdentifierToken,
		};
	}

	public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds()
	{
		SyntaxKind[] kinds = (SyntaxKind[]) Enum.GetValues(typeof(SyntaxKind));

		foreach (SyntaxKind kind in kinds)
		{
			if (GetUnaryOperatorPrecedence(kind) > 0)
				yield return kind;
		}
	}

	public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds()
	{
		SyntaxKind[] kinds = (SyntaxKind[]) Enum.GetValues(typeof(SyntaxKind));

		foreach (SyntaxKind kind in kinds)
		{
			if (GetBinaryOperatorPrecedence(kind) > 0)
				yield return kind;
		}
	}

	public static string? GetText(SyntaxKind kind)
	{
		return kind switch
		{
			SyntaxKind.PlusToken => "+",
			SyntaxKind.MinusToken => "-",
			SyntaxKind.StarToken => "*",
			SyntaxKind.SlashToken => "/",
			SyntaxKind.BangToken => "!",
			SyntaxKind.EqualsToken => "=",
			SyntaxKind.TildeToken => "~",
			SyntaxKind.CaretToken => "^",
			SyntaxKind.AmpersandToken => "&",
			SyntaxKind.AmpersandAmpersandToken => "&&",
			SyntaxKind.PipeToken => "|",
			SyntaxKind.PipePipeToken => "||",
			SyntaxKind.EqualsEqualsToken => "==",
			SyntaxKind.BangEqualsToken => "!=",
			SyntaxKind.LessToken => "<",
			SyntaxKind.LessOrEqualsToken => "<=",
			SyntaxKind.GreaterToken => ">",
			SyntaxKind.GreaterOrEqualsToken => ">=",
			SyntaxKind.OpenParenthesisToken => "(",
			SyntaxKind.CloseParenthesisToken => ")",
			SyntaxKind.OpenBraceToken => "{",
			SyntaxKind.CloseBraceToken => "}",
			SyntaxKind.FalseKeyword => "false",
			SyntaxKind.TrueKeyword => "true",
			SyntaxKind.LetKeyword => "let",
			SyntaxKind.VarKeyword => "var",
			SyntaxKind.ToKeyword => "to",
			SyntaxKind.IfKeyword => "if",
			SyntaxKind.ElseKeyword => "else",
			SyntaxKind.WhileKeyword => "while",
			SyntaxKind.ForKeyword => "for",
			_ => null,
		};
	}
}
