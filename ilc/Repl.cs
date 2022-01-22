using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace ILang;

internal abstract class Repl
{
	private bool _done;

	public void Run()
	{
		while (true)
		{
			string? text = EditSubmission();

			if (string.IsNullOrEmpty(text))
				continue;

			if (!text.Contains(Environment.NewLine) && text.StartsWith("#"))
				EvaluateMetaCommand(text);

			else
				EvaluateSumission(text);
		}
	}

	private sealed class SubmissionView
	{
		private readonly ObservableCollection<string> _submissionDocument;
		private readonly int _cursorTop;
		private int _renderedLineCount;
		private int _currentLineIndex;
		private int _currentCharacter;

		public SubmissionView(ObservableCollection<string> submissionDocument)
		{
			_submissionDocument = submissionDocument;
			_submissionDocument.CollectionChanged += SubmissionDocumentChanged;
			_cursorTop = Console.CursorTop;
			Render();
		}

		private void SubmissionDocumentChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			Render();
		}

		private void Render()
		{
			Console.SetCursorPosition(0, _cursorTop);
			Console.CursorVisible = false;

			int lineCount = 0;

			foreach (string line in _submissionDocument)
			{
				Console.ForegroundColor = ConsoleColor.DarkGreen;

				if (lineCount == 0)
					Console.Write("» ");

				else
					Console.Write("· ");

				Console.ResetColor();
				Console.WriteLine(line);
				Console.Write(new string(' ', Console.WindowWidth - line.Length - 1));

				lineCount++;
			}

			int numberOfBlankLines = _renderedLineCount - lineCount;

			if (numberOfBlankLines > 0)
			{
				string blankLine = new string(' ', Console.WindowWidth);

				while (numberOfBlankLines > 0)
					Console.WriteLine(blankLine);
			}

			_renderedLineCount = lineCount;
			Console.CursorVisible = true;

			UpdateCursorPosition();
		}

		private void UpdateCursorPosition()
		{
			Console.CursorTop = _cursorTop + _currentLineIndex;
			Console.CursorLeft = 2 + _currentCharacter;
		}

		public int CurrentLineIndex
		{
			get => _currentLineIndex;

			set
			{
				if (_currentLineIndex != value)
				{
					_currentLineIndex = value;
					UpdateCursorPosition();
				}
			}
		}

		public int CurrentCharacter
		{
			get => _currentCharacter;

			set
			{
				if (_currentCharacter != value)
				{
					_currentCharacter = value;
					UpdateCursorPosition();
				}
			}
		}
	}

	private string? EditSubmission()
	{
		_done = false;
		ObservableCollection<string> document = new ObservableCollection<string>() { "" };
		SubmissionView view = new SubmissionView(document);

		while (!_done)
		{
			ConsoleKeyInfo key = Console.ReadKey(true);
			HandleKey(key, document, view);
		}

		Console.WriteLine();

		return string.Join(Environment.NewLine, document);
	}

	private void HandleKey(ConsoleKeyInfo key, ObservableCollection<string> document, SubmissionView view)
	{
		if (key.Modifiers == default(ConsoleModifiers))
		{
			switch (key.Key)
			{
				case ConsoleKey.Enter:
					HandleEnter(document, view);
					break;

				case ConsoleKey.Backspace:
					HandleBackspace(document, view);
					break;

				case ConsoleKey.Delete:
					HandleDelete(document, view);
					break;

				case ConsoleKey.LeftArrow:
					HandleLeftArrow(document, view);
					break;

				case ConsoleKey.RightArrow:
					HandleRightArrow(document, view);
					break;

				case ConsoleKey.UpArrow:
					HandleUpArrow(document, view);
					break;

				case ConsoleKey.DownArrow:
					HandleDownArrow(document, view);
					break;
			}
		}

		else if (key.Modifiers == ConsoleModifiers.Control)
		{
			switch (key.Key)
			{
				case ConsoleKey.Enter:
					HandleControlEnter(document, view);
					break;
			}
		}

		if (key.KeyChar >= ' ')
			HandleTyping(document, view, key.KeyChar.ToString());
	}

	private void HandleEnter(ObservableCollection<string> document, SubmissionView view)
	{
		string submissionText = string.Join(Environment.NewLine, document);

		if (submissionText.StartsWith("#") || IsCompleteSubmission(submissionText))
		{
			_done = true;
			return;
		}

		document.Add(string.Empty);
		view.CurrentCharacter = 0;
		view.CurrentLineIndex = document.Count - 1;
	}

	private void HandleControlEnter(ObservableCollection<string> document, SubmissionView view) => _done = true;

	private void HandleBackspace(ObservableCollection<string> document, SubmissionView view)
	{
		int start = view.CurrentCharacter;

		if (start == 0)
			return;

		int lineIndex = view.CurrentLineIndex;
		string line = document[lineIndex];
		string before = line[0..(start - 1)];
		string after = line[start..];
		document[lineIndex] = before + after;
		view.CurrentCharacter--;
	}

	private void HandleDelete(ObservableCollection<string> document, SubmissionView view)
	{
		int lineIndex = view.CurrentLineIndex;
		string line = document[lineIndex];
		int start = view.CurrentCharacter;

		if (start >= line.Length - 1)
			return;

		string before = line[0..start];
		string after = line[(start + 1)..];
		document[lineIndex] = before + after;
	}

	private void HandleLeftArrow(ObservableCollection<string> document, SubmissionView view)
	{
		if (view.CurrentCharacter > 0)
			view.CurrentCharacter--;
	}

	private void HandleRightArrow(ObservableCollection<string> document, SubmissionView view)
	{
		string line = document[view.CurrentLineIndex];

		if (view.CurrentCharacter < line.Length)
			view.CurrentCharacter++;
	}

	private void HandleUpArrow(ObservableCollection<string> document, SubmissionView view)
	{
		if (view.CurrentLineIndex > 0)
			view.CurrentLineIndex--;

		string line = document[view.CurrentLineIndex];

		if (view.CurrentCharacter >= line.Length)
			view.CurrentCharacter = line.Length;
	}

	private void HandleDownArrow(ObservableCollection<string> document, SubmissionView view)
	{
		if (view.CurrentLineIndex < document.Count - 1)
			view.CurrentLineIndex++;

		string line = document[view.CurrentLineIndex];

		if (view.CurrentCharacter >= line.Length)
			view.CurrentCharacter = line.Length;
	}

	private void HandleTyping(ObservableCollection<string> document, SubmissionView view, string text)
	{
		int lineIndex = view.CurrentLineIndex;
		int start = view.CurrentCharacter;
		document[lineIndex] = document[lineIndex].Insert(start, text);
		view.CurrentCharacter += text.Length;
	}

	protected virtual void EvaluateMetaCommand(string input)
	{
		Console.ForegroundColor = ConsoleColor.DarkRed;
		Console.WriteLine($"Error: Invalid command {input}\n");
		Console.ResetColor();
	}

	protected abstract bool IsCompleteSubmission(string text);

	protected abstract void EvaluateSumission(string text);
}
