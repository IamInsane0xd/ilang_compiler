using ILang.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using Xunit;

namespace ILang.Tests.CodeAnalysis.Syntax;

public class SyntaxFactTests
{
	[Theory]
	[MemberData(nameof(GetSyntaxKindData))]
	public void SyntaxFactGetTextRoundTrips(SyntaxKind kind)
	{
		string? text = SyntacFacts.GetText(kind);

		if (text == null)
			return;

		IEnumerable<SyntaxToken>? tokens = SyntaxTree.ParseTokens(text);
		SyntaxToken? token = Assert.Single(tokens);

		Assert.Equal(kind, token.Kind);
		Assert.Equal(text, token.Text);
	}

	public static IEnumerable<object[]> GetSyntaxKindData()
	{
		SyntaxKind[]? kinds = (SyntaxKind[]) Enum.GetValues(typeof(SyntaxKind));

		foreach (SyntaxKind kind in kinds)
			yield return new object[] { kind };
	}
}
