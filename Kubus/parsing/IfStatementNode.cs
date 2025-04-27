namespace Kubus.parsing
{
    public class IfStatementNode : StatementNode
    {
        public ExpressionNode Condition { get; }
        public BlockNode ThenBlock { get; }
        public BlockNode? ElseBlock { get; }

        public IfStatementNode(ExpressionNode condition, BlockNode thenBlock, BlockNode? elseBlock = null) : base("IfStatement")
        {
            Condition = condition;
            ThenBlock = thenBlock;
            ElseBlock = elseBlock;
        }
    }
}