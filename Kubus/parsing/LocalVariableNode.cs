namespace Kubus.parsing;

public class LocalVariableNode : ExpressionNode
{
    public string Name { get; }

    public LocalVariableNode(string name) : base("LocalVariable")
    {
        Name = name;
    }
}