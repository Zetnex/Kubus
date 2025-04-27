namespace Kubus.parsing;

public class ConstructorNode : StatementNode
{
    public List<ParameterNode> Parameters { get; }
    public BlockNode Body { get; }

    public ConstructorNode(List<ParameterNode> parameters, BlockNode body)
        : base("Constructor")
    {
        Parameters = parameters;
        Body = body;
    }
}