namespace Kubus.parsing;

public class VariableDeclarationNode : StatementNode
{
    public string Name { get; }
    public ExpressionNode Initializer { get; }

    public VariableDeclarationNode(string name, ExpressionNode initializer) 
        : base("VariableDeclaration")
    {
        Name = name;
        Initializer = initializer;
    }
}