using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class ReflectEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null || _ctx.senderPart == null) return;

            int damage = Mathf.RoundToInt(_entry.attributeValue);
            battleCtx.ApplyDamageToPart(_ctx.senderPart, _caster, damage);
        }
    }
}
