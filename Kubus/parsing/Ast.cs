namespace Kubus.parsing;

public class Ast
{
    public string Namespace { get; set; }
    public List<string> Imports { get; } = new();
    public List<ClassNode> Classes { get; } = new();
}