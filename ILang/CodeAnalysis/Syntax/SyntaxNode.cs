﻿using ILang.CodeAnalysis.Text;
using System.Reflection;

namespace ILang.CodeAnalysis.Syntax;

public abstract class SyntaxNode
{
	public abstract SyntaxKind Kind { get; }

	public virtual TextSpan Span
	{
		get
		{
			TextSpan first = GetChildren().First().Span;
			TextSpan last = GetChildren().Last().Span;
			return TextSpan.FromBounds(first.Start, last.End);
		}
	}

	public IEnumerable<SyntaxNode> GetChildren()
	{
		PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

		foreach (PropertyInfo property in properties)
		{
			if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType))
			{
				SyntaxNode? child = (SyntaxNode?) property.GetValue(this);

				if (child != null)
					yield return child;
			}

			else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType))
			{
				IEnumerable<SyntaxNode>? children = (IEnumerable<SyntaxNode>?) property.GetValue(this);

				foreach (SyntaxNode child in children ?? throw new ArgumentNullException(nameof(children)))
					if (child != null)
						yield return child;
			}
		}
	}

	public void WriteTo(TextWriter writer) => PrettyPrint(writer, this);

	private static void PrettyPrint(TextWriter writer, SyntaxNode node, string indent = "", bool isLast = true, bool isFirst = true)
	{
		bool isToConsole = writer == Console.Out;
		string marker = isFirst ? "" : isLast ? "└──" : "├──";

		writer.Write(indent);

		if (isToConsole)
			Console.ForegroundColor = ConsoleColor.DarkGray;

		writer.Write(marker);

		if (isToConsole)
			Console.ForegroundColor = node is SyntaxToken ? ConsoleColor.Blue : ConsoleColor.Cyan;

		writer.Write(node.Kind);

		if (node is SyntaxToken t && t.Value != null)
			writer.Write($" {t.Value}");

		if (isToConsole)
			Console.ForegroundColor = ConsoleColor.DarkGray;

		writer.WriteLine();

		indent += isFirst ? "" : isLast ? "   " : "│  ";

		SyntaxNode? lastChild = node.GetChildren().LastOrDefault();

		foreach (SyntaxNode child in node.GetChildren())
			PrettyPrint(writer, child ?? throw new ArgumentNullException(nameof(child)), indent, child == lastChild, false);
	}

	public override string ToString()
	{
		using StringWriter writer = new StringWriter();
		WriteTo(writer);
		return writer.ToString();
	}
}
