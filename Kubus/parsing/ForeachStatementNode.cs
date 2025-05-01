namespace Kubus.parsing;

public class ForeachStatementNode : StatementNode
{
    public ExpressionNode Iterator { get; }
    public ExpressionNode Collection { get; }
    public BlockNode Block { get; }

    public ForeachStatementNode(ExpressionNode iterator, ExpressionNode collection, BlockNode block) : base("ForeachStatement")
    {
        Iterator = iterator;
        Collection = collection;
        Block = block;
    }
}