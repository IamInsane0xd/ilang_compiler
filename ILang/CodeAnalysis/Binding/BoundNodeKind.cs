namespace ILang.CodeAnalysis.Binding;

internal enum BoundNodeKind
{
	// Statements
	BlockStatement,
	VariableDeclaration,
	ExpressionStatement,

	// Expressions
	LiteralExpression,
	VariableExpression,
	AssignmentExpression,
	UnaryExpression,
	BinaryExpression,
}
