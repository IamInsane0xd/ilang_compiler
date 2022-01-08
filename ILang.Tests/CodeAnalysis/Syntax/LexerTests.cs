using ILang.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ILang.Tests.CodeAnalysis.Syntax;

public class LexerTests
{
	[Fact]
	public void LexerTestsAllTokens()
	{
		IEnumerable<SyntaxKind> tokenKinds = Enum.GetValues(typeof(SyntaxKind))
																						 .Cast<SyntaxKind>()
																						 .Where(k => k.ToString().EndsWith("Keyword") || k.ToString().EndsWith("Token"));
		IEnumerable<SyntaxKind> testedTokenKinds = GetTokens().Concat(GetSeparators()).Select(t => t.kind);
		SortedSet<SyntaxKind> untestedTokenKinds = new SortedSet<SyntaxKind>(tokenKinds);

		untestedTokenKinds.Remove(SyntaxKind.BadToken);
		untestedTokenKinds.Remove(SyntaxKind.EndOfFileToken);
		untestedTokenKinds.ExceptWith(testedTokenKinds);

		Assert.Empty(untestedTokenKinds);
	}

	[Theory]
	[MemberData(nameof(GetTokensData))]
	public void LexerLexesToken(SyntaxKind kind, string text)
	{
		IEnumerable<SyntaxToken> tokens = SyntaxTree.ParseTokens(text);
		SyntaxToken token = Assert.Single(tokens);

		Assert.Equal(kind, token.Kind);
		Assert.Equal(text, token.Text);
	}

	[Theory]
	[MemberData(nameof(GetTokenPairsData))]
	public void LexerLexesTokenPairs(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)
	{
		string text = t1Text + t2Text;
		SyntaxToken[] tokens = SyntaxTree.ParseTokens(text).ToArray();

		Assert.Equal(2, tokens.Length);
		Assert.Equal(tokens[0].Kind, t1Kind);
		Assert.Equal(tokens[0].Text, t1Text);
		Assert.Equal(tokens[1].Kind, t2Kind);
		Assert.Equal(tokens[1].Text, t2Text);
	}

	[Theory]
	[MemberData(nameof(GetTokenPairsWithSeparatorData))]
	public void LexerLexesTokenPairsWithSeparator(SyntaxKind t1Kind, string t1Text, SyntaxKind separatorKind, string separatorText, SyntaxKind t2Kind, string t2Text)
	{
		string text = t1Text + separatorText + t2Text;
		SyntaxToken[] tokens = SyntaxTree.ParseTokens(text).ToArray();

		Assert.Equal(3, tokens.Length);
		Assert.Equal(tokens[0].Kind, t1Kind);
		Assert.Equal(tokens[0].Text, t1Text);
		Assert.Equal(tokens[1].Kind, separatorKind);
		Assert.Equal(tokens[1].Text, separatorText);
		Assert.Equal(tokens[2].Kind, t2Kind);
		Assert.Equal(tokens[2].Text, t2Text);
	}

	public static IEnumerable<object[]> GetTokensData()
	{
		foreach ((SyntaxKind kind, string text) t in GetTokens().Concat(GetSeparators()))
			yield return new object[] { t.kind, t.text };
	}

	public static IEnumerable<object[]> GetTokenPairsData()
	{
		foreach ((SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text) t in GetTokenPairs())
			yield return new object[] { t.t1Kind, t.t1Text, t.t2Kind, t.t2Text };
	}

	public static IEnumerable<object[]> GetTokenPairsWithSeparatorData()
	{
		foreach ((SyntaxKind t1Kind, string t1Text, SyntaxKind separatorKind, string separatorText, SyntaxKind t2Kind, string t2Text) t in GetTokenPairsWithSeparator())
			yield return new object[] { t.t1Kind, t.t1Text, t.separatorKind, t.separatorText, t.t2Kind, t.t2Text };
	}

