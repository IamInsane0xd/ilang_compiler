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
		if (node is BoundLiteralExpression l)
			return l.Value;

		if (node is BoundVariableExpression v)
			return _variables[v.Variable];

		if (node is BoundAssignmentExpression a)
		{
			var value = EvaluateExpression(a.Expression);
			_variables[a.Variable] = value;
			return value;
		}

		if (node is BoundUnaryExpression u)
		{
			var operand = EvaluateExpression(u.Operand);

			if (u.Op == null)
				throw new ArgumentNullException(nameof(u.Op));

			if (operand == null)
				throw new ArgumentNullException(nameof(operand));

			switch (u.Op.Kind)
			{
				case BoundUnaryOperatorKind.Identity:
					return (int)operand;

				case BoundUnaryOperatorKind.Negation:
					return -(int)operand;

				case BoundUnaryOperatorKind.LogicalNegation:
					return !(bool)operand;
			}

			throw new Exception($"Unexpected unary operator {u.Op}");
		}

		if (node is BoundBinaryExpression b)
		{
			var left = EvaluateExpression(b.Left) ?? 0;
			var right = EvaluateExpression(b.Right) ?? 0;

			if (b.Op == null)
				throw new ArgumentNullException(nameof(b.Op));

			switch (b.Op.Kind)
			{
				case BoundBinaryOperatorKind.Addition:
					return (int)left + (int)right;

				case BoundBinaryOperatorKind.Subtraction:
					return (int)left - (int)right;

				case BoundBinaryOperatorKind.Multiplication:
					return (int)left * (int)right;

				case BoundBinaryOperatorKind.Division:
					if ((int)right == 0)
						return 0;

					return (int)left / (int)right;

				case BoundBinaryOperatorKind.LogicalAnd:
					return (bool)left && (bool)right;

				case BoundBinaryOperatorKind.LogicalOr:
					return (bool)left || (bool)right;

				case BoundBinaryOperatorKind.Equals:
					return Equals(left, right);

				case BoundBinaryOperatorKind.NotEquals:
					return !Equals(left, right);
			}

			throw new Exception($"Unexpected binary operator {b.Op}");
		}

		throw new Exception($"Unexpected node {node.Kind}");
	}
}
