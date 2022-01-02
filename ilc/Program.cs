using ILang.CodeAnalysis;
using ILang.CodeAnalysis.Syntax;
using ILang.CodeAnalysis.Text;

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

			SyntaxTree syntaxTree = SyntaxTree.Parse(line);
			Compilation compilation = new Compilation(syntaxTree);
			EvaluationResult result = compilation.Evaluate(variables);
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
				Console.WriteLine();
			}

			else
			{
				SourceText text = syntaxTree.Text;

				foreach (Diagnostic? diagnostic in diagnostics)
				{
					int lineIndex = text.GetLineIndex(diagnostic.Span.Start);
					int lineNumber = lineIndex + 1;
					int character = diagnostic.Span.Start - text.Lines[lineIndex].Start + 1;
					string prefix = line[0..diagnostic.Span.Start];
					string error = diagnostic.Span.Start < line.Length ? line[diagnostic.Span.Start..diagnostic.Span.End] : "";
					string suffix = diagnostic.Span.End < line.Length ? line[diagnostic.Span.End..] : "";

					Console.ForegroundColor = ConsoleColor.DarkGray;
					Console.Write($"({lineNumber}:{character}) ");
					Console.ForegroundColor = ConsoleColor.DarkRed;
					Console.WriteLine(diagnostic);
					Console.ResetColor();
					Console.Write("   ");
					Console.Write(prefix);
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Write(error);
					Console.ResetColor();
					Console.Write(suffix);
					Console.WriteLine();
				}

				Console.WriteLine();
			}
		}
	}
}
