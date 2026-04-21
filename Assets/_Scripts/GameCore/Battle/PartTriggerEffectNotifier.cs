using SCFrame;

namespace GameCore.Battle
{
    /// <summary>
    /// Fires UI message and GET_EFFECT buff hooks when a part is "triggered" by another part's effect chain.
    /// </summary>
    public static class PartTriggerEffectNotifier
    {
        public static void Notify(PartInfo target, PartInfo caster)
        {
            if (target == null) return;
            SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_EFFECT, target, caster);
            if (target.currentHealth > 0 && target.HasBuff(EAttributeTriggerPointType.GET_EFFECT))
                target.TriggerBuff(EAttributeTriggerPointType.GET_EFFECT);
        }
    }
}
