namespace Kubus.parsing;

public class LocalVariableIncrementNode : StatementNode
{
    public string Name { get; }

    public LocalVariableIncrementNode(string name)
        : base("LocalVariableIncrement")
    {
        Name = name;
    }
}