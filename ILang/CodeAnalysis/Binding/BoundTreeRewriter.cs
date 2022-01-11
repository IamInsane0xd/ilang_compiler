using System.Collections.Immutable;

namespace ILang.CodeAnalysis.Binding;

internal abstract class BoundTreeRewriter
{
	public virtual BoundStatement RewriteStatement(BoundStatement node)
	{
		return node.Kind switch
		{
			BoundNodeKind.BlockStatement => RewriteBlockStatement((BoundBlockStatement) node),
			BoundNodeKind.VariableDeclaration => RewriteVariableDeclaration((BoundVariableDeclaration) node),
			BoundNodeKind.IfStatement => RewriteIfStatement((BoundIfStatement) node),
			BoundNodeKind.WhileStatement => RewriteWhileStatement((BoundWhileStatement) node),
			BoundNodeKind.ForStatement => RewriteForStatement((BoundForStatement) node),
			BoundNodeKind.LabelStatement => RewriteLabelStatement((BoundLabelStatement) node),
			BoundNodeKind.GotoStatement => RewriteGotoStatement((BoundGotoStatement) node),
			BoundNodeKind.ConditionalGotoStatement => RewriteConditionalGotoStatement((BoundConditionalGotoStatement) node),
			BoundNodeKind.ExpressionStatement => RewriteExpressionStatement((BoundExpressionStatement) node),
			_ => throw new Exception($"Unexpected node: {node.Kind}"),
		};
	}

	protected virtual BoundStatement RewriteBlockStatement(BoundBlockStatement node)
	{
		ImmutableArray<BoundStatement>.Builder? builder = null;

		for (int i = 0; i < node.Statements.Length; i++)
		{
			BoundStatement oldStatement = node.Statements[i];
			BoundStatement newStatement = RewriteStatement(oldStatement);

			if (newStatement != oldStatement && builder == null)
			{
				builder = ImmutableArray.CreateBuilder<BoundStatement>(node.Statements.Length);

				for (int j = 0; j < i; j++)
					builder.Add(node.Statements[j]);
			}

			if (builder != null)
				builder.Add(newStatement);
		}

		if (builder == null)
			return node;

		return new BoundBlockStatement(builder.MoveToImmutable());
	}

	protected virtual BoundStatement RewriteVariableDeclaration(BoundVariableDeclaration node)
	{
		BoundExpression initializer = RewriteExpression(node.Initializer);

		if (initializer == node.Initializer)
			return node;

		return new BoundVariableDeclaration(node.Variable, initializer);
	}

	protected virtual BoundStatement RewriteIfStatement(BoundIfStatement node)
	{
		BoundExpression condition = RewriteExpression(node.Condition);
		BoundStatement thenStatement = RewriteStatement(node.ThenStatement);
		BoundStatement? elseStatement = node.ElseStatement == null ? null : RewriteStatement(node.ElseStatement);

		if (condition == node.Condition && thenStatement == node.ThenStatement && elseStatement == node.ElseStatement)
			return node;

		return new BoundIfStatement(condition, thenStatement, elseStatement);
	}

	protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement node)
	{
		BoundExpression condition = RewriteExpression(node.Condition);
		BoundStatement body = RewriteStatement(node.Body);

		if (condition == node.Condition && body == node.Body)
			return node;

		return new BoundWhileStatement(condition, body);
	}

	protected virtual BoundStatement RewriteForStatement(BoundForStatement node)
	{
		BoundExpression lowerBound = RewriteExpression(node.LowerBound);
		BoundExpression upperBound = RewriteExpression(node.UpperBound);
		BoundStatement body = RewriteStatement(node.Body);

		if (lowerBound == node.LowerBound && upperBound == node.UpperBound && body == node.Body)
			return node;

		return new BoundForStatement(node.Variable, lowerBound, upperBound, body);
	}

	protected virtual BoundStatement RewriteLabelStatement(BoundLabelStatement node) => node;

	protected virtual BoundStatement RewriteGotoStatement(BoundGotoStatement node) => node;

	protected virtual BoundStatement RewriteConditionalGotoStatement(BoundConditionalGotoStatement node)
	{
		BoundExpression condition = RewriteExpression(node.Condition);

		if (condition == node.Condition)
			return node;

		return new BoundConditionalGotoStatement(node.Label, condition, node.JumpIfFalse);
	}

	protected virtual BoundStatement RewriteExpressionStatement(BoundExpressionStatement node)
	{
		BoundExpression expression = RewriteExpression(node.Expression);

		if (expression == node.Expression)
			return node;

		return new BoundExpressionStatement(expression);
	}

	public virtual BoundExpression RewriteExpression(BoundExpression node)
	{
		return node.Kind switch
		{
			BoundNodeKind.LiteralExpression => RewriteLiteralExpression((BoundLiteralExpression) node),
			BoundNodeKind.VariableExpression => RewriteVariableExpression((BoundVariableExpression) node),
			BoundNodeKind.AssignmentExpression => RewriteAssignmentExpression((BoundAssignmentExpression) node),
			BoundNodeKind.UnaryExpression => RewriteUnaryExpression((BoundUnaryExpression) node),
			BoundNodeKind.BinaryExpression => RewriteBinaryExpression((BoundBinaryExpression) node),
			_ => throw new Exception($"Unexpected node: {node.Kind}"),
		};
	}

	protected virtual BoundExpression RewriteLiteralExpression(BoundLiteralExpression node) => node;

	protected virtual BoundExpression RewriteVariableExpression(BoundVariableExpression node) => node;

	protected virtual BoundExpression RewriteAssignmentExpression(BoundAssignmentExpression node)
	{
		BoundExpression expression = RewriteExpression(node.Expression);

		if (expression == node.Expression)
			return node;

		return new BoundAssignmentExpression(node.Variable, expression);
	}

	protected virtual BoundExpression RewriteUnaryExpression(BoundUnaryExpression node)
	{
		BoundExpression operand = RewriteExpression(node.Operand);

		if (operand == node.Operand)
			return node;

		return new BoundUnaryExpression(node.Op, operand);
	}

	protected virtual BoundExpression RewriteBinaryExpression(BoundBinaryExpression node)
	{
		BoundExpression left = RewriteExpression(node.Left);
		BoundExpression right = RewriteExpression(node.Right);

		if (left == node.Left && right == node.Right)
			return node;

		return new BoundBinaryExpression(left, node.Op, right);
	}
}
