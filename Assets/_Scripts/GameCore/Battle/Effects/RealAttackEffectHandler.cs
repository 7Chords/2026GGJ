using GameCore;
using GameCore.Battle;
using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class RealAttackEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            float dmgF = _entry.attributeValueList[0];
            dmgF += BuffCombatModifiers.GetStrongAttackBonus(_caster);
            dmgF = EnemyPassiveController.ApplyOutgoingMouthAttackTotalFlatPenalty(_caster, dmgF);
            int damage = Mathf.RoundToInt(dmgF);
            if (_caster.partRefObj.partType == EPartType.MOUTH)
            {
                MouthAttackCoordinator.RegisterPendingAttack(_caster, new MouthAttackDamageData
                {
                    kind = MouthPendingDamageKind.RealAttackBody,
                    caster = _caster,
                    realAttackBodyDamage = damage
                });
                return;
            }

            if (_caster.isEnemyPart)
                battleCtx.ApplyDamageToPlayer(damage);
            else
                battleCtx.ApplyDamageToEnemy(damage, _caster);
        }
    }
}
