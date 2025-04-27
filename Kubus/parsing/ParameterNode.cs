namespace Kubus.parsing;

public class ParameterNode : AstNode
{
    public string Name { get; }
    public string Type { get; }

    public ParameterNode(string name, string type) : base("Parameter")
    {
        Name = name;
        Type = type;
    }
}