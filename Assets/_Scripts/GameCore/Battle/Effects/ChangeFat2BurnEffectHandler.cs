using System.Collections;
using System.Collections.Generic;
using GameCore;
using UnityEngine;

namespace GameCore.Battle.Effects
{
    public class ChangeFat2BurnEffectHandler : IPartEffectHandler
    {
        public void Execute(PartInfo _caster, EntryInfo _entry, PartEffectContext _ctx)
        {
            var battleCtx = BattleContext.current;
            if (battleCtx == null) return;
            int changeUnit = (int)_entry.attributeValueList[0];
            int targetUnit = (int)_entry.attributeValueList[1];
            var partInfoList = GameModel.instance.GetEntryRealTargetPartList(_caster, _entry, _ctx);
            if (partInfoList == null)
                return;

            foreach (var part in partInfoList)
            {
                if(part.GetBuff(EBuffType.FAT) == null) continue;
                //todo��Ӳ��������֬��ȼ��id
                int changeAmount = part.GetBuff(EBuffType.FAT).buffLayer / changeUnit;
                battleCtx.ApplyReduceBuffLayerToPart(part, GameConst.BUFF_ID_FAT, changeAmount * changeUnit);
                battleCtx.ApplyBuffToPart(part, _caster, GameConst.BUFF_ID_BURN, changeAmount * targetUnit);

            }
        }
    }
}
