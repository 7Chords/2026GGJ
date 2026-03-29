using System.Collections;
using System.Collections.Generic;
using GameCore;
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
            battleCtx.ApplyBuffToPart(_ctx.senderPart, _caster, GameConst.BUFF_ID_BLEED, buffLayer);
        }
    }
}
