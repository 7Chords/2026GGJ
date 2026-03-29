using System.Collections;
using System.Collections.Generic;
using GameCore;
using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class SpreadBurnEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;
            int reduceLayer = (int)_entry.attributeValueList[0];
            int minLayer = (int)_entry.attributeValueList[1];
            var partInfoList = GameModel.instance.GetEntryRealTargetPartList(_caster, _entry, _ctx);
            if (partInfoList == null)
                return;

            PartInfo maxBurnPart = null;
            int maxBurnLayer = 0;
            foreach (var part in partInfoList)
            {
                BuffInfo buffInfo = part.GetBuff(EBuffType.BURN);
                if (buffInfo == null)
                    continue;
                if (buffInfo.buffLayer > maxBurnLayer)
                {
                    maxBurnLayer = buffInfo.buffLayer;
                    maxBurnPart = part;
                }
            }
            if (maxBurnPart == null)
                return;
            foreach (var part in partInfoList)
            {
                if (part == maxBurnPart)
                    continue;
                battleCtx.ApplyReduceBuffLayerToPart(part, GameConst.BUFF_ID_BURN, Mathf.Max(minLayer, maxBurnLayer + reduceLayer));
            }
        }
    }
}
