public class SimpleMatchingRule : IMatchingRule
{
    public static SimpleMatchingRule Instance { get; } = new SimpleMatchingRule();

    public bool Matching(IMatchingUnit matchingUnit, long user)
    {
        return true;
    }
}