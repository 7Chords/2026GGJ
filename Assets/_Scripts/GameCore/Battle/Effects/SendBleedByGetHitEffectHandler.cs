using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class SendBleedByGetHitEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null || _ctx.senderPart == null) return;
            int buffLayer = (int)_entry.attributeValueList[0];
            var targetList = GameModel.instance.GetEntryRealTargetPartList(_caster, _entry, _ctx);
            if (targetList == null || targetList.Count == 0)
                return;
            battleCtx.ApplyBuffToPart(targetList[0], _caster, 100001, buffLayer);
        }
    }
}
