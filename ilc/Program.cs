namespace ILang;

internal static class Program
{
	private static void Main()
	{
		Repl repl = new IlangRepl();
		repl.Run();
	}
}
