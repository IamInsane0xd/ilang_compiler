using ILang.CodeAnalysis;
using ILang.CodeAnalysis.Syntax;
using ILang.CodeAnalysis.Text;
using System;
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
	[InlineData("12 == 3", false)]
	[InlineData("2 == 2", true)]
	[InlineData("12 != 3", true)]
	[InlineData("2 != 2", false)]
	[InlineData("3 < 4", true)]
	[InlineData("5 < 4", false)]
	[InlineData("4 <= 4", true)]
	[InlineData("4 <= 5", true)]
	[InlineData("5 <= 4", false)]
	[InlineData("3 > 4", false)]
	[InlineData("5 > 4", true)]
	[InlineData("4 >= 4", true)]
	[InlineData("4 >= 5", false)]
	[InlineData("5 >= 4", true)]
	[InlineData("true == false", false)]
	[InlineData("false == false", true)]
	[InlineData("true != true", false)]
	[InlineData("true != false", true)]
	[InlineData("true", true)]
	[InlineData("!true", false)]
	[InlineData("false", false)]
	[InlineData("!false", true)]
	[InlineData("{ var a = 0 (a = 10) * a }", 100)]
	public void EvaluatorComputesCorrectValues(string text, object expectedValue)
	{
		AssertValue(text, expectedValue);
	}

	[Fact]
	public void EvaluatorVariableDeclarationReportsRedeclaration()
	{
		var text =
		@"
			{
				var x = 10
				var y = 100

				{
					var x = 10
				}

				var [x] = 5
			}
		";

		var diagnostics =
		@"
			Error: Variable 'x' is already declared
		";

		AssertDiagnostics(text, diagnostics);
	}

	[Fact]
	public void EvaluatorNameReportsUndefined()
	{
		var text =
		@"
			[x] + 1
		";

		var diagnostics =
		@"
			Error: Variable 'x' is not defined
		";

		AssertDiagnostics(text, diagnostics);
	}

	[Fact]
	public void EvaluatorAssignmentReportsUndefined()
	{
		var text =
		@"
			[x] = 1
		";

		var diagnostics =
		@"
			Error: Variable 'x' is not defined
		";

		AssertDiagnostics(text, diagnostics);
	}

	[Fact]
	public void EvaluatorAssignmentReportsCannotBeAssigned()
	{
		var text =
		@"
			{
				let x = 1
				x [=] 0
			}
		";

		var diagnostics =
		@"
			Error: Variable 'x' is read-only and cannot be assigned to
		";

		AssertDiagnostics(text, diagnostics);
	}

	[Fact]
	public void EvaluatorAssignmentReportsCannotConvert()
	{
		var text =
		@"
			{
				var x = 1
				x = [true]
			}
		";

		var diagnostics =
		@"
			Error: Cannot convert type System.Boolean to System.Int32
		";

		AssertDiagnostics(text, diagnostics);
	}

	[Fact]
	public void EvaluatorUnaryReportsUndefined()
	{
		var text =
		@"
			[-]true
		";

		var diagnostics =
		@"
			Error: Unary operator '-' is not defined for type System.Boolean
		";

		AssertDiagnostics(text, diagnostics);
	}

	[Fact]
	public void EvaluatorBinaryReportsUndefined()
	{
		var text =
		@"
			1 [+] true
		";

		var diagnostics =
		@"
			Error: Binary operator '+' is not defined for types System.Int32 and System.Boolean
		";

		AssertDiagnostics(text, diagnostics);
	}

	private static void AssertValue(string text, object expectedValue)
	{
		SyntaxTree syntaxTree = SyntaxTree.Parse(text);
		Compilation compilation = new Compilation(syntaxTree);
		Dictionary<VariableSymbol, object?> variables = new Dictionary<VariableSymbol, object?>();
		EvaluationResult? result = compilation.Evaluate(variables);

		Assert.Empty(result.Diagnostics);
		Assert.Equal(expectedValue, result.Value);
	}

	private void AssertDiagnostics(string text, string diagnosticText)
	{
		AnnotatedText annotatedText = AnnotatedText.Parse(text);
		SyntaxTree syntaxTree = SyntaxTree.Parse(annotatedText.Text);
		Compilation compilation = new Compilation(syntaxTree);
		EvaluationResult result = compilation.Evaluate(new Dictionary<VariableSymbol, object?>());
		string[] expectedDiagnostics = AnnotatedText.UnindentLines(diagnosticText);

		if (annotatedText.Spans.Length != expectedDiagnostics.Length)
			throw new Exception("Error: Must mark as many spans as there are expected diagnostics");

		Assert.Equal(expectedDiagnostics.Length, result.Diagnostics.Length);

		for (int i = 0; i < expectedDiagnostics.Length; i++)
		{
			string expectedMessage = expectedDiagnostics[i];
			string actualMessage = result.Diagnostics[i].Message;

			Assert.Equal(expectedMessage, actualMessage);

			TextSpan expectedSpan = annotatedText.Spans[i];
			TextSpan actualSpan = result.Diagnostics[i].Span;

			Assert.Equal(expectedSpan, actualSpan);
		}
	}
}
