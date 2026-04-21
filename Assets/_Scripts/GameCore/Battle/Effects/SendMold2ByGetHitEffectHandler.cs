using GameCore;
using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class SendMold2ByGetHitEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null || _ctx.senderPart == null) return;
            if (!HasAnyGermStack(_caster)) return;
            int moldLayers = 2;
            if (_entry?.attributeValueList != null && _entry.attributeValueList.Count > 0)
                moldLayers = Mathf.Max(1, Mathf.RoundToInt(_entry.attributeValueList[0]));
            battleCtx.ApplyBuffToPart(_ctx.senderPart, _caster, GameConst.BUFF_ID_MOLD, moldLayers);
        }

        static bool HasAnyGermStack(PartInfo p)
        {
            if (p == null) return false;
            return LayerOf(p, EBuffType.HEAL_MASS) > 0
                || LayerOf(p, EBuffType.ATTACK_MASS) > 0
                || LayerOf(p, EBuffType.BREEDING_MASS) > 0;
        }

        static int LayerOf(PartInfo p, EBuffType t)
        {
            var b = p.GetBuff(t);
            return b != null ? b.buffLayer : 0;
        }
    }
}
