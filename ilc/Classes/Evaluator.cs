using ILang.Classes.Syntax;

namespace ILang.Classes
{
	public sealed class Evaluator
	{
		private readonly ExpressionSyntax _root;

		public Evaluator(ExpressionSyntax root)
		{
			_root = root;
		}

		public int Evaluate() => EvaluateExpression(_root);

		private int EvaluateExpression(ExpressionSyntax node)
		{
			if (node is LiteralExpressionSyntax l)
			{
				if (l.LiteralToken.Value == null)
					throw new Exception("value is null");

				return (int) l.LiteralToken.Value;
			}

			if (node is UnaryExpressionSyntax u)
			{
				var operand = EvaluateExpression(u.Operand);

				switch (u.OperatorToken.Kind)
				{
					case SyntaxKind.PlusToken:
						return operand;
					
					case SyntaxKind.MinusToken:
						return -operand;
				}

				throw new Exception($"Unexpected unary operator {u.OperatorToken.Kind}");
			}

			if (node is BinaryExpressionSyntax b)
			{
				var left = EvaluateExpression(b.Left);
				var right = EvaluateExpression(b.Right);

				switch (b.OperatorToken.Kind)
				{
					case SyntaxKind.PlusToken:
						return left + right;
					
					case SyntaxKind.MinusToken:
						return left - right;
					
					case SyntaxKind.StarToken:
						return left * right;
					
					case SyntaxKind.SlashToken:
						return left / right;
				}

				throw new Exception($"Unexpected binary operator {b.OperatorToken.Kind}");
			}

			if (node is ParenthesizedExpressionSyntax p)
				return EvaluateExpression(p.Expression);

			throw new Exception($"Unexpected node {node.Kind}");
		}
	}
}
