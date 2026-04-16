using SCFrame;
using System.Collections.Generic;

namespace GameCore.Battle.Effects
{
    public class DamageMultiplierEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            float multiplier = _entry.attributeValueList[0];
            var partInfoList = GameModel.instance.GetEntryRealTargetPartList(_caster, _entry, _ctx);
            if (partInfoList == null)
                return;

            foreach (var part in partInfoList)
            {
                if (part.partRefObj.partType != EPartType.MOUTH)
                    continue;
                var attackEntry = part.entryInfoList.Find(x =>
                    x.attributeType == EAttributeType.ATTACK
                    || x.attributeType == EAttributeType.REAL_ATTACK
                    || x.attributeType == EAttributeType.ATTACK_BY_ENEMY_BLEED);
                if (attackEntry == null)
                    continue;

                SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_EFFECT, part, _caster);
                if (attackEntry.attributeType == EAttributeType.ATTACK_BY_ENEMY_BLEED)
                {
                    // AttackByBleedEffectHandler: [0]=bleedUnit, [1]=attackUnit; scale per-stack damage.
                    if (attackEntry.attributeValueList != null && attackEntry.attributeValueList.Count > 1)
                        attackEntry.attributeValueList[1] *= multiplier;
                }
                else
                    attackEntry.attributeValueList[0] *= multiplier;

                SCMsgCenter.SendMsg(SCMsgConst.PART_POSITIVE_BUFF_GAIN, part);
            }
        }
    }
}
