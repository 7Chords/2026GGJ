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
            battleCtx.ApplyBuffToPart(_ctx.senderPart, _caster, 100002, buffLayer);
            battleCtx.ApplyReduceBuffLayerToPart(_caster, 100002, buffLayer);
        }
    }
}
