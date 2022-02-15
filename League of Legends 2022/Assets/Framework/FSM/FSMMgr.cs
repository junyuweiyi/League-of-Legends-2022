/********************************************************************
	created:	2020/09/11
	author:		ZYL
	
	purpose:	
*********************************************************************/

using System;
using System.Collections.Generic;

namespace iFramework
{
    internal class FSMMgr : IFSMMgr, IFrameworkModule
    {
        private List<FSM> _fsms = new List<FSM>();
        public int Priority => 1;

        public IFSM CreateFSM<StartState>(string name, params FSMState[] states) where StartState : FSMState
        {
            if (name == null || GetFSM(name) != null)
            {
                throw new Exception(Util.String.Format("CreateFSM by invalid name {0}", name));
            }
            FSM fsm = new FSM(name);
            fsm.Initialize<StartState>(states);

            _fsms.Add(fsm);
            return fsm;
        }

        public IFSM GetFSM(string name)
        {
            foreach (var fsm in _fsms)
            {
                if (fsm.Name == name)
                {
                    return fsm;
                }
            }
            return null;
        }

        public void DestroyFSM(IFSM fsm)
        {
            if (fsm == null)
            {
                throw new Exception("DestroyFSM by invalid fsm object");
            }
            _fsms.Remove(fsm as FSM);
            (fsm as FSM).Destory();
        }

        public void Shutdown()
        {
            for (int i = 0; i < _fsms.Count; i++)
            {
                _fsms[i].Destory();
            }
            _fsms.Clear();
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            for (int i = 0; i < _fsms.Count; i++)
            {
                _fsms[i].Update(elapseSeconds, realElapseSeconds);
            }
        }
    }
}