using ILang.CodeAnalysis;
using ILang.CodeAnalysis.Syntax;
using System.Collections.Generic;
using Xunit;

namespace ILang.Tests.CodeAnalysis;

public class EvaluationTests
{
	[Theory]
	[InlineData("1", 1)]
	[InlineData("+1", 1)]
	[InlineData("-1", -1)]
	[InlineData("14 + 12", 26)]
	[InlineData("12 - 3", 9)]
	[InlineData("4 * 2", 8)]
	[InlineData("9 / 3", 3)]
	[InlineData("(1 + 2) * 3", 9)]
	[InlineData("(a = 10) * a", 100)]
	[InlineData("12 == 3", false)]
	[InlineData("2 == 2", true)]
	[InlineData("12 != 3", true)]
	[InlineData("2 != 2", false)]
	[InlineData("true == false", false)]
	[InlineData("false == false", true)]
	[InlineData("true != true", false)]
	[InlineData("true != false", true)]
	[InlineData("true", true)]
	[InlineData("!true", false)]
	[InlineData("false", false)]
	[InlineData("!false", true)]
	public void SyntaxFactGetTextRoundTrips(string text, object expectedValue)
	{
		SyntaxTree? syntaxTree = SyntaxTree.Parse(text);
		Compilation? compilation = new Compilation(syntaxTree);
		Dictionary<VariableSymbol, object?>? variables = new Dictionary<VariableSymbol, object?>();
		EvaluationResult? result = compilation.Evaluate(variables);

		Assert.Empty(result.Diagnostics);
		Assert.Equal(expectedValue, result.Value);
	}
}
