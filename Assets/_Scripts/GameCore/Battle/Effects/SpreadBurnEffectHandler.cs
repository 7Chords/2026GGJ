using System.Collections;
using System.Collections.Generic;
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
            var gridInfoList = _caster.isEnemyPart
                ? GameModel.instance.playerFaceGridInfoList
                : GameModel.instance.enemyFaceGridInfoList;
            var partInfoList = new List<PartInfo>();

            foreach (var pos in _caster.curEffectFacePosList)
            {
                var gridInfo = gridInfoList?.Find(x => x.pos == pos);
                if (gridInfo?.hasPart == true && gridInfo.ownerPart != null && !partInfoList.Contains(gridInfo.ownerPart))
                    partInfoList.Add(gridInfo.ownerPart);
            }

            PartInfo maxBurnPart = null;
            int maxBurnLayer = 0;
            foreach (var part in partInfoList)
            {
                if(part.GetBuff(EBuffType.BURN).buffLayer > maxBurnLayer)
                {
                    maxBurnLayer = part.GetBuff(EBuffType.BURN).buffLayer;
                    maxBurnPart = part;
                }
            }
            if (maxBurnPart == null)
                return;
            foreach (var part in partInfoList)
            {
                if (part == maxBurnPart)
                    continue;
                battleCtx.ApplyReduceBuffLayerToPart(part, 100003, Mathf.Max(minLayer, maxBurnLayer + reduceLayer));
            }
        }
    }
}
