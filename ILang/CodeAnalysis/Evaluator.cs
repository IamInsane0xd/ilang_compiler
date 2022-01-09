using ILang.CodeAnalysis.Binding;

namespace ILang.CodeAnalysis;

internal sealed class Evaluator
{
	private readonly BoundStatement _root;
	private readonly Dictionary<VariableSymbol, object?> _variables;
	private object? _lastValue;

	public Evaluator(BoundStatement root, Dictionary<VariableSymbol, object?> variables)
	{
		_root = root;
		_variables = variables;
	}

	public object? Evaluate()
	{
		EvaluateStatement(_root);
		return _lastValue;
	}

	private void EvaluateStatement(BoundStatement node)
	{
		switch (node.Kind)
		{
			case BoundNodeKind.BlockStatement:
				EvaluateBlockStatement((BoundBlockStatement) node);
				break;

			case BoundNodeKind.VariableDeclaration:
				EvaluateVariableDeclaration((BoundVariableDeclaration) node);
				break;

			case BoundNodeKind.IfStatement:
				EvaluateIfStatement((BoundIfStatement) node);
				break;

			case BoundNodeKind.WhileStatement:
				EvaluateWhileStatement((BoundWhileStatement) node);
				break;

			case BoundNodeKind.ForStatement:
				EvaluateForStatement((BoundForStatement) node);
				break;

			case BoundNodeKind.ExpressionStatement:
				EvaluateExpressionStatement((BoundExpressionStatement) node);
				break;

			default:
				throw new Exception($"Unexpected node {node.Kind}");
		}
	}

	private void EvaluateBlockStatement(BoundBlockStatement node)
	{
		foreach (var statement in node.Statements)
			EvaluateStatement(statement);
	}

	private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
	{
		object? value = EvaluateExpression(node.Initializer);
		_variables[node.Variable] = value;
		_lastValue = value;
	}

	private void EvaluateIfStatement(BoundIfStatement node)
	{
		if ((bool) (EvaluateExpression(node.Condition) ?? throw new ArgumentNullException(nameof(node.Condition))))
			EvaluateStatement(node.ThenStatement);

		else if (node.ElseStatement != null)
			EvaluateStatement(node.ElseStatement);
	}

	private void EvaluateWhileStatement(BoundWhileStatement node)
	{
		while ((bool) (EvaluateExpression(node.Condition) ?? throw new ArgumentNullException(nameof(node.Condition))))
			EvaluateStatement(node.Body);
	}

	private void EvaluateForStatement(BoundForStatement node)
	{
		int lowerBound = (int) (EvaluateExpression(node.LowerBound) ?? throw new ArgumentNullException(nameof(node.LowerBound)));
		int upperBound = (int) (EvaluateExpression(node.UpperBound) ?? throw new ArgumentNullException(nameof(node.UpperBound)));

		for (int i = lowerBound; i <= upperBound; i++)
		{
			_variables[node.Variable] = i;
			EvaluateStatement(node.Body);
		}
	}

	private void EvaluateExpressionStatement(BoundExpressionStatement node) => _lastValue = EvaluateExpression(node.Expression);

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

		switch (u.Op.Kind)
		{
			case BoundUnaryOperatorKind.Identity:
				return (int) operand;

			case BoundUnaryOperatorKind.Negation:
				return -(int) operand;

			case BoundUnaryOperatorKind.LogicalNegation:
				return !(bool) operand;

			case BoundUnaryOperatorKind.OnesComplement:
				return ~(int) operand;

			default:
				throw new Exception($"Unexpected unary operator {u.Op}");
		}
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
