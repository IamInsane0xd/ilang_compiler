using ILang.CodeAnalysis.Syntax;
using System.Reflection;

namespace ILang.CodeAnalysis.Binding;

internal abstract class BoundNode
{
	public abstract BoundNodeKind Kind { get; }

	public IEnumerable<BoundNode> GetChildren()
	{
		PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

		foreach (PropertyInfo property in properties)
		{
			if (typeof(BoundNode).IsAssignableFrom(property.PropertyType))
			{
				BoundNode? child = (BoundNode?) property.GetValue(this);

				if (child != null)
					yield return child;
			}

			else if (typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
			{
				IEnumerable<BoundNode>? children = (IEnumerable<BoundNode>?) property.GetValue(this);

				foreach (BoundNode child in children ?? throw new ArgumentNullException(nameof(children)))
					if (child != null)
						yield return child;
			}
		}
	}

	private IEnumerable<(string name, object value)> GetProperties()
	{
		PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

		foreach (PropertyInfo property in properties)
		{
			if (property.Name == nameof(Kind) ||
					property.Name == nameof(BoundBinaryExpression.Op) ||
					typeof(BoundNode).IsAssignableFrom(property.PropertyType) ||
					typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
				continue;

			object? value = property.GetValue(this);

			if (value != null)
				yield return (property.Name, value);
		}
	}

	public void WriteTo(TextWriter writer) => PrettyPrint(writer, this);

	private static void PrettyPrint(TextWriter writer, BoundNode node, string indent = "", bool isLast = true, bool isFirst = true)
	{
		bool isToConsole = writer == Console.Out;
		string marker = isFirst ? "" : isLast ? "└──" : "├──";

		if (isToConsole)
			Console.ForegroundColor = ConsoleColor.DarkGray;

		writer.Write(indent);
		writer.Write(marker);

		if (isToConsole)
			Console.ForegroundColor = GetColor(node);

		string text = GetText(node);
		writer.Write(text);

		bool isFirstProperty = true;

		foreach ((string name, object value) p in node.GetProperties())
		{
			if (isFirstProperty)
			{
				isFirstProperty = false;

				if (isToConsole)
					Console.ForegroundColor = ConsoleColor.DarkGray;

				writer.Write(" : ");
			}

			else
			{
				if (isToConsole)
					Console.ForegroundColor = ConsoleColor.DarkGray;

				writer.Write(", ");
			}

			if (isToConsole)
				Console.ForegroundColor = ConsoleColor.Yellow;

			writer.Write(p.name);

			if (isToConsole)
				Console.ForegroundColor = ConsoleColor.DarkGray;

			writer.Write(" = ");

			if (isToConsole)
				Console.ForegroundColor = ConsoleColor.DarkYellow;

			writer.Write(p.value);
		}

		if (isToConsole)
			Console.ResetColor();

		if (isToConsole)
			Console.ForegroundColor = ConsoleColor.DarkGray;

		writer.WriteLine();

		indent += isFirst ? "" : isLast ? "   " : "│  ";

		BoundNode? lastChild = node.GetChildren().LastOrDefault();

		foreach (BoundNode child in node.GetChildren())
			PrettyPrint(writer, child ?? throw new ArgumentNullException(nameof(child)), indent, child == lastChild, false);
	}

	private static string GetText(BoundNode node)
	{
		return node switch
		{
			BoundBinaryExpression b => $"{b.Op.Kind}Expression",
			BoundUnaryExpression u => $"{u.Op.Kind}Expression",
			_ => node.Kind.ToString(),
		};
	}

	private static ConsoleColor GetColor(BoundNode node)
	{
		return node switch
		{
			BoundExpression => ConsoleColor.Blue,
			BoundStatement => ConsoleColor.Cyan,
			_ => ConsoleColor.Yellow,
		};
	}

	public override string ToString()
	{
		using StringWriter writer = new StringWriter();
		WriteTo(writer);
		return writer.ToString();
	}

}
