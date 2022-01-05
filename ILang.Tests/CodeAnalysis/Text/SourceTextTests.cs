using ILang.CodeAnalysis.Text;
using Xunit;

namespace ILang.Tests.CodeAnalysis.Syntax;

public class SourceTextTests
{
	[Theory]
	[InlineData(".", 1)]
	[InlineData(".\r\n", 2)]
	[InlineData(".\r\n\r\n", 3)]
	public void SourceTextIncludesLastLine(string text, int expectedLineCound)
	{
		SourceText sourceText = SourceText.From(text);
		Assert.Equal(expectedLineCound, sourceText.Lines.Length);
	}
}
