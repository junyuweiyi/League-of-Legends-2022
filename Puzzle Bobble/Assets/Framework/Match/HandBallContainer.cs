using System.Collections.Generic;

namespace Core
{
    public class HandBallContainer
    {
        readonly List<Ball> _balls = new List<Ball>();
        readonly LevelBallContainer _levelBallContainer;

        public HandBallContainer(LevelBallContainer levelBallContainer)
        {
            _levelBallContainer = levelBallContainer;
        }

        public void Dispose()
        {
            _balls.Clear();
        }

        public void CreateNextHandBall()
        {
            AddHandBall(CreateHandBall());
        }

        void AddHandBall(Ball ball)
        {
            _balls.Add(ball);
        }

        public void RemoveHandBall(Ball ball)
        {
            _balls.Remove(ball);
        }

        public List<Ball> GetHandBalls()
        {
            return _balls;
        }

        Ball CreateHandBall()
        {
            var color = CreateColor();
            var itemKind = LevelEditorBase.THIS.items.Find((e) => e.color == color && e.itemType == ItemTypes.Simple);
            return new Ball(itemKind);
        }

        ItemColor CreateColor()
        {
            var colors = _levelBallContainer.GetBallColors();
            return colors[UnityEngine.Random.Range(0, colors.Count)];
        }
    }
}