using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class ClearEnemyBleedAndHealPartEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            int maxReduceLayer = (int)_entry.attributeValueList[0];
            int healUnit = (int)_entry.attributeValueList[1];
            int healAmount = (int)_entry.attributeValueList[2];

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

            int layer = 0;
            foreach (var part in partInfoList)
            {
                BuffInfo bleedBuffInfo = part.GetBuff(EBuffType.BLEED);
                if (bleedBuffInfo == null)
                    continue;
                int reduceLayer = Mathf.Min(maxReduceLayer, bleedBuffInfo.buffLayer);
                layer += reduceLayer;
                bleedBuffInfo.ReduceBuffLayer(reduceLayer);
                if (bleedBuffInfo.buffLayer == 0)
                    part.buffLogic.RemoveBuff(bleedBuffInfo);
                else
                    SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_UPDATE, bleedBuffInfo);
            }
            layer /= healUnit;
            battleCtx.ApplyHealToPart(_caster, layer * healAmount);
        }
    }

}