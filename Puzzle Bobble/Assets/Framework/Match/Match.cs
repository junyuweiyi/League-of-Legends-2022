using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core {

    public class Match
    {
        readonly LevelBallContainer _levelBallContainer;
        readonly HandBallContainer _handBallContainer;

        public Match()
        {
            _levelBallContainer = new LevelBallContainer();
            _handBallContainer = new HandBallContainer(_levelBallContainer);
            _levelBallContainer.CreateBalls(LevelData.map);
            _handBallContainer.CreateNextHandBall();
        }

        public void Dispose()
        {
            _levelBallContainer.Dispose();
            _handBallContainer.Dispose();
        }
    } 
}
