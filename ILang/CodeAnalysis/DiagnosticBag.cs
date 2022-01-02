using ILang.CodeAnalysis.Syntax;
using ILang.CodeAnalysis.Text;
using System.Collections;

namespace ILang.CodeAnalysis;

internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
{
	private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

	public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	internal void AddRange(DiagnosticBag diagnostics) => _diagnostics.AddRange(diagnostics._diagnostics);

	private void Report(TextSpan span, string message)
	{
		Diagnostic? diagnostic = new Diagnostic(span, message);
		_diagnostics.Add(diagnostic);
	}

	public void ReportInvalidNumber(TextSpan span, string text, Type type)
	{
		string? message = $"Error: The number {text} is not a valid {type}";
		Report(span, message);
	}

	public void ReportBadCharacter(int position, char current)
	{
		TextSpan span = new TextSpan(position, 1);
		string? message = $"Error: bad character in input '{current}'";
		Report(span, message);
	}

	public void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind)
	{
		string? message = $"Error: Unexpected token <{actualKind}>, expected <{expectedKind}>";
		Report(span, message);
	}

	public void ReportUndefinedUnaryOperator(TextSpan span, string? operatorText, Type operandType)
	{
		string? message = $"Error: Unary operator '{operatorText}' is not defined for type {operandType}";
		Report(span, message);
	}

	public void ReportUndefinedBinaryOperator(TextSpan span, string? operatorText, Type leftType, Type rightType)
	{
		string? message = $"Error: Binary operator '{operatorText}' is not defined for types {leftType} and {rightType}";
		Report(span, message);
	}

	public void ReportUndefinedName(TextSpan span, string name)
	{
		string? message = $"Variable '{name}' is not defined";
		Report(span, message);
	}
}
