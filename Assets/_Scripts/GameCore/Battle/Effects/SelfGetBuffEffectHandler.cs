using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class SelfGetBuffEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            long buffId = (long)_entry.attributeValueList[0];
            int buffLayer = (int)_entry.attributeValueList[1];

            var partInfoList = GameModel.instance.GetEntryRealTargetPartList(_caster, _entry, _ctx);
            if (partInfoList == null)
                return;

            foreach (var part in partInfoList)
                battleCtx.ApplyBuffToPart(part, _caster, buffId, buffLayer);
        }
    }
}
