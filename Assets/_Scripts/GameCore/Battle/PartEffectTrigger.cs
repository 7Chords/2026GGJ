using System;

namespace GameCore.Battle
{
    /// <summary>
    /// 效果触发点：封装 EntryInfo + 上下文工厂，便于 PartLogic 统一管理。
    /// </summary>
    public class PartEffectTrigger
    {
        public EntryInfo entry { get; }
        public EAttributeTriggerPointType triggerPoint { get; }
        public Func<PartEffectContext> contextFactory { get; set; }

        public PartEffectTrigger(EntryInfo _entry, EAttributeTriggerPointType _triggerPoint, Func<PartEffectContext> _contextFactory = null)
        {
            entry = _entry;
            triggerPoint = _triggerPoint;
            contextFactory = _contextFactory ?? (() => PartEffectContext.Active);
        }

        public void Execute(PartInfo _caster)
        {
            if (entry == null || _caster == null) return;
            var ctx = contextFactory?.Invoke() ?? PartEffectContext.Active;
            PartEffectExecutor.Execute(_caster, entry, ctx);
        }
    }
}
