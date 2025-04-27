namespace Kubus.parsing
{
    public class ConstantAccessNode : ExpressionNode
    {
        public string Name { get; }

        public ConstantAccessNode(string name) : base("ConstantAccess")
        {
            Name = name;
        }
    }
}