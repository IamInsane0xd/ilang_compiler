using ILang.Classes;

namespace ILang
{
	internal static class Program
	{
		private static void Main()
		{
			var showTree = false;

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

				if (showTree)
				{
					Console.ForegroundColor = ConsoleColor.DarkGray;
					PrettyPrint(syntaxTree.Root);
					Console.ResetColor();
				}

				if (!syntaxTree.Diagnostics.Any())
				{
					var evaluator = new Evaluator(syntaxTree.Root);
					var result = evaluator.Evaluate();

					Console.WriteLine(result);
				}

				else
				{
					Console.ForegroundColor = ConsoleColor.DarkRed;

					foreach (var diagnostic in syntaxTree.Diagnostics)
						Console.WriteLine(diagnostic);

					Console.ResetColor();
				}
			}
		}

		static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true, bool isFirst = true)
		{
			var marker = isLast ? isFirst ? "" : "└──" : "├──";
			Console.Write(indent);
			Console.Write(marker);
			Console.Write(node.Kind);
			
			if (node is SyntaxToken t && t.Value != null)
			{
				Console.Write($" {t.Value}");
			}

			Console.WriteLine();

			indent += isFirst ? "" : isLast ? "   " : "│  ";

			var lastChild = node.GetChildren().LastOrDefault();
			
			foreach (var child in node.GetChildren())
				PrettyPrint(child, indent, child == lastChild, false);
		}
	}
}
