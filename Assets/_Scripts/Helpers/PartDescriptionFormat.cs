using System;
using GameCore;
using GameCore.Battle;
using GameCore.RefData;
using SCFrame;
using UnityEngine;

namespace GameCore.Helpers
{
    /// <summary>
    /// Resolves part_level partDesc translate keys with dynamic values (buff-affected attack, etc.).
    /// </summary>
    public static class PartDescriptionFormat
    {
        public static string GetResolvedDescription(PartInfo part)
        {
            if (part?.levelRefObj == null)
                return "";
            if (part.entryInfoList == null)
                return part.levelRefObj.partDesc ?? "";
            return ResolveKey(part.levelRefObj.partDesc, part);
        }

        public static string GetResolvedDescription(PartLevelRefObj levelRef, PartRefObj partRef)
        {
            if (levelRef == null || partRef == null)
                return "";
            var temp = new PartInfo(partRef, false, levelRef.partLevel);
            if (temp?.levelRefObj == null || temp.entryInfoList == null)
                return levelRef.partDesc ?? "";
            return ResolveKey(levelRef.partDesc, temp);
        }

        private static string ResolveKey(string key, PartInfo part)
        {
            if (string.IsNullOrEmpty(key))
                return "";
            if (!key.StartsWith("#", StringComparison.Ordinal))
                return key;
            object[] args = BuildFormatArgs(part);
            return LanguageHelper.instance.GetTextTranslate(key, args);
        }

        private static EntryInfo FindEntry(PartInfo part, EAttributeTriggerPointType trigger,
            EAttributeType attr)
        {
            if (part?.entryInfoList == null)
                return null;
            for (int i = 0; i < part.entryInfoList.Count; i++)
            {
                var e = part.entryInfoList[i];
                if (e == null)
                    continue;
                if (e.triggerPointType == trigger && e.attributeType == attr)
                    return e;
            }
            return null;
        }

        private static string ChancePercentLabel(float chance)
        {
            float p = chance;
            if (p > 0f && p <= 1f)
                p *= 100f;
            return $"{Mathf.RoundToInt(p)}%";
        }

        private static int HeatExtraAttackPercent(EntryInfo e)
        {
            if (e?.attributeValueList == null || e.attributeValueList.Count < 2)
                return 0;
            float x = e.attributeValueList[1];
            if (x > 0f && x <= 1f)
                x *= 100f;
            return Mathf.RoundToInt(x);
        }

        private static int AttackBasePlusStrong(PartInfo part, EntryInfo e)
        {
            if (e?.attributeValueList == null || e.attributeValueList.Count < 1)
                return 0;
            int baseDmg = Mathf.RoundToInt(e.attributeValueList[0]);
            if (part.partRefObj != null && part.partRefObj.partType == EPartType.MOUTH)
                baseDmg += BuffCombatModifiers.GetStrongAttackBonus(part);
            if (part.isEnemyPart && part.partRefObj != null && part.partRefObj.partType == EPartType.MOUTH)
                baseDmg = Mathf.Max(0, Mathf.RoundToInt(EnemyPassiveController.ApplyOutgoingMouthAttackTotalFlatPenalty(part, baseDmg)));
            // Match combat: non-positive damage is not applied (BattleContext.ApplyDamageToPart).
            return Mathf.Max(0, baseDmg);
        }

        private static int RealAttackTotal(PartInfo part, EntryInfo e)
        {
            if (e?.attributeValueList == null || e.attributeValueList.Count < 1)
                return 0;
            int v = Mathf.RoundToInt(e.attributeValueList[0]);
            if (part.partRefObj != null && part.partRefObj.partType == EPartType.MOUTH)
                v += BuffCombatModifiers.GetStrongAttackBonus(part);
            if (part.isEnemyPart && part.partRefObj != null && part.partRefObj.partType == EPartType.MOUTH)
                v = Mathf.Max(0, Mathf.RoundToInt(EnemyPassiveController.ApplyOutgoingMouthAttackTotalFlatPenalty(part, v)));
            return Mathf.Max(0, v);
        }

