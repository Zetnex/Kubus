namespace Kubus.parsing;

public class StaticPropertyAccessNode : ExpressionNode
{
    public string ClassName { get; }
    public string PropertyName { get; }

    public StaticPropertyAccessNode(string className, string propertyName) : base("StaticPropertyAccess")
    {
        ClassName = className;
        PropertyName = propertyName;
    }
}