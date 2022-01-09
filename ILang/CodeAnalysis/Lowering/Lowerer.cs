using ILang.CodeAnalysis.Binding;

namespace ILang.CodeAnalysis.Lowering;

internal sealed class Lowerer : BoundTreeRewriter
{
	private Lowerer()
	{
	}

	// public static BoundStatement Lower(BoundStatement statement) => new Lowerer().RewriteStatement(statement);

	public static BoundStatement Lower(BoundStatement statement)
	{
		Lowerer lowerer	= new Lowerer();
		return lowerer.RewriteStatement(statement);
	}
}
