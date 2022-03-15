namespace Core
{
    public class LevelBall : Ball
    {
        public (int x, int y) Slot { get; }

        public LevelBall(ItemKind itemKind, int x, int y)
            : base(itemKind)
        {
            Slot = (x, y);
        }
    }
}