namespace VisitorDesignPattern;

/// <summary>
/// The element abstraction. Every node in a SQL routine accepts a visitor, and the node's
/// runtime type decides which visitor method runs. That is the double dispatch at the heart
/// of the pattern: <c>node.Accept(visitor)</c> becomes <c>visitor.Visit(concreteNode)</c>.
/// </summary>
public abstract class SqlNode
{
    public abstract void Accept(SqlVisitor visitor);
}
