namespace Domain.Teams
{
    public class GoldCoins
    {
        public int Value { get; }

        public GoldCoins(int value)
        {
            Value = value;
        }

        public bool LessThan(GoldCoins teamChest)
        {
            return Value > teamChest.Value;
        }

        public GoldCoins Minus(GoldCoins playerCost)
        {
            return new GoldCoins(Value - playerCost.Value);
        }
    }
}