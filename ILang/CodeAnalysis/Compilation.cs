using ILang.CodeAnalysis.Binding;
using ILang.CodeAnalysis.Syntax;

namespace ILang.CodeAnalysis;

public sealed class Compilation
{
	public Compilation(SyntaxTree syntax)
	{
		Syntax = syntax;
	}

	public SyntaxTree Syntax { get; }

	public EvaluationResult Evaluate(Dictionary<VariableSymbol, object?> variables)
	{
		var binder = new Binder(variables);
		var boundExpression = binder.BindExpression(Syntax.Root);
		var evaluator = new Evaluator(boundExpression, variables);
		var diagnostics = Syntax.Diagnostics.Concat(binder.Diagnostics).ToArray();

		if (diagnostics.Any())
			return new EvaluationResult(diagnostics, null);

		var value = evaluator.Evaluate();

		return new EvaluationResult(Array.Empty<Diagnostic>(), value);
	}
}
