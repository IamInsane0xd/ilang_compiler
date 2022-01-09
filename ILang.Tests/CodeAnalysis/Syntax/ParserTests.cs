using ILang.CodeAnalysis.Syntax;
using System.Collections.Generic;
using Xunit;

namespace ILang.Tests.CodeAnalysis.Syntax;

public class ParserTests
{
	[Theory]
	[MemberData(nameof(GetBinaryOperatorPairsData))]
	public void ParserBinaryExpressionHonorsPrecedences(SyntaxKind op1, SyntaxKind op2)
	{
		int op1Precedence = SyntacFacts.GetBinaryOperatorPrecedence(op1);
		int op2Precedence = SyntacFacts.GetBinaryOperatorPrecedence(op2);
		string? op1Text = SyntacFacts.GetText(op1);
		string? op2Text = SyntacFacts.GetText(op2);
		string text = $"a {op1Text} b {op2Text} c";
		ExpressionSyntax expression = ParseExpression(text);

		if (op1Precedence >= op2Precedence)
		{
			using AssertingEnumerator? e = new AssertingEnumerator(expression);
			e.AssertNode(SyntaxKind.BinaryExpression);
			e.AssertNode(SyntaxKind.BinaryExpression);
			e.AssertNode(SyntaxKind.NameExpression);
			e.AssertToken(SyntaxKind.IdentifierToken, "a");
			e.AssertToken(op1, op1Text ?? "");
			e.AssertNode(SyntaxKind.NameExpression);
			e.AssertToken(SyntaxKind.IdentifierToken, "b");
			e.AssertToken(op2, op2Text ?? "");
			e.AssertNode(SyntaxKind.NameExpression);
			e.AssertToken(SyntaxKind.IdentifierToken, "c");
		}

		else
		{
			using AssertingEnumerator? e = new AssertingEnumerator(expression);
			e.AssertNode(SyntaxKind.BinaryExpression);
			e.AssertNode(SyntaxKind.NameExpression);
			e.AssertToken(SyntaxKind.IdentifierToken, "a");
			e.AssertToken(op1, op1Text ?? "");
			e.AssertNode(SyntaxKind.BinaryExpression);
			e.AssertNode(SyntaxKind.NameExpression);
			e.AssertToken(SyntaxKind.IdentifierToken, "b");
			e.AssertToken(op2, op2Text ?? "");
			e.AssertNode(SyntaxKind.NameExpression);
			e.AssertToken(SyntaxKind.IdentifierToken, "c");
		}
	}

	[Theory]
	[MemberData(nameof(GetUnaryOperatorPairsData))]
	public void ParserUnaryExpressionHonorsPrecedences(SyntaxKind unaryKind, SyntaxKind binaryKind)
	{
		int unaryPrecedence = SyntacFacts.GetUnaryOperatorPrecedence(unaryKind);
		int binaryPrecedence = SyntacFacts.GetBinaryOperatorPrecedence(binaryKind);
		string? unaryText = SyntacFacts.GetText(unaryKind);
		string? binaryText = SyntacFacts.GetText(binaryKind);
		string text = $"{unaryText} a {binaryText} b";
		ExpressionSyntax expression = ParseExpression(text);

		if (unaryPrecedence >= binaryPrecedence)
		{
			using AssertingEnumerator? e = new AssertingEnumerator(expression);
			e.AssertNode(SyntaxKind.BinaryExpression);
			e.AssertNode(SyntaxKind.UnaryExpression);
			e.AssertToken(unaryKind, unaryText ?? "");
			e.AssertNode(SyntaxKind.NameExpression);
			e.AssertToken(SyntaxKind.IdentifierToken, "a");
			e.AssertToken(binaryKind, binaryText ?? "");
			e.AssertNode(SyntaxKind.NameExpression);
			e.AssertToken(SyntaxKind.IdentifierToken, "b");
		}

		else
		{
			using AssertingEnumerator? e = new AssertingEnumerator(expression);
			e.AssertNode(SyntaxKind.UnaryExpression);
			e.AssertToken(unaryKind, unaryText ?? "");
			e.AssertNode(SyntaxKind.BinaryExpression);
			e.AssertNode(SyntaxKind.NameExpression);
			e.AssertToken(SyntaxKind.IdentifierToken, "a");
			e.AssertToken(binaryKind, binaryText ?? "");
			e.AssertNode(SyntaxKind.NameExpression);
			e.AssertToken(SyntaxKind.IdentifierToken, "b");
			e.AssertNode(SyntaxKind.NameExpression);
		}
	}

	private static ExpressionSyntax ParseExpression(string text)
	{
		SyntaxTree syntaxTree = SyntaxTree.Parse(text);
		CompilationUnitSyntax root = syntaxTree.Root;
		StatementSyntax statement = root.Statement;
		return Assert.IsType<ExpressionStatementSyntax>(statement).Expression;
	}

	public static IEnumerable<object[]> GetBinaryOperatorPairsData()
	{
		foreach (SyntaxKind op1 in SyntacFacts.GetBinaryOperatorKinds())
		{
			foreach (SyntaxKind op2 in SyntacFacts.GetBinaryOperatorKinds())
				yield return new object[] { op1, op2 };
		}
	}

	public static IEnumerable<object[]> GetUnaryOperatorPairsData()
	{
		foreach (SyntaxKind unary in SyntacFacts.GetUnaryOperatorKinds())
		{
			foreach (SyntaxKind binary in SyntacFacts.GetBinaryOperatorKinds())
				yield return new object[] { unary, binary };
		}
	}
}
