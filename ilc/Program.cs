using ILang.CodeAnalysis;
using ILang.CodeAnalysis.Syntax;
using ILang.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace ILang;

internal static class Program
{
	private static void Main()
	{
		bool showTree = false;
		bool showProgram = false;
		Dictionary<VariableSymbol, object?> variables = new Dictionary<VariableSymbol, object?>();
		StringBuilder textBuilder = new StringBuilder();
		Compilation? previous = null;

		while (true)
		{
			Console.ForegroundColor = ConsoleColor.DarkGreen;

			if (textBuilder.Length == 0)
				Console.Write("» ");

			else
				Console.Write("· ");

			Console.ResetColor();

			string? input = Console.ReadLine();
			bool isBlank = string.IsNullOrWhiteSpace(input);

			if (textBuilder.Length == 0)
			{
				if (isBlank)
					continue;

				switch (input)
				{
					case "#showTree":
						showTree = !showTree;
						Console.WriteLine(showTree ? "Showing parse tree\n" : "Not showing parse tree\n");
						continue;

					case "#showProgram":
						showProgram = !showProgram;
						Console.WriteLine(showProgram ? "Showing bound tree\n" : "Not showing bound tree\n");
						continue;

					case "#clear":
						Console.Clear();
						continue;

					case "#reset":
						previous = null;
						Console.WriteLine();
						continue;

					case "#exit":
						Console.ResetColor();
						return;
				}
			}

			textBuilder.AppendLine(input);

			string text = textBuilder.ToString();
			SyntaxTree syntaxTree = SyntaxTree.Parse(text);

			if (!isBlank && syntaxTree.Diagnostics.Any())
				continue;

			Compilation compilation = previous == null ? new Compilation(syntaxTree) : previous.ContinueWith(syntaxTree);

			if (showTree)
			{
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.WriteLine("Parse tree:");
				syntaxTree.Root.WriteTo(Console.Out);

				if (showProgram)
					Console.WriteLine();
			}

			if (showProgram)
			{
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.WriteLine("Bound tree:");
				compilation.EmitTree(Console.Out);
			}

			Console.ResetColor();

			EvaluationResult result = compilation.Evaluate(variables);

			if (!result.Diagnostics.Any())
			{
				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine(result.Value);
				Console.WriteLine();
				Console.ResetColor();

				previous = compilation;
			}

			else
			{
				foreach (Diagnostic? diagnostic in result.Diagnostics)
				{
					int lineIndex = syntaxTree.Text.GetLineIndex(diagnostic.Span.Start);
					TextLine line = syntaxTree.Text.Lines[lineIndex];
					int lineNumber = lineIndex + 1;
					int character = diagnostic.Span.Start - line.Start + 1;

					Console.ForegroundColor = ConsoleColor.DarkGray;
					Console.Write($"({lineNumber}:{character}) ");

					Console.ForegroundColor = ConsoleColor.DarkRed;
					Console.WriteLine(diagnostic);
					Console.ResetColor();

					TextSpan prefixSpan = TextSpan.FormBounds(line.Start, diagnostic.Span.Start);
					TextSpan suffixSpan = TextSpan.FormBounds(diagnostic.Span.End, line.End);

					string prefix = syntaxTree.Text.ToString(prefixSpan);
					string error = syntaxTree.Text.ToString(diagnostic.Span);
					string suffix = syntaxTree.Text.ToString(suffixSpan);

					Console.Write("   ");
					Console.Write(prefix.TrimStart());

					Console.ForegroundColor = ConsoleColor.Red;
					Console.Write(error);
					Console.ResetColor();

					Console.Write(suffix);
					Console.WriteLine();
				}

				Console.WriteLine();
			}

			textBuilder.Clear();
		}
	}
}
