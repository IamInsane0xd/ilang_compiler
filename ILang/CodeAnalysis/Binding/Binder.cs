using ILang.CodeAnalysis.Syntax;

namespace ILang.CodeAnalysis.Binding;

internal sealed class Binder
{
	private readonly Dictionary<VariableSymbol, object?> _variables;
	private readonly DiagnosticBag _diagnostics = new DiagnosticBag();

	public Binder(Dictionary<VariableSymbol, object?> variables) => _variables = variables;

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
		object? value = syntax.Value ?? 0;
		return new BoundLiteralExpression(value);
	}

	private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
	{
		string? name = syntax.IdentifierToken.Text ?? throw new ArgumentNullException(nameof(name));
		VariableSymbol? variable = _variables.Keys.FirstOrDefault(v => v.Name == name);

		if (variable == null)
		{
			_diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
			return new BoundLiteralExpression(0);
		}

		return new BoundVariableExpression(variable);
	}

	private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
	{
		string? name = syntax.IdentifierToken.Text;
		BoundExpression boundExpression = BindExpression(syntax.Expression);

		if (name == null)
			throw new ArgumentNullException(nameof(name));

		VariableSymbol? existingVariable = _variables.Keys.FirstOrDefault(v => v.Name == name);

		if (existingVariable != null)
			_variables.Remove(existingVariable);

		VariableSymbol? variable = new VariableSymbol(name, boundExpression.Type);

		_variables[variable] = null;

		return new BoundAssignmentExpression(variable, boundExpression);
	}

	private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
	{
		BoundExpression? boundOperand = BindExpression(syntax.Operand);
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
		BoundExpression? boundLeft = BindExpression(syntax.Left);
		BoundExpression? boundRight = BindExpression(syntax.Right);
		BoundBinaryOperator? boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

		if (boundOperator == null)
		{
			_diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
			return boundLeft;
		}

		return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
	}
}
