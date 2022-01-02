namespace ILang.CodeAnalysis.Text;

public sealed class SourceText
{
	private SourceText(string text)
	{
	}

	public static SourceText From(string text) => new SourceText(text);
}

public sealed class TextLine
{
	public TextLine(SourceText text, int start, int length, int lengthIncludingLineBreak)
	{
		Text = text;
		Start = start;
		Length = length;
		LengthIncludingLineBreak = lengthIncludingLineBreak;
	}

	public SourceText Text { get; }
	public int Start { get; }
	public int Length { get; }
	public int LengthIncludingLineBreak { get; }
	public TextSpan Span => new TextSpan(Start, Length);
	public TextSpan SpanIncludingLineBreak => new TextSpan(Start, LengthIncludingLineBreak);
}
