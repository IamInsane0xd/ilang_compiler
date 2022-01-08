using ILang.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace ILang.CodeAnalysis.Binding;

internal sealed class Binder
{
	private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
	private BoundScope _scope;

	public Binder(BoundScope? parent)
	{
		_scope = new BoundScope(parent);
	}

	public static BoundGlobalScope BindGlobalScope(BoundGlobalScope? previous, CompilationUnitSyntax syntax)
	{
		BoundScope? parentScope = CreateParentScopes(previous);
		Binder binder = new Binder(parentScope);
		BoundExpression expression = binder.BindExpression(syntax.Expression);
		ImmutableArray<VariableSymbol> variables = binder._scope.GetDeclaredVariables();
		ImmutableArray<Diagnostic> diagnostics = binder.Diagnostics.ToImmutableArray();

		if (previous != null)
			diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

		return new BoundGlobalScope(previous, diagnostics, variables, expression);
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

	public BoundExpression BindExpression(ExpressionSyntax syntax)
	{
		switch (syntax.Kind)
		{
			case SyntaxKind.ParenthesizedExpression:
				return BindParenthesizedExpression((ParenthesizedExpressionSyntax) syntax);

			case SyntaxKind.LiteralExpression:
				return BindLiteralExpression((LiteralExpressionSyntax) syntax);

			case SyntaxKind.NameExpression:
				return BindNameExpression((NameExpressionSyntax) syntax);

			case SyntaxKind.AssignmentExpression:
				return BindAssignmentExpression((AssignmentExpressionSyntax) syntax);

			case SyntaxKind.UnaryExpression:
				return BindUnaryExpression((UnaryExpressionSyntax) syntax);

			case SyntaxKind.BinaryExpression:
				return BindBinaryExpression((BinaryExpressionSyntax) syntax);
		}

		throw new Exception($"Unexpected syntax {syntax.Kind}");
	}

	private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax) => BindExpression(syntax.Expression);

	private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
	{
		object value = syntax.Value ?? 0;
		return new BoundLiteralExpression(value);
	}

	private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
	{
		string? name = syntax.IdentifierToken.Text;

		if (name == null)
		{
			_diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, "");
			return new BoundLiteralExpression(0);
		}

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
		string? name = syntax.IdentifierToken.Text;
		BoundExpression boundExpression = BindExpression(syntax.Expression);

		if (name == null)
			throw new ArgumentNullException(nameof(name));

		if (!_scope.TryLookup(name, out VariableSymbol? variable))
		{
			variable = new VariableSymbol(name, boundExpression.Type);
			_scope.TryDeclare(variable);
		}

		if (variable == null)
			throw new ArgumentNullException(nameof(variable));

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
