using SCFrame;
using System;

namespace GameCore.Battle
{
    /// <summary>
    /// 效果执行器：统一处理 EntryInfo 的条件检查、概率判定、消息发送、效果执行。
    /// 将 PartLogicFactory 与 PartLogicHandler 解耦，便于扩展（如条件系统、日志、回放）。
    /// </summary>
    public static class PartEffectExecutor
    {
        /// <summary>
        /// 执行一个 EntryInfo 的效果：检查条件、判定概率、发送消息、调用处理器。
        /// </summary>
        public static bool Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            if (_caster == null || _entry == null) return false;

            // 条件检查（可扩展：EntryInfo 增加条件字段，这里检查）
            // if (!CheckConditions(_caster, _entry, _ctx)) return false;

            // 概率判定
            bool triggered = CheckChance(_entry);
            if (!triggered)
            {
                SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_FAIL, _caster);
                return false;
            }

            // 发送成功消息
            SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_SUCCESS, _caster);

            // 查找并执行处理器
            var handler = PartEffectHandlerRegistry.Get(_entry.attributeType);
            if (handler == null)
            {
                UnityEngine.Debug.LogWarning($"未注册的效果类型: {_entry.attributeType}");
                return false;
            }

            handler.Execute(_caster, _entry, _ctx);
            return true;
        }

        private static bool CheckChance(EntryInfo _entry)
        {
            if (_entry.attributeChance <= 0) return false;
            if (_entry.attributeChance >= 1) return true;
            float roll01 = UnityEngine.Random.value;
            return roll01 < _entry.attributeChance;
        }
    }
}
