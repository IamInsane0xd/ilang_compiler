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
	[InlineData("~1",-2)]
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
	[InlineData("1 | 2", 3)]
	[InlineData("1 | 0", 1)]
	[InlineData("1 & 3", 1)]
	[InlineData("1 & 0", 0)]
	[InlineData("1 ^ 0", 1)]
	[InlineData("0 ^ 1", 1)]
	[InlineData("1 ^ 3", 2)]
	[InlineData("true == false", false)]
	[InlineData("false == false", true)]
	[InlineData("true != true", false)]
	[InlineData("true != false", true)]
	[InlineData("true && false", false)]
	[InlineData("true || false", true)]
	[InlineData("false | false", false)]
	[InlineData("false | true", true)]
	[InlineData("true | false", true)]
	[InlineData("true | true", true)]
	[InlineData("false & false", false)]
	[InlineData("false & true", false)]
	[InlineData("true & false", false)]
	[InlineData("true & true", true)]
	[InlineData("false ^ false", false)]
	[InlineData("false ^ true", true)]
	[InlineData("true ^ false", true)]
	[InlineData("true ^ true", false)]
	[InlineData("true", true)]
	[InlineData("!true", false)]
	[InlineData("false", false)]
	[InlineData("!false", true)]
	[InlineData("var a = 10", 10)]
	[InlineData("{ var a = 10 (a * a) }", 100)]
	[InlineData("{ var a = 0 (a = 10) * a }", 100)]
	[InlineData("{ var a = 0 if a == 0 a = 10 a }", 10)]
	[InlineData("{ var a = 0 if a == 4 a = 10 a }", 0)]
	[InlineData("{ var a = 0 if a == 0 a = 10 else a = 5 a }", 10)]
	[InlineData("{ var a = 0 if a == 4 a = 10 else a = 5 a }", 5)]
	[InlineData("{ var i = 10 var result = 0 while i > 0 { result = result + i i = i - 1 } result }", 55)]
	[InlineData("{ var result = 0 for i = 1 to 10 { result = result + i } result }", 55)]
	[InlineData("{ var a = 10 for i = 1 to (a = a - 1) { } a }", 9)]
	public void EvaluatorComputesCorrectValues(string text, object expectedValue) => AssertValue(text, expectedValue);

	[Fact]
	public void EvaluatorVariableDeclarationReportsRedeclaration()
	{
		string text =
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

		string diagnostics =
		@"
			Error: Variable 'x' is already declared
		";

		AssertDiagnostics(text, diagnostics);
	}

	[Fact]
	public void EvaluatorBlockStatementNoInfiniteLoop()
	{
		string text =
		@"
			{
			[)][]
		";

		string diagnostics =
		@"
			Error: Unexpected token <CloseParenthesisToken>, expected <IdentifierToken>
			Error: Unexpected token <EndOfFileToken>, expected <CloseBraceToken>
		";

		AssertDiagnostics(text, diagnostics);
	}

	[Fact]
	public void EvaluatorIfStatementReportsCannotConvert()
	{
		string text =
		@"
			{
				var x = 0

				if [10]
				  x = 10
			}
		";

		string diagnostics =
		@"
			Error: Cannot convert type System.Int32 to System.Boolean
		";

		AssertDiagnostics(text, diagnostics);
	}

	[Fact]
	public void EvaluatorWhileStatementReportsCannotConvert()
	{
		string text =
		@"
			{
				var x = 0

				while [10]
				  x = 10
			}
		";

		string diagnostics =
		@"
			Error: Cannot convert type System.Int32 to System.Boolean
		";

		AssertDiagnostics(text, diagnostics);
	}

	[Fact]
	public void EvaluatorForStatementReportsCannotConvertLowerBound()
	{
		string text =
		@"
			{
				var result = 0

				for i = [false] to 10
				  result = result + i
			}
		";

		string diagnostics =
		@"
			Error: Cannot convert type System.Boolean to System.Int32
		";

		AssertDiagnostics(text, diagnostics);
	}

	[Fact]
	public void EvaluatorForStatementReportsCannotConvertUpperBound()
	{
		string text =
		@"
			{
				var result = 0

				for i = 1 to [false]
				  result = result + i
			}
		";

		string diagnostics =
		@"
			Error: Cannot convert type System.Boolean to System.Int32
		";

		AssertDiagnostics(text, diagnostics);
	}

	[Fact]
	public void EvaluatorNameExpressionReportsUndefined()
	{
		string text =
		@"
			[x] + 1
		";

		string diagnostics =
		@"
			Error: Variable 'x' is not defined
		";

		AssertDiagnostics(text, diagnostics);
	}

	[Fact]
	public void EvaluatorNameExpressionReportsNoErrorForInsertedToken()
	{
		string text =
		@"
			[]
		";

		string diagnostics =
		@"
			Error: Unexpected token <EndOfFileToken>, expected <IdentifierToken>
		";

		AssertDiagnostics(text, diagnostics);
	}

	[Fact]
	public void EvaluatorAssignmentExpressionReportsUndefined()
	{
		string text =
		@"
			[x] = 1
		";

		string diagnostics =
		@"
			Error: Variable 'x' is not defined
		";

		AssertDiagnostics(text, diagnostics);
	}

	[Fact]
	public void EvaluatorAssignmentExpressionReportsCannotBeAssigned()
	{
		string text =
		@"
			{
				let x = 1
				x [=] 0
			}
		";

		string diagnostics =
		@"
			Error: Variable 'x' is read-only and cannot be assigned to
		";

		AssertDiagnostics(text, diagnostics);
	}

	[Fact]
	public void EvaluatorAssignmentExpressionReportsCannotConvert()
	{
		string text =
		@"
			{
				var x = 1
				x = [true]
			}
		";

		string diagnostics =
		@"
			Error: Cannot convert type System.Boolean to System.Int32
		";

		AssertDiagnostics(text, diagnostics);
	}


	[Fact]
	public void EvaluatorUnaryExpressionReportsUndefined()
	{
		string text =
		@"
			[-]true
		";

		string diagnostics =
		@"
			Error: Unary operator '-' is not defined for type System.Boolean
		";

		AssertDiagnostics(text, diagnostics);
	}

	[Fact]
	public void EvaluatorBinaryExpressionReportsUndefined()
	{
		string text =
		@"
			1 [+] true
		";

		string diagnostics =
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

	private static void AssertDiagnostics(string text, string diagnosticText)
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
