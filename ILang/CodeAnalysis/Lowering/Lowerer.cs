using ILang.CodeAnalysis.Binding;
using ILang.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace ILang.CodeAnalysis.Lowering;

internal sealed class Lowerer : BoundTreeRewriter
{
	private int _labelCount;

	private Lowerer()
	{
	}

	private LabelSymbol GenerateLabel()
	{
		string name = $"Label{++_labelCount}";
		return new LabelSymbol(name);
	}

	public static BoundBlockStatement Lower(BoundStatement statement)
	{
		Lowerer lowerer = new Lowerer();
		BoundStatement result = lowerer.RewriteStatement(statement);
		return Flatten(result);
	}

	private static BoundBlockStatement Flatten(BoundStatement statement)
	{
		ImmutableArray<BoundStatement>.Builder builder = ImmutableArray.CreateBuilder<BoundStatement>();
		Stack<BoundStatement> stack = new Stack<BoundStatement>();

		stack.Push(statement);

		while (stack.Count > 0)
		{
			BoundStatement current = stack.Pop();

			if (current is BoundBlockStatement b)
			{
				foreach (BoundStatement s in b.Statements.Reverse())
					stack.Push(s);
			}

			else
				builder.Add(current);
		}

		return new BoundBlockStatement(builder.ToImmutable());
	}

	protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
	{
		/*
		 * if <condition>
		 *   <then>
		 *   
		 * --->
		 * 
		 * gotoFalse <condition> end
		 * <then>
		 * end:
		 * 
		 * ==================================
		 * 
		 * if <condition>
		 *   <then>
		 * 
		 * else
		 *   <else>
		 * 
		 * --->
		 * 
		 * gotoFalse <condition> end
		 * <then>
		 * goto end
		 * 
		 * else:
		 * <else>
		 * 
		 * end:
		 */

		if (node.ElseStatement == null)
		{
			LabelSymbol endLabel = GenerateLabel();
			BoundConditionalGotoStatement gotoFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, true);
			BoundLabelStatement endLabelStatelemt = new BoundLabelStatement(endLabel);
			BoundBlockStatement result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(gotoFalse, node.ThenStatement, endLabelStatelemt));

			return RewriteStatement(result);
		}

		else
		{
			LabelSymbol elseLabel = GenerateLabel();
			LabelSymbol endLabel = GenerateLabel();
			BoundConditionalGotoStatement gotoFalse = new BoundConditionalGotoStatement(elseLabel, node.Condition, true);
			BoundGotoStatement gotoEndStatement = new BoundGotoStatement(endLabel);
			BoundLabelStatement elseLabelStatelemt = new BoundLabelStatement(elseLabel);
			BoundLabelStatement endLabelStatelemt = new BoundLabelStatement(endLabel);
			BoundBlockStatement result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
				gotoFalse,
				node.ThenStatement,
				gotoEndStatement,
				elseLabelStatelemt,
				node.ElseStatement,
				endLabelStatelemt
			));

			return RewriteStatement(result);
		}
	}

	protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
	{
		/*
		 * while <condition>
		 *   <body>
		 * 
		 * --->
		 * 
		 * goto check
		 * 
		 * continue:
		 * <body>
		 * 
		 * check:
		 * gotoTrue <condition> continue
		 *  
		 * end:
		 */

		LabelSymbol continueLabel = GenerateLabel();
		LabelSymbol checkLabel = GenerateLabel();
		LabelSymbol endLabel = GenerateLabel();
		BoundGotoStatement gotoCheck = new BoundGotoStatement(checkLabel);
		BoundLabelStatement continueLabelStatement = new BoundLabelStatement(continueLabel);
		BoundLabelStatement checkLabelStatement = new BoundLabelStatement(checkLabel);
		BoundConditionalGotoStatement gotoTrue = new BoundConditionalGotoStatement(continueLabel, node.Condition, false);
		BoundLabelStatement endLabelStatement = new BoundLabelStatement(endLabel);
		BoundBlockStatement result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
			gotoCheck,
			continueLabelStatement,
			node.Body,
			checkLabelStatement,
			gotoTrue,
			endLabelStatement
		));

		return RewriteStatement(result);
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
