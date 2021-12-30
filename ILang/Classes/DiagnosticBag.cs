using System.Collections;
using ILang.Classes.Syntax;

namespace ILang.Classes
{
	internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
	{
		private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();

		public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		internal void AddRange(DiagnosticBag diagnostics)
		{
			_diagnostics.AddRange(diagnostics._diagnostics);
		}

		private void Report(TextSpan span, string message)
		{
			var diagnostic = new Diagnostic(span, message);
			_diagnostics.Add(diagnostic);
		}

		public void ReportInvalidNumber(TextSpan span, string text, Type type)
		{
			var message = $"Error: The number {text} is not a valid {type}";
			Report(span, message);
		}

		internal void ReportBadCharacter(int position, char current)
		{
			var span = new TextSpan(position, 1);
			var message = $"Error: bad character in input '{current}'";
			Report(span, message);
		}

		internal void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind)
		{
			var message = $"Error: Unexpected token <{actualKind}>, expected <{expectedKind}>";
			Report(span, message);
		}

		internal void ReportUndefinedUnaryOperator(TextSpan span, string? operatorText, Type operandType)
		{
			var message = $"Error: Unary operator '{operatorText}' is not defined for type {operandType}";
			Report(span, message);
		}

		internal void ReportUndefinedBinaryOperator(TextSpan span, string? operatorText, Type leftType, Type rightType)
		{
			var message = $"Error: Binary operator '{operatorText}' is not defined for types {leftType} and {rightType}";
			Report(span, message);
		}
	}
}
