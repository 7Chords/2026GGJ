using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class HealPartEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            int healAmount = Mathf.RoundToInt(_entry.attributeValueList[0]);
            var partInfoList = GameModel.instance.GetEntryRealTargetPartList(_caster, _entry, _ctx);
            if (partInfoList == null)
                return;

            foreach (var part in partInfoList)
                battleCtx.ApplyHealToPart(part, healAmount);
        }
    }
}
