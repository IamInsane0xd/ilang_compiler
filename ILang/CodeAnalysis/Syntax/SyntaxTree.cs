using ILang.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace ILang.CodeAnalysis.Syntax;

public sealed class SyntaxTree
{
	public SyntaxTree(SourceText text)
	{
		Parser parser = new Parser(text);
		CompilationUnitSyntax root = parser.ParseCompilationUnit();

		Text = text;
		Root = root;
		Diagnostics = parser.Diagnostics.ToImmutableArray();
	}

	public SourceText Text { get; }
	public CompilationUnitSyntax Root { get; }
	public ImmutableArray<Diagnostic> Diagnostics { get; }

	public static SyntaxTree Parse(string text)
	{
		SourceText sourceText = SourceText.From(text);
		return Parse(sourceText);
	}

	public static SyntaxTree Parse(SourceText text) => new SyntaxTree(text);

	public static IEnumerable<SyntaxToken> ParseTokens(string text)
	{
		SourceText sourceText = SourceText.From(text);
		return ParseTokens(sourceText);
	}

	public static IEnumerable<SyntaxToken> ParseTokens(SourceText text)
	{
		Lexer lexer = new Lexer(text);

		while (true)
		{
			SyntaxToken? token = lexer.Lex();

			if (token.Kind == SyntaxKind.EndOfFileToken)
				break;

			yield return token;
		}
	}
}
