using ILang.CodeAnalysis;
using ILang.CodeAnalysis.Syntax;

namespace ILang;

internal static class Program
{
	private static void Main()
	{
		bool showTree = false;
		Dictionary<VariableSymbol, object?>? variables = new Dictionary<VariableSymbol, object?>();

		while (true)
		{
			Console.Write(">>> ");
			string? line = Console.ReadLine();

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

			SyntaxTree? syntaxTree = SyntaxTree.Parse(line);
			Compilation? compilation = new Compilation(syntaxTree);
			EvaluationResult? result = compilation.Evaluate(variables);
			System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics = result.Diagnostics;

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
				foreach (Diagnostic? diagnostic in diagnostics)
				{
					Console.ForegroundColor = ConsoleColor.DarkRed;
					Console.WriteLine(diagnostic);
					Console.ResetColor();

					string? prefix = line[0..diagnostic.Span.Start];
					string? error = diagnostic.Span.Start < line.Length ? line[diagnostic.Span.Start..diagnostic.Span.End] : "";
					string? suffix = diagnostic.Span.End < line.Length ? line[^(diagnostic.Span.End - 1)..] : "";

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
