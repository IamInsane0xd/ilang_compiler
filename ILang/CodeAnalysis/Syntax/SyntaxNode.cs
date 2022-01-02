using ILang.CodeAnalysis.Text;
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
			return TextSpan.FormBounds(first.Start, last.End);
		}
	}

	public IEnumerable<SyntaxNode> GetChildren()
	{
		PropertyInfo[]? properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

		foreach (PropertyInfo? property in properties)
		{
			if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType))
			{
				SyntaxNode? child = (SyntaxNode?) property.GetValue(this);
				yield return child ?? throw new ArgumentNullException(nameof(child));
			}

			else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType))
			{
				IEnumerable<SyntaxNode>? children = (IEnumerable<SyntaxNode>?) property.GetValue(this);

				foreach (SyntaxNode? child in children ?? throw new ArgumentNullException(nameof(children)))
					yield return child;
			}
		}
	}

	public void WriteTo(TextWriter writer) => PrettyPrint(writer, this);

	private static void PrettyPrint(TextWriter writer, SyntaxNode node, string indent = "", bool isLast = true, bool isFirst = true)
	{
		string? marker = isFirst ? "" : isLast ? "└──" : "├──";
		writer.Write(indent);
		writer.Write(marker);
		writer.Write(node.Kind);

		if (node is SyntaxToken t && t.Value != null)
		{
			writer.Write($" {t.Value}");
		}

		writer.WriteLine();

		indent += isFirst ? "" : isLast ? "   " : "│  ";

		SyntaxNode? lastChild = node.GetChildren().LastOrDefault();

		foreach (SyntaxNode? child in node.GetChildren())
			PrettyPrint(writer, child ?? throw new ArgumentNullException(nameof(child)), indent, child == lastChild, false);
	}

	public override string ToString()
	{
		using (StringWriter? writer = new StringWriter())
		{
			WriteTo(writer);
			return writer.ToString();
		}
	}
}
