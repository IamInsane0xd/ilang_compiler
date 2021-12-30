namespace ILang.Classes.Syntax
{
	public sealed class SyntaxToken : SyntaxNode
	{
		public SyntaxToken(SyntaxKind kind, int position, string? text, object? value)
		{
			Kind = kind;
			Position = position;
			Text = text;
			Value = value;
		}

		public override SyntaxKind Kind { get; }
		public int Position { get; }
		public string? Text { get; }
		public object? Value { get; }
		public TextSpan Span => Text != null ? new TextSpan(Position, Text.Length) : throw new Exception("Text is null");

		public override IEnumerable<SyntaxNode> GetChildren()
		{
			return Enumerable.Empty<SyntaxNode>();
		}
	}
}
