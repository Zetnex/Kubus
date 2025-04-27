namespace Kubus.parsing;

public class StaticLocalVariableIncrementNode : StatementNode
{
    public string Name { get; }

    public StaticLocalVariableIncrementNode(string name)
        : base("StaticLocalVariableIncrement")
    {
        Name = name;
    }
}