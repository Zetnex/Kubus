namespace Kubus.parsing;

public class VariableIncrementNode : StatementNode
{
    public string Name { get; }

    public VariableIncrementNode(string name)
        : base("VariableIncrement")
    {
        Name = name;
    }
}