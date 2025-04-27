namespace Kubus.parsing;

public class StaticLocalVariableDecrementNode : StatementNode
{
    public string Name { get; }

    public StaticLocalVariableDecrementNode(string name)
        : base("StaticLocalVariableDecrement")
    {
        Name = name;
    }
}