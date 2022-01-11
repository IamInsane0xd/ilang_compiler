using ILang.CodeAnalysis.Binding;
using ILang.CodeAnalysis.Lowering;
using ILang.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace ILang.CodeAnalysis;

public sealed class Compilation
{
	private BoundGlobalScope? _globalScope;

	public Compilation(SyntaxTree syntaxTree) : this(null, syntaxTree)
	{
	}

	private Compilation(Compilation? previous, SyntaxTree syntaxTree)
	{
		Previous = previous;
		SyntaxTree = syntaxTree;
	}

	public Compilation? Previous { get; }
	public SyntaxTree SyntaxTree { get; }

	internal BoundGlobalScope GlobalScope
	{
		get
		{
			if (_globalScope == null)
			{
				BoundGlobalScope globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTree.Root);
				Interlocked.CompareExchange(ref _globalScope, globalScope, null);
			}

			return _globalScope;
		}
	}

	public Compilation ContinueWith(SyntaxTree syntaxTree) => new Compilation(this, syntaxTree);

	public EvaluationResult Evaluate(Dictionary<VariableSymbol, object?> variables)
	{
		ImmutableArray<Diagnostic> diagnostics = SyntaxTree.Diagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();

		if (diagnostics.Any())
			return new EvaluationResult(diagnostics, null);

		BoundBlockStatement statement = GetStatement();
		Evaluator evaluator = new Evaluator(statement, variables);
		object? value = evaluator.Evaluate();

		return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
	}

	public void EmitTree(TextWriter writer)
	{
		BoundStatement statement = GetStatement();
		statement.WriteTo(writer);
	}

	private BoundBlockStatement GetStatement()
	{
		BoundStatement result = GlobalScope.Statement;
		return Lowerer.Lower(result);
	}
}
