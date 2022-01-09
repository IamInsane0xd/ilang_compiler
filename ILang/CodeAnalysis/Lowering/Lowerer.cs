using ILang.CodeAnalysis.Binding;
using ILang.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace ILang.CodeAnalysis.Lowering;

internal sealed class Lowerer : BoundTreeRewriter
{
	private Lowerer()
	{
	}

	public static BoundStatement Lower(BoundStatement statement)
	{
		Lowerer lowerer	= new Lowerer();
		return lowerer.RewriteStatement(statement);
	}

	protected override BoundStatement RewriteForStatement(BoundForStatement node)
	{
		/* 
		 * for <var> = <lower> to <upper>
		 *   <body>
		 * 
		 * --->
		 * 
		 * {
		 *   var <var> = <lower>
		 *   while <var> <= <upper>
		 *   {
		 *     <body>
		 *     <var> = <var> + 1
		 *   }
		 * }
		 */

		BoundVariableDeclaration variableDeclaration = new BoundVariableDeclaration(node.Variable, node.LowerBound);
		BoundVariableExpression variableExpression = new BoundVariableExpression(node.Variable);
		BoundBinaryExpression condition = new BoundBinaryExpression(
			variableExpression,
			(BoundBinaryOperator.Bind(SyntaxKind.LessOrEqualsToken, typeof(int), typeof(int))) ?? throw new ArgumentNullException("op"),
			node.UpperBound
		);
		BoundExpressionStatement increment = new BoundExpressionStatement(
			new BoundAssignmentExpression(
				node.Variable,
				new BoundBinaryExpression(
					variableExpression,
					(BoundBinaryOperator.Bind(SyntaxKind.PlusToken, typeof(int), typeof(int))) ?? throw new ArgumentNullException("op"),
					new BoundLiteralExpression(1)
				)
			)
		);
		BoundBlockStatement whileBody = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(node.Body, increment));
		BoundWhileStatement whileStatement = new BoundWhileStatement(condition, whileBody);
		BoundBlockStatement result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(variableDeclaration, whileStatement));

		return RewriteStatement(result);
	}
}
