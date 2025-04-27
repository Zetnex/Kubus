namespace Kubus.parsing;

public abstract class AstNode
{
    public string NodeType { get; }

    protected AstNode(string nodeType)
    {
        NodeType = nodeType;
    }
}