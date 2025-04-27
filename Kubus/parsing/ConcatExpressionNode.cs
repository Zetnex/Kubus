using Kubus.lexicalization;

namespace Kubus.parsing;

public class ConcatExpressionNode : ExpressionNode
{
    public ExpressionNode Left { get; }
    public Token Operator { get; }
    public ExpressionNode Right { get; }

    public ConcatExpressionNode(ExpressionNode left, Token @operator, ExpressionNode right) : base("ConcatExpression")
    {
        Left = left;
        Operator = @operator;
        Right = right;
    }
}