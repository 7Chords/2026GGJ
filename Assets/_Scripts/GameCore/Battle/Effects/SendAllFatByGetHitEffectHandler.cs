using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class SendAllFatByGetHitEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null || _ctx.senderPart == null) return;
            BuffInfo info = _caster.GetBuff(EBuffType.FAT);
            if (info == null)
                return;
            int buffLayer = info.buffLayer;
            var targetList = GameModel.instance.GetEntryRealTargetPartList(_caster, _entry, _ctx);
            if (targetList == null || targetList.Count == 0)
                return;
            battleCtx.ApplyBuffToPart(targetList[0], _caster, 100002, buffLayer);
            battleCtx.ApplyReduceBuffLayerToPart(_caster, 100002, buffLayer);
        }
    }
}
