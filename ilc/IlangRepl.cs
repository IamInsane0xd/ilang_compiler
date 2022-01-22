using ILang.CodeAnalysis;
using ILang.CodeAnalysis.Syntax;
using ILang.CodeAnalysis.Text;

namespace ILang;

internal sealed class IlangRepl : Repl
{
	private Compilation? _previous;
	private bool _showTree;
	private bool _showProgram;
	private readonly Dictionary<VariableSymbol, object?> _variables = new Dictionary<VariableSymbol, object?>();

	protected override void EvaluateMetaCommand(string input)
	{
		switch (input)
		{
			case "#showTree":
				_showTree = !_showTree;
				Console.WriteLine(_showTree ? "Showing parse tree\n" : "Not showing parse tree\n");
				break;

			case "#showProgram":
				_showProgram = !_showProgram;
				Console.WriteLine(_showProgram ? "Showing bound tree\n" : "Not showing bound tree\n");
				break;

			case "#clear":
				Console.Clear();
				break;

			case "#reset":
				_previous = null;
				Console.WriteLine();
				break;

			case "#exit":
				Console.ForegroundColor = ConsoleColor.DarkGray;
				Console.WriteLine("Goodbye!\n");
				Console.ResetColor();
				Environment.Exit(0);
				break;

			default:
				base.EvaluateMetaCommand(input);
				break;
		}
	}

	protected override bool IsCompleteSubmission(string text)
	{
		SyntaxTree syntaxTree = SyntaxTree.Parse(text);

		if (string.IsNullOrEmpty(text))
			return true;

		if (syntaxTree.Diagnostics.Any())
			return false;

		return true;
	}

	protected override void EvaluateSumission(string text)
	{
		SyntaxTree syntaxTree = SyntaxTree.Parse(text);
		Compilation compilation = _previous == null ? new Compilation(syntaxTree) : _previous.ContinueWith(syntaxTree);

		if (_showTree)
		{
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.WriteLine("Parse tree:");
			syntaxTree.Root.WriteTo(Console.Out);

			if (_showProgram)
				Console.WriteLine();
		}

		if (_showProgram)
		{
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.WriteLine("Bound tree:");
			compilation.EmitTree(Console.Out);
		}

		Console.ResetColor();

		EvaluationResult result = compilation.Evaluate(_variables);

		if (!result.Diagnostics.Any())
		{
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine(result.Value);
			Console.WriteLine();
			Console.ResetColor();

			_previous = compilation;
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

				TextSpan prefixSpan = TextSpan.FromBounds(line.Start, diagnostic.Span.Start);
				TextSpan suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);

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
	}
}
