namespace Kubus.parsing;

public abstract class StatementNode : AstNode
{
    protected StatementNode(string type) : base(type)
    {
    }
}