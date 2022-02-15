/********************************************************************
	created:	2020/09/24
	author:		ZYL
	
	purpose:	
*********************************************************************/

using System;

namespace iFramework
{
    public interface ITimerMgr
    {
        /// <summary>
        /// 一次性触发的Timer
        /// </summary>
        /// <param name="seconds">触发时长</param>
        /// <param name="callback">回调函数</param>
        /// <returns>timerID，可用于停止该Timer</returns>
        int Timeout(float seconds, Action callback);
        /// <summary>
        /// 按间隔时间多次触发的Timer
        /// ！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
        /// ！！！！！请一定记得在对象销毁前停止正在执行的Interval！！！！！
        /// ！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！
        /// </summary>
        /// <param name="interval">间隔时长</param>
        /// <param name="callback">回调函数，函数参数为触发计数，从0开始，每触发1次加1</param>
        /// <param name="duration">持续时长，时长为负时一直触发（此情况下需要在需要时手动停止）</param>
        /// <returns>timerID，可用于停止该Timer</returns>
        int Interval(float interval, Action<int> callback, float duration = -1);
        int Interval(float interval, Action callback, float duration = -1);
        int Updater(Action update);
        /// <summary>
        /// 停止Timer
        /// </summary>
        /// <param name="id"></param>
        void Stop(int id);
        /// <summary>
        /// 停止所有Timer
        /// </summary>
        void ClearAll();
    }
}
