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
        protected PartInfo partInfo;

        private Action onPartActiveAction;
        private Action onPartGetHitAction;
        private Action onPartDieAction;

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
        public void OnPartGetHit(float damage) 
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
    }
}
