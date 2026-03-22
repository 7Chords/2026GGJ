using System.Collections;
using System.Collections.Generic;
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
            var partInfoList = GameModel.instance.GetEntryAttributeTargetPartList(_caster, _entry, _ctx);
            if (partInfoList == null)
                return;

            foreach (var part in partInfoList)
            {
                if(part.GetBuff(EBuffType.FAT) == null) continue;
                //todo£∫”≤±‡¬Î¡À”Õ÷¨∫Õ»º…’id
                int changeAmount = part.GetBuff(EBuffType.FAT).buffLayer / changeUnit;
                battleCtx.ApplyReduceBuffLayerToPart(part, 100002, changeAmount * changeUnit);
                battleCtx.ApplyBuffToPart(part, _caster, 100003, changeAmount* targetUnit);

            }
        }
    }
}
