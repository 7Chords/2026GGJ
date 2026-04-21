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
                bool beneficiary = false;
                for(int i=0;i<part.entryInfoList.Count;i++)
                {
                    //ע��ӵ��ĸ�������
                    if(part.entryInfoList[i].attributeType==EAttributeType.CHANGE_FAT_2_BURN)
                    {
                        part.entryInfoList[i].attributeValueList[1] += addLayer;
                        PartTriggerEffectNotifier.Notify(part, _caster);
                        beneficiary = true;
                    }
                    if (part.entryInfoList[i].attributeType == EAttributeType.SPREAD_BURN)
                    {
                        part.entryInfoList[i].attributeValueList[0] += addLayer;
                        PartTriggerEffectNotifier.Notify(part, _caster);
                        beneficiary = true;
                    }
                }
                if (beneficiary)
                    SCMsgCenter.SendMsg(SCMsgConst.PART_POSITIVE_BUFF_GAIN, part);
            }
        }
    }
}
