using ILang.CodeAnalysis.Binding;

namespace ILang.CodeAnalysis;

internal sealed class Evaluator
{
	private readonly BoundBlockStatement _root;
	private readonly Dictionary<VariableSymbol, object?> _variables;
	private object? _lastValue;

	public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object?> variables)
	{
		_root = root;
		_variables = variables;
	}

	public object? Evaluate()
	{
		Dictionary<LabelSymbol, int> labelToIndex = new Dictionary<LabelSymbol, int>();

		for (int i = 0; i < _root.Statements.Length; i++)
		{
			if (_root.Statements[i] is BoundLabelStatement l)
				labelToIndex.Add(l.Label, i + 1);
		}

		int index = 0;

		while (index < _root.Statements.Length)
		{
			BoundStatement statement = _root.Statements[index];

			switch (statement.Kind)
			{
				case BoundNodeKind.VariableDeclaration:
					EvaluateVariableDeclaration((BoundVariableDeclaration) statement);
					index++;
					break;

				case BoundNodeKind.ExpressionStatement:
					EvaluateExpressionStatement((BoundExpressionStatement) statement);
					index++;
					break;

				case BoundNodeKind.GotoStatement:
					BoundGotoStatement gotoStatement = (BoundGotoStatement) statement;
					index = labelToIndex[gotoStatement.Label];
					break;

				case BoundNodeKind.ConditionalGotoStatement:
					BoundConditionalGotoStatement conditionalGotoStatement = (BoundConditionalGotoStatement) statement;
					bool condition = (bool) (EvaluateExpression(conditionalGotoStatement.Condition) ?? throw new ArgumentNullException(nameof(EvaluateExpression)));

					if (condition == conditionalGotoStatement.JumpIfTrue)
						index = labelToIndex[conditionalGotoStatement.Label];

					else
						index++;

					break;

				case BoundNodeKind.LabelStatement:
					index++;
					break;

				default:
					throw new Exception($"Unexpected node {statement.Kind}");
			}
		}

		return _lastValue;
	}

	private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
	{
		object? value = EvaluateExpression(node.Initializer);
		_variables[node.Variable] = value;
		_lastValue = value;
	}

	private void EvaluateExpressionStatement(BoundExpressionStatement node) => _lastValue = EvaluateExpression(node.Expression);

	private object? EvaluateExpression(BoundExpression node)
	{
		return node.Kind switch
		{
			BoundNodeKind.LiteralExpression => EvaluateLiteralExpression((BoundLiteralExpression) node),
			BoundNodeKind.VariableExpression => EvaluateVariableExpression((BoundVariableExpression) node),
			BoundNodeKind.AssignmentExpression => EvaluateAssignmentExpression((BoundAssignmentExpression) node),
			BoundNodeKind.UnaryExpression => EvaluateUnaryExpression((BoundUnaryExpression) node),
			BoundNodeKind.BinaryExpression => EvaluateBinaryExpression((BoundBinaryExpression) node),
			_ => throw new Exception($"Unexpected node {node.Kind}"),
		};
	}

	private static object EvaluateLiteralExpression(BoundLiteralExpression l) => l.Value;

	private object? EvaluateVariableExpression(BoundVariableExpression v)
	{
		if (_variables.ContainsKey(v.Variable))
			return _variables[v.Variable];

		return 0;
	}

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

		return u.Op.Kind switch
		{
			BoundUnaryOperatorKind.Identity => (int) operand,
			BoundUnaryOperatorKind.Negation => -(int) operand,
			BoundUnaryOperatorKind.LogicalNegation => !(bool) operand,
			BoundUnaryOperatorKind.OnesComplement => ~(int) operand,
			_ => throw new Exception($"Unexpected unary operator {u.Op}"),
		};
	}

	private object EvaluateBinaryExpression(BoundBinaryExpression b)
	{
		object left = EvaluateExpression(b.Left) ?? 0;
		object right = EvaluateExpression(b.Right) ?? 0;

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

			case BoundBinaryOperatorKind.BitwiseAnd:
				if (b.Type == typeof(int))
					return (int) left & (int) right;

				else
					return (bool) left & (bool) right;

			case BoundBinaryOperatorKind.BitwiseOr:
				if (b.Type == typeof(int))
					return (int) left | (int) right;

				else
					return (bool) left | (bool) right;

			case BoundBinaryOperatorKind.BitwiseXor:
				if (b.Type == typeof(int))
					return (int) left ^ (int) right;

				else
					return (bool) left ^ (bool) right;

			case BoundBinaryOperatorKind.LogicalAnd:
				return (bool) left && (bool) right;

			case BoundBinaryOperatorKind.LogicalOr:
				return (bool) left || (bool) right;

			case BoundBinaryOperatorKind.Equals:
				return Equals(left, right);

			case BoundBinaryOperatorKind.NotEquals:
				return !Equals(left, right);

			case BoundBinaryOperatorKind.Less:
				return (int) left < (int) right;

			case BoundBinaryOperatorKind.LessOrEquals:
				return (int) left <= (int) right;

			case BoundBinaryOperatorKind.Greater:
				return (int) left > (int) right;

			case BoundBinaryOperatorKind.GreaterOrEquals:
				return (int) left >= (int) right;

			default:
				throw new Exception($"Unexpected binary operator {b.Op}");
		}
	}
}
