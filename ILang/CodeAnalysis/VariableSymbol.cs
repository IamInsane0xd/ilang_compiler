namespace ILang.CodeAnalysis;
public sealed class VariableSymbol
{
	public VariableSymbol(string name, bool isReadOnly, Type type)
	{
		Name = name;
		IsReadOnly = isReadOnly;
		Type = type;
	}

	public string Name { get; }
	public bool IsReadOnly { get; }
	public Type Type { get; }
}
