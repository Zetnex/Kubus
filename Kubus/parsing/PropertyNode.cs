namespace Kubus.parsing;

public class PropertyNode : AstNode
{
    public string Visibility { get; }
    public bool IsStatic { get; }
    public string Name { get; }
    public ExpressionNode? InitialValue { get; }

    public PropertyNode(string visibility, bool isStatic, string name, ExpressionNode? initialValue)
        : base("Property")
    {
        Visibility = visibility;
        IsStatic = isStatic;
        Name = name;
        InitialValue = initialValue;
    }
}