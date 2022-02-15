/********************************************************************
	created:	2020/09/11
	author:		ZYL
	
	purpose:	游戏状态机管理器，负责状态机的创建、销毁等功能。
*********************************************************************/

namespace iFramework
{
    public interface IFSMMgr
    {
        IFSM CreateFSM<StartState>(string name, params FSMState[] states) where StartState : FSMState;
        void DestroyFSM(IFSM fsm);
        IFSM GetFSM(string name);
    }
}