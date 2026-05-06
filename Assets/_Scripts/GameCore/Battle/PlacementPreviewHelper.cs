using System.Collections.Generic;
using GameCore;
using SCFrame;
using UnityEngine;

namespace GameCore.Battle
{
    /// <summary>
    /// Per-part damage/heal numbers for face placement preview (aligns with active entry resolution).
    /// </summary>
    public sealed class PartPlacementPreviewPayload
    {
        public readonly Dictionary<PartInfo, int> damageToPart = new Dictionary<PartInfo, int>();
        public readonly Dictionary<PartInfo, int> healToPart = new Dictionary<PartInfo, int>();
        public int damageToEnemyBody;
        public int damageToPlayerBody;
    }

    public static class PlacementPreviewHelper
    {
        public static void BroadcastValues(PartInfo caster)
        {
            SCMsgCenter.SendMsg(SCMsgConst.FACE_PART_TARGET_PREVIEW_VALUES, Compute(caster));
        }

        public static void BroadcastClear()
        {
            SCMsgCenter.SendMsg(SCMsgConst.FACE_PART_TARGET_PREVIEW_VALUES, new PartPlacementPreviewPayload());
        }

        public static PartPlacementPreviewPayload Compute(PartInfo caster)
        {
            var p = new PartPlacementPreviewPayload();
            if (caster == null || caster.entryInfoList == null)
                return p;
            if (caster.curEffectFacePosList == null || caster.curEffectFacePosList.Count == 0)
                return p;

            foreach (var entry in caster.entryInfoList)
            {
                if (entry == null || entry.triggerPointType != EAttributeTriggerPointType.ACTIVE)
                    continue;
                if (entry.attributeValueList == null || entry.attributeValueList.Count == 0)
                    continue;

                switch (entry.attributeType)
                {
                    case EAttributeType.ATTACK:
                        {
                            float totalDamage = entry.attributeValueList[0];
                            totalDamage += BuffCombatModifiers.GetStrongAttackBonus(caster);
                            totalDamage = EnemyPassiveController.ApplyOutgoingMouthAttackTotalFlatPenalty(caster, totalDamage);
                            AccumulateGridAttack(caster, totalDamage, p);
                            break;
                        }
                    case EAttributeType.REAL_ATTACK:
                        {
                            float damageF = entry.attributeValueList[0];
                            damageF += BuffCombatModifiers.GetStrongAttackBonus(caster);
                            damageF = EnemyPassiveController.ApplyOutgoingMouthAttackTotalFlatPenalty(caster, damageF);
                            int damage = Mathf.RoundToInt(damageF);
                            if (damage <= 0)
                                break;
                            if (caster.isEnemyPart)
                                p.damageToPlayerBody += damage;
                            else
                                p.damageToEnemyBody += damage;
                            break;
                        }
                    case EAttributeType.ATTACK_BY_ENEMY_BLEED:
                        AccumulateAttackByBleed(caster, entry, p);
                        break;
                    case EAttributeType.HEAL_ALL_PART:
                    case EAttributeType.HEAL_WEAK_PART:
                        AccumulateHealAll(caster, entry, p);
                        break;
                    case EAttributeType.CLEAR_ENEMY_BLEED_AND_HEAL_PART:
                        AccumulateClearEnemyBleedHeal(caster, entry, p);
                        break;
                }
            }

            return p;
        }

        private static void AccumulateHealAll(PartInfo caster, EntryInfo entry, PartPlacementPreviewPayload p)
        {
            int healAmount = Mathf.RoundToInt(entry.attributeValueList[0]);
            if (healAmount <= 0)
                return;
            var list = GameModel.instance.GetEntryPreviewTargetPartList(caster, entry);
            if (list == null)
                return;
            foreach (var part in list)
                AddHeal(p, part, healAmount);
        }

