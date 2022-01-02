using System.Collections.Immutable;

namespace ILang.CodeAnalysis.Text;

public sealed class SourceText
{
	private readonly string _text;

	private SourceText(string text)
	{
		_text = text;
		Lines = ParseLines(this, _text);
	}

	public ImmutableArray<TextLine> Lines { get; }

	public char this[int index] => _text[index];

	public int Length => _text.Length;

	public int GetLineIndex(int position)
	{
		int lower = 0;
		int upper = Lines.Length - 1;

		while (lower <= upper)
		{
			int index = lower + (upper - lower) / 2;
			int start = Lines[index].Start;

			if (position == start)
				return index;

			if (position < start)
				upper = index - 1;

			else
				lower = index + 1;
		}

		return lower - 1;
	}

	private ImmutableArray<TextLine> ParseLines(SourceText sourceText, string text)
	{
		ImmutableArray<TextLine>.Builder result = ImmutableArray.CreateBuilder<TextLine>();
		int position = 0;
		int lineStart = 0;

		while (position < text.Length)
		{
			int lineBreakWidth = GetLineBreakWidth(text, position);

			if (lineBreakWidth == 0)
				position++;

			else
			{
				AddLine(result, sourceText, position, lineStart, lineBreakWidth);

				position += lineBreakWidth;
				lineStart = position;
			}
		}

		if (position > lineStart)
			AddLine(result, sourceText, position, lineStart, 0);

		return result.ToImmutable();
	}

	private static void AddLine(ImmutableArray<TextLine>.Builder result, SourceText sourceText, int position, int lineStart, int lineBreakWidth)
	{
		int lineLength = position - lineStart;
		int lineLengthIncludingLineBreak = lineLength + lineBreakWidth;
		TextLine line = new TextLine(sourceText, lineStart, lineLength, lineLengthIncludingLineBreak);
		result.Add(line);
	}

	private int GetLineBreakWidth(string text, int i)
	{
		char c = text[i];
		char l = i + 1 >= text.Length ? '\0' : text[i + 1];

		if (c == '\r' && l == '\n')
			return 2;

		if (c == '\r' || c == '\n')
			return 1;

		return 0;
	}

	public static SourceText From(string text) => new SourceText(text);

	public override string ToString() => _text;

	public string ToString(int start, int length) => _text[start..(start + length)];

	public string ToString(TextSpan span) => ToString(span.Start, span.Length);
}
