using System;
using System.Collections.Generic;
using GameCore;
using SCFrame;
using UnityEngine;

namespace GameCore.Battle
{
    public enum MouthPendingDamageKind
    {
        GridAttack,
        RealAttackBody,
    }

    /// <summary>
    /// Mouth-type ATTACK / REAL_ATTACK: defer damage until the mouth attack shake plays on UI.
    /// </summary>
    public static class MouthAttackCoordinator
    {
        public static bool PendingMouthAttack { get; private set; }

        private static MouthAttackDamageData _pending;
        private static Action _resumeAfterAnimation;

        public static void ResetPendingForNewActivation()
        {
            PendingMouthAttack = false;
            _pending = null;
            _resumeAfterAnimation = null;
        }

        public static void BindResume(Action onComplete)
        {
            _resumeAfterAnimation = onComplete;
        }

        public static void CancelResume()
        {
            _resumeAfterAnimation = null;
        }

        public static void RegisterPendingAttack(PartInfo caster, MouthAttackDamageData data)
        {
            if (caster == null || data == null) return;
            PendingMouthAttack = true;
            _pending = data;
            SCMsgCenter.SendMsg(SCMsgConst.PART_MOUTH_ATTACK, caster);
        }

        public static void ApplyPendingDamage()
        {
            if (_pending == null || BattleContext.current == null) return;
            var battleCtx = BattleContext.current;
            var d = _pending;
            var caster = d.caster;

            _pending = null;

            if (d.kind == MouthPendingDamageKind.RealAttackBody)
            {
                if (caster.isEnemyPart)
                    battleCtx.ApplyDamageToPlayer(d.realAttackBodyDamage);
                else
                    battleCtx.ApplyDamageToEnemy(d.realAttackBodyDamage, caster);
                return;
            }

            float perGridDamage = d.perGridDamage;
            int emptyGridNum = d.emptyGridNum;
            var partOccupyGridNumDic = d.partOccupyGridNumDic;

            if (caster.isEnemyPart)
                battleCtx.ApplyDamageToPlayer(Mathf.RoundToInt(perGridDamage * emptyGridNum));
            else
                battleCtx.ApplyDamageToEnemy(Mathf.RoundToInt(perGridDamage * emptyGridNum), caster);

            if (partOccupyGridNumDic != null)
            {
                foreach (var pair in partOccupyGridNumDic)
                    battleCtx.ApplyDamageToPart(pair.Key, caster, Mathf.RoundToInt(pair.Value * perGridDamage));
            }
        }

        public static void NotifyAnimationComplete()
        {
            PendingMouthAttack = false;
            var cb = _resumeAfterAnimation;
            _resumeAfterAnimation = null;
            cb?.Invoke();
        }
    }

    public sealed class MouthAttackDamageData
    {
        public MouthPendingDamageKind kind = MouthPendingDamageKind.GridAttack;
        public PartInfo caster;
        public float perGridDamage;
        public int emptyGridNum;
        public Dictionary<PartInfo, int> partOccupyGridNumDic;
        public int realAttackBodyDamage;
    }
}
