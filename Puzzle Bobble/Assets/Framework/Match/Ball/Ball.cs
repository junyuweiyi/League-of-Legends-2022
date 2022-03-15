namespace Core
{
    public class Ball
    {
        public ItemKind ItemKind { get; }

        public Ball(ItemKind itemKind)
        {
            ItemKind = itemKind;
        }
    }
}