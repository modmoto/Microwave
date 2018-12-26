namespace TestHost.Write.Domain
{
    public class GoldCoins
    {
        public int Value { get; }

        public GoldCoins(int value)
        {
            Value = value;
        }

        public bool LessThan(GoldCoins otherValue)
        {
            return Value < otherValue.Value;
        }

        public GoldCoins Minus(GoldCoins otherCost)
        {
            return new GoldCoins(Value - otherCost.Value);
        }
    }
}