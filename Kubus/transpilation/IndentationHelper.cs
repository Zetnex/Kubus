namespace Kubus.transpilation;

public class IndentationHandler
{
    private int _indentLevel;
    private const string IndentString = "    "; // 4 spaces

    public void IncreaseIndent() => _indentLevel++;

    public void DecreaseIndent()
    {
        if (_indentLevel > 0)
            _indentLevel--;
    }

    public string ApplyIndent(string code)
    {
        return new string(' ', _indentLevel * IndentString.Length) + code;
    }

    public string ApplyIndentToBlock(IEnumerable<string> lines)
    {
        return string.Join("\n", lines.Select(ApplyIndent));
    }
}