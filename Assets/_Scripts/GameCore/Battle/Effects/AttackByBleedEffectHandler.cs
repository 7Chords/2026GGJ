using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class AttackByBleedEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            int bleedUnit = (int)_entry.attributeValueList[0];
            int attackUnit = (int)_entry.attributeValueList[1];

            var partInfoList = GameModel.instance.GetEntryAttributeTargetPartList(_caster, _entry, _ctx);
            if (partInfoList == null)
                return;

            int layer = 0;
            foreach (var part in partInfoList)
            {
                BuffInfo bleedBuffInfo = part.GetBuff(EBuffType.BLEED);
                if (bleedBuffInfo == null)
                    continue;
                layer += bleedBuffInfo.buffLayer;
            }
            layer /= bleedUnit;


            float totalDamage = layer * attackUnit;
            if (_caster.curEffectFacePosList == null || _caster.curEffectFacePosList.Count == 0) return;

            float perGridDamage = totalDamage / _caster.curEffectFacePosList.Count;
            int emptyGridNum = 0;
            var partOccupyGridNumDic = new Dictionary<PartInfo, int>();

            var gridInfoList = _caster.isEnemyPart
                ? GameModel.instance.playerFaceGridInfoList
                : GameModel.instance.enemyFaceGridInfoList;

            foreach (var pos in _caster.curEffectFacePosList)
            {
                var gridInfo = gridInfoList?.Find(x => x.pos == pos);
                if (gridInfo == null) continue;

                if (gridInfo.ownerPart == null)
                    emptyGridNum++;
                else
                {
                    if (!partOccupyGridNumDic.ContainsKey(gridInfo.ownerPart))
                        partOccupyGridNumDic[gridInfo.ownerPart] = 1;
                    else
                        partOccupyGridNumDic[gridInfo.ownerPart]++;
                }
            }

            if (_caster.isEnemyPart)
                battleCtx.ApplyDamageToPlayer(Mathf.RoundToInt(perGridDamage * emptyGridNum));
            else
                battleCtx.ApplyDamageToEnemy(Mathf.RoundToInt(perGridDamage * emptyGridNum));

            foreach (var pair in partOccupyGridNumDic)
                battleCtx.ApplyDamageToPart(pair.Key, _caster, Mathf.RoundToInt(pair.Value * perGridDamage));

        }
    }
}
