using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Logic
{

    /// <summary>
    /// 部位逻辑基类 只做逻辑的东西 效果不在这里做
    /// </summary>
    public class PartLogic
    {
        private PartInfo _m_partInfo;

        private Action onPartActiveAction;
        private Action onPartGetHitAction;
        private Action onPartDieAction;

        public PartLogic(PartInfo _info)
        {
            _m_partInfo = _info;
        }
        public void RegisterPartActiveAction(Action _action)
        {
            onPartActiveAction += _action;
        }
        public void RegisterPartGetHitAction(Action _action)
        {
            onPartGetHitAction += _action;
        }
        public void RegisterPartDieAction(Action _action)
        {
            onPartDieAction += _action;
        }
        public void OnPartGetHit() 
        {
            onPartGetHitAction?.Invoke();
        }
        public void OnPartDie()
        {
            onPartDieAction?.Invoke();
        }
        public void OnPartActive()
        {
            onPartActiveAction?.Invoke();
        }

        public void ClearOnPartActiveAction()
        {
            onPartActiveAction = null;
        }
        public void ClearOnPartGetHitAction()
        {
            onPartGetHitAction = null;
        }
        public void ClearOnPartDieAction()
        {
            onPartDieAction = null;
        }
    }
}
