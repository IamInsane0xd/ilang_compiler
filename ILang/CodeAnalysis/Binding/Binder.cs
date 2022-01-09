using ILang.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace ILang.CodeAnalysis.Binding;

internal sealed class Binder
{
	private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
	private BoundScope? _scope;

	public Binder(BoundScope? parent)
	{
		_scope = new BoundScope(parent);
	}

	public static BoundGlobalScope BindGlobalScope(BoundGlobalScope? previous, CompilationUnitSyntax syntax)
	{
		BoundScope? parentScope = CreateParentScopes(previous);
		Binder binder = new Binder(parentScope);
		BoundStatement statement = binder.BindStatement(syntax.Statement);
		ImmutableArray<VariableSymbol> variables = binder._scope?.GetDeclaredVariables() ?? throw new ArgumentNullException(nameof(_scope));
		ImmutableArray<Diagnostic> diagnostics = binder.Diagnostics.ToImmutableArray();

		if (previous != null)
			diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

		return new BoundGlobalScope(previous, diagnostics, variables, statement);
	}

	private static BoundScope? CreateParentScopes(BoundGlobalScope? previous)
	{
		Stack<BoundGlobalScope> stack = new Stack<BoundGlobalScope>();

		while (previous != null)
		{
			stack.Push(previous);
			previous = previous.Previous;
		}

		BoundScope? parent = null;

		while (stack.Count > 0)
		{
			previous = stack.Pop();
			BoundScope scope = new BoundScope(parent);
			
			foreach (var v in previous.Variables)
				scope.TryDeclare(v);

			parent = scope;
		}

		return parent;
	}

	public DiagnosticBag Diagnostics => _diagnostics;

	private BoundStatement BindStatement(StatementSyntax syntax)
	{
		return syntax.Kind switch
		{
			SyntaxKind.BlockStatement => BindBlockStatement((BlockStatementSyntax) syntax),
			SyntaxKind.VariableDeclaration => BindVariableDeclaration((VariableDeclarationSyntax) syntax),
			SyntaxKind.IfStatement => BindIfStatement((IfStatementSyntax) syntax),
			SyntaxKind.WhileStatement => BindWhileStatement((WhileStatementSyntax) syntax),
			SyntaxKind.ForStatement => BindForStatement((ForStatementSyntax) syntax),
			SyntaxKind.ExpressionStatement => BindExpressionStatement((ExpressionStatementSyntax) syntax),
			_ => throw new Exception($"Unexpected syntax {syntax.Kind}"),
		};
	}

	private BoundStatement BindBlockStatement(BlockStatementSyntax syntax)
	{
		ImmutableArray<BoundStatement>.Builder statements = ImmutableArray.CreateBuilder<BoundStatement>();
		_scope = new BoundScope(_scope);

		foreach (StatementSyntax statementSyntax in syntax.Statements)
		{
			BoundStatement statement = BindStatement(statementSyntax);
			statements.Add(statement);
		}

		_scope = _scope.Parent;

		return new BoundBlockStatement(statements.ToImmutable());
	}

	private BoundStatement BindVariableDeclaration(VariableDeclarationSyntax syntax)
	{
		string name = syntax.Identifier.Text ?? throw new ArgumentNullException(nameof(syntax.Identifier.Text));
		bool isReadOnly = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
		BoundExpression initializer = BindExpression(syntax.Initializer);
		VariableSymbol variable = new VariableSymbol(name, isReadOnly, initializer.Type);

		if (!_scope?.TryDeclare(variable) ?? throw new ArgumentNullException(nameof(_scope)))
			_diagnostics.ReportVariableAlreadyDeclared(syntax.Identifier.Span, name);

		return new BoundVariableDeclaration(variable, initializer);
	}

	private BoundStatement BindIfStatement(IfStatementSyntax syntax)
	{
		BoundExpression condition = BindExpression(syntax.Condition, typeof(bool));
		BoundStatement thenStatement = BindStatement(syntax.ThenStatement);
		BoundStatement? elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);

