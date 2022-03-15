using System.Collections;
using System.Collections.Generic;

namespace Core
{
    public class LevelBallContainer
    {
        public const int kLevelBallXCount = 11;
        public const int kLevelBallYCount = 70;

        readonly List<LevelBall> _balls = new List<LevelBall>();
        //场上球的颜色种类
        readonly List<ItemColor> _ballColors = new List<ItemColor>();

        public LevelBallContainer()
        {

        }

        public void Dispose()
        {
            _balls.Clear();
            _ballColors.Clear();
        }

        public void CreateBalls(int[] levelBalls)
        {
            _balls.Clear();
            for (int i = 0; i < levelBalls.Length; i++)
            {
                var ballID = levelBalls[i];
                if (ballID == 0)
                    continue;

                ItemKind itemKind = LevelEditorBase.THIS.items[ballID];
                int x = i % kLevelBallXCount;
                int y = i / kLevelBallXCount;
                LevelBall ball = new LevelBall(itemKind, x, y);
                _balls.Add(ball);
            }
            UpdateBallColors();
        }

        public void Remove(LevelBall ball)
        {
            _balls.Remove(ball);
            UpdateBallColors();
        }

        public List<LevelBall> GetBalls()
        {
            return _balls;
        }

        public List<ItemColor> GetBallColors()
        {
            return _ballColors;
        }

        void UpdateBallColors()
        {
            _ballColors.Clear();
            foreach (var ball in _balls)
            {
                var color = ball.ItemKind.color;
                if (!_ballColors.Contains(color))
                    _ballColors.Add(color);
            }
        }
    }

}