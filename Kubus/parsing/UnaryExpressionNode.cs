using Kubus.lexicalization;

namespace Kubus.parsing;

public class UnaryExpressionNode : ExpressionNode
{
    public Token Operator { get; }
    public ExpressionNode Operand { get; }

    public UnaryExpressionNode(Token operatorToken, ExpressionNode operand)
        : base("UnaryExpression")
    {
        Operator = operatorToken;
        Operand = operand;
    }
}