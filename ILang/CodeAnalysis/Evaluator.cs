using ILang.CodeAnalysis.Binding;

namespace ILang.CodeAnalysis;

internal sealed class Evaluator
{
	private readonly BoundExpression _root;
	private readonly Dictionary<VariableSymbol, object?> _variables;

	public Evaluator(BoundExpression root, Dictionary<VariableSymbol, object?> variables)
	{
		_root = root;
		_variables = variables;
	}

	public object? Evaluate() => EvaluateExpression(_root);

	private object? EvaluateExpression(BoundExpression node)
	{
		switch (node.Kind)
		{
			case BoundNodeKind.LiteralExpression:
				return EvaluateLiteralExpression((BoundLiteralExpression) node);

			case BoundNodeKind.VariableExpression:
				return EvaluateVariableExpression((BoundVariableExpression) node);

			case BoundNodeKind.AssignmentExpression:
				return EvaluateAssignmentExpression((BoundAssignmentExpression) node);

			case BoundNodeKind.UnaryExpression:
				return EvaluateUnaryExpression((BoundUnaryExpression) node);

			case BoundNodeKind.BinaryExpression:
				return EvaluateBinaryExpression((BoundBinaryExpression) node);

			default:
				throw new Exception($"Unexpected node {node.Kind}");
		}
	}

	private static object EvaluateLiteralExpression(BoundLiteralExpression l) => l.Value;

	private object? EvaluateVariableExpression(BoundVariableExpression v) => _variables[v.Variable];

	private object? EvaluateAssignmentExpression(BoundAssignmentExpression a)
	{
		object? value = EvaluateExpression(a.Expression);
		_variables[a.Variable] = value;
		return value;
	}

	private object EvaluateUnaryExpression(BoundUnaryExpression u)
	{
		object operand;
		operand = EvaluateExpression(u.Operand) ?? throw new ArgumentNullException(nameof(operand));

		switch (u.Op.Kind)
		{
			case BoundUnaryOperatorKind.Identity:
				return (int) operand;

			case BoundUnaryOperatorKind.Negation:
				return -(int) operand;

			case BoundUnaryOperatorKind.LogicalNegation:
				return !(bool) operand;

			default:
				throw new Exception($"Unexpected unary operator {u.Op}");
		}
	}

	private object EvaluateBinaryExpression(BoundBinaryExpression b)
	{
		object? left = EvaluateExpression(b.Left) ?? 0;
		object? right = EvaluateExpression(b.Right) ?? 0;

		if (b.Op == null)
			throw new ArgumentNullException(nameof(b.Op));

		switch (b.Op.Kind)
		{
			case BoundBinaryOperatorKind.Addition:
				return (int) left + (int) right;

			case BoundBinaryOperatorKind.Subtraction:
				return (int) left - (int) right;

			case BoundBinaryOperatorKind.Multiplication:
				return (int) left * (int) right;

			case BoundBinaryOperatorKind.Division:
				if ((int) right == 0)
					return 0;

				return (int) left / (int) right;

			case BoundBinaryOperatorKind.LogicalAnd:
				return (bool) left && (bool) right;

			case BoundBinaryOperatorKind.LogicalOr:
				return (bool) left || (bool) right;

			case BoundBinaryOperatorKind.Equals:
				return Equals(left, right);

			case BoundBinaryOperatorKind.NotEquals:
				return !Equals(left, right);

			default:
				throw new Exception($"Unexpected binary operator {b.Op}");
		}
	}
}
