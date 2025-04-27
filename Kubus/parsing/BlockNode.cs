namespace Kubus.parsing;

public class BlockNode : StatementNode
{
    public List<StatementNode> Statements { get; }

    public BlockNode(List<StatementNode> statements) 
        : base("Block")
    {
        Statements = statements;
    }
}