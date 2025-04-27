namespace Kubus.parsing;

public abstract class ExpressionNode : AstNode
{
    protected ExpressionNode(string nodeType) : base(nodeType) { }
}