using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class ReflectEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null || _ctx.senderPart == null) return;

            int damage = Mathf.RoundToInt(_entry.attributeValueList[0]);
            var targetList = GameModel.instance.GetEntryRealTargetPartList(_caster, _entry, _ctx);
            if (targetList == null || targetList.Count == 0)
                return;
            battleCtx.ApplyDamageToPart(targetList[0], _caster, damage);
        }
    }
}
