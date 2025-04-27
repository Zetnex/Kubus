using Kubus.lexicalization;

namespace Kubus.parsing;

public class LiteralExpressionNode : ExpressionNode
{
    public Token Value { get; }

    public LiteralExpressionNode(Token value) : base("LiteralExpression")
    {
        Value = value;
    }
}