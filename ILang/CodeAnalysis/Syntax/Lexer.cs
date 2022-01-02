using ILang.CodeAnalysis.Text;

namespace ILang.CodeAnalysis.Syntax;

internal sealed class Lexer
{
	private readonly string _text;
	private readonly DiagnosticBag _diagnostics = new DiagnosticBag();
	private int _position;
	private int _start;
	private SyntaxKind _kind;
	private object? _value;

	public Lexer(string text)
	{
		_text = text;
	}

	public DiagnosticBag Diagnostics => _diagnostics;

	private char Current => Peek(0);

	private char Lookahead => Peek(1);

	private char Peek(int offset)
	{
		var index = _position + offset;

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

			case '&':
				if (Lookahead == '&')
				{
					_position += 2;
					_kind = SyntaxKind.AmpersandAmpersandToken;
				}

				break;

			case '|':
				if (Lookahead == '|')
				{
					_position += 2;
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

		var length = _position - _start;
		var text = SyntacFacts.GetText(_kind);

		if (text == null)
			text = _text.Substring(_start, length);

		return new SyntaxToken(_kind, _start, text, _value);
	}

	private void ReadNumberToken()
	{
		while (char.IsDigit(Current))
			_position++;

		var length = _position - _start;
		var text = _text.Substring(_start, length);

		if (!int.TryParse(text, out var value))
			_diagnostics.ReportInvalidNumber(new TextSpan(_start, length), _text, typeof(int));

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

		var length = _position - _start;
		var text = _text.Substring(_start, length);

		_kind = SyntacFacts.GetKeywordKind(text);
	}
}
