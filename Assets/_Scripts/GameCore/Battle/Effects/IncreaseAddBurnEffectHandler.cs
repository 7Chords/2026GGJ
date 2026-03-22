using SCFrame;
using System.Collections.Generic;

namespace GameCore.Battle.Effects
{
    public class IncreaseAddBurnEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;

            float addLayer = _entry.attributeValueList[0];
            var partInfoList = GameModel.instance.GetEntryRealTargetPartList(_caster, _entry, _ctx);
            if (partInfoList == null)
                return;

            foreach (var part in partInfoList)
            {
                for(int i=0;i<part.entryInfoList.Count;i++)
                {
                    //注意加到哪个参数上
                    if(part.entryInfoList[i].attributeType==EAttributeType.CHANGE_FAT_2_BURN)
                    {
                        part.entryInfoList[i].attributeValueList[1] += addLayer;
                        SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_EFFECT, part, _caster);
                    }
                    if (part.entryInfoList[i].attributeType == EAttributeType.SPREAD_BURN)
                    {
                        part.entryInfoList[i].attributeValueList[0] += addLayer;
                        SCMsgCenter.SendMsg(SCMsgConst.PART_TRIGGER_EFFECT, part, _caster);
                    }
                }
            }
        }
    }
}
