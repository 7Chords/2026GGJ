using System.Collections.Generic;
using GameCore;
using SCFrame;
using UnityEngine;

namespace GameCore.Battle
{
    /// <summary>
    /// Runtime logic for germ / mold buff stacks (HEAL_MASS → grant BREEDING_MASS in area, ATTACK_MASS, BREEDING_MASS, MOLD).
    /// </summary>
    public static class GermMassBuffEffects
    {
        /// <summary>
        /// 医疗菌团 GET_EFFECT：用触发者当前医疗菌团层数，给效果范围内每个己方部位增加相同层数的繁殖菌团。
        /// </summary>
        public static void RunHealMassEffect(BuffInfo buff)
        {
            if (buff?.owner == null) return;
            if (BattleContext.current == null) return;
            var owner = buff.owner;
            if (!owner.isOnFace || buff.buffLayer <= 0) return;
            int addLayers = buff.buffLayer;
            var allyGrid = owner.isEnemyPart
                ? GameModel.instance.enemyFaceGridInfoList
                : GameModel.instance.playerFaceGridInfoList;
            var targets = GameModel.CollectPartsInEffectArea(owner, allyGrid);
            for (int i = 0; i < targets.Count; i++)
            {
                var p = targets[i];
                if (p == null || p.currentHealth <= 0) continue;
                if (p.isEnemyPart != owner.isEnemyPart) continue;

                var breedingDelta = BuffFactory.CreateBuffInfoByType(EBuffType.BREEDING_MASS, addLayers, owner, p);
                if (breedingDelta != null)
                    p.AddBuff(breedingDelta);
            }
        }

        /// <summary>
        /// Same per-grid split as normal ATTACK: total damage = stack layers over curEffectFacePosList;
        /// empty opposing cells damage body, occupied cells damage parts and apply mold to those parts.
        /// </summary>
        public static void RunAttackMassEffect(BuffInfo buff)
        {
            if (buff?.owner == null) return;
            var ctx = BattleContext.current;
            if (ctx == null) return;
            var owner = buff.owner;
            if (!owner.isOnFace || buff.buffLayer <= 0) return;
            float totalDamage = buff.buffLayer;
            int moldExtra = Mathf.RoundToInt(buff.buffValue);
            if (moldExtra <= 0) moldExtra = 2;
            if (owner.curEffectFacePosList == null || owner.curEffectFacePosList.Count == 0) return;

            float perGridDamage = totalDamage / owner.curEffectFacePosList.Count;
            int emptyGridNum = 0;
            var partOccupyGridNumDic = new Dictionary<PartInfo, int>();

            var gridInfoList = owner.isEnemyPart
                ? GameModel.instance.playerFaceGridInfoList
                : GameModel.instance.enemyFaceGridInfoList;

            foreach (var pos in owner.curEffectFacePosList)
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

            if (owner.isEnemyPart)
                ctx.ApplyDamageToPlayer(Mathf.RoundToInt(perGridDamage * emptyGridNum));
            else
                ctx.ApplyDamageToEnemy(Mathf.RoundToInt(perGridDamage * emptyGridNum));

            foreach (var pair in partOccupyGridNumDic)
            {
                ctx.ApplyDamageToPart(pair.Key, owner, Mathf.RoundToInt(pair.Value * perGridDamage));
                ctx.ApplyBuffToPart(pair.Key, owner, GameConst.BUFF_ID_MOLD, moldExtra);
            }
        }

        public static void RunBreedingMassOnTurnOver(BuffInfo buff)
        {
            if (buff?.owner == null) return;
            var ctx = BattleContext.current;
            var owner = buff.owner;
            if (buff.buffLayer <= 0)
            {
                owner.buffLogic?.RemoveBuff(buff);
                if (ctx != null && owner.currentHealth > 0)
                    ctx.ApplyDamageToPart(owner, owner, owner.maxHealth + owner.currentHealth);
                return;
            }
            buff.AddBuffLayer(10);
            SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_UPDATE, buff);
        }

        public static void RunMoldTotalTurnOver(BuffInfo buff)
        {
            if (buff?.owner == null) return;
            var ctx = BattleContext.current;
            if (ctx == null) return;
            var owner = buff.owner;
            int layers = buff.buffLayer;
            if (layers <= 0) return;
            ctx.ApplyDamageToPart(owner, owner, layers);
            if (owner.currentHealth <= 0) return;
            if (owner.isOnFace)
            {
                buff.AddBuffLayer(layers);
                SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_UPDATE, buff);
            }
            else
            {
                int halveReduce = buff.buffLayer - buff.buffLayer / 2;
                if (halveReduce > 0)
                {
                    buff.ReduceBuffLayer(halveReduce);
                    if (buff.buffLayer == 0)
                        owner.buffLogic?.RemoveBuff(buff);
                    else
                        SCMsgCenter.SendMsg(SCMsgConst.PART_BUFF_UPDATE, buff);
                }
            }
        }

        public static void TriggerHealMassIfPresent(PartInfo owner)
        {
            var b = owner?.GetBuff(EBuffType.HEAL_MASS);
            if (b != null && b.buffLayer > 0)
                RunHealMassEffect(b);
        }

        public static void TriggerAttackMassIfPresent(PartInfo owner)
        {
            var b = owner?.GetBuff(EBuffType.ATTACK_MASS);
            if (b != null && b.buffLayer > 0)
                RunAttackMassEffect(b);
        }

        /// <summary>
        /// Non-breeding only: higher stack wins; tie runs both (matches buff copy: 最高 / 并列).
        /// </summary>
        public static void RunTriggerMaxMassNonBreeding(PartInfo owner)
        {
            if (owner == null) return;
            var heal = owner.GetBuff(EBuffType.HEAL_MASS);
            var atk = owner.GetBuff(EBuffType.ATTACK_MASS);
            int h = heal != null ? heal.buffLayer : 0;
            int a = atk != null ? atk.buffLayer : 0;
            if (h <= 0 && a <= 0) return;
            if (h > a)
                TriggerHealMassIfPresent(owner);
            else if (a > h)
                TriggerAttackMassIfPresent(owner);
            else
            {
                TriggerHealMassIfPresent(owner);
                TriggerAttackMassIfPresent(owner);
            }
        }
    }
}
