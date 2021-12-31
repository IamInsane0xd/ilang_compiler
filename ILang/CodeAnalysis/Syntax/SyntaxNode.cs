using System.Reflection;

namespace ILang.CodeAnalysis.Syntax;

public abstract class SyntaxNode
{
	public abstract SyntaxKind Kind { get; }

	public virtual TextSpan Span
	{
		get
		{
			var first = GetChildren().First().Span;
			var last = GetChildren().Last().Span;
			return TextSpan.FormBounds(first.Start, last.End);
		}
	}

	public IEnumerable<SyntaxNode> GetChildren()
	{
		var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

		foreach (var property in properties)
		{
			if (typeof(SyntaxNode).IsAssignableFrom(property.PropertyType))
			{
				var child = (SyntaxNode?) property.GetValue(this);
				yield return child ?? throw new ArgumentNullException(nameof(child));
			}

			else if (typeof(IEnumerable<SyntaxNode>).IsAssignableFrom(property.PropertyType))
			{
				var children = (IEnumerable<SyntaxNode>?) property.GetValue(this);

				foreach (var child in children ?? throw new ArgumentNullException(nameof(children)))
					yield return child;
			}
		}
	}
}
