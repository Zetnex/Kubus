namespace Kubus.parsing;

public class VariableDecrementNode : StatementNode
{
    public string Name { get; }

    public VariableDecrementNode(string name)
        : base("VariableDecrement")
    {
        Name = name;
    }
}