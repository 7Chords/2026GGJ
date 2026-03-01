using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class ClearSelfBleedAndHealSelfEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            int healthBleedUnit = (int)_entry.attributeValueList[0];
            int healthAmount = (int)_entry.attributeValueList[1];

            var gridInfoList = _caster.isEnemyPart
                ? GameModel.instance.enemyFaceGridInfoList
                : GameModel.instance.playerFaceGridInfoList;
            var partInfoList = new List<PartInfo>();

            foreach (var pos in _caster.curEffectFacePosList)
            {
                var gridInfo = gridInfoList?.Find(x => x.pos == pos);
                if (gridInfo?.hasPart == true && gridInfo.ownerPart != null && !partInfoList.Contains(gridInfo.ownerPart))
                    partInfoList.Add(gridInfo.ownerPart);
            }

            int layer = 0;
            foreach (var part in partInfoList)
            {
                BuffInfo bleedBuffInfo = part.GetBuff(EBuffType.BLEED);
                if (bleedBuffInfo == null)
                    continue;
                layer += bleedBuffInfo.buffLayer;
                part.buffLogic.ClearBuff(EBuffType.BLEED);
            }
            layer /= healthBleedUnit;
            battleCtx.ApplyHealToPlayer(layer * healthAmount);

        }
    }
}
