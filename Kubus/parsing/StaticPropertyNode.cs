namespace Kubus.parsing;

public class StaticPropertyNode : ExpressionNode
{
    public string PropertyName { get; }

    public StaticPropertyNode(string propertyName) : base("StaticProperty")
    {
        PropertyName = propertyName;
    }
}