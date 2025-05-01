namespace Kubus.parsing;

public class ArrayExpressionNode : ExpressionNode
{
    public List<ExpressionNode> Elements { get; }

    public ArrayExpressionNode(List<ExpressionNode> elements) : base("ArrayExpression")
    {
        Elements = elements;
    }
}