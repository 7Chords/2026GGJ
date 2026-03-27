using GameCore;
using UnityEngine;

namespace GameCore.Battle
{
    /// <summary>
    /// STRONG: mouth-only flat attack bonus (layer * buffValue from buff table).
    /// PREY: extra flat damage taken by the part (layer * buffValue), applied in ApplyDamageToPart.
    /// HATE: no combat modifier (reserved for other systems).
    /// </summary>
    public static class BuffCombatModifiers
    {
        public static int GetStrongAttackBonus(PartInfo caster)
        {
            if (caster == null || caster.partRefObj == null)
                return 0;
            if (caster.partRefObj.partType != EPartType.MOUTH)
                return 0;
            BuffInfo strong = caster.GetBuff(EBuffType.STRONG);
            if (strong == null || strong.buffLayer <= 0)
                return 0;
            return Mathf.RoundToInt(strong.buffLayer * strong.buffValue);
        }

        public static int GetPreyExtraDamage(PartInfo target)
        {
            if (target == null)
                return 0;
            BuffInfo prey = target.GetBuff(EBuffType.PREY);
            if (prey == null || prey.buffLayer <= 0)
                return 0;
            return Mathf.RoundToInt(prey.buffLayer * prey.buffValue);
        }
    }
}
