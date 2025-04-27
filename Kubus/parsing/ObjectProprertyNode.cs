namespace Kubus.parsing;

public class ObjectProprertyNode : ExpressionNode
{
    public string PropertyName { get; }

    public ObjectProprertyNode(string propertyName) : base("ObjectProperty")
    {
        PropertyName = propertyName;
    }
}