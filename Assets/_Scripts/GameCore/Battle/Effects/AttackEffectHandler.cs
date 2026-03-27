using GameCore;
using GameCore.Battle;
using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class AttackEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            float totalDamage = _entry.attributeValueList[0];
            totalDamage += BuffCombatModifiers.GetStrongAttackBonus(_caster);
            if (_caster.curEffectFacePosList == null || _caster.curEffectFacePosList.Count == 0) return;

            // ATTACK 需按「每格」分摊伤害（含空格打本体），不能仅用部位去重列表，故不调用 GetEntryAttributeTargetPartList。
            float perGridDamage = totalDamage / _caster.curEffectFacePosList.Count;
            int emptyGridNum = 0;
            var partOccupyGridNumDic = new System.Collections.Generic.Dictionary<PartInfo, int>();

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

            if (_caster.partRefObj.partType == EPartType.MOUTH)
            {
                MouthAttackCoordinator.RegisterPendingAttack(_caster, new MouthAttackDamageData
                {
                    kind = MouthPendingDamageKind.GridAttack,
                    caster = _caster,
                    perGridDamage = perGridDamage,
                    emptyGridNum = emptyGridNum,
                    partOccupyGridNumDic = partOccupyGridNumDic
                });
                return;
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
