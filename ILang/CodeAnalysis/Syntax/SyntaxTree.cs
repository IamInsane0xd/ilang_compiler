using System.Collections.Immutable;

namespace ILang.CodeAnalysis.Syntax;

public sealed class SyntaxTree
{
	public SyntaxTree(ImmutableArray<Diagnostic> diagnostics, ExpressionSyntax root, SyntaxToken endOfFileToken)
	{
		Diagnostics = diagnostics;
		Root = root;
		EndOfFileToken = endOfFileToken;
	}

	public ImmutableArray<Diagnostic> Diagnostics { get; }
	public ExpressionSyntax Root { get; }
	public SyntaxToken EndOfFileToken { get; }

	public static SyntaxTree Parse(string text)
	{
		Parser? parser = new Parser(text);
		return parser.Parse();
	}

	public static IEnumerable<SyntaxToken> ParseTokens(string text)
	{
		Lexer? lexer = new Lexer(text);

		while (true)
		{
			SyntaxToken? token = lexer.Lex();

			if (token.Kind == SyntaxKind.EndOfFileToken)
				break;

			yield return token;
		}
	}
}
