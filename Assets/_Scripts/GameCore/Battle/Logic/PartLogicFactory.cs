using GameCore.RefData;
using System;

namespace GameCore.Battle
{
    /// <summary>
    /// 部位逻辑工厂：从 PartInfo 的 EntryInfo 列表创建 PartLogic，统一使用注册表。
    /// 简化：移除重复的 Recreate 方法，统一用 CreateLogic + 更新触发器。
    /// </summary>
    public static class PartLogicFactory
    {
        /// <summary> 从 PartInfo 创建 PartLogic，注册所有 EntryInfo 对应的触发器。 </summary>
        public static PartLogic CreateLogic(PartInfo _partInfo)
        {
            if (_partInfo == null) return null;
            var logic = new PartLogic(_partInfo);
            RefreshTriggers(logic, _partInfo);
            return logic;
        }

        /// <summary> 刷新指定触发点的触发器（如 ACTIVE 在每次触发前刷新，GET_HIT 在受击时刷新）。 </summary>
        public static void RefreshTriggers(PartLogic _logic, PartInfo _partInfo, EAttributeTriggerPointType? _triggerPoint = null)
        {
            if (_logic == null || _partInfo == null) return;

            if (_triggerPoint == null || _triggerPoint == EAttributeTriggerPointType.ACTIVE)
            {
                _logic.ClearTriggers(EAttributeTriggerPointType.ACTIVE);
                foreach (var entry in _partInfo.entryInfoList)
                {
                    if (entry?.triggerPointType == EAttributeTriggerPointType.ACTIVE)
                        _logic.RegisterTrigger(new PartEffectTrigger(entry, EAttributeTriggerPointType.ACTIVE, () => PartEffectContext.Active));
                }
            }

            if (_triggerPoint == null || _triggerPoint == EAttributeTriggerPointType.GET_HIT)
            {
                _logic.ClearTriggers(EAttributeTriggerPointType.GET_HIT);
                foreach (var entry in _partInfo.entryInfoList)
                {
                    if (entry?.triggerPointType == EAttributeTriggerPointType.GET_HIT)
                    {
                        // GET_HIT 的上下文在触发时动态创建
                        _logic.RegisterTrigger(new PartEffectTrigger(entry, EAttributeTriggerPointType.GET_HIT, null));
                    }
                }
            }

            if (_triggerPoint == null || _triggerPoint == EAttributeTriggerPointType.DIE)
            {
                _logic.ClearTriggers(EAttributeTriggerPointType.DIE);
                foreach (var entry in _partInfo.entryInfoList)
                {
                    if (entry?.triggerPointType == EAttributeTriggerPointType.DIE)
                        _logic.RegisterTrigger(new PartEffectTrigger(entry, EAttributeTriggerPointType.DIE, () => PartEffectContext.Active));
                }
            }
        }
    }
}
