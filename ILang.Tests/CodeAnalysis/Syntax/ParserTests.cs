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
		var op1Precedence = SyntacFacts.GetBinaryOperatorPrecedence(op1);
		var op2Precedence = SyntacFacts.GetBinaryOperatorPrecedence(op2);
		var op1Text = SyntacFacts.GetText(op1);
		var op2Text = SyntacFacts.GetText(op2);
		var text = $"a {op1Text} b {op2Text} c";
		var expression = SyntaxTree.Parse(text).Root;

		if (op1Precedence >= op2Precedence)
		{
			using (var e = new AssertingEnumerator(expression))
			{
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
		}

		else
		{
			using (var e = new AssertingEnumerator(expression))
			{
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
	}

	public static IEnumerable<object[]> GetBinaryOperatorPairsData()
	{
		foreach (var op1 in SyntacFacts.GetBinaryOperatorKinds())
			foreach (var op2 in SyntacFacts.GetBinaryOperatorKinds())
			{
				yield return new object[] { op1, op2 };
				yield break;
			}
	}
}