	private static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
	{
		IEnumerable<(SyntaxKind, string text)> fixedTokens = (IEnumerable<(SyntaxKind, string text)>) Enum.GetValues(typeof(SyntaxKind))
																														 .Cast<SyntaxKind>()
																														 .Select(k => (kind: k, text: SyntacFacts.GetText(k)))
																														 .Where(t => t.text != null);

		(SyntaxKind, string)[] dynamicTokens = new[]
		{
			(SyntaxKind.NumberToken, "1"),
			(SyntaxKind.NumberToken, "123"),
			(SyntaxKind.IdentifierToken, "a"),
			(SyntaxKind.IdentifierToken, "abc")
		};

		return fixedTokens.Concat(dynamicTokens);
	}

	private static IEnumerable<(SyntaxKind kind, string text)> GetSeparators()
	{
		return new[]
		{
			(SyntaxKind.WhiteSpaceToken, " "),
			(SyntaxKind.WhiteSpaceToken, "  "),
			(SyntaxKind.WhiteSpaceToken, "\r"),
			(SyntaxKind.WhiteSpaceToken, "\n"),
			(SyntaxKind.WhiteSpaceToken, "\r\n"),
			(SyntaxKind.WhiteSpaceToken, "\t")
		};
	}

	private static bool RequiresSeparator(SyntaxKind t1Kind, SyntaxKind t2Kind)
	{
		bool t1IsKeyword = t1Kind.ToString().EndsWith("Keyword");
		bool t2IsKeyword = t2Kind.ToString().EndsWith("Keyword");

		if (t1Kind == SyntaxKind.IdentifierToken && t2Kind == SyntaxKind.IdentifierToken)
			return true;

		if (t1IsKeyword && t2IsKeyword)
			return true;

		if (t1IsKeyword && t2Kind == SyntaxKind.IdentifierToken)
			return true;

		if (t1Kind == SyntaxKind.IdentifierToken && t2IsKeyword)
			return true;

		if (t1Kind == SyntaxKind.NumberToken && t2Kind == SyntaxKind.NumberToken)
			return true;

		if (t1Kind == SyntaxKind.BangToken && t2Kind == SyntaxKind.EqualsToken)
			return true;

		if (t1Kind == SyntaxKind.BangToken && t2Kind == SyntaxKind.EqualsEqualsToken)
			return true;

		if (t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualsToken)
			return true;

		if (t1Kind == SyntaxKind.EqualsToken && t2Kind == SyntaxKind.EqualsEqualsToken)
			return true;

		if (t1Kind == SyntaxKind.LessToken && t2Kind == SyntaxKind.EqualsToken)
			return true;

		if (t1Kind == SyntaxKind.LessToken && t2Kind == SyntaxKind.EqualsEqualsToken)
			return true;

		if (t1Kind == SyntaxKind.GreaterToken && t2Kind == SyntaxKind.EqualsToken)
			return true;

		if (t1Kind == SyntaxKind.GreaterToken && t2Kind == SyntaxKind.EqualsEqualsToken)
			return true;

		return false;
	}

	private static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)> GetTokenPairs()
	{
		foreach ((SyntaxKind kind, string text) t1 in GetTokens())
		{
			foreach ((SyntaxKind kind, string text) t2 in GetTokens())
			{
				if (!RequiresSeparator(t1.kind, t2.kind))
					yield return (t1.kind, t1.text, t2.kind, t2.text);
			}
		}
	}

	private static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind separatorKind, string separatorText, SyntaxKind t2Kind, string t2Text)> GetTokenPairsWithSeparator()
	{
		foreach ((SyntaxKind kind, string text) t1 in GetTokens())
		{
			foreach ((SyntaxKind kind, string text) t2 in GetTokens())
			{
				if (RequiresSeparator(t1.kind, t2.kind))
				{
					foreach ((SyntaxKind kind, string text) s in GetSeparators())
						yield return (t1.kind, t1.text, s.kind, s.text, t2.kind, t2.text);
				}
			}
		}
	}
}
