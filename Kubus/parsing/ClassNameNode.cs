namespace Kubus.parsing;

public class ClassNameNode : ExpressionNode {
    public string Name { get; }
    
    public ClassNameNode(string name) : base("ClassName")
    {
        Name = name;
    }
}