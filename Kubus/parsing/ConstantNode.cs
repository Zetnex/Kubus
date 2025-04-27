namespace Kubus.parsing;

public class ConstantNode : AstNode
{
    public string Visibility { get; }
    public string Name { get; }
    public ExpressionNode Value { get; }

    public ConstantNode(string visibility, string name, ExpressionNode value) : base("Constant")
    {
        Visibility = visibility;
        Name = name;
        Value = value;
    }
}