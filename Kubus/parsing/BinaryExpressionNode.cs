using Kubus.lexicalization;

namespace Kubus.parsing;

public class BinaryExpressionNode : ExpressionNode
{
    public ExpressionNode Left { get; }
    public Token Operator { get; }
    public ExpressionNode Right { get; }

    public BinaryExpressionNode(ExpressionNode left, Token operatorToken, ExpressionNode right)
        : base("BinaryExpression")
    {
        Left = left;
        Operator = operatorToken;
        Right = right;
    }
}