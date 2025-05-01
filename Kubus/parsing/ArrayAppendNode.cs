namespace Kubus.parsing;

public class ArrayAppendNode : StatementNode
{
    public string Name { get; }
    public ExpressionNode? Index { get; }
    public ExpressionNode Value { get; }
    
    public ArrayAppendNode(string name, ExpressionNode? index, ExpressionNode value) : base("ArrayAppend")
    {
        Name = name;
        Index = index;
        Value = value;
    }
}