        private static object[] BuildFormatArgs(PartInfo part)
        {
            long id = part.partRefObj != null ? part.partRefObj.id : 0L;
            switch (id)
            {
                case 101001:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE, EAttributeType.CLEAR_DEFULL);
                        int v = e?.attributeValueList != null && e.attributeValueList.Count > 0
                            ? Mathf.RoundToInt(e.attributeValueList[0])
                            : 0;
                        return new object[] { v };
                    }
                case 101002:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE, EAttributeType.TRIGGER_MORE);
                        int times = e?.attributeValueList != null && e.attributeValueList.Count > 0
                            ? Mathf.RoundToInt(e.attributeValueList[0])
                            : 0;
                        return new object[] { ChancePercentLabel(e?.attributeChance ?? 0f), times };
                    }
                case 101003:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE, EAttributeType.REAL_ATTACK);
                        return new object[] { RealAttackTotal(part, e) };
                    }
                case 101004:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE, EAttributeType.HEAL_ALL_PART);
                        int v = e?.attributeValueList != null && e.attributeValueList.Count > 0
                            ? Mathf.RoundToInt(e.attributeValueList[0])
                            : 0;
                        return new object[] { v };
                    }
                case 101005:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE, EAttributeType.PART_LOSE_TURN);
                        return new object[] { ChancePercentLabel(e?.attributeChance ?? 0f) };
                    }
                case 101006:
                case 101011:
                case 101012:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE, EAttributeType.ATTACK);
                        return new object[] { AttackBasePlusStrong(part, e) };
                    }
                case 101008:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE, EAttributeType.DAMAGE_MULTIPILER);
                        // EntryEffectObj: chance = strArr[2], multiplier = attributeValueList[0] (see DamageMultiplierEffectHandler).
                        int mult = e?.attributeValueList != null && e.attributeValueList.Count > 0
                            ? Mathf.RoundToInt(e.attributeValueList[0])
                            : 0;
                        return new object[] { ChancePercentLabel(e?.attributeChance ?? 0f), mult };
                    }
                case 101010:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.GET_HIT, EAttributeType.REFLECT);
                        int v = e?.attributeValueList != null && e.attributeValueList.Count > 0
                            ? Mathf.RoundToInt(e.attributeValueList[0])
                            : 0;
                        return new object[] { v };
                    }
                case 101013:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE, EAttributeType.ENEMY_GET_BUFF);
                        int layers = e?.attributeValueList != null && e.attributeValueList.Count > 1
                            ? Mathf.RoundToInt(e.attributeValueList[1])
                            : 0;
                        return new object[] { layers };
                    }
                case 101015:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE,
                            EAttributeType.CLEAR_SELF_BLEED_AND_HEAL_SELF);
                        int u = e?.attributeValueList != null && e.attributeValueList.Count > 0
                            ? Mathf.RoundToInt(e.attributeValueList[0])
                            : 0;
                        int h = e?.attributeValueList != null && e.attributeValueList.Count > 1
                            ? Mathf.RoundToInt(e.attributeValueList[1])
                            : 0;
                        return new object[] { u, h };
                    }
                case 101016:
                    {
                        var atk = FindEntry(part, EAttributeTriggerPointType.ACTIVE, EAttributeType.ATTACK);
                        int dmg = AttackBasePlusStrong(part, atk);
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE,
                            EAttributeType.CLEAR_ENEMY_BLEED_AND_HEAL_PART);
                        int maxStrip = e?.attributeValueList != null && e.attributeValueList.Count > 0
                            ? Mathf.RoundToInt(e.attributeValueList[0])
                            : 0;
                        int healUnit = e?.attributeValueList != null && e.attributeValueList.Count > 1
                            ? Mathf.RoundToInt(e.attributeValueList[1])
                            : 0;
                        int healAmt = e?.attributeValueList != null && e.attributeValueList.Count > 2
                            ? Mathf.RoundToInt(e.attributeValueList[2])
                            : 0;
                        return new object[] { dmg, maxStrip, healUnit, healAmt };
                    }
                case 101017:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE,
                            EAttributeType.ATTACK_BY_ENEMY_BLEED);
                        int strong = BuffCombatModifiers.GetStrongAttackBonus(part);
                        int bleedU = e?.attributeValueList != null && e.attributeValueList.Count > 0
                            ? Mathf.RoundToInt(e.attributeValueList[0])
                            : 0;
                        int atkU = e?.attributeValueList != null && e.attributeValueList.Count > 1
                            ? Mathf.RoundToInt(e.attributeValueList[1])
                            : 0;
                        return new object[] { strong, bleedU, atkU };
                    }
                case 101018:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE, EAttributeType.SELF_GET_BUFF);
                        int layers = e?.attributeValueList != null && e.attributeValueList.Count > 1
                            ? Mathf.RoundToInt(e.attributeValueList[1])
                            : 0;
                        return new object[] { layers };
                    }
                case 101019:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.GET_HIT,
                            EAttributeType.SEND_BLEED_BY_GET_HIT);
                        int layers = e?.attributeValueList != null && e.attributeValueList.Count > 0
                            ? Mathf.RoundToInt(e.attributeValueList[0])
                            : 0;
                        return new object[] { layers };
                    }
                case 101020:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE, EAttributeType.CHANGE_FAT_2_BURN);
                        int burnOut = e?.attributeValueList != null && e.attributeValueList.Count > 1
                            ? Mathf.RoundToInt(e.attributeValueList[1])
                            : 0;
                        return new object[] { burnOut };
                    }
                case 101021:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE, EAttributeType.SPREAD_BURN);
                        int delta = e?.attributeValueList != null && e.attributeValueList.Count > 0
                            ? Mathf.RoundToInt(e.attributeValueList[0])
                            : 0;
                        return new object[] { Mathf.Abs(delta) };
                    }
                case 101022:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE,
                            EAttributeType.INCREASE_ADD_BURN);
                        int v = e?.attributeValueList != null && e.attributeValueList.Count > 0
                            ? Mathf.RoundToInt(e.attributeValueList[0])
                            : 0;
                        return new object[] { v };
                    }
                case 101023:
                    {
                        var atk = FindEntry(part, EAttributeTriggerPointType.ACTIVE, EAttributeType.ATTACK);
                        var fat = FindEntry(part, EAttributeTriggerPointType.ACTIVE, EAttributeType.ENEMY_GET_BUFF);
                        int d = AttackBasePlusStrong(part, atk);
                        int fl = fat?.attributeValueList != null && fat.attributeValueList.Count > 1
                            ? Mathf.RoundToInt(fat.attributeValueList[1])
                            : 0;
                        return new object[] { d, fl };
                    }
                case 101024:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE, EAttributeType.SELF_GET_BUFF);
                        int layers = e?.attributeValueList != null && e.attributeValueList.Count > 1
                            ? Mathf.RoundToInt(e.attributeValueList[1])
                            : 0;
                        return new object[] { layers };
                    }
                case 101025:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE,
                            EAttributeType.ENEMY_MOUTH_GET_BUFF);
                        int layers = e?.attributeValueList != null && e.attributeValueList.Count > 1
                            ? Mathf.RoundToInt(e.attributeValueList[1])
                            : 0;
                        return new object[] { Mathf.Abs(layers) };
                    }
                case 101026:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE, EAttributeType.ENEMY_GET_BUFF);
                        int layers = e?.attributeValueList != null && e.attributeValueList.Count > 1
                            ? Mathf.RoundToInt(e.attributeValueList[1])
                            : 0;
                        return new object[] { layers };
                    }
                case 101027:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.ACTIVE,
                            EAttributeType.CLEAR_ALL_HATE_AND_MOUTH_GET_STRONG);
                        int hatePer = e?.attributeValueList != null && e.attributeValueList.Count > 0
                            ? Mathf.RoundToInt(e.attributeValueList[0])
                            : 0;
                        int strongPer = e?.attributeValueList != null && e.attributeValueList.Count > 1
                            ? Mathf.RoundToInt(e.attributeValueList[1])
                            : 0;
                        return new object[] { hatePer, strongPer };
                    }
                case 101028:
                    {
                        var atk = FindEntry(part, EAttributeTriggerPointType.ACTIVE, EAttributeType.ATTACK);
                        var heat = FindEntry(part, EAttributeTriggerPointType.ACTIVE,
                            EAttributeType.USE_HEAT_2_ATTACK_AGAIN);
                        int d = AttackBasePlusStrong(part, atk);
                        int threshold = heat?.attributeValueList != null && heat.attributeValueList.Count > 0
                            ? Mathf.RoundToInt(heat.attributeValueList[0])
                            : 0;
                        int perLayerPct = HeatExtraAttackPercent(heat);
                        // Matches #2_part_desc_101028: {2}=layers per probability step (config has per-layer x% only).
                        const int layersPerChanceStep = 1;
                        return new object[] { d, threshold, layersPerChanceStep, perLayerPct };
                    }
                case 101029:
                    {
                        var e = FindEntry(part, EAttributeTriggerPointType.GET_HIT, EAttributeType.SELF_GET_BUFF);
                        int layers = e?.attributeValueList != null && e.attributeValueList.Count > 1
                            ? Mathf.RoundToInt(e.attributeValueList[1])
                            : 0;
                        return new object[] { layers };
                    }
                case 101030:
                    {
                        EntryInfo activePrey = null, hitHate = null;
                        if (part.entryInfoList != null)
                        {
                            for (int i = 0; i < part.entryInfoList.Count; i++)
                            {
                                var e = part.entryInfoList[i];
                                if (e == null || e.attributeType != EAttributeType.SELF_GET_BUFF)
                                    continue;
                                if (e.triggerPointType == EAttributeTriggerPointType.ACTIVE)
                                    activePrey = e;
                                else if (e.triggerPointType == EAttributeTriggerPointType.GET_HIT)
                                    hitHate = e;
                            }
                        }
                        int p0 = activePrey?.attributeValueList != null && activePrey.attributeValueList.Count > 1
                            ? Mathf.RoundToInt(activePrey.attributeValueList[1])
                            : 0;
                        int p1 = hitHate?.attributeValueList != null && hitHate.attributeValueList.Count > 1
                            ? Mathf.RoundToInt(hitHate.attributeValueList[1])
                            : 0;
                        return new object[] { p0, p1 };
                    }
                case 101031:
                    {
                        var change = FindEntry(part, EAttributeTriggerPointType.ACTIVE,
                            EAttributeType.CHANGE_BREEDING_MASS_2_OTHER);
                        var mold = FindEntry(part, EAttributeTriggerPointType.GET_HIT,
                            EAttributeType.SEND_MOLD_2_BY_GET_HIT);
                        int capSelf = change?.attributeValueList != null && change.attributeValueList.Count > 0
                            ? Mathf.RoundToInt(change.attributeValueList[0])
                            : 20;
                        int capAlly = change?.attributeValueList != null && change.attributeValueList.Count > 1
                            ? Mathf.RoundToInt(change.attributeValueList[1])
                            : 20;
                        int moldLayers = mold?.attributeValueList != null && mold.attributeValueList.Count > 0
                            ? Mathf.RoundToInt(mold.attributeValueList[0])
                            : 2;
                        return new object[] { capSelf, capAlly, moldLayers };
                    }
                default:
                    return Array.Empty<object>();
            }
        }
    }
}
