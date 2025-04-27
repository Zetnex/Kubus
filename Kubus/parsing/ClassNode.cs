namespace Kubus.parsing;

public class ClassNode : AstNode
{
    public string Name { get; }
    public string Parent { get; }
    public string? Interface { get; }
    public List<AstNode> Members { get; } = new();
    public List<ConstantNode> Constants { get; } = new List<ConstantNode>();

    public ClassNode(string name, string parent, string? @interface) : base("Class")
    {
        Name = name;
        Parent = parent;
        Interface = @interface;
    }

    public void AddMember(AstNode member)
    {
        Members.Add(member);
    }
    
    public void AddConstant(ConstantNode constant)
    {
        Constants.Add(constant);
    }
}