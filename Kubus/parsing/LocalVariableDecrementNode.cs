namespace Kubus.parsing;

public class LocalVariableDecrementNode : StatementNode
{
    public string Name { get; }

    public LocalVariableDecrementNode(string name)
        : base("LocalVariableDecrement")
    {
        Name = name;
    }
}