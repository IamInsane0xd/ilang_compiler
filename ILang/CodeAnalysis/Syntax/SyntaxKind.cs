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
	LessToken,
	LessOrEqualsToken,
	GreaterToken,
	GreaterOrEqualsToken,
	OpenParenthesisToken,
	CloseParenthesisToken,
	OpenBraceToken,
	CloseBraceToken,
	IdentifierToken,

	// Keywords
	TrueKeyword,
	FalseKeyword,
	LetKeyword,
	VarKeyword,
	ToKeyword,
	IfKeyword,
	ElseKeyword,
	WhileKeyword,
	ForKeyword,

	// Nodes
	CompilationUnit,
	ElseClause,

	//Statements
	BlockStatement,
	VariableDeclaration,
	IfStatement,
	WhileStatement,
	ForStatement,
	ExpressionStatement,

	// Expressions
	LiteralExpression,
	NameExpression,
	UnaryExpression,
	BinaryExpression,
	ParenthesizedExpression,
	AssignmentExpression,
}
