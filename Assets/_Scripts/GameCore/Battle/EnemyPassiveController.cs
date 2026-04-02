using GameCore.RefData;
using UnityEngine;
using GameCore;

namespace GameCore.Battle
{
    public static class EnemyPassiveController
    {
        static float FloatParamAt(EnemyPassiveRefObj row, int index, float defaultValue)
        {
            if (row?.paramList == null || index < 0 || index >= row.paramList.Count)
                return defaultValue;
            return row.paramList[index];
        }

        public static void OnEnemyPhaseStart()
        {
            var e = GameModel.instance.curEnemyInfo;
            if (e?.passiveBattleState == null || e.enemyRefObj == null)
                return;

            var st = e.passiveBattleState;
            st.outgoingPartDamageMod = EEnemyOutgoingPartDamageMod.NONE;
            st.outgoingPreyBonus = 0;
            st.outgoingPartPenalty = 0;

            st.enemyPhaseCounter++;
            var refPassive = FindPassiveRef(e, EEnemyPassiveSkillType.REVENGE_EVERY_N_PHASES);
            if (refPassive == null)
                return;

            int interval = Mathf.Max(1, Mathf.RoundToInt(FloatParamAt(refPassive, 0, 2f)));
            if (st.enemyPhaseCounter % interval != 0)
                return;

            int preyBonusV = Mathf.RoundToInt(FloatParamAt(refPassive, 1, 2f));
            int preyBonus = preyBonusV > 0 ? preyBonusV : 2;
            int partPenaltyV = Mathf.RoundToInt(FloatParamAt(refPassive, 2, 5f));
            int partPenalty = partPenaltyV > 0 ? partPenaltyV : 5;
            bool hasHate = AnyEnemyPartHasHate(e);
            if (hasHate)
            {
                st.outgoingPartDamageMod = EEnemyOutgoingPartDamageMod.PREY_FLAT_BONUS;
                st.outgoingPreyBonus = preyBonus;
            }
            else
            {
                st.outgoingPartDamageMod = EEnemyOutgoingPartDamageMod.ALL_PART_FLAT_PENALTY;
                st.outgoingPartPenalty = partPenalty;
            }
        }

        public static void OnEnemyBodyDamageApplied(int damageAmount)
        {
            if (damageAmount <= 0)
                return;
            var e = GameModel.instance.curEnemyInfo;
            if (e?.passiveBattleState == null)
                return;

            // Caller has already applied body damage. DEAD_NOT_STIFF heal uses Max(1, ...) so at 0 HP it would revive.
            if (e.currentHealth <= 0)
                return;

            var refP = FindPassiveRef(e, EEnemyPassiveSkillType.DEAD_NOT_STIFF);
            if (refP == null)
                return;

            float lossFrac = FloatParamAt(refP, 0, 0.2f);
            if (lossFrac <= 0f)
                lossFrac = 0.2f;
            float healFrac = FloatParamAt(refP, 1, 0.1f);
            if (healFrac < 0f)
                healFrac = 0f;
            int strongLayers = Mathf.RoundToInt(FloatParamAt(refP, 2, 0f));

            int max = e.maxHealth;
            if (max <= 0)
                return;
            int threshold = Mathf.Max(1, Mathf.RoundToInt(max * lossFrac));

            var st = e.passiveBattleState;
            st.bodyDamageAccumulator += damageAmount;

            var ctx = BattleContext.current;
            while (st.bodyDamageAccumulator >= threshold)
            {
                st.bodyDamageAccumulator -= threshold;
                if (healFrac > 0f)
                {
                    int heal = Mathf.Max(1, Mathf.CeilToInt(e.currentHealth * healFrac));
                    if (ctx != null)
                        ctx.ApplyHealToEnemy(heal);
                    else
                        GameModel.instance.EnemyHeal(heal);
                }
                if (strongLayers != 0)
                    AddStrongToAllEnemyMouths(e, strongLayers);
            }
        }

        static void AddStrongToAllEnemyMouths(EnemyInfo e, int layers)
        {
            if (e?.battlePartInfoList == null || layers == 0)
                return;
            foreach (var p in e.battlePartInfoList)
            {
                if (p?.partRefObj == null || p.partRefObj.partType != EPartType.MOUTH)
                    continue;
                BuffInfo bi = BuffFactory.CreateBuffInfoByType(EBuffType.STRONG, layers, p, p);
                if (bi != null)
                    p.AddBuff(bi);
            }
        }

        public static int AdjustEnemyOutgoingDamageToPlayerPart(PartInfo target, PartInfo sender, int baseAmount)
        {
            if (baseAmount <= 0 || sender == null || !sender.isEnemyPart || target == null || target.isEnemyPart)
                return baseAmount;
            var e = GameModel.instance.curEnemyInfo;
            var st = e?.passiveBattleState;
            if (st == null)
                return baseAmount;
            switch (st.outgoingPartDamageMod)
            {
                case EEnemyOutgoingPartDamageMod.PREY_FLAT_BONUS:
                    if (PartHasNonZeroBuff(target, EBuffType.PREY))
                        return baseAmount + st.outgoingPreyBonus;
                    return baseAmount;
                case EEnemyOutgoingPartDamageMod.ALL_PART_FLAT_PENALTY:
                    return Mathf.Max(0, baseAmount - st.outgoingPartPenalty);
                default:
                    return baseAmount;
            }
        }

        static bool PartHasNonZeroBuff(PartInfo p, EBuffType t)
        {
            BuffInfo b = p.GetBuff(t);
            return b != null && b.buffLayer != 0;
        }

        static bool AnyEnemyPartHasHate(EnemyInfo e)
        {
            if (e?.battlePartInfoList == null)
                return false;
            for (int i = 0; i < e.battlePartInfoList.Count; i++)
            {
                if (PartHasNonZeroBuff(e.battlePartInfoList[i], EBuffType.HATE))
                    return true;
            }
            return false;
        }

        static EnemyPassiveRefObj FindPassiveRef(EnemyInfo e, EEnemyPassiveSkillType type)
        {
            if (e?.enemyRefObj?.passiveIdList == null || e.enemyRefObj.passiveIdList.Count == 0)
                return null;
            var list = SCRefDataMgr.instance.enemyPassiveRefList.refDataList;
            if (list == null)
                return null;
            for (int i = 0; i < e.enemyRefObj.passiveIdList.Count; i++)
            {
                long pid = e.enemyRefObj.passiveIdList[i];
                EnemyPassiveRefObj row = list.Find(x => x.id == pid);
                if (row != null && row.passiveType == type)
                    return row;
            }
            return null;
        }
    }
}
