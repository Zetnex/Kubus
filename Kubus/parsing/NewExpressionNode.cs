namespace Kubus.parsing;

public class NewExpressionNode : ExpressionNode
{
    public string TypeName { get; }
    public List<ExpressionNode> Arguments { get; }

    public NewExpressionNode(string typeName, List<ExpressionNode> arguments) : base("NewExpression")
    {
        TypeName = typeName;
        Arguments = arguments;
    }
}