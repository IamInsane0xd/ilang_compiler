using ILang.CodeAnalysis.Binding;
using ILang.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace ILang.CodeAnalysis;

public sealed class Compilation
{
	public Compilation(SyntaxTree syntax) => Syntax = syntax;

	public SyntaxTree Syntax { get; }

	public EvaluationResult Evaluate(Dictionary<VariableSymbol, object?> variables)
	{
		Binder? binder = new Binder(variables);
		BoundExpression? boundExpression = binder.BindExpression(Syntax.Root);
		Evaluator? evaluator = new Evaluator(boundExpression, variables);
		ImmutableArray<Diagnostic> diagnostics = Syntax.Diagnostics.Concat(binder.Diagnostics).ToImmutableArray();

		if (diagnostics.Any())
			return new EvaluationResult(diagnostics, null);

		object? value = evaluator.Evaluate();

		return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
	}
}
