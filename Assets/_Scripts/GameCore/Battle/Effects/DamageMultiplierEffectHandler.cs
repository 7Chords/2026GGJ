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
                if (part.partRefObj.partType == EPartType.MOUTH)
                {
                    var attackEntry = part.entryInfoList.Find(x => 
                        x.attributeType == EAttributeType.ATTACK || x.attributeType == EAttributeType.REAL_ATTACK);
                    if (attackEntry != null)
                    {
                        SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_EFFECT, part, _caster);
                        attackEntry.attributeValueList[0] *= multiplier;
                    }
                }
            }
        }
    }
}
