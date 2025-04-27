namespace Kubus.parsing;

public class PropertyAccessNode : ExpressionNode
{
    public ExpressionNode Receiver { get; } // e.g., ThisNode for $this
    public string PropertyName { get; }    // e.g., "logger"
    public PropertyAccessNode(ExpressionNode receiver, string propertyName) : base("PropertyAccess")
    {
        Receiver = receiver;
        PropertyName = propertyName;
    }
}