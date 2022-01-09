using ILang.CodeAnalysis.Text;

namespace ILang.CodeAnalysis.Syntax;

public sealed class SyntaxToken : SyntaxNode
{
	public SyntaxToken(SyntaxKind kind, int position, string? text, object? value)
	{
		Kind = kind;
		Position = position;
		Text = text ?? string.Empty;
		Value = value;
	}

	public override SyntaxKind Kind { get; }
	public int Position { get; }
	public string Text { get; }
	public object? Value { get; }
	public override TextSpan Span => new TextSpan(Position, Text?.Length ?? 0);
}