		return new BoundIfStatement(condition, thenStatement, elseStatement);
	}

	private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
	{
		BoundExpression condition = BindExpression(syntax.Condition, typeof(bool));
		BoundStatement body = BindStatement(syntax.Body);

		return new BoundWhileStatement(condition, body);
	}

	private BoundStatement BindForStatement(ForStatementSyntax syntax)
	{
		BoundExpression lowerBound = BindExpression(syntax.LowerBound, typeof(int));
		BoundExpression upperBound = BindExpression(syntax.UpperBound, typeof(int));

		_scope = new BoundScope(_scope);

		string name = syntax.Identifier.Text ?? throw new ArgumentNullException(nameof(syntax.Identifier.Text));
		VariableSymbol variable = new VariableSymbol(name, true, typeof(int));

		if (!_scope?.TryDeclare(variable) ?? throw new ArgumentNullException(nameof(_scope)))
			_diagnostics.ReportVariableAlreadyDeclared(syntax.Identifier.Span, name);

		BoundStatement body = BindStatement(syntax.Body);

		_scope = _scope?.Parent;

		return new BoundForStatement(variable, lowerBound, upperBound, body);
	}

	private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
	{
		BoundExpression expression = BindExpression(syntax.Expression);
		return new BoundExpressionStatement(expression);
	}

	private BoundExpression BindExpression(ExpressionSyntax syntax, Type targetType)
	{
		BoundExpression result = BindExpression(syntax);

		if (result.Type != targetType)
			_diagnostics.ReportCannotConvert(syntax.Span, result.Type, targetType);

		return result;
	}

	private BoundExpression BindExpression(ExpressionSyntax syntax)
	{
		return syntax.Kind switch
		{
			SyntaxKind.ParenthesizedExpression => BindParenthesizedExpression((ParenthesizedExpressionSyntax) syntax),
			SyntaxKind.LiteralExpression => BindLiteralExpression((LiteralExpressionSyntax) syntax),
			SyntaxKind.NameExpression => BindNameExpression((NameExpressionSyntax) syntax),
			SyntaxKind.AssignmentExpression => BindAssignmentExpression((AssignmentExpressionSyntax) syntax),
			SyntaxKind.UnaryExpression => BindUnaryExpression((UnaryExpressionSyntax) syntax),
			SyntaxKind.BinaryExpression => BindBinaryExpression((BinaryExpressionSyntax) syntax),
			_ => throw new Exception($"Unexpected syntax {syntax.Kind}"),
		};
	}

	private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax) => BindExpression(syntax.Expression);

	private static BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
	{
		object value = syntax.Value ?? 0;
		return new BoundLiteralExpression(value);
	}

	private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
	{
		string name = syntax.IdentifierToken.Text;

		if (string.IsNullOrEmpty(name))
			return new BoundLiteralExpression(0);

		if (_scope == null)
			throw new ArgumentNullException(nameof(_scope));

		if (!_scope.TryLookup(name, out VariableSymbol? variable))
		{
			_diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
			return new BoundLiteralExpression(0);
		}

		if (variable == null)
			throw new ArgumentNullException(nameof(variable));

		return new BoundVariableExpression(variable);
	}

	private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
	{
		string name = syntax.IdentifierToken.Text ?? throw new ArgumentNullException(nameof(syntax.IdentifierToken.Text));
		BoundExpression boundExpression = BindExpression(syntax.Expression);

		if (_scope == null)
			throw new ArgumentNullException(nameof(_scope));

		if (!_scope.TryLookup(name, out VariableSymbol? variable))
		{
			_diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
			return boundExpression;
		}

		if (variable == null)
			throw new ArgumentNullException(nameof(variable));

		if (variable.IsReadOnly)
			_diagnostics.ReportCannotAssign(syntax.EqualsToken.Span, name);

		if (boundExpression.Type != variable.Type)
		{
			_diagnostics.ReportCannotConvert(syntax.Expression.Span, boundExpression.Type, variable.Type);
			return boundExpression;
		}

		return new BoundAssignmentExpression(variable, boundExpression);
	}

	private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
	{
		BoundExpression boundOperand = BindExpression(syntax.Operand);
		BoundUnaryOperator? boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);

		if (boundOperator == null)
		{
			_diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundOperand.Type);
			return boundOperand;
		}

		return new BoundUnaryExpression(boundOperator, boundOperand);
	}

	private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
	{
		BoundExpression boundLeft = BindExpression(syntax.Left);
		BoundExpression boundRight = BindExpression(syntax.Right);
		BoundBinaryOperator? boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

		if (boundOperator == null)
		{
			_diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
			return boundLeft;
		}

		return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
	}
}
