using ILang.CodeAnalysis.Text;

namespace ILang.CodeAnalysis.Syntax;

internal sealed class Lexer
{
	private readonly SourceText _text;
	private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
	private int _position;
	private int _start;
	private SyntaxKind _kind;
	private object? _value;

	public Lexer(SourceText text) => _text = text;

	public DiagnosticBag Diagnostics => _diagnostics;

	private char Current => Peek(0);

	private char Peek(int offset)
	{
		int index = _position + offset;

		if (index >= _text.Length)
			return '\0';

		return _text[index];
	}

	public SyntaxToken Lex()
	{
		_start = _position;
		_kind = SyntaxKind.BadToken;
		_value = null;

		switch (Current)
		{
			case '\0':
				_kind = SyntaxKind.EndOfFileToken;
				break;

			case '+':
				_position++;
				_kind = SyntaxKind.PlusToken;
				break;

			case '-':
				_position++;
				_kind = SyntaxKind.MinusToken;
				break;

			case '*':
				_position++;
				_kind = SyntaxKind.StarToken;
				break;

			case '/':
				_position++;
				_kind = SyntaxKind.SlashToken;
				break;

			case '(':
				_position++;
				_kind = SyntaxKind.OpenParenthesisToken;
				break;

			case ')':
				_position++;
				_kind = SyntaxKind.CloseParenthesisToken;
				break;

			case '{':
				_position++;
				_kind = SyntaxKind.OpenBraceToken;
				break;

			case '}':
				_position++;
				_kind = SyntaxKind.CloseBraceToken;
				break;

			case '~':
				_position++;
				_kind = SyntaxKind.TildeToken;
				break;

			case '^':
				_position++;
				_kind = SyntaxKind.CaretToken;
				break;

			case '&':
				_position++;

				if (Current != '&')
				{
					_kind = SyntaxKind.AmpersandToken;
				}
				else
				{
					_position++;
					_kind = SyntaxKind.AmpersandAmpersandToken;
				}

				break;

			case '|':
				_position++;

				if (Current != '|')
				{
					_kind = SyntaxKind.PipeToken;
				}
				else
				{
					_position++;
					_kind = SyntaxKind.PipePipeToken;
				}

				break;

			case '=':
				_position++;

				if (Current != '=')
				{
					_kind = SyntaxKind.EqualsToken;
				}
				else
				{
					_position++;
					_kind = SyntaxKind.EqualsEqualsToken;
				}

				break;

			case '!':
				_position++;

				if (Current != '=')
				{
					_kind = SyntaxKind.BangToken;
				}
				else
				{
					_position++;
					_kind = SyntaxKind.BangEqualsToken;
				}

				break;

			case '<':
				_position++;

				if (Current != '=')
				{
					_kind = SyntaxKind.LessToken;
				}
				else
				{
					_position++;
					_kind = SyntaxKind.LessOrEqualsToken;
				}

				break;

			case '>':
				_position++;

				if (Current != '=')
				{
					_kind = SyntaxKind.GreaterToken;
				}
				else
				{
					_position++;
					_kind = SyntaxKind.GreaterOrEqualsToken;
				}

				break;

			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				ReadNumberToken();
				break;

			case ' ':
			case '\t':
			case '\n':
			case '\r':
				ReadWhiteSpaceToken();
				break;

			default:
				if (char.IsLetter(Current))
				{
					ReadIdentiferOrKeywordToken();
				}
				else if (char.IsWhiteSpace(Current))
				{
					ReadWhiteSpaceToken();
				}
				else
				{
					_diagnostics.ReportBadCharacter(_position, Current);
					_position++;
				}

				break;
		}

		int length = _position - _start;
		string text = SyntacFacts.GetText(_kind) ?? _text.ToString(_start, length);

		return new SyntaxToken(_kind, _start, text, _value);
	}

	private void ReadNumberToken()
	{
		while (char.IsDigit(Current))
			_position++;

		int length = _position - _start;
		string text = _text.ToString(_start, length);

		if (!int.TryParse(text, out int value))
			_diagnostics.ReportInvalidNumber(new TextSpan(_start, length), text, typeof(int));

		_value = value;
		_kind = SyntaxKind.NumberToken;
	}

	private void ReadWhiteSpaceToken()
	{
		while (char.IsWhiteSpace(Current))
			_position++;

		_kind = SyntaxKind.WhiteSpaceToken;
	}

	private void ReadIdentiferOrKeywordToken()
	{
		while (char.IsLetter(Current))
			_position++;

		int length = _position - _start;
		string text = _text.ToString(_start, length);

		_kind = SyntacFacts.GetKeywordKind(text);
	}
}
