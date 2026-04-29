using System;
using System.Collections.Generic;

namespace GameCore.Battle
{
    /// <summary>
    /// 部位逻辑：管理该部位在不同触发点的效果列表，统一执行。
    /// 改进：支持多个效果、条件检查、状态管理（后续可扩展）。
    /// </summary>
    public class PartLogic
    {
        private readonly PartInfo _m_partInfo;
        private readonly List<PartEffectTrigger> _m_activeTriggers = new List<PartEffectTrigger>();
        private readonly List<PartEffectTrigger> _m_getHitTriggers = new List<PartEffectTrigger>();
        private readonly List<PartEffectTrigger> _m_dieTriggers = new List<PartEffectTrigger>();
        private readonly List<PartEffectTrigger> _m_actionOverTriggers = new List<PartEffectTrigger>();

        public PartLogic(PartInfo _partInfo)
        {
            _m_partInfo = _partInfo;
        }

        public void RegisterTrigger(PartEffectTrigger _trigger)
        {
            if (_trigger == null) return;
            switch (_trigger.triggerPoint)
            {
                case EAttributeTriggerPointType.ACTIVE:
                    _m_activeTriggers.Add(_trigger);
                    break;
                case EAttributeTriggerPointType.GET_HIT:
                    _m_getHitTriggers.Add(_trigger);
                    break;
                case EAttributeTriggerPointType.DIE:
                    _m_dieTriggers.Add(_trigger);
                    break;
                case EAttributeTriggerPointType.ACTION_OVER:
                    _m_actionOverTriggers.Add(_trigger);
                    break;
            }
        }

        public void ClearTriggers(EAttributeTriggerPointType? _triggerPoint = null)
        {
            if (_triggerPoint == null || _triggerPoint == EAttributeTriggerPointType.ACTIVE)
                _m_activeTriggers.Clear();
            if (_triggerPoint == null || _triggerPoint == EAttributeTriggerPointType.GET_HIT)
                _m_getHitTriggers.Clear();
            if (_triggerPoint == null || _triggerPoint == EAttributeTriggerPointType.DIE)
                _m_dieTriggers.Clear();
            if (_triggerPoint == null || _triggerPoint == EAttributeTriggerPointType.ACTION_OVER)
                _m_actionOverTriggers.Clear();
        }

        public void OnPartActive()
        {
            foreach (var trigger in _m_activeTriggers)
                trigger.Execute(_m_partInfo);
        }

        public void OnPartGetHit(PartInfo _sender, int _damage)
        {
            var ctx = PartEffectContext.GetHit(_sender, _damage);
            foreach (var trigger in _m_getHitTriggers)
            {
                var originalFactory = trigger.contextFactory;
                trigger.contextFactory = () => ctx;
                trigger.Execute(_m_partInfo);
                if (originalFactory != null)
                    trigger.contextFactory = originalFactory;
            }
        }

        public void OnPartDie()
        {
            foreach (var trigger in _m_dieTriggers)
                trigger.Execute(_m_partInfo);
        }

        public void OnPartActionOver()
        {
            foreach (var trigger in _m_actionOverTriggers)
                trigger.Execute(_m_partInfo);
        }
    }
}
