using SCFrame;
using UnityEngine;

namespace GameCore.Battle
{
    /// <summary>
    /// Runtime logic for germ / mold buff stacks (HEAL_MASS, ATTACK_MASS, BREEDING_MASS, MOLD).
    /// </summary>
    public static class GermMassBuffEffects
    {
        public static void RunHealMassEffect(BuffInfo buff)
        {
            if (buff?.owner == null) return;
            var ctx = BattleContext.current;
            if (ctx == null) return;
            var owner = buff.owner;
            if (!owner.isOnFace || buff.buffLayer <= 0) return;
            int healEach = buff.buffLayer - 6;
            if (healEach <= 0) return;
            var allyGrid = owner.isEnemyPart
                ? GameModel.instance.enemyFaceGridInfoList
                : GameModel.instance.playerFaceGridInfoList;
            var targets = GameModel.CollectPartsInEffectArea(owner, allyGrid);
            for (int i = 0; i < targets.Count; i++)
            {
                var p = targets[i];
                if (p == null || p.currentHealth <= 0) continue;
                if (p.isEnemyPart != owner.isEnemyPart) continue;
                ctx.ApplyHealToPart(p, healEach);
            }
        }

        public static void RunAttackMassEffect(BuffInfo buff)
        {
            if (buff?.owner == null) return;
            var ctx = BattleContext.current;
            if (ctx == null) return;
            var owner = buff.owner;
            if (!owner.isOnFace || buff.buffLayer <= 0) return;
            int dmg = buff.buffLayer;
            int moldExtra = Mathf.RoundToInt(buff.buffValue);
            if (moldExtra <= 0) moldExtra = 2;
            var enemyGrid = owner.isEnemyPart
                ? GameModel.instance.playerFaceGridInfoList
                : GameModel.instance.enemyFaceGridInfoList;
            var targets = GameModel.CollectPartsInEffectArea(owner, enemyGrid);
            for (int i = 0; i < targets.Count; i++)
            {
                var p = targets[i];
                if (p == null || p.currentHealth <= 0) continue;
                if (p.isEnemyPart == owner.isEnemyPart) continue;
                ctx.ApplyDamageToPart(p, owner, dmg);
                ctx.ApplyBuffToPart(p, owner, GameConst.BUFF_ID_MOLD, moldExtra);
            }
        }

        public static void RunBreedingMassAfterPartAction(BuffInfo buff)
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
        /// Among heal vs attack stacks, run GET_EFFECT-like logic for the side with higher layers (both if tied).
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
