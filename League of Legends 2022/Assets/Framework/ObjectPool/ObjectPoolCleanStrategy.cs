/********************************************************************
	created:	2021/03/04
	author:		ZYL
	
	purpose:	
*********************************************************************/

namespace iFramework
{
    internal class ObjectPoolCleanStrategy
    {        
        public float recordDeltaTime;
        public float maxRecordTime;
        public float catchRate;
        public float catchCleanInterval;

        private float _initRecordDeltaTime = 10;
        private float _initMaxRecordTime = 90;
        private float _initCatchRate = 1.5f;
        private float _initCatchCleanInterval = 0.2f;
        private float _paramRecoverRate = 0.001f;

        public ObjectPoolCleanStrategy()
        {
            recordDeltaTime = _initRecordDeltaTime;
            maxRecordTime = _initMaxRecordTime;
            catchRate = _initCatchRate;
            catchCleanInterval = _initCatchCleanInterval;
        }

        public void OnLowMemory()
        {
            recordDeltaTime = 5;
            maxRecordTime = 10;
            catchRate = 1;
            catchCleanInterval = 0.05f;
        }

        public void UpdateStrategy()
        {
            recordDeltaTime += (_initRecordDeltaTime - recordDeltaTime) * _paramRecoverRate;
            maxRecordTime += (_initMaxRecordTime - maxRecordTime) * _paramRecoverRate;
            catchRate += (_initCatchRate - catchRate) * _paramRecoverRate;
            catchCleanInterval += (_initCatchCleanInterval - catchCleanInterval) * _paramRecoverRate;
        }
    }
}