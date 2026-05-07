using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class ReflectEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null || _ctx.senderPart == null) return;
            // Bleed/burn ticks use sender == victim; reflecting would hit self and loop forever.
            if (ReferenceEquals(_ctx.senderPart, _caster)) return;

            int damage = Mathf.RoundToInt(_entry.attributeValueList[0]);
            battleCtx.ApplyDamageToPart(_ctx.senderPart, _caster, damage);
        }
    }
}
