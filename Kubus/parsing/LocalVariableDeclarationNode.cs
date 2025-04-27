namespace Kubus.parsing;

public class LocalVariableDeclarationNode : StatementNode
{
    public string Name { get; }
    public ExpressionNode Initializer { get; }

    public LocalVariableDeclarationNode(string name, ExpressionNode initializer)
        : base("LocalVariableDeclaration")
    {
        Name = name;
        Initializer = initializer;
    }
}