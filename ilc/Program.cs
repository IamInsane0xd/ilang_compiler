﻿using ILang.CodeAnalysis;
using ILang.CodeAnalysis.Syntax;

namespace ILang;

internal static class Program
{
	private static void Main()
	{
		var showTree = false;
		var variables = new Dictionary<VariableSymbol, object?>();

		while (true)
		{
			Console.Write(">>> ");
			var line = Console.ReadLine();

			if (string.IsNullOrWhiteSpace(line))
				continue;

			switch (line)
			{
				case "#showTree":
					showTree = !showTree;
					Console.WriteLine(showTree ? "Showing parse tree" : "Not showing parse tree");
					continue;

				case "#clear":
					Console.Clear();
					continue;

				case "#exit":
					return;
			}

			var syntaxTree = SyntaxTree.Parse(line);
			var compilation = new Compilation(syntaxTree);
			var result = compilation.Evaluate(variables);
			var diagnostics = result.Diagnostics;

			if (showTree)
			{
				Console.ForegroundColor = ConsoleColor.DarkGray;
				syntaxTree.Root.WriteTo(Console.Out);
				Console.ResetColor();
			}

			if (!diagnostics.Any())
			{
				Console.WriteLine(result.Value);
			}

			else
			{
				foreach (var diagnostic in diagnostics)
				{
					Console.ForegroundColor = ConsoleColor.DarkRed;
					Console.WriteLine(diagnostic);
					Console.ResetColor();

					var prefix = line.Substring(0, diagnostic.Span.Start);
					var error = diagnostic.Span.Start < line.Length ? line.Substring(diagnostic.Span.Start, diagnostic.Span.Length) : "";
					var suffix = diagnostic.Span.End < line.Length ? line.Substring(diagnostic.Span.End) : "";

					Console.Write("   ");
					Console.Write(prefix);

					Console.ForegroundColor = ConsoleColor.Red;
					Console.Write(error);
					Console.ResetColor();

					Console.Write(suffix);
					Console.WriteLine("\n");
				}
			}
		}
	}
}