        private static void AccumulateClearEnemyBleedHeal(PartInfo caster, EntryInfo entry, PartPlacementPreviewPayload p)
        {
            if (entry.attributeValueList.Count < 3)
                return;
            int maxReduceLayer = (int)entry.attributeValueList[0];
            int healUnit = (int)entry.attributeValueList[1];
            int healAmount = (int)entry.attributeValueList[2];
            if (healUnit <= 0)
                return;

            var partInfoList = GameModel.instance.GetEntryPreviewTargetPartList(caster, entry);
            if (partInfoList == null)
                return;

            int layer = 0;
            foreach (var part in partInfoList)
            {
                BuffInfo bleedBuffInfo = part.GetBuff(EBuffType.BLEED);
                if (bleedBuffInfo == null)
                    continue;
                int reduceLayer = Mathf.Min(maxReduceLayer, bleedBuffInfo.buffLayer);
                layer += reduceLayer;
            }

            layer /= healUnit;
            int totalHeal = layer * healAmount;
            AddHeal(p, caster, totalHeal);
        }

        private static void AccumulateAttackByBleed(PartInfo caster, EntryInfo entry, PartPlacementPreviewPayload p)
        {
            if (entry.attributeValueList.Count < 2)
                return;
            int bleedUnit = (int)entry.attributeValueList[0];
            int attackUnit = (int)entry.attributeValueList[1];
            if (bleedUnit <= 0)
                return;

            var partInfoList = GameModel.instance.GetEntryPreviewTargetPartList(caster, entry);
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
            totalDamage += BuffCombatModifiers.GetStrongAttackBonus(caster);
            totalDamage = EnemyPassiveController.ApplyOutgoingMouthAttackTotalFlatPenalty(caster, totalDamage);
            AccumulateGridAttack(caster, totalDamage, p);
        }

        private static void AccumulateGridAttack(PartInfo caster, float totalDamage, PartPlacementPreviewPayload p)
        {
            if (caster.curEffectFacePosList == null || caster.curEffectFacePosList.Count == 0)
                return;
            float perGridDamage = totalDamage / caster.curEffectFacePosList.Count;
            int emptyGridNum = 0;
            var partOccupyGridNumDic = new Dictionary<PartInfo, int>();

            var gridInfoList = caster.isEnemyPart
                ? GameModel.instance.playerFaceGridInfoList
                : GameModel.instance.enemyFaceGridInfoList;

            foreach (var pos in caster.curEffectFacePosList)
            {
                var gridInfo = gridInfoList?.Find(x => x.pos == pos);
                if (gridInfo == null)
                    continue;

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

            foreach (var pair in partOccupyGridNumDic)
            {
                int baseDmg = Mathf.RoundToInt(pair.Value * perGridDamage);
                if (caster.isEnemyPart)
                    baseDmg = EnemyPassiveController.AdjustEnemyOutgoingDamageToPlayerPart(pair.Key, caster, baseDmg);
                int prey = BuffCombatModifiers.GetPreyExtraDamage(pair.Key);
                AddDamage(p, pair.Key, baseDmg + prey);
            }

            int bodyDmg = Mathf.RoundToInt(perGridDamage * emptyGridNum);
            if (bodyDmg > 0)
            {
                if (caster.isEnemyPart)
                    p.damageToPlayerBody += bodyDmg;
                else
                    p.damageToEnemyBody += bodyDmg;
            }
        }

        private static void AddDamage(PartPlacementPreviewPayload p, PartInfo part, int amount)
        {
            if (part == null || amount <= 0)
                return;
            if (p.damageToPart.ContainsKey(part))
                p.damageToPart[part] += amount;
            else
                p.damageToPart[part] = amount;
        }

        private static void AddHeal(PartPlacementPreviewPayload p, PartInfo part, int amount)
        {
            if (part == null || amount <= 0)
                return;
            if (p.healToPart.ContainsKey(part))
                p.healToPart[part] += amount;
            else
                p.healToPart[part] = amount;
        }
    }
}
