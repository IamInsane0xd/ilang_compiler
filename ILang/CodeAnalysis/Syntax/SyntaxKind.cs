namespace ILang.CodeAnalysis.Syntax;

public enum SyntaxKind
{
	// Tokens
	BadToken,
	EndOfFileToken,
	WhiteSpaceToken,
	NumberToken,
	PlusToken,
	MinusToken,
	StarToken,
	SlashToken,
	BangToken,
	EqualsToken,
	AmpersandAmpersandToken,
	PipePipeToken,
	EqualsEqualsToken,
	BangEqualsToken,
	OpenParenthesisToken,
	CloseParenthesisToken,
	OpenBraceToken,
	CloseBraceToken,
	IdentifierToken,

	// Keywords
	TrueKeyword,
	FalseKeyword,

	// Nodes
	CompilationUnit,

	//Statements
	BlockStatement,
	ExpressionStatement,

	// Expressions
	LiteralExpression,
	NameExpression,
	UnaryExpression,
	BinaryExpression,
	ParenthesizedExpression,
	AssignmentExpression,
}
