using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class RealAttackEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            int damage = Mathf.RoundToInt(_entry.attributeValue);
            if (_caster.isEnemyPart)
                battleCtx.ApplyDamageToPlayer(damage);
            else
                battleCtx.ApplyDamageToEnemy(damage);
        }
    }
}
